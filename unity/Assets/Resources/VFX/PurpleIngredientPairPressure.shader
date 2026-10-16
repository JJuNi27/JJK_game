Shader "JJKGame/VFX/Purple Ingredient Pair Pressure"
{
 Properties { _PhaseTime("Time",Float)=0 _Color("Pressure tint",Color)=(.3,.65,.9,1) _Density("Density",Float)=.72 _Gain("Diffuse emission",Float)=1.55 _Crest("Broken crest emission",Float)=4.2 _Erosion("Breakup",Float)=.62 }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+11" "RenderType"="Transparent" }
  Pass
  {
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off Cull Back
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   float4 _Color;
   float _PhaseTime,_Density,_Gain,_Crest,_Erosion;
   CBUFFER_END
   struct A {float4 positionOS:POSITION;float3 normalOS:NORMAL;float2 uv:TEXCOORD0;float4 color:COLOR;};
   struct V {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float3 local:TEXCOORD2;float2 uv:TEXCOORD3;float4 color:COLOR;};
   float Hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
   float Noise(float3 p)
   {
    float3 a=floor(p),f=frac(p);f=f*f*(3-2*f);
    return lerp(lerp(lerp(Hash(a),Hash(a+float3(1,0,0)),f.x),lerp(Hash(a+float3(0,1,0)),Hash(a+float3(1,1,0)),f.x),f.y),lerp(lerp(Hash(a+float3(0,0,1)),Hash(a+float3(1,0,1)),f.x),lerp(Hash(a+float3(0,1,1)),Hash(a+1),f.x),f.y),f.z);
   }
   V Vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.local=i.positionOS.xyz;o.world=TransformObjectToWorld(i.positionOS.xyz);o.normal=TransformObjectToWorldNormal(i.normalOS);o.uv=i.uv;o.color=i.color;return o;}
   half4 Frag(V i):SV_Target
   {
    float3 advect=float3(_PhaseTime*1.8,-_PhaseTime*2.3,_PhaseTime*.9);
    float broad=Noise(i.local*1.9+advect);
    float torn=Noise(i.local*5.2-advect*2+float3(broad*2,0,0));
    float face=pow(saturate(abs(dot(normalize(i.normal),normalize(GetCameraPositionWS()-i.world)))),.7);
    float ends=pow(saturate(sin(i.uv.x*3.14159265)),.65);
    float breakup=smoothstep(.19,.71,broad*.65+torn*.35);
    float coverage=lerp(.68,breakup,_Erosion)*face*ends*i.color.a;
    float sharp=pow(saturate(1-abs(torn-.56)*12),3)*smoothstep(.35,.85,broad);
    // Diffuse pressure remains broad; only short irregular pockets become a luminous crest.
    float3 diffuse=_Color.rgb*_Gain*(.75+.55*broad);
    float3 crest=lerp(_Color.rgb,float3(.86,.9,.94),.5)*sharp*_Crest*i.color.r;
    return half4(diffuse+crest,saturate(coverage*_Density));
   }
   ENDHLSL
  }
 }
}
