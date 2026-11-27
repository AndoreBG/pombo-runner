

Shader "Pombo/Transparent"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 0.5)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "ForwardTransparent"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float distanceCS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END


            half4 _PomboFogColor;
            float4 _PomboFogParams;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(positionWS);
                OUT.distanceCS = distance(positionWS, GetCameraPositionWS());
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float span = max(_PomboFogParams.y - _PomboFogParams.x, 0.001);
                float fogT = saturate((IN.distanceCS - _PomboFogParams.x) / span);
                half3 col = lerp(_BaseColor.rgb, _PomboFogColor.rgb, fogT);
                return half4(col, _BaseColor.a);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
