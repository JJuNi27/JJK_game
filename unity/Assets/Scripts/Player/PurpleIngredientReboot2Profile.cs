using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 재료 구체 리부트 2차 후보")]
    public sealed class PurpleIngredientReboot2Profile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(1.8f,3)] public float energyReach=2.35f;
        [Range(.25f,.6f)] public float bandWidth=.42f;
        [Range(.8f,2.5f)] public float collapseSpeed=1.5f;
        [Range(.8f,2.5f)] public float pressureSpeed=1.7f;
        [Range(.03f,.2f)] public float ruptureDuration=.12f;
        [Range(0,.15f)] public float fusionVerticalArc=.08f;
        [Range(1,2)] public int collisionFrames=2;
        [Range(.5f,1)] public float collisionContrast=.95f;
        private static PurpleIngredientReboot2Profile current;
        public static PurpleIngredientReboot2Profile Current=>current!=null?current:current=Resources.Load<PurpleIngredientReboot2Profile>("VFX/PurpleIngredientReboot2Profile");
    }
}
