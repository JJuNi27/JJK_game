using JJKGame.Dev.PurpleBodyExploration;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace JJKGame.EditorTools
{
    public sealed class PurpleBodyHybridD2R2Window : EditorWindow
    {
        private PurpleBodyHybridD2R2Bench bench;
        [MenuItem("Tools/JJK Game/VFXLab/Purple Hybrid D2-R2 비교")]
        public static void Open()=>GetWindow<PurpleBodyHybridD2R2Window>("Purple R1 ↔ R2");
        private void OnEnable(){minSize=new Vector2(720,660);EditorApplication.playModeStateChanged+=OnPlayMode;}
        private void OnDisable(){EditorApplication.playModeStateChanged-=OnPlayMode;CloseBench();}
        private void OnPlayMode(PlayModeStateChange state){if(state==PlayModeStateChange.ExitingPlayMode || state==PlayModeStateChange.EnteredEditMode)CloseBench();}
        private void CloseBench(){if(bench==null)return;var go=bench.gameObject;bench=null;go.SetActive(false);if(Application.isPlaying)Destroy(go);else DestroyImmediate(go);}
        private void Begin()
        {
            var profile=Resources.Load<PurpleBodyHybridD2R2Profile>("VFX/PurpleBodyHybridD2R2Profile");
            if(profile==null || profile.frozenR1==null){Debug.LogError("D2-R2 비교 설정이 없습니다.");return;}
            bench=new GameObject("CodexPurpleD2R2Comparison").AddComponent<PurpleBodyHybridD2R2Bench>();bench.Configure(profile,Camera.main);
        }
        private void Update(){if(bench!=null && (!EditorApplication.isPlaying || SceneManager.GetActiveScene().name!="VFXLab"))CloseBench();if(bench!=null)Repaint();}
        private void OnGUI()
        {
            EditorGUILayout.LabelField("R1 ↔ R2 · INTERNAL SUPERNOVA / OUTER RUPTURE",EditorStyles.boldLabel);
            EditorGUILayout.LabelField("동일 지름 · 카메라 · 조명 · 배경 · 후처리 / production 미적용",EditorStyles.miniLabel);
            if(!EditorApplication.isPlaying || SceneManager.GetActiveScene().name!="VFXLab")
            {EditorGUILayout.HelpBox("VFXLab Play Mode에서 시작하세요. 기존 Scene과 D는 보존됩니다.",MessageType.Info);return;}
            if(bench==null){if(GUILayout.Button("별도 D2-R2 비교 화면 열기",GUILayout.Height(32)))Begin();return;}
            int selected=GUILayout.Toolbar(bench.ShowR2?1:0,new[]{"R1 · 동결된 비교 기준","R2 · 입체 플라즈마"},GUILayout.Height(28));
            if((selected==1)!=bench.ShowR2){bench.Playing=false;bench.Select(selected==1);}
            var common=bench.Common;int view=GUILayout.Toolbar((int)common.View,new[]{"정면","측면","약간 낮게","동일 궤도"});
            if(view!=(int)common.View){bench.Playing=false;bench.Sample(common.Clock,(PurpleBenchmarkView)view);}
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("0 → 3초 재생",GUILayout.Width(115))){bench.Sample(0,common.View);bench.Playing=true;}
            if(GUILayout.Button(bench.Playing?"일시 정지":"이어서 재생",GUILayout.Width(100)))bench.Playing=!bench.Playing;
            EditorGUI.BeginChangeCheck();float time=EditorGUILayout.Slider("공통 시각",common.Clock,0,3);
            if(EditorGUI.EndChangeCheck()){bench.Playing=false;bench.Sample(time,common.View);}
            EditorGUILayout.EndHorizontal();GUI.DrawTexture(GUILayoutUtility.GetAspectRect(16f/9f),common.Preview,ScaleMode.ScaleToFit,false);
            EditorGUILayout.HelpBox("구체 안의 빛과 난류가 폭주하는가? 어두운 틈과 내향 흐름이 남아 있는가?",MessageType.None);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("D2R2 설정 선택"))Selection.activeObject=Resources.Load<PurpleBodyHybridD2R2Profile>("VFX/PurpleBodyHybridD2R2Profile");
            if(GUILayout.Button("D2R2 설정 다시 읽기")){bool selectedD2R2=bench.ShowR2;var v=common.View;var t=common.Clock;CloseBench();Begin();if(bench!=null){bench.Select(selectedD2R2);bench.Sample(t,v);}}
            if(GUILayout.Button("비교 종료"))CloseBench();EditorGUILayout.EndHorizontal();
        }
    }
    [CustomEditor(typeof(PurpleBodyHybridD2R2Profile))]
    public sealed class PurpleBodyHybridD2R2ProfileEditor : UnityEditor.Editor
    {
        private static readonly System.Collections.Generic.Dictionary<string,string> Labels=new()
        {
            ["frozenR1"]="동결된 R1 비교 기준",["shellEmission"]="표면 보라 발광",["shellDensity"]="표면 가림 밀도",["internalEnergy"]="내부 광원 에너지",
            ["inwardSpeed"]="내향 흐름 속도",["turbulenceSpeed"]="표면 난류 속도",["arcRadius"]="입체 아크 반경",["outerReach"]="외곽 파열 거리",["instability"]="불규칙 에너지 요동",["voidContrast"]="날카로운 암부 대비"
        };
        public static string Label(string field)=>Labels.TryGetValue(field,out var value)?value:field;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();EditorGUILayout.HelpBox("D2R2 전용 설정입니다. 참조하는 D와 A/B/C의 원본 asset은 수정하지 마세요.",MessageType.Info);
            var p=serializedObject.GetIterator();bool children=true;
            while(p.NextVisible(children)){children=false;if(p.name=="m_Script")continue;EditorGUILayout.PropertyField(p,new GUIContent(Label(p.name)),true);}
            serializedObject.ApplyModifiedProperties();
        }
    }
}
