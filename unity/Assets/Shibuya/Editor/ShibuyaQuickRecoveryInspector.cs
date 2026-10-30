using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace JJKGame.Shibuya.EditorTools
{
    [CustomEditor(typeof(ShibuyaQuickRecoveryProfile)),CanEditMultipleObjects]
    public sealed class ShibuyaQuickRecoveryInspector : UnityEditor.Editor
    {
        public static readonly Dictionary<string,string> Labels=new Dictionary<string,string>{
            {"recoveryEnabled","다운 긴급 탈출 사용"},{"distance","탈출 이동 거리"},{"duration","탈출 이동 시간"},
            {"speedCurve","이동 속도 곡선"},{"evadeStart","회피 무적 시작 시간"},{"evadeEnd","회피 무적 종료 시간"},
            {"cooldownAfterEnd","탈출 종료·중단 후 재사용 제한"},{"poseStrength","임시 방향 기상 포즈 강도"},{"poseDuration","임시 기상 포즈 복구 시간"}
        };
        public override void OnInspectorGUI(){
            serializedObject.UpdateIfRequiredOrScript();var it=serializedObject.GetIterator();bool child=true;
            while(it.NextVisible(child)){child=false;using(new EditorGUI.DisabledScope(it.name=="m_Script"))EditorGUILayout.PropertyField(it,new GUIContent(it.name=="m_Script"?"스크립트":Labels[it.name]),true);}
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("최소 대기·자연 기상 시간은 기존 타격의 다운 설정이 단독 소유합니다. 실제 착지 후 새 WASD+Space로만 탈출하며 입력을 예약하지 않습니다.",MessageType.Info);
        }
    }
}
