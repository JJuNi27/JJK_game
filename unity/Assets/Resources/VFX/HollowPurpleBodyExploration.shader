Shader "JJKGame/Exploration/Purple Body ABC"
{
    Properties
    {
        _Variant("Prototype",Float)=0 _PhaseTime("Clock",Float)=0
        _CoreRatio("Core ratio",Float)=.175 _Density("Shared density",Float)=1.15
        _BodyEmission("Body emission",Float)=.7 _CoreEmission("Core emission",Float)=7
        _NearBlack("Near black",Color)=(.008,.0015,.02,1)
        _DeepViolet("Deep violet",Color)=(.065,.008,.18,1)
        _Magenta("Magenta",Color)=(.8,.016,.27,1)
        _Violet("Violet",Color)=(.26,.035,.9,1)
        _HotPink("Hot pink",Color)=(1.8,.055,.65,1)
        _Breakup("Silhouette displacement",Float)=0 _Depth("Depth strength",Float)=1
        _FlowRates("Inward / surface / outward",Vector)=(.75,.42,1.1,0)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Blend One OneMinusSrcAlpha
            ZWrite Off ZTest Always Cull Front
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float _Variant,_PhaseTime,_CoreRatio,_Density,_BodyEmission,_CoreEmission,_Breakup,_Depth;
            float4 _NearBlack,_DeepViolet,_Magenta,_Violet,_HotPink,_FlowRates;
            CBUFFER_END
            struct A { float4 positionOS:POSITION; };
            struct V { float4 positionCS:SV_POSITION; float3 p:TEXCOORD0; };
            V Vert(A i) { V o; o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.p=i.positionOS.xyz;return o; }
            float H(float3 p) { p=frac(p*.3183099+float3(.17,.31,.53));p*=19.19;return frac(p.x*p.y*p.z*(p.x+p.y+p.z)); }
            float N(float3 p)
            {
                float3 a=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(lerp(H(a),H(a+float3(1,0,0)),f.x),lerp(H(a+float3(0,1,0)),H(a+float3(1,1,0)),f.x),f.y),
                    lerp(lerp(H(a+float3(0,0,1)),H(a+float3(1,0,1)),f.x),lerp(H(a+float3(0,1,1)),H(a+1),f.x),f.y),f.z);
            }
            float3 Rz(float3 p,float a) { float s=sin(a),c=cos(a);return float3(c*p.x-s*p.y,s*p.x+c*p.y,p.z); }
            float3 Ry(float3 p,float a) { float s=sin(a),c=cos(a);return float3(c*p.x+s*p.z,p.y,-s*p.x+c*p.z); }
            half4 Frag(V i):SV_Target
            {
                float3 eye=TransformWorldToObject(GetCameraPositionWS()),rd=normalize(i.p-eye);
                // The cube is only a raster proxy; every visible boundary comes from this density field.
                float b=dot(eye,rd),h=b*b-dot(eye,eye)+.49*.49;
                if(h<=0) return 0;
                h=sqrt(h);float begin=max(0,-b-h),end=-b+h;
                float2 uv=i.positionCS.xy/_ScaledScreenParams.xy;
                float scene=LinearEyeDepth(SampleSceneDepth(uv),_ZBufferParams);
                float3 rayWS=mul((float3x3)unity_ObjectToWorld,rd);
                float limit=scene/max(.0001,-mul((float3x3)UNITY_MATRIX_V,rayWS).z);
                end=min(end,limit);if(end<=begin)return 0;
                bool deep=_Variant>1.5;
                // Review quality: enough samples for thin density layers without grain or coherent bands.
                int steps=deep?160:128;
                float stride=(end-begin)/steps,clock=_PhaseTime;
                float4 sum=0;
                [loop] for(int k=0;k<steps;k++)
                {
                    float3 p=eye+rd*(begin+(k+.5)*stride);
                    float r=length(p),t=clock*.35;
                    float3 flow=p*15+sin(p.yzx*13+float3(t,-t*.7,t*.5))*.6+float3(t,-t*.6,t*.3);
                    float n=N(flow)*.7+N(flow*2.1-t*.3)*.3;
                    float ridges=pow(saturate(1-abs(n-.57)*13),5);
                    float3 angular=normalize(p+float3(.00001,0,0));
                    float boundaryFrequency=deep?8:3.6;
                    float boundaryNoise=N(angular*boundaryFrequency+sin(angular.zxy*7-clock*.6)*.35);
                    float displacement=_Breakup*(boundaryNoise-.5)*.90;
                    // Broad attached breaks: bounded folds instead of dense needle-like extrusions.
                    displacement+=_Breakup*pow(saturate((boundaryNoise-.61)*3.7),3)*.28;
                    float radius=.375+displacement;
                    float mass=1-smoothstep(radius-.007,radius+.002,r);
                    float skin=exp(-abs(r-radius)*180);
                    float density=mass*55;
                    float lit=.40+.60*saturate(dot(normalize(p+float3(.00001,0,0)),normalize(float3(-.45,.7,-.5))));
                    float3 colour=lerp(_NearBlack.rgb,_DeepViolet.rgb,smoothstep(.26,.73,n))*lit;
                    float energy=ridges*smoothstep(.49,.73,N(p*9-t))*.65;
                    colour+=lerp(_Violet.rgb,_Magenta.rgb,n)*energy;
                    colour+=_Violet.rgb*skin*.09;
                    if(_Variant>.5 && !deep)
                    {
                        float tear=skin*pow(saturate((boundaryNoise-.56)*3),5);
                        colour+=_Magenta.rgb*tear*.65;
                        density*=1-skin*(1-smoothstep(.02,.25,boundaryNoise))*.65;
                    }
                    if(deep)
                    {
                        // Inner absorbing mass. Different axes, rates and scales in each 3D layer.
                        float3 inner=Ry(Rz(p,clock*.17),-.5);
                        float innerN=N(inner*13+float3(0,clock*.23,0));
                        float innerMass=(1-smoothstep(.22,.25,r));
                        float3 mid=Ry(Rz(p,-clock*_FlowRates.x*.45),.7);
                        float a=atan2(mid.y,mid.x);
                        float spiral=pow(saturate(cos(a*3+log(max(.025,r))*8+clock*_FlowRates.x*4)),22);
                        float inward=pow(saturate(cos(r*62+clock*_FlowRates.x*10+a*2)),10);
                        float midN=N(mid*23+float3(clock*.31,-clock*.24,clock*.17));
                        float midLayer=smoothstep(.17,.23,r)*(1-smoothstep(.315,.35,r));
                        float midDensity=(.08+midN*.18+spiral*(.50+inward*1.2))*midLayer;
                        float3 outer=Rz(Ry(p,clock*_FlowRates.y*.5),-.65);
                        float surfaceN=N(outer*17+sin(p.yzx*11-clock*_FlowRates.y)*.8);
                        float surfaceLayer=exp(-abs(r-radius)*120)*smoothstep(.43,.72,surfaceN);
                        float outward=pow(saturate(cos((r-.34)*55-clock*_FlowRates.z*8+atan2(p.y,p.x)*3+angular.z*5)),14)
                            *smoothstep(.355,.38,r)*(1-smoothstep(.40,.425,r))*smoothstep(.67,.82,surfaceN);
                        float innerD=innerMass*(65+innerN*25)*mass;
                        float midD=midDensity*36*mass;
                        float surfaceD=surfaceLayer*24*mass;
                        float outwardD=outward*(25*mass+8);
                        density=innerD+midD+surfaceD+outwardD;
                        float3 innerColour=lerp(_NearBlack.rgb,_DeepViolet.rgb,innerN*.65);
                        float3 midColour=lerp(_DeepViolet.rgb*.35,_Violet.rgb*.80,spiral*.90);
                        midColour+=_Magenta.rgb*inward*spiral*1.1;
                        float3 surfaceColour=lerp(_NearBlack.rgb,_DeepViolet.rgb,surfaceN);
                        surfaceColour+=_Magenta.rgb*pow(saturate(1-abs(surfaceN-.55)*14),6)*.55;
                        // Each layer contributes its own emission; an outer pulse cannot tint the entire interior.
                        colour=((innerColour*innerD+midColour*midD+surfaceColour*surfaceD)*lit
                            +_HotPink.rgb*outwardD*.6)/max(.0001,density);
                        density=lerp(mass*55,density,_Depth);
                    }
                    float alpha=1-exp(-max(0,density)*_Density*stride);
                    sum.rgb+=(1-sum.a)*colour*_BodyEmission*alpha;
                    sum.a+=(1-sum.a)*alpha;
                    if(sum.a>.995)break;
                }
                // Same compact centre for A/B/C, with a thin hot rim and scene-depth occlusion.
                float3 nearest=eye+rd*clamp(-b,begin,end);
                float cr=.375*_CoreRatio;
                float distance=length(nearest)+(N(nearest*110+clock*.5)-.5)*.003;
                float core=1-smoothstep(cr*.87,cr,distance);
                float rim=exp(-abs(distance-cr*1.045)*480);
                sum.rgb+=core*float3(1,.9,1)*_CoreEmission+rim*_HotPink.rgb*.7;
                sum.a=max(sum.a,saturate(core+rim*.4));
                return sum;
            }
            ENDHLSL
        }
    }
}
