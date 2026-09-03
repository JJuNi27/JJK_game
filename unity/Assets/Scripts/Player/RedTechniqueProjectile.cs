using System;
using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    /// <summary>
    /// Canonical production defaults shared with presentation-only preview hosts.
    /// These values describe Red's spatial inputs and do not execute gameplay.
    /// </summary>
    public static class GojoRedProductionDefaults
    {
        public const float Range = 11f;
        public const float ProjectileSpeed = 22f;
        public const float Radius = 1.7f;
        public const float SpawnHeight = 1f;
        public const float SpawnForwardOffset = 0.9f;

        public static float TravelDuration => Range / ProjectileSpeed;
        public static float PreviewEndForwardDistance => SpawnForwardOffset + Range;
    }

    public sealed class RedTechniqueProjectile : MonoBehaviour
    {
        private readonly HashSet<Health> damagedTargets = new HashSet<Health>();
        private readonly HashSet<Health> frameTargets = new HashSet<Health>();

        private Health owner;
        private Vector3 direction;
        private float speed;
        private float maxRange;
        private float radius;
        private float damage;
        private float pushSpeed;
        private float hitStun;
        private float travelled;
        private Action<Health> onTargetHit;
        private Action onFirstImpact;
        private bool impactPlayed;
        private PresentationVfxHandle presentationHandle;

        public void Configure(
            Health newOwner,
            Vector3 newDirection,
            float newSpeed,
            float newMaxRange,
            float newRadius,
            float newDamage,
            float newPushSpeed,
            float newHitStun,
            Action<Health> newOnTargetHit,
            Action newOnFirstImpact
        )
        {
            owner = newOwner;
            direction = newDirection;
            direction.y = 0f;
            direction = direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : Vector3.forward;
            speed = Mathf.Max(0.1f, newSpeed);
            maxRange = Mathf.Max(0.1f, newMaxRange);
            radius = Mathf.Max(0.1f, newRadius);
            damage = Mathf.Max(0f, newDamage);
            pushSpeed = Mathf.Max(0f, newPushSpeed);
            hitStun = Mathf.Max(0f, newHitStun);
            onTargetHit = newOnTargetHit;
            onFirstImpact = newOnFirstImpact;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            BuildVisual();
        }

        private void Update()
        {
            float step = speed * Time.deltaTime;
            Vector3 previous = transform.position;
            Vector3 next = previous + direction * step;

            ApplyHitsBetween(previous, next);
            transform.position = next;
            travelled += step;
            if (travelled >= maxRange)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            presentationHandle.Stop(PresentationVfxStopMode.Immediate);
        }

        private void ApplyHitsBetween(Vector3 previous, Vector3 next)
        {
            frameTargets.Clear();
            Collider[] hits = Physics.OverlapCapsule(previous, next, radius);
            foreach (Collider hit in hits)
            {
                Health target = hit.GetComponentInParent<Health>();
                if (
                    target == null
                    || target == owner
                    || target.IsDead
                    || damagedTargets.Contains(target)
                    || !frameTargets.Add(target)
                )
                {
                    continue;
                }

                DamageContext context = new DamageContext(
                    damage,
                    owner != null ? owner.gameObject : gameObject,
                    DamageDeliveryType.CursedTechnique,
                    DamageTraits.None,
                    "CURSED TECHNIQUE REVERSAL: RED",
                    target.transform.position + Vector3.up * 0.8f
                );
                if (target.ReceiveDamage(context) != DamageResolution.Applied)
                {
                    continue;
                }

                damagedTargets.Add(target);
                ApplyHitReaction(target, direction * pushSpeed, hitStun);
                onTargetHit?.Invoke(target);

                if (!impactPlayed)
                {
                    impactPlayed = true;
                    onFirstImpact?.Invoke();
                    PresentationVfxRuntime.Spawn(
                        GojoRedPresentationPreset.CreateImpactRequest(
                            context.HitPoint,
                            radius,
                            direction
                        )
                    );
                }
            }
        }

        private static void ApplyHitReaction(Health target, Vector3 impulse, float stun)
        {
            MonoBehaviour[] behaviours = target.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IHitReactable hitReactable)
                {
                    hitReactable.ApplyHitReaction(impulse, stun);
                    break;
                }
            }
        }

        private void BuildVisual()
        {
            presentationHandle = PresentationVfxRuntime.Spawn(
                GojoRedPresentationPreset.CreateReleaseRequest(
                    transform,
                    radius,
                    maxRange,
                    speed,
                    direction
                )
            );
        }
    }

    /// <summary>
    /// Presentation-only Red request factory shared by the gameplay projectile and
    /// developer preview hosts. Gameplay owns radius and lifetime inputs; this type
    /// owns only the renderer-facing production tuning.
    /// </summary>
    public static class GojoRedPresentationPreset
    {
        private static readonly Color ReleasePrimary =
            new Color(0.84f, 0.015f, 0.025f, 0.94f);
        private static readonly Color ReleaseSecondary =
            new Color(1f, 0.26f, 0.04f, 0.76f);
        private static readonly Color ImpactPrimary =
            new Color(0.88f, 0.015f, 0.025f, 0.92f);
        private static readonly Color ImpactSecondary =
            new Color(1f, 0.30f, 0.04f, 0.74f);

        public static PresentationVfxSpawnRequest CreateReleaseRequest(
            Transform anchor,
            float radius,
            float range,
            float projectileSpeed,
            Vector3 direction
        )
        {
            float duration = range / Mathf.Max(0.1f, projectileSpeed) + 0.15f;
            return PresentationVfxSpawnRequest.Follow(
                anchor,
                Vector3.zero,
                ReleasePrimary,
                ReleaseSecondary,
                radius * 0.28f,
                radius * 1.8f,
                duration,
                0f,
                PresentationVfxTimePolicy.Scaled,
                PresentationVfxStyleId.GojoRed,
                direction
            );
        }

        public static PresentationVfxSpawnRequest CreateImpactRequest(
            Vector3 worldPosition,
            float radius,
            Vector3 direction
        )
        {
            return PresentationVfxSpawnRequest.AtWorld(
                worldPosition,
                ImpactPrimary,
                ImpactSecondary,
                radius * 0.20f,
                radius * 2.4f,
                0.26f,
                0f,
                PresentationVfxTimePolicy.Unscaled,
                PresentationVfxStyleId.GojoRed,
                direction
            );
        }
    }
}
