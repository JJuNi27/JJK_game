using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class GojoCharacterPresentation : MonoBehaviour
    {
        private enum FallbackPose
        {
            None,
            Attack1,
            Attack2,
            Attack3,
            Blue,
            Red,
            Purple,
            Domain,
            Dodge,
        }

        private const string PrototypeRootName = "PrototypeGojoAvatar";
        private const string ExternalRootName = "ExternalCharacterModel";

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int IsDodgingHash = Animator.StringToHash("IsDodging");
        private static readonly int Attack1Hash = Animator.StringToHash("Attack1");
        private static readonly int Attack2Hash = Animator.StringToHash("Attack2");
        private static readonly int Attack3Hash = Animator.StringToHash("Attack3");
        private static readonly int BlueHash = Animator.StringToHash("Blue");
        private static readonly int RedHash = Animator.StringToHash("Red");
        private static readonly int PurpleHash = Animator.StringToHash("Purple");
        private static readonly int DomainHash = Animator.StringToHash("Domain");
        private static readonly int DodgeHash = Animator.StringToHash("Dodge");

        [Header("External Humanoid Model")]
        [SerializeField] private bool loadModelFromResources = true;
        [SerializeField] private Vector3 externalModelLocalPosition = Vector3.zero;
        [SerializeField] private Vector3 externalModelLocalEuler = Vector3.zero;
        [SerializeField] private float externalModelScale = 1f;

        [Header("Fallback Pose Timing")]
        [SerializeField, Min(0.05f)] private float attackPoseDuration = 0.22f;
        [SerializeField, Min(0.05f)] private float techniquePoseDuration = 0.42f;
        [SerializeField, Min(0.05f)] private float purplePoseDuration = 0.70f;
        [SerializeField, Min(0.05f)] private float domainPoseDuration = 0.90f;

        private readonly HashSet<int> animatorParameters = new HashSet<int>();

        private GojoVariantController variant;
        private GojoPrototypeAvatar prototypeAvatar;
        private ThirdPersonPlayerController movement;
        private CharacterController characterController;
        private BasicAttack basicAttack;
        private GojoTechniqueController technique;
        private GojoDomainController domain;

        private GameObject externalModel;
        private Animator animator;
        private Transform prototypeRoot;
        private Transform torso;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftLeg;
        private Transform rightLeg;

        private int previousAttackStep;
        private bool previousBlueReady;
        private bool previousRedReady;
        private bool previousPurpleVisualActive;
        private bool previousDodging;
        private GojoDomainController.DomainState previousDomainState;

        private FallbackPose fallbackPose;
        private float fallbackPoseStartedAt;
        private float fallbackPoseEndsAt;

        public bool UsingExternalModel => externalModel != null;
        public string ActiveResourcePath => ResolveResourcePath();

        public static GojoCharacterPresentation GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            GojoCharacterPresentation presentation = owner.GetComponent<GojoCharacterPresentation>();
            return presentation != null
                ? presentation
                : owner.AddComponent<GojoCharacterPresentation>();
        }

        private void Awake()
        {
            variant = GetComponent<GojoVariantController>();
            movement = GetComponent<ThirdPersonPlayerController>();
            characterController = GetComponent<CharacterController>();
            basicAttack = GetComponent<BasicAttack>();
            technique = GetComponent<GojoTechniqueController>();
            domain = GetComponent<GojoDomainController>();
        }

        private void Start()
        {
            EnsureReferences();
            BuildPresentation();
            CaptureInitialStates();
        }

        private void OnDestroy()
        {
            if (externalModel != null)
            {
                Destroy(externalModel);
            }
        }

        private void Update()
        {
            EnsureReferences();
            UpdateAnimatorLocomotion();
            DetectCombatPresentationEvents();
            UpdateFallbackPose();
        }

        public void RebuildForVariant()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            BuildPresentation();
            CaptureInitialStates();
        }

        private void EnsureReferences()
        {
            variant ??= GetComponent<GojoVariantController>();
            movement ??= GetComponent<ThirdPersonPlayerController>();
            characterController ??= GetComponent<CharacterController>();
            basicAttack ??= GetComponent<BasicAttack>();
            technique ??= GetComponent<GojoTechniqueController>();
            domain ??= GetComponent<GojoDomainController>();
            prototypeAvatar ??= GojoPrototypeAvatar.GetOrCreate(gameObject);
        }

        private void BuildPresentation()
        {
            Transform oldExternal = transform.Find(ExternalRootName);
            if (oldExternal != null)
            {
                Destroy(oldExternal.gameObject);
            }
            externalModel = null;
            animator = null;
            animatorParameters.Clear();

            prototypeAvatar ??= GojoPrototypeAvatar.GetOrCreate(gameObject);
            prototypeAvatar?.Rebuild();
            prototypeRoot = transform.Find(PrototypeRootName);
            CachePrototypeBones();

            GameObject modelPrefab = loadModelFromResources
                ? Resources.Load<GameObject>(ResolveResourcePath())
                : null;

            if (modelPrefab != null)
            {
                externalModel = Instantiate(modelPrefab, transform);
                externalModel.name = ExternalRootName;
                externalModel.transform.localPosition = externalModelLocalPosition;
                externalModel.transform.localRotation = Quaternion.Euler(externalModelLocalEuler);
                externalModel.transform.localScale = Vector3.one * Mathf.Max(0.01f, externalModelScale);
                animator = externalModel.GetComponentInChildren<Animator>();
                CacheAnimatorParameters();

                if (prototypeRoot != null)
                {
                    prototypeRoot.gameObject.SetActive(false);
                }
                if (prototypeAvatar != null)
                {
                    prototypeAvatar.enabled = false;
                }
            }
            else
            {
                if (prototypeRoot != null)
                {
                    prototypeRoot.gameObject.SetActive(true);
                }
                if (prototypeAvatar != null)
                {
                    // Presentation owns the fallback locomotion and combat poses.
                    prototypeAvatar.enabled = false;
                }
            }
        }

        private void CaptureInitialStates()
        {
            previousAttackStep = basicAttack != null ? basicAttack.DisplayChainStep : 0;
            previousBlueReady = technique == null || technique.BlueReady;
            previousRedReady = technique == null || technique.RedReady;
            previousPurpleVisualActive = IsPurpleVisualActive();
            previousDodging = movement != null && movement.IsDodging;
            previousDomainState = domain != null
                ? domain.State
                : GojoDomainController.DomainState.Normal;
            fallbackPose = FallbackPose.None;
            fallbackPoseEndsAt = 0f;
        }

        private void UpdateAnimatorLocomotion()
        {
            float horizontalSpeed = 0f;
            if (characterController != null)
            {
                Vector3 velocity = characterController.velocity;
                velocity.y = 0f;
                horizontalSpeed = velocity.magnitude;
            }

            float normalizedSpeed = Mathf.Clamp01(horizontalSpeed / 5.5f);
            bool isMoving = normalizedSpeed > 0.05f;
            bool isDodging = movement != null && movement.IsDodging;

            SetAnimatorFloat(SpeedHash, normalizedSpeed);
            SetAnimatorBool(IsMovingHash, isMoving);
            SetAnimatorBool(IsDodgingHash, isDodging);

            if (animator == null && fallbackPose == FallbackPose.None)
            {
                ApplyFallbackLocomotion(normalizedSpeed);
            }
        }

        private void DetectCombatPresentationEvents()
        {
            if (basicAttack != null)
            {
                int currentStep = basicAttack.DisplayChainStep;
                if (currentStep > 0 && currentStep != previousAttackStep)
                {
                    PlayAttack(currentStep);
                }
                previousAttackStep = currentStep;
            }

            if (technique != null)
            {
                bool blueReadyNow = technique.BlueReady;
                bool redReadyNow = technique.RedReady;
                if (previousBlueReady && !blueReadyNow)
                {
                    PlayTechnique(FallbackPose.Blue, BlueHash, techniquePoseDuration);
                }
                if (previousRedReady && !redReadyNow)
                {
                    PlayTechnique(FallbackPose.Red, RedHash, techniquePoseDuration);
                }
                previousBlueReady = blueReadyNow;
                previousRedReady = redReadyNow;
            }

            bool purpleActiveNow = IsPurpleVisualActive();
            if (!previousPurpleVisualActive && purpleActiveNow)
            {
                PlayTechnique(FallbackPose.Purple, PurpleHash, purplePoseDuration);
            }
            previousPurpleVisualActive = purpleActiveNow;

            if (domain != null)
            {
                GojoDomainController.DomainState currentDomainState = domain.State;
                if (
                    previousDomainState != GojoDomainController.DomainState.Active
                    && currentDomainState == GojoDomainController.DomainState.Active
                )
                {
                    PlayTechnique(FallbackPose.Domain, DomainHash, domainPoseDuration);
                }
                previousDomainState = currentDomainState;
            }

            bool dodgingNow = movement != null && movement.IsDodging;
            if (!previousDodging && dodgingNow)
            {
                PlayTechnique(FallbackPose.Dodge, DodgeHash, 0.26f);
            }
            previousDodging = dodgingNow;
        }

        private void PlayAttack(int step)
        {
            switch (step)
            {
                case 2:
                    TriggerAnimator(Attack2Hash);
                    BeginFallbackPose(FallbackPose.Attack2, attackPoseDuration);
                    break;
                case 3:
                    TriggerAnimator(Attack3Hash);
                    BeginFallbackPose(FallbackPose.Attack3, attackPoseDuration * 1.45f);
                    break;
                default:
                    TriggerAnimator(Attack1Hash);
                    BeginFallbackPose(FallbackPose.Attack1, attackPoseDuration);
                    break;
            }
        }

        private void PlayTechnique(FallbackPose pose, int triggerHash, float duration)
        {
            TriggerAnimator(triggerHash);
            BeginFallbackPose(pose, duration);
        }

        private void BeginFallbackPose(FallbackPose pose, float duration)
        {
            fallbackPose = pose;
            fallbackPoseStartedAt = Time.time;
            fallbackPoseEndsAt = Time.time + Mathf.Max(0.05f, duration);
        }

        private void UpdateFallbackPose()
        {
            if (animator != null || prototypeRoot == null || !prototypeRoot.gameObject.activeSelf)
            {
                return;
            }

            if (fallbackPose == FallbackPose.None)
            {
                return;
            }

            float duration = Mathf.Max(0.05f, fallbackPoseEndsAt - fallbackPoseStartedAt);
            float normalized = Mathf.Clamp01((Time.time - fallbackPoseStartedAt) / duration);
            float weight = Mathf.Sin(normalized * Mathf.PI);
            ApplyFallbackCombatPose(fallbackPose, weight);

            if (Time.time >= fallbackPoseEndsAt)
            {
                fallbackPose = FallbackPose.None;
                ResetFallbackBones();
            }
        }

        private void ApplyFallbackLocomotion(float normalizedSpeed)
        {
            if (prototypeRoot == null)
            {
                return;
            }

            float swing = Mathf.Sin(Time.time * 8f) * 20f * normalizedSpeed;
            SetLocalRotation(leftArm, new Vector3(swing, 0f, 0f));
            SetLocalRotation(rightArm, new Vector3(-swing, 0f, 0f));
            SetLocalRotation(leftLeg, new Vector3(-swing * 0.65f, 0f, 0f));
            SetLocalRotation(rightLeg, new Vector3(swing * 0.65f, 0f, 0f));
            SetLocalRotation(torso, Vector3.zero);
            prototypeRoot.localPosition = Vector3.up * (Mathf.Sin(Time.time * 2.2f) * 0.015f);
        }

        private void ApplyFallbackCombatPose(FallbackPose pose, float weight)
        {
            Vector3 leftArmEuler = Vector3.zero;
            Vector3 rightArmEuler = Vector3.zero;
            Vector3 leftLegEuler = Vector3.zero;
            Vector3 rightLegEuler = Vector3.zero;
            Vector3 torsoEuler = Vector3.zero;

            switch (pose)
            {
                case FallbackPose.Attack1:
                    rightArmEuler = new Vector3(-105f, 0f, -10f);
                    leftArmEuler = new Vector3(25f, 0f, 8f);
                    torsoEuler = new Vector3(0f, -18f, 0f);
                    break;
                case FallbackPose.Attack2:
                    leftArmEuler = new Vector3(-105f, 0f, 10f);
                    rightArmEuler = new Vector3(25f, 0f, -8f);
                    torsoEuler = new Vector3(0f, 18f, 0f);
                    break;
                case FallbackPose.Attack3:
                    leftArmEuler = new Vector3(-82f, 0f, 14f);
                    rightArmEuler = new Vector3(-82f, 0f, -14f);
                    torsoEuler = new Vector3(-8f, 0f, 0f);
                    leftLegEuler = new Vector3(18f, 0f, 0f);
                    rightLegEuler = new Vector3(-18f, 0f, 0f);
                    break;
                case FallbackPose.Blue:
                    rightArmEuler = new Vector3(-92f, 0f, -8f);
                    leftArmEuler = new Vector3(-20f, 0f, 18f);
                    torsoEuler = new Vector3(0f, -10f, 0f);
                    break;
                case FallbackPose.Red:
                    leftArmEuler = new Vector3(-92f, 0f, 8f);
                    rightArmEuler = new Vector3(-18f, 0f, -16f);
                    torsoEuler = new Vector3(0f, 10f, 0f);
                    break;
                case FallbackPose.Purple:
                    leftArmEuler = new Vector3(-96f, 0f, 15f);
                    rightArmEuler = new Vector3(-96f, 0f, -15f);
                    torsoEuler = new Vector3(-6f, 0f, 0f);
                    break;
                case FallbackPose.Domain:
                    leftArmEuler = new Vector3(-132f, 0f, 38f);
                    rightArmEuler = new Vector3(-132f, 0f, -38f);
                    torsoEuler = new Vector3(-4f, 0f, 0f);
                    break;
                case FallbackPose.Dodge:
                    leftArmEuler = new Vector3(55f, 0f, 12f);
                    rightArmEuler = new Vector3(55f, 0f, -12f);
                    torsoEuler = new Vector3(16f, 0f, 0f);
                    leftLegEuler = new Vector3(-28f, 0f, 0f);
                    rightLegEuler = new Vector3(22f, 0f, 0f);
                    break;
            }

            SetLocalRotation(leftArm, Vector3.Lerp(Vector3.zero, leftArmEuler, weight));
            SetLocalRotation(rightArm, Vector3.Lerp(Vector3.zero, rightArmEuler, weight));
            SetLocalRotation(leftLeg, Vector3.Lerp(Vector3.zero, leftLegEuler, weight));
            SetLocalRotation(rightLeg, Vector3.Lerp(Vector3.zero, rightLegEuler, weight));
            SetLocalRotation(torso, Vector3.Lerp(Vector3.zero, torsoEuler, weight));
        }

        private void ResetFallbackBones()
        {
            SetLocalRotation(leftArm, Vector3.zero);
            SetLocalRotation(rightArm, Vector3.zero);
            SetLocalRotation(leftLeg, Vector3.zero);
            SetLocalRotation(rightLeg, Vector3.zero);
            SetLocalRotation(torso, Vector3.zero);
        }

        private void CachePrototypeBones()
        {
            prototypeRoot = transform.Find(PrototypeRootName);
            torso = prototypeRoot != null ? prototypeRoot.Find("Torso") : null;
            leftArm = prototypeRoot != null ? prototypeRoot.Find("LeftArm") : null;
            rightArm = prototypeRoot != null ? prototypeRoot.Find("RightArm") : null;
            leftLeg = prototypeRoot != null ? prototypeRoot.Find("LeftLeg") : null;
            rightLeg = prototypeRoot != null ? prototypeRoot.Find("RightLeg") : null;
        }

        private bool IsPurpleVisualActive()
        {
            Transform purple = transform.Find("HollowPurplePrototypeVisual");
            return purple != null && purple.gameObject.activeSelf;
        }

        private string ResolveResourcePath()
        {
            GojoVariantId activeVariant = variant != null
                ? variant.ActiveVariant
                : GojoVariantId.ModernTeacher;

            return activeVariant switch
            {
                GojoVariantId.HiddenInventoryPreAwakening => "CharacterModels/Gojo_HiddenInventory_PreAwakening",
                GojoVariantId.HiddenInventoryAwakened => "CharacterModels/Gojo_HiddenInventory_Awakened",
                GojoVariantId.ShinjukuShowdown => "CharacterModels/Gojo_Shinjuku",
                _ => "CharacterModels/Gojo_Modern",
            };
        }

        private void CacheAnimatorParameters()
        {
            animatorParameters.Clear();
            if (animator == null)
            {
                return;
            }

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                animatorParameters.Add(parameter.nameHash);
            }
        }

        private void SetAnimatorFloat(int parameterHash, float value)
        {
            if (animator != null && animatorParameters.Contains(parameterHash))
            {
                animator.SetFloat(parameterHash, value);
            }
        }

        private void SetAnimatorBool(int parameterHash, bool value)
        {
            if (animator != null && animatorParameters.Contains(parameterHash))
            {
                animator.SetBool(parameterHash, value);
            }
        }

        private void TriggerAnimator(int parameterHash)
        {
            if (animator != null && animatorParameters.Contains(parameterHash))
            {
                animator.SetTrigger(parameterHash);
            }
        }

        private static void SetLocalRotation(Transform target, Vector3 euler)
        {
            if (target != null)
            {
                target.localRotation = Quaternion.Euler(euler);
            }
        }
    }
}
