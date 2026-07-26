using System;
using UnityEngine;

namespace JJKGame.Core
{
    public enum DamageDeliveryType
    {
        Unspecified,
        PhysicalStrike,
        CursedTechnique,
        DomainSureHit,
        Environmental,
    }

    [Flags]
    public enum DamageTraits
    {
        None = 0,
        DomainAmplification = 1 << 0,
        TechniqueNullification = 1 << 1,
        IgnoresInfinity = 1 << 2,
        Unblockable = 1 << 3,
    }

    public enum DamageResolution
    {
        Applied,
        Invalid,
        TargetDead,
        Invulnerable,
        Guarded,
    }

    public enum DamageGuardDecision
    {
        NoDecision,
        Block,
        Bypass,
    }

    public interface IDamageGuard
    {
        DamageGuardDecision EvaluateDamage(DamageContext context);
    }

    public readonly struct DamageContext
    {
        public DamageContext(
            float amount,
            GameObject source,
            DamageDeliveryType deliveryType,
            DamageTraits traits = DamageTraits.None,
            string actionName = "",
            Vector3 hitPoint = default
        )
        {
            Amount = Mathf.Max(0f, amount);
            Source = source;
            DeliveryType = deliveryType;
            Traits = traits;
            ActionName = string.IsNullOrWhiteSpace(actionName) ? deliveryType.ToString() : actionName;
            HitPoint = hitPoint;
        }

        public float Amount { get; }
        public GameObject Source { get; }
        public DamageDeliveryType DeliveryType { get; }
        public DamageTraits Traits { get; }
        public string ActionName { get; }
        public Vector3 HitPoint { get; }

        public bool HasTrait(DamageTraits trait)
        {
            return (Traits & trait) != 0;
        }

        public bool BypassesInfinity =>
            DeliveryType == DamageDeliveryType.DomainSureHit
            || HasTrait(DamageTraits.DomainAmplification)
            || HasTrait(DamageTraits.TechniqueNullification)
            || HasTrait(DamageTraits.IgnoresInfinity)
            || HasTrait(DamageTraits.Unblockable);

        public static DamageContext Legacy(float amount)
        {
            return new DamageContext(
                amount,
                null,
                DamageDeliveryType.Unspecified,
                DamageTraits.None,
                "LEGACY DAMAGE"
            );
        }
    }
}
