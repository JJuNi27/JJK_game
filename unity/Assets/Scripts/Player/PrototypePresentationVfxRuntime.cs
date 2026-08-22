using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    /// <summary>
    /// Gate 4D adapter from the renderer-agnostic PresentationVfxRuntime contract
    /// to the current procedural PrototypeSignatureSpatialVfx implementation.
    /// A prefab/Particle/VFX Graph runtime can replace this adapter later without
    /// changing technique request producers or presentation consumers.
    /// </summary>
    [DefaultExecutionOrder(1450)]
    [DisallowMultipleComponent]
    public sealed class PrototypePresentationVfxRuntime : MonoBehaviour, IPresentationVfxRuntime
    {
        private static PrototypePresentationVfxRuntime instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapAfterSceneLoad()
        {
            if (instance != null)
            {
                return;
            }

            PrototypePresentationVfxRuntime existing = FindFirstObjectByType<PrototypePresentationVfxRuntime>();
            if (existing != null)
            {
                instance = existing;
                PresentationVfxRuntime.Register(existing);
                return;
            }

            GameObject host = new GameObject("PrototypePresentationVfxRuntime");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<PrototypePresentationVfxRuntime>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            PresentationVfxRuntime.Register(this);
        }

        private void OnEnable()
        {
            PresentationVfxRuntime.Register(this);
        }

        private void OnDisable()
        {
            PresentationVfxRuntime.Unregister(this);
        }

        private void OnDestroy()
        {
            PresentationVfxRuntime.Unregister(this);
            if (instance == this)
            {
                instance = null;
            }
        }

        public PresentationVfxHandle Spawn(PresentationVfxSpawnRequest request)
        {
            PrototypeSignatureSpatialVfx effect = PrototypeSignatureSpatialVfx.Spawn(request);
            return effect != null ? new PresentationVfxHandle(effect) : default;
        }
    }
}
