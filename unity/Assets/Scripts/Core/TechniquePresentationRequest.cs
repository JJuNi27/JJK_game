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
    /// It describes what happened, not how camera/VFX/audio should render it.
    /// </summary>
    public readonly struct TechniquePresentationRequest
    {
        public TechniquePresentationRequest(
            Health owner,
            TechniquePresentationId techniqueId,
            TechniquePresentationPhase phase,
            Vector3 worldPoint,
            bool hasWorldPoint,
            bool amplified
        )
        {
            Owner = owner;
            TechniqueId = techniqueId;
            Phase = phase;
            WorldPoint = worldPoint;
            HasWorldPoint = hasWorldPoint;
            Amplified = amplified;
        }

        public Health Owner { get; }
        public TechniquePresentationId TechniqueId { get; }
        public TechniquePresentationPhase Phase { get; }
        public Vector3 WorldPoint { get; }
        public bool HasWorldPoint { get; }
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
                Vector3.zero,
                false,
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
