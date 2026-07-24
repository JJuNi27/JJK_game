using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Health))]
    public sealed class ThirdPersonPlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float moveSpeed = 5.5f;
        [SerializeField, Min(0.1f)] private float rotationSpeed = 14f;
        [SerializeField] private float gravity = -24f;
        [SerializeField] private Transform cameraTransform;

        [Header("Dodge")]
        [SerializeField, Min(0.1f)] private float dodgeSpeed = 12f;
        [SerializeField, Min(0.05f)] private float dodgeDuration = 0.24f;
        [SerializeField, Min(0.05f)] private float dodgeCooldown = 0.75f;
        [SerializeField, Min(0.05f)] private float dodgeInvulnerabilityDuration = 0.30f;

        private CharacterController controller;
        private Health health;
        private float verticalVelocity;
        private float dodgeEndsAt;
        private float nextDodgeAt;
        private Vector3 dodgeDirection;

        public bool IsDodging => Time.time < dodgeEndsAt;
        public bool DodgeReady => Time.time >= nextDodgeAt && !IsDodging;
        public float DodgeCooldownRemaining => Mathf.Max(0f, nextDodgeAt - Time.time);

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            health = GetComponent<Health>();

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            Vector2 rawInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );
            rawInput = Vector2.ClampMagnitude(rawInput, 1f);
            Vector3 moveDirection = BuildCameraRelativeDirection(rawInput);

            if (!IsDodging && Input.GetKeyDown(KeyCode.Space) && DodgeReady)
            {
                StartDodge(moveDirection);
            }

            if (IsDodging)
            {
                ApplyDodgeMovement();
                return;
            }

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            ApplyGroundingAndGravity();
            Vector3 velocity = moveDirection * moveSpeed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }

        private void StartDodge(Vector3 requestedDirection)
        {
            dodgeDirection = requestedDirection.sqrMagnitude > 0.001f
                ? requestedDirection.normalized
                : transform.forward;
            dodgeDirection.y = 0f;
            dodgeDirection.Normalize();

            transform.rotation = Quaternion.LookRotation(dodgeDirection, Vector3.up);
            dodgeEndsAt = Time.time + dodgeDuration;
            nextDodgeAt = Time.time + dodgeCooldown;
            health.GrantInvulnerability(dodgeInvulnerabilityDuration);
        }

        private void ApplyDodgeMovement()
        {
            ApplyGroundingAndGravity();
            Vector3 velocity = dodgeDirection * dodgeSpeed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }

        private void ApplyGroundingAndGravity()
        {
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;
        }

        private Vector3 BuildCameraRelativeDirection(Vector2 input)
        {
            if (cameraTransform == null)
            {
                return new Vector3(input.x, 0f, input.y);
            }

            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            return (forward * input.y + right * input.x).normalized;
        }
    }
}
