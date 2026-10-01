Shader "JJKGame/Exploration/Purple Tempest Polish Discharge"
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
            float _Emission,_Dark,_PhaseTime,_Radius,_Coupling;float4 _Sphere,_Event0,_Event1,_Event2;
            CBUFFER_END
            struct A{float4 p:POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;};
            struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float4 c:COLOR;float3 world:TEXCOORD1;float3 local:TEXCOORD2;};
            V Vert(A i){V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);o.uv=i.uv;o.c=i.c;o.local=i.p.xyz;return o;}
            float H(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
            float N(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(H(a),H(a+float2(1,0)),f.x),lerp(H(a+float2(0,1)),H(a+1),f.x),f.y);}
            float Event(float3 p,float4 e){float3 d=p-e.xyz;return e.w*exp(-dot(d,d)*1.6);}
            half4 Frag(V i):SV_Target
            {
                float3 eye=GetCameraPositionWS(),ray=normalize(i.world-eye),oc=eye-_Sphere.xyz;
                float b=dot(oc,ray),h=b*b-dot(oc,oc)+_Sphere.w*_Sphere.w;
                if(h>0 && -b-sqrt(h)>0 && -b-sqrt(h)<distance(i.world,eye)-.02)discard;
                float seed=i.c.r*91,t=_PhaseTime;
                float3 local=i.local/max(_Radius,.001);
                float coupling=saturate(Event(local,_Event0)+Event(local,_Event1)+Event(local,_Event2))*_Coupling;
                float side=abs(i.uv.y-.5)*2;
                if(_Dark>.5)
                {
                    // Black is a drifting field inside the energy, independent of strip borders.
                    float coarse=N(local.xz*4+float2(-t*2.8,t*1.7)+seed);
                    float torn=N(local.xy*11+float2(t*8,-t*5)+coarse*2+seed);
                    float depth=N(local.yz*7+float2(-t*6,t*3)+seed);
                    float coverage=smoothstep(.19,.56,coarse*.45+torn*.35+depth*.20);
                    float edge=pow(saturate(1-side),.8);
                    float ends=smoothstep(0,.06,i.uv.x)*(1-smoothstep(.87,1,i.uv.x));
                    float alpha=saturate(coverage*edge*ends*i.c.a*i.c.b*(1.7+.35*coupling));
                    float burn=smoothstep(.32,.77,torn*.65+depth*.35+coupling*.22);
                    float3 color=lerp(float3(.005,.001,.016),float3(.32,.009,.9)*_Emission,burn);
                    return half4(color*alpha,alpha);
                }
                // Preserve white/neon palette, adding location-dependent exposure and interruption.
                float width=.55+N(float2(i.uv.x*19-t*13+seed,0))*.4;
                float outline=1-smoothstep(width-.13,width,side);
                float gap=smoothstep(.12,.32,N(float2(i.uv.x*15+floor(t*17)+seed,0)));
                float hot=1-smoothstep(width*.18,width*.48,side);
                float3 bright=(float3(.44,.008,1.2)*(1-side)+float3(1.5,1.15,1.5)*hot)*_Emission;
                float alpha=outline*i.c.a*gap;
                return half4(bright*alpha*(1+coupling*.32),alpha);
            }
            ENDHLSL
        }
    }
}
