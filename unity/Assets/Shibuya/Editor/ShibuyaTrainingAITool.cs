using UnityEditor;
using UnityEngine;
namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaTrainingAITool : EditorWindow
    {
        public const string ProfilePath="Assets/Shibuya/Data/Shibuya Training AI.asset";
        [SerializeField] private ShibuyaTrainingAIProfile settings;
        private UnityEditor.Editor profileEditor;
        private ShibuyaTrainingAI bound;
        private void OnInspectorUpdate(){if(EditorApplication.isPlaying)Repaint();}
        [MenuItem("JJK Game/시부야 PASS4F/훈련 AI 현재 상태·설정")]
        public static void Open()=>GetWindow<ShibuyaTrainingAITool>("공격형 훈련 AI");
        [MenuItem("JJK Game/시부야 PASS4F/공격형 훈련 AI ON·OFF")]
        public static void Toggle()
        {
            if(!EditorApplication.isPlaying)return;
            var match=Object.FindFirstObjectByType<ShibuyaMatch>();if(match==null||match.Finished)return;
            var ai=match.Dummy.GetComponent<ShibuyaTrainingAI>();
            if(ai==null){ai=match.Dummy.gameObject.AddComponent<ShibuyaTrainingAI>();ai.Configure(AssetDatabase.LoadAssetAtPath<ShibuyaTrainingAIProfile>(ProfilePath),match.Player);}
            ai.SetTrainingEnabled(!ai.Running);
        }
        public static void SelectMode(ShibuyaTrainingMode mode)
        {
            if(!EditorApplication.isPlaying)return;var match=Object.FindFirstObjectByType<ShibuyaMatch>();if(match==null||match.Finished)return;
            var ai=match.Dummy.GetComponent<ShibuyaTrainingAI>();
            if(ai==null){ai=match.Dummy.gameObject.AddComponent<ShibuyaTrainingAI>();ai.Configure(AssetDatabase.LoadAssetAtPath<ShibuyaTrainingAIProfile>(ProfilePath),match.Player);}
            ai.SetMode(mode);
        }
        [MenuItem("JJK Game/시부야 PASS4F/모드/공격형")] public static void Offensive()=>SelectMode(ShibuyaTrainingMode.Offensive);
        [MenuItem("JJK Game/시부야 PASS4F/모드/가드 연습")] public static void Guard()=>SelectMode(ShibuyaTrainingMode.GuardPractice);
        [MenuItem("JJK Game/시부야 PASS4F/모드/회피 연습")] public static void Dodge()=>SelectMode(ShibuyaTrainingMode.DodgePractice);
        [MenuItem("JJK Game/시부야 PASS4F/모드/패링 연습")] public static void Parry()=>SelectMode(ShibuyaTrainingMode.ParryPractice);
        [MenuItem("JJK Game/시부야 PASS4F/모드/종합")] public static void Mixed()=>SelectMode(ShibuyaTrainingMode.Mixed);
        [MenuItem("JJK Game/시부야 PASS4F/공격형 훈련 AI ON·OFF",true)]
        private static bool CanToggle()=>EditorApplication.isPlaying&&!EditorApplication.isCompiling;
        private void OnDisable(){if(bound!=null)bound.SetTrainingEnabled(false);if(profileEditor!=null)DestroyImmediate(profileEditor);profileEditor=null;}
        private void OnGUI()
        {
            if(settings==null)settings=AssetDatabase.LoadAssetAtPath<ShibuyaTrainingAIProfile>(ProfilePath);
            EditorGUILayout.HelpBox("새 Play Mode는 AI OFF입니다. ON 후 Game View로 돌아가 직접 가드·패링·이동을 연습하세요. 기존 가드/잡기/다운 검수 도구는 AI OFF에서 사용하세요. 이 창을 닫으면 연결된 AI를 OFF합니다. OFF는 실행 중 타격/피격을 강제 취소하지 않습니다.",MessageType.Info);
            var selected=(ShibuyaTrainingAIProfile)EditorGUILayout.ObjectField("훈련 AI 판단 설정",settings,typeof(ShibuyaTrainingAIProfile),false);
            if(selected!=settings){if(bound!=null)bound.SetTrainingEnabled(false);settings=selected;if(profileEditor!=null)DestroyImmediate(profileEditor);profileEditor=null;}
            var match=EditorApplication.isPlaying?Object.FindFirstObjectByType<ShibuyaMatch>():null;
            bound=match!=null?match.Dummy.GetComponent<ShibuyaTrainingAI>():null;
            using(new EditorGUI.DisabledScope(match==null||match.Finished||settings==null)){
                if(GUILayout.Button(bound!=null&&bound.Running?"훈련 AI OFF":"훈련 AI ON")){
                    if(bound==null)bound=match.Dummy.gameObject.AddComponent<ShibuyaTrainingAI>();
                    if(bound.Profile!=settings||bound.Target!=match.Player)bound.Configure(settings,match.Player);
                    bound.SetTrainingEnabled(!bound.Running);
                }
            }
            if(bound!=null){ShibuyaTrainingAIInspector.DrawMode(bound);ShibuyaTrainingAIInspector.DrawRuntime(bound);}
            if(settings!=null){EditorGUILayout.Space();EditorGUILayout.LabelField("판단 설정 (기존 전투 수치는 변경하지 않음)",EditorStyles.boldLabel);UnityEditor.Editor.CreateCachedEditor(settings,null,ref profileEditor);profileEditor.OnInspectorGUI();}
        }
    }
}
