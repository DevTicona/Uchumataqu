Shader "Uchumataqu/RA/PlanoCuadricula"
{
    Properties
    {
        _BaseColor ("Color de las líneas", Color) = (0.4, 0.9, 1, 0.65)
        _CellSize ("Tamaño de celda en metros", Float) = 0.1
        _LineWidth ("Grosor de línea", Range(0.5, 2)) = 0.8
        _FocusPoint ("Centro de la guía", Vector) = (0, 0, 0, 0)
        _FocusRadius ("Radio visible en metros", Float) = 0.6
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            Offset -1, -1
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _CellSize;
                float _LineWidth;
                float4 _FocusPoint;
                float _FocusRadius;
            CBUFFER_END
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float2 coordenadas = input.positionWS.xz / max(_CellSize, 0.01);
                float2 distancia = abs(frac(coordenadas - 0.5) - 0.5);
                float2 pixeles = distancia / max(fwidth(coordenadas), 0.0001);
                half linea = 1.0 - saturate(min(pixeles.x, pixeles.y) - _LineWidth + 0.5);
                // Limita la guía al entorno del impacto y atenúa celdas menores que dos píxeles.
                float radio = max(_FocusRadius, 0.01);
                float distanciaCentro = distance(input.positionWS.xz, _FocusPoint.xz);
                half foco = 1.0 - smoothstep(radio * 0.75, radio, distanciaCentro);
                half resolucion = 1.0 - smoothstep(0.25, 0.5,
                    max(fwidth(coordenadas.x), fwidth(coordenadas.y)));
                return half4(_BaseColor.rgb, _BaseColor.a * linea * foco * resolucion);
            }
            ENDHLSL
        }
    }
}
