Shader "Custom/Bullet_shader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _GlowIntencity("Glow intencity", Range(1,10)) = 5
        _EdgesRoundingStrength("Edges rounding strength", Range(0,0.5)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha One
        ZWrite Off
        ZTest LEqual
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityDOTSInstancing.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            #if defined(UNITY_DOTS_INSTANCING_ENABLED)
                UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                    UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
                    UNITY_DOTS_INSTANCED_PROP(float,  _GlowIntencity)
                    UNITY_DOTS_INSTANCED_PROP(float,  _EdgesRoundingStrength)
                UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
                
                #define GET_BASE_COLOR          UNITY_ACCESS_DOTS_INSTANCED_PROP(float4, _BaseColor)
                #define GET_GLOW_INTENCITY      UNITY_ACCESS_DOTS_INSTANCED_PROP(float,  _GlowIntencity)
                #define GET_EDGES_ROUNDING_STRENGTH   UNITY_ACCESS_DOTS_INSTANCED_PROP(float,  _EdgesRoundingStrength)
            #else
                CBUFFER_START(UnityPerMaterial)
                    float4 _BaseColor;
                    float  _GlowIntencity;
                    float  _EdgesRoundingStrength;
                CBUFFER_END
                
                #define GET_BASE_COLOR          _BaseColor
                #define GET_GLOW_INTENCITY      _GlowIntencity
                #define GET_EDGES_ROUNDING_STRENGTH   _EdgesRoundingStrength
            #endif

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                 // calculate distance from center
                float distFromCenter = length(input.uv - 0.5) * 2.0;
    
                // Inverse the distance and use power to control the "tightness" of the core
                float glow = exp(-distFromCenter * (1.0 / 0.31));//GET_EDGES_ROUNDING_STRENGTH));
    
                //  Apply intensity and color
                half4 color = GET_BASE_COLOR * 8 * glow;//GET_GLOW_INTENCITY * glow;

                return half4(color.rgb, glow);
            }

            ENDHLSL
        }
    }
}
