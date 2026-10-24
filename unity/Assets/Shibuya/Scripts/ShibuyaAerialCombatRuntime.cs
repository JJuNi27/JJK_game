using UnityEngine;

namespace JJKGame.Shibuya
{
    public sealed partial class ShibuyaFighter
    {
        private ShibuyaFighter aerialTarget;
        private ShibuyaAerialCombatDefinition aerialSettings;
        private int launchVersion, damageVersion, offeredLaunch, offeredDamage, offeredAttack=-1;
        private float opportunityUntil, chaseBufferUntil, chaseElapsed, chaseTravel, chaseTurn, chaseStalled, arrivedAt, launchRootY;
        private bool chaseQueued, chaseConsumed, airSession, chasing, airAttackUsed, executingAirAttack, leftGround;
        private Vector3 chaseAxis, chaseFrameRequest;
        public bool IsChasing => chasing;
        public bool IsInAirSession => airSession;
        public bool IsAirAttacking => executingAirAttack;
        public bool HasAirChaseOpportunity => !chaseConsumed && ValidAerialTarget() && combatTime<=opportunityUntil;
        public bool HasBufferedAirChase => chaseQueued && combatTime<=chaseBufferUntil && HasAirChaseOpportunity;
        public bool CanAirAttack => enabled && !Health.IsDead && !stopped && !IsActionLocked && !attacking && !motor.isGrounded
            && (!airSession ? AerialDefinition != null && AerialDefinition.IsValid
                : !chasing && WithinAirHoverBudget && (!airAttackUsed
                    ? ValidAerialTarget() && combatTime<=arrivedAt+aerialSettings.aerialInputWindow
                    : airGrace && combatTime<=airGraceUntil && LegalAerialContact(airContactTarget)));
        public ShibuyaFighter AirChaseTarget => ValidAerialTarget()?aerialTarget:null;
        public float AirChaseTravel => chaseTravel;
        public int AirChaseCount {get;private set;}
        public int AirChaseContactCount {get;private set;}
        public string AirChaseReason {get;private set;}="추격 기회 없음";
        public ShibuyaAerialCombatDefinition AerialDefinition => moveset!=null && moveset.aerialCombat!=null
            ?moveset.aerialCombat:profile==null?null:profile.aerialCombat;
        public float ChaseLeanAngle => chasing && aerialSettings!=null?aerialSettings.chaseLeanAngle:0;

        private bool ValidAerialTarget() => aerialSettings!=null && aerialSettings.chaseEnabled
            && IsLiveTarget(aerialTarget) && aerialTarget.airborne && !aerialTarget.Health.IsInvulnerable
            && aerialTarget.launchVersion==offeredLaunch && aerialTarget.damageVersion==offeredDamage;

        private void OfferAirChase(ShibuyaFighter victim,ShibuyaHitEvent hit)
        {
            if(executingAirAttack || CurrentFinisher!=ShibuyaFinisher.Uppercut
                || hit.outcome!=ShibuyaHitOutcome.Launch || offeredAttack==AcceptedAttackCount) return;
            var settings=AerialDefinition;
            if(settings==null || !settings.chaseEnabled || !settings.IsValid || !victim.airborne) return;
            ClearAerialState();
            aerialTarget=victim; aerialSettings=settings;offeredLaunch=victim.launchVersion;
            offeredDamage=victim.damageVersion; offeredAttack=AcceptedAttackCount;
            launchRootY=victim.transform.position.y;
            opportunityUntil=combatTime+settings.chaseInputWindow;
            AirChaseReason="어퍼컷 적중: Space 입력 가능";
        }

        private bool QueueAirChase()
        {
            if(!enabled || Health.IsDead || stopped || IsActionLocked || !HasAirChaseOpportunity || airSession) return false;
            Vector3 to=aerialTarget.transform.position-transform.position;
            if(to.magnitude>aerialSettings.chaseMaxRange || !ChasePathClear(ChaseDestination()-transform.position)) {
                AirChaseReason="거리/경로 제한";return false;
            }
            chaseQueued=true;chaseBufferUntil=combatTime+aerialSettings.chaseInputBuffer;
            jumpIntent=false;AirChaseReason="Space 예약: 국소 히트스톱 종료 대기";return true;
        }

        private void RefreshAerialState()
        {
            RefreshAerialHover();
            if(airSession && airAttackUsed) return; // The shared air attack no longer requires a launch token.
            if(aerialTarget==null) return;
            if(!ValidAerialTarget()) {
                if(airSession) EndChaseToFall("대상 착지/사망/무적/다른 타격으로 종료");
                else ClearAerialState();
                return;
            }
            if(!airSession && combatTime>opportunityUntil) {ClearAerialState();return;}
            if(chaseQueued && combatTime>chaseBufferUntil) chaseQueued=false;
        }

