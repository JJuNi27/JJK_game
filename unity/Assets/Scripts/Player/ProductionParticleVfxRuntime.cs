using JJKGame.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.Player
{
    /// <summary>
    /// CombatMVP-only ParticleSystem implementation of the Gate 4 VFX runtime boundary.
    /// The host and every spawned effect are scene-owned so reloads cannot retain
    /// registrations, particles, or runtime materials.
    /// </summary>
    [DefaultExecutionOrder(1450)]
    [DisallowMultipleComponent]
    public sealed class ProductionParticleVfxRuntime : MonoBehaviour, IPresentationVfxRuntime
    {
        private const string TargetSceneName = "CombatMVP";
        private static ProductionParticleVfxRuntime activeInstance;

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
                || FindFirstObjectByType<ProductionParticleVfxRuntime>() != null
            )
            {
                return;
            }

            new GameObject("ProductionParticleVfxRuntime")
                .AddComponent<ProductionParticleVfxRuntime>();
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
            PresentationVfxRuntime.Register(this);
        }

        private void OnEnable()
        {
            if (activeInstance == this)
            {
                PresentationVfxRuntime.Register(this);
            }
        }

        private void OnDisable()
        {
            PresentationVfxRuntime.Unregister(this);
        }

        private void OnDestroy()
        {
            PresentationVfxRuntime.Unregister(this);
            if (activeInstance == this)
            {
                activeInstance = null;
            }
        }

        public PresentationVfxHandle Spawn(PresentationVfxSpawnRequest request)
        {
            ProductionParticleVfxInstance instance =
                ProductionParticleVfxInstance.Spawn(request, transform);
            return instance != null ? new PresentationVfxHandle(instance) : default;
        }
    }
}
