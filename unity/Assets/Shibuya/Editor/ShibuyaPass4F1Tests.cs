using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass4F1DataTests
    {
        [Test] public void DefenseDataKoreanInspectorUndoAndOffensiveReferenceValues()
        {
            var a=AssetDatabase.LoadAssetAtPath<ShibuyaTrainingAIProfile>(ShibuyaTrainingAITool.ProfilePath);
            Assert.That(a.initialMode,Is.EqualTo(ShibuyaTrainingMode.Offensive));Assert.That(a.defense,Is.Not.Null);
            Assert.That(a.detectionRange,Is.EqualTo(10));Assert.That(a.approachStopDistance,Is.EqualTo(1.55f));Assert.That(a.minimumRest,Is.EqualTo(.9f));
            var p=Object.Instantiate(a.defense);
            try{
                Assert.That(p.observationInterval,Is.EqualTo(.1f));Assert.That(p.minimumReaction,Is.EqualTo(.16f));Assert.That(p.maximumReaction,Is.EqualTo(.3f));
                Assert.That(p.minimumDecisionInterval,Is.EqualTo(.75f));Assert.That(p.minimumGuardHold,Is.EqualTo(.35f));Assert.That(p.maximumGuardHold,Is.EqualTo(.75f));
                Assert.That(new[]{p.guardWeight,p.dodgeWeight,p.parryWeight,p.noResponseWeight},Is.EqualTo(new[]{50f,25,10,15}));
                var so=new SerializedObject(p);var it=so.GetIterator();bool child=true;
                while(it.NextVisible(child)){child=false;if(it.name!="m_Script")Assert.That(ShibuyaTrainingDefenseInspector.Labels.ContainsKey(it.name),Is.True,it.name);}
                var ed=UnityEditor.Editor.CreateEditor(p);Assert.That(ed,Is.TypeOf<ShibuyaTrainingDefenseInspector>());Object.DestroyImmediate(ed);
                Undo.IncrementCurrentGroup();so.FindProperty("minimumReaction").floatValue=.25f;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();Undo.PerformUndo();Assert.That(p.minimumReaction,Is.EqualTo(.16f));
            }finally{Undo.ClearUndo(p);Object.DestroyImmediate(p);}
        }
    }
    public sealed class ShibuyaPass4F1Tests : ShibuyaPass4CFixture
    {
        private ShibuyaTrainingAI ai;
        private ShibuyaTrainingAIProfile training;
        private ShibuyaTrainingDefenseProfile defense;
        private const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        private bool bodyCompleted;
        private void CompleteBody()
        {
            bodyCompleted=true;Record("4F1-completed-"+TestContext.CurrentContext.Test.Name,new{mode=ai.Mode.ToString(),observations=ai.ThreatObservationCount,requests=DefenseRequests,guardSuccess=ai.DefenseGuardSuccessCount,parrySuccess=ai.DefenseParrySuccessCount,dodges=ai.DodgeStartedCount});
        }
        [UnityTearDown] public IEnumerator VerifyBodyCompleted(){Assert.That(bodyCompleted,Is.True,"Test body did not reach its completion sentinel; do not accept a partial coroutine as PASS");yield return null;}
        [UnitySetUp] public IEnumerator DefenseSetup()
        {
            bodyCompleted=false;
            training=Clone(AssetDatabase.LoadAssetAtPath<ShibuyaTrainingAIProfile>(ShibuyaTrainingAITool.ProfilePath));defense=Clone(training.defense);training.defense=defense;
            training.useFixedSeed=true;training.randomSeed=1729;dummy.Configure(combat,AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root+"/Data/Dummy Stats.asset"));
            ai=dummy.gameObject.AddComponent<ShibuyaTrainingAI>();ai.Configure(training,player);yield return Wait(.04f);
        }
        private IEnumerator Pair(ShibuyaTrainingMode mode,float delay=.2f)
        {
            ai.SetTrainingEnabled(false);frame=default;
            yield return Until(()=>dummy.CanAcceptGroundCombatIntent&&player.CanAcceptGroundCombatIntent&&dummy.ComboStep==0&&player.ComboStep==0&&dummy.GroundDashCooldownRemaining<=0&&dummy.ParryRetryRemaining<=0);
            player.SetMoveset(null);dummy.SetMoveset(null);Place(player,new Vector3(0,.03f,-8));Place(dummy,player.transform.position+Vector3.forward*1.4f);
            player.transform.rotation=Quaternion.identity;dummy.transform.rotation=Quaternion.Euler(0,180,0);player.Health.ResetHealth();dummy.Health.ResetHealth();
            defense.observationInterval=.01f;defense.minimumReaction=defense.maximumReaction=delay;defense.minimumGuardHold=defense.maximumGuardHold=.65f;
            ai.SetMode(mode);yield return Wait(.05f);
        }
        private ShibuyaMovesetDefinition Author(float startup=.5f,bool parryable=true,bool guardable=true,int hits=1,float guardDamage=16,float radius=-1,ShibuyaHitOutcome outcome=ShibuyaHitOutcome.Normal)
        {
            var m=ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();m.name="PASS4B3 QA defense actual visible attack";
            var s=JsonUtility.FromJson<ShibuyaStrikeDefinition>(JsonUtility.ToJson(combat.strikes[0]));
            s.damage=5;s.startup=startup;s.activeTime=.02f;s.recovery=.15f;s.advanceDistance=0;s.knockbackSpeed=0;s.hitStun=.12f;s.hitStopDuration=0;s.guardDamage=guardDamage;s.parryable=parryable;s.guardable=guardable;
            if(radius>0)s.attackRadius=radius;
            m.steps=new[]{new ShibuyaMovesetStep{hits=Enumerable.Range(0,hits).Select(i=>new ShibuyaHitEvent{strike=s,startTime=i*.04f,outcome=outcome}).ToArray()}};
            Assert.That(player.SetMoveset(m),Is.True);return m;
        }
        private void StartVisibleAttack(){Assert.That(ai.SetTrainingEnabled(true),Is.True);Assert.That(player.TryAttack(),Is.True);}
        private int DefenseRequests=>ai.GuardAttemptCount+ai.ParryRequestCount+ai.DodgeAttemptCount;
        [UnityTest] public IEnumerator ObserveDelayRealGuardConsumesGuardNotHpAndReleases()
        {
            yield return Pair(ShibuyaTrainingMode.GuardPractice);Author();double start=Time.timeAsDouble;StartVisibleAttack();
            yield return Until(()=>ai.ThreatObservationCount==1);Assert.That(ai.ThreatObservedAt,Is.GreaterThanOrEqualTo(start));Assert.That(DefenseRequests,Is.Zero);
            yield return Wait(.10f);Assert.That(DefenseRequests,Is.Zero);Assert.That(dummy.IsGuarding,Is.False);
            yield return Until(()=>dummy.IsGuarding);Assert.That(ai.LastDefenseAttemptAt-ai.ThreatObservedAt,Is.GreaterThanOrEqualTo(.199));
            float guard=dummy.CurrentGuard;yield return Until(()=>dummy.GuardBlockCount==1);
            Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(1000));Assert.That(dummy.CurrentGuard,Is.EqualTo(guard-16));Assert.That(ai.DefenseGuardSuccessCount,Is.EqualTo(1));Assert.That(dummy.ParrySuccessCount,Is.Zero);
            yield return Until(()=>!dummy.IsGuarding);Assert.That((bool)typeof(ShibuyaFighter).GetField("guardInputHeld",Private).GetValue(dummy),Is.False);
            Assert.That(dummy.AcceptedAttackCount+dummy.GroundDashCount,Is.Zero);Record("4F1-guard",new{delay=ai.LastDefenseAttemptAt-ai.ThreatObservedAt,guardCost=16,observations=ai.ThreatObservationCount});
            CompleteBody();
        }
        [UnityTest] public IEnumerator ShortPreparationHitsBeforeReactionWithoutRetroactiveGuard()
        {
            yield return Pair(ShibuyaTrainingMode.ParryPractice);Author(.05f);StartVisibleAttack();yield return Until(()=>dummy.Health.CurrentHealth<1000);
            Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(995));Assert.That(DefenseRequests,Is.Zero);Assert.That(ai.ReactionFailureCount,Is.EqualTo(1));
            yield return Wait(.4f);Assert.That(dummy.ParrySuccessCount+dummy.GuardBlockCount,Is.Zero);Assert.That(ai.ThreatObservationCount,Is.EqualTo(1));
            CompleteBody();
        }
        [UnityTest] public IEnumerator ActualParryWindowCancelsAttackerAndNeverAutoCounters()
        {
            yield return Pair(ShibuyaTrainingMode.ParryPractice);Author(.30f);Vector3 start=player.transform.position;StartVisibleAttack();
            yield return Until(()=>dummy.ParrySuccessCount==1);Assert.That(ai.ParryRequestCount,Is.EqualTo(1));Assert.That(ai.DefenseParryWindowCount,Is.EqualTo(1));Assert.That(ai.DefenseParrySuccessCount,Is.EqualTo(1));
            Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(1000));Assert.That(player.ConfirmedHitCount,Is.Zero);Assert.That(player.ComboStep,Is.Zero);Assert.That(player.HasBufferedAttack,Is.False);
            yield return Wait(.5f);Assert.That(Vector3.Distance(start,player.transform.position),Is.GreaterThan(.8f));Assert.That(dummy.AcceptedAttackCount,Is.Zero);Assert.That(Time.timeScale,Is.EqualTo(1));
            Record("4F1-parry",new{delay=ai.LastDefenseAttemptAt-ai.ThreatObservedAt,windows=ai.DefenseParryWindowCount,successes=ai.DefenseParrySuccessCount,recoil=Vector3.Distance(start,player.transform.position)});
            CompleteBody();
        }
        [UnityTest] public IEnumerator ParryWindowExpiredAndUnparryableAttacksUseOrdinaryGuard()
        {
            foreach(bool canParry in new[]{true,false}){
                yield return Pair(ShibuyaTrainingMode.ParryPractice);int before=dummy.GuardBlockCount,parries=dummy.ParrySuccessCount;Author(canParry?.55f:.3f,canParry);StartVisibleAttack();
                yield return Until(()=>dummy.GuardBlockCount>before);Assert.That(dummy.ParrySuccessCount,Is.EqualTo(parries));Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(1000));
                Assert.That(dummy.ParrySettings.windowDuration,Is.EqualTo(.12f));Assert.That(dummy.ParrySettings.retryInterval,Is.EqualTo(.5f));
            }
            CompleteBody();
        }
        [UnityTest] public IEnumerator BehindAttackerIsNeverTreatedAsFrontalGuard()
        {
            yield return Pair(ShibuyaTrainingMode.GuardPractice,0);Place(player,dummy.transform.position+Vector3.forward*1.4f);player.transform.rotation=Quaternion.Euler(0,180,0);
            Author(.2f);Quaternion before=dummy.transform.rotation;StartVisibleAttack();yield return Until(()=>dummy.IsGuarding);Assert.That(Quaternion.Angle(before,dummy.transform.rotation),Is.LessThan(1));
            yield return Until(()=>player.ComboStep==0);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(995));Assert.That(dummy.GuardBlockCount+dummy.ParrySuccessCount,Is.Zero);
            CompleteBody();
        }
        [UnityTest] public IEnumerator GuardBreakUsesOriginalDamageOnceAndLocksReguard()
        {
            yield return Pair(ShibuyaTrainingMode.GuardPractice);Author(.5f,hits:3,guardDamage:40);StartVisibleAttack();yield return Until(()=>dummy.IsGuardBroken);
            Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(995));Assert.That(dummy.GuardBreakCount,Is.EqualTo(1));Assert.That(player.ConfirmedHitCount,Is.EqualTo(1));Assert.That(dummy.IsGuarding,Is.False);
            int requests=DefenseRequests;yield return Wait(.25f);Assert.That(DefenseRequests,Is.EqualTo(requests));Assert.That(dummy.IsGuardBroken,Is.True);
            yield return Until(()=>dummy.CanAcceptGroundCombatIntent);Assert.That(dummy.GuardSettings.breakLockDuration,Is.EqualTo(.8f));
            CompleteBody();
        }
        [UnityTest] public IEnumerator UnblockableAndEnvironmentAreNotReportedAsAiGuardSuccess()
        {
            yield return Pair(ShibuyaTrainingMode.GuardPractice);Author(1);StartVisibleAttack();yield return Until(()=>dummy.IsGuarding);yield return Wait(.14f);
            foreach(var type in new[]{DamageDeliveryType.PhysicalStrike,DamageDeliveryType.Environmental}){
                var result=dummy.ReceiveStrike(new DamageContext(1,player.gameObject,type,DamageTraits.Unblockable),combat.strikes[0]);Assert.That(result,Is.EqualTo(DamageResolution.Applied));
            }
            Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(998));Assert.That(ai.DefenseGuardSuccessCount+ai.DefenseParrySuccessCount,Is.Zero);ai.SetTrainingEnabled(false);
            CompleteBody();
        }
        [UnityTest] public IEnumerator LeftRightBackDashesKeepDistancesAndIgnorePlayerCamera()
        {
            var rows=new List<object>();
            foreach(int direction in new[]{0,1,2}){
                yield return Pair(ShibuyaTrainingMode.DodgePractice,.02f);defense.leftWeight=direction==0?1:0;defense.rightWeight=direction==1?1:0;defense.backWeight=direction==2?1:0;
                Vector3 start=dummy.transform.position,forward=(player.transform.position-start).normalized,right=Vector3.Cross(Vector3.up,forward);Vector3 expected=direction==0?-right:direction==1?right:-forward;
                int count=ai.DodgeStartedCount;Author(1);StartVisibleAttack();yield return Until(()=>ai.DodgeStartedCount>count);
                Assert.That(Vector3.Dot(dummy.GroundDashDirection,expected),Is.GreaterThan(.999f));var actual=dummy.GroundDashDirection;camera.transform.rotation=Quaternion.Euler(70,127,0);
                yield return Until(()=>dummy.IsGroundDashEvading);Assert.That(dummy.GroundDashDirection,Is.EqualTo(actual));yield return Until(()=>!dummy.IsGroundDashing);
                float distance=Vector3.ProjectOnPlane(dummy.transform.position-start,Vector3.up).magnitude;Assert.That(distance,Is.EqualTo(direction==2?3.3f:3.6f).Within(.04f));
                Assert.That(dummy.GroundDashCooldownRemaining,Is.InRange(.40f,.45f));Assert.That(ai.DodgeDamageBlockedCount,Is.Zero);ai.SetTrainingEnabled(false);
                rows.Add(new{direction,distance,axis=new[]{actual.x,actual.y,actual.z}});
            }Record("4F1-dash-directions",rows);
            CompleteBody();
        }
        [UnityTest] public IEnumerator DodgeSuccessRequiresActualGuardedResolutionAndOutsideWindowTakesDamage()
        {
            foreach(bool inside in new[]{true,false}){
                yield return Pair(ShibuyaTrainingMode.DodgePractice,.02f);defense.leftWeight=1;defense.rightWeight=defense.backWeight=0;
                int blocked=ai.DodgeDamageBlockedCount;Author(inside?.14f:.36f,radius:8);StartVisibleAttack();yield return Until(()=>player.ComboStep==0);
                Assert.That(ai.DodgeDamageBlockedCount-blocked,Is.EqualTo(inside?1:0));Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(inside?1000:995));
                Assert.That(dummy.GuardBlockCount+dummy.ParrySuccessCount,Is.Zero);Assert.That(combat.groundDash.evadeStartDelay,Is.EqualTo(.04f));Assert.That(combat.groundDash.evadeDuration,Is.EqualTo(.10f));
            }
            CompleteBody();
        }
        [UnityTest] public IEnumerator BlockedDodgeDoesNotRetryOrTeleportThroughWall()
        {
            yield return Pair(ShibuyaTrainingMode.DodgePractice,.02f);defense.leftWeight=1;defense.rightWeight=defense.backWeight=0;
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=dummy.transform.position+Vector3.right+Vector3.up;wall.transform.localScale=new Vector3(.2f,2,5);Physics.SyncTransforms();
            Vector3 start=dummy.transform.position;Author(.5f);StartVisibleAttack();yield return Until(()=>ai.DodgeAttemptCount==1);yield return Wait(.25f);
            Assert.That(ai.DodgeStartedCount,Is.Zero);Assert.That(ai.DodgeAttemptCount,Is.EqualTo(1));Assert.That(ai.DefenseRejectedCount,Is.EqualTo(1));Assert.That(Vector3.Distance(start,dummy.transform.position),Is.LessThan(.03f));Object.Destroy(wall);
            CompleteBody();
        }
        [UnityTest] public IEnumerator OneObservedActionCannotRepeatOrReadFutureBufferedStep()
        {
            yield return Pair(ShibuyaTrainingMode.Mixed,.02f);defense.mixedAllowsOffense=false;defense.guardWeight=defense.dodgeWeight=defense.parryWeight=0;defense.noResponseWeight=100;defense.minimumDecisionInterval=.05f;
            var m=Author(.6f);m.steps[0].hits[0].strike.attackRadius=.1f;m.steps[0].hits[0].strike.attackForwardOffset=.1f;m.steps=new[]{m.steps[0],m.steps[0]};
            StartVisibleAttack();yield return Until(()=>ai.NoResponseCount==1);
            // This test needs a valid future reservation, not the known first-instant expiry boundary.
            yield return Until(()=>player.CanReserveGroundAttack&&player.AttackElapsed>=(player.GroundAttackReservationOpensAt+player.AttackDuration)*.5f);Assert.That(player.TryAttack(),Is.True);
            Record("4F1-source-buffer-accepted",new{clock=player.CombatTime,elapsed=player.AttackElapsed,duration=player.AttackDuration,opens=player.GroundAttackReservationOpensAt,expires=typeof(ShibuyaFighter).GetField("bufferedUntil",Private).GetValue(player)});
            Assert.That(player.HasBufferedAttack,Is.True);int observed=ai.ThreatObservationCount;yield return Wait(.05f);Assert.That(ai.ThreatObservationCount,Is.EqualTo(observed));
            var trace=new List<object>();double deadline=Time.unscaledTimeAsDouble+3;int lastFrame=-1;
            while(player.ComboStep!=2&&Time.unscaledTimeAsDouble<deadline){if(Time.frameCount!=lastFrame){lastFrame=Time.frameCount;trace.Add(new{frame=lastFrame,dt=Time.deltaTime,elapsed=player.AttackElapsed,clock=player.CombatTime,stage=player.ComboStep,accepted=player.AcceptedAttackCount,buffer=player.HasBufferedAttack,observed=ai.ThreatObservationCount,response=ai.NoResponseCount});}yield return null;}
            Record("4F1-source-buffer-trace",trace);Assert.That(player.ComboStep,Is.EqualTo(2));yield return Until(()=>ai.ThreatObservationCount==2);yield return Until(()=>ai.NoResponseCount==2);
            yield return Wait(.3f);Assert.That(ai.ThreatObservationCount,Is.EqualTo(2));Assert.That(DefenseRequests,Is.Zero);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(1000));
            CompleteBody();
        }
        [UnityTest] public IEnumerator WeightedChoicesSeedReproduceAndDoNotChangeGlobalRandom()
        {
            yield return Pair(ShibuyaTrainingMode.Mixed);var method=typeof(ShibuyaTrainingAI).GetMethod("ChooseDefense",Private);ai.SetTrainingEnabled(true);var global=UnityEngine.Random.state;
            Func<ShibuyaTrainingDefenseAction[]> sample=()=>Enumerable.Range(0,2000).Select(_=>(ShibuyaTrainingDefenseAction)method.Invoke(ai,new object[]{defense})).ToArray();
            var first=sample();ai.SetTrainingEnabled(false);ai.SetTrainingEnabled(true);var second=sample();Assert.That(first,Is.EqualTo(second));Assert.That(UnityEngine.Random.state,Is.EqualTo(global));
            var actions=new[]{ShibuyaTrainingDefenseAction.Guard,ShibuyaTrainingDefenseAction.Dodge,ShibuyaTrainingDefenseAction.Parry,ShibuyaTrainingDefenseAction.None};
            var fractions=actions.Select(a=>first.Count(x=>x==a)/2000f).ToArray();for(int i=0;i<4;i++)Assert.That(fractions[i],Is.EqualTo(new[]{.5f,.25f,.1f,.15f}[i]).Within(.04f));
            defense.guardWeight=defense.dodgeWeight=defense.parryWeight=defense.noResponseWeight=0;Assert.That((ShibuyaTrainingDefenseAction)method.Invoke(ai,new object[]{defense}),Is.EqualTo(ShibuyaTrainingDefenseAction.None));
            defense.guardWeight=float.NaN;defense.parryWeight=float.PositiveInfinity;Assert.That((ShibuyaTrainingDefenseAction)method.Invoke(ai,new object[]{defense}),Is.EqualTo(ShibuyaTrainingDefenseAction.None));
            Record("4F1-weighted",new{fractions,seed=training.randomSeed});ai.SetTrainingEnabled(false);
            CompleteBody();
        }
        [UnityTest] public IEnumerator GuardHoldReleaseAllowsFreshPressButDecisionsKeepMinimumInterval()
        {
            yield return Pair(ShibuyaTrainingMode.GuardPractice,.02f);defense.minimumGuardHold=defense.maximumGuardHold=.15f;
            Author(.5f);StartVisibleAttack();yield return Until(()=>ai.GuardAttemptCount==1);double first=ai.LastDefenseAttemptAt;
            yield return Until(()=>!dummy.IsGuarding);Assert.That((bool)typeof(ShibuyaFighter).GetField("guardInputHeld",Private).GetValue(dummy),Is.False);
            yield return Until(()=>player.ComboStep==0);Assert.That(player.TryAttack(),Is.True);yield return Until(()=>ai.GuardAttemptCount==2);
            Assert.That(ai.LastDefenseAttemptAt-first,Is.GreaterThanOrEqualTo(.749));Assert.That(dummy.IsGuarding,Is.True);Assert.That(dummy.ParryAttemptCount,Is.EqualTo(2));
            Record("4F1-decision-spacing",new{seconds=ai.LastDefenseAttemptAt-first,configured=defense.minimumDecisionInterval});
            CompleteBody();
        }
        [UnityTest] public IEnumerator DodgeCooldownExcludesUnavailableChoiceWithoutRepeatedApiRequests()
        {
            yield return Pair(ShibuyaTrainingMode.DodgePractice,.02f);defense.minimumDecisionInterval=0;defense.leftWeight=1;defense.rightWeight=defense.backWeight=0;
            Author(.3f);StartVisibleAttack();yield return Until(()=>ai.DodgeStartedCount==1);yield return Until(()=>player.ComboStep==0);
            Assert.That(dummy.GroundDashCooldownRemaining,Is.GreaterThan(0));Assert.That(player.TryAttack(),Is.True);yield return Until(()=>ai.NoResponseCount==1);
            Assert.That(ai.DodgeAttemptCount,Is.EqualTo(1));Assert.That(ai.DodgeStartedCount,Is.EqualTo(1));yield return Until(()=>player.ComboStep==0&&dummy.GroundDashCooldownRemaining<=0);
            Assert.That(player.TryAttack(),Is.True);yield return Until(()=>ai.DodgeStartedCount==2);Assert.That(ai.DodgeAttemptCount,Is.EqualTo(2));
            CompleteBody();
        }
        [UnityTest] public IEnumerator OwnFourHitComboHasPriorityOverDefensiveThreat()
        {
            yield return Pair(ShibuyaTrainingMode.Mixed);training.reactionDelay=0;training.oneHitWeight=training.twoHitWeight=training.threeHitWeight=0;training.fourHitWeight=1;
            Author(1);ai.SetTrainingEnabled(true);yield return Until(()=>dummy.ComboStep==1);Assert.That(player.TryAttack(),Is.True);
            yield return Until(()=>ai.CompletedGroupCount==1,4);ai.SetTrainingEnabled(false);Assert.That(ai.StartedAttackCount,Is.EqualTo(4));Assert.That(ai.AttackRequestCount,Is.EqualTo(4));Assert.That(DefenseRequests,Is.Zero);
            Assert.That(player.Health.CurrentHealth,Is.EqualTo(15));Assert.That(dummy.GuardBlockCount+dummy.ParrySuccessCount,Is.Zero);
            CompleteBody();
        }
        [UnityTest] public IEnumerator OffAndModeSwitchReleaseOwnedGuardAndQueuedAttackOnly()
        {
            yield return Pair(ShibuyaTrainingMode.GuardPractice);Author(1);StartVisibleAttack();yield return Until(()=>dummy.IsGuarding);
            ai.SetMode(ShibuyaTrainingMode.ParryPractice);Assert.That(dummy.IsGuarding,Is.False);Assert.That((bool)typeof(ShibuyaFighter).GetField("guardInputHeld",Private).GetValue(dummy),Is.False);
            ai.SetTrainingEnabled(false);int attempts=DefenseRequests;yield return Wait(.2f);Assert.That(DefenseRequests,Is.EqualTo(attempts));
            yield return Pair(ShibuyaTrainingMode.Offensive);training.oneHitWeight=training.twoHitWeight=training.threeHitWeight=0;training.fourHitWeight=1;
            ai.SetTrainingEnabled(true);yield return Until(()=>dummy.HasBufferedAttack);int action=dummy.AcceptedAttackCount;float elapsed=dummy.AttackElapsed;
            ai.SetMode(ShibuyaTrainingMode.GuardPractice);Assert.That(dummy.HasBufferedAttack,Is.False);Assert.That(dummy.IsGroundAttackInProgress,Is.True);Assert.That(dummy.AttackElapsed,Is.EqualTo(elapsed));
            yield return Wait(.6f);Assert.That(dummy.AcceptedAttackCount,Is.EqualTo(action));ai.SetTrainingEnabled(false);
            CompleteBody();
        }
        [UnityTest] public IEnumerator ActualDownAndQuickRecoveryRejectDefenseUntilRecovery()
        {
            yield return Pair(ShibuyaTrainingMode.GuardPractice);Author(.03f,outcome:ShibuyaHitOutcome.Knockdown);StartVisibleAttack();yield return Until(()=>dummy.IsDowned);
            int attempts=DefenseRequests;yield return Until(()=>dummy.CanRequestDownRecovery);Assert.That(DefenseRequests,Is.EqualTo(attempts));Assert.That(dummy.TryQuickRecovery(Vector2.right,Vector3.forward,Vector3.right),Is.True);
            var direction=dummy.QuickRecoveryDirection;yield return Wait(.15f);Assert.That(dummy.IsQuickRecovering,Is.True);Assert.That(dummy.QuickRecoveryDirection,Is.EqualTo(direction));Assert.That(DefenseRequests,Is.EqualTo(attempts));
            yield return Until(()=>dummy.CanAcceptGroundCombatIntent);ai.SetTrainingEnabled(false);yield return Pair(ShibuyaTrainingMode.GuardPractice);Author(.5f);StartVisibleAttack();yield return Until(()=>dummy.IsGuarding);
            CompleteBody();
        }
        [UnityTest] public IEnumerator GrabCinematicDeathAndDisableCleanDefenseOwnership()
        {
            yield return Pair(ShibuyaTrainingMode.GuardPractice);ai.SetTrainingEnabled(true);frame.guardHeld=true;yield return Until(()=>player.IsGuarding);frame.attack=true;
            yield return Until(()=>dummy.IsGrabbed);int requests=DefenseRequests;yield return Wait(.05f);Assert.That(DefenseRequests,Is.EqualTo(requests));frame=default;yield return Until(()=>dummy.CanAcceptGroundCombatIntent&&player.CanAcceptGroundCombatIntent);
            ai.SetTrainingEnabled(false);yield return Pair(ShibuyaTrainingMode.GuardPractice);ai.SetTrainingEnabled(true);
            var film=ScriptableObject.CreateInstance<ShibuyaCinematicProfile>();film.name="PASS4B3 QA defense cinema";film.duration=.35f;
            var applied=dummy.Health.ReceiveDamage(new DamageContext(1,player.gameObject,DamageDeliveryType.PhysicalStrike));var session=ShibuyaCinematicSession.TryStartAfterApplied(player,dummy,film,applied,ShibuyaCinematicTrigger.Default,1234,0);Assert.That(session,Is.Not.Null);
            yield return Wait(.15f);Assert.That(DefenseRequests,Is.EqualTo(requests));yield return Until(()=>!dummy.IsInCinematic);
            var evt=typeof(ShibuyaFighter).GetField("GuardBlocked",Private);Func<int> subscribers=()=>((Delegate)evt.GetValue(dummy))?.GetInvocationList().Count(x=>x.Target==ai)??0;
            Assert.That(subscribers(),Is.EqualTo(1));ai.enabled=false;Assert.That(subscribers(),Is.Zero);ai.enabled=true;Assert.That(subscribers(),Is.EqualTo(1));Assert.That(ai.Running,Is.False);
            ai.SetTrainingEnabled(true);player.Health.Kill();yield return Until(()=>!ai.Running);Assert.That(DefenseRequests,Is.EqualTo(requests));Assert.That(dummy.IsGuarding,Is.False);
            CompleteBody();
        }
        [UnityTest] public IEnumerator ModeMenusKeepDefaultOffAndSelectEveryMode()
        {
            Assert.That(ai.Running,Is.False);foreach(ShibuyaTrainingMode mode in Enum.GetValues(typeof(ShibuyaTrainingMode))){ShibuyaTrainingAITool.SelectMode(mode);Assert.That(ai.Mode,Is.EqualTo(mode));Assert.That(ai.Running,Is.False);}
            Assert.That(ai.Target,Is.SameAs(player));Assert.That(training.defense,Is.SameAs(defense));Assert.That(dummy.GetComponent<ShibuyaPlayerInput>(),Is.Null);
            yield return null;
            CompleteBody();
        }
        [UnityTest] public IEnumerator ActualGrabCanBeatAnAiOwnedGuardWithoutDefensiveEscape()
        {
            yield return Pair(ShibuyaTrainingMode.GuardPractice);defense.minimumGuardHold=defense.maximumGuardHold=2;
            Author(.5f);StartVisibleAttack();yield return Until(()=>dummy.GuardBlockCount==1);yield return Until(()=>player.ComboStep==0);
            Assert.That(dummy.IsGuarding,Is.True);float hp=dummy.Health.CurrentHealth;frame.guardHeld=true;yield return Until(()=>player.IsGuarding);
            Assert.That(player.TryGrab(),Is.True);frame=default;yield return Until(()=>dummy.IsGrabbed);int attempts=DefenseRequests;
            yield return Until(()=>player.GrabSuccessCount==1);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(hp-15));Assert.That(dummy.IsGuarding,Is.False);
            Assert.That(DefenseRequests,Is.EqualTo(attempts));Assert.That(dummy.ParrySuccessCount,Is.Zero);ai.SetTrainingEnabled(false);CompleteBody();
        }
        [UnityTest] public IEnumerator OriginalSpeedGuardParryDodgeAndOffEvidence()
        {
            foreach(string name in new[]{"guard-success","guard-late","dodge","parry-success","parry-late","ai-off"}){
                var mode=name.StartsWith("parry")?ShibuyaTrainingMode.ParryPractice:name=="dodge"?ShibuyaTrainingMode.DodgePractice:ShibuyaTrainingMode.GuardPractice;
                yield return Pair(mode,name=="dodge"?.02f:.2f);defense.leftWeight=1;defense.rightWeight=defense.backWeight=0;
                Author(name.EndsWith("late")?.05f:name=="parry-success"?.3f:.5f);Pose(new Vector3(5,3,-6));if(name!="ai-off")ai.SetTrainingEnabled(true);Assert.That(player.TryAttack(),Is.True);
                yield return Film(name,2);ai.SetTrainingEnabled(false);
            }
            CompleteBody();
        }
        [UnityTest] public IEnumerator OriginalSpeedMixedEvidence()
        {
            yield return Pair(ShibuyaTrainingMode.Mixed);defense.minimumReaction=.16f;defense.maximumReaction=.30f;Author(.45f);Pose(new Vector3(5,3,-6));ai.SetTrainingEnabled(true);yield return Film("mixed",5,true);ai.SetTrainingEnabled(false);
            CompleteBody();
        }
        private IEnumerator Film(string name,float duration,bool repeat=false)
        {
            var rows=new List<object>();double start=Time.unscaledTimeAsDouble,next=0;int index=0;
            while(Time.unscaledTimeAsDouble-start<duration){
                if(repeat&&Time.unscaledTimeAsDouble>=next&&player.CanAcceptGroundCombatIntent&&!player.IsGroundAttackInProgress){player.TryAttack();next=Time.unscaledTimeAsDouble+.85;}
                Capture("4F1-media/"+name+"/frame-"+index.ToString("D4"));rows.Add(new{index,time=Time.unscaledTimeAsDouble-start,mode=ai.Mode.ToString(),state=ai.State.ToString(),observed=ai.ThreatObservationCount,guard=ai.DefenseGuardSuccessCount,parry=ai.DefenseParrySuccessCount,dodge=ai.DodgeStartedCount,blocked=ai.DodgeDamageBlockedCount,failures=ai.ReactionFailureCount,playerHp=player.Health.CurrentHealth,dummyHp=dummy.Health.CurrentHealth});index++;yield return Wait(1f/30);
            }Record("4F1-media/"+name+"-timestamps",rows);
        }
    }
}