        private void StartQueuedChase()
        {
            if(!HasBufferedAirChase || IsActionLocked) return;
            if(Vector3.Distance(transform.position,aerialTarget.transform.position)>aerialSettings.chaseMaxRange
                || !ChasePathClear(ChaseDestination()-transform.position)) {chaseQueued=false;return;}
            CancelCombo();
            chaseQueued=false;chaseConsumed=true;airSession=chasing=true;airAttackUsed=false;leftGround=false;
            chaseElapsed=chaseTravel=chaseTurn=chaseStalled=0;verticalSpeed=0;knockback=Vector3.zero;
            Vector3 flat=aerialTarget.transform.position-transform.position;flat.y=0;
            chaseAxis=flat.sqrMagnitude>.000001f?flat.normalized:transform.forward;
            AirChaseCount++;AirChaseReason="실제 3D 공중 추격";
        }

        private Vector3 ChaseDestination()
        {
            var target=aerialTarget;
            Vector3 flat=target.transform.position-transform.position;flat.y=0;
            Vector3 axis=chasing?chaseAxis:(flat.sqrMagnitude>.000001f?flat.normalized:transform.forward);
            Vector3 velocity=target.motor.velocity;velocity.y=target.verticalSpeed;
            Vector3 prediction=Vector3.ClampMagnitude(velocity*aerialSettings.chasePredictionTime,aerialSettings.chasePredictionLimit);
            float clearance=Mathf.Max(aerialSettings.chaseArrivalDistance,motor.radius+target.motor.radius+motor.skinWidth*2);
            return target.transform.position+prediction-axis*clearance;
        }

        private bool ChasePathClear(Vector3 displacement)
        {
            if(displacement.sqrMagnitude<.000001f) return true;
            Vector3 center=motor.bounds.center;
            float half=Mathf.Max(0,motor.height*.5f-motor.radius);
            foreach(var hit in Physics.CapsuleCastAll(center+Vector3.up*half,center-Vector3.up*half,
                motor.radius,displacement.normalized,displacement.magnitude,~0,QueryTriggerInteraction.Ignore)) {
                if(hit.collider.GetComponentInParent<ShibuyaFighter>()==this) continue;
                if(hit.distance<=motor.skinWidth && Vector3.Dot(hit.normal,displacement)>=0) continue;
                if(hit.distance<displacement.magnitude-motor.skinWidth) return false;
            }
            return true;
        }

        private Vector3 AdvanceAirChase(float delta)
        {
            chaseFrameRequest=Vector3.zero;
            if(!chasing) return Vector3.zero;
            chaseElapsed+=delta;
            if(!ValidAerialTarget() || chaseElapsed>aerialSettings.chaseMaxDuration
                || chaseTravel>=aerialSettings.chaseMaxTravel
                || Vector3.Distance(transform.position,aerialTarget.transform.position)>aerialSettings.chaseMaxRange) {
                EndChaseToFall("추격 시간/거리/대상 제한");return Vector3.zero;
            }
            Vector3 flat=aerialTarget.transform.position-transform.position;flat.y=0;
            if(Vector3.Angle(chaseAxis,flat)>aerialSettings.chaseMaxTurnAngle) {
                EndChaseToFall("대상 방향 이탈");return Vector3.zero;
            }
            if(flat.sqrMagnitude>.000001f) {
                float turn=Mathf.Min(Mathf.Max(0,aerialSettings.chaseMaxTurnAngle-chaseTurn),aerialSettings.chaseTurnSpeed*delta);
                var old=transform.rotation;transform.rotation=Quaternion.RotateTowards(old,Quaternion.LookRotation(flat),turn);
                chaseTurn+=Quaternion.Angle(old,transform.rotation);
            }
            Vector3 remaining=ChaseDestination()-transform.position;
            float speed=Mathf.Min(aerialSettings.chaseSpeed,Mathf.Sqrt(2*aerialSettings.chaseDeceleration*remaining.magnitude));
            float amount=Mathf.Min(remaining.magnitude,Mathf.Min(speed*delta,aerialSettings.chaseMaxTravel-chaseTravel));
            chaseFrameRequest=remaining.normalized*amount;chaseTravel+=amount;
            return chaseFrameRequest;
        }

