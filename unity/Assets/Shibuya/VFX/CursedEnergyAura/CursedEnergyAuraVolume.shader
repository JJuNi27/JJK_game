Shader "JJK/Shibuya/Cursed Energy Volume"
{
    Properties
    {
        [HDR] _MainColor("메인 주력 색상",Color)=(.02,.75,1,1)
        [HDR] _AccentColor("밝은 강조색",Color)=(.4,1,1,1)
        _InkColor("먹물 색상",Color)=(.004,.007,.018,1)
        [HideInInspector] _Ink("먹물 경계 모드",Float)=0
        _Strength("발광 강도",Float)=1
        _Opacity("불투명도",Float)=.7
        [HideInInspector] _AuraTime("표현 시간",Float)=0
        _MotionSpeed("움직임 속도",Float)=1.2
        _Irregularity("형태 불규칙성",Float)=.85
        [HideInInspector] _Wisp("가는 흐름 모드",Float)=0
        [HideInInspector] _InkCoverage("공유 먹물 경계",Float)=1
        [HideInInspector] _FlowDirection("표면과 상향 흐름",Vector)=(.45,.85,0,0)
        [HideInInspector] _FieldParams("주력장 두께·밀도·보조 크기",Vector)=(.14,.9,.55,0)
        [HideInInspector] _AuraExpansion("최대 주력량 기반 외부 성장",Float)=1
        [HideInInspector] _AuraVerticalExpansion("상향 성장",Float)=1
        [HideInInspector] _InnerFlow("내부 가시성·밀도",Vector)=(.65,1,0,0)
        [HideInInspector] _FieldViewMode("QA 필드 분리 (0 전체, 1 외곽, 2 내부)",Float)=0
        [HideInInspector] _FlameStyle("국소 형태·기준 성장·먹물 경계·암부 면적",Vector)=(0,1.25,1,1)
        [HideInInspector] _TipRate("분리 조각 주기",Float)=.7
        [HideInInspector] _EndConnection("상단·하단 흐름 연결·최하단 높이",Vector)=(0,0,0,0)
        _Shape("폭과 높이 배율",Vector)=(1,1,0,0)
        [HideInInspector] _Cull("면 선택",Float)=2
    }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull [_Cull]
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _MainColor,_AccentColor,_InkColor,_Shape,_FlowDirection,_FieldParams,_InnerFlow,_FlameStyle,_EndConnection;
            float4 _BodyRoots[48],_BodyNormals[48];
            float4 _TipRoots[8];float _TipRate;
            float _Ink,_Strength,_Opacity,_AuraTime,_MotionSpeed,_Irregularity,_Wisp,_InkCoverage,_AuraExpansion,_AuraVerticalExpansion,_FieldViewMode;
            CBUFFER_END
            struct A {float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;float2 seed:TEXCOORD1;float2 anchor:TEXCOORD2;};
            struct V {float4 p:SV_POSITION;float3 local:TEXCOORD0;float3 normal:TEXCOORD1;float3 world:TEXCOORD2;float2 uv:TEXCOORD3;float life:TEXCOORD4;float seed:TEXCOORD5;float field:TEXCOORD6;};
            float Hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
            float Noise(float3 p)
            {
                float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(lerp(Hash(i),Hash(i+float3(1,0,0)),f.x),lerp(Hash(i+float3(0,1,0)),Hash(i+float3(1,1,0)),f.x),f.y),
                    lerp(lerp(Hash(i+float3(0,0,1)),Hash(i+float3(1,0,1)),f.x),lerp(Hash(i+float3(0,1,1)),Hash(i+1),f.x),f.y),f.z);
            }
            float Field(float3 p){return Noise(p)*.64+Noise(p*2.13+5.7)*.25+Noise(p*4.17)*.11;}
            V Vert(A a)
            {
                V o;float time=_AuraTime*_MotionSpeed,seed=a.seed.x;
                if(a.anchor.y>2.5) {
                    float life=frac(time*_TipRate*(.78+seed*.54)+a.seed.y);
                    float flight=saturate(life/.72),fade=smoothstep(0,.06,life)*(1-smoothstep(.46,.72,life));
                    float3 p=a.p.xyz*(1-flight*.85);
                    float3 drift=float3(sin(flight*3.4+seed*13)*flight*.15,flight*(.24+seed*.35),cos(flight*4.6+seed*17)*flight*.13);
                    p+=mul((float3x3)unity_WorldToObject,drift);
                    p+=_TipRoots[(int)a.anchor.x].xyz;
                    p+=a.n*_Ink*.008*_FlameStyle.z;
                    o.p=TransformObjectToHClip(p);o.world=TransformObjectToWorld(p);o.normal=TransformObjectToWorldNormal(a.n);
                    o.local=p;o.uv=a.uv;o.seed=seed;o.life=fade;o.field=0;return o;
                }
                o.field=a.anchor.y>1.5?1:0;
                if(o.field>.5) {
                    float angle=a.uv.x*6.283185;
                    float wave=sin(angle*3+a.p.y*5.5-time*2.8)+.45*sin(angle*5-a.p.y*8+time*2);
                    float crest=pow(saturate(wave*.5+.35),4);
                    float3 growth=float3(_AuraExpansion,_AuraVerticalExpansion,_AuraExpansion);
                    float preserve=saturate(_AuraExpansion/max(.01,_FlameStyle.y)-1)*_FlameStyle.x;
                    // The carrier expands; local crest thickness/curl retain the 100-energy scale.
                    growth=lerp(growth,_FlameStyle.y.xxx,preserve);
                    float3 p=a.p.xyz+a.n*(_FieldParams.x*(.12+.32*wave)+crest*.13)*_Irregularity*growth;
                    p.y+=crest*.10*lerp(_AuraVerticalExpansion,_FlameStyle.y,preserve);
                    if(_EndConnection.y>0&&a.uv.y<.1)p.y=max(p.y,_EndConnection.z);
                    o.local=p;o.world=TransformObjectToWorld(p);o.p=TransformObjectToHClip(p);
                    o.normal=TransformObjectToWorldNormal(a.n);o.uv=a.uv;o.seed=.37;o.life=1;
                    p+=a.n*_Ink*(.008+Noise(p*8)*.012)*_Irregularity*lerp(sqrt(_AuraExpansion),sqrt(_FlameStyle.y),preserve)*_FlameStyle.z;
                    if(_EndConnection.y>0&&a.uv.y<.1)p.y=max(p.y,_EndConnection.z);
                    o.p=TransformObjectToHClip(p);return o;
                }
                float clock=time*(.78+seed*.54)+a.seed.y,life=frac(clock),cycle=floor(clock);
                float3 p=a.p.xyz;
                float t=a.uv.y,bend=t*t*_Irregularity;
                // A new 3D emission path is selected only while this flame is invisible.
                // No rotating sheet: the closed cross section twists along its own tail.
                float birth=Hash(float3(seed*23,cycle,7));
                float turn=(birth-.5)*.8*t+sin(time*.8+seed*31)*t*.17;
                float s=sin(turn),c=cos(turn);
                p.xz=float2(c*p.x-s*p.z,s*p.x+c*p.z);
                float3 normal=a.n;
                normal.xz=float2(c*a.n.x-s*a.n.z,s*a.n.x+c*a.n.z);
                p.x+=sin(t*5.3-time*3.4+seed*19)*bend*.20;
                p.z+=cos(t*7.1-time*2.9+seed*23)*bend*.19;
                // Only the tail rises: the emitting root remains embedded in the body.
                p.y+=((birth-.5)*.15+life*.24)*t;
                float notch=sin(t*24+seed*29-time*6.2)*sin(a.uv.x*12.566+t*12+time*3);
                p+=normal*notch*sin(t*3.14159)*.026*_Irregularity;
                float3 original=p;
                // A very thin matching hull, never a separate black flame or broad card.
                p+=normal*_Ink*(.008+Noise(p*8+seed*17)*.012)*_Irregularity;
                p*=float3(_Shape.x,_Shape.y,_Shape.x);
                float3 detailGrowth=lerp(float3(_AuraExpansion,_AuraVerticalExpansion,_AuraExpansion),_FlameStyle.y.xxx,saturate(_AuraExpansion/max(.01,_FlameStyle.y)-1)*_FlameStyle.x);
                p*=_FieldParams.z*detailGrowth;
                float3 surface=_BodyNormals[(int)a.anchor.x].xyz;
                float3 axis=normalize(surface*_FlowDirection.x+float3(0,max(.1,_FlowDirection.y),0));
                float3 right=normalize(cross(abs(axis.z)<.95?float3(0,0,1):float3(1,0,0),axis));
                float3 depth=cross(right,axis);
                p=right*p.x+axis*p.y+depth*p.z;
                normal=right*normal.x+axis*normal.y+depth*normal.z;
                original*=float3(_Shape.x,_Shape.y,_Shape.x);
                original*=_FieldParams.z*detailGrowth;
                original=right*original.x+axis*original.y+depth*original.z;
                p+=_BodyRoots[(int)a.anchor.x].xyz;
                o.p=TransformObjectToHClip(p);o.world=TransformObjectToWorld(p);
                o.normal=TransformObjectToWorldNormal(normal);o.local=original+_BodyRoots[(int)a.anchor.x].xyz;o.uv=a.uv;
                // Overlapping main currents share one body-space density field.
                o.seed=lerp(.37,seed,a.anchor.y);
                // The main body current persists; only detail tongues use full spawn/fade.
                o.life=lerp(.82+.18*sin(life*3.14159),smoothstep(0,.12,life)*(1-smoothstep(.72,1,life)),a.anchor.y);return o;
            }
            half4 Frag(V i):SV_Target
            {
                float time=_AuraTime*_MotionSpeed;
                // Anisotropic advected density: long rising channels, rapidly tearing tips.
                float3 flow=i.local*float3(7.5,2.7,7.5)+float3(i.seed*13,-time*3.3,i.seed*7);
                float n=Field(flow+Noise(flow*.85)*.65);
                float threshold=lerp(.30,.55,smoothstep(.22,1,i.uv.y));
                if(i.field>.5)threshold=.34+.06*sin(i.local.y*5-time*2+i.uv.x*12.566);
                if(i.field>.5) {
                    // Break the returning end flows with the SAME advected density field.
                    // No continuous ink lid or circular hem, and no change to the body spans.
                    float terminal=max(smoothstep(.8,.98,i.uv.y)*_EndConnection.x,(1-smoothstep(.01,.10,i.uv.y))*_EndConnection.y);
                    threshold+=terminal*.10;
                }
                float cut=smoothstep(threshold,threshold+.045,n);
                float end=smoothstep(0,.04,i.uv.y)*(1-smoothstep(.92,1,i.uv.y));
                if(i.field>.5)end=smoothstep(0,lerp(.035,.008,_EndConnection.y),i.uv.y)*(1-smoothstep(lerp(.84,.98,_EndConnection.x),1,i.uv.y));
                float facing=abs(dot(normalize(i.normal),GetWorldSpaceNormalizeViewDir(i.world)));
                float alpha=cut*end*i.life*_Opacity*saturate(_Strength);
                float extraInnerAlpha=0,legacyInnerAlpha=0;
                if(i.field>.5) {
                    // Grazing contour carries the field. Across the body, broken rising density
                    // stays translucent instead of exposing the complete carrier as a shell.
                    float blackEdge=1-smoothstep(.08,.26*_FlameStyle.z,facing);
                    float energyEdge=smoothstep(.18,.32,facing)*(1-smoothstep(.52,.70,facing));
                    float contour=max(blackEdge,energyEdge);
                    float window=.5+.5*sin(i.uv.x*18.8496+i.local.y*3-time*2.3+n*3);
                    float localShape=saturate(_AuraExpansion/max(.01,_FlameStyle.y)-1)*_FlameStyle.x;
                    // Advect local spans in body space instead of stretching six broad UV spans.
                    window=lerp(window,.5+.5*sin(i.local.x*9+i.local.z*7+i.local.y*3-time*2.3+n*3),localShape);
                    float streams=smoothstep(.42,.66,n)*smoothstep(.18,.65,window);
                    // Reveal large connected flow spans; never present the entire front surface.
                    float coverage=smoothstep(.12,.55,window+.18*contour);
                    float common=alpha*saturate(_FieldParams.y)*coverage;
                    legacyInnerAlpha=common*(.012+.025*streams)*(1-contour);
                    float density=clamp(_InnerFlow.y,.25,2);
                    float channels=smoothstep(.42-.10*(density-1),.66-.10*(density-1),n)*smoothstep(.16,.65,window);
                    // Reveal the existing rising channels on the front of the field, not a new hull.
                    // The original outer contour and black pass receive none of this extra alpha.
                    extraInnerAlpha=common*(1-contour)*saturate(_InnerFlow.x)*(.045+.32*channels);
                    alpha*=saturate(_FieldParams.y)*coverage*lerp(.012+.025*streams,.98,contour);
                    alpha*=lerp(1,_FlameStyle.z,blackEdge);
                }
                if(_Ink>.5) {
                    if(i.field>.5&&_FieldViewMode>1.5)clip(-1);
                    // Rear hull only appears along torn, grazing contours. Same erosion/phase.
                    alpha*=1-smoothstep(.12,.46*_FlameStyle.z,facing);
                    alpha=saturate(alpha*_FlameStyle.z);
                    clip(alpha-.012);
                    return half4(_InkColor.rgb,alpha);
                }
                if(i.field<.5)clip(alpha-.012);
                float crest=pow(saturate((n-.61)*4.2),3);
                float band=smoothstep(.32,.68,n);
                float3 color=lerp(_MainColor.rgb*float3(.18,.24,.65),_MainColor.rgb,band);
                color+=_AccentColor.rgb*crest*(.8+_Wisp);
                // Ink lives inside the SAME moving surface at grazing/eroded boundaries.
                float boundary=(1-smoothstep(.05,.17,n-threshold))*.85;
                float inkFlow=1-smoothstep(.39,.53,n+.06*sin(i.uv.y*11-time*3+i.seed*17));
                float inkMask=saturate(((1-smoothstep(.18,.53,facing))*.88+boundary*.65+inkFlow*.95)*1.10);
                if(i.field>.5)inkMask=saturate(((1-smoothstep(.10,.30*_FlameStyle.z,facing))*.98+boundary*.65*_FlameStyle.z+inkFlow*.95*_FlameStyle.w)*1.10);
                color=lerp(color*_Strength,_InkColor.rgb,inkMask*saturate(_InkCoverage));
                if(i.field>.5) {
                    if(_FieldViewMode>.5&&_FieldViewMode<1.5)alpha=max(0,alpha-legacyInnerAlpha);
                    else {
                        if(_FieldViewMode>1.5)alpha=legacyInnerAlpha;
                        if(extraInnerAlpha>0) {
                            float3 innerColor=_MainColor.rgb*(.5+.35*n)*_Strength;
                            color=(color*alpha+innerColor*extraInnerAlpha)/max(.00001,alpha+extraInnerAlpha);
                            alpha+=extraInnerAlpha;
                        }
                    }
                }
                clip(alpha-.012);
                return half4(color,alpha);
            }
            ENDHLSL
        }
    }
}
