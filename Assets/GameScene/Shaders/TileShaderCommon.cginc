
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

            
sampler2D _MainTex; // インデックステクスチャとして使用
half4 _MainTex_TexelSize;
sampler2D _TileTex; // タイルパレットテクスチャ
half4 _TileTex_TexelSize;
sampler2D _TraitsTex; // タイルの特性テクスチャ, r: アニメーションフレーム数, g: アニメーション速度（16=1秒）
int _PaletteCountX; // タイルのサイズ
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
 * Rect Mask 2D と Pixel Perfect を適用する
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
    // 境界部分に端数が出ると2つ隣のパレットを参照してしまうことがあるので、端数を切り捨てる
    float2 pixel = i.uv * _MainTex_TexelSize.zw * _TileTex_TexelSize.zw / float2(_PaletteCountX, _PaletteCountY);
    pixel = floor(pixel) + float2(0.5, 0.5);
    i.uv = pixel * float2(_PaletteCountX, _PaletteCountY) / _TileTex_TexelSize.zw / _MainTex_TexelSize.zw;
#endif
    
}

/*
 * パレットテクスチャから色を取得
 * index: パレットのインデックス
 * tileUV: タイル内のUV座標（パレットUV空間であること。0～1/パレット数）
 */
fixed4 GetPaletteColor(float index, float2 tileUV)
{
    // パレットの左上UV座標を計算
    // まず u + v の形にした後に、uvに変換
    float paletteU_V = floor(index) / _PaletteCountX;
    // u は 1 で割ったあまりでよい。v は1で割った整数部分だが、 v は下が0なので、1から引く
    float2 paletteUV = float2(frac(paletteU_V), 1 - (floor(paletteU_V) + 1) / _PaletteCountY);

#if _USE_TRAITS
    // タイルの特性を取得
    // r = (frame-1)/255, g = speed*16/255
    fixed4 traits = tex2D(_TraitsTex, paletteUV);
    
    // アニメーションさせる
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
 * インデックスから色を取得。インデックスが端数を含む場合は右隣の色と補間する。
 * index: パレットのインデックス
 * tileUV: タイル内のUV座標（パレットUV空間であること。0～1/パレット数）
 */
fixed4 GetInterporatedColor(float index, float2 tileUV)
{
    // パレットテクスチャから色を取得
    fixed4 color = GetPaletteColor(index, tileUV);
    
#if _USE_INTERPORATION_TILE

    fixed4 color2 = GetPaletteColor(index+1, tileUV);
    
    fixed rate = frac(index);
    
    color = color * (1 - rate) + color2 * rate;
#endif
    
    return color;
}

/*
 * タイルの拡大縮小を考慮して、色を取得
 * index: パレットのインデックス
 * scale: タイルの拡大縮小率
 * uv: テクスチャ内のuv座標
 */
fixed4 GetScaleAndInterporatedColor(float index, float scale, float2 uv)
{
    // タイル内での座標を計算（0～1）
    float2 tileUV = frac(uv * _MainTex_TexelSize.zw);
    
#if _USE_SCALE
    // center を中心に、拡大する
    // 今描画したい点が、パレット上のどこに対応するのか、を知りたいので
    // 倍率が小さいほど、座標は大きく変化する（逆変換をかけている）
    
    fixed2 center = fixed2(0.5, 0.5);

    tileUV = (tileUV-center) / clamp(scale, 0.001, 1) + center;
    
    // 今描画したい点がパレット外なので描画しない（縮小の時に起きる）
    if ( tileUV.x<0 || tileUV.x >= 1 || tileUV.y < 0 || tileUV.y >= 1 )
    {
        discard;
    }
#endif
    
    // タイル内の座標をパレット数で割って、パレットテクスチャ内のuvに変換
    tileUV /= int2(_PaletteCountX, _PaletteCountY);
    
    // パレットテクスチャから色を取得
    fixed4 color = GetInterporatedColor(index, tileUV);
    
    return color;
}
