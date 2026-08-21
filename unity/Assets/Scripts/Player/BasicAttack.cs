using System.Collections.Generic;
using JJKGame.CameraSystem;
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
        [SerializeField, Min(0.1f)] private float hitComboResetDelay = 1.05f;
        [SerializeField] private GojoDomainController domainController;

        [Header("Attack Chain Damage")]
        [SerializeField, Min(0.1f)] private float firstHitDamage = 12f;
        [SerializeField, Min(0.1f)] private float secondHitDamage = 14f;
        [SerializeField, Min(0.1f)] private float thirdHitDamage = 24f;

        [Header("Attack Chain Timing")]
        [SerializeField, Min(0.05f)] private float firstHitCooldown = 0.24f;
        [SerializeField, Min(0.05f)] private float secondHitCooldown = 0.28f;
        [SerializeField, Min(0.05f)] private float thirdHitCooldown = 0.52f;

        [Header("Attack Chain Hit Reaction")]
        [SerializeField, Min(0f)] private float firstHitKnockback = 4.5f;
        [SerializeField, Min(0f)] private float secondHitKnockback = 6f;
        [SerializeField, Min(0f)] private float thirdHitKnockback = 11f;
        [SerializeField, Min(0f)] private float firstHitStun = 0.12f;
        [SerializeField, Min(0f)] private float secondHitStun = 0.17f;
        [SerializeField, Min(0f)] private float thirdHitStun = 0.38f;

        [Header("Beauty Corner · Basic Hit Camera Feedback")]
        [SerializeField, Min(0f)] private float firstHitShake = 0.075f;
        [SerializeField, Min(0f)] private float secondHitShake = 0.11f;
        [SerializeField, Min(0f)] private float thirdHitShake = 0.22f;
        [SerializeField, Min(0.01f)] private float firstHitShakeDuration = 0.07f;
        [SerializeField, Min(0.01f)] private float secondHitShakeDuration = 0.085f;
        [SerializeField, Min(0.01f)] private float thirdHitShakeDuration = 0.13f;

        [Header("Beauty Corner · Basic Hit Stop / Flash")]
        [SerializeField, Min(0f)] private float firstHitStopDuration = 0.022f;
        [SerializeField, Min(0f)] private float secondHitStopDuration = 0.030f;
        [SerializeField, Min(0f)] private float thirdHitStopDuration = 0.055f;
        [SerializeField, Range(0.01f, 1f)] private float hitStopRelativeTimeScale = 0.08f;
        [SerializeField, Range(0f, 0.25f)] private float firstHitFlashAlpha = 0.025f;
        [SerializeField, Range(0f, 0.25f)] private float secondHitFlashAlpha = 0.040f;
        [SerializeField, Range(0f, 0.25f)] private float thirdHitFlashAlpha = 0.075f;
        [SerializeField, Min(0.01f)] private float firstHitFlashDuration = 0.055f;
        [SerializeField, Min(0.01f)] private float secondHitFlashDuration = 0.070f;
        [SerializeField, Min(0.01f)] private float thirdHitFlashDuration = 0.095f;

        private Health ownHealth;
        private TargetLockController targetLock;
        private PrototypeCombatAudio combatAudio;
        private CombatActionGate actionGate;
        private SimpleCameraFollow combatCamera;
        private float nextAttackAt;
        private float chainExpiresAt;
        private float chainDisplayUntil;
        private float hitComboExpiresAt;
        private int nextChainIndex;
        private int lastPerformedStep;
        private int hitComboCount;

        public int DisplayChainStep => Time.time <= chainDisplayUntil ? lastPerformedStep : 0;
        public int DisplayHitComboCount =>
            hitComboCount >= 2 && Time.time <= hitComboExpiresAt ? hitComboCount : 0;

        public string ChainLabel => DisplayChainStep switch
        {
            1 => "ATTACK CHAIN 1 / 3",
            2 => "ATTACK CHAIN 2 / 3",
            3 => "ATTACK CHAIN 3 / 3 · FINISH",
            _ => string.Empty,
        };

        public string HitComboLabel =>
            DisplayHitComboCount > 0 ? $"HIT COMBO × {DisplayHitComboCount}" : string.Empty;

        public void Configure(Transform newAttackOrigin, GojoDomainController newDomainController)
        {
            attackOrigin = newAttackOrigin;
            domainController = newDomainController;
        }

        public void ResetCombatSequence()
        {
            nextAttackAt = 0f;
            nextChainIndex = 0;
            chainExpiresAt = 0f;
            chainDisplayUntil = 0f;
            lastPerformedStep = 0;
            hitComboCount = 0;
            hitComboExpiresAt = 0f;
        }

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
            targetLock = GetComponent<TargetLockController>();
            combatAudio = PrototypeCombatAudio.GetOrCreate(gameObject);
            actionGate = CombatActionGate.GetOrCreate(gameObject);
            combatCamera = FindFirstObjectByType<SimpleCameraFollow>();

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
            if (nextChainIndex != 0 && Time.time > chainExpiresAt)
            {
                ResetAttackChain();
            }

            if (hitComboCount > 0 && Time.time > hitComboExpiresAt)
            {
                ResetHitCombo();
            }

            if (!Input.GetMouseButtonDown(0) || Time.time < nextAttackAt)
            {
                return;
            }

            if (domainController != null && domainController.CapturesMouseInput)
            {
                return;
            }

            actionGate ??= CombatActionGate.GetOrCreate(gameObject);
            if (actionGate != null && !actionGate.CanStartBasicAttack)
            {
                return;
            }

            if (targetLock == null)
            {
                targetLock = GetComponent<TargetLockController>();
            }

            targetLock?.FaceTargetInstant();
            PerformAttackChainStep(nextChainIndex);
        }

        private void PerformAttackChainStep(int chainIndex)
        {
            float damage = GetChainValue(chainIndex, firstHitDamage, secondHitDamage, thirdHitDamage);
            float cooldown = GetChainValue(chainIndex, firstHitCooldown, secondHitCooldown, thirdHitCooldown);
            float knockback = GetChainValue(chainIndex, firstHitKnockback, secondHitKnockback, thirdHitKnockback);
            float hitStun = GetChainValue(chainIndex, firstHitStun, secondHitStun, thirdHitStun);

            nextAttackAt = Time.time + cooldown;
            lastPerformedStep = chainIndex + 1;
            chainDisplayUntil = Time.time + comboDisplayDuration;
            combatAudio ??= PrototypeCombatAudio.GetOrCreate(gameObject);
            combatAudio?.PlayBasicSwing(lastPerformedStep);

            bool hitAnyTarget = PerformAttack(damage, knockback, hitStun);
            if (hitAnyTarget)
            {
                RegisterSuccessfulHit();
                combatAudio?.PlayBasicHit(lastPerformedStep);
                PlayBasicHitFeedback(chainIndex);
            }
            else
            {
                ResetHitCombo();
            }

            if (chainIndex >= 2)
            {
                nextChainIndex = 0;
                chainExpiresAt = 0f;
            }
            else
            {
                nextChainIndex = chainIndex + 1;
                chainExpiresAt = Time.time + comboResetDelay;
            }
        }

        private bool PerformAttack(float damage, float knockbackSpeed, float hitStunDuration)
        {
            Collider[] hits = Physics.OverlapSphere(attackOrigin.position, attackRadius);
            HashSet<Health> damagedTargets = new HashSet<Health>();
            bool hitAnyTarget = false;

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

                DamageContext context = new DamageContext(
                    damage,
                    gameObject,
                    DamageDeliveryType.PhysicalStrike,
                    DamageTraits.None,
                    $"BASIC ATTACK {lastPerformedStep}",
                    targetHealth.transform.position + Vector3.up * 0.8f
                );
                if (targetHealth.ReceiveDamage(context) != DamageResolution.Applied)
                {
                    continue;
                }

                PrototypeHitImpactVfx.Spawn(context.ImpactPoint, lastPerformedStep);
                hitAnyTarget = true;
                ApplyHitReaction(targetHealth, knockbackSpeed, hitStunDuration);
            }

            return hitAnyTarget;
        }

        private void PlayBasicHitFeedback(int chainIndex)
        {
            combatCamera ??= FindFirstObjectByType<SimpleCameraFollow>();
            if (combatCamera != null)
            {
                float shakeAmplitude = GetChainValue(
                    chainIndex,
                    firstHitShake,
                    secondHitShake,
                    thirdHitShake
                );
                float shakeDuration = GetChainValue(
                    chainIndex,
                    firstHitShakeDuration,
                    secondHitShakeDuration,
                    thirdHitShakeDuration
                );
                combatCamera.AddShake(shakeAmplitude, shakeDuration);

                float flashAlpha = GetChainValue(
                    chainIndex,
                    firstHitFlashAlpha,
                    secondHitFlashAlpha,
                    thirdHitFlashAlpha
                );
                float flashDuration = GetChainValue(
                    chainIndex,
                    firstHitFlashDuration,
                    secondHitFlashDuration,
                    thirdHitFlashDuration
                );
                Color flashColor = chainIndex >= 2
                    ? new Color(1f, 0.82f, 0.52f)
                    : Color.white;
                combatCamera.Flash(flashColor, flashAlpha, flashDuration);
            }

            float hitStopDuration = GetChainValue(
                chainIndex,
                firstHitStopDuration,
                secondHitStopDuration,
                thirdHitStopDuration
            );
            PrototypeHitStopController.Request(hitStopDuration, hitStopRelativeTimeScale);
        }

        private void RegisterSuccessfulHit()
        {
            if (Time.time > hitComboExpiresAt)
            {
                hitComboCount = 0;
            }

            hitComboCount += 1;
            hitComboExpiresAt = Time.time + hitComboResetDelay;
        }

        private void ApplyHitReaction(Health targetHealth, float knockbackSpeed, float hitStunDuration)
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

        private void ResetAttackChain()
        {
            nextChainIndex = 0;
            chainExpiresAt = 0f;
        }

        private void ResetHitCombo()
        {
            hitComboCount = 0;
            hitComboExpiresAt = 0f;
        }

        private static float GetChainValue(int chainIndex, float first, float second, float third)
        {
            return chainIndex switch
            {
                1 => second,
                2 => third,
                _ => first,
            };
        }
    }
}
