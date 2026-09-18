Shader "JJKGame/Exploration/Purple OuterR1 Background Distortion"
{
    Properties { _Centre("Centre",Vector)=(0,0,0,2.5) _Field("Field",Vector)=(1.36,.0035,0,0) }
    SubShader
    {
        // Before every transparent body / halo layer; opaque colour can never overwrite those layers.
        Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent-40" "RenderType"="Transparent"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha ZWrite Off ZTest Always Cull Front
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Centre,_Field;
            CBUFFER_END
            struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 world:TEXCOORD0;};
            V Vert(A i){V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);return o;}
            half4 Frag(V i):SV_Target
            {
                float3 eye=(GetCameraPositionWS()-_Centre.xyz)/_Centre.w,rd=normalize(i.world-GetCameraPositionWS());
                float b=dot(eye,rd);float3 closest=eye-rd*b;float d=length(closest),h=_Field.x*_Field.x-d*d;
                if(h<=0 || _Field.y<=0)return 0;
                float2 uv=i.p.xy/_ScaledScreenParams.xy;
                float scene=LinearEyeDepth(SampleSceneDepth(uv),_ZBufferParams)/(_Centre.w*max(.001,-mul((float3x3)UNITY_MATRIX_V,rd).z));
                if(scene<max(0,-b-sqrt(h)))return 0;
                float mask=smoothstep(1.075,1.17,d)*(1-smoothstep(_Field.x-.12,_Field.x,d));
                float t=_Field.z;float3 p=closest;
                float2 turbulence=float2(sin(p.y*10+p.z*8-t*5)+.45*cos(p.x*13+t*2.7),cos(p.x*9-p.z*11+t*4.2));
                float2 offset=turbulence*_Field.y*mask*float2(_ScaledScreenParams.y/_ScaledScreenParams.x,1);
                return half4(SampleSceneColor(saturate(uv+offset)),mask*.65);
            }
            ENDHLSL
        }
    }
}
