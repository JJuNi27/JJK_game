using UnityEngine;

namespace JJKGame.Player
{
    [CreateAssetMenu(menuName = "JJK Game/Combat/Targeting Profile", fileName = "Targeting Profile")]
    public sealed class TargetingProfile : ScriptableObject
    {
        [Header("타겟팅 게임플레이")]
        [SerializeField, InspectorName("최대 락온 거리"), Min(1f)] private float maxLockDistance = 30f;
        public float MaxLockDistance => Mathf.Max(1f, maxLockDistance);
    }
}
