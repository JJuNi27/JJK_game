Shader "JJKGame/Candidate/Purple Ingredient Outer Discharge"
{
    Properties { _Color("Discharge",Color)=(1,0,1,1) _PhaseTime("Event Clock",Float)=0 _EventFlash("Event Flash",Float)=1 _Breakup("Path Breakup",Float)=0 }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+16"}
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;float _PhaseTime,_EventFlash,_Breakup;
            CBUFFER_END
            struct A {float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;float4 c:COLOR;};
            struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float2 uv:TEXCOORD2;float4 c:COLOR;};
            V Vert(A i){V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(i.n);o.uv=i.uv;o.c=i.c;return o;}
            half4 Frag(V i):SV_Target
            {
                float flicker=.42+.58*pow(saturate(sin(_PhaseTime*157+i.uv.x*91+i.uv.y*17)),4);
                float tears=sin(i.uv.x*53+i.uv.y*19+_PhaseTime*61)*.5+.5;
                float breakup=smoothstep(.22,.82,tears+_Breakup*.19);
                float facing=abs(dot(normalize(i.normal),normalize(GetCameraPositionWS()-i.world)));
                float hot=pow(facing,5)*(.55+.45*flicker);
                float energy=(.22+hot*2.7+breakup*.65)*flicker*_EventFlash;
                return half4(_Color.rgb*energy, i.c.a*breakup*(.62+.38*flicker));
            }
            ENDHLSL
        }
    }
}
