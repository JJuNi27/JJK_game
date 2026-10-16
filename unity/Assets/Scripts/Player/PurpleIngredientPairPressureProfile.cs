using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 재료 쌍 대형 풍압 후보")]
    public sealed class PurpleIngredientPairPressureProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(1.5f,3.5f)] public float pressureReach=2.7f;
        [Range(.4f,1.6f)] public float pressureWidth=1.15f;
        [Range(.2f,1.2f)] public float pressureDepth=.72f;
        [Range(.2f,.9f)] public float density=.72f;
        [Range(.5f,4)] public float diffuseEmission=1.55f;
        [Range(1,8)] public float crestEmission=4.2f;
        [Range(.1f,.6f)] public float blueSpan=.38f;
        [Range(.4f,1.7f)] public float redSpread=1.15f;
        [Range(0,.7f)] public float gapBend=.32f;
        [Range(.2f,1)] public float erosion=.62f;
        static PurpleIngredientPairPressureProfile current;
        public static PurpleIngredientPairPressureProfile Current=>current!=null?current:current=Resources.Load<PurpleIngredientPairPressureProfile>("VFX/PurpleIngredientPairPressureProfile");
    }
}
