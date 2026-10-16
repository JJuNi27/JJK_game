Shader "JJKGame/Exploration/Purple Ingredient Caster Pressure"
{
    Properties {_Opacity("Density",Float)=.82 _PhaseTime("Time",Float)=0 _Crest("Crest",Float)=2.7}
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
            float _Opacity,_PhaseTime,_Crest;
            CBUFFER_END
            struct A {float4 p:POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;};
            struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;float3 local:TEXCOORD1;};
            V Vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.c=i.c;o.local=i.p.xyz;return o;}
            float H(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float N(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(H(a),H(a+float2(1,0)),f.x),lerp(H(a+float2(0,1)),H(a+1),f.x),f.y);}
            float4 Frag(V i):SV_Target
            {
                float seed=i.c.r*29,t=_PhaseTime;
                float2 p=float2(i.uv.x*5-t*2.8+seed,i.uv.y*3.7+seed);
                float broad=N(p*.63+float2(t*.31,-t*.6));
                float tear=N(p*2.1+float2(-t*2.1,t*.8));
                // A changing span/width leaves neither full long edge exposed at once.
                float midpoint=.5+(N(float2(i.uv.x*3-t,seed))-.5)*.48;
                float halfwidth=.20+broad*.24;
                float width=1-smoothstep(halfwidth*.34,halfwidth,abs(i.uv.y-midpoint));
                float ends=smoothstep(0,.12,i.uv.x)*(1-smoothstep(.82,1,i.uv.x));
                float space=N(i.local.xz*1.15+float2(t*3.4,-t*2.3)+seed+i.local.y*1.3);
                float cut=N(float2(i.uv.x*12-t*9+seed,i.uv.y*6+space*2));
                float coverage=smoothstep(.25,.65,broad*.42+tear*.26+space*.32);
                coverage*=lerp(.23,1,smoothstep(.28,.68,cut));
                float strands=N(float2(i.uv.x*8-t*7+seed,i.uv.y*9+seed));
                float crest=smoothstep(.60,.87,strands)*smoothstep(.48,.78,broad)*smoothstep(.38,.7,space);
                float a=saturate(width*ends*coverage*i.c.a*_Opacity*(.9+tear*.8));
                float3 diffuse=lerp(float3(.38,.42,.46),float3(.76,.79,.81),broad);
                float3 color=diffuse+crest*_Crest*float3(.87,.91,1);
                return float4(color*a,a);
            }
            ENDHLSL
        }
    }
}
