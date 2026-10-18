using System;
using System.Collections;
using System.Collections.Generic;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass22Tests
    {
        [Serializable] private sealed class Metric {
            public string scenario;
            public int hits;
            public float lateralBefore, lateralAfter, correction, forwardTravel, maxStep, headingChange, actionGap;
        }
        [Test] public void KoreanSettingsSerializedUndoAndUserValues()
        {
            var profile = AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root + "/Data/Shibuya Combat Profile.asset");
            var stats = AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root + "/Data/Dummy Stats.asset");
            Assert.That(stats.MaxHealth, Is.EqualTo(1000));
            CollectionAssert.AreEqual(new[] {20f,20f,20f,25f}, Array.ConvertAll(profile.strikes, s => s.damage));
            CollectionAssert.AreEqual(new[] {6f,4f,4f,11f}, Array.ConvertAll(profile.strikes, s => s.knockbackSpeed));
            CollectionAssert.AreEqual(new[] {.25f,.25f,.25f,.45f}, Array.ConvertAll(profile.strikes, s => s.hitStun));
            Assert.That(profile.strikes[3].alignVictimOnHit, Is.False);
            var copy = Object.Instantiate(profile);
            try {
                var so = new SerializedObject(copy);
                foreach (string field in new[] {"victimAlignmentEnabled", "victimAlignmentMaxLateralOffset",
                    "victimAlignmentMaxDistance", "victimAlignmentSpeed", "victimAlignmentDuration",
                    "attackLateralInputMultiplier", "attackBackwardInputMultiplier"}) {
                    Assert.That(so.FindProperty(field), Is.Not.Null);
                    Assert.That(ShibuyaInspector.KoreanLabel(field), Is.Not.EqualTo(field));
                }
                Assert.That(ShibuyaInspector.KoreanLabel("alignVictimOnHit"), Is.Not.EqualTo("alignVictimOnHit"));
                var property = so.FindProperty("victimAlignmentSpeed"); float old = property.floatValue;
                Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
                property.floatValue = old + 1; so.ApplyModifiedProperties(); Undo.FlushUndoRecordObjects();
                Assert.That(copy.victimAlignmentSpeed, Is.EqualTo(old + 1));
                Undo.CollapseUndoOperations(group); Undo.PerformUndo();
                Assert.That(copy.victimAlignmentSpeed, Is.EqualTo(old));
            } finally { Object.DestroyImmediate(copy); }
        }

        [UnityTest] public IEnumerator FrontAndObliqueStandingCombos() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return ComboScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator ComboScenario()
        {
            Application.runInBackground = true; yield return Wait(.1f);
            var m = Object.FindFirstObjectByType<ShibuyaMatch>(); var p = m.Player; var d = m.Dummy;
            bool press = false, alternating = false;
            p.GetComponent<ShibuyaPlayerInput>().TestInput = () => new ShibuyaInputFrame {attack = press,
                move = !alternating ? Vector2.zero : Time.frameCount%3==0 ? Vector2.left
                    : Time.frameCount%3==1 ? Vector2.right : Vector2.down};
            foreach (float offset in new[] {0f, .45f, -.45f, .3f}) {
                alternating = offset == .3f;
                p.ApplyHitReaction(Vector3.zero, 0); Place(p, new Vector3(0,.03f,-1.7f));
                p.transform.rotation = Quaternion.identity;
                Place(d, new Vector3(offset,.03f,0)); d.transform.rotation = Quaternion.Euler(0,110,0);
                Quaternion facing = d.transform.rotation;
                float hp = d.Health.CurrentHealth; int hits = p.ConfirmedHitCount;
                Vector3 start = p.transform.position;
                int attacks=p.AcceptedAttackCount;
                press = true; yield return Until(()=>p.AcceptedAttackCount>attacks, "initial press offset "+offset);
                press = false; yield return Wait(.02f);
                for (int step=1; step<4; step++) {
                    yield return Until(() => p.ComboStep == step && p.AttackElapsed >= p.CurrentStrike.Duration - .07f,
                        "buffer window step "+step+" offset "+offset);
                    Assert.That(p.ConfirmedHitCount-hits, Is.EqualTo(step), "each oblique light hit must connect");
                    press = true; yield return Until(()=>p.HasBufferedAttack, "reserve successor "+step);
                    press = false; yield return Wait(.02f);
                    yield return Until(() => p.ComboStep == step+1, "next step "+step);
                }
                yield return Until(() => p.State == ShibuyaFighterState.Ready);
                yield return Until(() => d.State == ShibuyaFighterState.Ready);
                Assert.That(p.ConfirmedHitCount-hits, Is.EqualTo(4));
                Assert.That(d.Health.CurrentHealth, Is.EqualTo(hp-85));
                Assert.That(Quaternion.Angle(facing,d.transform.rotation), Is.LessThan(.01f), "victim facing is independent");
                Record(new Metric {scenario=(alternating?"alternating side/back combo offset ":"combo offset ")+offset, hits=p.ConfirmedHitCount-hits,
                    lateralBefore=Mathf.Abs(offset), lateralAfter=Mathf.Abs(d.transform.position.x),
                    forwardTravel=p.transform.position.z-start.z});
            }
        }

        [UnityTest] public IEnumerator VictimAlignmentIsBoundedAndLeavesRealActionGaps() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return AlignmentScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator AlignmentScenario()
        {
            Application.runInBackground=true; yield return Wait(.1f);
            var m=Object.FindFirstObjectByType<ShibuyaMatch>(); var p=m.Player; var d=m.Dummy;
            p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default;
            // Isolate victim alignment from the separately tested PASS3B.1 first-contact aim.
            Clone(p).trackingEnabled=false;
            Place(p,new Vector3(0,.03f,-1.7f)); p.transform.rotation=Quaternion.identity;
            Place(d,new Vector3(.5f,.03f,0)); d.transform.rotation=Quaternion.Euler(0,90,0);
            Assert.That(p.TryAttack(),Is.True); yield return Until(()=>p.ConfirmedHitCount==1);
            Assert.That(d.KnockbackVelocity.x,Is.EqualTo(0).Within(.001f));
            Assert.That(d.KnockbackVelocity.z,Is.GreaterThan(0));
            float x=d.transform.position.x, hitAt=Time.time; Vector3 previous=d.transform.position;
            float maxStep=0;
            while(d.HasVictimAlignment) {
                yield return null;
                float step=Mathf.Abs(d.transform.position.x-previous.x); maxStep=Mathf.Max(maxStep,step);
                Assert.That(step,Is.LessThanOrEqualTo(p.Profile.victimAlignmentSpeed*Time.deltaTime+.003f),"no lateral snap");
                previous=d.transform.position;
            }
            float correction=x-d.transform.position.x;
            Assert.That(correction,Is.GreaterThan(.08f));
            Assert.That(correction,Is.LessThanOrEqualTo(p.Profile.victimAlignmentMaxDistance+.002f));
            Assert.That(d.transform.eulerAngles.y,Is.EqualTo(90).Within(.01f));
            yield return Until(()=>d.State==ShibuyaFighterState.Ready);
            Assert.That(p.State,Is.EqualTo(ShibuyaFighterState.Attacking));
            float readyAt=Time.time;
            Assert.That(d.TryAttack(),Is.True,"alignment must not lengthen stun or remove real action windows");
            Record(new Metric {scenario="bounded lateral correction", lateralBefore=x,lateralAfter=d.transform.position.x,
                correction=correction,maxStep=maxStep,actionGap=p.CurrentStrike.Duration-p.AttackElapsed,
                forwardTravel=readyAt-hitAt});
        }

        [UnityTest] public IEnumerator OppositeSideAndBackInputDoNotSlideDuringStrikeAndRecoverControl() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return MovementScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator MovementScenario()
        {
            Application.runInBackground=true; yield return Wait(.1f);
            var m=Object.FindFirstObjectByType<ShibuyaMatch>(); var p=m.Player; var d=m.Dummy;
            var input=p.GetComponent<ShibuyaPlayerInput>();
            var cam=Object.FindFirstObjectByType<ShibuyaOrbitCamera>();
            Place(d,new Vector3(15,.03f,15));
            foreach(Vector3 direction in new[] {Vector3.left,Vector3.forward,Vector3.back}) {
                input.TestInput=()=>default; p.ApplyHitReaction(Vector3.zero,0);
                Place(p,new Vector3(0,.03f,-2)); p.transform.rotation=Quaternion.Euler(0,90,0);
                Quaternion facing=p.transform.rotation; Vector3 start=p.transform.position;
                Assert.That(p.TryAttack(),Is.True);
                input.TestInput=()=> {
                    Vector3 f=cam.transform.forward; f.y=0; f.Normalize();
                    Vector3 r=cam.transform.right; r.y=0; r.Normalize();
                    return new ShibuyaInputFrame {move=new Vector2(Vector3.Dot(direction,r),Vector3.Dot(direction,f))};
                };
                while(p.AttackElapsed<p.CurrentStrike.ActiveEnd-.015f) {
                    yield return null;
                    Assert.That(Quaternion.Angle(facing,p.transform.rotation),Is.LessThan(.01f));
                    Assert.That(Mathf.Abs(p.transform.position.z-start.z),Is.LessThan(.005f));
                    Assert.That(p.transform.position.x,Is.GreaterThanOrEqualTo(start.x-.001f));
                }
                Assert.That(Vector3.Dot(p.MoveIntent,direction),Is.GreaterThan(.99f),"raw intent survives filtering");
                Assert.That(input.MoveAxes.magnitude,Is.GreaterThan(.99f));
                Assert.That(p.AttackAdvanceRequested,Is.GreaterThan(.1f));
                Record(new Metric {scenario="right-facing input "+direction, forwardTravel=p.transform.position.x-start.x,
                    lateralAfter=p.transform.position.z-start.z,headingChange=Quaternion.Angle(facing,p.transform.rotation)});
                yield return Until(()=>p.State==ShibuyaFighterState.Ready);
                Vector3 recovered=p.transform.position;
                yield return Wait(.12f);
                Assert.That(Vector3.Dot(p.transform.position-recovered,direction),Is.GreaterThan(.25f),"normal movement returns");
            }
        }

        [UnityTest] public IEnumerator EscapeInvulnerabilityAndSourceInterruptionCancelAlignment() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return EscapeScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator EscapeScenario()
        {
            Application.runInBackground=true; yield return Wait(.1f);
            var m=Object.FindFirstObjectByType<ShibuyaMatch>(); var p=m.Player; var d=m.Dummy;
            p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default;
            // Isolate victim alignment from the separately tested PASS3B.1 first-contact aim.
            Clone(p).trackingEnabled=false;
            for(int trial=0;trial<4;trial++) {
                yield return Until(()=>d.State==ShibuyaFighterState.Ready);
                p.ApplyHitReaction(Vector3.zero,0); Place(p,new Vector3(0,.03f,-1.7f)); p.transform.rotation=Quaternion.identity;
                Place(d,new Vector3(.5f,.03f,0)); int hits=p.ConfirmedHitCount;
                Assert.That(p.TryAttack(),Is.True); yield return Until(()=>p.ConfirmedHitCount==hits+1);
                if(trial==0) Place(d,new Vector3(4,.03f,0));
                if(trial==1) Place(d,new Vector3(.5f,.03f,8));
                if(trial==2) d.Health.GrantInvulnerability(.5f);
                if(trial==3) p.ApplyHitReaction(Vector3.zero,.2f);
                float x=d.transform.position.x;
                yield return null; yield return null;
                Assert.That(d.HasVictimAlignment,Is.False);
                Assert.That(d.transform.position.x,Is.EqualTo(x).Within(.001f),"no post-escape pull");
                yield return Until(()=>p.State==ShibuyaFighterState.Ready);
                if(trial<2) {
                    Assert.That(p.TryAttack(),Is.True); yield return Wait(.2f);
                    Assert.That(p.ConfirmedHitCount,Is.EqualTo(hits+1),"real escape must miss");
                }
                yield return Wait(.55f);
            }
        }

        [UnityTest] public IEnumerator VictimCorrectionRespectsWallsAndOtherFighters() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return CollisionScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator CollisionScenario()
        {
            Application.runInBackground=true; yield return Wait(.1f);
            var m=Object.FindFirstObjectByType<ShibuyaMatch>(); var p=m.Player; var d=m.Dummy;
            p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default;
            for(int trial=0;trial<2;trial++) {
                yield return Until(()=>d.State==ShibuyaFighterState.Ready);
                p.ApplyHitReaction(Vector3.zero,0); Place(p,new Vector3(0,.03f,-1.7f)); p.transform.rotation=Quaternion.identity;
                Place(d,new Vector3(.6f,.03f,0)); int hits=p.ConfirmedHitCount;
                Assert.That(p.TryAttack(),Is.True); yield return Until(()=>p.ConfirmedHitCount==hits+1);
                GameObject obstacle;
                if(trial==0) {
                    obstacle=GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obstacle.transform.position=new Vector3(.12f,1,.7f); obstacle.transform.localScale=new Vector3(.1f,3,4);
                } else {
                    obstacle=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(ShibuyaSceneBuilder.Root+"/Prefabs/ShibuyaDummy.prefab"));
                    Place(obstacle.GetComponent<ShibuyaFighter>(),new Vector3(-.18f,.03f,d.transform.position.z));
                }
                Physics.SyncTransforms();
                float end=Time.time+.16f;
                while(Time.time<end) {
                    yield return null;
                    if(trial==0) Assert.That(d.transform.position.x,Is.GreaterThan(.49f),"capsule may not cross wall");
                    else {
                        Vector3 separation=d.transform.position-obstacle.transform.position; separation.y=0;
                        Assert.That(separation.magnitude,Is.GreaterThanOrEqualTo(.68f),"no fighter capsule penetration; going around is allowed");
                    }
                }
                Record(new Metric {scenario=trial==0?"alignment wall":"alignment third fighter",lateralBefore=.6f,
                    lateralAfter=d.transform.position.x,correction=d.VictimAlignmentRequested});
                Object.Destroy(obstacle); yield return Wait(.15f);
                float x=d.transform.position.x; yield return Wait(.1f);
                Assert.That(d.transform.position.x,Is.EqualTo(x).Within(.001f),"no stored snap after obstruction clears");
            }
        }

        [UnityTest] public IEnumerator ConflictingAttackersDoNotCombineAlignmentForces() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return MultiAttackerScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator MultiAttackerScenario()
        {
            Application.runInBackground=true; yield return Wait(.1f);
            var m=Object.FindFirstObjectByType<ShibuyaMatch>(); var p=m.Player; var d=m.Dummy;
            p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default;
            // Isolate victim alignment from the separately tested PASS3B.1 first-contact aim.
            Clone(p).trackingEnabled=false;
            var third=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(ShibuyaSceneBuilder.Root+"/Prefabs/ShibuyaDummy.prefab"))
                .GetComponent<ShibuyaFighter>();
            var copy=Clone(third); copy.strikes[0].startup=.01f; copy.strikes[0].advanceDistance=0;
            Place(third,new Vector3(8,.03f,8));
            Place(p,new Vector3(0,.03f,-1.7f)); p.transform.rotation=Quaternion.identity;
            Place(d,new Vector3(.5f,.03f,0));
            p.HitConfirmed+=(point,intensity)=> {
                Assert.That(d.HasVictimAlignment,Is.True);
                Place(third,d.transform.position+new Vector3(1.4f,0,.5f));
                third.transform.rotation=Quaternion.LookRotation(d.transform.position-third.transform.position);
                Assert.That(third.TryAttack(),Is.True);
            };
            Assert.That(p.TryAttack(),Is.True); yield return Until(()=>third.ConfirmedHitCount>0);
            Assert.That(d.HasVictimAlignment,Is.False,"a conflicting valid hit cancels both alignment owners");
            Assert.That(d.Health.CurrentHealth,Is.EqualTo(960));
            yield return Wait(.12f); Assert.That(d.HasVictimAlignment,Is.False);
        }

        private static ShibuyaCombatProfile Clone(ShibuyaFighter p) {
            var copy=Object.Instantiate(p.Profile); copy.name="Shibuya PASS22 QA Profile";
            var original=new SerializedObject(p).FindProperty("stats").objectReferenceValue as CharacterStatsProfile;
            p.Configure(copy,original); return copy;
        }
        private static void Record(Metric value) {
            System.IO.Directory.CreateDirectory("Logs/ShibuyaPass22QA");
            System.IO.File.AppendAllText("Logs/ShibuyaPass22QA/metrics.jsonl",JsonUtility.ToJson(value)+Environment.NewLine);
        }
        private static IEnumerator Wait(float seconds) { float end=Time.time+seconds; while(Time.time<end) yield return null; }
        private static IEnumerator Until(Func<bool> condition, string context="") {
            float end=Time.realtimeSinceStartup+4; while(!condition() && Time.realtimeSinceStartup<end) yield return null;
            Assert.That(condition(),Is.True,"Play Mode condition timed out: "+context);
        }
        private static void Place(ShibuyaFighter p,Vector3 position) {
            var motor=p.GetComponent<CharacterController>(); motor.enabled=false; p.transform.position=position;
            motor.enabled=true; Physics.SyncTransforms();
        }
        [UnityTearDown] public IEnumerator Leave() { if(EditorApplication.isPlaying) yield return new ExitPlayMode(); }
        [TearDown] public void Cleanup() {
            foreach(var p in Resources.FindObjectsOfTypeAll<ShibuyaCombatProfile>())
                if(p.name=="Shibuya PASS22 QA Profile" && !AssetDatabase.Contains(p)) Object.DestroyImmediate(p);
        }
    }
}
