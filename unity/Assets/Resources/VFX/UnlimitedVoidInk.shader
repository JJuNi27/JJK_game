Shader "JJK/UnlimitedVoidInk"
{
    Properties
    {
        _Bloom ("Bloom", Range(0,1)) = 0
        _Age ("Age", Float) = 0
        _Seed ("Seed", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
                float _Bloom, _Age, _Seed;
            CBUFFER_END
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            float Hash(float2 p) { return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453); }
            float Noise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                f = f*f*(3-2*f);
                return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),
                    lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y);
            }
            float Fbm(float2 p)
            {
                return Noise(p)*0.57 + Noise(p*2.03+4.2)*0.28 + Noise(p*4.11)*0.15;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float2 p = input.uv * 2 - 1;
                float2 flow = p * 3.2 + _Seed + float2(_Age*0.035,-_Age*0.055);
                float n = Fbm(flow + Fbm(flow + 3.7) * 1.6);
                float angle = atan2(p.y,p.x);
                float radial = length(p * float2(0.95,1.1));
                float fingers = pow(0.5 + 0.5*sin(angle*9 + _Seed + n*5), 5)*0.27;
                float shape = 0.57 + fingers - radial + (n-0.5)*0.52;
                float body = smoothstep(-0.012,0.018,shape);
                float droplets = smoothstep(0.82,0.88,Noise(p*19 + _Seed))
                    * (1-smoothstep(0.68,0.95,radial)) * (1-body);
                float alpha = max(body,droplets) * _Bloom * 0.78;
                // Crisp suspended liquid, with torn fingers, holes and isolated droplets.
                float holes = smoothstep(0.23,0.27,n);
                alpha *= holes;
                float rim = 1-smoothstep(0.015,0.065,abs(shape));
                return half4(lerp(float3(0.42,0.53,0.72),float3(1.1,1.2,1.38),n) + rim*0.20,alpha);
            }
            ENDHLSL
        }
    }
}
