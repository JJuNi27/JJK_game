using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    [CustomEditor(typeof(ShibuyaGroundDashProfile)), CanEditMultipleObjects]
    public sealed class ShibuyaGroundDashInspector : UnityEditor.Editor
    {
        public static readonly Dictionary<string, string> Labels = new Dictionary<string, string> {
            {"dashEnabled", "지상 방향 대시 사용"}, {"forwardDistance", "전방 이동 거리 (m)"}, {"forwardDuration", "전방 지속 시간 (초)"},
            {"sideDistance", "측면 이동 거리 (m)"}, {"sideDuration", "측면 지속 시간 (초)"},
            {"backwardDistance", "후방 이동 거리 (m)"}, {"backwardDuration", "후방 지속 시간 (초)"},
            {"evadeStartDelay", "회피 무적 시작 지연 (초)"}, {"evadeDuration", "측면·후방 무적 시간 (초)"},
            {"cooldownAfterEnd", "대시 종료 후 공통 쿨타임 (초)"}, {"speedCurve", "시간별 속도 분포 (적분 후 거리 정규화)"},
            {"stepLeanAngle", "블록 캐릭터 스텝 기울기 (도)"}
        };
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript(); var it = serializedObject.GetIterator(); bool children = true;
            while (it.NextVisible(children)) {
                children = false;
                using (new EditorGUI.DisabledScope(it.name == "m_Script"))
                    EditorGUILayout.PropertyField(it, new GUIContent(it.name == "m_Script" ? "스크립트" : Labels[it.name]), true);
            }
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("전방 성분 대각선은 전방(무적 없음), 후방 대각선은 후방 회피입니다. 무적은 회피 동작 내 지연 후 짧게 적용되며 일반 체술만 차단합니다. 속도 곡선의 음수 구간은 0, 전체가 0이면 일정 속도로 처리합니다. 값은 USER VERIFIED 전 실험값입니다.", MessageType.Info);
        }
    }
}
