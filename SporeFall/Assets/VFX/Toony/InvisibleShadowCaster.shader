
Shader "Transparent/InvisibleShadowCaster"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        // ---------------------------
        // Shadow caster pass - writes to shadow map (depth)
        // ---------------------------
        Pass
        {
            Name "SHADOWCASTER"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment fragShadow

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            // This fragment is minimal; shadow map uses depth from the rasterizer.
            half4 fragShadow(Varyings IN) : SV_Target
            {
                // No color necessary. Return a placeholder to satisfy compilation.
                return half4(0,0,0,0);
            }

            ENDHLSL
        }

        // ---------------------------
        // Invisible camera pass - prevents any color or depth writes to the main camera
        // ---------------------------
        Pass
        {
            Name "INVISIBLE"
            Tags { "LightMode" = "UniversalForward" }

            ColorMask 0    // Prevent writing any color to the framebuffer
            ZWrite Off     // Do not write depth so object doesn't occlude other objects in camera
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment fragInvisible

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 fragInvisible(Varyings IN) : SV_Target
            {
                // No visible output; this keeps the object invisible to the camera.
                return half4(0,0,0,0);
            }

            ENDHLSL
        }
    }

    // Fallback to ensure nothing unexpected happens in other pipelines
    Fallback Off
}
