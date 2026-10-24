using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaCinematicQAGuard : MonoBehaviour, IDamageGuard { public DamageGuardDecision EvaluateDamage(DamageContext c) => DamageGuardDecision.Block; }
    public sealed class ShibuyaPass3DTests
    {
        private static ShibuyaMatch M => Object.FindFirstObjectByType<ShibuyaMatch>();
        private static ShibuyaFighter P => M.Player;
        private static ShibuyaFighter D => M.Dummy;
        private static ShibuyaOrbitCamera C => Array.Find(Object.FindObjectsByType<ShibuyaOrbitCamera>(FindObjectsSortMode.None), c => c.LocalOwner == P);
        private static ShibuyaCinematicProfile Film => P.Profile.defaultFinisherCinematic;
        private static string Root => ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass3DQA");

        [Test] public void ProfileKoreanUndoAndDefaultsAreSafe()
        {
            var original = AssetDatabase.LoadAssetAtPath<ShibuyaCinematicProfile>(ShibuyaCinematicExample.Path);
            Assert.That(original.IsValid); Assert.That(original.cinematicEnabled, Is.False);
            var p = AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root + "/Data/Shibuya Combat Profile.asset");
            Assert.That(p.defaultFinisherCinematic, Is.Null);
            foreach (var type in new[] { typeof(ShibuyaCinematicProfile), typeof(ShibuyaCinematicShot) })
                foreach (var field in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                    Assert.That(ShibuyaInspector.KoreanLabel(field.Name), Is.Not.EqualTo(field.Name), field.Name);
            var copy = Object.Instantiate(original); copy.name = "PASS3D QA Undo";
            try {
                var so = new SerializedObject(copy); float before = copy.duration;
                Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup(); so.FindProperty("duration").floatValue = .7f;
                so.ApplyModifiedProperties(); Undo.FlushUndoRecordObjects(); Assert.That(copy.duration, Is.EqualTo(.7f));
                Undo.CollapseUndoOperations(group); Undo.PerformUndo(); Assert.That(copy.duration, Is.EqualTo(before));
                copy.shots[0].distance = float.NaN; Assert.That(copy.IsValid, Is.False);
            } finally { Object.DestroyImmediate(copy); }
        }
        [UnityTest] public IEnumerator FinisherDamageShotsAndCameraRestore() { Open(); yield return new EnterPlayMode(); yield return CameraSequence(); yield return new ExitPlayMode(); }
        private IEnumerator CameraSequence()
        {
            yield return Setup(); float fov = C.GetComponent<Camera>().fieldOfView;
            float yaw = ReadFloat(C, "yaw"), pitch = ReadFloat(C, "pitch");
            yield return Four(); Assert.That(D.Health.CurrentHealth, Is.EqualTo(915));
            Assert.That(P.IsInCinematic && D.Cinematic == P.Cinematic); var session = P.Cinematic;
            Vector3 pa = P.transform.position, da = D.transform.position; float clock = P.CombatTime;
            var seen = new HashSet<int>(); double began = Time.unscaledTimeAsDouble;
            while (session != null && session.Active) {
                if (seen.Add(C.CinematicShotIndex)) Record("camera-shot", "index="+C.CinematicShotIndex+" elapsed="+session.Elapsed+" fov="+C.GetComponent<Camera>().fieldOfView);
                Assert.That(P.TryAttack(), Is.False);
                P.SetIntent(Vector3.right, true, true); yield return null;
                Assert.That(Vector3.Distance(pa, P.transform.position), Is.LessThan(.001f));
                Assert.That(Vector3.Distance(da, D.transform.position), Is.LessThan(.001f));
                Assert.That(P.CombatTime, Is.EqualTo(clock));
            }
            CollectionAssert.IsSubsetOf(new[] { 0, 1, 2 }, seen);
            float duration = (float)(Time.unscaledTimeAsDouble-began); Assert.That(duration, Is.InRange(.48f,.62f));
            Record("measured-session-duration", duration.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            yield return Wait(.2f); Assert.That(!C.IsCinematicView && !C.IsReturningView);
            Assert.That(C.GetComponent<Camera>().fieldOfView, Is.EqualTo(fov).Within(.001f));
            Assert.That(ReadFloat(C, "yaw"), Is.EqualTo(yaw)); Assert.That(ReadFloat(C, "pitch"), Is.EqualTo(pitch));
            Assert.That(P.ConfirmedHitCount, Is.EqualTo(4)); Assert.That(D.Health.CurrentHealth, Is.EqualTo(915));
            Assert.That(Time.timeScale, Is.EqualTo(1)); Record("finisher-shots", "3 shots, 85 total damage, local clock/motion held, FOV/yaw/pitch restored");
        }
        [UnityTest] public IEnumerator MissImmuneGuardedAndOffDoNotStart() { Open(); yield return new EnterPlayMode(); yield return Rejections(); yield return new ExitPlayMode(); }
        private IEnumerator Rejections()
        {
            for (int mode = 0; mode < 4; mode++) {
                yield return Setup(); if (mode == 0) Place(D, new Vector3(12, .03f, 0));
                if (mode == 1) D.Health.GrantInvulnerability(5);
                var guard = mode == 2 ? D.gameObject.AddComponent<ShibuyaCinematicQAGuard>() : null;
                if (mode == 3) Film.cinematicEnabled = false;
                int before = P.AcceptedAttackCount; yield return Four(false);
                yield return Until(() => P.State == ShibuyaFighterState.Ready);
                Assert.That(P.AcceptedAttackCount, Is.EqualTo(before + 4)); Assert.That(!P.IsInCinematic && !C.IsCinematicView);
                Assert.That(D.Health.CurrentHealth, Is.EqualTo(mode == 3 ? 915 : 1000));
                if (guard != null) Object.Destroy(guard); yield return null;
            }
        }
        [UnityTest] public IEnumerator ThirdPartyActualHitsAndDirectReactionsAreBlocked() { Open(); yield return new EnterPlayMode(); yield return Protection(); yield return new ExitPlayMode(); }
        private IEnumerator Protection()
        {
            yield return Setup(); Film.duration = 1; var third = Actor(new Vector3(0, .03f, 1.4f)); third.transform.rotation = Quaternion.Euler(0, 180, 0);
            var s = Start(P, D); Assert.That(s != null); float hp = D.Health.CurrentHealth; int reactions = 0;
            D.HitReacted += _ => reactions++;
            Assert.That(third.TryAttack()); yield return Wait(.25f);
            Assert.That(third.ConfirmedHitCount, Is.Zero); Assert.That(D.Health.CurrentHealth, Is.EqualTo(hp)); Assert.That(reactions, Is.Zero);
            Assert.That(P.Health.ReceiveDamage(new DamageContext(5, third.gameObject, DamageDeliveryType.CursedTechnique)), Is.EqualTo(DamageResolution.Guarded));
            Vector3 v = D.KnockbackVelocity; D.ApplyHitReaction(Vector3.right * 20, 1);
            Assert.That(D.KnockbackVelocity, Is.EqualTo(v)); Assert.That(reactions, Is.Zero);
            Assert.That(s.ApplyAuthoredDamage(P, D, 7), Is.EqualTo(DamageResolution.Applied)); Assert.That(D.Health.CurrentHealth, Is.EqualTo(hp - 7));
            Assert.That(s.Active); s.Finish("test-done");
        }
        [UnityTest] public IEnumerator ThirdPairCombatMovementAndCameraContinue() { Open(); yield return new EnterPlayMode(); yield return OtherPair(); yield return new ExitPlayMode(); }
        private IEnumerator OtherPair()
        {
            yield return Setup(); Film.duration = 1;
            var e = Actor(new Vector3(10, .03f, -1.4f)); var f = Actor(new Vector3(10, .03f, 0));
            var cameraObject = new GameObject("PASS3D QA Other Camera", typeof(Camera));
            cameraObject.GetComponent<Camera>().enabled = false;
            var otherCamera = cameraObject.AddComponent<ShibuyaOrbitCamera>(); otherCamera.Configure(e.transform, e.Profile);
            yield return null; float fov = cameraObject.GetComponent<Camera>().fieldOfView;
            var s = Start(P, D); Vector3 before = e.transform.position; float clock = e.CombatTime;
            Assert.That(e.TryAttack()); yield return Until(() => e.ConfirmedHitCount == 1);
            Assert.That(f.Health.CurrentHealth, Is.EqualTo(980)); Assert.That(e.CombatTime, Is.GreaterThan(clock));
            Assert.That((e.transform.position - before).magnitude, Is.GreaterThan(.05f));
            Assert.That(!otherCamera.IsCinematicView && !otherCamera.IsReturningView); Assert.That(cameraObject.GetComponent<Camera>().fieldOfView, Is.EqualTo(fov));
            Assert.That(s.Active); Assert.That(Time.timeScale, Is.EqualTo(1)); s.Finish("test-done");
        }
        [UnityTest] public IEnumerator SessionsCannotShareActorsOrRepeatEvent() { Open(); yield return new EnterPlayMode(); yield return SessionOwnership(); yield return new ExitPlayMode(); }
        private IEnumerator SessionOwnership()
        {
            yield return Setup(); var e = Actor(new Vector3(10, .03f, -1.4f)); var f = Actor(new Vector3(10, .03f, 0));
            var first = Start(P, D); Assert.That(first != null);
            Assert.That(ShibuyaCinematicSession.TryStartAfterApplied(P, e, Film, DamageResolution.Applied, ShibuyaCinematicTrigger.Default, 902, 0), Is.Null);
            var second = Start(e, f, 903); Assert.That(second != null && second != first);
            first.Finish("cancel-first"); Assert.That(second.Active); Assert.That(!P.IsInCinematic && !D.IsInCinematic);
            Assert.That(Start(P, D), Is.Null, "same action/event cannot reacquire after cancellation");
            var next = Start(P, D, 904); Assert.That(next != null && C.IsCinematicView, "next session can preempt return blend without losing original camera state");
            next.Finish("test-done"); second.Finish("test-done"); yield return null;
        }
        [UnityTest] public IEnumerator MultipleHitTargetsKeepDamageAndOnlyTwoParticipate() { Open(); yield return new EnterPlayMode(); yield return MultipleTargets(); yield return new ExitPlayMode(); }
        private IEnumerator MultipleTargets()
        {
            yield return Setup(); var extra = Actor(new Vector3(.8f, .03f, .3f));
            int before = extra.ConfirmedHitCount; Assert.That(P.TryAttack());
            for (int i = 1; i <= 3; i++) { yield return Until(() => P.ComboStep == i && P.AttackElapsed >= P.AttackDuration - .12f); Assert.That(P.TryAttack()); yield return Until(() => P.ComboStep == i + 1); }
            // Put both eligible recipients in the final real overlap; no gameplay teleport is introduced.
            Place(D, P.transform.position + Vector3.forward * 1.2f); Place(extra, P.transform.position + Vector3.forward * 1.2f + Vector3.right * .75f);
            float d = D.Health.CurrentHealth, e = extra.Health.CurrentHealth;
            yield return Until(() => P.IsInCinematic);
            Assert.That(D.Health.CurrentHealth, Is.EqualTo(d - 25)); Assert.That(extra.Health.CurrentHealth, Is.EqualTo(e - 25));
            Assert.That(P.Cinematic.Victim, Is.EqualTo(D)); Assert.That(extra.IsInCinematic, Is.False);
        }
        [UnityTest] public IEnumerator LethalFinisherPreservesImmediateBattleEnd() { Open(); yield return new EnterPlayMode(); yield return Lethal(); yield return new ExitPlayMode(); }
        private IEnumerator Lethal()
        {
            yield return Setup(); D.Health.SetCurrentHealth(85); yield return Four(false); yield return Until(() => M.Finished);
            Assert.That(M.Victory); Assert.That(!P.IsInCinematic && !D.IsInCinematic && !C.IsCinematicView);
            Assert.That(D.Health.CurrentHealth, Is.Zero); Assert.That(P.ConfirmedHitCount, Is.EqualTo(4));
        }
        [UnityTest] public IEnumerator EnvironmentDeathEndsSessionAndRestartIsClean() { Open(); yield return new EnterPlayMode(); yield return DeathRestart(); yield return new ExitPlayMode(); }
        private IEnumerator DeathRestart()
        {
            yield return Setup(); Start(P, D); D.Health.Kill(); Assert.That(M.Finished && !P.IsInCinematic);
            yield return Wait(.2f); Assert.That(!C.IsCinematicView && !C.IsReturningView);
            var old = M; old.RestartBattle(); yield return Until(() => M != null && M != old);
            Assert.That(P.Health.CurrentHealth, Is.EqualTo(100)); Assert.That(D.Health.CurrentHealth, Is.EqualTo(1000));
            Assert.That(!P.IsInCinematic && !D.IsInCinematic && !M.Finished); Assert.That(Object.FindObjectsByType<ShibuyaCinematicSession>(FindObjectsSortMode.None).Length, Is.Zero);
            yield return Setup(); Start(P, D); P.Health.Kill(); Assert.That(M.Finished && !M.Victory && !D.IsInCinematic);
        }
        [UnityTest] public IEnumerator DisableCameraRebindAndSceneExitReleaseOwnership() { Open(); yield return new EnterPlayMode(); yield return Disabled(); yield return new ExitPlayMode(); }
        private IEnumerator Disabled()
        {
            yield return Setup(); var view = C; float fov = view.GetComponent<Camera>().fieldOfView; var s = Start(P, D); yield return null;
            D.enabled = false; Assert.That(!P.IsInCinematic); yield return Wait(.2f); Assert.That(!view.IsCinematicView && !view.IsReturningView);
            D.enabled = true; s = Start(P, D, 902); yield return null; view.enabled = false;
            Assert.That(view.GetComponent<Camera>().fieldOfView, Is.EqualTo(fov)); view.enabled = true;
            s.Finish("test-done"); yield return Wait(.2f);
            s = Start(P, D, 903); var other = Actor(new Vector3(10, .03f, 0)); view.Configure(other.transform, other.Profile);
            Assert.That(!view.IsCinematicView); Assert.That(view.GetComponent<Camera>().fieldOfView, Is.EqualTo(fov));
            s.Finish("test-done");
        }
        [UnityTest] public IEnumerator WallsAndSeparationCancelWithoutActorSnapping() { Open(); yield return new EnterPlayMode(); yield return Walls(); yield return new ExitPlayMode(); }
        private IEnumerator Walls()
        {
            yield return Setup(); var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.transform.position = new Vector3(0, 1, -.7f); wall.transform.localScale = new Vector3(3, 3, .1f); Physics.SyncTransforms();
            Assert.That(Start(P, D), Is.Null); Object.Destroy(wall); yield return null;
            Place(D, new Vector3(12, .03f, 0)); Assert.That(Start(P, D, 902), Is.Null);
            Place(D, new Vector3(0, .03f, 0)); var s = Start(P, D, 903); Assert.That(s != null);
            Vector3 a = P.transform.position, b = D.transform.position;
            var cameraWall = GameObject.CreatePrimitive(PrimitiveType.Cube); cameraWall.transform.position = new Vector3(-2, 1.5f, -1); cameraWall.transform.localScale = new Vector3(.25f, 4, 4); Physics.SyncTransforms();
            yield return Wait(.1f); Assert.That(cameraWall.GetComponent<Collider>().bounds.Contains(C.transform.position), Is.False);
            Assert.That(P.transform.position, Is.EqualTo(a)); Assert.That(D.transform.position, Is.EqualTo(b));
            Place(D, new Vector3(15, .03f, 0)); yield return Until(() => !P.IsInCinematic);
            yield return Wait(.2f); Assert.That(!C.IsCinematicView && !C.IsReturningView);
        }
        [UnityTest] public IEnumerator UppercutCinematicResumesLaunchPursuitAndAirAttack() { Open(); yield return new EnterPlayMode(); yield return Upper(); yield return new ExitPlayMode(); }
        private IEnumerator Upper()
        {
            yield return Setup(); CloneFinishers(); Film.triggers |= ShibuyaCinematicTrigger.Uppercut;
            P.Profile.finishers.uppercut.hits[0].cinematic = Film;
            yield return Four(true, ShibuyaFinisher.Uppercut); Assert.That(D.IsAirborne);
            yield return Until(() => !P.IsInCinematic); P.SetIntent(Vector3.zero, false, true); yield return Until(() => P.CanAirAttack);
            Assert.That(P.TryAttack()); float y = P.transform.position.y; yield return Wait(.2f);
            Assert.That(P.transform.position.y, Is.EqualTo(y).Within(.001f)); yield return Until(() => P.IsGrounded && !P.IsInAirSession);
            Assert.That(D.Health.CurrentHealth, Is.EqualTo(890));
        }
        [UnityTest] public IEnumerator DownCinematicResumesKnockdownAndRecovery() { Open(); yield return new EnterPlayMode(); yield return Down(); yield return new ExitPlayMode(); }
        private IEnumerator Down()
        {
            yield return Setup(); CloneFinishers(); Film.triggers |= ShibuyaCinematicTrigger.Down;
            P.Profile.finishers.downFinisher.hits[0].cinematic = Film;
            yield return Four(true, ShibuyaFinisher.Down); Assert.That(D.IsDowned); yield return Until(() => !P.IsInCinematic);
            yield return Until(() => !D.IsDowned && D.IsGrounded); Assert.That(D.transform.position.y, Is.GreaterThan(-.1f));
        }
        [UnityTest] public IEnumerator AirHoverDeadlineReleasesAfterCinematicWithoutExtraHits() { Open(); yield return new EnterPlayMode(); yield return Air(); yield return new ExitPlayMode(); }
        private IEnumerator Air()
        {
            yield return Setup(); Film.duration = 1.3f; Film.triggers |= ShibuyaCinematicTrigger.Authored;
            P.Profile.aerialCombat = Object.Instantiate(P.Profile.aerialCombat); P.Profile.aerialCombat.name = "PASS3D QA Air";
            P.Profile.aerialCombat.aerialAttack.hits[0].cinematic = Film;
            P.SetIntent(Vector3.zero, false, true); D.SetIntent(Vector3.zero, false, true); yield return Until(() => P.transform.position.y > .7f && D.transform.position.y > .7f);
            Assert.That(P.TryAttack()); yield return Until(() => P.IsInCinematic);
            yield return Until(() => !P.IsInCinematic); yield return Until(() => P.IsGrounded && !P.IsInAirSession);
            Assert.That(P.ConfirmedHitCount, Is.EqualTo(1)); Assert.That(D.Health.CurrentHealth, Is.EqualTo(988)); Assert.That(!D.IsHeldByAerialHit);
        }
        [UnityTest] public IEnumerator AuthoredPermitIsSingleUseAndEnvironmentStillWins() { Open(); yield return new EnterPlayMode(); yield return Permit(); yield return new ExitPlayMode(); }
        private IEnumerator Permit()
        {
            yield return Setup(); var s = Start(P, D); float hp = D.Health.CurrentHealth; DamageResolution nested = DamageResolution.Invalid;
            Action<Health, DamageContext, DamageResolution> callback = (h, c, r) => { if (r == DamageResolution.Applied) nested = h.ReceiveDamage(new DamageContext(10, P.gameObject, DamageDeliveryType.PhysicalStrike)); };
            D.Health.DamageResolved += callback;
            Assert.That(s.ApplyAuthoredDamage(P, D, 7), Is.EqualTo(DamageResolution.Applied)); D.Health.DamageResolved -= callback;
            Assert.That(nested, Is.EqualTo(DamageResolution.Guarded)); Assert.That(D.Health.CurrentHealth, Is.EqualTo(hp - 7));
            Film.allowAuthoredDamage = false; Assert.That(s.ApplyAuthoredDamage(P, D, 7), Is.EqualTo(DamageResolution.Invalid));
            var guard = D.gameObject.AddComponent<ShibuyaCinematicQAGuard>(); Film.allowAuthoredDamage = true;
            Assert.That(s.ApplyAuthoredDamage(P, D, 7), Is.EqualTo(DamageResolution.Guarded)); Object.Destroy(guard); yield return null;
            Assert.That(D.Health.ReceiveDamage(new DamageContext(1, null, DamageDeliveryType.Environmental)), Is.EqualTo(DamageResolution.Applied));
            Assert.That(!P.IsInCinematic && !D.IsInCinematic && !M.Finished);
        }
        [UnityTest] public IEnumerator InputCancelDoesNotReplayAndExampleToggleLeavesAssetsAlone() { Open(); yield return new EnterPlayMode(); yield return CancelAndMenu(); yield return new ExitPlayMode(); }
        private IEnumerator CancelAndMenu()
        {
            yield return Setup(); Film.cancelOnAttackInput = true; var s = Start(P, D); int count = P.AcceptedAttackCount;
            Assert.That(P.TryAttack(), Is.False); Assert.That(!P.IsInCinematic); Assert.That(P.AcceptedAttackCount, Is.EqualTo(count));
            yield return Wait(.2f); Film.cancelOnAttackInput = false; Film.cancelOnMoveInput = true; Start(P, D, 902);
            P.SetIntent(Vector3.right, true, false); Assert.That(!P.IsInCinematic); yield return Wait(.2f);
            var before = P.Profile; ShibuyaCinematicExample.Enable(); Assert.That(P.Profile, Is.Not.SameAs(before));
            Assert.That(P.Profile.defaultFinisherCinematic.cinematicEnabled); ShibuyaCinematicExample.Disable(); Assert.That(P.Profile, Is.SameAs(before));
            Assert.That(AssetDatabase.LoadAssetAtPath<ShibuyaCinematicProfile>(ShibuyaCinematicExample.Path).cinematicEnabled, Is.False);
        }
        [UnityTest] public IEnumerator OptionalPhysicsAndFacingStillRespectControllerWalls() { Open(); yield return new EnterPlayMode(); yield return PhysicsPolicy(); yield return new ExitPlayMode(); }
        private IEnumerator PhysicsPolicy()
        {
            yield return Setup(); Film.holdMotion = Film.holdFacing = false;
            P.ApplyHitReaction(Vector3.right * 4, .1f);
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube); wall.transform.position = P.transform.position + new Vector3(.7f, 1, 0); wall.transform.localScale = new Vector3(.2f, 4, 4); Physics.SyncTransforms();
            var session = Start(P, D); Assert.That(session != null); Vector3 before = P.transform.position;
            var view = C;
            P.GetComponent<ShibuyaPlayerInput>().TestInput = () => {
                Vector3 f = view.transform.forward; f.y = 0; f.Normalize(); Vector3 r = view.transform.right; r.y = 0; r.Normalize();
                return new ShibuyaInputFrame { move = new Vector2(Vector3.Dot(Vector3.right, r), Vector3.Dot(Vector3.right, f)) };
            };
            yield return Wait(.2f);
            float moved = P.transform.position.x - before.x;
            Assert.That(moved, Is.InRange(.01f, .3f)); Assert.That(Vector3.Angle(P.transform.forward, Vector3.right), Is.LessThan(10));
            Assert.That(wall.GetComponent<Collider>().bounds.Contains(P.transform.position), Is.False);
            session.Finish("test-done");
        }
        [UnityTest] public IEnumerator DisabledProfileAndActiveSceneReloadLeaveNoSession() { Open(); yield return new EnterPlayMode(); yield return SceneExit(); yield return new ExitPlayMode(); }
        private IEnumerator SceneExit()
        {
            yield return Setup(); Start(P, D); Film.cinematicEnabled = false; yield return Until(() => !P.IsInCinematic);
            yield return Wait(.2f); Film.cinematicEnabled = true; Start(P, D, 902);
            var previous = M; EditorSceneManager.LoadSceneInPlayMode(M.gameObject.scene.path, new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
            yield return Until(() => M != null && M != previous); yield return null;
            Assert.That(!C.IsCinematicView && !C.IsReturningView && !P.IsInCinematic && !D.IsInCinematic);
            Assert.That(P.Profile.defaultFinisherCinematic, Is.Null); Assert.That(Object.FindObjectsByType<ShibuyaCinematicSession>(FindObjectsSortMode.None).Length, Is.Zero);
        }
        private static void CloneFinishers() { P.Profile.finishers = Object.Instantiate(P.Profile.finishers); P.Profile.finishers.name = "PASS3D QA Finishers"; }
        private static ShibuyaCinematicSession Start(ShibuyaFighter a, ShibuyaFighter b, int action = 901)
        {
            var result = b.Health.ReceiveDamage(new DamageContext(1, a.gameObject, DamageDeliveryType.PhysicalStrike));
            return ShibuyaCinematicSession.TryStartAfterApplied(a, b, Film, result, ShibuyaCinematicTrigger.Default, action, 0);
        }
        private static IEnumerator Setup()
        {
            Application.runInBackground = true; yield return Wait(.1f);
            P.GetComponent<ShibuyaPlayerInput>().TestInput = () => default;
            if (P.IsInCinematic) P.Cinematic.Finish("setup");
            var profile = Object.Instantiate(AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root + "/Data/Shibuya Combat Profile.asset")); profile.name = "PASS3D QA Combat";
            var film = Object.Instantiate(AssetDatabase.LoadAssetAtPath<ShibuyaCinematicProfile>(ShibuyaCinematicExample.Path)); film.name = "PASS3D QA Camera"; film.cinematicEnabled = true;
            profile.defaultFinisherCinematic = film;
            P.Configure(profile, AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root + "/Data/Player Stats.asset"));
            P.Health.ResetHealth(); D.Health.ResetHealth();
            Place(P, new Vector3(0, .03f, -1.4f)); Place(D, new Vector3(0, .03f, 0)); P.transform.rotation = Quaternion.identity; yield return null;
        }
        private static ShibuyaFighter Actor(Vector3 position) { var actor = Object.Instantiate(D.gameObject, position, Quaternion.identity).GetComponent<ShibuyaFighter>(); actor.name = "PASS3D QA Other"; return actor; }
        private static IEnumerator Four(bool expectSession = true, ShibuyaFinisher finisher = ShibuyaFinisher.Default)
        {
            Assert.That(P.TryAttack());
            for (int i = 1; i <= 3; i++) { yield return Until(() => P.ComboStep == i && P.AttackElapsed >= P.AttackDuration - .12f); Assert.That(P.TryAttack(finisher)); yield return Until(() => P.ComboStep == i + 1); }
            if (expectSession) yield return Until(() => P.IsInCinematic);
            else yield return Wait(.3f);
        }
        private static float ReadFloat(object obj, string field) => (float)obj.GetType().GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(obj);
        private static void Open() => EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);
        private static void Place(ShibuyaFighter f, Vector3 p) { var motor = f.GetComponent<CharacterController>(); motor.enabled = false; f.transform.position = p; motor.enabled = true; Physics.SyncTransforms(); }
        private static IEnumerator Wait(float seconds) { double until = Time.realtimeSinceStartupAsDouble + seconds; while (Time.realtimeSinceStartupAsDouble < until) yield return null; }
        private static IEnumerator Until(Func<bool> condition) { double until = Time.realtimeSinceStartupAsDouble + 9; while (!condition() && Time.realtimeSinceStartupAsDouble < until) yield return null; Assert.That(condition(), "PASS3D scenario timed out"); }
        private static void Record(string kind, string detail) { Directory.CreateDirectory(Root); File.AppendAllText(Root + "/measurements.jsonl", JsonUtility.ToJson(new Metric { kind=kind, detail=detail, time=Time.unscaledTime }) + Environment.NewLine); }
        [Serializable] private sealed class Metric { public string kind, detail; public float time; }
        [UnityTearDown] public IEnumerator Leave() { if (EditorApplication.isPlaying) yield return new ExitPlayMode(); }
        [TearDown] public void Cleanup() { foreach (var obj in Resources.FindObjectsOfTypeAll<ScriptableObject>()) if (obj.name.StartsWith("PASS3D QA", StringComparison.Ordinal) && !AssetDatabase.Contains(obj)) Object.DestroyImmediate(obj); }
    }
}
