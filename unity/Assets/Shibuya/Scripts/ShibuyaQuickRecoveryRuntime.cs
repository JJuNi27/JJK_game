using JJKGame.Core;
using UnityEngine;
namespace JJKGame.Shibuya
{
    public sealed partial class ShibuyaFighter
    {
        private bool quickRecovering;
        private double recoveryStartedAt,recoveryReadyAt;
        private float recoveryDistance,recoveryDuration,recoveryConsumed,recoveryEvadeStart,recoveryEvadeEnd,recoveryCooldown;
        private float recoveryPoseStrength,recoveryPoseDuration,recoveryDownAngle;
        private Quaternion recoveryFacing;
        private ShibuyaQuickRecoveryProfile activeRecovery;
        private readonly float[] recoveryIntegral=new float[65];
        public ShibuyaQuickRecoveryProfile QuickRecoverySettings=>profile!=null?profile.quickRecovery:null;
        public bool IsQuickRecovering=>quickRecovering;
        public Vector3 QuickRecoveryDirection {get;private set;}
        public float QuickRecoveryElapsed=>Mathf.Max(0,(float)(Time.timeAsDouble-recoveryStartedAt));
        public float QuickRecoveryProgress=>quickRecovering?Mathf.Clamp01(QuickRecoveryElapsed/recoveryDuration):0;
        public float QuickRecoveryCooldownRemaining=>Mathf.Max(0,(float)(recoveryReadyAt-Time.timeAsDouble));
        public float QuickRecoveryRequestedDistance=>recoveryConsumed;
        public float QuickRecoveryPoseWeight=>quickRecovering?1-Mathf.SmoothStep(0,1,QuickRecoveryElapsed/recoveryPoseDuration):0;
        public float QuickRecoveryPoseAngle=>recoveryDownAngle;
        public float QuickRecoveryPoseStrength=>recoveryPoseStrength;
        public int QuickRecoveryCount {get;private set;}
        public bool IsQuickRecoveryEvading=>quickRecovering&&isActiveAndEnabled&&!Health.IsDead&&!stopped
            &&!IsStunned&&!IsInHitStop&&!IsInCinematic&&!IsGrabBusy&&IsGrounded&&TryLandingSurface(out _)
            &&QuickRecoveryElapsed>=recoveryEvadeStart&&QuickRecoveryElapsed<recoveryEvadeEnd;
        public bool TryQuickRecovery(Vector2 rawDirection,Vector3 cameraForward,Vector3 cameraRight)
        {
            var p=QuickRecoverySettings;
            if(!isActiveAndEnabled||p==null||!p.recoveryEnabled||!CanRequestDownRecovery||quickRecovering
                ||!IsGrounded||!TryLandingSurface(out _)||IsStunned||IsInHitStop||IsInCinematic||IsGrabBusy
                ||IsParryRecovering||attacking||airSession||chasing||chaseQueued||Time.timeAsDouble<recoveryReadyAt
                ||rawDirection.sqrMagnitude<.000001f||!float.IsFinite(p.distance)||p.distance<=0||!float.IsFinite(p.duration)||p.duration<=0)return false;
            // Same horizontal camera basis as PASS4C.2, kept independent of its protected dash runtime.
            Vector3 forward=Vector3.ProjectOnPlane(cameraForward,Vector3.up);
            Vector3 right=Vector3.ProjectOnPlane(cameraRight,Vector3.up);
            if(forward.sqrMagnitude<.000001f)forward=Vector3.Cross(right,Vector3.up);
            if(forward.sqrMagnitude<.000001f)forward=Vector3.ProjectOnPlane(transform.forward,Vector3.up);
            if(forward.sqrMagnitude<.000001f)forward=Vector3.forward;
            forward.Normalize();if(right.sqrMagnitude<.000001f)right=Vector3.Cross(Vector3.up,forward);else right.Normalize();
            var input=rawDirection.normalized;QuickRecoveryDirection=(forward*input.y+right*input.x).normalized;
            recoveryDownAngle=DownPoseAngle;
            CancelCombo();ClearIncomingLink();ClearVictimAlignment();ResetGrab();CancelGuardInput();CancelGroundDash();
            downed=gettingUp=downLanded=false;downStartedAt=getUpStartedAt=0;
            jumpIntent=false;moveIntent=knockback=Vector3.zero;distanceKnockbackRemaining=0;
            if(groundImpact!=null)groundImpact.Clear();
            var aura=GetComponent<CursedEnergyAuraController>();if(aura!=null)aura.Clear();
            var charge=GetComponent<ShibuyaEnergyCharge>();if(charge!=null)charge.CancelCharge();
            activeRecovery=p;recoveryDistance=p.distance;recoveryDuration=Mathf.Max(.01f,p.duration);
            recoveryEvadeStart=SafeNonnegative(p.evadeStart);recoveryEvadeEnd=Mathf.Clamp(SafeNonnegative(p.evadeEnd),recoveryEvadeStart,recoveryDuration);
            recoveryCooldown=SafeNonnegative(p.cooldownAfterEnd);recoveryPoseStrength=SafeNonnegative(p.poseStrength);recoveryPoseDuration=Mathf.Max(.01f,p.poseDuration);
            recoveryIntegral[0]=0;
            for(int i=1;i<65;i++){
                float a=p.speedCurve!=null?SafeNonnegative(p.speedCurve.Evaluate((i-1)/64f)):1;
                float b=p.speedCurve!=null?SafeNonnegative(p.speedCurve.Evaluate(i/64f)):1;
                recoveryIntegral[i]=recoveryIntegral[i-1]+(a+b)/128f;
            }
            float total=recoveryIntegral[64];for(int i=1;i<65;i++)recoveryIntegral[i]=total>.000001f?recoveryIntegral[i]/total:i/64f;
            recoveryConsumed=0;recoveryFacing=transform.rotation;recoveryStartedAt=Time.timeAsDouble;
            quickRecovering=true;QuickRecoveryCount++;return true;
        }
        private Vector3 AdvanceQuickRecovery()
        {
            if(!quickRecovering)return Vector3.zero;
            // Only a verified grounded down can START this finite motion. Carry its remaining
            // horizontal step over edges while the existing motor applies gravity independently.
            // Cutting X/Z at the first missing support can strand the rounded capsule on a rim.
            // Airborne support never grants evade immunity, extra duration, or a new air command.
            if(IsStunned||IsInCinematic||IsGrabBusy||QuickRecoverySettings!=activeRecovery
                ||activeRecovery==null||!activeRecovery.recoveryEnabled){CancelQuickRecovery();return Vector3.zero;}
            transform.rotation=recoveryFacing;
            float phase=Mathf.Clamp01(QuickRecoveryElapsed/recoveryDuration)*64;int i=Mathf.Min(63,Mathf.FloorToInt(phase));
            float target=recoveryDistance*Mathf.Lerp(recoveryIntegral[i],recoveryIntegral[i+1],phase-i);
            float amount=Mathf.Max(0,target-recoveryConsumed);recoveryConsumed=target;
            if(phase>=64)CancelQuickRecovery();return QuickRecoveryDirection*amount;
        }
        public void CancelQuickRecovery()
        {
            if(!quickRecovering)return;quickRecovering=false;recoveryReadyAt=Time.timeAsDouble+recoveryCooldown;jumpIntent=false;
        }
        private DamageGuardDecision EvaluateQuickRecoveryDamage(DamageContext context)=>IsQuickRecoveryEvading
            &&context.DeliveryType==DamageDeliveryType.PhysicalStrike&&!context.HasTrait(DamageTraits.Unblockable)
            ?DamageGuardDecision.Block:DamageGuardDecision.NoDecision;
    }
}
