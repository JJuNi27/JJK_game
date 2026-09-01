Shader "JJKGame/VFX/Gojo Blue Energy"
{
    Properties
    {
        [HDR] _BodyColor("Deep Body", Color) = (0.003, 0.018, 0.24, 1)
        [HDR] _MidColor("Electric Mid", Color) = (0.015, 0.10, 0.78, 1)
        [HDR] _EdgeColor("Cyan Edge", Color) = (0.08, 0.48, 1, 1)
        _Opacity("Opacity", Range(0, 1)) = 1
        _LayerMode("Layer Mode", Range(0, 2)) = 0
        _NoiseScale("Primary Noise Scale", Range(0.5, 12)) = 3.8
        _NoiseSpeed("Primary Noise Speed", Range(-2, 2)) = 0.22
        _DetailScale("Detail Noise Scale", Range(2, 32)) = 11.5
        _DetailSpeed("Detail Noise Speed", Range(-3, 3)) = -0.48
        _FresnelPower("Fresnel Power", Range(0.25, 8)) = 3.4
        _Breakup("Edge Breakup", Range(0, 1)) = 0.2
        _Emission("Emission", Range(0, 3)) = 1.2
        _PulseSpeed("Pulse Speed", Range(0, 16)) = 7.2
        _PulseAmount("Pulse Amount", Range(0, 0.35)) = 0.08
        _PhaseOffset("Phase Offset", Float) = 0
        _Compression("Compression", Range(0, 1)) = 0.35

        [HideInInspector] _BaseColor("Fallback Color", Color) = (0.015, 0.10, 0.78, 1)
        [HideInInspector] _Surface("Surface", Float) = 1
        [HideInInspector] _SrcBlend("Source Blend", Float) = 5
        [HideInInspector] _DstBlend("Destination Blend", Float) = 10
        [HideInInspector] _ZWrite("Z Write", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+20"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Unlit"
        }
        LOD 200

        Pass
        {
            Name "GojoBlueEnergy"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BodyColor;
                half4 _MidColor;
                half4 _EdgeColor;
                half4 _BaseColor;
                float _Opacity;
                float _LayerMode;
                float _NoiseScale;
                float _NoiseSpeed;
                float _DetailScale;
                float _DetailSpeed;
                float _FresnelPower;
                float _Breakup;
                float _Emission;
                float _PulseSpeed;
                float _PulseAmount;
                float _PhaseOffset;
                float _Compression;
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
                float3 positionOS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half3 viewDirectionWS : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float Hash31(float3 point)
            {
                point = frac(point * 0.1031);
                point += dot(point, point.yzx + 33.33);
                return frac((point.x + point.y) * point.z);
            }

            float ValueNoise(float3 point)
            {
                float3 cell = floor(point);
                float3 fraction = frac(point);
                float3 blend = fraction * fraction * (3.0 - 2.0 * fraction);

                float n000 = Hash31(cell + float3(0, 0, 0));
                float n100 = Hash31(cell + float3(1, 0, 0));
                float n010 = Hash31(cell + float3(0, 1, 0));
                float n110 = Hash31(cell + float3(1, 1, 0));
                float n001 = Hash31(cell + float3(0, 0, 1));
                float n101 = Hash31(cell + float3(1, 0, 1));
                float n011 = Hash31(cell + float3(0, 1, 1));
                float n111 = Hash31(cell + float3(1, 1, 1));

                float nearZ = lerp(
                    lerp(n000, n100, blend.x),
                    lerp(n010, n110, blend.x),
                    blend.y
                );
                float farZ = lerp(
                    lerp(n001, n101, blend.x),
                    lerp(n011, n111, blend.x),
                    blend.y
                );
                return lerp(nearZ, farZ, blend.z);
            }

            float FractalNoise(float3 point)
            {
                float total = 0.0;
                float amplitude = 0.56;
                [unroll]
                for (int octave = 0; octave < 4; octave++)
                {
                    total += ValueNoise(point) * amplitude;
                    point = point * 2.03 + float3(7.1, 3.7, 5.9);
                    amplitude *= 0.48;
                }
                return saturate(total);
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
                output.positionOS = input.positionOS.xyz;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirectionWS = GetWorldSpaceNormalizeViewDir(
                    positionInputs.positionWS
                );
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float time = _Time.y + _PhaseOffset;
                float3 primaryPoint = input.positionOS * _NoiseScale
                    + float3(
                        time * _NoiseSpeed,
                        -time * _NoiseSpeed * 0.73,
                        time * _NoiseSpeed * 0.41
                    );
                float3 detailPoint = input.positionOS * _DetailScale
                    + float3(
                        -time * _DetailSpeed * 0.31,
                        time * _DetailSpeed,
                        time * _DetailSpeed * 0.57
                    );
                float primaryNoise = FractalNoise(primaryPoint);
                float detailNoise = FractalNoise(detailPoint);
                float energy = saturate(primaryNoise * 0.68 + detailNoise * 0.32);

                half3 normalWS = normalize(input.normalWS);
                half3 viewDirectionWS = normalize(input.viewDirectionWS);
                float fresnel = pow(
                    1.0 - saturate(dot(normalWS, viewDirectionWS)),
                    max(0.25, _FresnelPower)
                );

                float movingBand = sin(
                    (primaryNoise * 1.7 + detailNoise * 0.8 + time * 0.31) * 6.2831853
                ) * 0.5 + 0.5;
                float breakupMask = smoothstep(
                    _Breakup * 0.72,
                    min(0.99, _Breakup + 0.42),
                    energy * 0.78 + fresnel * 0.22
                );

                float bodyWeight = 1.0 - saturate(_LayerMode);
                float shellWeight = saturate(1.0 - abs(_LayerMode - 1.0));
                float outerWeight = saturate(_LayerMode - 1.0);

                float bodyAlpha = saturate(0.72 + energy * 0.34);
                float shellAlpha = fresnel * lerp(0.32, 1.0, breakupMask);
                float outerAlpha = fresnel * fresnel
                    * breakupMask
                    * lerp(0.30, 1.0, movingBand);
                float alphaMask = bodyAlpha * bodyWeight
                    + shellAlpha * shellWeight
                    + outerAlpha * outerWeight;

                half3 energyColor = lerp(_BodyColor.rgb, _MidColor.rgb, energy);
                energyColor = lerp(
                    energyColor,
                    _EdgeColor.rgb,
                    saturate(fresnel * (0.45 + energy * 0.55))
                );
                energyColor = lerp(
                    energyColor,
                    _EdgeColor.rgb,
                    shellWeight * 0.24 + outerWeight * 0.48
                );

                float pulse = 1.0 + sin(time * _PulseSpeed + energy * 5.4)
                    * _PulseAmount;
                float compressionBrightness = 1.0 + _Compression * 0.14;
                float noiseBrightness = lerp(0.74, 1.08, energy);
                half3 finalColor = energyColor
                    * (_Emission * pulse * compressionBrightness * noiseBrightness);
                float finalAlpha = saturate(_Opacity * alphaMask);
                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
