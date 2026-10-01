Shader "JJKGame/Exploration/Purple Tempest Discharge"
{
    Properties {_Emission("Emission",Float)=7 _Dark("Dark current",Float)=0 _PhaseTime("Time",Float)=0}
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+40" "RenderType"="Transparent"}
        Pass
        {
            Blend One OneMinusSrcAlpha ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _Emission,_Dark,_PhaseTime;float4 _Sphere;
            CBUFFER_END
            struct A{float4 p:POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;};
            struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;float3 world:TEXCOORD1;};
            V Vert(A i){V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);o.uv=i.uv;o.c=i.c;return o;}
            float H(float p){return frac(sin(p*127.1+31.7)*43758.5453);}
            float N(float p){float a=floor(p),f=frac(p);return lerp(H(a),H(a+1),f*f*(3-2*f));}
            half4 Frag(V i):SV_Target
            {
                float3 eye=GetCameraPositionWS(),ray=normalize(i.world-eye),oc=eye-_Sphere.xyz;
                float b=dot(oc,ray),h=b*b-dot(oc,oc)+_Sphere.w*_Sphere.w;
                if(h>0 && -b-sqrt(h)>0 && -b-sqrt(h)<distance(i.world,eye)-.02)discard;
                float seed=i.c.r*91,t=_PhaseTime,side=abs(i.uv.y-.5)*2;
                float width=.55+N(i.uv.x*19-t*13+seed)*.4;
                float outline=1-smoothstep(width-.13,width,side);
                float gap=smoothstep(.12,.32,N(i.uv.x*15+floor(t*17)+seed));
                float alpha=outline*lerp(i.c.a,saturate(i.c.a*3),_Dark)*lerp(gap,.82+.18*gap,_Dark);
                float hot=1-smoothstep(width*.18,width*.48,side);
                float3 bright=(float3(.44,.008,1.2)*(1-side)+float3(1.5,1.15,1.5)*hot)*_Emission;
                float darkFill=1-smoothstep(width*.42,width*.64,side);
                float3 dark=lerp(float3(.003,.001,.008),float3(.28,.006,.82)*_Emission,darkFill);
                return half4(lerp(bright,dark,_Dark)*alpha,alpha);
            }
            ENDHLSL
        }
    }
}
