using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    [CustomEditor(typeof(ShibuyaCinematicProfile)), CanEditMultipleObjects]
    public sealed class ShibuyaCinematicInspector : ShibuyaInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.HelpBox("기본값은 예제 OFF입니다. PASS 3D 메뉴의 Play Mode 전용 ON은 복제 데이터만 사용합니다. 연출 중 공격 진행·자세 시계·새 행동은 국소 정지합니다. 이동·공격 입력으로 취소 허용 시 해당 입력을 공격으로 재실행하지 않습니다. 사망·전투 종료·비활성화·씬 종료는 즉시 취소하며 기존 승패를 지연하지 않습니다. 환경 피해/사망은 차단하지 않습니다. 복귀 시 카메라 FOV와 자유 시점 조작을 복원합니다.", MessageType.Info);
            if (!((ShibuyaCinematicProfile)target).IsValid) EditorGUILayout.HelpBox("시네마틱 시간·거리·샷 설정을 확인하세요.", MessageType.Error);
        }
    }
    [InitializeOnLoad] public static class ShibuyaCinematicExample
    {
        public const string Path = "Assets/Shibuya/Data/Shibuya Cinematic Example.asset";
        private static ShibuyaCombatProfile original, clone;
        private static ShibuyaCinematicProfile cinema;
        static ShibuyaCinematicExample() { EditorApplication.playModeStateChanged += state => { if (state == PlayModeStateChange.EnteredEditMode) Cleanup(); }; }
        public static ShibuyaCinematicProfile Create()
        {
            var data = ScriptableObject.CreateInstance<ShibuyaCinematicProfile>();
            data.cinematicEnabled = false;
            data.shots = new[] {
                new ShibuyaCinematicShot { duration=.16f, angle=65, distance=4.1f, height=1.7f, fieldOfView=46 },
                new ShibuyaCinematicShot { duration=.16f, anchor=ShibuyaShotAnchor.Victim, angle=-55, distance=2.9f, height=1.6f, fieldOfView=44 },
                new ShibuyaCinematicShot { duration=.18f, angle=95, distance=4.6f, height=1.8f, fieldOfView=52 }
            };
            return data;
        }
        [MenuItem("JJK Game/시부야 PASS3D/시네마틱 예제 ON (Play Mode 전용)")]
        public static void Enable()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("ShibuyaCombatMVP Play Mode에서만 실행하세요. 씬과 자산은 저장하지 않습니다.");
            var match = Object.FindFirstObjectByType<ShibuyaMatch>();
            if (match == null || match.Finished || match.Player.IsInCinematic) throw new InvalidOperationException("진행 중인 시부야 전투가 필요합니다.");
            if (clone != null) return;
            original = match.Player.Profile; clone = Object.Instantiate(original); clone.name = "PASS3D Example Combat Clone";
            var source = AssetDatabase.LoadAssetAtPath<ShibuyaCinematicProfile>(Path);
            cinema = source == null ? Create() : Object.Instantiate(source); cinema.name = "PASS3D Example Camera Clone"; cinema.cinematicEnabled = true;
            clone.defaultFinisherCinematic = cinema;
            match.Player.Configure(clone, AssetDatabase.LoadAssetAtPath<JJKGame.Core.CharacterStatsProfile>(ShibuyaSceneBuilder.Root + "/Data/Player Stats.asset"));
        }
        [MenuItem("JJK Game/시부야 PASS3D/시네마틱 예제 OFF (Play Mode 전용)")]
        public static void Disable()
        {
            var match = Object.FindFirstObjectByType<ShibuyaMatch>();
            if (EditorApplication.isPlaying && match != null) {
                if (match.Player.IsInCinematic) match.Player.Cinematic.Finish("example-off");
                if (clone != null && match.Player.Profile == clone && original != null)
                    match.Player.Configure(original, AssetDatabase.LoadAssetAtPath<JJKGame.Core.CharacterStatsProfile>(ShibuyaSceneBuilder.Root + "/Data/Player Stats.asset"));
            }
            Cleanup();
        }
        private static void Cleanup() { if (clone != null) Object.DestroyImmediate(clone); if (cinema != null) Object.DestroyImmediate(cinema); original = clone = null; cinema = null; }
    }
}
