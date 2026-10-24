Shader "JJK/Shibuya/Cursed Energy Rim"
{
    Properties { [HDR] _Tint("몸 윤곽 발광색",Color)=(.02,.8,1,1) _Strength("발광 강도",Float)=1 }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+10" "RenderType"="Transparent"}
        Pass
        {
            Blend One One ZWrite Off Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial) float4 _Tint;float _Strength; CBUFFER_END
            struct A {float4 p:POSITION;float3 n:NORMAL;};
            struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 n:TEXCOORD1;};
            V Vert(A a){V o;float3 p=a.p.xyz+a.n*.008;o.p=TransformObjectToHClip(p);o.world=TransformObjectToWorld(p);o.n=TransformObjectToWorldNormal(a.n);return o;}
            half4 Frag(V i):SV_Target
            {
                float rim=pow(1-saturate(dot(normalize(i.n),GetWorldSpaceNormalizeViewDir(i.world))),2.7);
                return half4(_Tint.rgb*rim*_Strength*.7,0);
            }
            ENDHLSL
        }
    }
}
