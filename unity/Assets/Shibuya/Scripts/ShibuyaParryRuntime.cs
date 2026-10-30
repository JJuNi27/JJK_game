using System;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Shibuya
{
    public sealed partial class ShibuyaFighter
    {
        private double parryUntil,parryReadyAt;
        private float parryPoseUntil,parryPoseStarted;
        private ShibuyaParryProfile attemptedParry;
        public ShibuyaParryProfile ParrySettings=>GuardSettings!=null?GuardSettings.parry:null;
        public bool IsParryWindowOpen=>IsGuarding&&ParrySettings!=null&&ParrySettings.parryEnabled
            &&attemptedParry==ParrySettings&&Time.timeAsDouble<parryUntil&&!IsInHitStop;
        public bool IsParryRecovering=>ParrySettings!=null&&isActiveAndEnabled&&!Health.IsDead&&!stopped
            &&!IsInCinematic&&!IsStunned&&!airborne&&!downed&&combatTime<parryPoseUntil;
        public float ParryMotionProgress=>Mathf.InverseLerp(parryPoseStarted,parryPoseUntil,combatTime);
        public float ParryRetryRemaining=>Mathf.Max(0,(float)(parryReadyAt-Time.timeAsDouble));
        public int ParryAttemptCount {get;private set;}
        public int ParrySuccessCount {get;private set;}
        public Vector3 ParryContactWorld {get;private set;}
        public Vector3 ParryContactNormal {get;private set;}
        public event Action<ShibuyaFighter> ParrySucceeded;
        private void BeginParryAttempt()
        {
            var p=ParrySettings;
            if(p==null||!p.parryEnabled||p.windowDuration<=0||Time.timeAsDouble<parryReadyAt)return;
            attemptedParry=p;parryUntil=Time.timeAsDouble+Mathf.Max(0,p.windowDuration);
            // Never rearm a still-open window, even with an unusually short configured retry.
            parryReadyAt=Time.timeAsDouble+Mathf.Max(p.windowDuration,Mathf.Max(0,p.retryInterval));
            ParryAttemptCount++;
        }
        private bool TryParry(DamageContext context)
        {
            if(!IsParryWindowOpen||(incomingGuardStrike!=null&&!incomingGuardStrike.parryable))return false;
            var attacker=context.Source.GetComponentInParent<ShibuyaFighter>();
            if(attacker==null||attacker==this||!IsLiveTarget(attacker)||attacker.IsInCinematic)return false;
            Vector3 toward=context.Source.transform.position-transform.position;toward.y=0;
            var p=attemptedParry;
            if(toward.sqrMagnitude<.000001f||Vector3.Angle(transform.forward,toward)>Mathf.Clamp(p.halfAngle,0,90))return false;
            EndGuard();jumpIntent=false;parryPoseStarted=combatTime;parryPoseUntil=combatTime+Mathf.Max(0,p.successMotionDuration);
            ParryContactNormal=toward.normalized;
            ParryContactWorld=(context.HitPoint==Vector3.zero?transform.position+Vector3.up*GuardSettings.shieldCenterHeight:context.HitPoint)
                +ParryContactNormal*GuardSettings.shieldRadius*.5f;
            ParrySuccessCount++;
            attacker.ApplyParryRecoil(this,p);
            if(p.localHitStop>0){ApplyLocalHitStop(p.localHitStop);attacker.ApplyLocalHitStop(p.localHitStop);}
            ParrySucceeded?.Invoke(this);return true;
        }
        private void ApplyParryRecoil(ShibuyaFighter defender,ShibuyaParryProfile p)
        {
            Vector3 away=transform.position-defender.transform.position;away.y=0;
            if(away.sqrMagnitude<.000001f)away=-transform.forward;
            away.Normalize();
            // Reuse cancellation/reaction without touching Health or normal hit counters.
            ApplyHitReaction(Vector3.zero,Mathf.Max(0,p.attackerStun),defender.transform.position,away,false,false);
            distanceKnockbackRemaining=Mathf.Max(0,p.pushDistance);
            distanceKnockbackSpeed=distanceKnockbackRemaining/Mathf.Max(.01f,p.attackerStun);
            distanceKnockbackDirection=away;
        }
    }
}
