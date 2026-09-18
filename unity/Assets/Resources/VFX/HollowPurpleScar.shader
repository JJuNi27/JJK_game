Shader "JJKGame/VFX/Hollow Purple Scar"
{
    Properties { _Age("Age",Float)=0 _Life("Life",Float)=1.8 _Seed("Seed",Float)=0 _Ground("Ground",Float)=1 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-5" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off Cull Off Offset -1,-1
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _Age,_Life,_Seed,_Ground;
            CBUFFER_END
            struct A {float4 positionOS:POSITION; float2 uv:TEXCOORD0;};
            struct V {float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0;};
            V Vert(A i) {V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;return o;}
            float H(float2 p) {return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float N(float2 p) {float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(H(a),H(a+float2(1,0)),f.x),lerp(H(a+float2(0,1)),H(a+1),f.x),f.y);}
            half4 Frag(V i):SV_Target
            {
                float2 p=i.uv;
                float n=N(p*float2(11,4)+_Seed)*.65+N(p*float2(29,13)+_Seed)*.35;
                float edge=abs(p.x*2-1);
                float cut=1-smoothstep(.38+n*.52,.57+n*.48,edge);
                float ends=smoothstep(0,.15+n*.13,p.y)*(1-smoothstep(.78-n*.14,1,p.y));
                float fissure=1-smoothstep(.04,.09,abs(edge-(.39+n*.22)));
                float sparks=fissure*step(.55,N(p*float2(19,27)+_Seed))*exp(-_Age*2.4);
                float broken=smoothstep(.20,.42,n);
                float fade=1-smoothstep(_Life*.2,_Life*(.65+H(_Seed.xx)*.35),_Age);
                float3 dark=float3(.012,.006,.025)*_Ground;
                float3 energy=float3(.7,.04,1.5)*sparks*(1+sin(_Age*12+_Seed)*.18);
                float alpha=cut*ends*broken*fade*lerp(.2,.87,_Ground);
                return half4(dark+energy,alpha);
            }
            ENDHLSL
        }
    }
}
