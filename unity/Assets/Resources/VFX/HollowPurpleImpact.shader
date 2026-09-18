Shader "JJKGame/VFX/Hollow Purple Impact"
{
    Properties { _MainTex("UI",2D)="white"{} _Stage("Stage", Float)=0 _BeatProgress("Beat progress",Float)=0 _Opacity("Opacity",Float)=1 _Centre("Epicentre",Vector)=(.5,.5,.18,1.7778) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Overlay" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off ZTest Always Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _Stage, _Opacity, _BeatProgress; float4 _Centre;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            V Vert(A i) { V o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz); o.uv=i.uv; return o; }
            float H(float x) { return frac(sin(x*127.1)*43758.5453); }
            half4 Frag(V i):SV_Target
            {
                float2 uv=i.uv;
                // Four separate drawings: horizontal slash, offset radial implosion,
                // mass + abstract caster silhouette, then an exposed violet release cut.
                if(_Stage<.5)
                {
                    float2 p=uv-float2(.76,.48); p.x*=_Centre.w;
                    float a=atan2(p.y,p.x), r=length(p);
                    float wedge=1-step(.018+max(0,p.x)*.35,abs(p.y+p.x*.08));
                    float sector=floor((a+3.142)*92);
                    float speed=step(.65,H(sector))*step(.22+H(sector+5)*.7,r);
                    float slash=1-smoothstep(.004,.012,abs(p.y-p.x*.31+sin(p.x*35)*.006));
                    return half4(saturate(wedge+speed+slash).xxx,_Opacity);
                }
                if(_Stage<1.5)
                {
                    float2 p=uv-float2(.31,.58); p.x*=_Centre.w;
                    float a=atan2(p.y,p.x), r=length(p), sector=floor((a+3.142)*61);
                    float radial=step(.44,H(sector))*step(.12+H(sector+13)*.65,r);
                    float crescent=1-smoothstep(.008,.023,abs(r-(.23+sin(a*11)*.017)));
                    float cut=step(.025,abs(p.x*.72+p.y+sin(p.x*27)*.014));
                    float ink=saturate(radial+crescent+(1-smoothstep(.10,.13,r))*cut);
                    return half4((1-ink).xxx,_Opacity);
                }
                if(_Stage<2.5)
                {
                    float2 p=uv-float2(.65,.60); p.x*=_Centre.w;
                    float a=atan2(p.y,p.x),r=length(p);
                    float orb=1-smoothstep(.30+sin(a*19)*.008,.32+sin(a*19)*.008,r);
                    float core=1-smoothstep(.055,.075,r);
                    float2 actor=uv-float2(.36,.31);
                    float head=1-smoothstep(.060,.066,length((actor-float2(0,.12))*float2(1,1.18)));
                    float torso=step(abs(actor.x),.065+(.10-actor.y)*.25)*step(actor.y,.085)*step(-.34,actor.y);
                    float hair=step(length((actor-float2(0,.14))*float2(1,1.1)),.07+sin(atan2(actor.y-.14,actor.x)*13)*.012);
                    float silhouette=saturate(head+torso+hair);
                    float hatch=step(.91,frac((uv.x*1.4+uv.y)*96))*step(.38,r);
                    float3 col=lerp(float3(.035,.003,.07),float3(.68,.06,.85),orb);
                    col=lerp(col,float3(1,1,1),core); col*=1-silhouette;
                    col+=hatch*.55;
                    return half4(col,_Opacity);
                }
                float2 q=uv-_Centre.xy; q.x*=_Centre.w;
                float exposure=saturate(length(q)*.36);
                float3 flash=lerp(float3(1,1,1),float3(.82,.64,1),exposure);
                return half4(flash,_Opacity);
            }
            ENDHLSL
        }
    }
}
