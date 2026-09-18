Shader "JJKGame/Exploration/Purple R3 Event Arc"
{
    Properties { _BodyCentre("Centre",Vector)=(0,0,0,2.5) _PhaseTime("Clock",Float)=0 }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+20" "RenderType"="Transparent"}
        Pass
        {
            Blend One One
            ZWrite Off Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _BodyCentre;float _PhaseTime;
            CBUFFER_END
            struct A{float4 positionOS:POSITION;float3 normalOS:NORMAL;float4 color:COLOR;float2 uv:TEXCOORD0;};
            struct V{float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float4 color:COLOR;float2 uv:TEXCOORD2;};
            V Vert(A i){V o;o.world=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);o.normal=TransformObjectToWorldNormal(i.normalOS);o.color=i.color;o.uv=i.uv;return o;}
            half4 Frag(V i):SV_Target
            {
                float3 eye=GetCameraPositionWS(),delta=i.world-eye;float distance=length(delta);float3 rd=delta/max(.001,distance);
                float3 origin=eye-_BodyCentre.xyz;float b=dot(origin,rd),h=b*b-dot(origin,origin)+_BodyCentre.w*_BodyCentre.w;
                float through=0;
                if(h>0){h=sqrt(h);through=max(0,min(distance,-b+h)-max(0,-b-h))/_BodyCentre.w;}
                float facing=saturate(dot(normalize(i.normal),-rd));
                float white=pow(facing,3)*i.color.g;
                float3 light=lerp(float3(4,.008,1.6),float3(22,15,23),white);
                light*=exp(-through*float3(.7,2.8,1.1));
                float pulse=.65+.35*abs(sin(i.uv.x*23-_PhaseTime*93));
                float alpha=saturate(i.color.a)*exp(-through*1.8)*smoothstep(0,.035,i.uv.x)*(1-smoothstep(.90,1,i.uv.x));
                return half4(light*pulse*alpha,alpha);
            }
            ENDHLSL
        }
    }
}
