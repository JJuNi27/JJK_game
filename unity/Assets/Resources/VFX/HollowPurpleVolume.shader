Shader "JJKGame/VFX/Hollow Purple Volume"
{
    Properties
    {
        _PhaseTime("Flow clock", Float) = 0
        _Density("Density", Float) = 1
        _Emission("Emission", Float) = 1
        _Chaos("Turbulence", Float) = 1
        _Haze("Haze", Float) = 0.4
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend One OneMinusSrcAlpha
            ZWrite Off
            Cull Front
            ZTest Always
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _PhaseTime, _Density, _Emission, _Chaos, _Haze;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; };
            struct V { float4 positionCS:SV_POSITION; float3 positionOS:TEXCOORD0; };
            V Vert(A i) { V o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz); o.positionOS=i.positionOS.xyz; return o; }
            float Hash(float3 p) { p=frac(p*.3183099+float3(.17,.31,.53)); p*=19.19; return frac(p.x*p.y*p.z*(p.x+p.y+p.z)); }
            float Noise(float3 p)
            {
                float3 a=floor(p), f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(lerp(Hash(a),Hash(a+float3(1,0,0)),f.x),lerp(Hash(a+float3(0,1,0)),Hash(a+float3(1,1,0)),f.x),f.y),
                    lerp(lerp(Hash(a+float3(0,0,1)),Hash(a+float3(1,0,1)),f.x),lerp(Hash(a+float3(0,1,1)),Hash(a+1),f.x),f.y),f.z);
            }
            half4 Frag(V i):SV_Target
            {
                float3 eye=TransformWorldToObject(GetCameraPositionWS());
                float3 rd=normalize(i.positionOS-eye);
                float b=dot(eye,rd), h=b*b-dot(eye,eye)+.25;
                if(h<=0) return 0;
                h=sqrt(h); float begin=max(0,-b-h), end=-b+h;
                float2 uv=i.positionCS.xy/_ScaledScreenParams.xy;
                float scene=LinearEyeDepth(SampleSceneDepth(uv),_ZBufferParams);
                float3 originWS=TransformObjectToWorld(eye);
                float3 rayWS=mul((float3x3)unity_ObjectToWorld,rd);
                float sceneLimit=(scene+TransformWorldToView(originWS).z)/max(.0001,-mul((float3x3)UNITY_MATRIX_V,rayWS).z);
                end=min(end,sceneLimit);
                if(end<=begin) return 0;
                // True depth integration: opaque hot centre, turbulent middle and sparse outer cloud.
                float stride=(end-begin)/32;
                float jitter=.5;
                float4 sum=0;
                [loop] for(int k=0;k<32;k++)
                {
                    float3 p=eye+rd*(begin+(k+jitter)*stride);
                    float t=_PhaseTime;
                    float3 flow=p*9+float3(t*.52,-t*.71,t*.24);
                    flow+=float3(sin(p.y*17+t),cos(p.z*13-t*.7),sin(p.x*11+t*.8))*.65*_Chaos;
                    float n=Noise(flow)*.68+Noise(flow*2.13-t*.4)*.32;
                    float r=length(p);
                    // Turbulence travels over a spherical envelope, never displacing its entire mass.
                    float surface=.305+(n-.5)*.012*min(_Chaos,2);
                    float core=1-smoothstep(.073,.116+n*.045,r);
                    float mass=saturate((surface-r)*160)*(1-core*.8);
                    float folds=pow(saturate(1-abs(n-.52)*9),3);
                    float shell=exp(-abs(r-(.318+(n-.5)*.025))*190)*smoothstep(.65,.84,n);
                    float haze=exp(-r*r*21)*smoothstep(.26,.33,r)*(1-smoothstep(.39,.49,r))*_Haze;
                    float density=(core*55+mass*32+shell*6+haze*.2)*_Density;
                    float alpha=1-exp(-density*stride);
                    float3 col=lerp(float3(.018,.001,.046),float3(.48,.007,.22),smoothstep(.48,.76,n));
                    col+=folds*mass*float3(.26,.012,.22);
                    col*=.55+.65*saturate(dot(normalize(p),normalize(float3(-.4,.65,-.6)))*.5+.5);
                    // The centre alone carries the extreme HDR energy; the mass absorbs light.
                    col=lerp(col,float3(18,15,20),core);
                    col=lerp(col,float3(.15,.025,.48),saturate(shell*1.3+haze*2));
                    sum.rgb+=(1-sum.a)*col*alpha*_Emission;
                    sum.a+=(1-sum.a)*alpha;
                    if(sum.a>.985) break;
                }
                // Confine the extreme radiance to a small, irregular projected core.
                // Clamp to scene depth so this cannot glow through an opaque foreground surface.
                float3 closest=eye+rd*clamp(-b,begin,end);
                float coreDistance=length(closest*float3(1.05,.95,1));
                coreDistance+=sin(closest.x*91+closest.y*53+_PhaseTime)*.004;
                float coreLight=1-smoothstep(.038,.062,coreDistance);
                sum.rgb+=coreLight*float3(4.5,3.8,5)*_Emission;
                sum.a=max(sum.a,coreLight);
                return sum;
            }
            ENDHLSL
        }
    }
}
