using UnityEngine;
namespace JJKGame.Player
{
    /// <summary>Opt-in comparison only. Both switches default off; baseline assets are never written.</summary>
    [CreateAssetMenu(menuName="JJK Game/VFX/Purple 최종 폴리시 후보")]
    public sealed class PurpleFinalPolishProfile : ScriptableObject
    {
        public bool outerPolishEnabled;
        public bool fusionBirthEnabled;
        public ProductionPurpleOuterSettings outer = new()
        {
            sparkSpeed=3.4f, sparkEmission=5.5f, heroFragmentScale=3f,
            arcWidth=.11f, arcReach=2.05f, arcEmission=8.5f
        };
        [Range(0,1)] public float coronaContrast=1f;
        [Range(0,2)] public float compressionStrength=1f;
        [Range(0,2)] public float birthStrength=1f;
        [Range(.12f,.4f)] public float birthDuration=.28f;
        private static PurpleFinalPolishProfile current;
        public static PurpleFinalPolishProfile Current => current!=null?current:current=Resources.Load<PurpleFinalPolishProfile>("VFX/PurpleFinalPolishProfile");
    }
}
