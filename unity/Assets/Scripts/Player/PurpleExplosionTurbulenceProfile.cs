using UnityEngine;

namespace JJKGame.Player
{
    [CreateAssetMenu(menuName = "JJK Game/VFX/Purple 폭발 본체 난류 후보")]
    public sealed class PurpleExplosionTurbulenceProfile : ScriptableObject
    {
        public bool candidateEnabled;
        [Range(1, 1.6f)] public float energyGain = 1.25f;
        [Range(.5f, 3)] public float flowSpeed = 1.8f;
        [Range(.02f, .13f)] public float silhouetteBreakup = .095f;
        [Range(.02f, .1f)] public float domainWarp = .072f;
        [Range(.5f, 2)] public float darkDepth = 1.45f;
        [Range(12, 50)] public float coreRadiance = 34;
        [Range(.055f, .13f)] public float coreRadius = .105f;
        [Range(.5f, 2)] public float density = 1.15f;

        private static PurpleExplosionTurbulenceProfile current;
        public static PurpleExplosionTurbulenceProfile Current => current != null ? current :
            current = Resources.Load<PurpleExplosionTurbulenceProfile>("VFX/PurpleExplosionTurbulenceProfile");
    }
}
