Shader "JJKGame/VFX/Gojo Blue Particle"
{
    Properties
    {
        [HDR] _TintColor("Tint", Color) = (1, 1, 1, 1)
        _Mode("Mask Mode", Range(0, 5)) = 0
        _Fade("Fade", Range(0, 1)) = 1
        _Emission("Emission", Range(0, 3)) = 1.2
        _Breakup("Edge Breakup", Range(0, 0.5)) = 0.15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Unlit"
        }
        LOD 100

        Pass
        {
            Name "GojoBlueParticle"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            ColorMask RGB

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _TintColor;
                float _Mode;
                float _Fade;
                float _Emission;
                float _Breakup;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half4 color : COLOR;
                float2 uv : TEXCOORD1;
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
                output.positionWS = positionInputs.positionWS;
                output.color = input.color;
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 centeredUv = input.uv * 2.0 - 1.0;
                float radialDistance = length(centeredUv);
                float time = _Time.y;
                float noise = ValueNoise2D(
                    input.uv * 6.7
                        + input.positionWS.xz * 1.35
                        + float2(time * 0.31, -time * 0.23)
                );

                float softMote = 1.0 - smoothstep(0.18, 1.0, radialDistance);
                softMote *= lerp(1.0 - _Breakup, 1.0, noise);

                float diamondDistance = abs(centeredUv.x) + abs(centeredUv.y);
                float taperedStreak = 1.0 - smoothstep(0.48, 1.28, diamondDistance);
                float roundedTip = 1.0 - smoothstep(0.52, 1.02, radialDistance);
                taperedStreak *= lerp(roundedTip, 1.0, 0.38);
                taperedStreak *= lerp(1.0 - _Breakup, 1.0, noise);

                float bend = centeredUv.y
                    + centeredUv.x * 0.24
                    + sin(centeredUv.x * 4.8 + time * 0.74) * 0.11;
                float longitudinal = abs(centeredUv.x);
                float wispWidth = lerp(
                    0.34,
                    0.055,
                    smoothstep(0.08, 1.0, longitudinal)
                );
                float brokenWisp = 1.0 - smoothstep(
                    wispWidth,
                    wispWidth + 0.16,
                    abs(bend)
                );
                brokenWisp *= 1.0 - smoothstep(0.58, 1.0, longitudinal);
                brokenWisp *= lerp(1.0 - _Breakup, 1.0, noise);

                float2 shardUv = float2(
                    centeredUv.x + centeredUv.y * 0.28,
                    centeredUv.y - centeredUv.x * 0.12
                );
                float shardDistance = max(
                    abs(shardUv.x) * 0.82,
                    abs(shardUv.y) * 1.34
                );
                float darkFragment = 1.0 - smoothstep(0.58, 0.82, shardDistance);
                float chippedCorner = smoothstep(
                    -0.86,
                    -0.18,
                    shardUv.x + shardUv.y * 0.72
                );
                darkFragment *= chippedCorner;
                darkFragment *= lerp(1.0 - _Breakup, 1.0, noise);

                float airflowLongitudinal = abs(centeredUv.x);
                float airflowBend = centeredUv.y
                    + centeredUv.x * 0.16
                    + sin(centeredUv.x * 3.4 + time * 0.52) * 0.08;
                float airflowWidth = lerp(
                    0.26,
                    0.07,
                    smoothstep(0.0, 1.0, airflowLongitudinal)
                );
                float airflowWisp = 1.0 - smoothstep(
                    airflowWidth,
                    airflowWidth + 0.20,
                    abs(airflowBend)
                );
                airflowWisp *= 1.0 - smoothstep(
                    0.60,
                    1.0,
                    airflowLongitudinal
                );
                airflowWisp *= lerp(
                    0.58 * (1.0 - _Breakup),
                    1.0,
                    noise
                );

                float ribbonBend = centeredUv.y
                    + centeredUv.x * 0.10
                    + sin(centeredUv.x * 2.7 - time * 0.38) * 0.13;
                float ribbonWidth = 0.085 + noise * 0.035;
                float windRibbon = 1.0 - smoothstep(
                    ribbonWidth,
                    ribbonWidth + 0.18,
                    abs(ribbonBend)
                );
                windRibbon *= 1.0 - smoothstep(
                    0.62,
                    1.0,
                    airflowLongitudinal
                );
                windRibbon *= lerp(
                    0.48 * (1.0 - _Breakup),
                    1.0,
                    noise
                );

                float mode = clamp(_Mode, 0.0, 5.0);
                float moteWeight = 1.0 - step(0.5, mode);
                float streakWeight = step(0.5, mode) * (1.0 - step(1.5, mode));
                float wispWeight = step(1.5, mode) * (1.0 - step(2.5, mode));
                float fragmentWeight = step(2.5, mode) * (1.0 - step(3.5, mode));
                float airflowWeight = step(3.5, mode) * (1.0 - step(4.5, mode));
                float ribbonWeight = step(4.5, mode);
                float mask = saturate(
                    softMote * moteWeight
                    + taperedStreak * streakWeight
                    + brokenWisp * wispWeight
                    + darkFragment * fragmentWeight
                    + airflowWisp * airflowWeight
                    + windRibbon * ribbonWeight
                );

                float centerGlow = 1.0 - smoothstep(0.0, 0.82, radialDistance);
                half3 finalColor = input.color.rgb
                    * _TintColor.rgb
                    * _Emission
                    * lerp(0.74, 1.18, centerGlow);
                float finalAlpha = input.color.a
                    * _TintColor.a
                    * mask
                    * saturate(_Fade);
                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
