Shader "JJKGame/Exploration/Purple Tempest Final Cleanup Wind"
{
    Properties {_Opacity("Opacity",Float)=.76 _PhaseTime("Time",Float)=0}
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
            float _Opacity,_PhaseTime,_Radius,_Coupling,_Release,_LayerOcclusion,_CoverageBreakup;float4 _Sphere,_Event0,_Event1,_Event2;
            float4 _CoverCenters[40],_CoverU[40],_CoverV[40],_HotSites[12];
            CBUFFER_END
            struct A{float4 p:POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;};
            struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;float3 world:TEXCOORD1;};
            V Vert(A i){V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);o.uv=i.uv;o.c=i.c;return o;}
            float H(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float N(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(H(a),H(a+float2(1,0)),f.x),lerp(H(a+float2(0,1)),H(a+1),f.x),f.y);}
            // Approximate soft occlusion from actual finite mesh patches, only if nearer
            // on this camera ray. Fixed transparent draw order remains unchanged.
            float Cover(float3 p,int first,int last)
            {
                float3 eye=GetCameraPositionWS(),ray=normalize(p-eye);float distanceToPixel=distance(p,eye),cover=0;
                [loop] for(int k=first;k<last;k++)
                {
                    if(_CoverCenters[k].w<.02)continue;
                    float3 normal=normalize(cross(_CoverU[k].xyz,_CoverV[k].xyz));
                    float denom=dot(ray,normal);if(abs(denom)<.06)continue;
                    float d=dot(_CoverCenters[k].xyz-eye,normal)/denom;
                    if(d<=0 || d>=distanceToPixel-max(.02,_Sphere.w*.025))continue;
                    float3 delta=eye+ray*d-_CoverCenters[k].xyz;
                    float2 uv=float2(dot(delta,_CoverU[k].xyz),dot(delta,_CoverV[k].xyz));
                    float shape=1-smoothstep(.35,1,dot(uv,uv));
                    float density=N((eye+ray*d).xz/max(_Sphere.w,.001)*3+float2(-_PhaseTime*3,_PhaseTime));
                    cover=max(cover,shape*_CoverCenters[k].w*(.55+.45*density));
                }
                return saturate(cover)*_LayerOcclusion;
            }
            float Hot(float3 p)
            {
                float h=0;
                [loop] for(int k=0;k<12;k++)
                {
                    float3 d=(p-_HotSites[k].xyz)/max(_Sphere.w*.3,.001);
                    h=max(h,_HotSites[k].w*exp(-dot(d,d)*2.5));
                }
                return h;
            }
            half4 Frag(V i):SV_Target
            {
                float3 eye=GetCameraPositionWS(),ray=normalize(i.world-eye),oc=eye-_Sphere.xyz;
                float b=dot(oc,ray),h=b*b-dot(oc,oc)+_Sphere.w*_Sphere.w;
                if(h>0 && -b-sqrt(h)>0 && -b-sqrt(h)<distance(i.world,eye)-.02)discard;
                float2 uv=i.uv;float t=_PhaseTime,seed=i.c.r*73;
                float large=N(float2(uv.x*4-t*4+seed,uv.y*4+t*.9));
                float tear=N(float2(uv.x*17-t*12+large*3,uv.y*13+seed));
                // Advect broad density/width/visible spans along flow; never expose
                // both straight mesh borders as one continuous cloth silhouette.
                float flow=uv.x*3.2-t*(2.1+i.c.g*.9)+seed;
                float lobe=N(float2(flow,seed+3));
                float center=.18+.64*N(float2(flow+1.7,seed+17));
                float width=.20+.24*lobe;
                float crossFlow=abs(uv.y-center)/width;
                float edge=1-smoothstep(.35,1.15,crossFlow+(.5-tear)*.32);
                float ends=smoothstep(0,.09,uv.x)*(1-smoothstep(.75,1,uv.x));
                float span=smoothstep(.29,.63,lobe*.65+large*.35);
                float gap=lerp(smoothstep(.23,.55,large*.7+tear*.3),span,_CoverageBreakup);
                float crestY=center+(large-.5)*width*.5;
                float crest=exp(-pow((uv.y-crestY)/(.045+tear*.055),2))*smoothstep(.48,.78,tear)*span;
                float shoulder=N(float2(flow*1.7+uv.y*2.4,seed+29));
                float broad=gap*edge*(.85+.8*tear)*(.6+.65*shoulder);
                float alpha=saturate(broad*.85+crest*.34)*ends*i.c.a*_Opacity;
                float3 gray=lerp(float3(.32,.36,.4),float3(.95,1.01,1.08),large);
                float3 tone=gray+float3(.75,.8,.9)*crest;
                alpha*=1+_Release*.2;
                // Pressure crest at a shared disturbance picks up violet; most wind stays neutral.
                tone=lerp(tone,tone+float3(.25,.015,.45),i.c.b*_Coupling*.24);
                float hot=Hot(i.world);tone=lerp(tone,tone+float3(.8,.65,1),hot);
                return half4(tone*alpha,alpha);
            }
            ENDHLSL
        }
    }
}
