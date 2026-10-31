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
    public sealed class ShibuyaPass3BTests
    {
        [Test] public void BranchDataKoreanInspectorUndoAndProtectedNumbers()
        {
            var p = AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root + "/Data/Shibuya Combat Profile.asset");
            Assert.That(p.finishers != null && p.finishers.IsValid);
            CollectionAssert.AreEqual(new[] {20f,20f,20f,25f}, Array.ConvertAll(p.strikes, s => s.damage));
            CollectionAssert.AreEqual(new[] {.1f,.09f,.1f,.18f}, Array.ConvertAll(p.strikes, s => s.startup));
            CollectionAssert.AreEqual(new[] {6f,4f,4f,11f}, Array.ConvertAll(p.strikes, s => s.knockbackSpeed));
            Assert.That(p.strikes[3].hitStopDuration, Is.EqualTo(.065f));
            Assert.That(p.confirmedLinkMaxHold, Is.EqualTo(.75f)); Assert.That(p.confirmedLinkMaxSequence, Is.EqualTo(3));
            Assert.That(AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root + "/Data/Dummy Stats.asset").MaxHealth, Is.EqualTo(1000));
            Assert.That(AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root + "/Data/Player Stats.asset").MaxHealth, Is.EqualTo(100));
            foreach (var type in new[] { typeof(ShibuyaAirSettings), typeof(ShibuyaDownSettings), typeof(ShibuyaFinisherSet) })
                foreach (var field in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                    Assert.That(ShibuyaInspector.KoreanLabel(field.Name), Is.Not.EqualTo(field.Name));
            var copy = Object.Instantiate(p.finishers); copy.name = "PASS3B QA Finisher";
            try {
                var so = new SerializedObject(copy);
                var speed = so.FindProperty("uppercut").FindPropertyRelative("hits").GetArrayElementAtIndex(0)
                    .FindPropertyRelative("air").FindPropertyRelative("launchSpeed");
                float before = speed.floatValue; Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
                speed.floatValue = 8; so.ApplyModifiedProperties(); Undo.FlushUndoRecordObjects();
                Assert.That(copy.uppercut.hits[0].air.launchSpeed, Is.EqualTo(8));
                Undo.CollapseUndoOperations(group); Undo.PerformUndo();
                Assert.That(copy.uppercut.hits[0].air.launchSpeed, Is.EqualTo(before));
            } finally { Object.DestroyImmediate(copy); }
        }

        [UnityTest] public IEnumerator NeutralFourClicksKeepVerifiedFinisher() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return CommandScenario(0,0,false,false); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator WHeldFromFirstClickSelectsUppercut() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return CommandScenario(1,1,false,false); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator SHeldFromFirstClickSelectsDown() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return CommandScenario(-1,-1,false,false); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator FourthWThenReleaseRetainsBufferedUppercut() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return CommandScenario(0,1,true,false); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator FourthSThenWRetainsBufferedDown() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return CommandScenario(0,-1,true,false); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator SimultaneousWSFallsBackToNormal() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return CommandScenario(2,2,false,false); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator RawWCommandIsIndependentOfWorldMoveAndLook() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return CommandScenario(0,1,true,true); yield return new ExitPlayMode();
        }
        private IEnumerator CommandScenario(int initial, int fourth, bool changeAfter, bool conflictingAxes)
        {
            yield return Setup(); var m = Object.FindFirstObjectByType<ShibuyaMatch>(); var p = m.Player; var d = m.Dummy;
            var input = p.GetComponent<ShibuyaPlayerInput>(); var frame = Frame(initial);
            input.TestInput = () => frame;
            frame.attack = true; yield return null; yield return null; frame.attack = false;
            yield return Until(() => p.ComboStep == 1);
            for (int stage = 1; stage <= 3; stage++) {
                yield return Until(() => p.ComboStep == stage && p.AttackElapsed >= p.AttackDuration - .12f);
                Assert.That(p.CurrentFinisher, Is.EqualTo(ShibuyaFinisher.Default));
                Assert.That(p.CurrentStrike.motion, Is.EqualTo(p.Profile.strikes[stage-1].motion));
                frame = Frame(stage == 3 ? fourth : initial);
                if (stage == 3 && conflictingAxes) { frame.move = Vector2.down; frame.look = new Vector2(8,0); }
                frame.attack = true; yield return null; yield return null; frame.attack = false;
                Assert.That(p.HasBufferedAttack, Is.True);
                if (stage == 3) {
                    var expected = fourth == 1 ? ShibuyaFinisher.Uppercut : fourth == -1 ? ShibuyaFinisher.Down : ShibuyaFinisher.Default;
                    Assert.That(p.BufferedFinisher, Is.EqualTo(expected));
                    if (changeAfter) frame = Frame(fourth == -1 ? 1 : 0);
                    yield return Until(() => p.ComboStep == 4);
                    Assert.That(p.CurrentFinisher, Is.EqualTo(expected));
                    if (expected == ShibuyaFinisher.Default) Assert.That(p.CurrentStrike, Is.SameAs(p.Profile.strikes[3]));
                } else yield return Until(() => p.ComboStep == stage + 1);
            }
            yield return Until(() => p.ConfirmedHitCount == 4);
            Assert.That(d.Health.CurrentHealth, Is.EqualTo(915));
            Assert.That(p.AcceptedAttackCount, Is.EqualTo(4));
            if (fourth == 1) Assert.That(d.IsAirborne);
            else if (fourth == -1) Assert.That(d.IsDowned);
            else { Assert.That(!d.IsAirborne && !d.IsDowned); Assert.That(d.KnockbackVelocity.magnitude, Is.GreaterThan(9)); }
            frame = default; yield return Wait(.8f);
            Assert.That(p.AcceptedAttackCount, Is.EqualTo(4), "no held-button auto chain");
            Assert.That(p.CurrentFinisher, Is.EqualTo(ShibuyaFinisher.Default));
            Record("command", $"initial={initial},fourth={fourth},changedAfter={changeAfter},hits={p.ConfirmedHitCount},damage=85");
        }
        private static ShibuyaInputFrame Frame(int command) => new ShibuyaInputFrame {
            forwardKey = command == 1 || command == 2, backwardKey = command == -1 || command == 2,
            move = new Vector2(0, command == 2 ? 0 : command)
        };

        [UnityTest] public IEnumerator CustomMovesetCanAuthorDifferentFinalStep() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return CustomScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator CustomScenario()
        {
            yield return Setup(); var m=Object.FindFirstObjectByType<ShibuyaMatch>();var p=m.Player;var d=m.Dummy;
            var data=ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();data.name="PASS3B QA Moveset";
            data.finishers=Object.Instantiate(p.Profile.finishers);data.finishers.name="PASS3B QA Finisher";
            var authored=data.finishers.uppercut.hits[0];authored.strike.damage=31;authored.strike.startup=.16f;authored.air.launchSpeed=7;authored.air.usePeakHeight=false;
            data.steps=new ShibuyaMovesetStep[4];
            for(int i=0;i<4;i++) {
                var strike=JsonUtility.FromJson<ShibuyaStrikeDefinition>(JsonUtility.ToJson(p.Profile.strikes[i]));
                data.steps[i]=new ShibuyaMovesetStep { hits=new[]{new ShibuyaHitEvent {strike=strike,heavyReaction=i==3}},
                    movements=new[]{new ShibuyaMovementDefinition {kind=ShibuyaMovementKind.Forward,
                        startTime=strike.advanceStartTime,duration=strike.advanceDuration,maxDistance=strike.advanceDistance,speed=strike.advanceSpeed}}
                };
            }
            Assert.That(p.SetMoveset(data));Assert.That(p.TryAttack());
            for(int stage=1;stage<=3;stage++) {
                yield return Until(()=>p.ComboStep==stage&&p.AttackElapsed>=p.AttackDuration-.1f);
                Assert.That(p.TryAttack(ShibuyaFinisher.Uppercut));yield return Until(()=>p.ComboStep==stage+1);
            }
            Assert.That(p.CurrentStrike.damage,Is.EqualTo(31));Assert.That(p.CurrentStrike.Startup,Is.EqualTo(.16f));
            yield return Until(()=>p.ConfirmedHitCount==4);
            Assert.That(d.Health.CurrentHealth,Is.EqualTo(909));Assert.That(d.IsAirborne);
            Assert.That(d.VerticalVelocity,Is.EqualTo(7).Within(.001f));
            Assert.That(p.Profile.finishers.uppercut.hits[0].strike.damage,Is.EqualTo(25));
            Record("custom", "4 input stages, damage=91, authored startup=.16, launchSpeed=7; default data intact");
        }

        [UnityTest] public IEnumerator BufferedBranchCancelledByHitCannotLeakIntoNextCombo() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return CancelScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator CancelScenario()
        {
            yield return Setup();var m=Object.FindFirstObjectByType<ShibuyaMatch>();var p=m.Player;var d=m.Dummy;
            Assert.That(p.TryAttack());
            for(int stage=1;stage<=2;stage++) {
                yield return Until(()=>p.ComboStep==stage&&p.AttackElapsed>=p.AttackDuration-.1f);
                Assert.That(p.TryAttack());yield return Until(()=>p.ComboStep==stage+1);
            }
            yield return Until(()=>p.AttackElapsed>=p.AttackDuration-.12f);
            Assert.That(p.TryAttack(ShibuyaFinisher.Uppercut));Assert.That(p.HasBufferedAttack);
            p.ApplyHitReaction(Vector3.zero,.12f);
            Assert.That(p.HasBufferedAttack,Is.False);Assert.That(p.CurrentFinisher,Is.EqualTo(ShibuyaFinisher.Default));
            yield return Until(()=>p.State==ShibuyaFighterState.Ready);
            Assert.That(p.TryAttack(ShibuyaFinisher.Down));Assert.That(p.ComboStep,Is.EqualTo(1));
            Assert.That(p.CurrentFinisher,Is.EqualTo(ShibuyaFinisher.Default));
            Assert.That(!d.IsAirborne&&!d.IsDowned);
        }

        [UnityTest] public IEnumerator UppercutRisesFallsLandsAndLocksAirActions() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return AirScenario(false); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator UppercutCeilingAndWallUseRealControllerCollision() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return AirScenario(true); yield return new ExitPlayMode();
        }
        private IEnumerator AirScenario(bool obstacles)
        {
            yield return Setup(); var m=Object.FindFirstObjectByType<ShibuyaMatch>(); var p=m.Player; var d=m.Dummy;
            var data = Single(p, ShibuyaFinisher.Uppercut);
            if (obstacles) {
                Cube("PASS3B QA Ceiling", new Vector3(0,3,0),new Vector3(6,.3f,6));
                Cube("PASS3B QA Wall", new Vector3(0,1,1.5f),new Vector3(5,4,.2f));
                data.steps[0].hits[0].strike.knockbackSpeed=12;
            }
            Vector3 start=d.transform.position;
            Assert.That(p.TryAttack()); yield return Until(()=>d.IsAirborne);
            Assert.That(d.IsInHitStop); float clock=d.CombatTime; Vector3 frozen=d.transform.position;
            yield return Wait(.025f); Assert.That(d.CombatTime,Is.EqualTo(clock)); Assert.That(d.transform.position,Is.EqualTo(frozen));
            float peak=start.y, end=Time.realtimeSinceStartup+4; bool rose=false;
            while(d.IsAirborne && Time.realtimeSinceStartup<end) {
                peak=Mathf.Max(peak,d.transform.position.y); rose|=d.transform.position.y>start.y+.1f;
                d.SetIntent(Vector3.right,true,true); Assert.That(d.TryAttack(),Is.False);
                yield return null;
            }
            Assert.That(rose); Assert.That(d.IsAirborne,Is.False); Assert.That(d.IsGrounded);
            Assert.That(d.transform.position.y,Is.GreaterThan(-.1f));
            Assert.That(Mathf.Abs(d.transform.position.x-start.x),Is.LessThan(.04f));
            if(obstacles) { Assert.That(peak-start.y,Is.LessThan(1.1f)); Assert.That(d.transform.position.z,Is.LessThan(1.3f)); }
            else Assert.That(peak-start.y,Is.GreaterThan(1.5f));
            d.SetIntent(Vector3.zero,false,false); yield return Until(()=>d.State==ShibuyaFighterState.Ready);
            Assert.That(d.TryAttack()); Assert.That(Time.timeScale,Is.EqualTo(1));
            Record("air",$"obstacles={obstacles},peakHeight={peak-start.y},landed={d.IsGrounded},position={d.transform.position}");
        }

        [UnityTest] public IEnumerator DownHasMinimumNaturalRecoveryPoseAndActionLock() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return DownScenario(false); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator RepeatedDownHitDoesNotRestartWakeTimer() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return DownScenario(true); yield return new ExitPlayMode();
        }
        private IEnumerator DownScenario(bool repeat)
        {
            yield return Setup(); var m=Object.FindFirstObjectByType<ShibuyaMatch>();var p=m.Player;var d=m.Dummy;
            var data=Single(p,ShibuyaFinisher.Down);
            if(repeat) {
                var next=JsonUtility.FromJson<ShibuyaHitEvent>(JsonUtility.ToJson(data.steps[0].hits[0])); next.startTime=.3f;
                data.steps[0].hits=new[]{data.steps[0].hits[0],next};
            }
            Assert.That(p.TryAttack()); yield return Until(()=>d.IsDowned);
            float begin=d.CombatTime; Vector3 start=d.transform.position;
            yield return null;
            Assert.That(Quaternion.Angle(d.transform.Find("BlockVisual").localRotation,Quaternion.identity),Is.GreaterThan(60));
            float deadline=Time.realtimeSinceStartup+4; bool minimumObserved=false;
            while(!d.IsGettingUp && Time.realtimeSinceStartup<deadline) {
                float age=d.CombatTime-begin;
                if(age<.43f) Assert.That(d.CanRequestDownRecovery,Is.False);
                if(age>.5f) { Assert.That(d.CanRequestDownRecovery); minimumObserved=true; }
                Assert.That(d.TryAttack(),Is.False); d.SetIntent(Vector3.right,true,true);
                yield return null;
            }
            float wakeAge=d.CombatTime-begin;
            Assert.That(d.IsGettingUp); Assert.That(minimumObserved);
            Assert.That(wakeAge,Is.InRange(.84f,.94f));
            Assert.That(Mathf.Abs(d.transform.position.x-start.x),Is.LessThan(.04f));
            Assert.That(d.transform.position.y,Is.LessThan(.1f));
            d.SetIntent(Vector3.zero,false,false);
            yield return Until(()=>!d.IsDowned); yield return null;
            Assert.That(d.State,Is.EqualTo(ShibuyaFighterState.Ready));
            Assert.That(Quaternion.Angle(d.transform.Find("BlockVisual").localRotation,Quaternion.identity),Is.LessThan(.1f));
            Assert.That(p.ConfirmedHitCount,Is.EqualTo(repeat?2:1));
            Record("down",$"repeat={repeat},wakeAge={wakeAge},recoveredAt={d.CombatTime-begin}");
        }

        [UnityTest] public IEnumerator DownOnAirborneTargetFallsWithoutTeleportingThroughFloor() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return AirDownScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator AirDownScenario()
        {
            yield return Setup(); var m=Object.FindFirstObjectByType<ShibuyaMatch>();var p=m.Player;var d=m.Dummy;
            var data=Single(p,ShibuyaFinisher.Uppercut);
            var next=JsonUtility.FromJson<ShibuyaHitEvent>(JsonUtility.ToJson(p.Profile.finishers.downFinisher.hits[0]));
            next.startTime=.3f; next.strike.attackHeight=3; next.strike.attackRadius=1.5f;
            data.steps[0].hits=new[]{data.steps[0].hits[0],next};
            Assert.That(p.TryAttack()); yield return Until(()=>d.IsDowned);
            Assert.That(d.transform.position.y,Is.GreaterThan(.3f)); Assert.That(d.IsAirborne,Is.False);
            Assert.That(d.VerticalVelocity,Is.LessThan(0));
            float end=Time.realtimeSinceStartup+3;
            while(!d.IsGrounded && Time.realtimeSinceStartup<end) {
                Assert.That(d.transform.position.y,Is.GreaterThan(-.1f)); Assert.That(d.CanRequestDownRecovery,Is.False);
                yield return null;
            }
            Assert.That(d.IsGrounded); yield return Wait(.3f); Assert.That(d.CanRequestDownRecovery,Is.False);
            yield return Until(()=>d.State==ShibuyaFighterState.Ready); Assert.That(p.ConfirmedHitCount,Is.EqualTo(2));
        }

        [UnityTest] public IEnumerator MissAndImmunityNeverApplyLaunchOrDown() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return MissScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator MissScenario()
        {
            yield return Setup(); var m=Object.FindFirstObjectByType<ShibuyaMatch>();var p=m.Player;var d=m.Dummy;
            foreach(var branch in new[]{ShibuyaFinisher.Uppercut,ShibuyaFinisher.Down}) {
                Single(p,branch); Place(p,new Vector3(0,.03f,-1.4f)); Place(d,new Vector3(15,.03f,0));
                Assert.That(p.TryAttack()); yield return Wait(.8f);
                Assert.That(!d.IsAirborne&&!d.IsDowned); Assert.That(p.ConfirmedHitCount,Is.EqualTo(0));
                Place(p,new Vector3(0,.03f,-1.4f)); Place(d,new Vector3(0,.03f,0));
                d.Health.GrantInvulnerability(1); Assert.That(p.TryAttack()); yield return Wait(1.1f);
                Assert.That(!d.IsAirborne&&!d.IsDowned); Assert.That(d.Health.CurrentHealth,Is.EqualTo(1000));
            }
        }

        [UnityTest] public IEnumerator DeathInAirAndRestartClearState() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return LifecycleScenario(ShibuyaFinisher.Uppercut,true); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator DeathWhileDownAndRestartClearState() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return LifecycleScenario(ShibuyaFinisher.Down,true); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator BattleEndInAirAndRestartClearState() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return LifecycleScenario(ShibuyaFinisher.Uppercut,false); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator BattleEndWhileDownAndRestartClearState() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return LifecycleScenario(ShibuyaFinisher.Down,false); yield return new ExitPlayMode();
        }
        private IEnumerator LifecycleScenario(ShibuyaFinisher branch,bool victimDeath)
        {
            yield return Setup(); var m=Object.FindFirstObjectByType<ShibuyaMatch>(); var p=m.Player;var d=m.Dummy;
            Single(p,branch); Assert.That(p.TryAttack()); yield return Until(()=>d.IsAirborne||d.IsDowned);
            if(victimDeath) d.Health.Kill(); else p.Health.Kill();
            yield return null;
            Assert.That(m.Finished); Assert.That(!d.IsAirborne&&!d.IsDowned&&!d.IsGettingUp);
            Assert.That(p.HasBufferedAttack,Is.False); Assert.That(Time.timeScale,Is.EqualTo(1));
            m.RestartBattle(); yield return Until(()=>Object.FindFirstObjectByType<ShibuyaMatch>()!=m);
            var fresh=Object.FindFirstObjectByType<ShibuyaMatch>();
            fresh.Player.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default;
            Assert.That(fresh.Dummy.Health.CurrentHealth,Is.EqualTo(1000));
            Assert.That(fresh.Player.Health.CurrentHealth,Is.EqualTo(100));
            Assert.That(!fresh.Dummy.IsDowned&&!fresh.Dummy.IsAirborne);
        }

        private static ShibuyaMovesetDefinition Single(ShibuyaFighter fighter,ShibuyaFinisher branch)
        {
            var data=ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>(); data.name="PASS3B QA Moveset";
            var step=branch==ShibuyaFinisher.Uppercut?fighter.Profile.finishers.uppercut:fighter.Profile.finishers.downFinisher;
            data.steps=new[]{JsonUtility.FromJson<ShibuyaMovesetStep>(JsonUtility.ToJson(step))};
            Assert.That(fighter.SetMoveset(data)); return data;
        }
        private static IEnumerator Setup() {
            Application.runInBackground=true; yield return Wait(.1f);
            var m=Object.FindFirstObjectByType<ShibuyaMatch>();m.Player.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default;
            // These legacy scenarios verify base damage, not the independent random grade layer.
            m.Player.SetCombatRandom(new ShibuyaGradeTestRandom());m.Dummy.SetCombatRandom(new ShibuyaGradeTestRandom());
            Place(m.Player,new Vector3(0,.03f,-1.4f));Place(m.Dummy,new Vector3(0,.03f,0));m.Player.transform.rotation=Quaternion.identity;
        }
        private static void Cube(string name,Vector3 position,Vector3 scale) {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.position=position;go.transform.localScale=scale;Physics.SyncTransforms();
        }
        private static void Record(string name,string detail) {
            System.IO.Directory.CreateDirectory(ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass3BQA"));System.IO.File.AppendAllText(ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass3BQA/metrics.txt"),name+": "+detail+Environment.NewLine);
        }
        private static IEnumerator Wait(float seconds) { float end=Time.time+seconds;while(Time.time<end)yield return null; }
        private static IEnumerator Until(Func<bool> predicate) {
            float end=Time.realtimeSinceStartup+5;while(!predicate()&&Time.realtimeSinceStartup<end)yield return null;
            Assert.That(predicate(),Is.True,"Play Mode condition timed out");
        }
        private static void Place(ShibuyaFighter f,Vector3 pos) { var cc=f.GetComponent<CharacterController>();cc.enabled=false;f.transform.position=pos;cc.enabled=true;Physics.SyncTransforms(); }
        [UnityTearDown] public IEnumerator Leave() { if(EditorApplication.isPlaying)yield return new ExitPlayMode(); }
        [TearDown] public void Cleanup() {
            foreach(var data in Resources.FindObjectsOfTypeAll<ShibuyaMovesetDefinition>())
                if(data.name=="PASS3B QA Moveset"&&!AssetDatabase.Contains(data)) Object.DestroyImmediate(data);
            foreach(var data in Resources.FindObjectsOfTypeAll<ShibuyaFinisherSet>())
                if(data.name=="PASS3B QA Finisher"&&!AssetDatabase.Contains(data)) Object.DestroyImmediate(data);
        }
    }
}
