using JJKGame.Dev.PurpleBodyExploration;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.EditorTools
{
    public sealed class PurpleBodyExplorationWindow : EditorWindow
    {
        private PurpleBodyExplorationBench bench;
        private static readonly string[] CandidateLabels={"A · 어두운 질량","B · 불규칙 외곽","C · 깊이와 내향 흐름"};
        private static readonly string[] ViewLabels={"정면","측면","약간 낮게","동일 궤도"};
        [MenuItem("Tools/JJK Game/VFXLab/Purple 본체 A B C 비교")]
        public static void Open() => GetWindow<PurpleBodyExplorationWindow>("Purple 본체 탐색");

        private void OnEnable() { minSize=new Vector2(640,640); EditorApplication.playModeStateChanged+=PlayModeChanged; }
        private void OnDisable() { EditorApplication.playModeStateChanged-=PlayModeChanged; CloseBench(); }
        private void PlayModeChanged(PlayModeStateChange state)
        { if(state==PlayModeStateChange.ExitingPlayMode || state==PlayModeStateChange.EnteredEditMode) CloseBench(); }
        private void Update()
        {
            if(bench!=null && (!EditorApplication.isPlaying || SceneManager.GetActiveScene().name!="VFXLab")) CloseBench();
            if(bench!=null) Repaint();
        }
        private void CloseBench()
        {
            if(bench==null) return;
            var go=bench.gameObject; bench=null; go.SetActive(false);
            if(Application.isPlaying) Destroy(go); else DestroyImmediate(go);
        }
        private void Begin()
        {
            var profile=Resources.Load<PurpleBodyExplorationProfile>("VFX/PurpleBodyExplorationProfile");
            if(profile==null) { Debug.LogError("Purple 본체 탐색 공통 설정을 찾지 못했습니다."); return; }
            var go=new GameObject("CodexPurpleBodyExplorationBench");
            bench=go.AddComponent<PurpleBodyExplorationBench>(); bench.Configure(profile,Camera.main);
        }
        private void OnGUI()
        {
            EditorGUILayout.LabelField("PURPLE · 본체 A / B / C 비교",EditorStyles.boldLabel);
            EditorGUILayout.LabelField("동일 크기 · 위치 · 팔레트 · 코어 · 카메라 · 조명 · 후처리",EditorStyles.miniLabel);
            if(!EditorApplication.isPlaying || SceneManager.GetActiveScene().name!="VFXLab")
            {
                EditorGUILayout.HelpBox("VFXLab을 Play Mode로 실행한 뒤 이 창에서 비교를 시작하세요. Scene은 자동으로 열거나 저장하지 않습니다.",MessageType.Info);
                return;
            }
            if(bench==null)
            {
                if(GUILayout.Button("별도 비교 화면 열기",GUILayout.Height(32))) Begin();
                EditorGUILayout.HelpBox("사용자 prototype과 production 시전은 그대로 유지됩니다. 새 단축키는 없습니다.",MessageType.None);
                return;
            }
            int selected=GUILayout.Toolbar((int)bench.Selected,CandidateLabels,GUILayout.Height(28));
            if(selected!=(int)bench.Selected) { bench.Playing=false; bench.Select((PurpleBodyVariant)selected); }
            int view=GUILayout.Toolbar((int)bench.View,ViewLabels);
            if(view!=(int)bench.View) { bench.Playing=false; bench.Sample(bench.Clock,(PurpleBenchmarkView)view); }
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("0 → 3초 재생",GUILayout.Width(115))) { bench.Sample(0,bench.View); bench.Playing=true; }
            if(GUILayout.Button(bench.Playing?"일시 정지":"이어서 재생",GUILayout.Width(100))) bench.Playing=!bench.Playing;
            EditorGUI.BeginChangeCheck(); float time=EditorGUILayout.Slider("공통 시각",bench.Clock,0,3);
            if(EditorGUI.EndChangeCheck()) { bench.Playing=false; bench.Sample(time,bench.View); }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("후보 전환 시 같은 시각에 정지합니다. 같은 프레임 또는 같은 3초 움직임으로 비교하세요.",EditorStyles.miniLabel);
            Rect rect=GUILayoutUtility.GetAspectRect(16f/9f);
            GUI.DrawTexture(rect,bench.Preview,ScaleMode.ScaleToFit,false);
            string question=selected==0?"A: 어두운 면적과 작은 코어가 질량감을 만드는가?":selected==1?
                "B: 구형 질량을 유지하면서 경계가 살아 움직이는가?":"C: 안쪽 수렴 + 다른 방향의 표면 난류 + 작은 바깥쪽 파열이 깊이로 읽히는가?";
            EditorGUILayout.HelpBox(question,MessageType.None);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("공통 설정 선택")) Selection.activeObject=Resources.Load<PurpleBodyExplorationProfile>("VFX/PurpleBodyExplorationProfile");
            if(GUILayout.Button("설정 다시 읽기")) { var mode=bench.Selected;var v=bench.View;float t=bench.Clock;CloseBench();Begin();bench.Select(mode);bench.Sample(t,v); }
            if(GUILayout.Button("비교 종료")) CloseBench();
            EditorGUILayout.EndHorizontal();
        }
    }

    [CustomEditor(typeof(PurpleBodyExplorationProfile))]
    public sealed class PurpleBodyExplorationProfileEditor : UnityEditor.Editor
    {
        private static readonly System.Collections.Generic.Dictionary<string,string> Labels=new()
        {
            ["bodyDiameter"]="공통 본체 지름",["coreRatio"]="공통 코어 지름 비율",["density"]="공통 질량 밀도",
            ["bodyEmission"]="공통 본체 발광",["coreEmission"]="공통 코어 발광",["nearBlack"]="공통 흑자색",
            ["deepViolet"]="공통 깊은 보라",["magenta"]="공통 마젠타",["violet"]="공통 바이올렛",["hotPink"]="공통 고에너지 핑크",
            ["brokenSilhouette"]="B 외곽 변위",["depthStrength"]="C 내부 깊이",["inwardSpeed"]="C 안쪽 수렴 속도",
            ["surfaceSpeed"]="C 표면 난류 속도",["outwardSpeed"]="C 바깥 파열 속도"
        };
        public static string Label(string property) => Labels.TryGetValue(property,out var label)?label:property;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox("세 후보가 공유하는 실험 조건입니다. 비교 창에서 ‘설정 다시 읽기’를 누르면 반영됩니다. Production 설정과 연결되지 않습니다.",MessageType.Info);
            var p=serializedObject.GetIterator();bool children=true;
            while(p.NextVisible(children))
            {
                children=false;if(p.name=="m_Script") continue;
                EditorGUILayout.PropertyField(p,new GUIContent(Label(p.name)),true);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
