Shader "JJKGame/Exploration/Purple Hybrid D2R1 Rupture"
{
    Properties { _BodyCentre("Body centre",Vector)=(0,0,0,2.5) }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+20" "RenderType"="Transparent"}
        Pass
        {
            Blend One OneMinusSrcAlpha
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
                float depth=lerp(.32,1,smoothstep(-.6,.3,front));
                depth=lerp(depth,1,smoothstep(.95,1.1,length(local)/_BodyCentre.w));
                float distance=abs(i.uv.y*2-1);
                float across=1-smoothstep(.82,1,distance);
                float ends=smoothstep(0,.03,i.uv.x)*(1-smoothstep(.82,1,i.uv.x));
                float white=1-smoothstep(.23,.42,distance);
                float pink=1-smoothstep(.54,.73,distance);
                float3 colour=lerp(float3(.045,.001,.13),float3(5,.015,1.8),pink);
                colour=lerp(colour,float3(9,6.5,9),white*i.color.g);
                float alpha=saturate(i.color.a*across*ends*depth)*lerp(.55,1,pink);
                return half4(colour*alpha,alpha);
            }
            ENDHLSL
        }
    }
}
