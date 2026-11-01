Shader "Hidden/JJK/BlackFlashRnd"
{
    Properties { _Atlas("자체 먹물 플립북",2D)="white"{} _Age("재생 시간",Float)=0 _Mode("표현 종류",Float)=0 _Opacity("투명도",Float)=1 _Layer("깊이 레이어",Float)=0 _Red("붉은 경계 색",Color)=(3.2,.012,.025,1) _ZWrite("깊이 기록",Float)=1 }
    SubShader {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass {
            Tags {"LightMode"="SRPDefaultUnlit"}
            Blend SrcAlpha OneMinusSrcAlpha ZWrite [_ZWrite] Cull Off ZTest LEqual
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_Atlas);SAMPLER(sampler_Atlas);
            CBUFFER_START(UnityPerMaterial)
            float _Age,_Mode,_Opacity,_Layer;float4 _Red;
            CBUFFER_END
            struct A{float4 p:POSITION;float3 n:NORMAL;float4 uv:TEXCOORD0;float4 anchor:TEXCOORD1;};
            struct V{float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 n:TEXCOORD1;float4 uv:TEXCOORD2;float3 local:TEXCOORD3;};
            V Vert(A v) {
                V o;float3 p=v.p.xyz;float frame=floor(_Age*30),id=v.uv.y;
                if(_Mode<.5) {
                    // Whole torn surfaces replace their contour immediately; no root-tip growth.
                    p+=float3(sin(id*31+v.uv.x*12+frame*1.7),cos(id*19+v.uv.x*7-frame*2.1),sin(frame+id*13))*.09*v.uv.x;
                    p+=(p-v.anchor.xyz)*max(0,_Age-.12)*1.2;
                } else if(_Mode>1.5&&_Mode<2.5) {
                    p+=normalize(v.anchor.xyz+float3(0,.01,.2))*max(0,_Age-.13)*.8+float3(0,max(0,_Age-.13)*.5,0);
                } else if(_Mode>3.5) {
                    p+=normalize(v.anchor.xyz+float3(0,.1,.5))*max(0,_Age-.07)*1.5;
                }
                o.p=TransformObjectToHClip(p);o.world=TransformObjectToWorld(p);o.n=TransformObjectToWorldNormal(v.n);o.uv=v.uv;o.local=p;return o;
            }
            half4 Frag(V i):SV_Target {
                float3 ink=float3(.003,.001,.004);
                if(_Mode>.5&&_Mode<1.5) {
                    clip(_Age-.015);clip(.30-_Age);
                    float frame=min(15,floor(_Age/.30*16+max(0,_Layer)*.7));float2 cell=float2(fmod(frame,4),floor(frame/4));
                    float4 tex=SAMPLE_TEXTURE2D(_Atlas,sampler_Atlas,(cell+i.uv.xy*.998+.001)/4);
                    float facing=abs(dot(normalize(i.n),normalize(GetWorldSpaceViewDir(i.world))));
                    float alpha=tex.a*_Opacity*(_Layer>=0?smoothstep(.1,.55,facing):1);
                    clip(alpha-.05);return half4(lerp(ink,_Red.rgb,tex.r*.8),alpha);
                }
                if(_Mode>2.5&&_Mode<3.5){clip(.035-_Age);return half4(6,6,6,saturate(1-_Age/.035));}
                float facing=abs(dot(normalize(i.n),normalize(GetWorldSpaceViewDir(i.world))));
                if(_Mode>1.5&&_Mode<2.5) {
                    clip(_Age-.13);float fade=saturate(1-(_Age-.13)/.52);
                    return half4(ink,smoothstep(0,.8,facing)*fade*.35);
                }
                clip(_Age-.015);clip((_Mode>3.5?.42:.30)-_Age);
                float frame=floor(_Age*30);
                float noise=sin(i.local.x*21+i.local.z*17+frame)*sin(i.local.y*19-frame*1.7);
                clip(.85+noise*.3-saturate((_Age-.11)/.19));
                float edge=smoothstep(.6,.97,1-facing);
                float alpha=saturate((.32-_Age)*12);
                return half4(lerp(ink,_Red.rgb,edge*.7),alpha);
            }
            ENDHLSL
        }
    }
}
