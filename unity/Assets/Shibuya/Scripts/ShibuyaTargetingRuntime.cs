using System;
using UnityEngine;

namespace JJKGame.Shibuya
{
    [Serializable]
    public sealed class ShibuyaTargetingSettings
    {
        [Min(0)] public float targetRange = 2.7f;
        [Range(0, 89)] public float acquisitionAngle = 50f;
        [Range(0, 180)] public float retentionAngle = 70f;
        [Range(0, 180)] public float maxTurnAngle = 45f;
        [Min(0)] public float turnSpeed = 540f;
        [Min(0)] public float approachDistance = .30f;
        [Min(0)] public float approachSpeed = 4f;
        [Min(0)] public float preparationTime = .16f;
        [Min(0)] public float retentionTime = 1.2f;
        [Min(0)] public float maxHeightDifference = 1.4f;
        [Min(0)] public float predictionTime = .05f;
    }

    public sealed partial class ShibuyaFighter
    {
        private ShibuyaFighter comboTarget;
        private readonly ShibuyaTargetingSettings inheritedTargeting = new ShibuyaTargetingSettings();
        private float targetLastHitAt;
        private bool comboHasContact;
        private ShibuyaFighter[] eventAim;
        private bool[] aimStarted, aimStopped, aimIsExplicit;
        private float[] aimTurns, aimDistances;
        private Vector3[] aimForward;
        public ShibuyaFighter ComboTarget => IsLiveTarget(comboTarget) ? comboTarget : null;
        public string TargetingReason { get; private set; } = "대상 없음";

        private ShibuyaTargetingSettings Targeting(ShibuyaStrikeDefinition strike)
        {
            if (strike.overrideTargeting && strike.targeting != null) return strike.targeting;
            var value=inheritedTargeting;
            value.targetRange=profile.trackingTargetRange; value.acquisitionAngle=profile.firstTargetCone;
            value.retentionAngle=profile.trackingConeAngle; value.maxTurnAngle=profile.trackingMaxTurnAngle;
            value.turnSpeed=profile.trackingTurnSpeed; value.approachDistance=profile.trackingAdvanceDistance;
            value.approachSpeed=profile.trackingAdvanceSpeed; value.preparationTime=profile.trackingDuration;
            value.retentionTime=profile.targetRetentionTime; value.maxHeightDifference=profile.trackingMaxHeightDifference;
            value.predictionTime=profile.trackingPredictionTime;
            return value;

        }
        private void PrepareTargeting()
        {
            int n=activeStep.hits.Length;
            eventAim=new ShibuyaFighter[n]; aimStarted=new bool[n]; aimStopped=new bool[n];
            aimIsExplicit=new bool[n];
            aimTurns=new float[n]; aimDistances=new float[n]; aimForward=new Vector3[n];
            // Snapshot events that begin with the accepted attack before another input can
            // reselect the lock. Delayed events snapshot at their own preparation start.
            for (int i=0;i<n;i++) if (activeStep.hits[i].startTime<=0) SnapshotEventAim(i);
        }
        private void SnapshotEventAim(int i)
        {
            aimStarted[i]=true; aimForward[i]=transform.forward;
            if(executingCounter){eventAim[i]=counterLeaseTarget;return;}
            var evt=activeStep.hits[i];
            if(evt.requiredMovement>=0) return; // Authored movement owns its target.
            if(executingAirAttack) {
                // A legal ongoing air contact/pursuit owns the sequence before a new lock.
                var owned=LegalAerialContact(airContactTarget)?airContactTarget:
                    ValidAerialTarget()?aerialTarget:null;
                if(owned!=null) { eventAim[i]=owned; return; }
            }
            eventAim[i]=LockedMeleeTarget(evt.strike,transform.forward);
            aimIsExplicit[i]=eventAim[i]!=null;
            if(executingAirAttack || eventAim[i]!=null) return;
            var settings=Targeting(evt.strike);
            if(comboHasContact) {
                if(combatTime-targetLastHitAt<=settings.retentionTime) eventAim[i]=ComboTarget;
            } else eventAim[i]=AcquireTarget(evt.strike,settings);
        }
        private static bool IsLiveTarget(ShibuyaFighter value) => value != null && value.isActiveAndEnabled
            && !value.Health.IsDead && !value.stopped;

