
#include "UnityCG.cginc"
#if _USE_RECT_MASK
#include "UnityUI.cginc"
#endif

struct appdata_t
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 vertex : SV_POSITION;
#if _USE_RECT_MASK
    float4 worldPos : TEXCORD1;
#endif
};

            
sampler2D _MainTex; // �C���f�b�N�X�e�N�X�`���Ƃ��Ďg�p
half4 _MainTex_TexelSize;
sampler2D _TileTex; // �^�C���p���b�g�e�N�X�`��
half4 _TileTex_TexelSize;
sampler2D _TraitsTex; // �^�C���̓����e�N�X�`��, r: �A�j���[�V�����t���[����, g: �A�j���[�V�������x�i16=1�b�j
int _PaletteCountX; // �^�C���̃T�C�Y
int _PaletteCountY;
int _ForcedMipLevel;
float _Timer;
#if _USE_RECT_MASK
float4 _ClipRect;
#endif

v2f vert(appdata_t v)
{
    v2f o;
    o.uv = v.uv;
    o.vertex = UnityObjectToClipPos(v.vertex);
#if _USE_RECT_MASK
    o.worldPos = v.vertex;
#endif
    
    return o;
}

/*
 * Rect Mask 2D �� Pixel Perfect ��K�p����
 */
void RectMaskAndPixelPerfect(inout v2f i)
{
#if _USE_RECT_MASK
    if (UnityGet2DClipping(i.worldPos, _ClipRect) == 0)
    {
        discard;
    }
#endif

#ifdef _USE_PIXEL_PERFECT
    // ���E�����ɒ[�����o���2�ׂ̃p���b�g���Q�Ƃ��Ă��܂����Ƃ�����̂ŁA�[����؂�̂Ă�
    float2 pixel = i.uv * _MainTex_TexelSize.zw * _TileTex_TexelSize.zw / float2(_PaletteCountX, _PaletteCountY);
    pixel = floor(pixel) + float2(0.5, 0.5);
    i.uv = pixel * float2(_PaletteCountX, _PaletteCountY) / _TileTex_TexelSize.zw / _MainTex_TexelSize.zw;
#endif
    
}

/*
 * �p���b�g�e�N�X�`������F���擾
 * index: �p���b�g�̃C���f�b�N�X
 * tileUV: �^�C������UV���W�i�p���b�gUV��Ԃł��邱�ƁB0�`1/�p���b�g���j
 */
fixed4 GetPaletteColor(float index, float2 tileUV)
{
    // �p���b�g�̍���UV���W���v�Z
    // �܂� u + v �̌`�ɂ�����ɁAuv�ɕϊ�
    float paletteU_V = floor(index) / _PaletteCountX;
    // u �� 1 �Ŋ��������܂�ł悢�Bv ��1�Ŋ������������������A v �͉���0�Ȃ̂ŁA1�������
    float2 paletteUV = float2(frac(paletteU_V), 1 - (floor(paletteU_V) + 1) / _PaletteCountY);

#if _USE_TRAITS
    // �^�C���̓������擾
    // r = (frame-1)/255, g = speed*16/255
    fixed4 traits = tex2D(_TraitsTex, paletteUV);
    
    // �A�j���[�V����������
    float animationFrameCount = round(traits.r * 255) + 1;
    float animationSpeed = round(traits.g * 255) / 16;
    
    float animationLength = animationFrameCount/animationSpeed;

    float animationFrame = floor(frac(_Timer/animationLength)*animationFrameCount);
    
    paletteUV.y -= animationFrame / _PaletteCountY;
#endif
    
    fixed4 color = tex2Dlod(_TileTex, float4(paletteUV + tileUV, 0, _ForcedMipLevel));
    
    return color;
}

/*
 * �C���f�b�N�X����F���擾�B�C���f�b�N�X���[�����܂ޏꍇ�͉E�ׂ̐F�ƕ�Ԃ���B
 * index: �p���b�g�̃C���f�b�N�X
 * tileUV: �^�C������UV���W�i�p���b�gUV��Ԃł��邱�ƁB0�`1/�p���b�g���j
 */
fixed4 GetInterporatedColor(float index, float2 tileUV)
{
    // �p���b�g�e�N�X�`������F���擾
    fixed4 color = GetPaletteColor(index, tileUV);
    
#if _USE_INTERPORATION_TILE

    fixed4 color2 = GetPaletteColor(index+1, tileUV);
    
    fixed rate = frac(index);
    
    color = color * (1 - rate) + color2 * rate;
#endif
    
    return color;
}

/*
 * �^�C���̊g��k�����l�����āA�F���擾
 * index: �p���b�g�̃C���f�b�N�X
 * scale: �^�C���̊g��k����
 * uv: �e�N�X�`������uv���W
 */
fixed4 GetScaleAndInterporatedColor(float index, float scale, float2 uv)
{
    // �^�C�����ł̍��W���v�Z�i0�`1�j
    float2 tileUV = frac(uv * _MainTex_TexelSize.zw);
    
#if _USE_SCALE
    // center �𒆐S�ɁA�g�傷��
    // ���`�悵�����_���A�p���b�g��̂ǂ��ɑΉ�����̂��A��m�肽���̂�
    // �{�����������قǁA���W�͑傫���ω�����i�t�ϊ��������Ă���j
    
    fixed2 center = fixed2(0.5, 0.5);

    tileUV = (tileUV-center) / clamp(scale, 0.001, 1) + center;
    
    // ���`�悵�����_���p���b�g�O�Ȃ̂ŕ`�悵�Ȃ��i�k���̎��ɋN����j
    if ( tileUV.x<0 || tileUV.x >= 1 || tileUV.y < 0 || tileUV.y >= 1 )
    {
        discard;
    }
#endif
    
    // �^�C�����̍��W���p���b�g���Ŋ����āA�p���b�g�e�N�X�`������uv�ɕϊ�
    tileUV /= int2(_PaletteCountX, _PaletteCountY);
    
    // �p���b�g�e�N�X�`������F���擾
    fixed4 color = GetInterporatedColor(index, tileUV);
    
    return color;
}
