using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/무라사키 다층 폭풍 후보")]
    public sealed class PurpleTempestLayersProfile:ScriptableObject
    {
        public bool candidateEnabled;
        [Range(1.5f,4)] public float windReach=3.0f;
        [Range(.5f,2)] public float windWidth=.9f;
        [Range(.5f,2.5f)] public float windSpeed=1.25f;
        [Range(0,1)] public float windOpacity=.72f;
        [Range(.03f,.18f)] public float brightWidth=.095f;
        [Range(1,12)] public float brightEmission=7;
        [Range(.08f,.5f)] public float darkWidth=.26f;
        [Range(.5f,5)] public float darkEmission=2.6f;
        [Range(.5f,2)] public float stormSpeed=1.2f;
        [Range(1,2.5f)] public float dischargeReach=1.65f;
        private static PurpleTempestLayersProfile current;
        public static PurpleTempestLayersProfile Current=>current!=null?current:current=Resources.Load<PurpleTempestLayersProfile>("VFX/PurpleTempestLayersProfile");
    }
}
