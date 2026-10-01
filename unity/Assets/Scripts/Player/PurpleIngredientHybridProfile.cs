using UnityEngine;

namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 재료 Hybrid 방전 탐색")]
    public sealed class PurpleIngredientHybridProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(2f,5.5f)] public float eventRate=3.8f;
        [Range(1.6f,3.6f)] public float reachMultiplier=2.75f;
        [Range(.9f,2.4f)] public float depth=1.7f;
        [Range(1f,2.8f)] public float eventIntensity=2.1f;

        private static PurpleIngredientHybridProfile current;
        public static PurpleIngredientHybridProfile Current=>current!=null?current:current=Resources.Load<PurpleIngredientHybridProfile>("VFX/PurpleIngredientHybridProfile");
    }
}
