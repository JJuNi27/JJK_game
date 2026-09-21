Shader "JJKGame/Candidate/Purple Ingredient Collision Final"
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
                float r=length(q),a=atan2(q.y,q.x);
                float teeth=pow(abs(sin(a*13+sin(a*7)*.7)),5);
                float border=1.12+.52*teeth+.10*sin(a*23);
                clip(border-r);
                float sector=sin(a*23+sin(a*9)*1.4);
                float wedges=step(.32+r*.22,sector)*step(.18+.13*sin(a*11),r);
                float starRadius=.12+.40*pow(abs(cos(a*3+.25)),18)+.23*pow(abs(sin(a*5)),26);
                float star=1-step(starRadius,r);
                float3 scene=SampleSceneColor(i.p.xy/_ScaledScreenParams.xy);
                float ink=1-step(.11,dot(scene,float3(.2126,.7152,.0722)));
                // Opaque local ink with a fused contact star, coarse radial wedges and sharp burst edge.
                float bw=max(star,wedges);
                bw=max(bw,ink*step(.58,r)*step(r,.82)*step(.86,sector));
                if(_Stage>=1)bw=1-bw;
                return half4(bw.xxx,1);
            }
            ENDHLSL
        }
    }
}
