using System;
using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [RequireComponent(typeof(Health))]
    public sealed class GojoTechniqueController : MonoBehaviour
    {
        [Header("Cursed Technique Lapse: Blue")]
        [SerializeField, Min(0.1f)] private float blueRadius = 8f;
        [SerializeField, Min(0f)] private float blueDamage = 8f;
        [SerializeField, Min(0f)] private float bluePullSpeed = 13f;
        [SerializeField, Min(0f)] private float blueHitStun = 0.42f;
        [SerializeField, Min(0.1f)] private float blueCooldown = 3.2f;
        [SerializeField, Min(0.1f)] private float blueVisualDuration = 0.65f;

        [Header("Cursed Technique Reversal: Red")]
        [SerializeField, Min(0.1f)] private float redRadius = 7f;
        [SerializeField, Min(0f)] private float redDamage = 18f;
        [SerializeField, Min(0f)] private float redPushSpeed = 20f;
        [SerializeField, Min(0f)] private float redHitStun = 0.52f;
        [SerializeField, Min(0.1f)] private float redCooldown = 4.5f;
        [SerializeField, Min(0.1f)] private float redVisualDuration = 0.58f;

        private Health ownHealth;
        private Health[] combatHealth;
        private GojoDomainController domainController;
        private TechniqueVisual blueVisual;
        private TechniqueVisual redVisual;
        private float nextBlueAt;
        private float nextRedAt;
        private GUIStyle skillStyle;
        private int styledForHeight = -1;

        public event Action<Health> BlueHit;
        public event Action<Health> RedHit;

        public bool BlueReady => Time.time >= nextBlueAt;
        public bool RedReady => Time.time >= nextRedAt;
        public bool CanUseUltimate => CombatActive && !DomainBusy;
        public float BlueCooldownRemaining => Mathf.Max(0f, nextBlueAt - Time.time);
        public float RedCooldownRemaining => Mathf.Max(0f, nextRedAt - Time.time);
        public float BlueCooldownProgress => GetCooldownProgress(BlueCooldownRemaining, blueCooldown);
        public float RedCooldownProgress => GetCooldownProgress(RedCooldownRemaining, redCooldown);

        public string BlueStatusText => BuildStatusText(
            $"{CombatInputBindings.Skill1Label} · 술식순전 「창」",
            BlueReady,
            BlueCooldownRemaining
        );

        public string RedStatusText => BuildStatusText(
            $"{CombatInputBindings.Skill2Label} · 술식반전 「혁」",
            RedReady,
            RedCooldownRemaining
        );

        private bool DomainBusy =>
            domainController != null
            && domainController.State != GojoDomainController.DomainState.Normal;

        private bool CombatActive =>
            ownHealth != null
            && !ownHealth.IsDead
            && HasLivingOpponent();

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
            domainController = GetComponent<GojoDomainController>();
            blueVisual = BuildTechniqueVisual(
                "BluePrototypeVisual",
                blueRadius,
                blueVisualDuration,
                new Color(0.10f, 0.72f, 1f, 0.95f),
                new Color(0.32f, 0.92f, 1f, 0.90f),
                new Color(0.12f, 0.55f, 1f),
                3.4f,
                90f
            );
            redVisual = BuildTechniqueVisual(
                "RedPrototypeVisual",
                redRadius,
                redVisualDuration,
                new Color(1f, 0.12f, 0.16f, 0.98f),
                new Color(1f, 0.52f, 0.12f, 0.92f),
                new Color(1f, 0.12f, 0.08f),
                4.2f,
                -130f
            );
        }

        private void Start()
        {
            RefreshCombatHealth();
        }

        private void OnDisable()
        {
            SetVisualActive(blueVisual, false);
            SetVisualActive(redVisual, false);
        }

        private void Update()
        {
            UpdateVisual(blueVisual);
            UpdateVisual(redVisual);

            if (Input.GetKeyDown(CombatInputBindings.Skill1) && CanUseTechnique(BlueReady))
            {
                ActivateBlue();
            }

            if (Input.GetKeyDown(CombatInputBindings.Skill2) && CanUseTechnique(RedReady))
            {
                ActivateRed();
            }
        }

        private bool CanUseTechnique(bool ready)
        {
            return ready && CombatActive && !DomainBusy;
        }

        private void ActivateBlue()
        {
            nextBlueAt = Time.time + blueCooldown;
            ShowVisual(blueVisual);
            ApplyRadialTechnique(
                blueRadius,
                blueDamage,
                bluePullSpeed,
                blueHitStun,
                true,
                target => BlueHit?.Invoke(target)
            );
        }

        private void ActivateRed()
        {
            nextRedAt = Time.time + redCooldown;
            ShowVisual(redVisual);
            ApplyRadialTechnique(
                redRadius,
                redDamage,
                redPushSpeed,
                redHitStun,
                false,
                target => RedHit?.Invoke(target)
            );
        }

        private void ApplyRadialTechnique(
            float radius,
            float damage,
            float impulseSpeed,
            float hitStun,
            bool pullTowardCaster,
            Action<Health> onTargetHit
        )
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            HashSet<Health> affectedTargets = new HashSet<Health>();

            foreach (Collider hit in hits)
            {
                Health targetHealth = hit.GetComponentInParent<Health>();
                if (
                    targetHealth == null
                    || targetHealth == ownHealth
                    || targetHealth.IsDead
                    || !affectedTargets.Add(targetHealth)
                )
                {
                    continue;
                }

                if (!targetHealth.TakeDamage(damage))
                {
                    continue;
                }

                ApplyImpulse(targetHealth, impulseSpeed, hitStun, pullTowardCaster);
                onTargetHit?.Invoke(targetHealth);
            }
        }

        private void ApplyImpulse(
            Health targetHealth,
            float impulseSpeed,
            float hitStun,
            bool pullTowardCaster
        )
        {
            Vector3 direction = pullTowardCaster
                ? transform.position - targetHealth.transform.position
                : targetHealth.transform.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = pullTowardCaster
                    ? -targetHealth.transform.forward
                    : targetHealth.transform.forward;
            }

            Vector3 impulse = direction.normalized * impulseSpeed;
            MonoBehaviour[] behaviours = targetHealth.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IHitReactable hitReactable)
                {
                    hitReactable.ApplyHitReaction(impulse, hitStun);
                    break;
                }
            }
        }

        private TechniqueVisual BuildTechniqueVisual(
            string objectName,
            float radius,
            float duration,
            Color outerColor,
            Color innerColor,
            Color lightColor,
            float lightIntensity,
            float rotationSpeed
        )
        {
            TechniqueVisual visual = new TechniqueVisual
            {
                Root = new GameObject(objectName),
                Duration = duration,
                LightIntensity = lightIntensity,
                RotationSpeed = rotationSpeed,
            };
            visual.Root.transform.SetParent(transform, false);
            visual.Root.transform.localPosition = Vector3.up * 0.15f;

            CreateRing(
                visual,
                objectName + "OuterRing",
                radius,
                0.14f,
                outerColor,
                Quaternion.identity,
                RingPlane.XZ
            );
            CreateRing(
                visual,
                objectName + "InnerRing",
                radius * 0.42f,
                0.10f,
                innerColor,
                Quaternion.Euler(72f, 0f, 18f),
                RingPlane.XY
            );

            GameObject lightObject = new GameObject(objectName + "Light");
            lightObject.transform.SetParent(visual.Root.transform, false);
            lightObject.transform.localPosition = Vector3.up * 1.4f;
            visual.Light = lightObject.AddComponent<Light>();
            visual.Light.type = LightType.Point;
            visual.Light.color = lightColor;
            visual.Light.range = radius * 1.6f;
            visual.Light.intensity = lightIntensity;
            visual.Light.shadows = LightShadows.None;

            visual.Root.SetActive(false);
            return visual;
        }

        private static void CreateRing(
            TechniqueVisual visual,
            string objectName,
            float radius,
            float width,
            Color color,
            Quaternion localRotation,
            RingPlane plane
        )
        {
            GameObject ringObject = new GameObject(objectName);
            ringObject.transform.SetParent(visual.Root.transform, false);
            ringObject.transform.localRotation = localRotation;

            LineRenderer line = ringObject.AddComponent<LineRenderer>();
            line.loop = true;
            line.useWorldSpace = false;
            line.positionCount = 96;
            line.startWidth = width;
            line.endWidth = width;
            line.startColor = color;
            line.endColor = color;
            line.numCornerVertices = 4;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader != null)
            {
                line.material = new Material(shader)
                {
                    color = color,
                };
            }

            for (int index = 0; index < line.positionCount; index++)
            {
                float angle = (float)index / line.positionCount * Mathf.PI * 2f;
                float cosine = Mathf.Cos(angle) * radius;
                float sine = Mathf.Sin(angle) * radius;
                Vector3 point = plane switch
                {
                    RingPlane.XY => new Vector3(cosine, sine, 0f),
                    _ => new Vector3(cosine, 0f, sine),
                };
                line.SetPosition(index, point);
            }

            visual.Rings.Add(line);
            visual.RingColors.Add(color);
        }

        private static void ShowVisual(TechniqueVisual visual)
        {
            if (visual == null || visual.Root == null)
            {
                return;
            }

            visual.StartedAt = Time.time;
            visual.Root.transform.localScale = Vector3.one * 0.08f;
            RestoreVisualColors(visual, 1f);
            if (visual.Light != null)
            {
                visual.Light.intensity = visual.LightIntensity;
            }
            visual.Root.SetActive(true);
        }

        private static void UpdateVisual(TechniqueVisual visual)
        {
            if (visual == null || visual.Root == null || !visual.Root.activeSelf)
            {
                return;
            }

            float elapsed = Time.time - visual.StartedAt;
            float normalized = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, visual.Duration));
            float expansion = 1f - Mathf.Pow(1f - normalized, 3f);
            float pulse = 1f + Mathf.Sin(elapsed * 18f) * 0.045f;
            visual.Root.transform.localScale =
                Vector3.one * Mathf.Max(0.08f, expansion * pulse);
            visual.Root.transform.Rotate(
                Vector3.up,
                visual.RotationSpeed * Time.deltaTime,
                Space.Self
            );

            float alpha = 1f - normalized;
            RestoreVisualColors(visual, alpha);

            if (visual.Light != null)
            {
                visual.Light.intensity = Mathf.Lerp(visual.LightIntensity, 0f, normalized);
            }

            if (elapsed >= visual.Duration)
            {
                visual.Root.SetActive(false);
            }
        }

        private static void RestoreVisualColors(TechniqueVisual visual, float alphaMultiplier)
        {
            for (int index = 0; index < visual.Rings.Count; index++)
            {
                Color color = visual.RingColors[index];
                color.a *= alphaMultiplier;
                visual.Rings[index].startColor = color;
                visual.Rings[index].endColor = color;
            }
        }

        private static void SetVisualActive(TechniqueVisual visual, bool active)
        {
            if (visual != null && visual.Root != null)
            {
                visual.Root.SetActive(active);
            }
        }

        private void OnGUI()
        {
            if (ownHealth == null || ownHealth.IsDead)
            {
                return;
            }

            EnsureStyle();
            float width = Mathf.Min(350f, Screen.width - 48f);
            DrawSkillPanel(
                new Rect(24f, 151f, width, 38f),
                BlueStatusText,
                BlueCooldownProgress,
                BlueReady,
                new Color(0.12f, 0.72f, 1f, 0.98f),
                new Color(0.05f, 0.34f, 0.68f, 0.70f),
                new Color(0.012f, 0.035f, 0.075f, 0.92f)
            );
            DrawSkillPanel(
                new Rect(24f, 195f, width, 38f),
                RedStatusText,
                RedCooldownProgress,
                RedReady,
                new Color(1f, 0.22f, 0.18f, 0.98f),
                new Color(0.70f, 0.08f, 0.06f, 0.72f),
                new Color(0.075f, 0.012f, 0.018f, 0.92f)
            );
        }

        private void DrawSkillPanel(
            Rect panel,
            string statusText,
            float cooldownProgress,
            bool ready,
            Color readyAccent,
            Color fillColor,
            Color backgroundColor
        )
        {
            bool available = ready && CombatActive && !DomainBusy;
            Color accent = available
                ? readyAccent
                : new Color(0.34f, 0.40f, 0.50f, 0.95f);

            DrawRect(panel, backgroundColor);
            Rect fill = new Rect(
                panel.x + 2f,
                panel.y + 2f,
                (panel.width - 4f) * cooldownProgress,
                panel.height - 4f
            );
            DrawRect(fill, fillColor);
            DrawBorder(panel, accent, 2f);
            skillStyle.normal.textColor = Color.white;
            GUI.Label(panel, statusText, skillStyle);
        }

        private string BuildStatusText(string skillName, bool ready, float cooldownRemaining)
        {
            if (!CombatActive)
            {
                return skillName + "  전투 종료";
            }

            if (DomainBusy)
            {
                return skillName + "  영역 입력 중";
            }

            return ready
                ? skillName + "  READY"
                : $"{skillName}  {cooldownRemaining:0.0}s";
        }

        private bool HasLivingOpponent()
        {
            if (combatHealth == null || combatHealth.Length == 0)
            {
                RefreshCombatHealth();
            }

            foreach (Health health in combatHealth)
            {
                if (health != null && health != ownHealth && !health.IsDead)
                {
                    return true;
                }
            }

            return false;
        }

        private void RefreshCombatHealth()
        {
            combatHealth = FindObjectsByType<Health>(FindObjectsSortMode.None);
        }

        private static float GetCooldownProgress(float remaining, float cooldown)
        {
            return cooldown <= 0f
                ? 1f
                : Mathf.Clamp01(1f - remaining / cooldown);
        }

        private void EnsureStyle()
        {
            if (styledForHeight == Screen.height)
            {
                return;
            }

            styledForHeight = Screen.height;
            int fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 48f, 14f, 20f));
            skillStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        private static void DrawBorder(Rect rect, Color color, float thickness)
        {
            DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, blueRadius);
            Gizmos.DrawWireSphere(transform.position, redRadius);
        }

        private sealed class TechniqueVisual
        {
            public GameObject Root;
            public readonly List<LineRenderer> Rings = new List<LineRenderer>();
            public readonly List<Color> RingColors = new List<Color>();
            public Light Light;
            public float StartedAt;
            public float Duration;
            public float LightIntensity;
            public float RotationSpeed;
        }

        private enum RingPlane
        {
            XZ,
            XY,
        }
    }
}
