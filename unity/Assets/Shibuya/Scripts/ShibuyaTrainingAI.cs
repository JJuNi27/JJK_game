using System;
using UnityEngine;
namespace JJKGame.Shibuya
{
    public enum ShibuyaTrainingAIState { Disabled, Idle, Approach, FaceTarget, Attack, Recovery, UnableToAct, Blocked, DefenseReaction, GuardDefense, EvadeDefense, DefenseWait }
    [DisallowMultipleComponent,RequireComponent(typeof(ShibuyaFighter)),DefaultExecutionOrder(-40)]
    public sealed partial class ShibuyaTrainingAI : MonoBehaviour
    {
        [SerializeField] private ShibuyaTrainingAIProfile profile;
        [SerializeField] private ShibuyaFighter target;
        private ShibuyaFighter owner;
        private ShibuyaTrainingAIProfile activeProfile;
        private System.Random random;
        private bool running,groupActive,approaching,reactionScheduled,pathClear,targetDetected;
        private int groupSteps,submittedAction=-1,queuedFromAction=-1;
        private double nextDecisionAt,nextAttackAt,restUntil;
        private Vector3 targetDelta;
        private float decisionPlanarDistance;
        public ShibuyaTrainingAIProfile Profile=>profile;
        public ShibuyaFighter Target=>target;
        public ShibuyaFighter Owner=>owner;
        public bool Running=>running;
        public ShibuyaTrainingAIState State {get;private set;}=ShibuyaTrainingAIState.Disabled;
        public string LastReason {get;private set;}="기본 OFF";
        public float TargetDistance {get;private set;}=float.PositiveInfinity;
        public float NextAttackRemaining=>Mathf.Max(0,(float)(nextAttackAt-Time.timeAsDouble));
        public int AttackRequestCount {get;private set;}
        public int StartedAttackCount {get;private set;}
        public int GroupCount {get;private set;}
        public int CompletedGroupCount {get;private set;}
        public int InterruptedGroupCount {get;private set;}
        public int RejectedRequestCount {get;private set;}
        public int CurrentGroupLength {get;private set;}
        public int CurrentGroupStartedSteps=>groupSteps;
        private void Awake(){owner=GetComponent<ShibuyaFighter>();}
        private void OnEnable(){if(owner==null)owner=GetComponent<ShibuyaFighter>();owner.AttackStarted+=OnAttackStarted;BindDefenseEvents();}
        private void OnDisable(){SetTrainingEnabled(false);UnbindDefenseEvents();if(owner!=null)owner.AttackStarted-=OnAttackStarted;}
        public void Configure(ShibuyaTrainingAIProfile settings,ShibuyaFighter opponent)
        {
            SetTrainingEnabled(false);profile=settings;target=opponent;Mode=settings!=null?settings.initialMode:ShibuyaTrainingMode.Offensive;
        }
        public bool SetTrainingEnabled(bool value)
        {
            if(!value){
                ResetDefense();
                if(running){WithdrawOwnQueue();ClearIntent();}
                running=groupActive=approaching=reactionScheduled=false;State=ShibuyaTrainingAIState.Disabled;LastReason="AI OFF";return true;
            }
            if(owner==null||!isActiveAndEnabled||profile==null||!profile.trainingAllowed||!LivePair()
                ||owner.GetComponent<ShibuyaPlayerInput>()!=null){LastReason="설정·대상·소유자 조건 불충족";return false;}
            if(running)return true;
            activeProfile=profile;random=new System.Random(profile.useFixedSeed?profile.randomSeed:unchecked(Environment.TickCount^owner.GetInstanceID()));
            running=true;groupActive=approaching=reactionScheduled=false;nextDecisionAt=nextAttackAt=restUntil=0;submittedAction=queuedFromAction=-1;
            InitializeDefense();
            State=ShibuyaTrainingAIState.Idle;LastReason="AI ON";return true;
        }
        private bool LivePair()=>owner!=null&&target!=null&&target!=owner&&owner.isActiveAndEnabled&&target.isActiveAndEnabled
            &&!owner.Health.IsDead&&!target.Health.IsDead&&owner.State!=ShibuyaFighterState.Finished&&target.State!=ShibuyaFighterState.Finished
            &&owner.gameObject.scene==target.gameObject.scene&&target.gameObject.scene.isLoaded;
        private void ClearIntent(){if(owner!=null){owner.SetIntent(Vector3.zero,false,false);owner.ClearFacingIntent();}}
        private void WithdrawOwnQueue(){if(owner!=null&&queuedFromAction>=0)owner.WithdrawBufferedGroundAttack(queuedFromAction);queuedFromAction=-1;}
        private void OnAttackStarted(int step)
        {
            if(!running||!groupActive)return;StartedAttackCount++;groupSteps++;queuedFromAction=-1;
        }
        private int ChooseGroupLength()
        {
            float[] weights={Mathf.Max(0,profile.oneHitWeight),Mathf.Max(0,profile.twoHitWeight),Mathf.Max(0,profile.threeHitWeight),Mathf.Max(0,profile.fourHitWeight)};
            float total=0;foreach(float w in weights)total+=w;
            if(!float.IsFinite(total)||total<=0)return 1;
            double choice=random.NextDouble()*total;
            for(int i=0;i<4;i++){choice-=weights[i];if(choice<0)return Mathf.Min(i+1,owner.GroundAttackStepCount);}
            return Mathf.Min(4,owner.GroundAttackStepCount);
        }
        private void Rest(string reason,bool complete)
        {
            WithdrawOwnQueue();groupActive=false;reactionScheduled=false;ClearIntent();
            float min=Mathf.Max(0,profile.minimumRest),max=Mathf.Max(min,profile.maximumRest);
            nextAttackAt=Time.timeAsDouble+Mathf.Lerp(min,max,(float)random.NextDouble());
            restUntil=nextAttackAt;
            if(complete)CompletedGroupCount++;else InterruptedGroupCount++;
            State=ShibuyaTrainingAIState.Recovery;LastReason=reason;
        }
        private bool HasClearPath()
        {
            var cc=owner.GetComponent<CharacterController>();var other=target.GetComponent<CharacterController>();
            Vector3 a=cc.bounds.center,b=other.bounds.center,d=b-a;
            foreach(var hit in Physics.RaycastAll(a,d.normalized,d.magnitude,~0,QueryTriggerInteraction.Ignore))
                if(hit.collider.GetComponentInParent<ShibuyaFighter>()!=owner&&hit.collider.GetComponentInParent<ShibuyaFighter>()!=target)return false;
            Vector3 flat=Vector3.ProjectOnPlane(d,Vector3.up);float distance=Mathf.Max(0,flat.magnitude-profile.approachStopDistance);
            if(distance<=.0001f)return true;
            float radius=cc.radius*Mathf.Max(Mathf.Abs(owner.transform.lossyScale.x),Mathf.Abs(owner.transform.lossyScale.z))*.9f;
            float half=Mathf.Max(0,cc.height*Mathf.Abs(owner.transform.lossyScale.y)*.5f-radius);
            foreach(var hit in Physics.CapsuleCastAll(a+Vector3.up*half,a-Vector3.up*half,radius,flat.normalized,distance,~0,QueryTriggerInteraction.Ignore))
                if(hit.collider.GetComponentInParent<ShibuyaFighter>()!=owner&&hit.collider.GetComponentInParent<ShibuyaFighter>()!=target)return false;
            return true;
        }
        private void Update()
        {
            if(!running)return;
            if(profile!=activeProfile||profile==null||!profile.trainingAllowed||!LivePair()){SetTrainingEnabled(false);LastReason="대상/전투 종료 또는 설정 변경";return;}
            double now=Time.timeAsDouble;
            targetDelta=target.transform.position-owner.transform.position;TargetDistance=targetDelta.magnitude;
            if(UpdateDefense(now))return;
            if(owner.IsInHitStop){ClearIntent();State=ShibuyaTrainingAIState.UnableToAct;LastReason="국소 히트스톱 대기 (예약 보존)";return;}
            if(!owner.CanAcceptGroundCombatIntent){
                if(groupActive)Rest("피격·다운·행동 소유권으로 묶음 중단",false);
                ClearIntent();reactionScheduled=false;State=ShibuyaTrainingAIState.UnableToAct;LastReason="기존 Fighter 상태 우선";return;
            }
            if(now>=nextDecisionAt){
                nextDecisionAt=now+Mathf.Max(.01f,profile.decisionInterval);pathClear=HasClearPath();
                targetDetected=TargetDistance<=Mathf.Max(.1f,profile.detectionRange);
                decisionPlanarDistance=Vector3.ProjectOnPlane(targetDelta,Vector3.up).magnitude;
            }
            if(!targetDetected||!pathClear){
                if(groupActive)Rest("대상 이탈/경로 차단으로 후속 입력 중단",false);
                ClearIntent();reactionScheduled=false;State=pathClear?ShibuyaTrainingAIState.Idle:ShibuyaTrainingAIState.Blocked;
                LastReason=pathClear?"탐색 거리 밖":"직선 경로 차단 — 경로 탐색 없음";return;
            }
            if(groupActive){
                ClearIntent();State=ShibuyaTrainingAIState.Attack;
                if(!owner.IsGroundAttackInProgress){Rest("공격 묶음 종료 후 휴식",true);return;}
                // Stay inside the existing reservation window, with margin on both sides.
                // Its first instant can expire on the frame that resolves attack completion.
                float reserveAt=(owner.GroundAttackReservationOpensAt+owner.AttackDuration)*.5f;
                if(groupSteps<CurrentGroupLength&&owner.AcceptedAttackCount!=submittedAction&&owner.CanReserveGroundAttack&&owner.AttackElapsed>=reserveAt){
                    int token=owner.AcceptedAttackCount;submittedAction=token;AttackRequestCount++;
                    if(owner.TryAttack(ShibuyaFinisher.Default)){queuedFromAction=token;LastReason="유효 예약 구간에 후속 입력1회";}
                    else{RejectedRequestCount++;LastReason="예약 거절 — 같은 단계에서 재시도하지 않음";}
                }
                return;
            }
            // OFF or another development tool may leave one valid, already-started attack.
            if(owner.IsGroundAttackInProgress){ClearIntent();State=ShibuyaTrainingAIState.UnableToAct;LastReason="기존 진행 공격 완료 대기";return;}
            if(now<restUntil){ClearIntent();State=ShibuyaTrainingAIState.Recovery;return;}
            Vector3 flatDelta=Vector3.ProjectOnPlane(targetDelta,Vector3.up);float distance=flatDelta.magnitude;
            float stop=Mathf.Max(.1f,profile.approachStopDistance),start=Mathf.Max(stop,profile.approachStartDistance);
            if(decisionPlanarDistance>=start)approaching=true;
            if(approaching&&distance>stop+.0001f){
                reactionScheduled=false;State=ShibuyaTrainingAIState.Approach;LastReason="접근 중";owner.ClearFacingIntent();
                // Analog intent caps the final approach step. Fighter still owns speed, collision and Move.
                float speed=profile.useRunning?owner.Profile.runSpeed:owner.Profile.walkSpeed;
                float magnitude=Mathf.Clamp01((distance-stop)/Mathf.Max(.0001f,speed*Time.deltaTime));
                owner.SetIntent(flatDelta.normalized*magnitude,profile.useRunning,false);return;
            }
            approaching=false;owner.SetIntent(Vector3.zero,false,false);owner.SetFacingIntent(flatDelta);
            State=ShibuyaTrainingAIState.FaceTarget;
            if(Mathf.Abs(targetDelta.y)>Mathf.Max(0,profile.maximumAttackHeightDifference)||Vector3.Angle(owner.transform.forward,flatDelta)>profile.facingTolerance){reactionScheduled=false;LastReason="높이/몸 방향 정렬 대기";return;}
            if(!reactionScheduled){reactionScheduled=true;nextAttackAt=now+Mathf.Max(0,profile.reactionDelay);LastReason="공격 반응 지연";}
            if(now<nextAttackAt)return;
            groupActive=true;groupSteps=0;CurrentGroupLength=ChooseGroupLength();submittedAction=queuedFromAction=-1;AttackRequestCount++;
            if(owner.TryStartGroundAttackSequence()){GroupCount++;State=ShibuyaTrainingAIState.Attack;LastReason="새 지상 공격 묶음 시작";}
            else{groupActive=false;RejectedRequestCount++;reactionScheduled=false;nextAttackAt=now+Mathf.Max(.01f,profile.decisionInterval);LastReason="묶음 시작 거절 — 다음 판단까지 대기";}
        }
    }
}
