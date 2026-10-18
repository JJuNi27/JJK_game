using System;
using UnityEngine;

namespace JJKGame.Shibuya
{
    public enum ShibuyaFinisher {
        [InspectorName("기본 수평 마무리")] Default,
        [InspectorName("어퍼컷")] Uppercut,
        [InspectorName("다운 마무리")] Down
    }
    public enum ShibuyaHitOutcome {
        [InspectorName("일반 피격")] Normal,
        [InspectorName("공중으로 띄우기")] Launch,
        [InspectorName("넘어뜨리기")] Knockdown
    }
    [Serializable]
    public sealed class ShibuyaAirSettings {
        [Min(.1f)] public float launchSpeed = 11f;
        [Min(.1f)] public float airGravity = 25f;
        [Min(0)] public float apexHoldTime = .06f;
        [Range(0, 60)] public float airReactionLean = 28f;
        public bool usePeakHeight;
        [Min(.1f)] public float peakHeight = 5.5f;
        [Min(0)] public float launchHorizontalDistance = 2.4f;
        [Min(.01f)] public float launchHorizontalDuration = .55f;
        [Min(0)] public float descendingHorizontalSpeed = 1.2f;
        [Min(0)] public float landingSeparationSpeed = 8f;
    }
    [Serializable]
    public sealed class ShibuyaDownSettings {
        [Min(.1f)] public float fallSpeed = 12f;
        [Min(0)] public float minimumDownTime = .45f;
        [Min(0)] public float naturalRecoveryTime = .85f;
        [Min(.01f)] public float getUpDuration = .22f;
        [Range(45, 90)] public float downPoseAngle = 85f;
        public bool groundCrackEnabled = true;
        [Min(.1f)] public float crackRadius = 1.8f;
        [Min(0)] public float crackHoldTime = .65f;
        [Min(.01f)] public float crackFadeTime = .65f;
        [Range(0, 1)] public float crackDarkness = .85f;
    }

    // Only alternative final steps. Normal strikes remain owned by their existing profile/moveset.
    [CreateAssetMenu(menuName = "JJK/시부야/방향별 마무리 설정")]
    public sealed class ShibuyaFinisherSet : ScriptableObject
    {
        public ShibuyaMovesetStep uppercut = new ShibuyaMovesetStep();
        public ShibuyaMovesetStep downFinisher = new ShibuyaMovesetStep();
        public bool IsValid => ShibuyaMovesetDefinition.ValidStep(uppercut) && ShibuyaMovesetDefinition.ValidStep(downFinisher);
    }
}
