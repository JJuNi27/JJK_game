using UnityEngine;

namespace JJKGame.Core
{
    [CreateAssetMenu(menuName = "JJK Game/Combat/Burnout Policy Profile", fileName = "Burnout Policy Profile")]
    public sealed class BurnoutPolicyProfile : ScriptableObject
    {
        [SerializeField, InspectorName("영역 종료 후 발생"), Tooltip("영역 자연 종료 뒤 술식 번아웃을 시작합니다.")] private bool occursAfterDomain = true;
        [SerializeField, InspectorName("번아웃 지속시간"), Min(0.1f)] private float duration = 5f;
        [SerializeField, InspectorName("조기 복구 허용"), Tooltip("캐릭터 Trait/Passive가 허용할 때 조기 복구 시도를 받을 수 있습니다.")] private bool allowEarlyRecovery = true;
        public bool OccursAfterDomain => occursAfterDomain;
        public float Duration => Mathf.Max(0.1f, duration);
        public bool AllowEarlyRecovery => allowEarlyRecovery;
    }
}
