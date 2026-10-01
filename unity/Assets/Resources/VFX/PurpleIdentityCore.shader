Shader "JJKGame/Exploration/Purple Identity Core"
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
            float4 _Identity,_Sources[6],_PrimaryPower,_SecondaryPower;
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
                    // The large mass stays anchored; its edge only breaks locally.
                    float permanent=N(direction*4.2+float3(2.1,7.3,1.4));
                    float local=N(direction*12.7+float3(t*.44,-t*.57,t*.32));
                    float radius=.362+_Identity.y*((permanent-.5)*1.2+(local-.5)*.52);
                    float inside=1-smoothstep(radius-.038,radius+.018,r);
                    if(inside<.0001)continue;
                    float skin=exp(-abs(r-radius+.009)*52);

                    // Three independently advected energy fields change visibility, not the main mass.
                    float3 q=p*12.2;
                    float wave=N(q+float3(t*.79,-t*.41,t*.27));
                    float opposing=N(Rotate(q)*.81+float3(-t*.54,6.1,t*.67));
                    float fine=N(Rotate(q)*1.84+float3(t*1.12,-t*.94,9.2));
                    float emission=smoothstep(.42,.73,wave*.54+opposing*.46);
                    float flash=smoothstep(.65,.83,fine*.56+wave*.44);
                    float narrow=1-smoothstep(.018,.073,abs(wave-opposing*.82-.11));
                    float fissure=narrow*smoothstep(.42,.7,fine)*_Identity.z;
                    float reveal=saturate(emission*.8+flash*.7-fissure*.7);

                    float baseD=inside*(.6+skin*2.4);
                    float plasmaD=inside*(.65+skin*1.5)*(.35+reveal*.65)*(1+_Identity.w*.35);
                    float fissureD=inside*fissure*(1+skin*3.3);
                    float density=baseD+plasmaD+fissureD;
                    float3 weighted=float3(.16,.002,.32)*baseD;
                    weighted+=float3(.9,.012,.58)*skin*baseD;
                    weighted+=lerp(float3(.9,.012,.71),float3(9,.24,5.1),flash)*plasmaD;
                    weighted+=float3(.001,0,.004)*fissureD;

                    // Connected centre: fixed location, uneven luminous boundary and brief hot channels.
                    float3 core=p*float3(.94,1.10,.98);
                    float coreR=.069*(.78+.40*N(core*24+float3(t*.62,-t*.48,2.4)));
                    float coreD=exp(-dot(core,core)/(coreR*coreR)*1.2)*50*inside;
                    float channel=exp(-r*r*35)*pow(smoothstep(.55,.76,wave+fine*.14),2)*inside*24;
                    density+=coreD+channel;
                    float3 attenuation=exp(-occlusion*float3(.18,.69,.27));
                    weighted+=float3(1,.72,.96)*_Identity.x*coreD*attenuation;
                    weighted+=float3(12,2.4,8)*channel*attenuation;

                    float alpha=1-exp(-density*stride);
                    sum.rgb+=(1-sum.a)*weighted/max(.0001,density)*alpha;
                    sum.a+=(1-sum.a)*alpha;
                    occlusion+=(fissureD*.42+baseD*.16)*stride;
                    if(sum.a>.998)break;
                }
                return sum;
            }
            ENDHLSL
        }
    }
}
