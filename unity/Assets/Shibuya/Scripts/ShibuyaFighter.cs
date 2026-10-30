using System;
using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Shibuya
{
    public enum ShibuyaFighterState { Ready, Attacking, Hitstunned, Dead, Finished, Airborne, Downed, GettingUp, AirChasing, AirReady, AirFalling, GroundDashing, Guarding }

    // Presentation event: contact direction is independent of knockback and target alignment.
    public readonly struct ShibuyaHitReaction
    {
        public readonly Vector3 SourcePosition, WorldDirection, LocalDirection;
        public readonly bool IsHeavy, HasHitStop;
        public readonly float StunDuration;
        public ShibuyaHitReaction(Transform victim, Vector3 sourcePosition, Vector3 strikeDirection,
            bool isHeavy, bool hasHitStop, float stunDuration)
        {
            SourcePosition = sourcePosition;
            strikeDirection.y = 0f;
            if (strikeDirection.sqrMagnitude < 0.000001f) {
                strikeDirection = victim.position - sourcePosition; strikeDirection.y = 0f;
            }
            WorldDirection = strikeDirection.normalized;
            LocalDirection = victim.InverseTransformDirection(WorldDirection);
            IsHeavy = isHeavy; HasHitStop = hasHitStop; StunDuration = stunDuration;
        }
    }

    // The only component allowed to move this fighter's CharacterController.
    [DisallowMultipleComponent, RequireComponent(typeof(CharacterController), typeof(Health))]
    public sealed partial class ShibuyaFighter : MonoBehaviour, IHitReactable
    {
        [SerializeField] private ShibuyaCombatProfile profile;
        [SerializeField] private CharacterStatsProfile stats;
        private CharacterController motor;
        private Health health;
        private Vector3 moveIntent;
        private bool runIntent;
        private bool jumpIntent;
        private float verticalSpeed;
        private Vector3 knockback;
        private float stunnedUntil;
        private float attackStartedAt;
        private bool attacking;
        private bool stopped;
        private int strikeIndex = -1;
        private float comboExpiresAt;
        private float bufferedUntil = float.NegativeInfinity;
        private bool buffered;
        private float combatTime;
        private double hitStopUntil;
        private ShibuyaFighter lastHitTarget, correctionTarget;
        private Vector3 initialAttackForward;
        private float advanceConsumed, correctionConsumed, turnConsumed;
        private bool hitThisAttack;
        private ShibuyaFighter alignmentSource;
        private int alignmentAttack;
        private Vector3 alignmentOrigin, alignmentForward, alignmentRight;
        private float alignmentUntil, alignmentRemaining, alignmentSpeed, alignmentMaxOffset, alignmentReach;
        private float alignmentConflictUntil;
        private HashSet<Health> hitTargets = new HashSet<Health>();

        public event Action<Vector3, float> HitConfirmed;
        public event Action<int> AttackStarted;
        public event Action<ShibuyaHitReaction> HitReacted;
        public Health Health => health != null ? health : GetComponent<Health>();
        public ShibuyaCombatProfile Profile => profile;
        public ShibuyaFighterState State => Health.IsDead ? ShibuyaFighterState.Dead
            : stopped ? ShibuyaFighterState.Finished
            : downed ? (gettingUp ? ShibuyaFighterState.GettingUp : ShibuyaFighterState.Downed)
            : airborne ? ShibuyaFighterState.Airborne
            : IsStunned ? ShibuyaFighterState.Hitstunned
            : guarding ? ShibuyaFighterState.Guarding
            : groundDashing ? ShibuyaFighterState.GroundDashing
            : attacking ? ShibuyaFighterState.Attacking
            : chasing ? ShibuyaFighterState.AirChasing
            : airSession ? (CanAirAttack?ShibuyaFighterState.AirReady:ShibuyaFighterState.AirFalling)
            : ShibuyaFighterState.Ready;
        public bool IsGrounded => motor != null && motor.isGrounded;
        public float PlanarSpeed => motor == null ? 0f : new Vector2(motor.velocity.x, motor.velocity.z).magnitude;
        public ShibuyaStrikeDefinition CurrentStrike => attacking && activeStep != null ? activeStep.hits[presentationEvent].strike : null;
        public int ComboStep => attacking ? strikeIndex + 1 : 0;
        public bool HasBufferedAttack => buffered && combatTime <= bufferedUntil;
        public float AttackElapsed => attacking ? Mathf.Max(0f, combatTime - attackStartedAt) : 0f;
        public float AttackProgress => attacking ? Mathf.Clamp01(AttackElapsed / AttackDuration) : 0f;
        public Vector3 KnockbackVelocity => knockback;
        public float CombatTime => combatTime;
        public bool IsInHitStop => Time.unscaledTimeAsDouble < hitStopUntil;
        public double HitStopUntilUnscaled => hitStopUntil;
        public int HitStopCount { get; private set; }
        public float LastHitSoundPitch { get; private set; } = 1f;
        public float AttackAdvanceRequested => advanceConsumed;
        public float TrackingAdvanceRequested => correctionConsumed;
        public float TrackingTurnApplied => turnConsumed;
        public float HitFlashUntil { get; private set; }
        public int AcceptedAttackCount { get; private set; }
        public int ConfirmedHitCount { get; private set; }
        public Vector3 MoveIntent => moveIntent;
        public bool HasVictimAlignment => alignmentSource != null;
        public float VictimAlignmentRequested { get; private set; }

        public void Configure(ShibuyaCombatProfile combatProfile, CharacterStatsProfile healthStats)
        {
            profile = combatProfile;
            stats = healthStats;
        }

        private void Awake()
        {
            motor = GetComponent<CharacterController>();
            health = GetComponent<Health>();
            if (profile == null || stats == null || !profile.HasValidCombo || (moveset != null && !moveset.IsValid)
                || (profile.finishers != null && !profile.finishers.IsValid)
                || (AerialDefinition != null && !AerialDefinition.IsValid))
            {
                Debug.LogError("시부야 전투 설정 / 체력 프로필 연결이 필요합니다.", this);
                enabled = false;
                return;
            }
            health.ApplyProfile(stats);
            combatTime = Time.time;
            health.DamageResolved += OnDamageResolved;
            health.Died += OnDied;
            EnsureGuard();
            if(profile.guard!=null)gameObject.AddComponent<ShibuyaGuardVisual>();
            if(profile.guard!=null&&profile.guard.parry!=null)gameObject.AddComponent<ShibuyaParryVisual>();
        }

        public void SetIntent(Vector3 direction, bool run, bool jump)
        {
            if (ConsumeCinematicInput(false, direction)) { if (IsInCinematic && !Cinematic.Profile.holdFacing) moveIntent = direction; return; }
            direction.y = 0f;
            moveIntent = Vector3.ClampMagnitude(direction, 1f);
            runIntent = run;
            if (groundDashing || guarding || IsGuardBroken || IsParryRecovering) { jumpIntent = false; return; }
            if(jump && !QueueAirChase()) jumpIntent=true;
        }

        public bool TryAttack(ShibuyaFinisher requestedFinisher = ShibuyaFinisher.Default)
        {
            if (ConsumeCinematicInput(true, Vector3.zero)) return false;
            if (groundDashing || guarding || IsGuardBroken) return false;
            if (!enabled || profile == null || Health.IsDead || stopped || IsActionLocked) return false;
            if(airSession || !motor.isGrounded) return TryAirAttack();
            if (!motor.isGrounded) requestedFinisher = ShibuyaFinisher.Default;
            if (attacking)
            {
                if (buffered && combatTime > bufferedUntil) buffered = false;
                // A click only reserves one successor near the end; it never restarts a phase.
                if (strikeIndex >= StepCount - 1 || buffered || AttackElapsed < AttackDuration - ReservationWindow) return false;
                buffered = true;
                bufferedFinisher = requestedFinisher;
                bufferedUntil = combatTime + Mathf.Max(0.01f, ReservationWindow);
                ReserveNextInput();
                return true;
            }
            int next = strikeIndex >= 0 && combatTime <= comboExpiresAt ? strikeIndex + 1 : 0;
            BeginAttack(next >= StepCount ? 0 : next, requestedFinisher);
            return true;
        }

        private void BeginAttack(int index, ShibuyaFinisher selection = ShibuyaFinisher.Default)
        {
            if (index == 0) { ClearAerialState(); ReleaseOutgoingLink(); ResetComboTarget(); lastHitTarget = null; sequenceStartedAt = combatTime; sequenceToken++; }
            correctionTarget = lastHitTarget;
            initialAttackForward = transform.forward;
            advanceConsumed = correctionConsumed = turnConsumed = 0f;
            hitThisAttack = false;
            strikeIndex = index;
            attacking = true;
            buffered = false;
            PrepareStep(index, selection);
            PrepareTargeting();
            attackStartedAt = combatTime;
            hitTargets.Clear();
            AcceptedAttackCount++;
            AttackStarted?.Invoke(index + 1);
        }

        private void Update()
        {
            if (State == ShibuyaFighterState.Dead || stopped)
            {
                CancelCombo();
                ClearVictimAlignment();
                moveIntent = Vector3.zero;
                knockback = Vector3.zero;
                jumpIntent = false;
                return;
            }
            if (IsInCinematic) { UpdateCinematicMotion(); return; }
            RefreshAerialState();
            // Only this fighter's clock/motor pauses. Other actors and Time.timeScale are untouched.
            double now = Time.unscaledTimeAsDouble;
            double frameStart = now - Time.unscaledDeltaTime;
            float fraction = Mathf.Clamp01((float)((now - Math.Max(frameStart, hitStopUntil))
                / Math.Max(0.000001, Time.unscaledDeltaTime)));
            float delta = Time.deltaTime * fraction;
            if (IsInHitStop) { jumpIntent = false; return; }
            combatTime += delta;
            UpdateGuard(delta);
            StartQueuedChase();
            UpdateRecoveryState();
            bool stunned = IsActionLocked;
            bool dashFrame = groundDashing;
            if (stunned) CancelCombo();
            Vector3 targetAdvance = !stunned ? AdvanceTargeting(delta) : Vector3.zero;
            if(!stunned) AimAirAttack(delta);
            if (!stunned) UpdateAttack();
            if (stopped || Health.IsDead) return;
            if (IsInHitStop) return; // The active phase may have started a pause in ResolveHits.
            // A parry may have cancelled/locked this attacker during ResolveHits above.
            stunned=IsActionLocked;if(stunned)targetAdvance=Vector3.zero;

            Vector3 advance = Vector3.zero;
            if (!stunned && attacking) {
                advance = !executingAirAttack && moveset == null && CurrentFinisher == ShibuyaFinisher.Default ? AdvanceAttack(delta) : AdvanceMoveset(delta);

            }

            Vector3 voluntary = stunned || dashFrame ? Vector3.zero : AttackMoveIntent();
            if(airSession) voluntary*=chasing?0:aerialSettings.aerialAirControl;
            if (!stunned && !attacking && !airSession && voluntary.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(voluntary), Mathf.Max(0.1f, profile.turnSpeed) * delta);
            float speed = Mathf.Max(0.1f, runIntent ? profile.runSpeed : profile.walkSpeed);
            if (attacking) speed *= Mathf.Clamp01(profile.attackMoveMultiplier);
            if (guarding) speed = profile.walkSpeed * Mathf.Clamp01(guardSettings.movementMultiplier);
            if (attacking && PresentationElapsed < CurrentStrike.PowerWindup) speed *= CurrentStrike.windupMoveMultiplier;
            UpdateVerticalMotion(delta, stunned);
            Vector3 displacement = (voluntary * speed + knockback + Vector3.up * verticalSpeed) * delta
                + advance + targetAdvance + AdvanceVictimAlignment(delta) + AdvanceDistanceKnockback(delta)
                + AdvanceAirHorizontal(delta);
            if (dashFrame) displacement += AdvanceGroundDash();
            Vector3 chaseMove=!stunned?AdvanceAirChase(delta):Vector3.zero;
            if(chasing) displacement=chaseMove;
            displacement = SeparateLandingCapsules(displacement, delta);
            displacement = ConstrainFighterMovement(displacement);
            // Consume blocked displacement immediately; it must never accumulate behind walls.
            int steps = Mathf.Max(1, Mathf.CeilToInt(displacement.magnitude / Mathf.Max(0.05f, motor.radius * 0.5f)));
            CollisionFlags collisions = CollisionFlags.None;
            Vector3 beforeMotor=transform.position;
            for (int i = 0; i < steps; i++) collisions |= motor.Move(displacement / steps);
            FinishVerticalMotion(collisions);
            AfterAerialMotor(beforeMotor,collisions,delta);
            knockback = Vector3.MoveTowards(knockback, Vector3.zero,
                Mathf.Max(0.1f, profile.knockbackDamping) * delta);
        }

        private Vector3 AttackMoveIntent()
        {
            if (!attacking) return moveIntent;
            float forward = Vector3.Dot(moveIntent, transform.forward);
            float sideways = Vector3.Dot(moveIntent, transform.right);
            Vector3 constrained = transform.forward * (forward >= 0f ? forward
                : forward * Mathf.Clamp01(profile.attackBackwardInputMultiplier))
                + transform.right * sideways * Mathf.Clamp01(profile.attackLateralInputMultiplier);
            // Give locomotion back progressively during recovery; never discard the raw command.
            float recovery = Mathf.InverseLerp(AttackActiveEnd, AttackDuration, AttackElapsed);
            return Vector3.Lerp(constrained, moveIntent, Mathf.SmoothStep(0f, 1f, recovery));
        }

        private Vector3 ConstrainFighterMovement(Vector3 displacement)
        {
            Vector3 planar = new Vector3(displacement.x, 0f, displacement.z);
            float distance = planar.magnitude;
            if (distance < 0.000001f) return displacement;
            Vector3 scale = transform.lossyScale;
            float radius = motor.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float half = Mathf.Max(0f, motor.height * Mathf.Abs(scale.y) * 0.5f - radius);
            Vector3 center = transform.TransformPoint(motor.center);
            float allowed = distance;
            // All movement, including victim knockback/alignment, uses the same capsule clearance.
            // This is collision clearance, not target acquisition or added homing.
            foreach (RaycastHit hit in Physics.CapsuleCastAll(center + Vector3.up * half,
                center - Vector3.up * half, radius, planar / distance, distance + motor.skinWidth,
                ~0, QueryTriggerInteraction.Ignore))
            {
                ShibuyaFighter other = hit.collider.GetComponentInParent<ShibuyaFighter>();
                if (other == null || other == this) continue;
                Vector3 toward = other.transform.position - transform.position; toward.y = 0f;
                if (Vector3.Dot(toward, planar) <= 0f) continue; // Never prevent backing away.
                allowed = Mathf.Min(allowed, Mathf.Max(0f, hit.distance - motor.skinWidth));
            }
            planar *= allowed / distance;
            return new Vector3(planar.x, displacement.y, planar.z);
        }

        private void UpdateAttack()
        {
            if (!attacking) return;
            int token = AcceptedAttackCount;
            float elapsed = AttackElapsed;
            for (int i = 0; i < activeStep.hits.Length; i++)
                if (elapsed >= activeStep.hits[i].startTime) presentationEvent = i;
            for (int i = 0; i < activeStep.hits.Length; i++) {
                var evt = activeStep.hits[i];
                if (elapsed < evt.ActiveStart) break;
                if (evt.requiredMovement >= 0 && !movementArrived[evt.requiredMovement]) {
                    if (movementFailed[evt.requiredMovement] || elapsed > evt.ActiveEnd) { CancelCombo(); return; }
                    return; // Arrival is a gameplay prerequisite; later hits cannot overtake this event.
                }
                if (!eventVisited[i] || elapsed <= evt.ActiveEnd) {
                    resolvingEvent = i;
                    if (!eventVisited[i]) HitEventStarted?.Invoke(i);
                    if (!attacking || token != AcceptedAttackCount) return;
                    eventVisited[i] = true;
                    hitTargets = eventTargets[i];
                    ResolveHits(evt.strike);
                    if (!attacking || token != AcceptedAttackCount || IsInHitStop) return;
                }
                if(executingAirAttack && elapsed>evt.ActiveEnd && eventTargets[i].Count==0) {
                    ReleaseAerialVictimHover(); ReleaseOutgoingLink();
                }
            }
            if (!attacking) return;
            if (elapsed >= AttackDuration) {
                if(executingAirAttack) {FinishAirAttack();return;}
                bool chain = HasBufferedAttack && strikeIndex < StepCount - 1;
                ShibuyaFinisher nextFinisher = bufferedFinisher;
                if (!hitThisAttack) { lastHitTarget = null; ReleaseOutgoingLink(); }
                attacking = false;
                buffered = false;
                comboExpiresAt = combatTime + Mathf.Max(0.01f, profile.comboResetTime);
                if (chain) BeginAttack(strikeIndex + 1, nextFinisher);
                else {
                    ReleaseOutgoingLink(); CurrentFinisher = bufferedFinisher = ShibuyaFinisher.Default;
                    if (strikeIndex == StepCount - 1) strikeIndex = -1;
                }
            }
        }

        private Vector3 AdvanceAttack(float delta, bool includeBaseAdvance = true)
        {
            ShibuyaStrikeDefinition strike = CurrentStrike;
            float elapsed = AttackElapsed;
            float previous = Mathf.Max(0f, elapsed - delta);
            float begin = Mathf.Max(strike.PowerWindup, Mathf.Max(0f, strike.advanceStartTime));
            float end = Mathf.Min(strike.Duration, begin + Mathf.Max(0f, strike.advanceDuration));
            float window = Mathf.Max(0f, Mathf.Min(elapsed, end) - Mathf.Max(previous, begin));
            float distance = includeBaseAdvance ? Mathf.Min(Mathf.Max(0f, strike.advanceDistance - advanceConsumed),
                Mathf.Max(0f, strike.advanceSpeed) * window) : 0f;
            // Consume attempted displacement even against a wall: no stored burst after obstruction.
            advanceConsumed += distance;
            return transform.forward * distance;
        }

        private void ResolveHits(ShibuyaStrikeDefinition strike)
        {
            var cinematicHit = activeStep.hits[resolvingEvent];
            int cinematicActionId = AcceptedAttackCount, cinematicEventId = resolvingEvent;
            ShibuyaFighter cinematicVictim = null;
            Vector3 origin = transform.position + Vector3.up * strike.attackHeight;
            Vector3 center = origin + transform.forward * strike.attackForwardOffset;
            foreach (Collider collider in Physics.OverlapSphere(center, Mathf.Max(0.1f, strike.attackRadius),
                ~0, QueryTriggerInteraction.Ignore))
            {
                if (!attacking || stopped || Health.IsDead) break;
                ShibuyaFighter other = collider.GetComponentInParent<ShibuyaFighter>();
                if (other == null || other == this || other.Health.IsDead || other.stopped || hitTargets.Contains(other.Health)) continue;
                if(executingAirAttack && Mathf.Abs(other.transform.position.y-transform.position.y)>aerialSettings.aerialMaxHeightDifference) continue;
                Vector3 targetPoint = other.transform.position + Vector3.up * strike.attackHeight;
                Vector3 flat = other.transform.position - transform.position;
                flat.y = 0f;
                if (Vector3.Dot(transform.forward, flat.normalized) < 0.1f || WorldBlocks(origin, targetPoint)) continue;
                hitTargets.Add(other.Health);
                // CharacterStatsProfile attack/defense multipliers remain unused, as in PASS 1.
                DamageContext context = new DamageContext(Mathf.Max(0.1f, strike.damage), gameObject,
                    DamageDeliveryType.PhysicalStrike, DamageTraits.None, "SHIBUYA STRIKE", targetPoint);
                // Two attackers must not tug the same victim toward different attack corridors.
                bool conflictingAlignment = other.alignmentSource != null && other.alignmentSource != this;
                var resolution=other.ReceiveStrike(context,strike);
                // Parry can synchronously cancel this attack. Do not visit another collider/event or start a saved cinematic.
                if(!attacking||stopped||Health.IsDead)return;
                if (resolution != DamageResolution.Applied) continue;
                float cinematicDistance = (other.transform.position-transform.position).sqrMagnitude;
                if (cinematicVictim == null || cinematicDistance < (cinematicVictim.transform.position-transform.position).sqrMagnitude
                    || (Mathf.Approximately(cinematicDistance, (cinematicVictim.transform.position-transform.position).sqrMagnitude)
                        && other.GetInstanceID() < cinematicVictim.GetInstanceID())) cinematicVictim = other;
                if (!hitThisAttack || moveset != null) lastHitTarget = other;
                RecordComboContact(other);
                hitThisAttack = true;
                ConfirmedHitCount++;
                Vector3 direction = strike.knockbackDirection == ShibuyaKnockbackDirection.AttackerForward
                    || flat.sqrMagnitude <= 0.001f ? transform.forward : flat.normalized;
                other.ApplyHitReaction(direction * Mathf.Max(0f, strike.knockbackSpeed), Mathf.Max(0f, strike.hitStun),
                    transform.position, transform.forward, moveset == null && !executingAirAttack ? strikeIndex == 3 : activeStep.hits[resolvingEvent].heavyReaction, strike.hitStopDuration > 0f);
                if (conflictingAlignment)
                    other.alignmentConflictUntil = other.combatTime + Mathf.Max(0f, profile.victimAlignmentDuration);
                else other.BeginVictimAlignment(this, strike);
                ApplyEventPolicies(other, direction);
                if (strike.hitStopDuration > 0f)
                {
                    ApplyLocalHitStop(strike.hitStopDuration);
                    other.ApplyLocalHitStop(strike.hitStopDuration);
                }
                LastHitSoundPitch = strike.soundPitch;
                HitConfirmed?.Invoke(targetPoint, strike.presentationIntensity);
            }
            if (cinematicVictim != null) TryCinematicAfterHit(cinematicVictim, cinematicHit, cinematicActionId, cinematicEventId);
        }

        private void BeginVictimAlignment(ShibuyaFighter source, ShibuyaStrikeDefinition strike)
        {
            ShibuyaCombatProfile settings = source.profile;
            if (!settings.victimAlignmentEnabled || !strike.alignVictimOnHit
                || strike.knockbackDirection != ShibuyaKnockbackDirection.AttackerForward
                || State != ShibuyaFighterState.Hitstunned || combatTime < alignmentConflictUntil) return;
            Vector3 flat = transform.position - source.transform.position; flat.y = 0f;
            float lateral = Vector3.Dot(flat, source.transform.right);
            if (Mathf.Abs(lateral) > settings.victimAlignmentMaxLateralOffset) return;
            alignmentSource = source;
            alignmentAttack = source.AcceptedAttackCount;
            alignmentOrigin = source.transform.position;
            alignmentForward = source.transform.forward;
            alignmentRight = source.transform.right;
            alignmentUntil = Mathf.Min(stunnedUntil, combatTime + Mathf.Max(0f, settings.victimAlignmentDuration));
            alignmentMaxOffset = Mathf.Max(0f, settings.victimAlignmentMaxLateralOffset);
            alignmentRemaining = Mathf.Min(Mathf.Abs(lateral), Mathf.Max(0f, settings.victimAlignmentMaxDistance));
            alignmentSpeed = Mathf.Max(0f, settings.victimAlignmentSpeed);
            // Original hit envelope plus the natural decelerating knockback travel, not a new lock range.
            alignmentReach = strike.attackForwardOffset + strike.attackRadius + motor.radius
                + strike.knockbackSpeed * strike.knockbackSpeed / (2f * Mathf.Max(0.1f, profile.knockbackDamping));
            VictimAlignmentRequested = 0f;
        }

        private Vector3 AdvanceVictimAlignment(float delta)
        {
            if (alignmentSource == null) return Vector3.zero;
            Vector3 flat = transform.position - alignmentOrigin; flat.y = 0f;
            float lateral = Vector3.Dot(flat, alignmentRight);
            float forward = Vector3.Dot(flat, alignmentForward);
            if (!alignmentSource.isActiveAndEnabled || !alignmentSource.attacking || alignmentSource.stopped
                || alignmentSource.Health.IsDead || alignmentSource.AcceptedAttackCount != alignmentAttack
                || State != ShibuyaFighterState.Hitstunned || Health.IsInvulnerable
                || combatTime >= alignmentUntil || alignmentRemaining <= 0f
                || Mathf.Abs(lateral) > alignmentMaxOffset || forward < 0f || forward > alignmentReach)
            {
                ClearVictimAlignment();
                return Vector3.zero;
            }
            float amount = Mathf.Min(Mathf.Abs(lateral), Mathf.Min(alignmentRemaining,
                alignmentSpeed * Mathf.Min(delta, alignmentUntil - combatTime)));
            alignmentRemaining -= amount; // Blocked movement is spent, never banked for a later snap.
            VictimAlignmentRequested += amount;
            return -alignmentRight * Mathf.Sign(lateral) * amount;
        }

        private void ClearVictimAlignment() { alignmentSource = null; alignmentRemaining = 0f; }

        private void ApplyLocalHitStop(float duration)
        {
            if (duration <= 0f) return;
            if (!IsInHitStop) HitStopCount++;
            hitStopUntil = Math.Max(hitStopUntil, Time.unscaledTimeAsDouble + duration);
        }

        private static bool WorldBlocks(Vector3 origin, Vector3 target)
        {
            Vector3 delta = target - origin;
            foreach (RaycastHit hit in Physics.RaycastAll(origin, delta.normalized, delta.magnitude,
                ~0, QueryTriggerInteraction.Ignore))
                if (hit.collider.GetComponentInParent<ShibuyaFighter>() == null) return true;
            return false;
        }

        public void ApplyHitReaction(Vector3 impulse, float stunDuration)
        {
            if (IsInCinematic) return;
            ApplyHitReaction(impulse, stunDuration, transform.position - impulse, impulse, false, false);
        }

        private void ApplyHitReaction(Vector3 impulse, float stunDuration, Vector3 sourcePosition,
            Vector3 strikeDirection, bool isHeavy, bool hasHitStop)
        {
            EndGuard();
            CancelGroundDash();
            ReleaseIncomingAirHover();
            ClearVictimAlignment();
            if (Health.IsDead || stopped) return;
            // This method follows Applied damage in ResolveHits. A new hit owns subsequent motion.
            chaseContactSource = null;
            if (airborne) EndLaunchMotion();
            InterruptAerial();
            impulse.y = 0f;
            knockback = impulse;
            distanceKnockbackRemaining = 0f;
            ClearIncomingLink();
            stunnedUntil = Mathf.Max(stunnedUntil, combatTime + Mathf.Max(0f, stunDuration));
            HitFlashUntil = combatTime + profile.hitFlashDuration;
            CancelCombo();
            jumpIntent = false;
            // A zero-impact/zero-stun cancellation is not a new hit presentation event.
            if (stunDuration > 0f || strikeDirection.sqrMagnitude > 0.000001f)
                HitReacted?.Invoke(new ShibuyaHitReaction(transform, sourcePosition, strikeDirection,
                    isHeavy, hasHitStop, Mathf.Max(0f, stunnedUntil - combatTime)));
        }

        public void StopCombat()
        {
            CancelGuardInput(); guardLockedUntil=0;
            CancelGroundDash();
            CancelCinematic("battle-ended");
            stopped = true;
            ClearAerialState();
            ClearHitOutcome();
            ClearIncomingLink(); distanceKnockbackRemaining = 0f;
            CancelCombo();
            ClearVictimAlignment();
            moveIntent = knockback = Vector3.zero;
            jumpIntent = false;
            hitStopUntil = 0;
        }

        private void CancelCombo(bool preserveAerialVictim = false)
        {
            CancelAerialHover(preserveAerialVictim);
            executingAirAttack=false;
            ReleaseOutgoingLink();
            CurrentFinisher = bufferedFinisher = ShibuyaFinisher.Default;
            attacking = buffered = false;
            bufferedUntil = float.NegativeInfinity;
            comboExpiresAt = 0f;
            strikeIndex = -1;
            hitTargets.Clear();
            lastHitTarget = correctionTarget = null;
            ResetComboTarget();
        }

        private void OnDamageResolved(Health target, DamageContext context, DamageResolution result)
        {
            if (result == DamageResolution.Applied) { CancelGroundDash(); EndGuard(); }
            if (result == DamageResolution.Applied && IsInCinematic && !Cinematic.ApplyingAuthoredDamage) CancelCinematic("external-or-environmental-damage");
            if (result == DamageResolution.Applied) { ReleaseIncomingAirHover(); damageVersion++; InterruptAerial(); ClearVictimAlignment(); CancelCombo(); }
        }
        private void OnDied(Health target) { CancelGuardInput(); guardLockedUntil=0; CancelGroundDash(); CancelCinematic("participant-dead"); ClearAerialState(); ClearHitOutcome(); ClearIncomingLink(); ClearVictimAlignment(); CancelCombo(); }
        private void OnDestroy()
        {
            if (health == null) return;
            health.DamageResolved -= OnDamageResolved;
            health.Died -= OnDied;
        }
        private void OnDisable() { CancelGuardInput(); guardLockedUntil=0; CancelGroundDash(); CancelCinematic("participant-disabled"); ClearAerialState(); ClearHitOutcome(); ClearIncomingLink(); distanceKnockbackRemaining = 0f; ClearVictimAlignment(); moveIntent = knockback = Vector3.zero; jumpIntent = false; hitStopUntil = 0; CancelCombo(); }
    }
}
