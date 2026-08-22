using System;
using UnityEngine;

namespace JJKGame.Core
{
    public enum TechniquePresentationId
    {
        HollowPurple,
        Fuga,
        UnlimitedVoid,
        MalevolentShrine,
        DivineDog,
        Nue,
    }

    public enum TechniquePresentationPhase
    {
        Anticipation,
        Release,
        Culmination,
        Impact,
        Active,
        End,
    }

    /// <summary>
    /// Gate 4C semantic request emitted by gameplay-adjacent detection code.
    /// It describes what happened and where it happened, not how camera/VFX/audio
    /// should render it. Consumers are responsible for technique-specific offsets
    /// and presentation tuning.
    /// </summary>
    public readonly struct TechniquePresentationRequest
    {
        public TechniquePresentationRequest(
            Health owner,
            TechniquePresentationId techniqueId,
            TechniquePresentationPhase phase,
            Vector3 worldPoint,
            bool hasWorldPoint,
            Vector3 direction,
            bool hasDirection,
            bool amplified
        )
        {
            Owner = owner;
            TechniqueId = techniqueId;
            Phase = phase;
            WorldPoint = worldPoint;
            HasWorldPoint = hasWorldPoint;
            Direction = direction;
            HasDirection = hasDirection;
            Amplified = amplified;
        }

        public Health Owner { get; }
        public TechniquePresentationId TechniqueId { get; }
        public TechniquePresentationPhase Phase { get; }

        /// <summary>
        /// Semantic event origin. This is intentionally not a precomputed camera focus point.
        /// </summary>
        public Vector3 WorldPoint { get; }
        public bool HasWorldPoint { get; }

        /// <summary>
        /// Optional event facing/travel direction. Presentation consumers may use it for
        /// forward offsets, projectile-facing effects, or future asset orientation.
        /// </summary>
        public Vector3 Direction { get; }
        public bool HasDirection { get; }
        public bool Amplified { get; }

        public static TechniquePresentationRequest AtOwner(
            Health owner,
            TechniquePresentationId techniqueId,
            TechniquePresentationPhase phase,
            bool amplified = false
        )
        {
            return new TechniquePresentationRequest(
                owner,
                techniqueId,
                phase,
                owner != null ? owner.transform.position : Vector3.zero,
                owner != null,
                owner != null ? owner.transform.forward : Vector3.forward,
                owner != null,
                amplified
            );
        }

        public static TechniquePresentationRequest AtWorldPoint(
            Health owner,
            TechniquePresentationId techniqueId,
            TechniquePresentationPhase phase,
            Vector3 worldPoint,
            bool amplified = false
        )
        {
            return new TechniquePresentationRequest(
                owner,
                techniqueId,
                phase,
                worldPoint,
                true,
                Vector3.zero,
                false,
                amplified
            );
        }

        public static TechniquePresentationRequest AtPose(
            Health owner,
            TechniquePresentationId techniqueId,
            TechniquePresentationPhase phase,
            Vector3 worldPoint,
            Vector3 direction,
            bool amplified = false
        )
        {
            Vector3 normalizedDirection = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : Vector3.forward;

            return new TechniquePresentationRequest(
                owner,
                techniqueId,
                phase,
                worldPoint,
                true,
                normalizedDirection,
                true,
                amplified
            );
        }
    }

    /// <summary>
    /// Presentation-only event boundary. Raising a request must never change combat state.
    /// Consumers may translate semantic requests into camera/VFX/audio presentation.
    /// </summary>
    public static class TechniquePresentationRequests
    {
        public static event Action<TechniquePresentationRequest> Requested;

        public static void Raise(TechniquePresentationRequest request)
        {
            Requested?.Invoke(request);
        }
    }
}
