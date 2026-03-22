Shader "Custom/Asteroid_DOTS"
{
    Properties
    {
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        _NoiseTex("Noise Texture (R)", 2D) = "white" {}
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

            // ============================================================================
            // DOTS INSTANCING: Property declaration with fallback for standard instancing
            // ============================================================================
            #if defined(UNITY_DOTS_INSTANCING_ENABLED)
                UNITY_DOTS_INSTANCING_START(MaterialPropertyMetadata)
                    UNITY_DOTS_INSTANCED_PROP(float4, _BaseColor)
                    UNITY_DOTS_INSTANCED_PROP(float,  _TintStrength)
                    UNITY_DOTS_INSTANCED_PROP(float,  _DisplaceStrength)
                    UNITY_DOTS_INSTANCED_PROP(float,  _NoiseScale)
                UNITY_DOTS_INSTANCING_END(MaterialPropertyMetadata)
                
                // Macros for unified property access (DOTS vs standard)
                #define GET_BASE_COLOR          UNITY_ACCESS_DOTS_INSTANCED_PROP(float4, _BaseColor)
                #define GET_TINT_STRENGTH       UNITY_ACCESS_DOTS_INSTANCED_PROP(float,  _TintStrength)
                #define GET_DISPLACE_STRENGTH   UNITY_ACCESS_DOTS_INSTANCED_PROP(float,  _DisplaceStrength)
                #define GET_NOISE_SCALE         UNITY_ACCESS_DOTS_INSTANCED_PROP(float,  _NoiseScale)
            #else
                // Standard instancing: properties from UnityPerMaterial CBUFFER
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

            // Texture & Sampler declarations (URP syntax)
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID  // Required for instancing
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;  // World-space normal for lighting
                float2 uv         : TEXCOORD1;
                float3 worldPos   : TEXCOORD2;  // World position for randomization
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // ============================================================================
            // VERTEX SHADER: Displacement + Transform
            // ============================================================================
            Varyings vert(Attributes input) {
                Varyings output = (Varyings)0;
                
                // Setup instancing ID for property access
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 posOS = input.positionOS.xyz;

                // --- NOISE-BASED DISPLACEMENT ---
                // Scale UVs for noise sampling
                float2 noiseUV = input.uv * GET_NOISE_SCALE;
                
                // SAMPLE_TEXTURE2D_LOD: LOD=0 required (no mip-calc in vertex stage)
                float4 noiseSample = SAMPLE_TEXTURE2D_LOD(_NoiseTex, sampler_NoiseTex, noiseUV, 0);
                
                // Remap noise: texture [0..1] → displacement [-1..1] × strength
                float n = (noiseSample.r * 2.0 - 1.0) * GET_DISPLACE_STRENGTH;
                
                // Displace along normalized object-space position vector
                // Using normalize(posOS) preserves shape continuity better than normalOS
                posOS += normalize(posOS) * n; 

                // Standard URP transforms
                output.positionCS = TransformObjectToHClip(posOS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.worldPos = TransformObjectToWorld(posOS);
                output.uv = input.uv;
                return output;
            }

            // ============================================================================
            // FRAGMENT SHADER: Albedo + Procedural Tint + Lighting
            // ============================================================================
            half4 frag(Varyings input) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(input);
                
                // --- PROCEDURAL PER-OBJECT TINT ---
                // Extract world position from model matrix (translation component)
                float3 worldOrigin = UNITY_MATRIX_M._m03_m13_m23;
                
                // Hash function: deterministic pseudo-random based on world position
                float seed = dot(worldOrigin, float3(12.9898, 78.233, 45.543));
                float3 randomValue = frac(sin(seed) * float3(43758.5453, 22578.1459, 11158.3453));
                
                // Generate tint variation: range [0.7..1.0] per RGB channel
                float3 randomTint = lerp(0.7, 1.0, randomValue);
                
                // --- ALBEDO CALCULATION ---
                float4 baseColor = GET_BASE_COLOR;
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                
                float3 finalAlbedo = baseColor.rgb * tex.rgb;
                // Apply random tint with controlled strength
                finalAlbedo = lerp(finalAlbedo, finalAlbedo * randomTint, GET_TINT_STRENGTH);
                
                // --- URP LIGHTING (Main Light + Ambient) ---
                Light mainLight = GetMainLight();
                // Lambertian diffuse
                float3 diffuse = mainLight.color * saturate(dot(input.normalWS, mainLight.direction));
                // Simple ambient term (replace with SH for accuracy if needed)
                float3 ambient = half3(0.2, 0.2, 0.2); 
                
                // Final color: albedo × lighting, alpha from base color
                return half4(finalAlbedo * (diffuse + ambient), baseColor.a);
            }
            ENDHLSL
        }
    }
}