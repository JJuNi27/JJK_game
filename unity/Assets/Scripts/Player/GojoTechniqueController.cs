using System;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(TargetLockController))]
    public sealed class GojoTechniqueController : MonoBehaviour
    {
        private enum CastState
        {
            None,
            Blue,
            Red,
        }

        [Header("Cursed Technique Lapse: Blue")]
        [SerializeField, Min(0.01f)] private float blueCastTime = 0.24f;
        [SerializeField, Min(0.1f)] private float blueCastDistance = 10f;
        [SerializeField, Min(0.1f)] private float blueRadius = 4.5f;
        [SerializeField, Min(0.1f)] private float blueFieldDuration = 0.95f;
        [SerializeField, Min(0.03f)] private float bluePulseInterval = 0.10f;
        [SerializeField, Min(0f)] private float blueDamage = 8f;
        [SerializeField, Min(0f)] private float bluePullSpeed = 16f;
        [SerializeField, Min(0f)] private float blueHitStun = 0.42f;
        [SerializeField, Min(0.1f)] private float blueCooldown = 3.2f;
        [SerializeField, Min(0f)] private float blueEnergyCost = 16f;
        [SerializeField, Min(0f)] private float lockedBluePointOffset = 1.8f;

        [Header("Cursed Technique Reversal: Red")]
        [SerializeField, Min(0.01f)] private float redCastTime = 0.30f;
        [SerializeField, Min(0.1f)] private float redRange = 11f;
        [SerializeField, Min(0.1f)] private float redProjectileSpeed = 22f;
        [SerializeField, Min(0.1f)] private float redRadius = 1.7f;
        [SerializeField, Min(0f)] private float redDamage = 18f;
        [SerializeField, Min(0f)] private float redPushSpeed = 23f;
        [SerializeField, Min(0f)] private float redHitStun = 0.52f;
        [SerializeField, Min(0.1f)] private float redCooldown = 4.5f;
        [SerializeField, Min(0f)] private float redEnergyCost = 24f;

        private Health ownHealth;
        private Health[] combatHealth;
        private GojoDomainController domainController;
        private TargetLockController targetLock;
        private PrototypeCombatAudio combatAudio;
        private CursedEnergyController cursedEnergy;
        private CombatActionGate actionGate;
        private TechniqueBurnoutController burnout;
        private CastState castState;
        private float castStartedAt;
        private float castCompletesAt;
        private Vector3 pendingBluePoint;
        private Vector3 pendingRedDirection;
        private float nextBlueAt;
        private float nextRedAt;
        private float nextCombatHealthRefreshAt;
        private GUIStyle skillStyle;
        private int styledForHeight = -1;

        public event Action<Health> BlueHit;
        public event Action<Health> RedHit;

        public bool BlueReady => Time.time >= nextBlueAt;
        public bool RedReady => Time.time >= nextRedAt;
        public bool IsCasting => castState != CastState.None;
        public bool CanUseUltimate => CombatActive && actionGate != null && actionGate.CanStartUltimate;
        public float BlueCooldownRemaining => Mathf.Max(0f, nextBlueAt - Time.time);
        public float RedCooldownRemaining => Mathf.Max(0f, nextRedAt - Time.time);
        public float BlueCooldownProgress => GetCooldownProgress(BlueCooldownRemaining, blueCooldown);
        public float RedCooldownProgress => GetCooldownProgress(RedCooldownRemaining, redCooldown);
        public float CastProgress => !IsCasting
            ? 0f
            : Mathf.Clamp01((Time.time - castStartedAt) / Mathf.Max(0.01f, castCompletesAt - castStartedAt));

        public string BlueStatusText => BuildStatusText("Q · 순전 「창」", BlueReady, BlueCooldownRemaining, CastState.Blue, blueEnergyCost);
        public string RedStatusText => BuildStatusText("E · 반전 「혁」", RedReady, RedCooldownRemaining, CastState.Red, redEnergyCost);

        private bool DomainBusy => domainController != null && domainController.State != GojoDomainController.DomainState.Normal;
        private bool CombatActive => ownHealth != null && !ownHealth.IsDead && HasLivingOpponent();

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
            domainController = GetComponent<GojoDomainController>();
            targetLock = GetComponent<TargetLockController>();
            combatAudio = PrototypeCombatAudio.GetOrCreate(gameObject);
            cursedEnergy = CursedEnergyController.GetOrCreate(gameObject);
            cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SixEyesEfficiency);
            burnout = TechniqueBurnoutController.GetOrCreate(gameObject);
            actionGate = CombatActionGate.GetOrCreate(gameObject);
        }

        private void Start()
        {
            RefreshCombatHealth();
        }

        private void OnDisable()
        {
            CancelCast();
        }

        private void Update()
        {
            if (!CombatActive)
            {
                CancelCast();
                return;
            }

            if (IsCasting)
            {
                if (DomainBusy)
                {
                    CancelCast();
                    return;
                }
                if (Time.time >= castCompletesAt)
                {
                    CompleteCast();
                }
                return;
            }

            if (Input.GetKeyDown(CombatInputBindings.Skill1))
            {
                TryBeginTechnique(CastState.Blue);
            }
            else if (Input.GetKeyDown(CombatInputBindings.Skill2))
            {
                TryBeginTechnique(CastState.Red);
            }
        }

        private void TryBeginTechnique(CastState requestedState)
        {
            bool blue = requestedState == CastState.Blue;
            bool ready = blue ? BlueReady : RedReady;
            float cost = blue ? blueEnergyCost : redEnergyCost;
            string actionName = blue ? "창" : "혁";
            if (!ready || !CombatActive || actionGate == null || !actionGate.CanStartTechnique)
            {
                return;
            }

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SixEyesEfficiency);
            if (cursedEnergy != null && !cursedEnergy.TrySpend(cost, actionName))
            {
                return;
            }

            if (blue)
            {
                pendingBluePoint = ResolveBlueConvergencePoint();
                FaceHorizontalPoint(pendingBluePoint);
                nextBlueAt = Time.time + blueCooldown;
                BeginCast(CastState.Blue, blueCastTime);
                combatAudio?.PlayBlueCast();
            }
            else
            {
                pendingRedDirection = ResolveAimDirection();
                transform.rotation = Quaternion.LookRotation(pendingRedDirection, Vector3.up);
                nextRedAt = Time.time + redCooldown;
                BeginCast(CastState.Red, redCastTime);
                combatAudio?.PlayRedCast();
            }
        }

        private void BeginCast(CastState state, float duration)
        {
            castState = state;
            castStartedAt = Time.time;
            castCompletesAt = Time.time + Mathf.Max(0.01f, duration);
        }

        private void CompleteCast()
        {
            CastState completed = castState;
            CancelCast();
            if (completed == CastState.Blue)
            {
                SpawnBlueField();
            }
            else if (completed == CastState.Red)
            {
                SpawnRedProjectile();
            }
        }

        private void CancelCast()
        {
            castState = CastState.None;
            castStartedAt = 0f;
            castCompletesAt = 0f;
        }

        private void SpawnBlueField()
        {
            GameObject fieldObject = new GameObject("BlueConvergenceField");
            fieldObject.transform.position = pendingBluePoint + Vector3.up * 0.12f;
            BlueConvergenceField field = fieldObject.AddComponent<BlueConvergenceField>();
            field.Configure(
                ownHealth,
                blueRadius,
                blueFieldDuration,
                bluePulseInterval,
                blueDamage,
                bluePullSpeed,
                blueHitStun,
                target => BlueHit?.Invoke(target),
                () => combatAudio?.PlayBlueImpact()
            );
        }

        private void SpawnRedProjectile()
        {
            Vector3 direction = pendingRedDirection.sqrMagnitude > 0.001f ? pendingRedDirection.normalized : transform.forward;
            Vector3 start = transform.position + Vector3.up * 1.0f + direction * 0.9f;
            GameObject projectileObject = new GameObject("RedTechniqueProjectile");
            projectileObject.transform.position = start;
            RedTechniqueProjectile projectile = projectileObject.AddComponent<RedTechniqueProjectile>();
            projectile.Configure(
                ownHealth,
                direction,
                redProjectileSpeed,
                redRange,
                redRadius,
                redDamage,
                redPushSpeed,
                redHitStun,
                target => RedHit?.Invoke(target),
                () => combatAudio?.PlayRedImpact()
            );
        }

        private Vector3 ResolveBlueConvergencePoint()
        {
            Health lockedTarget = targetLock != null ? targetLock.CurrentTarget : null;
            if (lockedTarget != null)
            {
                Vector3 point = lockedTarget.transform.position;
                Vector3 towardCaster = transform.position - point;
                towardCaster.y = 0f;
                if (towardCaster.sqrMagnitude > 0.001f)
                {
                    point += towardCaster.normalized * lockedBluePointOffset;
                }
                return point;
            }
            return transform.position + ResolveAimDirection() * blueCastDistance;
        }

        private Vector3 ResolveAimDirection()
        {
            if (targetLock != null && targetLock.TryGetAimDirection(out Vector3 lockedDirection))
            {
                return lockedDirection;
            }
            Vector3 forward = transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
        }

        private void FaceHorizontalPoint(Vector3 point)
        {
            Vector3 direction = point - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }

        private string BuildStatusText(string skillName, bool ready, float cooldownRemaining, CastState ownCast, float baseCost)
        {
            float cost = cursedEnergy != null ? cursedEnergy.ResolveCost(baseCost) : baseCost;
            if (!CombatActive)
            {
                return skillName + " · 종료";
            }
            if (burnout != null && burnout.IsBurnedOut)
            {
                return $"{skillName} · 번아웃 {burnout.Remaining:0.0}s";
            }
            if (DomainBusy)
            {
                return skillName + " · 영역 입력 중";
            }
            if (castState == ownCast)
            {
                return $"{skillName} · 시전 {CastProgress * 100f:0}%";
            }
            if (IsCasting)
            {
                return skillName + " · 다른 술식 시전 중";
            }
            if (!ready)
            {
                return $"{skillName} · {cooldownRemaining:0.0}s";
            }
            if (cursedEnergy != null && !cursedEnergy.CanSpend(baseCost))
            {
                return $"{skillName} · 주력 부족 {cost:0}";
            }
            return $"{skillName} · READY · CE {cost:0}";
        }

        private bool HasLivingOpponent()
        {
            if (combatHealth == null || combatHealth.Length == 0 || Time.time >= nextCombatHealthRefreshAt)
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
            nextCombatHealthRefreshAt = Time.time + 0.5f;
        }

        private static float GetCooldownProgress(float remaining, float cooldown)
        {
            return cooldown <= 0f ? 1f : Mathf.Clamp01(1f - remaining / cooldown);
        }

        private void OnGUI()
        {
            if (ownHealth == null || ownHealth.IsDead)
            {
                return;
            }

            EnsureStyle();
            float width = Mathf.Min(330f, Screen.width - 24f);
            DrawSkillPanel(
                new Rect(12f, 104f, width, 29f),
                BlueStatusText,
                BlueCooldownProgress,
                BlueReady && (cursedEnergy == null || cursedEnergy.CanSpend(blueEnergyCost)),
                castState == CastState.Blue,
                new Color(0.12f, 0.72f, 1f),
                new Color(0.05f, 0.34f, 0.68f, 0.66f),
                new Color(0.012f, 0.035f, 0.075f, 0.88f)
            );
            DrawSkillPanel(
                new Rect(12f, 137f, width, 29f),
                RedStatusText,
                RedCooldownProgress,
                RedReady && (cursedEnergy == null || cursedEnergy.CanSpend(redEnergyCost)),
                castState == CastState.Red,
                new Color(1f, 0.22f, 0.18f),
                new Color(0.70f, 0.08f, 0.06f, 0.66f),
                new Color(0.075f, 0.012f, 0.018f, 0.88f)
            );
        }

        private void DrawSkillPanel(Rect panel, string text, float cooldownProgress, bool ready, bool casting, Color readyAccent, Color fillColor, Color background)
        {
            bool available = ready && CombatActive && actionGate != null && actionGate.CanStartTechnique;
            Color accent = casting ? Color.white : available ? readyAccent : new Color(0.34f, 0.40f, 0.50f);
            DrawRect(panel, background);
            float progress = casting ? CastProgress : cooldownProgress;
            DrawRect(new Rect(panel.x + 2f, panel.y + 2f, (panel.width - 4f) * progress, panel.height - 4f), fillColor);
            DrawBorder(panel, accent, casting ? 2f : 1f);
            skillStyle.normal.textColor = Color.white;
            GUI.Label(panel, text, skillStyle);
        }

        private void EnsureStyle()
        {
            if (styledForHeight == Screen.height)
            {
                return;
            }
            styledForHeight = Screen.height;
            skillStyle = new GUIStyle(GUI.skin.label)
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
