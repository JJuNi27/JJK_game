Shader "JJKGame/Exploration/Purple Explosion Turbulence"
{
    Properties { _PhaseTime("Sequence clock", Float) = 0 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
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
            float4 _R3Settings, _R3Flow, _Sources[6], _PrimaryPower, _SecondaryPower;
            float4 _Turbulence, _DepthCore;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; };
            struct V { float4 positionCS:SV_POSITION; float3 p:TEXCOORD0; };
            V Vert(A i) { V o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz); o.p=i.positionOS.xyz; return o; }
            float H(float3 p)
            {
                p=frac(p*.3183099+float3(.17,.31,.53)); p*=19.19;
                return frac(p.x*p.y*p.z*(p.x+p.y+p.z));
            }
            float N(float3 p)
            {
                float3 a=floor(p), f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(lerp(H(a),H(a+float3(1,0,0)),f.x),lerp(H(a+float3(0,1,0)),H(a+float3(1,1,0)),f.x),f.y),
                    lerp(lerp(H(a+float3(0,0,1)),H(a+float3(1,0,1)),f.x),lerp(H(a+float3(0,1,1)),H(a+1),f.x),f.y),f.z);
            }
            float3 Rotate(float3 p)
            {
                return float3(dot(p,float3(.36,.48,-.8)),dot(p,float3(-.8,.6,0)),dot(p,float3(.48,.64,.6)));
            }
            float3 Warp(float3 p, float t)
            {
                // Spatially nonperiodic opposed advection; no polar petals, circular comb or orbit phase.
                float3 q=p*7;
                return float3(N(q+float3(t*.83,3.1,-t*.71)),
                    N(Rotate(q)+float3(7.3,-t*.92,t*.34)),
                    N(q.yzx+float3(-t*.61,t*.57,11.4)))-.5;
            }
            half4 Frag(V i):SV_Target
            {
                float3 eye=TransformWorldToObject(GetCameraPositionWS()), rd=normalize(i.p-eye);
                float b=dot(eye,rd), h=b*b-dot(eye,eye)+.49*.49;
                if(h<=0)return 0;
                h=sqrt(h); float begin=max(0,-b-h), end=-b+h;
                float scene=LinearEyeDepth(SampleSceneDepth(i.positionCS.xy/_ScaledScreenParams.xy),_ZBufferParams);
                float3 originWS=TransformObjectToWorld(eye), rayWS=mul((float3x3)unity_ObjectToWorld,rd);
                end=min(end,(scene+TransformWorldToView(originWS).z)/max(.0001,-mul((float3x3)UNITY_MATRIX_V,rayWS).z));
                if(end<=begin)return 0;
                float t=_PhaseTime*_Turbulence.y;
                float3 drift=float3(t*.73,-t*.47,t*.29);
                const int steps=144;
                float stride=(end-begin)/steps, optical=0;
                float4 sum=0;
                [loop]for(int k=0;k<steps;k++)
                {
                    float3 p=eye+rd*(begin+(k+.5)*stride);
                    float3 warp=Warp(p,t)*_Turbulence.w;
                    float3 q=p+warp;
                    float r=length(q);
                    float broad=N(q*6.4+drift);
                    float opposing=N(Rotate(q)*8.3+float3(-t*.57,t*.89,4.8));
                    float rupture=N(q.yzx*15.1+Warp(q*1.7,t*.74)*.9+float3(t*.91,5.3,-t*.63));
                    float fracture=N(Rotate(q)*26.7+Warp(q*2.6,t*1.9)*1.8+float3(-t*1.2,t*.97,7.1));
                    float split=1-smoothstep(.022,.10,abs(fracture-opposing*.66-.22));
                    float fold=broad*.59+opposing*.41;
                    // Different sectors swell, recede and tear at different times. No regular lobe arrangement.
                    float pressure=N(q*4.1+float3(-t*.48,8.2,t*.77));
                    float radius=.355+_Turbulence.z*((broad-.5)*1.7+(opposing-.5)*.95);
                    radius+=_Turbulence.z*.58*smoothstep(.52,.78,pressure)*smoothstep(.47,.69,rupture);
                    radius-=split*.028*smoothstep(.28,.40,r);
                    float inside=1-smoothstep(radius-.017,radius+.006,r);
                    if(inside<.0001)continue;
                    float shell=exp(-abs(r-radius+.034)*34);
                    float torn=smoothstep(.53,.76,rupture)*smoothstep(.47,.72,opposing);
                    float voidFold=pow(1-smoothstep(.04,.19,abs(broad-opposing*.92-.075)),1.25);
                    float intrusion=smoothstep(.51,.72,N(Rotate(q)*10.7+float3(9.7,-t*.81,t*.44)));
                    float voidMask=saturate(voidFold*.8+intrusion*.55+split*.42)*_DepthCore.x;
                    // Absorbing dark matter is integrated in front of / between luminous volumes.
                    float massD=inside*(3+shell*5)*_DepthCore.w;
                    float darkD=inside*voidMask*(5+shell*14)*_DepthCore.w;
                    float plasmaD=inside*(torn*13+shell*fold*6)*_DepthCore.w*(.45+.9*fracture);
                    float density=massD+darkD+plasmaD;
                    float bright=smoothstep(.50,.76,fold+rupture*.16);
                    float3 tint=lerp(float3(.018,.0005,.09),float3(.85,.015,.8),bright);
                    float3 weighted=tint*massD+float3(.0003,0,.0014)*darkD;
                    weighted+=lerp(float3(.17,.006,.55),float3(4.8,.12,2.9),torn)*plasmaD;
                    float ruptureD=inside*shell*pow(smoothstep(.55,.76,fracture),2)*split*9;
                    density+=ruptureD;
                    weighted+=float3(8,.42,5)*ruptureD;
                    // Broad fused irregular core, not the six separate emitter silhouettes of the prior body.
                    float3 core=q-float3(.017*sin(t*.71),.021*cos(t*.89),.012*sin(t*1.13));
                    core+=Warp(core*2.5,t*1.19)*.055;
                    float coreR=_DepthCore.z*(.87+.23*N(core*24+float3(t*.72,-t*.58,2.7)));
                    float coreKernel=exp(-dot(core*float3(.92,1.12,.97),core*float3(.92,1.12,.97))/(coreR*coreR)*1.4);
                    float coreD=coreKernel*95*inside;
                    float hotVein=pow(smoothstep(.48,.73,rupture)*smoothstep(.51,.72,opposing),2)
                        *exp(-r*r*18)*inside;
                    float veinD=hotVein*19;
                    density+=coreD+veinD;
                    float3 transmission=exp(-optical*float3(.33,1.05,.48));
                    weighted+=float3(1,.84,.96)*_DepthCore.y*coreD*transmission;
                    weighted+=float3(10,1.1,6)*veinD*transmission;
                    float a=1-exp(-density*stride);
                    sum.rgb+=(1-sum.a)*weighted/max(.0001,density)*a*_Turbulence.x;
                    sum.a+=(1-sum.a)*a;
                    optical+=(darkD+massD*.36)*stride;
                    if(sum.a>.998)break;
                }
                return sum;
            }
            ENDHLSL
        }
    }
}
