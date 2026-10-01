Shader "JJKGame/Candidate/Purple Ingredient Outer Exploration"
{
    Properties { _Color("Energy",Color)=(1,0,0,1) _PhaseTime("Clock",Float)=0 _Violence("Instability",Float)=1 }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+14"}
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;float _PhaseTime;float _Violence;
            CBUFFER_END
            struct A{float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;float4 c:COLOR;};
            struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float2 uv:TEXCOORD2;float4 c:COLOR;};
            V Vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(i.n);o.uv=i.uv;o.c=i.c;return o;}
            half4 Frag(V i):SV_Target
            {
                float facing=abs(dot(normalize(i.n),normalize(GetCameraPositionWS()-i.w)));
                float tear=sin(i.uv.x*23+i.uv.y*13+_PhaseTime*47)*.5+.5;
                float crack=smoothstep(.46,.72,tear)*(.58+.42*sin(i.uv.y*37-_PhaseTime*29)*.5+.5);
                float pulse=.58+.42*pow(saturate(sin(_PhaseTime*31+i.uv.x*8)),2);
                float rim=pow(facing,3.2)*2.6;
                float hot=pow(facing,8)*(.35+.65*pow(saturate(sin(i.uv.x*17-_PhaseTime*41)),2))*2.1;
                float energy=(.09+rim+hot)*(.38+.9*crack)*pulse*_Violence;
                float3 rgb=_Color.rgb*energy+hot.xxx*.62;
                return half4(rgb,i.c.a*(.72+.28*crack));
            }
            ENDHLSL
        }
    }
}
