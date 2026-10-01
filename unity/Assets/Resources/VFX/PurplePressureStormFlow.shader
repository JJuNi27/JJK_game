Shader "JJKGame/Exploration/Purple Pressure Storm Flow"
{
    Properties { _Pressure("Pressure",Vector)=(3.2,.72,0,0) _PhaseTime("Time",Float)=0 }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+32" "RenderType"="Transparent"}
        Pass
        {
            Blend One OneMinusSrcAlpha ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Pressure;float _PhaseTime;
            CBUFFER_END
            struct A {float4 p:POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;};
            struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;};
            V Vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.c=i.c;return o;}
            float Hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float Noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(Hash(a),Hash(a+float2(1,0)),f.x),lerp(Hash(a+float2(0,1)),Hash(a+1),f.x),f.y);}
            half4 Frag(V i):SV_Target
            {
                float2 uv=i.uv;float seed=i.c.r*53,t=_PhaseTime;
                float flow=Noise(float2(uv.x*6-t*7+seed,uv.y*18+seed));
                float torn=Noise(float2(uv.x*12-t*11+seed,uv.y*5));
                float edge=smoothstep(0,.12,uv.y)*(1-smoothstep(.82,1,uv.y));
                float ends=pow(saturate(sin(uv.x*3.14159265)),.65);
                float gap=smoothstep(.28,.55,Noise(float2(uv.x*8-t*4,seed)));
                float streak=smoothstep(.25,.62,flow)*smoothstep(.20,.52,torn)*gap;
                float alpha=streak*edge*ends*i.c.a*_Pressure.y;
                float hot=smoothstep(.64,.85,flow)*smoothstep(.5,.75,torn);
                float3 tint=lerp(float3(.065,.025,.12),float3(.63,.52,.76),flow);
                float3 light=float3(.95,.24,1.4)*hot*_Pressure.x;
                return half4((tint+light)*alpha,alpha);
            }
            ENDHLSL
        }
    }
}
