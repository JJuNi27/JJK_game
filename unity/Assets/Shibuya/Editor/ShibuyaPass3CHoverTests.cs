using System;
using System.Collections;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass3CHoverTests
    {
        private static ShibuyaMatch Match=>Object.FindFirstObjectByType<ShibuyaMatch>();
        private static ShibuyaFighter P=>Match.Player;
        private static ShibuyaFighter D=>Match.Dummy;

        [Test] public void HoverDataKoreanUndoAndProtectedValues() {
            var profile=AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root+"/Data/Shibuya Combat Profile.asset");
            var data=Object.Instantiate(profile.aerialCombat);data.name="PASS3CHover QA Data";
            foreach(var field in new[]{"aerialHoverEnabled","aerialMaxHoverTime","aerialLinkInputWindow","holdVictimOnAirHit"})
                Assert.That(ShibuyaInspector.KoreanLabel(field),Is.Not.EqualTo(field));
            var so=new SerializedObject(data);float before=data.aerialMaxHoverTime;
            Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();so.FindProperty("aerialMaxHoverTime").floatValue=.9f;
            so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();Assert.That(data.aerialMaxHoverTime,Is.EqualTo(.9f));
            Undo.CollapseUndoOperations(group);Undo.PerformUndo();Assert.That(data.aerialMaxHoverTime,Is.EqualTo(before));
            data.aerialMaxHoverTime=float.PositiveInfinity;Assert.That(data.IsValid,Is.False);
            CollectionAssert.AreEqual(new[]{20f,20f,20f,25f},Array.ConvertAll(profile.strikes,s=>s.damage));
            Assert.That(profile.finishers.uppercut.hits[0].air.peakHeight,Is.EqualTo(5.5f));
            Assert.That(profile.aerialCombat.aerialAttack.hits.Length,Is.EqualTo(2));
        }
        [UnityTest] public IEnumerator JumpNativeLmbUsesCommonTwoEventsAndHoldsAllPhases() {Open();yield return new EnterPlayMode();yield return JumpAttack();yield return new ExitPlayMode();}
        private IEnumerator JumpAttack() {
            yield return Setup();yield return JumpPair();
            var input=P.GetComponent<ShibuyaPlayerInput>();var frame=new ShibuyaInputFrame {attack=true,forwardKey=true};input.TestInput=()=>frame;
            yield return null;frame.attack=false;yield return Until(()=>P.IsAirAttacking);
            Assert.That(P.CurrentFinisher,Is.EqualTo(ShibuyaFinisher.Default));
            Assert.That(P.CurrentStrike,Is.SameAs(P.AerialDefinition.aerialAttack.hits[0].strike));
            float start=P.transform.position.y,min=start,max=start;int accepted=P.AcceptedAttackCount;
            while(P.IsAirAttacking) {yield return null;if(!P.IsAirAttacking)break;min=Mathf.Min(min,P.transform.position.y);max=Mathf.Max(max,P.transform.position.y);}
            Assert.That(max-min,Is.LessThan(.001f));Assert.That(D.Health.CurrentHealth,Is.EqualTo(975));
            Assert.That(P.ConfirmedHitCount,Is.EqualTo(2));Assert.That(P.AcceptedAttackCount,Is.EqualTo(accepted));
            Record("ordinary jump full attack",start,min,max,0);yield return Land();
        }
        [UnityTest] public IEnumerator PursuitLmbHoldsAttackerAndActuallyStunnedVictim() {Open();yield return new EnterPlayMode();yield return PursuitAttack();yield return new ExitPlayMode();}
        private IEnumerator PursuitAttack() {
            yield return Setup();yield return Pursuit();Assert.That(P.TryAttack());
            float start=P.transform.position.y,min=start,max=start,victimDrift=0,previous=D.transform.position.y;bool wasHeld=false;int heldFrames=0;
            while(P.IsAirAttacking) {
                yield return null;if(!P.IsAirAttacking)break;min=Mathf.Min(min,P.transform.position.y);max=Mathf.Max(max,P.transform.position.y);
                bool held=D.IsHeldByAerialHit;
                if(held&&wasHeld)victimDrift=Mathf.Max(victimDrift,Mathf.Abs(D.transform.position.y-previous));
                if(held)heldFrames++;wasHeld=held;previous=D.transform.position.y;
            }
            Assert.That(max-min,Is.LessThan(.001f));Assert.That(victimDrift,Is.LessThan(.001f));Assert.That(heldFrames,Is.GreaterThan(2));
            Assert.That(D.Health.CurrentHealth,Is.EqualTo(890));Record("pursuit full attack",start,min,max,victimDrift);yield return Land();
        }
        [UnityTest] public IEnumerator TotalUnscaledHoverCapCancelsAndFallsWithoutStoredVelocity() {Open();yield return new EnterPlayMode();yield return Cap();yield return new ExitPlayMode();}
        private IEnumerator Cap() {
            yield return Setup();P.AerialDefinition.aerialMaxHoverTime=.2f;Place(D,new Vector3(15,.03f,0));yield return JumpPair(false);
            float start=P.transform.position.y;double began=Time.unscaledTimeAsDouble;Assert.That(P.TryAttack());
            yield return Until(()=>!P.IsAirAttacking);double duration=Time.unscaledTimeAsDouble-began;
            Assert.That(duration,Is.GreaterThanOrEqualTo(.19));Assert.That(duration,Is.LessThan(.28));Assert.That(P.IsAerialHovering,Is.False);
            Assert.That(P.VerticalVelocity,Is.GreaterThan(-3));Assert.That(P.transform.position.y,Is.GreaterThan(start-.05f));
            Record("total cap",start,P.transform.position.y,(float)duration,P.VerticalVelocity);yield return Land();
        }
        [UnityTest] public IEnumerator MissHoldsOnlyAttackerAndHasNoFollowupGrace() {Open();yield return new EnterPlayMode();yield return Miss();yield return new ExitPlayMode();}
        private IEnumerator Miss() {
            yield return Setup();Place(D,new Vector3(15,.03f,0));yield return JumpPair(false);Assert.That(P.TryAttack());
            float start=P.transform.position.y;
            while(P.IsAirAttacking){Assert.That(D.IsHeldByAerialHit,Is.False);yield return null;if(P.IsAirAttacking)Assert.That(P.transform.position.y,Is.EqualTo(start).Within(.001f));}
            Assert.That(P.IsAerialHovering,Is.False);Assert.That(P.TryAttack(),Is.False);Assert.That(D.Health.CurrentHealth,Is.EqualTo(1000));
            yield return Wait(.07f);Assert.That(P.transform.position.y,Is.LessThan(start-.01f));yield return Land();
        }
        [UnityTest] public IEnumerator FreshBufferedClickReusesSameStepWithoutRenewingHoverBudget() {Open();yield return new EnterPlayMode();yield return Buffer();yield return new ExitPlayMode();}
        private IEnumerator Buffer() {
            yield return Setup();P.AerialDefinition.aerialMaxHoverTime=.95f;yield return Pursuit();Assert.That(P.TryAttack());Assert.That(P.TryAttack(),Is.False);
            float start=P.transform.position.y;double began=Time.unscaledTimeAsDouble;int count=P.AcceptedAttackCount;
            yield return Until(()=>P.AttackElapsed>=P.AttackDuration-P.AerialDefinition.aerialLinkInputWindow+.01f);
            Assert.That(P.TryAttack(ShibuyaFinisher.Down));Assert.That(P.HasBufferedAirAttack);Assert.That(D.HasConfirmedLink);
            yield return Until(()=>P.AcceptedAttackCount==count+1);
            Assert.That(P.CurrentFinisher,Is.EqualTo(ShibuyaFinisher.Default));
            Assert.That(P.CurrentStrike,Is.SameAs(P.AerialDefinition.aerialAttack.hits[0].strike));
            while(P.IsAirAttacking){Assert.That(P.transform.position.y,Is.EqualTo(start).Within(.001f));yield return null;}
            Assert.That(Time.unscaledTimeAsDouble-began,Is.LessThan(1.04));Assert.That(P.TryAttack(),Is.False);
            Assert.That(P.AirChaseCount,Is.EqualTo(1));Assert.That(P.AcceptedAttackCount,Is.EqualTo(count+1));yield return Land();
        }
        [UnityTest] public IEnumerator SuccessfulAttackGraceHoldsConfirmedVictimThenReleasesBoth() {Open();yield return new EnterPlayMode();yield return Grace();yield return new ExitPlayMode();}
        private IEnumerator Grace() {
            yield return Setup();yield return Pursuit();Assert.That(P.TryAttack());yield return Until(()=>!P.IsAirAttacking);
            Assert.That(P.IsAerialHovering);float y=P.transform.position.y,dy=D.transform.position.y;Assert.That(D.IsHeldByAerialHit);
            yield return Wait(.04f);Assert.That(P.transform.position.y,Is.EqualTo(y).Within(.001f));
            Assert.That(D.transform.position.y,Is.EqualTo(dy).Within(.001f));Assert.That(D.IsHeldByAerialHit);
            yield return Wait(.13f);Assert.That(!P.IsAerialHovering&&!P.CanAirAttack&&!D.IsHeldByAerialHit);
            Assert.That(P.TryAttack(),Is.False);Assert.That(D.transform.position.y,Is.LessThan(dy));yield return Land();
        }
        [UnityTest] public IEnumerator StunExpiryKeepsConfirmedAirHeightUntilLinkWindowEnds() {Open();yield return new EnterPlayMode();yield return Stun();yield return new ExitPlayMode();}
        private IEnumerator Stun() {
            yield return Setup();P.AerialDefinition.aerialAttack.hits=new[]{P.AerialDefinition.aerialAttack.hits[0]};
            var s=P.AerialDefinition.aerialAttack.hits[0].strike;s.hitStun=.025f;s.recovery=.3f;s.hitStopDuration=0;
            yield return Pursuit();Assert.That(P.TryAttack());yield return Until(()=>D.IsHeldByAerialHit);float y=D.transform.position.y;
            // PASS 3C.1 explicitly separates short hit-stun from the finite, actual-hit air relation.
            yield return Wait(.08f);Assert.That(P.IsAirAttacking);Assert.That(P.IsAerialHovering);
            Assert.That(D.IsHeldByAerialHit);Assert.That(D.transform.position.y,Is.EqualTo(y).Within(.001f));
            yield return Until(()=>!P.IsAerialHovering);yield return null;Assert.That(D.IsHeldByAerialHit,Is.False);
            yield return Wait(.06f);Assert.That(D.transform.position.y,Is.LessThan(y-.01f));yield return Land();
        }
        [UnityTest] public IEnumerator ImmunityReleasesVictimAndPreventsFollowingDamage() {Open();yield return new EnterPlayMode();yield return Immunity();yield return new ExitPlayMode();}
        private IEnumerator Immunity() {
            yield return Setup();yield return Pursuit();Assert.That(P.TryAttack());yield return Until(()=>D.IsHeldByAerialHit);
            float hp=D.Health.CurrentHealth;D.Health.GrantInvulnerability(.8f);yield return null;Assert.That(D.IsHeldByAerialHit,Is.False);
            yield return Until(()=>!P.IsAirAttacking);Assert.That(D.Health.CurrentHealth,Is.EqualTo(hp));yield return Land();
        }
        [UnityTest] public IEnumerator AttackerHitCancelsBothHeightOwnersAndRestoresFall() {Open();yield return new EnterPlayMode();yield return Interrupted();yield return new ExitPlayMode();}
        private IEnumerator Interrupted() {
            yield return Setup();yield return Pursuit();Assert.That(P.TryAttack());yield return Until(()=>D.IsHeldByAerialHit);
            float y=P.transform.position.y;P.ApplyHitReaction(Vector3.back,.1f);yield return null;
            Assert.That(!P.IsAirAttacking&&!P.IsAerialHovering&&!D.IsHeldByAerialHit);
            yield return Wait(.08f);Assert.That(P.transform.position.y,Is.LessThan(y-.02f));yield return Land();
        }
        [UnityTest] public IEnumerator DeathAndBattleEndClearBothHeightOwners() {Open();yield return new EnterPlayMode();yield return Death();yield return new ExitPlayMode();}
        private IEnumerator Death() {
            yield return Setup();yield return Pursuit();Assert.That(P.TryAttack());yield return Until(()=>D.IsHeldByAerialHit);D.Health.Kill();yield return null;
            Assert.That(Match.Finished);Assert.That(!P.IsAerialHovering&&!D.IsHeldByAerialHit&&!P.IsAirAttacking);
        }
        [UnityTest] public IEnumerator DisablingAttackerReleasesVictimHeight() {Open();yield return new EnterPlayMode();yield return Disabled();yield return new ExitPlayMode();}
        private IEnumerator Disabled() {
            yield return Setup();yield return Pursuit();Assert.That(P.TryAttack());yield return Until(()=>D.IsHeldByAerialHit);P.enabled=false;yield return null;
            Assert.That(!P.IsAerialHovering&&!D.IsHeldByAerialHit);
        }
        [UnityTest] public IEnumerator HoverToggleOffKeepsSharedAirDataAndAppliesGravity() {Open();yield return new EnterPlayMode();yield return Toggle();yield return new ExitPlayMode();}
        private IEnumerator Toggle() {
            yield return Setup();P.AerialDefinition.aerialHoverEnabled=false;yield return JumpPair();Assert.That(P.TryAttack());
            float y=P.transform.position.y;Assert.That(P.IsAerialHovering,Is.False);
            Assert.That(P.CurrentStrike,Is.SameAs(P.AerialDefinition.aerialAttack.hits[0].strike));
            yield return Wait(.12f);Assert.That(Mathf.Abs(P.transform.position.y-y),Is.GreaterThan(.05f));yield return Land();
        }
        [UnityTest] public IEnumerator WallCrossingTheActualPursuitCapsulePathRejectsSpace() {Open();yield return new EnterPlayMode();yield return WallPath();yield return new ExitPlayMode();}
        private IEnumerator WallPath() {
            yield return Setup();Assert.That(P.TryAttack());
            for(int i=1;i<=3;i++){yield return Until(()=>P.ComboStep==i&&P.AttackElapsed>=P.AttackDuration-.12f);Assert.That(P.TryAttack(ShibuyaFinisher.Uppercut));yield return Until(()=>P.ComboStep==i+1);}
            yield return Until(()=>P.ConfirmedHitCount==4);yield return Wait(.18f);
            // Test-only target displacement ensures the wall intersects the real path, not just the view.
            Place(D,D.transform.position+Vector3.forward*2);
            var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
            var destination=(Vector3)typeof(ShibuyaFighter).GetMethod("ChaseDestination",flags).Invoke(P,null);
            Vector3 path=destination-P.transform.position;
            var clear=typeof(ShibuyaFighter).GetMethod("ChasePathClear",flags);
            Assert.That((bool)clear.Invoke(P,new object[]{path}),Is.True,"clear capsule path before adding obstacle");
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position=P.transform.position+new Vector3(0,1.4f,.45f);wall.transform.localScale=new Vector3(5,5,.12f);Physics.SyncTransforms();
            Assert.That((bool)clear.Invoke(P,new object[]{path}),Is.False,"new obstacle actually intersects intended path");
            Vector3 before=P.transform.position;P.SetIntent(Vector3.zero,false,true);
            Assert.That(P.HasBufferedAirChase,Is.False);yield return Wait(.2f);
            Assert.That(P.AirChaseCount,Is.Zero);Assert.That(P.transform.position.z-before.z,Is.LessThan(.2f));
        }

        private static IEnumerator Setup() {
            Application.runInBackground=true;yield return Wait(.1f);P.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default;
            var profile=Object.Instantiate(P.Profile);profile.name="PASS3CHover QA Profile";
            profile.aerialCombat=Object.Instantiate(profile.aerialCombat);profile.aerialCombat.name="PASS3CHover QA Data";
            P.Configure(profile,AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root+"/Data/Player Stats.asset"));
            Place(P,new Vector3(0,.03f,-1.4f));Place(D,new Vector3(0,.03f,0));P.transform.rotation=Quaternion.identity;yield return null;
        }
        private static IEnumerator JumpPair(bool target=true) {
            P.SetIntent(Vector3.zero,false,true);if(target)D.SetIntent(Vector3.zero,false,true);
            yield return Until(()=>P.transform.position.y>.7f && (!target||D.transform.position.y>.7f));
        }
        private static IEnumerator Pursuit() {
            Assert.That(P.TryAttack());
            for(int i=1;i<=3;i++){yield return Until(()=>P.ComboStep==i&&P.AttackElapsed>=P.AttackDuration-.12f);Assert.That(P.TryAttack(ShibuyaFinisher.Uppercut));yield return Until(()=>P.ComboStep==i+1);}
            yield return Until(()=>P.ConfirmedHitCount==4);P.SetIntent(Vector3.zero,false,true);yield return Until(()=>P.CanAirAttack);
        }
        private static IEnumerator Land(){yield return Until(()=>!P.IsInAirSession&&P.IsGrounded);yield return Until(()=>P.State==ShibuyaFighterState.Ready);var input=P.GetComponent<ShibuyaPlayerInput>();var camera=Object.FindFirstObjectByType<ShibuyaOrbitCamera>();input.TestInput=()=>{Vector3 f=camera.transform.forward;f.y=0;f.Normalize();Vector3 r=camera.transform.right;r.y=0;r.Normalize();return new ShibuyaInputFrame {move=new Vector2(Vector3.Dot(Vector3.right,r),Vector3.Dot(Vector3.right,f))};};Vector3 at=P.transform.position;yield return Wait(.1f);Assert.That(P.transform.position.x,Is.GreaterThan(at.x+.1f));input.TestInput=()=>default;Assert.That(Time.timeScale,Is.EqualTo(1));}
        private static void Open()=>EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);
        private static void Place(ShibuyaFighter f,Vector3 at){var cc=f.GetComponent<CharacterController>();cc.enabled=false;f.transform.position=at;cc.enabled=true;Physics.SyncTransforms();}
        private static IEnumerator Wait(float seconds){float end=Time.time+seconds;while(Time.time<end)yield return null;}
        private static IEnumerator Until(Func<bool> test){float end=Time.realtimeSinceStartup+7;while(!test()&&Time.realtimeSinceStartup<end)yield return null;Assert.That(test(),Is.True,"air hover scenario timed out");}
        [Serializable] private sealed class Metric {public string scenario;public float startY,minY,maxY,other;}
        private static void Record(string name,float start,float min,float max,float other){System.IO.Directory.CreateDirectory(ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass3CHoverQA"));System.IO.File.AppendAllText(ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass3CHoverQA/metrics.jsonl"),JsonUtility.ToJson(new Metric {scenario=name,startY=start,minY=min,maxY=max,other=other})+Environment.NewLine);}
        [UnityTearDown] public IEnumerator Leave(){if(EditorApplication.isPlaying)yield return new ExitPlayMode();}
        [TearDown] public void Cleanup(){foreach(var obj in Resources.FindObjectsOfTypeAll<ScriptableObject>())if(obj.name.StartsWith("PASS3CHover QA",StringComparison.Ordinal)&&!AssetDatabase.Contains(obj))Object.DestroyImmediate(obj);}
    }
}
