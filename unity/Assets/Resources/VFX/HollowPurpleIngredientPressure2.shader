Shader "JJKGame/Candidate/Hollow Purple Ingredient Pressure 2"
{
    Properties { _Color("Pressure colour",Color)=(1,0,0,1) _PhaseTime("Clock",Float)=0 _Blend("Fusion fade",Float)=1 }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+12"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;float _PhaseTime,_Blend;
            CBUFFER_END
            struct A {float4 p:POSITION;float2 uv:TEXCOORD0;float4 colour:COLOR;};
            struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 colour:COLOR;};
            V Vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.colour=i.colour;return o;}
            half4 Frag(V i):SV_Target
            {
                float u=i.uv.x,y=i.uv.y;
                float warp=.1*sin(u*31+_PhaseTime*11)+.06*sin(u*63-_PhaseTime*17);
                float centre=y-.5-warp;
                float edge=1-smoothstep(.25,.48,abs(centre));
                float tear=.3+.7*smoothstep(-.4,.6,sin(u*13+sin(u*39)*1.2+_PhaseTime*9));
                float hot=exp(-pow((centre+.19)*10,2));
                float fissure=exp(-pow(centre*16,2));
                float3 colour=_Color.rgb*(.12+hot*1.5)*(1-fissure*.92);
                return half4(colour,edge*tear*i.colour.a*_Blend);
            }
            ENDHLSL
        }
    }
}
