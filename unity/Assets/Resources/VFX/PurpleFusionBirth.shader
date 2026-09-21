Shader "JJKGame/Candidate/Purple Fusion Birth"
{
    Properties { _Centre("Centre",Vector)=(0,0,0,3) _Birth("Birth",Vector)=(0,.28,1,0) }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+35" "RenderType"="Transparent"}
        Pass
        {
            Blend One One
            ZWrite Off ZTest Always Cull Front
            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Centre,_Birth;
            CBUFFER_END
            struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 world:TEXCOORD0;};
            V Vert(A i){V o;o.world=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.world);return o;}
            float Hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
            float Noise(float3 p)
            {
                float3 a=floor(p),f=frac(p);f=f*f*(3-2*f);
                return lerp(lerp(lerp(Hash(a),Hash(a+float3(1,0,0)),f.x),lerp(Hash(a+float3(0,1,0)),Hash(a+float3(1,1,0)),f.x),f.y),
                    lerp(lerp(Hash(a+float3(0,0,1)),Hash(a+float3(1,0,1)),f.x),lerp(Hash(a+float3(0,1,1)),Hash(a+1),f.x),f.y),f.z);
            }
            half4 Frag(V i):SV_Target
            {
                float3 eye=(GetCameraPositionWS()-_Centre.xyz)/_Centre.w,rd=normalize(i.world-GetCameraPositionWS());
                float b=dot(eye,rd),h=b*b-dot(eye,eye)+2.3*2.3;if(h<=0)return 0;
                h=sqrt(h);float begin=max(0,-b-h),end=-b+h;
                float scene=LinearEyeDepth(SampleSceneDepth(i.p.xy/_ScaledScreenParams.xy),_ZBufferParams);
                end=min(end,scene/(_Centre.w*max(.001,-mul((float3x3)UNITY_MATRIX_V,rd).z)));if(end<=begin)return 0;
                float age=_Birth.x,flash=exp(-pow((age+.003)/.023,2));
                // Only the pressure shell ends early. Flash and fragment clocks stay intact.
                float waveT=saturate(age/min(_Birth.y,.11));
                float waveRadius=1.02+.86*pow(waveT,.6),waveFade=saturate(age/.01)*pow(1-waveT,1.7);
                float closest=length(eye-rd*b),clearBody=smoothstep(1.01,1.12,closest);
                // Analytic shell intersections avoid radial bands from undersampling a
                // thin, fast-expanding shell. Front/back masks remain genuinely 3D.
                // Bound the diffuse flash by scene depth too; it must not shine through an occluder.
                float flashSum=exp(-closest*closest*2.8)*.5*(tanh(1.9*(end+b))-tanh(1.9*(begin+b))),waveSum=0;
                float wh=b*b-dot(eye,eye)+waveRadius*waveRadius;
                if(wh>0 && waveFade>0)
                {
                    float distance=sqrt(wh);
                    [unroll]for(int side=0;side<2;side++)
                    {
                        float hit=-b+(side==0?-distance:distance);
                        if(hit<begin || hit>end)continue;
                        float3 d=(eye+rd*hit)/waveRadius;
                        float tear=Noise(d*4.7+float3(age*3.1,-age*5.3,1.7))*.7+Noise(d*11.3+age*4)*.3;
                        // Fixed world-space sectors, not a camera-facing ring. Broad missing
                        // sectors and a soft limb prevent a complete transparent dome contour.
                        float sector=max(smoothstep(.22,.72,dot(d,normalize(float3(.82,.30,-.48)))),
                            .65*smoothstep(.62,.90,dot(d,normalize(float3(-.60,.30,.74)))));
                        float broken=smoothstep(.49,.66,tear)*sector;
                        float softLimb=smoothstep(.04,.28,distance/waveRadius);
                        waveSum+=broken*.20*softLimb*smoothstep(0,.025,wh);
                    }
                }
                float3 colour=flashSum*flash*float3(3.3,1.65,2.8)+waveSum*waveFade*clearBody*float3(5,.13,2.65);
                return half4(colour*_Birth.z,0);
            }
            ENDHLSL
        }
    }
}
