using UnityEngine;

namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 재료 확장 방전 프로필")]
    public sealed class PurpleIngredientExpandedDischargeProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(1,4)] public int largeEvents=2;
        [Range(2,8)] public int mediumEvents=5;
        [Range(2,10)] public int localEvents=6;
        [Range(.8f,3.5f)] public float largeRate=2.35f;
        [Range(2f,7f)] public float mediumRate=4.4f;
        [Range(3f,10f)] public float localRate=6.8f;
        [Range(.12f,.5f)] public float largeLifeMin=.19f;
        [Range(.18f,.7f)] public float largeLifeMax=.38f;
        [Range(.06f,.3f)] public float mediumLifeMin=.095f;
        [Range(.1f,.4f)] public float mediumLifeMax=.19f;
        [Range(.02f,.15f)] public float localLifeMin=.035f;
        [Range(.035f,.22f)] public float localLifeMax=.09f;
        [Range(2.3f,4.5f)] public float largeReach=3.05f;
        [Range(1.3f,2.6f)] public float mediumReach=1.9f;
        [Range(.72f,1.25f)] public float localReach=.98f;
        [Range(1f,3f)] public float depth=1.9f;
        [Range(1f,3f)] public float intensity=2.25f;
        [Range(2,7)] public int largeTopologyStates=5;
        [Range(2,8)] public int mediumTopologyStates=6;
        [Range(2,6)] public int localTopologyStates=3;

        private static PurpleIngredientExpandedDischargeProfile current;
        public static PurpleIngredientExpandedDischargeProfile Current=>current!=null?current:current=Resources.Load<PurpleIngredientExpandedDischargeProfile>("VFX/PurpleIngredientExpandedDischargeProfile");
    }
}
