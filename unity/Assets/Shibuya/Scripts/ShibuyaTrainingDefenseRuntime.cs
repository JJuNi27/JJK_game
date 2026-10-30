using System;
using JJKGame.Core;
using UnityEngine;
namespace JJKGame.Shibuya
{
    public sealed partial class ShibuyaTrainingAI
    {
        public ShibuyaTrainingMode Mode {get;private set;}=ShibuyaTrainingMode.Offensive;
        public ShibuyaTrainingDefenseProfile DefenseProfile=>profile!=null?profile.defense:null;
        public ShibuyaTrainingDefenseAction LastDefenseChoice {get;private set;}
        public string LastDefenseResult {get;private set;}="방어 판단 없음";
        public int ThreatObservationCount {get;private set;}
        public int GuardAttemptCount {get;private set;}
        public int DefenseGuardSuccessCount {get;private set;}
        public int ParryRequestCount {get;private set;}
        public int DefenseParryWindowCount {get;private set;}
        public int DefenseParrySuccessCount {get;private set;}
        public int DodgeAttemptCount {get;private set;}
        public int DodgeStartedCount {get;private set;}
        public int DodgeDamageBlockedCount {get;private set;}
        public int NoResponseCount {get;private set;}
        public int ReactionFailureCount {get;private set;}
        public int DefenseRejectedCount {get;private set;}
        public double ThreatObservedAt {get;private set;}
        public double DefenseReactionDueAt {get;private set;}
        public double LastDefenseAttemptAt {get;private set;}
        private System.Random defenseRandom;
        private bool pendingDefense,ownsGuard,ownsDodge;
        private int lastObservedAction=-1;
        private double observeAt,defenseReadyAt,guardReleaseAt;

