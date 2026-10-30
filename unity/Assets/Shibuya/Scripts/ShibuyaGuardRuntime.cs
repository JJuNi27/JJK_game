using System;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Shibuya
{
    public enum ShibuyaGuardVisualState { Stable, Strained, Critical, Broken }
    public sealed partial class ShibuyaFighter
    {
        private ShibuyaGuardProfile guardSettings;
        private bool guarding,guardInputHeld;
        private float currentGuard,guardRecoverAt,guardLockedUntil;
        private ShibuyaStrikeDefinition incomingGuardStrike;
        public event Action<ShibuyaFighter> GuardChanged,GuardBlocked,GuardBroken;
        public bool IsGuarding => guarding && GuardSettings!=null && GuardSettings.guardEnabled;
        public bool IsGuardBroken => combatTime<guardLockedUntil;
        public float CurrentGuard { get { EnsureGuard();return currentGuard; } }
        public float MaximumGuard => profile!=null&&profile.guard!=null?Mathf.Max(1,profile.guard.maximumGuard):0;
        public ShibuyaGuardProfile GuardSettings => profile!=null?profile.guard:null;
        public int GuardBlockCount {get;private set;}
        public int GuardBreakCount {get;private set;}
        public ShibuyaGuardVisualState GuardVisualState => GuardSettings==null?ShibuyaGuardVisualState.Stable:IsGuardBroken||CurrentGuard<=0?ShibuyaGuardVisualState.Broken
            : CurrentGuard/Mathf.Max(1,MaximumGuard)>guardSettings.stableThreshold?ShibuyaGuardVisualState.Stable
            : CurrentGuard/Mathf.Max(1,MaximumGuard)>guardSettings.criticalThreshold?ShibuyaGuardVisualState.Strained:ShibuyaGuardVisualState.Critical;
        private void EnsureGuard()
        {
            var p=GuardSettings;
            if(p==null){EndGuard();guardSettings=null;currentGuard=guardLockedUntil=0;return;}
            if(guardSettings==p) {
                float clamped=Mathf.Clamp(currentGuard,0,MaximumGuard);
                if(currentGuard!=clamped){currentGuard=clamped;GuardChanged?.Invoke(this);}return;
            }
            EndGuard();guardSettings=p;currentGuard=MaximumGuard;guardLockedUntil=0;
        }
        // A failed/interrupted hold is consumed. Release is required before another attempt.
        public void SetGuardHeld(bool held)
        {
            EnsureGuard();
            if(!held){guardInputHeld=false;EndGuard();return;}
            if(guardInputHeld)return;guardInputHeld=true;
            if(guardSettings==null||!guardSettings.guardEnabled||!enabled||Health.IsDead||stopped
                ||!IsGrounded||attacking||groundDashing||IsActionLocked||IsInHitStop||downed||gettingUp
                ||airSession||chasing||chaseQueued||IsInCinematic||IsGuardBroken||FocusingEnergy||currentGuard<=0)return;
            guarding=true;jumpIntent=false;CancelCombo();ClearIncomingLink();ClearVictimAlignment();BeginParryAttempt();GuardChanged?.Invoke(this);
        }
        public void CancelGuardInput(){EndGuard();guardInputHeld=true;}
        private void EndGuard()
        {
            parryUntil=0;
            if(!guarding)return;guarding=false;
            guardRecoverAt=combatTime+(guardSettings!=null?Mathf.Max(0,guardSettings.recoveryDelay):0);
            GuardChanged?.Invoke(this);
        }
        private void UpdateGuard(float delta)
        {
            EnsureGuard();if(guardSettings==null)return;
            if(guarding&&(!guardSettings.guardEnabled||!IsGrounded||IsActionLocked||IsInHitStop||IsInCinematic||airSession||chasing))EndGuard();
            if(!guarding&&combatTime>=guardRecoverAt&&currentGuard<MaximumGuard) {
                currentGuard=Mathf.Min(MaximumGuard,currentGuard+Mathf.Max(0,guardSettings.recoveryPerSecond)*delta);GuardChanged?.Invoke(this);
            }
        }
        // Supplies authored guard metadata only around this synchronous Health transaction.
        // Shared DamageContext/Health and unrelated combat systems are unchanged.
        public DamageResolution ReceiveStrike(DamageContext context,ShibuyaStrikeDefinition strike)
        {
            var previous=incomingGuardStrike;incomingGuardStrike=strike;
            try{return Health.ReceiveDamage(context);}finally{incomingGuardStrike=previous;}
        }
        private DamageGuardDecision EvaluateHeldGuard(DamageContext context)
        {
            if(receivingGrabFrom!=null)return DamageGuardDecision.NoDecision;
            EnsureGuard();
            if(!guarding||guardSettings==null||!guardSettings.guardEnabled||Health.IsDead||stopped||!IsGrounded
                ||IsActionLocked||IsInHitStop||IsInCinematic||context.Source==null
                ||context.DeliveryType!=DamageDeliveryType.PhysicalStrike||context.HasTrait(DamageTraits.Unblockable))return DamageGuardDecision.NoDecision;
            if(TryParry(context))return DamageGuardDecision.Block;
            if(incomingGuardStrike!=null&&!incomingGuardStrike.guardable)return DamageGuardDecision.NoDecision;
            Vector3 toward=context.Source.transform.position-transform.position;toward.y=0;
            if(toward.sqrMagnitude<.000001f||Vector3.Angle(transform.forward,toward)>Mathf.Clamp(guardSettings.halfAngle,0,90))return DamageGuardDecision.NoDecision;
            float cost=Mathf.Max(0,incomingGuardStrike!=null?incomingGuardStrike.guardDamage:guardSettings.fallbackGuardDamage);
            currentGuard=Mathf.Max(0,currentGuard-cost);
            if(currentGuard<=0) {
                EndGuard();guardLockedUntil=combatTime+Mathf.Max(0,guardSettings.breakLockDuration);
                stunnedUntil=Mathf.Max(stunnedUntil,guardLockedUntil);GuardBreakCount++;
                GuardChanged?.Invoke(this);GuardBroken?.Invoke(this);return DamageGuardDecision.NoDecision;
            }
            GuardBlockCount++;GuardChanged?.Invoke(this);GuardBlocked?.Invoke(this);return DamageGuardDecision.Block;
        }
    }
}
