Shader "JJKGame/Exploration/Purple Wrapper Texture Layers Wind"
{
 Properties {_Opacity("Opacity",Float)=.88 _Emission("Emission",Float)=1.7 _PhaseTime("Time",Float)=0}
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+34" "RenderType"="Transparent"}
  Pass
  {
   Blend One OneMinusSrcAlpha ZWrite Off Cull Off
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   float _Opacity,_Emission,_PhaseTime,_Radius,_Coupling,_Release,_LayerOcclusion,_CoverageBreakup;
   float4 _Sphere,_Event0,_Event1,_Event2,_CoverCenters[40],_CoverU[40],_CoverV[40],_HotSites[12];
   CBUFFER_END
   struct A {float4 p:POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;};
   struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;float3 world:TEXCOORD1;};
   V Vert(A i){V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);o.uv=i.uv;o.c=i.c;return o;}
   float H(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float N(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(H(a),H(a+float2(1,0)),f.x),lerp(H(a+float2(0,1)),H(a+1),f.x),f.y);}
   float Cover(float3 p)
   {
    float3 eye=GetCameraPositionWS(),ray=normalize(p-eye);float dist=distance(p,eye),cover=0;
    [loop] for(int k=24;k<40;k++)
    {
     float3 normal=normalize(cross(_CoverU[k].xyz,_CoverV[k].xyz));float denom=dot(ray,normal);
     if(_CoverCenters[k].w<.02 || abs(denom)<.06)continue;
     float d=dot(_CoverCenters[k].xyz-eye,normal)/denom;
     if(d<=0 || d>=dist-max(.02,_Sphere.w*.025))continue;
     float3 delta=eye+ray*d-_CoverCenters[k].xyz;
     float2 uv=float2(dot(delta,_CoverU[k].xyz),dot(delta,_CoverV[k].xyz));
     cover=max(cover,(1-smoothstep(.2,1,dot(uv,uv)))*_CoverCenters[k].w);
    }
    return saturate(cover)*_LayerOcclusion;
   }
   half4 Frag(V i):SV_Target
   {
    float3 eye=GetCameraPositionWS(),ray=normalize(i.world-eye),oc=eye-_Sphere.xyz;
    float b=dot(oc,ray),h=b*b-dot(oc,oc)+_Sphere.w*_Sphere.w;
    if(h>0 && -b-sqrt(h)>0 && -b-sqrt(h)<distance(i.world,eye)-.02)discard;
    float2 uv=i.uv;float t=_PhaseTime,seed=i.c.r*73;
    // Two procedural alpha families: short, thick tapered brush vs slender curved sweep.
    // These are independent shape masks, not the same strip with a new color.
    float coarse=N(float2(uv.x*6-t*8+seed,uv.y*5+seed));
    float fiber=N(float2(uv.x*19-t*17+seed,uv.y*24+seed));
    float center=.42+.24*sin(uv.x*3.8+seed);
    float brushWidth=(.07+.36*pow(saturate(1-uv.x),.6))*(.62+.5*coarse);
    float curveWidth=.08+.065*coarse;
    float width=lerp(brushWidth,curveWidth,i.c.g);
    float tear=(coarse-.5)*.13+(fiber-.5)*.065;
    float dist=abs(uv.y-center)+tear;
    float contour=1-smoothstep(width*.55,width,dist);
    float ends=smoothstep(.015,.12,uv.x)*(1-smoothstep(.80,.97,uv.x));
    float islands=smoothstep(.26,.51,coarse*.55+fiber*.45);
    float alpha=contour*ends*islands*i.c.a*_Opacity;
    alpha*=1-Cover(i.world)*.88;
    float crest=exp(-pow((uv.y-center)/(width*.23+.008),2))*smoothstep(.52,.8,fiber);
    float3 color=lerp(float3(.16,.015,.30),float3(.7,.07,1.05),coarse)*_Emission;
    color+=float3(.65,.2,.75)*crest*_Emission;
    return half4(color*alpha,alpha);
   }
   ENDHLSL
  }
 }
}
