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
    /// Renderer-facing visual language only. These identifiers carry no damage,
    /// cooldown, hit timing, or other gameplay rules.
    /// </summary>
    public enum PresentationVfxStyleId
    {
        Generic,
        GojoBlue,
        GojoRed,
        HollowPurpleRelease,
        HollowPurpleFormation,
        FugaCharge,
        FugaRelease,
        FugaImpact,
        SukunaDismantle,
        SukunaCleave,
        UnlimitedVoidAnticipation,
        UnlimitedVoidActive,
        MalevolentShrineAnticipation,
        MalevolentShrineActive,
        DivineDogRelease,
        DivineDogImpact,
        NueRelease,
        NueImpact,
        BasicHit1,
        BasicHit2,
        BasicHitFinisher,
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
            PresentationVfxTimePolicy timePolicy,
            PresentationVfxStyleId styleId,
            Vector3 direction,
            bool hasDirection,
            bool amplified
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
            StyleId = styleId;
            Direction = direction;
            HasDirection = hasDirection;
            Amplified = amplified;
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
        public PresentationVfxStyleId StyleId { get; }
        public Vector3 Direction { get; }
        public bool HasDirection { get; }
        public bool Amplified { get; }

        public static PresentationVfxSpawnRequest AtWorld(
            Vector3 worldPosition,
            Color primaryColor,
            Color secondaryColor,
            float startRadius,
            float endRadius,
            float duration,
            float spinSpeed,
            PresentationVfxTimePolicy timePolicy = PresentationVfxTimePolicy.Unscaled,
            PresentationVfxStyleId styleId = PresentationVfxStyleId.Generic,
            Vector3 direction = default,
            bool amplified = false
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
                timePolicy,
                styleId,
                direction,
                direction.sqrMagnitude > 0.0001f,
                amplified
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
            PresentationVfxTimePolicy timePolicy = PresentationVfxTimePolicy.Unscaled,
            PresentationVfxStyleId styleId = PresentationVfxStyleId.Generic,
            Vector3 direction = default,
            bool amplified = false
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
                timePolicy,
                styleId,
                direction,
                direction.sqrMagnitude > 0.0001f,
                amplified
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