        private bool ValidAim(ShibuyaFighter target, ShibuyaStrikeDefinition strike,
            ShibuyaTargetingSettings settings, Vector3 forward, bool acquiring)
        {
            if (!IsLiveTarget(target)) { TargetingReason="대상 사망/종료"; return false; }
            if (target.Health.IsInvulnerable) { TargetingReason="대상 무적"; return false; }
            Vector3 flat=target.transform.position-transform.position; flat.y=0;
            float angle=Vector3.Angle(forward, flat);
            if (flat.magnitude>settings.targetRange || angle>(acquiring?settings.acquisitionAngle:settings.retentionAngle)) {
                TargetingReason="거리/각도 이탈"; return false;
            }
            if (Mathf.Abs(target.transform.position.y-transform.position.y)>settings.maxHeightDifference) {
                TargetingReason="높이 차이"; return false;
            }
            Vector3 origin=transform.position+Vector3.up*strike.attackHeight;
            if (WorldBlocks(origin,target.transform.position+Vector3.up*strike.attackHeight)) {
                TargetingReason="장애물 차단"; return false;
            }
            if (acquiring) {
                // Evaluate the real hit sphere after bounded rotation and physical preparation travel.
                Vector3 heading=Vector3.RotateTowards(forward,flat.normalized,
                    Mathf.Min(settings.maxTurnAngle,settings.turnSpeed*Mathf.Min(strike.Startup,settings.preparationTime))*Mathf.Deg2Rad,0);
                float travel=Mathf.Min(strike.advanceDistance,strike.advanceSpeed*strike.Startup)
                    +Mathf.Min(settings.approachDistance,settings.approachSpeed*strike.Startup);
                Vector3 reachable=origin+heading*(strike.attackForwardOffset+travel);
                if (Vector3.Distance(target.motor.bounds.ClosestPoint(reachable),reachable)>strike.attackRadius) {
                    TargetingReason="실제 판정 도달 불가"; return false;
                }
            }
            return true;
        }
        private ShibuyaFighter AcquireTarget(ShibuyaStrikeDefinition strike, ShibuyaTargetingSettings settings)
        {
            ShibuyaFighter best=null; float score=float.PositiveInfinity;
            foreach(var collider in Physics.OverlapSphere(transform.position,settings.targetRange,~0,QueryTriggerInteraction.Ignore)) {
                var target=collider.GetComponentInParent<ShibuyaFighter>();
                if(target==this || !ValidAim(target,strike,settings,transform.forward,true)) continue;
                Vector3 flat=target.transform.position-transform.position; flat.y=0;
                float value=flat.magnitude+Vector3.Angle(transform.forward,flat)/90f;
                if(value<score || (Mathf.Approximately(value,score) && best!=null && target.GetInstanceID()<best.GetInstanceID())) {
                    score=value; best=target;
                }
            }
            return best;
        }
        private Vector3 AdvanceTargeting(float delta)
        {
            if(!attacking || airSession || executingCounter) return Vector3.zero;
            Vector3 result=Vector3.zero;
            for(int i=0;i<activeStep.hits.Length;i++) {
                var evt=activeStep.hits[i]; var strike=evt.strike; var settings=Targeting(strike);
                float elapsed=AttackElapsed-evt.startTime;
                if(elapsed<0 || aimStopped[i]) continue;
                // Authored ApproachLastHit owns its turn and travel, never stack two movement budgets.
                if(evt.requiredMovement>=0) continue;
                if(!aimStarted[i]) SnapshotEventAim(i);
                if(aimIsExplicit[i]) TurnLockedAttack(i,strike,elapsed,delta);
                if(!profile.trackingEnabled) continue;
                var target=eventAim[i];
                if(target==null) { TargetingReason="유효 대상 없음"; aimStopped[i]=true; continue; }
                bool overrideInput=moveIntent.sqrMagnitude>.001f
                    && Vector3.Angle(moveIntent,initialAttackForward)>profile.directionalInputOverrideAngle;
                if((!aimIsExplicit[i] && overrideInput) || (aimIsExplicit[i]
                    ? !ValidLockedAttackTarget(target,strike)
                    : !ValidAim(target,strike,settings,aimForward[i],false))
                    || (!aimIsExplicit[i] && comboHasContact && combatTime-targetLastHitAt>settings.retentionTime)) {
                    if(overrideInput) TargetingReason="명시적 방향 입력 우선";
                    aimStopped[i]=true; continue;
                }
                float window=Mathf.Min(strike.Startup,settings.preparationTime);
                float dt=Mathf.Max(0,Mathf.Min(elapsed,window)-Mathf.Max(0,elapsed-delta));
                if(dt<=0) continue; // Never rotate toward an escape after attack activation.
                Vector3 flat=target.transform.position-transform.position; flat.y=0;
                Vector3 velocity=target.motor.velocity; velocity.y=0;
                Vector3 predicted=flat+Vector3.ClampMagnitude(velocity*settings.predictionTime,settings.approachDistance);
                if(predicted.sqrMagnitude<.000001f) continue;
                if(!aimIsExplicit[i]) {
                    var old=transform.rotation;
                    float turn=Mathf.Min(Mathf.Max(0,settings.maxTurnAngle-aimTurns[i]),settings.turnSpeed*dt);
                    transform.rotation=Quaternion.RotateTowards(old,Quaternion.LookRotation(predicted),turn);
                    float applied=Quaternion.Angle(old,transform.rotation); aimTurns[i]+=applied; turnConsumed+=applied;
                } else if(!ValidAim(target,strike,settings,transform.forward,true)) continue;
                float clearance=motor.radius+target.motor.radius+motor.skinWidth;
                float amount=Mathf.Min(Mathf.Max(0,settings.approachDistance-aimDistances[i]),
                    Mathf.Min(settings.approachSpeed*dt,Mathf.Max(0,flat.magnitude-clearance)));
                aimDistances[i]+=amount; correctionConsumed+=amount;
                result+=transform.forward*amount; TargetingReason="준비 중 제한 보정";
            }
            return result;
        }
        private void RecordComboContact(ShibuyaFighter target)
        {
            if(!comboHasContact) { comboTarget=target; comboHasContact=true; }
            if(target==comboTarget) targetLastHitAt=combatTime;
        }
        private void ResetComboTarget() { comboTarget=null; comboHasContact=false; targetLastHitAt=0; }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if(profile==null || !profile.showCombatGizmos) return;
            var strike=CurrentStrike??profile.strikes[0]; var settings=Targeting(strike);
            Vector3 origin=transform.position+Vector3.up*strike.attackHeight;
            Gizmos.color=Color.yellow; Gizmos.DrawWireSphere(origin+transform.forward*strike.attackForwardOffset,strike.attackRadius);
            Gizmos.color=Color.cyan;
            for(int sign=-1;sign<=1;sign+=2) Gizmos.DrawRay(origin,
                Quaternion.AngleAxis(settings.acquisitionAngle*sign,Vector3.up)*transform.forward*settings.targetRange);
            if(ComboTarget!=null) { Gizmos.color=Color.green; Gizmos.DrawLine(origin,ComboTarget.transform.position+Vector3.up); }
            UnityEditor.Handles.Label(origin+Vector3.up,TargetingReason);
        }
#endif
    }
}
