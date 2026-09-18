using System;
using UnityEngine;
namespace JJKGame.Player
{
    [Serializable] public sealed class ProductionPurpleBodySettings
    {
        [Range(1f,4f)] public float shellEmission=1.8f;
        [Range(.2f,2f)] public float shellDensity=.8f;
        [Range(4f,30f)] public float internalEnergy=22f;
        [Range(.2f,1.2f)] public float inwardSpeed=.58f;
        [Range(.1f,1.5f)] public float turbulenceSpeed=.8f;
        [Range(.015f,.12f)] public float arcRadius=.10f;
        [Range(6f,18f)] public float plasmaScale=11f;
        [Range(.05f,.20f)] public float arcLifetime=.105f;
        [Range(1f,1.7f)] public float outerReach=1.4f;
        [Range(.3f,1.5f)] public float instability=1f;
        [Range(.2f,2f)] public float voidContrast=1f;
    }
    [Serializable] public sealed class ProductionPurpleOuterSettings
    {
        public bool haloEnabled=true,lightningEnabled=true,debrisEnabled=true,distortionEnabled=true;
        [Range(0,.2f)] public float residualSeconds=.09f;
        [Range(0,1)] public float velocityInheritance=.35f;
        [Range(0,1)] public float maxResidualRadius=.55f;

        [Range(1.4f,1.8f)] public float haloRatio=1.55f;
        [Range(.05f,1f)] public float haloIntensity=.38f;
        [Range(8,48)] public int sparkCount=24;
        [Range(1f,5f)] public float sparkSpeed=3f;
        [Range(1f,8f)] public float sparkEmission=4f;
        [Range(1,5)] public int heroFragments=3;
        [Range(1f,3f)] public float heroFragmentScale=2.6f;
        [Range(1.4f,2.1f)] public float arcReach=1.95f;
        [Range(.08f,.2f)] public float arcLifetime=.13f;
        [Range(.025f,.12f)] public float arcWidth=.095f;
        [Range(2f,12f)] public float arcEmission=7f;
        [Range(1.15f,1.5f)] public float distortionRatio=1.5f;
        [Range(0f,.008f)] public float distortionStrength=.006f;
    }
    [CreateAssetMenu(menuName="JJK Game/VFX/프로덕션 Purple 통합 설정")]
    public sealed class ProductionPurpleVisualProfile : ScriptableObject
    {
        public bool useIntegratedCandidate=true;
        public bool bodyEnabled=true,outerEnabled=true;
        public ProductionPurpleBodySettings body=new();
        public ProductionPurpleOuterSettings outer=new();
        private static ProductionPurpleVisualProfile current;
        public static ProductionPurpleVisualProfile Current=>current!=null?current:current=Resources.Load<ProductionPurpleVisualProfile>("VFX/ProductionPurpleVisualProfile");
    }
}
