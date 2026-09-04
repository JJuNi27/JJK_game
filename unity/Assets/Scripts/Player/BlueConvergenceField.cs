using System;
using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    internal static class GojoBluePulseSchedule
    {
        public const int HitCount = 4;

        public static float GetNormalizedTime(int pulseIndex)
        {
            return pulseIndex switch
            {
                0 => 0.18f,
                1 => 0.38f,
                2 => 0.58f,
                3 => 0.78f,
                _ => 1f,
            };
        }
    }

    public sealed class BlueConvergenceField : MonoBehaviour
    {
        private readonly Dictionary<Health, int> targetHitCounts =
            new Dictionary<Health, int>();
        private readonly HashSet<Health> pulseTargets = new HashSet<Health>();

        private Health owner;
        private float radius;
        private float duration;
        private float pulseInterval;
        private float damagePerHit;
        private float pullSpeed;
        private float hitStun;
        private float startedAt;
        private float nextPulseAt;
        private int nextDamagePulseIndex;
        private Action<Health> onTargetHit;
        private Action onFirstImpact;
        private bool impactPlayed;
        private PresentationVfxHandle presentationHandle;

        public void Configure(
            Health newOwner,
            float newRadius,
            float newDuration,
            float newPulseInterval,
            float newDamage,
            float newPullSpeed,
            float newHitStun,
            Action<Health> newOnTargetHit,
            Action newOnFirstImpact
        )
        {
            owner = newOwner;
            radius = Mathf.Max(0.1f, newRadius);
            duration = Mathf.Max(0.1f, newDuration);
            pulseInterval = Mathf.Max(0.03f, newPulseInterval);
            damagePerHit = Mathf.Max(0f, newDamage) / GojoBluePulseSchedule.HitCount;
            pullSpeed = Mathf.Max(0f, newPullSpeed);
            hitStun = Mathf.Max(0f, newHitStun);
            onTargetHit = newOnTargetHit;
            onFirstImpact = newOnFirstImpact;
            startedAt = Time.time;
            nextPulseAt = Time.time;
            nextDamagePulseIndex = 0;

            BuildVisual();
            ApplyPulse();
        }

        private void Update()
        {
            float elapsed = Time.time - startedAt;
            if (elapsed >= duration)
            {
                Destroy(gameObject);
                return;
            }

            if (Time.time >= nextPulseAt || IsDamagePulseDue(Time.time))
            {
                ApplyPulse();
            }

        }

        private void OnDestroy()
        {
            presentationHandle.Stop(PresentationVfxStopMode.Immediate);
        }

        private void ApplyPulse()
        {
            nextPulseAt = Time.time + pulseInterval;
            bool damagePulse = IsDamagePulseDue(Time.time);
            if (damagePulse)
            {
                nextDamagePulseIndex++;
            }
            pulseTargets.Clear();

            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider hit in hits)
            {
                Health target = hit.GetComponentInParent<Health>();
                if (
                    target == null
                    || target == owner
                    || target.IsDead
                    || !pulseTargets.Add(target)
                )
                {
                    continue;
                }

                targetHitCounts.TryGetValue(target, out int previousHitCount);
                bool firstSuccessfulHit = false;
                if (
                    damagePulse
                    && previousHitCount < GojoBluePulseSchedule.HitCount
                )
                {
                    DamageContext context = new DamageContext(
                        damagePerHit,
                        owner != null ? owner.gameObject : gameObject,
                        DamageDeliveryType.CursedTechnique,
                        DamageTraits.None,
                        "CURSED TECHNIQUE LAPSE: BLUE",
                        target.transform.position + Vector3.up * 0.8f
                    );
                    if (target.ReceiveDamage(context) != DamageResolution.Applied)
                    {
                        continue;
                    }

                    targetHitCounts[target] = previousHitCount + 1;
                    firstSuccessfulHit = previousHitCount == 0;
                    onTargetHit?.Invoke(target);
                    if (!impactPlayed)
                    {
                        impactPlayed = true;
                        onFirstImpact?.Invoke();
                        PresentationVfxRuntime.Spawn(
                            GojoBluePresentationPreset.CreateImpactRequest(
                                context.HitPoint,
                                radius
                            )
                        );
                    }
                }

                Vector3 direction = transform.position - target.transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude <= 0.001f)
                {
                    direction = owner != null
                        ? owner.transform.position - target.transform.position
                        : -target.transform.forward;
                    direction.y = 0f;
                }

                if (direction.sqrMagnitude <= 0.001f)
                {
                    direction = -target.transform.forward;
                }

                float stun = firstSuccessfulHit ? hitStun : Mathf.Min(0.08f, hitStun);
                ApplyHitReaction(target, direction.normalized * pullSpeed, stun);
            }
        }

        private bool IsDamagePulseDue(float currentTime)
        {
            if (nextDamagePulseIndex >= GojoBluePulseSchedule.HitCount)
            {
                return false;
            }

            float normalized = Mathf.Clamp01((currentTime - startedAt) / duration);
            return normalized
                >= GojoBluePulseSchedule.GetNormalizedTime(nextDamagePulseIndex);
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
                GojoBluePresentationPreset.CreateFieldRequest(
                    transform,
                    radius,
                    duration
                )
            );
        }
    }
}
