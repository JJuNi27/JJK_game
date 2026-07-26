using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(GojoTechniqueController))]
    public sealed class GojoTechniqueChainController : MonoBehaviour
    {
        [Header("Blue To Red Chain")]
        [SerializeField, Min(0.1f)] private float blueMarkDuration = 2.2f;
        [SerializeField, Min(0f)] private float chainBonusDamage = 12f;
        [SerializeField, Min(0f)] private float empoweredPushSpeed = 28f;
        [SerializeField, Min(0f)] private float empoweredHitStun = 0.72f;
        [SerializeField, Min(0.1f)] private float chainNoticeDuration = 1.15f;

        [Header("Hollow Purple")]
        [SerializeField, Min(0.1f)] private float purpleChargeDuration = 8f;
        [SerializeField, Min(0.1f)] private float purpleCooldown = 10f;
        [SerializeField, Min(0.1f)] private float purpleRange = 18f;
        [SerializeField, Min(0.1f)] private float purpleRadius = 2.2f;
        [SerializeField, Min(0f)] private float purpleDamage = 55f;
        [SerializeField, Min(0f)] private float purplePushSpeed = 34f;
        [SerializeField, Min(0f)] private float purpleHitStun = 1f;
        [SerializeField, Min(0.1f)] private float purpleVisualDuration = 0.85f;

        private readonly Dictionary<Health, float> blueMarkedUntil =
            new Dictionary<Health, float>();
        private readonly List<Health> expiredMarks = new List<Health>();

        private Health ownHealth;
        private GojoTechniqueController techniqueController;
        private float blueChargeUntil;
        private float redChargeUntil;
        private float nextPurpleAt;
        private float chainNoticeUntil;
        private float purpleNoticeUntil;

        private GameObject purpleVisualRoot;
        private LineRenderer purpleOuterBeam;
        private LineRenderer purpleCoreBeam;
        private Light purpleLight;
        private float purpleVisualStartedAt;

        private GUIStyle skillStyle;
        private GUIStyle noticeStyle;
        private int styledForHeight = -1;

        private bool BlueCharged => Time.time <= blueChargeUntil;
        private bool RedCharged => Time.time <= redChargeUntil;
        private bool PurpleCooldownReady => Time.time >= nextPurpleAt;
        private bool PurpleReady =>
            BlueCharged
            && RedCharged
            && PurpleCooldownReady
            && techniqueController != null
            && techniqueController.CanUseUltimate;

        private float PurpleCooldownRemaining => Mathf.Max(0f, nextPurpleAt - Time.time);
        private float PurpleCooldownProgress => purpleCooldown <= 0f
            ? 1f
            : Mathf.Clamp01(1f - PurpleCooldownRemaining / purpleCooldown);

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
            techniqueController = GetComponent<GojoTechniqueController>();
            BuildPurpleVisual();
        }

        private void OnEnable()
        {
            if (techniqueController == null)
            {
                techniqueController = GetComponent<GojoTechniqueController>();
            }

            if (techniqueController != null)
            {
                techniqueController.BlueHit -= HandleBlueHit;
                techniqueController.RedHit -= HandleRedHit;
                techniqueController.BlueHit += HandleBlueHit;
                techniqueController.RedHit += HandleRedHit;
            }
        }

        private void OnDisable()
        {
            if (techniqueController != null)
            {
                techniqueController.BlueHit -= HandleBlueHit;
                techniqueController.RedHit -= HandleRedHit;
            }

            if (purpleVisualRoot != null)
            {
                purpleVisualRoot.SetActive(false);
            }
        }

        private void Update()
        {
            RemoveExpiredMarks();
            UpdatePurpleVisual();

            if (Input.GetKeyDown(CombatInputBindings.Ultimate) && PurpleReady)
            {
                ActivatePurple();
            }
        }

        private void HandleBlueHit(Health target)
        {
            if (target == null)
            {
                return;
            }

            blueMarkedUntil[target] = Time.time + blueMarkDuration;
            blueChargeUntil = Time.time + purpleChargeDuration;
        }

        private void HandleRedHit(Health target)
        {
            if (target == null)
            {
                return;
            }

            redChargeUntil = Time.time + purpleChargeDuration;

            if (
                !blueMarkedUntil.TryGetValue(target, out float markedUntil)
                || Time.time > markedUntil
            )
            {
                return;
            }

            blueMarkedUntil.Remove(target);

            if (!target.IsDead)
            {
                target.TakeDamage(chainBonusDamage);
                ApplyHitReaction(target, GetDirectionAwayFromCaster(target), empoweredPushSpeed, empoweredHitStun);
            }

            chainNoticeUntil = Time.time + chainNoticeDuration;
        }

        private void ActivatePurple()
        {
            Vector3 direction = FindPurpleAimDirection();
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

            blueChargeUntil = 0f;
            redChargeUntil = 0f;
            nextPurpleAt = Time.time + purpleCooldown;
            purpleNoticeUntil = Time.time + 1.2f;

            ShowPurpleVisual();
            ApplyPurpleDamage(direction);
        }

        private void ApplyPurpleDamage(Vector3 direction)
        {
            Vector3 start = transform.position + Vector3.up * 1.0f + direction * 0.8f;
            Vector3 end = start + direction * purpleRange;
            Collider[] hits = Physics.OverlapCapsule(start, end, purpleRadius);
            HashSet<Health> affectedTargets = new HashSet<Health>();

            foreach (Collider hit in hits)
            {
                Health target = hit.GetComponentInParent<Health>();
                if (
                    target == null
                    || target == ownHealth
                    || target.IsDead
                    || !affectedTargets.Add(target)
                )
                {
                    continue;
                }

                if (!target.TakeDamage(purpleDamage))
                {
                    continue;
                }

                ApplyHitReaction(target, direction, purplePushSpeed, purpleHitStun);
            }
        }

        private Vector3 FindPurpleAimDirection()
        {
            Health[] healthObjects = FindObjectsByType<Health>(FindObjectsSortMode.None);
            Health nearest = null;
            float nearestDistanceSqr = float.MaxValue;

            foreach (Health health in healthObjects)
            {
                if (health == null || health == ownHealth || health.IsDead)
                {
                    continue;
                }

                Vector3 offset = health.transform.position - transform.position;
                offset.y = 0f;
                float distanceSqr = offset.sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearest = health;
                }
            }

            if (nearest == null)
            {
                return transform.forward;
            }

            Vector3 direction = nearest.transform.position - transform.position;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : transform.forward;
        }

        private Vector3 GetDirectionAwayFromCaster(Health target)
        {
            Vector3 direction = target.transform.position - transform.position;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : transform.forward;
        }

        private static void ApplyHitReaction(
            Health target,
            Vector3 direction,
            float impulseSpeed,
            float hitStun
        )
        {
            Vector3 impulse = direction.normalized * impulseSpeed;
            MonoBehaviour[] behaviours = target.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IHitReactable hitReactable)
                {
                    hitReactable.ApplyHitReaction(impulse, hitStun);
                    break;
                }
            }
        }

        private void RemoveExpiredMarks()
        {
            expiredMarks.Clear();

            foreach (KeyValuePair<Health, float> mark in blueMarkedUntil)
            {
                if (mark.Key == null || mark.Key.IsDead || Time.time > mark.Value)
                {
                    expiredMarks.Add(mark.Key);
                }
            }

            foreach (Health target in expiredMarks)
            {
                blueMarkedUntil.Remove(target);
            }
        }

        private bool HasBlueMarkedTarget()
        {
            foreach (KeyValuePair<Health, float> mark in blueMarkedUntil)
            {
                if (mark.Key != null && !mark.Key.IsDead && Time.time <= mark.Value)
                {
                    return true;
                }
            }

            return false;
        }

        private void BuildPurpleVisual()
        {
            purpleVisualRoot = new GameObject("HollowPurplePrototypeVisual");
            purpleVisualRoot.transform.SetParent(transform, false);
            purpleVisualRoot.transform.localPosition = Vector3.up * 1.05f + Vector3.forward * 0.8f;

            purpleOuterBeam = CreateBeam(
                "PurpleOuterBeam",
                0.95f,
                1.65f,
                new Color(0.62f, 0.10f, 1f, 0.95f)
            );
            purpleCoreBeam = CreateBeam(
                "PurpleCoreBeam",
                0.28f,
                0.55f,
                new Color(0.96f, 0.82f, 1f, 0.98f)
            );

            GameObject lightObject = new GameObject("PurpleLight");
            lightObject.transform.SetParent(purpleVisualRoot.transform, false);
            lightObject.transform.localPosition = Vector3.forward * (purpleRange * 0.45f);
            purpleLight = lightObject.AddComponent<Light>();
            purpleLight.type = LightType.Point;
            purpleLight.color = new Color(0.55f, 0.10f, 1f);
            purpleLight.range = 10f;
            purpleLight.intensity = 6f;
            purpleLight.shadows = LightShadows.None;

            purpleVisualRoot.SetActive(false);
        }

        private LineRenderer CreateBeam(
            string objectName,
            float startWidth,
            float endWidth,
            Color color
        )
        {
            GameObject beamObject = new GameObject(objectName);
            beamObject.transform.SetParent(purpleVisualRoot.transform, false);

            LineRenderer line = beamObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, Vector3.forward * purpleRange);
            line.startWidth = startWidth;
            line.endWidth = endWidth;
            line.startColor = color;
            line.endColor = color;
            line.numCapVertices = 8;
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

            return line;
        }

        private void ShowPurpleVisual()
        {
            purpleVisualStartedAt = Time.time;
            SetBeamColor(purpleOuterBeam, new Color(0.62f, 0.10f, 1f, 0.95f));
            SetBeamColor(purpleCoreBeam, new Color(0.96f, 0.82f, 1f, 0.98f));
            purpleOuterBeam.startWidth = 0.95f;
            purpleOuterBeam.endWidth = 1.65f;
            purpleCoreBeam.startWidth = 0.28f;
            purpleCoreBeam.endWidth = 0.55f;
            purpleLight.intensity = 6f;
            purpleVisualRoot.SetActive(true);
        }

        private void UpdatePurpleVisual()
        {
            if (purpleVisualRoot == null || !purpleVisualRoot.activeSelf)
            {
                return;
            }

            float elapsed = Time.time - purpleVisualStartedAt;
            float normalized = Mathf.Clamp01(elapsed / purpleVisualDuration);
            float pulse = 1f + Mathf.Sin(elapsed * 28f) * 0.12f;
            float alpha = 1f - normalized;

            purpleOuterBeam.startWidth = Mathf.Lerp(0.95f, 1.7f, normalized) * pulse;
            purpleOuterBeam.endWidth = Mathf.Lerp(1.65f, 2.6f, normalized) * pulse;
            purpleCoreBeam.startWidth = Mathf.Lerp(0.28f, 0.08f, normalized);
            purpleCoreBeam.endWidth = Mathf.Lerp(0.55f, 0.14f, normalized);

            SetBeamColor(
                purpleOuterBeam,
                new Color(0.62f, 0.10f, 1f, 0.95f * alpha)
            );
            SetBeamColor(
                purpleCoreBeam,
                new Color(0.96f, 0.82f, 1f, 0.98f * alpha)
            );
            purpleLight.intensity = Mathf.Lerp(6f, 0f, normalized);

            if (elapsed >= purpleVisualDuration)
            {
                purpleVisualRoot.SetActive(false);
            }
        }

        private static void SetBeamColor(LineRenderer line, Color color)
        {
            if (line == null)
            {
                return;
            }

            line.startColor = color;
            line.endColor = color;
        }

        private void OnGUI()
        {
            if (ownHealth == null || ownHealth.IsDead)
            {
                return;
            }

            EnsureStyles();
            DrawPurpleSkillPanel();

            if (Time.time <= purpleNoticeUntil)
            {
                DrawCenterNotice(
                    "HOLLOW PURPLE · 허식 「자」",
                    new Color(0.74f, 0.26f, 1f, 0.98f)
                );
                return;
            }

            if (Time.time <= chainNoticeUntil)
            {
                DrawCenterNotice(
                    "BLUE → RED CHAIN  ·  BONUS +12",
                    new Color(0.82f, 0.28f, 1f, 0.98f)
                );
                return;
            }

            if (HasBlueMarkedTarget())
            {
                float width = Mathf.Min(350f, Screen.width - 48f);
                Rect primedRect = new Rect(24f, 283f, width, 34f);
                DrawRect(primedRect, new Color(0.09f, 0.025f, 0.13f, 0.92f));
                DrawBorder(primedRect, new Color(0.72f, 0.30f, 1f, 0.96f), 2f);
                GUI.Label(
                    primedRect,
                    $"BLUE MARK · {CombatInputBindings.Skill2Label}로 혁 연계",
                    skillStyle
                );
            }
        }

        private void DrawPurpleSkillPanel()
        {
            float width = Mathf.Min(350f, Screen.width - 48f);
            Rect panel = new Rect(24f, 239f, width, 38f);
            bool usable = PurpleReady;
            Color accent = usable
                ? new Color(0.76f, 0.28f, 1f, 0.98f)
                : new Color(0.42f, 0.34f, 0.52f, 0.96f);

            DrawRect(panel, new Color(0.055f, 0.012f, 0.085f, 0.93f));
            Rect fill = new Rect(
                panel.x + 2f,
                panel.y + 2f,
                (panel.width - 4f) * PurpleCooldownProgress,
                panel.height - 4f
            );
            DrawRect(fill, new Color(0.38f, 0.06f, 0.58f, 0.72f));
            DrawBorder(panel, accent, 2f);
            GUI.Label(panel, BuildPurpleStatusText(), skillStyle);
        }

        private string BuildPurpleStatusText()
        {
            string skillName = $"{CombatInputBindings.UltimateLabel} · 허식 「자」";

            if (techniqueController == null || !techniqueController.CanUseUltimate)
            {
                return skillName + "  사용 불가";
            }

            if (!PurpleCooldownReady)
            {
                return $"{skillName}  {PurpleCooldownRemaining:0.0}s";
            }

            if (PurpleReady)
            {
                return skillName + "  READY";
            }

            return $"{skillName}  창 {(BlueCharged ? 1 : 0)}/1 · 혁 {(RedCharged ? 1 : 0)}/1";
        }

        private void DrawCenterNotice(string text, Color accent)
        {
            float width = Mathf.Min(560f, Screen.width - 48f);
            Rect noticeRect = new Rect(
                (Screen.width - width) * 0.5f,
                Screen.height * 0.22f,
                width,
                68f
            );
            DrawRect(noticeRect, new Color(0.11f, 0.008f, 0.16f, 0.95f));
            DrawBorder(noticeRect, accent, 3f);
            noticeStyle.normal.textColor = accent;
            GUI.Label(noticeRect, text, noticeStyle);
        }

        private void EnsureStyles()
        {
            if (styledForHeight == Screen.height)
            {
                return;
            }

            styledForHeight = Screen.height;
            int baseSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 48f, 14f, 20f));

            skillStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = baseSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            skillStyle.normal.textColor = new Color(0.94f, 0.86f, 1f);

            noticeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(22, baseSize + 8),
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
            Vector3 direction = transform.forward;
            Vector3 start = transform.position + Vector3.up * 1.0f + direction * 0.8f;
            Vector3 end = start + direction * purpleRange;
            Gizmos.DrawWireSphere(start, purpleRadius);
            Gizmos.DrawWireSphere(end, purpleRadius);
            Gizmos.DrawLine(start, end);
        }
    }
}
