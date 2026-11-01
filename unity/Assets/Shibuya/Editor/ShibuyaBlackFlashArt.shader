Shader "Hidden/JJK/BlackFlashArtDirected"
{
    Properties {
        _Atlas("파열 키 포즈",2D)="white"{} _Age("재생 시각",Float)=0
        _Times("키 포즈 종료 시각",Vector)=(.035,.105,.19,.3)
        _PoseOrder("원화 재생 순서",Vector)=(1,0,2,3)
        _Red("붉은 에너지",Color)=(5,.012,.008,1)
        _Edge("선명 경계 폭",Float)=2.4 _Halo("잔광 폭",Float)=7 _Coverage("외곽 발광 분포",Float)=.72
        _Kind("표현 종류",Float)=0 _Layer("검정·붉은 레이어",Float)=0
        [HideInInspector]_DstBlend("합성 목적",Float)=10 [HideInInspector]_ZWrite("깊이 기록",Float)=1 [HideInInspector]_Offset("깊이 편향",Float)=0
    }
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    TEXTURE2D(_Atlas);SAMPLER(sampler_Atlas);float4 _Atlas_TexelSize;
    CBUFFER_START(UnityPerMaterial)
    float4 _Times,_Red,_PoseOrder;float _Age,_Edge,_Halo,_Coverage,_Kind,_Layer;
    CBUFFER_END
    struct A{float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;};
    struct V{float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 n:TEXCOORD1;float2 uv:TEXCOORD2;};
    V Vert(A v){V o;o.p=TransformObjectToHClip(v.p.xyz);o.world=TransformObjectToWorld(v.p.xyz);o.n=TransformObjectToWorldNormal(v.n);o.uv=v.uv;return o;}
    float Pose(){return _Age<_Times.x?_PoseOrder.x:_Age<_Times.y?_PoseOrder.y:_Age<_Times.z?_PoseOrder.z:_PoseOrder.w;}
    float Mask(float2 uv){
        if(any(uv<.006)||any(uv>.994))return 0;
        float tail=saturate((_Age-_Times.z)/max(.01,_Times.w-_Times.z));
        uv-=normalize(uv-.5+float2(.00001,.00002))*tail*.055;
        float pose=Pose();float2 cell=float2(fmod(pose,2),1-floor(pose/2));
        return SAMPLE_TEXTURE2D(_Atlas,sampler_Atlas,(cell+uv)*.5).a;
    }
    // Cull a grazing sheet as a whole. Bent per-pixel normals would leave a thin
    // printed sliver visible when the primary plane is exactly edge-on.
    float Facing(V i){return abs(dot(normalize(TransformObjectToWorldDir(float3(0,0,1))),normalize(GetWorldSpaceViewDir(TransformObjectToWorld(float3(0,0,0))))));}
    void Alive(){clip(_Age);clip(_Times.w-_Age-.00001);}
    float Fade(){return saturate((_Times.w-_Age)/.045);}
    ENDHLSL
    SubShader {
        Tags{"RenderPipeline"="UniversalPipeline" "Queue"="Transparent-10" "RenderType"="Transparent"}
        Pass {
            Name "ArtLayer" Tags{"LightMode"="SRPDefaultUnlit"}
            Blend SrcAlpha [_DstBlend] ZWrite [_ZWrite] ZTest LEqual Cull Off Offset [_Offset],[_Offset]
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            half4 Frag(V i):SV_Target {
                Alive();
                if(_Layer<.5){
                    if(_Kind>.5){clip(_Times.z-_Age);return half4(.002,.0005,.002,1);}
                    clip(Facing(i)-.18);float a=Mask(i.uv);clip(a-.18);
                    return half4(.002,.0005,.002,Fade());
                }
                if(_Kind>1.5){clip(_Age-.025);return half4(_Red.rgb,Fade()*.75*saturate(1-i.uv.x));}
                if(_Kind>.5)discard;
                float face=Facing(i);clip(face-.18);
                float2 texel=_Atlas_TexelSize.xy*2;float a=Mask(i.uv),outer=a,inner=a,halo=0;
                float width=_Edge*(.8+.2*sin(i.uv.x*43+i.uv.y*37+_Age*65));
                [unroll]for(int k=0;k<8;k++){
                    float angle=k*.785398;float2 d=float2(cos(angle),sin(angle));
                    float n=Mask(i.uv+d*texel*width);outer=max(outer,n);inner=min(inner,n);
                    halo+=Mask(i.uv+d*texel*_Halo);
                }
                float edge=saturate(outer-inner);halo=saturate(halo/8-a);
                float patch=smoothstep(.18,.8,sin(i.uv.x*29+i.uv.y*53+_Age*73)*.5+.5+_Coverage-.5);
                float energy=(edge+halo*.3)*patch*Fade()*smoothstep(.18,.65,face);
                clip(energy-.005);return half4(_Red.rgb,energy);
            }
            ENDHLSL
        }
    }
}
