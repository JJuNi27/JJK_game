Shader "JJK/PurpleConvergenceFlash"
{
    Properties { _Strength("Strength",Float)=0 }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Blend One One
            ZWrite Off ZTest Always Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            float _Strength;
            struct Output { float4 positionCS:SV_POSITION; };
            Output vert(uint id:SV_VertexID){Output o;o.positionCS=GetFullScreenTriangleVertexPosition(id);return o;}
            half4 frag(Output i):SV_Target{return half4(_Strength.xxx,0);}
            ENDHLSL
        }
    }
}
