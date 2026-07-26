using System;
using UnityEngine;

namespace JJKGame.Core
{
    public enum CursedEnergyProfileId
    {
        Standard,
        SixEyesEfficiency,
        SukunaVastReserve,
        YutaLargeReserve,
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class CursedEnergyController : MonoBehaviour
    {
        [Header("GAME_ORIGINAL Cursed Energy")]
        [SerializeField] private CursedEnergyProfileId activeProfile = CursedEnergyProfileId.Standard;
        [SerializeField, Min(1f)] private float maxEnergy = 100f;
        [SerializeField, Min(0f)] private float startingEnergy = 100f;
        [SerializeField, Min(0f)] private float regenerationPerSecond = 12f;
        [SerializeField, Min(0f)] private float regenerationDelayAfterSpend = 0.8f;
        [SerializeField, Min(0f)] private float costMultiplier = 1f;
        [SerializeField, Min(0f)] private float minimumTechniqueCost;
        [SerializeField, Min(0.1f)] private float noticeDuration = 1.1f;
        [SerializeField] private string profileLabel = "STANDARD";

        private Health health;
        private float nextRegenerationAt;
        private float noticeUntil;
        private string noticeText = string.Empty;
        private GUIStyle energyStyle;
        private int styledForHeight = -1;
        private bool profileApplied;

        public event Action<CursedEnergyController, float> EnergyChanged;

        public CursedEnergyProfileId ActiveProfile => activeProfile;
        public string ProfileLabel => profileLabel;
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
            ApplyProfile(activeProfile, true);
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

        public void ApplyProfile(CursedEnergyProfileId profileId, bool refill = true)
        {
            if (profileApplied && activeProfile == profileId)
            {
                return;
            }

            activeProfile = profileId;
            switch (profileId)
            {
                case CursedEnergyProfileId.SixEyesEfficiency:
                    ConfigureValues(100f, 100f, 12f, 0.8f, 0.01f, 1f, "SIX EYES · 육안 효율");
                    break;
                case CursedEnergyProfileId.SukunaVastReserve:
                    ConfigureValues(300f, 300f, 12f, 0.8f, 1f, 0f, "VAST RESERVE · 스쿠나");
                    break;
                case CursedEnergyProfileId.YutaLargeReserve:
                    ConfigureValues(150f, 150f, 10f, 0.8f, 1f, 0f, "LARGE RESERVE · 유타");
                    break;
                default:
                    ConfigureValues(100f, 100f, 12f, 0.8f, 1f, 0f, "STANDARD");
                    break;
            }

            profileApplied = true;
            if (refill)
            {
                CurrentEnergy = Mathf.Clamp(startingEnergy, 0f, maxEnergy);
                EnergyChanged?.Invoke(this, CurrentEnergy);
            }
            else
            {
                SetEnergy(CurrentEnergy);
            }
        }

        public float ResolveCost(float baseCost)
        {
            baseCost = Mathf.Max(0f, baseCost);
            if (baseCost <= 0f)
            {
                return 0f;
            }

            float adjusted = Mathf.Ceil(baseCost * Mathf.Max(0f, costMultiplier) - 0.001f);
            return Mathf.Max(minimumTechniqueCost, adjusted);
        }

        public bool CanSpend(float baseCost)
        {
            float actualCost = ResolveCost(baseCost);
            return actualCost <= 0f || CurrentEnergy + 0.001f >= actualCost;
        }

        public bool TrySpend(float baseCost, string actionName = "술식")
        {
            float actualCost = ResolveCost(baseCost);
            if (!CanSpend(baseCost))
            {
                NotifyInsufficient(actionName, baseCost);
                return false;
            }

            SetEnergy(CurrentEnergy - actualCost);
            nextRegenerationAt = Time.time + regenerationDelayAfterSpend;
            return true;
        }

        public void NotifyInsufficient(string actionName, float baseRequiredAmount)
        {
            string resolvedName = string.IsNullOrWhiteSpace(actionName) ? "술식" : actionName;
            float actualCost = ResolveCost(baseRequiredAmount);
            noticeText = $"주력 부족 · {resolvedName} 필요 {actualCost:0}";
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

        private void ConfigureValues(
            float newMax,
            float newStarting,
            float newRegeneration,
            float newRegenerationDelay,
            float newCostMultiplier,
            float newMinimumCost,
            string newProfileLabel
        )
        {
            maxEnergy = Mathf.Max(1f, newMax);
            startingEnergy = Mathf.Clamp(newStarting, 0f, maxEnergy);
            regenerationPerSecond = Mathf.Max(0f, newRegeneration);
            regenerationDelayAfterSpend = Mathf.Max(0f, newRegenerationDelay);
            costMultiplier = Mathf.Max(0f, newCostMultiplier);
            minimumTechniqueCost = Mathf.Max(0f, newMinimumCost);
            profileLabel = string.IsNullOrWhiteSpace(newProfileLabel)
                ? activeProfile.ToString()
                : newProfileLabel;
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

        private void OnGUI()
        {
            if (health == null || health.IsDead)
            {
                return;
            }

            EnsureStyle();
            const float margin = 24f;
            float availableHalfWidth = (Screen.width - margin * 3f) * 0.5f;
            float panelWidth = Mathf.Clamp(availableHalfWidth, 250f, 440f);
            Rect bar = new Rect(margin + 14f, margin + 63f, panelWidth - 28f, 15f);
            bool showingNotice = !string.IsNullOrEmpty(NoticeText);
            Color accent = showingNotice
                ? new Color(1f, 0.24f, 0.18f, 0.98f)
                : new Color(0.34f, 0.34f, 1f, 0.98f);

            DrawRect(bar, new Color(0.055f, 0.045f, 0.13f, 1f));
            DrawRect(
                new Rect(bar.x + 1f, bar.y + 1f, (bar.width - 2f) * Normalized, bar.height - 2f),
                new Color(0.30f, 0.18f, 0.92f, 0.96f)
            );
            DrawBorder(bar, accent, 1f);

            energyStyle.normal.textColor = Color.white;
            string text = showingNotice
                ? NoticeText
                : $"CURSED ENERGY  {CurrentEnergy:0} / {MaxEnergy:0}  ·  {ProfileLabel}";
            GUI.Label(bar, text, energyStyle);
        }

        private void EnsureStyle()
        {
            if (styledForHeight == Screen.height)
            {
                return;
            }

            styledForHeight = Screen.height;
            energyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 78f, 10f, 14f)),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private static void DrawBorder(Rect rect, Color color, float thickness)
        {
            DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }
    }
}
