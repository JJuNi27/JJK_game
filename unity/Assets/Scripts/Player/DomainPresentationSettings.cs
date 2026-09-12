using UnityEngine;

namespace JJKGame.Player
{
    /// <summary>Presentation distances never participate in the gameplay overlap query.</summary>
    [System.Serializable]
    public sealed class DomainPresentationSettings
    {
        [Header("외부 결계 (연출 전용)")]
        [InspectorName("결계 시각 지름 (미터)"), Tooltip("게임플레이 포획 반경과 독립적인 검은 결계의 시각 지름입니다."), Min(0.1f)]
        public float barrierVisualDiameterMeters = 10f;
        [InspectorName("포획 대상에 결계 맞춤"), Tooltip("포획된 참가자가 결계 안에 보이도록 시각 지름을 확장합니다.")]
        public bool fitBarrierToCapturedParticipants = true;
        [InspectorName("결계 닫힘 시간"), Min(0.01f)] public float barrierCloseDuration = 0.55f;
        [InspectorName("결계 불투명도"), Range(0f, 1f)] public float barrierOpacity = 1f;
        [InspectorName("암전 전환 시간"), Min(0.02f)] public float blackTransitionDuration = 0.20f;
        [InspectorName("영역 해제 시간"), Min(.2f)] public float barrierReleaseDuration = 1.15f;
        [Header("독립 내부 공간")]
        [InspectorName("내부 공간 원점")]
        public Vector3 interiorOrigin = new Vector3(0f, 2048f, 0f);
        [InspectorName("내부 공간 반경"), Min(60f)] public float interiorRadius = 90f;
        [InspectorName("내부 밝기"), Range(0.1f, 3f)] public float interiorBrightness = 1.35f;
        [InspectorName("화이트 블러드 밀도"), Range(18, 72)] public int whiteBloodDensity = 32;
        [InspectorName("화이트 블러드 크기"), Min(0.1f)] public float whiteBloodScale = 1.4f;
        [InspectorName("성운 강도"), Range(0f, 2f)] public float nebulaIntensity = 1.15f;
        [InspectorName("화이트 블러드 깊이"), Range(0.2f, 1f)] public float whiteBloodDepth = 0.85f;
        [InspectorName("화이트 블러드 수직 분산"), Range(.3f, 1.5f)] public float whiteBloodVerticalSpread = 1f;
        [InspectorName("시각 바닥 숨김")]
        public bool hideVisualFloor = true;
        [InspectorName("중앙 초점 크기"), Range(.5f, 6f)] public float centralFocalScale = 3.5f;
        [InspectorName("중앙 초점 강도"), Range(.1f, 3f)] public float centralFocalIntensity = 1.25f;
        [Header("보라색 터널 이후 도착 연출")]
        [InspectorName("영역 읽기 시간"), Min(.1f)] public float voidReadDuration = .6f;
        [InspectorName("구름 비트 간격"), Min(.1f)] public float cloudBeatInterval = .4f;
        [InspectorName("구름 개화 시간"), Min(.1f)] public float cloudBloomDuration = .7f;
        [Header("화이트 블러드 박자 · 구름 연출 시작 이후 초")]
        [InspectorName("1그룹 타격 시각"), Tooltip("첫 그룹의 단일 타격 시각")][Min(0f)] public float Group1Time = 0f;
        [InspectorName("2그룹 첫 타격 시각"), Tooltip("두 번째 그룹 첫 타격 시각")][Min(0f)] public float Group2Time = .8f;
        [InspectorName("2그룹 연타 간격"), Tooltip("두 번째 그룹의 투둥 사이 간격")][Min(.02f)] public float Group2SubBeatGap = .16f;
        [InspectorName("3그룹 타격 시각"), Tooltip("세 번째 그룹 단일 타격 시각")][Min(0f)] public float Group3Time = 1.8f;
        public float WhiteBloodEnd => Mathf.Max(Group1Time, Mathf.Max(Group2Time + Group2SubBeatGap, Group3Time)) + cloudBloomDuration;
        [Header("시네마틱 (영역 유지 전, 스케일 시간)")]
        [InspectorName("시네마틱 사용")]
        public bool cinematicEnabled = true;
        [InspectorName("손 디테일 샷 시간"), Min(0.01f)] public float handShotDuration = 0.55f;
        [InspectorName("얼굴 샷 시간"), Min(0.01f)] public float faceShotDuration = 0.65f;
        [InspectorName("눈 샷 시간"), Min(0.01f)] public float eyeShotDuration = 0.50f;
        [InspectorName("히어로 샷 시간"), Min(0.01f)] public float heroShotDuration = 0.80f;
        [InspectorName("피격 대상 샷 시간"), Min(0.01f)] public float victimShotDuration = 1.20f;
        [InspectorName("카메라 복귀 시간"), Min(0.01f)] public float cameraReturnDuration = 0.65f;
        [InspectorName("손 기준 오프셋")]
        public Vector3 handOffset = new Vector3(0.28f, 1.25f, 0.55f);
        [InspectorName("얼굴 기준 오프셋")]
        public Vector3 faceOffset = new Vector3(0f, 1.65f, 0.05f);
        [InspectorName("디테일 카메라 오프셋")]
        public Vector3 detailCameraOffset = new Vector3(0.35f, 0.12f, 0.65f);
        [InspectorName("히어로 카메라 오프셋")]
        public Vector3 heroCameraOffset = new Vector3(0.4f, 1.6f, 3.1f);
        [InspectorName("디테일 샷 화각"), Range(15f, 70f)] public float detailFov = 32f;
        [InspectorName("눈 샷 화각"), Range(10f, 50f)] public float eyeFov = 20f;
        [InspectorName("와이드 샷 화각"), Range(30f, 85f)] public float wideFov = 62f;
        [Header("선택적 제작 애니메이션 / 음성 훅 (연출 전용)")]
        [InspectorName("손 앵커")]
        public Transform handAnchor;
        [InspectorName("얼굴 앵커")]
        public Transform faceAnchor;
        [InspectorName("눈 앵커")]
        public Transform eyeAnchor;
        [InspectorName("샷 변경 이벤트")]
        public UnityEngine.Events.UnityEvent<string> onShot = new UnityEngine.Events.UnityEvent<string>();
        public float DetailDuration(bool victim) => !cinematicEnabled ? 0f :
            victim ? handShotDuration + faceShotDuration + eyeShotDuration + heroShotDuration
                : handShotDuration + heroShotDuration;
        public float ArrivalDuration(bool victim) => DetailDuration(victim) + barrierCloseDuration
            + blackTransitionDuration + .72f + voidReadDuration + Mathf.Max(cloudBeatInterval * 2f + cloudBloomDuration, WhiteBloodEnd);
    }
}
