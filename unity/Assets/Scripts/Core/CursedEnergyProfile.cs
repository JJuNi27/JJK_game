using UnityEngine;

namespace JJKGame.Core
{
    [CreateAssetMenu(menuName = "JJK Game/Combat/Cursed Energy Profile", fileName = "Cursed Energy Profile")]
    public sealed class CursedEnergyProfile : ScriptableObject
    {
        [Header("주력 보유량")]
        [SerializeField, InspectorName("표시 이름"), Tooltip("HUD에 표시할 주력 프로필 이름입니다.")] private string profileLabel = "STANDARD";
        [SerializeField, InspectorName("최대 주력"), Min(1f)] private float maxEnergy = 100f;
        [SerializeField, InspectorName("시작 주력"), Min(0f)] private float startingEnergy = 100f;
        [Header("주력 회복")]
        [SerializeField, InspectorName("초당 회복량"), Min(0f)] private float regenerationPerSecond = 12f;
        [SerializeField, InspectorName("사용 후 회복 지연"), Min(0f)] private float regenerationDelayAfterSpend = 0.8f;
        [Header("술식 소모")]
        [SerializeField, InspectorName("소모 배율"), Min(0f)] private float costMultiplier = 1f;
        [SerializeField, InspectorName("최소 술식 소모량"), Min(0f)] private float minimumTechniqueCost;
        [SerializeField, InspectorName("알림 표시시간"), Min(0.1f)] private float noticeDuration = 1.1f;

        public string ProfileLabel => string.IsNullOrWhiteSpace(profileLabel) ? name : profileLabel;
        public float MaxEnergy => Mathf.Max(1f, maxEnergy);
        public float StartingEnergy => Mathf.Clamp(startingEnergy, 0f, MaxEnergy);
        public float RegenerationPerSecond => Mathf.Max(0f, regenerationPerSecond);
        public float RegenerationDelayAfterSpend => Mathf.Max(0f, regenerationDelayAfterSpend);
        public float CostMultiplier => Mathf.Max(0f, costMultiplier);
        public float MinimumTechniqueCost => Mathf.Max(0f, minimumTechniqueCost);
        public float NoticeDuration => Mathf.Max(0.1f, noticeDuration);
    }
}
