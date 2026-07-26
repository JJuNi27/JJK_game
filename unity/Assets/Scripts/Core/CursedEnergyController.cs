using System;
using UnityEngine;

namespace JJKGame.Core
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class CursedEnergyController : MonoBehaviour
    {
        [Header("GAME_ORIGINAL Cursed Energy")]
        [SerializeField, Min(1f)] private float maxEnergy = 100f;
        [SerializeField, Min(0f)] private float startingEnergy = 100f;
        [SerializeField, Min(0f)] private float regenerationPerSecond = 12f;
        [SerializeField, Min(0f)] private float regenerationDelayAfterSpend = 0.8f;
        [SerializeField, Min(0.1f)] private float noticeDuration = 1.1f;

        private Health health;
        private float nextRegenerationAt;
        private float noticeUntil;
        private string noticeText = string.Empty;

        public event Action<CursedEnergyController, float> EnergyChanged;

        public float MaxEnergy => maxEnergy;
        public float CurrentEnergy { get; private set; }
        public float Normalized => maxEnergy > 0f
            ? Mathf.Clamp01(CurrentEnergy / maxEnergy)
            : 0f;
        public string NoticeText => Time.time <= noticeUntil ? noticeText : string.Empty;

        public static CursedEnergyController GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            CursedEnergyController energy = owner.GetComponent<CursedEnergyController>();
            return energy != null ? energy : owner.AddComponent<CursedEnergyController>();
        }

        private void Awake()
        {
            health = GetComponent<Health>();
            CurrentEnergy = Mathf.Clamp(startingEnergy, 0f, maxEnergy);
        }

        private void Update()
        {
            if (
                health == null
                || health.IsDead
                || regenerationPerSecond <= 0f
                || Time.time < nextRegenerationAt
                || CurrentEnergy >= maxEnergy
            )
            {
                return;
            }

            SetEnergy(CurrentEnergy + regenerationPerSecond * Time.deltaTime);
        }

        public bool CanSpend(float amount)
        {
            return amount <= 0f || CurrentEnergy + 0.001f >= amount;
        }

        public bool TrySpend(float amount, string actionName = "술식")
        {
            amount = Mathf.Max(0f, amount);
            if (!CanSpend(amount))
            {
                NotifyInsufficient(actionName, amount);
                return false;
            }

            SetEnergy(CurrentEnergy - amount);
            nextRegenerationAt = Time.time + regenerationDelayAfterSpend;
            return true;
        }

        public void NotifyInsufficient(string actionName, float requiredAmount)
        {
            string resolvedName = string.IsNullOrWhiteSpace(actionName) ? "술식" : actionName;
            noticeText = $"주력 부족 · {resolvedName} 필요 {requiredAmount:0}";
            noticeUntil = Time.time + noticeDuration;
        }

        public void Restore(float amount)
        {
            if (amount > 0f)
            {
                SetEnergy(CurrentEnergy + amount);
            }
        }

        public void ResetEnergy()
        {
            nextRegenerationAt = 0f;
            noticeUntil = 0f;
            noticeText = string.Empty;
            SetEnergy(Mathf.Clamp(startingEnergy, 0f, maxEnergy));
        }

        private void SetEnergy(float value)
        {
            float clamped = Mathf.Clamp(value, 0f, maxEnergy);
            if (Mathf.Approximately(clamped, CurrentEnergy))
            {
                return;
            }

            CurrentEnergy = clamped;
            EnergyChanged?.Invoke(this, CurrentEnergy);
        }
    }
}
