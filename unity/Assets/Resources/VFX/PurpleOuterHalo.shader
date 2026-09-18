Shader "JJKGame/Exploration/Purple OuterR1 Halo"
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
            float Chord(float radius,float b,float d2,float end)
            {
                float h=radius*radius-d2;if(h<=0)return 0;h=sqrt(h);
                return max(0,min(end,-b+h)-max(0,-b-h));
            }
            half4 Frag(V i):SV_Target
            {
                float3 eye=(GetCameraPositionWS()-_Centre.xyz)/_Centre.w,rd=normalize(i.world-GetCameraPositionWS());
                float b=dot(eye,rd),rmax=_Halo.x*1.12,h=b*b-dot(eye,eye)+rmax*rmax;if(h<=0)return 0;
                h=sqrt(h);float begin=max(0,-b-h),end=-b+h;
                float scene=LinearEyeDepth(SampleSceneDepth(i.p.xy/_ScaledScreenParams.xy),_ZBufferParams);
                end=min(end,scene/(_Centre.w*max(.001,-mul((float3x3)UNITY_MATRIX_V,rd).z)));if(end<=begin)return 0;
                // Rays across the body remain untouched, regardless of the viewing angle.
                float clearBody=smoothstep(1.045,1.16,length(eye-rd*b));
                float d2=dot(eye,eye)-b*b,t=_Halo.z;
                float3 p=eye+rd*max(0,-b-sqrt(max(0,_Halo.x*_Halo.x-d2)));
                float angular=.5+.25*sin(p.x*4.7+p.y*2.1-t*1.3)+.25*sin(p.z*5.3-p.y*3.1+t*.73);
                float broken=.12+.88*smoothstep(.26,.78,angular);
                // Analytic shell thickness avoids concentric ray-march sampling bands.
                float shell=(Chord(_Halo.x+.02,b,d2,end)-Chord(_Halo.x-.025,b,d2,end))*2.8;
                float soft=(Chord(_Halo.x+.065,b,d2,end)-Chord(_Halo.x-.07,b,d2,end))*.65;
                float aura=(Chord(_Halo.x,b,d2,end)-Chord(_Halo.x-.24,b,d2,end))*.18;
                float sum=(shell+soft+aura)*broken;
                return half4(float3(1.3,.012,.65)*sum*_Halo.y*clearBody,0);
            }
            ENDHLSL
        }
    }
}