        public void SetMode(ShibuyaTrainingMode value)
        {
            if(!Enum.IsDefined(typeof(ShibuyaTrainingMode),value)||Mode==value)return;
            ResetDefense();WithdrawOwnQueue();groupActive=false;reactionScheduled=false;
            if(running)ClearIntent();Mode=value;LastDefenseResult="모드 변경 — 진행 중 타격/대시는 기존 Fighter가 마무리";
        }
        private void InitializeDefense()
        {
            ResetDefense();lastObservedAction=-1;observeAt=defenseReadyAt=0;
            defenseRandom=new System.Random(profile.useFixedSeed?profile.randomSeed:unchecked(Environment.TickCount^owner.GetInstanceID()));
        }
        private void ResetDefense()
        {
            if(ownsGuard&&owner!=null)owner.SetGuardHeld(false);
            pendingDefense=ownsGuard=ownsDodge=false;
        }
        private void BindDefenseEvents()
        {
            owner.GuardBlocked+=OnDefenseGuard;owner.ParrySucceeded+=OnDefenseParry;
            owner.Health.DamageResolved+=OnDefenseDamage;
        }
        private void UnbindDefenseEvents()
        {
            if(owner==null)return;owner.GuardBlocked-=OnDefenseGuard;owner.ParrySucceeded-=OnDefenseParry;
            if(owner.Health!=null)owner.Health.DamageResolved-=OnDefenseDamage;
        }
        private void OnDefenseGuard(ShibuyaFighter fighter)
        {
            if(!running||!ownsGuard)return;DefenseGuardSuccessCount++;LastDefenseResult="실제 가드 성공 (HP 방어·가드 체력 소비)";
        }
        private void OnDefenseParry(ShibuyaFighter fighter)
        {
            if(!running||!ownsGuard)return;DefenseParrySuccessCount++;LastDefenseResult="실제 패링 성공 — 자동 반격 없음";
        }
        private void OnDefenseDamage(Health health,DamageContext hit,DamageResolution result)
        {
            if(!running)return;
            if(result==DamageResolution.Guarded&&ownsDodge&&owner.IsGroundDashEvading&&hit.DeliveryType==DamageDeliveryType.PhysicalStrike){
                DodgeDamageBlockedCount++;LastDefenseResult="회피 무적 구간에서 실제 Guarded 피해 판정 확인";
            }
            if(result==DamageResolution.Applied&&(pendingDefense||ownsGuard||ownsDodge)){
                if(pendingDefense){pendingDefense=false;ReactionFailureCount++;defenseReadyAt=Time.timeAsDouble+Positive(DefenseProfile!=null?DefenseProfile.minimumDecisionInterval:0);}
                LastDefenseResult="실제 피격 — 늦은 반응/방향/무적 밖 공격";
            }
        }
        private static float Positive(float value)=>float.IsFinite(value)?Mathf.Max(0,value):0;
        private float Between(float low,float high)
        {
            low=Positive(low);high=Mathf.Max(low,Positive(high));return Mathf.Lerp(low,high,(float)defenseRandom.NextDouble());
        }
        private bool AttackIsObservable(ShibuyaTrainingDefenseProfile p)
        {
            // CurrentStrike describes the currently presented attack only. Never inspect buffered inputs.
            if(target.ComboStep==0||target.CurrentStrike==null||TargetDistance>Mathf.Min(profile.detectionRange,Mathf.Max(.1f,p.threatRange)))return false;
            Vector3 toward=owner.transform.position-target.transform.position;toward.y=0;
            if(toward.sqrMagnitude>.000001f&&Vector3.Angle(target.transform.forward,toward)>p.attackerFacingHalfAngle)return false;
            Vector3 from=target.GetComponent<CharacterController>().bounds.center,to=owner.GetComponent<CharacterController>().bounds.center;
            foreach(var h in Physics.RaycastAll(from,(to-from).normalized,Vector3.Distance(from,to),~0,QueryTriggerInteraction.Ignore))
                if(h.collider.GetComponentInParent<ShibuyaFighter>()!=owner&&h.collider.GetComponentInParent<ShibuyaFighter>()!=target)return false;
            return true;
        }
        private ShibuyaTrainingDefenseAction ChooseDefense(ShibuyaTrainingDefenseProfile p)
        {
            bool guard=owner.GuardSettings!=null&&owner.GuardSettings.guardEnabled&&owner.CurrentGuard>0;
            bool dodge=owner.Profile.groundDash!=null&&owner.Profile.groundDash.dashEnabled&&owner.GroundDashCooldownRemaining<=0;
            bool parry=guard&&owner.ParrySettings!=null&&owner.ParrySettings.parryEnabled&&owner.ParryRetryRemaining<=0;
            if(Mode==ShibuyaTrainingMode.GuardPractice)return guard?ShibuyaTrainingDefenseAction.Guard:ShibuyaTrainingDefenseAction.None;
            if(Mode==ShibuyaTrainingMode.DodgePractice)return dodge?ShibuyaTrainingDefenseAction.Dodge:ShibuyaTrainingDefenseAction.None;
            if(Mode==ShibuyaTrainingMode.ParryPractice)return parry?ShibuyaTrainingDefenseAction.Parry:guard?ShibuyaTrainingDefenseAction.Guard:ShibuyaTrainingDefenseAction.None;
            float g=guard?Positive(p.guardWeight):0,d=dodge?Positive(p.dodgeWeight):0,pa=parry?Positive(p.parryWeight):0,n=Positive(p.noResponseWeight);
            double total=(double)g+d+pa+n;if(total<=0||!double.IsFinite(total))return ShibuyaTrainingDefenseAction.None;
            double roll=defenseRandom.NextDouble()*total;
            if((roll-=g)<0)return ShibuyaTrainingDefenseAction.Guard;
            if((roll-=d)<0)return ShibuyaTrainingDefenseAction.Dodge;
            if((roll-=pa)<0)return ShibuyaTrainingDefenseAction.Parry;
            return ShibuyaTrainingDefenseAction.None;
        }
        private bool DodgePathClear(Vector3 direction,float distance)
        {
            var cc=owner.GetComponent<CharacterController>();Vector3 center=cc.bounds.center;
            float radius=cc.radius*Mathf.Max(Mathf.Abs(owner.transform.lossyScale.x),Mathf.Abs(owner.transform.lossyScale.z))*.9f;
            float half=Mathf.Max(0,cc.height*Mathf.Abs(owner.transform.lossyScale.y)*.5f-radius);
            foreach(var h in Physics.CapsuleCastAll(center+Vector3.up*half,center-Vector3.up*half,radius,direction,distance,~0,QueryTriggerInteraction.Ignore))
                if(h.collider.GetComponentInParent<ShibuyaFighter>()!=owner)return false;
            return true;
        }
        private bool TryDefenseDodge(ShibuyaTrainingDefenseProfile p)
        {
            Vector3 forward=p.dodgeBasis==ShibuyaTrainingDodgeBasis.Body?owner.transform.forward:target.transform.position-owner.transform.position;
            forward.y=0;if(forward.sqrMagnitude<.000001f)forward=owner.transform.forward;forward.Normalize();Vector3 right=Vector3.Cross(Vector3.up,forward);
            var dash=owner.Profile.groundDash;float left=Positive(p.leftWeight),r=Positive(p.rightWeight),back=Positive(p.backWeight);
            if(!DodgePathClear(-right,dash.sideDistance))left=0;if(!DodgePathClear(right,dash.sideDistance))r=0;if(!DodgePathClear(-forward,dash.backwardDistance))back=0;
            double total=(double)left+r+back;if(total<=0||!double.IsFinite(total))return false;
            double roll=defenseRandom.NextDouble()*total;Vector2 input=(roll-=left)<0?Vector2.left:(roll-=r)<0?Vector2.right:Vector2.down;
            return owner.TryGroundDash(input,forward,right);
        }
        private void ExecuteDefense(ShibuyaTrainingDefenseProfile p,double now)
        {
            pendingDefense=false;defenseReadyAt=now+Positive(p.minimumDecisionInterval);LastDefenseChoice=ChooseDefense(p);
            LastDefenseAttemptAt=now;ClearIntent();reactionScheduled=false;
            if(LastDefenseChoice==ShibuyaTrainingDefenseAction.None){NoResponseCount++;LastDefenseResult="이번 위협에 반응하지 않음/가능한 방어 없음";return;}
            if(LastDefenseChoice==ShibuyaTrainingDefenseAction.Dodge){
                DodgeAttemptCount++;ownsDodge=TryDefenseDodge(p);
                if(ownsDodge){DodgeStartedCount++;LastDefenseResult="실제 대시 시작 — 위치 이동만으로 성공 집계하지 않음";}
                else{DefenseRejectedCount++;LastDefenseResult="회피 거절/이동 경로 차단 — 반복 요청 없음";}return;
            }
            if(LastDefenseChoice==ShibuyaTrainingDefenseAction.Parry)ParryRequestCount++;else GuardAttemptCount++;
            int before=owner.ParryAttemptCount;owner.SetGuardHeld(false);owner.SetGuardHeld(true);ownsGuard=owner.IsGuarding;
            if(owner.ParryAttemptCount>before)DefenseParryWindowCount++;
            if(!ownsGuard){owner.SetGuardHeld(false);DefenseRejectedCount++;LastDefenseResult="기존 Fighter가 가드 진입 거절";return;}
            guardReleaseAt=now+Between(p.minimumGuardHold,p.maximumGuardHold);
            LastDefenseResult=LastDefenseChoice==ShibuyaTrainingDefenseAction.Parry?"실제 새 가드/패링 시도 — 성공은 판정 대기":"가드 시작 — 새 가드의 기존 패링 창도 그대로 적용";
        }
        private bool UpdateDefense(double now)
        {
            if(Mode==ShibuyaTrainingMode.Offensive)return false;
            var p=DefenseProfile;
            if(p==null||!p.defenseEnabled){ResetDefense();return Mode!=ShibuyaTrainingMode.Mixed;}
            // An ongoing legitimate combo always wins. Do not cancel its next input for a threat.
            if(owner.IsGroundAttackInProgress||groupActive){if(pendingDefense){pendingDefense=false;ReactionFailureCount++;}return false;}
            if(ownsGuard){
                ClearIntent();
                if(!owner.IsGuarding||now>=guardReleaseAt){owner.SetGuardHeld(false);ownsGuard=false;}
                else{State=ShibuyaTrainingAIState.GuardDefense;LastReason="기존 가드 유지 — 시간 만료 시 해제";return true;}
            }
            if(ownsDodge){
                ClearIntent();if(!owner.IsGroundDashing)ownsDodge=false;
                else{State=ShibuyaTrainingAIState.EvadeDefense;LastReason="기존 Fighter 대시 완료 대기";return true;}
            }
            if(!owner.CanAcceptGroundCombatIntent){
                if(pendingDefense){pendingDefense=false;ReactionFailureCount++;LastDefenseResult="반응 전 피격/행동 제한 — 소급 방어 없음";defenseReadyAt=now+Positive(p.minimumDecisionInterval);}
                return false;
            }
            if(pendingDefense){
                ClearIntent();State=ShibuyaTrainingAIState.DefenseReaction;LastReason="관측 후 반응 지연 중";
                if(now>=DefenseReactionDueAt){
                    if(target.AcceptedAttackCount!=lastObservedAction||!AttackIsObservable(p)){pendingDefense=false;ReactionFailureCount++;defenseReadyAt=now+Positive(p.minimumDecisionInterval);LastDefenseResult="관측한 공격이 종료/취소/이탈 — 뒤늦은 예약 없음";}
                    else ExecuteDefense(p,now);
                }return true;
            }
            if(now<defenseReadyAt){ClearIntent();State=ShibuyaTrainingAIState.DefenseWait;LastReason="다음 판단까지 유한 대기 (자동 반격 없음)";return true;}
            if(now>=observeAt){
                observeAt=now+Mathf.Max(.01f,Positive(p.observationInterval));
                if(target.AcceptedAttackCount!=lastObservedAction&&AttackIsObservable(p)){
                    lastObservedAction=target.AcceptedAttackCount;ThreatObservationCount++;ThreatObservedAt=now;
                    DefenseReactionDueAt=now+Between(p.minimumReaction,p.maximumReaction);pendingDefense=true;
                    ClearIntent();reactionScheduled=false;State=ShibuyaTrainingAIState.DefenseReaction;LastReason="현재 실제 공격을 관측 — 미래 입력은 읽지 않음";return true;
                }
            }
            if(Mode==ShibuyaTrainingMode.Mixed&&p.mixedAllowsOffense)return false;
            owner.SetIntent(Vector3.zero,false,false);owner.SetFacingIntent(target.transform.position-owner.transform.position);
            State=ShibuyaTrainingAIState.Idle;LastReason="방어 연습 — 새 공격 관측 대기";return true;
        }
    }
}
