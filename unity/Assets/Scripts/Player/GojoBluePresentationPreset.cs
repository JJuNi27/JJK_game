using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    /// <summary>
    /// Presentation-only Blue request factory shared by CombatMVP and developer
    /// preview hosts. It owns renderer tuning, never gameplay rules or timing.
    /// </summary>
    public static class GojoBluePresentationPreset
    {
        private static readonly Color FieldPrimary =
            new Color(0.02f, 0.18f, 0.92f, 0.94f);
        private static readonly Color FieldSecondary =
            new Color(0.12f, 0.82f, 1f, 0.78f);
        private static readonly Color ImpactPrimary =
            new Color(0.02f, 0.20f, 0.94f, 0.90f);
        private static readonly Color ImpactSecondary =
            new Color(0.14f, 0.88f, 1f, 0.72f);

        public static PresentationVfxSpawnRequest CreateFieldRequest(
            Transform anchor,
            float radius,
            float duration,
            PresentationVfxTimePolicy timePolicy = PresentationVfxTimePolicy.Scaled
        )
        {
            return PresentationVfxSpawnRequest.Follow(
                anchor,
                Vector3.up * 0.35f,
                FieldPrimary,
                FieldSecondary,
                radius * 0.16f,
                radius,
                duration,
                0f,
                timePolicy,
                PresentationVfxStyleId.GojoBlue
            );
        }

        public static PresentationVfxSpawnRequest CreateImpactRequest(
            Vector3 worldPosition,
            float radius,
            PresentationVfxTimePolicy timePolicy = PresentationVfxTimePolicy.Unscaled
        )
        {
            return PresentationVfxSpawnRequest.AtWorld(
                worldPosition,
                ImpactPrimary,
                ImpactSecondary,
                0.10f,
                radius * 0.72f,
                0.28f,
                0f,
                timePolicy,
                PresentationVfxStyleId.GojoBlue
            );
        }
    }
}
