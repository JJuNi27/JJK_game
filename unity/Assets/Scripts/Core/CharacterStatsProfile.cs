using UnityEngine;

namespace JJKGame.Core
{
    [CreateAssetMenu(menuName = "JJK Game/Combat/Character Stats Profile", fileName = "Character Stats Profile")]
    public sealed class CharacterStatsProfile : ScriptableObject
    {
        [Header("기본 전투 능력치")]
        [SerializeField, InspectorName("최대 체력"), Min(1f), Tooltip("캐릭터가 가질 수 있는 최대 체력입니다.")]
        private float maxHealth = 100f;
        [SerializeField, InspectorName("기본 공격력 배율"), Min(0f), Tooltip("향후 공용 피해 계산에서 사용할 기본 공격력 배율입니다. 현재 평타 원본 값에는 곱하지 않습니다.")]
        private float baseAttackMultiplier = 1f;
        [SerializeField, InspectorName("방어력 배율"), Min(0f), Tooltip("향후 공용 방어 계산을 위한 확장 값입니다. 현재 피해 계산에는 적용하지 않습니다.")]
        private float defenseMultiplier = 1f;

        public float MaxHealth => Mathf.Max(1f, maxHealth);
        public float BaseAttackMultiplier => Mathf.Max(0f, baseAttackMultiplier);
        public float DefenseMultiplier => Mathf.Max(0f, defenseMultiplier);
    }
}
