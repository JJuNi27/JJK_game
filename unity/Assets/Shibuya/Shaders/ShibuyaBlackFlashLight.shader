Shader "JJK/Shibuya/BlackFlashLight"
{
    Properties { _Color("Color",Color)=(1,1,1,1) _Age("Age",Float)=0 _Life("Life",Float)=.1 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+10" "RenderType"="Transparent" }
        Pass
        {
            Name "Light" Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha One ZWrite Off ZTest LEqual Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Color; float _Age,_Life;
            CBUFFER_END
            struct A {float4 vertex:POSITION;float4 color:COLOR;};
            struct V {float4 vertex:SV_POSITION;float4 color:COLOR;};
            V Vert(A i){V o;o.vertex=TransformObjectToHClip(i.vertex.xyz);o.color=i.color;return o;}
            half4 Frag(V i):SV_Target {return half4(_Color.rgb*i.color.rgb,_Color.a*i.color.a*saturate(1-_Age/max(.01,_Life)));}
            ENDHLSL
        }
    }
}
