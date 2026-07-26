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
        private GUIStyle energyStyle;
        private int styledForHeight = -1;

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
                : $"CURSED ENERGY  {CurrentEnergy:0} / {MaxEnergy:0}  ·  회복 {regenerationPerSecond:0}/s";
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
