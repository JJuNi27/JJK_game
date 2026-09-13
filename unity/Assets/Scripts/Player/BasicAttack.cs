using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    public sealed class BasicAttack : MonoBehaviour
    {
        [SerializeField, InspectorName("평타 프로필"), Tooltip("연결되면 공격 반경, 타이밍, 타수와 각 단계 수치는 이 Data Asset에서 읽습니다.")]
        private BasicAttackProfile profile;
        [Header("평타 판정")]
        [SerializeField, InspectorName("공격 판정 중심"), Tooltip("근접 공격 판정의 중심입니다.")] private Transform attackOrigin;
        [SerializeField, InspectorName("공격 반경"), Min(0.1f), Tooltip("평타가 적을 찾는 구형 판정 반경입니다.")] private float attackRadius = 1.6f;
        [SerializeField, InspectorName("콤보 초기화 대기시간"), Min(0.1f), Tooltip("다음 입력까지 콤보 단계가 유지되는 시간입니다.")] private float comboResetDelay = 0.9f;
        [SerializeField, InspectorName("콤보 표시 시간"), Min(0.1f), Tooltip("현재 공격 단계가 HUD와 애니메이션 소스에 표시되는 시간입니다.")] private float comboDisplayDuration = 0.75f;
        [SerializeField, InspectorName("적중 콤보 초기화 대기시간"), Min(0.1f), Tooltip("연속 적중 횟수가 초기화되기 전 대기시간입니다.")] private float hitComboResetDelay = 1.05f;
        [SerializeField, InspectorName("영역 컨트롤러"), Tooltip("영역 손동작 입력과 평타 입력을 구분할 영역 컨트롤러입니다.")] private GojoDomainController domainController;

        [Header("평타 연계 피해량")]
        [SerializeField, InspectorName("1타 피해량"), Min(0.1f), Tooltip("평타 1타의 기본 피해량입니다.")] private float firstHitDamage = 12f;
        [SerializeField, InspectorName("2타 피해량"), Min(0.1f), Tooltip("평타 2타의 기본 피해량입니다.")] private float secondHitDamage = 14f;
        [SerializeField, InspectorName("3타 피해량"), Min(0.1f), Tooltip("평타 마무리 타격의 기본 피해량입니다.")] private float thirdHitDamage = 24f;

        [Header("평타 연계 타이밍")]
        [SerializeField, InspectorName("1타 입력 간격"), Min(0.05f), Tooltip("평타 1타 뒤 다음 공격 입력을 받을 수 있기까지의 시간입니다.")] private float firstHitCooldown = 0.24f;
        [SerializeField, InspectorName("2타 입력 간격"), Min(0.05f), Tooltip("평타 2타 뒤 다음 공격 입력을 받을 수 있기까지의 시간입니다.")] private float secondHitCooldown = 0.28f;
        [SerializeField, InspectorName("3타 입력 간격"), Min(0.05f), Tooltip("평타 마무리 뒤 다음 공격 입력을 받을 수 있기까지의 시간입니다.")] private float thirdHitCooldown = 0.52f;

        [Header("평타 적중 반응")]
        [SerializeField, InspectorName("1타 넉백"), Min(0f), Tooltip("평타 1타 적중 시 적용할 밀어내기 속도입니다.")] private float firstHitKnockback = 4.5f;
        [SerializeField, InspectorName("2타 넉백"), Min(0f), Tooltip("평타 2타 적중 시 적용할 밀어내기 속도입니다.")] private float secondHitKnockback = 6f;
        [SerializeField, InspectorName("3타 넉백"), Min(0f), Tooltip("평타 마무리 적중 시 적용할 밀어내기 속도입니다.")] private float thirdHitKnockback = 11f;
        [SerializeField, InspectorName("1타 경직시간"), Min(0f), Tooltip("평타 1타 적중 시 대상이 경직되는 시간입니다.")] private float firstHitStun = 0.12f;
        [SerializeField, InspectorName("2타 경직시간"), Min(0f), Tooltip("평타 2타 적중 시 대상이 경직되는 시간입니다.")] private float secondHitStun = 0.17f;
        [SerializeField, InspectorName("3타 경직시간"), Min(0f), Tooltip("평타 마무리 적중 시 대상이 경직되는 시간입니다.")] private float thirdHitStun = 0.38f;

        private Health ownHealth;
        private TargetLockController targetLock;
        private CombatActionGate actionGate;
        private float nextAttackAt;
        private float chainExpiresAt;
        private float chainDisplayUntil;
        private float hitComboExpiresAt;
        private int nextChainIndex;
        private int lastPerformedStep;
        private int hitComboCount;

        public int DisplayChainStep => Time.time <= chainDisplayUntil ? lastPerformedStep : 0;
        public event System.Action<int> AttackStarted;
        public int DisplayHitComboCount =>
            hitComboCount >= 2 && Time.time <= hitComboExpiresAt ? hitComboCount : 0;

        private int StepCount => profile != null && profile.StepCount > 0 ? profile.StepCount : 3;
        private float AttackRadius => profile != null ? profile.AttackRadius : attackRadius;
        private float ComboResetDelay => profile != null ? profile.ComboResetDelay : comboResetDelay;
        private float ComboDisplayDuration => profile != null ? profile.ComboDisplayDuration : comboDisplayDuration;
        private float HitComboResetDelay => profile != null ? profile.HitComboResetDelay : hitComboResetDelay;

        public string ChainLabel
        {
            get
            {
                int step = DisplayChainStep;
                if (step <= 0) return string.Empty;
                BasicAttackStep data = profile?.GetStep(step - 1);
                string suffix = data != null && data.Finisher ? " · FINISH" : string.Empty;
                return $"ATTACK CHAIN {step} / {StepCount}{suffix}";
            }
        }

        public string HitComboLabel =>
            DisplayHitComboCount > 0 ? $"HIT COMBO × {DisplayHitComboCount}" : string.Empty;

        public void Configure(Transform newAttackOrigin, GojoDomainController newDomainController)
        {
            attackOrigin = newAttackOrigin;
            domainController = newDomainController;
        }

        public void ResetCombatSequence()
        {
            nextAttackAt = 0f;
            nextChainIndex = 0;
            chainExpiresAt = 0f;
            chainDisplayUntil = 0f;
            lastPerformedStep = 0;
            hitComboCount = 0;
            hitComboExpiresAt = 0f;
        }

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
            targetLock = GetComponent<TargetLockController>();
            actionGate = CombatActionGate.GetOrCreate(gameObject);

            if (attackOrigin == null)
            {
                attackOrigin = transform;
            }

            if (domainController == null)
            {
                domainController = GetComponent<GojoDomainController>();
            }
        }

        private void Update()
        {
            if (nextChainIndex != 0 && Time.time > chainExpiresAt)
            {
                ResetAttackChain();
            }

            if (hitComboCount > 0 && Time.time > hitComboExpiresAt)
            {
                ResetHitCombo();
            }

            if (ProductionCombatInput.BasicAttackPressed) TryAttack();
        }

        public void ApplyProfile(BasicAttackProfile nextProfile)
        {
            if (nextProfile == null || nextProfile.StepCount == 0) return;
            profile = nextProfile;
            ResetCombatSequence();
        }

        public bool TryAttack()
        {
            if (!isActiveAndEnabled || Time.time < nextAttackAt)
            {
                return false;
            }

            if (domainController != null && domainController.CapturesMouseInput)
            {
                return false;
            }

            actionGate ??= CombatActionGate.GetOrCreate(gameObject);
            if (actionGate != null && !actionGate.CanStartBasicAttack)
            {
                return false;
            }

            if (targetLock == null)
            {
                targetLock = GetComponent<TargetLockController>();
            }

            targetLock?.FaceTargetInstant();
            PerformAttackChainStep(nextChainIndex);
            return true;
        }

        private void PerformAttackChainStep(int chainIndex)
        {
            BasicAttackStep step = profile?.GetStep(chainIndex);
            float damage = step != null ? step.Damage : GetChainValue(chainIndex, firstHitDamage, secondHitDamage, thirdHitDamage);
            float cooldown = step != null ? step.Cooldown : GetChainValue(chainIndex, firstHitCooldown, secondHitCooldown, thirdHitCooldown);
            float knockback = step != null ? step.Knockback : GetChainValue(chainIndex, firstHitKnockback, secondHitKnockback, thirdHitKnockback);
            float hitStun = step != null ? step.HitStun : GetChainValue(chainIndex, firstHitStun, secondHitStun, thirdHitStun);

            nextAttackAt = Time.time + cooldown;
            lastPerformedStep = chainIndex + 1;
            chainDisplayUntil = Time.time + ComboDisplayDuration;
            AttackStarted?.Invoke(lastPerformedStep);
            CombatAudioEvents.Raise(
                CombatAudioEvent.ForOwner(
                    ownHealth,
                    CombatAudioEventId.BasicSwing,
                    lastPerformedStep
                )
            );

            bool hitAnyTarget = PerformAttack(damage, knockback, hitStun);
            if (hitAnyTarget)
            {
                RegisterSuccessfulHit();
                CombatAudioEvents.Raise(
                    CombatAudioEvent.ForOwner(
                        ownHealth,
                        CombatAudioEventId.BasicHit,
                        lastPerformedStep
                    )
                );
                BasicHitPresentationRequests.Raise(
                    new BasicHitPresentationRequest(ownHealth, lastPerformedStep)
                );
            }
            else
            {
                ResetHitCombo();
            }

            if (chainIndex >= StepCount - 1)
            {
                nextChainIndex = 0;
                chainExpiresAt = 0f;
            }
            else
            {
                nextChainIndex = chainIndex + 1;
                chainExpiresAt = Time.time + ComboResetDelay;
            }
        }

        private bool PerformAttack(float damage, float knockbackSpeed, float hitStunDuration)
        {
            Collider[] hits = Physics.OverlapSphere(attackOrigin.position, AttackRadius);
            HashSet<Health> damagedTargets = new HashSet<Health>();
            bool hitAnyTarget = false;

            foreach (Collider hit in hits)
            {
                Health targetHealth = hit.GetComponentInParent<Health>();
                if (
                    targetHealth == null
                    || targetHealth == ownHealth
                    || targetHealth.IsDead
                    || !damagedTargets.Add(targetHealth)
                )
                {
                    continue;
                }

                DamageContext context = new DamageContext(
                    damage,
                    gameObject,
                    DamageDeliveryType.PhysicalStrike,
                    DamageTraits.None,
                    $"BASIC ATTACK {lastPerformedStep}",
                    targetHealth.transform.position + Vector3.up * 0.8f
                );
                if (targetHealth.ReceiveDamage(context) != DamageResolution.Applied)
                {
                    continue;
                }

                PrototypeHitImpactVfx.Spawn(context.HitPoint, lastPerformedStep);
                hitAnyTarget = true;
                ApplyHitReaction(targetHealth, knockbackSpeed, hitStunDuration);
            }

            return hitAnyTarget;
        }

        private void RegisterSuccessfulHit()
        {
            if (Time.time > hitComboExpiresAt)
            {
                hitComboCount = 0;
            }

            hitComboCount += 1;
            hitComboExpiresAt = Time.time + HitComboResetDelay;
        }

        private void ApplyHitReaction(Health targetHealth, float knockbackSpeed, float hitStunDuration)
        {
            Vector3 direction = targetHealth.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = transform.forward;
            }

            Vector3 impulse = direction.normalized * knockbackSpeed;
            MonoBehaviour[] behaviours = targetHealth.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IHitReactable hitReactable)
                {
                    hitReactable.ApplyHitReaction(impulse, hitStunDuration);
                    break;
                }
            }
        }

        private void ResetAttackChain()
        {
            nextChainIndex = 0;
            chainExpiresAt = 0f;
        }

        private void ResetHitCombo()
        {
            hitComboCount = 0;
            hitComboExpiresAt = 0f;
        }

        private static float GetChainValue(int chainIndex, float first, float second, float third)
        {
            return chainIndex switch
            {
                1 => second,
                2 => third,
                _ => first,
            };
        }
    }
}
