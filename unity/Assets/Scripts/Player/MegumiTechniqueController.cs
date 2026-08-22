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
            PrototypeCharacterController[] characters =
                FindObjectsByType<PrototypeCharacterController>(FindObjectsSortMode.None);
            foreach (PrototypeCharacterController character in characters)
            {
                if (character != null)
                {
                    GetOrCreate(character.gameObject);
                }
            }
        }

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
            cursedEnergy = CursedEnergyController.GetOrCreate(gameObject);
            characterController = GetComponent<PrototypeCharacterController>();
            targetLock = GetComponent<TargetLockController>();
        }

        private void Update()
        {
            characterController ??= GetComponent<PrototypeCharacterController>();
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
            if (Time.time < nextDivineDogAt || IsCasting)
            {
                return;
            }

            actionGate ??= CombatActionGate.GetOrCreate(gameObject);
            if (actionGate != null && !actionGate.CanStartTechnique)
            {
                return;
            }

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            if (cursedEnergy == null || !cursedEnergy.TrySpend(divineDogEnergyCost, "옥견"))
            {
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
