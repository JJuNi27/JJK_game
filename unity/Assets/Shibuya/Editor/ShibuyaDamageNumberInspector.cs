using UnityEditor;
using UnityEngine;
namespace JJKGame.Shibuya.EditorTools
{
    [CustomEditor(typeof(ShibuyaDamageNumberProfile)),CanEditMultipleObjects]
    public sealed class ShibuyaDamageNumberInspector : UnityEditor.Editor
    {
        public static string Label(string name){switch(name){
            case "m_Script":return "스크립트";case "displayEnabled":return "데미지 표시 활성화";
            case "normalColor":return "일반 숫자 색상";case "criticalColor":return "치명타 숫자 색상";case "blackFlashColor":return "흑섬 숫자 색상";
            case "normalSize":return "일반 숫자 크기";case "criticalSize":return "치명타 숫자 크기";case "blackFlashSize":return "흑섬 숫자 크기";
            case "displayDuration":return "표시 시간 (실시간 초)";case "risePixels":return "위로 떠오르는 거리 (픽셀)";
            case "normalPopScale":return "일반 등장 확대 배율";case "criticalPopScale":return "치명타 등장 확대 배율";case "blackFlashPopScale":return "흑섬 등장 확대 배율";
            case "popCurve":return "등장 확대 곡선 (수명 비율)";case "opacityCurve":return "투명도 곡선 (수명 비율)";
            case "worldOffset":return "대상 근처 월드 위치 보정";case "screenOffset":return "화면 위치 보정 (픽셀)";
            case "overlapSpacing":return "연속 타격 숫자 간격 (픽셀)";case "maximumVisible":return "동시 표시 최대 개수";
            case "showCriticalText":return "치명타 문구 표시";case "showBlackFlashText":return "흑섬 문구 표시";case "showZoneText":return "Zone 보조 문구 표시";
            case "zoneTextScale":return "Zone 보조 문구 크기 비율";case "zoneTextColor":return "Zone 보조 문구 색상";
            case "criticalText":return "치명타 표시 문구";case "blackFlashText":return "흑섬 표시 문구";
            case "outlineColor":return "숫자 외곽 색상";case "outlineWidth":return "숫자 외곽 두께 (픽셀)";
            case "font":return "표시 폰트 (비우면 운영체제 폰트)";case "fallbackFontName":return "대체 운영체제 폰트 이름";
            default:return name;}}
        public override void OnInspectorGUI(){serializedObject.Update();var p=serializedObject.GetIterator();
            if(p.NextVisible(true))do{using(new EditorGUI.DisabledScope(p.name=="m_Script"))EditorGUILayout.PropertyField(p,new GUIContent(Label(p.name)),true);}while(p.NextVisible(false));
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("숫자는 이미 확정된 실제 HP 감소와 공격 등급만 읽습니다. 표시 OFF는 전투·흑섬·Zone·타격음/VFX에 영향을 주지 않습니다. 곡선 가로축은 표시 수명의0~1이며, 숫자는 최대 개수 안에서 재사용합니다.",MessageType.Info);}
    }
}
