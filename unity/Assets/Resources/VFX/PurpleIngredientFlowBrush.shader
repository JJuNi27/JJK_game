Shader "JJKGame/VFX/Purple Ingredient Flow Brush"
{
    Properties { _PhaseTime("Time",Float)=0 _Gain("Emission",Float)=1 }
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
            float _PhaseTime;
            float _Gain;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            V Vert(A i) { V o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;o.color=i.color;return o; }
            half4 Frag(V i):SV_Target
            {
                float u=i.uv.x, v=i.uv.y*2-1;
                float rag=.15*sin(u*17-_PhaseTime*19)+.08*sin(u*39+_PhaseTime*23);
                float edge=saturate((1-abs(v+rag))*2.1);
                float fibers=.72+.28*sin(v*12+sin(u*7-_PhaseTime*14));
                float bite=.78+.22*sin(u*19+v*2-_PhaseTime*21);
                float ends=smoothstep(0,.07,u)*(1-smoothstep(.75,1,u));
                return half4(i.color.rgb*_Gain,i.color.a*edge*edge*fibers*lerp(.25,1,bite)*ends);
            }
            ENDHLSL
        }
    }
}
