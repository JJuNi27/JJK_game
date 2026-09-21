using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 재료 구체 매크로 후보")]
    public sealed class PurpleIngredientMacroProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(.07f,.18f)] public float silhouetteBreakup=.13f;
        [Range(2,3.6f)] public float flowReach=3.05f;
        [Range(1,3)] public float blueSpeed=1.7f;
        [Range(.1f,.4f)] public float blueWidth=.24f;
        [Range(1,3)] public float blueEmission=2;
        [Range(1,3)] public float redSpeed=2.2f;
        [Range(.1f,.4f)] public float redWidth=.30f;
        [Range(1,3)] public float redEmission=2.5f;
        [Range(.1f,.4f)] public float trailSpan=.32f;
        private static PurpleIngredientMacroProfile current;
        public static PurpleIngredientMacroProfile Current=>current!=null?current:current=Resources.Load<PurpleIngredientMacroProfile>("VFX/PurpleIngredientMacroProfile");
    }
}
