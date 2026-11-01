Shader "JJK/Shibuya/BlackFlashInk"
{
    Properties {
        _Color("Color",Color)=(3,.01,.02,1) _Age("Age",Float)=0 _Life("Life",Float)=.3
        _Mode("Mode",Float)=0 _Seed("Seed",Float)=1 _ZWrite("Depth write",Float)=1
        _Growth("Growth",Float)=.095 _Morph("Morph",Float)=.32 _Interval("Interval",Float)=.045
        _DischargeLife("Residual discharge time",Float)=.38 _SmokeSpread("Smoke spread",Float)=1.8
    }
    SubShader {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-20" "RenderType"="Transparent" }
        Pass {
            Name "Ink" Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite [_ZWrite] ZTest LEqual Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;float _Age,_Life,_Mode,_Seed,_Growth,_Morph,_Interval,_DischargeLife,_SmokeSpread;
            CBUFFER_END
            struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;float4 color:COLOR;float4 motion:TEXCOORD0;float4 anchor:TEXCOORD1;};
            struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float3 local:TEXCOORD2;float4 motion:TEXCOORD3;float4 color:COLOR;};
            float Hash(float x) {return frac(sin(x*127.1+311.7)*43758.5453);}
            V Vert(A v) {
                V o;float3 p=v.positionOS.xyz;float f=v.motion.x,kind=v.motion.y,s=v.motion.z+_Seed;
                if(kind<.5) {
                    float burst=saturate(_Age/max(.03,_Growth));
                    p*=lerp(.25,1,burst);
                    float3 tear=float3(sin(s+f*7),cos(s*.73+f*9),.8+f);
                    p+=tear*_Morph*burst*burst*(.12+f*.7);
                } else if(kind<2.5) {
                    // Reveal travels from the root. Shape shears and curls after growth,
                    // while small branches replace their contour at a separate cadence.
                    float phase=floor(_Age/max(.02,_Interval));
                    float clock=kind>1.5?phase:(_Age*12);
                    float wobble=kind>1.5?.13:.055;
                    p.xy+=float2(sin(s+f*16+clock*2.1),cos(s*.7+f*11+clock*3.3))*wobble*_Morph*min(1,f*3);
                    float grow=saturate((_Age-.008)/max(.03,_Growth));
                    p=v.anchor.xyz+(p-v.anchor.xyz)*lerp(.24,1,grow);
                    p.z+=grow*grow*f*_Morph*.35;
                } else if(kind<3.5) {
                    float3 direction=normalize(v.anchor.xyz+float3(0,.1,.35));
                    float speed=.8+Hash(s)*1.4;
                    p+=direction*_Age*_SmokeSpread*speed+float3(0,_Age*_Age*1.6,0);
                    p=v.anchor.xyz+(p-v.anchor.xyz)*(1+_Age*.8);
                    p+=v.normalOS*sin(dot(p,float3(7,9,11))+_Age*5+s)*.035;
                } else if(kind<4.5) {
                    float cycle=floor(_Age/max(.02,_Interval));
                    p.xy+=float2(sin(s+f*17+cycle*2.7),cos(s+f*13-cycle*1.7))*.18*_Morph;
                    p+=normalize(v.anchor.xyz+float3(0,.01,.3))*_Age*.6;
                }
                o.positionCS=TransformObjectToHClip(p);o.world=TransformObjectToWorld(p);
                o.normal=TransformObjectToWorldNormal(v.normalOS);o.local=p;o.motion=v.motion;o.color=v.color;return o;
            }
            half4 Frag(V i):SV_Target {
                float kind=i.motion.y,f=i.motion.x,s=i.motion.z+_Seed;
                float t=saturate(_Age/max(.01,_Life));
                float ndv=abs(dot(normalize(i.normal),normalize(GetWorldSpaceViewDir(i.world))));
                float n=sin(i.local.x*19+i.local.z*13+s)*sin(i.local.y*17-i.local.z*11+_Age*23);
                if(kind>2.5&&kind<3.5) {
                    float density=.5+.25*sin(i.local.x*11+i.local.y*8+s+_Age*4)*sin(i.local.z*9-_Age*3);
                    float appear=smoothstep(0,.05,_Age);
                    return half4(.009,.004,.012,smoothstep(0,.75,ndv)*density*appear*pow(1-t,1.05));
                }
                bool fine=kind>1.5&&kind<2.5;
                bool residual=kind>3.5&&kind<4.5;
                if(kind>.5&&kind<2.5) {
                    float birth=i.motion.w*_Growth;
                    float growth=saturate((_Age-.008-birth)/max(.03,_Growth));
                    clip(growth-f*.92);
                    if(fine) {float phase=floor(_Age/max(.02,_Interval));clip(Hash(s+phase*3.7)-.23);}
                }
                if(residual) {
                    float phase=floor(_Age/max(.02,_Interval));clip(Hash(s+phase*2.9)-.32);
                    clip(_DischargeLife-_Age);
                }
                float dissolve=saturate((t-.35)/.65);
                clip(n*.24+.75-dissolve);
                float rim=smoothstep(.65,.98,1-ndv);
                // A bright branching vein crosses the dark blade surface, rather than
                // relying exclusively on its Fresnel outline. Fine bolts are emissive.
                float vein=pow(saturate(1-abs(i.color.g-.35)*6),3)*saturate(n+.4);
                float heat=fine||residual?.85:saturate(rim*.6+vein*.35);
                float3 ink=float3(.003,.0007,.003);
                float3 color=lerp(ink,_Color.rgb,heat);
                float alpha=(fine||residual?.9:.99)*saturate((1-t)*5)*_Color.a;
                if(residual) alpha*=saturate(1-_Age/max(.01,_DischargeLife));
                return half4(color,alpha);
            }
            ENDHLSL
        }
    }
}
