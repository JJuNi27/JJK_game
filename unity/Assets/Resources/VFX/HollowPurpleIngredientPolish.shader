Shader "JJKGame/Candidate/Hollow Purple Ingredient Polish"
{
    Properties { _PhaseTime("Clock",Float)=0 _Polarity("Polarity",Float)=-1 _Fusion("Fusion",Float)=0 _Breakup("Silhouette",Float)=.055 }
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
            float _PhaseTime, _Polarity, _Fusion, _Breakup;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct V { float4 positionCS:SV_POSITION; float3 p:TEXCOORD0; float3 world:TEXCOORD1; float3 normal:TEXCOORD2; };

            float H(float3 p) { p=frac(p*.3183099+float3(.17,.31,.53)); p*=19.19; return frac(p.x*p.y*p.z*(p.x+p.y+p.z)); }
            float N(float3 p)
            {
                float3 a=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(lerp(H(a),H(a+float3(1,0,0)),f.x),lerp(H(a+float3(0,1,0)),H(a+float3(1,1,0)),f.x),f.y),
                    lerp(lerp(H(a+float3(0,0,1)),H(a+float3(1,0,1)),f.x),lerp(H(a+float3(0,1,1)),H(a+1),f.x),f.y),f.z);
            }
            V Vert(A i)
            {
                V o;float3 dir=normalize(i.positionOS.xyz);float clock=_PhaseTime*.8;
                float fluctuation=(N(dir*3.1+float3(clock,-clock*.6,1.7))-.5)*1.4
                    +(N(dir*7.3+float3(-clock*.8,2.3,clock*.7))-.5)*.6;
                float3 displaced=i.positionOS.xyz*(1+_Breakup*fluctuation);
                o.positionCS=TransformObjectToHClip(displaced);o.p=displaced;o.world=TransformObjectToWorld(displaced);
                o.normal=TransformObjectToWorldNormal(i.normalOS);return o;
            }
            half4 Frag(V i):SV_Target
            {
                float3 p=normalize(i.p), n=normalize(i.normal), v=normalize(GetCameraPositionWS()-i.world);
                float t=_PhaseTime*_Polarity;
                float3 flow=p*6+sin(p.yzx*7+t)*.6+float3(t*.5,-t*.7,t*.3);
                float noise=N(flow)*.65+N(flow*2.17-t*.2)*.35;
                float fissure=pow(saturate(1-abs(noise-.52)*7),9)*smoothstep(.30,.65,N(flow*3.1));
                float facing=saturate(dot(n,v));
                float rim=pow(1-facing,3);
                float red=step(0,_Polarity);
                float3 dark=lerp(float3(.003,.018,.095),float3(.12,.001,.012),red);
                float3 colour=lerp(float3(.025,.65,1.8),float3(1.5,.012,.035),red);
                colour=lerp(colour,float3(.7,.02,1.1),_Fusion*.65);
                float light=.8+.2*saturate(dot(n,normalize(float3(-.5,.8,-.3))));
                float edgeBreak=smoothstep(.25,.72,N(p*8+float3(t*.2,-t*.3,1.1)));
                float3 result=dark+colour*(fissure*2.2+rim*.30*edgeBreak+smoothstep(.48,.76,noise)*.09)*light;
                // Blue retains a dense dark sink. Red is an additive pressure light with
                // a broad red shoulder, never a hard replacement disc on the surface.
                float coreNoise=N(p*10.7+float3(t*.8,-t*.3,t*.5));
                if(red<.5)
                {
                    float sink=exp(-(1-facing)*(25+coreNoise*12));
                    result=lerp(result,float3(.001,.004,.018),sink);
                }
                else
                {
                    float hot=exp(-(1-facing)*(52+(coreNoise-.5)*35));
                    float shoulder=exp(-(1-facing)*13);
                    result+=float3(5,2.6,2.9)*hot+colour*shoulder*.65;
                }
                return half4(result,1);
            }
            ENDHLSL
        }
    }
}
