using System.Collections.Generic;
using JJKGame.Core;
using JJKGame.Dev.VFXLab;
using JJKGame.Enemy;
using JJKGame.Player;
using UnityEditor;
using UnityEngine;

namespace JJKGame.EditorTools
{
    /// <summary>
    /// Explicit label table for the small set of user-facing combat/profile inspectors.
    /// SerializedProperty remains responsible for editing, so Unity's normal Undo,
    /// prefab override, multi-edit, range, object-reference and enum behavior is kept.
    /// </summary>
    internal static class KoreanInspectorLabels
    {
        private static readonly IReadOnlyDictionary<string, string> Labels =
            new Dictionary<string, string>
            {
                ["blueCompressionEnabled"] = "창 준비 압축 연출 사용",
                ["blueCompressionStrength"] = "창 준비 압축 강도",
                ["blueDebrisScale"] = "창 파편 크기",
                ["blueHitAccents"] = "창 타격별 강조 (X=1타, Y=2타, Z=3타, W=4타)",
                ["blueDustLeadTime"] = "창 먼지 예고 선행 시간",
                ["blueAftermathDuration"] = "창 종료 후 잔류 시간",
                ["blueFinalCoreScale"] = "창 마지막 압축 중심 크기 배율",
                ["blueFinalShake"] = "창 마지막 타격 카메라 진동",
                ["redFlashIntensity"] = "혁 섬광 강도",
                ["redShockRingScale"] = "혁 압력 전면 크기",
                ["redAftermathDuration"] = "혁 압력 잔류 시간",
                ["redAftermathDensity"] = "혁 압력 증기 밀도",
                ["redFrontIrregularity"] = "혁 압력 전면 불규칙도",
                ["redSurfaceDebrisCount"] = "혁 표면 파편 수",
                ["redSurfaceReactionScale"] = "혁 표면 반응 크기",
                ["purpleFusionHoldDuration"] = "자 완성 후 충전 시간",
                ["purpleFormationScale"] = "자 소환용 창·혁 크기",
                ["purpleFormationSeparation"] = "자 소환용 창·혁 중심 간격 (편측)",
                ["purpleTerminalDuration"] = "자 종점 폭발 시간",
                ["purpleTerminalScale"] = "자 종점 폭발 크기",
                ["purpleVisualScale"] = "자 시각 크기",
                ["purpleHoldOffset"] = "자 완성 위치 보정 (Y 높이, Z 전방)",
                ["purpleScarWidthMultiplier"] = "자 지름 대비 상흔 폭 배율",
                ["purpleScarDuration"] = "자 상흔 잔류 시간",
                ["purpleBlueLeadDuration"] = "자 · 창 단독 등장 시간",
                ["purpleRedOppositionDuration"] = "자 · 혁 등장과 대치 시간",
                ["purpleFusionDuration"] = "자 · 두 힘의 융합 시간",
                ["purpleImpactFrameCount"] = "자 · 흑백 연출 프레임 수 (60fps 기준)",
                ["purpleVolumeDensity"] = "자 내부 에너지 밀도",
                ["purpleEmission"] = "자 기본 발광 강도",
                ["purpleTurbulence"] = "자 에너지 난류 강도",
                ["purpleFlowSpeed"] = "자 내부 흐름 속도",
                ["purpleHazeOpacity"] = "자 외곽 공간 안개",
                ["purpleRibbonWidth"] = "자 곡면 리본 굵기",
                ["purpleLightningWidth"] = "자 번개 굵기",
                ["purpleLightningIntensity"] = "자 번개 발광 강도",
                ["purpleReleaseEmission"] = "자 방출 순간 발광 배율",
                ["purpleCameraChoreography"] = "자 짧은 카메라 연출 사용",
                ["purpleImpactFrames"] = "자 흑백 임팩트 프레임 사용",
                ["purpleCameraSideOffset"] = "자 카메라 측면 거리",
                ["purpleCameraTensionFov"] = "자 방출 직전 시야각 압축",
                ["purpleReleaseFov"] = "자 방출 시야각 충격",
                ["purpleCameraImpulse"] = "자 방출 카메라 진동",
                ["purpleCameraFollowThrough"] = "자 방출 후 카메라 복귀 시간",
                ["characterProfile"] = "캐릭터 프로필",
                ["profile"] = "데이터 프로필",
                ["profileLabel"] = "표시 이름",
                ["movementProfile"] = "이동 프로필",
                ["authoredModelRoot"] = "캐릭터 모델 루트",
                ["animator"] = "애니메이터",
                ["planarSpeedParameter"] = "평면 이동 속도 파라미터",
                ["idleTrigger"] = "대기 트리거",
                ["basicAttack1Trigger"] = "평타 1타 트리거",
                ["basicAttack2Trigger"] = "평타 2타 트리거",
                ["basicAttackFinisherTrigger"] = "평타 마무리 트리거",
                ["dodgeTrigger"] = "회피 트리거",
                ["anticipationTrigger"] = "기술 준비 동작 트리거",
                ["castTrigger"] = "기술 시전 트리거",
                ["releaseTrigger"] = "기술 방출 트리거",
                ["recoverTrigger"] = "기술 회복 트리거",
                ["domainAnticipationTrigger"] = "영역 준비 동작 트리거",
                ["domainReleaseTrigger"] = "영역 방출 트리거",

                ["domainActiveDuration"] = "영역 유지시간",
                ["domainPresentation"] = "영역 연출 설정",

                ["characterId"] = "캐릭터 ID",
                ["displayName"] = "표시 이름",
                ["displayNameOverride"] = "표시 이름 재정의",
                ["animatorController"] = "애니메이터 컨트롤러",
                ["movement"] = "이동 프로필",
                ["combatAudio"] = "전투 오디오 프로필",
                ["stats"] = "전투 능력치",
                ["cursedEnergy"] = "주력",
                ["basicAttack"] = "평타",
                ["gojoTechniques"] = "술식 게임플레이",
                ["burnout"] = "술식 번아웃",
                ["targeting"] = "타게팅",
                ["trainingBot"] = "훈련 봇",
                ["traits"] = "특성",
                ["passives"] = "패시브",
                ["characters"] = "캐릭터 정의 목록",

                ["maxHealth"] = "최대 체력",
                ["baseAttackMultiplier"] = "기본 공격 배율",
                ["defenseMultiplier"] = "방어 배율",
                ["maxEnergy"] = "최대 주력",
                ["startingEnergy"] = "시작 주력",
                ["regenerationPerSecond"] = "초당 회복량",
                ["regenerationDelayAfterSpend"] = "사용 후 회복 지연",
                ["costMultiplier"] = "주력 소모 배율",
                ["minimumTechniqueCost"] = "최소 술식 소모량",
                ["noticeDuration"] = "알림 표시 시간",

                ["walkSpeed"] = "걷기 속도",
                ["runSpeed"] = "달리기 속도",
                ["evade"] = "회피 설정",
                ["burstSpeed"] = "순간 속도",
                ["movementDuration"] = "이동 시간",
                ["speedCurve"] = "속도 곡선",
                ["recoveryDuration"] = "회복 시간",
                ["cooldown"] = "재사용 대기시간",
                ["invulnerabilityStart"] = "무적 시작 시점",
                ["invulnerabilityDuration"] = "무적 지속시간",
                ["styleId"] = "연출 스타일 식별자",
                ["animationTrigger"] = "애니메이션 트리거",
                ["animationState"] = "애니메이션 상태",
                ["vfxCue"] = "시각 효과 큐",
                ["sfxCue"] = "효과음 큐",

                ["skill1"] = "기술 1",
                ["skill2"] = "기술 2",
                ["ultimate"] = "필살기",
                ["domain"] = "영역",
                ["voice"] = "음성",
                ["castSfx"] = "시전 효과음",
                ["releaseSfx"] = "방출 효과음",
                ["travelSfx"] = "이동 효과음",
                ["impactSfx"] = "충돌 효과음",
                ["beatHookId"] = "비트 훅 식별자",
                ["basicSwing"] = "평타 휘두르기 효과음",
                ["basicHit"] = "평타 적중 효과음",
                ["basicFinisher"] = "평타 마무리 효과음",
                ["dodge"] = "회피 효과음",

                ["presentationProfile"] = "전투 오디오 프로필",
                ["backgroundMusic"] = "전투 배경음악",
                ["blueVoice"] = "아오 음성",
                ["redVoice"] = "아카 음성",
                ["purpleVoice"] = "무라사키 음성",
                ["domainVoice"] = "무량공처 음성",
                ["basicSwingSound"] = "평타 휘두르기 효과음",
                ["basicHitSound"] = "평타 적중 효과음",
                ["basicFinisherSound"] = "평타 마무리 효과음",
                ["playerHitSound"] = "피격 효과음",
                ["dodgeSound"] = "회피 효과음",
                ["victorySound"] = "승리 효과음",
                ["defeatSound"] = "패배 효과음",
                ["sfxVolume"] = "효과음 볼륨",
                ["voiceVolume"] = "음성 볼륨",
                ["musicVolume"] = "음악 볼륨",

                ["barrierVisualDiameterMeters"] = "결계 시각 지름 (미터)",
                ["fitBarrierToCapturedParticipants"] = "포획 대상에 결계 맞춤",
                ["barrierCloseDuration"] = "결계 닫힘 시간",
                ["barrierOpacity"] = "결계 불투명도",
                ["blackTransitionDuration"] = "암전 전환 시간",
                ["barrierReleaseDuration"] = "영역 해제 시간",
                ["interiorOrigin"] = "내부 공간 원점",
                ["interiorRadius"] = "내부 공간 반경",
                ["interiorBrightness"] = "내부 밝기",
                ["whiteBloodDensity"] = "화이트 블러드 밀도",
                ["whiteBloodScale"] = "화이트 블러드 크기",
                ["nebulaIntensity"] = "성운 강도",
                ["whiteBloodDepth"] = "화이트 블러드 깊이",
                ["whiteBloodVerticalSpread"] = "화이트 블러드 수직 분산",
                ["hideVisualFloor"] = "시각 바닥 숨김",
                ["centralFocalScale"] = "중앙 초점 크기",
                ["centralFocalIntensity"] = "중앙 초점 강도",
                ["voidReadDuration"] = "영역 읽기 시간",
                ["cloudBeatInterval"] = "구름 비트 간격",
                ["cloudBloomDuration"] = "구름 개화 시간",
                ["Group1Time"] = "1그룹 타격 시각",
                ["Group2Time"] = "2그룹 첫 타격 시각",
                ["Group2SubBeatGap"] = "2그룹 연타 간격",
                ["Group3Time"] = "3그룹 타격 시각",
                ["cinematicEnabled"] = "시네마틱 사용",
                ["handShotDuration"] = "손 디테일 샷 시간",
                ["faceShotDuration"] = "얼굴 샷 시간",
                ["eyeShotDuration"] = "눈 샷 시간",
                ["heroShotDuration"] = "히어로 샷 시간",
                ["victimShotDuration"] = "피격 대상 샷 시간",
                ["cameraReturnDuration"] = "카메라 복귀 시간",
                ["handOffset"] = "손 기준 오프셋",
                ["faceOffset"] = "얼굴 기준 오프셋",
                ["detailCameraOffset"] = "디테일 카메라 오프셋",
                ["heroCameraOffset"] = "히어로 카메라 오프셋",
                ["detailFov"] = "디테일 샷 화각",
                ["eyeFov"] = "눈 샷 화각",
                ["wideFov"] = "와이드 샷 화각",
                ["handAnchor"] = "손 앵커",
                ["faceAnchor"] = "얼굴 앵커",
                ["eyeAnchor"] = "눈 앵커",
                ["onShot"] = "샷 변경 이벤트",

                ["burnoutDuration"] = "번아웃 지속시간",
                ["attackOrigin"] = "공격 판정 중심",
                ["attackRadius"] = "공격 반경",
                ["comboResetDelay"] = "콤보 초기화 시간",
                ["comboDisplayDuration"] = "콤보 표시 시간",
                ["hitComboResetDelay"] = "연속 적중 초기화 시간",
                ["steps"] = "공격 단계",
                ["damage"] = "피해량",
                ["knockback"] = "넉백 강도",
                ["hitStun"] = "경직 시간",
                ["finisher"] = "마무리 공격",
                ["presentationTag"] = "연출 태그",
                ["occursAfterDomain"] = "영역 종료 후 번아웃 발생",
                ["duration"] = "지속 시간",
                ["allowEarlyRecovery"] = "조기 회복 허용",
                ["blue"] = "순전 「창」 (Blue)",
                ["red"] = "반전 「혁」 (Red)",
                ["purple"] = "허식 「자」 (Purple)",
                ["blueRedSynergy"] = "창 → 혁 연계",
                ["castTime"] = "시전 시간",
                ["castDistance"] = "시전 거리",
                ["radius"] = "적중 반경",
                ["fieldDuration"] = "유지 시간",
                ["pulseInterval"] = "다단 적중 간격",
                ["pullSpeed"] = "끌어당김 속도",
                ["energyCost"] = "주력 소모량",
                ["lockedTargetOffset"] = "락온 대상 거리 보정",
                ["range"] = "최대 사거리",
                ["projectileSpeed"] = "투사체 속도",
                ["pushSpeed"] = "밀어내기 속도",
                ["spawnHeight"] = "생성 높이",
                ["spawnForwardOffset"] = "전방 생성 거리",
                ["preparationDuration"] = "준비 유지 시간",
                ["blueMarkDuration"] = "창 표식 유지 시간",
                ["bonusDamage"] = "추가 피해량",
                ["empoweredPushSpeed"] = "강화 밀어내기 속도",
                ["empoweredHitStun"] = "강화 경직 시간",
                ["readyTimeout"] = "영역 준비 제한 시간",
                ["rightToLeftTimeout"] = "우클릭 → 좌클릭 제한 시간",
                ["targetReleaseTime"] = "목표 해제 시간",
                ["releaseTolerance"] = "해제 허용 오차",
                ["failedDuration"] = "실패 상태 유지 시간",
                ["activeDuration"] = "영역 활성 시간",
                ["victimStunDuration"] = "피격자 정지 시간",
                ["captureRadius"] = "포획 반경",
                ["maxLockDistance"] = "최대 락온 거리",
                ["moveSpeed"] = "이동 속도",
                ["rotationSpeed"] = "회전 속도",
                ["gravity"] = "중력",
                ["engagementRadius"] = "교전 반경",
                ["engagementSlotTolerance"] = "교전 위치 허용 오차",
                ["attackMode"] = "공격 모드",
                ["attackRange"] = "공격 사거리",
                ["attackDamage"] = "공격 피해량",
                ["attackCooldown"] = "공격 재사용 대기시간",
                ["attackWindupDuration"] = "공격 준비 시간",
                ["attackReachBuffer"] = "공격 도달 여유 거리",
                ["knockbackDamping"] = "넉백 감쇠",
                ["domainController"] = "영역 컨트롤러",
                ["firstHitDamage"] = "1타 피해량",
                ["secondHitDamage"] = "2타 피해량",
                ["thirdHitDamage"] = "3타 피해량",
                ["firstHitCooldown"] = "1타 입력 간격",
                ["secondHitCooldown"] = "2타 입력 간격",
                ["thirdHitCooldown"] = "3타 입력 간격",
                ["firstHitKnockback"] = "1타 넉백",
                ["secondHitKnockback"] = "2타 넉백",
                ["thirdHitKnockback"] = "3타 넉백",
                ["firstHitStun"] = "1타 경직시간",
                ["secondHitStun"] = "2타 경직시간",
                ["thirdHitStun"] = "3타 경직시간",
            };

