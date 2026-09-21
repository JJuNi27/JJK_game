Shader "JJKGame/Candidate/Hollow Purple Ingredient Rupture"
{
    Properties { _Color("Pressure colour",Color)=(1,0,0,1) _PhaseTime("Clock",Float)=0 _Blend("Fusion fade",Float)=1 }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+12"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;float _PhaseTime,_Blend;
            CBUFFER_END
            struct A {float4 p:POSITION;float2 uv:TEXCOORD0;float4 colour:COLOR;};
            struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 colour:COLOR;};
            V Vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.colour=i.colour;return o;}
            half4 Frag(V i):SV_Target
            {
                float u=i.uv.x;
                float serration=abs(frac(u*5.7+_PhaseTime*1.3)-.5);
                float centre=i.uv.y-.5+(serration-.25)*.32;
                float edge=1-smoothstep(.18+serration*.30,.30+serration*.34,abs(centre));
                // Actual zero-opacity gaps; no continuous emissive lip across the strip.
                float fracture=smoothstep(-.40,-.12,sin(u*8+sin(u*11)*.6+_PhaseTime*19));
                float taper=1-smoothstep(.80,1,u);
                float hot=exp(-centre*centre*70)*(.65+.65*saturate(sin(u*17+_PhaseTime*27)));
                return half4(_Color.rgb*(1.2+hot*4),edge*fracture*taper*saturate(i.colour.a*1.45)*_Blend);
            }
            ENDHLSL
        }
    }
}
