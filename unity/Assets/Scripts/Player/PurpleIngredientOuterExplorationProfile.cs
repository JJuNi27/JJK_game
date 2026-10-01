using UnityEngine;

namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 재료 외곽 폭주 탐색 후보")]
    public sealed class PurpleIngredientOuterExplorationProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(1.15f,1.8f)] public float violence=1.45f;
        [Range(2.1f,3.2f)] public float energyReach=2.65f;
        [Range(.65f,1.6f)] public float frontBackDepth=1.15f;
        [Range(3f,6f)] public float eventRate=4.6f;

        private static PurpleIngredientOuterExplorationProfile current;
        public static PurpleIngredientOuterExplorationProfile Current=>current!=null?current:current=Resources.Load<PurpleIngredientOuterExplorationProfile>("VFX/PurpleIngredientOuterExplorationProfile");
    }
}
