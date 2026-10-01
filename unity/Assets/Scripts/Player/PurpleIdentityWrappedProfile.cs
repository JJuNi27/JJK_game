using UnityEngine;

namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/무라사키 외곽 에너지 후보")]
    public sealed class PurpleIdentityWrappedProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(0,1)] public float edgeMotionScale=.25f;
        [Range(0,6)] public float rimGain=1.1f;
        [Range(0,8)] public float plumeGain=.55f;
        [Range(1.15f,2.1f)] public float reach=1.72f;
        [Range(.4f,2.5f)] public float eventRate=1.0f;
        [Range(1,4)] public float sparkGain=1.8f;
        [Range(1,5)] public float arcGain=2.3f;

        private static PurpleIdentityWrappedProfile current;
        public static PurpleIdentityWrappedProfile Current => current!=null?current:
            current=Resources.Load<PurpleIdentityWrappedProfile>("VFX/PurpleIdentityWrappedProfile");
    }
}
