Shader "JJKGame/Exploration/Purple Body Hybrid D2"
{
    Properties
    {
        _PhaseTime("Clock",Float)=0
        _D2Energy("Luminosity / separation / plasma / rupture",Vector)=(2.8,.32,7.5,1.4)
        _D2Flow("Inward / turbulence",Vector)=(.58,.7,0,0)
    }
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
            float _PhaseTime;float4 _D2Energy,_D2Flow;
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
                float b=dot(eye,rd),h=b*b-dot(eye,eye)+.45*.45;if(h<=0)return 0;
                h=sqrt(h);float begin=max(0,-b-h),end=-b+h;
                float scene=LinearEyeDepth(SampleSceneDepth(i.positionCS.xy/_ScaledScreenParams.xy),_ZBufferParams);
                float3 rayWS=mul((float3x3)unity_ObjectToWorld,rd);
                end=min(end,scene/max(.0001,-mul((float3x3)UNITY_MATRIX_V,rayWS).z));if(end<=begin)return 0;
                float clock=_PhaseTime;
                float3 centres[8],axes[8];float life[8];
                [unroll]for(int j=0;j<8;j++)
                {
                    float seed=j*2.39996;float y=lerp(-.82,.82,(j+.5)/8.0),side=sqrt(1-y*y);
                    float phase=frac(clock*_D2Flow.x+j*.618034);
                    float3 direction=float3(cos(seed)*side,y,sin(seed)*side);
                    direction=Ry(Rz(direction,.22*sin(seed+phase*2.2)),.16*sin(phase*3.1+seed));
                    axes[j]=direction;centres[j]=direction*lerp(.355,.10,phase);
                    life[j]=smoothstep(0,.13,phase)*(1-smoothstep(.78,1,phase));
                }
                const int steps=144;float stride=(end-begin)/steps;float4 sum=0;
                [loop]for(int k=0;k<steps;k++)
                {
                    float3 p=eye+rd*(begin+(k+.5)*stride);float r=length(p);float3 angular=normalize(p+float3(.00001,0,0));
                    float3 surface=Ry(Rz(p,.61-clock*_D2Flow.y*.44),.37+clock*_D2Flow.y*.19);
                    float surfaceN=N(surface*19+sin(p.yzx*17-clock*_D2Flow.y)*.55);
                    float macro=N(surface*8+sin(surface.zxy*11+clock*.23)*.62+float3(clock*.08,0,-clock*.13));
                    float tear=N(Rz(angular,.63)*float3(3,9,5)+float3(clock*.1,-clock*.23,0));
                    float split=pow(saturate(1-abs(tear-.53)*8),2);
                    float radius=.375+.018*(pow(saturate(tear),3)-split*.7)*smoothstep(.25,.6,macro);
                    float mass=1-smoothstep(radius-.006,radius+.004,r);
                    float coreMass=1-smoothstep(.18,.25,r);
                    float baseD=mass*(20+coreMass*48);
                    float alive=smoothstep(_D2Energy.y-.06,_D2Energy.y+.08,macro);
                    float3 tint=lerp(float3(.20,.008,.9),float3(1.15,.03,.65),surfaceN);
                    float3 baseColour=lerp(float3(.025,.001,.075),tint,alive)*_D2Energy.x;
                    float3 innerCoord=Rz(p,.47)*exp(clock*_D2Flow.x*.32)*20;
                    float innerN=N(innerCoord+sin(p.zxy*17)*.55);
                    float crack=pow(saturate(1-abs(innerN-.55)*10),5);
                    float crackD=crack*mass*10*(.15+.85*alive);
                    float hotField=N(surface*float3(18,31,13)+sin(surface.yzx*23-clock*.6)*.9);
                    float hotspot=pow(saturate(1-abs(hotField-.52)*7),1.6)
                        *smoothstep(.48,.72,N(surface*12+sin(surface.zxy*17)*.65));
                    float sparkD=hotspot*smoothstep(.40,.60,macro)*mass*110;
                    float stream=0;
                    [unroll]for(int j=0;j<8;j++)
                    {
                        float3 delta=p-centres[j];float along=dot(delta,axes[j]);float3 across=delta-axes[j]*along;
                        float shape=dot(across,across)/(.014*.014)+along*along/(.08*.08);
                        stream+=pow(saturate(1-shape),2)*life[j];
                    }
                    float streamD=stream*mass*95;
                    float glowD=exp(-abs(r-radius-.012)*125)*split*smoothstep(.46,.70,macro)*22;
                    float density=baseD+crackD+sparkD+streamD+glowD;
                    float3 colour=(baseColour*baseD+float3(2.8,.12,2.1)*crackD*_D2Energy.x
                        +float3(10,6,9)*sparkD+float3(8,2.2,7)*streamD
                        +float3(4,.025,1.6)*glowD)/max(.0001,density);
                    float alpha=1-exp(-density*stride);
                    sum.rgb+=(1-sum.a)*colour*alpha;sum.a+=(1-sum.a)*alpha;if(sum.a>.997)break;
                }
                // An animated union of warped plasma lobes, not the preserved D's analytic white circle.
                float3 nearest=eye+rd*clamp(-b,begin,end);
                float3 q=Ry(Rz(nearest,clock*.31),-.22);
                float central=length(q*float3(1.1,.91,1.04))-(.061+.006*sin(clock*9));
                float3 lobeA=float3(.041*sin(clock*2.3),.044*cos(clock*1.8),.024*sin(clock*3));
                float3 lobeB=float3(-.046*cos(clock*2),-.036*sin(clock*2.7),-.02);
                float field=min(central,min(length(q-lobeA)-.041,length(q-lobeB)-.038));
                field+=(N(q*67+float3(clock*2.2,-clock*1.7,clock*.9))-.5)*.032;
                float plasma=1-smoothstep(-.004,.009,field);
                float hotTransition=exp(-abs(field)*80);
                float heat=.80+.20*sin(clock*17+N(q*43)*9);
                sum.rgb+=plasma*float3(1,.72,.94)*_D2Energy.z*heat+hotTransition*float3(3,.025,.8);
                sum.a=max(sum.a,saturate(plasma+hotTransition*.55));
                return sum;
            }
            ENDHLSL
        }
    }
}
