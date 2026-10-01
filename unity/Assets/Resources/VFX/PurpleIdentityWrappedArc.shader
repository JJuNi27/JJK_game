Shader "JJKGame/Exploration/Purple Identity Wrapped Arc"
{
    Properties { _Emission("Emission",Float)=4 _Centre("Centre",Vector)=(0,0,0,2.5) }
    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+40" "RenderType"="Transparent"}
        Pass
        {
            Blend One One ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _Centre; float _Emission;
            CBUFFER_END
            struct A { float4 p:POSITION; float4 c:COLOR; };
            struct V { float4 p:SV_POSITION; float3 world:TEXCOORD0; float3 local:TEXCOORD1; float4 c:COLOR; };
            V Vert(A i)
            {
                V o; o.world=TransformObjectToWorld(i.p.xyz); o.p=TransformWorldToHClip(o.world);
                o.local=i.p.xyz; o.c=i.c; return o;
            }
            half4 Frag(V i):SV_Target
            {
                float3 eye=(GetCameraPositionWS()-_Centre.xyz)/_Centre.w;
                float3 rd=normalize(i.world-GetCameraPositionWS());
                float b=dot(eye,rd),screenRadius=length(eye-rd*b);
                float front=dot(normalize(GetCameraPositionWS()-_Centre.xyz),normalize(i.world-_Centre.xyz));
                float bodyMask=lerp(smoothstep(.96,1.10,screenRadius),1,smoothstep(-.12,.40,front));
                float axial=saturate(1-abs(i.local.y)*1.68);
                float fracture=frac(sin(dot(floor(i.world*21+_Time.y*19),float3(127.1,311.7,74.7)))*43758.5453);
                float light=(.55+.45*axial)*(.65+.35*fracture);
                return half4(i.c.rgb*_Emission*i.c.a*bodyMask*light,0);
            }
            ENDHLSL
        }
    }
}
