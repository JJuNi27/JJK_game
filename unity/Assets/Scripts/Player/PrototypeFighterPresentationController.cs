using UnityEngine;

namespace JJKGame.Player
{
    [DefaultExecutionOrder(1500)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BasicAttack))]
    public sealed class PrototypeFighterPresentationController : MonoBehaviour
    {
        private const string GojoRootName = "PrototypeGojoAvatar";
        private const string SukunaRootName = "PrototypeSukunaAvatar";

        private BasicAttack basicAttack;
        private ThirdPersonPlayerController movementController;
        private CharacterController characterController;
        private Transform activeVisualRoot;
        private Transform leftArm;
        private Transform rightArm;
        private int observedAttackStep;
        private float poseStartedAt;
        private float entryStartedAt;
        private string activeRootName = string.Empty;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapAfterSceneLoad()
        {
            BasicAttack[] attacks = FindObjectsByType<BasicAttack>(FindObjectsSortMode.None);
            foreach (BasicAttack attack in attacks)
            {
                if (attack == null || attack.GetComponent<PrototypeFighterPresentationController>() != null)
                {
                    continue;
                }

                attack.gameObject.AddComponent<PrototypeFighterPresentationController>();
            }
        }

        private void Awake()
        {
            basicAttack = GetComponent<BasicAttack>();
            movementController = GetComponent<ThirdPersonPlayerController>();
            characterController = GetComponent<CharacterController>();
            ResolveActiveVisual(true);
        }

        private void Update()
        {
            if (basicAttack == null)
            {
                return;
            }

            ResolveActiveVisual(false);
            if (activeVisualRoot == null)
            {
                return;
            }

            int attackStep = basicAttack.DisplayChainStep;
            if (attackStep != 0 && attackStep != observedAttackStep)
            {
                observedAttackStep = attackStep;
                poseStartedAt = Time.time;
            }
            else if (attackStep == 0)
            {
                observedAttackStep = 0;
            }

            ApplyEntryPulse();

            if (attackStep > 0)
            {
                ApplyAttackPose(attackStep);
                return;
            }

            if (movementController != null && movementController.IsDodging)
            {
                ApplyDodgePose();
                return;
            }

            ApplyLocomotionStance();
        }

        private void ResolveActiveVisual(bool force)
        {
            Transform gojoRoot = transform.Find(GojoRootName);
            Transform sukunaRoot = transform.Find(SukunaRootName);

            Transform nextRoot = null;
            if (gojoRoot != null && gojoRoot.gameObject.activeInHierarchy)
            {
                nextRoot = gojoRoot;
            }
            else if (sukunaRoot != null && sukunaRoot.gameObject.activeInHierarchy)
            {
                nextRoot = sukunaRoot;
            }

            if (!force && nextRoot == activeVisualRoot)
            {
                return;
            }

            if (activeVisualRoot != null)
            {
                activeVisualRoot.localRotation = Quaternion.identity;
                activeVisualRoot.localScale = Vector3.one;
            }

            activeVisualRoot = nextRoot;
            leftArm = activeVisualRoot != null ? activeVisualRoot.Find("LeftArm") : null;
            rightArm = activeVisualRoot != null ? activeVisualRoot.Find("RightArm") : null;
            activeRootName = activeVisualRoot != null ? activeVisualRoot.name : string.Empty;
            observedAttackStep = 0;
            poseStartedAt = 0f;
            entryStartedAt = Time.time;
        }

        private void ApplyEntryPulse()
        {
            float elapsed = Time.time - entryStartedAt;
            const float duration = 0.22f;
            if (elapsed >= duration)
            {
                activeVisualRoot.localScale = Vector3.one;
                return;
            }

            float progress = Mathf.Clamp01(elapsed / duration);
            float pulse = (1f - progress) * 0.075f;
            activeVisualRoot.localScale = Vector3.one * (1f + pulse);
        }

        private void ApplyLocomotionStance()
        {
            float planarSpeed = 0f;
            if (characterController != null)
            {
                Vector3 velocity = characterController.velocity;
                velocity.y = 0f;
                planarSpeed = velocity.magnitude;
            }

            float moveWeight = Mathf.Clamp01(planarSpeed / 5.5f);
            bool sukuna = activeRootName == SukunaRootName;
            float breathing = Mathf.Sin(Time.time * (sukuna ? 3.6f : 2.8f));

            Vector3 targetRootEuler;
            if (sukuna)
            {
                targetRootEuler = new Vector3(
                    4f + moveWeight * 7f,
                    breathing * (1.2f + moveWeight * 1.6f),
                    breathing * 0.8f * moveWeight
                );
            }
            else
            {
                targetRootEuler = new Vector3(
                    1.5f + moveWeight * 4f,
                    breathing * (0.8f + moveWeight * 0.8f),
                    breathing * 0.5f * moveWeight
                );
            }

            activeVisualRoot.localRotation = Quaternion.Slerp(
                activeVisualRoot.localRotation,
                Quaternion.Euler(targetRootEuler),
                1f - Mathf.Exp(-10f * Time.deltaTime)
            );
        }

        private void ApplyDodgePose()
        {
            float progress = movementController != null ? movementController.DodgeProgress : 0f;
            float envelope = Mathf.Sin(Mathf.Clamp01(progress) * Mathf.PI);
            bool sukuna = activeRootName == SukunaRootName;

            float forwardLean = sukuna ? 34f : 27f;
            float twist = sukuna ? 10f : 6f;
            Vector3 rootEuler = new Vector3(forwardLean * envelope, twist * envelope, 0f);
            activeVisualRoot.localRotation = Quaternion.Euler(rootEuler);

            if (leftArm != null)
            {
                Vector3 leftEuler = sukuna
                    ? new Vector3(48f, -16f, 24f)
                    : new Vector3(38f, -10f, 16f);
                leftArm.localRotation = Quaternion.Slerp(
                    leftArm.localRotation,
                    Quaternion.Euler(leftEuler),
                    envelope
                );
            }

            if (rightArm != null)
            {
                Vector3 rightEuler = sukuna
                    ? new Vector3(52f, 18f, -28f)
                    : new Vector3(42f, 12f, -18f);
                rightArm.localRotation = Quaternion.Slerp(
                    rightArm.localRotation,
                    Quaternion.Euler(rightEuler),
                    envelope
                );
            }

            float stretch = envelope * (sukuna ? 0.06f : 0.045f);
            activeVisualRoot.localScale = new Vector3(
                1f - stretch * 0.35f,
                1f + stretch,
                1f - stretch * 0.20f
            );
        }

        private void ApplyAttackPose(int attackStep)
        {
            activeVisualRoot.localRotation = Quaternion.identity;
            if (attackStep <= 0 || leftArm == null || rightArm == null)
            {
                return;
            }

            float duration = attackStep switch
            {
                1 => 0.18f,
                2 => 0.20f,
                _ => 0.28f,
            };
            float progress = Mathf.Clamp01((Time.time - poseStartedAt) / duration);
            float envelope = Mathf.Sin(progress * Mathf.PI);
            if (envelope <= 0.001f)
            {
                return;
            }

            bool sukuna = activeRootName == SukunaRootName;
            if (sukuna)
            {
                ApplySukunaPose(attackStep, envelope);
            }
            else
            {
                ApplyGojoPose(attackStep, envelope);
            }
        }

        private void ApplyGojoPose(int attackStep, float weight)
        {
            Vector3 leftEuler;
            Vector3 rightEuler;
            Vector3 rootEuler;

            switch (attackStep)
            {
                case 1:
                    leftEuler = new Vector3(10f, -4f, 8f);
                    rightEuler = new Vector3(-76f, 12f, -20f);
                    rootEuler = new Vector3(0f, 8f, 0f);
                    break;
                case 2:
                    leftEuler = new Vector3(-82f, -14f, 20f);
                    rightEuler = new Vector3(12f, 5f, -8f);
                    rootEuler = new Vector3(0f, -9f, 0f);
                    break;
                default:
                    leftEuler = new Vector3(-58f, -18f, 26f);
                    rightEuler = new Vector3(-58f, 18f, -26f);
                    rootEuler = new Vector3(7f, 0f, 0f);
                    break;
            }

            leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, Quaternion.Euler(leftEuler), weight);
            rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, Quaternion.Euler(rightEuler), weight);
            activeVisualRoot.localRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(rootEuler), weight);
        }

        private void ApplySukunaPose(int attackStep, float weight)
        {
            Vector3 leftEuler;
            Vector3 rightEuler;
            Vector3 rootEuler;

            switch (attackStep)
            {
                case 1:
                    leftEuler = new Vector3(18f, -8f, 16f);
                    rightEuler = new Vector3(-92f, 18f, -26f);
                    rootEuler = new Vector3(5f, 11f, 0f);
                    break;
                case 2:
                    leftEuler = new Vector3(-96f, -20f, 28f);
                    rightEuler = new Vector3(18f, 8f, -16f);
                    rootEuler = new Vector3(6f, -12f, 0f);
                    break;
                default:
                    leftEuler = new Vector3(-70f, -26f, 32f);
                    rightEuler = new Vector3(-70f, 26f, -32f);
                    rootEuler = new Vector3(12f, 0f, 0f);
                    break;
            }

            leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, Quaternion.Euler(leftEuler), weight);
            rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, Quaternion.Euler(rightEuler), weight);
            activeVisualRoot.localRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(rootEuler), weight);
        }
    }
}
