Shader "JJKGame/Candidate/Purple Ingredient Expanded Discharge"
{
    Properties { _Color("Energy",Color)=(1,0,1,1) _Phase("Event Phase",Float)=0 }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+16"}
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;float _Phase;
            CBUFFER_END
            struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
            struct V {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
            V Vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;o.color=i.color;return o;}
            half4 Frag(V i):SV_Target
            {
                float cell=floor(i.uv.x*17)+floor(i.uv.y*11)*19;
                float flicker=.42+.58*pow(saturate(sin(_Phase*181+cell*7.13)),5);
                float tear=sin(i.uv.x*97+i.uv.y*31+_Phase*73)*sin(i.uv.x*23-i.uv.y*89-_Phase*47)*.5+.5;
                float cut=smoothstep(.16,.66,tear);
                float energy=(.5+flicker*1.35)*cut*i.color.a;
                return half4(_Color.rgb*energy,energy);
            }
            ENDHLSL
        }
    }
}
