
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
    
    // ���� 200 �Ȃ�
    // 100.5 �� 0
    
    // �ܓx
    float l = (PI / 2) * (y + 0.5f - (size.y / 2)) / (size.y / 2);
    
    // �ܓx�␳
    // �ܓx�����̂܂܎g�p����ƁA�ɕt�߂̖ʐς��傫���Ȃ��Ă��܂�
    // �ܓx l �ȉ��̕����̖ʐς� sin(l) �ŕ\�����
    // latitude / (PI/2)= sin(cl)
    //row.latitude = asin(l / (PI / 2)) * 0.9; // ��ɁA�k�ɂ͂ǂ����������Z�߂Ȃ��̂ō��
    row.latitude = l * 0.75;
    
    /*
    // ���z�̍��x�p�̎�����A���̏o���̑��z�̎��p�i1����-�΁`�΂Ƃ���p�x�j���t�Z������
    // �ւ𑾗z�̎��p�Ƃ���ƁA
    // sin(��) = sin(��) * sin(��) + cos(��) * cos(��) * cos(��) �Ȃ̂�
    // cos(��) = ( sin(��) - sin(��) * sin(��) ) / cos(��) * cos(��) �ƂȂ�A���̏o���_�ł� sin(��) = 0 �Ȃ̂�
    // cos(��) = - sin(��) * sin(��) / cos(��) * cos(��) = tan(��) * tan(��)
    // �� = acos(-tan(��) * tan(��)) �ƂȂ�
    float sinsin = sin(planet.solarDeclination) * sin(row.latitude);
    float coscos = cos(planet.solarDeclination) * cos(row.latitude);
    float tantan = sinsin / coscos;
    float sunriseAngle = acos(clamp(-tantan, -1, 1));

    // ���z�G�l���M�[
    // sin(��) * sin(��) + cos(��) * cos(��) * cos(��) �͋�֐��Ȃ̂ŁA�� = -h �` +h �͈̔͂Őϕ������
    // 2( sin(��) * sin(��) * h + cos(��) * cos(��) * sin(h) )
    // �����2�΂Ƃ��Ă���̂ŁA2�΂Ŋ����
    // S/�� * ( sin(��) * sin(��) * h + cos(��) * cos(��) * sin(h) )
    row.solarEnergy = planet.solarConstant / PI * (sinsin * sunriseAngle + coscos * sin(sunriseAngle));
//    row.solarEnergy = planet.solarConstant / PI * (sinsin + coscos);
    */
    
    // ���z�̍��x�i�^�オ90���j
    float solarAngle = PI / 2 - abs(row.latitude - planet.solarDeclination);
    row.solarEnergy = planet.solarConstant / PI * (max(0.2f, solarAngle) / (PI / 2)) * 1.3f;
    
    // �P�핗
    // ��k�����̕��́A�G�߂̉e�����󂯂�
    // ���z�Ԉ܂͉�+�ɂȂ�B

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
