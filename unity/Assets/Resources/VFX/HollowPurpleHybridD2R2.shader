Shader "JJKGame/Exploration/Purple Body D2-R2"
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
            float _PhaseTime;float4 _R2Settings,_R2Flow,_Sources[6],_PrimaryPower,_SecondaryPower;
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
                    float phase=frac(t*_R2Flow.x+j*.618034);
                    float3 axis=float3(cos(seed)*side,y,sin(seed)*side);
                    axis=Ry(Rz(axis,.22*sin(seed+phase*2.2)),.16*sin(phase*3.1+seed));
                    axes[j]=axis;centres[j]=axis*lerp(.33,.095,phase);life[j]=smoothstep(0,.13,phase)*(1-smoothstep(.78,1,phase));
                }
                const int steps=160;float stride=(end-begin)/steps;float4 sum=0;float opticalDepth=0;
                [loop]for(int k=0;k<steps;k++)
                {
                    float3 p=eye+rd*(begin+(k+.5)*stride);float r=length(p);
                    float3 surface=Ry(Rz(p,.61-t*_R2Flow.y*.44),.37+t*_R2Flow.y*.19);
                    float3 flow=surface+float3(.10*sin(surface.z*9+t*.8),.018*sin(surface.x*23-t),.04*sin(surface.y*12+t*.3));
                    float shear=N(flow*float3(6,62,19)+float3(t*.4,-t*.9,t*.17));
                    float macro=N(flow*float3(8,4,11)+float3(0,t*.13,0));
                    float ribbon=pow(smoothstep(.48,.83,shear),2);
                    float gash=smoothstep(.77,.88,N(flow*float3(5,48,13)+float3(1.7,-t*.48,2.3)))
                        *smoothstep(.40,.60,macro);
                    float3 angular=normalize(p+float3(.00001,0,0));
                    float tear=N(Rz(angular,.63)*float3(3,9,5)+float3(t*.1,-t*.23,0));
                    float split=pow(smoothstep(.45,.8,tear),3);
                    float radius=.375+.024*(ribbon*.65-gash*.8)*smoothstep(.25,.6,macro);
                    radius+=.002*_R2Settings.w*sin(t*11.3+angular.y*5)*sin(t*3.7+angular.z*7);
                    float mass=1-smoothstep(radius-.006,radius+.004,r);
                    float shell=exp(-abs(r-radius+.015)*65);
                    float baseD=mass*(1.8+shell*6*_R2Settings.y);
                    float3 tint=lerp(float3(.12,.006,.65),float3(1.0,.035,.9),smoothstep(.36,.68,shear));
                    float3 bodyColour=tint*_R2Settings.x*lerp(.35,1,smoothstep(.22,.62,macro));
                    float3 inner=Rz(p,.47)*exp(t*_R2Flow.x*.32);
                    float fibre=pow(smoothstep(.54,.82,N(inner*float3(7,55,23)+sin(inner.zxy*12)*.3)),3.5);
                    float filamentD=fibre*mass*(4+shell*12);
                    float voidD=gash*shell*220*_R2Flow.z;
                    float density=baseD+filamentD+voidD;
                    float3 weighted=bodyColour*baseD+float3(3,.045,1.8)*filamentD+float3(.0002,0,.0006)*voidD;
                    // A soft, overlapping cluster of internal emitters replaces the single hard white disc.
                    [unroll]for(int j=0;j<6;j++)
                    {
                        float3 q=p-_Sources[j].xyz;
                        q=Ry(Rz(q,j*.87+t*(.4+j*.06)),j*.41);
                        q+=float3(.014*sin(q.y*61+t*3.7+j),.013*sin(q.z*49-t*4.1),.008*sin(q.x*71+t*2.3));
                        float radiusS=_Sources[j].w*(1+.09*sin(t*(9+j*1.3)+j));
                        float3 shaped=q*float3(.8,1.35,1.0)/radiusS;
                        float kernel=exp(-dot(shaped,shaped)*2.8);
                        float sourceD=kernel*145*mass;
                        float hotSkin=exp(-dot(shaped,shaped)*1.15)*9*mass;
                        float halo=exp(-dot(shaped,shaped)*.55)*2*mass;
                        density+=sourceD+hotSkin+halo;
                        float power=j<3?_PrimaryPower[min(j,2)]:_SecondaryPower[max(0,j-3)];
                        float3 absorbed=exp(-opticalDepth*float3(.26,1.4,.48));
                        weighted+=float3(1,.85,.98)*_R2Settings.z*power*sourceD*absorbed
                            +float3(6,.12,2.5)*power*hotSkin+float3(3.5,.1,2.8)*halo;
                    }
                    float stream=0;
                    [unroll]for(int j=0;j<8;j++)
                    {
                        float3 delta=p-centres[j];float along=dot(delta,axes[j]);float3 across=delta-axes[j]*along;
                        float shape=dot(across,across)/(.009*.009)+along*along/(.057*.057);
                        stream+=pow(saturate(1-shape),2)*life[j];
                    }
                    float streamD=stream*mass*110*(1-smoothstep(.30,.345,r));
                    float edgeD=exp(-abs(r-radius-.009)*150)*split*ribbon*22;
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
