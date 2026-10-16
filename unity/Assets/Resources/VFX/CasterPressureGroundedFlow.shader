Shader "JJKGame/Exploration/Caster Grounded Pressure Flow"
{
 Properties {_Role("Role",Float)=0 _Clock("Clock",Float)=0 _Seed("Seed",Float)=0 _Weight("Envelope",Float)=1 _Density("Density",Float)=.96 _Crest("Crest",Float)=2.4}
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+32" "RenderType"="Transparent"}
  Pass
  {
   Blend One OneMinusSrcAlpha ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   float _Role,_Clock,_Seed,_Weight,_Density,_Crest;
   CBUFFER_END
   struct A {float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;};
   struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;float3 normal:TEXCOORD2;};
   V Vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.world=TransformObjectToWorld(i.p.xyz);o.normal=TransformObjectToWorldNormal(i.n);o.uv=i.uv;return o;}
   float H(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float N(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(H(a),H(a+float2(1,0)),f.x),lerp(H(a+float2(0,1)),H(a+1),f.x),f.y);}
   float4 Frag(V i):SV_Target
   {
    float u=i.uv.x,v=i.uv.y,t=_Clock,role=_Role,seed=_Seed;
    float speed=role<.5?4.5:role<1.5?7.5:14;
    float2 adv=float2(u*4-t*speed+seed,v*3+seed*.7);
    float broad=N(adv*.8),grain=N(adv*2.8+float2(-t,t*.35));
    float bend=.5+(N(float2(u*5-t*5+seed,seed))-.5)*.5;
    float width=role>1.5?.33+broad*.13:.19+broad*.23;
    float lateral=1-smoothstep(width*.18,width,abs(v-bend));
    float span=smoothstep(.02,.2,u)*(1-smoothstep(.67,.99,u));
    float packet=role>1.5?lerp(.48,1,smoothstep(.2,.65,broad*.7+grain*.3)):smoothstep(.15,.63,broad*.75+grain*.25);
    float density=packet*lateral*span*_Weight*_Density;
    if(role>.5 && role<1.5)density*=lerp(.25,1,smoothstep(.25,.65,N(float2(u*8-t*10+seed,v*5))));
    if(role>1.5)density*=lerp(.25,1,smoothstep(.28,.65,N(float2(u*7-t*19+seed,v*4))));
    float facing=abs(dot(normalize(i.normal),normalize(GetCameraPositionWS()-i.world)));
    density*=lerp(.48,1,smoothstep(.03,.5,facing));
    float a=saturate(density*(role<.5?.72:role<1.5?.8:.85));
    float3 color=role<.5?lerp(float3(.32,.36,.39),float3(.72,.77,.8),grain):role<1.5?lerp(float3(.42,.48,.51),float3(.94,1,1.03),broad):float3(.87,.93,1)*_Crest;
    return float4(color*a,a);
   }
   ENDHLSL
  }
 }
}
