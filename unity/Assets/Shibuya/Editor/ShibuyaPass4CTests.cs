using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass4CDataTests
    {
        [Test] public void SharedDataKoreanInspectorAndUndo()
        {
            var combat = AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root + "/Data/Shibuya Combat Profile.asset");
            Assert.That(combat.groundDash, Is.Not.Null);
            var p = Object.Instantiate(combat.groundDash);
            try {
                Assert.That(p.forwardDistance, Is.EqualTo(5.25f)); Assert.That(p.forwardDuration, Is.EqualTo(.22f));
                Assert.That(p.sideDistance, Is.EqualTo(3.6f)); Assert.That(p.sideDuration, Is.EqualTo(.18f));
                Assert.That(p.backwardDistance, Is.EqualTo(3.3f)); Assert.That(p.backwardDuration, Is.EqualTo(.2f));
                Assert.That(p.evadeStartDelay, Is.EqualTo(.04f)); Assert.That(p.evadeDuration, Is.EqualTo(.1f)); Assert.That(p.cooldownAfterEnd, Is.EqualTo(.45f));
                var so = new SerializedObject(p); var it = so.GetIterator(); bool child = true;
                while (it.NextVisible(child)) { child = false; if (it.name != "m_Script") Assert.That(ShibuyaGroundDashInspector.Labels.ContainsKey(it.name), Is.True, it.name); }
                var editor = UnityEditor.Editor.CreateEditor(p);
                try { Assert.That(editor, Is.TypeOf<ShibuyaGroundDashInspector>()); } finally { Object.DestroyImmediate(editor); }
                Undo.IncrementCurrentGroup(); so.FindProperty("sideDistance").floatValue = 3;
                so.ApplyModifiedProperties(); Undo.FlushUndoRecordObjects(); Assert.That(p.sideDistance, Is.EqualTo(3));
                Undo.PerformUndo(); Assert.That(p.sideDistance, Is.EqualTo(3.6f));
                Assert.That(combat.groundDash.sideDistance, Is.EqualTo(3.6f));
            } finally { Undo.ClearUndo(p); Object.DestroyImmediate(p); }
        }
    }
    public abstract class ShibuyaPass4CFixture : ShibuyaPass4B3Fixture
    {
        protected ShibuyaGroundDashProfile dash;
        private int oldFps;
        [UnitySetUp] public IEnumerator DashSetup()
        {
            oldFps = Application.targetFrameRate; Application.targetFrameRate = 60;
            dash = Clone(combat.groundDash); combat.groundDash = dash;
            Place(player, new Vector3(0, .03f, -8)); Place(dummy, new Vector3(8, .03f, 8));
            camera.transform.rotation = Quaternion.identity; player.transform.rotation = Quaternion.identity;
            yield return Wait(.12f);
        }
        [UnityTearDown] public IEnumerator RestoreFps() { Application.targetFrameRate = oldFps; yield return null; }
        protected static IEnumerator Until(Func<bool> condition, float timeout = 3)
        {
            double end = Time.unscaledTimeAsDouble + timeout;
            while (!condition() && Time.unscaledTimeAsDouble < end) yield return null;
            Assert.That(condition(), Is.True, "PASS4C condition timed out");
        }
        protected IEnumerator Cooldown() { frame = default; yield return Until(() => player.GroundDashCooldownRemaining <= 0); }
        protected IEnumerator StartInput(Vector2 direction)
        {
            int count = player.GroundDashCount; frame.move = direction; frame.dash = true;
            yield return Until(() => player.GroundDashCount > count);
            frame.move = Vector2.zero; frame.dash = false;
        }
        protected void Select(ShibuyaFighter target)
        {
            camera.transform.position = player.transform.position + new Vector3(0, 1.2f, -5);
            camera.transform.LookAt(target.GetComponent<CharacterController>().bounds.center);
            input.LockOn.Clear(); input.LockOn.HandleButton(true, Time.unscaledTimeAsDouble); input.LockOn.HandleButton(false, Time.unscaledTimeAsDouble);
            Assert.That(input.LockOn.LockedTarget, Is.SameAs(target));
        }
        protected static readonly BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        protected static void Set(ShibuyaFighter owner, string field, object value) => typeof(ShibuyaFighter).GetField(field, Private).SetValue(owner, value);
    }
    public sealed class ShibuyaPass4CTests : ShibuyaPass4CFixture
    {
        [UnityTest] public IEnumerator TunedCooldownStartsAtEndAndCancellation()
        {
            var samples = new List<object>();
            foreach (bool cancel in new[] { false, true }) {
                yield return Cooldown();
                Assert.That(player.TryGroundDash(Vector2.right, Vector3.forward, Vector3.right));
                if (cancel) { yield return Wait(.05f); frame.auraHeld = true; }
                yield return Until(() => !player.IsGroundDashing);
                frame = default;
                double observedEnd = Time.timeAsDouble;
                float remaining = player.GroundDashCooldownRemaining;
                Assert.That(remaining, Is.InRange(.39f, .45f));
                Assert.That(player.TryGroundDash(Vector2.left, Vector3.forward, Vector3.right), Is.False);
                yield return Until(() => player.GroundDashCooldownRemaining <= .12f);
                Assert.That(player.TryGroundDash(Vector2.left, Vector3.forward, Vector3.right), Is.False);
                yield return Until(() => player.GroundDashCooldownRemaining <= 0);
                double elapsed = Time.timeAsDouble - observedEnd;
                Assert.That(elapsed, Is.InRange(remaining, remaining + .06));
                Assert.That(player.TryGroundDash(Vector2.left, Vector3.forward, Vector3.right), Is.True);
                samples.Add(new { cancel, remainingAtEnd = remaining, secondsUntilReady = elapsed });
                yield return Until(() => !player.IsGroundDashing);
            }
            Record("4C1-cooldown", samples);
        }
        [UnityTest] public IEnumerator CardinalInputDistancesDurationAndRecovery()
        {
            var data = new List<object>();
            foreach (var direction in new[] { Vector2.up, Vector2.left, Vector2.right, Vector2.down }) {
                yield return Cooldown(); Place(player, new Vector3(0, .03f, -8)); yield return Wait(.05f);
                Vector3 start = player.transform.position; double began = Time.timeAsDouble;
                yield return StartInput(direction); var kind = player.GroundDashKind;
                Assert.That(player.TryAttack(), Is.False);
                yield return Until(() => !player.IsGroundDashing);
                float length = Vector3.ProjectOnPlane(player.transform.position - start, Vector3.up).magnitude;
                float distance = direction.y > 0 ? 5.25f : direction.y < 0 ? 3.3f : 3.6f;
                float duration = direction.y > 0 ? .22f : direction.y < 0 ? .2f : .18f;
                Assert.That(length, Is.EqualTo(distance).Within(.04f));
                Assert.That(Vector3.Dot((player.transform.position - start).normalized, new Vector3(direction.x, 0, direction.y)), Is.GreaterThan(.99f));
                Assert.That(Time.timeAsDouble - began, Is.InRange(duration, duration + .09f));
                Assert.That(player.GroundDashCooldownRemaining, Is.InRange(dash.cooldownAfterEnd - .06f, dash.cooldownAfterEnd));
                data.Add(new { direction = new[] { direction.x, direction.y }, kind = kind.ToString(), length, seconds = Time.timeAsDouble - began });
                Assert.That(player.TryAttack(), Is.True); yield return Until(() => player.ComboStep == 0);
            }
            Record("4C-cardinal", data);
        }
        [UnityTest] public IEnumerator DiagonalsAndNeutralClassifyWithoutSpeedBoost()
        {
            frame.dash = true; yield return Wait(.12f); Assert.That(player.GroundDashCount, Is.Zero); frame.dash = false; yield return null;
            var samples = new List<object>();
            foreach (Vector2 inputDirection in new[] { new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1), new Vector2(-1, -1) }) {
                yield return Cooldown(); Place(player, new Vector3(0, .03f, -8)); yield return Wait(.05f);
                Vector3 start = player.transform.position; yield return StartInput(inputDirection);
                bool protectedFrame = false;
                while (player.IsGroundDashing) { protectedFrame |= player.IsGroundDashEvading; yield return null; }
                float length = Vector3.ProjectOnPlane(player.transform.position - start, Vector3.up).magnitude;
                Assert.That(length, Is.EqualTo(inputDirection.y > 0 ? 5.25f : 3.3f).Within(.04f));
                Assert.That(protectedFrame, Is.EqualTo(inputDirection.y < 0));
                Assert.That(player.GroundDashDirection, Is.EqualTo(new Vector3(inputDirection.normalized.x, 0, inputDirection.normalized.y)));
                samples.Add(new { x = inputDirection.x, y = inputDirection.y, length, protectedFrame });
            }
            Record("4C-diagonal", samples);
        }
        [UnityTest] public IEnumerator HeldInputCooldownAndNoBufferedRepeat()
        {
            frame.move = Vector2.up; frame.dash = true; yield return Until(() => player.IsGroundDashing);
            frame.move = Vector2.zero; yield return Until(() => !player.IsGroundDashing);
            Assert.That(player.TryGroundDash(Vector2.right, Vector3.forward, Vector3.right), Is.False);
            yield return Wait(1); Assert.That(player.GroundDashCount, Is.EqualTo(1));
            frame.move = Vector2.right; yield return Wait(.08f); Assert.That(player.GroundDashCount, Is.EqualTo(1));
            frame.dash = false; yield return Wait(.05f); frame.dash = true;
            yield return Until(() => player.GroundDashCount == 2); frame = default;
            yield return Until(() => !player.IsGroundDashing); Assert.That(player.GroundDashCount, Is.EqualTo(2));
            Record("4C-edge-cooldown", new { count = player.GroundDashCount, cooldown = player.GroundDashCooldownRemaining });
        }
        [UnityTest] public IEnumerator LockedDirectionsSnapshotAndFreeFallback()
        {
            var observations = new List<object>();
            foreach (Vector2 direction in new[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right }) {
                yield return Cooldown(); Place(player, new Vector3(0, .03f, -8)); Place(dummy, new Vector3(5, .03f, -8)); yield return Wait(.08f);
                Select(dummy);
                Vector3 cameraForward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
                Vector3 cameraRight = Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized;
                Vector3 expected = (cameraForward * direction.y + cameraRight * direction.x).normalized;
                yield return StartInput(direction);
                Assert.That(Vector3.Angle(player.GroundDashDirection, expected), Is.LessThan(.1f));
                Place(dummy, new Vector3(0, .03f, -3)); yield return Wait(.04f);
                Assert.That(Vector3.Angle(player.GroundDashDirection, expected), Is.LessThan(.1f));
                Assert.That(Vector3.Angle(player.transform.forward, Vector3.right), Is.LessThan(.1f));
                yield return Until(() => !player.IsGroundDashing);
                observations.Add(new { x = direction.x, y = direction.y, snapshot = new[] { expected.x, expected.z } });
            }
            yield return Cooldown(); input.LockOn.Clear(); camera.transform.rotation = Quaternion.Euler(0, 90, 0);
            yield return StartInput(Vector2.up); Assert.That(Vector3.Angle(player.GroundDashDirection, Vector3.right), Is.LessThan(.1f));
            Record("4C-lock-basis", observations);
        }
        [UnityTest] public IEnumerator DefaultEvadeWindowAndAttackTypePolicy()
        {
            var observations = new List<object>();
            var cases = new[] {
                (Vector2.right,.01f,DamageDeliveryType.PhysicalStrike,DamageTraits.None,false),
                (Vector2.right,.08f,DamageDeliveryType.PhysicalStrike,DamageTraits.None,true),
                (Vector2.right,.16f,DamageDeliveryType.PhysicalStrike,DamageTraits.None,false),
                (Vector2.down,.08f,DamageDeliveryType.PhysicalStrike,DamageTraits.None,true),
                (Vector2.up,.08f,DamageDeliveryType.PhysicalStrike,DamageTraits.None,false),
                (Vector2.right,.08f,DamageDeliveryType.PhysicalStrike,DamageTraits.Unblockable,false),
                (Vector2.right,.08f,DamageDeliveryType.CursedTechnique,DamageTraits.None,false),
                (Vector2.right,.08f,DamageDeliveryType.Environmental,DamageTraits.None,false)
            };
            foreach (var sample in cases) {
                yield return Cooldown(); Place(player, new Vector3(0, .03f, -8)); yield return Wait(.05f);
                Assert.That(player.TryGroundDash(sample.Item1, Vector3.forward, Vector3.right)); double start = Time.timeAsDouble;
                yield return Until(() => Time.timeAsDouble - start >= sample.Item2);
                double age = Time.timeAsDouble - start; float hp = player.Health.CurrentHealth;
                var result = player.Health.ReceiveDamage(new DamageContext(1, dummy.gameObject, sample.Item3, sample.Item4));
                Assert.That(result, Is.EqualTo(sample.Item5 ? DamageResolution.Guarded : DamageResolution.Applied));
                Assert.That(player.Health.CurrentHealth, Is.EqualTo(sample.Item5 ? hp : hp - 1));
                observations.Add(new { age, expectedEvade = sample.Item5, result = result.ToString(), hp = player.Health.CurrentHealth, delta = Time.deltaTime });
                yield return Until(() => !player.IsGroundDashing);
            }
            Record("4C-invulnerability", observations);
        }
        [UnityTest] public IEnumerator ActualHitPipelineDoesNotCountEvadedHit()
        {
            Place(player, new Vector3(0, .03f, -1.4f)); Place(dummy, new Vector3(0, .03f, 0)); yield return Wait(.1f);
            var defenderCombat = Clone(dummy.Profile); defenderCombat.groundDash = Clone(dash); defenderCombat.groundDash.sideDistance = .01f;
            dummy.Configure(defenderCombat, AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root + "/Data/Dummy Stats.asset"));
            combat.strikes[0].hitStopDuration = .065f; int events = 0, guards = 0;
            var moves = ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>(); moves.name = "PASS4B3 QA dash hit fixture";
            moves.finisherStepIndex = 0; moves.steps[0].hits[0].strike = combat.strikes[0];
            var cinema = Clone(AssetDatabase.LoadAssetAtPath<ShibuyaCinematicProfile>(ShibuyaSceneBuilder.Root + "/Data/Shibuya Cinematic Example.asset"));
            cinema.cinematicEnabled = true; moves.steps[0].hits[0].cinematic = cinema;
            Assert.That(player.SetMoveset(moves), Is.True);
            player.HitConfirmed += (_, __) => events++;
            dummy.Health.DamageResolved += (_, __, result) => { if (result == DamageResolution.Guarded) guards++; };
            Assert.That(player.TryAttack()); yield return Until(() => player.AttackElapsed >= .035f);
            Assert.That(dummy.TryGroundDash(Vector2.right, Vector3.forward, Vector3.right));
            yield return Until(() => player.ComboStep == 0);
            Assert.That(guards, Is.EqualTo(1)); Assert.That(dummy.Health.CurrentHealth, Is.EqualTo(1000));
            Assert.That(player.ConfirmedHitCount, Is.Zero); Assert.That(events, Is.Zero);
            Assert.That(player.HitStopCount, Is.Zero); Assert.That(dummy.HitStopCount, Is.Zero);
            Assert.That(dummy.KnockbackVelocity, Is.EqualTo(Vector3.zero)); Assert.That(dummy.State, Is.Not.EqualTo(ShibuyaFighterState.Hitstunned));
            Assert.That(player.IsInCinematic || dummy.IsInCinematic, Is.False);
            Record("4C-real-guard", new { guards, hits = player.ConfirmedHitCount, hp = dummy.Health.CurrentHealth, hitStop = player.HitStopCount });
            // Same authored event without an active evade must produce damage, hit-stop and cinema.
            yield return Wait(.7f); Place(player, new Vector3(0, .03f, -1.4f)); Place(dummy, new Vector3(0, .03f, 0)); yield return Wait(.05f);
            Assert.That(player.TryAttack()); yield return Until(() => player.ConfirmedHitCount == 1);
            Assert.That(dummy.Health.CurrentHealth, Is.EqualTo(980)); Assert.That(player.HitStopCount, Is.EqualTo(1));
            Assert.That(player.IsInCinematic && dummy.IsInCinematic, Is.True);
        }
        [UnityTest] public IEnumerator WallCapsuleCollisionAndBlockedDistanceIsSpent()
        {
            Place(player, new Vector3(0, .03f, -8)); var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = new Vector3(0, 1.5f, -7); wall.transform.localScale = new Vector3(8, 3, .2f); Physics.SyncTransforms();
            yield return StartInput(Vector2.up); yield return Wait(.1f);
            float blockedZ = player.transform.position.z, spent = player.GroundDashRequestedDistance;
            Assert.That(blockedZ, Is.LessThan(-7.4f)); Object.Destroy(wall); yield return null;
            yield return Until(() => !player.IsGroundDashing);
            Assert.That(player.transform.position.z - blockedZ, Is.LessThanOrEqualTo(5.25f - spent + .08f));
            float unbanked = player.transform.position.z - blockedZ;
            yield return Cooldown(); Place(player, new Vector3(0, .03f, -8)); Place(dummy, new Vector3(0, .03f, -6.5f)); yield return Wait(.08f);
            yield return StartInput(Vector2.up); yield return Until(() => !player.IsGroundDashing);
            float separation = Vector3.Distance(player.transform.position, dummy.transform.position);
            Assert.That(player.transform.position.z, Is.LessThan(dummy.transform.position.z));
            Assert.That(separation, Is.GreaterThanOrEqualTo(player.GetComponent<CharacterController>().radius + dummy.GetComponent<CharacterController>().radius));
            Record("4C-collision", new { blockedZ, spent, unbanked, separation });
        }
        [UnityTest] public IEnumerator AttackJumpStunDownHitStopAndDeathRejectDash()
        {
            Assert.That(player.TryAttack());
            foreach (float phase in new[] { .01f, .11f, .23f }) {
                yield return Until(() => player.AttackElapsed >= phase);
                Assert.That(player.TryGroundDash(Vector2.up, Vector3.forward, Vector3.right), Is.False);
            }
            yield return Until(() => player.ComboStep == 0);
            player.ApplyHitReaction(Vector3.zero, .2f); Assert.That(player.TryGroundDash(Vector2.up, Vector3.forward, Vector3.right), Is.False);
            yield return Wait(.25f);
            foreach (string flag in new[] { "downed", "gettingUp" }) {
                Set(player, "downed", true); Set(player, "gettingUp", flag == "gettingUp");
                Assert.That(player.TryGroundDash(Vector2.up, Vector3.forward, Vector3.right), Is.False);
                Set(player, "downed", false); Set(player, "gettingUp", false);
            }
            typeof(ShibuyaFighter).GetMethod("ApplyLocalHitStop", Private).Invoke(player, new object[] { .065f });
            Assert.That(player.TryGroundDash(Vector2.up, Vector3.forward, Vector3.right), Is.False); yield return Wait(.1f);
            frame.jump = true; yield return Wait(.05f); frame.jump = false; yield return Wait(.1f);
            Assert.That(player.IsGrounded, Is.False); Assert.That(player.TryGroundDash(Vector2.up, Vector3.forward, Vector3.right), Is.False);
            yield return Wait(1.4f); player.Health.Kill();
            Assert.That(player.TryGroundDash(Vector2.up, Vector3.forward, Vector3.right), Is.False);
            Assert.That(player.GroundDashCount, Is.Zero); Assert.That(Time.timeScale, Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator FocusChargeWinsAndHudResourceRemainsIndependent()
        {
            input.EnergyCharge.Resource.SetCurrentEnergy(0);
            frame.auraHeld = true; frame.dash = true; frame.move = Vector2.up; yield return Wait(.3f);
            Assert.That(player.GroundDashCount, Is.Zero); Assert.That(input.EnergyCharge.IsCharging, Is.True);
            Assert.That(input.EnergyCharge.Resource.CurrentEnergy, Is.GreaterThan(2));
            Assert.That(Object.FindFirstObjectByType<ShibuyaMatch>().PlayerEnergy, Is.SameAs(input.EnergyCharge.Resource));
            frame = default; yield return Wait(.1f);
            yield return StartInput(Vector2.right); yield return Wait(.03f); frame.auraHeld = true; yield return Wait(.08f);
            Assert.That(player.IsGroundDashing, Is.False); Assert.That(player.IsGroundDashEvading, Is.False);
            Assert.That(input.EnergyCharge.IsCharging, Is.True); Assert.That(aura.Held, Is.True);
            Assert.That(aura.Profile, Is.SameAs(settings)); Assert.That(settings.upperFieldConnection, Is.EqualTo(1));
            Record("4C-F-priority", new { energy = input.EnergyCharge.Resource.CurrentEnergy, holds = input.EnergyCharge.HoldDuration, dashes = player.GroundDashCount });
        }
        [UnityTest] public IEnumerator CinematicCancelsDashAndKeepsCameraOwnership()
        {
            Place(dummy, new Vector3(0, .03f, -5)); yield return StartInput(Vector2.right);
            var p = Clone(AssetDatabase.LoadAssetAtPath<ShibuyaCinematicProfile>(ShibuyaSceneBuilder.Root + "/Data/Shibuya Cinematic Example.asset")); p.cinematicEnabled = true;
            Assert.That(ShibuyaCinematicSession.TryStartAfterApplied(player, dummy, p, DamageResolution.Applied, ShibuyaCinematicTrigger.Default, 100, 0), Is.Not.Null);
            Assert.That(player.IsGroundDashing, Is.False); Assert.That(player.IsGroundDashEvading, Is.False);
            Assert.That(player.TryGroundDash(Vector2.up, Vector3.forward, Vector3.right), Is.False);
            Vector3 position = player.transform.position; yield return Wait(.15f); Assert.That(player.transform.position, Is.EqualTo(position));
            Assert.That(Time.timeScale, Is.EqualTo(1));
            yield return Until(() => !player.IsInCinematic); yield return Wait(.2f);
            var orbit = camera.GetComponent<ShibuyaOrbitCamera>(); Assert.That(orbit.IsCinematicView, Is.False);
        }
        [UnityTest] public IEnumerator StaleDeadLockFallsBackToCamera()
        {
            var other = Object.Instantiate(dummy.gameObject, player.transform.position + Vector3.right * 5, Quaternion.identity).GetComponent<ShibuyaFighter>();
            yield return Wait(.05f); Select(other); camera.transform.rotation = Quaternion.identity;
            other.Health.Kill(); // Not the match's designated dummy: the battle remains live.
            Assert.That(player.TryGroundDash(Vector2.up, Vector3.forward, Vector3.right), Is.True);
            Assert.That(Vector3.Angle(player.GroundDashDirection, Vector3.forward), Is.LessThan(.1f));
            yield return Until(() => !player.IsGroundDashing);
        }
        [UnityTest] public IEnumerator RepeatedDashResourcesAndNormalMovementResume()
        {
            int transforms = player.GetComponentsInChildren<Transform>().Length;
            var meshes = player.GetComponentsInChildren<MeshFilter>().Select(x => x.sharedMesh.GetInstanceID()).ToArray();
            var materials = player.GetComponentsInChildren<Renderer>().Select(x => x.sharedMaterial.GetInstanceID()).ToArray();
            float energy = input.EnergyCharge.Resource.CurrentEnergy;
            for (int i = 0; i < 8; i++) {
                yield return Cooldown(); Assert.That(player.TryGroundDash(i % 2 == 0 ? Vector2.right : Vector2.left, Vector3.forward, Vector3.right));
                yield return Until(() => !player.IsGroundDashing);
            }
            Assert.That(player.GetComponentsInChildren<Transform>().Length, Is.EqualTo(transforms));
            Assert.That(player.GetComponentsInChildren<MeshFilter>().Select(x => x.sharedMesh.GetInstanceID()), Is.EqualTo(meshes));
            Assert.That(player.GetComponentsInChildren<Renderer>().Select(x => x.sharedMaterial.GetInstanceID()), Is.EqualTo(materials));
            Assert.That(input.EnergyCharge.Resource.CurrentEnergy, Is.EqualTo(energy));
            Vector3 start = player.transform.position; frame.move = Vector2.up; yield return Wait(.25f); frame.move = Vector2.zero;
            float walk = Vector3.ProjectOnPlane(player.transform.position - start, Vector3.up).magnitude;
            Assert.That(walk, Is.GreaterThan(.8f)); Assert.That(player.GroundDashCount, Is.EqualTo(8));
            Record("4C-resources-resume", new { cycles = 8, transforms, meshBindings = meshes.Length, materialBindings = materials.Length, walk, energy });
        }
        [UnityTest] public IEnumerator ActualAirChaseAndAirAttackRejectGroundDash()
        {
            Place(player, new Vector3(0, .03f, -1.4f)); Place(dummy, new Vector3(0, .03f, 0)); yield return Wait(.1f);
            Assert.That(player.TryAttack());
            for (int step = 1; step <= 3; step++) {
                yield return Until(() => player.AcceptedAttackCount == step && player.AttackElapsed >= player.AttackDuration - .12f);
                Assert.That(player.TryAttack(ShibuyaFinisher.Uppercut)); yield return Until(() => player.AcceptedAttackCount == step + 1);
            }
            yield return Until(() => player.ConfirmedHitCount == 4);
            player.SetIntent(Vector3.zero, false, true);
            Assert.That(player.TryGroundDash(Vector2.up, Vector3.forward, Vector3.right), Is.False);
            yield return Until(() => player.IsChasing);
            Assert.That(player.TryGroundDash(Vector2.up, Vector3.forward, Vector3.right), Is.False);
            yield return Until(() => player.CanAirAttack);
            Assert.That(player.TryGroundDash(Vector2.up, Vector3.forward, Vector3.right), Is.False);
            Assert.That(player.TryAttack()); Assert.That(player.TryGroundDash(Vector2.up, Vector3.forward, Vector3.right), Is.False);
            yield return Until(() => player.ConfirmedHitCount > 4);
            Assert.That(player.GroundDashCount, Is.Zero);
            Record("4C-air-ownership", new { chaseCount = player.AirChaseCount, contacts = player.AirChaseContactCount, hits = player.ConfirmedHitCount, dashes = player.GroundDashCount });
        }
    }

    public sealed class ShibuyaPass4C2Tests : ShibuyaPass4CFixture
    {
        [UnityTest] public IEnumerator OriginalSpeedFreeLockedDiagonalNearFarAndLook()
        {
            foreach(string scenario in new[] {"free-WD","locked-WD","locked-near-D","locked-far-D","live-camera-WD"}) {
                yield return Cooldown();frame=default;input.LockOn.Clear();
                Place(player,new Vector3(0,.03f,-8));Place(dummy,new Vector3(0,.03f,scenario=="locked-far-D"?17: -3));yield return Wait(.08f);
                if(scenario!="free-WD")Select(dummy);
                var orbit=camera.GetComponent<ShibuyaOrbitCamera>();orbit.enabled=false;
                camera.transform.position=player.transform.position+new Vector3(0,4,-9);camera.transform.LookAt(player.transform.position+Vector3.up);
                player.transform.rotation=Quaternion.identity;
                string folder="4C2-media/"+scenario;var samples=new List<object>();double began=Time.timeAsDouble;bool pressed=false;int index=0;
                while(Time.timeAsDouble-began<1.8) {
                    double t=Time.timeAsDouble-began;bool press=!pressed&&t>=.35;
                    frame.move=press?(scenario.EndsWith("-D")?Vector2.right:Vector2.one):Vector2.zero;frame.dash=press;
                    if(press)pressed=true;
                    if(scenario=="live-camera-WD"&&pressed) {orbit.enabled=true;frame.look=new Vector2(1.5f,.15f);}
                    else {camera.transform.position=player.transform.position+new Vector3(0,4,-9);camera.transform.LookAt(player.transform.position+Vector3.up);}
                    Capture(folder+"/frame-"+index.ToString("D4"));
                    samples.Add(new {index,time=t,x=player.transform.position.x,y=player.transform.position.y,z=player.transform.position.z,
                        inputC=press,dash=player.IsGroundDashing,dx=player.GroundDashDirection.x,dz=player.GroundDashDirection.z,
                        cameraYaw=camera.transform.eulerAngles.y,locked=input.LockOn.LockedTarget!=null});
                    index++;yield return Wait(1f/30);
                }
                Record(folder+"-timestamps",samples);frame=default;orbit.enabled=false;
            }
        }
        [UnityTest] public IEnumerator EightInputsMatchForFreeAndLockedCameraBasis()
        {
            var samples = new List<object>();
            foreach (Vector2 direction in new[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right,
                new Vector2(1,1), new Vector2(-1,1), new Vector2(1,-1), new Vector2(-1,-1) }) {
                Vector3 free = Vector3.zero;
                foreach (bool locked in new[] { false, true }) {
                    yield return Cooldown(); frame = default; input.LockOn.Clear();
                    Place(player, new Vector3(0,.03f,-8)); Place(dummy, new Vector3(5,.03f,-8)); yield return Wait(.05f);
                    if (locked) Select(dummy);
                    camera.transform.rotation = Quaternion.Euler(25,37,0); player.transform.rotation = Quaternion.Euler(0,-60,0);
                    Vector3 f = Vector3.ProjectOnPlane(camera.transform.forward,Vector3.up).normalized;
                    Vector3 r = Vector3.ProjectOnPlane(camera.transform.right,Vector3.up).normalized;
                    Vector3 expected = (f*direction.y+r*direction.x).normalized;
                    yield return StartInput(direction);
                    Assert.That(Vector3.Angle(player.GroundDashDirection,expected),Is.LessThan(.1f));
                    if (!locked) free=player.GroundDashDirection;
                    else {
                        Assert.That(Vector3.Angle(player.GroundDashDirection,free),Is.LessThan(.1f));
                        Assert.That(input.LockOn.LockedTarget,Is.SameAs(dummy));
                    }
                    yield return Until(()=>!player.IsGroundDashing);
                    Assert.That(Vector3.Angle(player.transform.forward,locked?Vector3.right:Quaternion.Euler(0,-60,0)*Vector3.forward),Is.LessThan(.1f));
                    samples.Add(new { x=direction.x,y=direction.y,locked,dx=expected.x,dz=expected.z,kind=player.GroundDashKind.ToString() });
                }
            }
            Record("4C2-eight-directions",samples);
        }
        [UnityTest] public IEnumerator ExtremePitchAndRearCameraNeverUseTargetTravelBasis()
        {
            var samples=new List<object>();
            foreach (Vector2 angle in new[] { new Vector2(180,20),new Vector2(90,85),new Vector2(-90,-75),new Vector2(37,90) }) {
                yield return Cooldown(); input.LockOn.Clear(); frame=default;
                Place(player,new Vector3(0,.03f,-8)); Place(dummy,new Vector3(5,.03f,-8)); yield return Wait(.05f); Select(dummy);
                camera.transform.rotation=Quaternion.Euler(angle.y,angle.x,0);
                Vector3 f=Quaternion.Euler(0,angle.x,0)*Vector3.forward,r=Quaternion.Euler(0,angle.x,0)*Vector3.right;
                Vector3 expected=(f+r).normalized; Vector3 start=player.transform.position;
                yield return StartInput(Vector2.one);
                Assert.That(Vector3.Angle(player.GroundDashDirection,expected),Is.LessThan(.1f));
                Assert.That(Mathf.Abs(player.GroundDashDirection.y),Is.LessThan(.00001f));
                yield return Until(()=>!player.IsGroundDashing);
                Vector3 delta=Vector3.ProjectOnPlane(player.transform.position-start,Vector3.up);
                Assert.That(Vector3.Angle(delta,expected),Is.LessThan(.1f));
                samples.Add(new {yaw=angle.x,pitch=angle.y,dx=delta.x,dz=delta.z,length=delta.magnitude});
            }
            Record("4C2-camera-angles",samples);
        }
        [UnityTest] public IEnumerator NearFarMovingTargetAndLiveCameraKeepInitialPath()
        {
            var samples=new List<object>();
            foreach (float range in new[] { 3f,25f }) {
                yield return Cooldown(); frame=default;input.LockOn.Clear();
                Place(player,new Vector3(0,.03f,-8));Place(dummy,new Vector3(range,.03f,-8));yield return Wait(.05f);Select(dummy);
                camera.transform.rotation=Quaternion.Euler(25,0,0);
                Vector3 start=player.transform.position; yield return StartInput(Vector2.right);
                Vector3 direction=player.GroundDashDirection;
                Assert.That(Vector3.Angle(direction,Vector3.right),Is.LessThan(.1f));
                // Target moves and the real lock camera receives manual look while dash is live.
                Place(dummy,new Vector3(range,.03f,-3));
                var orbit=camera.GetComponent<ShibuyaOrbitCamera>(); orbit.enabled=true; frame.look=new Vector2(20,5);
                while(player.IsGroundDashing) {
                    Assert.That(Vector3.Angle(player.GroundDashDirection,direction),Is.LessThan(.1f));yield return null;
                }
                frame=default;orbit.enabled=false;
                Vector3 delta=Vector3.ProjectOnPlane(player.transform.position-start,Vector3.up);
                // Near target collision may shorten travel, but cannot redirect the requested path.
                Assert.That(delta.x,Is.GreaterThan(0));Assert.That(Mathf.Abs(delta.z),Is.LessThan(.04f));
                Assert.That(input.LockOn.LockedTarget,Is.SameAs(dummy));
                samples.Add(new {range,dx=delta.x,dz=delta.z,cameraYaw=camera.transform.eulerAngles.y});
            }
            Record("4C2-live-snapshot",samples);
        }
        [UnityTest] public IEnumerator LockedAttackAfterDashStillFacesTarget()
        {
            Select(dummy);camera.transform.rotation=Quaternion.Euler(0,180,0);
            yield return StartInput(Vector2.left);yield return Until(()=>!player.IsGroundDashing);
            Place(dummy,player.transform.position+Vector3.right*1.4f);yield return Wait(.05f);
            Assert.That(player.TryAttack(),Is.True);
            yield return Until(()=>player.ConfirmedHitCount>0);
            Assert.That(Vector3.Angle(player.transform.forward,Vector3.right),Is.LessThan(10));
            Record("4C2-post-dash-attack",new {hits=player.ConfirmedHitCount,hp=dummy.Health.CurrentHealth});
        }
    }

    public sealed class ShibuyaPass4CMedia : ShibuyaPass4CFixture
    {
        [UnityTest] public IEnumerator TunedOriginalSpeedNearFarBackAndRepeat()
        {
            yield return TunedClips(new[] { "free-forward", "lock-near-side", "lock-far-side", "free-back", "cooldown-repeat" });
        }
        [UnityTest] public IEnumerator TunedFarSideUnobstructedFraming()
        {
            yield return TunedClips(new[] { "lock-far-side" });
        }
        private IEnumerator TunedClips(string[] scenarios)
        {
            foreach (string scenario in scenarios) {
                yield return Cooldown(); frame = default; input.LockOn.Clear();
                Place(player, new Vector3(0, .03f, -8));
                Place(dummy, new Vector3(scenario == "lock-far-side" ? 25 : 5, .03f, -8));
                var orbit = camera.GetComponent<ShibuyaOrbitCamera>(); orbit.enabled = false;
                camera.transform.rotation = Quaternion.identity; player.transform.rotation = Quaternion.identity;
                yield return Wait(.1f);
                if (scenario.StartsWith("lock-")) Select(dummy);
                string folder = "4C1-media/" + scenario; var samples = new List<object>();
                double start = Time.timeAsDouble; int index = 0, presses = 0;
                while (Time.timeAsDouble - start < (scenario == "cooldown-repeat" ? 3.0 : 1.6)) {
                    double t = Time.timeAsDouble - start;
                    bool press = presses == 0 ? t >= .35 : scenario == "cooldown-repeat" && presses < 3 && t >= .35 + presses * .8;
                    frame.move = press ? (scenario.Contains("side") ? Vector2.right : scenario == "free-back" ? Vector2.down : Vector2.up) : Vector2.zero;
                    frame.dash = press; if (press) presses++;
                    Vector3 focus = scenario.StartsWith("lock-") ? (player.transform.position + dummy.transform.position) * .5f : player.transform.position;
                    float span = scenario == "lock-far-side" ? 22 : 9;
                    camera.transform.position = focus + new Vector3(0, span * .5f, scenario == "lock-far-side" ? span : -span);
                    camera.transform.LookAt(focus + Vector3.up);
                    foreach (ShibuyaFighter subject in scenario.StartsWith("lock-") ? new[] { player, dummy } : new[] { player }) {
                        Vector3 screen = camera.WorldToViewportPoint(subject.GetComponent<CharacterController>().bounds.center);
                        Assert.That(screen.z, Is.GreaterThan(0)); Assert.That(screen.x, Is.InRange(.02f, .98f)); Assert.That(screen.y, Is.InRange(.02f, .98f));
                    }
                    Capture(folder + "/frame-" + index.ToString("D4"));
                    samples.Add(new { index, time = t, x = player.transform.position.x, y = player.transform.position.y,
                        z = player.transform.position.z, dash = player.IsGroundDashing, inputC = press, cooldown = player.GroundDashCooldownRemaining });
                    index++; yield return Wait(1f / 30);
                }
                Assert.That(player.GroundDashCount, Is.GreaterThanOrEqualTo(presses));
                Record(folder + "-timestamps", samples); frame = default;
            }
        }
        [UnityTest] public IEnumerator OriginalSpeedFrontDiagonalLockAndRuntimeCamera()
        {
            foreach (string scenario in new[] { "front-free-W", "diagonal-free-A", "locked-D", "runtime-camera-W" }) {
                yield return Cooldown(); Place(player, new Vector3(0, .03f, -8)); Place(dummy, new Vector3(5, .03f, -8));
                input.LockOn.Clear(); frame = default;
                var orbit = camera.GetComponent<ShibuyaOrbitCamera>(); orbit.enabled = false;
                if (scenario == "front-free-W") { camera.transform.position = player.transform.position + new Vector3(0, 3, 7); camera.transform.LookAt(player.transform.position + Vector3.up); }
                else { camera.transform.position = player.transform.position + new Vector3(6, 3, -6); camera.transform.LookAt(player.transform.position + Vector3.up); }
                if (scenario == "locked-D") Select(dummy);
                if (scenario == "runtime-camera-W") orbit.enabled = true;
                Vector3 facing = camera.transform.forward; facing.y = 0; player.transform.rotation = Quaternion.LookRotation(facing);
                yield return Wait(.2f);
                string folder = "4C-media/" + scenario; var samples = new List<object>();
                double start = Time.timeAsDouble; int index = 0; bool pressed = false;
                while (Time.timeAsDouble - start < 2.2) {
                    double t = Time.timeAsDouble - start;
                    if (!pressed && t >= .45) {
                        pressed = true; frame.move = scenario.Contains("-A") ? Vector2.left : scenario.Contains("-D") ? Vector2.right : Vector2.up;
                        frame.dash = true;
                    } else if (pressed) { frame.dash = false; frame.move = Vector2.zero; }
                    // Technical shots keep the actor visible; the fourth clip uses the real orbit camera.
                    if (scenario == "locked-D") {
                        Vector3 middle = (player.transform.position + dummy.transform.position) * .5f;
                        camera.transform.position = middle + new Vector3(0, 4, -10); camera.transform.LookAt(middle + Vector3.up);
                    } else if (scenario != "runtime-camera-W") {
                        camera.transform.position = player.transform.position + (scenario == "front-free-W" ? new Vector3(0, 3, 7) : new Vector3(6, 3, -6));
                        camera.transform.LookAt(player.transform.position + Vector3.up);
                    }
                    Vector3 screen = camera.WorldToViewportPoint(player.GetComponent<CharacterController>().bounds.center);
                    Assert.That(screen.z, Is.GreaterThan(0)); Assert.That(screen.x, Is.InRange(.05f, .95f)); Assert.That(screen.y, Is.InRange(.05f, .95f));
                    Capture(folder + "/frame-" + index.ToString("D4"));
                    samples.Add(new { index, time = Time.timeAsDouble - start, x = player.transform.position.x,
                        y = player.transform.position.y, z = player.transform.position.z, dash = player.IsGroundDashing,
                        evade = player.IsGroundDashEvading, inputC = frame.dash, kind = player.GroundDashKind.ToString() });
                    index++; yield return Wait(1f / 30);
                }
                Record(folder + "-timestamps", samples); orbit.enabled = false; frame = default;
            }
        }
    }
}
