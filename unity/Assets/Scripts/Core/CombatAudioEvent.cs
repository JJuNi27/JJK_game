using System;

namespace JJKGame.Core
{
    public enum CombatAudioEventId
    {
        BasicSwing,
        BasicHit,
        Dodge,
        GojoBlueCast,
        GojoBlueImpact,
        GojoRedCast,
        GojoRedImpact,
        HollowPurple,
        UnlimitedVoid,
        MalevolentShrine,
        Fuga,
        PlayerHit,
        Victory,
        Defeat,
    }

    /// <summary>
    /// Gate 4F presentation-only audio request. Producers describe a semantic combat sound
    /// without knowing AudioClip, AudioSource, Resources paths, mixer routing, or fallback synthesis.
    /// </summary>
    public readonly struct CombatAudioEvent
    {
        public CombatAudioEvent(
            Health owner,
            CombatAudioEventId eventId,
            int variant = 0,
            bool amplified = false
        )
        {
            Owner = owner;
            EventId = eventId;
            Variant = variant;
            Amplified = amplified;
        }

        public Health Owner { get; }
        public CombatAudioEventId EventId { get; }
        public int Variant { get; }
        public bool Amplified { get; }

        public static CombatAudioEvent ForOwner(
            Health owner,
            CombatAudioEventId eventId,
            int variant = 0,
            bool amplified = false
        )
        {
            return new CombatAudioEvent(owner, eventId, variant, amplified);
        }
    }

    public static class CombatAudioEvents
    {
        public static event Action<CombatAudioEvent> Raised;

        public static void Raise(CombatAudioEvent audioEvent)
        {
            Raised?.Invoke(audioEvent);
        }
    }
}
