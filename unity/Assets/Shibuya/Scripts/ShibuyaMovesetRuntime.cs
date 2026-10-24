using System;
using System.Collections.Generic;
using UnityEngine;

namespace JJKGame.Shibuya
{
    public sealed partial class ShibuyaFighter
    {
        [SerializeField] private ShibuyaMovesetDefinition moveset;
        private ShibuyaMovesetStep activeStep;
        private bool[] eventVisited, movementArrived, movementFailed;
        private HashSet<JJKGame.Core.Health>[] eventTargets;
        private float[] movementSpent, movementTurn;
        private int presentationEvent, resolvingEvent, sequenceToken;
        private float sequenceStartedAt;
        private ShibuyaFighter linkVictim, linkSource;
        private int linkToken;
        private float linkUntilSourceTime;
        private float distanceKnockbackRemaining, distanceKnockbackSpeed;
        private Vector3 distanceKnockbackDirection;
        private ShibuyaFinisher bufferedFinisher;
        public ShibuyaFinisher CurrentFinisher { get; private set; }
        public ShibuyaFinisher BufferedFinisher => HasBufferedAttack ? bufferedFinisher : ShibuyaFinisher.Default;

        public event Action<int> HitEventStarted;
        public ShibuyaMovesetDefinition Moveset => moveset;
        public float AttackDuration => activeStep == null ? 0f : activeStep.Duration;
        public float PresentationElapsed => attacking ? Mathf.Max(0f, AttackElapsed - activeStep.hits[presentationEvent].startTime) : 0f;
        private float AttackActiveEnd => activeStep.ActiveEnd;
        private int StepCount => moveset == null ? profile.strikes.Length : moveset.steps.Length;
        private float ReservationWindow => Mathf.Max(profile.inputBufferTime,
            profile.confirmedLinkEnabled ? profile.confirmedLinkInputWindow : 0f);
        private bool IsStunned => combatTime < stunnedUntil || HasConfirmedLink;
        public bool HasConfirmedLink => linkSource != null && linkSource.isActiveAndEnabled
            && !linkSource.Health.IsDead && !linkSource.stopped && linkSource.attacking
            && linkSource.sequenceToken == linkToken && linkSource.combatTime < linkUntilSourceTime
            && linkSource.combatTime - linkSource.sequenceStartedAt < linkSource.profile.confirmedLinkMaxSequence
            && !Health.IsInvulnerable && !Health.IsDead && !stopped;

        // Runtime assignment is opt-in; no scene, default profile or character asset migration.
        public bool SetMoveset(ShibuyaMovesetDefinition value)
        {
            if (value != null && !value.IsValid) return false;
            InterruptAerial(); CancelCombo(); moveset = value; return true;
        }

        private ShibuyaMovesetStep ResolveStep(int index, ShibuyaFinisher selection, out ShibuyaFinisher resolved)
        {
            resolved = ShibuyaFinisher.Default;
            var set = moveset == null ? profile.finishers : moveset.finishers;
            int finalIndex = moveset == null ? profile.strikes.Length - 1 : moveset.finisherStepIndex;
            if (index == finalIndex && set != null && set.IsValid && selection != ShibuyaFinisher.Default) {
                resolved = selection;
                return selection == ShibuyaFinisher.Uppercut ? set.uppercut : set.downFinisher;
            }
            return moveset == null ? new ShibuyaMovesetStep {
                hits = new[] { new ShibuyaHitEvent { strike = profile.strikes[index],
                    cinematic = index == profile.strikes.Length - 1 ? profile.defaultFinisherCinematic : null } }
            } : moveset.steps[index];
        }

        private void PrepareStep(int index, ShibuyaFinisher selection)
        {
            activeStep = ResolveStep(index, selection, out var resolved);
            CurrentFinisher = resolved;
            PrepareActiveStep();
        }

        private void PrepareActiveStep()
        {
            presentationEvent = 0;
            eventVisited = new bool[activeStep.hits.Length];
            eventTargets = new HashSet<JJKGame.Core.Health>[activeStep.hits.Length];
            for (int i = 0; i < eventTargets.Length; i++) eventTargets[i] = new HashSet<JJKGame.Core.Health>();
            int count = activeStep.movements.Length;
            movementArrived = new bool[count]; movementFailed = new bool[count];
            movementSpent = new float[count]; movementTurn = new float[count];
        }

        private void ReserveNextInput()
        {
            if (lastHitTarget == null || strikeIndex + 1 >= StepCount) return;
            var next = ResolveStep(strikeIndex + 1, bufferedFinisher, out _);
            ReserveLink(lastHitTarget, AttackDuration - AttackElapsed + next.hits[0].ActiveStart + .05f);
        }

        private void ReserveLink(ShibuyaFighter victim, float duration)
        {
            if (!profile.confirmedLinkEnabled || victim == null || victim.Health.IsDead || victim.Health.IsInvulnerable
                || victim.stopped || (victim.State != ShibuyaFighterState.Hitstunned
                    && !(executingAirAttack && victim.airborne && victim.IsStunned))
                || (victim.HasConfirmedLink && victim.linkSource != this)) return;
            if (linkVictim != null && linkVictim != victim) ReleaseOutgoingLink();
            linkVictim = victim; victim.linkSource = this; victim.linkToken = sequenceToken;
            victim.linkUntilSourceTime = Mathf.Min(sequenceStartedAt + Mathf.Max(.01f, profile.confirmedLinkMaxSequence),
                combatTime + Mathf.Min(Mathf.Max(0f, duration), Mathf.Max(0f, profile.confirmedLinkMaxHold)));
        }

