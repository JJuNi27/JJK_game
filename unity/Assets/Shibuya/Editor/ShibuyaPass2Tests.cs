using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass2Tests
    {
        [Test]
        public void FourStrikeDataKoreanLabelsAndSerializedUndo()
        {
            var profile = AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root + "/Data/Shibuya Combat Profile.asset");
            Assert.That(profile.HasValidCombo, Is.True);
            Assert.That(profile.strikes[0].damage, Is.EqualTo(20));
            Assert.That(profile.strikes[0].startup, Is.EqualTo(0.1f));
            Assert.That(profile.strikes[0].activeTime, Is.EqualTo(0.08f));
            Assert.That(profile.strikes[0].recovery, Is.EqualTo(0.22f));
            Assert.That(profile.strikes[0].knockbackSpeed, Is.EqualTo(6));
            Assert.That(profile.strikes[3].knockbackSpeed, Is.GreaterThan(profile.strikes.Take(3).Max(s => s.knockbackSpeed)));
            Assert.That(profile.strikes.Select(s => s.motion).Distinct().Count(), Is.EqualTo(4));
            foreach (var field in typeof(ShibuyaStrikeDefinition).GetFields())
                Assert.That(ShibuyaInspector.KoreanLabel(field.Name), Is.Not.EqualTo(field.Name), field.Name);
            foreach (string field in new[] { "inputBufferTime", "comboResetTime", "strikes" })
                Assert.That(ShibuyaInspector.KoreanLabel(field), Is.Not.EqualTo(field));
            var clone = Object.Instantiate(profile);
            try
            {
                var so = new SerializedObject(clone);
                Assert.That(so.FindProperty("damage"), Is.Null, "no ambiguous legacy attack settings");
                var value = so.FindProperty("strikes").GetArrayElementAtIndex(1).FindPropertyRelative("damage");
                float old = value.floatValue;
                Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
                value.floatValue = old + 3f; so.ApplyModifiedProperties(); Undo.FlushUndoRecordObjects();
                Assert.That(clone.strikes[1].damage, Is.EqualTo(old + 3f));
                Undo.CollapseUndoOperations(group); Undo.PerformUndo();
                Assert.That(clone.strikes[1].damage, Is.EqualTo(old));
            }
            finally { Object.DestroyImmediate(clone); }
        }

        [UnityTest]
        public IEnumerator BufferedFourStrikesUseIndependentDamageTimingAndKnockback()
        {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);
            yield return new EnterPlayMode();
            // Allocate captured scenario locals AFTER domain reload, never before EnterPlayMode.
            yield return BufferedFourStrikesUseIndependentDamageTimingAndKnockbackScenario();
            yield return new ExitPlayMode();
        }

        private IEnumerator BufferedFourStrikesUseIndependentDamageTimingAndKnockbackScenario()
        {
            Application.runInBackground = true;
            yield return Wait(0.1f);
            var match = Object.FindFirstObjectByType<ShibuyaMatch>();
            var player = match.Player; var dummy = match.Dummy;
            player.GetComponent<ShibuyaPlayerInput>().TestInput = () => default;
            var profile = CloneProfile(player);
            var amounts = new[] { 7f, 11f, 13f, 19f };
            for (int i = 0; i < 4; i++)
            {
                var strike = profile.strikes[i];
                // Exercise the optional radial policy explicitly; production light hits now use attack-forward.
                if (i < 3) strike.knockbackDirection = ShibuyaKnockbackDirection.AwayFromAttacker;
                strike.damage = amounts[i]; strike.startup = 0.08f + i * 0.035f;
                strike.activeTime = 0.05f + i * 0.01f; strike.recovery = 0.19f + i * 0.025f;
                strike.attackRadius = 0.8f + i * 0.05f; strike.attackForwardOffset = 1.05f + i * 0.03f;
                strike.attackHeight = 0.9f + i * 0.05f; strike.hitStun = 0.18f + i * 0.04f;
            }
            var extra = new GameObject("QA_MultipleHurtboxes"); extra.transform.SetParent(dummy.transform, false);
            extra.transform.localPosition = Vector3.up;
            extra.AddComponent<BoxCollider>().size = Vector3.one * 0.5f;
            extra.AddComponent<SphereCollider>().radius = 0.35f;
            var starts = new List<float>(); var frames = new List<int>(); var steps = new List<int>();
            var hits = new List<float>(); var impulses = new List<Vector3>();
            player.AttackStarted += step => { steps.Add(step); starts.Add(Time.time); frames.Add(Time.frameCount); };
            player.HitConfirmed += (point, intensity) => { hits.Add(Time.time); impulses.Add(dummy.KnockbackVelocity); };
            float health = dummy.Health.CurrentHealth;
            Place(player, new Vector3(0, 0.03f, -1.35f)); player.transform.rotation = Quaternion.identity;
            Place(dummy, new Vector3(0.25f, 0.03f, 0));
            Assert.That(player.TryAttack(), Is.True);
            for (int i = 0; i < 4; i++)
            {
                Assert.That(player.ComboStep, Is.EqualTo(i + 1));
                for (int spam = 0; spam < 50; spam++) player.TryAttack();
                Assert.That(player.AcceptedAttackCount, Is.EqualTo(i + 1), "spam cannot skip phases");
                yield return Until(() => hits.Count > i, 2f);
                health -= amounts[i];
                Assert.That(dummy.Health.CurrentHealth, Is.EqualTo(health).Within(0.01f));
                Assert.That(hits[i] - starts[i], Is.GreaterThanOrEqualTo(profile.strikes[i].Startup - 0.002f));
                Assert.That(impulses[i].magnitude, Is.EqualTo(profile.strikes[i].knockbackSpeed).Within(0.01f));
                Assert.That(impulses[i].y, Is.Zero);
                Assert.That(dummy.State, Is.EqualTo(ShibuyaFighterState.Hitstunned));
                if (i == 3) Assert.That(impulses[i].normalized, Is.EqualTo(Vector3.forward));
                else Assert.That(impulses[i].x, Is.GreaterThan(0), "away-from-attacker policy");
                yield return Until(() => player.AttackElapsed >= profile.strikes[i].Duration - 0.075f, 2f);
                Assert.That(player.ConfirmedHitCount, Is.EqualTo(i + 1), "one damage despite multiple colliders and active frames");
                if (i < 3)
                {
                    Assert.That(player.TryAttack(), Is.True);
                    Assert.That(player.TryAttack(), Is.False, "one buffered successor only");
                    Place(dummy, new Vector3(0.25f, 0.03f, 0));
                    yield return Until(() => player.ComboStep == i + 2, 2f);
                }
                else Assert.That(player.TryAttack(), Is.False, "finisher cannot reserve a new combo");
            }
            yield return Until(() => player.State == ShibuyaFighterState.Ready, 2f);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, steps);
            for (int i = 1; i < 4; i++)
            {
                Assert.That(starts[i] - starts[i - 1], Is.GreaterThanOrEqualTo(profile.strikes[i - 1].Duration - 0.002f));
                Assert.That(frames[i], Is.GreaterThan(frames[i - 1]));
            }
            Assert.That(player.TryAttack(), Is.True); Assert.That(player.ComboStep, Is.EqualTo(1));
            Debug.Log("SHIBUYA_PASS2_COMBO: 1-2-3-4; independent damage/timing/hit volumes; dedup; stronger finisher; one buffered successor; no phase skipping PASS");
        }

        [UnityTest]
        public IEnumerator HeldInputSlowClicksTimeoutAndEarlyBufferExpiry()
        {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);
            yield return new EnterPlayMode();
            // Allocate captured scenario locals AFTER domain reload, never before EnterPlayMode.
            yield return HeldInputSlowClicksTimeoutAndEarlyBufferExpiryScenario();
            yield return new ExitPlayMode();
        }

        private IEnumerator HeldInputSlowClicksTimeoutAndEarlyBufferExpiryScenario()
        {
            Application.runInBackground = true;
            yield return Wait(0.1f);
            var player = Object.FindFirstObjectByType<ShibuyaMatch>().Player;
            var input = player.GetComponent<ShibuyaPlayerInput>();
            var profile = CloneProfile(player);
            input.TestInput = () => new ShibuyaInputFrame { attack = true };
            yield return Wait(profile.strikes[0].Duration + profile.comboResetTime + 0.12f);
            Assert.That(player.AcceptedAttackCount, Is.EqualTo(1), "held input is not auto-combo");
            input.TestInput = () => default; yield return null;
            Assert.That(player.TryAttack(), Is.True); Assert.That(player.ComboStep, Is.EqualTo(1));
            Assert.That(player.TryAttack(), Is.False, "startup click is outside buffer window");
            yield return Until(() => player.State == ShibuyaFighterState.Ready, 2f);
            yield return Wait(0.05f);
            Assert.That(player.TryAttack(), Is.True); Assert.That(player.ComboStep, Is.EqualTo(2), "slow click within reset time");
            yield return Until(() => player.State == ShibuyaFighterState.Ready, 2f);
            yield return Wait(profile.comboResetTime + 0.05f);
            Assert.That(player.TryAttack(), Is.True); Assert.That(player.ComboStep, Is.EqualTo(1), "timeout resets combo");
            // Changing the runtime-only clone makes an accepted input expire before a later completion.
            yield return Until(() => player.AttackElapsed >= profile.strikes[0].Duration - 0.10f, 2f);
            Assert.That(player.TryAttack(), Is.True);
            profile.strikes[0].recovery += 0.25f;
            yield return Wait(0.5f);
            Assert.That(player.HasBufferedAttack, Is.False);
            Assert.That(player.State, Is.EqualTo(ShibuyaFighterState.Ready), "expired input cannot chain");
            Debug.Log("SHIBUYA_PASS2_INPUT: hold/early click/slow click/timeout/buffer expiry PASS");
        }

        [UnityTest]
        public IEnumerator HitDeathAndMatchFinishCancelComboAndBuffer()
        {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);
            yield return new EnterPlayMode();
            // Allocate captured scenario locals AFTER domain reload, never before EnterPlayMode.
            yield return HitDeathAndMatchFinishCancelComboAndBufferScenario();
            yield return new ExitPlayMode();
        }

        private IEnumerator HitDeathAndMatchFinishCancelComboAndBufferScenario()
        {
            Application.runInBackground = true;
            yield return Wait(0.1f);
            var match = Object.FindFirstObjectByType<ShibuyaMatch>();
            var player = match.Player;
            player.GetComponent<ShibuyaPlayerInput>().TestInput = () => default;
            var profile = CloneProfile(player);
            Assert.That(player.TryAttack(), Is.True);
            yield return Until(() => player.AttackElapsed >= profile.strikes[0].Duration - 0.10f, 2f);
            Assert.That(player.TryAttack(), Is.True); Assert.That(player.HasBufferedAttack, Is.True);
            player.Health.ReceiveDamage(new DamageContext(1, match.Dummy.gameObject, DamageDeliveryType.PhysicalStrike));
            player.ApplyHitReaction(Vector3.right * 4, 0.2f);
            Assert.That(player.ComboStep, Is.Zero); Assert.That(player.HasBufferedAttack, Is.False);
            Assert.That(player.TryAttack(), Is.False);
            yield return Wait(0.3f);
            Assert.That(player.AcceptedAttackCount, Is.EqualTo(1));
            Assert.That(player.TryAttack(), Is.True); Assert.That(player.ComboStep, Is.EqualTo(1));
            yield return Until(() => player.AttackElapsed >= profile.strikes[0].Duration - 0.10f, 2f);
            Assert.That(player.TryAttack(), Is.True);
            match.EditorDefeatPlayer();
            Assert.That(match.Finished, Is.True); Assert.That(player.State, Is.EqualTo(ShibuyaFighterState.Dead));
            Assert.That(player.ComboStep, Is.Zero); Assert.That(player.HasBufferedAttack, Is.False);
            yield return Wait(0.5f);
            Assert.That(player.AcceptedAttackCount, Is.EqualTo(2));
            match.RestartBattle(); yield return Wait(0.5f);
            match = Object.FindFirstObjectByType<ShibuyaMatch>(); player = match.Player;
            player.GetComponent<ShibuyaPlayerInput>().TestInput = () => default;
            Assert.That(player.AcceptedAttackCount, Is.Zero); Assert.That(player.ComboStep, Is.Zero);
            Assert.That(player.TryAttack(), Is.True);
            yield return Until(() => player.AttackElapsed >= player.Profile.strikes[0].Duration - 0.10f, 2f);
            Assert.That(player.TryAttack(), Is.True);
            match.Dummy.Health.Kill();
            Assert.That(match.Finished && match.Victory, Is.True);
            Assert.That(player.State, Is.EqualTo(ShibuyaFighterState.Finished));
            Assert.That(player.ComboStep, Is.Zero); Assert.That(player.HasBufferedAttack, Is.False);
            Assert.That(player.TryAttack(), Is.False);
            Debug.Log("SHIBUYA_PASS2_CANCEL: applied hit/stun/death/victory/restart clears combo and buffer PASS");
        }

        [UnityTearDown]
        public IEnumerator LeavePlayModeEvenOnFailure()
        {
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
        }
        [TearDown]
        public void ReleaseRuntimeOnlyTestProfiles()
        {
            foreach (var profile in Resources.FindObjectsOfTypeAll<ShibuyaCombatProfile>())
                if (profile.name == "Shibuya PASS2 QA Profile" && !AssetDatabase.Contains(profile)) Object.DestroyImmediate(profile);
        }
        private static ShibuyaCombatProfile CloneProfile(ShibuyaFighter player)
        {
            var profile = Object.Instantiate(player.Profile);
            profile.name = "Shibuya PASS2 QA Profile";
            profile.trackingEnabled = false;
            foreach (var strike in profile.strikes) strike.advanceDistance = 0f; // Isolate PASS2 phase/data assertions.
            player.Configure(profile, AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root + "/Data/Player Stats.asset"));
            return profile;
        }

        [UnityTest]
        public IEnumerator RealInputComboHitsWithoutRepositioningBetweenStrikes()
        {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);
            yield return new EnterPlayMode();
            yield return RealInputComboScenario();
            yield return new ExitPlayMode();
        }
        private IEnumerator RealInputComboScenario()
        {
            Application.runInBackground = true;
            yield return Wait(0.1f);
            var match = Object.FindFirstObjectByType<ShibuyaMatch>();
            var player = match.Player;
            var input = player.GetComponent<ShibuyaPlayerInput>();
            match.Dummy.Health.SetCurrentHealth(80f); // Runtime fixture, not the user's max HP.
            System.IO.Directory.CreateDirectory("Logs/ShibuyaPass21QA");
            System.IO.File.WriteAllText("Logs/ShibuyaPass21QA/resume-forward-trace.jsonl", "");
            System.Action<string> trace = beat => System.IO.File.AppendAllText("Logs/ShibuyaPass21QA/resume-forward-trace.jsonl",
                $"{beat}: t={Time.time:F3} step={player.ComboStep} elapsed={player.AttackElapsed:F3} hits={player.ConfirmedHitCount} hp={match.Dummy.Health.CurrentHealth} p={player.transform.position} d={match.Dummy.transform.position} forward={player.transform.forward}\n");
            player.AttackStarted += step => trace("start");
            player.HitConfirmed += (point, force) => trace("hit");
            bool press = false;
            input.TestInput = () => new ShibuyaInputFrame { move = Vector2.up, attack = press };
            Place(player, new Vector3(0, 0.03f, -1.35f)); player.transform.rotation = Quaternion.identity;
            Place(match.Dummy, new Vector3(0, 0.03f, 0));
            press = true; yield return null; press = false; yield return null;
            for (int step = 1; step < 4; step++)
            {
                trace($"wait window {step}");
                yield return Until(() => player.ComboStep == step && player.AttackElapsed >= player.CurrentStrike.Duration - 0.075f, 2f);
                trace($"press next {step + 1}");
                press = true; yield return null; press = false; yield return null;
                yield return Until(() => player.ComboStep == step + 1, 2f);
            }
            trace("wait victory");
            yield return Until(() => match.Finished, 2f);
            Assert.That(match.Victory, Is.True);
            Assert.That(player.AcceptedAttackCount, Is.EqualTo(4));
            Assert.That(player.ConfirmedHitCount, Is.EqualTo(4));
            Assert.That(match.Dummy.Health.CurrentHealth, Is.Zero);
            Debug.Log("SHIBUYA_PASS2_REAL_INPUT: four press edges + forward approach, no target/player reposition between strikes, four hits and victory PASS");
        }
        private static IEnumerator Until(System.Func<bool> predicate, float timeout)
        {
            float deadline = Time.realtimeSinceStartup + timeout;
            while (!predicate() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(predicate(), Is.True, "Play Mode condition timed out");
        }
        private static IEnumerator Wait(float duration)
        {
            float end = Time.time + duration;
            while (Time.time < end) yield return null;
        }
        private static void Place(ShibuyaFighter actor, Vector3 position)
        {
            var motor = actor.GetComponent<CharacterController>(); motor.enabled = false;
            actor.transform.position = position; motor.enabled = true; Physics.SyncTransforms();
        }
    }
}
