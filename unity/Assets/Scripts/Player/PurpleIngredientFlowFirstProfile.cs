using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 재료 Flow First 후보")]
    public sealed class PurpleIngredientFlowFirstProfile : ScriptableObject
    {
        public bool candidateEnabled;
        public bool blueFlowEnabled = true;
        public bool redFlowEnabled = true;
        public bool detailsEnabled = true;
        public bool scatterEnabled = true;
        public bool purpleAccentEnabled = true;
        [Range(6,20)] public int flowCount = 12;
        [Range(2,5)] public float reach = 3.5f;
        [Range(.06f,.35f)] public float brushWidth = .21f;
        [Range(.5f,3)] public float flowBrightness = 1.8f;
        [Range(.5f,3)] public float flowSpeed = 1.7f;
        [Range(0,.5f)] public float groundStrength = .17f;
        [Range(4,20)] public int orbCount = 12;
        [Range(.5f,3)] public float accentStrength = 1.5f;
        private static PurpleIngredientFlowFirstProfile current;
        public static PurpleIngredientFlowFirstProfile Current => current != null ? current : current = Resources.Load<PurpleIngredientFlowFirstProfile>("VFX/PurpleIngredientFlowFirstProfile");
    }
}
