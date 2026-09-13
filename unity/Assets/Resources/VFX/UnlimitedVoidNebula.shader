Shader "JJK/UnlimitedVoidNebula"
{
    Properties
    {
        _Age ("Presentation time", Float) = 0
        _Layer ("Depth layer", Float) = 0
        _Intensity ("Nebula intensity", Float) = 1.15
        _Visibility ("Arrival", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-8" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Front
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 positionOS:POSITION; };
            struct V { float4 positionCS:SV_POSITION; float3 direction:TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
                float _Age, _Layer, _Intensity, _Visibility;
            CBUFFER_END
            V Vert(A a) { V v; v.positionCS=TransformObjectToHClip(a.positionOS.xyz); v.direction=a.positionOS.xyz; return v; }
            float Hash(float3 p) { return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453); }
            float Noise(float3 p)
            {
                float3 i=floor(p), f=frac(p); f=f*f*(3-2*f);
                return lerp(lerp(lerp(Hash(i),Hash(i+float3(1,0,0)),f.x),
                    lerp(Hash(i+float3(0,1,0)),Hash(i+float3(1,1,0)),f.x),f.y),
                    lerp(lerp(Hash(i+float3(0,0,1)),Hash(i+float3(1,0,1)),f.x),
                    lerp(Hash(i+float3(0,1,1)),Hash(i+1),f.x),f.y),f.z);
            }
            float Fbm(float3 p)
            {
                return Noise(p)*.52+Noise(p*2.03+7.1)*.27+Noise(p*4.13-5.2)*.14+Noise(p*8.21)*.07;
            }
            half4 Frag(V v):SV_Target
            {
                // Continuous 3D noise across the entire sphere: no UV seam or horizon.
                float3 d=normalize(v.direction);
                float3 p=d*(3.8+_Layer*1.7)+float3(_Age*.009,-_Age*.006,_Layer*13.7);
                float warp=Fbm(p+17);
                float cloud=Fbm(p+warp*2.8);
                float filament=Fbm(p*3.2+cloud*4);
                float structure=Fbm(d*2.4+float3(7,1,3)+_Layer*11);
                float islands=pow(smoothstep(.40,.64,structure),1.5);
                float mass=smoothstep(.40,.74,cloud)*islands;
                float grain=Fbm(p*7.3+filament*1.6);
                float ridge=pow(saturate(1-abs(filament-.51)*6),3)*mass;
                float3 color=lerp(float3(.001,.002,.009),float3(.016,.065,.24),mass);
                color+=float3(.07,.25,.48)*ridge*mass*(.28+grain*.9);
                color+=float3(.25,.46,.67)*pow(mass,2)*grain;
                float alpha=(_Layer<.5 ? .97 : mass*.32)*_Visibility;
                return half4(color*_Intensity,alpha);
            }
            ENDHLSL
        }
    }
}
