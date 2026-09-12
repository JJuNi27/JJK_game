Shader "JJK/UnlimitedVoidIris"
{
    Properties
    {
        _Age ("Presentation time", Float) = 0
        _Intensity ("Spectral intensity", Float) = 1.25
        _Layer ("Eye / tail", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+100" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
                float _Age, _Intensity, _Layer;
            CBUFFER_END
            V Vert(A a) { V v; v.positionCS=TransformObjectToHClip(a.positionOS.xyz); v.uv=a.uv; return v; }
            float Hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
            float Noise(float2 p)
            {
                float2 i=floor(p), f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),
                    lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y);
            }
            float Flow(float2 p) { return Noise(p)*.55+Noise(p*2.07+4)*.3+Noise(p*4.1)*.15; }
            half4 Frag(V v):SV_Target
            {
                float2 p=v.uv*2-1;
                if (_Layer>.5) p=float2(lerp(.6,2.85,v.uv.x),p.y*.75);
                float r=length(p), a=atan2(p.y,p.x);
                float2 warp=float2(Flow(p*3+_Age*.013),Flow(p*3+9-_Age*.019))-.5;
                float angle=a+r*1.5+warp.x*.7-_Age*.025;
                float2 uv=float2(cos(angle),sin(angle))*r*4;
                float broad=Flow(uv+warp*1.7+float2(_Age*.017,0));
                float fine=Flow(p*11+warp*3-float2(_Age*.027,_Age*.009));
                float cloud=smoothstep(.32,.72,broad)*(.55+fine*.65);
                float upperRight=saturate(.55+p.x*.5+p.y*.6);
                float band=.54+upperRight*.10+warp.y*.035;
                cloud=(.2+cloud*.8)*exp(-pow((r-band)/(.055+upperRight*.11+fine*.02),2));
                float iris=smoothstep(.435,.49,r)*(1-smoothstep(.80,.91,r));
                float rimRadius=.86+.012*sin(a*3+.7)+.009*sin(a*5);
                float width=.009+.01*saturate(p.y+.5);
                float rim=exp(-pow((r-rimRadius)/width,2));
                float glow=exp(-pow((r-rimRadius)/(.042+cloud*.035),2));
                float warm=saturate(.25+p.y*.9-p.x*.18);
                float3 rimColor=lerp(float3(.76,.85,1),float3(1.15,.91,.62),warm);
                float3 cold=lerp(float3(.22,.19,.42),float3(.58,.76,.95),saturate(broad+upperRight*.3));
                float3 light=(float3(.012,.014,.035)+cold*cloud*(.35+upperRight*.5))*iris;
                light+=rimColor*(rim*1.10+glow*.30)*(.48+fine*.8);
                float corona=(1-smoothstep(.91,1,r))*smoothstep(.87,.92,r)*cloud*.25;
                light+=float3(.28,.46,.73)*corona;
                float fringe=exp(-pow((r-.925-warp.x*.025)/.021,2))*.10;
                light+=lerp(float3(.17,.28,.48),float3(.65,.43,.22),warm)*fringe;
                float tailY=p.y-(.06+warp.y*.18);
                float stream=exp(-pow(tailY/(.09+fine*.07),2));
                float plume=exp(-pow((p.x-1.35)/.43,2)-pow((p.y-.30)/.23,2));
                float tail=(stream*.75+plume*.65)*smoothstep(.65,1.05,p.x)
                    *(1-smoothstep(2.35,2.85,p.x))*smoothstep(.28,.67,Flow(float2(p.x*3,tailY*8)+warp*2-_Age*.02));
                if (_Layer>.5)
                    return half4(lerp(float3(.20,.23,.43),float3(.56,.72,.91),fine)*_Intensity,
                        saturate(tail)*.8*smoothstep(.85,1.05,r));
                light+=float3(.5,.7,.94)*tail*.6;
                float pupil=1-smoothstep(.429,.434,r);
                float alpha=max(1-smoothstep(.88,1,r),corona);
                return half4(light*(1-pupil)*_Intensity,alpha);
            }
            ENDHLSL
        }
    }
}
