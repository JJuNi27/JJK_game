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
    public sealed class ShibuyaPass3B1Tests
    {
        [Test] public void KoreanDataUndoAndProtectedValues() {
            var p=AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root+"/Data/Shibuya Combat Profile.asset");
            CollectionAssert.AreEqual(new[]{20f,20f,20f,25f},Array.ConvertAll(p.strikes,s=>s.damage));
            CollectionAssert.AreEqual(new[]{.1f,.09f,.1f,.18f},Array.ConvertAll(p.strikes,s=>s.startup));
            Assert.That(p.finishers.IsValid); Assert.That(p.finishers.uppercut.hits[0].air.usePeakHeight);
            Assert.That(p.finishers.uppercut.hits[0].air.peakHeight,Is.GreaterThan(4));
            foreach(var type in new[]{typeof(ShibuyaTargetingSettings),typeof(ShibuyaAirSettings),typeof(ShibuyaDownSettings)})
                foreach(var field in type.GetFields()) Assert.That(ShibuyaInspector.KoreanLabel(field.Name),Is.Not.EqualTo(field.Name));
            var copy=Object.Instantiate(p); copy.name="PASS3B1 QA Profile";
            var so=new SerializedObject(copy); var property=so.FindProperty("strikes").GetArrayElementAtIndex(0).FindPropertyRelative("targeting").FindPropertyRelative("maxTurnAngle");
            float before=property.floatValue;Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();
            property.floatValue=12;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();Assert.That(copy.strikes[0].targeting.maxTurnAngle,Is.EqualTo(12));
            Undo.CollapseUndoOperations(group);Undo.PerformUndo();Assert.That(copy.strikes[0].targeting.maxTurnAngle,Is.EqualTo(before));
        }
        [UnityTest] public IEnumerator FirstTargetAtZeroDegrees() {Open();yield return new EnterPlayMode();yield return Angle(0,true);yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator FirstTargetAtThirtyDegrees() {Open();yield return new EnterPlayMode();yield return Angle(30,true);yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator FirstTargetAtSixtyDegreesIsLegalMiss() {Open();yield return new EnterPlayMode();yield return Angle(60,false);yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator FirstTargetAtNinetyDegreesIsLegalMiss() {Open();yield return new EnterPlayMode();yield return Angle(90,false);yield return new ExitPlayMode();}
        private IEnumerator Angle(float angle,bool hit) {
            yield return Setup();var p=Match.Player;var d=Match.Dummy;
            Place(p,new Vector3(0,.03f,0));Place(d,Quaternion.AngleAxis(angle,Vector3.up)*Vector3.forward*1.8f+Vector3.up*.03f);
            Assert.That(p.TryAttack());yield return Wait(.25f);
            Assert.That(p.ConfirmedHitCount,Is.EqualTo(hit?1:0));
            Assert.That(p.TrackingTurnApplied,Is.LessThanOrEqualTo(p.Profile.trackingMaxTurnAngle+.01f));
            Assert.That(p.TrackingAdvanceRequested,Is.LessThanOrEqualTo(p.Profile.trackingAdvanceDistance+.001f));
            Record("angle",$"{angle}deg hits={p.ConfirmedHitCount} turn={p.TrackingTurnApplied:F2} approach={p.TrackingAdvanceRequested:F3}");
        }
        [UnityTest] public IEnumerator FocusSurvivesCameraTurnAndNearbyActor() {Open();yield return new EnterPlayMode();yield return Focus();yield return new ExitPlayMode();}
        private IEnumerator Focus() {
            yield return Setup();var p=Match.Player;var d=Match.Dummy;var input=p.GetComponent<ShibuyaPlayerInput>();
            Assert.That(p.TryAttack());yield return Until(()=>p.ConfirmedHitCount==1);Assert.That(p.ComboTarget,Is.SameAs(d));
            var other=Object.Instantiate(d.gameObject).GetComponent<ShibuyaFighter>();other.name="PASS3B1 Other";
            Place(other,p.transform.position+new Vector3(-1.3f,0,.5f));
            Place(d,p.transform.position+new Vector3(.5f,0,1.7f));
            input.TestInput=()=>new ShibuyaInputFrame {look=new Vector2(15,0)};
            yield return Until(()=>p.AttackElapsed>=p.AttackDuration-.12f);Assert.That(p.TryAttack());
            yield return Until(()=>p.AcceptedAttackCount==2);yield return Until(()=>d.Health.CurrentHealth==960);
            Assert.That(p.ComboTarget,Is.SameAs(d));Assert.That(other.Health.CurrentHealth,Is.EqualTo(1000));
            Record("focus",$"target={p.ComboTarget.name}, side error=.5m, camera input active, other damage=0");
        }
        [UnityTest] public IEnumerator TargetCannotReacquireAfterRangeWallImmunityOrHeightEscape() {Open();yield return new EnterPlayMode();yield return Escapes();yield return new ExitPlayMode();}
        private IEnumerator Escapes() {
            yield return Setup();var p=Match.Player;var d=Match.Dummy;
            for(int mode=0;mode<4;mode++) {
                p.ApplyHitReaction(Vector3.zero,0);Place(p,new Vector3(0,.03f,-1.4f));p.transform.rotation=Quaternion.identity;Place(d,new Vector3(0,.03f,0));
                Assert.That(p.TryAttack());int expected=p.ConfirmedHitCount+1;yield return Until(()=>p.ConfirmedHitCount==expected);
                yield return Until(()=>p.AttackElapsed>=p.AttackDuration-.12f);
                GameObject wall=null;
                if(mode==0) Place(d,p.transform.position+new Vector3(4,0,2));
                if(mode==1) {wall=Cube(p.transform.position+new Vector3(0,1,.7f),new Vector3(4,3,.12f));}
                if(mode==2) d.Health.GrantInvulnerability(.6f);
                if(mode==3) Place(d,p.transform.position+new Vector3(0,4,1.5f));
                Assert.That(p.TryAttack());yield return Wait(.55f);
                Assert.That(p.ConfirmedHitCount,Is.EqualTo(expected),"escape case "+mode);
                if(wall!=null) Object.Destroy(wall);yield return Wait(.2f);
            }
        }
        [UnityTest] public IEnumerator DefaultFinisherTracksDisplacedTarget() {Open();yield return new EnterPlayMode();yield return FullCombo(ShibuyaFinisher.Default);yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator UppercutFinisherTracksDisplacedTarget() {Open();yield return new EnterPlayMode();yield return FullCombo(ShibuyaFinisher.Uppercut);yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator DownFinisherTracksDisplacedTarget() {Open();yield return new EnterPlayMode();yield return FullCombo(ShibuyaFinisher.Down);yield return new ExitPlayMode();}
        private IEnumerator FullCombo(ShibuyaFinisher branch) {
            yield return Setup();var p=Match.Player;var d=Match.Dummy;Assert.That(p.TryAttack());
            for(int i=1;i<=3;i++) {
                yield return Until(()=>p.ComboStep==i&&p.AttackElapsed>=p.AttackDuration-.12f);
                Vector3 side=p.transform.right*(i%2==0?-.12f:.12f);Place(d,d.transform.position+side);
                Assert.That(p.TryAttack(branch));yield return Until(()=>p.ComboStep==i+1);
            }
            yield return Until(()=>p.ConfirmedHitCount==4);Assert.That(d.Health.CurrentHealth,Is.EqualTo(915));
            Assert.That(Time.timeScale,Is.EqualTo(1));Record("combo",branch+" damage=85");
        }
        [UnityTest] public IEnumerator ExplicitEventsReuseBoundedAimAndKeepPerEventDedup() {Open();yield return new EnterPlayMode();yield return Explicit();yield return new ExitPlayMode();}
        private IEnumerator Explicit() {
            yield return Setup();var p=Match.Player;var d=Match.Dummy;
            var data=Single(p,ShibuyaFinisher.Down);var a=data.steps[0].hits[0];a.outcome=ShibuyaHitOutcome.Normal;a.strike.hitStopDuration=0;
            a.strike.overrideTargeting=true;a.strike.targeting.maxTurnAngle=10;a.strike.targeting.approachDistance=.05f;
            var b=JsonUtility.FromJson<ShibuyaHitEvent>(JsonUtility.ToJson(a));b.startTime=.35f;b.strike.damage=7;
            data.steps[0].hits=new[]{a,b};
            d.gameObject.AddComponent<BoxCollider>().size=Vector3.one*.5f;
            Assert.That(p.TryAttack());yield return Until(()=>p.ConfirmedHitCount==1);
            Place(d,d.transform.position+Vector3.right*.1f);
            yield return Until(()=>p.ConfirmedHitCount==2);yield return Wait(.3f);
            Assert.That(d.Health.CurrentHealth,Is.EqualTo(968));Assert.That(p.ConfirmedHitCount,Is.EqualTo(2));
            Assert.That(p.TrackingTurnApplied,Is.LessThanOrEqualTo(20.01f));
        }
        [UnityTest] public IEnumerator HighUppercutStationaryAttackerLandsOnWorld() {Open();yield return new EnterPlayMode();yield return Launch(false,false);yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator DescendingCapsuleSeparatesFromMovingAttackerHead() {Open();yield return new EnterPlayMode();yield return Launch(true,false);yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator LaunchInsideWallAndCeilingDoesNotEscape() {Open();yield return new EnterPlayMode();yield return Launch(true,true);yield return new ExitPlayMode();}
        private IEnumerator Launch(bool overlap,bool obstacles) {
            yield return Setup();var p=Match.Player;var d=Match.Dummy;var data=Single(p,ShibuyaFinisher.Uppercut);
            if(overlap&&!obstacles) {
                data.steps[0].hits[0].air.launchHorizontalDistance=0;
                data.steps[0].hits[0].air.descendingHorizontalSpeed=0;
            }
            if(!overlap&&!obstacles) {
                data.steps[0].hits[0].strike.knockbackSpeed=0;
                d.transform.rotation=Quaternion.Euler(0,90,0);
            }
            if(obstacles) {Cube(new Vector3(0,3,0),new Vector3(6,.2f,8));Cube(new Vector3(0,1.5f,1.2f),new Vector3(6,3,.15f));}
            Assert.That(p.TryAttack());yield return Until(()=>d.IsAirborne);
            float peak=0,start=d.transform.position.y;bool repositioned=false;float end=Time.realtimeSinceStartup+5;
            while(d.IsAirborne&&Time.realtimeSinceStartup<end) {
                peak=Mathf.Max(peak,d.transform.position.y-start);
                if(overlap&&!obstacles&&!repositioned&&d.VerticalVelocity<=0) {
                    Place(p,new Vector3(d.transform.position.x,.03f,d.transform.position.z));
                    p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>new ShibuyaInputFrame {move=Vector2.up*.02f};repositioned=true;
                }
                Assert.That(d.transform.position.y,Is.GreaterThan(-.12f));yield return null;
            }
            Assert.That(d.IsAirborne,Is.False);Assert.That(d.IsGrounded);
            Assert.That(d.transform.position.y,Is.LessThan(.2f),"must not treat another fighter's head as world ground");
            if(!obstacles) Assert.That(peak,Is.GreaterThan(4.7f));else Assert.That(peak,Is.LessThan(1.3f));
            if(overlap&&!obstacles) Assert.That(d.LandingSeparationRequested,Is.GreaterThan(.4f),"zero drift must exercise actual capsule separation");
            if(!overlap&&!obstacles) {
                Assert.That(d.transform.position.z,Is.GreaterThan(2),"launch direction uses the real strike, independent of zero velocity knockback");
                Assert.That(Mathf.Abs(d.transform.position.x),Is.LessThan(.1f),"victim facing must not steer the launch");
            }
            Record("launch",$"overlap={overlap} obstacles={obstacles} peak={peak:F3}m landing={d.transform.position} player={p.transform.position} separation request={d.LandingSeparationRequested:F3}m");
        }
        [UnityTest] public IEnumerator GroundCrackWaitsForFloorAndReusesMeshThenFades() {Open();yield return new EnterPlayMode();yield return Crack();yield return new ExitPlayMode();}
        private IEnumerator Crack() {
            yield return Setup();var p=Match.Player;var d=Match.Dummy;Single(p,ShibuyaFinisher.Down);
            Assert.That(p.TryAttack());yield return Until(()=>d.IsDowned);
            yield return Until(()=>d.GetComponent<ShibuyaGroundImpact>()!=null&&d.GetComponent<ShibuyaGroundImpact>().IsVisible);
            var fx=d.GetComponent<ShibuyaGroundImpact>();int id=fx.MeshIdentity;Assert.That(fx.PlayCount,Is.EqualTo(1));
            yield return Until(()=>!fx.IsVisible);yield return Until(()=>d.State==ShibuyaFighterState.Ready);
            Place(p,new Vector3(0,.03f,-1.4f));Place(d,new Vector3(0,.03f,0));p.transform.rotation=Quaternion.identity;
            Assert.That(p.TryAttack());yield return Until(()=>fx.PlayCount==2);
            Assert.That(fx.MeshIdentity,Is.EqualTo(id));Assert.That(fx.IsVisible);d.StopCombat();Assert.That(fx.IsVisible,Is.False);
            Record("crack","actual ground contact; fade complete; mesh reused; combat stop clears");
        }
        [UnityTest] public IEnumerator AirDownCreatesNoCrackUntilGroundImpact() {Open();yield return new EnterPlayMode();yield return AirCrack();yield return new ExitPlayMode();}
        private IEnumerator AirCrack() {
            yield return Setup();var p=Match.Player;var d=Match.Dummy;var data=Single(p,ShibuyaFinisher.Down);
            data.steps[0].hits[0].strike.attackHeight=3;data.steps[0].hits[0].strike.attackRadius=1.2f;
            Place(d,new Vector3(0,2,0));Assert.That(p.TryAttack());yield return Until(()=>d.IsDowned);
            Assert.That(d.GetComponent<ShibuyaGroundImpact>()==null);Assert.That(d.transform.position.y,Is.GreaterThan(.3f));
            yield return Until(()=>d.GetComponent<ShibuyaGroundImpact>()!=null);
            Assert.That(d.transform.position.y,Is.LessThan(.2f));
        }
        [UnityTest] public IEnumerator MissAndImmunityNeverCreateGroundCrack() {Open();yield return new EnterPlayMode();yield return NoCrack();yield return new ExitPlayMode();}
        private IEnumerator NoCrack() {
            yield return Setup();var p=Match.Player;var d=Match.Dummy;Single(p,ShibuyaFinisher.Down);
            Place(d,new Vector3(5,.03f,0));Assert.That(p.TryAttack());yield return Wait(.8f);Assert.That(d.GetComponent<ShibuyaGroundImpact>()==null);
            Place(d,new Vector3(0,.03f,0));d.Health.GrantInvulnerability(1);Assert.That(p.TryAttack());yield return Wait(.8f);
            Assert.That(d.GetComponent<ShibuyaGroundImpact>()==null);Assert.That(p.ConfirmedHitCount,Is.Zero);
        }
        [UnityTest] public IEnumerator SceneExitReleasesOwnedGroundCrackResources() {Open();yield return new EnterPlayMode();yield return CrackResources();yield return new ExitPlayMode();
            Assert.That(Array.Exists(Resources.FindObjectsOfTypeAll<Mesh>(),m=>m.name=="Shibuya Pooled Ground Crack"),Is.False);
            Assert.That(Array.Exists(Resources.FindObjectsOfTypeAll<Material>(),m=>m.name=="Shibuya Ground Crack Runtime"),Is.False);
        }
        private IEnumerator CrackResources() {yield return Setup();Single(Match.Player,ShibuyaFinisher.Down);Match.Player.TryAttack();yield return Until(()=>Match.Dummy.GetComponent<ShibuyaGroundImpact>()!=null);}
        [UnityTest] public IEnumerator AdvancingAttackerAndHighLaunchUseRealCapsulePaths() {Open();yield return new EnterPlayMode();yield return AdvancingLaunch();yield return new ExitPlayMode();}
        private IEnumerator AdvancingLaunch() {
            yield return Setup();var p=Match.Player;var d=Match.Dummy;Single(p,ShibuyaFinisher.Uppercut);
            Vector3 start=p.transform.position;Assert.That(p.TryAttack());yield return Until(()=>d.IsAirborne);
            p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>new ShibuyaInputFrame {move=Vector2.up};
            yield return Until(()=>!d.IsAirborne);Assert.That(d.transform.position.y,Is.LessThan(.2f));
            Assert.That(Vector3.Distance(p.transform.position,start),Is.GreaterThan(2));
            Record("advancing launch",$"p travel={Vector3.Distance(p.transform.position,start):F3}m, target ground y={d.transform.position.y:F3}");
        }
        [UnityTest] public IEnumerator NoFloorAndVerticalWallCannotProduceCrack() {Open();yield return new EnterPlayMode();yield return InvalidSurface();yield return new ExitPlayMode();}
        private IEnumerator InvalidSurface() {
            yield return Setup();var p=Match.Player;var d=Match.Dummy;
            var wall=Cube(new Vector3(0,1,3),new Vector3(5,2,.2f));
            Assert.That(Physics.Raycast(new Vector3(0,1,0),Vector3.forward,out var hit,4));
            var effect=d.gameObject.AddComponent<ShibuyaGroundImpact>();effect.Play(hit,p.Profile.finishers.downFinisher.hits[0].down);
            Assert.That(effect.IsVisible,Is.False);Object.Destroy(wall);
            Single(p,ShibuyaFinisher.Down);Place(p,new Vector3(1000,2,-1.4f));Place(d,new Vector3(1000,2,0));
            Assert.That(p.TryAttack());yield return Until(()=>d.IsDowned);yield return Wait(.5f);
            Assert.That(effect.PlayCount,Is.Zero);Assert.That(effect.IsVisible,Is.False);
        }
        [UnityTest] public IEnumerator SharedTrackingAndCompetingAlignmentHaveOneOwner() {Open();yield return new EnterPlayMode();yield return Competing();yield return new ExitPlayMode();}
        private IEnumerator Competing() {
            yield return Setup();var p=Match.Player;var d=Match.Dummy;
            Place(p,new Vector3(0,.03f,-1.7f));Place(d,new Vector3(.5f,.03f,0));
            var third=Object.Instantiate(d.gameObject).GetComponent<ShibuyaFighter>();
            var copy=Object.Instantiate(p.Profile);copy.name="PASS3B1 QA Profile";
            copy.strikes[0].startup=.01f;copy.strikes[0].advanceDistance=0;
            third.Configure(copy,AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root+"/Data/Dummy Stats.asset"));
            Place(third,new Vector3(10,.03f,10));bool competing=false,observed=false;
            third.HitEventStarted+=i=>competing=d.HasVictimAlignment;
            third.HitConfirmed+=(point,intensity)=> {
                if(competing)Assert.That(d.HasVictimAlignment,Is.False,"a still-active alignment cannot transfer to a competing owner");
                observed=true;
            };
            p.HitConfirmed+=(point,intensity)=> {
                Place(third,d.transform.position+new Vector3(1.4f,0,.5f));
                third.transform.rotation=Quaternion.LookRotation(d.transform.position-third.transform.position);
                Assert.That(third.TryAttack());
            };
            Assert.That(p.TryAttack());yield return Until(()=>observed);
            Assert.That(d.Health.CurrentHealth,Is.EqualTo(960));
            Assert.That(d.VictimAlignmentRequested,Is.LessThanOrEqualTo(p.Profile.victimAlignmentMaxDistance+.001f));
            Record("combined alignment","tracking enabled, two valid attackers, duplicate damage absent, competing active owner cancelled; active at second contact="+competing);
        }
        private static ShibuyaMatch Match=>Object.FindFirstObjectByType<ShibuyaMatch>();
        private static void Open()=>EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);
        private static IEnumerator Setup() {
            Application.runInBackground=true;yield return Wait(.1f);var p=Match.Player;
            p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default;
            var profile=Object.Instantiate(p.Profile);profile.name="PASS3B1 QA Profile";p.Configure(profile,
                AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root+"/Data/Player Stats.asset"));
            Place(p,new Vector3(0,.03f,-1.4f));Place(Match.Dummy,new Vector3(0,.03f,0));p.transform.rotation=Quaternion.identity;
        }
        private static ShibuyaMovesetDefinition Single(ShibuyaFighter p,ShibuyaFinisher branch) {
            var data=ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();data.name="PASS3B1 QA Moveset";
            data.steps=new[]{JsonUtility.FromJson<ShibuyaMovesetStep>(JsonUtility.ToJson(branch==ShibuyaFinisher.Uppercut?p.Profile.finishers.uppercut:p.Profile.finishers.downFinisher))};
            Assert.That(p.SetMoveset(data));return data;
        }
        private static GameObject Cube(Vector3 pos,Vector3 scale) {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.transform.position=pos;go.transform.localScale=scale;Physics.SyncTransforms();return go;}
        private static void Place(ShibuyaFighter f,Vector3 pos) {var cc=f.GetComponent<CharacterController>();cc.enabled=false;f.transform.position=pos;cc.enabled=true;Physics.SyncTransforms();}
        private static IEnumerator Wait(float duration) {float end=Time.time+duration;while(Time.time<end)yield return null;}
        private static IEnumerator Until(Func<bool> test) {float end=Time.realtimeSinceStartup+6;while(!test()&&Time.realtimeSinceStartup<end)yield return null;Assert.That(test(),Is.True,"condition timed out");}
        private static void Record(string key,string detail) {System.IO.Directory.CreateDirectory("Logs/ShibuyaPass3B1QA");System.IO.File.AppendAllText("Logs/ShibuyaPass3B1QA/metrics.txt",key+": "+detail+Environment.NewLine);}
        [UnityTearDown] public IEnumerator Leave() {if(EditorApplication.isPlaying)yield return new ExitPlayMode();}
        [TearDown] public void Cleanup() {
            foreach(var data in Resources.FindObjectsOfTypeAll<ScriptableObject>())
                if((data.name=="PASS3B1 QA Profile"||data.name=="PASS3B1 QA Moveset")&&!AssetDatabase.Contains(data))Object.DestroyImmediate(data);
        }
    }
}
