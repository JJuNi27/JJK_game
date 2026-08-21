using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [DefaultExecutionOrder(1650)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BasicAttack))]
    public sealed class PrototypeSignatureSpatialVfxController : MonoBehaviour
    {
        private const string PurpleVisualName = "HollowPurplePrototypeVisual";

        private Health ownHealth;
        private GojoDomainController gojoDomain;
        private SukunaDomainController sukunaDomain;

        private GojoDomainController.DomainState previousGojoState;
        private bool sukunaWasCasting;
        private bool gojoWasActive;
        private bool sukunaWasActive;
        private bool purpleWasActive;
        private int observedFugaProjectileId;

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
            gojoDomain = GetComponent<GojoDomainController>();
            sukunaDomain = GetComponent<SukunaDomainController>();

            previousGojoState = gojoDomain != null
                ? gojoDomain.State
                : GojoDomainController.DomainState.Normal;
            sukunaWasCasting = sukunaDomain != null && sukunaDomain.IsCasting;
            gojoWasActive =
                gojoDomain != null
                && gojoDomain.State == GojoDomainController.DomainState.Active;
            sukunaWasActive = sukunaDomain != null && sukunaDomain.IsActive;
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
            DetectDomainCastAura();
            DetectPurpleReleaseBurst();
            DetectFugaReleaseBurst();
            DetectDomainOpenBurst();
        }

        private void DetectDomainCastAura()
        {
            gojoDomain ??= GetComponent<GojoDomainController>();
            GojoDomainController.DomainState gojoState = gojoDomain != null
                ? gojoDomain.State
                : GojoDomainController.DomainState.Normal;
            if (
                gojoDomain != null
                && gojoDomain.enabled
                && gojoState == GojoDomainController.DomainState.DomainReady
                && previousGojoState != GojoDomainController.DomainState.DomainReady
            )
            {
                PrototypeSignatureSpatialVfx.SpawnFollowAura(
                    transform,
                    new Vector3(0f, 0.20f, 0f),
                    new Color(0.20f, 0.55f, 1f, 0.95f),
                    new Color(0.68f, 0.88f, 1f, 0.88f),
                    0.45f,
                    2.9f,
                    0.58f,
                    125f
                );
            }
            previousGojoState = gojoState;

            sukunaDomain ??= GetComponent<SukunaDomainController>();
            bool sukunaCasting =
                sukunaDomain != null
                && sukunaDomain.enabled
                && sukunaDomain.IsCasting;
            if (sukunaCasting && !sukunaWasCasting)
            {
                PrototypeSignatureSpatialVfx.SpawnFollowAura(
                    transform,
                    new Vector3(0f, 0.18f, 0f),
                    new Color(0.72f, 0.025f, 0.015f, 0.96f),
                    new Color(1f, 0.30f, 0.07f, 0.88f),
                    0.55f,
                    3.4f,
                    0.62f,
                    185f
                );
            }
            sukunaWasCasting = sukunaCasting;
        }

        private void DetectPurpleReleaseBurst()
        {
            Transform purpleRoot = transform.Find(PurpleVisualName);
            bool purpleActive = purpleRoot != null && purpleRoot.gameObject.activeInHierarchy;
            if (purpleActive && !purpleWasActive)
            {
                Vector3 burstPosition =
                    transform.position + Vector3.up * 1.05f + transform.forward * 1.05f;
                PrototypeSignatureSpatialVfx.SpawnWorldBurst(
                    burstPosition,
                    new Color(0.62f, 0.08f, 1f, 0.98f),
                    new Color(0.96f, 0.78f, 1f, 0.92f),
                    0.30f,
                    4.1f,
                    0.42f,
                    230f
                );
            }

            purpleWasActive = purpleActive;
        }

        private void DetectFugaReleaseBurst()
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
            PrototypeSignatureSpatialVfx.SpawnWorldBurst(
                projectile.transform.position,
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
        }

        private void HandleFugaExploded(
            Health projectileOwner,
            Vector3 worldPosition,
            bool domainAmplified
        )
        {
            if (projectileOwner == null || ownHealth == null || projectileOwner != ownHealth)
            {
                return;
            }

            PrototypeSignatureSpatialVfx.SpawnWorldBurst(
                worldPosition,
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

        private void DetectDomainOpenBurst()
        {
            gojoDomain ??= GetComponent<GojoDomainController>();
            bool gojoActive =
                gojoDomain != null
                && gojoDomain.enabled
                && gojoDomain.State == GojoDomainController.DomainState.Active;
            if (gojoActive && !gojoWasActive)
            {
                PrototypeSignatureSpatialVfx.SpawnWorldBurst(
                    transform.position + Vector3.up * 0.15f,
                    new Color(0.16f, 0.42f, 1f, 0.96f),
                    new Color(0.72f, 0.82f, 1f, 0.90f),
                    0.8f,
                    8.5f,
                    0.62f,
                    175f
                );
            }
            gojoWasActive = gojoActive;

            sukunaDomain ??= GetComponent<SukunaDomainController>();
            bool sukunaActive =
                sukunaDomain != null
                && sukunaDomain.enabled
                && sukunaDomain.IsActive;
            if (sukunaActive && !sukunaWasActive)
            {
                Vector3 center = sukunaDomain.DomainCenter;
                if (center == Vector3.zero)
                {
                    center = transform.position;
                }

                PrototypeSignatureSpatialVfx.SpawnWorldBurst(
                    center + Vector3.up * 0.12f,
                    new Color(0.72f, 0.015f, 0.01f, 0.98f),
                    new Color(1f, 0.24f, 0.04f, 0.92f),
                    0.9f,
                    9.4f,
                    0.68f,
                    240f
                );
            }
            sukunaWasActive = sukunaActive;
        }
    }
}
