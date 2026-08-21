using JJKGame.CameraSystem;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [DefaultExecutionOrder(1600)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BasicAttack))]
    public sealed class PrototypeSignatureTechniqueFeedbackController : MonoBehaviour
    {
        private const string PurpleVisualName = "HollowPurplePrototypeVisual";

        private Health ownHealth;
        private PrototypeCharacterController characterController;
        private SukunaTechniqueController sukunaTechnique;
        private GojoDomainController gojoDomain;
        private SukunaDomainController sukunaDomain;
        private SimpleCameraFollow combatCamera;

        private bool purpleWasActive;
        private bool purpleCulminationPending;
        private float purpleCulminationAt;
        private bool gojoDomainWasActive;
        private GojoDomainController.DomainState previousGojoDomainState;
        private bool sukunaDomainWasCasting;
        private bool sukunaDomainWasActive;
        private int observedFugaProjectileId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapAfterSceneLoad()
        {
            BasicAttack[] attacks = FindObjectsByType<BasicAttack>(FindObjectsSortMode.None);
            foreach (BasicAttack attack in attacks)
            {
                if (
                    attack == null
                    || attack.GetComponent<PrototypeSignatureTechniqueFeedbackController>() != null
                )
                {
                    continue;
                }

                attack.gameObject.AddComponent<PrototypeSignatureTechniqueFeedbackController>();
            }
        }

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
            characterController = GetComponent<PrototypeCharacterController>();
            sukunaTechnique = GetComponent<SukunaTechniqueController>();
            gojoDomain = GetComponent<GojoDomainController>();
            sukunaDomain = GetComponent<SukunaDomainController>();
            combatCamera = FindFirstObjectByType<SimpleCameraFollow>();

            previousGojoDomainState = gojoDomain != null
                ? gojoDomain.State
                : GojoDomainController.DomainState.Normal;
            sukunaDomainWasCasting = sukunaDomain != null && sukunaDomain.IsCasting;
            sukunaDomainWasActive = sukunaDomain != null && sukunaDomain.IsActive;
        }

        private void OnEnable()
        {
            SukunaFugaProjectile.Exploded -= HandleFugaExploded;
            SukunaFugaProjectile.Exploded += HandleFugaExploded;
        }

        private void OnDisable()
        {
            SukunaFugaProjectile.Exploded -= HandleFugaExploded;
        }

        private void Update()
        {
            DetectSignatureAnticipation();
            DetectPurpleRelease();
            DetectPurpleCulmination();
            DetectFugaRelease();
            DetectDomainActivation();
        }

        private void DetectSignatureAnticipation()
        {
            gojoDomain ??= GetComponent<GojoDomainController>();
            GojoDomainController.DomainState gojoState = gojoDomain != null
                ? gojoDomain.State
                : GojoDomainController.DomainState.Normal;
            if (
                gojoDomain != null
                && gojoDomain.enabled
                && gojoState == GojoDomainController.DomainState.DomainReady
                && previousGojoDomainState != GojoDomainController.DomainState.DomainReady
            )
            {
                PlayFeedback(
                    new Color(0.38f, 0.58f, 1f),
                    0.035f,
                    0.10f,
                    0.060f,
                    0.080f,
                    0f,
                    1f
                );
            }
            previousGojoDomainState = gojoState;

            sukunaDomain ??= GetComponent<SukunaDomainController>();
            bool sukunaCasting =
                sukunaDomain != null
                && sukunaDomain.enabled
                && sukunaDomain.IsCasting;
            if (sukunaCasting && !sukunaDomainWasCasting)
            {
                PlayFeedback(
                    new Color(0.55f, 0.025f, 0.015f),
                    0.050f,
                    0.12f,
                    0.085f,
                    0.10f,
                    0f,
                    1f
                );
            }
            sukunaDomainWasCasting = sukunaCasting;

            characterController ??= GetComponent<PrototypeCharacterController>();
            if (
                !Input.GetKeyDown(CombatInputBindings.Ultimate)
                || characterController == null
                || !characterController.IsSukuna
            )
            {
                return;
            }

            sukunaTechnique ??= GetComponent<SukunaTechniqueController>();
            if (
                sukunaTechnique == null
                || !sukunaTechnique.enabled
                || !sukunaTechnique.FugaPrepared
                || sukunaTechnique.FugaCooldownRemaining > 0.02f
            )
            {
                return;
            }

            bool domainActive = sukunaDomain != null && sukunaDomain.enabled && sukunaDomain.IsActive;
            int activeOpponents = CountLivingActiveOpponents();
            bool likelyValidFuga = domainActive ? activeOpponents > 0 : activeOpponents == 1;
            if (!likelyValidFuga)
            {
                return;
            }

            PlayFeedback(
                domainActive ? new Color(0.78f, 0.035f, 0.015f) : new Color(0.70f, 0.12f, 0.02f),
                domainActive ? 0.070f : 0.055f,
                0.12f,
                domainActive ? 0.13f : 0.10f,
                0.10f,
                0f,
                1f
            );
        }

        private void DetectPurpleRelease()
        {
            Transform purpleRoot = transform.Find(PurpleVisualName);
            bool purpleActive = purpleRoot != null && purpleRoot.gameObject.activeInHierarchy;
            if (purpleActive && !purpleWasActive)
            {
                PlayFeedback(
                    new Color(0.72f, 0.28f, 1f),
                    0.14f,
                    0.16f,
                    0.42f,
                    0.20f,
                    0.065f,
                    0.10f
                );

                purpleCulminationPending = true;
                purpleCulminationAt = Time.unscaledTime + 0.09f;
            }

            purpleWasActive = purpleActive;
        }

        private void DetectPurpleCulmination()
        {
            if (!purpleCulminationPending || Time.unscaledTime < purpleCulminationAt)
            {
                return;
            }

            purpleCulminationPending = false;
            PlayFeedback(
                new Color(0.86f, 0.62f, 1f),
                0.075f,
                0.10f,
                0.20f,
                0.11f,
                0.020f,
                0.16f
            );
        }

        private void DetectFugaRelease()
        {
            SukunaFugaProjectile projectile = FindFirstObjectByType<SukunaFugaProjectile>();
            if (projectile == null)
            {
                observedFugaProjectileId = 0;
                return;
            }

            int instanceId = projectile.GetInstanceID();
            if (instanceId == observedFugaProjectileId)
            {
                return;
            }

            observedFugaProjectileId = instanceId;
            bool domainAmplified = projectile.gameObject.name.Contains("Domain");
            PlayFeedback(
                domainAmplified ? new Color(1f, 0.18f, 0.04f) : new Color(1f, 0.42f, 0.08f),
                domainAmplified ? 0.14f : 0.10f,
                domainAmplified ? 0.17f : 0.12f,
                domainAmplified ? 0.38f : 0.28f,
                domainAmplified ? 0.19f : 0.14f,
                domainAmplified ? 0.050f : 0.035f,
                0.10f
            );
        }

        private void HandleFugaExploded(Health projectileOwner, Vector3 worldPosition, bool domainAmplified)
        {
            if (projectileOwner == null || ownHealth == null || projectileOwner != ownHealth)
            {
                return;
            }

            _ = worldPosition;
            PlayFeedback(
                domainAmplified ? new Color(1f, 0.12f, 0.015f) : new Color(1f, 0.48f, 0.06f),
                domainAmplified ? 0.22f : 0.18f,
                domainAmplified ? 0.24f : 0.20f,
                domainAmplified ? 0.60f : 0.48f,
                domainAmplified ? 0.26f : 0.22f,
                domainAmplified ? 0.075f : 0.060f,
                0.08f
            );
        }

        private void DetectDomainActivation()
        {
            gojoDomain ??= GetComponent<GojoDomainController>();
            bool gojoActive =
                gojoDomain != null
                && gojoDomain.enabled
                && gojoDomain.State == GojoDomainController.DomainState.Active;
            if (gojoActive && !gojoDomainWasActive)
            {
                PlayFeedback(
                    new Color(0.42f, 0.62f, 1f),
                    0.13f,
                    0.22f,
                    0.20f,
                    0.18f,
                    0.045f,
                    0.12f
                );
            }
            gojoDomainWasActive = gojoActive;

            sukunaDomain ??= GetComponent<SukunaDomainController>();
            bool sukunaActive =
                sukunaDomain != null
                && sukunaDomain.enabled
                && sukunaDomain.IsActive;
            if (sukunaActive && !sukunaDomainWasActive)
            {
                PlayFeedback(
                    new Color(0.92f, 0.08f, 0.035f),
                    0.15f,
                    0.24f,
                    0.34f,
                    0.22f,
                    0.055f,
                    0.10f
                );
            }
            sukunaDomainWasActive = sukunaActive;
        }

        private int CountLivingActiveOpponents()
        {
            Health[] healthObjects = FindObjectsByType<Health>(FindObjectsSortMode.None);
            int count = 0;
            foreach (Health health in healthObjects)
            {
                if (
                    health == null
                    || health == ownHealth
                    || health.IsDead
                    || !health.gameObject.activeInHierarchy
                )
                {
                    continue;
                }

                count += 1;
            }
            return count;
        }

        private void PlayFeedback(
            Color flashColor,
            float flashAlpha,
            float flashDuration,
            float shakeAmplitude,
            float shakeDuration,
            float hitStopDuration,
            float hitStopRelativeScale
        )
        {
            combatCamera ??= FindFirstObjectByType<SimpleCameraFollow>();
            if (combatCamera != null)
            {
                combatCamera.Flash(flashColor, flashAlpha, flashDuration);
                combatCamera.AddShake(shakeAmplitude, shakeDuration);
            }

            if (hitStopDuration > 0f)
            {
                PrototypeHitStopController.Request(hitStopDuration, hitStopRelativeScale);
            }
        }
    }
}
