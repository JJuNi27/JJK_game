using JJKGame.Player;
using UnityEngine;

namespace JJKGame.Core
{
    [DisallowMultipleComponent]
    public sealed class TechniqueBurnoutController : MonoBehaviour
    {
        [Header("OFFICIAL_CONCEPT / GAME_ORIGINAL Duration")]
        [SerializeField, Min(0.1f)] private float burnoutDuration = 5f;

        private GojoDomainController domain;
        private GojoVariantController variant;
        private GojoDomainController.DomainState previousDomainState;
        private float burnoutEndsAt;
        private GUIStyle burnoutStyle;
        private int styledForHeight = -1;

        public bool IsBurnedOut => Time.time < burnoutEndsAt;
        public float Remaining => Mathf.Max(0f, burnoutEndsAt - Time.time);
        public float NormalizedRemaining => burnoutDuration <= 0f
            ? 0f
            : Mathf.Clamp01(Remaining / burnoutDuration);

        public static TechniqueBurnoutController GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            TechniqueBurnoutController burnout = owner.GetComponent<TechniqueBurnoutController>();
            return burnout != null ? burnout : owner.AddComponent<TechniqueBurnoutController>();
        }

        private void Awake()
        {
            domain = GetComponent<GojoDomainController>();
            variant = GojoVariantController.GetOrCreate(gameObject);
            previousDomainState = domain != null
                ? domain.State
                : GojoDomainController.DomainState.Normal;
        }

        private void Update()
        {
            domain ??= GetComponent<GojoDomainController>();
            if (domain == null)
            {
                return;
            }

            GojoDomainController.DomainState currentState = domain.State;
            if (
                previousDomainState == GojoDomainController.DomainState.Active
                && currentState == GojoDomainController.DomainState.Normal
            )
            {
                BeginBurnout();
            }

            previousDomainState = currentState;
        }

        public void BeginBurnout()
        {
            burnoutEndsAt = Mathf.Max(burnoutEndsAt, Time.time + burnoutDuration);
        }

        public bool TryRestoreTechniqueEarly()
        {
            variant ??= GojoVariantController.GetOrCreate(gameObject);
            if (variant == null || !variant.CanManuallyRestoreTechniqueBurnout)
            {
                return false;
            }

            burnoutEndsAt = 0f;
            return true;
        }

        private void OnGUI()
        {
            if (CombatHudPresentationMode.ProductionCanvasActive)
            {
                return;
            }

            if (!IsBurnedOut)
            {
                return;
            }

            EnsureStyle();
            float width = Mathf.Min(330f, Screen.width - 24f);
            Rect panel = new Rect(12f, 286f, width, 31f);
            DrawRect(panel, new Color(0.12f, 0.035f, 0.02f, 0.92f));
            DrawRect(
                new Rect(panel.x + 2f, panel.y + 2f, (panel.width - 4f) * NormalizedRemaining, panel.height - 4f),
                new Color(0.80f, 0.18f, 0.06f, 0.60f)
            );
            DrawBorder(panel, new Color(1f, 0.40f, 0.12f), 1f);
            GUI.Label(panel, $"술식 번아웃 · Q/E/R/V 봉인 {Remaining:0.0}s", burnoutStyle);
        }

        private void EnsureStyle()
        {
            if (styledForHeight == Screen.height)
            {
                return;
            }

            styledForHeight = Screen.height;
            burnoutStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 68f, 11f, 15f)),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            burnoutStyle.normal.textColor = new Color(1f, 0.88f, 0.78f);
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
