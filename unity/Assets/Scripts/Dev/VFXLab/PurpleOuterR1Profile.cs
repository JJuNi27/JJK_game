using UnityEngine;
namespace JJKGame.Dev.PurpleBodyExploration
{
    public enum PurpleOuterStage { [InspectorName("본체만")] Body, [InspectorName("아우라까지")] Halo, [InspectorName("파편까지")] Debris, [InspectorName("외향 번개까지")] Lightning, [InspectorName("왜곡장까지")] Distortion }
    [CreateAssetMenu(menuName="JJK Game/시각 탐색/Purple OuterR1 설정")]
    public sealed class PurpleOuterR1Profile : ScriptableObject
    {
        public PurpleBodyHybridD2R3Profile frozenR3;
        public PurpleOuterStage previewStage=PurpleOuterStage.Distortion;
        [Range(1.4f,1.8f)] public float haloRatio=1.55f;
        [Range(.05f,1f)] public float haloIntensity=.38f;
        [Range(8,48)] public int sparkCount=24;
        [Range(1f,5f)] public float sparkSpeed=3f;
        [Range(1f,8f)] public float sparkEmission=4f;
        [Range(1.4f,2.1f)] public float arcReach=1.95f;
        [Range(.08f,.2f)] public float arcLifetime=.13f;
        [Range(.025f,.12f)] public float arcWidth=.095f;
        [Range(2f,12f)] public float arcEmission=7f;
        [Range(1.15f,1.5f)] public float distortionRatio=1.36f;
        [Range(0f,.008f)] public float distortionStrength=.0035f;
    }
}
