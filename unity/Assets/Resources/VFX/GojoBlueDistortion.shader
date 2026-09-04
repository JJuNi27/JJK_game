Shader "JJKGame/VFX/Gojo Blue Distortion"
{
    Properties
    {
        _Strength("Normalized Strength", Range(0, 1)) = 0.15
        _WorldRadius("World Radius", Float) = 1
        _Impact("Impact Cue", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent-20"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Unlit"
        }
        LOD 100

        Pass
        {
            Name "GojoBlueLocalizedDistortion"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back
            ColorMask RGB

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _Strength;
                float _WorldRadius;
                float _Impact;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS : TEXCOORD0;
                half3 viewDirectionWS : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float Hash21(float2 samplePos)
            {
                samplePos = frac(samplePos * float2(123.34, 456.21));
                samplePos += dot(samplePos, samplePos + 45.32);
                return frac(samplePos.x * samplePos.y);
            }

            float ValueNoise2D(float2 samplePos)
            {
                float2 cell = floor(samplePos);
                float2 fraction = frac(samplePos);
                float2 blend = fraction * fraction * (3.0 - 2.0 * fraction);

                float nearX = lerp(
                    Hash21(cell),
                    Hash21(cell + float2(1.0, 0.0)),
                    blend.x
                );
                float farX = lerp(
                    Hash21(cell + float2(0.0, 1.0)),
                    Hash21(cell + float2(1.0, 1.0)),
                    blend.x
                );
                return lerp(nearX, farX, blend.y);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirectionWS = GetWorldSpaceNormalizeViewDir(
                    positionInputs.positionWS
                );
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 screenUv = GetNormalizedScreenSpaceUV(input.positionCS);
                float3 centerPositionWS = TransformObjectToWorld(float3(0.0, 0.0, 0.0));
                float2 centerUv = ComputeNormalizedDeviceCoordinates(
                    centerPositionWS,
                    GetWorldToHClipMatrix()
                );

                float aspect = GetScaledScreenParams().x
                    / max(1.0, GetScaledScreenParams().y);
                float2 aspectScale = float2(aspect, 1.0);
                float2 radialToCenter = (centerUv - screenUv) * aspectScale;
                float radialDistance = length(radialToCenter);
                float2 inwardDirection = radialToCenter
                    / max(radialDistance, 0.00001)
                    / aspectScale;
                float2 tangentDirection = float2(
                    -inwardDirection.y / max(aspect, 0.00001),
                    inwardDirection.x * aspect
                );

                half3 normalWS = normalize(input.normalWS);
                half3 viewDirectionWS = normalize(input.viewDirectionWS);
                float facing = saturate(dot(normalWS, viewDirectionWS));
                float outerProfile = smoothstep(0.015, 0.62, facing);
                outerProfile *= outerProfile;
                float innerProfile = smoothstep(0.48, 0.96, facing);
                innerProfile *= innerProfile;
                float localizedFalloff = saturate(
                    outerProfile * 0.46 + innerProfile * 0.78
                );

                float impact = saturate(_Impact);
                float radiusNoiseScale = lerp(
                    22.0,
                    13.0,
                    saturate(_WorldRadius * 0.5)
                );
                float time = _Time.y * lerp(0.48, 0.82, impact);
                float primaryNoise = ValueNoise2D(
                    screenUv * radiusNoiseScale + float2(time, -time * 0.67)
                );
                float detailNoise = ValueNoise2D(
                    screenUv * (radiusNoiseScale * 2.17)
                        + float2(-time * 0.41, time * 0.73)
                );
                float animatedNoise = (
                    primaryNoise * 0.72 + detailNoise * 0.28 - 0.5
                ) * 2.0;

                float maximumOffset = lerp(0.014, 0.022, impact);
                float radialCompression = lerp(0.34, 1.62, innerProfile);
                float offsetAmount = saturate(_Strength)
                    * maximumOffset
                    * localizedFalloff
                    * radialCompression;
                float2 warpedUv = screenUv
                    + inwardDirection * offsetAmount * (1.0 + animatedNoise * 0.10)
                    + tangentDirection * offsetAmount * animatedNoise * 0.045;

                half3 warpedSceneColor = SampleSceneColor(warpedUv);
                float opacity = localizedFalloff
                    * saturate(0.36 + innerProfile * 0.48 + _Strength * 1.18);
                return half4(warpedSceneColor, opacity);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
