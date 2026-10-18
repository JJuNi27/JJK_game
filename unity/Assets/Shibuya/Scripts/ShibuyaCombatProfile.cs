using UnityEngine;

namespace JJKGame.Shibuya
{
    public enum ShibuyaKnockbackDirection {
        [InspectorName("공격자에게서 멀어지는 방향")] AwayFromAttacker,
        [InspectorName("공격자가 바라보는 전방")] AttackerForward
    }
    public enum ShibuyaStrikeMotion {
        [InspectorName("오른 주먹 직선타")] RightJab, [InspectorName("왼 주먹 직선타")] LeftJab,
        [InspectorName("오른 주먹 회전타")] RightHook, [InspectorName("왼 주먹 마무리 강타")] HeavyCross
    }

    [System.Serializable]
    public sealed class ShibuyaStrikeDefinition
    {
        [Min(0.1f)] public float damage = 20f;
        [Min(0.01f)] public float startup = 0.10f;
        [Min(0.01f)] public float activeTime = 0.08f;
        [Min(0.01f)] public float recovery = 0.22f;
        [Min(0.1f)] public float attackRadius = 0.85f;
        [Min(0.1f)] public float attackForwardOffset = 1.1f;
        [Min(0f)] public float attackHeight = 1f;
        [Min(0f)] public float hitStun = 0.25f;
        [Min(0f)] public float knockbackSpeed = 6f;
        public ShibuyaKnockbackDirection knockbackDirection = ShibuyaKnockbackDirection.AttackerForward;
        public bool alignVictimOnHit = true;
        public ShibuyaStrikeMotion motion;
        [Range(0.25f, 2f)] public float presentationIntensity = 1f;
        [Min(0f)] public float advanceDistance = 0.70f;
        [Min(0f)] public float advanceSpeed = 5f;
        [Min(0f)] public float advanceStartTime = 0.02f;
        [Min(0f)] public float advanceDuration = 0.18f;
        [Tooltip("추가 지연이 아니라 기존 공격 준비 시간 안에서 힘을 모으는 구간입니다.")]
        [Min(0f)] public float powerWindupTime;
        [Range(0f, 1f)] public float windupMoveMultiplier = 0.2f;
        [Min(0f)] public float hitStopDuration;
        [Range(0.5f, 1.5f)] public float soundPitch = 1.05f;
        public float PowerWindup => Mathf.Clamp(powerWindupTime, 0f, Startup);
        public float Startup => Mathf.Max(0.01f, startup);
        public float ActiveEnd => Startup + Mathf.Max(0.01f, activeTime);
        public float Duration => ActiveEnd + Mathf.Max(0.01f, recovery);
    }

