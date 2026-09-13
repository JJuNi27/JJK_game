using UnityEngine;

namespace JJKGame.Player
{
    [CreateAssetMenu(menuName = "JJK Game/Combat/Domain Gameplay Profile", fileName = "Domain Gameplay Profile")]
    public sealed class DomainGameplayProfile : ScriptableObject
    {
        [Header("영역 입력 타이밍")]
        [SerializeField, InspectorName("준비 입력 제한시간"), Min(0.1f)] private float readyTimeout = 3f;
        [SerializeField, InspectorName("우클릭→좌클릭 제한시간"), Min(0.05f)] private float rightToLeftTimeout = 0.65f;
        [SerializeField, InspectorName("목표 해제 시점"), Min(0.05f)] private float targetReleaseTime = 0.90f;
        [SerializeField, InspectorName("해제 허용 오차"), Min(0.01f)] private float releaseTolerance = 0.22f;
        [SerializeField, InspectorName("실패 표시시간"), Min(0.1f)] private float failedDuration = 1.2f;
        [Header("영역 게임플레이")]
        [SerializeField, InspectorName("영역 유지시간"), Min(0.1f)] private float activeDuration = 10f;
        [SerializeField, InspectorName("대상 경직시간"), Min(0.1f)] private float victimStunDuration = 16f;
        [SerializeField, InspectorName("게임플레이 포획 반경"), Min(0.1f)] private float captureRadius = 30f;
        [SerializeField, InspectorName("영역 주력 소모량"), Min(0f)] private float energyCost = 60f;
        public float ReadyTimeout => readyTimeout; public float RightToLeftTimeout => rightToLeftTimeout;
        public float TargetReleaseTime => targetReleaseTime; public float ReleaseTolerance => releaseTolerance;
        public float FailedDuration => failedDuration; public float ActiveDuration => activeDuration;
        public float VictimStunDuration => victimStunDuration; public float CaptureRadius => captureRadius;
        public float EnergyCost => energyCost;
    }
}
