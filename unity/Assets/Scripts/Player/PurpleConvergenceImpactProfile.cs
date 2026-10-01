using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 수렴 임팩트 후보")]
    public sealed class PurpleConvergenceImpactProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(1,2)] public int flashFrames=2;
        [Range(1,20)] public float flashStrength=12f;
        [Range(.03f,.05f)] public float hitStopDuration=.04f;
        [Range(.01f,.1f)] public float hitStopScale=.01f;
        [Range(.04f,.15f)] public float impulseDuration=.10f;
        [Range(0,.35f)] public float impulseDistance=.18f;
        [Range(0,1.5f)] public float impulseRoll=.7f;
        private static PurpleConvergenceImpactProfile current;
        public static PurpleConvergenceImpactProfile Current=>current!=null?current:current=Resources.Load<PurpleConvergenceImpactProfile>("VFX/PurpleConvergenceImpactProfile");
    }
}
