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
        private GojoTechniqueController gojoTechnique;
        private SukunaTechniqueController sukunaTechnique;
        private GojoDomainController gojoDomain;
        private TechniqueBurnoutController burnout;

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

                if (
                    gojoDomain != null
                    && gojoDomain.enabled
                    && gojoDomain.State == GojoDomainController.DomainState.Active
                )
                {
                    return CombatActionState.DomainActive;
                }

                if (
                    gojoDomain != null
                    && gojoDomain.enabled
                    && gojoDomain.State != GojoDomainController.DomainState.Normal
                )
                {
                    return CombatActionState.DomainInput;
                }

                if (
                    (gojoTechnique != null && gojoTechnique.enabled && gojoTechnique.IsCasting)
                    || (sukunaTechnique != null && sukunaTechnique.enabled && sukunaTechnique.IsCasting)
                )
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

        public bool TechniqueBurnedOut
        {
            get
            {
                RefreshReferences();
                return burnout != null && burnout.enabled && burnout.IsBurnedOut;
            }
        }

        public bool CanStartBasicAttack => CurrentState == CombatActionState.Normal;
        public bool CanStartTechnique => CurrentState == CombatActionState.Normal && !TechniqueBurnedOut;
        public bool CanStartUltimate => CurrentState == CombatActionState.Normal && !TechniqueBurnedOut;
        public bool CanStartDodge => CurrentState == CombatActionState.Normal;
        public bool CanStartDomain => CurrentState == CombatActionState.Normal && !TechniqueBurnedOut;

        private void Awake()
        {
            RefreshReferences();
        }

        private void RefreshReferences()
        {
            health ??= GetComponent<Health>();
            movement ??= GetComponent<ThirdPersonPlayerController>();
            gojoTechnique ??= GetComponent<GojoTechniqueController>();
            sukunaTechnique ??= GetComponent<SukunaTechniqueController>();
            gojoDomain ??= GetComponent<GojoDomainController>();
            burnout ??= GetComponent<TechniqueBurnoutController>();
        }
    }
}
