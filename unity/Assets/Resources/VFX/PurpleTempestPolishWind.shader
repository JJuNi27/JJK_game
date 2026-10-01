Shader "JJKGame/Exploration/Purple Tempest Polish Wind"
{
    Properties {_Opacity("Opacity",Float)=.76 _PhaseTime("Time",Float)=0}
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
            float _Opacity,_PhaseTime,_Radius,_Coupling;float4 _Sphere,_Event0,_Event1,_Event2;
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
                float large=N(float2(uv.x*4-t*4+seed,uv.y*4+t*.9));
                float tear=N(float2(uv.x*17-t*12+large*3,uv.y*13+seed));
                float edge=pow(saturate(sin(uv.y*3.14159265)),.65);
                float ends=smoothstep(0,.09,uv.x)*(1-smoothstep(.75,1,uv.x));
                float gap=smoothstep(.23,.55,large*.7+tear*.3);
                float crestY=.30+large*.30;
                float crest=exp(-pow((uv.y-crestY)/(.035+tear*.045),2))*smoothstep(.3,.7,tear);
                float broad=gap*edge*(.25+.5*tear);
                float alpha=saturate(broad*.55+crest*.8)*ends*i.c.a*_Opacity;
                float3 gray=lerp(float3(.32,.36,.4),float3(.95,1.01,1.08),large);
                float3 tone=gray+float3(1.35,1.4,1.5)*crest;
                // Pressure crest at a shared disturbance picks up violet; most wind stays neutral.
                tone=lerp(tone,tone+float3(.25,.015,.45),i.c.b*_Coupling*.24);
                return half4(tone*alpha,alpha);
            }
            ENDHLSL
        }
    }
}
