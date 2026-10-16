using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/무라사키 텍스처 레이어 후보")]
    public sealed class PurpleWrapperTextureLayersProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(1,2.4f)] public float shellReach=1.65f;
        [Range(.4f,1.6f)] public float brushWidth=.95f;
        [Range(.12f,.6f)] public float brushCycle=.27f;
        [Range(.15f,.8f)] public float curveCycle=.43f;
        [Range(0,1)] public float windOpacity=.88f;
        [Range(1,5)] public float windEmission=1.7f;
        [Range(.06f,.5f)] public float releaseWindSeconds=.24f;
        [Range(.1f,1)] public float travelWindStrength=.82f;
        [Range(.04f,.25f)] public float darkDepth=.16f;
        [Range(.1f,1.4f)] public float darkWidth=.82f;
        [Range(.5f,5)] public float darkEmission=3.2f;
        [Range(.015f,.12f)] public float brightWidth=.045f;
        [Range(1,12)] public float brightEmission=7;
        [Range(.04f,.2f)] public float arcCycle=.105f;
        [Range(.5f,2)] public float stormSpeed=1.2f;
        [Range(0,1)] public float layerOcclusion=.85f;
        [Range(0,1)] public float spatialCoupling=.8f;
        static PurpleWrapperTextureLayersProfile current;
        public static PurpleWrapperTextureLayersProfile Current=>current!=null?current:current=Resources.Load<PurpleWrapperTextureLayersProfile>("VFX/PurpleWrapperTextureLayersProfile");
    }
}
