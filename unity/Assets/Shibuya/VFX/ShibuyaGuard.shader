Shader "JJK/Shibuya Guard"
{
    Properties { _Tint("Tint",Color)=(.2,.85,1,.18) }
    SubShader {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Tint;
            float _Impact,_Instability,_Cracks,_Break,_BreakForce;
            CBUFFER_END
            struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
            struct V {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
            float hash(float2 p){return frac(sin(dot(p,float2(37.17,91.7)))*43758.5453);}
            V vert(A i){
                V o;float3 p=i.positionOS.xyz;
                float wobble=sin(_Time.y*48+i.uv.y*18)*_Impact+sin(i.uv.y*31+_Time.y*27)*_Instability*.025;
                p.x+=wobble;p.z+=_Impact*.5;
                p+=float3((i.uv.x-.5)*2,i.uv.y-.3,1)*_Break*_BreakForce;
                o.positionCS=TransformObjectToHClip(p);o.uv=i.uv;return o;
            }
            half4 frag(V i):SV_Target {
                float2 uv=i.uv;float edge=min(min(uv.x,1-uv.x),min(uv.y,1-uv.y));
                float rim=1-smoothstep(.01,.07,edge);
                float n=floor(uv.y*9),f=frac(uv.y*9);
                float zigzag=lerp(hash(float2(n,4)),hash(float2(n+1,4)),f);
                float crack=1-smoothstep(.004,.012,abs(uv.x-(.35+.26*zigzag)));
                float branch=.22+.35*uv.x+abs(frac(uv.x*7)-.5)*.12;
                crack=max(crack,1-smoothstep(.004,.012,abs(uv.y-branch)));
                float flicker=1-_Instability*(.15+.35*step(.8,sin(_Time.y*39+uv.y*23)));
                float wave=pow(saturate(sin(length((uv-.5)*float2(1,1.4))*30-_Time.y*22)),14)*_Impact*3;
                float alive=step(_Break,hash(floor(uv*float2(9,7)))*.7+.3);
                float alpha=(_Tint.a+rim*.22+wave+crack*_Cracks*.35)*smoothstep(0,.012,edge)*flicker*alive*(1-_Break);
                return half4(_Tint.rgb*(1+rim+wave)+crack*_Cracks*.7,saturate(alpha));
            }
            ENDHLSL
        }
    }
}
