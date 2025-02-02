
#include "h_Structs.hlsl"
#include "h_Variables.hlsl"
#include "h_PhysicalConstants.hlsl"
#include "h_Functions.hlsl"

[numthreads(1, 1, 1)]
void CommandGetCenterH()
{
    uint xy = commandPos.y * size.x + commandPos.x;
    
    commandCenterH = tile.elevation;
}

[numthreads(8, 8, 1)]
void CommandMeteor(uint2 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;
    
    // クレーター形状のパラメータ
    float R = commandR; // クレーター全体の半径
    float R_c = R / 5; // 中央隆起の広がり（半径）
    float h = -20 * R; // クレーター底の深さ
    float h_c = 20 * R; // 中央隆起の最大高さ
    float h_r = 20 * R; // リムの最大高さ
    float w_r = R / 5; // リムの幅
    
    float centerH = commandCenterH;
    
    float sigma_r = w_r / 2.355; // 標準偏差の計算（リムのガウス分布）

    int2 d = abs((int2) commandPos - (int2) id);
    d = min(d, size - d);
    
    // ユークリッド距離を計算
    float r = length(d);
    
    if (r < R)
    {
        // クレーター形状の高さを計算する関数

        // 中央隆起
        float central = h_c * exp(-r * r / (R_c * R_c));
    
        // クレーター底
        float bottom = h * (1.0 - r * r / (R * R));
    
        // リム
        float rim = h_r * exp(-pow(r - R, 2.0) / (2.0 * sigma_r * sigma_r));
    
        // 高さを合成
        tile.elevation = tile.elevation * 0.8f + centerH * 0.2f +central + bottom + rim;
    }
    
    if (r < R * 2)
    {
        tile.temperature += 1600 * (R * 2 - r) / (R * 2);
    }

}


[numthreads(8, 8, 1)]
void CommandVolcano(uint2 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;
    
    // 火山形状のパラメータ
    float R = commandR; // 火山全体の半径
    float R_c = R / 10; // 火口（半径）
    
    float centerH = commandCenterH + commandH;
    
    int2 d = abs((int2) commandPos - (int2) id);
    d = min(d, size - d);
    
    // ユークリッド距離を計算
    float r = length(d);
    
    if (r < R_c)
    {
        tile.elevation = centerH;
        tile.temperature += 1400;
    }
    else if (r<R)
    {
        // 高さを合成
        float f = (R - r) / R;
        tile.elevation = centerH * f + tile.elevation * (1 - f);
    }
}

[numthreads(8, 8, 1)]
void CommandCollapse(uint2 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;
    
    // 火山形状のパラメータ
    float R = commandR; // 火山全体の半径
    float R_c = R / 5; // 火口（半径）
    
    float centerH = commandCenterH + commandH;
    
    int2 d = abs((int2) commandPos - (int2) id);
    d = min(d, size - d);
    
    // ユークリッド距離を計算
    float r = length(d);
    
    // 高さを合成
    if (r < R)
    {
        float f = (R - r) / R;
        tile.elevation = centerH * f + tile.elevation * (1 - f);
    }
}

