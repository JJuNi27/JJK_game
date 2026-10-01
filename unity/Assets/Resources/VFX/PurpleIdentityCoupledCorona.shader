Shader "JJKGame/Exploration/Purple Identity Coupled Corona"
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
            float4 _Centre,_Halo;
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
                float b=dot(eye,rd),rmax=_Halo.x*1.22,h=b*b-dot(eye,eye)+rmax*rmax;if(h<=0)return 0;
                h=sqrt(h);float begin=max(0,-b-h),end=-b+h;
                float scene=LinearEyeDepth(SampleSceneDepth(i.p.xy/_ScaledScreenParams.xy),_ZBufferParams);
                end=min(end,scene/(_Centre.w*max(.001,-mul((float3x3)UNITY_MATRIX_V,rd).z)));if(end<=begin)return 0;
                // Sparse asymmetric energy pockets, never a continuous circular guard shell.
                float clearBody=smoothstep(.89,1.02,length(eye-rd*b));
                float t=_Halo.z,stride=(end-begin)/80,sum=0;
                [loop]for(int k=0;k<80;k++)
                {
                    float3 p=eye+rd*(begin+(k+.5)*stride);float r=length(p);float3 d=p/max(.001,r);
                    float large=Noise(d*2.9+float3(t*.31,-t*.24,t*.15));
                    float tear=Noise(d*10.7+float3(-t*.91,1.7,t*.61));
                    float other=Noise(d.zxy*6.8+float3(t*.53,-t*.72,3.4));
                    float active=smoothstep(.68,.84,large)*smoothstep(.47,.72,tear);
                    float split=smoothstep(.48,.72,other);
                    float radius=1.02+(.06+.11*large)*split;
                    float thickness=lerp(.018,.047,large);
                    float crest=exp(-pow((r-radius)/thickness,2)*1.5);
                    float root=exp(-abs(r-1.02)*11)*active*.3;
                    sum+=(crest*active*(.55+split)*1.35+root)*stride;
                }
                return half4(float3(1.3,.012,.65)*sum*_Halo.y*clearBody,0);
            }
            ENDHLSL
        }
    }
}

