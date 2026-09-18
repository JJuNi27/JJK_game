using JJKGame.Dev.PurpleBodyExploration;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.EditorTools
{
    public sealed class PurpleBodyHybridDWindow : EditorWindow
    {
        private PurpleBodyHybridDBench bench;
        private static readonly string[] Candidates={"A · 암부","B · 외곽","C · 깊이","D · 하이브리드"};
        private static readonly string[] Views={"정면","측면","약간 낮게","동일 궤도"};
        [MenuItem("Tools/JJK Game/VFXLab/Purple Hybrid D 비교")]
        public static void Open()=>GetWindow<PurpleBodyHybridDWindow>("Purple Hybrid D");
        private void OnEnable(){minSize=new Vector2(720,660);EditorApplication.playModeStateChanged+=PlayModeChanged;}
        private void OnDisable(){EditorApplication.playModeStateChanged-=PlayModeChanged;CloseBench();}
        private void PlayModeChanged(PlayModeStateChange state)
        {if(state==PlayModeStateChange.ExitingPlayMode || state==PlayModeStateChange.EnteredEditMode)CloseBench();}
        private void Update()
        {
            if(bench!=null && (!EditorApplication.isPlaying || SceneManager.GetActiveScene().name!="VFXLab"))CloseBench();
            if(bench!=null)Repaint();
        }
        private void CloseBench()
        {
            if(bench==null)return;var go=bench.gameObject;bench=null;go.SetActive(false);
            if(Application.isPlaying)Destroy(go);else DestroyImmediate(go);
        }
        private void Begin()
        {
            var profile=Resources.Load<PurpleBodyHybridDProfile>("VFX/PurpleBodyHybridDProfile");
            if(profile==null || profile.commonConditions==null){Debug.LogError("Hybrid D 비교 설정이 없습니다.");return;}
            bench=new GameObject("CodexPurpleHybridDComparison").AddComponent<PurpleBodyHybridDBench>();bench.Configure(profile,Camera.main);
        }
        private void OnGUI()
        {
            EditorGUILayout.LabelField("HYBRID D · A의 암부 + 약한 외곽 찢김 + 내부 수렴",EditorStyles.boldLabel);
            EditorGUILayout.LabelField("동일 크기 · 작은 코어 · 팔레트 · 카메라 · 조명 · 후처리",EditorStyles.miniLabel);
            if(!EditorApplication.isPlaying || SceneManager.GetActiveScene().name!="VFXLab")
            {EditorGUILayout.HelpBox("VFXLab Play Mode에서 시작하세요. 기존 A/B/C 비교 창과 별개의 실험이며 Scene을 저장하지 않습니다.",MessageType.Info);return;}
            if(bench==null){if(GUILayout.Button("별도 D 비교 화면 열기",GUILayout.Height(32)))Begin();return;}
            var common=bench.CommonBench;
            int mode=GUILayout.Toolbar(bench.Selected,Candidates,GUILayout.Height(28));
            if(mode!=bench.Selected){bench.Playing=false;bench.Select(mode);}
            int view=GUILayout.Toolbar((int)common.View,Views);
            if(view!=(int)common.View){bench.Playing=false;bench.Sample(common.Clock,(PurpleBenchmarkView)view);}
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("0 → 3초 재생",GUILayout.Width(115))){bench.Sample(0,common.View);bench.Playing=true;}
            if(GUILayout.Button(bench.Playing?"일시 정지":"이어서 재생",GUILayout.Width(100)))bench.Playing=!bench.Playing;
            EditorGUI.BeginChangeCheck();float time=EditorGUILayout.Slider("공통 시각",common.Clock,0,3);
            if(EditorGUI.EndChangeCheck()){bench.Playing=false;bench.Sample(time,common.View);}
            EditorGUILayout.EndHorizontal();
            GUI.DrawTexture(GUILayoutUtility.GetAspectRect(16f/9f),common.Preview,ScaleMode.ScaleToFit,false);
            EditorGUILayout.HelpBox("D: 어두운 질량을 유지하며 짧은 에너지 줄기가 코어로 들어가는가? 외곽이 암석·고리·눈으로 읽히지 않는가?",MessageType.None);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("D 설정 선택"))Selection.activeObject=Resources.Load<PurpleBodyHybridDProfile>("VFX/PurpleBodyHybridDProfile");
            if(GUILayout.Button("D 설정 다시 읽기"))
            {var m=bench.Selected;var v=common.View;var t=common.Clock;CloseBench();Begin();if(bench!=null){bench.Select(m);bench.Sample(t,v);}}
            if(GUILayout.Button("비교 종료"))CloseBench();
            EditorGUILayout.EndHorizontal();
        }
    }
    [CustomEditor(typeof(PurpleBodyHybridDProfile))]
    public sealed class PurpleBodyHybridDProfileEditor : UnityEditor.Editor
    {
        private static readonly System.Collections.Generic.Dictionary<string,string> Labels=new()
        {
            ["commonConditions"]="보존된 A/B/C 공통 조건",["tearingAmplitude"]="D 외곽 찢김 크기",["implosionSpeed"]="D 내부 수렴 속도",
            ["surfaceSpeed"]="D 표면 흐름 속도",["internalDepth"]="D 내부 깊이",["streamEnergy"]="D 수렴 에너지 강도",["edgeEnergy"]="D 외곽 보조 에너지"
        };
        public static string Label(string field)=>Labels.TryGetValue(field,out var value)?value:field;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();EditorGUILayout.HelpBox("공통 조건은 기존 A/B/C asset에서 읽습니다. 해당 asset은 보존하고, D 전용 값만 조절하세요.",MessageType.Info);
            var p=serializedObject.GetIterator();bool children=true;
            while(p.NextVisible(children)){children=false;if(p.name=="m_Script")continue;EditorGUILayout.PropertyField(p,new GUIContent(Label(p.name)),true);}
            serializedObject.ApplyModifiedProperties();
        }
    }
}
