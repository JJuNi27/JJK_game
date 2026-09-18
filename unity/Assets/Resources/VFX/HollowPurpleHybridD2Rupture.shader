Shader "JJKGame/Exploration/Purple Hybrid D2 Rupture"
{
    Properties { _BodyCentre("Body centre",Vector)=(0,0,0,2.5) }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+20" "RenderType"="Transparent"}
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _BodyCentre;
            CBUFFER_END
            struct A{float4 positionOS:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
            struct V{float4 positionCS:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;};
            V Vert(A i){V o;o.world=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.color=i.color;o.uv=i.uv;return o;}
            half4 Frag(V i):SV_Target
            {
                float3 local=i.world-_BodyCentre.xyz;
                float front=dot(local,normalize(GetCameraPositionWS()-_BodyCentre.xyz))/_BodyCentre.w;
                float depth=lerp(.20,1,smoothstep(-.6,.3,front));
                depth=lerp(depth,1,smoothstep(.95,1.1,length(local)/_BodyCentre.w));
                float across=pow(saturate(1-abs(i.uv.y*2-1)),.7);
                float ends=smoothstep(0,.03,i.uv.x)*(1-smoothstep(.82,1,i.uv.x));
                return half4(i.color.rgb,i.color.a*across*ends*depth);
            }
            ENDHLSL
        }
    }
}
