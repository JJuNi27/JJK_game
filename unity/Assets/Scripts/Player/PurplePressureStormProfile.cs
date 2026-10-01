using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/무라사키 풍압 폭풍 후보")]
    public sealed class PurplePressureStormProfile:ScriptableObject
    {
        public bool candidateEnabled;
        [Range(4,14)] public int flowLanes=9;
        [Range(1.5f,4)] public float pressureReach=3.0f;
        [Range(.1f,1)] public float flowWidth=.70f;
        [Range(.5f,2.5f)] public float flowSpeed=1.3f;
        [Range(0,1)] public float pressureOpacity=.90f;
        [Range(0,8)] public float pressureEmission=3.2f;
        [Range(.006f,.08f)] public float lightningWidth=.045f;
        [Range(.5f,5)] public float lightningEmission=3.4f;
        [Range(.3f,2)] public float dischargeReach=1.1f;
        private static PurplePressureStormProfile current;
        public static PurplePressureStormProfile Current=>current!=null?current:current=Resources.Load<PurplePressureStormProfile>("VFX/PurplePressureStormProfile");
    }
}
