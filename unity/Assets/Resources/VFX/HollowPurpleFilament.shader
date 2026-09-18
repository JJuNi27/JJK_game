Shader "JJKGame/VFX/Hollow Purple Filament"
{
    Properties { _Color("Tint", Color)=(1,1,1,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+12" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            V Vert(A i) { V o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz); o.uv=i.uv; o.color=i.color*_Color; return o; }
            half4 Frag(V i):SV_Target
            {
                float edge=pow(saturate(1-abs(i.uv.y*2-1)),1.6);
                float hot=pow(edge,8);
                return half4(i.color.rgb+hot*i.color.rgb*.75,i.color.a*edge);
            }
            ENDHLSL
        }
    }
}
