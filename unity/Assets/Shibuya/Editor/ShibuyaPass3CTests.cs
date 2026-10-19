using System;
using System.Collections;
using System.Collections.Generic;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass3CTests
    {
        private static ShibuyaMatch Match=>Object.FindFirstObjectByType<ShibuyaMatch>();
        private static ShibuyaFighter P=>Match.Player;
        private static ShibuyaFighter D=>Match.Dummy;
        [Test] public void DataKoreanUndoAndProtectedGroundValues() {
            var p=AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root+"/Data/Shibuya Combat Profile.asset");
            Assert.That(p.aerialCombat!=null&&p.aerialCombat.IsValid);
            foreach(var field in typeof(ShibuyaAerialCombatDefinition).GetFields(System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance))
                Assert.That(ShibuyaInspector.KoreanLabel(field.Name),Is.Not.EqualTo(field.Name));
            CollectionAssert.AreEqual(new[]{20f,20f,20f,25f},Array.ConvertAll(p.strikes,s=>s.damage));
            Assert.That(p.finishers.uppercut.hits[0].air.peakHeight,Is.EqualTo(5.5f));
            Assert.That(p.finishers.uppercut.hits[0].air.apexHoldTime,Is.EqualTo(.12f));
            Assert.That(p.strikes[3].hitStopDuration,Is.EqualTo(.065f));
            var data=Object.Instantiate(p.aerialCombat);data.name="PASS3C QA Air";
            var so=new SerializedObject(data);var speed=so.FindProperty("chaseSpeed");float before=speed.floatValue;
            Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();speed.floatValue=17;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();
            Assert.That(data.chaseSpeed,Is.EqualTo(17));Undo.CollapseUndoOperations(group);Undo.PerformUndo();Assert.That(data.chaseSpeed,Is.EqualTo(before));
            data.aerialAttack.hits[0].outcome=ShibuyaHitOutcome.Launch;Assert.That(data.IsValid,Is.False,"no air relaunch loop data");
        }
        [UnityTest] public IEnumerator UppercutSpaceArrivesInRealThreeDimensionsWithoutOverlap() {Open();yield return new EnterPlayMode();yield return ChaseArrival();yield return new ExitPlayMode();}
        private IEnumerator ChaseArrival() {
            yield return Setup();yield return Upper();Vector3 start=P.transform.position;float hp=D.Health.CurrentHealth;
            PressSpace();Assert.That(P.HasBufferedAirChase);yield return Arrive();
            Assert.That(P.transform.position.y-start.y,Is.GreaterThan(2));Assert.That(P.IsAirborne,Is.False,"pursuer is not an action-locked victim");
            Vector3 difference=D.transform.position-P.transform.position;float height=Mathf.Abs(difference.y);difference.y=0;
            float minimum=P.GetComponent<CharacterController>().radius+D.GetComponent<CharacterController>().radius;
            Assert.That(difference.magnitude,Is.GreaterThanOrEqualTo(minimum));Assert.That(height,Is.LessThanOrEqualTo(P.AerialDefinition.chaseVerticalTolerance+.02f));
            Assert.That(D.Health.CurrentHealth,Is.EqualTo(hp));Assert.That(P.AirChaseCount,Is.EqualTo(1));
            Vector3 view=Camera.main.WorldToViewportPoint(D.transform.position+Vector3.up);
            Assert.That(view.z,Is.GreaterThan(0));Assert.That(view.x,Is.InRange(0,1));Assert.That(view.y,Is.InRange(0,1));
            Record("arrival",$"source height={P.transform.position.y:F3}, target height={D.transform.position.y:F3}, gap={difference.magnitude:F3}, travel={P.AirChaseTravel:F3}");
        }
        [UnityTest] public IEnumerator NoSpaceNeverAutoChases() {Open();yield return new EnterPlayMode();yield return NoSpace();yield return new ExitPlayMode();}
        private IEnumerator NoSpace() {yield return Setup();yield return Upper();yield return Wait(.35f);Assert.That(P.AirChaseCount,Is.Zero);Assert.That(P.transform.position.y,Is.LessThan(.2f));Assert.That(D.Health.CurrentHealth,Is.EqualTo(915));}
        [UnityTest] public IEnumerator NormalAndDownFinishersDoNotOfferChase() {Open();yield return new EnterPlayMode();yield return NoOffer();yield return new ExitPlayMode();}
        private IEnumerator NoOffer() {
            yield return Setup();yield return Combo(ShibuyaFinisher.Default);Assert.That(P.HasAirChaseOpportunity,Is.False);PressSpace();yield return Wait(.1f);Assert.That(P.AirChaseCount,Is.Zero);
            yield return Until(()=>P.State==ShibuyaFighterState.Ready&&D.State==ShibuyaFighterState.Ready);P.ApplyHitReaction(Vector3.zero,0);ResetPositions();
            yield return Combo(ShibuyaFinisher.Down);Assert.That(P.HasAirChaseOpportunity,Is.False);PressSpace();yield return Wait(.1f);Assert.That(P.AirChaseCount,Is.Zero);
        }
        [UnityTest] public IEnumerator MissedUppercutAndPreHitSpaceDoNotCreateChase() {Open();yield return new EnterPlayMode();yield return Miss();yield return new ExitPlayMode();}
        private IEnumerator Miss() {
            yield return Setup();Place(D,new Vector3(20,.03f,0));
            Assert.That(P.TryAttack());PressSpace();yield return Until(()=>P.AttackElapsed>=P.AttackDuration-.1f);
            for(int i=1;i<=3;i++) {Assert.That(P.TryAttack(ShibuyaFinisher.Uppercut));yield return Until(()=>P.ComboStep==i+1);if(i<3)yield return Until(()=>P.AttackElapsed>=P.AttackDuration-.1f);}
            yield return Wait(.3f);Assert.That(P.HasAirChaseOpportunity,Is.False);Assert.That(P.AirChaseCount,Is.Zero);
        }
        [UnityTest] public IEnumerator InvulnerableUppercutNeverOffersChase() {Open();yield return new EnterPlayMode();yield return InvulnerableUpper();yield return new ExitPlayMode();}
        private IEnumerator InvulnerableUpper() {
            yield return Setup();D.Health.GrantInvulnerability(3);yield return Combo(ShibuyaFinisher.Uppercut,false);
            Assert.That(!D.IsAirborne&&!P.HasAirChaseOpportunity);PressSpace();yield return Wait(.2f);Assert.That(P.AirChaseCount,Is.Zero);
        }
        [UnityTest] public IEnumerator SpaceBeforeValidContactIsNotADeferredChase() {Open();yield return new EnterPlayMode();yield return Early();yield return new ExitPlayMode();}
        private IEnumerator Early() {
            yield return Setup();Assert.That(P.TryAttack());
            for(int i=1;i<=3;i++) {yield return Until(()=>P.ComboStep==i&&P.AttackElapsed>=P.AttackDuration-.12f);Assert.That(P.TryAttack(ShibuyaFinisher.Uppercut));yield return Until(()=>P.ComboStep==i+1);}
            PressSpace();yield return Until(()=>D.IsAirborne);yield return Wait(.2f);Assert.That(P.AirChaseCount,Is.Zero);Assert.That(P.HasAirChaseOpportunity);
        }
        [UnityTest] public IEnumerator OrdinaryJumpAndHeldSpaceRemainOneJump() {Open();yield return new EnterPlayMode();yield return Jump();yield return new ExitPlayMode();}
        private IEnumerator Jump() {
            yield return Setup();var frame=new ShibuyaInputFrame {jump=true};P.GetComponent<ShibuyaPlayerInput>().TestInput=()=>frame;
            float peak=0,end=Time.time+1.1f;while(Time.time<end){peak=Mathf.Max(peak,P.transform.position.y);yield return null;}
            Assert.That(peak,Is.GreaterThan(1));Assert.That(P.IsGrounded);Assert.That(P.AirChaseCount,Is.Zero);Assert.That(P.transform.position.y,Is.LessThan(.2f));
        }
        [UnityTest] public IEnumerator HitStopBuffersNativeSpaceAndChaseWinsOverJump() {Open();yield return new EnterPlayMode();yield return NativeBuffer();yield return new ExitPlayMode();}
        private IEnumerator NativeBuffer() {
            yield return Setup();yield return Upper();Assert.That(P.IsInHitStop);
            var frame=new ShibuyaInputFrame {jump=true};P.GetComponent<ShibuyaPlayerInput>().TestInput=()=>frame;
            yield return null;Assert.That(P.HasBufferedAirChase);float clock=P.CombatTime;yield return null;
            Assert.That(P.CombatTime,Is.EqualTo(clock).Within(.001f));frame.jump=false;
            yield return Arrive();Assert.That(P.transform.position.y,Is.GreaterThan(2));Assert.That(P.AirChaseCount,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator OpportunityExpiresAndSpaceReturnsToOrdinaryJump() {Open();yield return new EnterPlayMode();yield return Expiry();yield return new ExitPlayMode();}
        private IEnumerator Expiry() {
            yield return Setup();P.AerialDefinition.chaseInputWindow=.12f;yield return Upper();yield return Wait(.7f);
            Assert.That(P.HasAirChaseOpportunity,Is.False);PressSpace();yield return Wait(.15f);
            Assert.That(P.AirChaseCount,Is.Zero);Assert.That(P.transform.position.y,Is.GreaterThan(.4f));
        }
        [UnityTest] public IEnumerator ChaseWithoutLmbAddsNoDamageAndReturnsGroundControl() {Open();yield return new EnterPlayMode();yield return NoAirAttack();yield return new ExitPlayMode();}
        private IEnumerator NoAirAttack() {
            yield return Setup();yield return Upper();PressSpace();yield return Arrive();float hp=D.Health.CurrentHealth;
            yield return Until(()=>!P.IsInAirSession&&P.IsGrounded);Assert.That(D.Health.CurrentHealth,Is.EqualTo(hp));
            Vector3 before=P.transform.position;P.GetComponent<ShibuyaPlayerInput>().TestInput=()=>new ShibuyaInputFrame {move=Vector2.right};yield return Wait(.2f);
            Assert.That(Vector3.Distance(before,P.transform.position),Is.GreaterThan(.5f));PressSpace();yield return Wait(.1f);Assert.That(P.VerticalVelocity,Is.GreaterThan(0));
        }
        [UnityTest] public IEnumerator OneNativeLmbRunsIndependentAirHitEventsWithoutDuplicates() {Open();yield return new EnterPlayMode();yield return AirHits();yield return new ExitPlayMode();}
        private IEnumerator AirHits() {
            yield return Setup();yield return Upper();PressSpace();yield return Arrive();D.gameObject.AddComponent<BoxCollider>().size=Vector3.one*.4f;
            var order=new List<int>();P.HitEventStarted+=i=>order.Add(i);int hits=P.ConfirmedHitCount;
            var frame=new ShibuyaInputFrame {attack=true};P.GetComponent<ShibuyaPlayerInput>().TestInput=()=>frame;yield return null;
            Assert.That(P.IsAirAttacking);yield return Until(()=>P.ConfirmedHitCount==hits+2);yield return Until(()=>!P.IsAirAttacking);
            CollectionAssert.AreEqual(new[]{0,1},order);Assert.That(D.Health.CurrentHealth,Is.EqualTo(890));
            yield return Wait(.2f);Assert.That(P.ConfirmedHitCount,Is.EqualTo(6));Assert.That(Time.timeScale,Is.EqualTo(1));
            Record("air hits","one held LMB -> two events, 12+13 damage, multi-collider dedup, no repeat");
        }
        [UnityTest] public IEnumerator AirMissOutsideRangeAndHeightCannotDealRemoteDamage() {Open();yield return new EnterPlayMode();yield return AirMiss();yield return new ExitPlayMode();}
        private IEnumerator AirMiss() {
            yield return Setup();yield return Upper();PressSpace();yield return Arrive();Assert.That(P.TryAttack());
            Place(D,D.transform.position+new Vector3(3,2,0));yield return Wait(.5f);
            Assert.That(D.Health.CurrentHealth,Is.EqualTo(915));Assert.That(P.ConfirmedHitCount,Is.EqualTo(4));
        }
        [UnityTest] public IEnumerator WallBeforeSpaceRejectsPathWithoutTeleport() {Open();yield return new EnterPlayMode();yield return BeforeWall();yield return new ExitPlayMode();}
        private IEnumerator BeforeWall() {
            yield return Setup();yield return Upper();yield return Wait(.18f);
            Cube(P.transform.position+new Vector3(0,1.4f,.45f),new Vector3(5,5,.12f));Vector3 before=P.transform.position;
            var destination=(Vector3)typeof(ShibuyaFighter).GetMethod("ChaseDestination",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(P,null);
            Record("wall-before-Space geometry",$"source={P.transform.position:F4}, target={D.transform.position:F4}, destination={destination:F4}, wallCenter={P.transform.position+new Vector3(0,1.4f,.45f):F4}, sourceRadius={P.GetComponent<CharacterController>().radius:F3}");
            PressSpace();Assert.That(P.HasBufferedAirChase,Is.False);yield return Wait(.2f);
            Assert.That(P.AirChaseCount,Is.Zero);Assert.That(P.transform.position.z-before.z,Is.LessThan(.2f));
        }
        [UnityTest] public IEnumerator CeilingDuringChaseStopsAndFallsSafely() {Open();yield return new EnterPlayMode();yield return Obstacle(true);yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator WallDuringChaseStopsAndFallsSafely() {Open();yield return new EnterPlayMode();yield return Obstacle(false);yield return new ExitPlayMode();}
        private IEnumerator Obstacle(bool ceiling) {
            yield return Setup();yield return Upper();PressSpace();yield return Until(()=>P.IsChasing);
            Vector3 at=P.transform.position;
            if(ceiling) Cube(at+new Vector3(0,2.1f,0),new Vector3(8,.12f,8));else Cube(at+new Vector3(0,2,.6f),new Vector3(8,8,.12f));
            yield return Until(()=>!P.IsChasing);Assert.That(P.CanAirAttack,Is.False);
            if(ceiling) Assert.That(P.transform.position.y,Is.LessThan(at.y+.4f));else Assert.That(P.transform.position.z,Is.LessThan(at.z+.35f));
            yield return Until(()=>!P.IsInAirSession);Assert.That(P.IsGrounded);
        }
        [UnityTest] public IEnumerator ChaseDurationAndTravelCapsTerminateWithoutStoredBurst() {Open();yield return new EnterPlayMode();yield return Caps();yield return new ExitPlayMode();}
        private IEnumerator Caps() {
            yield return Setup();P.AerialDefinition.chaseMaxDuration=.02f;yield return Upper();PressSpace();yield return Until(()=>P.AirChaseCount==1);yield return Until(()=>!P.IsChasing);
            Assert.That(!P.CanAirAttack&&P.AirChaseTarget==null);yield return Until(()=>!P.IsInAirSession);
            Record("duration cap",$"first chase count={P.AirChaseCount}, target still airborne={D.IsAirborne}, travel={P.AirChaseTravel:F3}");
            yield return Until(()=>D.IsGrounded&&D.State==ShibuyaFighterState.Ready);
            P.AerialDefinition.chaseMaxDuration=.8f;P.AerialDefinition.chaseMaxTravel=.05f;P.ApplyHitReaction(Vector3.zero,0);ResetPositions();
            yield return Upper();PressSpace();yield return Until(()=>P.AirChaseCount==2);yield return Until(()=>!P.IsChasing);
            Assert.That(P.AirChaseTravel,Is.LessThanOrEqualTo(.0501f));yield return Until(()=>!P.IsInAirSession);
        }
        [UnityTest] public IEnumerator EscapingTargetRangeEndsChaseWithoutRetargeting() {Open();yield return new EnterPlayMode();yield return InvalidTarget(0);yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator TargetImmunityEndsChaseAndReleasesReference() {Open();yield return new EnterPlayMode();yield return InvalidTarget(1);yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator DisabledTargetEndsChaseAndReleasesReference() {Open();yield return new EnterPlayMode();yield return InvalidTarget(2);yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator ThirdPartyHitInvalidatesTheOriginalLaunchContext() {Open();yield return new EnterPlayMode();yield return InvalidTarget(3);yield return new ExitPlayMode();}
        private IEnumerator InvalidTarget(int mode) {
            yield return Setup();yield return Upper();PressSpace();yield return Until(()=>P.IsChasing);
            if(mode==0)Place(D,P.transform.position+new Vector3(15,5,0));
            if(mode==1)D.Health.GrantInvulnerability(.5f);
            if(mode==2)D.gameObject.SetActive(false);
            if(mode==3)D.Health.ReceiveDamage(new DamageContext(1,new GameObject("Third Party"),DamageDeliveryType.PhysicalStrike,DamageTraits.None,"QA INTERFERENCE",D.transform.position));
            yield return null;yield return null;Assert.That(!P.IsChasing&&P.AirChaseTarget==null&&!P.CanAirAttack);
            yield return Until(()=>!P.IsInAirSession);Assert.That(Time.timeScale,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator DeathAndBattleEndReleaseAllAerialState() {Open();yield return new EnterPlayMode();yield return Death();yield return new ExitPlayMode();}
        private IEnumerator Death() {yield return Setup();yield return Upper();PressSpace();yield return Until(()=>P.IsChasing);D.Health.Kill();yield return null;Assert.That(Match.Finished);Assert.That(!P.IsInAirSession&&P.AirChaseTarget==null&&!P.HasBufferedAirChase);}
        [UnityTest] public IEnumerator AttackerHitInterruptsChaseAndStillLands() {Open();yield return new EnterPlayMode();yield return Interrupted();yield return new ExitPlayMode();}
        private IEnumerator Interrupted() {
            yield return Setup();yield return Upper();PressSpace();yield return Until(()=>P.IsChasing);
            P.ApplyHitReaction(Vector3.back*2,.2f);yield return null;Assert.That(!P.IsChasing&&P.AirChaseTarget==null);
            yield return Until(()=>!P.IsInAirSession&&P.IsGrounded);
            if(P.State==ShibuyaFighterState.Hitstunned) Assert.That(P.TryAttack(),Is.False,"landing must not bypass remaining hit stun");
            yield return Until(()=>P.State==ShibuyaFighterState.Ready);Assert.That(P.TryAttack());
        }
        [UnityTest] public IEnumerator OneLaunchCannotRepeatChaseOrAirAttack() {Open();yield return new EnterPlayMode();yield return Once();yield return new ExitPlayMode();}
        private IEnumerator Once() {
            yield return Setup();
            // Preserve the original single-input policy when continuation is explicitly disabled.
            // Default bounded continuation is covered separately by the hover tests.
            P.AerialDefinition.aerialLinkInputWindow=0;
            yield return Upper();PressSpace();yield return Arrive();PressSpace();Assert.That(P.TryAttack());Assert.That(P.TryAttack(),Is.False);
            yield return Until(()=>!P.IsAirAttacking);Assert.That(P.TryAttack(),Is.False);PressSpace();yield return Until(()=>!P.IsInAirSession);
            Assert.That(P.AirChaseCount,Is.EqualTo(1));Assert.That(!P.HasAirChaseOpportunity&&!P.HasBufferedAirChase);
        }
        [UnityTest] public IEnumerator CharacterMovesetCanOwnItsAirDataWithoutChangingDefaults() {Open();yield return new EnterPlayMode();yield return CharacterData();yield return new ExitPlayMode();}
        private IEnumerator CharacterData() {
            yield return Setup();var data=ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();data.name="PASS3C QA Moveset";
            data.finishers=P.Profile.finishers;data.aerialCombat=Object.Instantiate(P.AerialDefinition);data.aerialCombat.name="PASS3C QA Air";
            data.aerialCombat.aerialAttack.hits=new[]{data.aerialCombat.aerialAttack.hits[0]};data.aerialCombat.aerialAttack.hits[0].strike.damage=17;
            data.steps=new ShibuyaMovesetStep[4];
            for(int i=0;i<4;i++) {var s=JsonUtility.FromJson<ShibuyaStrikeDefinition>(JsonUtility.ToJson(P.Profile.strikes[i]));
                data.steps[i]=new ShibuyaMovesetStep {hits=new[]{new ShibuyaHitEvent {strike=s}},movements=new[]{new ShibuyaMovementDefinition {
                    kind=ShibuyaMovementKind.Forward,startTime=s.advanceStartTime,duration=s.advanceDuration,maxDistance=s.advanceDistance,speed=s.advanceSpeed}}};}
            Assert.That(P.SetMoveset(data));yield return Upper();PressSpace();yield return Arrive();Assert.That(P.TryAttack());yield return Until(()=>P.ConfirmedHitCount==5);
            Assert.That(D.Health.CurrentHealth,Is.EqualTo(898));Assert.That(P.Profile.aerialCombat.aerialAttack.hits.Length,Is.EqualTo(2));
            Assert.That(P.Profile.aerialCombat.aerialAttack.hits[0].strike.damage,Is.EqualTo(12));
        }
        [UnityTest] public IEnumerator AirHitStopFreezesOnlyParticipantsAndPreservesEventOrder() {Open();yield return new EnterPlayMode();yield return AirStop();yield return new ExitPlayMode();}
        private IEnumerator AirStop() {
            yield return Setup();yield return Upper();PressSpace();yield return Arrive();
            var other=Object.Instantiate(D.gameObject).GetComponent<ShibuyaFighter>();Place(other,new Vector3(15,.03f,15));other.SetIntent(Vector3.right,false,false);
            Assert.That(P.TryAttack());yield return Until(()=>P.IsInHitStop);Vector3 a=P.transform.position,b=D.transform.position,c=other.transform.position;float clock=P.CombatTime;
            yield return null;Assert.That(P.CombatTime,Is.EqualTo(clock).Within(.001f));Assert.That(Vector3.Distance(P.transform.position,a),Is.LessThan(.001f));Assert.That(Vector3.Distance(D.transform.position,b),Is.LessThan(.001f));
            Assert.That(Vector3.Distance(other.transform.position,c),Is.GreaterThan(.01f));Assert.That(Time.timeScale,Is.EqualTo(1));
            yield return Until(()=>P.ConfirmedHitCount==6);Record("air hitstop","source/target frozen locally, third actor moves, timeScale=1, both events resolve");
        }
        [UnityTest] public IEnumerator AirEventTimeSkipStillExecutesOrderedHitsExactlyOnce() {Open();yield return new EnterPlayMode();yield return TimeSkip();yield return new ExitPlayMode();}
        private IEnumerator TimeSkip() {
            yield return Setup();foreach(var hit in P.AerialDefinition.aerialAttack.hits)hit.strike.hitStopDuration=0;
            yield return Upper();PressSpace();yield return Arrive();var order=new List<int>();P.HitEventStarted+=i=>order.Add(i);
            Assert.That(P.TryAttack());
            var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            typeof(ShibuyaFighter).GetField("attackStartedAt",flags).SetValue(P,P.CombatTime-.35f);
            typeof(ShibuyaFighter).GetMethod("UpdateAttack",flags).Invoke(P,null);
            CollectionAssert.AreEqual(new[]{0,1},order);Assert.That(D.Health.CurrentHealth,Is.EqualTo(890));
            yield return Wait(.15f);Assert.That(P.ConfirmedHitCount,Is.EqualTo(6));
            Record("air event time skip","actual UpdateAttack across .35s: ordered 0,1, one damage per event; not a sustained render FPS benchmark");
        }
        [UnityTest] public IEnumerator LateAirLmbCannotStartAfterItsDataWindow() {Open();yield return new EnterPlayMode();yield return LateAir();yield return new ExitPlayMode();}
        private IEnumerator LateAir() {
            yield return Setup();P.AerialDefinition.aerialInputWindow=.04f;yield return Upper();PressSpace();yield return Arrive();
            yield return Wait(.08f);Assert.That(P.TryAttack(),Is.False);Assert.That(D.Health.CurrentHealth,Is.EqualTo(915));
            yield return Until(()=>!P.IsInAirSession&&P.IsGrounded);
        }
        [UnityTest] public IEnumerator ThirdCapsuleObstructionDoesNotAllowPenetrationOrInfiniteChase() {Open();yield return new EnterPlayMode();yield return ThirdCapsule();yield return new ExitPlayMode();}
        private IEnumerator ThirdCapsule() {
            yield return Setup();yield return Upper();PressSpace();yield return Until(()=>P.IsChasing);
            var other=Object.Instantiate(D.gameObject).GetComponent<ShibuyaFighter>();
            Place(other,P.transform.position+new Vector3(0,2.4f,.6f));other.enabled=false;
            var a=P.GetComponent<CharacterController>();var b=other.GetComponent<CharacterController>();
            float minimum=float.PositiveInfinity,end=Time.realtimeSinceStartup+5;
            while(P.IsInAirSession&&Time.realtimeSinceStartup<end) {
                Vector3 flat=a.bounds.center-b.bounds.center;flat.y=0;
                float halfA=a.height*.5f-a.radius,halfB=b.height*.5f-b.radius;
                float vertical=Mathf.Max(0,Mathf.Abs(a.bounds.center.y-b.bounds.center.y)-halfA-halfB);
                float gap=Mathf.Sqrt(flat.sqrMagnitude+vertical*vertical);minimum=Mathf.Min(minimum,gap);
                Assert.That(gap,Is.GreaterThanOrEqualTo(a.radius+b.radius-.08f),"capsule centerline clearance including controller skin");
                yield return null;
            }
            Assert.That(P.IsInAirSession,Is.False);Assert.That(P.IsGrounded);Assert.That(D.Health.CurrentHealth,Is.EqualTo(915));
            Record("third capsule",$"minimum centerline gap={minimum:F3}m; finite chase and ground return");
        }
        private static IEnumerator Setup() {
            Application.runInBackground=true;yield return Wait(.1f);P.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default;
            var profile=Object.Instantiate(P.Profile);profile.name="PASS3C QA Profile";
            profile.aerialCombat=Object.Instantiate(profile.aerialCombat);profile.aerialCombat.name="PASS3C QA Air";
            P.Configure(profile,AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root+"/Data/Player Stats.asset"));ResetPositions();
        }
        private static IEnumerator Combo(ShibuyaFinisher branch,bool expectHit=true) {
            int hits=P.ConfirmedHitCount;Assert.That(P.TryAttack());
            for(int i=1;i<=3;i++) {yield return Until(()=>P.ComboStep==i&&P.AttackElapsed>=P.AttackDuration-.12f);Assert.That(P.TryAttack(branch));yield return Until(()=>P.ComboStep==i+1);}
            if(expectHit)yield return Until(()=>P.ConfirmedHitCount==hits+4);else yield return Wait(.25f);
        }
        private static IEnumerator Upper() {yield return Combo(ShibuyaFinisher.Uppercut);Assert.That(D.IsAirborne);Assert.That(P.HasAirChaseOpportunity);}
        private static IEnumerator Arrive() {yield return Until(()=>P.CanAirAttack);Assert.That(!P.IsChasing&&P.IsInAirSession);}
        private static void PressSpace()=>P.SetIntent(Vector3.zero,false,true);
        private static void Open()=>EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);
        private static void ResetPositions(){Place(P,new Vector3(0,.03f,-1.4f));Place(D,new Vector3(0,.03f,0));P.transform.rotation=Quaternion.identity;}
        private static void Place(ShibuyaFighter f,Vector3 pos){var cc=f.GetComponent<CharacterController>();cc.enabled=false;f.transform.position=pos;cc.enabled=true;Physics.SyncTransforms();}
        private static void Cube(Vector3 pos,Vector3 scale){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.transform.position=pos;go.transform.localScale=scale;Physics.SyncTransforms();}
        private static IEnumerator Wait(float seconds){float end=Time.time+seconds;while(Time.time<end)yield return null;}
        private static IEnumerator Until(Func<bool> predicate){float end=Time.realtimeSinceStartup+6;while(!predicate()&&Time.realtimeSinceStartup<end)yield return null;Assert.That(predicate(),Is.True,"timed out: "+(P==null?"no fighter":P.AirChaseReason));}
        private static void Record(string name,string value){System.IO.Directory.CreateDirectory("Logs/ShibuyaPass3CQA");System.IO.File.AppendAllText("Logs/ShibuyaPass3CQA/metrics.txt",name+": "+value+Environment.NewLine);}
        [UnityTearDown] public IEnumerator Leave(){if(EditorApplication.isPlaying)yield return new ExitPlayMode();}
        [TearDown] public void Cleanup(){foreach(var obj in Resources.FindObjectsOfTypeAll<ScriptableObject>())if(obj.name.StartsWith("PASS3C QA",StringComparison.Ordinal)&&!AssetDatabase.Contains(obj))Object.DestroyImmediate(obj);}
    }
}
