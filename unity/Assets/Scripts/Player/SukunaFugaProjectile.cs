using System;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    public sealed class SukunaFugaProjectile : MonoBehaviour
    {
        public static event Action<Health, Vector3, bool> Exploded;

        private Health owner;
        private Health target;
        private Vector3 direction;
        private float speed;
        private float maxRange;
        private float collisionRadius;
        private float explosionRadius;
        private float damage;
        private float knockbackSpeed;
        private float hitStun;
        private float travelled;
        private float explodedAt;
        private bool exploded;
        private Action onExploded;
        private PresentationVfxHandle flightHandle;
        private PresentationVfxHandle impactHandle;

        public void Configure(
            Health newOwner,
            Health newTarget,
            Vector3 initialDirection,
            float newSpeed,
            float newMaxRange,
            float newCollisionRadius,
            float newExplosionRadius,
            float newDamage,
            float newKnockbackSpeed,
            float newHitStun,
            Action newOnExploded
        )
        {
            owner = newOwner;
            target = newTarget;
            direction = initialDirection;
            direction.y = 0f;
            direction = direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : Vector3.forward;
            speed = Mathf.Max(0.1f, newSpeed);
            maxRange = Mathf.Max(0.1f, newMaxRange);
            collisionRadius = Mathf.Max(0.1f, newCollisionRadius);
            explosionRadius = Mathf.Max(0.1f, newExplosionRadius);
            damage = Mathf.Max(0f, newDamage);
            knockbackSpeed = Mathf.Max(0f, newKnockbackSpeed);
            hitStun = Mathf.Max(0f, newHitStun);
            onExploded = newOnExploded;

            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            BuildProjectileVisual();
        }

        private void Update()
        {
            if (exploded)
            {
                if (Time.time - explodedAt >= 0.58f)
                {
                    Destroy(gameObject);
                }
                return;
            }

            UpdateDirectionTowardTarget();

            float step = speed * Time.deltaTime;
            Vector3 previous = transform.position;
            Vector3 next = previous + direction * step;
            transform.position = next;
            travelled += step;

            if (HasReachedTarget(previous, next) || travelled >= maxRange)
            {
                Explode();
            }
        }

        private void OnDestroy()
        {
            flightHandle.Stop(PresentationVfxStopMode.Immediate);
            impactHandle.Stop(PresentationVfxStopMode.Immediate);
        }

        private void UpdateDirectionTowardTarget()
        {
            if (target == null || target.IsDead)
            {
                return;
            }

            Vector3 desired = target.transform.position + Vector3.up * 0.75f - transform.position;
            if (desired.sqrMagnitude <= 0.001f)
            {
                return;
            }

            float maxRadians = 210f * Mathf.Deg2Rad * Time.deltaTime;
            direction = Vector3.RotateTowards(direction, desired.normalized, maxRadians, 0f).normalized;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        private bool HasReachedTarget(Vector3 segmentStart, Vector3 segmentEnd)
        {
            if (target == null || target.IsDead)
            {
                return false;
            }

            Vector3 targetPoint = target.transform.position + Vector3.up * 0.75f;
            Vector3 segment = segmentEnd - segmentStart;
            float denominator = segment.sqrMagnitude;
            float t = denominator > 0.0001f
                ? Mathf.Clamp01(Vector3.Dot(targetPoint - segmentStart, segment) / denominator)
                : 0f;
            Vector3 closest = segmentStart + segment * t;
            return Vector3.Distance(closest, targetPoint) <= collisionRadius + 0.55f;
        }

        private void Explode()
        {
            if (exploded)
            {
                return;
            }

            exploded = true;
            explodedAt = Time.time;
            onExploded?.Invoke();
            flightHandle.Stop(PresentationVfxStopMode.FadeOut);

            ApplySingleTargetDamage();

            bool domainAmplified = gameObject.name.Contains("Domain");
            impactHandle = PresentationVfxRuntime.Spawn(
                PresentationVfxSpawnRequest.AtWorld(
                    transform.position,
                    new Color(1f, 0.08f, 0.01f, 0.92f),
                    new Color(1f, 0.38f, 0.03f, 0.74f),
                    collisionRadius,
                    explosionRadius,
                    0.58f,
                    0f,
                    PresentationVfxTimePolicy.Scaled,
                    PresentationVfxStyleId.FugaImpact,
                    direction,
                    domainAmplified
                )
            );
            Exploded?.Invoke(owner, transform.position, domainAmplified);
        }

        private void ApplySingleTargetDamage()
        {
            if (target == null || target.IsDead)
            {
                return;
            }

            Vector3 targetPoint = target.transform.position + Vector3.up * 0.75f;
            if (Vector3.Distance(transform.position, targetPoint) > explosionRadius + 1.2f)
            {
                return;
            }

            DamageContext context = new DamageContext(
                damage,
                owner != null ? owner.gameObject : gameObject,
                DamageDeliveryType.CursedTechnique,
                DamageTraits.None,
                "푸가",
                targetPoint
            );
            if (target.ReceiveDamage(context) != DamageResolution.Applied)
            {
                return;
            }

            Vector3 pushDirection = target.transform.position - transform.position;
            pushDirection.y = 0f;
            if (pushDirection.sqrMagnitude <= 0.001f)
            {
                pushDirection = direction;
            }
            ApplyHitReaction(target, pushDirection.normalized * knockbackSpeed, hitStun);
        }

        private static void ApplyHitReaction(Health targetHealth, Vector3 impulse, float stun)
        {
            MonoBehaviour[] behaviours = targetHealth.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IHitReactable hitReactable)
                {
                    hitReactable.ApplyHitReaction(impulse, stun);
                    break;
                }
            }
        }

        private void BuildProjectileVisual()
        {
            bool domainAmplified = gameObject.name.Contains("Domain");
            flightHandle = PresentationVfxRuntime.Spawn(
                PresentationVfxSpawnRequest.Follow(
                    transform,
                    Vector3.zero,
                    new Color(1f, 0.10f, 0.01f, 0.94f),
                    new Color(1f, 0.34f, 0.025f, 0.78f),
                    collisionRadius,
                    collisionRadius * 2.1f,
                    maxRange / speed + 0.3f,
                    0f,
                    PresentationVfxTimePolicy.Scaled,
                    PresentationVfxStyleId.FugaRelease,
                    direction,
                    domainAmplified
                )
            );
        }
    }
}
