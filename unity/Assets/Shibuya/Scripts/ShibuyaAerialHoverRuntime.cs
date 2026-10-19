using UnityEngine;

namespace JJKGame.Shibuya
{
    // Height ownership only. Both jump and pursuit attacks use the existing hit-event engine.
    public sealed partial class ShibuyaFighter
    {
        private bool airHoverStarted, airGrace, airBuffered;
        private double airHoverDeadline;
        private float airGraceUntil, airBufferedUntil;
        private ShibuyaFighter airContactTarget, heldAirVictim, airHoldSource;
        private bool WithinAirHoverBudget => !airHoverStarted || Time.unscaledTimeAsDouble < airHoverDeadline;
        public bool IsAerialHovering => airSession && !chasing && aerialSettings != null
            && aerialSettings.aerialHoverEnabled && WithinAirHoverBudget && !Health.IsDead && !stopped
            && (executingAirAttack || (airGrace && combatTime <= airGraceUntil));
        public bool IsHeldByAerialHit => airHoldSource != null && !Health.IsDead && !stopped && !downed
            && !motor.isGrounded && !Health.IsInvulnerable && IsStunned
            && airHoldSource.IsAerialHovering && airHoldSource.heldAirVictim == this
            && airHoldSource.LegalAerialContact(this);
        public bool HasBufferedAirAttack => airBuffered && combatTime <= airBufferedUntil && WithinAirHoverBudget;

        private bool LegalAerialContact(ShibuyaFighter target)
        {
            if(aerialSettings == null || !IsLiveTarget(target) || target.Health.IsInvulnerable) return false;
            var hits = aerialSettings.aerialAttack.hits;
            var strike = executingAirAttack ? CurrentStrike : hits[hits.Length - 1].strike;
            Vector3 flat = target.transform.position - transform.position; flat.y = 0;
            float reach = strike.attackForwardOffset + strike.attackRadius + target.motor.radius;
            return flat.magnitude <= reach
                && Mathf.Abs(target.transform.position.y-transform.position.y) <= aerialSettings.aerialMaxHeightDifference
                && !WorldBlocks(transform.position+Vector3.up*strike.attackHeight,
                    target.transform.position+Vector3.up*strike.attackHeight);
        }

        private void BeginCommonAirStep(bool successor)
        {
            if(!successor) CancelCombo();
            if(!airHoverStarted) {
                airHoverStarted = true;
                airHoverDeadline = Time.unscaledTimeAsDouble + aerialSettings.aerialMaxHoverTime;
                sequenceStartedAt = combatTime; sequenceToken++;
            }
            airGrace = airBuffered = false;
            executingAirAttack = airAttackUsed = attacking = true; strikeIndex = 0;
            hitThisAttack = false;
            activeStep = aerialSettings.aerialAttack; PrepareActiveStep(); PrepareTargeting();
            attackStartedAt = combatTime; initialAttackForward = transform.forward;
            correctionTarget = airContactTarget != null ? airContactTarget : aerialTarget;
            advanceConsumed = correctionConsumed = turnConsumed = 0;
            if(aerialSettings.aerialHoverEnabled) verticalSpeed = 0;
            AcceptedAttackCount++; AttackStarted?.Invoke(1);
        }

        private bool BufferCommonAirStep()
        {
            float window = aerialSettings.aerialLinkInputWindow;
            if(window <= 0 || HasBufferedAirAttack || !WithinAirHoverBudget || !hitThisAttack
                || AttackElapsed < AttackDuration-window || !LegalAerialContact(airContactTarget)) return false;
            airBuffered = true; airBufferedUntil = attackStartedAt + AttackDuration + window;
            // Reserve only while the defender is already legitimately stunned. Existing caps remain.
            ReserveLink(airContactTarget, AttackDuration-AttackElapsed+activeStep.hits[0].ActiveStart+.05f);
            return true;
        }

        private void RecordAerialContact(ShibuyaFighter victim)
        {
            if(airContactTarget != victim) ReleaseAerialVictimHover();
            airContactTarget = victim;
            if(!aerialSettings.holdVictimOnAirHit || !IsAerialHovering || victim.motor.isGrounded
                || victim.downed || !victim.IsStunned || !LegalAerialContact(victim)) return;
            victim.ReleaseIncomingAirHover();
            heldAirVictim = victim; victim.airHoldSource = this;
            victim.verticalSpeed = 0;
        }

        private void RefreshAerialHover()
        {
            if(airHoldSource != null && !IsHeldByAerialHit) ReleaseIncomingAirHover();
            if(heldAirVictim != null && !heldAirVictim.IsHeldByAerialHit) ReleaseAerialVictimHover();
            if(airSession && airHoverStarted && !WithinAirHoverBudget && (executingAirAttack || airGrace)) {
                EndChaseToFall("공중 체공 총 시간 만료: 자연 낙하"); return;
            }
            if(airGrace && combatTime > airGraceUntil) {
                CancelAerialHover(); AirChaseReason = "공중 후속 입력 시간 만료: 자연 낙하";
            }
        }

        private void CancelAerialHover()
        {
            if(executingAirAttack || airGrace) verticalSpeed = 0;
            airGrace = airBuffered = false;
            ReleaseAerialVictimHover();
        }

        private void ReleaseAerialVictimHover()
        {
            var victim = heldAirVictim; heldAirVictim = null;
            if(victim != null && victim.airHoldSource == this) {
                victim.airHoldSource = null; victim.verticalSpeed = 0;
            }
        }

        private void ReleaseIncomingAirHover()
        {
            var source = airHoldSource; airHoldSource = null;
            if(source == null) return;
            if(source.heldAirVictim == this) source.heldAirVictim = null;
            verticalSpeed = 0;
        }
    }
}
