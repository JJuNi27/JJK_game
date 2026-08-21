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

        private GojoDomainController gojoDomain;
        private SukunaDomainController sukunaDomain;
        private SimpleCameraFollow combatCamera;

        private bool purpleWasActive;
        private bool gojoDomainWasActive;
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
            gojoDomain = GetComponent<GojoDomainController>();
            sukunaDomain = GetComponent<SukunaDomainController>();
            combatCamera = FindFirstObjectByType<SimpleCameraFollow>();
        }

        private void Update()
        {
            DetectPurpleRelease();
            DetectFugaRelease();
            DetectDomainActivation();
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
            }

            purpleWasActive = purpleActive;
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

            PrototypeHitStopController.Request(hitStopDuration, hitStopRelativeScale);
        }
    }
}
