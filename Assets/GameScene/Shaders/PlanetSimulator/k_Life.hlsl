
#include "h_Structs.hlsl"
#include "h_Variables.hlsl"
#include "h_PhysicalConstants.hlsl"
#include "h_Functions.hlsl"

[numthreads(8, 8, LAYER_COUNT)]
void LifeGrow(uint3 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;
    
    uint layer = id.z;
    
    if (tile.life.species != 0)
    {
        float adaptation = totalAdaptability(id.xy, tile.life.species, tile.life.variant);
        
        if (adaptation < 0.5)
        {
             // 適応度に反比例してダメージを受ける
            tile.life.health -= 0.001 / max(0.001, adaptation) * deltaTick;
        }
        else
        {
            // 適応度に比例して回復する
            tile.life.health = saturate(tile.life.health + 0.1 * adaptation * deltaTick);
        }
        
        // 適応度に比例して成長する
        tile.life.maturity += adaptation * col[tile.life.species].growthSpeed * deltaTick;
        
        tile.life.blessedTick -= deltaTick;
        
    
        if (tile.life.health <= 0)
        {
            tile.life.species = 0;
            tile.life.variant = 0;
            tile.life.health = 0;
            tile.life.maturity = 0;
            tile.life.blessedTick = 0;
        }
    }
}

[numthreads(8, 8, LAYER_COUNT)]
void LifeReproduce(uint3 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;
    
    uint layer = id.z;
    
    if (tile.life.maturity >= col[tile.life.species].reproductAt)
    {
        tile.life.maturity = 1;
        
        // 分裂
        int2 d = int2(
            (int) floor(random(id, 1) * (col[tile.life.species].mobility * 2 + 1) - col[tile.life.species].mobility),
            (int) floor(random(id, 2) * (col[tile.life.species].mobility * 2 + 1) - col[tile.life.species].mobility));
        uint2 n = (id.xy + d + size) % size;
        int nxy = n.y * size.x + n.x;
            
        // 移動先での自分の適応度
        float adaptation = totalAdaptability(n, tile.life.species, tile.life.variant);
        
        bool reproduce = false;
        Life newLife;
            
        if (ntile.life.species == 0 && adaptation > 0.5)
        {
            // 移動先は空き地
            ntile.life.species = tile.life.species;
            ntile.life.variant = tile.life.variant;
            ntile.life.health = 1;
            ntile.life.maturity = 0;
            ntile.life.blessedTick = tile.life.blessedTick;
        }
        else if (ntile.life.species == tile.life.species && ntile.life.variant == tile.life.variant)
        {
            // 同じ種類の微生物がいる
        }
        else
        {
            // 異なる種類の微生物がいる
            if (col[tile.life.species].uniteWith == ntile.life.species && totalAdaptability(n, col[tile.life.species].uniteTo, 0) > 0.5 && random(id, 4) < 0.1)
            {
                // 共生する
                ntile.life.species = col[tile.life.species].uniteTo;
                ntile.life.variant = 0;
                ntile.life.health = 1;
                ntile.life.maturity = 0;
                ntile.life.blessedTick = INITIAL_BLESS_TICK;
            }
            else
            {
                // 攻撃を仕掛ける
                // 適性度に比例して相手の寿命を削る
                
                float nadaptation = totalAdaptability(n, ntile.life.species, ntile.life.variant);
                    
                float size = col[tile.life.species].competitiveness * variants[tile.life.variant].competitiveness * adaptation * (tile.life.blessedTick > 0 ? 1000 : 1);
                float nsize = ntile.life.health*col[ntile.life.species].competitiveness * variants[ntile.life.variant].competitiveness * nadaptation * (ntile.life.blessedTick > 0 ? 1000 : 1);
                
                float r = random(id, 3) * (size + nsize);
                if (r < size)
                {
                    // 勝った
                    // 自分の種類に変わる
                    ntile.life.species = tile.life.species;
                    ntile.life.variant = tile.life.variant;
                    ntile.life.health = 1;
                    ntile.life.maturity = 0;
                    ntile.life.blessedTick = tile.life.blessedTick;
                }
            }
        }
        
    }
    
}
