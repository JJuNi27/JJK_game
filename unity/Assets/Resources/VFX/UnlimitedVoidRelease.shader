Shader "JJK/UnlimitedVoidRelease"
{
    Properties
    {
        [PerRendererData] _MainTex ("Interior view", 2D) = "black" {}
        _Progress ("World reveal", Range(0,1)) = 0
        _Aspect ("View aspect", Float) = 1.77778
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            ZTest Always
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; };
            struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
                float _Progress, _Aspect, _ReleaseRadius;
                float3 _ReleaseOrigin;
                float4x4 _WorldFromClip;
            CBUFFER_END
            V Vert(A a) { V v; v.positionCS=TransformObjectToHClip(a.positionOS.xyz); v.uv=a.uv; return v; }
            half4 Frag(V v):SV_Target
            {
                float depth=SampleSceneDepth(v.uv);
                #if !UNITY_REVERSED_Z
                    depth=lerp(UNITY_NEAR_CLIP_VALUE,1,depth);
                #endif
                float3 world=ComputeWorldSpacePosition(v.uv,depth,_WorldFromClip);
                float edge=distance(world,_ReleaseOrigin)-_ReleaseRadius;
                float alpha=smoothstep(-.6,.6,edge)*(1-smoothstep(.92,1,_Progress));
                half3 color=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,v.uv).rgb;
                color+=float3(.14,.31,.59)*exp(-abs(edge)*2)*sin(_Progress*3.14159);
                return half4(color,alpha);
            }
            ENDHLSL
        }
    }
}
