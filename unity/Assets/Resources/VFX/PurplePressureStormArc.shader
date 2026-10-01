Shader "JJKGame/Exploration/Purple Pressure Storm Arc"
{
    Properties {_Emission("Emission",Float)=3.4}
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+42" "RenderType"="Transparent"}
        Pass
        {
            Blend One OneMinusSrcAlpha ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _Emission;
            CBUFFER_END
            struct A {float4 p:POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;};
            struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;};
            V Vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.c=i.c;return o;}
            half4 Frag(V i):SV_Target
            {
                float side=abs(i.uv.y-.5)*2;
                float alpha=(1-smoothstep(.50,1,side))*i.c.a;
                float core=exp(-side*side*26);
                float3 color=float3(.12,.005,.25)+float3(.8,.08,1.2)*exp(-side*side*6)*_Emission;
                color+=float3(1.25,.95,1.4)*core*_Emission;
                return half4(color*alpha,alpha);
            }
            ENDHLSL
        }
    }
}
