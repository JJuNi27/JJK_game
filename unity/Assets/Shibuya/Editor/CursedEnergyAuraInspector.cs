using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    [CustomEditor(typeof(CursedEnergyAuraProfile)),CanEditMultipleObjects]
    public sealed class CursedEnergyAuraProfileInspector : Editor
    {
        public static readonly Dictionary<string,string> Labels=new Dictionary<string,string> {
            {"auraEnabled","주력 아우라 사용"},{"visualPrefab","공통 아우라 프리팹"},
            {"fadeIn","활성화 시간 (초)"},{"fadeOut","해제 감쇠 시간 (초)"},
            {"scale","캐릭터 대비 전체 크기"},{"intensity","전체 발광 강도"},{"offset","표현 기준 위치 보정"},
            {"suppressWhenHit","피격 중 시각적으로 억제"},{"suppressDuringCinematic","시네마틱 중 시각적으로 억제"},
            {"mainColor","메인 주력 색상"},{"overrideSupportingColors","보조색 직접 지정"},
            {"accentColor","밝은 강조색"},{"rimColor","몸 윤곽 발광색"},{"particleColor","미세 입자 발광색"},{"inkColor","검은 먹물 색상"},
            {"mainAura","메인 주력"},{"ink","먹물 경계와 틈"},{"wisps","신체 표면 보조 불꽃 (Surface Details)"},{"bodyRim","몸 윤곽광"},{"motes","미세 입자"},
            {"height","솟아오르는 전체 높이 (m)"},{"width","몸 주변 에너지 폭 (m)"},
            {"motionSpeed","흐름과 형태 교체 속도"},{"irregularity","형태 불규칙성"},{"particleRate","미세 입자 초당 방출량"},
            {"fieldThickness","메인 주력장 두께 (m)"},{"fieldDensity","메인 주력장 밀도"},{"detailScale","보조 불꽃 상대 크기"},
            {"referenceMaxEnergy","기준 최대 주력량"},{"baselineExpansion","기준 외곽·화염 확장 배율"},
            {"maxEnergySizeCurve","최대 주력량별 크기 곡선 (X: 기준 대비 주력량 / Y: 배율)"},
            {"minimumSizeMultiplier","아우라 크기 최소 배율"},{"maximumSizeMultiplier","아우라 크기 최대 배율"},
            {"sizeResponseTime","크기 변화 보간 시간 (초)"},
            {"verticalSizeResponse","주력량 증가 시 상향 성장 비중"},
            {"innerFlowVisibility","내부 상승 에너지 가시성"},{"innerFlowDensity","내부 상승 흐름 밀도"},
            {"preserveLocalFlameShape","고배율 국소 불꽃 형태 보존"},{"extraFieldReach","증가 주력량의 추가 공간 확장"},
            {"inkBoundaryScale","먹물 경계 두께·대비 배율"},{"inkInteriorArea","넓은 내부 먹물 면적 비중"},
            {"detachedTips","화염 끝 분리 조각 사용"},{"tipCycleRate","분리 조각 교체 속도"},
            {"upperFieldConnection","머리 위 주력 흐름 연결"},{"lowerFieldConnection","발밑 주력 흐름 연결"},
            {"detailFlowCount","전신 보조 흐름 수"},
            {"surfaceOffset","신체 표면 방출 간격 (m)"},{"lowerBodyWeight","하체 표면 배분 가중치"},
            {"outwardFlow","보조 불꽃 바깥 방향 흐름"},{"upwardFlow","보조 불꽃 상향 흐름"}
        };
        public override void OnInspectorGUI()
        {
            serializedObject.Update();var it=serializedObject.GetIterator();bool enter=true;
            while(it.NextVisible(enter)) {
                enter=false;
                if(it.name=="m_Script") {using(new EditorGUI.DisabledScope(true))EditorGUILayout.PropertyField(it);continue;}
                bool automatic=!serializedObject.FindProperty("overrideSupportingColors").boolValue
                    && (it.name=="accentColor"||it.name=="rimColor"||it.name=="particleColor");
                using(new EditorGUI.DisabledScope(automatic))
                    EditorGUILayout.PropertyField(it,new GUIContent(Labels.TryGetValue(it.name,out var label)?label:it.displayName,
                        automatic?"보조색 직접 지정을 켜면 개별 색상을 편집합니다. 현재는 메인 색상에서 자동으로 파생됩니다.":"주력 아우라 표현에만 적용됩니다. 전투 수치와 카메라는 변경하지 않습니다."),true);
            }
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("최대 주력량은 전투 설정의 ‘캐릭터 주력 능력치’가 소유합니다. 곡선 X=최대 주력량/기준 최대 주력량, Y=크기 배율입니다. 기준 확장은 몸을 키우지 않고 몸 바깥의 필드·불꽃 성장에만 적용됩니다. 현재 남은 주력량은 읽지 않습니다.",MessageType.Info);
        }
    }
    [CustomPropertyDrawer(typeof(CursedAuraLayer))]
    public sealed class CursedAuraLayerDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty p,GUIContent label)
            => (EditorGUIUtility.singleLineHeight+EditorGUIUtility.standardVerticalSpacing)*(p.isExpanded?4:1);
        public override void OnGUI(Rect r,SerializedProperty p,GUIContent label)
        {
            EditorGUI.BeginProperty(r,label,p);r.height=EditorGUIUtility.singleLineHeight;
            p.isExpanded=EditorGUI.Foldout(r,p.isExpanded,label,true);
            if(p.isExpanded) {
                EditorGUI.indentLevel++;
                foreach(var pair in new[]{new[]{"enabled","레이어 사용"},new[]{"intensity","레이어 강도"},new[]{"opacity","레이어 불투명도"}}) {
                    r.y+=EditorGUIUtility.singleLineHeight+EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.PropertyField(r,p.FindPropertyRelative(pair[0]),new GUIContent(pair[1]),true);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.EndProperty();
        }
    }
    [CustomEditor(typeof(CursedEnergyAuraController)),CanEditMultipleObjects]
    public sealed class CursedEnergyAuraControllerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("profile"),new GUIContent("캐릭터 주력 아우라 설정"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("presentationAnchor"),new GUIContent("표현 기준 Transform (비우면 캐릭터 루트)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bodyRenderers"),new GUIContent("방출할 신체 Renderer (비우면 자동 선별)"),true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showEmissionPoints"),new GUIContent("선택 시 Scene에 방출점 표시"));
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("F 홀드는 시각 표현만 요청합니다. 이동·공격·피해·자원에는 영향을 주지 않습니다.",MessageType.Info);
        }
    }
    [CustomEditor(typeof(CursedEnergyAuraVisual)),CanEditMultipleObjects]
    public sealed class CursedEnergyAuraVisualInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            foreach(var pair in new[]{new[]{"energyTemplate","청록 주력 머티리얼"},new[]{"inkTemplate","먹물 경계 머티리얼"},new[]{"rimTemplate","몸 윤곽광 머티리얼"},new[]{"moteTemplate","미세 입자 머티리얼"}})
                EditorGUILayout.PropertyField(serializedObject.FindProperty(pair[0]),new GUIContent(pair[1]));
            serializedObject.ApplyModifiedProperties();
        }
    }
}
