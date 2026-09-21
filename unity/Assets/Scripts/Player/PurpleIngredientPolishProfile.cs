using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 재료 구체 폴리시 후보")]
    public sealed class PurpleIngredientPolishProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(0,.09f)] public float silhouetteBreakup=.07f;
        [Range(1,2.5f)] public float blueFlowWidth=1.8f;
        [Range(1,2.5f)] public float blueFlowEmission=1.6f;
        [Range(1,2.5f)] public float redFlowWidth=1.65f;
        [Range(1,2.5f)] public float redFlowEmission=1.5f;
        [Range(.12f,.5f)] public float moteLength=.34f;
        private static PurpleIngredientPolishProfile current;
        public static PurpleIngredientPolishProfile Current=>current!=null?current:current=Resources.Load<PurpleIngredientPolishProfile>("VFX/PurpleIngredientPolishProfile");
    }
}
