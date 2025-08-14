Shader "Custom/TwoColorGradient"
{
     Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorA ("Top Color", Color) = (1,0,0,1)
        _ColorB ("Bottom Color", Color) = (0,0,1,1)
        _Blend ("Gradient Blend", Range(0,1)) = 1

        // Surface Options
        [Enum(Opaque,0,Transparent,1)] _Surface("Surface Type", Float) = 0
        [Enum(Alpha,0,Additive,1,Multiply,2)] _BlendMode("Blend Mode", Float) = 0
        [Toggle(_ALPHATEST_ON)] _AlphaClip ("Alpha Clipping", Float) = 0
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        [Toggle(_DOUBLE_SIDED_ON)] _DoubleSided ("Double Sided", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode"="UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ALPHATEST_ON
            #pragma multi_compile _ _DOUBLE_SIDED_ON
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : NORMAL;
                float3 positionWS : POSITION1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            float4 _ColorA;
            float4 _ColorB;
            float _Blend;
            float _Cutoff;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 texCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 gradCol = lerp(_ColorB, _ColorA, IN.uv.y);
                half4 finalCol = lerp(texCol, texCol * gradCol, _Blend);

                #ifdef _ALPHATEST_ON
                clip(finalCol.a - _Cutoff);
                #endif

                // Apply lighting
                Light mainLight = GetMainLight();
                half3 normal = normalize(IN.normalWS);
                half NdotL = saturate(dot(normal, mainLight.direction));
                finalCol.rgb *= (mainLight.color * NdotL + 0.2); // +0.2 for ambient

                return finalCol;
            }
            ENDHLSL
        }
    }
}
