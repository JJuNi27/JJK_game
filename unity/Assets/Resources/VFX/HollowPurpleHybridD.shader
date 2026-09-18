Shader "JJKGame/Exploration/Purple Body Hybrid D"
{
    Properties
    {
        _PhaseTime("Clock",Float)=0
        _CoreRatio("Core ratio",Float)=.175 _Density("Shared density",Float)=1.15
        _BodyEmission("Body emission",Float)=.7 _CoreEmission("Core emission",Float)=7
        _NearBlack("Near black",Color)=(.008,.0015,.02,1) _DeepViolet("Deep violet",Color)=(.065,.008,.18,1)
        _Magenta("Magenta",Color)=(.8,.016,.27,1) _Violet("Violet",Color)=(.26,.035,.9,1) _HotPink("Hot pink",Color)=(1.8,.055,.65,1)
        _HybridShape("Tear / depth / stream / edge",Vector)=(.032,.98,3.3,.25)
        _HybridFlow("Implosion / surface",Vector)=(.58,.24,0,0)
    }
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
            float _PhaseTime,_CoreRatio,_Density,_BodyEmission,_CoreEmission;
            float4 _NearBlack,_DeepViolet,_Magenta,_Violet,_HotPink,_HybridShape,_HybridFlow;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; };
            struct V { float4 positionCS:SV_POSITION; float3 p:TEXCOORD0; };
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
                float b=dot(eye,rd),h=b*b-dot(eye,eye)+.43*.43;
                if(h<=0)return 0;
                h=sqrt(h);float begin=max(0,-b-h),end=-b+h;
                float scene=LinearEyeDepth(SampleSceneDepth(i.positionCS.xy/_ScaledScreenParams.xy),_ZBufferParams);
                float3 rayWS=mul((float3x3)unity_ObjectToWorld,rd);
                end=min(end,scene/max(.0001,-mul((float3x3)UNITY_MATRIX_V,rayWS).z));if(end<=begin)return 0;
                float clock=_PhaseTime;
                // Eight compact 3D streaks physically change radius. No concentric rings or rotating hollow shell.
                float3 streamCentre[8],streamAxis[8];float life[8];
                [unroll] for(int j=0;j<8;j++)
                {
                    float seed=j*2.39996;
                    float y=lerp(-.82,.82,(j+.5)/8.0),side=sqrt(1-y*y);
                    float phase=frac(clock*_HybridFlow.x+j*.618034);
                    float3 direction=float3(cos(seed)*side,y,sin(seed)*side);
                    direction=Ry(Rz(direction,.22*sin(seed+phase*2.2)),.16*sin(phase*3.1+seed));
                    streamAxis[j]=direction;
                    streamCentre[j]=direction*lerp(.355,.10,phase);
                    life[j]=smoothstep(0,.13,phase)*(1-smoothstep(.78,1,phase));
                }
                const int steps=176;
                float stride=(end-begin)/steps;float4 sum=0;
                [loop] for(int k=0;k<steps;k++)
                {
                    float3 p=eye+rd*(begin+(k+.5)*stride);float r=length(p);
                    float3 angular=normalize(p+float3(.00001,0,0));
                    float3 tearCoord=Rz(angular,.63)*float3(3,9,5)+float3(clock*.10,-clock*_HybridFlow.y,clock*.07);
                    float tearNoise=N(tearCoord);
                    float slit=pow(saturate(1-abs(tearNoise-.53)*8),2);
                    float tearGate=smoothstep(.25,.60,N(angular*5+float3(0,clock*.11,0)));
                    float radius=.375+_HybridShape.x*(pow(saturate(tearNoise),3)*.20-slit*.70)*tearGate;
                    float mass=1-smoothstep(radius-.006,radius+.002,r);
                    float skin=exp(-abs(r-radius)*130);
                    float3 surface=Ry(Rz(p,-clock*_HybridFlow.y*.30),clock*_HybridFlow.y*.17);
                    float surfN=N(surface*18+sin(p.yzx*12-clock*_HybridFlow.y)*.5);
                    float filaments=pow(saturate(1-abs(surfN-.55)*16),6)*smoothstep(.48,.70,N(surface*8+.7));
                    // Solid absorption persists through the entire diameter; the centre is never carved out.
                    float inner=1-smoothstep(.18,.25,r);
                    float baseD=mass*lerp(55,12+inner*52,_HybridShape.y);
                    float lit=.45+.55*saturate(dot(angular,normalize(float3(-.45,.7,-.5))));
                    float3 baseColour=lerp(_NearBlack.rgb*.65,_DeepViolet.rgb*.80,smoothstep(.25,.8,surfN))*lit;
                    float surfaceD=mass*skin*filaments*22;
                    // Nonperiodic interior creases contract through a filled volume, with no annular topology.
                    float3 innerCoord=Rz(p,.47)*exp(clock*_HybridFlow.x*.32)*19;
                    float innerNoise=N(innerCoord+sin(p.zxy*17)*.55);
                    float innerCrease=pow(saturate(1-abs(innerNoise-.55)*9),4);
                    float interiorAccent=smoothstep(.54,.76,N(innerCoord*.28+float3(.7,2.1,-.4)));
                    float innerGate=smoothstep(.09,.17,r)*(1-smoothstep(.29,.36,r));
                    float creaseD=innerCrease*innerGate*mass*interiorAccent*36;
                    float stream=0;
                    [unroll] for(int j=0;j<8;j++)
                    {
                        float3 delta=p-streamCentre[j];float along=dot(delta,streamAxis[j]);
                        float3 across=delta-streamAxis[j]*along;
                        float shape=dot(across,across)/(.011*.011)+along*along/(.085*.085);
                        stream+=pow(saturate(1-shape),2)*life[j];
                    }
                    float streamD=stream*mass*120;
                    float edgeD=exp(-abs(r-radius-.005)*230)*slit*tearGate*_HybridShape.w*9;
                    float density=baseD+surfaceD+streamD+edgeD+creaseD;
                    float3 streamColour=lerp(_Violet.rgb,_Magenta.rgb,.18+surfN*.20)*_HybridShape.z;
                    float3 colour=(baseColour*baseD+_Violet.rgb*surfaceD*1.3+streamColour*streamD
                        +lerp(_Violet.rgb,_Magenta.rgb,.3)*edgeD*.8
                        +lerp(_Violet.rgb,_Magenta.rgb,innerNoise*.38)*creaseD*_HybridShape.z*.55)/max(.0001,density);
                    float alpha=1-exp(-density*_Density*stride);
                    sum.rgb+=(1-sum.a)*colour*_BodyEmission*alpha;sum.a+=(1-sum.a)*alpha;
                    if(sum.a>.997)break;
                }
                // Limit broad body HDR; only the shared compact core and thin rim exceed the bloom threshold.
                sum.rgb=min(sum.rgb,float3(.85,.85,.85));
                float3 nearest=eye+rd*clamp(-b,begin,end);float cr=.375*_CoreRatio;
                float distance=length(nearest)+(N(nearest*110+clock*.5)-.5)*.003;
                float core=1-smoothstep(cr*.87,cr,distance);
                float rim=exp(-abs(distance-cr*1.045)*480);
                sum.rgb+=core*float3(1,.9,1)*_CoreEmission+rim*_HotPink.rgb*.7;
                sum.a=max(sum.a,saturate(core+rim*.4));return sum;
            }
            ENDHLSL
        }
    }
}
