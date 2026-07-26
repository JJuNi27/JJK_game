using JJKGame.Player;
using UnityEngine;

namespace JJKGame.Core
{
    public enum CombatActionState
    {
        Normal,
        Dodging,
        TechniqueCasting,
        DomainInput,
        DomainActive,
        Disabled,
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class CombatActionGate : MonoBehaviour
    {
        private Health health;
        private ThirdPersonPlayerController movement;
        private GojoTechniqueController technique;
        private GojoDomainController domain;

        public static CombatActionGate GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            CombatActionGate gate = owner.GetComponent<CombatActionGate>();
            return gate != null ? gate : owner.AddComponent<CombatActionGate>();
        }

        public CombatActionState CurrentState
        {
            get
            {
                RefreshReferences();
                if (health == null || health.IsDead)
                {
                    return CombatActionState.Disabled;
                }

                if (domain != null && domain.State == GojoDomainController.DomainState.Active)
                {
                    return CombatActionState.DomainActive;
                }

                if (domain != null && domain.State != GojoDomainController.DomainState.Normal)
                {
                    return CombatActionState.DomainInput;
                }

                if (technique != null && technique.IsCasting)
                {
                    return CombatActionState.TechniqueCasting;
                }

                if (movement != null && movement.IsDodging)
                {
                    return CombatActionState.Dodging;
                }

                return CombatActionState.Normal;
            }
        }

        public bool CanStartBasicAttack => CurrentState == CombatActionState.Normal;
        public bool CanStartTechnique => CurrentState == CombatActionState.Normal;
        public bool CanStartUltimate => CurrentState == CombatActionState.Normal;
        public bool CanStartDodge => CurrentState == CombatActionState.Normal;
        public bool CanStartDomain => CurrentState == CombatActionState.Normal;

        private void Awake()
        {
            RefreshReferences();
        }

        private void RefreshReferences()
        {
            health ??= GetComponent<Health>();
            movement ??= GetComponent<ThirdPersonPlayerController>();
            technique ??= GetComponent<GojoTechniqueController>();
            domain ??= GetComponent<GojoDomainController>();
        }
    }
}
