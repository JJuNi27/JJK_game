Shader "JJKGame/Candidate/Purple Ingredient Burst Volume"
{
    Properties { _Color("Energy",Color)=(1,0,0,1) _PhaseTime("Clock",Float)=0 }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+12"}
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;float _PhaseTime;
            CBUFFER_END
            struct A{float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;float4 c:COLOR;};
            struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float2 uv:TEXCOORD2;float4 c:COLOR;};
            V Vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(i.n);o.uv=i.uv;o.c=i.c;return o;}
            half4 Frag(V i):SV_Target
            {
                float facing=abs(dot(normalize(i.n),normalize(GetCameraPositionWS()-i.w)));
                float fracture=.20+.80*smoothstep(-.5,.3,sin(i.uv.x*31+i.uv.y*8+_PhaseTime*41));
                float energy=(.06+pow(facing,3)*2.2)*fracture;
                float hot=pow(facing,8)*(.25+.75*pow(saturate(sin(i.uv.x*17-_PhaseTime*29)),2))*1.4;
                return half4(_Color.rgb*energy+hot.xxx,i.c.a);
            }
            ENDHLSL
        }
    }
}
