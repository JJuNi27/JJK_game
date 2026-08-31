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
                case TechniquePresentationId.Nue:
                    HandleNue(request);
                    break;
            }
        }

        private void HandleHollowPurple(TechniquePresentationRequest request)
        {
            if (
                request.Phase != TechniquePresentationPhase.Release
                && request.Phase != TechniquePresentationPhase.Culmination
            )
            {
                return;
            }

            bool release = request.Phase == TechniquePresentationPhase.Release;
            Vector3 origin = ResolveOrigin(request);
            Vector3 direction = request.HasDirection && request.Direction.sqrMagnitude > 0.0001f
                ? request.Direction.normalized
                : request.Owner != null
                    ? request.Owner.transform.forward
                    : Vector3.forward;
            if (!release && request.Owner != null)
            {
                origin = request.Owner.transform.position + Vector3.up * 1.05f + direction * 1.10f;
            }

            SpawnWorldBurst(
                origin,
                new Color(0.62f, 0.08f, 1f, 0.98f),
                new Color(0.96f, 0.78f, 1f, 0.92f),
                0.30f,
                release ? 4.1f : 2.4f,
                release ? 1.02f : 0.28f,
                230f,
                release
                    ? PresentationVfxStyleId.HollowPurpleRelease
                    : PresentationVfxStyleId.HollowPurpleFormation,
                direction
            );
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
                    125f,
                    PresentationVfxStyleId.UnlimitedVoidAnticipation,
                    request.Direction
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
                    175f,
                    PresentationVfxStyleId.UnlimitedVoidActive,
                    request.Direction
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
