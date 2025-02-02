
#include "TileShaderCommon.cginc"

/*
 * �C���f�b�N�X�e�N�X�`������F���擾����
 * r*(1-b)+ g*b �̐F��Ԃ�
 * uv: �e�N�X�`�����W
 */
fixed4 GetBlendedColor(float2 uv)
{
    // �`��Ώۂ̃e�N�X�`������C���f�b�N�X�l���擾
    // r = firstIndex/255, g = secondIndex/255, b = rate, a = alpha
    float4 index = tex2D(_MainTex, uv);

    fixed4 color = GetScaleAndInterporatedColor(index.r * 255, 1, uv);
    
    fixed4 color2 = GetScaleAndInterporatedColor(index.g * 255, 1, uv);
    
    float rate = index.b*color2.a;
    
    color = color * (1 - rate) + color2 * rate;
    
    return color;
}

/*
 * �C���f�b�N�X�e�N�X�`������F���擾����
 * ���_��Ԃ��L���Ȃ�Auv �����Ƃɂǂ���ׂ̗ɋ߂����𒲂ׂāA
 * �אڃ}�X�ƐF���Ԃ���
 * uv: �e�N�X�`�����W
 */
fixed4 GetColor(float2 uv)
{
    // �`��Ώۂ̃e�N�X�`������C���f�b�N�X�l���擾
    // r = firstIndex/255, g = secondIndex/255, b = rate, a = alpha
    fixed4 color = GetBlendedColor(uv);

#if _USE_INTERPORATION_VERTEX
    // �^�C�����ł̍��W���v�Z�i0�`1�j
    float2 tileUV = frac(uv * _MainTex_TexelSize.zw);
    
    float2 signs = sign(tileUV - float2(0.5f,0.5f)); // �E�����Ȃ�+�A�������Ȃ�-
    
    float2 nuv = signs * _MainTex_TexelSize;
    
    // ���ׂ̐F
    fixed4 nxcolor = GetBlendedColor(uv + float2(nuv.x, 0));
    // �㉺�ׂ̐F
    fixed4 nycolor = GetBlendedColor(uv + float2(0, nuv.y));
    // �΂߂̐F
    fixed4 nxycolor = GetBlendedColor(uv + nuv);
    
    // �ׂ̐F�̏d��
    float2 nrate = abs(tileUV - float2(0.5f,0.5f));
    
    // �������Ńu�����h����
    color = color * (1 - nrate.x) + nxcolor * nrate.x;
    nycolor = nycolor * (1 - nrate.x) + nxycolor * nrate.x;
    
    // �c�����Ńu�����h����
    color = color * (1 - nrate.y) + nycolor * nrate.y;
#endif
 
    return color;
}

fixed4 frag (v2f i) : SV_Target
{
    RectMaskAndPixelPerfect(i);
    
    fixed4 color = GetColor(i.uv);
    
    return color;
}
