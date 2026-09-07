using System;
using UnityEngine;

namespace JJKGame.Core
{
    [Serializable]
    public sealed class TechniqueAnimationBinding
    {
        [Tooltip("Leave off until an authored clip and its release cue are verified. Timer always remains a fallback.")]
        public bool acceptPresentationRelease;
        public string animationTrigger = "";
        public string animationState = "";
    }

    public interface ITechniqueReleaseReceiver
    {
        bool RequestPresentationRelease(int castToken);
    }

    public enum TechniqueChoreographyPhase { Began, Released, Cancelled }

    /// <summary>
    /// Anticipation binding for future animation/voice/VFX adapters. This is separate from
    /// existing effect presentation requests, which continue to describe real gameplay effects.
    /// An adapter captures CastToken at Began; never fetch a fresh token in a late animation event.
    /// </summary>
    public readonly struct TechniqueChoreographyCue
    {
        public TechniqueChoreographyCue(Health owner, TechniquePresentationId technique,
            int castToken, float fallbackDuration, TechniqueAnimationBinding animation,
            TechniqueChoreographyPhase phase, ITechniqueReleaseReceiver receiver)
        {
            Owner = owner;
            Technique = technique;
            CastToken = castToken;
            FallbackDuration = fallbackDuration;
            Animation = animation;
            Phase = phase;
            Receiver = receiver;
        }
        public Health Owner { get; }
        public TechniquePresentationId Technique { get; }
        public int CastToken { get; }
        public float FallbackDuration { get; }
        public TechniqueAnimationBinding Animation { get; }
        public TechniqueChoreographyPhase Phase { get; }
        public ITechniqueReleaseReceiver Receiver { get; }
    }

    public static class TechniqueChoreographyCues
    {
        public static event Action<TechniqueChoreographyCue> Raised;
        public static void Raise(TechniqueChoreographyCue cue) => Raised?.Invoke(cue);
    }
}
