Shader "JJKGame/VFX/Hollow Purple Dust"
{
    Properties { _Opacity("Opacity",Float)=1 _PhaseTime("Clock",Float)=0 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+15" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _Opacity,_PhaseTime;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            V Vert(A i) { V o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz); o.uv=i.uv; return o; }
            half4 Frag(V i):SV_Target
            {
                float2 p=i.uv*2-1;
                float n=sin(p.x*13+sin(p.y*9+_PhaseTime)*2)*sin(p.y*11-p.x*4);
                float alpha=(1-smoothstep(.25,.96,length(p)+n*.12))*_Opacity;
                return half4(lerp(float3(.075,.066,.085),float3(.22,.19,.21),n*.5+.5),alpha*.5);
            }
            ENDHLSL
        }
    }
}
