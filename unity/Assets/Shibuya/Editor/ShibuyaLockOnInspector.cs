using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    [CustomEditor(typeof(ShibuyaLockOnProfile)), CanEditMultipleObjects]
    public sealed class ShibuyaLockOnProfileInspector : Editor
    {
        public static readonly Dictionary<string, string> Labels = new Dictionary<string, string> {
            {"acquireRange", "신규 선택 거리 (m)"}, {"retainRange", "기존 대상 유지 거리 (m)"},
            {"screenCenterRadius", "화면 중앙 후보 반경 (뷰포트 비율)"}, {"reselectHoldTime", "재선택으로 판단할 휠 누름 시간 (초)"},
            {"sightGraceTime", "환경 시야 상실 유예 (초)"}, {"environmentMask", "시야를 가리는 환경 레이어"},
            {"aimOffset", "대상 충돌체 중심 기준 조준점 보정"},
            {"attackFacingEnabled", "락온 공격 준비 회전 사용"}, {"attackTurnSpeed", "락온 공격 초당 회전 각도"},
            {"attackMaxTurnAngle", "타격별 락온 최대 회전 각도"}, {"attackPreparationTime", "락온 회전 준비 구간 상한 (초)"},
            {"cameraAssistEnabled", "느슨한 락온 카메라 사용"},
            {"cameraDeadZone", "자동 추적 여유 영역 (가로·세로 반폭)"},
            {"cameraAcceleration", "자동 회전 가속·감속 (도/초²)"},
            {"cameraShoulderOffset", "겹침 완화 최대 어깨 이동 (m)"},
            {"cameraOverlapStrength", "화면 겹침 완화 강도"},
            {"cameraOverlapOrbitAngle", "겹침 완화 최대 측면 구도 (도)"},
            {"cameraFramingSpeed", "어깨·높이·거리 보정 속도 (m/초)"},
            {"cameraFramingRange", "근거리 구도 보정 범위 (m)"},
            {"cameraExtraDistance", "추가 카메라 거리 상한 (m)"},
            {"cameraVerticalFocusWeight", "상대 높이의 시선 중심 반영 비율"},
            {"cameraVerticalFocusLimit", "시선 중심 높이 보정 상한 (m)"},
            {"cameraReturnGrace", "시네마틱 복귀 후 보정 대기 (초)"},
            {"cameraScreenRecoveryEnabled", "화면 밖 대상 복구 사용"},
            {"cameraRecoveryMargin", "화면 복귀 성공 여백 (뷰포트 비율)"},
            {"cameraRecoveryStallTime", "일반 보정 정체 확인 시간 (초)"},
            {"cameraRecoveryStableTime", "화면 복귀 안정 확인 시간 (초)"},
            {"cameraRecoveryTimeout", "화면 복구 시도 시간 상한 (초)"},
            {"cameraStrength", "카메라 보조 강도"},
            {"cameraSharpness", "카메라 보조 수렴 속도"}, {"cameraDegreesPerSecond", "카메라 초당 최대 회전 각도"},
            {"manualLookGrace", "마우스 조작 후 보조 대기 시간 (초)"}, {"showUI", "최소 락온 표시 사용"},
            {"lostFeedbackTime", "대상 상실 표시 시간 (초)"}, {"markerSize", "선택 표시 크기"},
            {"candidateColor", "후보 색상"}, {"lockedColor", "확정 대상 색상"}, {"occludedColor", "시야 가림 색상"},
            {"showAimDebug", "조준 방향 디버그 선 표시"}, {"debugAimLength", "디버그 선 길이 (m)"}
        };
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var it = serializedObject.GetIterator(); bool enter = true;
            while (it.NextVisible(enter)) {
                enter = false;
                if (it.name == "m_Script") { using (new EditorGUI.DisabledScope(true)) EditorGUILayout.PropertyField(it); continue; }
                EditorGUILayout.PropertyField(it, new GUIContent(Labels.TryGetValue(it.name, out var label) ? label : it.displayName), true);
            }
            serializedObject.ApplyModifiedProperties();
            foreach (var obj in targets) if (!((ShibuyaLockOnProfile)obj).IsValid) {
                EditorGUILayout.HelpBox("유효한 수치가 필요합니다. 유지 거리는 신규 선택 거리 이상이어야 합니다.", MessageType.Error); break;
            }
        }
    }
    [CustomEditor(typeof(ShibuyaLockOn)), CanEditMultipleObjects]
    public sealed class ShibuyaLockOnInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("settings"), new GUIContent("타깃 락온 설정"));
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.HelpBox("기존 씬·프리팹은 수정하지 않습니다. 플레이어 입력이 실행 중에 공통 Resources 설정을 연결합니다.", MessageType.Info);
        }
    }
}
