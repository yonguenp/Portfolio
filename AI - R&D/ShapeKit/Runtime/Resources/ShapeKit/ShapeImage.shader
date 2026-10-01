// ShapeKit UI shape shader.
// Every per-shape parameter travels in vertex data, so all ShapeImages that share a texture
// batch into a single draw call with this one material.
//
// Vertex layout (filled by ShapeImage.OnPopulateMesh):
//   TEXCOORD0.xy  texture uv          TEXCOORD0.zw  position relative to shape center
//   TEXCOORD1     corner radii (top-left, top-right, bottom-right, bottom-left)
//   TEXCOORD2.xy  half size           TEXCOORD2.z   outline width   TEXCOORD2.w  edge softness / shadow blur
//   TEXCOORD3     fill color (Graphic.color x gradient), or the shadow color on shadow quads
//   TANGENT       outline color       NORMAL.x      0 = shape, 1 = shadow
//   COLOR         white; Unity multiplies in CanvasRenderer tint and CanvasGroup alpha here,
//                 so button color transitions and fades reach fill, outline and shadow alike.
Shader "ShapeKit/UI Shape"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex    : POSITION;
                float4 color     : COLOR;
                float3 normal    : NORMAL;
                float4 tangent   : TANGENT;
                float4 texcoord0 : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                float4 texcoord2 : TEXCOORD2;
                float4 texcoord3 : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                half4  color         : COLOR;
                float4 uvLocal       : TEXCOORD0;
                float4 radii         : TEXCOORD1;
                float4 shape         : TEXCOORD2;
                half4  fillColor     : TEXCOORD3;
                half4  outlineColor  : TEXCOORD4;
                float4 worldPosition : TEXCOORD5;
                float  mode          : TEXCOORD6;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            // Set globally by Unity 2022.1+; stays 0 on older versions, matching UI-Default behaviour.
            int _UIVertexColorAlwaysGammaSpace;

            half3 VertexColorToActiveSpace(half3 c)
            {
                if (_UIVertexColorAlwaysGammaSpace && !IsGammaSpace())
                {
                    return GammaToLinearSpace(c);
                }
                return c;
            }

            // Signed distance to a box with a different radius per corner (negative inside).
            float RoundedBoxSdf(float2 p, float2 halfSize, float4 radii)
            {
                float r = p.x > 0.0
                    ? (p.y > 0.0 ? radii.y : radii.z)
                    : (p.y > 0.0 ? radii.x : radii.w);
                float2 q = abs(p) - halfSize + r;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uvLocal = float4(TRANSFORM_TEX(v.texcoord0.xy, _MainTex), v.texcoord0.zw);
                o.radii = v.texcoord1;
                o.shape = v.texcoord2;

                half4 tint = v.color;
                tint.rgb = VertexColorToActiveSpace(tint.rgb);
                o.color = tint * _Color;

                half4 fill = v.texcoord3;
                fill.rgb = VertexColorToActiveSpace(fill.rgb);
                o.fillColor = fill;

                half4 outline = v.tangent;
                outline.rgb = VertexColorToActiveSpace(outline.rgb);
                o.outlineColor = outline;

                o.mode = v.normal.x;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float d = RoundedBoxSdf(i.uvLocal.zw, i.shape.xy, i.radii);
                // Width of one screen pixel in local units, for resolution-independent anti-aliasing.
                float aa = max(fwidth(d), 1e-4);

                half4 col;
                if (i.mode > 0.5)
                {
                    float blur = max(i.shape.w, aa);
                    col = i.color * i.fillColor;
                    col.a *= 1.0 - smoothstep(-blur, blur, d);
                }
                else
                {
                    half4 fill = (tex2D(_MainTex, i.uvLocal.xy) + _TextureSampleAdd) * i.color * i.fillColor;
                    half4 outline = i.outlineColor * i.color;

                    float soft = max(i.shape.w, aa);
                    float shapeAlpha = saturate(0.5 - d / soft);
                    float innerAlpha = i.shape.z > 0.0 ? saturate(0.5 - (d + i.shape.z) / aa) : 1.0;

                    col = lerp(outline, fill, innerAlpha);
                    col.a *= shapeAlpha;
                }

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return col;
            }
        ENDCG
        }
    }
}
