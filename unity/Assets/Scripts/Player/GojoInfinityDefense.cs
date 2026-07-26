using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(GojoVariantController))]
    public sealed class GojoInfinityDefense : MonoBehaviour, IDamageGuard
    {
        [Header("Infinity Prototype")]
        [SerializeField] private bool infinityEnabled = true;
        [SerializeField, Min(0.1f)] private float feedbackDuration = 1.15f;
        [SerializeField, Min(0.1f)] private float rippleDuration = 0.32f;
        [SerializeField, Min(0.1f)] private float rippleRadius = 1.05f;

        private GojoVariantController variant;
        private GameObject rippleRoot;
        private LineRenderer rippleLine;
        private Material rippleMaterial;
        private float rippleStartedAt;
        private float feedbackUntil;
        private string feedbackText = string.Empty;
        private Color feedbackColor;
        private GUIStyle statusStyle;
        private int styledForHeight = -1;

        public bool IsInfinityActive =>
            infinityEnabled
            && variant != null
            && (
                variant.ActiveVariant == GojoVariantId.ModernTeacher
                || variant.ActiveVariant == GojoVariantId.ShinjukuShowdown
            );

        public static GojoInfinityDefense GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            GojoInfinityDefense defense = owner.GetComponent<GojoInfinityDefense>();
            return defense != null ? defense : owner.AddComponent<GojoInfinityDefense>();
        }

        private void Awake()
        {
            variant = GojoVariantController.GetOrCreate(gameObject);
            BuildRippleVisual();
        }

        private void Update()
        {
            UpdateRippleVisual();
        }

        private void OnDestroy()
        {
            if (rippleMaterial != null)
            {
                Destroy(rippleMaterial);
            }
        }

        public DamageGuardDecision EvaluateDamage(DamageContext context)
        {
            if (
                !IsInfinityActive
                || context.DeliveryType == DamageDeliveryType.Environmental
                || context.Source == gameObject
            )
            {
                return DamageGuardDecision.NoDecision;
            }

            if (context.BypassesInfinity)
            {
                ShowFeedback(
                    $"INFINITY BYPASSED · {ResolveBypassReason(context)}",
                    new Color(1f, 0.36f, 0.10f)
                );
                return DamageGuardDecision.Bypass;
            }

            ShowFeedback(
                $"INFINITY · BLOCKED · {context.ActionName}",
                new Color(0.22f, 0.78f, 1f)
            );
            ShowBlockRipple();
            return DamageGuardDecision.Block;
        }

        private static string ResolveBypassReason(DamageContext context)
        {
            if (context.DeliveryType == DamageDeliveryType.DomainSureHit)
            {
                return "DOMAIN SURE-HIT";
            }
            if (context.HasTrait(DamageTraits.DomainAmplification))
            {
                return "DOMAIN AMPLIFICATION";
            }
            if (context.HasTrait(DamageTraits.TechniqueNullification))
            {
                return "TECHNIQUE NULLIFICATION";
            }
            if (context.HasTrait(DamageTraits.IgnoresInfinity))
            {
                return "SPECIAL BYPASS";
            }
            return "UNBLOCKABLE";
        }

        private void ShowFeedback(string text, Color color)
        {
            feedbackText = text;
            feedbackColor = color;
            feedbackUntil = Time.time + feedbackDuration;
        }

        private void BuildRippleVisual()
        {
            rippleRoot = new GameObject("InfinityBlockRipple");
            rippleRoot.transform.SetParent(transform, false);
            rippleRoot.transform.localPosition = Vector3.up * 0.55f;

            rippleLine = rippleRoot.AddComponent<LineRenderer>();
            rippleLine.loop = true;
            rippleLine.useWorldSpace = false;
            rippleLine.positionCount = 96;
            rippleLine.startWidth = 0.06f;
            rippleLine.endWidth = 0.06f;
            rippleLine.numCornerVertices = 4;
            rippleLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rippleLine.receiveShadows = false;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }
            if (shader != null)
            {
                rippleMaterial = new Material(shader)
                {
                    color = new Color(0.28f, 0.86f, 1f, 0.95f),
                };
                rippleLine.material = rippleMaterial;
            }

            for (int index = 0; index < rippleLine.positionCount; index++)
            {
                float angle = (float)index / rippleLine.positionCount * Mathf.PI * 2f;
                rippleLine.SetPosition(
                    index,
                    new Vector3(
                        Mathf.Cos(angle) * rippleRadius,
                        Mathf.Sin(angle) * rippleRadius * 1.25f,
                        0f
                    )
                );
            }

            rippleRoot.SetActive(false);
        }

        private void ShowBlockRipple()
        {
            if (rippleRoot == null)
            {
                return;
            }

            rippleStartedAt = Time.time;
            rippleRoot.transform.localScale = Vector3.one;
            rippleRoot.transform.localRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
            SetRippleColor(new Color(0.28f, 0.86f, 1f, 0.95f));
            rippleRoot.SetActive(true);
        }

        private void UpdateRippleVisual()
        {
            if (rippleRoot == null || !rippleRoot.activeSelf)
            {
                return;
            }

            float normalized = Mathf.Clamp01((Time.time - rippleStartedAt) / rippleDuration);
            float scale = Mathf.Lerp(0.78f, 1.46f, normalized);
            rippleRoot.transform.localScale = Vector3.one * scale;
            SetRippleColor(new Color(0.28f, 0.86f, 1f, 0.95f * (1f - normalized)));

            if (normalized >= 1f)
            {
                rippleRoot.SetActive(false);
            }
        }

        private void SetRippleColor(Color color)
        {
            if (rippleLine != null)
            {
                rippleLine.startColor = color;
                rippleLine.endColor = color;
            }
            if (rippleMaterial != null)
            {
                rippleMaterial.color = color;
            }
        }

        private void OnGUI()
        {
            if (!IsInfinityActive)
            {
                return;
            }

            EnsureStyle();
            float width = Mathf.Min(330f, Screen.width - 24f);
            Rect panel = new Rect(12f, 236f, width, 31f);
            bool showingFeedback = Time.time <= feedbackUntil && !string.IsNullOrEmpty(feedbackText);
            Color accent = showingFeedback
                ? feedbackColor
                : new Color(0.22f, 0.72f, 1f);
            string text = showingFeedback ? feedbackText : "INFINITY · 무하한 자동 방어";

            DrawRect(panel, new Color(0.012f, 0.035f, 0.075f, 0.90f));
            DrawBorder(panel, accent, showingFeedback ? 2f : 1f);
            statusStyle.normal.textColor = accent;
            GUI.Label(panel, text, statusStyle);
        }

        private void EnsureStyle()
        {
            if (styledForHeight == Screen.height)
            {
                return;
            }

            styledForHeight = Screen.height;
            statusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 68f, 11f, 15f)),
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
