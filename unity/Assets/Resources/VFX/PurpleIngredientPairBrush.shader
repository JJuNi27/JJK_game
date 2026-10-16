Shader "JJKGame/VFX/Purple Ingredient Pair Brush"
{
 Properties { _PhaseTime("Time",Float)=0 _Gain("Emission",Float)=1 _Polarity("Force",Float)=0 _Breakup("Coverage breakup",Float)=.72 }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+12" "RenderType"="Transparent" }
  Pass
  {
   Blend SrcAlpha One
   ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   float _PhaseTime,_Gain,_Polarity,_Breakup;
   CBUFFER_END
   struct A { float4 positionOS:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR; };
   struct V { float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR; };
   V Vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.uv=i.uv;o.color=i.color;return o;}
   half4 Frag(V i):SV_Target
   {
    float u=i.uv.x,v=i.uv.y*2-1;
    float advect=_PhaseTime*(_Polarity<0?14:-21);
    float centre=.25*sin(u*19+advect)+.13*sin(u*37-advect*.61);
    float breadth=.44+.22*sin(u*11-advect*.7)+.14*sin(u*23+advect*.37);
    float edge=pow(saturate(1-abs(v-centre)/max(.12,breadth)),1.35);
    float gaps=smoothstep(-.3,.4,sin(u*27+advect+v*4)+.55*sin(u*47-advect*.8-v*7));
    float span=smoothstep(0,.06,u)*(1-smoothstep(.87,1,u));
    float hot=pow(saturate(1-abs(v-centre)*3.8),3)*smoothstep(.5,.95,u);
    float3 rgb=lerp(i.color.rgb,float3(.78,.9,1),hot*(_Polarity<0?.25:.08));
    return half4(rgb*_Gain,i.color.a*edge*span*lerp(1,gaps,_Breakup));
   }
   ENDHLSL
  }
 }
}
