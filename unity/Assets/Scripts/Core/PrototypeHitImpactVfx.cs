using UnityEngine;

namespace JJKGame.Core
{
    /// <summary>
    /// Compatibility facade for the existing BasicAttack call site. Rendering is
    /// delegated to the registered presentation runtime; no concrete renderer lives here.
    /// </summary>
    public static class PrototypeHitImpactVfx
    {
        public static void Spawn(Vector3 worldPosition, int chainStep)
        {
            int clampedStep = Mathf.Clamp(chainStep, 1, 3);
            PresentationVfxStyleId style = clampedStep switch
            {
                1 => PresentationVfxStyleId.BasicHit1,
                2 => PresentationVfxStyleId.BasicHit2,
                _ => PresentationVfxStyleId.BasicHitFinisher,
            };

            PresentationVfxRuntime.Spawn(
                PresentationVfxSpawnRequest.AtWorld(
                    worldPosition,
                    clampedStep >= 3
                        ? new Color(1f, 0.72f, 0.24f, 0.92f)
                        : new Color(0.72f, 0.90f, 1f, 0.84f),
                    Color.white,
                    0.08f,
                    clampedStep >= 3 ? 1.15f : 0.72f,
                    clampedStep >= 3 ? 0.20f : 0.13f,
                    0f,
                    PresentationVfxTimePolicy.Unscaled,
                    style
                )
            );
        }
    }
}
