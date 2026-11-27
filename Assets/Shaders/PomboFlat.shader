// Pombo/Flat — lambert simples autocontido (so API estavel de Core.hlsl).
// Luz e nevoa chegam via uniforms globais definidos pelo jogo (MaterialFactory /
// WorldBuilder), o que elimina dependencia de APIs de iluminacao do URP.
Shader "Pombo/Flat"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "Queue"="Geometry"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "ForwardFlat"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float distanceCS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END

            // ---- uniforms globais do jogo ----
            float4 _PomboLightDir;    // direcao da luz (superficie -> luz)
            half4 _PomboLightColor;
            half4 _PomboAmbient;
            half4 _PomboFogColor;
            float4 _PomboFogParams;   // x = near, y = far

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.distanceCS = distance(positionWS, GetCameraPositionWS());
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 n = normalize(IN.normalWS);
                float3 l = _PomboLightDir.xyz;
                l = l / max(length(l), 1e-4);
                half ndotl = saturate(dot(n, l));
                half3 lighting = _PomboLightColor.rgb * ndotl + _PomboAmbient.rgb;
                half3 col = _BaseColor.rgb * lighting;

                float span = max(_PomboFogParams.y - _PomboFogParams.x, 0.001);
                float fogT = saturate((IN.distanceCS - _PomboFogParams.x) / span);
                col = lerp(col, _PomboFogColor.rgb, fogT);
                return half4(col, 1);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
