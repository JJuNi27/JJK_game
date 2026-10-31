using UnityEngine;
using JJKGame.Core;
namespace JJKGame.Shibuya
{
    public sealed partial class ShibuyaFighter
    {
        ShibuyaFighter counterOpportunityTarget,counterLeaseTarget,counterSource;
        ShibuyaCounterProfile activeCounter;
        float counterOpportunityUntil,counterLeaseUntil;
        bool counterPending,executingCounter;
        public ShibuyaCounterProfile CounterSettings=>profile!=null?profile.counter:null;
        public bool IsCounterBusy=>counterPending||executingCounter;
        public bool HasCounterReservation=>counterSource!=null&&counterSource.isActiveAndEnabled
            &&counterSource.counterLeaseTarget==this&&counterSource.IsCounterBusy
            &&Time.time<counterSource.counterLeaseUntil&&!counterSource.Health.IsDead&&!counterSource.stopped;
        public bool HasCounterOpportunity=>counterOpportunityTarget!=null&&Time.time<counterOpportunityUntil
            &&CounterSettings!=null&&CounterSettings.IsValid&&IsLiveTarget(counterOpportunityTarget)
            &&isActiveAndEnabled&&!Health.IsDead&&!stopped&&!IsInCinematic&&!IsStunned&&!downed&&!airborne&&!IsGrabBusy;
        public float CounterOpportunityRemaining=>HasCounterOpportunity?Mathf.Max(0,counterOpportunityUntil-Time.time):0;
        public ShibuyaFighter CounterTarget=>counterLeaseTarget!=null?counterLeaseTarget:HasCounterOpportunity?counterOpportunityTarget:null;
        public int CounterInputCount {get;private set;}
        public int CounterStartedCount {get;private set;}
        public int CounterAppliedCount {get;private set;}
        public string LastCounterReason {get;private set;}="none";
        void OfferCounter(ShibuyaFighter attacker)
        {
            CancelCounter("new-parry-ownership");
            if(CounterSettings==null||!CounterSettings.IsValid)return;
            counterOpportunityTarget=attacker;counterOpportunityUntil=Time.time+CounterSettings.inputOpportunity;
            LastCounterReason="opportunity";
        }
        // Input adapter calls this once per fresh LMB edge. True means consumed, even if validation rejects it.
        public bool TryCounterInput()
        {
            if(!HasCounterOpportunity)return false;
            var target=counterOpportunityTarget;var p=CounterSettings;CounterInputCount++;
            counterOpportunityTarget=null;counterOpportunityUntil=0;
            if(!IsGrounded||groundDashing||attacking||guarding||IsGuardBroken||IsGrabBusy||HasCounterReservation
                ||airSession||chasing||FocusingEnergy||(IsActionLocked&&!IsParryRecovering)){
                LastCounterReason="source-action-conflict";return true;}
            if(!ValidCounterTarget(target,true,p,out string reason)){LastCounterReason=reason;return true;}
            if(p.maximumReservationTime<=p.counterStep.Duration+Mathf.Max(0,parryPoseUntil-combatTime)){
                LastCounterReason="reservation-shorter-than-authored-step";return true;}
            CancelCombo();activeCounter=p;counterLeaseTarget=target;counterPending=true;
            counterLeaseUntil=Time.time+p.maximumReservationTime;target.counterSource=this;
            target.CancelGuardInput();target.CancelGroundDash();target.CancelQuickRecovery();target.CancelCombo();
            target.ClearIncomingLink();target.ClearVictimAlignment();target.jumpIntent=false;
            LastCounterReason="reserved";return true;
        }
        bool ValidCounterTarget(ShibuyaFighter target,bool starting,ShibuyaCounterProfile settings,out string reason)
        {
            reason="target-invalid";
            if(!IsLiveTarget(target)||target==this||target.gameObject.scene!=gameObject.scene)return false;
            if(target.IsInCinematic||target.IsGrabBusy){reason="target-ownership-conflict";return false;}
            if(target.Health.IsInvulnerable){reason="target-special-invulnerable";return false;}
            if(target.HasCounterReservation&&target.counterSource!=this){reason="target-already-reserved";return false;}
            if(starting){var delta=target.motor.bounds.center-motor.bounds.center;
                if(delta.magnitude>settings.maximumStartRange||Vector3.Angle(transform.forward,Vector3.ProjectOnPlane(delta,Vector3.up))>settings.maximumStartAngle){reason="start-range-or-angle";return false;}}
            if(WorldBlocks(motor.bounds.center,target.motor.bounds.center)){reason="world-obstruction";return false;}
            reason="valid";return true;
        }
        void UpdateCounter()
        {
            if(counterOpportunityTarget!=null&&(!HasCounterOpportunity||IsActionLocked&&!IsParryRecovering)){
                counterOpportunityTarget=null;counterOpportunityUntil=0;LastCounterReason="opportunity-expired-or-invalid";}
            if(!IsCounterBusy)return;
            if(Health.IsDead||stopped||IsInCinematic||IsStunned||downed||airborne||IsGrabBusy||HasCounterReservation){CancelCounter("source-interrupted");return;}
            if(Time.time>=counterLeaseUntil){CancelCounter("reservation-timeout");return;}
            if(!ValidCounterTarget(counterLeaseTarget,false,activeCounter,out string reason)){CancelCounter(reason);return;}
            if(counterPending&&!IsParryRecovering&&!IsInHitStop)BeginCounter();
        }
        void BeginCounter()
        {
            counterPending=false;executingCounter=true;
            ClearAerialState();ResetComboTarget();ReleaseOutgoingLink();sequenceStartedAt=combatTime;sequenceToken++;
            initialAttackForward=transform.forward;lastHitTarget=correctionTarget=counterLeaseTarget;
            advanceConsumed=correctionConsumed=turnConsumed=0;hitThisAttack=false;
            activeStep=activeCounter.counterStep;PrepareActiveStep();PrepareTargeting();
            attacking=true;strikeIndex=0;buffered=false;CurrentFinisher=ShibuyaFinisher.Default;
            attackStartedAt=combatTime;AcceptedAttackCount++;CounterStartedCount++;
            counterAppliedAtStart=CounterAppliedCount;
            LastCounterReason="executing";AttackStarted?.Invoke(1);
        }
        void ReleaseCounterTarget()
        {
            if(counterLeaseTarget!=null&&counterLeaseTarget.counterSource==this)counterLeaseTarget.counterSource=null;
            counterLeaseTarget=null;counterLeaseUntil=0;activeCounter=null;counterPending=false;
        }
        public void CancelCounter(string reason)
        {
            bool running=executingCounter;executingCounter=false;
            if(counterOpportunityTarget!=null||counterPending||running)LastCounterReason=reason;
            counterOpportunityTarget=null;counterOpportunityUntil=0;ReleaseCounterTarget();
            if(running)CancelCombo();
        }
        void CancelIncomingCounter(string reason){if(counterSource!=null){var source=counterSource;counterSource=null;source.CancelCounter(reason);}}
        void FinishCounter(){LastCounterReason=CounterAppliedCount>counterAppliedAtStart?"applied-complete":"completed-without-applied-hit";CancelCombo();}
        int counterAppliedAtStart;
    }
}
