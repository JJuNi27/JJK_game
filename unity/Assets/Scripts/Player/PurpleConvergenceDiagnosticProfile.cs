using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 수렴 강도 진단 후보")]
    public sealed class PurpleConvergenceDiagnosticProfile:ScriptableObject
    {
        public bool candidateEnabled;
        [Range(6,8)] public int flashFrames=8;
        [Range(1,2)] public int peakFrames=2;
        [Range(.5f,4)] public float flashStrength=2.3f;
        [Range(.2f,.7f)] public float radialRadius=.42f;
        [Range(.015f,.07f)] public float hotCoreRadius=.035f;
        [Range(.12f,.15f)] public float holdDuration=.14f;
        [Range(.01f,.1f)] public float hitStopScale=.01f;
        [Range(.1f,.25f)] public float catchupDuration=.18f;
        [Range(.08f,.15f)] public float impulseDuration=.11f;
        [Range(.3f,.7f)] public float impulseDistance=.45f;
        [Range(1,3)] public float impulseRoll=1.75f;
        [Range(.1f,.2f)] public float pulseDuration=.15f;
        [Range(1,12)] public float bloomIntensity=8;
        [Range(.5f,2)] public float exposure=1.5f;
        private static PurpleConvergenceDiagnosticProfile current;
        public static PurpleConvergenceDiagnosticProfile Current=>current!=null?current:current=Resources.Load<PurpleConvergenceDiagnosticProfile>("VFX/PurpleConvergenceDiagnosticProfile");
    }
}
