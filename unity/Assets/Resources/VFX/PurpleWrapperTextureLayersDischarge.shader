Shader "JJKGame/Exploration/Purple Wrapper Texture Layers Discharge"
{
    Properties {_Emission("Emission",Float)=7 _Dark("Dark current",Float)=0 _PhaseTime("Time",Float)=0}
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+40" "RenderType"="Transparent"}
        Pass
        {
            Blend One OneMinusSrcAlpha ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _Emission,_Dark,_PhaseTime,_Radius,_Coupling,_LayerOcclusion,_CoverageBreakup;float4 _Sphere,_Event0,_Event1,_Event2;
            float4 _CoverCenters[40],_CoverU[40],_CoverV[40],_HotSites[12];
            CBUFFER_END
            struct A{float4 p:POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;};
            struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;float3 world:TEXCOORD1;float3 local:TEXCOORD2;};
            V Vert(A i){V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);o.uv=i.uv;o.c=i.c;o.local=i.p.xyz;return o;}
            float H(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float N(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(H(a),H(a+float2(1,0)),f.x),lerp(H(a+float2(0,1)),H(a+1),f.x),f.y);}
            float Event(float3 p,float4 e){float3 d=p-e.xyz;return e.w*exp(-dot(d,d)*1.6);}
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
                float seed=i.c.r*91,t=_PhaseTime;
                float3 local=i.local/max(_Radius,.001);
                float coupling=saturate(Event(local,_Event0)+Event(local,_Event1)+Event(local,_Event2))*_Coupling;
                float foregroundWind=Cover(i.world,0,24),hotField=Hot(i.world);
                float side=abs(i.uv.y-.5)*2;
                if(_Dark>.5)
                {
                    // Black is a drifting field inside the energy, independent of strip borders.
                    float coarse=N(local.xz*4+float2(-t*2.8,t*1.7)+seed);
                    float torn=N(local.xy*11+float2(t*8,-t*5)+coarse*2+seed);
                    float depth=N(local.yz*7+float2(-t*6,t*3)+seed);
                    float coverage=smoothstep(.19,.56,coarse*.40+torn*.35+depth*.25);
                    float edge=smoothstep(.08,.58,1-side+torn*.38-coarse*.18);
                    float shifted=i.uv.x+(torn-.5)*.13+(i.uv.y-.5)*.09;
                    float blade=1-smoothstep(.40,.85,abs(i.uv.y-.5)+(torn-.5)*.24+pow(abs(shifted-.5)*1.6,2));
                    float ends=blade*smoothstep(0,.12,shifted)*(1-smoothstep(.80,1,shifted));
                    float alpha=saturate(coverage*edge*ends*i.c.a*i.c.b*(1.7+.4*coupling))*.86;
                    alpha*=(1-foregroundWind*.8)*(1-hotField*.4);
                    float burn=smoothstep(.39,.81,torn*.47+depth*.53+coupling*.15);
                    float3 color=lerp(float3(.005,.001,.016),float3(.27,.006,.8)*_Emission,burn);
                    return half4(color*alpha,alpha);
                }
                // Preserve white/neon palette, adding location-dependent exposure and interruption.
                float width=.55+N(float2(i.uv.x*19-t*13+seed,0))*.4;
                float outline=1-smoothstep(width-.13,width,side);
                float segment=N(float2(i.uv.x*5.7-t*4.1+seed,seed*.17));
                float fine=N(float2(i.uv.x*19-t*11+seed,0));
                float gap=smoothstep(.25,.57,segment*.8+fine*.2);
                float foregroundDark=Cover(i.world,24,40);
                gap*=1-max(foregroundWind*.88,foregroundDark*.95);
                float hot=1-smoothstep(width*.18,width*.48,side);
                float3 bright=(float3(.44,.008,1.2)*(1-side)+float3(1.5,1.15,1.5)*hot)*_Emission;
                float alpha=outline*i.c.a*gap;
                return half4(bright*alpha*(1+coupling*.32+segment*.25),alpha);
            }
            ENDHLSL
        }
    }
}
