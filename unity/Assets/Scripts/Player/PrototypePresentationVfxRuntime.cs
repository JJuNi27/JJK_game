using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    /// <summary>
    /// Manual fallback/reference adapter for the old procedural ring implementation.
    /// It is no longer runtime-bootstrapped; CombatMVP registers the scene-owned
    /// ProductionParticleVfxRuntime instead.
    /// </summary>
    [DefaultExecutionOrder(1450)]
    [DisallowMultipleComponent]
    public sealed class PrototypePresentationVfxRuntime : MonoBehaviour, IPresentationVfxRuntime
    {
        private void Awake()
        {
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
        }

        public PresentationVfxHandle Spawn(PresentationVfxSpawnRequest request)
        {
            PrototypeSignatureSpatialVfx effect = PrototypeSignatureSpatialVfx.Spawn(request);
            return effect != null ? new PresentationVfxHandle(effect) : default;
        }
    }
}
