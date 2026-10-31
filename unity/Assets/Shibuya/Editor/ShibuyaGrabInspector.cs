using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace JJKGame.Shibuya.EditorTools
{
    [CustomEditor(typeof(ShibuyaGrabProfile)),CanEditMultipleObjects]
    public sealed class ShibuyaGrabInspector : UnityEditor.Editor
    {
        public static readonly Dictionary<string,string> Labels=new Dictionary<string,string>{
            {"attack","공격 분류 (잡기는 기본 등급 비허용)"},{"grabEnabled","공통 잡기 사용"},{"range","최대 사거리 (몸 중심 간 m)"},{"halfAngle","정면 반각 (도)"},
            {"startup","잡기 준비 시간"},{"activeTime","유효 판정 시간"},{"damage","성공 HP 피해"},
            {"contactDuration","붙잡기 순간 표현 시간"},{"pushDistance","상대 밀려남 목표 거리"},{"victimStun","상대 경직·밀려남 시간"},
            {"successRecovery","성공 후 행동 회복 시간"},{"failureRecovery","실패·중단 후 행동 회복 시간"},{"motionStrength","블록 잡기 모션 강도"}
        };
        public override void OnInspectorGUI(){
            serializedObject.UpdateIfRequiredOrScript();var it=serializedObject.GetIterator();bool child=true;
            while(it.NextVisible(child)){child=false;using(new EditorGUI.DisabledScope(it.name=="m_Script"))EditorGUILayout.PropertyField(it,new GUIContent(it.name=="m_Script"?"스크립트":Labels[it.name]),true);}
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("실제 가드 중 새 좌클릭으로 시작합니다. 가드·패링만 우회하며 회피 무적·특수 방어는 유지합니다. 종료 후 재가드는 우클릭을 놓았다 다시 눌러야 합니다.",MessageType.Info);
        }
    }
}
