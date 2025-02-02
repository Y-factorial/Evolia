
#include "h_Structs.hlsl"
#include "h_Variables.hlsl"
#include "h_PhysicalConstants.hlsl"
#include "h_Functions.hlsl"

uint renderingSpecies;
uint renderingVariant;

[numthreads(8, 8, 1)]
void RenderWorld(uint2 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;
    
    float r;
    
    // 地形タイル
    if (tile.elevation < planet.ocean.seaLevel)
    {
        
        float temperatureFactor = (tile.temperature - BoilingPoint()) / (1000 - BoilingPoint());
        
        float e = tile.elevation - planet.ocean.seaLevel;
        
        // -10000 = 0
        // 0 = 8
        // 10000 = 16
        // -10000 = 0
        // seaLevel = 8
        r = (lerp(clamp((1 + e / 10000) * 8, 0.0f, 8.0f), 8, temperatureFactor) + 0) / 255.0;
    }
    else
    {
        r = (clamp((tile.elevation - planet.ocean.seaLevel) / 10000.0f * 8, 0.0f, 7.0f) + 8) / 255.0;
    }
    
    float g = 0;
    float b = 0;

    if (tile.temperature > 1000)
    {
        // 高温の時、タイルの色的に最大で5,000℃だが、地球の温度がそこまで高くなったことは無いので最大1,800度とする
        // 1800 = 0
        // 1000 = 8
        g = (clamp((1800 - tile.temperature) / 800.0f * 8, 0.0f, 7.0f) + 32) / 255.0;
        b = 1-saturate((1800 - tile.temperature) / 800.0f * 8 - 7.0f);
    }
    else if (tile.temperature < CelsiusZero - 10)
    {
        // 低温の時
        if (tile.elevation < planet.ocean.seaLevel)
        {
            // 海の時
            g = (clamp((tile.temperature - (CelsiusZero - 10)) / -10 * 4, 0.0f, 3.0f) + 24) / 255.0;
            b = saturate((tile.temperature - (CelsiusZero - 10)) / -10 * 4);
        }
        else
        {
            // 陸の時
            g = (clamp((tile.temperature - (CelsiusZero - 10)) / -10 * 4, 0.0f, 3.0f) + 28) / 255.0;
            b = saturate((tile.temperature - (CelsiusZero - 10)) / -10 * 4);
        }
    }
    else if (tile.plant.species !=0 )
    {
        g = (col[tile.plant.species].palette + clamp(tile.plant.maturity * 5 - 1, 0, 3)) / 255.0f;
        b = saturate(tile.plant.maturity * 5);
    }
    
    surfaceTexture[id.xy] = float4(r, g, b, 1);
    
}

[numthreads(8, 8, 1)]
void RenderLife(uint2 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;

    uint layer = -1;
    float scale = 1;
    float alpha = 1;
    
    if ( tile.animal.species!=0)
    {
        layer = LAYER_ANIMAL;
    }
    else if (tile.microbe.species != 0)
    {
        layer = LAYER_MICROBE;
        scale = microbeScale;
        // 0.5 以上は 1 とする
        alpha = clamp(microbeScale * 2, 0, 1);
    }

    if (layer>=0 && tile.life.species!=0)
    {
        
        float color = tile.life.variant == 0 ? 0 :
            col[tile.life.species].variantType == 0 ? 0 :
            (tile.life.variant + (VARIANT_COUNT - 1) * (col[tile.life.species].variantType - 1)) / 255.0f;
        
        float size = min(saturate(0.2f + tile.life.maturity), tile.life.health);
        
        
        lifeTexture[id.xy] = float4(
                col[tile.life.species].palette / 255.0f,
                size * col[tile.life.species].scale * scale,
                color, alpha);
    }
    else
    {
        lifeTexture[id.xy] = float4(0, 0, 0, 1);
    }
}


[numthreads(8, 8, 1)]
void RenderWind(uint2 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;
    uint y = id.y;
    
    float l = length(row.wind);
    float angle = floor(frac(atan2(row.wind.y, row.wind.x) / (2 * PI) + 1 / 32.0f) * 16);
    
    overlayTexture[id.xy] = float4(angle / 255.0f, saturate(l), 0, 1);
    
}

[numthreads(8, 8, 1)]
void RenderUpDown(uint2 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;
    
    float l = sqrt( abs(updowns[xy] ));
    float angle = updowns[xy]>0 ? 4 : 12;
    
    overlayTexture[id.xy] = float4(angle / 255.0f, saturate(l), 0, 1);
}


[numthreads(8, 8, 1)]
void RenderTemperature(uint2 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;
    uint y = id.y;
    
    // -10 ～ 30
    float level = floor(clamp((tile.temperature - (CelsiusZero - 10)) / 2.5f, 0, 15));
    
    overlayTexture[id.xy] = float4((level + 16) / 255.0f, 0.5f, 0, 0.75f);
    
}


[numthreads(8, 8, 1)]
void RenderHumidity(uint2 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;
    uint y = id.y;
    
    float level = floor(clamp((1 - tile.humidity) * 16, 0, 15));
    
    overlayTexture[id.xy] = float4((level + 16) / 255.0f, 0.5f, 0, 0.75f);
    
}

[numthreads(8, 8, 1)]
void RenderHappiness(uint2 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;
    uint y = id.y;
    
    float level = clamp(totalAdaptability(id, renderingSpecies, renderingVariant) * 8, 0, 7);
    
    overlayTexture[id.xy] = float4((level + 32) / 255.0f, 0.5f, 0, 0.75f);
}
