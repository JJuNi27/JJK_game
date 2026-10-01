using UnityEngine;

namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 응집 질량 후보")]
    public sealed class PurpleIdentityMassProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(2,24)] public float coreRadiance=8;
        [Range(0,.04f)] public float edgeBreakup=.028f;
        [Range(.5f,3)] public float fissureContrast=2.2f;
        [Range(.5f,3)] public float energyChurn=1.8f;

        private static PurpleIdentityMassProfile current;
        public static PurpleIdentityMassProfile Current => current!=null?current:
            current=Resources.Load<PurpleIdentityMassProfile>("VFX/PurpleIdentityMassProfile");
    }
}
