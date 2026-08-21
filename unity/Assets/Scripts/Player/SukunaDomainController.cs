using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class SukunaDomainController : MonoBehaviour
    {
        public enum DomainState
        {
            Normal,
            Casting,
            Active,
        }

        [Header("V · 복마어주자 · Milestone 3")]
        [SerializeField, Min(0f)] private float domainEnergyCost = 80f;
        [SerializeField, Min(0.01f)] private float domainCastTime = 0.65f;
        [SerializeField, Min(0.1f)] private float domainDuration = 3.2f;
        [SerializeField, Min(0.1f)] private float domainRadius = 30f;
        [SerializeField, Min(1)] private int sureHitCount = 8;
        [SerializeField, Min(0f)] private float damagePerHit = 12f;
        [SerializeField, Min(0.03f)] private float sureHitInterval = 0.38f;
        [SerializeField, Min(0f)] private float sureHitStun = 0.10f;
        [SerializeField, Min(0.1f)] private float domainCooldown = 18f;

        private Health ownHealth;
        private CursedEnergyController cursedEnergy;
        private CombatActionGate actionGate;
        private PrototypeCombatAudio combatAudio;
        private SukunaCombatAudio sukunaAudio;
        private SukunaTechniqueController sukunaTechnique;
        private SukunaMalevolentShrineVisual domainVisual;
        private float castCompletesAt;
        private float activeEndsAt;
        private float nextSureHitAt;
        private float nextDomainAt;
        private int sureHitsApplied;
        private Vector3 domainCenter;

        public DomainState State { get; private set; } = DomainState.Normal;
        public bool IsCasting => State == DomainState.Casting;
        public bool IsActive => State == DomainState.Active;
        public float CastProgress => !IsCasting
            ? 0f
            : Mathf.Clamp01(1f - (castCompletesAt - Time.time) / Mathf.Max(0.01f, domainCastTime));
        public float ActiveRemaining => IsActive ? Mathf.Max(0f, activeEndsAt - Time.time) : 0f;
        public float CooldownRemaining => Mathf.Max(0f, nextDomainAt - Time.time);
        public float DomainEnergyCost => cursedEnergy != null
            ? cursedEnergy.ResolveCost(domainEnergyCost)
            : domainEnergyCost;
        public int SureHitsApplied => sureHitsApplied;
        public int SureHitCount => sureHitCount;
        public Vector3 DomainCenter => domainCenter;
        public float DomainRadius => domainRadius;

        public string StatusText
        {
            get
            {
                if (IsCasting)
                {
                    return $"V · 복마어주자 · 전개 준비 {CastProgress * 100f:0}%";
                }
                if (IsActive)
                {
                    return $"영역전개 · 복마어주자 · {ActiveRemaining:0.0}s · 필중 {sureHitsApplied}/{sureHitCount}";
                }
                if (CooldownRemaining > 0f)
                {
                    return $"V · 복마어주자 · {CooldownRemaining:0.0}s";
                }

                cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
                if (cursedEnergy != null && !cursedEnergy.CanSpend(domainEnergyCost))
                {
                    return $"V · 복마어주자 · 주력 부족 · CE {DomainEnergyCost:0}";
                }
                return $"V · 복마어주자 · READY · CE {DomainEnergyCost:0}";
            }
        }

        public static SukunaDomainController GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            SukunaDomainController controller = owner.GetComponent<SukunaDomainController>();
            return controller != null ? controller : owner.AddComponent<SukunaDomainController>();
        }

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
            cursedEnergy = CursedEnergyController.GetOrCreate(gameObject);
            cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SukunaShibuyaReserve);
            actionGate = CombatActionGate.GetOrCreate(gameObject);
            combatAudio = PrototypeCombatAudio.GetOrCreate(gameObject);
            sukunaAudio = SukunaCombatAudio.GetOrCreate(gameObject);
            sukunaTechnique = GetComponent<SukunaTechniqueController>();
        }

        private void OnDisable()
        {
            ResetDomain();
        }

        private void Update()
        {
            if (ownHealth == null || ownHealth.IsDead || !HasLivingOpponent())
            {
                if (State != DomainState.Normal)
                {
                    ResetDomain();
                }
                return;
            }

            if (State == DomainState.Normal)
            {
                if (Input.GetKeyDown(CombatInputBindings.Domain))
                {
                    TryBeginDomain();
                }
                return;
            }

            if (State == DomainState.Casting)
            {
                if (Time.time >= castCompletesAt)
                {
                    ActivateDomain();
                }
                return;
            }

            if (State == DomainState.Active)
            {
                if (Input.GetKeyDown(CombatInputBindings.Ultimate))
                {
                    sukunaTechnique ??= GetComponent<SukunaTechniqueController>();
                    sukunaTechnique?.TryUseFugaInsideDomain(domainCenter, domainRadius);
                }

                if (sureHitsApplied < sureHitCount && Time.time >= nextSureHitAt)
                {
                    ApplySureHitPulse();
                }

                if (Time.time >= activeEndsAt)
                {
                    EndDomain();
                }
            }
        }

        public void ResetDomain()
        {
            State = DomainState.Normal;
            castCompletesAt = 0f;
            activeEndsAt = 0f;
            nextSureHitAt = 0f;
            sureHitsApplied = 0;

            if (domainVisual != null)
            {
                domainVisual.BeginFadeOut();
                domainVisual = null;
            }
        }

        private void TryBeginDomain()
        {
            if (Time.time < nextDomainAt)
            {
                return;
            }

            actionGate ??= CombatActionGate.GetOrCreate(gameObject);
            if (actionGate == null || !actionGate.CanStartDomain)
            {
                return;
            }

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SukunaShibuyaReserve, false);
            if (cursedEnergy != null && !cursedEnergy.CanSpend(domainEnergyCost))
            {
                cursedEnergy.NotifyInsufficient("복마어주자", domainEnergyCost);
                return;
            }

            State = DomainState.Casting;
            castCompletesAt = Time.time + domainCastTime;
        }

        private void ActivateDomain()
        {
            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SukunaShibuyaReserve, false);
            if (cursedEnergy != null && !cursedEnergy.TrySpend(domainEnergyCost, "복마어주자"))
            {
                ResetDomain();
                return;
            }

            State = DomainState.Active;
            domainCenter = transform.position - Vector3.up * 1f;
            activeEndsAt = Time.time + domainDuration;
            nextSureHitAt = Time.time;
            nextDomainAt = Time.time + domainCooldown;
            sureHitsApplied = 0;

            GameObject visualObject = new GameObject("MalevolentShrinePrototypeVisual");
            visualObject.transform.position = domainCenter;
            domainVisual = visualObject.AddComponent<SukunaMalevolentShrineVisual>();
            domainVisual.Configure(domainRadius);

            sukunaAudio ??= SukunaCombatAudio.GetOrCreate(gameObject);
            sukunaAudio?.PlayDomain();
        }

        private void ApplySureHitPulse()
        {
            nextSureHitAt = Time.time + sureHitInterval;
            int pulseIndex = sureHitsApplied;
            sureHitsApplied += 1;

            Health[] allHealth = FindObjectsByType<Health>(FindObjectsSortMode.None);
            HashSet<Health> affected = new HashSet<Health>();
            foreach (Health target in allHealth)
            {
                if (
                    target == null
                    || target == ownHealth
                    || target.IsDead
                    || !affected.Add(target)
                )
                {
                    continue;
                }

                Vector3 offset = target.transform.position - domainCenter;
                offset.y = 0f;
                if (offset.magnitude > domainRadius)
                {
                    continue;
                }

                domainVisual?.PulseSlashAt(target.transform.position, pulseIndex);
                DamageContext context = new DamageContext(
                    damagePerHit,
                    gameObject,
                    DamageDeliveryType.DomainSureHit,
                    DamageTraits.None,
                    "복마어주자 · 필중 참격",
                    target.transform.position + Vector3.up * 0.8f
                );
                if (target.ReceiveDamage(context) != DamageResolution.Applied)
                {
                    continue;
                }

                ApplyHitReaction(target, Vector3.zero, sureHitStun);
            }

            if (pulseIndex == 0 || sureHitsApplied >= sureHitCount)
            {
                combatAudio ??= PrototypeCombatAudio.GetOrCreate(gameObject);
                combatAudio?.PlayBasicHit(sureHitsApplied >= sureHitCount ? 3 : 1);
            }
        }

        private void EndDomain()
        {
            State = DomainState.Normal;
            castCompletesAt = 0f;
            activeEndsAt = 0f;
            nextSureHitAt = 0f;
            sureHitsApplied = 0;

            if (domainVisual != null)
            {
                domainVisual.BeginFadeOut();
                domainVisual = null;
            }
        }

        private bool HasLivingOpponent()
        {
            Health[] allHealth = FindObjectsByType<Health>(FindObjectsSortMode.None);
            foreach (Health health in allHealth)
            {
                if (health != null && health != ownHealth && !health.IsDead)
                {
                    return true;
                }
            }
            return false;
        }

        private static void ApplyHitReaction(Health target, Vector3 impulse, float stun)
        {
            MonoBehaviour[] behaviours = target.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IHitReactable hitReactable)
                {
                    hitReactable.ApplyHitReaction(impulse, stun);
                    break;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, domainRadius);
        }
    }
}
