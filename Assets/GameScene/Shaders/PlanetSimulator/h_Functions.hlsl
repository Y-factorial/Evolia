#pragma once

#include "h_Variables.hlsl"
#include "h_PhysicalConstants.hlsl"

// 高品質な疑似乱数生成関数
float random(uint3 st, uint s, uint N = 0)
{
    // 定数（適当に選んだ大きな素数）
    const uint PRIME1 = 1664525u;
    const uint PRIME2 = 1013904223u;
    const uint PRIME3 = 69069u;

    // シードの統合と混ぜ合わせ
    uint value = (uint) st.x * 73856093u ^
                (uint) st.y * 19349663u ^
                (uint) st.z * 83492791u ^
                (uint) seed * 2971215073u ^
                (uint) s * 104395301u ^
                (uint) N * 16275479u;

    // 擬似乱数生成 (線形合同法の拡張)
    value ^= value >> 13;
    value *= PRIME1;
    value ^= value >> 17;
    value *= PRIME2;
    value ^= value >> 5;
    value *= PRIME3;

    // 正規化 (0.0 ～ 1.0 の範囲に変換)
    return float(value & 0x00FFFFFFu) / 16777216.0; // 24bit マスクで正規化
}

float random(uint2 st, uint seed, uint N = 0)
{
    return random(uint3(st.x, st.y, 0), seed, N);
}

float random(uint seed, uint N = 0)
{
    return random(uint2(0, 0), seed, N);
}

int randomCount(float rate, uint seed, uint N = 0)
{
    return (int) rate + (random(seed, N) < frac(rate) ? 1 : 0);

}

float adaptability(float2 points[4], float value)
{
    float segment1 = saturate((value - points[0].x) / (points[1].x - points[0].x)) * (points[1].y - points[0].y) + points[0].y;
    float segment2 = saturate((value - points[1].x) / (points[2].x - points[1].x)) * (points[2].y - points[1].y) + points[1].y;
    float segment3 = saturate((value - points[2].x) / (points[3].x - points[2].x)) * (points[3].y - points[2].y) + points[2].y;
    
    return (value < points[1].x) ? segment1 : (value < points[2].x) ? segment2 : segment3;
}

float o2Adaptability(uint xy, uint spec, uint variant)
{
    return adaptability(col[spec].prefO2, planet.atmosphere.o2Ratio - variants[variant].prefO2);
}

float elevationAdaptability(uint xy, uint spec, uint variant)
{
    return adaptability(col[spec].prefElevation, tile.elevation - planet.ocean.seaLevel - variants[variant].prefElevation);
}

float temperatureAdaptability(uint xy, uint spec, uint variant)
{
    return adaptability(col[spec].prefTemperature, tile.temperature - variants[variant].prefTemperature);
}

float humidityAdaptability(uint xy, uint spec, uint variant)
{
    return adaptability(col[spec].prefHumidity, saturate(tile.humidity - variants[variant].prefHumidity));
}

float envAdaptability(uint2 id, uint spec, uint variant)
{
    uint xy = id.y * size.x + id.x;
    
    float o2Adapter = o2Adaptability(xy, spec, variant);
    float elevationAdapter = elevationAdaptability(xy, spec, variant);
    float temperatureAdapter = temperatureAdaptability(xy, spec, variant);
    float humidityAdapter = humidityAdaptability(xy, spec, variant);
    
    return sqrt(o2Adapter * elevationAdapter * temperatureAdapter * humidityAdapter);
}

