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

        public bool TakeDamage(float amount)
        {
            if (IsDead || IsInvulnerable || amount <= 0f)
            {
                return false;
            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            HealthChanged?.Invoke(this, CurrentHealth);

            if (IsDead)
            {
                Died?.Invoke(this);
            }

            return true;
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

        public void ResetHealth()
        {
            CurrentHealth = maxHealth;
            invulnerableUntil = 0f;
            HealthChanged?.Invoke(this, CurrentHealth);
        }
    }
}
