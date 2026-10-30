Shader "JJK/Shibuya Parry"
{
    Properties {_Tint("충돌 섬광 색상",Color)=(.5,.95,1,1)}
    SubShader {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+10" "RenderType"="Transparent"}
        Pass {
            Blend SrcAlpha One
            Cull Off
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Tint;
            float _Progress,_Intensity;
            CBUFFER_END
            struct A {float4 p:POSITION;float2 uv:TEXCOORD0;};
            struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
            V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;return o;}
            half4 frag(V i):SV_Target{
                float2 p=i.uv*2-1;float r=length(p);
                float ring=(1-smoothstep(.02,.065,abs(r-lerp(.12,.88,_Progress))))*(1-_Progress);
                float flash=pow(saturate(1-r/.35),3)*pow(1-_Progress,2)*3;
                float streak=pow(saturate(1-abs(p.y)*24),3)*saturate(1-abs(p.x))*pow(1-_Progress,3);
                float alpha=saturate(ring+flash+streak)*smoothstep(1,.85,r);
                return half4(_Tint.rgb*_Intensity,alpha*_Tint.a);
            }
            ENDHLSL
        }
    }
}