        private void AfterAerialMotor(Vector3 before,CollisionFlags flags,float delta)
        {
            if(!airSession) return;
            if(!motor.isGrounded) leftGround=true;
            if(chasing && ValidAerialTarget()) {
                Vector3 difference=aerialTarget.transform.position-transform.position;
                float vertical=Mathf.Abs(difference.y);difference.y=0;
                float clearance=motor.radius+aerialTarget.motor.radius+motor.skinWidth;
                float arrival=Mathf.Max(aerialSettings.chaseArrivalDistance,clearance);
                if(leftGround && aerialTarget.transform.position.y-launchRootY>=aerialSettings.chaseMinimumRise
                    && difference.magnitude<=arrival+aerialSettings.chaseArrivalTolerance && difference.magnitude>=clearance-.02f
                    && vertical<=aerialSettings.chaseVerticalTolerance
                    && !IsInCinematic && !aerialTarget.IsInCinematic
                    && !WorldBlocks(transform.position+Vector3.up,aerialTarget.transform.position+Vector3.up)) {
                    chasing=false;arrivedAt=combatTime;
                    aerialTarget.ApplyChaseContact(this,aerialSettings.chaseContactHoldTime);
                    verticalSpeed=0; AirChaseContactCount++;
                    AirChaseReason="도착: 무피해 접촉 후 LMB 공중 체술 가능";
                } else {
                    float requested=chaseFrameRequest.magnitude;
                    float actual=Vector3.Distance(before,transform.position);
                    chaseStalled=requested>.001f && actual<requested*.4f?chaseStalled+delta:0;
                    if((flags&CollisionFlags.Above)!=0 || chaseStalled>=aerialSettings.chaseStallTimeout)
                        EndChaseToFall("충돌/정체로 추격 종료");
                }
            }
            if(!chasing && motor.isGrounded && verticalSpeed<=0 && TryLandingSurface(out _)) {
                if(executingAirAttack) CancelCombo();
                ClearAerialState();AirChaseReason="지면 착지: 지상 조작 복귀";
            }
        }

        private void EndChaseToFall(string reason)
        {
            if(executingAirAttack) CancelCombo();
            CancelAerialHover();
            chasing=chaseQueued=false;aerialTarget=null;
            if(aerialSettings!=null) verticalSpeed=Mathf.Clamp(verticalSpeed,-aerialSettings.failureFallSpeedLimit,0);
            AirChaseReason=reason;
        }

        private bool TryAirAttack()
        {
            if(executingAirAttack) return BufferCommonAirStep();
            if(!CanAirAttack) return false;
            if(!airSession) {
                var settings=AerialDefinition;
                ClearAerialState(); aerialSettings=settings;
                airSession=leftGround=true; arrivedAt=combatTime;
            }
            if(!aerialSettings.IsValid) return false;
            BeginCommonAirStep(false); return true;
        }

        private void AimAirAttack(float delta)
        {
            if(!executingAirAttack) return;
            for(int i=0;i<activeStep.hits.Length;i++) {
                var evt=activeStep.hits[i];float elapsed=AttackElapsed-evt.startTime;
                if(elapsed<0) continue;
                if(!aimStarted[i]) SnapshotEventAim(i);
                if(aimIsExplicit[i]) { TurnLockedAttack(i,evt.strike,elapsed,delta); continue; }
                if(elapsed>evt.strike.Startup) continue;
                var selected=evt.requiredMovement>=0?correctionTarget:eventAim[i];
                if(!IsLiveTarget(selected) || selected.Health.IsInvulnerable) continue;
                var settings=Targeting(evt.strike);
                Vector3 flat=selected.transform.position-transform.position;flat.y=0;
                if(flat.sqrMagnitude<.000001f || Vector3.Angle(initialAttackForward,flat)>settings.retentionAngle) continue;
                float dt=Mathf.Min(delta,Mathf.Max(0,elapsed));
                float turn=Mathf.Min(Mathf.Max(0,settings.maxTurnAngle-aimTurns[i]),settings.turnSpeed*dt);
                var old=transform.rotation;transform.rotation=Quaternion.RotateTowards(old,Quaternion.LookRotation(flat),turn);
                aimTurns[i]+=Quaternion.Angle(old,transform.rotation);
            }
        }

        private void FinishAirAttack()
        {
            if(HasBufferedAirAttack && LegalAerialContact(airContactTarget)) {
                BeginCommonAirStep(true); return;
            }
            bool grace=hitThisAttack && WithinAirHoverBudget && aerialSettings.aerialLinkInputWindow>0
                && LegalAerialContact(airContactTarget);
            CancelCombo(grace);
            if(grace) {airGrace=true;airGraceUntil=combatTime+aerialSettings.aerialLinkInputWindow;verticalSpeed=0;}
            AirChaseReason=grace?"공중 체술 종료: 짧은 후속 클릭 대기":"공중 체술 종료: 자연 낙하";
        }

        private void InterruptAerial()
        {
            if(airSession) EndChaseToFall("공격자 피격/중단: 자연 낙하");
            else ClearAerialState();
        }

        private void ClearAerialState()
        {
            CancelAerialHover();
            airHoverStarted=false;airHoverDeadline=0;airContactTarget=null;
            aerialTarget=null;aerialSettings=null;chaseQueued=chaseConsumed=airSession=chasing=airAttackUsed=executingAirAttack=leftGround=false;
            opportunityUntil=chaseBufferUntil=0;offeredAttack=-1;chaseFrameRequest=Vector3.zero;
        }
    }
}
