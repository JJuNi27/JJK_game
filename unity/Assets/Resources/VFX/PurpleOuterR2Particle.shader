Shader "JJKGame/Exploration/Purple OuterR2 Particles"
{
    Properties{_Emission("Emission",Float)=4 _Centre("Centre",Vector)=(0,0,0,2.5)}
    SubShader
    {
        Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+40" "RenderType"="Transparent"}
        Pass
        {
            Blend One One ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Centre;float _Emission;
            CBUFFER_END
            struct A{float4 p:POSITION;float4 c:COLOR;};struct V{float4 p:SV_POSITION;float3 world:TEXCOORD0;float4 c:COLOR;};
            V Vert(A i){V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);o.c=i.c;return o;}
            half4 Frag(V i):SV_Target
            {
                float3 eye=(GetCameraPositionWS()-_Centre.xyz)/_Centre.w,rd=normalize(i.world-GetCameraPositionWS());
                float b=dot(eye,rd),d=length(eye-rd*b);
                float bodyMask=smoothstep(1.02,1.12,d);
                return half4(i.c.rgb*_Emission*i.c.a*bodyMask,0);
            }
            ENDHLSL
        }
    }
}
