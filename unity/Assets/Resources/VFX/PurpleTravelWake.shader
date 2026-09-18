Shader "JJKGame/Production/Purple Short Plasma Wake"
{
    Properties { _Centre("Centre",Vector)=(0,0,0,2.5) _Motion("Direction / strength",Vector)=(0,0,1,0) _Wake("Length / width / time / emission",Vector)=(1.35,1.12,0,1.15) }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+29" "RenderType"="Transparent"}
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
            float4 _Centre,_Motion,_Wake;
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
                float rmax=1.25+_Wake.x,b=dot(eye,rd),h=b*b-dot(eye,eye)+rmax*rmax;if(h<=0)return 0;
                h=sqrt(h);float begin=max(0,-b-h),end=-b+h;
                float scene=LinearEyeDepth(SampleSceneDepth(i.p.xy/_ScaledScreenParams.xy),_ZBufferParams);
                end=min(end,scene/(_Centre.w*max(.001,-mul((float3x3)UNITY_MATRIX_V,rd).z)));if(end<=begin)return 0;
                float clearBody=smoothstep(1.02,1.13,length(eye-rd*b));
                float stride=(end-begin)/72,sum=0,hot=0;
                float jitter=Hash(float3(floor(i.p.xy),19.7));
                [loop]for(int k=0;k<72;k++)
                {
                    float3 p=eye+rd*(begin+(k+jitter)*stride);float q=-dot(p,_Motion.xyz);
                    float u=saturate((q-.5)/(.5+_Wake.x));
                    float longitudinal=smoothstep(.45,.85,q)*(1-smoothstep(1+_Wake.x*.58,1+_Wake.x,q));
                    float3 side=p+_Motion.xyz*q;
                    float width=_Wake.y*lerp(1,.22,u*u);
                    float edge=1-smoothstep(width*.65,width,length(side));
                    // Fragmented pressure volume, world-locked noise breaks apart instead of a smooth ribbon.
                    float3 world=(p+_Centre.xyz/_Centre.w)*3.4;
                    float coarse=Noise(world+float3(_Wake.z*.8,-_Wake.z*.5,_Wake.z*.3));
                    float tearing=Noise(world*2.6+_Motion.xyz*_Wake.z*4);
                    float broken=smoothstep(.52,.64,coarse)*smoothstep(.45,.58,tearing);
                    float density=edge*longitudinal*broken;
                    sum+=density*stride*2.8;
                    hot+=density*smoothstep(.85,.98,tearing)*(1-u)*stride*.15;
                }
                float3 colour=float3(1.8,.012,.72)*sum+float3(1,.5,.8)*hot;
                return half4(colour*_Wake.w*_Motion.w*clearBody,0);
            }
            ENDHLSL
        }
    }
}
