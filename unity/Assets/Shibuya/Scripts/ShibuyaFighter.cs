using System;
using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Shibuya
{
    public enum ShibuyaFighterState { Ready, Attacking, Hitstunned, Dead, Finished }

    // The only component allowed to move this fighter's CharacterController.
    [DisallowMultipleComponent, RequireComponent(typeof(CharacterController), typeof(Health))]
    public sealed class ShibuyaFighter : MonoBehaviour, IHitReactable
    {
        [SerializeField] private ShibuyaCombatProfile profile;
        [SerializeField] private CharacterStatsProfile stats;
        private CharacterController motor;
        private Health health;
        private Vector3 moveIntent;
        private bool runIntent;
        private bool jumpIntent;
        private float verticalSpeed;
        private Vector3 knockback;
        private float stunnedUntil;
        private float attackStartedAt;
        private bool attacking;
        private bool activeVisited;
        private bool stopped;
        private readonly HashSet<Health> hitTargets = new HashSet<Health>();

        public event Action<Vector3> HitConfirmed;
        public Health Health => health != null ? health : GetComponent<Health>();
        public ShibuyaCombatProfile Profile => profile;
        public ShibuyaFighterState State => Health.IsDead ? ShibuyaFighterState.Dead
            : stopped ? ShibuyaFighterState.Finished
            : Time.time < stunnedUntil ? ShibuyaFighterState.Hitstunned
            : attacking ? ShibuyaFighterState.Attacking : ShibuyaFighterState.Ready;
        public bool IsGrounded => motor != null && motor.isGrounded;
        public float PlanarSpeed => motor == null ? 0f : new Vector2(motor.velocity.x, motor.velocity.z).magnitude;
        public float AttackProgress => attacking ? Mathf.Clamp01((Time.time - attackStartedAt) / profile.AttackDuration) : 0f;
        public float HitFlashUntil { get; private set; }
        public int AcceptedAttackCount { get; private set; }
        public int ConfirmedHitCount { get; private set; }

        public void Configure(ShibuyaCombatProfile combatProfile, CharacterStatsProfile healthStats)
        {
            profile = combatProfile;
            stats = healthStats;
        }

        private void Awake()
        {
            motor = GetComponent<CharacterController>();
            health = GetComponent<Health>();
            if (profile == null || stats == null)
            {
                Debug.LogError("시부야 전투 설정 / 체력 프로필 연결이 필요합니다.", this);
                enabled = false;
                return;
            }
            health.ApplyProfile(stats);
        }

        public void SetIntent(Vector3 direction, bool run, bool jump)
        {
            direction.y = 0f;
            moveIntent = Vector3.ClampMagnitude(direction, 1f);
            runIntent = run;
            jumpIntent |= jump;
        }

        public bool TryAttack()
        {
            if (!enabled || profile == null || State != ShibuyaFighterState.Ready) return false;
            attacking = true;
            activeVisited = false;
            attackStartedAt = Time.time;
            hitTargets.Clear();
            AcceptedAttackCount++;
            return true;
        }

        private void Update()
        {
            if (State == ShibuyaFighterState.Dead || stopped)
            {
                moveIntent = Vector3.zero;
                knockback = Vector3.zero;
                jumpIntent = false;
                return;
            }
            bool stunned = State == ShibuyaFighterState.Hitstunned;
            if (stunned) attacking = false;
            else UpdateAttack();
            if (stopped || Health.IsDead) return;

            Vector3 voluntary = stunned ? Vector3.zero : moveIntent;
            if (!stunned && !attacking && voluntary.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(voluntary), Mathf.Max(0.1f, profile.turnSpeed) * Time.deltaTime);
            float speed = Mathf.Max(0.1f, runIntent ? profile.runSpeed : profile.walkSpeed);
            if (attacking) speed *= Mathf.Clamp01(profile.attackMoveMultiplier);
            if (motor.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
            if (!stunned && !attacking && jumpIntent && motor.isGrounded)
                verticalSpeed = Mathf.Sqrt(2f * Mathf.Max(0.1f, profile.gravity) * Mathf.Max(0.1f, profile.jumpHeight));
            jumpIntent = false;
            verticalSpeed -= Mathf.Max(0.1f, profile.gravity) * Time.deltaTime;
            Vector3 displacement = (voluntary * speed + knockback + Vector3.up * verticalSpeed) * Time.deltaTime;
            // Consume blocked displacement immediately; it must never accumulate behind walls.
            int steps = Mathf.Max(1, Mathf.CeilToInt(displacement.magnitude / Mathf.Max(0.05f, motor.radius * 0.5f)));
            for (int i = 0; i < steps; i++) motor.Move(displacement / steps);
            if ((motor.collisionFlags & CollisionFlags.Above) != 0 && verticalSpeed > 0f) verticalSpeed = 0f;
            knockback = Vector3.MoveTowards(knockback, Vector3.zero,
                Mathf.Max(0.1f, profile.knockbackDamping) * Time.deltaTime);
        }

        private void UpdateAttack()
        {
            if (!attacking) return;
            float elapsed = Time.time - attackStartedAt;
            float start = Mathf.Max(0.01f, profile.startup);
            float end = start + Mathf.Max(0.01f, profile.activeTime);
            // Visit the active phase once even if a slow frame crossed the whole window.
            if (elapsed >= start && (!activeVisited || elapsed <= end))
            {
                activeVisited = true;
                ResolveHits();
            }
            if (elapsed >= profile.AttackDuration) attacking = false;
        }

        private void ResolveHits()
        {
            Vector3 origin = transform.position + Vector3.up * profile.attackHeight;
            Vector3 center = origin + transform.forward * profile.attackForwardOffset;
            foreach (Collider collider in Physics.OverlapSphere(center, Mathf.Max(0.1f, profile.attackRadius),
                ~0, QueryTriggerInteraction.Ignore))
            {
                if (stopped || Health.IsDead) break;
                ShibuyaFighter other = collider.GetComponentInParent<ShibuyaFighter>();
                if (other == null || other == this || other.Health.IsDead || other.stopped || hitTargets.Contains(other.Health)) continue;
                Vector3 targetPoint = other.transform.position + Vector3.up * profile.attackHeight;
                Vector3 flat = other.transform.position - transform.position;
                flat.y = 0f;
                if (Vector3.Dot(transform.forward, flat.normalized) < 0.1f || WorldBlocks(origin, targetPoint)) continue;
                hitTargets.Add(other.Health);
                DamageContext context = new DamageContext(Mathf.Max(0.1f, profile.damage), gameObject,
                    DamageDeliveryType.PhysicalStrike, DamageTraits.None, "SHIBUYA STRIKE", targetPoint);
                if (other.Health.ReceiveDamage(context) != DamageResolution.Applied) continue;
                ConfirmedHitCount++;
                Vector3 direction = flat.sqrMagnitude > 0.001f ? flat.normalized : transform.forward;
                other.ApplyHitReaction(direction * Mathf.Max(0f, profile.knockbackSpeed), Mathf.Max(0f, profile.hitStun));
                HitConfirmed?.Invoke(targetPoint);
            }
        }

        private static bool WorldBlocks(Vector3 origin, Vector3 target)
        {
            Vector3 delta = target - origin;
            foreach (RaycastHit hit in Physics.RaycastAll(origin, delta.normalized, delta.magnitude,
                ~0, QueryTriggerInteraction.Ignore))
                if (hit.collider.GetComponentInParent<ShibuyaFighter>() == null) return true;
            return false;
        }

        public void ApplyHitReaction(Vector3 impulse, float stunDuration)
        {
            if (Health.IsDead || stopped) return;
            impulse.y = 0f;
            knockback = impulse;
            stunnedUntil = Mathf.Max(stunnedUntil, Time.time + Mathf.Max(0f, stunDuration));
            HitFlashUntil = Time.time + profile.hitFlashDuration;
            attacking = false;
            jumpIntent = false;
        }

        public void StopCombat()
        {
            stopped = true;
            attacking = false;
            moveIntent = knockback = Vector3.zero;
            jumpIntent = false;
        }

        private void OnDisable() { moveIntent = knockback = Vector3.zero; jumpIntent = false; attacking = false; }
    }
}
