using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 재료 구체 질량 방향 후보")]
    public sealed class PurpleIngredientRebootProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(.02f,.15f)] public float silhouetteBreakup=.085f;
        [Range(1,5)] public float crackEmission=2.8f;
        [Range(1.4f,2.8f)] public float bandReach=1.8f;
        [Range(.08f,.35f)] public float bandWidth=.26f;
        [Range(.5f,2)] public float blueCollapseSpeed=1.1f;
        [Range(.5f,2.5f)] public float redPressureSpeed=1.4f;
        [Range(.1f,1)] public float supportingFlow=.48f;
        private static PurpleIngredientRebootProfile current;
        public static PurpleIngredientRebootProfile Current=>current!=null?current:current=Resources.Load<PurpleIngredientRebootProfile>("VFX/PurpleIngredientRebootProfile");
    }
}
