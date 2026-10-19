using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    // Scoped to new components: preserves SerializedProperty editing, Undo and prefab overrides.
    public abstract class ShibuyaInspector : UnityEditor.Editor
    {
        private static readonly Dictionary<string, string> Labels = new Dictionary<string, string> {
            {"aerialHoverEnabled", "공중 공격 중 완전 체공"}, {"aerialMaxHoverTime", "공중 연계 총 체공 상한 (실시간)"},
            {"aerialLinkInputWindow", "공중 후속 클릭 허용 시간 (0이면 1회만)"}, {"holdVictimOnAirHit", "유효 공중 피격 경직 중 높이 유지"},
            {"aerialCombat", "공중 추격·체술 설정"}, {"chaseEnabled", "공중 추격 사용"},
            {"chaseInputWindow", "어퍼컷 적중 후 Space 허용 시간"}, {"chaseInputBuffer", "추격 입력 버퍼 시간"},
            {"chaseMaxRange", "추격 대상 최대 거리"}, {"chaseMaxTravel", "추격 누적 이동 상한"},
            {"chaseMaxDuration", "추격 최대 지속 시간"}, {"chaseSpeed", "추격 최고 속도"},
            {"chaseDeceleration", "도착 접근 감속도"}, {"chaseArrivalDistance", "목표 도착 간격"},
            {"chaseArrivalTolerance", "도착 간격 추가 허용 오차"},
            {"chaseVerticalTolerance", "도착 높이 허용 오차"}, {"chasePredictionTime", "추격 위치 예측 시간"},
            {"chaseMinimumRise", "공중 도착에 필요한 실제 상승 높이"},
            {"chasePredictionLimit", "추격 예측 이동 상한"}, {"chaseMaxTurnAngle", "추격 누적 회전 상한"},
            {"chaseTurnSpeed", "추격 회전 속도"}, {"chaseStallTimeout", "충돌 정체 종료 시간"},
            {"chaseLandingSeparationSpeed", "추격 후 착지 캡슐 분리 속도"},
            {"aerialInputWindow", "도착 후 공중 체술 입력 시간"}, {"aerialMaxHeightDifference", "공중 타격 허용 높이 차"},
            {"aerialAirControl", "추격 후 낙하 중 이동 입력 배율"}, {"failureFallSpeedLimit", "추격 실패 직후 낙하 속도 상한"},
            {"chaseLeanAngle", "추격 자세 기울기"}, {"aerialAttack", "LMB 1회 공중 체술 단계"},
            {"overrideTargeting", "이 타격의 추적 설정 개별 사용"}, {"targeting", "타격별 추적 보정"},
            {"acquisitionAngle", "첫 대상 선정 정면 각도"}, {"retentionAngle", "콤보 대상 유지 각도"},
            {"approachDistance", "추가 접근 최대 거리"}, {"approachSpeed", "추가 접근 속도"},
            {"preparationTime", "준비 중 추적 허용 시간"}, {"retentionTime", "마지막 적중 후 대상 유지 시간"},
            {"maxHeightDifference", "추적 허용 높이 차"}, {"firstTargetCone", "첫 대상 선정 정면 각도"},
            {"targetRetentionTime", "마지막 적중 후 대상 유지 시간"},
            {"trackingMaxHeightDifference", "추적 허용 높이 차"}, {"trackingPredictionTime", "넉백 속도 예측 시간"},
            {"showCombatGizmos", "선택 시 전투 진단 표시 (기본 꺼짐)"},
            {"usePeakHeight", "상승 속도 대신 목표 높이 사용"}, {"peakHeight", "발사 위치 기준 상승 목표 높이"},
            {"launchHorizontalDistance", "상승 중 수평 발사 거리"}, {"launchHorizontalDuration", "수평 발사 시간"},
            {"descendingHorizontalSpeed", "낙하 중 수평 속도"}, {"landingSeparationSpeed", "착지 캡슐 분리 속도"},
            {"groundCrackEnabled", "실제 바닥 충돌 균열 사용"}, {"crackRadius", "바닥 균열 반경"},
            {"crackHoldTime", "바닥 균열 유지 시간"}, {"crackFadeTime", "바닥 균열 사라지는 시간"},
            {"crackDarkness", "바닥 균열 진하기"},
            {"finishers", "방향별 마무리 설정"}, {"finisherStepIndex", "방향 마무리 입력 단계 (0부터)"},
            {"uppercut", "W 어퍼컷 마무리"}, {"down", "다운 상태 설정"},
            {"downFinisher", "S 다운 마무리"},
            {"outcome", "유효 적중 후 상태"}, {"air", "공중 상태 설정"},
            {"launchSpeed", "어퍼컷 상승 속도"}, {"airGravity", "공중 피격 중력"},
            {"apexHoldTime", "정점 체공 시간"}, {"airReactionLean", "공중 피격 외형 기울기"},
            {"fallSpeed", "다운 타격 낙하 속도"}, {"minimumDownTime", "최소 다운 유지 시간"},
            {"naturalRecoveryTime", "착지 후 자연 기상 시작 시간"}, {"getUpDuration", "기상 동작 시간"},
            {"downPoseAngle", "다운 외형 기울기"},
            {"confirmedLinkEnabled", "유효 예약 입력의 확정 연계 사용"},
            {"confirmedLinkInputWindow", "확정 연계 후속 입력 허용 시간"},
            {"confirmedLinkMaxHold", "한 연결의 최대 추가 구속 시간"},
            {"confirmedLinkMaxSequence", "한 콤보의 최대 연결 보장 시간"},
            {"moveset", "캐릭터 무브셋 (비우면 기존 4타)"},
            {"steps", "입력별 콤보 단계"}, {"hits", "시간순 타격 이벤트"}, {"movements", "실제 이동 클립"},
            {"startTime", "단계 시작 후 발동 시간"}, {"strike", "타격 및 시각 모션 설정"},
            {"requiredMovement", "도착 필수 이동 번호 (-1 없음, 0부터)"}, {"heavyReaction", "강타 피격 모션"},
            {"knockbackKind", "넉백 이동 방식"}, {"knockbackDistance", "넉백 요청 거리 (충돌 시 감소)"},
            {"knockbackDuration", "거리 넉백 시간"}, {"kind", "이동 방식"}, {"duration", "이동 제한 시간"},
            {"maxDistance", "최대 이동 거리"}, {"speed", "이동 속도"}, {"targetRange", "대상 유지 최대 거리"},
            {"arrivalDistance", "목표 도착 거리"}, {"maxTurnAngle", "최대 누적 회전각"},
            {"predictionTime", "현재 속도 기반 예측 시간"},
            {"advanceDistance", "타격 전진 최대 거리"}, {"advanceSpeed", "타격 전진 속도"},
            {"advanceStartTime", "타격 전진 시작 시간"}, {"advanceDuration", "타격 전진 지속 시간"},
            {"powerWindupTime", "힘 모으기 시간 (준비 시간에 포함)"}, {"windupMoveMultiplier", "힘 모으기 중 이동 배율"},
            {"hitStopDuration", "적중 시 국소 히트스톱 시간"}, {"soundPitch", "타격음 높낮이"},
            {"trackingEnabled", "준비 구간 타깃 보정 사용"}, {"trackingTargetRange", "보정 대상 최대 거리"},
            {"trackingConeAngle", "보정 대상 허용 각도"}, {"trackingMaxTurnAngle", "한 타격의 최대 회전 보정 각도"},
            {"trackingTurnSpeed", "방향 보정 속도 (도/초)"}, {"trackingAdvanceDistance", "추가 전진 보정 최대 거리"},
            {"trackingAdvanceSpeed", "추가 전진 보정 속도"}, {"trackingDuration", "준비 구간 보정 적용 시간"},
            {"directionalInputOverrideAngle", "방향 입력 우선 각도"},
            {"attackLateralInputMultiplier", "준비·활성 중 좌우 입력 영향도"},
            {"attackBackwardInputMultiplier", "준비·활성 중 후방 입력 영향도"},
            {"alignVictimOnHit", "적중 후 피격자 측면 정렬 허용"},
            {"victimAlignmentEnabled", "피격자 측면 정렬 사용"},
            {"victimAlignmentMaxLateralOffset", "피격자 정렬 허용 측면 오차"},
            {"victimAlignmentMaxDistance", "한 타격의 피격자 정렬 최대 거리"},
            {"victimAlignmentSpeed", "피격자 정렬 속도"},
            {"victimAlignmentDuration", "피격자 정렬 최대 시간"},
            {"strikes", "지상 4타 콤보"}, {"inputBufferTime", "후속 입력 버퍼 시간"}, {"comboResetTime", "콤보 초기화 대기 시간"},
            {"knockbackDirection", "넉백 방향 정책"}, {"motion", "체술 모션"}, {"presentationIntensity", "타격 연출 강도"},
            {"walkSpeed", "걷기 속도"}, {"runSpeed", "달리기 속도"}, {"turnSpeed", "회전 속도"},
            {"jumpHeight", "점프 높이"}, {"gravity", "중력 가속도"}, {"attackMoveMultiplier", "공격 중 이동 배율"},
            {"damage", "타격 피해량"}, {"attackRadius", "공격 판정 반경"}, {"attackForwardOffset", "공격 판정 전방 거리"},
            {"attackHeight", "공격 판정 높이"}, {"startup", "공격 준비 시간"}, {"activeTime", "공격 활성 시간"},
            {"recovery", "공격 회복 시간"}, {"knockbackSpeed", "수평 넉백 속도"}, {"hitStun", "피격 경직 시간"},
            {"knockbackDamping", "넉백 감속도"}, {"cameraDistance", "카메라 거리"}, {"cameraHeight", "카메라 주시 높이"},
            {"mouseSensitivity", "마우스 감도"}, {"minimumPitch", "최저 시점 각도"}, {"maximumPitch", "최고 시점 각도"},
            {"cameraCollisionRadius", "카메라 충돌 반경"}, {"cameraFollowSharpness", "카메라 추적 반응 속도"},
            {"hitFlashDuration", "피격 강조 시간"}, {"hitSoundVolume", "타격음 크기"}, {"profile", "전투 설정"},
            {"hitReactionEnabled", "방향별 피격 모션 사용"},
            {"frontHitReactionStrength", "정면 피격 반응 강도"}, {"leftHitReactionStrength", "왼쪽 피격 반응 강도"},
            {"rightHitReactionStrength", "오른쪽 피격 반응 강도"}, {"backHitReactionStrength", "후방 피격 반응 강도"},
            {"hitReactionLeanAngle", "상체 피격 기울기 (도)"}, {"hitReactionTwistAngle", "상체·어깨 비틀림 (도)"},
            {"hitReactionStartTime", "피격 자세 진입 시간"}, {"hitReactionRecoveryTime", "피격 자세 회복 시간"},
            {"normalHitReactionMultiplier", "일반 타격 피격 반응 배율"}, {"heavyHitReactionMultiplier", "4타 강타 피격 반응 배율"},
            {"stats", "체력 설정"}, {"orbitCamera", "자유 시점 카메라"}, {"target", "추적 대상"},
            {"visualRoot", "블록 외형 루트"}, {"leftArm", "왼팔"}, {"rightArm", "오른팔"},
            {"leftLeg", "왼다리"}, {"rightLeg", "오른다리"}, {"player", "플레이어"}, {"dummy", "훈련 더미"},
            {"source", "타격 이벤트 소유자"}, {"impactMaterial", "충돌 강조 재질"}
        };
        public static string KoreanLabel(string name) => Labels.TryGetValue(name, out string label) ? label : name;
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
                if (property.name == "strikes")
                {
                    property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, "지상 4타 콤보", true);
                    if (property.isExpanded)
                    {
                        using (new EditorGUI.IndentLevelScope())
                            for (int i = 0; i < property.arraySize; i++)
                                EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(i), new GUIContent($"{i + 1}타"), true);
                    }
                    continue;
                }
                EditorGUILayout.PropertyField(property,
                    new GUIContent(Labels.TryGetValue(property.name, out string label) ? label : property.displayName), true);
            }
            serializedObject.ApplyModifiedProperties();
            if (target is ShibuyaCombatProfile)
            {
                EditorGUILayout.HelpBox("피해량은 각 타격의 설정값을 사용합니다. 현재 캐릭터 능력치의 공격·방어 배율은 적용되지 않습니다. 입력 버퍼는 공격 종료 직전의 후속 클릭 1회만 예약합니다. 콤보 초기화는 공격 회복 완료 후부터 계산합니다.", MessageType.Info);
                EditorGUILayout.HelpBox("피격자 정렬은 공격자 전방 넉백 정책의 유효 적중에서만 적용됩니다. 피격 경직이 끝나거나 공격이 중단되면 종료됩니다. 준비·활성 중 좌우/후방 입력 영향도는 일반 이동만 제한하며 타격 고유 전진과 원본 입력은 유지합니다. 회복 구간에서 일반 이동이 점진적으로 복귀합니다.", MessageType.Info);
                EditorGUILayout.HelpBox("방향별 피격 설정은 피격자의 상체·머리·팔 외형에만 적용됩니다. 방향은 실제 타격 진행 방향 기준이며 위치 정렬·넉백·캐릭터 시선은 바꾸지 않습니다. 히트스톱이 있는 타격은 정지 시작부터 강한 피격 자세를 고정한 뒤 회복합니다.", MessageType.Info);
            }
            if (target is ShibuyaMatch match)
            {
                EditorGUILayout.HelpBox("Play Mode에서 패배 경로를 확인할 수 있습니다. 게임 빌드에는 이 검증 명령이 포함되지 않습니다.", MessageType.Info);
                using (new EditorGUI.DisabledScope(!Application.isPlaying || match.Finished))
                    if (GUILayout.Button("플레이어 패배 검증")) match.EditorDefeatPlayer();
            }
        }
    }

    [CustomPropertyDrawer(typeof(ShibuyaStrikeDefinition))]
    public sealed class ShibuyaStrikeDrawer : PropertyDrawer
    {
        private static readonly string[] Fields = { "damage", "startup", "activeTime", "recovery", "attackRadius",
            "attackForwardOffset", "attackHeight", "hitStun", "knockbackSpeed", "knockbackDirection", "motion", "presentationIntensity",
            "advanceDistance", "advanceSpeed", "advanceStartTime", "advanceDuration", "powerWindupTime", "windupMoveMultiplier",
            "hitStopDuration", "soundPitch", "alignVictimOnHit", "overrideTargeting", "targeting" };
        private static string[] VisibleFields(SerializedProperty property) => System.Array.FindAll(Fields,
            field => (!property.propertyPath.Contains(".hits.") || !field.StartsWith("advance"))
                && (field!="knockbackSpeed" || !IsLaunchEvent(property))
                && (field!="targeting" || property.FindPropertyRelative("overrideTargeting").boolValue));
        private static bool IsLaunchEvent(SerializedProperty property) {
            int index=property.propertyPath.LastIndexOf(".strike",System.StringComparison.Ordinal);
            if(index<0) return false;
            var outcome=property.serializedObject.FindProperty(property.propertyPath.Substring(0,index)+".outcome");
            return outcome!=null && outcome.enumValueIndex==(int)ShibuyaHitOutcome.Launch;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float height=EditorGUIUtility.singleLineHeight;
            if(property.isExpanded) foreach(string field in VisibleFields(property))
                height+=EditorGUIUtility.standardVerticalSpacing+EditorGUI.GetPropertyHeight(property.FindPropertyRelative(field),true);
            return height;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);
            if (property.isExpanded)
            {
                using (new EditorGUI.IndentLevelScope())
                    foreach (string field in VisibleFields(property))
                    {
                        position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
                        position.height=EditorGUI.GetPropertyHeight(property.FindPropertyRelative(field),true);
                        EditorGUI.PropertyField(position, property.FindPropertyRelative(field), new GUIContent(ShibuyaInspector.KoreanLabel(field)),true);
                    }
            }
            EditorGUI.EndProperty();
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
