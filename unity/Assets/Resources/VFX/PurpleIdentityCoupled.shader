Shader "JJKGame/Exploration/Purple Identity Coupled"
{
    Properties { _PhaseTime("Sequence clock",Float)=0 }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
        Pass
        {
            Blend One OneMinusSrcAlpha
            ZWrite Off ZTest Always Cull Front
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _PhaseTime;
            float4 _Identity,_Coupling,_Sources[6],_CouplingEvents[3],_PrimaryPower,_SecondaryPower;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; };
            struct V { float4 positionCS:SV_POSITION; float3 p:TEXCOORD0; };
            V Vert(A i) { V o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz); o.p=i.positionOS.xyz; return o; }
            float H(float3 p)
            {
                p=frac(p*.3183099+float3(.17,.31,.53));p*=19.19;
                return frac(p.x*p.y*p.z*(p.x+p.y+p.z));
            }
            float N(float3 p)
            {
                float3 a=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(lerp(H(a),H(a+float3(1,0,0)),f.x),lerp(H(a+float3(0,1,0)),H(a+float3(1,1,0)),f.x),f.y),
                    lerp(lerp(H(a+float3(0,0,1)),H(a+float3(1,0,1)),f.x),lerp(H(a+float3(0,1,1)),H(a+1),f.x),f.y),f.z);
            }
            float3 Rotate(float3 p)
            {
                return float3(dot(p,float3(.36,.48,-.8)),dot(p,float3(-.8,.6,0)),dot(p,float3(.48,.64,.6)));
            }
            half4 Frag(V i):SV_Target
            {
                float3 eye=TransformWorldToObject(GetCameraPositionWS()),rd=normalize(i.p-eye);
                float b=dot(eye,rd),h=b*b-dot(eye,eye)+.47*.47;
                if(h<=0)return 0;
                h=sqrt(h);float begin=max(0,-b-h),end=-b+h;
                float scene=LinearEyeDepth(SampleSceneDepth(i.positionCS.xy/_ScaledScreenParams.xy),_ZBufferParams);
                float3 rayWS=mul((float3x3)unity_ObjectToWorld,rd);
                end=min(end,scene/max(.0001,-mul((float3x3)UNITY_MATRIX_V,rayWS).z));
                if(end<=begin)return 0;

                float t=_PhaseTime;
                const int steps=120;
                float stride=(end-begin)/steps;
                float4 sum=0;float occlusion=0;
                [loop]for(int k=0;k<steps;k++)
                {
                    float3 p=eye+rd*(begin+(k+.5)*stride);
                    float r=length(p),d=max(r,.0001);
                    float3 direction=p/d;
                    float3 worldDirection=normalize(mul((float3x3)unity_ObjectToWorld,direction));
                    // The large mass stays anchored; its edge only breaks locally.
                    float permanent=N(direction*3.3+float3(2.1,7.3,1.4));
                    float local=N(direction*10.8+float3(t*.82,-t*.63,t*.51));
                    float radius=.365+_Identity.y*((permanent-.5)*1.45+(local-.5)*.4);
                    float inside=1-smoothstep(radius-.018,radius+.007,r);
                    if(inside<.0001)continue;
                    float skin=exp(-abs(r-radius+.009)*52);

                    // Three independently advected energy fields change visibility, not the main mass.
                    float3 q=p*float3(12.2,7.7,17.8);
                    float wave=N(q+float3(t*.92,-t*.55,t*.31));
                    float opposing=N(Rotate(p)*float3(8.4,15.7,11.1)+float3(-t*.63,6.1,t*.81));
                    float fine=N(Rotate(p)*float3(25.1,13.3,21.4)+float3(t*1.51,-t*1.19,9.2));
                    float emission=smoothstep(.52,.75,wave*.59+opposing*.41);
                    float flash=smoothstep(.72,.87,fine*.6+wave*.4);
                    float narrow=1-smoothstep(.015,.063,abs(wave-opposing*.82-.13));
                    float fissure=narrow*smoothstep(.38,.68,fine)*_Identity.z;
                    float vein=(1-smoothstep(.022,.080,abs(wave-fine*.82-.12)))*smoothstep(.43,.7,opposing);
                    float reveal=saturate(vein*.75+flash*.34-fissure*.8);
                    float sector=N(direction*float3(2.2,4.1,2.9)+float3(t*.32,-t*.26,1.7));
                    float surge=smoothstep(.43,.70,sector)*(.6+.4*smoothstep(.35,.64,opposing));
                    surge*=1-saturate(fissure*.55);
                    float erupt=0,rupture=0,ruptureEdge=0;
                    [unroll]for(int eventLane=0;eventLane<3;eventLane++)
                    {
                        float3 axis=normalize(_CouplingEvents[eventLane].xyz);
                        float region=smoothstep(.67,.91,dot(worldDirection,axis)+(fine-.5)*.29);
                        float life=_CouplingEvents[eventLane].w;
                        erupt+=region*life*(.75+eventLane*.12);
                        rupture=max(rupture,region*life);
                        ruptureEdge+=4*region*(1-region)*life;
                    }
                    surge=saturate(surge*.18+erupt*.52);

                    // A burst interrupts only the near shell; the inner mass and far shell survive.
                    // The same 3D event axes drive the companion discharge volume.
                    float front=smoothstep(-.6,.25,dot(direction,-rd));
                    float shellOnly=smoothstep(.22,.31,r);
                    float interruption=saturate(rupture*front*shellOnly*_Coupling.x);
                    float keep=1-interruption;
                    float baseD=inside*(4.2+skin*1.7)*keep;
                    float plasmaD=inside*(.22+skin*1.2)*reveal*(.4+_Identity.w*.24)*keep;
                    float surgeD=inside*(.3+skin*2.3)*surge*(1-interruption*.7);
                    float fissureD=inside*fissure*(2.2+skin*7.0)*keep;
                    float edgeD=inside*skin*ruptureEdge*front*4.8;
                    float voidD=inside*skin*rupture*front*_Coupling.x*24;
                    float density=baseD+plasmaD+surgeD+fissureD+edgeD+voidD;
                    float3 weighted=lerp(float3(.075,.0006,.20),float3(.7,.008,1.35),surge*surge)*baseD;
                    weighted+=float3(6.8,.08,6.1)*skin*baseD*vein*(1-saturate(fissure*.4));
                    weighted+=lerp(float3(.9,.009,1.2),float3(3.6,.075,3.1),flash)*plasmaD;
                    weighted+=float3(2.1,.028,2.4)*surgeD;
                    weighted+=float3(.0004,0,.001)*fissureD;
                    weighted+=float3(4.2,.08,3.6)*edgeD;
                    weighted+=float3(.0002,0,.0007)*voidD;

                    // Connected centre: fixed location, uneven luminous boundary and brief hot channels.
                    float3 core=p*float3(.93,1.13,.96);
                    float coreR=.075*(.63+.75*N(direction*9.4+float3(t*.82,-t*.62,2.4)));
                    float coreD=exp(-dot(core,core)/(coreR*coreR)*1.25)*30*inside;
                    float channel=exp(-r*r*68)*pow(smoothstep(.55,.77,wave+fine*.16),2)*inside*13;
                    density+=coreD+channel;
                    float3 attenuation=exp(-occlusion*float3(.25,.27,.26));
                    float corePulse=.82+.15*sin(t*12.9)+.25*pow(saturate(sin(t*7.4)),8);
                    weighted+=float3(1,.78,.96)*_Identity.x*coreD*attenuation*corePulse;
                    weighted+=float3(5,1.0,3.7)*channel*attenuation;

                    // Short internal ruptures connect the clustered centre to different depths.
                    // Their gaps and bends change within a single event instead of rotating as solid ornaments.
                    [unroll]for(int j=0;j<3;j++)
                    {
                        float3 root=_Sources[j].xyz;
                        float3 tip=normalize(_Sources[j+3].xyz+float3(.02,.013,-.01))*.33;
                        float3 axis=tip-root;
                        float u=saturate(dot(p-root,axis)/max(.0001,dot(axis,axis)));
                        float3 bend=float3(sin(u*18+t*(7.1+j)+j*2.3),
                            cos(u*25-t*(5.7+j*.6)+j),sin(u*22-t*(6.9+j*.8)))*.019*sin(u*3.14159);
                        float3 delta=p-(root+axis*u+bend);
                        float broken=smoothstep(.12,.5,sin(u*(39+j*7)+t*(19+j*3))
                            *cos(u*(23+j*5)-t*(11+j*2)+j*2));
                        float pulse=.35+.65*smoothstep(.05,.75,sin(t*(12+j*2.3)+j*2.7));
                        float filament=exp(-dot(delta,delta)/(.012*.012))*broken*pulse*inside;
                        float hotspot=exp(-dot(p-root,p-root)/(.031*.031))*pulse*inside;
                        float arcD=filament*31+hotspot*9;
                        density+=arcD;
                        weighted+=float3(12,1.1,10)*filament*31*attenuation;
                        weighted+=float3(4.5,.3,5.3)*hotspot*9*attenuation;
                    }

                    weighted*=1-saturate(rupture*front*shellOnly*_Coupling.x*.88);
                    float alpha=1-exp(-density*stride);
                    sum.rgb+=(1-sum.a)*weighted/max(.0001,density)*alpha;
                    sum.a+=(1-sum.a)*alpha;
                    occlusion+=(fissureD*.5+baseD*.3+voidD*.35)*stride;
                    if(sum.a>.998)break;
                }
                return sum;
            }
            ENDHLSL
        }
    }
}
