using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [DefaultExecutionOrder(1650)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BasicAttack))]
    public sealed class PrototypeSignatureSpatialVfxController : MonoBehaviour
    {
        private Health ownHealth;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapAfterSceneLoad()
        {
            BasicAttack[] attacks = FindObjectsByType<BasicAttack>(FindObjectsSortMode.None);
            foreach (BasicAttack attack in attacks)
            {
                if (
                    attack == null
                    || attack.GetComponent<PrototypeSignatureSpatialVfxController>() != null
                )
                {
                    continue;
                }

                attack.gameObject.AddComponent<PrototypeSignatureSpatialVfxController>();
            }
        }

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
        }

        private void OnEnable()
        {
            TechniquePresentationRequests.Requested -= HandlePresentationRequest;
            TechniquePresentationRequests.Requested += HandlePresentationRequest;
        }

        private void OnDisable()
        {
            TechniquePresentationRequests.Requested -= HandlePresentationRequest;
        }

        private void HandlePresentationRequest(TechniquePresentationRequest request)
        {
            if (ownHealth == null || request.Owner == null || request.Owner != ownHealth)
            {
                return;
            }

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
            }
        }

        private void HandleHollowPurple(TechniquePresentationRequest request)
        {
            if (request.Phase != TechniquePresentationPhase.Release)
            {
                return;
            }

            SpawnWorldBurst(
                ResolveOrigin(request),
                new Color(0.62f, 0.08f, 1f, 0.98f),
                new Color(0.96f, 0.78f, 1f, 0.92f),
                0.30f,
                4.1f,
                0.42f,
                230f
            );
        }

        private void HandleFuga(TechniquePresentationRequest request)
        {
            bool domainAmplified = request.Amplified;
            if (request.Phase == TechniquePresentationPhase.Release)
            {
                SpawnWorldBurst(
                    ResolveOrigin(request),
                    domainAmplified
                        ? new Color(1f, 0.07f, 0.01f, 0.98f)
                        : new Color(1f, 0.25f, 0.015f, 0.98f),
                    domainAmplified
                        ? new Color(1f, 0.48f, 0.05f, 0.92f)
                        : new Color(1f, 0.72f, 0.10f, 0.90f),
                    0.28f,
                    domainAmplified ? 3.6f : 3.0f,
                    domainAmplified ? 0.38f : 0.34f,
                    domainAmplified ? 260f : 220f
                );
                return;
            }

            if (request.Phase == TechniquePresentationPhase.Impact)
            {
                SpawnWorldBurst(
                    ResolveOrigin(request),
                    domainAmplified
                        ? new Color(1f, 0.025f, 0.01f, 1f)
                        : new Color(1f, 0.18f, 0.01f, 0.99f),
                    domainAmplified
                        ? new Color(1f, 0.58f, 0.08f, 0.96f)
                        : new Color(1f, 0.86f, 0.22f, 0.94f),
                    0.65f,
                    domainAmplified ? 7.2f : 5.8f,
                    domainAmplified ? 0.60f : 0.52f,
                    domainAmplified ? 320f : 275f
                );
            }
        }

        private void HandleUnlimitedVoid(TechniquePresentationRequest request)
        {
            if (request.Phase == TechniquePresentationPhase.Anticipation)
            {
                SpawnFollowAura(
                    ownHealth.transform,
                    new Vector3(0f, 0.20f, 0f),
                    new Color(0.20f, 0.55f, 1f, 0.95f),
                    new Color(0.68f, 0.88f, 1f, 0.88f),
                    0.45f,
                    2.9f,
                    0.58f,
                    125f
                );
                return;
            }

            if (request.Phase == TechniquePresentationPhase.Active)
            {
                SpawnWorldBurst(
                    ResolveOrigin(request) + Vector3.up * 0.15f,
                    new Color(0.16f, 0.42f, 1f, 0.96f),
                    new Color(0.72f, 0.82f, 1f, 0.90f),
                    0.8f,
                    8.5f,
                    0.62f,
                    175f
                );
            }
        }

        private void HandleMalevolentShrine(TechniquePresentationRequest request)
        {
            if (request.Phase == TechniquePresentationPhase.Anticipation)
            {
                SpawnFollowAura(
                    ownHealth.transform,
                    new Vector3(0f, 0.18f, 0f),
                    new Color(0.72f, 0.025f, 0.015f, 0.96f),
                    new Color(1f, 0.30f, 0.07f, 0.88f),
                    0.55f,
                    3.4f,
                    0.62f,
                    185f
                );
                return;
            }

            if (request.Phase == TechniquePresentationPhase.Active)
            {
                SpawnWorldBurst(
                    ResolveOrigin(request) + Vector3.up * 0.12f,
                    new Color(0.72f, 0.015f, 0.01f, 0.98f),
                    new Color(1f, 0.24f, 0.04f, 0.92f),
                    0.9f,
                    9.4f,
                    0.68f,
                    240f
                );
            }
        }

        private static void SpawnWorldBurst(
            Vector3 worldPosition,
            Color primary,
            Color secondary,
            float startRadius,
            float endRadius,
            float duration,
            float spinSpeed
        )
        {
            PresentationVfxRuntime.Spawn(
                PresentationVfxSpawnRequest.AtWorld(
                    worldPosition,
                    primary,
                    secondary,
                    startRadius,
                    endRadius,
                    duration,
                    spinSpeed,
                    PresentationVfxTimePolicy.Unscaled
                )
            );
        }

        private static void SpawnFollowAura(
            Transform target,
            Vector3 localOffset,
            Color primary,
            Color secondary,
            float startRadius,
            float endRadius,
            float duration,
            float spinSpeed
        )
        {
            PresentationVfxRuntime.Spawn(
                PresentationVfxSpawnRequest.Follow(
                    target,
                    localOffset,
                    primary,
                    secondary,
                    startRadius,
                    endRadius,
                    duration,
                    spinSpeed,
                    PresentationVfxTimePolicy.Unscaled
                )
            );
        }

        private Vector3 ResolveOrigin(TechniquePresentationRequest request)
        {
            if (request.HasWorldPoint)
            {
                return request.WorldPoint;
            }

            return ownHealth != null ? ownHealth.transform.position : transform.position;
        }
    }
}
