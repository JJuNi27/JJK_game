Shader "JJKGame/Candidate/Purple Ingredient Collision"
{
    Properties { _Centre("Centre",Vector)=(0,0,0,2) _Stage("Frame",Float)=0 _Strength("Contrast",Float)=1 }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+40"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off ZTest Always Cull Front
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Centre;float _Stage,_Strength;
            CBUFFER_END
            struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 world:TEXCOORD0;};
            V Vert(A i){V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);return o;}
            half4 Frag(V i):SV_Target
            {
                float3 eye=GetCameraPositionWS(),ray=normalize(i.world-eye),offset=eye+ray*dot(_Centre.xyz-eye,ray)-_Centre.xyz;
                float2 q=mul((float3x3)UNITY_MATRIX_V,offset).xy/_Centre.w;float r=length(q);
                float mask=1-smoothstep(1.15,1.75,r);if(mask<=0)discard;
                float3 scene=SampleSceneColor(i.p.xy/_ScaledScreenParams.xy);
                float luminance=dot(scene,float3(.2126,.7152,.0722));
                float bw=1-smoothstep(.04,.18,luminance);
                float angle=atan2(q.y,q.x);
                float rays=pow(saturate(sin(angle*43+sin(angle*19)*2)),12)*smoothstep(.2,.8,r);
                bw*=1-rays;
                // One local black/white contact beat; second nominal frame reverses contrast.
                if(_Stage>=1)bw=1-bw;
                return half4(bw.xxx,mask*_Strength);
            }
            ENDHLSL
        }
    }
}
