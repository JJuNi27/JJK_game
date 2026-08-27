using System.Collections;
using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(TargetLockController))]
    public sealed class SukunaTechniqueController : MonoBehaviour
    {
        [Header("Q · 해")]
        [SerializeField, Min(0f)] private float dismantleEnergyCost = 15f;
        [SerializeField, Min(0.1f)] private float dismantleCooldown = 3f;
        [SerializeField, Min(0.1f)] private float dismantleRange = 18f;
        [SerializeField, Range(20f, 180f)] private float dismantleMultiAngle = 130f;
        [SerializeField, Min(1)] private int dismantleSingleHits = 4;
        [SerializeField, Min(1)] private int dismantleMultiHits = 2;
        [SerializeField, Min(0f)] private float dismantleSingleDamagePerHit = 8f;
        [SerializeField, Min(0f)] private float dismantleMultiDamagePerHit = 10f;
        [SerializeField, Min(0.01f)] private float dismantleHitInterval = 0.075f;

        [Header("E · 팔")]
        [SerializeField, Min(0f)] private float cleaveEnergyCost = 45f;
        [SerializeField, Min(0.1f)] private float cleaveCooldown = 5f;
        [SerializeField, Min(0.1f)] private float cleaveRange = 3.4f;
        [SerializeField, Min(1)] private int cleaveHits = 7;
        [SerializeField, Min(0f)] private float cleaveDamagePerHit = 8f;
        [SerializeField, Min(0.01f)] private float cleaveHitInterval = 0.055f;
        [SerializeField, Min(0f)] private float cleaveFinisherKnockback = 12f;
        [SerializeField, Min(0f)] private float cleaveFinisherStun = 0.48f;

        [Header("R · 푸가 · Milestone 2")]
        [SerializeField, Min(0f)] private float fugaEnergyCost = 35f;
        [SerializeField, Min(0.1f)] private float fugaCooldown = 11f;
        [SerializeField, Min(0.01f)] private float fugaCastTime = 0.52f;
        [SerializeField, Min(0.1f)] private float fugaRange = 24f;
        [SerializeField, Min(0.1f)] private float fugaProjectileSpeed = 18f;
        [SerializeField, Min(0.1f)] private float fugaProjectileRadius = 0.65f;
        [SerializeField, Min(0.1f)] private float fugaExplosionRadius = 4.5f;
        [SerializeField, Min(0f)] private float fugaDamage = 78f;
        [SerializeField, Min(0f)] private float fugaKnockback = 24f;
        [SerializeField, Min(0f)] private float fugaHitStun = 1.15f;

        [Header("R · 푸가 · 복마어주자 증폭")]
        [SerializeField, Min(1f)] private float domainFugaProjectileSpeedMultiplier = 1.55f;

        private Health ownHealth;
        private TargetLockController targetLock;
        private CursedEnergyController cursedEnergy;
        private CombatActionGate actionGate;
        private PrototypeCombatAudio combatAudio;
        private SukunaDomainController sukunaDomain;
        private float nextDismantleAt;
        private float nextCleaveAt;
        private float nextFugaAt;
        private bool isCasting;
        private bool isFugaCasting;
        private GUIStyle skillStyle;
        private int styledForHeight = -1;

        public bool IsCasting => isCasting;
        public bool DismantleUsed { get; private set; }
        public bool CleaveUsed { get; private set; }
        public bool FugaPrepared => DismantleUsed && CleaveUsed;
        public float DismantleCooldownRemaining => Mathf.Max(0f, nextDismantleAt - Time.time);
        public float CleaveCooldownRemaining => Mathf.Max(0f, nextCleaveAt - Time.time);
        public float FugaCooldownRemaining => Mathf.Max(0f, nextFugaAt - Time.time);

        private bool CombatActive =>
            ownHealth != null
            && !ownHealth.IsDead
            && FindLivingEnemies().Count > 0;

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
            targetLock = GetComponent<TargetLockController>();
            cursedEnergy = CursedEnergyController.GetOrCreate(gameObject);
            cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SukunaShibuyaReserve);
            actionGate = CombatActionGate.GetOrCreate(gameObject);
            combatAudio = PrototypeCombatAudio.GetOrCreate(gameObject);
            sukunaDomain = GetComponent<SukunaDomainController>();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            isCasting = false;
            isFugaCasting = false;
        }

        private void Update()
        {
            if (!CombatActive || isCasting)
            {
                return;
            }

            if (Input.GetKeyDown(CombatInputBindings.Skill1))
            {
                TryUseDismantle();
            }
            else if (Input.GetKeyDown(CombatInputBindings.Skill2))
            {
                TryUseCleave();
            }
            else if (Input.GetKeyDown(CombatInputBindings.Ultimate))
            {
                TryUseFuga();
            }
        }

        private void TryUseDismantle()
        {
            actionGate ??= CombatActionGate.GetOrCreate(gameObject);
            if (
                Time.time < nextDismantleAt
                || actionGate == null
                || !actionGate.CanStartTechnique
            )
            {
                return;
            }

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SukunaShibuyaReserve, false);
            if (cursedEnergy != null && !cursedEnergy.TrySpend(dismantleEnergyCost, "해"))
            {
                return;
            }

            List<Health> enemies = FindLivingEnemies();
            bool multiTargetMode = enemies.Count >= 2;
            List<Health> targets = multiTargetMode
                ? FindDismantleConeTargets(enemies)
                : FindSingleDismantleTarget(enemies);

            Vector3 aimDirection = ResolveAimDirection(targets);
            FaceDirection(aimDirection);
            nextDismantleAt = Time.time + dismantleCooldown;
            DismantleUsed = true;

            int hitCount = multiTargetMode ? dismantleMultiHits : dismantleSingleHits;
            float damagePerHit = multiTargetMode
                ? dismantleMultiDamagePerHit
                : dismantleSingleDamagePerHit;
            StartCoroutine(
                ExecuteSlashSequence(
                    targets,
                    hitCount,
                    damagePerHit,
                    dismantleHitInterval,
                    multiTargetMode ? "해 · 다중 참격" : "해 · 집중 참격",
                    new Color(0.90f, 0.18f, 0.16f),
                    0f,
                    0f,
                    dismantleRange
                )
            );
        }

        private void TryUseCleave()
        {
            actionGate ??= CombatActionGate.GetOrCreate(gameObject);
            if (
                Time.time < nextCleaveAt
                || actionGate == null
                || !actionGate.CanStartTechnique
            )
            {
                return;
            }

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SukunaShibuyaReserve, false);
            if (cursedEnergy != null && !cursedEnergy.TrySpend(cleaveEnergyCost, "팔"))
            {
                return;
            }

            Health target = FindCleaveTarget();
            List<Health> targets = new List<Health>();
            if (target != null)
            {
                targets.Add(target);
                Vector3 direction = target.transform.position - transform.position;
                direction.y = 0f;
                FaceDirection(direction);
            }

            nextCleaveAt = Time.time + cleaveCooldown;
            CleaveUsed = true;
            StartCoroutine(
                ExecuteSlashSequence(
                    targets,
                    cleaveHits,
                    cleaveDamagePerHit,
                    cleaveHitInterval,
                    "팔",
                    new Color(1f, 0.48f, 0.10f),
                    cleaveFinisherKnockback,
                    cleaveFinisherStun,
                    cleaveRange
                )
            );
        }

        private void TryUseFuga()
        {
            sukunaDomain ??= GetComponent<SukunaDomainController>();
            if (sukunaDomain != null && sukunaDomain.enabled && sukunaDomain.IsActive)
            {
                return;
            }

            actionGate ??= CombatActionGate.GetOrCreate(gameObject);
            if (
                !FugaPrepared
                || Time.time < nextFugaAt
                || actionGate == null
                || !actionGate.CanStartUltimate
            )
            {
                return;
            }

            List<Health> enemies = FindLivingEnemies();
            if (enemies.Count != 1)
            {
                return;
            }

            Health target = enemies[0];
            if (
                target == null
                || target.IsDead
                || Vector3.Distance(transform.position, target.transform.position) > fugaRange
            )
            {
                return;
            }

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SukunaShibuyaReserve, false);
            if (cursedEnergy != null && !cursedEnergy.TrySpend(fugaEnergyCost, "푸가"))
            {
                return;
            }

            Vector3 direction = target.transform.position - transform.position;
            direction.y = 0f;
            FaceDirection(direction);

            nextFugaAt = Time.time + fugaCooldown;
            DismantleUsed = false;
            CleaveUsed = false;
            StartCoroutine(ExecuteFuga(target, direction, false, Vector3.zero, 0f));
        }

        public bool TryUseFugaInsideDomain(Vector3 activeDomainCenter, float activeDomainRadius)
        {
            sukunaDomain ??= GetComponent<SukunaDomainController>();
            if (
                !isActiveAndEnabled
                || isCasting
                || !FugaPrepared
                || Time.time < nextFugaAt
                || sukunaDomain == null
                || !sukunaDomain.enabled
                || !sukunaDomain.IsActive
            )
            {
                return false;
            }

            List<Health> targets = FindEnemiesInsideRadius(activeDomainCenter, activeDomainRadius);
            if (targets.Count == 0)
            {
                return false;
            }

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SukunaShibuyaReserve, false);
            if (cursedEnergy != null && !cursedEnergy.TrySpend(fugaEnergyCost, "푸가"))
            {
                return false;
            }

            Health target = ResolvePreferredDomainFugaTarget(targets, activeDomainCenter, activeDomainRadius);
            if (target == null)
            {
                return false;
            }

            Vector3 direction = target.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = transform.forward;
            }
            FaceDirection(direction);

            nextFugaAt = Time.time + fugaCooldown;
            DismantleUsed = false;
            CleaveUsed = false;
            StartCoroutine(
                ExecuteFuga(
                    target,
                    direction,
                    true,
                    activeDomainCenter,
                    activeDomainRadius
                )
            );
            return true;
        }

        private IEnumerator ExecuteSlashSequence(
            List<Health> targets,
            int hitCount,
            float damagePerHit,
            float interval,
            string actionName,
            Color visualColor,
            float finalKnockback,
            float finalStun,
            float visualLength
        )
        {
            isCasting = true;
            combatAudio ??= PrototypeCombatAudio.GetOrCreate(gameObject);

            for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
            {
                bool finalHit = hitIndex == hitCount - 1;
                if (hitIndex == 0 || finalHit)
                {
                    combatAudio?.PlayBasicSwing(finalHit ? 3 : 1);
                }

                SpawnSlashVisual(transform.forward, visualLength, visualColor, hitIndex);

                foreach (Health target in targets)
                {
                    if (target == null || target.IsDead)
                    {
                        continue;
                    }

                    Vector3 direction = target.transform.position - transform.position;
                    direction.y = 0f;
                    if (direction.sqrMagnitude <= 0.001f)
                    {
                        direction = transform.forward;
                    }

                    DamageContext context = new DamageContext(
                        damagePerHit,
                        gameObject,
                        DamageDeliveryType.CursedTechnique,
                        DamageTraits.None,
                        actionName,
                        target.transform.position + Vector3.up * 0.8f
                    );
                    if (target.ReceiveDamage(context) != DamageResolution.Applied)
                    {
                        continue;
                    }

                    if (finalHit)
                    {
                        combatAudio?.PlayBasicHit(3);
                        if (finalKnockback > 0f || finalStun > 0f)
                        {
                            ApplyHitReaction(
                                target,
                                direction.normalized * finalKnockback,
                                finalStun
                            );
                        }
                    }
                }

                if (!finalHit)
                {
                    yield return new WaitForSeconds(interval);
                }
            }

            isCasting = false;
        }

        private IEnumerator ExecuteFuga(
            Health target,
            Vector3 initialDirection,
            bool domainAmplified,
            Vector3 activeDomainCenter,
            float activeDomainRadius
        )
        {
            isCasting = true;
            isFugaCasting = true;
            combatAudio ??= PrototypeCombatAudio.GetOrCreate(gameObject);
            combatAudio?.PlayBasicSwing(3);

            yield return new WaitForSeconds(fugaCastTime);

            if (ownHealth == null || ownHealth.IsDead)
            {
                isCasting = false;
                isFugaCasting = false;
                yield break;
            }

            Vector3 direction = initialDirection;
            if (target != null && !target.IsDead)
            {
                direction = target.transform.position - transform.position;
                direction.y = 0f;
            }
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = transform.forward;
            }
            direction.Normalize();
            FaceDirection(direction);

            Vector3 spawnPosition = transform.position + Vector3.up * 1.0f + direction * 0.95f;
            GameObject projectileObject = new GameObject(
                domainAmplified ? "SukunaDomainFugaProjectile" : "SukunaFugaProjectile"
            );
            projectileObject.transform.position = spawnPosition;
            SukunaFugaProjectile projectile = projectileObject.AddComponent<SukunaFugaProjectile>();

            float projectileSpeed = domainAmplified
                ? fugaProjectileSpeed * domainFugaProjectileSpeedMultiplier
                : fugaProjectileSpeed;
            float projectileRange = fugaRange;
            if (domainAmplified)
            {
                float targetDistance = target != null
                    ? Vector3.Distance(spawnPosition, target.transform.position + Vector3.up * 0.75f)
                    : activeDomainRadius;
                projectileRange = Mathf.Max(fugaRange, activeDomainRadius * 1.10f, targetDistance + 2f);
            }

            projectile.Configure(
                ownHealth,
                target,
                direction,
                projectileSpeed,
                projectileRange,
                fugaProjectileRadius,
                fugaExplosionRadius,
                domainAmplified ? 0f : fugaDamage,
                domainAmplified ? 0f : fugaKnockback,
                domainAmplified ? 0f : fugaHitStun,
                () =>
                {
                    combatAudio?.PlayRedImpact();
                    if (domainAmplified)
                    {
                        SukunaDomainFugaBlast.Detonate(
                            ownHealth,
                            activeDomainCenter,
                            activeDomainRadius,
                            fugaDamage,
                            fugaKnockback,
                            fugaHitStun
                        );
                    }
                }
            );

            isCasting = false;
            isFugaCasting = false;
        }

        private List<Health> FindLivingEnemies()
        {
            Health[] allHealth = FindObjectsByType<Health>(FindObjectsSortMode.None);
            List<Health> enemies = new List<Health>();
            foreach (Health health in allHealth)
            {
                if (health != null && health != ownHealth && !health.IsDead)
                {
                    enemies.Add(health);
                }
            }
            return enemies;
        }

        private List<Health> FindEnemiesInsideRadius(Vector3 center, float radius)
        {
            List<Health> targets = new List<Health>();
            foreach (Health enemy in FindLivingEnemies())
            {
                Vector3 offset = enemy.transform.position - center;
                offset.y = 0f;
                if (offset.magnitude <= radius)
                {
                    targets.Add(enemy);
                }
            }
            return targets;
        }

        private List<Health> FindSingleDismantleTarget(List<Health> enemies)
        {
            List<Health> targets = new List<Health>();
            Health target = ResolvePreferredTarget(enemies, dismantleRange);
            if (target != null)
            {
                targets.Add(target);
            }
            return targets;
        }

        private List<Health> FindDismantleConeTargets(List<Health> enemies)
        {
            List<Health> targets = new List<Health>();
            Vector3 aimDirection = ResolveAimDirection(null);
            float minimumDot = Mathf.Cos(dismantleMultiAngle * 0.5f * Mathf.Deg2Rad);

            foreach (Health enemy in enemies)
            {
                Vector3 offset = enemy.transform.position - transform.position;
                offset.y = 0f;
                float distance = offset.magnitude;
                if (
                    distance <= dismantleRange
                    && distance > 0.001f
                    && Vector3.Dot(aimDirection, offset.normalized) >= minimumDot
                )
                {
                    targets.Add(enemy);
                }
            }

            if (targets.Count == 0)
            {
                Health fallback = ResolvePreferredTarget(enemies, dismantleRange);
                if (fallback != null)
                {
                    targets.Add(fallback);
                }
            }
            return targets;
        }

        private Health FindCleaveTarget()
        {
            List<Health> enemies = FindLivingEnemies();
            return ResolvePreferredTarget(enemies, cleaveRange);
        }

        private Health ResolvePreferredTarget(List<Health> enemies, float range)
        {
            Health locked = targetLock != null ? targetLock.CurrentTarget : null;
            if (
                locked != null
                && !locked.IsDead
                && Vector3.Distance(transform.position, locked.transform.position) <= range
            )
            {
                return locked;
            }

            Health nearest = null;
            float nearestDistance = range;
            foreach (Health enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = enemy;
                }
            }
            return nearest;
        }

        private Health ResolvePreferredDomainFugaTarget(
            List<Health> enemies,
            Vector3 center,
            float radius
        )
        {
            Health locked = targetLock != null ? targetLock.CurrentTarget : null;
            if (locked != null && !locked.IsDead)
            {
                Vector3 lockedOffset = locked.transform.position - center;
                lockedOffset.y = 0f;
                if (lockedOffset.magnitude <= radius)
                {
                    return locked;
                }
            }

            Health nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (Health enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = enemy;
                }
            }
            return nearest;
        }

        private Vector3 ResolveAimDirection(List<Health> preferredTargets)
        {
            if (preferredTargets != null && preferredTargets.Count > 0 && preferredTargets[0] != null)
            {
                Vector3 direction = preferredTargets[0].transform.position - transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.001f)
                {
                    return direction.normalized;
                }
            }

            if (targetLock != null && targetLock.TryGetAimDirection(out Vector3 lockedDirection))
            {
                return lockedDirection;
            }

            Vector3 forward = transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
        }

        private void FaceDirection(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }

        private static void ApplyHitReaction(Health target, Vector3 impulse, float stun)
        {
            MonoBehaviour[] behaviours = target.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IHitReactable hitReactable)
                {
                    hitReactable.ApplyHitReaction(impulse, stun);
                    break;
                }
            }
        }

        private void SpawnSlashVisual(Vector3 direction, float length, Color color, int hitIndex)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = transform.forward;
            }
            direction.Normalize();

            Vector3 side = Vector3.Cross(Vector3.up, direction).normalized;
            float alternatingOffset = (hitIndex % 2 == 0 ? -1f : 1f) * 0.45f;
            Vector3 center = transform.position + Vector3.up * (0.8f + hitIndex * 0.03f);
            Vector3 start = center + side * alternatingOffset;
            Vector3 end = center + direction * Mathf.Max(2f, length) - side * alternatingOffset;

            GameObject visual = new GameObject("SukunaSlashPrototype");
            LineRenderer line = visual.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startWidth = 0.11f;
            line.endWidth = 0.025f;
            line.startColor = color;
            line.endColor = new Color(color.r, color.g, color.b, 0.12f);
            line.numCapVertices = 4;
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
            Destroy(visual, 0.16f);
        }

        private string BuildDismantleStatus()
        {
            string mode = FindLivingEnemies().Count >= 2 ? "다중 2히트" : "단일 4히트";
            if (isCasting)
            {
                return $"Q · 해 · 다른 술식 시전 중 · {mode}";
            }
            if (DismantleCooldownRemaining > 0f)
            {
                return $"Q · 해 · {DismantleCooldownRemaining:0.0}s";
            }
            return $"Q · 해 · READY · CE {dismantleEnergyCost:0} · {mode}";
        }

        private string BuildCleaveStatus()
        {
            if (isCasting)
            {
                return "E · 팔 · 다른 술식 시전 중";
            }
            if (CleaveCooldownRemaining > 0f)
            {
                return $"E · 팔 · {CleaveCooldownRemaining:0.0}s";
            }
            return $"E · 팔 · READY · CE {cleaveEnergyCost:0} · 근거리 {cleaveHits}히트";
        }

        private string BuildFugaStatus()
        {
            if (isFugaCasting)
            {
                return "R · 푸가 · 개방 중";
            }
            if (FugaCooldownRemaining > 0f)
            {
                return $"R · 푸가 · {FugaCooldownRemaining:0.0}s";
            }

            sukunaDomain ??= GetComponent<SukunaDomainController>();
            bool domainActive = sukunaDomain != null && sukunaDomain.enabled && sukunaDomain.IsActive;

            if (!FugaPrepared)
            {
                string prefix = domainActive ? "영역 내 · " : string.Empty;
                return $"R · 푸가 · {prefix}해 {(DismantleUsed ? 1 : 0)}/1 · 팔 {(CleaveUsed ? 1 : 0)}/1";
            }

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            if (cursedEnergy != null && !cursedEnergy.CanSpend(fugaEnergyCost))
            {
                return $"R · 푸가 · 주력 부족 · CE {fugaEnergyCost:0}";
            }

            if (domainActive)
            {
                int domainTargets = FindEnemiesInsideRadius(
                    sukunaDomain.DomainCenter,
                    sukunaDomain.DomainRadius
                ).Count;
                if (domainTargets <= 0)
                {
                    return "R · 푸가 · 영역 내 대상 없음";
                }
                return $"R · 푸가 · READY · 영역 내 광역 {domainTargets}명 · CE {fugaEnergyCost:0}";
            }

            List<Health> enemies = FindLivingEnemies();
            if (enemies.Count != 1)
            {
                return $"R · 푸가 · 영역 밖 적 1명 필요 · 현재 {enemies.Count}";
            }

            Health target = enemies[0];
            if (
                target == null
                || Vector3.Distance(transform.position, target.transform.position) > fugaRange
            )
            {
                return $"R · 푸가 · 대상 거리 초과 · 최대 {fugaRange:0}m";
            }

            return $"R · 푸가 · READY · CE {fugaEnergyCost:0} · 단일 대상";
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

            EnsureStyle();
            float width = Mathf.Min(330f, Screen.width - 24f);
            DrawSkillPanel(
                new Rect(12f, 104f, width, 29f),
                BuildDismantleStatus(),
                new Color(0.96f, 0.18f, 0.15f)
            );
            DrawSkillPanel(
                new Rect(12f, 137f, width, 29f),
                BuildCleaveStatus(),
                new Color(1f, 0.48f, 0.10f)
            );
            DrawSkillPanel(
                new Rect(12f, 170f, width, 29f),
                BuildFugaStatus(),
                FugaPrepared
                    ? new Color(1f, 0.36f, 0.06f)
                    : new Color(0.60f, 0.20f, 0.08f)
            );
        }

        private void DrawSkillPanel(Rect rect, string text, Color accent)
        {
            DrawRect(rect, new Color(0.075f, 0.012f, 0.012f, 0.90f));
            DrawBorder(rect, accent, 1f);
            skillStyle.normal.textColor = Color.white;
            GUI.Label(rect, text, skillStyle);
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
