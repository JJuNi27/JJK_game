Shader "JJKGame/Candidate/Purple Ingredient Collision Tear"
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
                float2 q=mul((float3x3)UNITY_MATRIX_V,offset).xy/_Centre.w;
                float stage=step(1,_Stage);
                float row=floor(q.y*17),jag=frac(sin(row*127.1+stage*19)*43758.5453);
                float seam=q.x+.24*q.y+.09*sin(q.y*19)+.13*sin(q.y*7+1.2);
                float field=length(float2(q.x*.77,q.y*1.05))+.12*sin(q.y*15+q.x*7)+.14*(jag-.5);
                float mask=(1-smoothstep(1.13,1.30,field));clip(mask-.015);
                float displacement=(jag-.5)*.028*(1-stage*.35)*mask;
                float2 screen=i.p.xy/_ScaledScreenParams.xy;
                screen+=float2(displacement+sign(seam)*.006,.002*sin(q.x*13))*mask;
                float3 scene=SampleSceneColor(screen);
                float lum=dot(scene,float3(.2126,.7152,.0722));
                float ink=smoothstep(.01,.22,lum);
                float bw=lerp(1-ink,ink,stage);
                // Read the actual bodies and background through a torn negative exposure.
                // No radial artwork or opaque star shape is drawn over the collision.
                return half4(bw.xxx,mask);
            }
            ENDHLSL
        }
    }
}
