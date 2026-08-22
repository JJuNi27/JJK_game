using System;
using JJKGame.Player;
using UnityEngine;

namespace JJKGame.Core
{
    public enum FighterAnimationCueKind
    {
        CharacterEntered,
        DodgeStarted,
        BasicAttackStarted,
        TechniquePhase,
        Knockout,
    }

    /// <summary>
    /// Gate 4E read-only animation state. It describes the current fighter state
    /// without exposing concrete movement/attack controller implementations to a
    /// presentation consumer such as an Animator adapter or the current prototype pose renderer.
    /// </summary>
    public readonly struct FighterAnimationStateSnapshot
    {
        public FighterAnimationStateSnapshot(
            bool isValid,
            PrototypeCharacterId characterId,
            float planarSpeed,
            bool isDodging,
            float dodgeProgress,
            Vector3 dodgeDirection,
            int basicAttackStep,
            CombatActionState actionState,
            bool isDead
        )
        {
            IsValid = isValid;
            CharacterId = characterId;
            PlanarSpeed = planarSpeed;
            IsDodging = isDodging;
            DodgeProgress = dodgeProgress;
            DodgeDirection = dodgeDirection;
            BasicAttackStep = basicAttackStep;
            ActionState = actionState;
            IsDead = isDead;
        }

        public bool IsValid { get; }
        public PrototypeCharacterId CharacterId { get; }
        public float PlanarSpeed { get; }
        public bool IsDodging { get; }
        public float DodgeProgress { get; }
        public Vector3 DodgeDirection { get; }
        public int BasicAttackStep { get; }
        public CombatActionState ActionState { get; }
        public bool IsDead { get; }
    }

    /// <summary>
    /// Discrete animation-facing cue. This is presentation-only and never changes gameplay.
    /// Technique cues reuse Gate 4C semantic technique id/phase instead of inventing a second
    /// technique vocabulary.
    /// </summary>
    public readonly struct FighterAnimationCue
    {
        public FighterAnimationCue(
            Health owner,
            PrototypeCharacterId characterId,
            FighterAnimationCueKind kind,
            int variant,
            bool hasTechnique,
            TechniquePresentationId techniqueId,
            TechniquePresentationPhase techniquePhase
        )
        {
            Owner = owner;
            CharacterId = characterId;
            Kind = kind;
            Variant = variant;
            HasTechnique = hasTechnique;
            TechniqueId = techniqueId;
            TechniquePhase = techniquePhase;
        }

        public Health Owner { get; }
        public PrototypeCharacterId CharacterId { get; }
        public FighterAnimationCueKind Kind { get; }
        public int Variant { get; }
        public bool HasTechnique { get; }
        public TechniquePresentationId TechniqueId { get; }
        public TechniquePresentationPhase TechniquePhase { get; }

        public static FighterAnimationCue Simple(
            Health owner,
            PrototypeCharacterId characterId,
            FighterAnimationCueKind kind,
            int variant = 0
        )
        {
            return new FighterAnimationCue(
                owner,
                characterId,
                kind,
                variant,
                false,
                default,
                default
            );
        }

        public static FighterAnimationCue Technique(
            Health owner,
            PrototypeCharacterId characterId,
            TechniquePresentationId techniqueId,
            TechniquePresentationPhase phase
        )
        {
            return new FighterAnimationCue(
                owner,
                characterId,
                FighterAnimationCueKind.TechniquePhase,
                0,
                true,
                techniqueId,
                phase
            );
        }
    }

    public static class FighterAnimationCues
    {
        public static event Action<FighterAnimationCue> Raised;

        public static void Raise(FighterAnimationCue cue)
        {
            Raised?.Invoke(cue);
        }
    }
}
