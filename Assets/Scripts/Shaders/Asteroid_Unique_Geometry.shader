Shader "Custom/Asteroid_DOTS_Fixed"
{
    Properties
    {
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        _NoiseTex("Noise Texture (R)", 2D) = "white" {} // New property
        _TintStrength("Tint Strength", Range(0,1)) = 0.3
        _DisplaceStrength("Displace Strength", Range(0, 0.5)) = 0.1
        _NoiseScale("Noise Scale", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityDOTSInstancing.hlsl"

            #if defined(UNITY_DOTS_INSTANCING_ENABLED)
                UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                    UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
                    UNITY_DOTS_INSTANCED_PROP(float,  _TintStrength)
                    UNITY_DOTS_INSTANCED_PROP(float,  _DisplaceStrength)
                    UNITY_DOTS_INSTANCED_PROP(float,  _NoiseScale)
                UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
                
                #define GET_BASE_COLOR          UNITY_ACCESS_DOTS_INSTANCED_PROP(float4, _BaseColor)
                #define GET_TINT_STRENGTH       UNITY_ACCESS_DOTS_INSTANCED_PROP(float,  _TintStrength)
                #define GET_DISPLACE_STRENGTH   UNITY_ACCESS_DOTS_INSTANCED_PROP(float,  _DisplaceStrength)
                #define GET_NOISE_SCALE         UNITY_ACCESS_DOTS_INSTANCED_PROP(float,  _NoiseScale)
            #else
                CBUFFER_START(UnityPerMaterial)
                    float4 _BaseColor;
                    float  _TintStrength;
                    float  _DisplaceStrength;
                    float  _NoiseScale;
                CBUFFER_END
                
                #define GET_BASE_COLOR          _BaseColor
                #define GET_TINT_STRENGTH       _TintStrength
                #define GET_DISPLACE_STRENGTH   _DisplaceStrength
                #define GET_NOISE_SCALE         _NoiseScale
            #endif

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            // DECLARE NOISE TEXTURE
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                float2 uv         : TEXCOORD1;
                float3 worldPos   : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // === VERTEX SHADER ===
            Varyings vert(Attributes input) {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 posOS = input.positionOS.xyz;

                // FAST TEXTURE NOISE (Replacing math-heavy noise)
                // Use LOD 0 because vertex shaders cannot calculate mip levels automatically
                float2 noiseUV = input.uv * GET_NOISE_SCALE;
                float4 noiseSample = SAMPLE_TEXTURE2D_LOD(_NoiseTex, sampler_NoiseTex, noiseUV, 0);
                
                // Remap 0..1 texture to -1..1 displacement
                float n = (noiseSample.r * 2.0 - 1.0) * GET_DISPLACE_STRENGTH;
                
                // Displace along object-space position to prevent "tearing" gaps
                posOS += normalize(posOS) * n; 

                output.positionCS = TransformObjectToHClip(posOS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.worldPos = TransformObjectToWorld(posOS);
                output.uv = input.uv;
                return output;
            }

            // === FRAGMENT SHADER ===
            half4 frag(Varyings input) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(input);
                
                // Object-based random tint
                float3 worldOrigin = UNITY_MATRIX_M._m03_m13_m23;
                float seed = dot(worldOrigin, float3(12.9898, 78.233, 45.543));
                float3 randomValue = frac(sin(seed) * float3(43758.5453, 22578.1459, 11158.3453));
                float3 randomTint = lerp(0.7, 1.0, randomValue);
                
                float4 baseColor = GET_BASE_COLOR;
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                
                float3 finalAlbedo = baseColor.rgb * tex.rgb;
                finalAlbedo = lerp(finalAlbedo, finalAlbedo * randomTint, GET_TINT_STRENGTH);
                
                Light mainLight = GetMainLight();
                float3 diffuse = mainLight.color * saturate(dot(input.normalWS, mainLight.direction));
                float3 ambient = half3(0.2, 0.2, 0.2); 
                
                return half4(finalAlbedo * (diffuse + ambient), baseColor.a);
            }
            ENDHLSL
        }
    }
}
