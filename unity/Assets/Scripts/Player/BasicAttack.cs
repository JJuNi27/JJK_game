using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    public sealed class BasicAttack : MonoBehaviour
    {
        [SerializeField] private Transform attackOrigin;
        [SerializeField, Min(0.1f)] private float attackRadius = 1.6f;
        [SerializeField, Min(0.1f)] private float comboResetDelay = 0.9f;
        [SerializeField, Min(0.1f)] private float comboDisplayDuration = 0.75f;
        [SerializeField] private GojoDomainController domainController;

        [Header("Combo Damage")]
        [SerializeField, Min(0.1f)] private float firstHitDamage = 12f;
        [SerializeField, Min(0.1f)] private float secondHitDamage = 14f;
        [SerializeField, Min(0.1f)] private float thirdHitDamage = 24f;

        [Header("Combo Timing")]
        [SerializeField, Min(0.05f)] private float firstHitCooldown = 0.24f;
        [SerializeField, Min(0.05f)] private float secondHitCooldown = 0.28f;
        [SerializeField, Min(0.05f)] private float thirdHitCooldown = 0.52f;

        [Header("Combo Hit Reaction")]
        [SerializeField, Min(0f)] private float firstHitKnockback = 4.5f;
        [SerializeField, Min(0f)] private float secondHitKnockback = 6f;
        [SerializeField, Min(0f)] private float thirdHitKnockback = 11f;
        [SerializeField, Min(0f)] private float firstHitStun = 0.12f;
        [SerializeField, Min(0f)] private float secondHitStun = 0.17f;
        [SerializeField, Min(0f)] private float thirdHitStun = 0.38f;

        private Health ownHealth;
        private float nextAttackAt;
        private float comboExpiresAt;
        private float comboDisplayUntil;
        private int nextComboIndex;
        private int lastPerformedStep;

        public int DisplayComboStep => Time.time <= comboDisplayUntil ? lastPerformedStep : 0;
        public string ComboLabel => DisplayComboStep switch
        {
            1 => "COMBO 1 / 3",
            2 => "COMBO 2 / 3",
            3 => "COMBO 3 / 3 · FINISH",
            _ => string.Empty,
        };

        public void Configure(Transform newAttackOrigin, GojoDomainController newDomainController)
        {
            attackOrigin = newAttackOrigin;
            domainController = newDomainController;
        }

        private void Awake()
        {
            ownHealth = GetComponent<Health>();

            if (attackOrigin == null)
            {
                attackOrigin = transform;
            }

            if (domainController == null)
            {
                domainController = GetComponent<GojoDomainController>();
            }
        }

        private void Update()
        {
            if (nextComboIndex != 0 && Time.time > comboExpiresAt)
            {
                ResetCombo();
            }

            if (!Input.GetMouseButtonDown(0) || Time.time < nextAttackAt)
            {
                return;
            }

            if (domainController != null && domainController.CapturesMouseInput)
            {
                return;
            }

            PerformComboStep(nextComboIndex);
        }

        private void PerformComboStep(int comboIndex)
        {
            float damage = GetComboValue(
                comboIndex,
                firstHitDamage,
                secondHitDamage,
                thirdHitDamage
            );
            float cooldown = GetComboValue(
                comboIndex,
                firstHitCooldown,
                secondHitCooldown,
                thirdHitCooldown
            );
            float knockback = GetComboValue(
                comboIndex,
                firstHitKnockback,
                secondHitKnockback,
                thirdHitKnockback
            );
            float hitStun = GetComboValue(
                comboIndex,
                firstHitStun,
                secondHitStun,
                thirdHitStun
            );

            nextAttackAt = Time.time + cooldown;
            lastPerformedStep = comboIndex + 1;
            comboDisplayUntil = Time.time + comboDisplayDuration;
            PerformAttack(damage, knockback, hitStun);

            if (comboIndex >= 2)
            {
                nextComboIndex = 0;
                comboExpiresAt = 0f;
            }
            else
            {
                nextComboIndex = comboIndex + 1;
                comboExpiresAt = Time.time + comboResetDelay;
            }
        }

        private void PerformAttack(float damage, float knockbackSpeed, float hitStunDuration)
        {
            Collider[] hits = Physics.OverlapSphere(attackOrigin.position, attackRadius);
            HashSet<Health> damagedTargets = new HashSet<Health>();

            foreach (Collider hit in hits)
            {
                Health targetHealth = hit.GetComponentInParent<Health>();
                if (
                    targetHealth == null
                    || targetHealth == ownHealth
                    || targetHealth.IsDead
                    || !damagedTargets.Add(targetHealth)
                )
                {
                    continue;
                }

                targetHealth.TakeDamage(damage);
                ApplyHitReaction(targetHealth, knockbackSpeed, hitStunDuration);
            }
        }

        private void ApplyHitReaction(
            Health targetHealth,
            float knockbackSpeed,
            float hitStunDuration
        )
        {
            Vector3 direction = targetHealth.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = transform.forward;
            }

            Vector3 impulse = direction.normalized * knockbackSpeed;
            MonoBehaviour[] behaviours = targetHealth.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IHitReactable hitReactable)
                {
                    hitReactable.ApplyHitReaction(impulse, hitStunDuration);
                    break;
                }
            }
        }

        private void ResetCombo()
        {
            nextComboIndex = 0;
            comboExpiresAt = 0f;
        }

        private static float GetComboValue(
            int comboIndex,
            float first,
            float second,
            float third
        )
        {
            return comboIndex switch
            {
                1 => second,
                2 => third,
                _ => first,
            };
        }

        private void OnDrawGizmosSelected()
        {
            Transform origin = attackOrigin != null ? attackOrigin : transform;
            Gizmos.DrawWireSphere(origin.position, attackRadius);
        }
    }
}
