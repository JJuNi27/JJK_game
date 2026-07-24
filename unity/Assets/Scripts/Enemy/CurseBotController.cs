using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Enemy
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Health))]
    public sealed class CurseBotController : MonoBehaviour, IDomainStunnable
    {
        private enum BotState
        {
            Idle,
            Chase,
            Attack,
            Frozen,
            Dead,
        }

        [SerializeField] private Transform target;
        [SerializeField, Min(0.1f)] private float moveSpeed = 3.2f;
        [SerializeField, Min(0.1f)] private float rotationSpeed = 10f;
        [SerializeField, Min(0.1f)] private float attackRange = 1.7f;
        [SerializeField, Min(0.1f)] private float attackDamage = 12f;
        [SerializeField, Min(0.05f)] private float attackCooldown = 0.9f;
        [SerializeField] private float gravity = -24f;

        private CharacterController controller;
        private Health health;
        private BotState state = BotState.Idle;
        private float frozenUntil;
        private float nextAttackAt;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            health = GetComponent<Health>();
            health.Died += HandleDeath;
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
            if (state == BotState.Dead)
            {
                return;
            }

            if (Time.time < frozenUntil)
            {
                state = BotState.Frozen;
                ApplyGravityOnly();
                return;
            }

            if (target == null)
            {
                state = BotState.Idle;
                ApplyGravityOnly();
                return;
            }

            Health targetHealth = target.GetComponentInParent<Health>();
            if (targetHealth != null && targetHealth.IsDead)
            {
                state = BotState.Idle;
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
                TryAttack(targetHealth);
                ApplyGravityOnly();
            }
        }

        public void ApplyDomainStun(float duration)
        {
            if (state == BotState.Dead || duration <= 0f)
            {
                return;
            }

            frozenUntil = Mathf.Max(frozenUntil, Time.time + duration);
            state = BotState.Frozen;
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

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;
            Vector3 velocity = direction * moveSpeed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }

        private void TryAttack(Health targetHealth)
        {
            if (targetHealth == null || Time.time < nextAttackAt)
            {
                return;
            }

            nextAttackAt = Time.time + attackCooldown;
            targetHealth.TakeDamage(attackDamage);
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
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;
            controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
        }

        private void HandleDeath(Health _)
        {
            state = BotState.Dead;
            enabled = false;
        }
    }
}
