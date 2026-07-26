using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    public enum GojoVariantId
    {
        HiddenInventoryPreAwakening,
        HiddenInventoryAwakened,
        ModernTeacher,
        ShinjukuShowdown,
    }

    [DisallowMultipleComponent]
    public sealed class GojoVariantController : MonoBehaviour
    {
        [SerializeField] private GojoVariantId activeVariant = GojoVariantId.ModernTeacher;

        public GojoVariantId ActiveVariant => activeVariant;
        public string ShortLabel => activeVariant switch
        {
            GojoVariantId.HiddenInventoryPreAwakening => "회옥·옥절 · 각성 전",
            GojoVariantId.HiddenInventoryAwakened => "회옥·옥절 · 각성 후",
            GojoVariantId.ShinjukuShowdown => "신주쿠 결전",
            _ => "현대 · 교사",
        };

        public string DisplayName => $"GOJO SATORU · {ShortLabel}";

        public bool UsesRoundSunglasses =>
            activeVariant == GojoVariantId.HiddenInventoryPreAwakening
            || activeVariant == GojoVariantId.HiddenInventoryAwakened;

        public bool UsesBlindfold => activeVariant == GojoVariantId.ModernTeacher;
        public bool ShowsEyes => activeVariant == GojoVariantId.ShinjukuShowdown;

        // Only the Shinjuku battle version is planned to expose manual RCT burnout recovery.
        public bool CanManuallyRestoreTechniqueBurnout =>
            activeVariant == GojoVariantId.ShinjukuShowdown;

        public static GojoVariantController GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            GojoVariantController variant = owner.GetComponent<GojoVariantController>();
            return variant != null ? variant : owner.AddComponent<GojoVariantController>();
        }

        private void Awake()
        {
            CursedEnergyController.GetOrCreate(gameObject)
                ?.ApplyProfile(CursedEnergyProfileId.SixEyesEfficiency);
        }

        public void SetVariant(GojoVariantId variantId)
        {
            if (activeVariant == variantId)
            {
                return;
            }

            activeVariant = variantId;
            GojoPrototypeAvatar avatar = GetComponent<GojoPrototypeAvatar>();
            avatar?.Rebuild();
        }
    }
}
