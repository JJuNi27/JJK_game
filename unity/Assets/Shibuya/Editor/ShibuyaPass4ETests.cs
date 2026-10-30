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
    public sealed class ShibuyaPass4EDataTests
    {
        [Test] public void IndependentRecoveryDataKoreanInspectorUndoAndLockedDownSettings()
        {
            var c=AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root+"/Data/Shibuya Combat Profile.asset");
            var p=Object.Instantiate(c.quickRecovery);
            try{
                Assert.That(p.distance,Is.EqualTo(2.5f));Assert.That(p.duration,Is.EqualTo(.3f));Assert.That(p.evadeStart,Is.EqualTo(.04f));Assert.That(p.evadeEnd,Is.EqualTo(.18f));Assert.That(p.cooldownAfterEnd,Is.EqualTo(1));
                var d=c.finishers.downFinisher.hits[0].down;
                Assert.That(d.minimumDownTime,Is.EqualTo(.45f));Assert.That(d.naturalRecoveryTime,Is.EqualTo(.85f));Assert.That(d.getUpDuration,Is.EqualTo(.22f));
                var so=new SerializedObject(p);var it=so.GetIterator();bool child=true;
                while(it.NextVisible(child)){child=false;if(it.name!="m_Script")Assert.That(ShibuyaQuickRecoveryInspector.Labels.ContainsKey(it.name),Is.True,it.name);}
                var editor=UnityEditor.Editor.CreateEditor(p);Assert.That(editor,Is.TypeOf<ShibuyaQuickRecoveryInspector>());Object.DestroyImmediate(editor);
                Undo.IncrementCurrentGroup();so.FindProperty("distance").floatValue=3;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();Undo.PerformUndo();Assert.That(p.distance,Is.EqualTo(2.5f));
                Assert.That(c.groundDash.forwardDistance,Is.EqualTo(5.25f));Assert.That(c.grab.damage,Is.EqualTo(15));
            }finally{Undo.ClearUndo(p);Object.DestroyImmediate(p);}
        }
    }
    public sealed class ShibuyaPass4ETests : ShibuyaPass4CFixture
    {
        private ShibuyaQuickRecoveryProfile recovery;
        private string stage;
        private int checkpoint;
        private void Checkpoint(string label){Record("4E-check-"+TestContext.CurrentContext.Test.Name+"-"+(checkpoint++),new{label,time=Time.timeAsDouble,clock=player.CombatTime,quick=player.IsQuickRecovering,count=player.QuickRecoveryCount,elapsed=player.QuickRecoveryElapsed,cooldown=player.QuickRecoveryCooldownRemaining,snapshotCooldown=Field(player,"recoveryCooldown"),readyAt=typeof(ShibuyaFighter).GetField("recoveryReadyAt",Private).GetValue(player),grounded=player.IsGrounded,down=player.IsDowned,eligible=player.CanRequestDownRecovery,previousJump=typeof(ShibuyaPlayerInput).GetField("previousJump",Private).GetValue(input),jump=frame.jump});}
        private int sampledFrame=-1;
        private readonly System.Collections.Generic.List<object> trace=new System.Collections.Generic.List<object>();
        private void Sample()
        {
            if(player==null||recovery==null||!EditorApplication.isPlaying||sampledFrame==Time.frameCount)return;sampledFrame=Time.frameCount;
            trace.Add(new{stage,frame=Time.frameCount,dt=Time.deltaTime,time=Time.timeAsDouble,clock=player.CombatTime,phase=player.State.ToString(),down=player.IsDowned,eligible=player.CanRequestDownRecovery,
                quick=player.IsQuickRecovering,elapsed=player.QuickRecoveryElapsed,cooldown=player.QuickRecoveryCooldownRemaining,configuredCooldown=recovery.cooldownAfterEnd,
                direction=new[]{frame.move.x,frame.move.y},jump=frame.jump,aura=frame.auraHeld,attack=frame.attack,playerY=player.transform.position.y,enemyHits=dummy.ConfirmedHitCount,enemyActions=dummy.AcceptedAttackCount});
        }
        [UnitySetUp] public IEnumerator TraceSetup(){trace.Clear();EditorApplication.update+=Sample;yield return null;}
        [UnityTearDown] public IEnumerator TraceEnd(){EditorApplication.update-=Sample;Record("4E-trace-"+TestContext.CurrentContext.Test.Name,trace);yield return null;}
        private static float Field(ShibuyaFighter f,string name)=>(float)typeof(ShibuyaFighter).GetField(name,Private).GetValue(f);
        [UnitySetUp] public IEnumerator RecoverySetup(){recovery=Clone(combat.quickRecovery);combat.quickRecovery=recovery;yield return null;}
        private IEnumerator Down(bool elevated=false)
        {
            frame=default;yield return Until(()=>!player.IsDowned&&!player.IsQuickRecovering&&!player.IsGrabBusy&&player.State!=ShibuyaFighterState.Hitstunned);
            Place(player,new Vector3(0,elevated?5:.03f,-8));Place(dummy,new Vector3(0,.03f,-6.6f));player.transform.rotation=Quaternion.identity;dummy.transform.rotation=Quaternion.Euler(0,180,0);
            var m=ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();m.name="PASS4B3 QA actual quick recovery knockdown";
            var step=JsonUtility.FromJson<ShibuyaMovesetStep>(JsonUtility.ToJson(combat.finishers.downFinisher));
            foreach(var h in step.hits){h.strike.damage=1;h.strike.startup=.01f;h.strike.activeTime=.03f;h.strike.recovery=.03f;h.strike.advanceDistance=0;h.strike.knockbackSpeed=0;h.strike.hitStun=.01f;h.strike.hitStopDuration=0;h.cinematic=null;if(elevated){h.strike.attackHeight=6;h.strike.attackRadius=1.5f;}}
            m.steps=new[]{step};Assert.That(dummy.SetMoveset(m),Is.True);Assert.That(dummy.TryAttack(),Is.True);
            stage="await actual knockdown";yield return Until(()=>player.IsDowned);Assert.That(player.Health.CurrentHealth,Is.LessThan(100));
            stage="await attacker recovery";yield return Until(()=>dummy.ComboStep==0);Place(dummy,new Vector3(8,.03f,8));stage="down ready";
        }
        private IEnumerator Ready(){stage="await minimum down wait";yield return Until(()=>player.CanRequestDownRecovery);stage="eligible";}
        private IEnumerator Escape(Vector2 direction)
        {
            int count=player.QuickRecoveryCount;frame.move=direction;frame.jump=true;stage="await new escape";yield return Until(()=>player.QuickRecoveryCount>count);frame.move=Vector2.zero;stage="escaping";Checkpoint("Escape returned");
        }
        private DamageResolution Hit(DamageDeliveryType kind=DamageDeliveryType.PhysicalStrike,DamageTraits traits=DamageTraits.None)=>player.ReceiveStrike(new DamageContext(1,dummy.gameObject,kind,traits),combat.strikes[0]);
        [UnityTest] public IEnumerator RealDownMinimumWaitEarlyHeldSpaceNoBufferAndNaturalWake()
        {
            yield return Down();yield return Until(()=>Field(player,"downStartedAt")>0);float began=Field(player,"downStartedAt");
            frame.move=Vector2.right;frame.jump=true;yield return Wait(.25f);Assert.That(player.QuickRecoveryCount,Is.Zero);
            yield return Ready();Assert.That(player.CombatTime-began,Is.GreaterThanOrEqualTo(.45f));Assert.That(player.QuickRecoveryCount,Is.Zero);
            yield return Until(()=>player.IsGettingUp);float wake=player.CombatTime-began;Assert.That(wake,Is.InRange(.85f,.92f));
            yield return Until(()=>!player.IsDowned);yield return Wait(.08f);Assert.That(player.QuickRecoveryCount,Is.Zero);Assert.That(player.VerticalVelocity,Is.LessThanOrEqualTo(0));
            Record("4E-natural",new{minimum=.45f,wake,finish=player.CombatTime-began,holdDidNotJump=player.VerticalVelocity<=0});
        }
        [UnityTest] public IEnumerator CardinalAndDiagonalCameraBasisDistanceDurationCooldown()
        {
            var rows=new System.Collections.Generic.List<object>();
            foreach(var dir in new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right,new Vector2(1,1)}){
                yield return Until(()=>player.QuickRecoveryCooldownRemaining<=0);yield return Down();yield return Ready();
                camera.transform.rotation=Quaternion.Euler(25,90,0);Vector3 start=player.transform.position;double time=Time.timeAsDouble;
                yield return Escape(dir);Vector3 direction=player.QuickRecoveryDirection;
                Assert.That(Vector3.Angle(direction,(Vector3.right*dir.y-Vector3.forward*dir.x).normalized),Is.LessThan(.1f));
                yield return Until(()=>!player.IsQuickRecovering);float length=Vector3.ProjectOnPlane(player.transform.position-start,Vector3.up).magnitude;
                Assert.That(length,Is.EqualTo(2.5f).Within(.025f));Assert.That(player.QuickRecoveryRequestedDistance,Is.EqualTo(2.5f).Within(.00001f));
                Assert.That(player.QuickRecoveryCooldownRemaining,Is.InRange(.95f,1));Assert.That(player.IsDowned||player.IsGettingUp,Is.False);
                Assert.That(player.VerticalVelocity,Is.LessThanOrEqualTo(0));rows.Add(new{length,observedDuration=Time.timeAsDouble-time,dir=new[]{direction.x,direction.y,direction.z}});
                frame=default;yield return null;
            }
            Record("4E-distances",rows);
        }
        [UnityTest] public IEnumerator SpaceAloneAndDirectionAloneDoNothingThenFreshPressEscapes()
        {
            yield return Down();yield return Ready();frame.jump=true;yield return Wait(.04f);Assert.That(player.QuickRecoveryCount,Is.Zero);
            frame.jump=false;frame.move=Vector2.right;yield return Wait(.04f);Assert.That(player.QuickRecoveryCount,Is.Zero);
            yield return Escape(Vector2.right);frame.jump=true;yield return Until(()=>!player.IsQuickRecovering);yield return Wait(.08f);
            Assert.That(player.QuickRecoveryCount,Is.EqualTo(1));Assert.That(player.VerticalVelocity,Is.LessThanOrEqualTo(0));
        }
        [UnityTest] public IEnumerator HighPitchLockAndMovingCameraKeepHorizontalSnapshot()
        {
            yield return Down();yield return Ready();Select(dummy);camera.transform.rotation=Quaternion.Euler(90,37,0);
            yield return Escape(new Vector2(1,1));var snapshot=player.QuickRecoveryDirection;Assert.That(snapshot.y,Is.Zero.Within(.00001f));
            var expected=(Vector3.Cross(Vector3.ProjectOnPlane(camera.transform.right,Vector3.up),Vector3.up).normalized+Vector3.ProjectOnPlane(camera.transform.right,Vector3.up).normalized).normalized;
            Assert.That(Vector3.Angle(snapshot,expected),Is.LessThan(.1f));camera.transform.rotation=Quaternion.Euler(-70,180,0);Place(dummy,new Vector3(-8,.03f,-4));
            yield return Until(()=>!player.IsQuickRecovering);Assert.That(player.QuickRecoveryDirection,Is.EqualTo(snapshot));Assert.That(player.QuickRecoveryRequestedDistance,Is.EqualTo(2.5f).Within(.00001f));
        }
        [UnityTest] public IEnumerator ShortEvadeProtectsOnlyOrdinaryStrikeAndGrabNoGuardEvents()
        {
            yield return Down();yield return Ready();yield return Escape(Vector2.right);float hp=player.Health.CurrentHealth;
            Assert.That(player.IsQuickRecoveryEvading,Is.False);yield return Until(()=>player.IsQuickRecoveryEvading);
            Assert.That(Hit(),Is.EqualTo(DamageResolution.Guarded));Assert.That(player.Health.CurrentHealth,Is.EqualTo(hp));
            Assert.That(player.GuardBlockCount+player.GuardBreakCount+player.ParrySuccessCount,Is.Zero);
            Place(dummy,player.transform.position+player.transform.forward*1.4f);dummy.SetGuardHeld(true);Assert.That(dummy.TryGrab(),Is.True);
            Assert.That((bool)typeof(ShibuyaFighter).GetMethod("ValidGrabTarget",Private).Invoke(dummy,new object[]{player,false}),Is.False);
            dummy.CancelGrabInput();yield return Until(()=>player.QuickRecoveryElapsed>=.19f);Assert.That(player.IsQuickRecoveryEvading,Is.False);
            Assert.That(Hit(),Is.EqualTo(DamageResolution.Applied));Assert.That(player.IsQuickRecovering,Is.False);Assert.That(player.Health.CurrentHealth,Is.EqualTo(hp-1));
        }
        [UnityTest] public IEnumerator BeforeInvulnerabilityAndEnvironmentalOrUnblockableDamageCancel()
        {
            foreach(int mode in new[]{0,1,2}){
                yield return Until(()=>player.QuickRecoveryCooldownRemaining<=0);yield return Down();yield return Ready();yield return Escape(Vector2.right);
                if(mode!=0)yield return Until(()=>player.IsQuickRecoveryEvading);
                float hp=player.Health.CurrentHealth;
                Assert.That(Hit(mode==1?DamageDeliveryType.Environmental:DamageDeliveryType.PhysicalStrike,mode==2?DamageTraits.Unblockable:DamageTraits.None),Is.EqualTo(DamageResolution.Applied));
                Assert.That(player.Health.CurrentHealth,Is.EqualTo(hp-1));Assert.That(player.IsQuickRecovering,Is.False);frame=default;
            }
        }
        [UnityTest] public IEnumerator AllActionsBlockedDuringEscapeThenNormalMovementAttackAndJump()
        {
            yield return Down();yield return Ready();yield return Escape(Vector2.right);frame.guardHeld=frame.attack=frame.auraHeld=frame.dash=true;
            Assert.That(player.TryAttack(),Is.False);Assert.That(player.TryGrab(),Is.False);Assert.That(player.TryGroundDash(Vector2.up,Vector3.forward,Vector3.right),Is.False);
            yield return Wait(.07f);Checkpoint("after action attempts");Assert.That(player.IsGuarding,Is.False);Assert.That(input.EnergyCharge.IsCharging,Is.False);Assert.That(input.Aura.Held,Is.False);
            frame=default;yield return Until(()=>!player.IsQuickRecovering);Checkpoint("after waiting for end");Assert.That(player.QuickRecoveryCooldownRemaining,Is.GreaterThan(0));
            Assert.That(player.TryAttack(),Is.True);yield return Until(()=>player.ComboStep==0);frame.jump=false;yield return null;frame.jump=true;
            yield return Until(()=>player.VerticalVelocity>0);Assert.That(player.QuickRecoveryCount,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator WallCollisionSpendsBlockedDistanceAndNeverTeleports()
        {
            yield return Down();yield return Ready();var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=player.transform.position+new Vector3(.9f,1,0);wall.transform.localScale=new Vector3(.2f,2,5);Physics.SyncTransforms();
            float x=player.transform.position.x;yield return Escape(Vector2.right);yield return Wait(.20f);float blocked=player.transform.position.x;
            Assert.That(blocked-x,Is.InRange(0,.61f));float requested=player.QuickRecoveryRequestedDistance;Object.Destroy(wall);
            yield return Until(()=>!player.IsQuickRecovering);float movedAfter=player.transform.position.x-blocked;
            Assert.That(movedAfter,Is.LessThanOrEqualTo(2.5f-requested+.04f));Record("4E-wall",new{blocked=blocked-x,requested,movedAfter});
        }
        [UnityTest] public IEnumerator AirborneDownCannotReserveEscapeBeforeRealLanding()
        {
            yield return Down(true);Assert.That(player.CanRequestDownRecovery,Is.False);frame.move=Vector2.right;frame.jump=true;
            yield return Ready();Assert.That(player.QuickRecoveryCount,Is.Zero);Assert.That(player.IsGrounded,Is.True);
            frame.jump=false;yield return Until(()=>!(bool)typeof(ShibuyaPlayerInput).GetField("previousJump",Private).GetValue(input));
            Checkpoint("Space up consumed before new press");yield return Escape(Vector2.right);Assert.That(player.QuickRecoveryCount,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator NaturalGetUpCannotBeCancelledAndFocusDisableEndCleanEscape()
        {
            yield return Down();yield return Until(()=>player.IsGettingUp);Assert.That(player.TryQuickRecovery(Vector2.right,Vector3.forward,Vector3.right),Is.False);
            yield return Until(()=>!player.IsDowned);yield return Down();yield return Ready();yield return Escape(Vector2.right);
            input.SendMessage("OnApplicationFocus",false);Assert.That(player.IsQuickRecovering,Is.False);Assert.That(player.IsQuickRecoveryEvading,Is.False);
            frame=default;yield return Until(()=>player.QuickRecoveryCooldownRemaining<=0);yield return Down();yield return Ready();yield return Escape(Vector2.right);
            player.enabled=false;Assert.That(player.IsQuickRecovering,Is.False);Assert.That(player.IsQuickRecoveryEvading,Is.False);player.enabled=true;
        }
        [UnityTest] public IEnumerator CooldownPreventsSecondDownEscapeButNotNormalJump()
        {
            yield return Down();yield return Ready();yield return Escape(Vector2.right);frame=default;yield return Until(()=>!player.IsQuickRecovering);
            Assert.That(player.QuickRecoveryCooldownRemaining,Is.GreaterThan(.9f));yield return Down();yield return Ready();
            Assert.That(player.QuickRecoveryCooldownRemaining,Is.GreaterThan(0));Assert.That(player.TryQuickRecovery(Vector2.right,Vector3.forward,Vector3.right),Is.False);
            yield return Until(()=>!player.IsDowned);frame.jump=true;yield return Until(()=>player.VerticalVelocity>0);
        }
        [UnityTest] public IEnumerator ActualStrikeAndDownInterruptEscapeOutsideEvadeWindow()
        {
            yield return Down();yield return Ready();yield return Escape(Vector2.right);yield return Until(()=>player.QuickRecoveryElapsed>=.19f);
            // A real authored attack goes through overlap -> Health -> reaction -> Knockdown.
            Place(dummy,player.transform.position+Vector3.forward*1.4f);dummy.transform.rotation=Quaternion.Euler(0,180,0);
            Assert.That(dummy.TryAttack(),Is.True);yield return Until(()=>player.IsDowned);Assert.That(player.IsQuickRecovering,Is.False);
        }
        [UnityTest] public IEnumerator CharacterCollisionAndGroundCrackStayIndependent()
        {
            yield return Down();yield return Ready();var crack=player.GetComponent<ShibuyaGroundImpact>();int count=crack.PlayCount;
            Place(dummy,player.transform.position+Vector3.right*1.2f);Vector3 start=player.transform.position;
            yield return Escape(Vector2.right);yield return Until(()=>!player.IsQuickRecovering);
            Assert.That(player.transform.position.x-start.x,Is.LessThan(.7f));
            float gap=Vector3.ProjectOnPlane(player.transform.position-dummy.transform.position,Vector3.up).magnitude;
            Assert.That(gap,Is.GreaterThanOrEqualTo(.6f));Assert.That(player.QuickRecoveryRequestedDistance,Is.EqualTo(2.5f).Within(.00001f));Assert.That(crack.PlayCount,Is.EqualTo(count));
        }
        [UnityTest] public IEnumerator LeavingPlatformCancelsMotionAndFallsWithoutGroundSnap()
        {
            var slab=GameObject.CreatePrimitive(PrimitiveType.Cube);slab.transform.position=new Vector3(0,1.85f,-8);slab.transform.localScale=new Vector3(1.5f,.3f,1.5f);Physics.SyncTransforms();
            yield return Down(true);yield return Ready();Assert.That(player.transform.position.y,Is.GreaterThan(1.9f));
            yield return Escape(Vector2.right);var falling=new System.Collections.Generic.List<object>();
            while(player.IsQuickRecovering){
                float oldY=player.transform.position.y,oldVelocity=player.VerticalVelocity;double oldTime=Time.timeAsDouble;
                yield return null;float dt=(float)(Time.timeAsDouble-oldTime);float drop=oldY-player.transform.position.y;
                var cc=player.GetComponent<CharacterController>();float budget=Mathf.Abs(oldVelocity)*dt+combat.gravity*dt*dt+cc.stepOffset+cc.skinWidth;
                Assert.That(drop,Is.LessThanOrEqualTo(budget),"A fall must be explained by gravity and the existing controller's step/skin allowance");
                falling.Add(new{time=Time.timeAsDouble,dt,y=player.transform.position.y,velocity=player.VerticalVelocity,drop,budget});
            }
            float height=player.transform.position.y;Assert.That(player.IsGrounded,Is.False);Assert.That(player.IsQuickRecoveryEvading,Is.False);
            yield return Wait(.8f);Assert.That(player.transform.position.y,Is.LessThan(.1f));Object.Destroy(slab);
            Record("4E-platform-gravity",new{heightAtMotionEnd=height,landedHeight=player.transform.position.y,falling});
        }
        [UnityTest] public IEnumerator ActualOrdinaryAttackDuringEvadeHasNoHitFeedback()
        {
            recovery.distance=.05f;yield return Down();yield return Ready();yield return Escape(Vector2.right);yield return Until(()=>player.IsQuickRecoveryEvading);
            Place(dummy,player.transform.position+Vector3.forward*1.4f);dummy.transform.rotation=Quaternion.Euler(0,180,0);
            var m=ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();m.name="PASS4B3 QA recovery actual strike";
            var s=JsonUtility.FromJson<ShibuyaStrikeDefinition>(JsonUtility.ToJson(combat.strikes[0]));s.startup=.01f;s.activeTime=.01f;s.recovery=.01f;s.advanceDistance=0;s.hitStopDuration=.067f;
            m.steps=new[]{new ShibuyaMovesetStep{hits=new[]{new ShibuyaHitEvent{strike=s}}}};Assert.That(dummy.SetMoveset(m),Is.True);
            float hp=player.Health.CurrentHealth;int actions=dummy.AcceptedAttackCount;Assert.That(dummy.TryAttack(),Is.True);yield return Until(()=>dummy.ComboStep==0);
            Assert.That(dummy.AcceptedAttackCount,Is.EqualTo(actions+1));Assert.That(dummy.ConfirmedHitCount,Is.EqualTo(1)); // Initial real knockdown only.
            Assert.That(player.Health.CurrentHealth,Is.EqualTo(hp));Assert.That(dummy.HitStopCount,Is.Zero);Assert.That(player.GuardBlockCount+player.ParrySuccessCount,Is.Zero);
        }
        [UnityTest] public IEnumerator RealGrabWindowIsEvadedAndOutsideWindowCanCapture()
        {
            // Isolate the immunity rule from the separately validated 2.5m range escape.
            recovery.distance=.05f;
            foreach(bool inside in new[]{true,false}){
                yield return Until(()=>!dummy.IsGrabBusy&&player.QuickRecoveryCooldownRemaining<=0);yield return Down();yield return Ready();
                Place(dummy,player.transform.position+Vector3.forward*1.4f);dummy.transform.rotation=Quaternion.Euler(0,180,0);
                dummy.SetGuardHeld(false);dummy.SetGuardHeld(true);Assert.That(dummy.TryGrab(),Is.True);
                if(inside)yield return Wait(.10f);
                float hp=player.Health.CurrentHealth;int successes=dummy.GrabSuccessCount;
                yield return Escape(Vector2.right);yield return Until(()=>!dummy.IsGrabBusy);
                Assert.That(dummy.GrabSuccessCount,Is.EqualTo(successes+(inside?0:1)));Assert.That(player.Health.CurrentHealth,Is.EqualTo(hp-(inside?0:15)));
                if(!inside)Assert.That(player.IsQuickRecovering,Is.False);
                frame=default;dummy.SetGuardHeld(false);yield return Until(()=>player.State!=ShibuyaFighterState.Hitstunned);
            }
        }
        [UnityTest] public IEnumerator CinematicAndDeathEndRecoveryOwnership()
        {
            yield return Down();yield return Ready();yield return Escape(Vector2.right);
            Place(dummy,player.transform.position+Vector3.forward*2);
            var p=Clone(AssetDatabase.LoadAssetAtPath<ShibuyaCinematicProfile>(ShibuyaSceneBuilder.Root+"/Data/Shibuya Cinematic Example.asset"));p.cinematicEnabled=true;
            var result=dummy.Health.ReceiveDamage(new DamageContext(1,player.gameObject,DamageDeliveryType.PhysicalStrike));
            Assert.That(ShibuyaCinematicSession.TryStartAfterApplied(player,dummy,p,result,ShibuyaCinematicTrigger.Default,100,0),Is.Not.Null);
            Assert.That(player.IsQuickRecovering||player.IsQuickRecoveryEvading,Is.False);yield return Until(()=>!player.IsInCinematic);
            frame=default;yield return Until(()=>player.QuickRecoveryCooldownRemaining<=0);yield return Down();yield return Ready();yield return Escape(Vector2.right);
            player.Health.Kill();yield return null;Assert.That(player.IsQuickRecovering||player.IsQuickRecoveryEvading||player.IsDowned,Is.False);
        }
        [UnityTest] public IEnumerator DevelopmentToolUsesActualKnockdownAndRestoresMoveset()
        {
            var original=dummy.Moveset;var w=ScriptableObject.CreateInstance<ShibuyaQuickRecoveryTestWindow>();var flags=BindingFlags.Static|BindingFlags.NonPublic;
            try{
                typeof(ShibuyaQuickRecoveryTestWindow).GetMethod("Schedule",flags).Invoke(null,new object[]{0});
                yield return Until(()=>player.IsDowned,4);Assert.That(player.Health.CurrentHealth,Is.EqualTo(99));yield return Ready();yield return Escape(Vector2.right);
                yield return Until(()=>!player.IsQuickRecovering);Assert.That(dummy.Moveset,Is.SameAs(original));
            }finally{Object.DestroyImmediate(w);}
        }
        [UnityTest] public IEnumerator OriginalSpeedEarlyHoldNaturalAndDirectionalRecoveryEvidence()
        {
            foreach(string mode in new[]{"directional","natural","early-hold"}){
                yield return Until(()=>player.QuickRecoveryCooldownRemaining<=0);yield return Down();
                camera.transform.position=player.transform.position+new Vector3(4.5f,2.6f,4.5f);camera.transform.LookAt(player.transform.position+Vector3.up);
                var rows=new System.Collections.Generic.List<object>();double start=Time.timeAsDouble;int index=0;bool pressed=false;
                while(Time.timeAsDouble-start<1.35){
                    if(!pressed&&((mode=="directional"&&player.CanRequestDownRecovery)||mode=="early-hold")){
                        frame.move=Vector2.right;frame.jump=true;pressed=true;
                    }
                    if(player.IsQuickRecovering)frame.move=Vector2.zero;
                    var pos=player.transform.position;Capture("4E-media/"+mode+"/frame-"+index.ToString("D4"));
                    rows.Add(new{index,time=Time.timeAsDouble-start,x=pos.x,y=pos.y,z=pos.z,down=player.IsDowned,gettingUp=player.IsGettingUp,recovery=player.IsQuickRecovering,evade=player.IsQuickRecoveryEvading,count=player.QuickRecoveryCount});
                    index++;yield return Wait(1f/40);
                }
                Record("4E-media/"+mode+"-timestamps",rows);frame=default;yield return Wait(.04f);
            }
            Assert.That(player.QuickRecoveryCount,Is.EqualTo(1));
        }
    }
}
