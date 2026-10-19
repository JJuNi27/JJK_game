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
    // Read-only gameplay diagnostics: only transient profile clones and simulated press edges.
    public sealed class ShibuyaPass3CAirCountTests
    {
        private const string Root = "Logs/ShibuyaPass3CAirCountQA";
        private static ShibuyaMatch Match => Object.FindFirstObjectByType<ShibuyaMatch>();
        private static ShibuyaFighter P => Match.Player;
        private static ShibuyaFighter D => Match.Dummy;

        [UnityTest] public IEnumerator JumpWhiffOneClick() { Open(); yield return new EnterPlayMode(); yield return Single(false, false, false, false); yield return new ExitPlayMode(); }
        [UnityTest] public IEnumerator JumpHitOneClick() { Open(); yield return new EnterPlayMode(); yield return Single(false, true, false, false); yield return new ExitPlayMode(); }
        [UnityTest] public IEnumerator PursuitWhiffOneClick() { Open(); yield return new EnterPlayMode(); yield return Single(true, false, false, false); yield return new ExitPlayMode(); }
        [UnityTest] public IEnumerator PursuitHitOneClick() { Open(); yield return new EnterPlayMode(); yield return Single(true, true, false, false); yield return new ExitPlayMode(); }
        [UnityTest] public IEnumerator HitHeldButtonCannotAutoRepeat() { Open(); yield return new EnterPlayMode(); yield return Single(true, true, true, false); yield return new ExitPlayMode(); }
        [UnityTest] public IEnumerator HitWithoutHitStopStillOnlyTwoEvents() { Open(); yield return new EnterPlayMode(); yield return Single(true, true, false, true); yield return new ExitPlayMode(); }
        [UnityTest] public IEnumerator ThreeClicksOnHitRequireThreeAcceptedSteps() { Open(); yield return new EnterPlayMode(); yield return ExtraClicks(true); yield return new ExitPlayMode(); }
        [UnityTest] public IEnumerator ExtraClickOnWhiffCannotCreateFollowup() { Open(); yield return new EnterPlayMode(); yield return ExtraClicks(false); yield return new ExitPlayMode(); }
        [UnityTest] public IEnumerator ExistingAirMovementClipActuallyAdvancesWithoutLosingHover() { Open(); yield return new EnterPlayMode(); yield return ForwardClip(); yield return new ExitPlayMode(); }

        private static IEnumerator Single(bool pursuit, bool contact, bool held, bool noStop)
        {
            yield return Setup();
            if (noStop) foreach (var hit in P.AerialDefinition.aerialAttack.hits) hit.strike.hitStopDuration = 0;
            if (pursuit) { yield return Pursuit(); if (!contact) Place(D, new Vector3(15, D.transform.position.y, 0)); }
            else { if (!contact) Place(D, new Vector3(15, .03f, 0)); yield return Jump(contact); }
            var trace = new Trace((pursuit ? "pursuit" : "jump") + (contact ? "-hit" : "-whiff") + (held ? "-held" : "-one-click") + (noStop ? "-no-hitstop" : ""));
            try {
                yield return Press(trace, held);
                yield return Wait(1.65f);
                trace.frame.attack = false;
                Assert.That(trace.presses, Is.EqualTo(1));
                Assert.That(trace.steps, Is.EqualTo(1));
                CollectionAssert.AreEqual(new[] { 0, 1 }, trace.eventIds);
                Assert.That(trace.applied, Is.EqualTo(contact ? 2 : 0));
                Assert.That(trace.startHp - D.Health.CurrentHealth, Is.EqualTo(contact ? 25 : 0));
                Assert.That(trace.confirmed, Is.EqualTo(trace.applied));
                Assert.That(trace.commonData, Is.True);
                Assert.That(P.IsAerialHovering, Is.False);
                Assert.That(Time.timeScale, Is.EqualTo(1));
                if (contact && !noStop) Assert.That(trace.hitStopFrames, Is.GreaterThan(0));
                if (noStop) Assert.That(trace.hitStopFrames, Is.Zero);
            } finally { trace.Save(); trace.Dispose(); }
        }

        private static IEnumerator ExtraClicks(bool contact)
        {
            yield return Setup(); yield return Pursuit();
            if (!contact) Place(D, new Vector3(15, D.transform.position.y, 0));
            var trace = new Trace(contact ? "pursuit-hit-three-clicks" : "pursuit-whiff-two-clicks");
            try {
                yield return Press(trace);
                yield return Until(() => P.IsAirAttacking && P.AttackElapsed >= P.AttackDuration - P.AerialDefinition.aerialLinkInputWindow + .015f);
                yield return Press(trace);
                Assert.That(P.HasBufferedAirAttack, Is.EqualTo(contact));
                if (contact) {
                    yield return Until(() => trace.steps == 2);
                    yield return Until(() => P.IsAirAttacking && P.AttackElapsed >= P.AttackDuration - P.AerialDefinition.aerialLinkInputWindow + .015f);
                    yield return Press(trace);
                    Assert.That(P.HasBufferedAirAttack, Is.True);
                    yield return Until(() => trace.steps == 3);
                }
                yield return Wait(1.3f);
                Assert.That(trace.presses, Is.EqualTo(contact ? 3 : 2));
                Assert.That(trace.steps, Is.EqualTo(contact ? 3 : 1));
                Assert.That(trace.applied, Is.EqualTo(contact ? trace.eventIds.Count : 0));
                Assert.That(trace.commonData, Is.True);
                Assert.That(trace.eventIds.Count, Is.LessThanOrEqualTo(trace.steps * 2));
                Assert.That(P.IsAerialHovering, Is.False);
            } finally { trace.Save(); trace.Dispose(); }
        }

        private static IEnumerator ForwardClip()
        {
            yield return Setup(); Place(D, new Vector3(15, .03f, 0)); yield return Jump(false);
            P.AerialDefinition.aerialAttack.movements = new[] { new ShibuyaMovementDefinition {
                kind = ShibuyaMovementKind.Forward, startTime = 0, duration = .2f, maxDistance = .4f, speed = 2f } };
            Assert.That(P.AerialDefinition.IsValid);
            Vector3 before = P.transform.position;
            var trace = new Trace("jump-whiff-authored-forward-clip");
            try {
                yield return Press(trace); yield return Until(() => P.AttackElapsed > .25f);
                Vector3 after = P.transform.position;
                Assert.That(after.z - before.z, Is.EqualTo(.4f).Within(.02f));
                Assert.That(after.y, Is.EqualTo(before.y).Within(.001f));
                trace.Add("movement", -1, (after - before).magnitude);
                yield return Wait(1.3f);
                Assert.That(trace.steps, Is.EqualTo(1)); Assert.That(trace.applied, Is.Zero);
            } finally { trace.Save(); trace.Dispose(); }
        }

        [Serializable] private sealed class Sample {
            public string kind; public int frame, acceptedStep, eventId; public double unscaled;
            public float elapsed, value, hp, y; public bool hitStop;
        }
        [Serializable] private sealed class Trace : IDisposable {
            public string scenario; public int presses, steps, applied, confirmed, hitStopFrames;
            public float startHp, endHp; public bool commonData = true;
            public List<int> eventIds = new List<int>(); public List<Sample> samples = new List<Sample>();
            [NonSerialized] public ShibuyaInputFrame frame;
            [NonSerialized] private bool previous;
            [NonSerialized] private int currentEvent = -1;
            public Trace(string name) {
                scenario = name; startHp = D.Health.CurrentHealth;
                P.AttackStarted += OnStep; P.HitEventStarted += OnEvent; P.HitConfirmed += OnConfirmed;
                D.Health.DamageResolved += OnDamage;
                P.GetComponent<ShibuyaPlayerInput>().TestInput = Read;
            }
            private ShibuyaInputFrame Read() {
                if (frame.attack && !previous) { presses++; Add("press", -1, presses); }
                previous = frame.attack;
                if (P.IsInHitStop) hitStopFrames++;
                return frame;
            }
            private void OnStep(int _) {
                steps++; currentEvent = -1;
                commonData &= P.IsAirAttacking && ReferenceEquals(P.CurrentStrike, P.AerialDefinition.aerialAttack.hits[0].strike);
                Add("accepted-air-step", -1, steps);
            }
            private void OnEvent(int id) {
                Assert.That(samples.Exists(s => s.kind == "event" && s.acceptedStep == steps && s.eventId == id), Is.False, "same event redispatched in the same accepted step");
                currentEvent = id; eventIds.Add(id); Add("event", id, 0);
            }
            private void OnDamage(Health target, DamageContext context, DamageResolution result) {
                if (context.Source != P.gameObject) return;
                if (result == DamageResolution.Applied) applied++;
                Add("damage-" + result, currentEvent, context.Amount);
            }
            private void OnConfirmed(Vector3 _, float __) { confirmed++; Add("hit-confirmed", currentEvent, confirmed); }
            public void Add(string kind, int id, float value) { samples.Add(new Sample {
                kind = kind, frame = Time.frameCount, acceptedStep = steps, eventId = id, unscaled = Time.unscaledTimeAsDouble,
                elapsed = P.AttackElapsed, value = value, hp = D.Health.CurrentHealth, y = P.transform.position.y, hitStop = P.IsInHitStop }); }
            public void Save() { endHp = D.Health.CurrentHealth; Directory.CreateDirectory(Root); File.WriteAllText(Root + "/" + scenario + ".json", JsonUtility.ToJson(this, true)); }
            public void Dispose() { P.AttackStarted -= OnStep; P.HitEventStarted -= OnEvent; P.HitConfirmed -= OnConfirmed; D.Health.DamageResolved -= OnDamage; P.GetComponent<ShibuyaPlayerInput>().TestInput = () => default; }
        }
        private static IEnumerator Press(Trace trace, bool hold = false) { trace.frame.attack = true; yield return null; if (!hold) { trace.frame.attack = false; yield return null; } }
        private static IEnumerator Setup() {
            Application.runInBackground = true; yield return Wait(.1f); P.GetComponent<ShibuyaPlayerInput>().TestInput = () => default;
            var profile = Object.Instantiate(P.Profile); profile.name = "PASS3CAirCount QA Profile";
            profile.aerialCombat = Object.Instantiate(profile.aerialCombat); profile.aerialCombat.name = "PASS3CAirCount QA Data";
            P.Configure(profile, AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root + "/Data/Player Stats.asset"));
            Place(P, new Vector3(0, .03f, -1.4f)); Place(D, new Vector3(0, .03f, 0)); P.transform.rotation = Quaternion.identity; yield return null;
        }
        private static IEnumerator Jump(bool target) { P.SetIntent(Vector3.zero, false, true); if (target) D.SetIntent(Vector3.zero, false, true); yield return Until(() => P.transform.position.y > .7f && (!target || D.transform.position.y > .7f)); }
        private static IEnumerator Pursuit() {
            Assert.That(P.TryAttack());
            for (int i = 1; i <= 3; i++) { yield return Until(() => P.ComboStep == i && P.AttackElapsed >= P.AttackDuration - .12f); Assert.That(P.TryAttack(ShibuyaFinisher.Uppercut)); yield return Until(() => P.ComboStep == i + 1); }
            yield return Until(() => P.ConfirmedHitCount == 4); P.SetIntent(Vector3.zero, false, true); yield return Until(() => P.CanAirAttack);
        }
        private static void Open() => EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);
        private static void Place(ShibuyaFighter f, Vector3 at) { var cc = f.GetComponent<CharacterController>(); cc.enabled = false; f.transform.position = at; cc.enabled = true; Physics.SyncTransforms(); }
        private static IEnumerator Wait(float seconds) { float end = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < end) yield return null; }
        private static IEnumerator Until(Func<bool> test) { float end = Time.realtimeSinceStartup + 7; while (!test() && Time.realtimeSinceStartup < end) yield return null; Assert.That(test(), "air count diagnostic timed out"); }
        [UnityTearDown] public IEnumerator Leave() { if (EditorApplication.isPlaying) yield return new ExitPlayMode(); }
        [TearDown] public void Cleanup() { foreach (var obj in Resources.FindObjectsOfTypeAll<ScriptableObject>()) if (obj.name.StartsWith("PASS3CAirCount QA", StringComparison.Ordinal) && !AssetDatabase.Contains(obj)) Object.DestroyImmediate(obj); }
    }
}
