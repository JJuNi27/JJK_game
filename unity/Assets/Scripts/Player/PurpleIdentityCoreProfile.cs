using UnityEngine;

namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 중심 질량 후보")]
    public sealed class PurpleIdentityCoreProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(12,60)] public float coreRadiance=34;
        [Range(0,.035f)] public float edgeBreakup=.021f;
        [Range(.3f,2)] public float fissureContrast=1.55f;
        [Range(.5f,2)] public float energyChurn=1.3f;

        private static PurpleIdentityCoreProfile current;
        public static PurpleIdentityCoreProfile Current => current!=null?current:
            current=Resources.Load<PurpleIdentityCoreProfile>("VFX/PurpleIdentityCoreProfile");
    }
}