        private static readonly IReadOnlyDictionary<string, string> Tooltips =
            new Dictionary<string, string>
            {
                ["pulseInterval"] = "순전 「창」 유지 중 피해 판정이 반복되는 시간 간격입니다.",
                ["energyCost"] = "기술 또는 영역을 1회 사용할 때 소비되는 기본 주력입니다.",
                ["lockedTargetOffset"] = "락온 대상에게 기술을 생성할 때 확보하는 거리 보정값입니다.",
                ["captureRadius"] = "영역 발동 시 대상이 포획되는 게임플레이 판정 반경입니다.",
            };

        internal static GUIContent Content(SerializedProperty property)
        {
            return new GUIContent(
                TextFor(property.name, property.displayName),
                TooltipFor(property.name, property.tooltip));
        }

        internal static GUIContent Content(SerializedProperty property, string label)
        {
            return new GUIContent(label, TooltipFor(property.name, property.tooltip));
        }

        internal static string TextFor(string propertyName, string fallback)
        {
            return Labels.TryGetValue(propertyName, out string label) ? label : fallback;
        }

        internal static string TooltipFor(string propertyName, string fallback)
        {
            return Tooltips.TryGetValue(propertyName, out string tooltip) ? tooltip : fallback;
        }
    }

    internal abstract class KoreanSerializedObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                using (new EditorGUI.DisabledScope(property.propertyPath == "m_Script"))
                {
                    EditorGUILayout.PropertyField(
                        property,
                        KoreanInspectorLabels.Content(property),
                        true);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }

