
#include "h_Structs.hlsl"
#include "h_Variables.hlsl"
#include "h_PhysicalConstants.hlsl"
#include "h_Functions.hlsl"


[numthreads(8, 8, 1)]
void EnvCalcDiff(uint2 id : SV_DispatchThreadID)
{
    uint y = id.y;
    uint xy = id.y * size.x + id.x;
    
    uint down = (id.y + 1) % size.y * size.x + id.x;
    uint up = (id.y + size.y - 1) % size.y * size.x + id.x;
    uint right = id.y * size.x + (id.x + 1) % size.x;
    uint left = id.y * size.x + (id.x + size.x - 1) % size.x;
    
    float elevation = tile.elevation - planet.ocean.seaLevel;
    
    // 風上のマスを調べる
    
    // Get main and sub tile offsets
    int upstream1 = (id.y + row.upstream1.y + size.y) % size.y * size.x + (id.x + row.upstream1.x + size.x) % size.x;
    int upstream2 = (id.y + row.upstream2.y + size.y) % size.y * size.x + (id.x + row.upstream2.x + size.x) % size.x;

    float dTemperature = 0;
    float dHumidity = elevation > 0 ? -tile.humidity * 0.001f : 0.1f;
    float dElevation = 0;
    
    // 風上から影響を受ける
    dTemperature += (tiles[upstream1].temperature * row.up1Weight + tiles[upstream2].temperature * (1.0 - row.up1Weight) - tile.temperature) * row.windPower * 0.1f;
    dHumidity += (tiles[upstream1].humidity * row.up1Weight + tiles[upstream2].humidity * (1.0 - row.up1Weight) - tile.humidity) * row.windPower * 0.1f;
    
    // 周囲のマスと平均化する
    dTemperature += ((tiles[down].temperature+ tiles[up].temperature + tiles[right].temperature + tiles[left].temperature) / 4 - tile.temperature) * 0.5f;
    dHumidity += ((tiles[down].humidity + tiles[up].humidity + tiles[right].humidity + tiles[left].humidity) / 4 - tile.humidity) * 0.5f;
    
    // 登りなら湿度あがる、下りなら湿度下がる
    dHumidity -= (tiles[upstream1].elevation * row.up1Weight + tiles[upstream2].elevation * (1.0 - row.up1Weight) - tile.elevation) * row.windPower / 50000;

    // 1億年ごとに地殻変動があるので、一番激しいところで5,000mの変動がある
    // 10,000 で均衡してほしい
    dElevation += (updowns[xy] - elevation / 30000) * 5000.0f * geologicalSpeed / 100000000.0f;

    tile.dTemperature = dTemperature;
    tile.dHumidity = dHumidity;
    tile.dElevation = dElevation;
}

[numthreads(8, 8, 1)]
void EnvApplyDiff(uint2 id : SV_DispatchThreadID)
{
    uint y = id.y;
    uint xy = id.y * size.x + id.x;
    
    float elevation = tile.elevation - planet.ocean.seaLevel;
    
    // アルベド    
    float albedo;
    if (tile.temperature < CelsiusZero - 10)
    {
        albedo = IceAlbedo;
    }
    else if (elevation < 0)
    {
        albedo = OceanAlbedo;
    }
    else
    {
        albedo = DirtAlbedo;
    }
    
    // 平衡温度
    float Teq = pow(abs(((1 - albedo) * row.solarEnergy + planet.radiationForcing) / BoltzmannConstant), 0.25f);
    
    if (elevation > 4000)
    {
        // 高地では気温が下がる
        Teq -= (elevation - 4000) * 0.006;
    }
    
    // 比熱
    float heatCapacity;
    if (elevation < 0 && tile.temperature < CelsiusZero + 50) // 50 の代わりに BoilingPoint() を使うのが正しいが、速く冷えてほしいので 50 にしている
    {
        heatCapacity = OceanHeatCapacity;
    }
    else
    {
        heatCapacity = SoilHeatCapacity;
    }
    
    // dT = -4 * σ * (T^4 - Teq^4) / C * dt
    tile.temperature += clamp(-4 * BoltzmannConstant * (pow(tile.temperature, 4) - pow(Teq, 4)) / heatCapacity * deltaTick * heatSpeed, -50, 50);
    tile.temperature += tile.dTemperature * deltaTick;
    
    tile.humidity = clamp(tile.humidity + tile.dHumidity * deltaTick, 0, 1);
    
    tile.elevation += tile.dElevation * deltaTick;
}

[numthreads(8, 8, NUTRIENT_COUNT)]
void EnvLifeNutrients(uint3 id : SV_DispatchThreadID)
{
    uint y = id.y;
    uint xy = id.y * size.x + id.x;
    
    uint n = id.z;
    
    tile.nutrients[n] = (tile.nutrients[n] * 0.95 + col[tile.microbe.species].produce[n] + col[tile.plant.species].produce[n] + col[tile.animal.species].produce[n]) * deltaTick;
}

[numthreads(8, 8, NUTRIENT_COUNT)]
void EnvSumNutrients(uint3 id : SV_DispatchThreadID)
{
    uint y = id.y;
    uint xy = id.y * size.x + id.x;
    
    uint n = id.z;
    
    uint down = (id.y + 1) % size.y * size.x + id.x;
    uint up = (id.y + size.y - 1) % size.y * size.x + id.x;
    uint right = id.y * size.x + (id.x + 1) % size.x;
    uint left = id.y * size.x + (id.x + size.x - 1) % size.x;
    
    tile.nutrients[n] = saturate((tile.nutrients[n]
        + (tiles[down].nutrients[n] + tiles[up].nutrients[n] + tiles[right].nutrients[n] + tiles[left].nutrients[n])/4) / 2);
}
