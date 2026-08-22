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
                TechniquePresentationRequests.Raise(
                    TechniquePresentationRequest.AtPose(
                        ownHealth,
                        TechniquePresentationId.UnlimitedVoid,
                        TechniquePresentationPhase.Anticipation,
                        transform.position,
                        transform.forward
                    )
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
                TechniquePresentationRequests.Raise(
                    TechniquePresentationRequest.AtPose(
                        ownHealth,
                        TechniquePresentationId.MalevolentShrine,
                        TechniquePresentationPhase.Anticipation,
                        transform.position,
                        transform.forward
                    )
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

            TechniquePresentationRequests.Raise(
                TechniquePresentationRequest.AtPose(
                    ownHealth,
                    TechniquePresentationId.Fuga,
                    TechniquePresentationPhase.Anticipation,
                    transform.position,
                    transform.forward,
                    domainActive
                )
            );
        }

        private void DetectPurpleRelease()
        {
            Transform purpleRoot = transform.Find(PurpleVisualName);
            bool purpleActive = purpleRoot != null && purpleRoot.gameObject.activeInHierarchy;
            if (purpleActive && !purpleWasActive)
            {
                Vector3 releaseOrigin =
                    transform.position + Vector3.up * 1.05f + transform.forward * 1.05f;
                TechniquePresentationRequests.Raise(
                    TechniquePresentationRequest.AtPose(
                        ownHealth,
                        TechniquePresentationId.HollowPurple,
                        TechniquePresentationPhase.Release,
                        releaseOrigin,
                        transform.forward
                    )
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
            TechniquePresentationRequests.Raise(
                TechniquePresentationRequest.AtOwner(
                    ownHealth,
                    TechniquePresentationId.HollowPurple,
                    TechniquePresentationPhase.Culmination
                )
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
            Vector3 direction = projectile.transform.forward.sqrMagnitude > 0.0001f
                ? projectile.transform.forward
                : transform.forward;
            TechniquePresentationRequests.Raise(
                TechniquePresentationRequest.AtPose(
                    ownHealth,
                    TechniquePresentationId.Fuga,
                    TechniquePresentationPhase.Release,
                    projectile.transform.position,
                    direction,
                    domainAmplified
                )
            );
        }

        private void HandleFugaExploded(Health projectileOwner, Vector3 worldPosition, bool domainAmplified)
        {
            if (projectileOwner == null || ownHealth == null || projectileOwner != ownHealth)
            {
                return;
            }

            TechniquePresentationRequests.Raise(
                TechniquePresentationRequest.AtPose(
                    ownHealth,
                    TechniquePresentationId.Fuga,
                    TechniquePresentationPhase.Impact,
                    worldPosition,
                    transform.forward,
                    domainAmplified
                )
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
                TechniquePresentationRequests.Raise(
                    TechniquePresentationRequest.AtPose(
                        ownHealth,
                        TechniquePresentationId.UnlimitedVoid,
                        TechniquePresentationPhase.Active,
                        transform.position,
                        transform.forward
                    )
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
                Vector3 center = sukunaDomain.DomainCenter;
                if (center == Vector3.zero)
                {
                    center = transform.position;
                }

                TechniquePresentationRequests.Raise(
                    TechniquePresentationRequest.AtPose(
                        ownHealth,
                        TechniquePresentationId.MalevolentShrine,
                        TechniquePresentationPhase.Active,
                        center,
                        transform.forward
                    )
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
    }
}
