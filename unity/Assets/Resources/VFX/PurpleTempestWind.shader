Shader "JJKGame/Exploration/Purple Tempest Wind"
{
    Properties {_Opacity("Opacity",Float)=.72 _PhaseTime("Time",Float)=0}
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+34" "RenderType"="Transparent"}
        Pass
        {
            Blend One OneMinusSrcAlpha ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _Opacity,_PhaseTime;float4 _Sphere;
            CBUFFER_END
            struct A{float4 p:POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;};
            struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;float3 world:TEXCOORD1;};
            V Vert(A i){V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);o.uv=i.uv;o.c=i.c;return o;}
            float H(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float N(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(H(a),H(a+float2(1,0)),f.x),lerp(H(a+float2(0,1)),H(a+1),f.x),f.y);}
            half4 Frag(V i):SV_Target
            {
                float3 eye=GetCameraPositionWS(),ray=normalize(i.world-eye),oc=eye-_Sphere.xyz;
                float b=dot(oc,ray),h=b*b-dot(oc,oc)+_Sphere.w*_Sphere.w;
                if(h>0 && -b-sqrt(h)>0 && -b-sqrt(h)<distance(i.world,eye)-.02)discard;
                float2 uv=i.uv;float t=_PhaseTime,seed=i.c.r*73;
                float sweep=N(float2(uv.x*3-t*5+seed,uv.y*11));
                float veil=N(float2(uv.x*7-t*9+seed,uv.y*24+seed));
                float edge=pow(saturate(sin(uv.y*3.14159265)),1.3);
                float ends=smoothstep(0,.12,uv.x)*(1-smoothstep(.70,1,uv.x));
                float gap=smoothstep(.2,.45,N(float2(uv.x*5-t*3,seed)));
                float streak=smoothstep(.24,.66,sweep)*(.45+.55*veil)*gap;
                float alpha=streak*edge*ends*i.c.a*_Opacity;
                float3 tone=lerp(float3(.19,.22,.25),float3(1.7,1.85,1.95),smoothstep(.25,.85,sweep));
                return half4(tone*alpha,alpha);
            }
            ENDHLSL
        }
    }
}
