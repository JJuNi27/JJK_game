using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 재료 입체 파열·공간 반전 후보")]
    public sealed class PurpleIngredientFinal2Profile : ScriptableObject
    {
        public bool candidateEnabled;
        private static PurpleIngredientFinal2Profile current;
        public static PurpleIngredientFinal2Profile Current=>current!=null?current:current=Resources.Load<PurpleIngredientFinal2Profile>("VFX/PurpleIngredientFinal2Profile");
    }
}
