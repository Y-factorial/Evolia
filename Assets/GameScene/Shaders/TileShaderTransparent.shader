Shader "Custom/TilemapShaderTransparent"
{
    Properties
    {
        // r = index/255, g = scale, b = colorMatrix/255, a = alpha
        [HideInInspector] _MainTex ("Index Texture (Main Texture)", 2D) = "white" {} // 描画対象のSpriteのテクスチャ
        _TileTex ("Palette Texture", 2D) = "white" {} // タイルパレットテクスチャ
        _TraitsTex ("Traits Texture", 2D) = "white" {} // タイル特性テクスチャ
        _PaletteCountX ("Palette Count X", Int) = 8 // タイルのサイズ"
        _PaletteCountY ("Palette Count Y", Int) = 8 // タイルのサイズ"
        _ForcedMipLevel ("Forced Mip Level", Int) = 0 // ミップマップレベル"
        [Toggle(_USE_TRAITS)] _UseTraits ("Use Traits", Int) = 0
        [Toggle(_USE_SCALE)] _UseScale ("Use Scale", Int) = 0
        [Toggle(_USE_COLOR_MATRIX)] _UseColorMatrix ("Use Color Matrix", Int) = 0
        [Toggle(_USE_ALPHA)] _UseAlpha ("Use Alpha", Int) = 0
        [Toggle(_USE_INTERPORATION_TILE)] _UseInterporation ("Use Inter Poration", Int) = 0
        [Toggle(_USE_PIXEL_PERFECT)] _UsePixelPerfect ("Use Pixel Perfect", Int) = 1
        [Toggle(_USE_RECT_MASK)] _UseRectMask ("Use Rect Mask", Int) = 0
        [HideInInspector] _Timer("Timer", Float) = 0
        
        // Stencil用のプロパティ
        _Stencil ("Stencil ID", Float) = 1
        _StencilComp ("Stencil Comparison", Float) = 8   // Equal
        _StencilOp ("Stencil Operation", Float) = 0     // Keep
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        
        // Color Mask
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "PreviewType"="Plane"}
        ZWrite Off      // 深度バッファ無効
        Blend SrcAlpha OneMinusSrcAlpha // アルファブレンド
        
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        // ColorMask設定
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma shader_feature _USE_TRAITS
            #pragma shader_feature _USE_SCALE
            #pragma shader_feature _USE_COLOR_MATRIX
            #pragma shader_feature _USE_ALPHA
            #pragma multi_compile _ _USE_INTERPORATION_TILE
            #pragma shader_feature _USE_PIXEL_PERFECT
            #pragma shader_feature _USE_RECT_MASK

            #include "./TileShader.cginc"
            #pragma vertex vert
            #pragma fragment frag

            ENDCG
        }
    }
}