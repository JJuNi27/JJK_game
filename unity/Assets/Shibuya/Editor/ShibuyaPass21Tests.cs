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
    public sealed class ShibuyaPass21Tests
    {
        [Serializable] private sealed class ImpactMetrics {
            public float configuredStop, observedStop, sourceAdvance, timeScale, thirdPartyDistance, thirdPartyClockAdvance;
            public int hits, pausedFrames;
        }
        [Test]
        public void DataAndKoreanSettingsPreserveUserHealth()
        {
            var stats = AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root + "/Data/Dummy Stats.asset");
            Assert.That(stats.MaxHealth, Is.EqualTo(1000), "user-owned test HP must survive");
            var profile = AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root + "/Data/Shibuya Combat Profile.asset");
            Assert.That(profile.strikes[3].powerWindupTime, Is.InRange(0.08f, 0.12f));
            Assert.That(profile.strikes[3].hitStopDuration, Is.InRange(0.05f, 0.08f));
            Assert.That(profile.strikes[3].soundPitch, Is.LessThan(profile.strikes[0].soundPitch));
            foreach (var strike in profile.strikes) Assert.That(strike.PowerWindup, Is.LessThanOrEqualTo(strike.Startup));
            foreach (string field in new[] {"trackingEnabled", "trackingTargetRange", "trackingConeAngle", "trackingMaxTurnAngle",
                "trackingTurnSpeed", "trackingAdvanceDistance", "trackingAdvanceSpeed", "trackingDuration", "directionalInputOverrideAngle"})
                Assert.That(ShibuyaInspector.KoreanLabel(field), Is.Not.EqualTo(field));
        }

        [UnityTest] public IEnumerator StandingComboAdvancesAndHeavyHitStopsOnlyParticipants()
        {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);
            yield return new EnterPlayMode();
            yield return StandingScenario();
            yield return new ExitPlayMode();
        }
        private IEnumerator StandingScenario()
        {
            Application.runInBackground = true; yield return Wait(0.1f);
            var match = Object.FindFirstObjectByType<ShibuyaMatch>(); var p = match.Player; var d = match.Dummy;
            bool press = false;
            p.GetComponent<ShibuyaPlayerInput>().TestInput = () => new ShibuyaInputFrame { attack = press };
            Place(p, new Vector3(0, 0.03f, -1.8f)); p.transform.rotation = Quaternion.identity;
            Place(d, new Vector3(0, 0.03f, 0));
            var third = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(ShibuyaSceneBuilder.Root + "/Prefabs/ShibuyaDummy.prefab"))
                .GetComponent<ShibuyaFighter>();
            Place(third, new Vector3(10, 0.03f, 0)); third.SetIntent(Vector3.forward, false, false);
            var impactSteps = new List<int>(); var impulses = new List<float>(); var positions = new List<float>();
            p.HitConfirmed += (point, force) => { impactSteps.Add(p.ComboStep); impulses.Add(d.KnockbackVelocity.magnitude); positions.Add(p.transform.position.z); };
            float start = p.transform.position.z, hp = d.Health.CurrentHealth;
            press = true; yield return null; press = false; yield return null;
            for (int step = 1; step < 4; step++) {
                yield return Until(() => p.ComboStep == step && p.AttackElapsed >= p.CurrentStrike.Duration - 0.075f);
                Assert.That(p.ConfirmedHitCount, Is.EqualTo(step), "stationary mouse-only combo must hit each strike");
                Assert.That(p.AttackAdvanceRequested, Is.GreaterThan(0.1f));
                press = true; yield return null; press = false; yield return null;
                yield return Until(() => p.ComboStep == step + 1);
            }
            // Windup is an attacker-only presentation segment, never an enemy freeze command.
            Assert.That(p.AttackElapsed, Is.LessThan(p.CurrentStrike.PowerWindup));
            Assert.That(d.IsInHitStop, Is.False);
            yield return Until(() => p.IsInHitStop);
            Assert.That(d.IsInHitStop, Is.True); Assert.That(third.IsInHitStop, Is.False);
            Assert.That(p.HitStopCount, Is.EqualTo(1)); Assert.That(d.HitStopCount, Is.EqualTo(1));
            double began = Time.unscaledTimeAsDouble;
            float pc = p.CombatTime, dc = d.CombatTime, tc = third.CombatTime, gameTime = Time.time;
            Vector3 pp = p.transform.position, dp = d.transform.position, tp = third.transform.position;
            int frames = 0;
            while (p.IsInHitStop) {
                Assert.That(p.CombatTime, Is.EqualTo(pc).Within(0.001f));
                Assert.That(d.CombatTime, Is.EqualTo(dc).Within(0.001f));
                Assert.That(Vector3.Distance(p.transform.position, pp), Is.LessThan(0.001f));
                Assert.That(Vector3.Distance(d.transform.position, dp), Is.LessThan(0.001f));
                Assert.That(Time.timeScale, Is.EqualTo(1f));
                frames++; yield return null;
            }
            float observed = (float)(Time.unscaledTimeAsDouble - began);
            Assert.That(observed, Is.InRange(0.045f, 0.13f));
            Assert.That(frames, Is.GreaterThan(0)); Assert.That(Time.time, Is.GreaterThan(gameTime));
            Assert.That(third.CombatTime, Is.GreaterThan(tc));
            Assert.That(Vector3.Distance(third.transform.position, tp), Is.GreaterThan(0.02f));
            float thirdDistanceAtResume = Vector3.Distance(third.transform.position, tp);
            float thirdClockAtResume = third.CombatTime - tc;
            Assert.That(d.KnockbackVelocity.magnitude, Is.GreaterThan(0f), "finisher impulse is retained during pause");
            yield return Wait(0.18f);
            Assert.That(d.transform.position.z, Is.GreaterThan(dp.z + 0.2f));
            Assert.That(p.ConfirmedHitCount, Is.EqualTo(4));
            Assert.That(d.Health.CurrentHealth, Is.EqualTo(hp - 85f));
            CollectionAssert.AreEqual(new[] {1,2,3,4}, impactSteps);
            for(int i=0;i<3;i++) Assert.That(impulses[i], Is.GreaterThan(0f));
            Assert.That(impulses[3], Is.GreaterThan(impulses[0]));
            Assert.That(positions[2], Is.GreaterThan(positions[0]));
            System.IO.Directory.CreateDirectory("Logs/ShibuyaPass21QA");
            System.IO.File.WriteAllText("Logs/ShibuyaPass21QA/impact-metrics.json", JsonUtility.ToJson(new ImpactMetrics {
                configuredStop=p.Profile.strikes[3].hitStopDuration, observedStop=observed, pausedFrames=frames,
                sourceAdvance=p.transform.position.z-start, timeScale=Time.timeScale, hits=p.ConfirmedHitCount,
                thirdPartyDistance=thirdDistanceAtResume, thirdPartyClockAdvance=thirdClockAtResume }, true));
            yield return Until(() => d.State == ShibuyaFighterState.Ready);
            Assert.That(d.IsInHitStop, Is.False);
            Assert.That(d.TryAttack(), Is.True, "finisher stun ends and the victim can act again");
        }

        [UnityTest] public IEnumerator TrackingIsBoundedAndStopsAfterStartup()
        {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return TrackingScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator TrackingScenario()
        {
            Application.runInBackground=true; yield return Wait(0.1f);
            var m=Object.FindFirstObjectByType<ShibuyaMatch>(); var p=m.Player; var d=m.Dummy;
            p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default;
            var profile=Clone(p); foreach(var s in profile.strikes) s.advanceDistance=0f;
            yield return Prime(p,d);
            Place(d,new Vector3(0.45f,0.03f,p.transform.position.z+1.7f));
            Quaternion before=p.transform.rotation; float z=p.transform.position.z;
            Assert.That(p.TryAttack(),Is.True);
            yield return Until(()=>p.AttackElapsed>=profile.strikes[1].Startup+0.015f);
            Assert.That(Quaternion.Angle(before,p.transform.rotation),Is.GreaterThan(1f));
            Assert.That(p.TrackingTurnApplied,Is.LessThanOrEqualTo(profile.trackingMaxTurnAngle+0.01f));
            Assert.That(p.TrackingAdvanceRequested,Is.GreaterThan(0f));
            Assert.That(p.TrackingAdvanceRequested,Is.LessThanOrEqualTo(profile.trackingAdvanceDistance+0.001f));
            Assert.That(p.transform.position.z,Is.GreaterThan(z));
            Quaternion frozen=p.transform.rotation; Place(d,new Vector3(-0.45f,0.03f,p.transform.position.z+1.7f));
            yield return Wait(0.10f);
            Assert.That(Quaternion.Angle(frozen,p.transform.rotation),Is.LessThan(0.01f),"no active-phase homing");
            yield return Until(()=>p.State==ShibuyaFighterState.Ready);
            // PASS 2.2: opposite input cancels target assist, but not the moveset-owned lunge.
            p.ApplyHitReaction(Vector3.zero,0f); yield return Prime(p,d);
            profile.strikes[1].advanceDistance=.55f;
            Place(d,new Vector3(0.4f,0.03f,p.transform.position.z+1.7f));
            p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>new ShibuyaInputFrame { move=Vector2.down };
            before=p.transform.rotation;
            Assert.That(p.TryAttack(),Is.True); yield return Wait(0.12f);
            Assert.That(p.TrackingTurnApplied,Is.Zero); Assert.That(p.TrackingAdvanceRequested,Is.Zero);
            Assert.That(p.AttackAdvanceRequested,Is.GreaterThan(0f));
            Assert.That(Quaternion.Angle(before,p.transform.rotation),Is.LessThan(0.01f));
        }

        [UnityTest] public IEnumerator EscapesWallsAndOtherTargetsDoNotRelock()
        {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return EscapeScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator EscapeScenario()
        {
            Application.runInBackground=true; yield return Wait(0.1f);
            var m=Object.FindFirstObjectByType<ShibuyaMatch>(); var p=m.Player; var d=m.Dummy;
            p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default; var profile=Clone(p);
            foreach(var s in profile.strikes) s.advanceDistance=0f;
            for(int trial=0;trial<3;trial++) {
                p.ApplyHitReaction(Vector3.zero,0); yield return Prime(p,d);
                if(trial==0) Place(d,new Vector3(4,0.03f,p.transform.position.z+1));
                if(trial==1) Place(d,new Vector3(0,0.03f,p.transform.position.z+6));
                GameObject wall=null;
                if(trial==2) {
                    Place(d,new Vector3(.3f,.03f,p.transform.position.z+1.8f));
                    wall=GameObject.CreatePrimitive(PrimitiveType.Cube); wall.transform.position=p.transform.position+new Vector3(0,1,.8f);
                    wall.transform.localScale=new Vector3(4,3,.15f); Physics.SyncTransforms();
                }
                float hp=d.Health.CurrentHealth; int hits=p.ConfirmedHitCount;
                Assert.That(p.TryAttack(),Is.True); yield return Wait(.20f);
                Assert.That(p.TrackingTurnApplied,Is.Zero); Assert.That(p.TrackingAdvanceRequested,Is.Zero);
                Assert.That(p.ConfirmedHitCount,Is.EqualTo(hits)); Assert.That(d.Health.CurrentHealth,Is.EqualTo(hp));
                if(wall!=null) Object.Destroy(wall);
                yield return Until(()=>p.State==ShibuyaFighterState.Ready);
            }
            // PASS 3B.1: first-contact acquisition is now explicitly approved, still bounded to preparation.
            p.ApplyHitReaction(Vector3.zero,0); Place(d,new Vector3(.6f,.03f,p.transform.position.z+1.7f));
            Assert.That(p.TryAttack(),Is.True); yield return Wait(.075f);
            Assert.That(p.TrackingTurnApplied,Is.GreaterThan(0f));
            Assert.That(p.TrackingTurnApplied,Is.LessThanOrEqualTo(profile.trackingMaxTurnAngle+.01f));
            Assert.That(p.TrackingAdvanceRequested,Is.LessThanOrEqualTo(profile.trackingAdvanceDistance+.001f));
        }

        [UnityTest] public IEnumerator LocalHitStopPreservesOneBufferedSuccessor()
        {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return BufferDuringStopScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator BufferDuringStopScenario()
        {
            Application.runInBackground=true; yield return Wait(.1f);
            var m=Object.FindFirstObjectByType<ShibuyaMatch>(); var p=m.Player; var d=m.Dummy;
            p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default;
            var profile=Clone(p); profile.strikes[0].hitStopDuration=.07f;
            profile.inputBufferTime=.40f; // Runtime-only stress case: accept a press during the hit pause.
            Place(p,new Vector3(0,.03f,-1.6f)); p.transform.rotation=Quaternion.identity; Place(d,new Vector3(0,.03f,0));
            Assert.That(p.TryAttack(),Is.True); yield return Until(()=>p.IsInHitStop);
            float clock=p.CombatTime; Assert.That(p.TryAttack(),Is.True);
            for(int i=0;i<50;i++) Assert.That(p.TryAttack(),Is.False);
            Assert.That(p.HasBufferedAttack,Is.True); Assert.That(p.ConfirmedHitCount,Is.EqualTo(1));
            while(p.IsInHitStop) { Assert.That(p.HasBufferedAttack,Is.True); Assert.That(p.CombatTime,Is.EqualTo(clock).Within(.001f)); yield return null; }
            yield return Until(()=>p.ComboStep==2);
            yield return Until(()=>p.State==ShibuyaFighterState.Ready);
            Assert.That(p.AcceptedAttackCount,Is.EqualTo(2)); Assert.That(p.ConfirmedHitCount,Is.EqualTo(2));
            Assert.That(d.Health.CurrentHealth,Is.EqualTo(d.Health.MaxHealth-40));
            Assert.That(p.HitStopCount,Is.EqualTo(1)); Assert.That(Time.timeScale,Is.EqualTo(1));
        }

        [UnityTest] public IEnumerator HeavyMissAndInvulnerabilityNeverTriggerHitStop()
        {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return MissScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator MissScenario()
        {
            Application.runInBackground=true; yield return Wait(.1f);
            var m=Object.FindFirstObjectByType<ShibuyaMatch>(); var p=m.Player; var d=m.Dummy;
            p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default; var profile=Clone(p);
            foreach(var s in profile.strikes) s.advanceDistance=0;
            for(int trial=0;trial<2;trial++) {
                p.ApplyHitReaction(Vector3.zero,0); Place(p,new Vector3(0,.03f,-1.4f)); p.transform.rotation=Quaternion.identity;
                Place(d,new Vector3(10,.03f,0)); Assert.That(p.TryAttack(),Is.True);
                for(int step=1;step<4;step++) {
                    yield return Until(()=>p.ComboStep==step && p.AttackElapsed>=p.CurrentStrike.Duration-.075f);
                    Assert.That(p.TryAttack(),Is.True); yield return Until(()=>p.ComboStep==step+1);
                }
                if(trial==1) { Place(d,new Vector3(0,.03f,0)); d.Health.GrantInvulnerability(1f); }
                float hp=d.Health.CurrentHealth;
                yield return Wait(profile.strikes[3].Duration+.12f);
                Assert.That(p.HitStopCount,Is.Zero); Assert.That(d.HitStopCount,Is.Zero);
                Assert.That(d.Health.CurrentHealth,Is.EqualTo(hp));
                Assert.That(p.IsInHitStop || d.IsInHitStop,Is.False);
                Assert.That(d.State,Is.EqualTo(ShibuyaFighterState.Ready),"miss/guard must not impose stun");
                Assert.That(Time.timeScale,Is.EqualTo(1));
            }
        }

        [UnityTest] public IEnumerator WallBlocksAdvanceAndRepeatedFirstHitsLeaveActionGaps()
        {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return WallAndStunScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator WallAndStunScenario()
        {
            Application.runInBackground=true; yield return Wait(.1f);
            var m=Object.FindFirstObjectByType<ShibuyaMatch>(); var p=m.Player; var d=m.Dummy;
            p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default; var profile=Clone(p);
            Place(p,new Vector3(0,.03f,-2)); p.transform.rotation=Quaternion.identity;
            Place(d,new Vector3(0,.03f,0));
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube); wall.transform.position=new Vector3(0,1,-1.15f);
            wall.transform.localScale=new Vector3(4,3,.15f); Physics.SyncTransforms();
            Assert.That(p.TryAttack(),Is.True); yield return Wait(profile.strikes[0].Duration+.05f);
            Assert.That(p.transform.position.z,Is.LessThan(-1.5f)); Assert.That(p.ConfirmedHitCount,Is.Zero);
            Vector3 blocked=p.transform.position; Object.Destroy(wall); yield return Wait(.1f);
            Assert.That(Vector3.Distance(blocked,p.transform.position),Is.LessThan(.03f),"no banked wall displacement");
            float priorHit=-100; int original=p.ConfirmedHitCount;
            for(int i=0;i<3;i++) {
                yield return Wait(profile.comboResetTime+.02f);
                Place(p,new Vector3(0,.03f,-1.4f)); p.transform.rotation=Quaternion.identity; Place(d,new Vector3(0,.03f,0));
                Assert.That(p.TryAttack(),Is.True); Assert.That(p.ComboStep,Is.EqualTo(1));
                yield return Until(()=>p.ConfirmedHitCount==original+i+1);
                Assert.That(Time.time-priorHit,Is.GreaterThan(profile.strikes[0].hitStun)); priorHit=Time.time;
                Assert.That(d.TryAttack(),Is.False,"instant stun rejects actions");
                yield return Until(()=>d.State==ShibuyaFighterState.Ready);
                Assert.That(p.State,Is.EqualTo(ShibuyaFighterState.Attacking),"target regains action before the attacker can repeat");
                Assert.That(d.TryAttack(),Is.True,"real action window exists");
                d.ApplyHitReaction(Vector3.zero,0); // End the test-only retaliation, not a gameplay escape feature.
                yield return Until(()=>p.State==ShibuyaFighterState.Ready);
            }
        }

        private static IEnumerator Prime(ShibuyaFighter p,ShibuyaFighter d)
        {
            Place(p,new Vector3(0,.03f,-1.4f)); p.transform.rotation=Quaternion.identity; Place(d,new Vector3(0,.03f,0));
            int hits=p.ConfirmedHitCount; Assert.That(p.TryAttack(),Is.True);
            yield return Until(()=>p.ConfirmedHitCount>hits); yield return Until(()=>p.State==ShibuyaFighterState.Ready);
        }
        private static ShibuyaCombatProfile Clone(ShibuyaFighter p)
        {
            var profile=Object.Instantiate(p.Profile); profile.name="Shibuya PASS21 QA Profile";
            p.Configure(profile,AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root+"/Data/Player Stats.asset"));
            return profile;
        }
        private static IEnumerator Wait(float t) { float end=Time.time+t; while(Time.time<end) yield return null; }
        private static IEnumerator Until(Func<bool> predicate) {
            float end=Time.realtimeSinceStartup+3f; while(!predicate() && Time.realtimeSinceStartup<end) yield return null;
            Assert.That(predicate(),Is.True,"Play Mode condition timed out");
        }
        private static void Place(ShibuyaFighter p,Vector3 position) {
            var c=p.GetComponent<CharacterController>(); c.enabled=false; p.transform.position=position; c.enabled=true; Physics.SyncTransforms();
        }
        [UnityTearDown] public IEnumerator Leave() { if(EditorApplication.isPlaying) yield return new ExitPlayMode(); }
        [TearDown] public void Cleanup() {
            foreach(var p in Resources.FindObjectsOfTypeAll<ShibuyaCombatProfile>())
                if(p.name=="Shibuya PASS21 QA Profile" && !AssetDatabase.Contains(p)) Object.DestroyImmediate(p);
        }
    }
}
