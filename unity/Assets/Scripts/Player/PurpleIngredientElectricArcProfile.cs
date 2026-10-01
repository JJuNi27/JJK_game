using UnityEngine;

namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 재료 전기 아크 탐색 후보")]
    public sealed class PurpleIngredientElectricArcProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(8, 28)] public int arcSlots = 24;
        [Range(2f, 8f)] public float eventRate = 5.7f;
        [Range(1.3f, 2.1f)] public float nearReach = 1.65f;
        [Range(2.1f, 3.5f)] public float farReach = 2.85f;
        [Range(.01f, .07f)] public float lineWidth = .034f;
        [Range(.5f, 3f)] public float brightness = 2.4f;

        private static PurpleIngredientElectricArcProfile current;
        public static PurpleIngredientElectricArcProfile Current => current != null
            ? current : current = Resources.Load<PurpleIngredientElectricArcProfile>("VFX/PurpleIngredientElectricArcProfile");
    }
}
