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
    public sealed class ShibuyaPass23Tests
    {
        [Serializable] private sealed class Metric {
            public string scenario;
            public Vector3 localDirection, pose, headOffset;
            public float rootYawChange, normalPeak, heavyPeak, pausedSeconds;
            public int hits;
        }
        [Test] public void KoreanReactionDataUndoAndProtectedCombatValues()
        {
            var p=AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root+"/Data/Shibuya Combat Profile.asset");
            var stats=AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root+"/Data/Dummy Stats.asset");
            Assert.That(stats.MaxHealth,Is.EqualTo(1000));
            CollectionAssert.AreEqual(new[] {20f,20f,20f,25f},Array.ConvertAll(p.strikes,s=>s.damage));
            CollectionAssert.AreEqual(new[] {6f,4f,4f,11f},Array.ConvertAll(p.strikes,s=>s.knockbackSpeed));
            CollectionAssert.AreEqual(new[] {.1f,.09f,.1f,.18f},Array.ConvertAll(p.strikes,s=>s.startup));
            CollectionAssert.AreEqual(new[] {.25f,.25f,.25f,.45f},Array.ConvertAll(p.strikes,s=>s.hitStun));
            Assert.That(p.strikes[3].hitStopDuration,Is.EqualTo(.065f));
            Assert.That(p.heavyHitReactionMultiplier,Is.GreaterThan(p.normalHitReactionMultiplier));
            var copy=Object.Instantiate(p);
            try {
                var so=new SerializedObject(copy);
                foreach(string field in new[] {"hitReactionEnabled","frontHitReactionStrength","leftHitReactionStrength",
                    "rightHitReactionStrength","backHitReactionStrength","hitReactionLeanAngle","hitReactionTwistAngle",
                    "hitReactionStartTime","hitReactionRecoveryTime","normalHitReactionMultiplier","heavyHitReactionMultiplier"}) {
                    Assert.That(so.FindProperty(field),Is.Not.Null);
                    Assert.That(ShibuyaInspector.KoreanLabel(field),Is.Not.EqualTo(field));
                }
                var angle=so.FindProperty("hitReactionLeanAngle"); float original=angle.floatValue;
                Undo.IncrementCurrentGroup(); int group=Undo.GetCurrentGroup();
                angle.floatValue=original+2; so.ApplyModifiedProperties(); Undo.FlushUndoRecordObjects();
                Assert.That(copy.hitReactionLeanAngle,Is.EqualTo(original+2));
                Undo.CollapseUndoOperations(group); Undo.PerformUndo();
                Assert.That(copy.hitReactionLeanAngle,Is.EqualTo(original));
            } finally { Object.DestroyImmediate(copy); }
        }

        [UnityTest] public IEnumerator RealHitsReactInFourLocalDirectionsAndDiagonalsWithoutTurningMotor() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return DirectionScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator DirectionScenario()
        {
            Application.runInBackground=true; yield return Wait(.1f);
            var m=Object.FindFirstObjectByType<ShibuyaMatch>(); var p=m.Player; var d=m.Dummy;
            p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default;
            var profile=Clone(p); profile.victimAlignmentEnabled=false; profile.trackingEnabled=false;
            foreach(var s in profile.strikes) s.advanceDistance=0;
            var visual=d.GetComponent<ShibuyaBlockVisual>();
            var root=d.transform.Find("BlockVisual"); var head=root.Find("Head"); var torso=root.Find("Torso");
            Vector3 neutralHead=head.localPosition;
            var received=new List<ShibuyaHitReaction>(); d.HitReacted+=hit=>received.Add(hit);
            var directions=new[] {Vector3.back,Vector3.right,Vector3.left,Vector3.forward,
                new Vector3(1,0,-1).normalized,Vector3.back};
            var names=new[] {"front","left","right","back","diagonal","zero knockback front"};
            for(int i=0;i<directions.Length;i++) {
                p.ApplyHitReaction(Vector3.zero,0); d.ApplyHitReaction(Vector3.zero,0);
                Quaternion facing=Quaternion.Euler(0,37,0); d.transform.rotation=facing;
                Vector3 world=facing*directions[i];
                Place(d,new Vector3(0,.03f,0)); Place(p,d.transform.position-world*1.4f);
                p.transform.rotation=Quaternion.LookRotation(world);
                profile.strikes[0].knockbackSpeed=i==5?0f:6f;
                float hp=d.Health.CurrentHealth; int hits=p.ConfirmedHitCount;
                Assert.That(p.TryAttack(),Is.True); yield return Until(()=>p.ConfirmedHitCount>hits);
                yield return Wait(d.Profile.hitReactionStartTime+.008f);
                Vector3 pose=visual.ReactionEuler, offset=head.localPosition-neutralHead;
                Assert.That(received.Count,Is.EqualTo(i+1));
                Assert.That(Vector3.Distance(received[i].LocalDirection,directions[i]),Is.LessThan(.001f));
                Assert.That(d.Health.CurrentHealth,Is.EqualTo(hp-20));
                Assert.That(Quaternion.Angle(d.transform.rotation,facing),Is.LessThan(.001f));
                Assert.That(Quaternion.Angle(root.localRotation,Quaternion.identity),Is.LessThan(.001f),"no whole-root tilt");
                Assert.That(torso.localRotation,Is.Not.EqualTo(Quaternion.identity));
                Assert.That(pose.magnitude,Is.GreaterThan(10));
                if(i==0 || i==5) { Assert.That(pose.x,Is.LessThan(-10)); Assert.That(offset.z,Is.LessThan(-.1f)); }
                if(i==1) { Assert.That(pose.z,Is.LessThan(-10)); Assert.That(pose.y,Is.GreaterThan(10)); Assert.That(offset.x,Is.GreaterThan(.1f)); }
                if(i==2) { Assert.That(pose.z,Is.GreaterThan(10)); Assert.That(pose.y,Is.LessThan(-10)); Assert.That(offset.x,Is.LessThan(-.1f)); }
                if(i==3) { Assert.That(pose.x,Is.GreaterThan(10)); Assert.That(offset.z,Is.GreaterThan(.1f)); }
                if(i==4) {
                    Assert.That(pose.x,Is.LessThan(-5)); Assert.That(pose.z,Is.LessThan(-5));
                    Assert.That(Mathf.Abs(pose.x),Is.LessThan(d.Profile.hitReactionLeanAngle));
                    Assert.That(Mathf.Abs(pose.z),Is.LessThan(d.Profile.hitReactionLeanAngle));
                }
                Assert.That(d.GetComponent<CharacterController>().height,Is.EqualTo(1.8f));
                if(i==5) Assert.That(d.KnockbackVelocity.magnitude,Is.Zero,"visual direction must not depend on impulse magnitude");
                else Assert.That(Vector3.Dot(d.KnockbackVelocity,world),Is.GreaterThan(0));
                Record(new Metric {scenario=names[i],localDirection=received[i].LocalDirection,pose=pose,
                    headOffset=offset,rootYawChange=Quaternion.Angle(d.transform.rotation,facing),hits=1});
                yield return Until(()=>p.State==ShibuyaFighterState.Ready && d.State==ShibuyaFighterState.Ready);
                Assert.That(visual.IsReacting,Is.False);
                Assert.That(Vector3.Distance(head.localPosition,neutralHead),Is.LessThan(.001f));
                Assert.That(Quaternion.Angle(torso.localRotation,Quaternion.identity),Is.LessThan(.001f));
            }
        }

        [UnityTest] public IEnumerator FourHitReactionsRefreshAndHeavyPoseFreezesWithLocalHitStop() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return ComboScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator ComboScenario()
        {
            Application.runInBackground=true; yield return Wait(.1f);
            var m=Object.FindFirstObjectByType<ShibuyaMatch>(); var p=m.Player; var d=m.Dummy;
            p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default;
            var visual=d.GetComponent<ShibuyaBlockVisual>(); var head=d.transform.Find("BlockVisual/Head");
            Vector3 neutral=head.localPosition;
            Place(p,new Vector3(0,.03f,-1.7f)); p.transform.rotation=Quaternion.identity;
            Place(d,new Vector3(0,.03f,0)); d.transform.rotation=Quaternion.Euler(0,180,0);
            var heavyFlags=new List<bool>(); d.HitReacted+=hit=>heavyFlags.Add(hit.IsHeavy);
            float hp=d.Health.CurrentHealth,normalPeak=0;
            Assert.That(p.TryAttack(),Is.True);
            for(int step=1;step<=3;step++) {
                yield return Until(()=>p.ConfirmedHitCount==step);
                yield return Wait(d.Profile.hitReactionStartTime+.005f);
                Assert.That(visual.IsReacting,Is.True);
                Assert.That(visual.ReactionEuler.x,Is.LessThan(-10));
                normalPeak=Mathf.Max(normalPeak,Mathf.Abs(visual.ReactionEuler.x));
                Assert.That(Mathf.Abs(visual.ReactionEuler.x),Is.LessThanOrEqualTo(d.Profile.hitReactionLeanAngle+.01f));
                yield return Until(()=>p.ComboStep==step && p.AttackElapsed>=p.CurrentStrike.Duration-.07f);
                Assert.That(p.TryAttack(),Is.True);
                yield return Until(()=>p.ComboStep==step+1);
            }
            yield return Until(()=>d.IsInHitStop && visual.ReactionEuler.magnitude>25);
            float heavyPeak=Mathf.Abs(visual.ReactionEuler.x), clock=d.CombatTime;
            Vector3 localHead=head.localPosition, world=d.transform.position; Quaternion headRotation=head.localRotation;
            Assert.That(heavyPeak,Is.GreaterThan(normalPeak*1.5f));
            int frames=0; double start=Time.unscaledTimeAsDouble;
            while(d.IsInHitStop) {
                Assert.That(d.CombatTime,Is.EqualTo(clock).Within(.0001f));
                Assert.That(Vector3.Distance(head.localPosition,localHead),Is.LessThan(.0001f));
                Assert.That(Quaternion.Angle(head.localRotation,headRotation),Is.LessThan(.001f));
                Assert.That(Vector3.Distance(d.transform.position,world),Is.LessThan(.001f));
                Assert.That(Time.timeScale,Is.EqualTo(1)); frames++; yield return null;
            }
            Assert.That(frames,Is.GreaterThan(0));
            float paused=(float)(Time.unscaledTimeAsDouble-start);
            yield return Wait(.12f);
            Assert.That(d.transform.position.z,Is.GreaterThan(world.z+.2f),"heavy knockback resumes");
            Assert.That(Mathf.Abs(visual.ReactionEuler.x),Is.LessThan(heavyPeak),"recovery advances after the pause");
            yield return Until(()=>d.State==ShibuyaFighterState.Ready);
            Assert.That(visual.IsReacting,Is.False); Assert.That(Vector3.Distance(head.localPosition,neutral),Is.LessThan(.001f));
            Assert.That(p.ConfirmedHitCount,Is.EqualTo(4)); Assert.That(d.Health.CurrentHealth,Is.EqualTo(hp-85));
            CollectionAssert.AreEqual(new[] {false,false,false,true},heavyFlags);
            Record(new Metric {scenario="four hit + local pause",normalPeak=normalPeak,heavyPeak=heavyPeak,
                pausedSeconds=paused,hits=p.ConfirmedHitCount});
        }

        [UnityTest] public IEnumerator RepeatedHitsDisableDeathFinishAndRestartRestoreVisualPose() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return LifecycleScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator LifecycleScenario()
        {
            Application.runInBackground=true; yield return Wait(.1f);
            var m=Object.FindFirstObjectByType<ShibuyaMatch>(); var p=m.Player; var d=m.Dummy;
            p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default;
            var profile=Clone(d); var visual=d.GetComponent<ShibuyaBlockVisual>();
            var torso=d.transform.Find("BlockVisual/Torso"); var head=d.transform.Find("BlockVisual/Head");
            Vector3 neutral=head.localPosition;
            for(int i=0;i<8;i++) {
                d.ApplyHitReaction((i%2==0?Vector3.right:Vector3.left)*2,.25f);
                yield return Wait(.045f);
                Assert.That(Mathf.Abs(visual.ReactionEuler.y),Is.LessThanOrEqualTo(profile.hitReactionTwistAngle+.01f));
                Assert.That(Mathf.Abs(visual.ReactionEuler.z),Is.LessThanOrEqualTo(profile.hitReactionLeanAngle+.01f));
            }
            Assert.That(visual.IsReacting,Is.True);
            profile.hitReactionEnabled=false; yield return null; yield return null;
            Assert.That(visual.IsReacting,Is.False); Assert.That(Quaternion.Angle(torso.localRotation,Quaternion.identity),Is.LessThan(.001f));
            profile.hitReactionEnabled=true; d.ApplyHitReaction(Vector3.back*2,.25f); yield return Wait(.04f);
            Assert.That(visual.IsReacting,Is.True); visual.enabled=false;
            Assert.That(visual.IsReacting,Is.False); Assert.That(Vector3.Distance(head.localPosition,neutral),Is.LessThan(.001f));
            visual.enabled=true; d.ApplyHitReaction(Vector3.back*2,.25f); p.ApplyHitReaction(Vector3.forward*2,.25f);
            yield return Wait(.04f); Assert.That(visual.IsReacting,Is.True);
            d.Health.Kill(); yield return null; yield return null;
            Assert.That(m.Finished,Is.True); Assert.That(d.State,Is.EqualTo(ShibuyaFighterState.Dead));
            Assert.That(visual.IsReacting,Is.False); Assert.That(p.GetComponent<ShibuyaBlockVisual>().IsReacting,Is.False);
            Assert.That(Vector3.Distance(head.localPosition,neutral),Is.LessThan(.001f));
            Assert.That(Quaternion.Angle(torso.localRotation,Quaternion.identity),Is.LessThan(.001f));
            m.RestartBattle(); yield return Wait(.3f);
            m=Object.FindFirstObjectByType<ShibuyaMatch>(); Assert.That(m.Finished,Is.False);
            Assert.That(m.Dummy.Health.MaxHealth,Is.EqualTo(1000)); Assert.That(m.Dummy.Health.CurrentHealth,Is.EqualTo(1000));
            foreach(var fighter in new[] {m.Player,m.Dummy}) {
                Assert.That(fighter.GetComponent<ShibuyaBlockVisual>().IsReacting,Is.False);
                Assert.That(Quaternion.Angle(fighter.transform.Find("BlockVisual/Torso").localRotation,Quaternion.identity),Is.LessThan(.001f));
            }
        }

        private static ShibuyaCombatProfile Clone(ShibuyaFighter fighter) {
            var copy=Object.Instantiate(fighter.Profile); copy.name="Shibuya PASS23 QA Profile";
            fighter.Configure(copy,AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root+"/Data/Dummy Stats.asset")); return copy;
        }
        private static void Record(Metric value) {
            System.IO.Directory.CreateDirectory(ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass23QA"));
            System.IO.File.AppendAllText(ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass23QA/metrics.jsonl"),JsonUtility.ToJson(value)+Environment.NewLine);
        }
        private static IEnumerator Wait(float seconds) { float end=Time.time+seconds; while(Time.time<end) yield return null; }
        private static IEnumerator Until(Func<bool> predicate) {
            float end=Time.realtimeSinceStartup+4; while(!predicate() && Time.realtimeSinceStartup<end) yield return null;
            Assert.That(predicate(),Is.True,"Play Mode condition timed out");
        }
        private static void Place(ShibuyaFighter fighter,Vector3 position) {
            var motor=fighter.GetComponent<CharacterController>(); motor.enabled=false; fighter.transform.position=position;
            motor.enabled=true; Physics.SyncTransforms();
        }
        [UnityTearDown] public IEnumerator Leave() { if(EditorApplication.isPlaying) yield return new ExitPlayMode(); }
        [TearDown] public void Cleanup() {
            foreach(var p in Resources.FindObjectsOfTypeAll<ShibuyaCombatProfile>())
                if(p.name=="Shibuya PASS23 QA Profile" && !AssetDatabase.Contains(p)) Object.DestroyImmediate(p);
        }
    }
}
