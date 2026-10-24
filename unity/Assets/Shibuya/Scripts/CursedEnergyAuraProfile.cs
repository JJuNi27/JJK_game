using System;
using UnityEngine;

namespace JJKGame.Shibuya
{
    [Serializable]
    public sealed class CursedAuraLayer
    {
        public bool enabled = true;
        [Min(0)] public float intensity = 1;
        [Range(0,1)] public float opacity = .7f;
    }

    [CreateAssetMenu(menuName="JJK Game/시부야/주력 아우라 설정",fileName="Cursed Energy Aura")]
    public sealed class CursedEnergyAuraProfile : ScriptableObject
    {
        public bool auraEnabled = true;
        public GameObject visualPrefab;
        [Min(.01f)] public float fadeIn = .12f;
        [Min(.01f)] public float fadeOut = .24f;
        [Min(.1f)] public float scale = 1;
        [Min(0)] public float intensity = 1.25f;
        public Vector3 offset;
        public bool suppressWhenHit = true;
        public bool suppressDuringCinematic = true;
        [ColorUsage(false,true)] public Color mainColor = new Color(.02f,.75f,1f);
        public bool overrideSupportingColors;
        [ColorUsage(false,true)] public Color accentColor = new Color(.4f,1f,1f);
        [ColorUsage(false,true)] public Color rimColor = new Color(.02f,.8f,1f);
        [ColorUsage(false,true)] public Color particleColor = new Color(.4f,1f,1f);
        public Color inkColor = new Color(.004f,.007f,.018f,1);
        public CursedAuraLayer mainAura = new CursedAuraLayer {opacity=.84f,intensity=1.6f};
        public CursedAuraLayer ink = new CursedAuraLayer {opacity=1,intensity=1};
        public CursedAuraLayer wisps = new CursedAuraLayer {opacity=.8f,intensity=2.2f};
        public CursedAuraLayer bodyRim = new CursedAuraLayer {opacity=.55f,intensity=1.4f};
        public CursedAuraLayer motes = new CursedAuraLayer {opacity=.8f,intensity=1.5f};
        [Min(.2f)] public float height = 2.4f;
        [Min(.1f)] public float width = .75f;
        [Min(.05f)] public float motionSpeed = 1.2f;
        [Range(0,1.5f)] public float irregularity = .85f;
        [Range(0,60)] public float particleRate = 22;
        [Range(.015f,.2f)] public float fieldThickness = .14f;
        [Range(.25f,1.5f)] public float fieldDensity = .9f;
        [Range(.2f,.8f)] public float detailScale = .55f;
        [Min(.01f)] public float referenceMaxEnergy = 100f;
        [Range(1f,2f)] public float baselineExpansion = 1.25f;
        // X is max energy / referenceMaxEnergy; Y scales external reach, never the body.
        public AnimationCurve maxEnergySizeCurve = DefaultSizeCurve();
        [Min(.01f)] public float minimumSizeMultiplier = .5f;
        [Min(.01f)] public float maximumSizeMultiplier = 3f;
        [Min(0)] public float sizeResponseTime = .2f;
        [Range(0,1)] public float verticalSizeResponse = .5f;
        [Range(0,1)] public float innerFlowVisibility = .65f;
        [Range(.25f,2)] public float innerFlowDensity = 1f;
        // Neutral defaults retain the existing presentation. PASS4B3b is opt-in.
        public bool preserveLocalFlameShape;
        [Range(0,1)] public float extraFieldReach;
        [Range(1,1.5f)] public float inkBoundaryScale = 1;
        [Range(0,1.5f)] public float inkInteriorArea = 1;
        public bool detachedTips;
        [Range(.2f,1.5f)] public float tipCycleRate = .7f;
        [Range(0,1)] public float upperFieldConnection;
        [Range(0,1)] public float lowerFieldConnection;
        [Range(12,32)] public int detailFlowCount = 24;
        [Range(0,.06f)] public float surfaceOffset = .012f;
        [Range(.5f,3)] public float lowerBodyWeight = 1.5f;
        [Range(0,2)] public float outwardFlow = .45f;
        [Range(.1f,2)] public float upwardFlow = .85f;
        public int Revision {get;private set;}
        public Color Accent => overrideSupportingColors ? accentColor : Color.Lerp(mainColor,Color.white,.6f);
        public Color Rim => overrideSupportingColors ? rimColor : mainColor;
        public Color Particle => overrideSupportingColors ? particleColor : Color.Lerp(mainColor,Color.white,.45f);
        public static AnimationCurve DefaultSizeCurve() => new AnimationCurve(
            new Keyframe(.5f,.85f,.3f,.3f),new Keyframe(1,1,.3f,1),
            new Keyframe(1.5f,1.5f,1,1),new Keyframe(2,2,1,.4f),
            new Keyframe(2.5f,2.2f,.4f,.2f),new Keyframe(3,2.3f,.2f,.2f));
        public float ReferenceMaxEnergy => FinitePositive(referenceMaxEnergy,100);
        public float BaseExpansion => Mathf.Clamp(FinitePositive(baselineExpansion,1.25f),1,2);
        public float VerticalExpansion(float expansion) => BaseExpansion*Mathf.Pow(Mathf.Max(.01f,expansion/BaseExpansion),
            float.IsNaN(verticalSizeResponse)? .5f:Mathf.Clamp01(verticalSizeResponse));
        public float SizeMultiplier(float maximumEnergy)
        {
            float ratio=FinitePositive(maximumEnergy,ReferenceMaxEnergy)/ReferenceMaxEnergy;
            float value=maxEnergySizeCurve!=null&&maxEnergySizeCurve.length>0?maxEnergySizeCurve.Evaluate(ratio):1;
            if(float.IsNaN(value)||float.IsInfinity(value))value=1;
            float a=Mathf.Clamp(FinitePositive(minimumSizeMultiplier,.5f),.01f,8);
            float b=Mathf.Clamp(FinitePositive(maximumSizeMultiplier,3),.01f,8);
            return Mathf.Clamp(value,Mathf.Min(a,b),Mathf.Max(a,b));
        }
        private static float FinitePositive(float value,float fallback)=>float.IsNaN(value)||float.IsInfinity(value)||value<=0?fallback:value;
        private void OnValidate() { Revision++; }
    }
}
