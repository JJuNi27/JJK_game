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
    public static class ShibuyaPass3BTestRunner
    {
        private const string Key = "ShibuyaPass3BQA.Active";
        private const string Root = "Logs/ShibuyaPass3BQA";
        private static TestRunnerApi api;
        static ShibuyaPass3BTestRunner()
        {
            TestRunnerApi.RegisterTestCallback(new Recorder());
            EditorApplication.playModeStateChanged += state => {
                if (SessionState.GetBool(Key, false) && state == PlayModeStateChange.EnteredPlayMode)
                    Application.runInBackground = true; // Runtime only; no PlayerSettings writes.
            };
        }

        [MenuItem("JJK Game/시부야 PASS3B/PASS3B 신규 자동 검증")]
        public static void Run() => RunFiltered("ShibuyaPass3B");
        public static void RunFiltered(string filter)
        {
            if (EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Play Mode를 종료하고 씬 저장 상태를 먼저 확인하세요.");
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
        private sealed class Recorder : ICallbacks
        {
            public void RunStarted(ITestAdaptor tree) { }
            public void TestStarted(ITestAdaptor test) { }
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
                if (api != null) UnityEngine.Object.DestroyImmediate(api);
            }
        }
    }
}