float densityAdaptability(uint2 id, uint spec, uint variant)
{
    uint layer = col[spec].layer;
    
    uint down = (id.y + 1) % size.y * size.x + id.x;
    uint up = (id.y + size.y - 1) % size.y * size.x + id.x;
    uint right = id.y * size.x + (id.x + 1) % size.x;
    uint left = id.y * size.x + (id.x + size.x - 1) % size.x;
    
    uint dd = (id.y + 2) % size.y * size.x + id.x;
    uint uu = (id.y + size.y - 2) % size.y * size.x + id.x;
    uint rr = id.y * size.x + (id.x + 2) % size.x;
    uint ll = id.y * size.x + (id.x + size.x - 2) % size.x;
        
    uint lu = (id.y + size.y - 1) % size.y * size.x + (id.x + size.x - 1) % size.x;
    uint ru = (id.y + 1) % size.y * size.x + (id.x + size.x - 1) % size.x;
    uint ld = (id.y + size.y - 1) % size.y * size.x + (id.x + 1) % size.x;
    uint rd = (id.y + 1) % size.y * size.x + (id.x + 1) % size.x;
    
    float density = (tiles[down].life.species == spec ? 1 : 0) +
            (tiles[up].life.species == spec ? 1 : 0) +
            (tiles[right].life.species == spec ? 1 : 0) +
            (tiles[left].life.species == spec ? 1 : 0) +
            (tiles[lu].life.species == spec ? 1 : 0) +
            (tiles[ru].life.species == spec ? 1 : 0) +
            (tiles[ld].life.species == spec ? 1 : 0) +
            (tiles[rd].life.species == spec ? 1 : 0) +
            (tiles[dd].life.species == spec ? 0.5f : 0) +
            (tiles[uu].life.species == spec ? 0.5f : 0) +
            (tiles[rr].life.species == spec ? 0.5f : 0) +
            (tiles[ll].life.species == spec ? 0.5f : 0);
    
    return lerp(1, 0, saturate((density - col[spec].densityCapacity) / 4));
}


float nutrientAdaptability(uint2 id, uint spec, uint variant)
{
    uint xy = id.y * size.x + id.x;
    
    uint down = (id.y + 1) % size.y * size.x + id.x;
    uint up = (id.y + size.y - 1) % size.y * size.x + id.x;
    uint right = id.y * size.x + (id.x + 1) % size.x;
    uint left = id.y * size.x + (id.x + size.x - 1) % size.x;
        
    float nutrientAdaptation = 1 +
    col[spec].consume[NUTRIENT_SOIL] * lerp(-1, 0, tile.nutrients[NUTRIENT_SOIL]) +
    col[spec].consume[NUTRIENT_LEAVES] * lerp(-1, 0, tile.nutrients[NUTRIENT_LEAVES]) +
    col[spec].consume[NUTRIENT_HONEY] * lerp(-1, 0, tile.nutrients[NUTRIENT_HONEY]) +
    col[spec].consume[NUTRIENT_FRUITS] * lerp(-1, 0, tile.nutrients[NUTRIENT_FRUITS]) +
    col[spec].consume[NUTRIENT_MEATS] * lerp(-1, 0, tile.nutrients[NUTRIENT_MEATS]);
    
    return saturate(nutrientAdaptation);
}

float totalAdaptability(uint2 id, uint spec, uint variant)
{
    float envAdaptation = envAdaptability(id, spec, variant);
    
    float densityAdaptation = densityAdaptability(id, spec, variant);
    
    float nutrientAdaptation = nutrientAdaptability(id, spec, variant);
    
    return envAdaptation * densityAdaptation * nutrientAdaptation;
}

float BoilingPoint()
{
    float P = (planet.atmosphere.n2Mass + planet.atmosphere.o2Mass + planet.atmosphere.co2Mass + planet.atmosphere.h2oMass) * planet.orbit.gravity / (4 * PI * planet.orbit.radius * planet.orbit.radius);
    
    // 定数の定義
    float R = 8.314; // 気体定数（J/(mol・K)）

    float H_v = 40660; // 水の蒸発エンタルピー (J/mol) ※約100℃付近での値

    float T0 = CelsiusZero + 100; // 基準温度 (K)

    float P0 = 101325; // 基準圧力 (Pa)

    // クラウジウス-クラペイロンの式に基づく沸点計算
    float T_b = H_v * T0 / (H_v - R * T0 * log(P / P0));

    return T_b;
}