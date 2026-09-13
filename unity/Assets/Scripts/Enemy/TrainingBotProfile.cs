using UnityEngine;

namespace JJKGame.Enemy
{
    [CreateAssetMenu(menuName = "JJK Game/Combat/Training Bot Profile", fileName = "Training Bot Profile")]
    public sealed class TrainingBotProfile : ScriptableObject
    {
        [Header("훈련 봇 이동")]
        [SerializeField, InspectorName("이동 속도"), Min(0.1f)] private float moveSpeed = 3.2f;
        [SerializeField, InspectorName("회전 속도"), Min(0.1f)] private float rotationSpeed = 10f;
        [SerializeField, InspectorName("중력")] private float gravity = -24f;
        [SerializeField, InspectorName("교전 반경"), Min(0.2f)] private float engagementRadius = 1.15f;
        [SerializeField, InspectorName("교전 위치 허용 오차"), Min(0.05f)] private float engagementSlotTolerance = 0.42f;
        [Header("훈련 봇 공격")]
        [SerializeField, InspectorName("공격 모드")] private TrainingAttackMode attackMode = TrainingAttackMode.NormalStrike;
        [SerializeField, InspectorName("공격 사거리"), Min(0.1f)] private float attackRange = 1.7f;
        [SerializeField, InspectorName("공격 피해량"), Min(0.1f)] private float attackDamage = 12f;
        [SerializeField, InspectorName("공격 재사용 대기시간"), Min(0.05f)] private float attackCooldown = 0.9f;
        [SerializeField, InspectorName("공격 준비시간"), Min(0.05f)] private float attackWindupDuration = 0.55f;
        [SerializeField, InspectorName("공격 도달 여유거리"), Min(0f)] private float attackReachBuffer = 0.35f;
        [SerializeField, InspectorName("넉백 감쇠"), Min(0.1f)] private float knockbackDamping = 18f;
        public float MoveSpeed => moveSpeed; public float RotationSpeed => rotationSpeed; public float Gravity => gravity;
        public float EngagementRadius => engagementRadius; public float EngagementSlotTolerance => engagementSlotTolerance;
        public TrainingAttackMode AttackMode => attackMode; public float AttackRange => attackRange; public float AttackDamage => attackDamage;
        public float AttackCooldown => attackCooldown; public float AttackWindupDuration => attackWindupDuration;
        public float AttackReachBuffer => attackReachBuffer; public float KnockbackDamping => knockbackDamping;
    }
}
