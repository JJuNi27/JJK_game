using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [DefaultExecutionOrder(1650)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BasicAttack))]
    public sealed class ProductionSignatureVfxDirector : MonoBehaviour
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
                    || attack.GetComponent<ProductionSignatureVfxDirector>() != null
                )
                {
                    continue;
                }

                attack.gameObject.AddComponent<ProductionSignatureVfxDirector>();
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
                case TechniquePresentationId.Fuga:
                    HandleFuga(request);
                    break;
                case TechniquePresentationId.MalevolentShrine:
                    HandleMalevolentShrine(request);
                    break;
                case TechniquePresentationId.DivineDog:
                    HandleDivineDog(request);
                    break;
                case TechniquePresentationId.Nue:
                    HandleNue(request);
                    break;
            }
        }

        private void HandleFuga(TechniquePresentationRequest request)
        {
            bool domainAmplified = request.Amplified;
            if (request.Phase == TechniquePresentationPhase.Anticipation)
            {
                SpawnFollowAura(
                    ownHealth.transform,
                    new Vector3(0f, 1.0f, 0.55f),
                    new Color(0.58f, 0.01f, 0.005f, 0.92f),
                    new Color(1f, 0.18f, 0.015f, 0.82f),
                    0.18f,
                    domainAmplified ? 2.8f : 2.2f,
                    0.56f,
                    90f,
                    PresentationVfxStyleId.FugaCharge,
                    request.Direction,
                    domainAmplified
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
                    185f,
                    PresentationVfxStyleId.MalevolentShrineAnticipation,
                    request.Direction
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
                    240f,
                    PresentationVfxStyleId.MalevolentShrineActive,
                    request.Direction
                );
            }
        }

        private void HandleDivineDog(TechniquePresentationRequest request)
        {
            if (request.Phase == TechniquePresentationPhase.Release)
            {
                SpawnWorldBurst(
                    ResolveOrigin(request),
                    new Color(0.06f, 0.32f, 0.34f, 0.96f),
                    new Color(0.30f, 0.82f, 0.78f, 0.90f),
                    0.20f,
                    2.2f,
                    0.30f,
                    180f,
                    PresentationVfxStyleId.DivineDogRelease,
                    request.Direction
                );
                return;
            }

            if (request.Phase == TechniquePresentationPhase.Impact)
            {
                SpawnWorldBurst(
                    ResolveOrigin(request),
                    new Color(0.12f, 0.62f, 0.60f, 0.98f),
                    new Color(0.72f, 1f, 0.92f, 0.90f),
                    0.14f,
                    1.5f,
                    0.20f,
                    220f,
                    PresentationVfxStyleId.DivineDogImpact,
                    request.Direction
                );
            }
        }

        private void HandleNue(TechniquePresentationRequest request)
        {
            if (request.Phase == TechniquePresentationPhase.Release)
            {
                SpawnWorldBurst(
                    ResolveOrigin(request),
                    new Color(0.12f, 0.34f, 0.72f, 0.96f),
                    new Color(0.48f, 0.88f, 1f, 0.92f),
                    0.24f,
                    2.8f,
                    0.34f,
                    250f,
                    PresentationVfxStyleId.NueRelease,
                    request.Direction
                );
                return;
            }

            if (request.Phase == TechniquePresentationPhase.Impact)
            {
                SpawnWorldBurst(
                    ResolveOrigin(request),
                    new Color(0.20f, 0.62f, 1f, 0.98f),
                    new Color(0.86f, 0.96f, 1f, 0.96f),
                    0.18f,
                    2.6f,
                    0.26f,
                    340f,
                    PresentationVfxStyleId.NueImpact,
                    request.Direction
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
            float spinSpeed,
            PresentationVfxStyleId styleId,
            Vector3 direction,
            bool amplified = false
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
                    PresentationVfxTimePolicy.Unscaled,
                    styleId,
                    direction,
                    amplified
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
            float spinSpeed,
            PresentationVfxStyleId styleId,
            Vector3 direction,
            bool amplified = false
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
                    PresentationVfxTimePolicy.Unscaled,
                    styleId,
                    direction,
                    amplified
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
