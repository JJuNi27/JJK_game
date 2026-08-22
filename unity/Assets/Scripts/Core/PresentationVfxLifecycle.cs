using UnityEngine;

namespace JJKGame.Core
{
    public enum PresentationVfxTimePolicy
    {
        Scaled,
        Unscaled,
    }

    public enum PresentationVfxStopMode
    {
        FadeOut,
        Immediate,
    }

    /// <summary>
    /// Renderer-agnostic spawn description for short-lived combat presentation VFX.
    /// The request describes anchor/lifetime/tuning data, but not whether the concrete
    /// implementation is a procedural prototype, prefab, particle system, or VFX Graph.
    /// </summary>
    public readonly struct PresentationVfxSpawnRequest
    {
        private PresentationVfxSpawnRequest(
            Vector3 worldPosition,
            Transform followTarget,
            Vector3 followLocalOffset,
            Color primaryColor,
            Color secondaryColor,
            float startRadius,
            float endRadius,
            float duration,
            float spinSpeed,
            PresentationVfxTimePolicy timePolicy
        )
        {
            WorldPosition = worldPosition;
            FollowTarget = followTarget;
            FollowLocalOffset = followLocalOffset;
            PrimaryColor = primaryColor;
            SecondaryColor = secondaryColor;
            StartRadius = startRadius;
            EndRadius = endRadius;
            Duration = duration;
            SpinSpeed = spinSpeed;
            TimePolicy = timePolicy;
        }

        public Vector3 WorldPosition { get; }
        public Transform FollowTarget { get; }
        public Vector3 FollowLocalOffset { get; }
        public bool FollowsTarget => FollowTarget != null;
        public Color PrimaryColor { get; }
        public Color SecondaryColor { get; }
        public float StartRadius { get; }
        public float EndRadius { get; }
        public float Duration { get; }
        public float SpinSpeed { get; }
        public PresentationVfxTimePolicy TimePolicy { get; }

        public static PresentationVfxSpawnRequest AtWorld(
            Vector3 worldPosition,
            Color primaryColor,
            Color secondaryColor,
            float startRadius,
            float endRadius,
            float duration,
            float spinSpeed,
            PresentationVfxTimePolicy timePolicy = PresentationVfxTimePolicy.Unscaled
        )
        {
            return new PresentationVfxSpawnRequest(
                worldPosition,
                null,
                Vector3.zero,
                primaryColor,
                secondaryColor,
                startRadius,
                endRadius,
                duration,
                spinSpeed,
                timePolicy
            );
        }

        public static PresentationVfxSpawnRequest Follow(
            Transform target,
            Vector3 localOffset,
            Color primaryColor,
            Color secondaryColor,
            float startRadius,
            float endRadius,
            float duration,
            float spinSpeed,
            PresentationVfxTimePolicy timePolicy = PresentationVfxTimePolicy.Unscaled
        )
        {
            return new PresentationVfxSpawnRequest(
                Vector3.zero,
                target,
                localOffset,
                primaryColor,
                secondaryColor,
                startRadius,
                endRadius,
                duration,
                spinSpeed,
                timePolicy
            );
        }
    }

    public interface IPresentationVfxInstance
    {
        bool IsAlive { get; }
        void Stop(PresentationVfxStopMode mode = PresentationVfxStopMode.FadeOut);
    }

    public readonly struct PresentationVfxHandle
    {
        private readonly IPresentationVfxInstance instance;

        public PresentationVfxHandle(IPresentationVfxInstance instance)
        {
            this.instance = instance;
        }

        public bool IsValid => instance != null;
        public bool IsAlive => instance != null && instance.IsAlive;

        public void Stop(PresentationVfxStopMode mode = PresentationVfxStopMode.FadeOut)
        {
            instance?.Stop(mode);
        }
    }

    public interface IPresentationVfxRuntime
    {
        PresentationVfxHandle Spawn(PresentationVfxSpawnRequest request);
    }

    /// <summary>
    /// Gate 4D runtime boundary. Presentation consumers ask this gateway to spawn VFX;
    /// the registered runtime decides which concrete rendering implementation to use.
    /// </summary>
    public static class PresentationVfxRuntime
    {
        private static IPresentationVfxRuntime runtime;

        public static bool HasRuntime => runtime != null;

        public static void Register(IPresentationVfxRuntime nextRuntime)
        {
            runtime = nextRuntime;
        }

        public static void Unregister(IPresentationVfxRuntime currentRuntime)
        {
            if (ReferenceEquals(runtime, currentRuntime))
            {
                runtime = null;
            }
        }

        public static PresentationVfxHandle Spawn(PresentationVfxSpawnRequest request)
        {
            return runtime != null ? runtime.Spawn(request) : default;
        }
    }
}