[numthreads(1, 1, 1)]
void CommandCosmicRay()
{
    uint xy = commandPos.y * size.x + commandPos.x;
      
    int layer = -1;
    for (int l = 0; l < LAYER_COUNT; ++l)
    {
        if (tile.lives[l].species != 0)
        {
            if (layer < 0 || random(commandPos, 5, l) < 0.5f)
            {
                layer = l;
            }
        }
    }
            
    if (layer < 0)
    {
        // 空きタイル
        if (tile.elevation < planet.ocean.seaLevel && totalAdaptability(commandPos, 1, 0) >= 0.5)
        {
            tile.microbe.species = 1;
            tile.microbe.variant = 0;
            tile.microbe.health = 1;
            tile.microbe.maturity = 0;
            tile.microbe.blessedTick = 100.0f;
        }
    }
    else
    {
        // 生物がいる
        // 自分よりこの環境に適した形態に進化したい
        float myAdaptability = totalAdaptability(commandPos, tile.life.species, tile.life.variant);
        
        bool variantFound = false;
        
        if (col[tile.life.species].variantType!=0)
        {
            // 変種で対応できるならまず変種を試す
            // この土地に適した変種を探す
            // 少なくとも、今の種より適していること
            for (uint i = 1; i < VARIANT_COUNT && !variantFound; ++i)
            {
                uint nspec = tile.life.species;
                uint nvariant = (tile.life.variant + i) % VARIANT_COUNT;
                
                float nAdaptability = totalAdaptability(commandPos, nspec, nvariant);
                if (nAdaptability * col[nspec].competitiveness * variants[nvariant].competitiveness > myAdaptability * col[tile.life.species].competitiveness * variants[tile.life.variant].competitiveness)
                {
                    tile.life.variant = nvariant;
                    tile.life.blessedTick = INITIAL_BLESS_TICK;
                    variantFound = true;

                }
            }
        }
        
        if (!variantFound)
        {
            // よりふさわしい変種はいなかったので、進化にかける
            
            uint base = (uint) (random(commandPos, 3) * 4);
            for (int i = 0; i < 4 && !variantFound; ++i)
            {
                uint t = (base + i) % 4;
                uint nspec = col[tile.life.species].transforms[t];
                if (nspec != 0)
                {
                    uint nlayer = col[nspec].layer;
                
                    uint nvariant = 0;
                    float nAdaptability = totalAdaptability(commandPos, nspec, nvariant);
                    if (nAdaptability * col[nspec].competitiveness * variants[nvariant].competitiveness >= myAdaptability * col[tile.life.species].competitiveness * variants[tile.life.variant].competitiveness * 0.8f)
                    {
                        tile.nlife.species = nspec;
                        tile.nlife.variant = 0;
                        tile.nlife.health = tile.life.health;
                        tile.nlife.maturity = tile.life.maturity;
                        tile.nlife.blessedTick = INITIAL_BLESS_TICK;
                        variantFound = true;
                    }
                }
            }
        }
    }
}


[numthreads(1, 1, 1)]
void CommandAbductLife()
{
    uint xy = commandPos.y * size.x + commandPos.x;
    
    uint layer = LAYER_ANIMAL;
    
    if (microbeScale != 0 && tile.animal.species == 0)
    {
        layer = LAYER_MICROBE;
    }
    
    commandLife[0] = tile.life;
    tile.life.species = 0;
    tile.life.variant = 0;
    tile.life.health = 0;
    tile.life.maturity = 0;
    tile.life.blessedTick = 0;
}

[numthreads(1, 1, 1)]
void CommandIntroduceLife()
{
    uint xy = commandPos.y * size.x + commandPos.x;
    
    uint layer = col[commandLife[0].species].layer;
    tile.life = commandLife[0];
    tile.life.blessedTick = INITIAL_BLESS_TICK;

}


[numthreads(8, 8, LAYER_COUNT)]
void CommandSearchLife(uint3 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;
    
    uint layer = id.z;
    
    if (tile.life.species == commandSpecies)
    {
        
        int2 d = abs((int2) commandPos - (int2) id);
        d = min(d, size - d);
    
        // ユークリッド距離を計算
        float r = length(d);

        GroupMemoryBarrierWithGroupSync();
        
        if ((int) r < commandRxy[0])
        {
            InterlockedMin(commandRxy[0], (int) r);
            if (commandRxy[0] == (int) r)
            {
                commandRxy[1] = (int) xy;
            }
        }
    }
}

[numthreads(1, 1, 1)]
void CommandGetTileDetails()
{
    uint xy = commandPos.y * size.x + commandPos.x;
    
    uint layer = LAYER_ANIMAL;
    
    if (tile.animal.species==0 && microbeScale>0)
    {
        layer = LAYER_MICROBE;
    }

    uint spec = tile.life.species;
    uint variant = tile.life.variant;
    
    tileDetails[0]._tile = tile;
    tileDetails[0].visibleLayer = layer;
    tileDetails[0].happiness[PREFERENCE_O2] = o2Adaptability(xy, spec, variant);
    tileDetails[0].happiness[PREFERENCE_ELEVATION] = elevationAdaptability(xy, spec, variant);
    tileDetails[0].happiness[PREFERENCE_TEMPERATURE] = temperatureAdaptability(xy, spec, variant);
    tileDetails[0].happiness[PREFERENCE_HUMIDITY] = humidityAdaptability(xy, spec, variant);
    tileDetails[0].densityHappiness = densityAdaptability(commandPos, spec, variant);
    tileDetails[0].nutrientHappiness = nutrientAdaptability(commandPos, spec, variant);
    tileDetails[0].totalHappiness = totalAdaptability(commandPos, spec, variant);
    
}