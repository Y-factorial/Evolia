
#include "h_Structs.hlsl"
#include "h_Variables.hlsl"
#include "h_PhysicalConstants.hlsl"
#include "h_Functions.hlsl"

[numthreads(1, 1, 1)]
void Begin()
{
    // 太陽定数を更新
    float luminosityFactor;
    if (age < 0.1e9)
    {
        // 主系列星への移行（最初の1億年）
        luminosityFactor = 0.7 + 0.3 * age / 0.1e9;
    }
    else if (age < 10e9)
    {
        // 主系列星（100億年まで）
        // 10億年あたり 0.007 光度が増加する
        luminosityFactor = 1 + (1.07 - 1) * (age - 0.1e9) / (10e9 - 0.1e9);

    }
    else if (age < 10.2e9)
    {
        // 赤色巨星段階、だいたい2億年でピークに
        luminosityFactor = 1.07 * 200 * exp((age - 10e9) / 0.2e9);
    }
    else if (age < 11e9)
    {
        // 赤色巨星段階、その後安定
        luminosityFactor = 1.07 * 200;
    }
    else
    {
        // 白色矮星段階
        luminosityFactor = 1.07 * 200 * exp(-(age - 11e9) / 1e9);
    }

    planet.solarConstant =  luminosityFactor * planet.orbit.solarLuminosity / (4 * PI * planet.orbit.solarDistance * planet.orbit.solarDistance);
    
    // 太陽黄経を更新
    float orbitalPeriod = 360;
    planet.solarLongitude = (planet.solarLongitude + deltaTick * PI * 2 / orbitalPeriod) % (PI * 2);
    
    // 太陽の赤緯を更新
    // 地軸が傾いていなければ常に0
    // 地軸が90度傾いていれば、0～π
    // 但し緯度なので0 とπは同じ
    planet.solarDeclination = asin(sin(planet.orbit.axisTilt) * sin(planet.solarLongitude));

    
    // 温室効果を計算する
    // 280ppmのCO2の質量が2.19e15kg
    float EarthCO2Mass0 = 2.19e15; // Kg
    // 地球の単位面積当たりのCO2の質量
    float CO2MassPerArea0 = EarthCO2Mass0 / (4 * PI * 6371000.0f * 6371000.0f);
    // 単位面積当たりのCO2質量
    float CO2MassPerArea = planet.atmosphere.co2Mass / (4 * PI * planet.orbit.radius * planet.orbit.radius);
    // 放射強制力係数 IPCC (1990)およびMyhre et al. (1998)によれば
    float kCO2 = 5.35f;
    // CO2=CO20の時の放射強制力
    float fCO2base = 37.5f;
    // CO2の放射強制力
    float fCO2 = max(0, kCO2 * log(CO2MassPerArea / CO2MassPerArea0) + fCO2base);
    
    // 産業革命前の水蒸気量
    float EarthH2OMass0 = 1.27e16; // Kg
    float H2OMassPerArea0 = EarthH2OMass0 / (4 * PI * 6371000.0f * 6371000.0f);
    float H2OMassPerArea = planet.atmosphere.h2oMass / (4 * PI * planet.orbit.radius * planet.orbit.radius);
    // H2Oの放射強制力係数
    float kH2O = 0.7f;
    float fH2Obase = 75.0f;
    float fH2O = max(0, kH2O * log(H2OMassPerArea / H2OMassPerArea0) + fH2Obase);
    // その他の放射強制力
    float fOther = 37.5f;
    
    // 放射強制力
    planet.radiationForcing = fCO2 + fH2O + fOther;
    
    // 酸素濃度を計算する
    planet.atmosphere.o2Ratio = planet.atmosphere.o2Mass / (planet.atmosphere.o2Mass + planet.atmosphere.co2Mass + planet.atmosphere.n2Mass + planet.atmosphere.h2oMass);
    
    
}



