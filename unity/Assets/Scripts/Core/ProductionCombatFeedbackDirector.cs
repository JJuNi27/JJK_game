using JJKGame.CameraSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.Core
{
    /// <summary>
    /// CombatMVP-only production-facing consumer for semantic technique and confirmed
    /// basic-hit presentation requests. Gameplay producers provide identity/origin only;
    /// this scene-owned director owns camera, flash, and hit-stop tuning.
    /// </summary>
    [DefaultExecutionOrder(1550)]
    [DisallowMultipleComponent]
    public sealed class ProductionCombatFeedbackDirector : MonoBehaviour
    {
        private const string TargetSceneName = "CombatMVP";

        private static ProductionCombatFeedbackDirector activeInstance;
        private SimpleCameraFollow combatCamera;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            activeInstance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            InstallForCurrentScene();
        }

        private static void HandleSceneLoaded(Scene _, LoadSceneMode __)
        {
            InstallForCurrentScene();
        }

        private static void InstallForCurrentScene()
        {
            if (
                SceneManager.GetActiveScene().name != TargetSceneName
                || FindFirstObjectByType<ProductionCombatFeedbackDirector>() != null
            )
            {
                return;
            }

            GameObject host = new GameObject("ProductionCombatFeedbackDirector");
            host.AddComponent<ProductionCombatFeedbackDirector>();
        }

        private void Awake()
        {
            if (
                SceneManager.GetActiveScene().name != TargetSceneName
                || (activeInstance != null && activeInstance != this)
            )
            {
                enabled = false;
                Destroy(gameObject);
                return;
            }

            activeInstance = this;
        }

        private void OnEnable()
        {
            if (SceneManager.GetActiveScene().name != TargetSceneName)
            {
                return;
            }

            TechniquePresentationRequests.Requested -= HandleRequest;
            TechniquePresentationRequests.Requested += HandleRequest;
            BasicHitPresentationRequests.Requested -= HandleBasicHit;
            BasicHitPresentationRequests.Requested += HandleBasicHit;
        }

        private void OnDisable()
        {
            TechniquePresentationRequests.Requested -= HandleRequest;
            BasicHitPresentationRequests.Requested -= HandleBasicHit;
            combatCamera = null;
        }

        private void OnDestroy()
        {
            if (activeInstance == this)
            {
                activeInstance = null;
            }
        }

        private void HandleBasicHit(BasicHitPresentationRequest request)
        {
            switch (request.ChainStep)
            {
                case 1:
                    PlayFeedback(Color.white, 0.025f, 0.055f, 0.075f, 0.070f, 0.022f, 0.08f);
                    break;
                case 2:
                    PlayFeedback(Color.white, 0.040f, 0.070f, 0.110f, 0.085f, 0.030f, 0.08f);
                    break;
                default:
                    PlayFeedback(
                        new Color(1f, 0.82f, 0.52f),
                        0.075f,
                        0.095f,
                        0.220f,
                        0.130f,
                        0.055f,
                        0.08f
                    );
                    break;
            }
        }

        private void HandleRequest(TechniquePresentationRequest request)
        {
            switch (request.TechniqueId)
            {
                case TechniquePresentationId.HollowPurple:
                    HandleHollowPurple(request);
                    break;
                case TechniquePresentationId.Fuga:
                    HandleFuga(request);
                    break;
                case TechniquePresentationId.UnlimitedVoid:
                    HandleUnlimitedVoid(request);
                    break;
                case TechniquePresentationId.MalevolentShrine:
                    HandleMalevolentShrine(request);
                    break;
                case TechniquePresentationId.DivineDog:
                    HandleDivineDog(request);
                    break;
            }
        }

        private void HandleHollowPurple(TechniquePresentationRequest request)
        {
            if (request.Phase == TechniquePresentationPhase.Release)
            {
                PlayFeedback(
                    new Color(0.72f, 0.28f, 1f),
                    0.18f,
                    0.18f,
                    0.42f,
                    0.20f,
                    0.065f,
                    0.10f
                );
                PlayFovKick(6.5f, 0.26f);
                PlayWorldFocus(request, ResolveCameraFocusPoint(request), 0.34f, 0.42f);
                return;
            }

            if (request.Phase == TechniquePresentationPhase.Culmination)
            {
                PlayFeedback(
                    new Color(0.86f, 0.62f, 1f),
                    0.10f,
                    0.12f,
                    0.24f,
                    0.12f,
                    0.020f,
                    0.16f
                );
                PlayFovKick(3f, 0.15f);
            }
        }

        private void HandleFuga(TechniquePresentationRequest request)
        {
            bool amplified = request.Amplified;
            if (request.Phase == TechniquePresentationPhase.Anticipation)
            {
                PlayFeedback(
                    amplified ? new Color(0.84f, 0.035f, 0.015f) : new Color(0.78f, 0.14f, 0.02f),
                    amplified ? 0.13f : 0.10f,
                    0.16f,
                    amplified ? 0.15f : 0.12f,
                    0.13f,
                    0f,
                    1f
                );
                PlayFovKick(amplified ? -5f : -4f, 0.20f);
                PlayWorldFocus(
                    request,
                    ResolveCameraFocusPoint(request),
                    amplified ? 0.24f : 0.20f,
                    0.28f
                );
                return;
            }

            if (request.Phase == TechniquePresentationPhase.Release)
            {
                PlayFeedback(
                    amplified ? new Color(1f, 0.18f, 0.04f) : new Color(1f, 0.42f, 0.08f),
                    amplified ? 0.17f : 0.13f,
                    amplified ? 0.18f : 0.15f,
                    amplified ? 0.38f : 0.30f,
                    amplified ? 0.20f : 0.16f,
                    amplified ? 0.050f : 0.035f,
                    0.10f
                );
                PlayFovKick(amplified ? 5.5f : 4.5f, 0.20f);
                PlayWorldFocus(
                    request,
                    ResolveCameraFocusPoint(request),
                    amplified ? 0.30f : 0.25f,
                    0.32f
                );
                return;
            }

            if (request.Phase == TechniquePresentationPhase.Impact)
            {
                PlayFeedback(
                    amplified ? new Color(1f, 0.12f, 0.015f) : new Color(1f, 0.48f, 0.06f),
                    amplified ? 0.28f : 0.23f,
                    amplified ? 0.27f : 0.23f,
                    amplified ? 0.58f : 0.48f,
                    amplified ? 0.27f : 0.23f,
                    amplified ? 0.075f : 0.060f,
                    0.08f
                );
                PlayFovKick(amplified ? 9.5f : 8f, amplified ? 0.32f : 0.28f);
                PlayWorldFocus(
                    request,
                    ResolveCameraFocusPoint(request),
                    amplified ? 0.58f : 0.50f,
                    amplified ? 0.46f : 0.40f
                );
            }
        }

        private void HandleUnlimitedVoid(TechniquePresentationRequest request)
        {
            if (request.Phase == TechniquePresentationPhase.Anticipation)
            {
                PlayFeedback(
                    new Color(0.32f, 0.56f, 1f),
                    0.11f,
                    0.18f,
                    0.10f,
                    0.12f,
                    0f,
                    1f
                );
                PlayFovKick(-4.5f, 0.22f);
                PlayWorldFocus(request, ResolveCameraFocusPoint(request), 0.18f, 0.28f);
                return;
            }

            if (request.Phase == TechniquePresentationPhase.Active)
            {
                PlayFeedback(
                    new Color(0.42f, 0.62f, 1f),
                    0.21f,
                    0.26f,
                    0.62f,
                    0.28f,
                    0.065f,
                    0.10f
                );
                PlayFovKick(9f, 0.30f);
                PlayWorldFocus(request, ResolveCameraFocusPoint(request), 0.40f, 0.44f);
            }
        }

        private void HandleMalevolentShrine(TechniquePresentationRequest request)
        {
            if (request.Phase == TechniquePresentationPhase.Anticipation)
            {
                PlayFeedback(
                    new Color(0.64f, 0.025f, 0.015f),
                    0.12f,
                    0.20f,
                    0.13f,
                    0.15f,
                    0f,
                    1f
                );
                PlayFovKick(-5.5f, 0.24f);
                PlayWorldFocus(request, ResolveCameraFocusPoint(request), 0.20f, 0.30f);
                return;
            }

            if (request.Phase == TechniquePresentationPhase.Active)
            {
                PlayFeedback(
                    new Color(0.92f, 0.08f, 0.035f),
                    0.23f,
                    0.28f,
                    0.64f,
                    0.29f,
                    0.070f,
                    0.09f
                );
                PlayFovKick(9.5f, 0.31f);
                PlayWorldFocus(request, ResolveCameraFocusPoint(request), 0.44f, 0.46f);
            }
        }

        private void HandleDivineDog(TechniquePresentationRequest request)
        {
            if (request.Phase == TechniquePresentationPhase.Release)
            {
                PlayFeedback(
                    new Color(0.10f, 0.52f, 0.56f),
                    0.09f,
                    0.14f,
                    0.10f,
                    0.12f,
                    0f,
                    1f
                );
                PlayFovKick(2.5f, 0.16f);
                PlayWorldFocus(request, ResolveCameraFocusPoint(request), 0.18f, 0.24f);
                return;
            }

            if (request.Phase == TechniquePresentationPhase.Impact)
            {
                PlayFeedback(
                    new Color(0.42f, 0.86f, 0.82f),
                    0.055f,
                    0.09f,
                    0.26f,
                    0.13f,
                    0.025f,
                    0.12f
                );
                PlayFovKick(2f, 0.14f);
                PlayWorldFocus(request, ResolveCameraFocusPoint(request), 0.16f, 0.18f);
            }
        }

        private static Vector3 ResolveCameraFocusPoint(TechniquePresentationRequest request)
        {
            Vector3 origin = request.HasWorldPoint
                ? request.WorldPoint
                : request.Owner != null
                    ? request.Owner.transform.position
                    : Vector3.zero;
            Vector3 forward = request.HasDirection
                ? request.Direction
                : request.Owner != null
                    ? request.Owner.transform.forward
                    : Vector3.forward;

            if (forward.sqrMagnitude > 0.0001f)
            {
                forward.Normalize();
            }
            else
            {
                forward = Vector3.forward;
            }

            switch (request.TechniqueId)
            {
                case TechniquePresentationId.HollowPurple:
                    if (request.Phase == TechniquePresentationPhase.Release)
                    {
                        return origin + Vector3.up * 0.20f + forward * 6.45f;
                    }
                    break;

                case TechniquePresentationId.Fuga:
                    if (request.Phase == TechniquePresentationPhase.Anticipation)
                    {
                        return origin + Vector3.up * 1.25f + forward * 2.8f;
                    }
                    if (request.Phase == TechniquePresentationPhase.Release)
                    {
                        return origin + forward * 4.0f;
                    }
                    if (request.Phase == TechniquePresentationPhase.Impact)
                    {
                        return origin + Vector3.up * 0.70f;
                    }
                    break;

                case TechniquePresentationId.UnlimitedVoid:
                    if (request.Phase == TechniquePresentationPhase.Anticipation)
                    {
                        return origin + Vector3.up * 2.1f;
                    }
                    if (request.Phase == TechniquePresentationPhase.Active)
                    {
                        return origin + Vector3.up * 3.0f;
                    }
                    break;

                case TechniquePresentationId.MalevolentShrine:
                    if (request.Phase == TechniquePresentationPhase.Anticipation)
                    {
                        return origin + Vector3.up * 2.3f;
                    }
                    if (request.Phase == TechniquePresentationPhase.Active)
                    {
                        return origin + Vector3.up * 3.2f;
                    }
                    break;

                case TechniquePresentationId.DivineDog:
                    if (request.Phase == TechniquePresentationPhase.Release)
                    {
                        return origin + Vector3.up * 0.35f + forward * 1.15f;
                    }
                    if (request.Phase == TechniquePresentationPhase.Impact)
                    {
                        return origin + Vector3.up * 0.25f;
                    }
                    break;
            }

            return origin;
        }

        private SimpleCameraFollow GetCombatCamera()
        {
            if (combatCamera == null)
            {
                combatCamera = FindFirstObjectByType<SimpleCameraFollow>();
            }
            return combatCamera;
        }

        private void PlayFovKick(float delta, float duration)
        {
            SimpleCameraFollow camera = GetCombatCamera();
            if (camera != null)
            {
                camera.AddFovKick(delta, duration);
            }
        }

        private void PlayWorldFocus(
            TechniquePresentationRequest request,
            Vector3 focusPoint,
            float strength,
            float duration
        )
        {
            if (!request.HasWorldPoint && request.Owner == null)
            {
                return;
            }

            SimpleCameraFollow camera = GetCombatCamera();
            if (camera != null)
            {
                camera.AddWorldFocus(focusPoint, strength, duration);
            }
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
            SimpleCameraFollow camera = GetCombatCamera();
            if (camera != null)
            {
                camera.Flash(flashColor, flashAlpha, flashDuration);
                camera.AddShake(shakeAmplitude, shakeDuration);
            }

            if (hitStopDuration > 0f)
            {
                PrototypeHitStopController.Request(hitStopDuration, hitStopRelativeScale);
            }
        }
    }
}
