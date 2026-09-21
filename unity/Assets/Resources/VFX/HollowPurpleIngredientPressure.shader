Shader "JJKGame/Candidate/Hollow Purple Ingredient Pressure"
{
    Properties { _Color("Pressure colour",Color)=(1,0,0,1) _PhaseTime("Clock",Float)=0 _Blend("Fusion fade",Float)=1 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+12" }
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
            struct A { float4 positionOS:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR; };
            struct V { float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR; };
            V Vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;o.color=i.color;return o;}
            half4 Frag(V i):SV_Target
            {
                float across=abs(i.uv.y*2-1);
                float edge=pow(saturate(1-across),.65);
                float rough=.7+.3*sin(i.uv.x*19+_PhaseTime*9+sin(i.uv.x*7)*2);
                // One broad pressure front, not paired neon rails or periodic dashed lines.
                float pressureLip=exp(-pow((i.uv.y-.32)*4.2,2));
                float split=.55+.45*smoothstep(-.4,.6,sin(i.uv.x*11+sin(i.uv.x*23)*1.3+_PhaseTime*4));
                float3 colour=_Color.rgb*(.12+pressureLip*.72)*rough;
                return half4(colour,edge*i.color.a*split*_Blend);
            }
            ENDHLSL
        }
    }
}
