using System;
using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    public sealed class BlueConvergenceField : MonoBehaviour
    {
        private readonly HashSet<Health> damagedTargets = new HashSet<Health>();
        private readonly HashSet<Health> pulseTargets = new HashSet<Health>();

        private Health owner;
        private float radius;
        private float duration;
        private float pulseInterval;
        private float damage;
        private float pullSpeed;
        private float hitStun;
        private float startedAt;
        private float nextPulseAt;
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
            damage = Mathf.Max(0f, newDamage);
            pullSpeed = Mathf.Max(0f, newPullSpeed);
            hitStun = Mathf.Max(0f, newHitStun);
            onTargetHit = newOnTargetHit;
            onFirstImpact = newOnFirstImpact;
            startedAt = Time.time;
            nextPulseAt = Time.time;

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

            if (Time.time >= nextPulseAt)
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

                bool firstSuccessfulHit = !damagedTargets.Contains(target);
                if (firstSuccessfulHit)
                {
                    DamageContext context = new DamageContext(
                        damage,
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

                    damagedTargets.Add(target);
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
