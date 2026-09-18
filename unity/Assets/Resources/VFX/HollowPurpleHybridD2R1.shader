Shader "JJKGame/Exploration/Purple Body D2-R1"
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
            float _PhaseTime;float4 _R1Settings,_R1Flow,_Sources[4],_SourcePower;
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
                    float phase=frac(t*_R1Flow.x+j*.618034);
                    float3 axis=float3(cos(seed)*side,y,sin(seed)*side);
                    axis=Ry(Rz(axis,.22*sin(seed+phase*2.2)),.16*sin(phase*3.1+seed));
                    axes[j]=axis;centres[j]=axis*lerp(.33,.095,phase);life[j]=smoothstep(0,.13,phase)*(1-smoothstep(.78,1,phase));
                }
                const int steps=160;float stride=(end-begin)/steps;float4 sum=0;float opticalDepth=0;
                [loop]for(int k=0;k<steps;k++)
                {
                    float3 p=eye+rd*(begin+(k+.5)*stride);float r=length(p);
                    float3 surface=Ry(Rz(p,.61-t*_R1Flow.y*.44),.37+t*_R1Flow.y*.19);
                    float surfaceN=N(surface*19+sin(p.yzx*17-t*_R1Flow.y)*.55);
                    float macro=N(surface*8+sin(surface.zxy*11+t*.23)*.62);
                    float3 angular=normalize(p+float3(.00001,0,0));
                    float tear=N(Rz(angular,.63)*float3(3,9,5)+float3(t*.1,-t*.23,0));
                    float split=pow(saturate(1-abs(tear-.53)*8),2);
                    float radius=.375+.018*(pow(saturate(tear),3)-split*.7)*smoothstep(.25,.6,macro);
                    radius+=.0015*_R1Settings.w*sin(t*11.3+angular.y*5)*sin(t*3.7+angular.z*7);
                    float mass=1-smoothstep(radius-.006,radius+.004,r);
                    float shell=exp(-abs(r-radius+.015)*65);
                    float darkVein=pow(saturate((.48-macro)*4),2);
                    float baseD=mass*(1.9+shell*7*_R1Settings.y+darkVein*4);
                    float3 tint=lerp(float3(.24,.018,1.1),float3(.75,.024,.65),surfaceN);
                    float3 bodyColour=lerp(tint,float3(.035,.001,.12),darkVein)*_R1Settings.x
                        *lerp(.28,1,smoothstep(.28,.62,macro));
                    float innerN=N(Rz(p,.47)*exp(t*_R1Flow.x*.32)*20+sin(p.zxy*17)*.55);
                    float crease=pow(saturate(1-abs(innerN-.55)*10),5)*mass;
                    float creaseD=crease*3*(.10+shell);
                    float density=baseD+creaseD;
                    float3 weighted=bodyColour*baseD+float3(2.9,.025,1.45)*creaseD;
                    // White emission exists at bounded internal 3D sources, not in the surface noise field.
                    [unroll]for(int j=0;j<4;j++)
                    {
                        float3 q=p-_Sources[j].xyz;
                        float boiling=N(q*72+float3(t*(2.2+j*.31),-t*1.7,t*.9))-.5;
                        float sourceRadius=_Sources[j].w*(1+.055*sin(t*(9+j*1.3)+j));
                        float d=length(q*float3(1.08,.91,1.02))-sourceRadius+boiling*(j==0?.024:.014);
                        float sourceD=(1-smoothstep(-.008,.009,d))*115*mass;
                        float hotSkin=exp(-abs(d)*110)*18*mass;
                        float halo=exp(-length(q)*30)*mass;
                        density+=sourceD+hotSkin+halo;
                        float3 absorbed=exp(-opticalDepth*float3(.32,1.9,.55));
                        weighted+=float3(1,.72,.96)*_R1Settings.z*_SourcePower[j]*sourceD*absorbed*(j==0?1:.72)
                            +float3(5.5,.04,1.9)*_SourcePower[j]*hotSkin+float3(3,.03,2.4)*halo;
                    }
                    float stream=0;
                    [unroll]for(int j=0;j<8;j++)
                    {
                        float3 delta=p-centres[j];float along=dot(delta,axes[j]);float3 across=delta-axes[j]*along;
                        float shape=dot(across,across)/(.009*.009)+along*along/(.057*.057);
                        stream+=pow(saturate(1-shape),2)*life[j];
                    }
                    float streamD=stream*mass*110*(1-smoothstep(.30,.345,r));
                    float edgeD=exp(-abs(r-radius-.009)*150)*split*smoothstep(.46,.70,macro)*16;
                    density+=streamD+edgeD;
                    weighted+=float3(5,.6,4)*streamD+float3(3.8,.016,1.7)*edgeD;
                    float alpha=1-exp(-density*stride);
                    sum.rgb+=(1-sum.a)*weighted/max(.0001,density)*alpha;sum.a+=(1-sum.a)*alpha;
                    opticalDepth+=baseD*stride;
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
