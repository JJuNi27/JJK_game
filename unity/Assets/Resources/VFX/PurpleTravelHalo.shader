Shader "JJKGame/Production/Purple Travel Halo"
{
    Properties { _Centre("Centre",Vector)=(0,0,0,2.5) _Halo("Halo",Vector)=(1.55,.38,0,0) }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+30" "RenderType"="Transparent"}
        Pass
        {
            Blend One One
            ZWrite Off ZTest Always Cull Front
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Centre,_Halo,_Motion;float _RearStretch;
            CBUFFER_END
            struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 world:TEXCOORD0;};
            V Vert(A i){V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);return o;}
            float Hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
            float Noise(float3 p)
            {
                float3 a=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(lerp(Hash(a),Hash(a+float3(1,0,0)),f.x),lerp(Hash(a+float3(0,1,0)),Hash(a+float3(1,1,0)),f.x),f.y),
                    lerp(lerp(Hash(a+float3(0,0,1)),Hash(a+float3(1,0,1)),f.x),lerp(Hash(a+float3(0,1,1)),Hash(a+1),f.x),f.y),f.z);
            }
            half4 Frag(V i):SV_Target
            {
                float3 eye=(GetCameraPositionWS()-_Centre.xyz)/_Centre.w,rd=normalize(i.world-GetCameraPositionWS());
                float b=dot(eye,rd),rmax=_Halo.x*1.22+_Motion.w*_RearStretch,h=b*b-dot(eye,eye)+rmax*rmax;if(h<=0)return 0;
                h=sqrt(h);float begin=max(0,-b-h),end=-b+h;
                float scene=LinearEyeDepth(SampleSceneDepth(i.p.xy/_ScaledScreenParams.xy),_ZBufferParams);
                end=min(end,scene/(_Centre.w*max(.001,-mul((float3x3)UNITY_MATRIX_V,rd).z)));if(end<=begin)return 0;
                // Rays across the body remain untouched, regardless of the viewing angle.
                float clearBody=smoothstep(1.045,1.16,length(eye-rd*b));
                float t=_Halo.z,stride=(end-begin)/80,sum=0;
                [loop]for(int k=0;k<80;k++)
                {
                    float3 p=eye+rd*(begin+(k+.5)*stride);float r=length(p);float3 d=p/max(.001,r);
                    float rear=saturate(-dot(d,_Motion.xyz)),front=saturate(dot(d,_Motion.xyz));
                    float large=Noise(d*3.1+float3(t*.24,-t*.19,t*.11));
                    float tear=Noise(d*8.7+float3(-t*.47,1.7,t*.31)+_Motion.xyz*t*rear*_Motion.w*1.3);
                    float gap=smoothstep(.35,.66,large);
                    float thickness=lerp(.045,.14,smoothstep(.25,.78,large));
                    float radius=_Halo.x+.13*(large-.5)*2+.044*(tear-.5)*2;
                    radius+=_Motion.w*_RearStretch*(rear*rear-front*.28);
                    thickness*=1+rear*_Motion.w*.25;
                    float crest=exp(-pow((r-radius)/thickness,2)*2);
                    float rupture=smoothstep(.27,.65,tear)*gap;
                    float diffuse=exp(-abs(r-radius+.09)*10)*gap*.20;
                    sum+=(crest*rupture*2.8+diffuse)*stride;
                }
                return half4(float3(1.3,.012,.65)*sum*_Halo.y*clearBody,0);
            }
            ENDHLSL
        }
    }
}
