Shader "JJK/Shibuya/Cursed Energy Mote"
{
    Properties {[HDR] _Tint("미세 입자 색상",Color)=(.4,1,1,1) _Strength("발광 강도",Float)=1}
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+15" "RenderType"="Transparent"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial) float4 _Tint;float _Strength; CBUFFER_END
            struct A{float4 p:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
            struct V{float4 p:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
            V Vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.color=a.color;o.uv=a.uv;return o;}
            half4 Frag(V i):SV_Target{float2 q=abs(i.uv*2-1);float a=saturate(1-q.x-q.y*.7);return half4(_Tint.rgb*i.color.rgb,a*a*i.color.a*_Strength);}
            ENDHLSL
        }
    }
}
