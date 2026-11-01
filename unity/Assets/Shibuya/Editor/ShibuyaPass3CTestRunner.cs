using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    // Records the existing Test Framework tests across Enter/ExitPlayMode domain reloads.
    [InitializeOnLoad]
    public static class ShibuyaPass3CTestRunner
    {
        private const string Key = "ShibuyaPass3CQA.Active";
        private const string DefaultRoot = "Logs/ShibuyaPass3CQA";
        private static string Root => SessionState.GetString(Key+".Root",DefaultRoot);
        private const string DiagnosticKey = "ShibuyaPass3CQA.Diagnostic";
        private static readonly List<FrameSample> samples = new List<FrameSample>();
        private static int sampledFrame = -1;
        private static TestRunnerApi api;
        static ShibuyaPass3CTestRunner()
        {
            TestRunnerApi.RegisterTestCallback(new Recorder());
            EditorApplication.playModeStateChanged += state => {
                if (SessionState.GetBool(Key, false) && state == PlayModeStateChange.EnteredPlayMode)
                    Application.runInBackground = true; // Runtime only; no PlayerSettings writes.
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(DiagnosticKey, false)
                    && !Root.StartsWith("Logs/ShibuyaPass4AQA/", StringComparison.Ordinal)) {
                    var m = UnityEngine.Object.FindFirstObjectByType<ShibuyaMatch>();
                    if (m != null && m.Player != null) m.Player.HitEventStarted += ProbeAirHit;
                }
                if (state == PlayModeStateChange.ExitingPlayMode && samples.Count > 0) {
                    File.WriteAllText(Root + "/timing-" + SessionState.GetString(Key + ".Test", "unknown").Replace('.', '_') + ".json",
                        JsonUtility.ToJson(new FrameSamples { frames = samples.ToArray() }, true));
                    samples.Clear();
                }
            };
            EditorApplication.update += SampleTiming;
        }

        private static void ProbeAirHit(int eventId)
        {
            var m = UnityEngine.Object.FindFirstObjectByType<ShibuyaMatch>();
            if (m == null || m.Player == null || m.Dummy == null || !m.Player.IsAirAttacking) return;
            var p = m.Player; var d = m.Dummy;
            // Test teardown may destroy the profile before the active event completes.
            // Read the executing event, rather than a possibly missing profile definition.
            var step = (ShibuyaMovesetStep)typeof(ShibuyaFighter).GetField("activeStep",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(p);
            if (step == null || step.hits == null || eventId < 0 || eventId >= step.hits.Length
                || step.hits[eventId] == null || step.hits[eventId].strike == null) return;
            var strike = step.hits[eventId].strike;
            Vector3 origin = p.transform.position + Vector3.up * strike.attackHeight;
            Vector3 center = origin + p.transform.forward * strike.attackForwardOffset;
            var hits = Physics.OverlapSphere(center, Mathf.Max(.1f, strike.attackRadius), ~0, QueryTriggerInteraction.Ignore);
            bool included = Array.Exists(hits, c => c.GetComponentInParent<ShibuyaFighter>() == d);
            bool blocked = (bool)typeof(ShibuyaFighter).GetMethod("WorldBlocks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance).Invoke(p, new object[] { origin, d.transform.position + Vector3.up * strike.attackHeight });
            File.AppendAllText(Root + "/air-hit-query.jsonl", Newtonsoft.Json.JsonConvert.SerializeObject(new {
                accepted = p.AcceptedAttackCount, eventId, frame = Time.frameCount, elapsed = p.AttackElapsed,
                player = new[] { p.transform.position.x, p.transform.position.y, p.transform.position.z },
                victim = new[] { d.transform.position.x, d.transform.position.y, d.transform.position.z },
                forward = new[] { p.transform.forward.x, p.transform.forward.y, p.transform.forward.z },
                radius = strike.attackRadius, offset = strike.attackForwardOffset, height = strike.attackHeight,
                included, blocked, immune = d.Health.IsInvulnerable, cinematic = p.IsInCinematic,
                colliders = Array.ConvertAll(hits, c => c.name)
            }) + Environment.NewLine);
        }

        // Read-only QA telemetry: does not drive input, clocks, gameplay, or assertions.
        private static void SampleTiming()
        {
            if (!SessionState.GetBool(Key, false) || !SessionState.GetBool(DiagnosticKey, false)
                || !EditorApplication.isPlaying || sampledFrame == Time.frameCount) return;
            sampledFrame = Time.frameCount;
            var match = UnityEngine.Object.FindFirstObjectByType<ShibuyaMatch>();
            if (match == null || match.Player == null || match.Dummy == null) return;
            var p = match.Player; var d = match.Dummy;
            samples.Add(new FrameSample { frame = Time.frameCount, delta = Time.deltaTime, clock = p.CombatTime,
                elapsed = p.AttackElapsed, unscaledDelta = Time.unscaledDeltaTime, realTime = Time.realtimeSinceStartupAsDouble, step = p.ComboStep, buffered = p.HasBufferedAttack, hits = p.ConfirmedHitCount,
                hitStop = p.IsInHitStop, airSession = p.IsInAirSession, victimX = d.transform.position.x, playerY = p.transform.position.y, victimY = d.transform.position.y, playerZ = p.transform.position.z, victimZ = d.transform.position.z,
                victimImmune = d.Health.IsInvulnerable, cinematic = p.IsInCinematic,
                editorFocused = EditorApplication.isFocused, victimHeld = d.IsHeldByAerialHit, victimStunned = d.State == ShibuyaFighterState.Hitstunned,
                accepted = p.AcceptedAttackCount,
                alignment = d.HasVictimAlignment, state = p.State.ToString() });
        }

        // Isolate a new run's legacy screenshots/metrics without touching previous QA evidence.
        public static string OutputPath(string legacyPath) => SessionState.GetBool(Key, false) && Root != DefaultRoot
            ? Root + "/legacy/" + legacyPath.Substring("Logs/".Length) : legacyPath;

        public static void RunDiagnostic(string filter) { SessionState.SetBool(DiagnosticKey, true); RunFiltered(filter); }

        [MenuItem("JJK Game/시부야 PASS3C/PASS3C 신규 자동 검증")]
        public static void Run() => RunFiltered("ShibuyaPass3C");
        public static void RunFiltered(string filter) => RunFiltered(filter,DefaultRoot);
        public static void RunFiltered(string filter,string outputDirectory)
        {
            if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Play Mode를 종료하고 씬 저장 상태를 먼저 확인하세요.");
            if((!outputDirectory.StartsWith("Logs/ShibuyaPass3C",StringComparison.Ordinal) && !outputDirectory.StartsWith("Logs/ShibuyaPass3D",StringComparison.Ordinal) && !outputDirectory.StartsWith("Logs/ShibuyaPass4A",StringComparison.Ordinal) && !outputDirectory.StartsWith("Logs/ShibuyaPass4B",StringComparison.Ordinal) && !outputDirectory.StartsWith("Logs/ShibuyaPass4C",StringComparison.Ordinal) && !outputDirectory.StartsWith("Logs/ShibuyaPass4D",StringComparison.Ordinal) && !outputDirectory.StartsWith("Logs/ShibuyaPass4E",StringComparison.Ordinal) && !outputDirectory.StartsWith("Logs/ShibuyaPass4F",StringComparison.Ordinal) && !outputDirectory.StartsWith("Logs/ShibuyaPass4G",StringComparison.Ordinal) && !outputDirectory.StartsWith("Logs/ShibuyaPass4H",StringComparison.Ordinal)) || outputDirectory.Contains(".."))
                throw new ArgumentException("QA 출력 경로가 유효하지 않습니다.");
            SessionState.SetString(Key+".Root",outputDirectory);
            Directory.CreateDirectory(Root);
            api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RetrieveTestList(TestMode.EditMode, tree => {
                var names = new List<string>(); Collect(tree, names);
                string[] filters = filter.Split('|');
                names.RemoveAll(name => !Array.Exists(filters, part => name.Contains(part)));
                File.WriteAllText(Root + "/discovery.json", JsonUtility.ToJson(new Discovery { tests = names.ToArray() }, true));
                if (names.Count == 0) throw new InvalidOperationException("시부야 테스트 discovery 0개");
                File.WriteAllText(Root + "/execution.jsonl", "");
                SessionState.SetBool(Key, true);
                api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode, testNames = names.ToArray() }));
            });
        }
        private static void Collect(ITestAdaptor test, List<string> names)
        {
            if (!test.IsSuite && !test.HasChildren && test.FullName.StartsWith("JJKGame.Shibuya.EditorTools.ShibuyaPass", StringComparison.Ordinal)) names.Add(test.FullName);
            if (test.Children != null) foreach (var child in test.Children) Collect(child, names);
        }
        [Serializable] private sealed class Discovery { public string[] tests; }
        [Serializable] private sealed class Entry { public string name, status, message; public double duration; }
        [Serializable] private sealed class FrameSamples { public FrameSample[] frames; }
        [Serializable] private sealed class FrameSample {
            public double realTime; public float unscaledDelta;
            public int frame, step, hits, accepted; public float delta, clock, elapsed, victimX, playerY, victimY, playerZ, victimZ;
            public bool buffered, hitStop, airSession, alignment, editorFocused, victimHeld, victimStunned, victimImmune, cinematic; public string state;
        }
        private sealed class Recorder : ICallbacks
        {
            public void RunStarted(ITestAdaptor tree) { }
            public void TestStarted(ITestAdaptor test) { if (!test.IsSuite) SessionState.SetString(Key + ".Test", test.FullName); }
            public void TestFinished(ITestResultAdaptor result)
            {
                if (!SessionState.GetBool(Key, false) || result.Test.IsSuite) return;
                File.AppendAllText(Root + "/execution.jsonl", JsonUtility.ToJson(new Entry {
                    name = result.Test.FullName, status = result.TestStatus.ToString(), message = result.Message, duration = result.Duration
                }) + Environment.NewLine);
            }
            public void RunFinished(ITestResultAdaptor result)
            {
                if (!SessionState.GetBool(Key, false)) return;
                TestRunnerApi.SaveResultToFile(result, Root + "/results.xml");
                SessionState.SetBool(Key, false);
                SessionState.SetBool(DiagnosticKey, false);
                if (api != null) UnityEngine.Object.DestroyImmediate(api);
            }
        }
    }
}
