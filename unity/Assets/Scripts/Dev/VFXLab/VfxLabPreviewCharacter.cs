using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Dev.VFXLab
{
    public enum VfxLabTechniqueMotion
    {
        Idle,
        Anticipation,
        Cast,
        Release,
        Recover,
    }

    /// <summary>
    /// Developer-only movement and animation preview adapter. It deliberately has
    /// no Health, CE, cooldown, hitbox, or combat-controller dependency.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VfxLabPreviewCharacter : MonoBehaviour
    {
        private const float MoveSpeed = 4.5f;
        private const float RotationSpeed = 14f;
        private const float Gravity = -24f;

        private readonly List<Material> runtimeMaterials = new List<Material>(8);
        private readonly HashSet<int> animatorParameters = new HashSet<int>();

        [SerializeField] private Animator animator;
        [SerializeField] private string planarSpeedParameter = "PlanarSpeed";
        [SerializeField] private string anticipationTrigger = "TechniqueAnticipation";
        [SerializeField] private string castTrigger = "TechniqueCast";
        [SerializeField] private string releaseTrigger = "TechniqueRelease";
        [SerializeField] private string recoverTrigger = "TechniqueRecover";

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
        private VfxLabTechniqueMotion techniqueMotion;
        private bool usesAuthoredAnimator;

        public float PlanarSpeed => planarSpeed;
        public bool UsesAuthoredAnimator => usesAuthoredAnimator;
        public string AnimationSourceLabel => usesAuthoredAnimator
            ? "AUTHORED ANIMATOR HOOK"
            : "PROTOTYPE PROCEDURAL MOTION";

        public void Configure(Transform newCameraTransform)
        {
            cameraTransform = newCameraTransform;
        }

        private void Awake()
        {
            motor = GetComponent<CharacterController>();
            if (motor == null)
            {
                motor = gameObject.AddComponent<CharacterController>();
            }
            motor.height = 2f;
            motor.radius = 0.42f;
            motor.center = Vector3.up;
            motor.stepOffset = 0.28f;

            animator ??= GetComponentInChildren<Animator>();
            usesAuthoredAnimator = animator != null && animator.runtimeAnimatorController != null;
            CacheAnimatorParameters();
            if (!usesAuthoredAnimator)
            {
                BuildFallbackGojo();
            }
        }

        private void Update()
        {
            ApplyMovement();
            if (usesAuthoredAnimator)
            {
                SetAnimatorFloat(planarSpeedParameter, planarSpeed);
            }
            else
            {
                ApplyProceduralMotion();
            }
        }

        public void SetTechniqueMotion(VfxLabTechniqueMotion motion)
        {
            if (techniqueMotion == motion)
            {
                return;
            }

            techniqueMotion = motion;
            motionStartedAt = Time.time;
            if (!usesAuthoredAnimator)
            {
                return;
            }

            string trigger = motion switch
            {
                VfxLabTechniqueMotion.Anticipation => anticipationTrigger,
                VfxLabTechniqueMotion.Cast => castTrigger,
                VfxLabTechniqueMotion.Release => releaseTrigger,
                VfxLabTechniqueMotion.Recover => recoverTrigger,
                _ => string.Empty,
            };
            SetAnimatorTrigger(trigger);
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
            if (direction.sqrMagnitude > 0.001f)
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
            Vector3 velocity = direction * MoveSpeed;
            planarSpeed = velocity.magnitude;
            velocity.y = verticalVelocity;
            motor.Move(velocity * Time.deltaTime);
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

            float movementWeight = Mathf.Clamp01(planarSpeed / MoveSpeed);
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
            switch (techniqueMotion)
            {
                case VfxLabTechniqueMotion.Anticipation:
                    ApplyTechniquePose(
                        new Vector3(5f, -8f, 0f),
                        new Vector3(-54f, -16f, 30f),
                        new Vector3(-76f, 18f, -22f),
                        enterWeight
                    );
                    break;
                case VfxLabTechniqueMotion.Cast:
                    ApplyTechniquePose(
                        new Vector3(7f, 0f, 0f),
                        new Vector3(-80f, -10f, 22f),
                        new Vector3(-102f, 8f, -12f),
                        enterWeight
                    );
                    break;
                case VfxLabTechniqueMotion.Release:
                    ApplyTechniquePose(
                        new Vector3(-4f, 10f, 0f),
                        new Vector3(-34f, -20f, 28f),
                        new Vector3(-118f, 2f, -8f),
                        enterWeight
                    );
                    break;
                case VfxLabTechniqueMotion.Recover:
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

        private void CacheAnimatorParameters()
        {
            animatorParameters.Clear();
            if (!usesAuthoredAnimator)
            {
                return;
            }
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                animatorParameters.Add(parameter.nameHash);
            }
        }

        private void SetAnimatorFloat(string parameterName, float value)
        {
            int hash = Animator.StringToHash(parameterName);
            if (animator != null && animatorParameters.Contains(hash))
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
            if (animator != null && animatorParameters.Contains(hash))
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