    [CreateAssetMenu(menuName = "JJK Game/시부야/기본 전투 설정", fileName = "Shibuya Combat Profile")]
    public sealed class ShibuyaCombatProfile : ScriptableObject
    {
        [Header("이동")]
        [Min(0.1f)] public float walkSpeed = 4.8f;
        [Min(0.1f)] public float runSpeed = 8.5f;
        [Min(0.1f)] public float turnSpeed = 720f;
        [Min(0.1f)] public float jumpHeight = 1.4f;
        [Min(0.1f)] public float gravity = 25f;
        [Range(0f, 1f)] public float attackMoveMultiplier = 0.25f;
        [Range(0f, 1f)] public float attackLateralInputMultiplier;
        [Range(0f, 1f)] public float attackBackwardInputMultiplier;
        [Header("지상 4타 콤보")]
        public ShibuyaStrikeDefinition[] strikes = {
            new ShibuyaStrikeDefinition(),
            new ShibuyaStrikeDefinition { startup = 0.09f, activeTime = 0.07f, recovery = 0.18f,
                knockbackSpeed = 4f, motion = ShibuyaStrikeMotion.LeftJab,
                advanceDistance = 0.55f, advanceSpeed = 4f, advanceDuration = 0.16f },
            new ShibuyaStrikeDefinition { startup = 0.10f, activeTime = 0.07f, recovery = 0.18f,
                knockbackSpeed = 4f, motion = ShibuyaStrikeMotion.RightHook, presentationIntensity = 1.1f,
                advanceDistance = 0.55f, advanceSpeed = 4f, advanceDuration = 0.16f },
            new ShibuyaStrikeDefinition { damage = 25f, startup = 0.18f, activeTime = 0.10f, recovery = 0.32f,
                knockbackSpeed = 11f, hitStun = 0.45f, motion = ShibuyaStrikeMotion.HeavyCross,
                knockbackDirection = ShibuyaKnockbackDirection.AttackerForward, presentationIntensity = 1.5f,
                advanceDistance = 0.55f, advanceStartTime = 0.10f, advanceDuration = 0.16f,
                powerWindupTime = 0.10f, hitStopDuration = 0.065f, soundPitch = 0.72f, alignVictimOnHit = false }
        };
        [Min(0.01f)] public float inputBufferTime = 0.16f;
        [Min(0.01f)] public float comboResetTime = 0.60f;
        [Header("예약된 유효 연계만 확정")]
        public bool confirmedLinkEnabled = true;
        [Min(0.01f)] public float confirmedLinkInputWindow = 0.24f;
        [Min(0.01f)] public float confirmedLinkMaxHold = 0.75f;
        [Min(0.01f)] public float confirmedLinkMaxSequence = 3f;
        [Min(0.1f)] public float knockbackDamping = 22f;
        [Header("직전 적중 대상 준비 구간 보정")]
        public bool trackingEnabled = true;
        [Min(0f)] public float trackingTargetRange = 2.7f;
        [Range(0f, 90f)] public float trackingConeAngle = 35f;
        [Range(0f, 90f)] public float trackingMaxTurnAngle = 18f;
        [Min(0f)] public float trackingTurnSpeed = 180f;
        [Min(0f)] public float trackingAdvanceDistance = 0.16f;
        [Min(0f)] public float trackingAdvanceSpeed = 3f;
        [Min(0f)] public float trackingDuration = 0.07f;
        [Range(0f, 180f)] public float directionalInputOverrideAngle = 60f;
        [Header("유효 적중 후 피격자 측면 정렬")]
        public bool victimAlignmentEnabled = true;
        [Min(0f)] public float victimAlignmentMaxLateralOffset = 0.65f;
        [Min(0f)] public float victimAlignmentMaxDistance = 0.20f;
        [Min(0f)] public float victimAlignmentSpeed = 2f;
        [Min(0f)] public float victimAlignmentDuration = 0.10f;
        [Header("카메라")]
        [Min(0.5f)] public float cameraDistance = 6.5f;
        [Min(0f)] public float cameraHeight = 1.25f;
        [Min(0.1f)] public float mouseSensitivity = 3f;
        [Range(-80f, 0f)] public float minimumPitch = -25f;
        [Range(1f, 85f)] public float maximumPitch = 70f;
        [Range(0.05f, 0.5f)] public float cameraCollisionRadius = 0.2f;
        [Min(0.1f)] public float cameraFollowSharpness = 16f;
        [Header("기본 피격 표현")]
        public bool hitReactionEnabled = true;
        [Min(0f)] public float frontHitReactionStrength = 1f;
        [Min(0f)] public float leftHitReactionStrength = 1f;
        [Min(0f)] public float rightHitReactionStrength = 1f;
        [Min(0f)] public float backHitReactionStrength = 1f;
        [Range(0f, 45f)] public float hitReactionLeanAngle = 18f;
        [Range(0f, 45f)] public float hitReactionTwistAngle = 20f;
        [Min(0f)] public float hitReactionStartTime = 0.035f;
        [Tooltip("피격 경직 안에서 복귀하도록 제한됩니다. 히트스톱 동안에는 로컬 모션 시간도 멈춥니다.")]
        [Min(0.01f)] public float hitReactionRecoveryTime = 0.18f;
        [Range(0f, 3f)] public float normalHitReactionMultiplier = 1f;
        [Range(0f, 3f)] public float heavyHitReactionMultiplier = 1.8f;
        [Min(0.01f)] public float hitFlashDuration = 0.09f;
        [Range(0f, 1f)] public float hitSoundVolume = 0.35f;

        public bool HasValidCombo => strikes != null && strikes.Length == 4
            && System.Array.TrueForAll(strikes, strike => strike != null);
    }
}
