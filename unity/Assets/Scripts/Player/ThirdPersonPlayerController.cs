using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Health))]
    public sealed class ThirdPersonPlayerController : MonoBehaviour
    {
        [Header("이동")]
        [SerializeField, InspectorName("기본 이동 속도"), Min(0.1f), Tooltip("캐릭터 이동 프로필이 없을 때 사용하는 기본 이동 속도입니다.")] private float moveSpeed = 5.5f;
        [SerializeField, InspectorName("회전 속도"), Min(0.1f), Tooltip("이동 방향으로 회전하는 속도입니다.")] private float rotationSpeed = 14f;
        [SerializeField, InspectorName("중력"), Tooltip("수직 이동에 적용하는 중력 가속도입니다.")] private float gravity = -24f;
        [SerializeField, InspectorName("카메라 기준점"), Tooltip("카메라 기준 이동 방향을 계산할 Transform입니다.")] private Transform cameraTransform;
        [SerializeField, InspectorName("술식 시전 이동 배율"), Range(0f, 1f), Tooltip("술식 시전 중 적용하는 이동 속도 배율입니다.")] private float techniqueCastMoveMultiplier = 0.35f;

        [Header("회피 기본값")]
        [SerializeField, InspectorName("회피 속도"), Min(0.1f), Tooltip("프로필이 없을 때 사용하는 회피 이동 속도입니다.")] private float dodgeSpeed = 12f;
        [SerializeField, InspectorName("회피 이동 시간"), Min(0.05f), Tooltip("프로필이 없을 때 사용하는 회피 이동 시간입니다.")] private float dodgeDuration = 0.24f;
        [SerializeField, InspectorName("회피 재사용 대기시간"), Min(0.05f), Tooltip("프로필이 없을 때 사용하는 회피 재사용 대기시간입니다.")] private float dodgeCooldown = 0.75f;
        [SerializeField, InspectorName("회피 무적 시간"), Min(0.05f), Tooltip("프로필이 없을 때 사용하는 회피 무적 시간입니다.")] private float dodgeInvulnerabilityDuration = 0.30f;

        private CharacterController controller;
        private Health health;
        private GojoTechniqueController gojoTechnique;
        private SukunaTechniqueController sukunaTechnique;
        private SukunaDomainController sukunaDomain;
        private CombatActionGate actionGate;
        private float verticalVelocity;
        private readonly EvadeMotion evadeMotion = new EvadeMotion();
        private CharacterMovementProfile movementProfile;
        private EvadeProfile activeEvade;
        private float invulnerabilityStartsAt;
        private float invulnerabilityEndsAt;
        private bool pendingInvulnerability;
        private bool recoveryCueRaised;
        private bool completionCueRaised = true;
        private float dodgeStartedAt;
        private float dodgeEndsAt;
        private float nextDodgeAt;
        private Vector3 dodgeDirection;

        // Movement integration may consume its final interval in this frame. Keep the
        // existing wall-clock action lock until the full duration since accepted input.
        public bool IsDodging => Time.time < dodgeEndsAt || evadeMotion.IsActive;
        public bool DodgeReady =>
            Time.time >= nextDodgeAt
            && !IsDodging
            && (actionGate == null || actionGate.CanStartDodge);
        public float DodgeCooldownRemaining => Mathf.Max(0f, nextDodgeAt - Time.time);
        public float DodgeProgress => IsDodging
            ? Mathf.Clamp01((Time.time - dodgeStartedAt) / Mathf.Max(0.01f, dodgeEndsAt - dodgeStartedAt))
            : 0f;
        public Vector3 DodgeDirection => dodgeDirection;

        public void ConfigureMovement(CharacterMovementProfile profile)
        {
            CancelDodge();
            movementProfile = profile;
            // Preserve the owner cooldown across character swaps.
        }

        private bool TechniqueCasting
        {
            get
            {
                gojoTechnique ??= GetComponent<GojoTechniqueController>();
                sukunaTechnique ??= GetComponent<SukunaTechniqueController>();
                sukunaDomain ??= GetComponent<SukunaDomainController>();
                return (
                    gojoTechnique != null
                    && gojoTechnique.enabled
                    && gojoTechnique.IsCasting
                ) || (
                    sukunaTechnique != null
                    && sukunaTechnique.enabled
                    && sukunaTechnique.IsCasting
                ) || (
                    sukunaDomain != null
                    && sukunaDomain.enabled
                    && sukunaDomain.IsCasting
                );
            }
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            health = GetComponent<Health>();
            gojoTechnique = GetComponent<GojoTechniqueController>();
            sukunaTechnique = GetComponent<SukunaTechniqueController>();
            sukunaDomain = GetComponent<SukunaDomainController>();
            actionGate = CombatActionGate.GetOrCreate(gameObject);

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            if (health == null || health.IsDead)
            {
                CancelDodge();
                return;
            }
            UpdateDodgeInvulnerability();
            if (!IsDodging && !completionCueRaised)
            {
                completionCueRaised = true;
                RaiseEvadeCue(EvadePresentationPhase.Completed);
            }
            Vector2 rawInput = ProductionCombatInput.Move;
            rawInput = Vector2.ClampMagnitude(rawInput, 1f);
            Vector3 moveDirection = BuildCameraRelativeDirection(rawInput);

            actionGate ??= CombatActionGate.GetOrCreate(gameObject);
            if (
                !IsDodging
                && ProductionCombatInput.DodgePressed
                && DodgeReady
                && (actionGate == null || actionGate.CanStartDodge)
            )
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
            float locomotionSpeed = movementProfile == null ? moveSpeed
                : Mathf.Max(0.1f, ProductionCombatInput.RunHeld
                    ? movementProfile.runSpeed : movementProfile.walkSpeed);
            float currentMoveSpeed = TechniqueCasting
                ? locomotionSpeed * techniqueCastMoveMultiplier
                : locomotionSpeed;
            Vector3 velocity = moveDirection * currentMoveSpeed;
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
            activeEvade = movementProfile?.evade ?? new EvadeProfile
            {
                burstSpeed = dodgeSpeed,
                movementDuration = dodgeDuration,
                cooldown = dodgeCooldown,
                invulnerabilityDuration = dodgeInvulnerabilityDuration,
            };
            evadeMotion.Begin(activeEvade);
            recoveryCueRaised = false;
            completionCueRaised = false;
            dodgeStartedAt = Time.time;
            dodgeEndsAt = Time.time + activeEvade.ActionDuration;
            nextDodgeAt = Time.time + Mathf.Max(0f, activeEvade.cooldown);
            invulnerabilityStartsAt = Time.time + Mathf.Clamp(activeEvade.invulnerabilityStart, 0f, activeEvade.ActionDuration);
            invulnerabilityEndsAt = invulnerabilityStartsAt + Mathf.Max(0f, activeEvade.invulnerabilityDuration);
            pendingInvulnerability = activeEvade.invulnerabilityDuration > 0f;
            UpdateDodgeInvulnerability();
            RaiseEvadeCue(EvadePresentationPhase.Started);
            CombatAudioEvents.Raise(
                CombatAudioEvent.ForOwner(health, CombatAudioEventId.Dodge)
            );
        }

        private void ApplyDodgeMovement()
        {
            ApplyGroundingAndGravity();
            float displacement = evadeMotion.Advance(Time.deltaTime);
            EvadeMotion.Move(controller, dodgeDirection, displacement, verticalVelocity * Time.deltaTime);
            if (!recoveryCueRaised && (evadeMotion.IsRecovering || !evadeMotion.IsActive))
            {
                recoveryCueRaised = true;
                RaiseEvadeCue(EvadePresentationPhase.Recovery);
            }
        }

        private void UpdateDodgeInvulnerability()
        {
            if (!pendingInvulnerability || Time.time < invulnerabilityStartsAt) return;
            pendingInvulnerability = false;
            float remaining = invulnerabilityEndsAt - Time.time;
            if (remaining > 0f) health.GrantInvulnerability(remaining);
        }

        private void RaiseEvadeCue(EvadePresentationPhase phase)
        {
            EvadePresentationCues.Raise(new EvadePresentationCue(transform, dodgeDirection, activeEvade, phase));
        }

        private void CancelDodge()
        {
            bool wasActive = IsDodging;
            evadeMotion.Cancel();
            dodgeEndsAt = 0f;
            pendingInvulnerability = false;
            completionCueRaised = true;
            if (wasActive) RaiseEvadeCue(EvadePresentationPhase.Cancelled);
        }

        private void OnDisable() => CancelDodge();

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
