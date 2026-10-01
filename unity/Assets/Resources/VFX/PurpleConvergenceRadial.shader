Shader "JJK/PurpleConvergenceRadial"
{
    Properties { _Strength("Strength",Float)=0 _Radius("Radius",Float)=.42 _Core("Core",Float)=.035 }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Overlay" "RenderType"="Transparent"}
        Pass
        {
            Blend One One ZWrite Off ZTest Always Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            float _Strength,_Radius,_Core;float4 _Centre;
            struct V {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
            V vert(uint id:SV_VertexID){V o;o.positionCS=GetFullScreenTriangleVertexPosition(id);o.uv=GetFullScreenTriangleTexCoord(id);return o;}
            half4 frag(V i):SV_Target
            {
                float r=length((i.uv-_Centre.xy)*float2(_Centre.z,1));
                // Small white-hot centre; keep the existing burst/ring's surrounding contrast visible.
                float light=_Strength*(exp(-r*r/max(.00001,_Core*_Core))*.85+exp(-r*r/max(.001,_Radius*_Radius))*.10);
                return half4(light.xxx,0);
            }
            ENDHLSL
        }
    }
}
