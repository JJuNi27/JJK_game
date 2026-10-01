Shader "JJKGame/Exploration/Purple Identity Wrapped Corona"
{
    Properties { _Centre("Centre",Vector)=(0,0,0,2.5) _Halo("Halo",Vector)=(1.55,.38,0,0) }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+30" "RenderType"="Transparent"}
        Pass
        {
            Blend One One ZWrite Off ZTest Always Cull Front
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Centre,_Halo,_Wrapper;
            CBUFFER_END
            struct A { float4 p:POSITION; };
            struct V { float4 p:SV_POSITION; float3 world:TEXCOORD0; };
            V Vert(A i) { V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);return o; }
            float Hash(float3 p) { return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453); }
            float Noise(float3 p)
            {
                float3 a=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(lerp(Hash(a),Hash(a+float3(1,0,0)),f.x),lerp(Hash(a+float3(0,1,0)),Hash(a+float3(1,1,0)),f.x),f.y),
                    lerp(lerp(Hash(a+float3(0,0,1)),Hash(a+float3(1,0,1)),f.x),lerp(Hash(a+float3(0,1,1)),Hash(a+1),f.x),f.y),f.z);
            }
            half4 Frag(V i):SV_Target
            {
                float3 eye=(GetCameraPositionWS()-_Centre.xyz)/_Centre.w;
                float3 rd=normalize(i.world-GetCameraPositionWS());
                float b=dot(eye,rd),bound=max(_Halo.x*1.22,_Wrapper.z+.08);
                float h=b*b-dot(eye,eye)+bound*bound;if(h<=0)return 0;
                h=sqrt(h);float begin=max(0,-b-h),end=-b+h;
                float scene=LinearEyeDepth(SampleSceneDepth(i.p.xy/_ScaledScreenParams.xy),_ZBufferParams);
                end=min(end,scene/(_Centre.w*max(.001,-mul((float3x3)UNITY_MATRIX_V,rd).z)));
                if(end<=begin)return 0;
                float t=_Halo.z,stride=(end-begin)/80,sum=0;
                [loop]for(int k=0;k<80;k++)
                {
                    float3 p=eye+rd*(begin+(k+.5)*stride);
                    float r=length(p);if(r<.9 || r>_Wrapper.z+.08)continue;
                    float3 d=p/max(.001,r);
                    float large=Noise(d*2.9+float3(t*.46,-t*.34,t*.24));
                    float tear=Noise(d*10.7+float3(-t*1.22,1.7,t*.88));
                    float other=Noise(d.zxy*6.8+float3(t*.73,-t*.91,3.4));
                    float active=smoothstep(.60,.79,large)*smoothstep(.50,.72,tear);
                    float split=smoothstep(.48,.72,other);
                    float radius=1.04+(.07+.32*large)*split;
                    float thickness=lerp(.021,.055,large);
                    float crest=exp(-pow((r-radius)/thickness,2)*1.5);
                    float root=exp(-abs(r-1.02)*15)*active*.24;
                    sum+=(crest*active*(.55+split)*(1.8+_Wrapper.y*1.1)+root)*stride;
                }
                float closest=sqrt(max(0,dot(eye,eye)-b*b));
                float front=lerp(.14,1,smoothstep(.55,1.02,closest));
                return half4(float3(1.8,.025,.92)*sum*_Halo.y*_Wrapper.x*8*front,0);
            }
            ENDHLSL
        }
    }
}
