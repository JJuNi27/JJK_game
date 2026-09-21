Shader "JJKGame/Candidate/Hollow Purple Ingredient Reboot"
{
    Properties { _PhaseTime("Clock",Float)=0 _Polarity("Polarity",Float)=-1 _Fusion("Fusion",Float)=0 _Breakup("Silhouette",Float)=.085 _Emission("Cracks",Float)=2.8 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry+10" }
        Pass
        {
            ZWrite On Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _PhaseTime,_Polarity,_Fusion,_Breakup,_Emission;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct V { float4 positionCS:SV_POSITION; float3 p:TEXCOORD0; float3 world:TEXCOORD1; float3 normal:TEXCOORD2; };
            float H(float3 p){p=frac(p*.3183099+float3(.17,.31,.53));p*=19.19;return frac(p.x*p.y*p.z*(p.x+p.y+p.z));}
            float N(float3 p)
            {
                float3 a=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(lerp(H(a),H(a+float3(1,0,0)),f.x),lerp(H(a+float3(0,1,0)),H(a+float3(1,1,0)),f.x),f.y),lerp(lerp(H(a+float3(0,0,1)),H(a+float3(1,0,1)),f.x),lerp(H(a+float3(0,1,1)),H(a+1),f.x),f.y),f.z);
            }
            V Vert(A i)
            {
                V o;float3 dir=normalize(i.positionOS.xyz);float t=_PhaseTime;
                float fluct=(N(dir*3.7+float3(t,-t*.8,t*.3))-.5)*1.2+(N(dir*8.1-t*.6)-.5)*.4;
                float3 p=i.positionOS.xyz*(1+_Breakup*fluct);
                o.positionCS=TransformObjectToHClip(p);o.p=p;o.world=TransformObjectToWorld(p);o.normal=TransformObjectToWorldNormal(i.normalOS);return o;
            }
            half4 Frag(V i):SV_Target
            {
                float3 p=normalize(i.p),n=normalize(i.normal),v=normalize(GetCameraPositionWS()-i.world);
                float red=step(0,_Polarity),t=_PhaseTime*_Polarity;
                float facing=saturate(dot(n,v)),rim=pow(1-facing,2.6);
                float warp=N(p*3.2+float3(t*.4,-t*.5,t*.2));
                float tear=abs(sin(dot(p,float3(8,13,-5))+warp*8-t*3));
                float cut=1-smoothstep(.025,.15,tear);
                float patches=smoothstep(.43,.66,N(p*5.1+float3(-t*.5,t*.8,1)));
                float cracks=cut*patches;
                float sideCrack=pow(saturate(1-abs(N(p*7.2+warp*1.8-t*.6)-.53)*15),6);
                cracks=max(cracks,sideCrack*.25);
                float3 base=lerp(float3(.002,.006,.026),float3(.034,.0015,.004),red);
                float3 energy=lerp(float3(.014,.09,1.15),float3(1.5,.012,.024),red);
                float skin=.65+.65*warp+.3*saturate(dot(n,normalize(float3(-.4,.7,-.6))));
                float edgeBreak=.12+.88*smoothstep(.28,.7,N(p*6.3+t*.6));
                float centreWeight=lerp(.14,1,smoothstep(.03,.38,1-facing));
                float pressurePulse=1+red*.2*sin(_PhaseTime*11+warp*3);
                float3 result=base*skin+energy*_Emission*(cracks*centreWeight*.65+rim*edgeBreak*.45)*pressurePulse;
                // Both centres remain dense. There is deliberately no view-facing white light.
                return half4(result,1);
            }
            ENDHLSL
        }
    }
}
