
#include "TileShaderCommon.cginc"

/*
 * インデックステクスチャから色を取得する
 * r*(1-b)+ g*b の色を返す
 * uv: テクスチャ座標
 */
fixed4 GetBlendedColor(float2 uv)
{
    // 描画対象のテクスチャからインデックス値を取得
    // r = firstIndex/255, g = secondIndex/255, b = rate, a = alpha
    float4 index = tex2D(_MainTex, uv);

    fixed4 color = GetScaleAndInterporatedColor(index.r * 255, 1, uv);
    
    fixed4 color2 = GetScaleAndInterporatedColor(index.g * 255, 1, uv);
    
    float rate = index.b*color2.a;
    
    color = color * (1 - rate) + color2 * rate;
    
    return color;
}

/*
 * インデックステクスチャから色を取得する
 * 頂点補間が有効なら、uv をもとにどちらの隣に近いかを調べて、
 * 隣接マスと色を補間する
 * uv: テクスチャ座標
 */
fixed4 GetColor(float2 uv)
{
    // 描画対象のテクスチャからインデックス値を取得
    // r = firstIndex/255, g = secondIndex/255, b = rate, a = alpha
    fixed4 color = GetBlendedColor(uv);

#if _USE_INTERPORATION_VERTEX
    // タイル内での座標を計算（0～1）
    float2 tileUV = frac(uv * _MainTex_TexelSize.zw);
    
    float2 signs = sign(tileUV - float2(0.5f,0.5f)); // 右半分なら+、左半分なら-
    
    float2 nuv = signs * _MainTex_TexelSize;
    
    // 横隣の色
    fixed4 nxcolor = GetBlendedColor(uv + float2(nuv.x, 0));
    // 上下隣の色
    fixed4 nycolor = GetBlendedColor(uv + float2(0, nuv.y));
    // 斜めの色
    fixed4 nxycolor = GetBlendedColor(uv + nuv);
    
    // 隣の色の重み
    float2 nrate = abs(tileUV - float2(0.5f,0.5f));
    
    // 横方向でブレンドする
    color = color * (1 - nrate.x) + nxcolor * nrate.x;
    nycolor = nycolor * (1 - nrate.x) + nxycolor * nrate.x;
    
    // 縦方向でブレンドする
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
