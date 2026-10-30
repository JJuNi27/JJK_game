using System;
using System.Collections;
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
    public sealed class ShibuyaPass4F0DataTests
    {
        [Test] public void TrainingDataKoreanInspectorUndoAndProtectedCombatValues()
        {
            var saved=AssetDatabase.LoadAssetAtPath<ShibuyaTrainingAIProfile>(ShibuyaTrainingAITool.ProfilePath);Assert.That(saved,Is.Not.Null);var p=Object.Instantiate(saved);
            try{
                Assert.That(p.detectionRange,Is.EqualTo(10));Assert.That(p.approachStartDistance,Is.EqualTo(2.3f));Assert.That(p.approachStopDistance,Is.EqualTo(1.55f));
                Assert.That(p.decisionInterval,Is.EqualTo(.15f));Assert.That(p.facingTolerance,Is.EqualTo(20));Assert.That(p.reactionDelay,Is.EqualTo(.2f));
                Assert.That(p.minimumRest,Is.EqualTo(.9f));Assert.That(p.maximumRest,Is.EqualTo(1.5f));Assert.That(p.useRunning,Is.False);
                Assert.That(new[]{p.oneHitWeight,p.twoHitWeight,p.threeHitWeight,p.fourHitWeight},Is.EqualTo(new[]{25f,35,25,15}));
                var so=new SerializedObject(p);var it=so.GetIterator();bool child=true;
                while(it.NextVisible(child)){child=false;if(it.name!="m_Script")Assert.That(ShibuyaTrainingAIProfileInspector.Labels.ContainsKey(it.name),Is.True,it.name);}
                var editor=UnityEditor.Editor.CreateEditor(p);Assert.That(editor,Is.TypeOf<ShibuyaTrainingAIProfileInspector>());Object.DestroyImmediate(editor);
                Undo.IncrementCurrentGroup();so.FindProperty("detectionRange").floatValue=12;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();Undo.PerformUndo();Assert.That(p.detectionRange,Is.EqualTo(10));
                var c=AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root+"/Data/Shibuya Combat Profile.asset");Assert.That(c.strikes.Select(s=>s.damage),Is.EqualTo(new[]{20f,20,20,25}));
            }finally{Undo.ClearUndo(p);Object.DestroyImmediate(p);}
        }
    }
    public sealed class ShibuyaPass4F0Tests : ShibuyaPass4CFixture
    {
        private ShibuyaTrainingAI ai;
        private ShibuyaTrainingAIProfile data;
        [UnitySetUp] public IEnumerator TrainingSetup()
        {
            data=Clone(AssetDatabase.LoadAssetAtPath<ShibuyaTrainingAIProfile>(ShibuyaTrainingAITool.ProfilePath));data.useFixedSeed=true;data.randomSeed=1729;
            dummy.Configure(combat,AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root+"/Data/Dummy Stats.asset"));
            ai=dummy.gameObject.AddComponent<ShibuyaTrainingAI>();ai.Configure(data,player);yield return Wait(.04f);
        }
        private IEnumerator Pair(float distance=1.4f)
        {
            ai.SetTrainingEnabled(false);frame=default;yield return Until(()=>dummy.ComboStep==0&&dummy.CanAcceptGroundCombatIntent&&player.CanAcceptGroundCombatIntent);
            Place(player,new Vector3(0,.03f,-8));Place(dummy,player.transform.position+Vector3.forward*distance);player.transform.rotation=Quaternion.identity;dummy.transform.rotation=Quaternion.Euler(0,180,0);
            player.Health.ResetHealth();yield return Wait(.06f);
        }
        private void Length(int n){data.oneHitWeight=n==1?1:0;data.twoHitWeight=n==2?1:0;data.threeHitWeight=n==3?1:0;data.fourHitWeight=n==4?1:0;}
        [UnityTest] public IEnumerator DefaultOffHasNoCommandsAndOnUsesActualMatchReferences()
        {
            Assert.That(ai.Running,Is.False);Assert.That(ai.State,Is.EqualTo(ShibuyaTrainingAIState.Disabled));Assert.That(ai.Target,Is.SameAs(player));
            Vector3 start=dummy.transform.position;yield return Wait(.4f);Assert.That(dummy.AcceptedAttackCount,Is.Zero);Assert.That(Vector3.ProjectOnPlane(dummy.transform.position-start,Vector3.up).magnitude,Is.LessThan(.001f));
            Assert.That(dummy.GetComponent<ShibuyaPlayerInput>(),Is.Null);Assert.That(ai.SetTrainingEnabled(true),Is.True);Assert.That(ai.Running,Is.True);ai.SetTrainingEnabled(false);
        }
        [UnityTest] public IEnumerator DetectionApproachStopAndHysteresisReapproach()
        {
            Length(1);data.reactionDelay=10;yield return Pair(10.2f);Assert.That(ai.SetTrainingEnabled(true),Is.True);yield return Wait(.3f);
            Assert.That(ai.GroupCount,Is.Zero);Assert.That(ai.State,Is.EqualTo(ShibuyaTrainingAIState.Idle));float initial=Vector3.Distance(dummy.transform.position,player.transform.position);
            yield return Wait(.2f);Assert.That(Vector3.Distance(dummy.transform.position,player.transform.position),Is.EqualTo(initial).Within(.02f));
            Place(player,dummy.transform.position+Vector3.back*9.8f);yield return Until(()=>ai.State==ShibuyaTrainingAIState.Approach);
            yield return Until(()=>ai.State==ShibuyaTrainingAIState.FaceTarget,4);float stopped=Vector3.ProjectOnPlane(dummy.transform.position-player.transform.position,Vector3.up).magnitude;
            Assert.That(stopped,Is.EqualTo(1.55f).Within(.04f));Vector3 held=dummy.transform.position;
            Place(player,held+Vector3.back*2);yield return Wait(.4f);Assert.That(Vector3.Distance(dummy.transform.position,held),Is.LessThan(.03f));
            Place(player,held+Vector3.back*2.4f);yield return Until(()=>ai.State==ShibuyaTrainingAIState.Approach);yield return Until(()=>ai.State==ShibuyaTrainingAIState.FaceTarget,2);
            Record("4F0-hysteresis",new{stopped,reapproach=Vector3.Distance(dummy.transform.position,player.transform.position),requests=ai.AttackRequestCount});
        }
        [UnityTest] public IEnumerator ConfiguredDecisionCadenceCachesRangeButInvalidTargetStopsImmediately()
        {
            yield return Pair(10.2f);data.decisionInterval=.4f;data.reactionDelay=10;ai.SetTrainingEnabled(true);yield return Wait(.03f);
            double next=(double)typeof(ShibuyaTrainingAI).GetField("nextDecisionAt",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(ai);
            Assert.That(next-Time.timeAsDouble,Is.GreaterThan(.25f));Vector3 before=dummy.transform.position;Place(player,dummy.transform.position+Vector3.back*9.8f);
            yield return Wait(.12f);Assert.That(ai.State,Is.EqualTo(ShibuyaTrainingAIState.Idle));Assert.That(Vector3.Distance(before,dummy.transform.position),Is.LessThan(.03f));
            yield return Until(()=>ai.State==ShibuyaTrainingAIState.Approach);player.gameObject.SetActive(false);yield return Until(()=>!ai.Running,.15f);
            Record("4F0-decision-cadence",new{configured=data.decisionInterval,invalidTargetStopsWithoutWaiting=true});
        }
        [UnityTest] public IEnumerator StationaryFacingTracksSidesWithinFighterTurnSpeedWithoutCameraChanges()
        {
            yield return Pair();data.reactionDelay=10;Vector3 cam=camera.transform.position;Quaternion rotation=camera.transform.rotation;ai.SetTrainingEnabled(true);
            yield return Until(()=>ai.State==ShibuyaTrainingAIState.FaceTarget);Vector3 origin=dummy.transform.position;
            Place(player,origin+Vector3.right*1.4f);double began=Time.timeAsDouble;Quaternion before=dummy.transform.rotation;yield return Wait(.04f);
            float angle=Quaternion.Angle(before,dummy.transform.rotation);float time=(float)(Time.timeAsDouble-began);
            Assert.That(angle,Is.GreaterThan(0));Assert.That(angle,Is.LessThanOrEqualTo(combat.turnSpeed*(time+.02f)));
            yield return Until(()=>Vector3.Angle(dummy.transform.forward,player.transform.position-dummy.transform.position)<2);
            Assert.That(Vector3.Distance(origin,dummy.transform.position),Is.LessThan(.03f));Assert.That(camera.transform.position,Is.EqualTo(cam));Assert.That(camera.transform.rotation,Is.EqualTo(rotation));Assert.That(input.LockOn.LockedTarget,Is.Null);
        }
        [UnityTest] public IEnumerator RealOneToFourGroupsHitAndSubmitExactlyOneRequestPerStep()
        {
            var rows=new System.Collections.Generic.List<object>();
            foreach(int n in new[]{1,2,3,4}){
                yield return Pair();Length(n);int requests=ai.AttackRequestCount,started=ai.StartedAttackCount,done=ai.CompletedGroupCount;
                float hp=player.Health.CurrentHealth;var steps=new System.Collections.Generic.List<int>();var times=new System.Collections.Generic.List<float>();
                Action<int> handler=step=>{steps.Add(step);times.Add(dummy.CombatTime);};dummy.AttackStarted+=handler;
                var trace=new System.Collections.Generic.List<object>();
                Assert.That(ai.SetTrainingEnabled(true),Is.True);double deadline=Time.unscaledTimeAsDouble+5;
                while(ai.CompletedGroupCount<=done&&Time.unscaledTimeAsDouble<deadline){
                    trace.Add(new{frame=Time.frameCount,dt=Time.deltaTime,state=ai.State.ToString(),reason=ai.LastReason,step=dummy.ComboStep,elapsed=dummy.AttackElapsed,duration=dummy.AttackDuration,opens=dummy.GroundAttackReservationOpensAt,buffer=dummy.HasBufferedAttack,can=dummy.CanReserveGroundAttack,requests=ai.AttackRequestCount,starts=ai.StartedAttackCount,group=ai.CurrentGroupLength,interrupted=ai.InterruptedGroupCount});yield return null;
                }
                Record("4F0-group-trace-"+n,trace);Assert.That(ai.CompletedGroupCount,Is.GreaterThan(done));ai.SetTrainingEnabled(false);dummy.AttackStarted-=handler;
                Assert.That(steps,Is.EqualTo(Enumerable.Range(1,n)));Assert.That(ai.AttackRequestCount-requests,Is.EqualTo(n));Assert.That(ai.StartedAttackCount-started,Is.EqualTo(n));
                float damage=n==4?85:20*n;Assert.That(hp-player.Health.CurrentHealth,Is.EqualTo(damage));Assert.That(ai.RejectedRequestCount,Is.Zero);
                for(int i=1;i<times.Count;i++)Assert.That(times[i]-times[i-1],Is.GreaterThan(.2f));
                rows.Add(new{n,damage,steps,times,requests=ai.AttackRequestCount-requests});yield return Until(()=>player.CanAcceptGroundCombatIntent);
            }
            Record("4F0-real-groups",rows);
        }
        [UnityTest] public IEnumerator OffWithdrawsOnlyOwnedBufferAndLetsCurrentAttackRecover()
        {
            yield return Pair();Length(4);ai.SetTrainingEnabled(true);yield return Until(()=>dummy.HasBufferedAttack);
            int accepted=dummy.AcceptedAttackCount;float elapsed=dummy.AttackElapsed;ai.SetTrainingEnabled(false);
            Assert.That(dummy.IsGroundAttackInProgress,Is.True);Assert.That(dummy.HasBufferedAttack,Is.False);Assert.That(dummy.AttackElapsed,Is.EqualTo(elapsed));
            yield return Wait(.8f);Assert.That(dummy.AcceptedAttackCount,Is.EqualTo(accepted));Assert.That(dummy.ComboStep,Is.Zero);Assert.That(ai.Running,Is.False);
            Assert.That(dummy.TryAttack(),Is.True); // Manual dummy tools remain usable.
        }
        [UnityTest] public IEnumerator SeededWeightsAndRestAreDataDrivenWithoutGlobalRandomChanges()
        {
            yield return Pair();var choose=typeof(ShibuyaTrainingAI).GetMethod("ChooseGroupLength",BindingFlags.Instance|BindingFlags.NonPublic);ai.SetTrainingEnabled(true);
            var global=UnityEngine.Random.state;var first=Enumerable.Range(0,2000).Select(_=>(int)choose.Invoke(ai,null)).ToArray();ai.SetTrainingEnabled(false);ai.SetTrainingEnabled(true);
            var second=Enumerable.Range(0,2000).Select(_=>(int)choose.Invoke(ai,null)).ToArray();Assert.That(first,Is.EqualTo(second));
            Assert.That(UnityEngine.Random.state,Is.EqualTo(global));
            foreach(int n in new[]{1,2,3,4}){float fraction=first.Count(x=>x==n)/2000f;float expected=new[]{.25f,.35f,.25f,.15f}[n-1];Assert.That(fraction,Is.EqualTo(expected).Within(.04f));}
            ai.SetTrainingEnabled(false);Length(1);data.minimumRest=.4f;data.maximumRest=.55f;int groups=ai.GroupCount;
            ai.SetTrainingEnabled(true);yield return Until(()=>ai.CompletedGroupCount>0);float pause=ai.NextAttackRemaining;int requests=ai.AttackRequestCount;
            Assert.That(pause,Is.InRange(.38f,.55f));yield return Wait(.25f);Assert.That(ai.AttackRequestCount,Is.EqualTo(requests));ai.SetTrainingEnabled(false);
            Record("4F0-random",new{fractions=Enumerable.Range(1,4).Select(n=>first.Count(x=>x==n)/2000f).ToArray(),pause});
        }
        [UnityTest] public IEnumerator RealHeldGuardBlocksAiAndParryCancelsItsQueuedCombo()
        {
            yield return Pair();Length(4);frame.guardHeld=true;yield return Until(()=>player.IsGuarding);yield return Wait(.15f);
            float hp=player.Health.CurrentHealth;ai.SetTrainingEnabled(true);yield return Until(()=>player.GuardBlockCount>0);
            Assert.That(player.Health.CurrentHealth,Is.EqualTo(hp));Assert.That(dummy.ConfirmedHitCount,Is.Zero);ai.SetTrainingEnabled(false);frame=default;
            yield return Until(()=>player.ParryRetryRemaining<=0&&dummy.ComboStep==0);yield return Pair();Length(4);ai.SetTrainingEnabled(true);
            yield return Until(()=>dummy.ComboStep==1&&dummy.AttackElapsed>=.04f);frame.guardHeld=true;yield return Until(()=>player.ParrySuccessCount==1);
            int actions=dummy.AcceptedAttackCount;yield return Until(()=>ai.State==ShibuyaTrainingAIState.UnableToAct);Assert.That(dummy.HasBufferedAttack,Is.False);
            Vector3 start=dummy.transform.position;yield return Wait(.15f);Assert.That(dummy.AcceptedAttackCount,Is.EqualTo(actions));Assert.That(dummy.ComboStep,Is.Zero);
            Assert.That(Vector3.Distance(start,dummy.transform.position),Is.GreaterThan(.1f));frame=default;
            yield return Until(()=>ai.GroupCount>=3,5);Assert.That(ai.InterruptedGroupCount,Is.GreaterThan(0));ai.SetTrainingEnabled(false);
        }
        [UnityTest] public IEnumerator StraightWallBlocksMovementAndAttackThenResumesWhenRemoved()
        {
            yield return Pair(5);var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=(player.transform.position+dummy.transform.position)*.5f+Vector3.up;wall.transform.localScale=new Vector3(5,2,.2f);Physics.SyncTransforms();
            Vector3 start=dummy.transform.position;ai.SetTrainingEnabled(true);yield return Until(()=>ai.State==ShibuyaTrainingAIState.Blocked);yield return Wait(.35f);
            Assert.That(Vector3.Distance(dummy.transform.position,start),Is.LessThan(.03f));Assert.That(ai.AttackRequestCount,Is.Zero);Object.Destroy(wall);
            yield return Until(()=>ai.State==ShibuyaTrainingAIState.Approach);yield return Until(()=>ai.StartedAttackCount>0,4);ai.SetTrainingEnabled(false);
        }
        [UnityTest] public IEnumerator TargetDeathInactiveAndOwnerDisableStopAndCleanEvents()
        {
            yield return Pair(4);ai.SetTrainingEnabled(true);player.gameObject.SetActive(false);yield return Until(()=>!ai.Running);
            Assert.That(ai.SetTrainingEnabled(true),Is.False);player.gameObject.SetActive(true);yield return Wait(.1f);Assert.That(ai.Running,Is.False);
            ai.SetTrainingEnabled(true);ai.enabled=false;Assert.That(ai.Running,Is.False);ai.enabled=true;yield return Wait(.1f);Assert.That(ai.Running,Is.False);
            Assert.That(ai.SetTrainingEnabled(true),Is.True);player.Health.Kill();yield return Until(()=>!ai.Running);Assert.That(ai.SetTrainingEnabled(true),Is.False);
        }
        [UnityTest] public IEnumerator ProfilePermissionAndPlayerOwnershipNeverAutoEnable()
        {
            yield return Pair();data.trainingAllowed=false;Assert.That(ai.SetTrainingEnabled(true),Is.False);Assert.That(ai.AttackRequestCount,Is.Zero);
            data.trainingAllowed=true;Assert.That(ai.SetTrainingEnabled(true),Is.True);data.trainingAllowed=false;yield return Until(()=>!ai.Running);
            var wrong=player.gameObject.AddComponent<ShibuyaTrainingAI>();wrong.Configure(data,dummy);data.trainingAllowed=true;
            Assert.That(wrong.SetTrainingEnabled(true),Is.False);Object.Destroy(wrong);
        }
        [UnityTest] public IEnumerator ActualDownQuickRecoveryStunAndCinematicKeepTheirOwnership()
        {
            yield return Pair();data.reactionDelay=10;ai.SetTrainingEnabled(true);
            var strike=combat.strikes[0];Assert.That(dummy.ReceiveStrike(new DamageContext(1,player.gameObject,DamageDeliveryType.PhysicalStrike),strike),Is.EqualTo(DamageResolution.Applied));
            dummy.ApplyHitReaction(Vector3.right,.25f);yield return Until(()=>ai.State==ShibuyaTrainingAIState.UnableToAct);
            int requests=ai.AttackRequestCount;yield return Wait(.15f);Assert.That(ai.AttackRequestCount,Is.EqualTo(requests));yield return Until(()=>dummy.CanAcceptGroundCombatIntent);
            ai.SetTrainingEnabled(false);yield return Pair();data.reactionDelay=10;ai.SetTrainingEnabled(true);
            var move=ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();move.name="PASS4B3 QA AI actual knockdown";
            var step=JsonUtility.FromJson<ShibuyaMovesetStep>(JsonUtility.ToJson(combat.finishers.downFinisher));
            foreach(var h in step.hits){h.strike.damage=1;h.strike.startup=.01f;h.strike.activeTime=.03f;h.strike.recovery=.03f;h.strike.advanceDistance=0;h.strike.knockbackSpeed=0;h.strike.hitStun=.01f;h.strike.hitStopDuration=0;h.cinematic=null;}
            move.steps=new[]{step};Assert.That(player.SetMoveset(move),Is.True);Assert.That(player.TryAttack(),Is.True);yield return Until(()=>dummy.IsDowned);
            requests=ai.AttackRequestCount;yield return Until(()=>dummy.CanRequestDownRecovery);Assert.That(ai.AttackRequestCount,Is.EqualTo(requests));
            Assert.That(dummy.TryQuickRecovery(Vector2.right,Vector3.forward,Vector3.right),Is.True);Vector3 dir=dummy.QuickRecoveryDirection;
            yield return Wait(.12f);Assert.That(dummy.IsQuickRecovering,Is.True);Assert.That(dummy.QuickRecoveryDirection,Is.EqualTo(dir));Assert.That(ai.AttackRequestCount,Is.EqualTo(requests));
            yield return Until(()=>dummy.CanAcceptGroundCombatIntent);ai.SetTrainingEnabled(false);yield return Pair();data.reactionDelay=10;ai.SetTrainingEnabled(true);
            var film=ScriptableObject.CreateInstance<ShibuyaCinematicProfile>();film.name="PASS4B3 QA AI film";film.duration=.3f;
            var result=dummy.Health.ReceiveDamage(new DamageContext(1,player.gameObject,DamageDeliveryType.PhysicalStrike));
            var session=ShibuyaCinematicSession.TryStartAfterApplied(player,dummy,film,result,ShibuyaCinematicTrigger.Default,9001,0);Assert.That(session,Is.Not.Null);
            Quaternion facing=dummy.transform.rotation;requests=ai.AttackRequestCount;yield return Wait(.15f);Assert.That(dummy.IsInCinematic,Is.True);Assert.That(ai.AttackRequestCount,Is.EqualTo(requests));Assert.That(dummy.transform.rotation,Is.EqualTo(facing));
            yield return Until(()=>!dummy.IsInCinematic);data.reactionDelay=0;ai.SetTrainingEnabled(false);ai.SetTrainingEnabled(true);yield return Until(()=>ai.StartedAttackCount>0,4);ai.SetTrainingEnabled(false);
            Assert.That(Time.timeScale,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator ActualPlayerGrabSuspendsAiUntilExistingRecoveryEnds()
        {
            yield return Pair();data.reactionDelay=10;ai.SetTrainingEnabled(true);frame.guardHeld=true;yield return Until(()=>player.IsGuarding);frame.attack=true;
            yield return Until(()=>dummy.IsGrabbed);int requests=ai.AttackRequestCount;yield return Until(()=>ai.State==ShibuyaTrainingAIState.UnableToAct);
            Assert.That(dummy.CanAcceptGroundCombatIntent,Is.False);Assert.That(ai.AttackRequestCount,Is.EqualTo(requests));frame=default;
            yield return Until(()=>dummy.CanAcceptGroundCombatIntent);Assert.That(player.GrabSuccessCount,Is.EqualTo(1));
            data.reactionDelay=0;ai.SetTrainingEnabled(false);ai.SetTrainingEnabled(true);yield return Until(()=>ai.StartedAttackCount>0,4);ai.SetTrainingEnabled(false);
            Assert.That(dummy.GrabAttemptCount,Is.Zero);Assert.That(dummy.GroundDashCount,Is.Zero);
        }
        [UnityTest] public IEnumerator ActualSideDashEscapesAiAndDoesNotCreateConfirmedHit()
        {
            yield return Pair();Length(1);ai.SetTrainingEnabled(true);yield return Until(()=>dummy.ComboStep==1&&dummy.AttackElapsed>=.03f);
            float hp=player.Health.CurrentHealth;frame.move=Vector2.left;frame.dash=true;yield return Until(()=>player.GroundDashCount==1);frame.dash=false;frame.move=Vector2.zero;
            yield return Until(()=>ai.CompletedGroupCount==1);ai.SetTrainingEnabled(false);Assert.That(player.Health.CurrentHealth,Is.EqualTo(hp));Assert.That(dummy.ConfirmedHitCount,Is.Zero);
            Assert.That(player.GroundDashDirection.x,Is.LessThan(-.99f));
        }
        [UnityTest] public IEnumerator MenuToggleUsesMatchReferencesAndRestartReturnsToManualDummy()
        {
            ai.SetTrainingEnabled(false);Object.DestroyImmediate(ai);ai=null;var match=Object.FindFirstObjectByType<ShibuyaMatch>();
            ShibuyaTrainingAITool.Toggle();ai=match.Dummy.GetComponent<ShibuyaTrainingAI>();Assert.That(ai,Is.Not.Null);Assert.That(ai.Target,Is.SameAs(match.Player));Assert.That(ai.Owner,Is.SameAs(match.Dummy));Assert.That(ai.Running,Is.True);
            ShibuyaTrainingAITool.Toggle();Assert.That(ai.Running,Is.False);ShibuyaTrainingAITool.Toggle();Assert.That(ai.Running,Is.True);
            match.Player.Health.Kill();yield return Until(()=>match.Finished&&!ai.Running);match.RestartBattle();
            yield return Until(()=>Object.FindFirstObjectByType<ShibuyaMatch>()!=null&&Object.FindFirstObjectByType<ShibuyaMatch>()!=match);
            var fresh=Object.FindFirstObjectByType<ShibuyaMatch>();fresh.Player.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default;
            Assert.That(fresh.Dummy.GetComponent<ShibuyaTrainingAI>(),Is.Null);Assert.That(fresh.Finished,Is.False);Assert.That(fresh.Player.Health.MaxHealth,Is.EqualTo(100));Assert.That(fresh.Dummy.Health.MaxHealth,Is.EqualTo(1000));
        }
        [UnityTest] public IEnumerator DestroyUnsubscribesAndDefaultOffDoesNotReactivate()
        {
            yield return Pair();var evt=typeof(ShibuyaFighter).GetField("AttackStarted",BindingFlags.Instance|BindingFlags.NonPublic);
            Func<int> callbacks=()=>((Delegate)evt.GetValue(dummy))?.GetInvocationList().Count(x=>x.Target==ai)??0;
            Assert.That(callbacks(),Is.EqualTo(1));ai.enabled=false;Assert.That(callbacks(),Is.Zero);ai.enabled=true;Assert.That(callbacks(),Is.EqualTo(1));Assert.That(ai.Running,Is.False);
            var removed=ai;Object.DestroyImmediate(ai);Assert.That(((Delegate)evt.GetValue(dummy))?.GetInvocationList().Any(x=>ReferenceEquals(x.Target,removed))??false,Is.False);
            Assert.That(dummy.TryAttack(),Is.True);yield return Until(()=>dummy.ComboStep==0);
        }
        [UnityTest] public IEnumerator OriginalSpeedApproachComboParryResumeAndOffEvidence()
        {
            foreach(string mode in new[]{"approach-combo","parry-resume","ai-off"}){
                yield return Pair(mode=="approach-combo"?4:1.4f);Length(4);Pose(new Vector3(5,3,-6));bool parry=false,released=false;int baseline=ai.StartedAttackCount,completedBefore=ai.CompletedGroupCount,groupsBefore=ai.GroupCount;
                if(mode!="ai-off")ai.SetTrainingEnabled(true);
                var rows=new System.Collections.Generic.List<object>();double start=Time.unscaledTimeAsDouble;int index=0;
                while(Time.unscaledTimeAsDouble-start<5){
                    if(ai.CompletedGroupCount>completedBefore)ai.SetTrainingEnabled(false);
                    if(mode=="parry-resume"&&!parry&&dummy.ComboStep==1&&dummy.AttackElapsed>=.03f){frame.guardHeld=true;parry=true;}
                    if(parry&&!released&&player.ParrySuccessCount>0){frame=default;released=true;}
                    Capture("4F0-media/"+mode+"/frame-"+index.ToString("D4"));rows.Add(new{index,time=Time.unscaledTimeAsDouble-start,state=ai.State.ToString(),stage=dummy.ComboStep,requests=ai.AttackRequestCount,starts=ai.StartedAttackCount,parries=player.ParrySuccessCount,hp=player.Health.CurrentHealth});index++;yield return Wait(1f/30);
                }
                Record("4F0-media/"+mode+"-timestamps",rows);ai.SetTrainingEnabled(false);frame=default;
                if(mode=="ai-off")Assert.That(ai.StartedAttackCount,Is.EqualTo(baseline));
                else Assert.That(ai.CompletedGroupCount-completedBefore,Is.EqualTo(1));
                if(mode=="parry-resume"){Assert.That(player.ParrySuccessCount,Is.EqualTo(1));Assert.That(ai.GroupCount-groupsBefore,Is.GreaterThan(1));}
                yield return Wait(.04f);
            }
        }
    }
}
