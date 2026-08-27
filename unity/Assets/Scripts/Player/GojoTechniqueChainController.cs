using System.Collections;
using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(GojoTechniqueController))]
    [RequireComponent(typeof(TargetLockController))]
    public sealed class GojoTechniqueChainController : MonoBehaviour
    {
        [Header("Blue To Red Hit Chain")]
        [SerializeField, Min(0.1f)] private float blueMarkDuration = 2.2f;
        [SerializeField, Min(0f)] private float chainBonusDamage = 12f;
        [SerializeField, Min(0f)] private float empoweredPushSpeed = 28f;
        [SerializeField, Min(0f)] private float empoweredHitStun = 0.72f;
        [SerializeField, Min(0.1f)] private float chainNoticeDuration = 1.15f;

        [Header("Hollow Purple")]
        [SerializeField, Min(0.1f)] private float purplePreparationDuration = 8f;
        [SerializeField, Min(0.1f)] private float purpleCooldown = 10f;
        [SerializeField, Min(0.1f)] private float purpleRange = 18f;
        [SerializeField, Min(0.1f)] private float purpleRadius = 2.2f;
        [SerializeField, Min(0f)] private float purpleDamage = 55f;
        [SerializeField, Min(0f)] private float purplePushSpeed = 34f;
        [SerializeField, Min(0f)] private float purpleHitStun = 1f;
        [SerializeField, Min(0.1f)] private float purpleVisualDuration = 0.85f;
        [SerializeField, Min(0f)] private float purpleEnergyCost = 45f;

        [Header("Hollow Purple · Presentation / Damage Sync")]
        [SerializeField, Min(0f)] private float purplePresentationStartSlack = 0.08f;
        [SerializeField, Min(0f)] private float purpleMergeDuration = 0.24f;
        [SerializeField, Min(0.01f)] private float purpleLaunchDuration = 0.78f;

        private sealed class PendingPurpleHit
        {
            public Health Target;
            public float ImpactAt;
        }

        private readonly Dictionary<Health, float> blueMarkedUntil =
            new Dictionary<Health, float>();
        private readonly List<Health> expiredMarks = new List<Health>();

        private Health ownHealth;
        private GojoTechniqueController techniqueController;
        private TargetLockController targetLock;
        private CursedEnergyController cursedEnergy;
        private TechniqueBurnoutController burnout;
        private bool blueWasReady;
        private bool redWasReady;
        private float bluePreparedUntil;
        private float redPreparedUntil;
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

        private bool BluePrepared => Time.time <= bluePreparedUntil;
        private bool RedPrepared => Time.time <= redPreparedUntil;
        private bool PurpleCooldownReady => Time.time >= nextPurpleAt;
        private bool PurpleBaseReady =>
            BluePrepared
            && RedPrepared
            && PurpleCooldownReady
            && techniqueController != null
            && techniqueController.CanUseUltimate;
        private bool PurpleReady =>
            PurpleBaseReady
            && (cursedEnergy == null || cursedEnergy.CanSpend(purpleEnergyCost));
        private float PurpleCooldownRemaining => Mathf.Max(0f, nextPurpleAt - Time.time);
        private float PurpleCooldownProgress => purpleCooldown <= 0f
            ? 1f
            : Mathf.Clamp01(1f - PurpleCooldownRemaining / purpleCooldown);

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
            techniqueController = GetComponent<GojoTechniqueController>();
            targetLock = GetComponent<TargetLockController>();
            cursedEnergy = CursedEnergyController.GetOrCreate(gameObject);
            cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SixEyesEfficiency);
            burnout = TechniqueBurnoutController.GetOrCreate(gameObject);
            blueWasReady = techniqueController != null && techniqueController.BlueReady;
            redWasReady = techniqueController != null && techniqueController.RedReady;
            BuildPurpleVisual();
        }

        private void OnEnable()
        {
            techniqueController ??= GetComponent<GojoTechniqueController>();
            targetLock ??= GetComponent<TargetLockController>();
            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SixEyesEfficiency);
            burnout ??= TechniqueBurnoutController.GetOrCreate(gameObject);

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
            DetectTechniqueUses();
            UpdatePurpleVisual();

            if (!Input.GetKeyDown(CombatInputBindings.Ultimate))
            {
                return;
            }

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SixEyesEfficiency);
            if (PurpleReady)
            {
                ActivatePurple();
            }
            else if (
                PurpleBaseReady
                && cursedEnergy != null
                && !cursedEnergy.CanSpend(purpleEnergyCost)
            )
            {
                cursedEnergy.NotifyInsufficient("허식 자", purpleEnergyCost);
            }
        }

        private void DetectTechniqueUses()
        {
            if (techniqueController == null)
            {
                return;
            }

            bool blueReadyNow = techniqueController.BlueReady;
            bool redReadyNow = techniqueController.RedReady;
            if (blueWasReady && !blueReadyNow)
            {
                bluePreparedUntil = Time.time + purplePreparationDuration;
            }
            if (redWasReady && !redReadyNow)
            {
                redPreparedUntil = Time.time + purplePreparationDuration;
            }

            blueWasReady = blueReadyNow;
            redWasReady = redReadyNow;
        }

        private void HandleBlueHit(Health target)
        {
            if (target != null)
            {
                blueMarkedUntil[target] = Time.time + blueMarkDuration;
            }
        }

        private void HandleRedHit(Health target)
        {
            if (
                target == null
                || !blueMarkedUntil.TryGetValue(target, out float markedUntil)
                || Time.time > markedUntil
            )
            {
                return;
            }

            blueMarkedUntil.Remove(target);
            if (target.IsDead)
            {
                return;
            }

            DamageContext chainDamage = new DamageContext(
                chainBonusDamage,
                gameObject,
                DamageDeliveryType.CursedTechnique,
                DamageTraits.None,
                "BLUE → RED CHAIN",
                target.transform.position + Vector3.up * 0.8f
            );
            if (target.ReceiveDamage(chainDamage) != DamageResolution.Applied)
            {
                return;
            }

            ApplyHitReaction(
                target,
                DirectionAwayFromCaster(target),
                empoweredPushSpeed,
                empoweredHitStun
            );
            chainNoticeUntil = Time.time + chainNoticeDuration;
        }

        private void ActivatePurple()
        {
            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SixEyesEfficiency);
            if (
                cursedEnergy != null
                && !cursedEnergy.TrySpend(purpleEnergyCost, "허식 자")
            )
            {
                return;
            }

            Vector3 direction = FindPurpleAimDirection();
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            bluePreparedUntil = 0f;
            redPreparedUntil = 0f;
            nextPurpleAt = Time.time + purpleCooldown;
            purpleNoticeUntil = Time.time + 1.2f;
            ShowPurpleVisual();
            QueuePurpleDamage(direction);
        }

        private void QueuePurpleDamage(Vector3 direction)
        {
            Vector3 start = transform.position + Vector3.up * 1.0f + direction * 0.8f;
            Vector3 end = start + direction * purpleRange;
            Collider[] hits = Physics.OverlapCapsule(start, end, purpleRadius);
            HashSet<Health> affected = new HashSet<Health>();
            List<PendingPurpleHit> pendingHits = new List<PendingPurpleHit>();
            float sequenceStartedAt = Time.unscaledTime;

            foreach (Collider hit in hits)
            {
                Health target = hit.GetComponentInParent<Health>();
                if (
                    target == null
                    || target == ownHealth
                    || target.IsDead
                    || !affected.Add(target)
                )
                {
                    continue;
                }

                Vector3 offset = target.transform.position - start;
                float forwardDistance = Mathf.Clamp(
                    Vector3.Dot(offset, direction),
                    0f,
                    purpleRange
                );
                float travelProgress = purpleRange > 0f
                    ? forwardDistance / purpleRange
                    : 0f;

                pendingHits.Add(
                    new PendingPurpleHit
                    {
                        Target = target,
                        ImpactAt =
                            sequenceStartedAt
                            + purplePresentationStartSlack
                            + purpleMergeDuration
                            + purpleLaunchDuration * travelProgress,
                    }
                );
            }

            if (pendingHits.Count > 0)
            {
                StartCoroutine(ResolvePurpleHits(pendingHits, direction));
            }
        }

        private IEnumerator ResolvePurpleHits(
            List<PendingPurpleHit> pendingHits,
            Vector3 direction
        )
        {
            while (pendingHits.Count > 0)
            {
                float now = Time.unscaledTime;
                for (int index = pendingHits.Count - 1; index >= 0; index--)
                {
                    PendingPurpleHit pending = pendingHits[index];
                    if (pending == null || now < pending.ImpactAt)
                    {
                        continue;
                    }

                    pendingHits.RemoveAt(index);
                    Health target = pending.Target;
                    if (target == null || target.IsDead)
                    {
                        continue;
                    }

                    DamageContext purpleContext = new DamageContext(
                        purpleDamage,
                        gameObject,
                        DamageDeliveryType.CursedTechnique,
                        DamageTraits.None,
                        "HOLLOW PURPLE · 허식 「자」",
                        target.transform.position + Vector3.up * 0.8f
                    );
                    if (target.ReceiveDamage(purpleContext) != DamageResolution.Applied)
                    {
                        continue;
                    }

                    ApplyHitReaction(target, direction, purplePushSpeed, purpleHitStun);
                }

                if (pendingHits.Count > 0)
                {
                    yield return null;
                }
            }
        }

        private Vector3 FindPurpleAimDirection()
        {
            if (
                targetLock != null
                && targetLock.TryGetAimDirection(out Vector3 lockedDirection)
            )
            {
                return lockedDirection;
            }

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
                if (offset.sqrMagnitude < nearestDistanceSqr)
                {
                    nearestDistanceSqr = offset.sqrMagnitude;
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

        private Vector3 DirectionAwayFromCaster(Health target)
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
            purpleVisualRoot.transform.localPosition =
                Vector3.up * 1.05f + Vector3.forward * 0.8f;
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
                line.material = new Material(shader) { color = color };
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
            if (CombatHudPresentationMode.ProductionCanvasActive)
            {
                return;
            }

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
                    new Color(0.74f, 0.26f, 1f)
                );
                return;
            }
            if (Time.time <= chainNoticeUntil)
            {
                DrawCenterNotice(
                    "BLUE → RED · BONUS +12",
                    new Color(0.82f, 0.28f, 1f)
                );
                return;
            }
            if (HasBlueMarkedTarget())
            {
                float width = Mathf.Min(330f, Screen.width - 24f);
                Rect rect = new Rect(12f, 203f, width, 25f);
                DrawRect(rect, new Color(0.09f, 0.025f, 0.13f, 0.88f));
                DrawBorder(rect, new Color(0.72f, 0.30f, 1f), 1f);
                GUI.Label(rect, "BLUE MARK · E로 혁 연계", skillStyle);
            }
        }

        private void DrawPurpleSkillPanel()
        {
            float width = Mathf.Min(330f, Screen.width - 24f);
            Rect panel = new Rect(12f, 170f, width, 29f);
            Color accent = PurpleReady
                ? new Color(0.76f, 0.28f, 1f)
                : new Color(0.42f, 0.34f, 0.52f);
            DrawRect(panel, new Color(0.055f, 0.012f, 0.085f, 0.88f));
            DrawRect(
                new Rect(
                    panel.x + 2f,
                    panel.y + 2f,
                    (panel.width - 4f) * PurpleCooldownProgress,
                    panel.height - 4f
                ),
                new Color(0.38f, 0.06f, 0.58f, 0.66f)
            );
            DrawBorder(panel, accent, 1f);
            GUI.Label(panel, BuildPurpleStatusText(), skillStyle);
        }

        private string BuildPurpleStatusText()
        {
            string skillName = "R · 허식 「자」";
            float actualCost = cursedEnergy != null
                ? cursedEnergy.ResolveCost(purpleEnergyCost)
                : purpleEnergyCost;

            if (burnout != null && burnout.IsBurnedOut)
            {
                return $"{skillName} · 번아웃 {burnout.Remaining:0.0}s";
            }
            if (techniqueController == null || !techniqueController.CanUseUltimate)
            {
                return skillName + " · 사용 불가";
            }
            if (!PurpleCooldownReady)
            {
                return $"{skillName} · {PurpleCooldownRemaining:0.0}s";
            }
            if (!BluePrepared || !RedPrepared)
            {
                return $"{skillName} · 창 {(BluePrepared ? 1 : 0)}/1 · 혁 {(RedPrepared ? 1 : 0)}/1";
            }
            if (cursedEnergy != null && !cursedEnergy.CanSpend(purpleEnergyCost))
            {
                return $"{skillName} · 주력 부족 {actualCost:0}";
            }

            return $"{skillName} · READY · CE {actualCost:0}";
        }

        private void DrawCenterNotice(string text, Color accent)
        {
            float width = Mathf.Min(440f, Screen.width - 24f);
            Rect rect = new Rect(
                (Screen.width - width) * 0.5f,
                Screen.height * 0.20f,
                width,
                48f
            );
            DrawRect(rect, new Color(0.11f, 0.008f, 0.16f, 0.92f));
            DrawBorder(rect, accent, 2f);
            noticeStyle.normal.textColor = accent;
            GUI.Label(rect, text, noticeStyle);
        }

        private void EnsureStyles()
        {
            if (styledForHeight == Screen.height)
            {
                return;
            }

            styledForHeight = Screen.height;
            int baseSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 68f, 11f, 15f));
            skillStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = baseSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            skillStyle.normal.textColor = new Color(0.94f, 0.86f, 1f);
            noticeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(19, baseSize + 7),
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
