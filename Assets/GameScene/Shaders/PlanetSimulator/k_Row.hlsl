
#include "h_Structs.hlsl"
#include "h_Variables.hlsl"
#include "h_PhysicalConstants.hlsl"
#include "h_Functions.hlsl"

// Helper function to calculate weights and indices
void GetUpstream(float2 wind, out int2 upstream1, out int2 upstream2, out float weight)
{
    // Normalize wind vector and calculate angle
    float angle = (atan2(wind.y, wind.x) + PI * 2) % (PI * 2);

    // Determine the 45-degree sector (8 sectors in total)
    float sector = angle / (PI / 4); // pi/4 = 45 degrees
    int mainSector = uint(sector) % 8;

    // Calculate the remainder to determine weight
    float remainder = frac(sector);
    weight = 1.0 - remainder;

    // Map sector to grid offsets
    int2 offsets[8] =
    {
        int2(-1, 0), // 0 degrees (Right)
        int2(-1, -1), // 45 degrees (Bottom Right)
        int2(0, -1), // 90 degrees (Bottom)
        int2(1, -1), // 135 degrees (Bottom Left)
        int2(1, 0), // 180 degrees (Left)
        int2(1, 1), // 225 degrees (Top Left)
        int2(0, 1), // 270 degrees (Top)
        int2(-1, 1) // 315 degrees (Top Right)
    };

    // Get main and sub tile offsets
    upstream1 = offsets[mainSector];
    upstream2 = offsets[mainSector + 1];
}

[numthreads(1, 8, 1)]
void BeginRow(uint2 id : SV_DispatchThreadID)
{
    uint y = id.y;
    
    // 高さ 200 なら
    // 100.5 が 0
    
    // 緯度
    float l = (PI / 2) * (y + 0.5f - (size.y / 2)) / (size.y / 2);
    
    // 緯度補正
    // 緯度をそのまま使用すると、極付近の面積が大きくなってしまう
    // 緯度 l 以下の部分の面積は sin(l) で表される
    // latitude / (PI/2)= sin(cl)
    //row.latitude = asin(l / (PI / 2)) * 0.9; // 南極、北極はどうせ生物が住めないので削る
    row.latitude = l * 0.75;
    
    /*
    // 太陽の高度角の式から、日の出時の太陽の時角（1日を-π～πとする角度）を逆算したい
    // ωを太陽の時角とすると、
    // sin(θ) = sin(δ) * sin(φ) + cos(δ) * cos(φ) * cos(ω) なので
    // cos(ω) = ( sin(θ) - sin(δ) * sin(φ) ) / cos(δ) * cos(φ) となり、日の出時点では sin(θ) = 0 なので
    // cos(ω) = - sin(δ) * sin(φ) / cos(δ) * cos(φ) = tan(δ) * tan(φ)
    // ω = acos(-tan(δ) * tan(φ)) となる
    float sinsin = sin(planet.solarDeclination) * sin(row.latitude);
    float coscos = cos(planet.solarDeclination) * cos(row.latitude);
    float tantan = sinsin / coscos;
    float sunriseAngle = acos(clamp(-tantan, -1, 1));

    // 太陽エネルギー
    // sin(δ) * sin(φ) + cos(δ) * cos(φ) * cos(ω) は偶関数なので、ω = -h ～ +h の範囲で積分すると
    // 2( sin(δ) * sin(φ) * h + cos(δ) * cos(φ) * sin(h) )
    // 一日を2πとしているので、2πで割ると
    // S/π * ( sin(δ) * sin(φ) * h + cos(δ) * cos(φ) * sin(h) )
    row.solarEnergy = planet.solarConstant / PI * (sinsin * sunriseAngle + coscos * sin(sunriseAngle));
//    row.solarEnergy = planet.solarConstant / PI * (sinsin + coscos);
    */
    
    // 太陽の高度（真上が90℃）
    float solarAngle = PI / 2 - abs(row.latitude - planet.solarDeclination);
    row.solarEnergy = planet.solarConstant / PI * (max(0.2f, solarAngle) / (PI / 2)) * 1.3f;
    
    // 恒常風
    // 南北方向の風は、季節の影響を受ける
    // 太陽赤緯は夏+になる。

    row.wind = float2(-0.8 * sin(2 * PI * abs(row.latitude) / 1.05) - 0.2, 
    -0.4 * sin(2 * PI * row.latitude / 1.05)
    - 0.1 * sin(2 * PI * (row.latitude - planet.solarDeclination) / 1.05)
    );
    
    GetUpstream(row.wind, row.upstream1, row.upstream2, row.up1Weight);
    row.windPower = length(row.wind);    
}

[numthreads(1, 8, 1)]
void EndRow(uint2 id : SV_DispatchThreadID)
{
    uint y = id.y;
}
