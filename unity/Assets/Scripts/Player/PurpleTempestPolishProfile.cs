using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/무라사키 다층 폭풍 폴리시 후보")]
    public sealed class PurpleTempestPolishProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(1.5f,4)] public float windReach=3;
        [Range(.5f,2.5f)] public float windWidth=1.65f;
        [Range(.5f,2.5f)] public float windSpeed=1.25f;
        [Range(0,1)] public float windOpacity=.76f;
        [Range(.06f,.5f)] public float releaseWindSeconds=.24f;
        [Range(.1f,1)] public float travelWindStrength=.48f;
        [Range(.03f,.18f)] public float brightWidth=.095f;
        [Range(1,12)] public float brightEmission=7;
        [Range(.08f,.5f)] public float darkWidth=.42f;
        [Range(.5f,5)] public float darkEmission=2.6f;
        [Range(.5f,2)] public float stormSpeed=1.2f;
        [Range(1,2.5f)] public float dischargeReach=1.65f;
        [Range(0,1)] public float spatialCoupling=.8f;
        private static PurpleTempestPolishProfile current;
        public static PurpleTempestPolishProfile Current=>current!=null?current:current=Resources.Load<PurpleTempestPolishProfile>("VFX/PurpleTempestPolishProfile");
    }
}
