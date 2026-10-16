using UnityEngine;
namespace JJKGame.Player
{
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 시전자 차징 풍압 후보")]
    public sealed class PurpleIngredientCasterPressureProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(3,8)] public float stormRadius=5.2f;
        [Range(1,5)] public float stormHeight=3.4f;
        [Range(.5f,3)] public float flowWidth=1.8f;
        [Range(.2f,1)] public float opacity=.82f;
        [Range(1,5)] public float crestEmission=2.7f;
        [Range(.5f,3)] public float stormSpeed=1.55f;
        [Range(0,.6f)] public float ingredientCoupling=.24f;
        [Range(.1f,.6f)] public float releaseSeconds=.32f;
        [Range(1,6)] public float forwardSweep=3.8f;
        static PurpleIngredientCasterPressureProfile current;
        public static PurpleIngredientCasterPressureProfile Current=>current!=null?current:current=Resources.Load<PurpleIngredientCasterPressureProfile>("VFX/PurpleIngredientCasterPressureProfile");
    }
}
