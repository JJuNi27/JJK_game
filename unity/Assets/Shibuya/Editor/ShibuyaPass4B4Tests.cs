using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass4B4DataTests
    {
        [Test] public void ProfileOwnershipKoreanInspectorAndUndo()
        {
            var asset = AssetDatabase.LoadAssetAtPath<CursedEnergyProfile>(ShibuyaSceneBuilder.Root + "/Data/Shibuya Cursed Energy.asset");
            var profile = Object.Instantiate(asset);
            UnityEditor.Editor editor = null;
            try {
                Assert.That(asset.RecoveryMode, Is.EqualTo(CursedEnergyRecoveryMode.HoldCharge));
                Assert.That(asset.MaxEnergy, Is.EqualTo(100));
                Assert.That(asset.StartingRatio, Is.EqualTo(1));
                Assert.That(asset.RegenerationPerSecond, Is.EqualTo(12));
                var combat = AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root + "/Data/Shibuya Combat Profile.asset");
                Assert.That(combat.cursedEnergy, Is.SameAs(asset));
                editor = UnityEditor.Editor.CreateEditor(profile);
                Assert.That(editor.GetType().Name, Is.EqualTo("CursedEnergyProfileEditor"));
                foreach (string field in new[] { "maxEnergy", "recoveryMode", "startingRatio", "chargingEnabled", "regenerationPerSecond" })
                    Assert.That(JJKGame.EditorTools.KoreanInspectorLabels.TextFor(field, null), Is.Not.Null, field);
                var so = new SerializedObject(profile);
                so.FindProperty("startingRatio").floatValue = .25f;
                so.ApplyModifiedProperties(); Undo.FlushUndoRecordObjects();
                Assert.That(profile.StartingEnergy, Is.EqualTo(25));
                Undo.PerformUndo(); Assert.That(profile.StartingRatio, Is.EqualTo(1));
                Assert.That(asset.StartingRatio, Is.EqualTo(1));
            } finally { if (editor != null) Object.DestroyImmediate(editor); Undo.ClearUndo(profile); Object.DestroyImmediate(profile); }
        }
    }

    public sealed class ShibuyaPass4B4Tests : ShibuyaPass4B3aFixture
    {
        private int savedFrameRate;
        private ShibuyaEnergyCharge charge;
        private CursedEnergyController energy => charge.Resource;
        [UnitySetUp] public IEnumerator EnergySetup()
        {
            savedFrameRate = Application.targetFrameRate; Application.targetFrameRate = 60;
            charge = input.EnergyCharge; Assert.That(charge, Is.Not.Null);
            Capacity(100); yield return Wait(.12f);
            Assert.That(energy.Profile, Is.SameAs(reserve));
        }
        [UnityTearDown] public IEnumerator RestoreFrameRate() { Application.targetFrameRate = savedFrameRate; yield break; }
        private void Setting(string field, float value)
        {
            var so = new SerializedObject(reserve); so.FindProperty(field).floatValue = value; so.ApplyModifiedPropertiesWithoutUndo();
        }
        private void EnabledCharging(bool value)
        {
            var so = new SerializedObject(reserve); so.FindProperty("chargingEnabled").boolValue = value; so.ApplyModifiedPropertiesWithoutUndo();
        }

        [UnityTest] public IEnumerator CapacitiesStatesRecoveryAndResourceEvents()
        {
            var observations = new List<object>(); int changes = 0;
            energy.EnergyChanged += (_, value) => { changes++; Assert.That(value, Is.InRange(0, energy.MaxEnergy)); };
            foreach (float maximum in new[] { 100f, 150, 200 }) {
                Capacity(maximum); yield return Wait(.1f);
                Assert.That(energy.MaxEnergy, Is.EqualTo(maximum)); Assert.That(aura.MaximumEnergy, Is.EqualTo(maximum));
                energy.SetCurrentEnergy(0); yield return Wait(.2f); Assert.That(energy.CurrentEnergy, Is.Zero, "No passive regeneration in hold mode");
                foreach (float ratio in new[] { 0f, .5f, 1f }) {
                    energy.SetCurrentEnergy(maximum * ratio);
                    frame.auraHeld = true; yield return Wait(.3f);
                    float duration = charge.HoldDuration;
                    float expected = Mathf.Min(maximum, maximum * ratio + reserve.RegenerationPerSecond * duration);
                    Assert.That(energy.CurrentEnergy, Is.EqualTo(expected).Within(.02f));
                    observations.Add(new { maximum, ratio, duration, current = energy.CurrentEnergy });
                    frame.auraHeld = false; yield return Wait(.08f);
                    float stopped = energy.CurrentEnergy; yield return Wait(.12f); Assert.That(energy.CurrentEnergy, Is.EqualTo(stopped));
                    Assert.That(charge.HoldDuration, Is.Zero);
                }
                energy.Restore(10000); Assert.That(energy.CurrentEnergy, Is.EqualTo(maximum));
                Assert.That(energy.TrySpend(20), Is.True); Assert.That(energy.CurrentEnergy, Is.EqualTo(maximum - 20));
                Assert.That(energy.TrySpend(10000), Is.False); Assert.That(energy.CurrentEnergy, Is.EqualTo(maximum - 20));
                energy.SetCurrentEnergy(-10); Assert.That(energy.CurrentEnergy, Is.Zero);
                energy.Restore(float.NaN); energy.SetCurrentEnergy(float.PositiveInfinity); Assert.That(energy.CurrentEnergy, Is.Zero);
            }
            Assert.That(changes, Is.GreaterThan(30)); Record("4B4-capacities", new { changes, observations });
        }

        [UnityTest] public IEnumerator FullHoldReleaseAndRapidCycles()
        {
            int started = 0, released = 0, interrupted = 0; float releasedDuration = 0;
            charge.ChargeStarted += _ => started++;
            charge.ChargeReleased += (_, duration) => { released++; releasedDuration = duration; };
            charge.ChargeInterrupted += (_, __) => interrupted++;
            energy.ResetEnergy(); frame.auraHeld = true; yield return Wait(.4f);
            Assert.That(charge.IsCharging, Is.True); Assert.That(charge.HoldDuration, Is.GreaterThan(.3f));
            Assert.That(energy.CurrentEnergy, Is.EqualTo(100));
            frame.auraHeld = false; yield return Wait(.08f);
            Assert.That(releasedDuration, Is.GreaterThan(.3f)); Assert.That(charge.HoldDuration, Is.Zero);
            for (int i = 0; i < 12; i++) {
                frame.auraHeld = true; yield return Wait(.05f); Assert.That(charge.IsCharging, Is.True);
                frame.auraHeld = false; yield return Wait(.05f); Assert.That(charge.IsCharging, Is.False);
            }
            Assert.That(started, Is.EqualTo(13)); Assert.That(released, Is.EqualTo(13));
            frame.auraHeld = true; yield return Wait(.08f); input.enabled = false;
            Assert.That(charge.IsCharging, Is.False); Assert.That(aura.Held, Is.False);
            Assert.That(released, Is.EqualTo(13)); Assert.That(interrupted, Is.EqualTo(1));
            input.enabled = true; frame = default; yield return Wait(.1f);
            Record("4B4-hold-cycles", new { started, released, interrupted, lastDuration = releasedDuration, timeScale = Time.timeScale });
            Assert.That(Time.timeScale, Is.EqualTo(1));
        }

        [UnityTest] public IEnumerator StartingRatioDisabledChargingAndLiveMaximum()
        {
            Setting("startingRatio", .25f);
            foreach (float max in new[] { 100f, 150, 200 }) {
                Capacity(max); energy.ResetEnergy(); Assert.That(energy.CurrentEnergy, Is.EqualTo(max * .25f));
            }
            EnabledCharging(false); frame.auraHeld = true; yield return Wait(.25f);
            Assert.That(charge.IsCharging, Is.False); Assert.That(energy.CurrentEnergy, Is.EqualTo(50));
            Assert.That(aura.Held, Is.True, "Charging permission must not replace the approved visual switch");
            EnabledCharging(true); yield return Wait(.25f); Assert.That(energy.CurrentEnergy, Is.GreaterThan(50));
            frame.auraHeld = false; yield return Wait(.05f);
            int events = 0; energy.EnergyChanged += (_, __) => events++;
            energy.SetCurrentEnergy(200); Capacity(100); yield return Wait(.1f);
            Assert.That(energy.CurrentEnergy, Is.EqualTo(100)); Assert.That(energy.MaxEnergy, Is.EqualTo(100));
            Capacity(150); yield return Wait(.1f);
            Assert.That(energy.CurrentEnergy, Is.EqualTo(100)); Assert.That(events, Is.GreaterThanOrEqualTo(3));
            Record("4B4-start-live-max", new { startRatio = reserve.StartingRatio, current = energy.CurrentEnergy, maximum = energy.MaxEnergy, events });
        }

        [UnityTest] public IEnumerator AuraCurrentIndependenceAndOwnerOnlyHud()
        {
            Capacity(200); frame.auraHeld = true; yield return Wait(1.6f);
            float width = Field.vertices.Max(v => v.x) - Field.vertices.Min(v => v.x);
            float multiplier = aura.SizeMultiplier;
            var match = Object.FindFirstObjectByType<ShibuyaMatch>();
            var enemyEnergy = CursedEnergyController.GetOrCreate(dummy.gameObject);
            enemyEnergy.ApplyProfile(reserve); enemyEnergy.SetCurrentEnergy(17);
            foreach (float current in new[] { 0f, 100, 200 }) {
                energy.SetCurrentEnergy(current); yield return Wait(.15f);
                Assert.That(aura.SizeMultiplier, Is.EqualTo(multiplier));
                Assert.That(Field.vertices.Max(v => v.x) - Field.vertices.Min(v => v.x), Is.EqualTo(width).Within(.00001f));
                Assert.That(match.PlayerEnergy, Is.SameAs(energy)); Assert.That(match.PlayerEnergy, Is.Not.SameAs(enemyEnergy));
                Assert.That(enemyEnergy.CurrentEnergy, Is.EqualTo(17));
            }
            int objects = aura.Visual.GetComponentsInChildren<MeshFilter>().Length;
            var visual = aura.Visual;
            for (int i = 0; i < 6; i++) { frame.auraHeld = false; yield return Wait(.05f); frame.auraHeld = true; yield return Wait(.05f); }
            Assert.That(aura.Visual, Is.SameAs(visual)); Assert.That(visual.GetComponentsInChildren<MeshFilter>().Length, Is.EqualTo(objects));
            frame.auraHeld = false; yield return Wait(.4f); Assert.That(visual.HasVisibleRenderers, Is.False); Assert.That(visual.ParticleCount, Is.Zero);
            Assert.That(player.GetComponents<CursedEnergyController>().Length, Is.EqualTo(1));
            Assert.That(player.GetComponents<ShibuyaEnergyCharge>().Length, Is.EqualTo(1));
            Record("4B4-aura-hud", new { width, multiplier, ownedMeshes = visual.OwnedMeshCount, currentIndependent = true, onlyPlayerHudSource = true });
        }

        [UnityTest] public IEnumerator ExistingAutomaticRecoveryRemainsAutomatic()
        {
            var go = new GameObject("PASS4B4 legacy energy fixture");
            var legacy = ScriptableObject.CreateInstance<CursedEnergyProfile>();
            try {
                Assert.That(legacy.RecoveryMode, Is.EqualTo(CursedEnergyRecoveryMode.Automatic));
                Assert.That(legacy.StartingEnergy, Is.EqualTo(100));
                var resource = CursedEnergyController.GetOrCreate(go); resource.ApplyProfile(legacy);
                resource.SetCurrentEnergy(0); yield return Wait(.3f);
                Assert.That(resource.CurrentEnergy, Is.GreaterThan(2)); Assert.That(resource.CurrentEnergy, Is.LessThan(10));
                Assert.That(resource.UsesHoldCharge, Is.False);
                Assert.That(energy.UsesHoldCharge, Is.True); energy.SetCurrentEnergy(0); yield return Wait(.15f); Assert.That(energy.CurrentEnergy, Is.Zero);
                Record("4B4-legacy-mode", new { legacyRecovery = resource.CurrentEnergy, manualUnheld = energy.CurrentEnergy });
            } finally { Object.Destroy(go); Object.Destroy(legacy); }
        }

        [UnityTest] public IEnumerator ChargeDoesNotReplaceMovementJumpOrAttack()
        {
            energy.SetCurrentEnergy(0); Place(player, new Vector3(0, .03f, -12));
            frame.auraHeld = true; frame.move = Vector2.up; Vector3 start = player.transform.position;
            yield return Wait(.3f); Assert.That(Vector3.Distance(start, player.transform.position), Is.GreaterThan(1));
            frame.move = Vector2.zero; frame.jump = true; yield return Wait(.05f); frame.jump = false;
            yield return Wait(.15f); Assert.That(player.transform.position.y - start.y, Is.GreaterThan(.3f));
            yield return Wait(1.3f); Assert.That(player.IsGrounded, Is.True);
            Place(player, new Vector3(0, .03f, -1.4f)); Place(dummy, new Vector3(0, .03f, 0)); player.transform.rotation = Quaternion.identity;
            int attacks = player.AcceptedAttackCount; frame.attack = true; yield return Wait(.05f); frame.attack = false;
            yield return Wait(.5f); Assert.That(player.AcceptedAttackCount, Is.EqualTo(attacks + 1));
            Assert.That(dummy.Health.CurrentHealth, Is.EqualTo(980)); Assert.That(energy.CurrentEnergy, Is.GreaterThan(10));
            Assert.That(combat.strikes.Select(s => s.damage), Is.EqualTo(new[] { 20f, 20, 20, 25 }));
            Assert.That(dummy.Health.MaxHealth, Is.EqualTo(1000)); Assert.That(player.Health.MaxHealth, Is.EqualTo(100));
            Record("4B4-controls", new { current = energy.CurrentEnergy, attacks = player.AcceptedAttackCount, hits = player.ConfirmedHitCount });
        }

        [UnityTest] public IEnumerator HitDeathMatchEndAndRestartCancelCharge()
        {
            var match = Object.FindFirstObjectByType<ShibuyaMatch>(); energy.SetCurrentEnergy(0);
            frame.auraHeld = true; yield return Wait(.15f);
            player.Health.ReceiveDamage(new DamageContext(1, dummy.gameObject, DamageDeliveryType.PhysicalStrike));
            player.ApplyHitReaction(Vector3.zero, .25f);
            yield return Wait(.08f); Assert.That(charge.IsCharging, Is.False); float value = energy.CurrentEnergy;
            yield return Wait(.08f); Assert.That(energy.CurrentEnergy, Is.EqualTo(value));
            yield return Wait(.3f); Assert.That(charge.IsCharging, Is.True);
            dummy.Health.ReceiveDamage(new DamageContext(10000, player.gameObject, DamageDeliveryType.PhysicalStrike)); yield return Wait(.1f);
            Assert.That(match.Finished, Is.True); Assert.That(charge.IsCharging, Is.False); Assert.That(charge.HoldDuration, Is.Zero);
            value = energy.CurrentEnergy; yield return Wait(.1f); Assert.That(energy.CurrentEnergy, Is.EqualTo(value));
            match.RestartBattle(); yield return Wait(.4f);
            match = Object.FindFirstObjectByType<ShibuyaMatch>(); Assert.That(match.Finished, Is.False);
            var fresh = match.Player.GetComponent<ShibuyaPlayerInput>(); fresh.TestInput = () => default;
            Assert.That(fresh.EnergyCharge.Resource.CurrentEnergy, Is.EqualTo(fresh.EnergyCharge.Resource.MaxEnergy));
            Assert.That(fresh.EnergyCharge.HoldDuration, Is.Zero);
            fresh.TestInput = () => new ShibuyaInputFrame { auraHeld = true }; yield return Wait(.1f);
            match.EditorDefeatPlayer(); yield return Wait(.1f);
            Assert.That(match.Finished, Is.True); Assert.That(fresh.EnergyCharge.IsCharging, Is.False);
            Record("4B4-restart", new { newBattleFull = true, deathStops = true, timeScale = Time.timeScale });
        }

        [UnityTest] public IEnumerator HudVisualEvidence()
        {
            Capacity(150); energy.SetCurrentEnergy(30); frame.auraHeld = true;
            Pose(new Vector3(3, 2.5f, -4)); yield return Wait(.4f);
            Directory.CreateDirectory(Root);
            string path = Path.Combine(Root, "4B4-player-hud-charging.png");
            ScreenCapture.CaptureScreenshot(path); yield return Wait(.5f);
            Assert.That(File.Exists(path), Is.True);
            Record("4B4-hud-capture", new { file = path, current = energy.CurrentEnergy, maximum = energy.MaxEnergy, charging = charge.IsCharging });
        }
    }
}
