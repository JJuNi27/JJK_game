using System;
using System.Collections;
using System.Linq;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace JJKGame.Shibuya.EditorTools
{
    // Same decision as Infinity; no bypass trait may leak from the scoped grab transaction.
    public sealed class ShibuyaGrabDefenseProbe : MonoBehaviour,IDamageGuard
    {
        public int calls; public bool sawBypass;
        public DamageGuardDecision EvaluateDamage(DamageContext c){calls++;sawBypass|=c.BypassesInfinity;return c.BypassesInfinity?DamageGuardDecision.NoDecision:DamageGuardDecision.Block;}
    }
    public sealed class ShibuyaPass4D2DataTests
    {
        [Test] public void GrabDataKoreanInspectorUndoReferencesAndProtectedValues()
        {
            var c=AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root+"/Data/Shibuya Combat Profile.asset");
            Assert.That(c.grab,Is.Not.Null);var p=Object.Instantiate(c.grab);
            try{
                Assert.That(p.range,Is.EqualTo(1.55f));Assert.That(p.halfAngle,Is.EqualTo(50));Assert.That(p.startup,Is.EqualTo(.18f));Assert.That(p.activeTime,Is.EqualTo(.08f));
                Assert.That(p.damage,Is.EqualTo(15));Assert.That(p.contactDuration,Is.EqualTo(.1f));Assert.That(p.pushDistance,Is.EqualTo(1.4f));Assert.That(p.victimStun,Is.EqualTo(.4f));
                Assert.That(p.successRecovery,Is.EqualTo(.25f));Assert.That(p.failureRecovery,Is.EqualTo(.5f));
                var so=new SerializedObject(p);var it=so.GetIterator();bool child=true;
                while(it.NextVisible(child)){child=false;if(it.name!="m_Script")Assert.That(ShibuyaGrabInspector.Labels.ContainsKey(it.name),Is.True,it.name);}
                var editor=UnityEditor.Editor.CreateEditor(p);Assert.That(editor,Is.TypeOf<ShibuyaGrabInspector>());Object.DestroyImmediate(editor);
                Undo.IncrementCurrentGroup();so.FindProperty("range").floatValue=2;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();Undo.PerformUndo();Assert.That(p.range,Is.EqualTo(1.55f));
                Assert.That(c.strikes.Select(s=>s.damage).ToArray(),Is.EqualTo(new[]{20f,20,20,25}));
                Assert.That(c.groundDash.forwardDistance,Is.EqualTo(5.25f));Assert.That(c.guard.parry.windowDuration,Is.EqualTo(.12f));
            }finally{Undo.ClearUndo(p);Object.DestroyImmediate(p);}
        }
    }
    public sealed class ShibuyaPass4D2Tests : ShibuyaPass4CFixture
    {
        private ShibuyaGrabProfile grab;
        [UnitySetUp] public IEnumerator GrabSetup(){grab=Clone(combat.grab);combat.grab=grab;yield return null;}
        private IEnumerator Pair(float distance=1.4f,float angle=0)
        {
            frame=default;player.SetGuardHeld(false);dummy.SetGuardHeld(false);
            Place(player,new Vector3(0,.03f,-8));Place(dummy,player.transform.position+Quaternion.AngleAxis(angle,Vector3.up)*Vector3.forward*distance);
            player.transform.rotation=Quaternion.identity;dummy.transform.rotation=Quaternion.Euler(0,180,0);yield return Wait(.08f);
        }
        private IEnumerator Press(bool simultaneous=false)
        {
            int old=player.GrabAttemptCount;
            frame.attack=false;frame.guardHeld=false;yield return null;yield return null;
            frame.guardHeld=true;if(simultaneous)frame.attack=true;
            if(!simultaneous){yield return Until(()=>player.IsGuarding);frame.attack=true;}
            yield return Until(()=>player.GrabAttemptCount>old);
        }
        private IEnumerator End(){yield return Until(()=>!player.IsGrabBusy);}
        [UnityTest] public IEnumerator GuardClickPriorityDamageOncePushStunAndNoAutomaticRearm()
        {
            yield return Pair();float hp=dummy.Health.CurrentHealth;int hits=0,attack=0,success=0,failed=0;Vector3 pushedFrom=Vector3.zero;float stun=0;
            player.HitConfirmed+=(_,__)=>hits++;player.AttackStarted+=_=>attack++;
            player.GrabSucceeded+=_=>{success++;pushedFrom=dummy.transform.position;stun=(float)typeof(ShibuyaFighter).GetField("stunnedUntil",Private).GetValue(dummy)-dummy.CombatTime;};player.GrabFailed+=_=>failed++;
            yield return Press();Assert.That(player.IsParryWindowOpen,Is.False);Assert.That(player.TryAttack(),Is.False);
            Assert.That(player.TryGroundDash(Vector2.up,Vector3.forward,Vector3.right),Is.False);
            frame.jump=true;frame.auraHeld=true;
            yield return Until(()=>success==1);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(hp-15));Assert.That(stun,Is.EqualTo(.4f).Within(.001f));
            Assert.That(input.EnergyCharge.IsCharging,Is.False);frame.jump=frame.auraHeld=false;yield return End();yield return Wait(.3f);
            float distance=Vector3.ProjectOnPlane(dummy.transform.position-pushedFrom,Vector3.up).magnitude;
            Assert.That(distance,Is.EqualTo(1.4f).Within(.03f));Assert.That(player.GrabAttemptCount,Is.EqualTo(1));
            Assert.That(hits+attack+player.ConfirmedHitCount+player.AcceptedAttackCount,Is.Zero);Assert.That(failed,Is.Zero);
            Assert.That(player.IsGuarding,Is.False);Assert.That(input.EnergyCharge.IsCharging,Is.False);
            frame=default;yield return Wait(.04f);frame.guardHeld=true;yield return Until(()=>player.IsGuarding);
            Assert.That(dummy.GuardBreakCount,Is.Zero);Record("4D2-success",new{damage=hp-dummy.Health.CurrentHealth,distance,stun,success,hits,attack});
        }
        [UnityTest] public IEnumerator SimultaneousButtonsConsumeOneGrabAndCancelParry()
        {
            yield return Pair();yield return Press(true);Assert.That(player.IsGuarding,Is.False);Assert.That(player.IsParryWindowOpen,Is.False);
            yield return End();yield return Wait(.6f);Assert.That(player.GrabAttemptCount,Is.EqualTo(1));Assert.That(player.AcceptedAttackCount,Is.Zero);Assert.That(player.GrabSuccessCount,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator GuardAndLiveParryAreBypassedWithoutGuardDamageOrBreak()
        {
            yield return Pair();int parries=dummy.ParrySuccessCount;float g=dummy.CurrentGuard,hp=dummy.Health.CurrentHealth;
            yield return Press();yield return Until(()=>player.GrabElapsed>=.12f);dummy.SetGuardHeld(true);Assert.That(dummy.IsParryWindowOpen,Is.True);
            yield return Until(()=>player.GrabSuccessCount==1);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(hp-15));Assert.That(dummy.CurrentGuard,Is.EqualTo(g));
            Assert.That(dummy.GuardBlockCount+dummy.GuardBreakCount,Is.Zero);Assert.That(dummy.ParrySuccessCount,Is.EqualTo(parries));Assert.That(dummy.IsGuarding,Is.False);
        }
        [UnityTest] public IEnumerator RangeAngleAndWallRejectAndWhiffPenaltyHasNoArmor()
        {
            foreach(var scenario in new[]{new[]{2f,0},new[]{1.4f,65},new[]{1.4f,180},new[]{1.4f,0}}){
                yield return Pair(scenario[0],scenario[1]);GameObject wall=null;
                if(scenario[0]<2&&scenario[1]==0){wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=player.transform.position+new Vector3(0,1,.7f);wall.transform.localScale=new Vector3(3,2,.15f);Physics.SyncTransforms();}
                int old=player.GrabAttemptCount;frame.guardHeld=true;yield return Until(()=>player.IsGuarding);frame.attack=true;yield return Until(()=>player.GrabAttemptCount==old+1);
                frame.attack=false;yield return Until(()=>player.GrabPhase==ShibuyaGrabPhase.FailureRecovery);float began=player.CombatTime;
                Assert.That(player.TryAttack(),Is.False);Assert.That(player.TryGroundDash(Vector2.right,Vector3.forward,Vector3.right),Is.False);
                float hp=player.Health.CurrentHealth;Assert.That(player.Health.ReceiveDamage(new DamageContext(1,dummy.gameObject,DamageDeliveryType.PhysicalStrike)),Is.EqualTo(DamageResolution.Applied));
                Assert.That(player.Health.CurrentHealth,Is.EqualTo(hp-1));yield return End();Assert.That(player.CombatTime-began,Is.InRange(.49f,.56f));
                if(wall!=null)Object.Destroy(wall);frame=default;yield return Wait(.06f);
            }
            Assert.That(player.GrabFailureCount,Is.EqualTo(4));Assert.That(player.GrabSuccessCount,Is.Zero);
        }
        [UnityTest] public IEnumerator DodgeInvulnerabilityRejectsButForwardDashIsEligible()
        {
            foreach(var direction in new[]{Vector2.right,Vector2.down}){
                yield return Pair();yield return Until(()=>dummy.GroundDashCooldownRemaining<=0);
                Assert.That(dummy.TryGroundDash(direction,Vector3.forward,Vector3.right),Is.True);yield return Until(()=>dummy.IsGroundDashEvading);
                float hp=dummy.Health.CurrentHealth; // Probe the actual acquisition predicate during the protected interval, not a fake success counter.
                player.SetGuardHeld(true);Assert.That(player.TryGrab(),Is.True);
                var valid=typeof(ShibuyaFighter).GetMethod("ValidGrabTarget",Private);
                Assert.That((bool)valid.Invoke(player,new object[]{dummy,false}),Is.False);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(hp));
                player.CancelGrabInput();yield return End();frame=default;yield return Wait(.08f);
            }
            yield return Pair();Assert.That(dummy.TryGroundDash(Vector2.up,Vector3.forward,Vector3.right),Is.True);Assert.That(dummy.IsGroundDashEvading,Is.False);
            player.SetGuardHeld(true);Assert.That(player.TryGrab(),Is.True);
            Assert.That((bool)typeof(ShibuyaFighter).GetMethod("ValidGrabTarget",Private).Invoke(player,new object[]{dummy,false}),Is.True);
            player.CancelGrabInput();
        }
        [UnityTest] public IEnumerator OrdinaryInvulnerabilityAndInfinityMeaningRemainIntact()
        {
            yield return Pair();dummy.Health.GrantInvulnerability(.8f);yield return Press();yield return End();Assert.That(player.GrabSuccessCount,Is.Zero);
            frame=default;yield return Wait(.9f);yield return Pair();var defense=dummy.gameObject.AddComponent<ShibuyaGrabDefenseProbe>();
            yield return Press();yield return End();Assert.That(player.GrabSuccessCount,Is.Zero);Assert.That(defense.calls,Is.GreaterThan(0));Assert.That(defense.sawBypass,Is.False);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(1000));Object.Destroy(defense);
            // Exercise the real Infinity decision without awakening its unrelated legacy character bootstrap.
            var root=new GameObject("PASS4D2 inactive Infinity decision probe");root.SetActive(false);
            try{
                var infinity=root.AddComponent<JJKGame.Player.GojoInfinityDefense>();
                typeof(JJKGame.Player.GojoInfinityDefense).GetField("variant",Private).SetValue(infinity,root.GetComponent<JJKGame.Player.GojoVariantController>());
                var context=(DamageContext)typeof(ShibuyaFighter).GetMethod("GrabDamageContext",Private).Invoke(player,new object[]{dummy});
                Assert.That(context.BypassesInfinity,Is.False);Assert.That(context.HasTrait(DamageTraits.Unblockable),Is.False);
                Assert.That(infinity.IsInfinityActive,Is.True);Assert.That(infinity.EvaluateDamage(context),Is.EqualTo(DamageGuardDecision.Block));
            }finally{Object.DestroyImmediate(root);}
        }
        [UnityTest] public IEnumerator ActiveAttackWithHeldRmbStillBuffersOrdinaryCombo()
        {
            yield return Pair();frame.attack=true;yield return Until(()=>player.AcceptedAttackCount==1);frame.attack=false;frame.guardHeld=true;
            yield return Until(()=>player.AttackElapsed>=.19f);frame.attack=true;
            yield return Until(()=>player.AcceptedAttackCount==2);Assert.That(player.GrabAttemptCount,Is.Zero);Assert.That(player.IsGuarding,Is.False);
        }
        [UnityTest] public IEnumerator ParryHeldRmbThenLmbIsOrdinaryCounterattack()
        {
            yield return Pair();frame.guardHeld=true;yield return Until(()=>player.IsGuarding);
            var ctx=new DamageContext(1,dummy.gameObject,DamageDeliveryType.PhysicalStrike);
            Assert.That(player.ReceiveStrike(ctx,combat.strikes[0]),Is.EqualTo(DamageResolution.Guarded));Assert.That(player.ParrySuccessCount,Is.EqualTo(1));
            yield return Until(()=>!player.IsParryRecovering);frame.attack=true;yield return Until(()=>player.AcceptedAttackCount==1);
            Assert.That(player.GrabAttemptCount,Is.Zero);Assert.That(player.IsGuarding,Is.False);
        }
        [UnityTest] public IEnumerator ContactInterruptedByDamageOrDisableReleasesBothActors()
        {
            yield return Pair();yield return Press();yield return Until(()=>dummy.IsGrabbed);Assert.That(dummy.TryAttack(),Is.False);
            player.Health.ReceiveDamage(new DamageContext(1,dummy.gameObject,DamageDeliveryType.PhysicalStrike));
            Assert.That(dummy.IsGrabbed,Is.False);Assert.That(player.GrabFailureCount,Is.EqualTo(1));yield return End();
            frame=default;yield return Wait(.04f);yield return Pair();yield return Press();yield return Until(()=>dummy.IsGrabbed);
            dummy.gameObject.SetActive(false);Assert.That(player.GrabFailureCount,Is.EqualTo(2));Assert.That(dummy.IsGrabbed,Is.False);
            yield return End();Assert.That(player.GrabSuccessCount,Is.Zero);
        }
        [UnityTest] public IEnumerator LockPreferenceOneTargetMultiColliderAndNoCinematic()
        {
            yield return Pair();var extra=GameObject.CreatePrimitive(PrimitiveType.Cube);extra.name="PASS4D2 extra collider";extra.transform.SetParent(dummy.transform,false);extra.transform.localPosition=Vector3.up;
            Select(dummy);yield return Press();yield return End();Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(985));Assert.That(player.GrabSuccessCount,Is.EqualTo(1));
            Assert.That(player.IsInCinematic||dummy.IsInCinematic,Is.False);Assert.That(player.HitStopCount,Is.Zero);Assert.That(Time.timeScale,Is.EqualTo(1));Object.Destroy(extra);
        }
        [UnityTest] public IEnumerator WallStopsPushAndBlockedMotionIsSpent()
        {
            yield return Pair();var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=dummy.transform.position+new Vector3(0,1,.7f);wall.transform.localScale=new Vector3(3,2,.2f);Physics.SyncTransforms();
            float z=dummy.transform.position.z;yield return Press();yield return Until(()=>player.GrabSuccessCount==1);yield return Wait(.42f);
            Assert.That(dummy.transform.position.z-z,Is.InRange(0,.36f));float blocked=dummy.transform.position.z;Object.Destroy(wall);yield return Wait(.12f);
            Assert.That(dummy.transform.position.z-blocked,Is.LessThan(.04f));Assert.That(dummy.transform.position.z-player.transform.position.z,Is.GreaterThan(.7f));
        }
        [UnityTest] public IEnumerator FocusLossDeathAndCombatEndCleanOwnership()
        {
            yield return Pair();yield return Press();yield return Until(()=>dummy.IsGrabbed);
            input.SendMessage("OnApplicationFocus",false);Assert.That(dummy.IsGrabbed,Is.False);yield return End();
            frame=default;yield return Wait(.04f);yield return Pair();yield return Press();yield return Until(()=>dummy.IsGrabbed);
            player.StopCombat();Assert.That(player.IsGrabBusy||dummy.IsGrabbed,Is.False);Assert.That(player.TryAttack(),Is.False);
        }
        [UnityTest] public IEnumerator ActualTimedSideAndBackDashEvadeEntireAttempt()
        {
            foreach(var d in new[]{Vector2.right,Vector2.down}){
                yield return Pair();yield return Until(()=>dummy.GroundDashCooldownRemaining<=0);int attempts=player.GrabAttemptCount;
                yield return Press();yield return Until(()=>player.GrabElapsed>=.13f);
                Assert.That(dummy.TryGroundDash(d,Vector3.forward,Vector3.right),Is.True);
                yield return Until(()=>player.GrabPhase==ShibuyaGrabPhase.FailureRecovery);yield return End();
                Assert.That(player.GrabAttemptCount,Is.EqualTo(attempts+1));Assert.That(player.GrabSuccessCount,Is.Zero);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(1000));
            }
        }
        [UnityTest] public IEnumerator TargetMovedOrBecameInvulnerableDuringContactIsReleasedWithoutDamage()
        {
            foreach(bool move in new[]{true,false}){
                yield return Pair();yield return Press();yield return Until(()=>dummy.IsGrabbed);
                if(move)Place(dummy,dummy.transform.position+Vector3.right*.3f);else dummy.Health.GrantInvulnerability(.15f);
                yield return Until(()=>!dummy.IsGrabbed);yield return End();Assert.That(player.GrabSuccessCount,Is.Zero);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(1000));
            }
            Assert.That(player.GrabFailureCount,Is.EqualTo(2));
        }
        [UnityTest] public IEnumerator VictimAndAttackerDeathEndPairWithoutResidualOwnership()
        {
            yield return Pair();yield return Press();yield return Until(()=>dummy.IsGrabbed);
            dummy.Health.Kill();yield return null;Assert.That(player.IsGrabBusy||dummy.IsGrabbed,Is.False);Assert.That(player.GrabSuccessCount,Is.Zero);
        }
        [UnityTest] public IEnumerator LethalGrabUsesHealthDeathAndBattleEndOnce()
        {
            yield return Pair();dummy.Health.SetCurrentHealth(15);int died=0;dummy.Health.Died+=_=>died++;
            yield return Press();yield return Until(()=>dummy.Health.IsDead);yield return null;
            Assert.That(died,Is.EqualTo(1));Assert.That(player.GrabSuccessCount,Is.EqualTo(1));Assert.That(player.IsGrabBusy||dummy.IsGrabbed,Is.False);
            Assert.That(player.State,Is.EqualTo(ShibuyaFighterState.Finished));
        }
        [UnityTest] public IEnumerator RealStrikeInterruptsGrabPreparationWithoutArmor()
        {
            yield return Pair();yield return Press();Assert.That(dummy.TryAttack(),Is.True);
            yield return Until(()=>dummy.ConfirmedHitCount==1);Assert.That(player.Health.CurrentHealth,Is.EqualTo(80));
            Assert.That(player.GrabFailureCount,Is.EqualTo(1));Assert.That(player.GrabSuccessCount,Is.Zero);Assert.That(dummy.IsGrabbed,Is.False);
            yield return End();Assert.That(player.AcceptedAttackCount,Is.Zero);Assert.That(player.IsGuarding,Is.False);
        }
        [UnityTest] public IEnumerator AttackerDeathDuringContactReleasesVictim()
        {
            yield return Pair();yield return Press();yield return Until(()=>dummy.IsGrabbed);player.Health.Kill();yield return null;
            Assert.That(player.IsGrabBusy||dummy.IsGrabbed,Is.False);Assert.That(player.GrabSuccessCount,Is.Zero);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(1000));
        }
        [UnityTest] public IEnumerator LockPreferenceNearestFallbackAndStableSingleTarget()
        {
            yield return Pair(1.5f);var obj=Object.Instantiate(dummy.gameObject);obj.name="PASS4D2 extra fighter";var other=obj.GetComponent<ShibuyaFighter>();
            Place(other,player.transform.position+Quaternion.AngleAxis(30,Vector3.up)*Vector3.forward*1.4f);yield return Wait(.05f);
            Select(dummy);yield return Press();yield return End();Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(985));Assert.That(other.Health.CurrentHealth,Is.EqualTo(1000));
            input.LockOn.Clear();frame=default;yield return Wait(.04f);Place(dummy,player.transform.position+Vector3.forward*3);
            yield return Press();yield return End();Assert.That(other.Health.CurrentHealth,Is.EqualTo(985));Assert.That(player.GrabSuccessCount,Is.EqualTo(2));
            Object.Destroy(obj);
        }
        [UnityTest] public IEnumerator ChargingGroundAndAttackGatesAndProfileRemovalReleasePair()
        {
            yield return Pair();frame.auraHeld=true;yield return Until(()=>input.EnergyCharge.IsCharging);frame.guardHeld=true;yield return Wait(.04f);
            Assert.That(player.IsGuarding,Is.False);Assert.That(player.TryGrab(),Is.False);
            frame=default;yield return Wait(.04f);yield return Press();yield return Until(()=>dummy.IsGrabbed);
            combat.grab=null;yield return Until(()=>!dummy.IsGrabbed);yield return End();Assert.That(player.GrabFailureCount,Is.EqualTo(1));
            Assert.That(player.GrabSuccessCount,Is.Zero);Assert.That(player.TryAttack(),Is.True);
        }
        [UnityTest] public IEnumerator DevelopmentToolArmsRealGuardAndCleansWithoutAssetWrites()
        {
            var window=ScriptableObject.CreateInstance<ShibuyaGrabTestWindow>();var t=typeof(ShibuyaGrabTestWindow);
            var flags=System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic;
            try{
                t.GetMethod("Schedule",flags).Invoke(null,new object[]{1});
                yield return Until(()=>dummy.IsGuarding,4);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(1000));
                yield return Press();yield return Until(()=>player.GrabSuccessCount==1);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(985));
                player.StopCombat();yield return null;Assert.That((bool)t.GetField("armed",flags).GetValue(null),Is.False);
            }finally{Object.DestroyImmediate(window);}
        }
        [UnityTest] public IEnumerator OriginalSpeedNormalGuardedAndMissGrabEvidence()
        {
            foreach(string mode in new[]{"normal","guarded","miss"}){
                yield return Until(()=>!player.IsGrabBusy&&dummy.State!=ShibuyaFighterState.Hitstunned);yield return Pair(mode=="miss"?3:1.4f);
                if(mode=="guarded")dummy.SetGuardHeld(true);
                // QA framing only: keep both bodies visible even in the deliberately out-of-range case.
                Pose(new Vector3(5.5f,3,4.5f));camera.transform.LookAt((player.transform.position+dummy.transform.position)*.5f+Vector3.up*1.1f);
                var rows=new System.Collections.Generic.List<object>();
                double start=Time.unscaledTimeAsDouble;bool held=false,clicked=false;int index=0;
                while(Time.unscaledTimeAsDouble-start<1.45){
                    double time=Time.unscaledTimeAsDouble-start;
                    if(!held&&time>=.15){frame.guardHeld=true;held=true;}
                    if(!clicked&&time>=.32){frame.attack=true;clicked=true;}
                    Capture("4D2-media/"+mode+"/frame-"+index.ToString("D4"));
                    var p=player.transform.position;var d=dummy.transform.position;
                    rows.Add(new{index,time,phase=player.GrabPhase.ToString(),held=dummy.IsGrabbed,player=new[]{p.x,p.y,p.z},victim=new[]{d.x,d.y,d.z},hp=dummy.Health.CurrentHealth,success=player.GrabSuccessCount,failure=player.GrabFailureCount});
                    index++;yield return Wait(1f/40);
                }
                Record("4D2-media/"+mode+"-timestamps",rows);frame=default;yield return Wait(.04f);
            }
            Assert.That(player.GrabSuccessCount,Is.EqualTo(2));Assert.That(player.GrabFailureCount,Is.EqualTo(1));Assert.That(player.AcceptedAttackCount,Is.Zero);
        }
    }
}
