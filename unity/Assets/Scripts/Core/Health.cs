using System;
using UnityEngine;

namespace JJKGame.Core
{
    public sealed class Health : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maxHealth = 100f;

        [Header("Out Of Bounds")]
        [SerializeField] private bool dieBelowWorld = true;
        [SerializeField] private float worldDeathY = -12f;

        public event Action<Health> Died;
        public event Action<Health, float> HealthChanged;
        public event Action<Health, DamageContext, DamageResolution> DamageResolved;

        private float invulnerableUntil;

        public float MaxHealth => maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;
        public bool IsInvulnerable => Time.time < invulnerableUntil;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        private void Update()
        {
            if (dieBelowWorld && !IsDead && transform.position.y < worldDeathY)
            {
                Kill();
            }
        }

        public DamageResolution ReceiveDamage(DamageContext context)
        {
            if (IsDead)
            {
                return Resolve(context, DamageResolution.TargetDead);
            }

            if (context.Amount <= 0f)
            {
                return Resolve(context, DamageResolution.Invalid);
            }

            if (IsInvulnerable)
            {
                return Resolve(context, DamageResolution.Invulnerable);
            }

            MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (
                    behaviour == null
                    || !behaviour.isActiveAndEnabled
                    || !(behaviour is IDamageGuard guard)
                )
                {
                    continue;
                }

                DamageGuardDecision decision = guard.EvaluateDamage(context);
                if (decision == DamageGuardDecision.Block)
                {
                    return Resolve(context, DamageResolution.Guarded);
                }
            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - context.Amount);
            HealthChanged?.Invoke(this, CurrentHealth);
            DamageResolved?.Invoke(this, context, DamageResolution.Applied);

            if (IsDead)
            {
                Died?.Invoke(this);
            }

            return DamageResolution.Applied;
        }

        public bool TakeDamage(float amount)
        {
            return ReceiveDamage(DamageContext.Legacy(amount)) == DamageResolution.Applied;
        }

        public void Kill()
        {
            if (IsDead)
            {
                return;
            }

            CurrentHealth = 0f;
            HealthChanged?.Invoke(this, CurrentHealth);
            Died?.Invoke(this);
        }

        public void GrantInvulnerability(float duration)
        {
            if (IsDead || duration <= 0f)
            {
                return;
            }

            invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + duration);
        }

        public void Restore(float amount)
        {
            if (IsDead || amount <= 0f)
            {
                return;
            }

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            HealthChanged?.Invoke(this, CurrentHealth);
        }

        public void SetCurrentHealth(float value)
        {
            CurrentHealth = Mathf.Clamp(value, 0f, maxHealth);
            invulnerableUntil = 0f;
            HealthChanged?.Invoke(this, CurrentHealth);
        }

        public void ResetHealth()
        {
            SetCurrentHealth(maxHealth);
        }

        private DamageResolution Resolve(DamageContext context, DamageResolution resolution)
        {
            DamageResolved?.Invoke(this, context, resolution);
            return resolution;
        }
    }
}
