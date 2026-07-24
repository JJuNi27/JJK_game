using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    public sealed class BasicAttack : MonoBehaviour
    {
        [SerializeField] private Transform attackOrigin;
        [SerializeField, Min(0.1f)] private float attackRadius = 1.6f;
        [SerializeField, Min(0.1f)] private float attackDamage = 18f;
        [SerializeField, Min(0.05f)] private float attackCooldown = 0.45f;
        [SerializeField, Min(0f)] private float knockbackSpeed = 7.5f;
        [SerializeField, Min(0f)] private float hitStunDuration = 0.22f;
        [SerializeField] private GojoDomainController domainController;

        private Health ownHealth;
        private float nextAttackAt;

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
            if (!Input.GetMouseButtonDown(0) || Time.time < nextAttackAt)
            {
                return;
            }

            if (domainController != null && domainController.CapturesMouseInput)
            {
                return;
            }

            nextAttackAt = Time.time + attackCooldown;
            PerformAttack();
        }

        private void PerformAttack()
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

                targetHealth.TakeDamage(attackDamage);
                ApplyHitReaction(targetHealth);
            }
        }

        private void ApplyHitReaction(Health targetHealth)
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

        private void OnDrawGizmosSelected()
        {
            Transform origin = attackOrigin != null ? attackOrigin : transform;
            Gizmos.DrawWireSphere(origin.position, attackRadius);
        }
    }
}