[numthreads(1, 1, 1)]
void End()
{
    planet.atmosphere.temperature = planet.statistics.temperatureSum / (size.x * size.y);

    // CO2 は 50,000,000,000,000,000,000 ある
    // 100x100マスに100匹生息していたとして 1,000,000
    // 3億年の吸収量は
    // 300,000,000,000,000
    
    // 光合成は 1,000ppm でマックス、150ppm で止まる
    
    float metabolismUnit = 100000000; // 100000000 に意味はない。速度を合わせてるだけ
    
    float atmospherMass = planet.atmosphere.n2Mass + planet.atmosphere.o2Mass + planet.atmosphere.co2Mass + planet.atmosphere.h2oMass;
    
    float C = planet.atmosphere.co2Mass / atmospherMass;
    float Cmin = 150.0f / 1000000; // 最低濃度
    float Ck = 200.0f / 1000000; // 半飽和定数
    float G0 = planet.statistics.photosynthesisSum * metabolismUnit; 
    
    float O = planet.atmosphere.o2Mass / atmospherMass;
    float Omin = 40000 / 1000000; // 最低濃度 4%
    float Ok = 100.0f / 1000000; // 半飽和定数
    float R0 = planet.statistics.respirationSum * metabolismUnit;
    
    // 実際の代謝量ではなく、代謝能力を記録することにする
    // 実際の代謝量はCO2濃度の影響で光合成と呼吸がバランスしてしまい、どちらが優勢なのか見分けがつかないから
    planet.statistics.photosynthesisMass = G0;
    planet.statistics.respirationMass = R0;
    
    float deltaTime = metabolismSpeed * deltaTick;

    float deltaCO2Mass;
    if (C - Cmin > Ck)
    {
        // CO2濃度が高いときは、Gを定数として扱える
        // C(t) = C(0) - (G-R)*t
        
        float G = G0 * (C - Cmin) / (Ck + (C - Cmin));
        float R = R0 * (O - Omin) / (Ok + (O - Omin));
        
        // 正味の光合成量
        deltaCO2Mass = clamp((R - G) * deltaTime, (atmospherMass * Cmin - planet.atmosphere.co2Mass), planet.atmosphere.o2Mass * 44 / 32);
    }
    else
    {
        // CO2濃度が低いときは、G の式の分母を Ck に変える
        // G = G0 * (C - Cmin) / Ck とし、
        // 平衡状態を表す Ceq = Cmin + R0/G0 * Ck を用いると
        // C(t) = ( C(0) - Ceq ) * exp(-G0/R * t) + Ceq
        
        float Ceq = (Cmin + R0 / (G0 + EPSILON) * Ck) * atmospherMass; // 平衡状態のCO2質量
        deltaCO2Mass = (planet.atmosphere.co2Mass - Ceq) * exp(-G0 / (R0 + EPSILON) * deltaTime) + Ceq - planet.atmosphere.co2Mass;
    }
    planet.atmosphere.co2Mass += deltaCO2Mass;
    planet.atmosphere.o2Mass -= deltaCO2Mass * 32 / 44;

    
    {
        // 還元的鉱物がある限り、酸素が大気中に放出されることは無い
        // 吸収量は酸素濃度に比例
        float m = clamp(planet.atmosphere.o2Mass * 0.99f, 0, planet.atmosphere.o2Mass / atmospherMass * planet.ocean.mineral);
        planet.ocean.mineral -= m;
        planet.ocean.oxydizedMineral += m;
        planet.atmosphere.o2Mass -= m;
    }
    
    if (planet.atmosphere.o2Ratio > 0.3f)
    {
        // 山火事発生
        float fire = planet.atmosphere.o2Mass * 0.01f;
        planet.atmosphere.co2Mass += fire;
        planet.atmosphere.o2Mass -= fire * 32 / 44;
    }
    
    // 水蒸気圧
    // Clausius-Clapeyron方程式より
    float e0 = 611.2f; // 基準温度における飽和水蒸気圧(Pa)
    float L = 2.5e6f; // 水の蒸発潜熱(J/kg)
    float Rv = 461.0f; // 水蒸気の気体定数(J/kgK)
    float vaporPressure = e0 * exp(L / Rv * (1 / CelsiusZero - 1 / planet.atmosphere.temperature)); // 飽和水蒸気圧(Pa)
    
    // 大気中のH2O許容量
    // 単純に、湿度 50% 換算
    planet.atmosphere.h2oCapacity = vaporPressure * (4 * PI * planet.orbit.radius * planet.orbit.radius) / planet.orbit.gravity * 0.5;
    
    // 許容量を超えた分は海になる
    float condense = max(-planet.ocean.oceanMass, planet.atmosphere.h2oMass - planet.atmosphere.h2oCapacity);
        
    planet.ocean.oceanMass += condense;
    planet.atmosphere.h2oMass -= condense;
    
    float currentOceanMass = planet.statistics.oceanDepthSum * TILE_SIZE * TILE_SIZE * 1000.0f;
    
    float dOceanMass = planet.ocean.oceanMass - currentOceanMass;
    
    // 海の目標深度 = 海洋質量 / 地球の面積 / 海洋密度
    // 今の海のタイル数で割るとオーバーシュートのリスクがある
    // 海が蒸発するときは、オーバーシュートしてもあまり害はないので、そのままタイル数で割るのが良い
    // 海ができるときは、オーバーシュートすると全て水没してしまうので、惑星の全面積で割るのが良い
    float dOceanDepth = dOceanMass / ((dOceanMass < 0 ? max(1, planet.statistics.oceanTileCount) : size.x * size.y ) * TILE_SIZE * TILE_SIZE) / 1000.0f;
    
    // 今の深度が目標深度より浅い場合、海面が上昇する
    planet.ocean.seaLevel += dOceanDepth;
}