        private void ClearIncomingLink()
        {
            if (linkSource != null && linkSource.linkVictim == this) linkSource.linkVictim = null;
            linkSource = null;
        }
        private void ReleaseOutgoingLink()
        {
            if (linkVictim != null && linkVictim.linkSource == this) linkVictim.linkSource = null;
            linkVictim = null;
        }

        private void ApplyEventPolicies(ShibuyaFighter victim, Vector3 direction)
        {
            var evt = activeStep.hits[resolvingEvent];
            if (evt.knockbackKind == ShibuyaKnockbackKind.Distance) {
                victim.knockback = Vector3.zero;
                victim.distanceKnockbackRemaining = Mathf.Max(0f, evt.knockbackDistance);
                victim.distanceKnockbackSpeed = victim.distanceKnockbackRemaining / Mathf.Max(.01f, evt.knockbackDuration);
                victim.distanceKnockbackDirection = direction.normalized;
            }
            victim.ApplyHitOutcome(evt, direction);
            if(executingAirAttack) {
                if(victim==aerialTarget) offeredDamage=victim.damageVersion;
                RecordAerialContact(victim);
            } else OfferAirChase(victim,evt);
            if (resolvingEvent + 1 < activeStep.hits.Length)
                ReserveLink(victim, activeStep.hits[resolvingEvent + 1].ActiveStart - AttackElapsed + .05f);
            else if (HasBufferedAttack) ReserveNextInput();
        }

        private Vector3 AdvanceDistanceKnockback(float delta)
        {
            float distance = Mathf.Min(distanceKnockbackRemaining, distanceKnockbackSpeed * delta);
            distanceKnockbackRemaining -= distance; // Spend attempted motion even if collision blocks it.
            return distanceKnockbackDirection * distance;
        }

        private Vector3 AdvanceMoveset(float delta)
        {
            Vector3 result = Vector3.zero;
            for (int i = 0; i < activeStep.movements.Length; i++) {
                if (movementArrived[i] || movementFailed[i]) continue;
                var move = activeStep.movements[i];
                float elapsed = AttackElapsed;
                if (elapsed < move.startTime) continue;
                float window = Mathf.Max(0f, Mathf.Min(elapsed, move.startTime + move.duration)
                    - Mathf.Max(elapsed - delta, move.startTime));
                if (move.kind == ShibuyaMovementKind.Forward) {
                    float amount = Mathf.Min(Mathf.Max(0f, move.maxDistance - movementSpent[i]), move.speed * window);
                    movementSpent[i] += amount; result += transform.forward * amount;
                    if (elapsed >= move.startTime + move.duration) movementArrived[i] = true;
                    continue;
                }
                var target = correctionTarget;
                if (target == null || !target.isActiveAndEnabled || target.Health.IsDead || target.stopped
                    || target.Health.IsInvulnerable) { FailMovement(i); return Vector3.zero; }
                Vector3 flat = target.transform.position - transform.position; flat.y = 0f;
                Vector3 origin = transform.position + Vector3.up * CurrentStrike.attackHeight;
                if (flat.magnitude > move.targetRange || WorldBlocks(origin,
                    target.transform.position + Vector3.up * CurrentStrike.attackHeight)) { FailMovement(i); return Vector3.zero; }
                float clearance = Mathf.Max(move.arrivalDistance, motor.radius + target.motor.radius + motor.skinWidth);
                if (flat.magnitude <= clearance && Vector3.Angle(transform.forward, flat) < 45f) {
                    movementArrived[i] = true; continue;
                }
                if (elapsed >= move.startTime + move.duration || movementSpent[i] >= move.maxDistance) {
                    FailMovement(i); return Vector3.zero;
                }
                Vector3 velocity = target.motor.velocity; velocity.y = 0f;
                Vector3 predicted = flat + Vector3.ClampMagnitude(velocity * Mathf.Max(0f, move.predictionTime), clearance * .5f);
                float turn = Mathf.Min(Mathf.Max(0f, move.maxTurnAngle - movementTurn[i]), move.turnSpeed * window);
                Quaternion old = transform.rotation;
                transform.rotation = Quaternion.RotateTowards(old, Quaternion.LookRotation(predicted), turn);
                movementTurn[i] += Quaternion.Angle(old, transform.rotation);
                float distance = Mathf.Min(Mathf.Max(0f, move.maxDistance - movementSpent[i]),
                    Mathf.Min(move.speed * window, Mathf.Max(0f, flat.magnitude - clearance)));
                movementSpent[i] += distance;
                result += transform.forward * distance;
            }
            return result;
        }

        private void FailMovement(int index)
        {
            movementFailed[index] = true;
            CancelCombo(); // No remote damage and no automatic reacquisition after failure.
        }
    }
}
