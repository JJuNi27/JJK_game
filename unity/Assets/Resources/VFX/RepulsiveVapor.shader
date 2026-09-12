Shader "JJK/RepulsiveVapor"
{
    Properties { _BaseColor ("Tint", Color) = (0.7,0.72,0.8,0.4) }
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
            struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END
            V Vert(A a) { V v; v.positionCS=TransformObjectToHClip(a.positionOS.xyz); v.uv=a.uv; v.color=a.color; return v; }
            float Hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float Noise(float2 p)
            {
                float2 i=floor(p), f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),
                    lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y);
            }
            half4 Frag(V v):SV_Target
            {
                float2 p=v.uv*2-1;
                float n=Noise(p*float2(4,2))*.65+Noise(p*float2(9,5)+7)*.35;
                // Long torn sheets, with separated air filaments instead of circular puffs.
                float edge = abs(p.x + sin(p.y*9)*.13);
                float taper = pow(saturate(1-abs(p.y)), .45);
                float sheets = smoothstep(.2,.7,n);
                float density=(1-smoothstep(.06,.60*taper+.07,edge))*taper*sheets;
                return half4(_BaseColor.rgb*v.color.rgb, _BaseColor.a*v.color.a*density);
            }
            ENDHLSL
        }
    }
}
