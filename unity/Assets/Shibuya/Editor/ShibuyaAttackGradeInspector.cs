using UnityEditor;
using UnityEngine;
namespace JJKGame.Shibuya.EditorTools
{
    [CustomEditor(typeof(ShibuyaAttackGradeProfile)),CanEditMultipleObjects]
    public sealed class ShibuyaAttackGradeInspector : UnityEditor.Editor
    {
        public static string Label(string name) { switch(name){
            case "criticalChance":return "기본 치명타 확률 (%)";
            case "criticalMultiplier":return "치명타 피해 배율";
            case "allowCriticalModifiers":return "치명타 버프·디버프 허용";
            case "blackFlashCapable":return "캐릭터 흑섬 사용 가능 (기본 꺼짐)";
            case "normalConditional":return "일반 조건부 흑섬 확률 (%)";
            case "zoneConditional":return "Zone 조건부 흑섬 확률 (%)";
            case "stackConditional":return "스택당 조건부 흑섬 증가 (%p)";
            case "normalFinalCap":return "일반 최종 흑섬 상한 (%)";
            case "zoneFinalCap":return "Zone 최종 흑섬 상한 (%)";
            case "blackFlashMultiplier":return "흑섬 피해 배율";
            case "blackFlashEnergy":return "흑섬 성공 주력 회복량";
            case "zoneDuration":return "Zone 지속시간 (초)";
            case "zoneDamageBonus":return "Zone 공격력 증가 (%)";
            case "zoneCostReduction":return "Zone 주력 소비 감소 (%)";
            case "zoneChargeBonus":return "Zone F 충전 증가 (%)";
            default:return name;}}
        public override void OnInspectorGUI(){serializedObject.Update();var p=serializedObject.GetIterator();
            if(p.NextVisible(true))do{using(new EditorGUI.DisabledScope(p.name=="m_Script"))
                EditorGUILayout.PropertyField(p,new GUIContent(Label(p.name)),true);}while(p.NextVisible(false));
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("흑섬은 치명타 이후 추첨합니다. 최종 상한을 조건부 확률에 환산하며 Zone 스택은 현재 대전에서만 유지합니다. 캐릭터 흑섬 능력은 명시적으로 허용하세요.",MessageType.Info);}
    }
}
