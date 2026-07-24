using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Enemy
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Health))]
    public sealed class CurseBotController : MonoBehaviour, IDomainStunnable, IHitReactable
    {
        private enum BotState
        {
            Idle,
            Chase,
            Attack,
            AttackWindup,
            Hitstunned,
            Frozen,
            Dead,
        }

        [Header("Movement")]
        [SerializeField] private Transform target;
        [SerializeField, Min(0.1f)] private float moveSpeed = 3.2f;
        [SerializeField, Min(0.1f)] private float rotationSpeed = 10f;
        [SerializeField] private float gravity = -24f;

        [Header("Attack")]
        [SerializeField, Min(0.1f)] private float attackRange = 1.7f;
        [SerializeField, Min(0.1f)] private float attackDamage = 12f;
        [SerializeField, Min(0.05f)] private float attackCooldown = 0.9f;
        [SerializeField, Min(0.05f)] private float attackWindupDuration = 0.55f;
        [SerializeField, Min(0f)] private float attackReachBuffer = 0.35f;
        [SerializeField] private Color attackTelegraphColor = new Color(1f, 0.45f, 0.08f);

        [Header("Hit Reaction")]
        [SerializeField, Min(0.1f)] private float knockbackDamping = 18f;
        [SerializeField, Min(0.01f)] private float hitFlashDuration = 0.10f;
        [SerializeField] private Color hitFlashColor = Color.white;

        private CharacterController controller;
        private Health health;
        private Renderer bodyRenderer;
        private Material bodyMaterial;
        private Color baseColor;
        private BotState state = BotState.Idle;
        private float frozenUntil;
        private float hitStunUntil;
        private float flashUntil;
        private float nextAttackAt;
        private float attackWindupStartedAt;
        private float attackWindupEndsAt;
        private float verticalVelocity;
        private Vector3 knockbackVelocity;

        public bool IsAttackTelegraphing => state == BotState.AttackWindup;
        public float AttackWindupProgress => IsAttackTelegraphing
            ? Mathf.Clamp01(
                (Time.time - attackWindupStartedAt)
                    / Mathf.Max(0.01f, attackWindupDuration)
            )
            : 0f;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            health = GetComponent<Health>();
            health.Died += HandleDeath;

            bodyRenderer = GetComponentInChildren<Renderer>();
            if (bodyRenderer != null)
            {
                bodyMaterial = bodyRenderer.material;
                baseColor = ReadMaterialColor(bodyMaterial);
            }
        }

        private void Start()
        {
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= HandleDeath;
            }
        }

        private void Update()
        {
            UpdateBodyColor();

            if (state == BotState.Dead)
            {
                return;
            }

            if (Time.time < frozenUntil)
            {
                CancelAttackWindup();
                state = BotState.Frozen;
                knockbackVelocity = Vector3.zero;
                ApplyGravityOnly();
                return;
            }

            if (Time.time < hitStunUntil || knockbackVelocity.sqrMagnitude > 0.04f)
            {
                CancelAttackWindup();
                state = BotState.Hitstunned;
                ApplyHitReactionMovement();
                return;
            }

            if (target == null)
            {
                CancelAttackWindup();
                state = BotState.Idle;
                ApplyGravityOnly();
                return;
            }

            Health targetHealth = target.GetComponentInParent<Health>();
            if (targetHealth != null && targetHealth.IsDead)
            {
                CancelAttackWindup();
                state = BotState.Idle;
                ApplyGravityOnly();
                return;
            }

            if (state == BotState.AttackWindup)
            {
                UpdateAttackWindup(targetHealth);
                ApplyGravityOnly();
                return;
            }

            Vector3 flatOffset = target.position - transform.position;
            flatOffset.y = 0f;
            float distance = flatOffset.magnitude;

            if (distance > attackRange)
            {
                state = BotState.Chase;
                Chase(flatOffset);
            }
            else
            {
                state = BotState.Attack;
                FaceTarget(flatOffset);
                TryBeginAttack(targetHealth);
                ApplyGravityOnly();
            }
        }

        public void ApplyDomainStun(float duration)
        {
            if (state == BotState.Dead || duration <= 0f)
            {
                return;
            }

            CancelAttackWindup();
            frozenUntil = Mathf.Max(frozenUntil, Time.time + duration);
            knockbackVelocity = Vector3.zero;
            hitStunUntil = 0f;
            state = BotState.Frozen;
        }

        public void ApplyHitReaction(Vector3 impulse, float stunDuration)
        {
            if (state == BotState.Dead)
            {
                return;
            }

            CancelAttackWindup();
            impulse.y = 0f;
            knockbackVelocity = impulse;
            hitStunUntil = Mathf.Max(hitStunUntil, Time.time + Mathf.Max(0f, stunDuration));
            flashUntil = Time.time + hitFlashDuration;
            state = BotState.Hitstunned;
        }

        private void Chase(Vector3 flatOffset)
        {
            if (flatOffset.sqrMagnitude <= 0.001f)
            {
                ApplyGravityOnly();
                return;
            }

            Vector3 direction = flatOffset.normalized;
            FaceTarget(direction);

            ApplyGroundingAndGravity();
            Vector3 velocity = direction * moveSpeed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }

        private void ApplyHitReactionMovement()
        {
            ApplyGroundingAndGravity();
            Vector3 velocity = knockbackVelocity;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
            knockbackVelocity = Vector3.MoveTowards(
                knockbackVelocity,
                Vector3.zero,
                knockbackDamping * Time.deltaTime
            );
        }

        private void TryBeginAttack(Health targetHealth)
        {
            if (targetHealth == null || Time.time < nextAttackAt)
            {
                return;
            }

            state = BotState.AttackWindup;
            attackWindupStartedAt = Time.time;
            attackWindupEndsAt = Time.time + attackWindupDuration;
        }

        private void UpdateAttackWindup(Health targetHealth)
        {
            Vector3 flatOffset = target.position - transform.position;
            flatOffset.y = 0f;
            FaceTarget(flatOffset);

            if (Time.time < attackWindupEndsAt)
            {
                return;
            }

            float distance = flatOffset.magnitude;
            if (
                targetHealth != null
                && !targetHealth.IsDead
                && distance <= attackRange + attackReachBuffer
            )
            {
                targetHealth.TakeDamage(attackDamage);
            }

            nextAttackAt = Time.time + attackCooldown;
            CancelAttackWindup();
            state = BotState.Attack;
        }

        private void CancelAttackWindup()
        {
            attackWindupStartedAt = 0f;
            attackWindupEndsAt = 0f;
        }

        private void FaceTarget(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        private void ApplyGravityOnly()
        {
            ApplyGroundingAndGravity();
            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        }

        private void ApplyGroundingAndGravity()
        {
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;
        }

        private void UpdateBodyColor()
        {
            if (bodyMaterial == null)
            {
                return;
            }

            Color color = baseColor;
            if (Time.time < flashUntil)
            {
                color = hitFlashColor;
            }
            else if (IsAttackTelegraphing)
            {
                float pulse = 0.55f + Mathf.Sin(Time.time * 30f) * 0.2f;
                color = Color.Lerp(baseColor, attackTelegraphColor, pulse);
            }

            WriteMaterialColor(bodyMaterial, color);
        }

        private static Color ReadMaterialColor(Material material)
        {
            if (material.HasProperty("_BaseColor"))
            {
                return material.GetColor("_BaseColor");
            }

            return material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
        }

        private static void WriteMaterialColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private void HandleDeath(Health _)
        {
            CancelAttackWindup();
            state = BotState.Dead;
            knockbackVelocity = Vector3.zero;
            if (bodyMaterial != null)
            {
                WriteMaterialColor(bodyMaterial, baseColor * 0.35f);
            }
            enabled = false;
        }
    }
}
