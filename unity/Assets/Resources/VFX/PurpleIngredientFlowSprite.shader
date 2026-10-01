Shader "JJKGame/VFX/Purple Ingredient Flow Sprite"
{
    Properties { _Shape("Orb or Cross",Float)=0 _Gain("Emission",Float)=3 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+13" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _Shape,_Gain;
            CBUFFER_END
            struct A { float4 positionOS:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR; };
            struct V { float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR; };
            V Vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;o.color=i.color;return o;}
            half4 Frag(V i):SV_Target
            {
                float2 p=abs(i.uv*2-1);float circle=saturate(1-length(p));
                float cross=max(pow(saturate(1-p.x),18)*pow(saturate(1-p.y),1.5),pow(saturate(1-p.y),18)*pow(saturate(1-p.x),1.5));
                float alpha=lerp(circle*circle,cross,_Shape);
                float3 color=lerp(i.color.rgb,float3(1,1,1),pow(circle,12)*.48);
                return half4(color*_Gain,alpha*i.color.a);
            }
            ENDHLSL
        }
    }
}
