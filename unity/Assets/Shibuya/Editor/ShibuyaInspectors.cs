using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    // Scoped to new components: preserves SerializedProperty editing, Undo and prefab overrides.
    public abstract class ShibuyaInspector : UnityEditor.Editor
    {
        private static readonly Dictionary<string, string> Labels = new Dictionary<string, string> {
            {"walkSpeed", "걷기 속도"}, {"runSpeed", "달리기 속도"}, {"turnSpeed", "회전 속도"},
            {"jumpHeight", "점프 높이"}, {"gravity", "중력 가속도"}, {"attackMoveMultiplier", "공격 중 이동 배율"},
            {"damage", "타격 피해량"}, {"attackRadius", "공격 판정 반경"}, {"attackForwardOffset", "공격 판정 전방 거리"},
            {"attackHeight", "공격 판정 높이"}, {"startup", "공격 준비 시간"}, {"activeTime", "공격 활성 시간"},
            {"recovery", "공격 회복 시간"}, {"knockbackSpeed", "수평 넉백 속도"}, {"hitStun", "피격 경직 시간"},
            {"knockbackDamping", "넉백 감속도"}, {"cameraDistance", "카메라 거리"}, {"cameraHeight", "카메라 주시 높이"},
            {"mouseSensitivity", "마우스 감도"}, {"minimumPitch", "최저 시점 각도"}, {"maximumPitch", "최고 시점 각도"},
            {"cameraCollisionRadius", "카메라 충돌 반경"}, {"cameraFollowSharpness", "카메라 추적 반응 속도"},
            {"hitFlashDuration", "피격 강조 시간"}, {"hitSoundVolume", "타격음 크기"}, {"profile", "전투 설정"},
            {"stats", "체력 설정"}, {"orbitCamera", "자유 시점 카메라"}, {"target", "추적 대상"},
            {"visualRoot", "블록 외형 루트"}, {"leftArm", "왼팔"}, {"rightArm", "오른팔"},
            {"leftLeg", "왼다리"}, {"rightLeg", "오른다리"}, {"player", "플레이어"}, {"dummy", "훈련 더미"},
            {"source", "타격 이벤트 소유자"}, {"impactMaterial", "충돌 강조 재질"}
        };
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty property = serializedObject.GetIterator();
            bool enter = true;
            while (property.NextVisible(enter))
            {
                enter = false;
                if (property.name == "m_Script")
                {
                    using (new EditorGUI.DisabledScope(true)) EditorGUILayout.PropertyField(property, new GUIContent("스크립트"));
                    continue;
                }
                EditorGUILayout.PropertyField(property,
                    new GUIContent(Labels.TryGetValue(property.name, out string label) ? label : property.displayName), true);
            }
            serializedObject.ApplyModifiedProperties();
            if (target is ShibuyaMatch match)
            {
                EditorGUILayout.HelpBox("Play Mode에서 패배 경로를 확인할 수 있습니다. 게임 빌드에는 이 검증 명령이 포함되지 않습니다.", MessageType.Info);
                using (new EditorGUI.DisabledScope(!Application.isPlaying || match.Finished))
                    if (GUILayout.Button("플레이어 패배 검증")) match.EditorDefeatPlayer();
            }
        }
    }
    [CustomEditor(typeof(ShibuyaCombatProfile)), CanEditMultipleObjects] public sealed class ShibuyaCombatProfileEditor : ShibuyaInspector { }
    [CustomEditor(typeof(ShibuyaFighter)), CanEditMultipleObjects] public sealed class ShibuyaFighterEditor : ShibuyaInspector { }
    [CustomEditor(typeof(ShibuyaPlayerInput)), CanEditMultipleObjects] public sealed class ShibuyaPlayerInputEditor : ShibuyaInspector { }
    [CustomEditor(typeof(ShibuyaOrbitCamera)), CanEditMultipleObjects] public sealed class ShibuyaOrbitCameraEditor : ShibuyaInspector { }
    [CustomEditor(typeof(ShibuyaBlockVisual)), CanEditMultipleObjects] public sealed class ShibuyaBlockVisualEditor : ShibuyaInspector { }
    [CustomEditor(typeof(ShibuyaMatch))] public sealed class ShibuyaMatchEditor : ShibuyaInspector { }
    [CustomEditor(typeof(ShibuyaHitFeedback)), CanEditMultipleObjects] public sealed class ShibuyaHitFeedbackEditor : ShibuyaInspector { }
}
