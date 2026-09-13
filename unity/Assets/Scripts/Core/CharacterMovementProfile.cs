using System;
using UnityEngine;

namespace JJKGame.Core
{
    [Serializable]
    public sealed class CharacterMovementProfile
    {
        [InspectorName("걷기 속도"), Tooltip("달리기 키를 누르지 않았을 때의 이동 속도입니다."), Min(0.1f)]
        public float walkSpeed = 4f;
        [InspectorName("달리기 속도"), Tooltip("달리기 키를 누른 상태의 이동 속도입니다."), Min(0.1f)]
        public float runSpeed = 14f;
        [InspectorName("회피 설정"), Tooltip("회피의 속도, 시간, 무적 구간과 연출 연결을 설정합니다.")]
        public EvadeProfile evade = EvadeProfile.CreateGojo();
    }

    /// <summary>Character data only. Curve X is normalized movement time, Y is speed / burstSpeed.</summary>
    [Serializable]
    public sealed class EvadeProfile
    {
        [InspectorName("순간 속도"), Tooltip("회피 시작 시 적용되는 최대 이동 속도입니다."), Min(0f)] public float burstSpeed = 12f;
        [InspectorName("이동 시간"), Tooltip("회피 이동이 진행되는 시간입니다."), Min(0.01f)] public float movementDuration = 0.24f;
        [InspectorName("속도 곡선"), Tooltip("X는 정규화된 이동 시간, Y는 순간 속도 배율입니다.")]
        public AnimationCurve speedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        [InspectorName("회복 시간"), Tooltip("회피 이동이 끝난 뒤 행동이 회복되기까지의 시간입니다."), Min(0f)] public float recoveryDuration;
        [InspectorName("재사용 대기시간"), Tooltip("다음 회피를 사용할 수 있을 때까지의 시간입니다."), Min(0f)] public float cooldown = 0.75f;
        [InspectorName("무적 시작 시점"), Tooltip("회피 시작 후 무적이 적용되기 시작하는 시점입니다."), Min(0f)] public float invulnerabilityStart;
        [InspectorName("무적 지속시간"), Tooltip("회피 중 무적 상태가 유지되는 시간입니다."), Min(0f)] public float invulnerabilityDuration = 0.30f;
        [InspectorName("연출 스타일 식별자"), Tooltip("연출 메타데이터 전용이며 이동이나 무적 판정을 제어하지 않습니다.")]
        public string styleId = "evasive-step";
        [InspectorName("애니메이션 트리거"), Tooltip("회피 애니메이션을 재생할 트리거 파라미터 이름입니다.")]
        public string animationTrigger = "Dodge";
        [InspectorName("애니메이션 상태"), Tooltip("직접 재생할 선택적 애니메이션 상태 이름입니다.")]
        public string animationState = "";
        [InspectorName("시각 효과 큐"), Tooltip("회피 연출에 사용할 선택적 시각 효과 큐 이름입니다.")]
        public string vfxCue = "";
        [InspectorName("효과음 큐"), Tooltip("회피 연출에 사용할 선택적 효과음 큐 이름입니다.")]
        public string sfxCue = "Dodge";

        public float MovementDuration => Mathf.Max(0.01f, movementDuration);
        public float ActionDuration => MovementDuration + Mathf.Max(0f, recoveryDuration);

        public static EvadeProfile CreateGojo()
        {
            return new EvadeProfile
            {
                burstSpeed = 32f,
                movementDuration = 0.20f,
                recoveryDuration = 0.04f,
                // Piecewise linear: 32 at 0, 28 at .06, 14 at .14, zero at .20 seconds.
                speedCurve = new AnimationCurve(
                    new Keyframe(0f, 1f, 0f, -0.41666667f),
                    new Keyframe(0.3f, 0.875f, -0.41666667f, -1.09375f),
                    new Keyframe(0.7f, 0.4375f, -1.09375f, -1.45833333f),
                    new Keyframe(1f, 0f, -1.45833333f, 0f)),
                styleId = "gojo-blue-burst",
                vfxCue = "gojo.evade.spatial-streak",
            };
        }
    }
}
