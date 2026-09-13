using System.Collections.Generic;
using JJKGame.Core;
using JJKGame.Player;
using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Dev.VFXLab
{
    public enum VfxLabPreviewMotion
    {
        Idle,
        BasicAttack1,
        BasicAttack2,
        BasicAttackFinisher,
        Dodge,
        TechniqueAnticipation,
        TechniqueCast,
        TechniqueRelease,
        TechniqueRecover,
        DomainAnticipation,
        DomainRelease,
    }

    /// <summary>
    /// Developer-only movement and animation preview adapter. It deliberately has
    /// no Health, CE, cooldown, hitbox, or combat-controller dependency.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VfxLabPreviewCharacter : MonoBehaviour
    {
        private const float RotationSpeed = 14f;
        private const float Gravity = -24f;

        [Header("캐릭터 프리뷰 프로필")]
        [SerializeField, InspectorName("캐릭터 프리뷰 프로필"), Tooltip("비워 두면 현재 고죠 프리뷰 연결과 결과를 그대로 유지합니다.")]
        private VfxLabCharacterProfile characterProfile;

        [Header("이동")]
        [SerializeField, InspectorName("이동 프로필"), Tooltip("걷기, 달리기, 회피 동작의 이동 설정입니다.")]
        private CharacterMovementProfile movementProfile = new CharacterMovementProfile();
        private readonly EvadeMotion evadeMotion = new EvadeMotion();
        private bool recoveryCueRaised;

        private readonly List<Material> runtimeMaterials = new List<Material>(8);
        private readonly Dictionary<int, AnimatorControllerParameterType> animatorParameters =
            new Dictionary<int, AnimatorControllerParameterType>();
        private Animator cachedAnimator;
        private RuntimeAnimatorController cachedAnimatorController;

        [Header("제작 캐릭터 연결")]
        [SerializeField, InspectorName("캐릭터 모델 루트"), Tooltip("제작된 캐릭터 모델의 최상위 트랜스폼입니다.")]
        private Transform authoredModelRoot;
        [SerializeField, InspectorName("애니메이터"), Tooltip("캐릭터 프리뷰 애니메이션을 재생할 애니메이터입니다.")]
        private Animator animator;

        [Header("선택적 애니메이터 파라미터")]
        [SerializeField, InspectorName("평면 이동 속도 파라미터"), Tooltip("평면 이동 속도를 전달할 애니메이터 실수형 파라미터 이름입니다.")]
        private string planarSpeedParameter = "PlanarSpeed";
        [SerializeField, InspectorName("대기 트리거"), Tooltip("대기 동작을 재생할 애니메이터 트리거 파라미터 이름입니다.")]
        private string idleTrigger = "Idle";
        [SerializeField, InspectorName("평타 1타 트리거"), Tooltip("평타 1타를 재생할 애니메이터 트리거 파라미터 이름입니다.")]
        private string basicAttack1Trigger = "BasicAttack1";
        [SerializeField, InspectorName("평타 2타 트리거"), Tooltip("평타 2타를 재생할 애니메이터 트리거 파라미터 이름입니다.")]
        private string basicAttack2Trigger = "BasicAttack2";
        [SerializeField, InspectorName("평타 마무리 트리거"), Tooltip("평타 마무리 동작을 재생할 애니메이터 트리거 파라미터 이름입니다.")]
        private string basicAttackFinisherTrigger = "BasicAttackFinisher";
        [SerializeField, InspectorName("회피 트리거"), Tooltip("회피 동작을 재생할 애니메이터 트리거 파라미터 이름입니다.")]
        private string dodgeTrigger = "Dodge";
        [SerializeField, InspectorName("기술 준비 동작 트리거"), Tooltip("기술 준비 동작을 재생할 애니메이터 트리거 파라미터 이름입니다.")]
        private string anticipationTrigger = "TechniqueAnticipation";
        [SerializeField, InspectorName("기술 시전 트리거"), Tooltip("기술 시전 동작을 재생할 애니메이터 트리거 파라미터 이름입니다.")]
        private string castTrigger = "TechniqueCast";
        [SerializeField, InspectorName("기술 방출 트리거"), Tooltip("기술 방출 동작을 재생할 애니메이터 트리거 파라미터 이름입니다.")]
        private string releaseTrigger = "TechniqueRelease";
        [SerializeField, InspectorName("기술 회복 트리거"), Tooltip("기술 후딜레이 동작을 재생할 애니메이터 트리거 파라미터 이름입니다.")]
        private string recoverTrigger = "TechniqueRecover";
        [SerializeField, InspectorName("영역 준비 동작 트리거"), Tooltip("영역 준비 동작을 재생할 애니메이터 트리거 파라미터 이름입니다.")]
        private string domainAnticipationTrigger = "DomainAnticipation";
        [SerializeField, InspectorName("영역 방출 트리거"), Tooltip("영역 방출 동작을 재생할 애니메이터 트리거 파라미터 이름입니다.")]
        private string domainReleaseTrigger = "DomainRelease";

        private CharacterController motor;
        private Transform cameraTransform;
        private Transform visualRoot;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftLeg;
        private Transform rightLeg;
        private float verticalVelocity;
        private float planarSpeed;
        private float motionStartedAt;
        private VfxLabPreviewMotion previewMotion;
        private bool usesAuthoredModel;
        private bool usesAuthoredAnimator;
        private bool hasTechniqueAnchor;
        private Vector3 techniqueAnchor;
        private Vector3 dodgeDirection;

        public float PlanarSpeed => planarSpeed;
        public bool IsEvading => evadeMotion.IsActive;
        public bool IsEvadeRecovering => evadeMotion.IsRecovering;
        public bool UsesAuthoredModel => usesAuthoredModel;
        public bool UsesAuthoredAnimator => usesAuthoredAnimator;
        public PrototypeCharacterId CharacterId => characterProfile != null
            ? characterProfile.CharacterId
            : PrototypeCharacterId.GojoModern;
        public CharacterPresentationProfile PresentationProfile => characterProfile != null
            ? characterProfile.Presentation
            : CharacterPresentationProfiles.Get(PrototypeCharacterId.GojoModern);
        public CombatAudioProfile AudioProfile => characterProfile != null
            ? characterProfile.CombatAudio
            : null;
        private CharacterMovementProfile ActiveMovementProfile =>
            characterProfile != null && characterProfile.Movement != null
                ? characterProfile.Movement
                : movementProfile;
        public string AnimationSourceLabel => usesAuthoredAnimator
            ? "AUTHORED MODEL + ANIMATOR"
            : usesAuthoredModel
                ? "AUTHORED MODEL · NO ANIMATOR CONTROLLER"
                : "PROTOTYPE PROCEDURAL MOTION";

        public void Configure(Transform newCameraTransform)
        {
            cameraTransform = newCameraTransform;
        }

        public void ConfigureCharacterProfile(VfxLabCharacterProfile profile)
        {
            characterProfile = profile;
            ApplyProfileAnimatorController();
        }

        private void Awake()
        {
            movementProfile ??= new CharacterMovementProfile();
            movementProfile.evade ??= EvadeProfile.CreateGojo();
            motor = GetComponent<CharacterController>();
            if (motor == null)
            {
                motor = gameObject.AddComponent<CharacterController>();
            }
            motor.height = 2f;
            motor.radius = 0.42f;
            motor.center = Vector3.up;
            motor.stepOffset = 0.28f;

            authoredModelRoot ??= transform.Find("AuthoredModelRoot");
            RefreshAnimatorBinding();
            ApplyProfileAnimatorController();
            usesAuthoredModel = HasAuthoredVisual();
            if (!usesAuthoredModel)
            {
                BuildFallbackGojo();
            }
            else
            {
                Transform fallback = transform.Find("PreviewGojoFallback");
                if (fallback != null)
                {
                    fallback.gameObject.SetActive(false);
                }
            }
        }

        private void Update()
        {
            RefreshAnimatorBinding();
            ApplyMovement();
            if (usesAuthoredAnimator)
            {
                // Use the existing relaxed Idle until an authored evade clip is available.
                bool relaxedEvade = previewMotion == VfxLabPreviewMotion.Dodge
                    && ActiveMovementProfile.evade.styleId == "gojo-blue-burst";
                SetAnimatorFloat(planarSpeedParameter, relaxedEvade ? 0f : planarSpeed);
            }
            else if (!usesAuthoredModel)
            {
                ApplyProceduralMotion();
            }
        }

        public void SetPreviewMotion(VfxLabPreviewMotion motion)
        {
            if (previewMotion == motion)
            {
                return;
            }

            previewMotion = motion;
            motionStartedAt = Time.time;
            if (motion != VfxLabPreviewMotion.Dodge && evadeMotion.IsActive)
            {
                evadeMotion.Cancel();
                RaiseEvadeCue(EvadePresentationPhase.Cancelled);
            }
            if (motion == VfxLabPreviewMotion.Idle)
            {
                hasTechniqueAnchor = false;
            }
            if (motion == VfxLabPreviewMotion.Dodge)
            {
                CaptureDodgeDirection();
                evadeMotion.Begin(ActiveMovementProfile.evade);
                recoveryCueRaised = false;
                RaiseEvadeCue(EvadePresentationPhase.Started);
            }
            if (!usesAuthoredAnimator)
            {
                return;
            }

            string trigger = motion switch
            {
                VfxLabPreviewMotion.Idle => idleTrigger,
                VfxLabPreviewMotion.BasicAttack1 => basicAttack1Trigger,
                VfxLabPreviewMotion.BasicAttack2 => basicAttack2Trigger,
                VfxLabPreviewMotion.BasicAttackFinisher => basicAttackFinisherTrigger,
                VfxLabPreviewMotion.Dodge => string.IsNullOrEmpty(ActiveMovementProfile.evade.animationTrigger)
                    ? dodgeTrigger : ActiveMovementProfile.evade.animationTrigger,
                VfxLabPreviewMotion.TechniqueAnticipation => anticipationTrigger,
                VfxLabPreviewMotion.TechniqueCast => castTrigger,
                VfxLabPreviewMotion.TechniqueRelease => releaseTrigger,
                VfxLabPreviewMotion.TechniqueRecover => recoverTrigger,
                VfxLabPreviewMotion.DomainAnticipation => domainAnticipationTrigger,
                VfxLabPreviewMotion.DomainRelease => domainReleaseTrigger,
                _ => string.Empty,
            };
            SetAnimatorTrigger(trigger);
        }

        public void SetTechniqueAnchor(Vector3 worldPoint)
        {
            techniqueAnchor = worldPoint;
            hasTechniqueAnchor = true;
            FaceTechniqueAnchor(1f);
        }

        private void ApplyMovement()
        {
            if (motor == null)
            {
                return;
            }

            Vector2 rawInput = ProductionCombatInput.Move;
            rawInput = Vector2.ClampMagnitude(rawInput, 1f);
            Vector3 direction = BuildCameraRelativeDirection(rawInput);
            float speed = Mathf.Max(0.1f, ProductionCombatInput.RunHeld
                ? ActiveMovementProfile.runSpeed : ActiveMovementProfile.walkSpeed);
            bool evading = evadeMotion.IsActive;
            float evadeDisplacement = 0f;
            if (evading)
            {
                direction = dodgeDirection;
                evadeDisplacement = evadeMotion.Advance(Time.deltaTime);
                speed = Time.deltaTime > 0f ? evadeDisplacement / Time.deltaTime : 0f;
            }

            if (hasTechniqueAnchor && IsTechniqueOrDomainMotion())
            {
                FaceTechniqueAnchor(1f - Mathf.Exp(-RotationSpeed * Time.deltaTime));
            }
            else if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    1f - Mathf.Exp(-RotationSpeed * Time.deltaTime)
                );
            }

            if (motor.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
            verticalVelocity += Gravity * Time.deltaTime;
            Vector3 velocity = direction * speed;
            velocity.y = verticalVelocity;
            Vector3 before = transform.position;
            if (evading)
                EvadeMotion.Move(motor, direction, evadeDisplacement, verticalVelocity * Time.deltaTime);
            else
                motor.Move(velocity * Time.deltaTime);
            Vector3 actualMovement = transform.position - before;
            actualMovement.y = 0f;
            planarSpeed = Time.deltaTime > 0f ? actualMovement.magnitude / Time.deltaTime : 0f;
            if (evading)
            {
                if (!recoveryCueRaised && (evadeMotion.IsRecovering || !evadeMotion.IsActive))
                {
                    recoveryCueRaised = true;
                    RaiseEvadeCue(EvadePresentationPhase.Recovery);
                }
                if (!evadeMotion.IsActive) RaiseEvadeCue(EvadePresentationPhase.Completed);
            }
        }

        private void RaiseEvadeCue(EvadePresentationPhase phase)
        {
            EvadePresentationCues.Raise(new EvadePresentationCue(
                transform, dodgeDirection, ActiveMovementProfile.evade, phase));
        }

        private void OnDisable()
        {
            if (!evadeMotion.IsActive) return;
            evadeMotion.Cancel();
            RaiseEvadeCue(EvadePresentationPhase.Cancelled);
        }

        private bool IsTechniqueOrDomainMotion()
        {
            return previewMotion == VfxLabPreviewMotion.TechniqueAnticipation
                || previewMotion == VfxLabPreviewMotion.TechniqueCast
                || previewMotion == VfxLabPreviewMotion.TechniqueRelease
                || previewMotion == VfxLabPreviewMotion.TechniqueRecover
                || previewMotion == VfxLabPreviewMotion.DomainAnticipation
                || previewMotion == VfxLabPreviewMotion.DomainRelease;
        }

        private void CaptureDodgeDirection()
        {
            Vector2 input = Vector2.ClampMagnitude(ProductionCombatInput.Move, 1f);
            dodgeDirection = BuildCameraRelativeDirection(input);
            if (dodgeDirection.sqrMagnitude <= 0.001f)
            {
                dodgeDirection = transform.forward;
                dodgeDirection.y = 0f;
            }
            dodgeDirection = dodgeDirection.sqrMagnitude > 0.001f
                ? dodgeDirection.normalized
                : Vector3.forward;
            transform.rotation = Quaternion.LookRotation(dodgeDirection, Vector3.up);
        }

        private void FaceTechniqueAnchor(float weight)
        {
            Vector3 direction = techniqueAnchor - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(
                direction.normalized,
                Vector3.up
            );
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Mathf.Clamp01(weight)
            );
        }

        private Vector3 BuildCameraRelativeDirection(Vector2 input)
        {
            Transform source = cameraTransform != null
                ? cameraTransform
                : Camera.main != null ? Camera.main.transform : null;
            if (source == null)
            {
                return new Vector3(input.x, 0f, input.y);
            }

            Vector3 forward = source.forward;
            Vector3 right = source.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            Vector3 direction = forward * input.y + right * input.x;
            return direction.sqrMagnitude > 1f ? direction.normalized : direction;
        }

        private void ApplyProceduralMotion()
        {
            if (visualRoot == null)
            {
                return;
            }

            bool relaxedEvade = previewMotion == VfxLabPreviewMotion.Dodge
                && ActiveMovementProfile.evade.styleId == "gojo-blue-burst";
            float movementWeight = relaxedEvade ? 0f
                : Mathf.Clamp01(planarSpeed / Mathf.Max(0.1f, ActiveMovementProfile.runSpeed));
            float walkPhase = Time.time * 8f;
            float armSwing = Mathf.Sin(walkPhase) * 24f * movementWeight;
            float legSwing = -armSwing * 0.75f;
            float bob = Mathf.Sin(walkPhase * 2f) * 0.025f * movementWeight;
            float breathe = Mathf.Sin(Time.time * 2.8f) * 1.2f;

            visualRoot.localPosition = Vector3.up * (1.02f + bob);
            visualRoot.localRotation = Quaternion.Euler(1.5f + movementWeight * 3f, breathe, 0f);
            SetLocalRotation(leftArm, new Vector3(armSwing, 0f, 0f));
            SetLocalRotation(rightArm, new Vector3(-armSwing, 0f, 0f));
            SetLocalRotation(leftLeg, new Vector3(legSwing, 0f, 0f));
            SetLocalRotation(rightLeg, new Vector3(-legSwing, 0f, 0f));

            float elapsed = Time.time - motionStartedAt;
            float enterWeight = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / 0.12f));
            switch (previewMotion)
            {
                case VfxLabPreviewMotion.BasicAttack1:
                    ApplyTechniquePose(
                        new Vector3(6f, -14f, -3f),
                        new Vector3(-28f, -8f, 24f),
                        new Vector3(-96f, 4f, -12f),
                        enterWeight
                    );
                    break;
                case VfxLabPreviewMotion.BasicAttack2:
                    ApplyTechniquePose(
                        new Vector3(5f, 16f, 3f),
                        new Vector3(-102f, -4f, 12f),
                        new Vector3(-24f, 10f, -26f),
                        enterWeight
                    );
                    break;
                case VfxLabPreviewMotion.BasicAttackFinisher:
                    ApplyTechniquePose(
                        new Vector3(12f, 0f, 0f),
                        new Vector3(-112f, -12f, 18f),
                        new Vector3(-112f, 12f, -18f),
                        enterWeight
                    );
                    break;
                case VfxLabPreviewMotion.Dodge:
                    if (relaxedEvade) break;
                    ApplyTechniquePose(
                        new Vector3(18f, 0f, -8f),
                        new Vector3(28f, -6f, 18f),
                        new Vector3(28f, 6f, -18f),
                        enterWeight
                    );
                    break;
                case VfxLabPreviewMotion.TechniqueAnticipation:
                    ApplyTechniquePose(
                        new Vector3(5f, -8f, 0f),
                        new Vector3(-54f, -16f, 30f),
                        new Vector3(-76f, 18f, -22f),
                        enterWeight
                    );
                    break;
                case VfxLabPreviewMotion.TechniqueCast:
                    ApplyTechniquePose(
                        new Vector3(7f, 0f, 0f),
                        new Vector3(-80f, -10f, 22f),
                        new Vector3(-102f, 8f, -12f),
                        enterWeight
                    );
                    break;
                case VfxLabPreviewMotion.TechniqueRelease:
                    ApplyTechniquePose(
                        new Vector3(-4f, 10f, 0f),
                        new Vector3(-34f, -20f, 28f),
                        new Vector3(-118f, 2f, -8f),
                        enterWeight
                    );
                    break;
                case VfxLabPreviewMotion.TechniqueRecover:
                    float recoverWeight = 1f - Mathf.SmoothStep(
                        0f,
                        1f,
                        Mathf.Clamp01(elapsed / 0.30f)
                    );
                    ApplyTechniquePose(
                        new Vector3(-4f, 10f, 0f),
                        new Vector3(-34f, -20f, 28f),
                        new Vector3(-118f, 2f, -8f),
                        recoverWeight
                    );
                    break;
                case VfxLabPreviewMotion.DomainAnticipation:
                    ApplyTechniquePose(
                        new Vector3(0f, -5f, 0f),
                        new Vector3(-86f, -24f, 30f),
                        new Vector3(-86f, 24f, -30f),
                        enterWeight
                    );
                    break;
                case VfxLabPreviewMotion.DomainRelease:
                    ApplyTechniquePose(
                        new Vector3(-3f, 0f, 0f),
                        new Vector3(-118f, -8f, 12f),
                        new Vector3(-118f, 8f, -12f),
                        enterWeight
                    );
                    break;
            }
        }

        private void ApplyTechniquePose(
            Vector3 rootEuler,
            Vector3 leftEuler,
            Vector3 rightEuler,
            float weight
        )
        {
            visualRoot.localRotation = Quaternion.Slerp(
                visualRoot.localRotation,
                Quaternion.Euler(rootEuler),
                weight
            );
            if (leftArm != null)
            {
                leftArm.localRotation = Quaternion.Slerp(
                    leftArm.localRotation,
                    Quaternion.Euler(leftEuler),
                    weight
                );
            }
            if (rightArm != null)
            {
                rightArm.localRotation = Quaternion.Slerp(
                    rightArm.localRotation,
                    Quaternion.Euler(rightEuler),
                    weight
                );
            }
        }

        private bool HasAuthoredVisual()
        {
            if (authoredModelRoot == null)
            {
                return animator != null;
            }
            return authoredModelRoot.GetComponentInChildren<Renderer>(true) != null
                || animator != null;
        }

        private void BuildFallbackGojo()
        {
            Transform existing = transform.Find("PreviewGojoFallback");
            if (existing != null)
            {
                visualRoot = existing;
                leftArm = visualRoot.Find("LeftArm");
                rightArm = visualRoot.Find("RightArm");
                leftLeg = visualRoot.Find("LeftLeg");
                rightLeg = visualRoot.Find("RightLeg");
                return;
            }

            visualRoot = new GameObject("PreviewGojoFallback").transform;
            visualRoot.SetParent(transform, false);
            visualRoot.localPosition = Vector3.up * 1.02f;

            Material uniform = CreateMaterial(new Color(0.018f, 0.028f, 0.060f, 1f));
            Material highlight = CreateMaterial(new Color(0.045f, 0.075f, 0.15f, 1f));
            Material skin = CreateMaterial(new Color(0.93f, 0.75f, 0.64f, 1f));
            Material hair = CreateMaterial(new Color(0.86f, 0.93f, 1f, 1f));
            Material black = CreateMaterial(new Color(0.003f, 0.006f, 0.015f, 1f));

            CreatePart("Torso", PrimitiveType.Cube, visualRoot, new Vector3(0f, 0.05f, 0f), new Vector3(0.58f, 0.72f, 0.34f), uniform);
            CreatePart("HighCollar", PrimitiveType.Cube, visualRoot, new Vector3(0f, 0.47f, 0.01f), new Vector3(0.48f, 0.22f, 0.32f), highlight);
            leftArm = CreateLimb("LeftArm", new Vector3(-0.42f, 0.08f, 0f), uniform, skin);
            rightArm = CreateLimb("RightArm", new Vector3(0.42f, 0.08f, 0f), uniform, skin);
            leftLeg = CreateLeg("LeftLeg", new Vector3(-0.17f, -0.54f, 0f), uniform, black);
            rightLeg = CreateLeg("RightLeg", new Vector3(0.17f, -0.54f, 0f), uniform, black);

            CreatePart("Neck", PrimitiveType.Cylinder, visualRoot, new Vector3(0f, 0.54f, 0f), new Vector3(0.16f, 0.12f, 0.16f), skin);
            Transform head = CreatePart("Head", PrimitiveType.Sphere, visualRoot, new Vector3(0f, 0.80f, 0f), new Vector3(0.43f, 0.50f, 0.40f), skin);
            CreatePart("HairCap", PrimitiveType.Sphere, head, new Vector3(0f, 0.20f, -0.01f), new Vector3(0.45f, 0.30f, 0.41f), hair);
            CreatePart("Blindfold", PrimitiveType.Cube, head, new Vector3(0f, 0.04f, 0.36f), new Vector3(0.52f, 0.16f, 0.08f), black);

            for (int index = 0; index < 5; index++)
            {
                float x = (index - 2) * 0.12f;
                Transform tuft = CreatePart(
                    $"HairTuft_{index}",
                    PrimitiveType.Capsule,
                    head,
                    new Vector3(x, 0.34f + (2 - Mathf.Abs(index - 2)) * 0.035f, -0.01f),
                    new Vector3(0.13f, 0.26f, 0.13f),
                    hair
                );
                tuft.localRotation = Quaternion.Euler(0f, 0f, (2 - index) * 11f);
            }
        }

        private Transform CreateLimb(
            string name,
            Vector3 position,
            Material uniform,
            Material skin
        )
        {
            Transform pivot = new GameObject(name).transform;
            pivot.SetParent(visualRoot, false);
            pivot.localPosition = position;
            CreatePart("Sleeve", PrimitiveType.Capsule, pivot, new Vector3(0f, -0.20f, 0f), new Vector3(0.20f, 0.48f, 0.20f), uniform);
            CreatePart("Hand", PrimitiveType.Sphere, pivot, new Vector3(0f, -0.48f, 0f), Vector3.one * 0.17f, skin);
            return pivot;
        }

        private Transform CreateLeg(
            string name,
            Vector3 position,
            Material uniform,
            Material shoe
        )
        {
            Transform pivot = new GameObject(name).transform;
            pivot.SetParent(visualRoot, false);
            pivot.localPosition = position;
            CreatePart("Trouser", PrimitiveType.Capsule, pivot, new Vector3(0f, -0.17f, 0f), new Vector3(0.23f, 0.52f, 0.23f), uniform);
            CreatePart("Shoe", PrimitiveType.Cube, pivot, new Vector3(0f, -0.48f, 0.07f), new Vector3(0.23f, 0.14f, 0.36f), shoe);
            return pivot;
        }

        private static Transform CreatePart(
            string name,
            PrimitiveType primitive,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material
        )
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
            return part.transform;
        }

        private Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            shader ??= Shader.Find("Standard");
            shader ??= Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }
            Material material = new Material(shader) { color = color };
            runtimeMaterials.Add(material);
            return material;
        }

        private void ApplyProfileAnimatorController()
        {
            if (characterProfile == null || characterProfile.AnimatorController == null)
            {
                return;
            }

            RefreshAnimatorBinding();
            if (animator != null && animator.runtimeAnimatorController != characterProfile.AnimatorController)
            {
                animator.runtimeAnimatorController = characterProfile.AnimatorController;
                RefreshAnimatorBinding();
            }
        }

        private void RefreshAnimatorBinding()
        {
            Transform animatorRoot = authoredModelRoot != null
                ? authoredModelRoot
                : transform;
            bool animatorBelongsToPreview = animator != null
                && (animator.transform == animatorRoot
                    || animator.transform.IsChildOf(animatorRoot));
            if (!animatorBelongsToPreview)
            {
                animator = animatorRoot.GetComponentInChildren<Animator>(true);
            }

            bool animatorReady =
                animator != null &&
                animator.runtimeAnimatorController != null;

            RuntimeAnimatorController currentController = animatorReady ? animator.runtimeAnimatorController : null;
            if (animatorReady == usesAuthoredAnimator && cachedAnimator == animator
                && cachedAnimatorController == currentController)
            {
                return;
            }

            usesAuthoredAnimator = animatorReady;
            cachedAnimator = animator;
            cachedAnimatorController = currentController;
            if (animatorReady) animator.applyRootMotion = false;
            CacheAnimatorParameters();
        }

        private void CacheAnimatorParameters()
        {
            animatorParameters.Clear();
            if (!usesAuthoredAnimator)
            {
                return;
            }
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                animatorParameters[parameter.nameHash] = parameter.type;
            }
        }

        private void SetAnimatorFloat(string parameterName, float value)
        {
            if (string.IsNullOrEmpty(parameterName)) return;
            int hash = Animator.StringToHash(parameterName);
            if (animator != null && animatorParameters.TryGetValue(hash, out var type)
                && type == AnimatorControllerParameterType.Float)
            {
                animator.SetFloat(hash, value);
            }
        }

        private void SetAnimatorTrigger(string parameterName)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                return;
            }
            int hash = Animator.StringToHash(parameterName);
            if (animator != null && animatorParameters.TryGetValue(hash, out var type)
                && type == AnimatorControllerParameterType.Trigger)
            {
                animator.SetTrigger(hash);
            }
        }

        private static void SetLocalRotation(Transform target, Vector3 euler)
        {
            if (target != null)
            {
                target.localRotation = Quaternion.Euler(euler);
            }
        }

        private void OnDestroy()
        {
            foreach (Material material in runtimeMaterials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
            runtimeMaterials.Clear();
        }
    }
}
