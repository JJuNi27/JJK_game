using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass3ATests
    {
        [Test] public void DataReferencesKoreanInspectorAndUndo()
        {
            var a = AssetDatabase.LoadAssetAtPath<ShibuyaMovesetDefinition>(ShibuyaMovesetExamples.MultiPath);
            var b = AssetDatabase.LoadAssetAtPath<ShibuyaMovesetDefinition>(ShibuyaMovesetExamples.ApproachPath);
            Assert.That(a != null && a.IsValid && b != null && b.IsValid);
            foreach (Type type in new[] { typeof(ShibuyaHitEvent), typeof(ShibuyaMovementDefinition), typeof(ShibuyaMovesetStep) })
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                    Assert.That(ShibuyaInspector.KoreanLabel(field.Name), Is.Not.EqualTo(field.Name));
            foreach (string field in new[] { "moveset", "confirmedLinkEnabled", "confirmedLinkInputWindow", "confirmedLinkMaxHold", "confirmedLinkMaxSequence" })
                Assert.That(ShibuyaInspector.KoreanLabel(field), Is.Not.EqualTo(field));
            var copy = Object.Instantiate(a); copy.name = "PASS3A QA Moveset";
            try {
                var so = new SerializedObject(copy);
                var damage = so.FindProperty("steps").GetArrayElementAtIndex(0).FindPropertyRelative("hits")
                    .GetArrayElementAtIndex(1).FindPropertyRelative("strike").FindPropertyRelative("damage");
                float before = damage.floatValue;
                Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
                damage.floatValue = 19; so.ApplyModifiedProperties(); Undo.FlushUndoRecordObjects();
                Assert.That(copy.steps[0].hits[1].strike.damage, Is.EqualTo(19));
                Undo.CollapseUndoOperations(group); Undo.PerformUndo();
                Assert.That(copy.steps[0].hits[1].strike.damage, Is.EqualTo(before));
                copy.steps[0].hits[1].requiredMovement = 99;
                Assert.That(copy.IsValid, Is.False);
            } finally { Object.DestroyImmediate(copy); }
            Assert.That(AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root + "/Data/Dummy Stats.asset").MaxHealth, Is.EqualTo(1000));
            Assert.That(AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root + "/Data/Player Stats.asset").MaxHealth, Is.EqualTo(100));
        }

        [UnityTest] public IEnumerator OneInputTwoIndependentEventsAndColliderDedup() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return MultiScenario(false, false); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator LowFrameRateVisitsEventsInOrderExactlyOnce() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return MultiScenario(true, false); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator LocalHitStopDefersRemainingEventAndPreservesBuffer() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return MultiScenario(false, true); yield return new ExitPlayMode();
        }
        private IEnumerator MultiScenario(bool lowFps, bool stop)
        {
            yield return Setup(); var m = Object.FindFirstObjectByType<ShibuyaMatch>(); var p = m.Player; var d = m.Dummy;
            var data = ShibuyaMovesetExamples.Multi(); data.name = "PASS3A QA Moveset";
            if (stop) {
                data.steps[0].hits[0].strike.hitStopDuration = .07f;
                data.steps[0].hits[1].strike.hitStopDuration = .07f;
                data.steps[0].hits[1].strike.recovery = .11f;
                Array.Resize(ref data.steps, 2); data.steps[1] = new ShibuyaMovesetStep();
            }
            Assert.That(p.SetMoveset(data));
            d.gameObject.AddComponent<BoxCollider>().center = Vector3.up;
            float hp = d.Health.CurrentHealth;
            var order = new List<int>(); var values = new List<float>(); var times = new List<float>();
            p.HitEventStarted += i => order.Add(i);
            p.HitConfirmed += (pos, intensity) => { values.Add(d.Health.CurrentHealth); times.Add(p.AttackElapsed); };
            Assert.That(p.TryAttack());
            if (lowFps) {
                // Cross both complete active windows using the real scheduler, without changing global time.
                typeof(ShibuyaFighter).GetField("attackStartedAt", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(p, p.CombatTime - .5f);
                typeof(ShibuyaFighter).GetMethod("UpdateAttack", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(p, null);
            }
            yield return Until(() => p.ConfirmedHitCount == 2);
            CollectionAssert.AreEqual(new[] { 0, 1 }, order);
            CollectionAssert.AreEqual(new[] { hp - 7, hp - 18 }, values);
            Assert.That(p.AcceptedAttackCount, Is.EqualTo(1));
            if (!lowFps) Assert.That(times[1] - times[0], Is.GreaterThan(.16f));
            if (stop) {
                Assert.That(p.IsInHitStop);
                float clock = p.CombatTime;
                Assert.That(p.TryAttack(), Is.True);
                yield return Wait(.025f);
                Assert.That(p.CombatTime, Is.EqualTo(clock)); Assert.That(p.HasBufferedAttack);
                Assert.That(Time.timeScale, Is.EqualTo(1));
                yield return Until(() => p.AcceptedAttackCount == 2);
            } else { yield return Wait(.2f); Assert.That(p.ConfirmedHitCount, Is.EqualTo(2)); }
            Record("multi", $"lowFps={lowFps}, stop={stop}, damage={hp - values[1]}, order={string.Join(",", order)}");
        }

        [UnityTest] public IEnumerator EarlySuccessorLocksOnlyTheConfirmedLinkAndReleases() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return LinkScenario(false); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator LateInputNeverRelocksARecoveredDefender() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return LinkScenario(true); yield return new ExitPlayMode();
        }
        private IEnumerator LinkScenario(bool late)
        {
            yield return Setup(); var m = Object.FindFirstObjectByType<ShibuyaMatch>(); var p = m.Player; var d = m.Dummy;
            Assert.That(p.TryAttack()); yield return Until(() => p.ConfirmedHitCount == 1);
            yield return Until(() => p.AttackElapsed >= (late ? .37f : .22f));
            if (late) Assert.That(d.State, Is.EqualTo(ShibuyaFighterState.Ready));
            Assert.That(p.TryAttack());
            if (late) { Assert.That(d.HasConfirmedLink, Is.False); Assert.That(d.TryAttack(), Is.True); }
            else {
                Assert.That(d.HasConfirmedLink);
                yield return Until(() => p.ComboStep == 2);
                Assert.That(d.State, Is.EqualTo(ShibuyaFighterState.Hitstunned));
                Assert.That(d.TryAttack(), Is.False);
                float deadline = Time.realtimeSinceStartup + 3;
                while (p.ConfirmedHitCount < 2 && Time.realtimeSinceStartup < deadline) {
                    Assert.That(d.State, Is.EqualTo(ShibuyaFighterState.Hitstunned), "timely reserved successor stays connected");
                    yield return null;
                }
                Assert.That(p.ConfirmedHitCount, Is.EqualTo(2));
                yield return Until(() => d.State == ShibuyaFighterState.Ready);
                Assert.That(d.HasConfirmedLink, Is.False);
                Assert.That(d.TryAttack(), Is.True);
            }
        }

        [UnityTest] public IEnumerator NormalFourClickCadenceStaysConfirmedAndFinisherReleases() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return FourClickScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator FourClickScenario()
        {
            yield return Setup(); var m = Object.FindFirstObjectByType<ShibuyaMatch>(); var p = m.Player; var d = m.Dummy;
            float hp = d.Health.CurrentHealth;
            Assert.That(p.TryAttack());
            for (int stage = 1; stage <= 3; stage++) {
                yield return Until(() => p.ComboStep == stage && p.ConfirmedHitCount == stage && p.AttackElapsed >= .18f);
                Assert.That(p.TryAttack(), Is.True);
                Assert.That(d.HasConfirmedLink, Is.True);
                int next = stage + 1; float end = Time.realtimeSinceStartup + 2;
                while (p.ConfirmedHitCount < next && Time.realtimeSinceStartup < end) {
                    Assert.That(d.State, Is.EqualTo(ShibuyaFighterState.Hitstunned));
                    Assert.That(d.TryAttack(), Is.False);
                    yield return null;
                }
                Assert.That(p.ConfirmedHitCount, Is.EqualTo(next));
            }
            Assert.That(d.Health.CurrentHealth, Is.EqualTo(hp - 85));
            yield return Until(() => d.State == ShibuyaFighterState.Ready);
            Assert.That(d.HasConfirmedLink, Is.False); Assert.That(d.TryAttack());
            Record("default four", "damage=85, 4 accepted inputs, no actionable gap after on-time reservation, released after finisher");
        }

        [UnityTest] public IEnumerator AttackerDeathCancelsMultiHitAndReleasesVictim() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return DeathScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator DeathScenario()
        {
            yield return Setup(); var m = Object.FindFirstObjectByType<ShibuyaMatch>(); var p = m.Player; var d = m.Dummy;
            var data = ShibuyaMovesetExamples.Multi(); data.name = "PASS3A QA Moveset"; p.SetMoveset(data);
            Assert.That(p.TryAttack()); yield return Until(() => p.ConfirmedHitCount == 1);
            p.Health.ReceiveDamage(new DamageContext(10000, d.gameObject, DamageDeliveryType.PhysicalStrike,
                DamageTraits.None, "PASS3A QA", p.transform.position));
            yield return Wait(.5f);
            Assert.That(p.Health.IsDead); Assert.That(p.ConfirmedHitCount, Is.EqualTo(1));
            Assert.That(d.HasConfirmedLink, Is.False); Assert.That(m.Finished);
        }

        [UnityTest] public IEnumerator AuthoredForwardAndKnockbackBudgetsAreIndependentAndCollisionBounded() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return BudgetScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator BudgetScenario()
        {
            yield return Setup(); var m = Object.FindFirstObjectByType<ShibuyaMatch>(); var p = m.Player; var d = m.Dummy;
            var data = ShibuyaMovesetExamples.Approach(); data.name = "PASS3A QA Moveset";
            data.steps = new[] { data.steps[0] };
            data.steps[0].movements = new[] { new ShibuyaMovementDefinition {
                kind = ShibuyaMovementKind.Forward, maxDistance = .3f, speed = 2, duration = .2f
            } };
            // Measure only authored forward motion; optional automatic correction has its own budget.
            foreach(var step in data.steps) foreach(var hit in step.hits) {
                hit.strike.overrideTargeting=true;hit.strike.targeting.approachDistance=0;hit.strike.targeting.maxTurnAngle=0;
            }
            p.SetMoveset(data);
            Vector3 p0 = p.transform.position, d0 = d.transform.position;
            Assert.That(p.TryAttack()); yield return Wait(.7f);
            Assert.That(p.transform.position.z - p0.z, Is.EqualTo(.3f).Within(.03f));
            Assert.That(d.transform.position.z - d0.z, Is.EqualTo(4).Within(.06f));
            yield return Until(() => d.State == ShibuyaFighterState.Ready);
            Place(p, p0); Place(d, d0);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.name = "PASS3A QA Wall";
            wall.transform.position = d0 + new Vector3(0, 1, 1.5f); wall.transform.localScale = new Vector3(5, 3, .2f);
            Physics.SyncTransforms();
            Assert.That(p.TryAttack()); yield return Wait(.7f);
            Assert.That(d.transform.position.z - d0.z, Is.LessThan(1.3f));
            Object.Destroy(wall); yield return Wait(.2f);
            float stoppedAt = d.transform.position.z; yield return Wait(.3f);
            Assert.That(d.transform.position.z, Is.EqualTo(stoppedAt).Within(.02f), "blocked distance is spent, never stored");
            Record("independent budgets", "attacker=.3m, victim=4m unobstructed; wall reduces actual victim travel and no banked burst");
        }

        [UnityTest] public IEnumerator ConfirmedHoldCapDoesNotBridgeAnArbitrarilyLongAuthoredGap() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return CapScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator CapScenario()
        {
            yield return Setup(); var m = Object.FindFirstObjectByType<ShibuyaMatch>(); var p = m.Player; var d = m.Dummy;
            var data = ShibuyaMovesetExamples.Multi(); data.name = "PASS3A QA Moveset";
            data.steps[0].hits[1].startTime = 2;
            p.SetMoveset(data); Assert.That(p.TryAttack()); yield return Until(() => p.ConfirmedHitCount == 1);
            Assert.That(d.HasConfirmedLink);
            yield return Wait(p.Profile.confirmedLinkMaxHold + .08f);
            Assert.That(d.HasConfirmedLink, Is.False); Assert.That(d.TryAttack(), Is.True);
            Assert.That(p.ConfirmedHitCount, Is.EqualTo(1));
        }

        [UnityTest] public IEnumerator InvulnerableTargetIsNotForcedIntoApproachHit() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return ImmunityScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator ImmunityScenario()
        {
            yield return Setup(); var m = Object.FindFirstObjectByType<ShibuyaMatch>(); var p = m.Player; var d = m.Dummy;
            var data = ShibuyaMovesetExamples.Approach(); data.name = "PASS3A QA Moveset";
            p.SetMoveset(data); Assert.That(p.TryAttack()); yield return Until(() => p.ConfirmedHitCount == 1);
            yield return Until(() => p.AttackElapsed >= .23f); Assert.That(p.TryAttack());
            yield return Until(() => p.ComboStep == 2);
            d.Health.GrantInvulnerability(2); float hp = d.Health.CurrentHealth;
            yield return Wait(.8f);
            Assert.That(p.ConfirmedHitCount, Is.EqualTo(1)); Assert.That(d.Health.CurrentHealth, Is.EqualTo(hp));
            Assert.That(d.HasConfirmedLink, Is.False);
        }

        [UnityTest] public IEnumerator KnockedBackMovingTargetApproachArrivesBeforeDamage() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return ApproachScenario(0); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator ApproachWallBlocksMovementAndRemoteDamage() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return ApproachScenario(1); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator EscapedTargetCancelsApproachAndLink() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return ApproachScenario(2); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator LostTargetCancelsApproachWithoutRetargeting() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return ApproachScenario(3); yield return new ExitPlayMode();
        }
        private IEnumerator ApproachScenario(int failure)
        {
            yield return Setup(); var m = Object.FindFirstObjectByType<ShibuyaMatch>(); var p = m.Player; var d = m.Dummy;
            var data = ShibuyaMovesetExamples.Approach(); data.name = "PASS3A QA Moveset";
            Assert.That(p.SetMoveset(data));
            Vector3 start = p.transform.position, targetStart = d.transform.position;
            float hp = d.Health.CurrentHealth;
            Assert.That(p.TryAttack()); yield return Until(() => p.ConfirmedHitCount == 1);
            yield return Until(() => p.AttackElapsed >= .23f);
            Assert.That(p.TryAttack()); yield return Until(() => p.ComboStep == 2);
            Assert.That(d.Health.CurrentHealth, Is.EqualTo(hp - 9));
            if (failure == 1) {
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.name = "PASS3A QA Wall";
                wall.transform.position = (p.transform.position + d.transform.position) * .5f + Vector3.up;
                wall.transform.localScale = new Vector3(5, 3, .2f); Physics.SyncTransforms();
            }
            if (failure == 2) Place(d, d.transform.position + Vector3.right * 20);
            if (failure == 3) d.gameObject.SetActive(false);
            if (failure == 0) {
                // Target still has authored distance knockback when this approach begins.
                Assert.That(d.transform.position.z - targetStart.z, Is.GreaterThan(2));
                float deadline = Time.realtimeSinceStartup + 3;
                while (p.ConfirmedHitCount < 2 && Time.realtimeSinceStartup < deadline) {
                    Assert.That(d.State, Is.EqualTo(ShibuyaFighterState.Hitstunned), "authored approach fits its finite link cap");
                    yield return null;
                }
                Assert.That(p.ConfirmedHitCount, Is.EqualTo(2));
                Assert.That(d.Health.CurrentHealth, Is.EqualTo(hp - 32));
                Assert.That(Vector3.Distance(start, p.transform.position), Is.GreaterThan(2));
                Assert.That(p.transform.position.z, Is.LessThan(d.transform.position.z));
                Assert.That(Vector3.Distance(p.transform.position, d.transform.position), Is.LessThan(2));
                yield return Until(() => !d.IsInHitStop);
                Vector3 before = d.transform.position; yield return Wait(.12f);
                Assert.That(d.transform.position.z - before.z, Is.GreaterThan(.1f));
                Record("approach", $"attackerTravel={Vector3.Distance(start,p.transform.position)}, targetTravel={d.transform.position.z-targetStart.z}");
            } else {
                yield return Wait(.8f);
                Assert.That(p.ConfirmedHitCount, Is.EqualTo(1));
                Assert.That(d.Health.CurrentHealth, Is.EqualTo(hp - 9));
                Assert.That(d.HasConfirmedLink, Is.False);
                Assert.That(p.ComboStep, Is.EqualTo(0));
            }
        }

        [UnityTest] public IEnumerator DamageDeathAndCombatEndCancelRemainingEvents() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return CancelScenario(); yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator NextStageApproachesLatestValidEventTargetNotFirstVictim() {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath); yield return new EnterPlayMode();
            yield return LatestTargetScenario(); yield return new ExitPlayMode();
        }
        private IEnumerator LatestTargetScenario()
        {
            yield return Setup(); var m = Object.FindFirstObjectByType<ShibuyaMatch>(); var p = m.Player; var d = m.Dummy;
            var other = Object.Instantiate(d.gameObject).GetComponent<ShibuyaFighter>(); other.name = "PASS3A QA Other";
            Place(other, new Vector3(15, .03f, 0));
            var data = ShibuyaMovesetExamples.Multi(); data.name = "PASS3A QA Moveset";
            var approach = ShibuyaMovesetExamples.Approach(); approach.name = "PASS3A QA Moveset";
            Array.Resize(ref data.steps, 2); data.steps[1] = approach.steps[1];
            data.steps[1].hits[0].startTime = .1f;
            p.SetMoveset(data); Assert.That(p.TryAttack());
            yield return Until(() => p.ConfirmedHitCount == 1, () => "first: " + p.ConfirmedHitCount + " d=" + d.transform.position);
            yield return Until(() => p.AttackElapsed > data.steps[0].hits[0].ActiveEnd + .015f);
            Place(d, new Vector3(20, .03f, 0)); Place(other, new Vector3(0, .03f, 0));
            yield return Until(() => p.ConfirmedHitCount == 2 && p.AttackElapsed >= .43f,
                () => "second: " + p.ConfirmedHitCount + " other=" + other.transform.position + " hp=" + other.Health.CurrentHealth);
            Assert.That(p.TryAttack()); yield return Until(() => p.ConfirmedHitCount == 3,
                () => "approach: stage=" + p.ComboStep + " hits=" + p.ConfirmedHitCount + " p=" + p.transform.position
                    + " other=" + other.transform.position + " hp=" + other.Health.CurrentHealth);
            Assert.That(other.Health.CurrentHealth, Is.EqualTo(other.Health.MaxHealth - 34));
            Assert.That(d.Health.CurrentHealth, Is.EqualTo(993));
        }
        private IEnumerator CancelScenario()
        {
            yield return Setup(); var m = Object.FindFirstObjectByType<ShibuyaMatch>(); var p = m.Player; var d = m.Dummy;
            var data = ShibuyaMovesetExamples.Multi(); data.name = "PASS3A QA Moveset";
            p.SetMoveset(data); Assert.That(p.TryAttack()); yield return Until(() => p.ConfirmedHitCount == 1);
            p.ApplyHitReaction(Vector3.zero, .1f); yield return Wait(.5f);
            Assert.That(p.ConfirmedHitCount, Is.EqualTo(1)); Assert.That(d.HasConfirmedLink, Is.False);
            yield return Until(() => d.State == ShibuyaFighterState.Ready);
            Assert.That(p.TryAttack()); p.StopCombat(); yield return Wait(.5f);
            Assert.That(p.ConfirmedHitCount, Is.EqualTo(1));
            Assert.That(p.SetMoveset(null)); Assert.That(p.TryAttack(), Is.False);
        }

        private static IEnumerator Setup()
        {
            Application.runInBackground = true; yield return Wait(.1f);
            var m = Object.FindFirstObjectByType<ShibuyaMatch>();
            m.Player.GetComponent<ShibuyaPlayerInput>().TestInput = () => default;
            Place(m.Player, new Vector3(0, .03f, -1.4f)); Place(m.Dummy, new Vector3(0, .03f, 0));
            m.Player.transform.rotation = Quaternion.identity;
        }
        private static void Record(string scenario, string details) {
            System.IO.Directory.CreateDirectory(ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass3AQA"));
            System.IO.File.AppendAllText(ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass3AQA/metrics.txt"), scenario + ": " + details + Environment.NewLine);
        }
        private static IEnumerator Wait(float seconds) { float end = Time.time + seconds; while (Time.time < end) yield return null; }
        private static IEnumerator Until(Func<bool> predicate, Func<string> diagnostic = null) {
            float end = Time.realtimeSinceStartup + 4; while (!predicate() && Time.realtimeSinceStartup < end) yield return null;
            Assert.That(predicate(), Is.True, "Play Mode condition timed out " + diagnostic?.Invoke());
        }
        private static void Place(ShibuyaFighter fighter, Vector3 position) {
            var motor = fighter.GetComponent<CharacterController>(); motor.enabled = false;
            fighter.transform.position = position; motor.enabled = true; Physics.SyncTransforms();
        }
        [UnityTearDown] public IEnumerator Leave() { if (EditorApplication.isPlaying) yield return new ExitPlayMode(); }
        [TearDown] public void Cleanup() {
            foreach (var data in Resources.FindObjectsOfTypeAll<ShibuyaMovesetDefinition>())
                if (data.name == "PASS3A QA Moveset" && !AssetDatabase.Contains(data)) Object.DestroyImmediate(data);
        }
    }
}
