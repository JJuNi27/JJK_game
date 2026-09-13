using System;
using UnityEngine;

namespace JJKGame.Core
{
    [Serializable]
    public sealed class BasicAttackStep
    {
        [SerializeField, InspectorName("피해량"), Min(0.1f)] private float damage = 12f;
        [SerializeField, InspectorName("입력 간격"), Min(0.05f)] private float cooldown = 0.24f;
        [SerializeField, InspectorName("넉백 강도"), Min(0f)] private float knockback = 4.5f;
        [SerializeField, InspectorName("경직 시간"), Min(0f)] private float hitStun = 0.12f;
        [SerializeField, InspectorName("마무리 타격"), Tooltip("연출 시스템에서 마무리 타격으로 구분할지 결정합니다.")] private bool finisher;
        [SerializeField, InspectorName("연출 태그"), Tooltip("캐릭터별 애니메이션/VFX 매핑에 사용할 선택적 태그입니다.")] private string presentationTag;

        public float Damage => Mathf.Max(0.1f, damage);
        public float Cooldown => Mathf.Max(0.05f, cooldown);
        public float Knockback => Mathf.Max(0f, knockback);
        public float HitStun => Mathf.Max(0f, hitStun);
        public bool Finisher => finisher;
        public string PresentationTag => presentationTag;
    }

    [CreateAssetMenu(menuName = "JJK Game/Combat/Basic Attack Profile", fileName = "Basic Attack Profile")]
    public sealed class BasicAttackProfile : ScriptableObject
    {
        [Header("평타 판정")]
        [SerializeField, InspectorName("공격 반경"), Min(0.1f)] private float attackRadius = 1.6f;
        [SerializeField, InspectorName("콤보 입력 유지시간"), Min(0.1f)] private float comboResetDelay = 0.9f;
        [SerializeField, InspectorName("콤보 표시시간"), Min(0.1f)] private float comboDisplayDuration = 0.75f;
        [SerializeField, InspectorName("적중 콤보 유지시간"), Min(0.1f)] private float hitComboResetDelay = 1.05f;
        [Header("평타 연계")]
        [SerializeField, InspectorName("공격 단계"), Tooltip("배열 길이가 실제 평타 타수입니다. 3타 고정이 아닙니다.")]
        private BasicAttackStep[] steps = Array.Empty<BasicAttackStep>();

        public float AttackRadius => Mathf.Max(0.1f, attackRadius);
        public float ComboResetDelay => Mathf.Max(0.1f, comboResetDelay);
        public float ComboDisplayDuration => Mathf.Max(0.1f, comboDisplayDuration);
        public float HitComboResetDelay => Mathf.Max(0.1f, hitComboResetDelay);
        public int StepCount => steps?.Length ?? 0;
        public BasicAttackStep GetStep(int index) => StepCount > 0 ? steps[Mathf.Clamp(index, 0, StepCount - 1)] : null;
    }
}
