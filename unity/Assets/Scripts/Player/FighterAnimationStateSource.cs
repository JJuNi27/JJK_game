using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    /// <summary>
    /// Gate 4E production-candidate read-only bridge from gameplay state to animation presentation.
    /// It owns no movement, attack, tag, technique, or damage state.
    /// </summary>
    [DefaultExecutionOrder(1400)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(BasicAttack))]
    public sealed class FighterAnimationStateSource : MonoBehaviour
    {
        private Health health;
        private BasicAttack basicAttack;
        private ThirdPersonPlayerController movement;
        private CharacterController motor;
        private PrototypeCharacterController characterController;
        private CombatActionGate actionGate;

        private bool hasObservedCharacter;
        private PrototypeCharacterId previousCharacter;
        private bool previousDodging;
        private int previousAttackStep;

        public static FighterAnimationStateSource GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            FighterAnimationStateSource source = owner.GetComponent<FighterAnimationStateSource>();
            return source != null ? source : owner.AddComponent<FighterAnimationStateSource>();
        }

        public FighterAnimationStateSnapshot Snapshot
        {
            get
            {
                RefreshReferences();
                if (basicAttack == null || characterController == null)
                {
                    return default;
                }

                float planarSpeed = 0f;
                if (motor != null)
                {
                    Vector3 velocity = motor.velocity;
                    velocity.y = 0f;
                    planarSpeed = velocity.magnitude;
                }

                CombatActionState state = actionGate != null
                    ? actionGate.CurrentState
                    : CombatActionState.Normal;

                return new FighterAnimationStateSnapshot(
                    true,
                    characterController.ActiveCharacter,
                    planarSpeed,
                    movement != null && movement.IsDodging,
                    movement != null ? movement.DodgeProgress : 0f,
                    movement != null ? movement.DodgeDirection : Vector3.zero,
                    basicAttack.DisplayChainStep,
                    state,
                    health != null && health.IsDead
                );
            }
        }

        private void Awake()
        {
            RefreshReferences();
        }

        private void OnEnable()
        {
            RefreshReferences();
            TechniquePresentationRequests.Requested -= HandleTechniquePresentation;
            TechniquePresentationRequests.Requested += HandleTechniquePresentation;

            if (health != null)
            {
                health.Died -= HandleDied;
                health.Died += HandleDied;
            }
        }

        private void OnDisable()
        {
            TechniquePresentationRequests.Requested -= HandleTechniquePresentation;
            if (health != null)
            {
                health.Died -= HandleDied;
            }
        }

        private void Update()
        {
            FighterAnimationStateSnapshot snapshot = Snapshot;
            if (!snapshot.IsValid)
            {
                return;
            }

            if (!hasObservedCharacter || snapshot.CharacterId != previousCharacter)
            {
                previousCharacter = snapshot.CharacterId;
                hasObservedCharacter = true;
                FighterAnimationCues.Raise(
                    FighterAnimationCue.Simple(
                        health,
                        snapshot.CharacterId,
                        FighterAnimationCueKind.CharacterEntered
                    )
                );
            }

            if (snapshot.IsDodging && !previousDodging)
            {
                FighterAnimationCues.Raise(
                    FighterAnimationCue.Simple(
                        health,
                        snapshot.CharacterId,
                        FighterAnimationCueKind.DodgeStarted
                    )
                );
            }

            if (
                snapshot.BasicAttackStep > 0
                && snapshot.BasicAttackStep != previousAttackStep
            )
            {
                FighterAnimationCues.Raise(
                    FighterAnimationCue.Simple(
                        health,
                        snapshot.CharacterId,
                        FighterAnimationCueKind.BasicAttackStarted,
                        snapshot.BasicAttackStep
                    )
                );
            }

            previousDodging = snapshot.IsDodging;
            previousAttackStep = snapshot.BasicAttackStep;
        }

        private void HandleTechniquePresentation(TechniquePresentationRequest request)
        {
            if (health == null || request.Owner == null || request.Owner != health)
            {
                return;
            }

            RefreshReferences();
            PrototypeCharacterId characterId = characterController != null
                ? characterController.ActiveCharacter
                : PrototypeCharacterId.GojoModern;

            FighterAnimationCues.Raise(
                FighterAnimationCue.Technique(
                    health,
                    characterId,
                    request.TechniqueId,
                    request.Phase
                )
            );
        }

        private void HandleDied(Health deadHealth)
        {
            if (deadHealth == null || deadHealth != health)
            {
                return;
            }

            RefreshReferences();
            PrototypeCharacterId characterId = characterController != null
                ? characterController.ActiveCharacter
                : PrototypeCharacterId.GojoModern;
            FighterAnimationCues.Raise(
                FighterAnimationCue.Simple(
                    health,
                    characterId,
                    FighterAnimationCueKind.Knockout
                )
            );
        }

        private void RefreshReferences()
        {
            health ??= GetComponent<Health>();
            basicAttack ??= GetComponent<BasicAttack>();
            movement ??= GetComponent<ThirdPersonPlayerController>();
            motor ??= GetComponent<CharacterController>();
            characterController ??= GetComponent<PrototypeCharacterController>();
            actionGate ??= GetComponent<CombatActionGate>();
        }
    }
}
