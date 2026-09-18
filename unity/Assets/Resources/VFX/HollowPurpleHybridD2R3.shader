Shader "JJKGame/Exploration/Purple Body D2-R3"
{
    Properties { _PhaseTime("Clock",Float)=0 }
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
            float _PhaseTime;float4 _R3Settings,_R3Flow,_Sources[6],_PrimaryPower,_SecondaryPower;
            CBUFFER_END
            struct A{float4 positionOS:POSITION;};
            struct V{float4 positionCS:SV_POSITION;float3 p:TEXCOORD0;};
            V Vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.p=i.positionOS.xyz;return o;}
            float H(float3 p){p=frac(p*.3183099+float3(.17,.31,.53));p*=19.19;return frac(p.x*p.y*p.z*(p.x+p.y+p.z));}
            float N(float3 p)
            {
                float3 a=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(lerp(H(a),H(a+float3(1,0,0)),f.x),lerp(H(a+float3(0,1,0)),H(a+float3(1,1,0)),f.x),f.y),
                    lerp(lerp(H(a+float3(0,0,1)),H(a+float3(1,0,1)),f.x),lerp(H(a+float3(0,1,1)),H(a+1),f.x),f.y),f.z);
            }
            float3 Rz(float3 p,float a){float s=sin(a),c=cos(a);return float3(c*p.x-s*p.y,s*p.x+c*p.y,p.z);}
            float3 Ry(float3 p,float a){float s=sin(a),c=cos(a);return float3(c*p.x+s*p.z,p.y,-s*p.x+c*p.z);}
            half4 Frag(V i):SV_Target
            {
                float3 eye=TransformWorldToObject(GetCameraPositionWS()),rd=normalize(i.p-eye);
                float b=dot(eye,rd),h=b*b-dot(eye,eye)+.43*.43;if(h<=0)return 0;
                h=sqrt(h);float begin=max(0,-b-h),end=-b+h;
                float scene=LinearEyeDepth(SampleSceneDepth(i.positionCS.xy/_ScaledScreenParams.xy),_ZBufferParams);
                float3 rayWS=mul((float3x3)unity_ObjectToWorld,rd);
                end=min(end,scene/max(.0001,-mul((float3x3)UNITY_MATRIX_V,rayWS).z));if(end<=begin)return 0;
                float t=_PhaseTime;float3 centres[8],axes[8];float life[8];
                [unroll]for(int j=0;j<8;j++)
                {
                    float seed=j*2.39996,y=lerp(-.82,.82,(j+.5)/8.0),side=sqrt(1-y*y);
                    float phase=frac(t*_R3Flow.x+j*.618034);
                    float3 axis=float3(cos(seed)*side,y,sin(seed)*side);
                    axis=Ry(Rz(axis,.22*sin(seed+phase*2.2)),.16*sin(phase*3.1+seed));
                    axes[j]=axis;centres[j]=axis*lerp(.33,.095,phase);life[j]=smoothstep(0,.13,phase)*(1-smoothstep(.78,1,phase));
                }
                const int steps=160;float stride=(end-begin)/steps;float4 sum=0;float opticalDepth=0;
                [loop]for(int k=0;k<steps;k++)
                {
                    float3 p=eye+rd*(begin+(k+.5)*stride);float r=length(p);
                    // Three broad fields with independent advection. No combed high-frequency axis.
                    float speed=t*_R3Flow.y,scale=_R3Flow.w;
                    float3 flow=p+float3(.055*sin(p.z*11+speed),.048*sin(p.x*9-speed*.7),.05*sin(p.y*12+speed*.43));
                    float macro=N(flow*scale+float3(speed*.68,-speed*.42,.7));
                    float crossFlow=N(Ry(Rz(flow,.83),1.17)*scale*.71+float3(2.8,speed*.53,-speed*.47));
                    float fracture=N(Rz(flow,-.67)*scale*float3(1.15,1.6,1.25)+float3(-speed*.61,3.7,speed*.32));
                    float shear=macro*.55+crossFlow*.30+fracture*.15;
                    float ribbon=smoothstep(.40,.66,shear);
                    float chipped=N(flow*scale*2.1+float3(5.3,speed*.28,-speed*.34));
                    float boundary=1-smoothstep(.035,.12,abs(shear-.5));
                    float shards=smoothstep(.58,.77,chipped)*boundary;
                    ribbon=saturate(ribbon+(chipped-.5)*.5*boundary);
                    float gash=(1-smoothstep(.013,.043,abs(macro-crossFlow*.63-.18)))
                        *smoothstep(.42,.66,fracture);
                    float3 angular=normalize(p+float3(.00001,0,0));
                    float tear=N(Rz(angular,.63)*float3(2,3,2.6)+float3(t*.1,-t*.23,0));
                    float split=pow(smoothstep(.45,.8,tear),3);
                    float pressure=pow(saturate(sin(t*5.7+macro*13)*cos(t*3.1+fracture*11)),6)*_R3Settings.w;
                    float radius=.373+.034*(ribbon*.75-gash*.64)*smoothstep(.25,.6,macro);
                    radius+=shards*(.012+pressure*.013);
                    radius+=.003*_R3Settings.w*sin(t*11.3+angular.y*5)*sin(t*3.7+angular.z*7)*smoothstep(.48,.7,crossFlow);
                    float mass=1-smoothstep(radius-.006,radius+.004,r);
                    float shell=exp(-abs(r-radius+.015)*65);
                    float baseD=mass*(1.8+shell*6*_R3Settings.y);
                    float3 tint=lerp(float3(.045,.001,.23),float3(1.15,.024,.78),smoothstep(.38,.64,shear));
                    float3 bodyColour=tint*_R3Settings.x*lerp(.20,1.3,smoothstep(.30,.68,macro));
                    float3 inner=Rz(p,.47)*exp(t*_R3Flow.x*.16);
                    float fibre=smoothstep(.51,.76,N(inner*scale*.85+sin(inner.zxy*8)*.35+float3(0,-t*.29,t*.21)));
                    float filamentD=fibre*mass*(2+shell*7);
                    float voidD=gash*shell*100*_R3Flow.z;
                    float density=baseD+filamentD+voidD;
                    float3 weighted=bodyColour*baseD+lerp(float3(.55,.006,.70),float3(3.5,.06,2),ribbon)*filamentD+float3(.0002,0,.0006)*voidD;
                    // Distinct depths, irregular emitters and luminous connecting channels fuse perceptually.
                    [unroll]for(int j=0;j<6;j++)
                    {
                        float3 q=p-_Sources[j].xyz;
                        q=Ry(Rz(q,j*.87+t*(.4+j*.06)),j*.41);
                        q+=float3(.025*sin(q.y*36+t*3.7+j),.021*sin(q.z*33-t*4.1+j),.018*sin(q.x*38+t*2.3));
                        float radiusS=_Sources[j].w*(1+.09*sin(t*(9+j*1.3)+j));
                        float3 shaped=q*float3(.78,1.3,.95)/radiusS;
                        float kernel=exp(-dot(shaped,shaped)*1.9);
                        float sourceD=kernel*100*mass;
                        float hotSkin=exp(-dot(shaped,shaped)*.8)*8*mass;
                        float halo=exp(-dot(shaped,shaped)*.36)*2.5*mass;
                        density+=sourceD+hotSkin+halo;
                        float power=j<3?_PrimaryPower[min(j,2)]:_SecondaryPower[max(0,j-3)];
                        float3 absorbed=exp(-opticalDepth*float3(.26,1.4,.48));
                        weighted+=float3(1,.85,.98)*_R3Settings.z*power*sourceD*absorbed
                            +float3(6,.12,2.5)*power*hotSkin+float3(3.5,.1,2.8)*halo;
                    }
                    [unroll]for(int j=3;j<6;j++)
                    {
                        float3 root=_Sources[j-3].xyz,tip=_Sources[j].xyz;
                        float3 chord=tip-root;float u=saturate(dot(p-root,chord)/max(.0001,dot(chord,chord)));
                        float3 bridge=root+chord*u+float3(.012*sin(t*5+j),.018*sin(u*4+j+t*2.3),.013*cos(u*5+t))*sin(u*3.14159);
                        float width=.025+.012*sin(u*3.14159);
                        float link=exp(-dot(p-bridge,p-bridge)/(width*width))*mass;
                        float power=_SecondaryPower[j-3];float linkD=link*22;
                        density+=linkD;
                        weighted+=lerp(float3(6,.2,3),float3(18,7,12),saturate(power))*linkD*exp(-opticalDepth*float3(.3,1.2,.5));
                    }
                    float stream=0;
                    [unroll]for(int j=0;j<8;j++)
                    {
                        float3 delta=p-centres[j];float along=dot(delta,axes[j]);float3 across=delta-axes[j]*along;
                        float shape=dot(across,across)/(.018*.018)+along*along/(.065*.065);
                        stream+=pow(saturate(1-shape),2)*life[j];
                    }
                    float streamD=stream*mass*110*(1-smoothstep(.30,.345,r));
                    float edgeD=exp(-abs(r-radius-.009)*150)*(split*ribbon+shards*(.6+pressure))*22;
                    density+=streamD+edgeD;
                    weighted+=float3(5,.6,4)*streamD+float3(3.8,.016,1.7)*edgeD;
                    float alpha=1-exp(-density*stride);
                    sum.rgb+=(1-sum.a)*weighted/max(.0001,density)*alpha;sum.a+=(1-sum.a)*alpha;
                    opticalDepth+=(baseD+voidD)*stride;
                    if(sum.a>.998)break;
                }
                // Internal transmission is between energy layers, not through the whole body into the stage.
                float3 closest=eye+rd*clamp(-b,begin,end);
                float filled=1-smoothstep(.350,.373,length(closest));
                sum.rgb+=(1-sum.a)*filled*float3(.075,.002,.16);
                sum.a=max(sum.a,filled);
                return sum;
            }
            ENDHLSL
        }
    }
}
