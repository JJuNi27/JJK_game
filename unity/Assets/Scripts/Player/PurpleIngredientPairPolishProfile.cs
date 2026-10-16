using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple ?? ? ??? ??")]
    public sealed class PurpleIngredientPairPolishProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(1,1.6f)] public float reachMultiplier=1.28f;
        [Range(.5f,3)] public float blueRate=1.55f;
        [Range(.5f,3)] public float redRate=1.45f;
        [Range(.08f,.5f)] public float flowWidth=.27f;
        [Range(1,6)] public float emission=3.1f;
        [Range(.05f,.4f)] public float blueTrailSpan=.24f;
        [Range(.1f,.5f)] public float redTension=.23f;
        [Range(.1f,1)] public float pressureSpread=.72f;
        [Range(0,.7f)] public float relationshipBend=.38f;
        [Range(0,1)] public float groundResponse=.32f;
        [Range(.05f,.3f)] public float moteSize=.17f;
        [Range(.1f,1)] public float arrivalFlare=.58f;
        [Range(0,1)] public float coverageBreakup=.72f;
        static PurpleIngredientPairPolishProfile current;
        public static PurpleIngredientPairPolishProfile Current => current!=null ? current : current=Resources.Load<PurpleIngredientPairPolishProfile>("VFX/PurpleIngredientPairPolishProfile");
    }
}
