
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
    
    // �N���[�^�[�`��̃p�����[�^
    float R = commandR; // �N���[�^�[�S�̂̔��a
    float R_c = R / 5; // �������N�̍L����i���a�j
    float h = -20 * R; // �N���[�^�[��̐[��
    float h_c = 20 * R; // �������N�̍ő卂��
    float h_r = 20 * R; // �����̍ő卂��
    float w_r = R / 5; // �����̕�
    
    float centerH = commandCenterH;
    
    float sigma_r = w_r / 2.355; // �W���΍��̌v�Z�i�����̃K�E�X���z�j

    int2 d = abs((int2) commandPos - (int2) id);
    d = min(d, size - d);
    
    // ���[�N���b�h�������v�Z
    float r = length(d);
    
    if (r < R)
    {
        // �N���[�^�[�`��̍������v�Z����֐�

        // �������N
        float central = h_c * exp(-r * r / (R_c * R_c));
    
        // �N���[�^�[��
        float bottom = h * (1.0 - r * r / (R * R));
    
        // ����
        float rim = h_r * exp(-pow(r - R, 2.0) / (2.0 * sigma_r * sigma_r));
    
        // ����������
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
    
    // �ΎR�`��̃p�����[�^
    float R = commandR; // �ΎR�S�̂̔��a
    float R_c = R / 10; // �Ό��i���a�j
    
    float centerH = commandCenterH + commandH;
    
    int2 d = abs((int2) commandPos - (int2) id);
    d = min(d, size - d);
    
    // ���[�N���b�h�������v�Z
    float r = length(d);
    
    if (r < R_c)
    {
        tile.elevation = centerH;
        tile.temperature += 1400;
    }
    else if (r<R)
    {
        // ����������
        float f = (R - r) / R;
        tile.elevation = centerH * f + tile.elevation * (1 - f);
    }
}

[numthreads(8, 8, 1)]
void CommandCollapse(uint2 id : SV_DispatchThreadID)
{
    uint xy = id.y * size.x + id.x;
    
    // �ΎR�`��̃p�����[�^
    float R = commandR; // �ΎR�S�̂̔��a
    float R_c = R / 5; // �Ό��i���a�j
    
    float centerH = commandCenterH + commandH;
    
    int2 d = abs((int2) commandPos - (int2) id);
    d = min(d, size - d);
    
    // ���[�N���b�h�������v�Z
    float r = length(d);
    
    // ����������
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
        // �󂫃^�C��
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
        // ����������
        // ������肱�̊��ɓK�����`�Ԃɐi��������
        float myAdaptability = totalAdaptability(commandPos, tile.life.species, tile.life.variant);
        
        bool variantFound = false;
        
        if (col[tile.life.species].variantType!=0)
        {
            // �ώ�őΉ��ł���Ȃ�܂��ώ������
            // ���̓y�n�ɓK�����ώ��T��
            // ���Ȃ��Ƃ��A���̎���K���Ă��邱��
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
            // ���ӂ��킵���ώ�͂��Ȃ������̂ŁA�i���ɂ�����
            
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
    
        // ���[�N���b�h�������v�Z
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