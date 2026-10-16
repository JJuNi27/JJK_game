Shader "JJKGame/Exploration/Caster Pressure One 3D Flow"
{
 Properties {_Role("Role",Float)=0 _Clock("Clock",Float)=0 _Weight("Envelope",Float)=1 _Density("Density",Float)=.86 _Crest("Crest",Float)=2.6}
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
   float _Role,_Clock,_Weight,_Density,_Crest;
   CBUFFER_END
   struct A {float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;};
   struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;float3 world:TEXCOORD1;float3 normal:TEXCOORD2;};
   V Vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.world=TransformObjectToWorld(i.p.xyz);o.normal=TransformObjectToWorldNormal(i.n);o.uv=i.uv;return o;}
   float H(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float N(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(H(a),H(a+float2(1,0)),f.x),lerp(H(a+float2(0,1)),H(a+1),f.x),f.y);}
   float4 Frag(V i):SV_Target
   {
    float u=i.uv.x,v=i.uv.y,t=_Clock,role=_Role;
    float2 adv=float2(u*7-t*(role<.5?2.8:role<1.5?3.9:2.8),v*3+role*7);
    float broad=N(adv*.72),grain=N(adv*3.4+float2(-t*.6,t*.2));
    float bend=.5+(N(float2(u*5-t*2,role*9))-.5)*.58;
    float width=.21+broad*.25;
    float lateral=1-smoothstep(width*.24,width,abs(v-bend));
    float span=smoothstep(.03,.15,u)*(1-smoothstep(.82,.99,u));
    float packet=smoothstep(.20,.64,broad*.76+grain*.24);
    float density=packet*lateral*span*_Weight*_Density;
    // Interior has its own compressed, nonparallel span. Crest is local and intermittent, never the full path.
    if(role>.5 && role<1.5)density*=smoothstep(.18,.47,N(float2(u*10-t*4.1,v*6+3)))*1.2;
    if(role>1.5)density*=smoothstep(.64,.84,N(float2(u*13-t*5.2,v*4)))*smoothstep(.44,.7,broad);
    float facing=abs(dot(normalize(i.normal),normalize(GetCameraPositionWS()-i.world)));
    density*=lerp(.38,1,smoothstep(.03,.55,facing));
    float a=saturate(density*(role<.5?.65:role<1.5?.85:.8));
    float3 color=role<.5?lerp(float3(.36,.40,.43),float3(.75,.79,.81),grain):role<1.5?lerp(float3(.48,.53,.56),float3(1,1.06,1.1),broad):float3(.87,.93,1)*_Crest;
    return float4(color*a,a);
   }
   ENDHLSL
  }
 }
}
