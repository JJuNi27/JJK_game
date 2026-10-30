using System;
using JJKGame.Core;
using UnityEngine;
namespace JJKGame.Shibuya
{
    public enum ShibuyaGrabPhase { Idle, Startup, Active, Contact, SuccessRecovery, FailureRecovery }
    public sealed partial class ShibuyaFighter
    {
        private ShibuyaGrabProfile activeGrab;
        private ShibuyaFighter grabTarget,grabSource,receivingGrabFrom;
        private float grabStartedAt,grabContactAt,grabRecoverUntil;
        private Vector3 grabOrigin,grabForward,grabTargetOrigin;
        private bool grabQueried;
        public ShibuyaGrabProfile GrabSettings=>profile!=null?profile.grab:null;
        public ShibuyaGrabPhase GrabPhase {get;private set;}
        public bool IsGrabBusy=>GrabPhase!=ShibuyaGrabPhase.Idle||grabSource!=null;
        public bool IsGrabbed=>grabSource!=null;
        public float GrabElapsed=>Mathf.Max(0,combatTime-grabStartedAt);
        public int GrabAttemptCount {get;private set;}
        public int GrabSuccessCount {get;private set;}
        public int GrabFailureCount {get;private set;}
        public event Action<ShibuyaFighter> GrabStarted,GrabSucceeded,GrabFailed;
        public bool TryGrab()
        {
            var p=GrabSettings;
            if(!IsGuarding||p==null||!p.grabEnabled||!isActiveAndEnabled||stopped||Health.IsDead
                ||!IsGrounded||attacking||buffered||airBuffered||IsActionLocked||IsInHitStop
                ||IsInCinematic||airSession||chasing||chaseQueued||groundDashing||IsGuardBroken||FocusingEnergy)return false;
            CancelGuardInput();CancelCombo();ClearIncomingLink();ClearVictimAlignment();
            activeGrab=p;grabStartedAt=combatTime;grabForward=transform.forward;
            grabOrigin=transform.position;grabQueried=false;GrabPhase=ShibuyaGrabPhase.Startup;
            jumpIntent=false;moveIntent=Vector3.zero;GrabAttemptCount++;GrabStarted?.Invoke(this);return true;
        }
        private DamageContext GrabDamageContext(ShibuyaFighter target)=>new DamageContext(
            Mathf.Max(.1f,activeGrab.damage),gameObject,DamageDeliveryType.PhysicalStrike,
            DamageTraits.None,"SHIBUYA GRAB",target.motor.bounds.center);
        private bool ValidGrabTarget(ShibuyaFighter target,bool held=false)
        {
            if(!IsLiveTarget(target)||target==this||!target.IsGrounded||target.airborne||target.downed
                ||target.gettingUp||target.airSession||target.chasing||target.chaseQueued||target.IsInCinematic
                ||target.IsInHitStop||target.Health.IsInvulnerable||target.IsGroundDashEvading||target.IsQuickRecoveryEvading
                ||(held?target.grabSource!=this:target.IsGrabBusy||target.IsStunned||target.IsGuardBroken))return false;
            Vector3 delta=target.motor.bounds.center-motor.bounds.center;
            if(delta.magnitude>Mathf.Max(.1f,activeGrab.range)
                ||Vector3.Angle(grabForward,Vector3.ProjectOnPlane(delta,Vector3.up))>Mathf.Clamp(activeGrab.halfAngle,0,90)
                ||WorldBlocks(motor.bounds.center,target.motor.bounds.center))return false;
            // Only this Fighter's ordinary guard/parry are bypassed. Other damage guards keep their meaning.
            var context=GrabDamageContext(target);
            foreach(var b in target.GetComponents<MonoBehaviour>())
                if(b!=null&&b.isActiveAndEnabled&&b!=target&&b is IDamageGuard g&&g.EvaluateDamage(context)==DamageGuardDecision.Block)return false;
            return true;
        }
        private ShibuyaFighter FindGrabTarget()
        {
            var locked=ExplicitLockTarget;if(ValidGrabTarget(locked))return locked;
            ShibuyaFighter best=null;float distance=float.PositiveInfinity;
            foreach(var c in Physics.OverlapSphere(motor.bounds.center,Mathf.Max(.1f,activeGrab.range),~0,QueryTriggerInteraction.Ignore)) {
                var t=c.GetComponentInParent<ShibuyaFighter>();if(!ValidGrabTarget(t))continue;
                float d=(t.motor.bounds.center-motor.bounds.center).sqrMagnitude;
                if(d<distance||(Mathf.Approximately(d,distance)&&(best==null||t.GetInstanceID()<best.GetInstanceID()))){best=t;distance=d;}
            }
            return best;
        }
        private void UpdateGrab()
        {
            if(GrabPhase==ShibuyaGrabPhase.Idle)return;
            if(GrabPhase==ShibuyaGrabPhase.SuccessRecovery||GrabPhase==ShibuyaGrabPhase.FailureRecovery){
                if(combatTime>=grabRecoverUntil)GrabPhase=ShibuyaGrabPhase.Idle;return;
            }
            if(activeGrab==null||GrabSettings!=activeGrab||!activeGrab.grabEnabled||!IsGrounded||IsInCinematic||airborne||downed||airSession||IsStunned){InterruptGrab();return;}
            if((transform.position-grabOrigin).sqrMagnitude>activeGrab.range*activeGrab.range){FailGrab();return;}
            if(GrabPhase==ShibuyaGrabPhase.Contact){
                if(!ValidGrabTarget(grabTarget,true)||(grabTarget.transform.position-grabTargetOrigin).magnitude>motor.skinWidth+.1f){FailGrab();return;}
                if(combatTime>=grabContactAt+Mathf.Max(0,activeGrab.contactDuration))CompleteGrab();return;
            }
            if(GrabElapsed<Mathf.Max(.01f,activeGrab.startup))return;
            float end=Mathf.Max(.01f,activeGrab.startup)+Mathf.Max(.01f,activeGrab.activeTime);
            // Do not lose the first active crossing when one Editor frame spans the short window.
            if(!grabQueried||GrabElapsed<=end){
                GrabPhase=ShibuyaGrabPhase.Active;grabQueried=true;var t=FindGrabTarget();
                if(t!=null){
                    grabTarget=t;t.grabSource=this;grabTargetOrigin=t.transform.position;
                    t.CancelQuickRecovery();t.CancelGuardInput();t.CancelGroundDash();t.CancelCombo();t.ClearIncomingLink();t.ClearVictimAlignment();
                    t.jumpIntent=false;t.knockback=Vector3.zero;t.distanceKnockbackRemaining=0;
                    GrabPhase=ShibuyaGrabPhase.Contact;grabContactAt=combatTime;return;
                }
            }
            if(GrabElapsed>=end)FailGrab();
        }
        private void CompleteGrab()
        {
            var t=grabTarget;var p=activeGrab;var context=GrabDamageContext(t);
            ReleaseGrabTarget(); // Health/death/match callbacks must never retain paired ownership.
            GrabPhase=ShibuyaGrabPhase.SuccessRecovery;grabRecoverUntil=combatTime+Mathf.Max(0,p.successRecovery);
            var previous=t.receivingGrabFrom;t.receivingGrabFrom=this;DamageResolution result;
            try{result=t.Health.ReceiveDamage(context);}finally{t.receivingGrabFrom=previous;}
            if(result!=DamageResolution.Applied){FailGrab();return;}GrabSuccessCount++;
            if(IsLiveTarget(t)){
                Vector3 away=Vector3.ProjectOnPlane(t.transform.position-transform.position,Vector3.up).normalized;
                if(away.sqrMagnitude<.000001f)away=grabForward;
                t.ApplyHitReaction(Vector3.zero,Mathf.Max(.01f,p.victimStun),transform.position,away,false,false);
                t.distanceKnockbackDirection=away;t.distanceKnockbackRemaining=Mathf.Max(0,p.pushDistance);
                t.distanceKnockbackSpeed=t.distanceKnockbackRemaining/Mathf.Max(.01f,p.victimStun);
            }
            GrabSucceeded?.Invoke(this);
        }
        private void ReleaseGrabTarget(){if(grabTarget!=null&&grabTarget.grabSource==this)grabTarget.grabSource=null;grabTarget=null;}
        private void FailGrab()
        {
            ReleaseGrabTarget();if(GrabPhase==ShibuyaGrabPhase.Idle||GrabPhase==ShibuyaGrabPhase.FailureRecovery)return;
            GrabPhase=ShibuyaGrabPhase.FailureRecovery;grabRecoverUntil=combatTime+(activeGrab!=null?Mathf.Max(0,activeGrab.failureRecovery):0);
            GrabFailureCount++;GrabFailed?.Invoke(this);
        }
        private void InterruptGrab()
        {
            if(grabSource!=null){var source=grabSource;grabSource=null;source.FailGrab();}
            if(GrabPhase!=ShibuyaGrabPhase.SuccessRecovery)FailGrab();
        }
        public void CancelGrabInput()=>InterruptGrab();
        private void ResetGrab(){InterruptGrab();ReleaseGrabTarget();grabSource=null;GrabPhase=ShibuyaGrabPhase.Idle;}
    }
}