    internal abstract class KoreanNestedPropertyDrawer : PropertyDrawer
    {
        protected virtual GUIContent ContentFor(SerializedProperty property)
        {
            return KoreanInspectorLabels.Content(property);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded)
            {
                return height;
            }

            VisitDirectChildren(property, child =>
                height += EditorGUI.GetPropertyHeight(child, true)
                    + EditorGUIUtility.standardVerticalSpacing);
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect row = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(
                row,
                property.isExpanded,
                ContentFor(property),
                true);
            if (!property.isExpanded)
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                float y = row.yMax + EditorGUIUtility.standardVerticalSpacing;
                VisitDirectChildren(property, child =>
                {
                    float childHeight = EditorGUI.GetPropertyHeight(child, true);
                    Rect childRect = new Rect(position.x, y, position.width, childHeight);
                    EditorGUI.PropertyField(
                        childRect,
                        child,
                        ContentFor(child),
                        true);
                    y += childHeight + EditorGUIUtility.standardVerticalSpacing;
                });
            }
        }

        private static void VisitDirectChildren(
            SerializedProperty parent,
            System.Action<SerializedProperty> visitor)
        {
            SerializedProperty child = parent.Copy();
            SerializedProperty end = child.GetEndProperty();
            int childDepth = parent.depth + 1;
            bool enterChildren = true;
            while (child.NextVisible(enterChildren)
                && !SerializedProperty.EqualContents(child, end))
            {
                enterChildren = false;
                if (child.depth == childDepth)
                {
                    visitor(child.Copy());
                }
            }
        }
    }

    [CustomPropertyDrawer(typeof(CharacterMovementProfile))]
    internal sealed class CharacterMovementProfileDrawer : KoreanNestedPropertyDrawer { }

    [CustomPropertyDrawer(typeof(EvadeProfile))]
    internal sealed class EvadeProfileDrawer : KoreanNestedPropertyDrawer { }

    [CustomPropertyDrawer(typeof(TechniqueAudioProfile))]
    internal sealed class TechniqueAudioProfileDrawer : KoreanNestedPropertyDrawer { }

    [CustomPropertyDrawer(typeof(DomainPresentationSettings))]
    internal sealed class DomainPresentationSettingsDrawer : KoreanNestedPropertyDrawer { }

    [CustomPropertyDrawer(typeof(BasicAttackStep))]
    internal sealed class BasicAttackStepDrawer : KoreanNestedPropertyDrawer
    {
        protected override GUIContent ContentFor(SerializedProperty property)
        {
            return property.name == "cooldown"
                ? KoreanInspectorLabels.Content(property, "입력 간격")
                : base.ContentFor(property);
        }
    }

    [CustomPropertyDrawer(typeof(BlueGameplayData))]
    internal sealed class BlueGameplayDataDrawer : KoreanNestedPropertyDrawer { }

    [CustomPropertyDrawer(typeof(RedGameplayData))]
    internal sealed class RedGameplayDataDrawer : KoreanNestedPropertyDrawer { }

    [CustomPropertyDrawer(typeof(PurpleGameplayData))]
    internal sealed class PurpleGameplayDataDrawer : KoreanNestedPropertyDrawer { }

    [CustomPropertyDrawer(typeof(BlueRedSynergyData))]
    internal sealed class BlueRedSynergyDataDrawer : KoreanNestedPropertyDrawer { }

    [CustomEditor(typeof(VfxLabPreviewCharacter)), CanEditMultipleObjects]
    internal sealed class VfxLabPreviewCharacterEditor : KoreanSerializedObjectEditor { }

    [CustomEditor(typeof(VfxLabPreviewSequence)), CanEditMultipleObjects]
    internal sealed class VfxLabPreviewSequenceEditor : KoreanSerializedObjectEditor { }

    [CustomEditor(typeof(VfxLabCharacterProfile)), CanEditMultipleObjects]
    internal sealed class VfxLabCharacterProfileEditor : KoreanSerializedObjectEditor { }

    [CustomEditor(typeof(CombatAudioProfile)), CanEditMultipleObjects]
    internal sealed class CombatAudioProfileEditor : KoreanSerializedObjectEditor { }

    [CustomEditor(typeof(PrototypeCombatAudio)), CanEditMultipleObjects]
    internal sealed class PrototypeCombatAudioEditor : KoreanSerializedObjectEditor { }

    [CustomEditor(typeof(TechniqueBurnoutController)), CanEditMultipleObjects]
    internal sealed class TechniqueBurnoutControllerEditor : KoreanSerializedObjectEditor { }

    [CustomEditor(typeof(BasicAttack)), CanEditMultipleObjects]
    internal sealed class BasicAttackEditor : KoreanSerializedObjectEditor { }

    [CustomEditor(typeof(CharacterStatsProfile)), CanEditMultipleObjects]
    internal sealed class CharacterStatsProfileEditor : KoreanSerializedObjectEditor { }

    [CustomEditor(typeof(CursedEnergyProfile)), CanEditMultipleObjects]
    internal sealed class CursedEnergyProfileEditor : KoreanSerializedObjectEditor { }

    [CustomEditor(typeof(BasicAttackProfile)), CanEditMultipleObjects]
    internal sealed class BasicAttackProfileEditor : KoreanSerializedObjectEditor { }

    [CustomEditor(typeof(GojoTechniqueGameplayProfile)), CanEditMultipleObjects]
    internal sealed class GojoTechniqueGameplayProfileEditor : KoreanSerializedObjectEditor { }

    [CustomEditor(typeof(DomainGameplayProfile)), CanEditMultipleObjects]
    internal sealed class DomainGameplayProfileEditor : KoreanSerializedObjectEditor { }

    [CustomEditor(typeof(BurnoutPolicyProfile)), CanEditMultipleObjects]
    internal sealed class BurnoutPolicyProfileEditor : KoreanSerializedObjectEditor { }

    [CustomEditor(typeof(TargetingProfile)), CanEditMultipleObjects]
    internal sealed class TargetingProfileEditor : KoreanSerializedObjectEditor { }

    [CustomEditor(typeof(CharacterCombatDefinition)), CanEditMultipleObjects]
    internal sealed class CharacterCombatDefinitionEditor : KoreanSerializedObjectEditor { }

    [CustomEditor(typeof(CharacterCombatCatalog)), CanEditMultipleObjects]
    internal sealed class CharacterCombatCatalogEditor : KoreanSerializedObjectEditor { }

    [CustomEditor(typeof(TrainingBotProfile)), CanEditMultipleObjects]
    internal sealed class TrainingBotProfileEditor : KoreanSerializedObjectEditor { }

    [CustomEditor(typeof(GojoPolishSettings)), CanEditMultipleObjects]
    internal sealed class GojoPolishSettingsEditor : KoreanSerializedObjectEditor { }
}
