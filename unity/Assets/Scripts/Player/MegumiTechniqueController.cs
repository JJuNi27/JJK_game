using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class MegumiTechniqueController : MonoBehaviour
    {
        [Header("Gate 5A · 옥견 Stress Test")]
        [SerializeField, Min(0f)] private float divineDogEnergyCost = 20f;
        [SerializeField, Min(0.1f)] private float divineDogCooldown = 6.5f;
        [SerializeField, Min(0.05f)] private float summonCastDuration = 0.22f;
        [SerializeField, Min(0.5f)] private float summonLifetime = 6f;
        [SerializeField, Min(0.1f)] private float summonDamage = 16f;
        [SerializeField, Min(0.1f)] private float summonMoveSpeed = 8f;
        [SerializeField, Min(0.2f)] private float summonAttackRange = 1.35f;
        [SerializeField, Min(0.1f)] private float summonAttackInterval = 1.15f;

        private Health ownHealth;
        private CursedEnergyController cursedEnergy;
        private CombatActionGate actionGate;
        private PrototypeCharacterController characterController;
        private TargetLockController targetLock;
        private MegumiDivineDogSummon activeSummon;
        private float castEndsAt;
        private float nextDivineDogAt;
        private bool wasMegumi;
        private string debugStatus = "WAITING";
        private float debugStatusUntil;
        private GUIStyle debugStyle;

        public bool IsCasting => enabled && Time.time < castEndsAt;
        public float DivineDogCooldownRemaining => Mathf.Max(0f, nextDivineDogAt - Time.time);

        public static MegumiTechniqueController GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            MegumiTechniqueController controller = owner.GetComponent<MegumiTechniqueController>();
            return controller != null ? controller : owner.AddComponent<MegumiTechniqueController>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapAfterSceneLoad()
        {
            // Gate 5A must not depend on the order in which the fighter-shell helper
            // components are added. Health is the stable scene/runtime combat marker.
            // Non-player Health objects receive an inert controller because Update()
            // requires PrototypeCharacterController.IsMegumi before accepting input.
            Health[] combatants = FindObjectsByType<Health>(FindObjectsSortMode.None);
            foreach (Health combatant in combatants)
            {
                if (combatant != null)
                {
                    GetOrCreate(combatant.gameObject);
                }
            }
        }

        private void Awake()
        {
            RefreshReferences();
        }

        private void OnEnable()
        {
            RefreshReferences();
        }

        private void Update()
        {
            RefreshReferences();
            bool isMegumi =
                characterController != null
                && characterController.IsMegumi
                && ownHealth != null
                && !ownHealth.IsDead;

            if (!isMegumi)
            {
                if (wasMegumi)
                {
                    CancelActiveSummon();
                    castEndsAt = 0f;
                }
                wasMegumi = false;
                return;
            }

            wasMegumi = true;
            if (!Input.GetKeyDown(CombatInputBindings.Skill1))
            {
                return;
            }

            SetDebugStatus("Q RECEIVED");
            TrySummonDivineDog();
        }

        private void OnDisable()
        {
            CancelActiveSummon();
            castEndsAt = 0f;
        }

        private void OnDestroy()
        {
            CancelActiveSummon();
        }

        private void TrySummonDivineDog()
        {
            if (Time.time < nextDivineDogAt)
            {
                SetDebugStatus($"BLOCKED · COOLDOWN {DivineDogCooldownRemaining:0.0}s");
                return;
            }

            if (IsCasting)
            {
                SetDebugStatus("BLOCKED · CASTING");
                return;
            }

            actionGate ??= CombatActionGate.GetOrCreate(gameObject);
            if (actionGate != null && !actionGate.CanStartTechnique)
            {
                SetDebugStatus($"BLOCKED · ACTION {actionGate.CurrentState}");
                return;
            }

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            if (cursedEnergy == null)
            {
                SetDebugStatus("BLOCKED · NO CE CONTROLLER");
                return;
            }

            if (!cursedEnergy.TrySpend(divineDogEnergyCost, "옥견"))
            {
                SetDebugStatus($"BLOCKED · CE {cursedEnergy.CurrentEnergy:0}");
                return;
            }

            targetLock ??= GetComponent<TargetLockController>();
            Health preferredTarget = targetLock != null ? targetLock.CurrentTarget : null;
            Vector3 direction = transform.forward;
            if (preferredTarget != null)
            {
                Vector3 offset = preferredTarget.transform.position - transform.position;
                offset.y = 0f;
                if (offset.sqrMagnitude > 0.001f)
                {
                    direction = offset.normalized;
                    transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                }
            }

            castEndsAt = Time.time + summonCastDuration;
            nextDivineDogAt = Time.time + divineDogCooldown;
            CancelActiveSummon();

            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
            Vector3 spawnPoint = transform.position + direction * 1.25f + right * 0.55f;
            spawnPoint.y = transform.position.y + 0.10f;

            GameObject summonObject = new GameObject("MegumiDivineDogPrototype");
            summonObject.transform.position = spawnPoint;
            summonObject.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            activeSummon = summonObject.AddComponent<MegumiDivineDogSummon>();
            activeSummon.Initialize(
                ownHealth,
                preferredTarget,
                summonLifetime,
                summonDamage,
                summonMoveSpeed,
                summonAttackRange,
                summonAttackInterval
            );

            TechniquePresentationRequests.Raise(
                TechniquePresentationRequest.AtPose(
                    ownHealth,
                    TechniquePresentationId.DivineDog,
                    TechniquePresentationPhase.Release,
                    spawnPoint,
                    direction
                )
            );
            CombatAudioEvents.Raise(
                CombatAudioEvent.ForOwner(ownHealth, CombatAudioEventId.DivineDog, 1)
            );
            SetDebugStatus($"SUMMONED · CE {cursedEnergy.CurrentEnergy:0}");
        }

        private void RefreshReferences()
        {
            ownHealth ??= GetComponent<Health>();
            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            characterController ??= GetComponent<PrototypeCharacterController>();
            targetLock ??= GetComponent<TargetLockController>();
        }

        private void SetDebugStatus(string status)
        {
            debugStatus = status;
            debugStatusUntil = Time.unscaledTime + 2.2f;
        }

        private void OnGUI()
        {
            if (
                characterController == null
                || !characterController.IsMegumi
                || ownHealth == null
                || ownHealth.IsDead
            )
            {
                return;
            }

            debugStyle ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 13,
            };
            debugStyle.normal.textColor = new Color(0.72f, 1f, 0.96f);

            string status = Time.unscaledTime <= debugStatusUntil
                ? debugStatus
                : DivineDogCooldownRemaining > 0f
                    ? $"Q 옥견 · COOLDOWN {DivineDogCooldownRemaining:0.0}s"
                    : "Q 옥견 · READY";

            GUI.Label(
                new Rect(Screen.width * 0.5f - 150f, Screen.height - 92f, 300f, 22f),
                status,
                debugStyle
            );
        }

        private void CancelActiveSummon()
        {
            if (activeSummon != null)
            {
                activeSummon.Dismiss();
                activeSummon = null;
            }
        }
    }
}
