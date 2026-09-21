using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 재료 파열·충돌 최종 후보")]
    public sealed class PurpleIngredientFinalProfile : ScriptableObject
    {
        public bool candidateEnabled;
        private static PurpleIngredientFinalProfile current;
        public static PurpleIngredientFinalProfile Current=>current!=null?current:current=Resources.Load<PurpleIngredientFinalProfile>("VFX/PurpleIngredientFinalProfile");
    }
}
