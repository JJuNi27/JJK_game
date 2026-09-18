using UnityEngine;
namespace JJKGame.Dev.PurpleBodyExploration
{
    [CreateAssetMenu(menuName="JJK Game/시각 탐색/Purple OuterR2 설정")]
    public sealed class PurpleOuterR2Profile : ScriptableObject
    {
        public PurpleBodyHybridD2R3Profile frozenR3;
        public PurpleOuterStage previewStage=PurpleOuterStage.Distortion;
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
}
