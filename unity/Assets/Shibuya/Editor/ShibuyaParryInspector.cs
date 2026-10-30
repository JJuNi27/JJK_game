using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    [CustomEditor(typeof(ShibuyaParryProfile)),CanEditMultipleObjects]
    public sealed class ShibuyaParryInspector : UnityEditor.Editor
    {
        public static readonly Dictionary<string,string> Labels=new Dictionary<string,string>{
            {"parryEnabled","타이밍 패링 사용"},{"windowDuration","새 우클릭 패링 판정 시간"},{"retryInterval","패링 시작 사이 최소 간격"},
            {"halfAngle","패링 정면 반각 (도)"},{"attackerStun","상대 경직·밀려남 시간"},{"pushDistance","상대 밀려남 목표 거리"},
            {"successMotionDuration","방어자 쳐내기 동작·행동 제한 시간"},{"localHitStop","성공 시 두 참가자 국소 히트스톱"},
            {"effectShader","전용 패링 섬광 셰이더"},{"effectColor","패링 섬광 색상"},{"effectIntensity","섬광·충격파 강도"},
            {"effectSize","섬광·충격파 크기"},{"effectDuration","성공 이펙트 지속시간"},{"motionStrength","블록 캐릭터 쳐내기 강도"}
        };
        public override void OnInspectorGUI(){
            serializedObject.UpdateIfRequiredOrScript();var it=serializedObject.GetIterator();bool child=true;
            while(it.NextVisible(child)){child=false;using(new EditorGUI.DisabledScope(it.name=="m_Script"))EditorGUILayout.PropertyField(it,new GUIContent(it.name=="m_Script"?"스크립트":Labels[it.name]),true);}
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("재시도 제한 중에도 일반 가드는 가능합니다. 성공은 가드를 종료하며 새 우클릭을 다시 눌러야 가드를 재개합니다. 자동 반격은 없습니다.",MessageType.Info);
        }
    }
}
