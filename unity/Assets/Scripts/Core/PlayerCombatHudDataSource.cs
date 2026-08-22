using JJKGame.Player;
using UnityEngine;

namespace JJKGame.Core
{
    public enum PlayerTagHudState
    {
        Hidden,
        Ready,
        Cooldown,
        ActionLocked,
        ReserveKnockedOut,
    }

    public readonly struct PlayerTeamMemberHudSnapshot
    {
        public PlayerTeamMemberHudSnapshot(
            bool isValid,
            bool isActive,
            PrototypeCharacterId characterId,
            CharacterPresentationProfile presentationProfile,
            bool initialized,
            bool knockedOut,
            float health,
            float energy
        )
        {
            IsValid = isValid;
            IsActive = isActive;
            CharacterId = characterId;
            PresentationProfile = presentationProfile;
            Initialized = initialized;
            KnockedOut = knockedOut;
            Health = health;
            Energy = energy;
        }

        public bool IsValid { get; }
        public bool IsActive { get; }
        public PrototypeCharacterId CharacterId { get; }
        public CharacterPresentationProfile PresentationProfile { get; }
        public bool Initialized { get; }
        public bool KnockedOut { get; }
        public float Health { get; }
        public float Energy { get; }
    }

    public readonly struct PlayerCombatHudSnapshot
    {
        public PlayerCombatHudSnapshot(
            bool isValid,
            bool isDead,
            PrototypeCharacterId characterId,
            CharacterPresentationProfile presentationProfile,
            float currentHealth,
            float maxHealth,
            float currentEnergy,
            float maxEnergy,
            bool hasEnergy,
            CombatActionState actionState,
            bool techniqueBurnedOut,
            bool canUseTechnique,
            bool canUseUltimate,
            bool canUseDomain,
            bool teamMode,
            PrototypeCharacterId reserveCharacter,
            bool hasLivingReserve,
            float tagCooldownRemaining,
            PlayerTagHudState tagState,
            PlayerTeamMemberHudSnapshot activeMember,
            PlayerTeamMemberHudSnapshot reserveMember
        )
        {
            IsValid = isValid;
            IsDead = isDead;
            CharacterId = characterId;
            PresentationProfile = presentationProfile;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            CurrentEnergy = currentEnergy;
            MaxEnergy = maxEnergy;
            HasEnergy = hasEnergy;
            ActionState = actionState;
            TechniqueBurnedOut = techniqueBurnedOut;
            CanUseTechnique = canUseTechnique;
            CanUseUltimate = canUseUltimate;
            CanUseDomain = canUseDomain;
            TeamMode = teamMode;
            ReserveCharacter = reserveCharacter;
            HasLivingReserve = hasLivingReserve;
            TagCooldownRemaining = tagCooldownRemaining;
            TagState = tagState;
            ActiveMember = activeMember;
            ReserveMember = reserveMember;
        }

        public bool IsValid { get; }
        public bool IsDead { get; }
        public PrototypeCharacterId CharacterId { get; }
        public CharacterPresentationProfile PresentationProfile { get; }
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
        public float CurrentEnergy { get; }
        public float MaxEnergy { get; }
        public bool HasEnergy { get; }
        public CombatActionState ActionState { get; }
        public bool TechniqueBurnedOut { get; }
        public bool CanUseTechnique { get; }
        public bool CanUseUltimate { get; }
        public bool CanUseDomain { get; }
        public bool TeamMode { get; }
        public PrototypeCharacterId ReserveCharacter { get; }
        public bool HasLivingReserve { get; }
        public float TagCooldownRemaining { get; }
        public PlayerTagHudState TagState { get; }
        public PlayerTeamMemberHudSnapshot ActiveMember { get; }
        public PlayerTeamMemberHudSnapshot ReserveMember { get; }
    }

    /// <summary>
    /// Gate 4B production-candidate read-only binding between gameplay state and HUD.
    /// It owns no gameplay values and issues no combat commands. HUD implementations
    /// can consume this snapshot without knowing which concrete controller owns each value.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(PrototypeCharacterController))]
    public sealed class PlayerCombatHudDataSource : MonoBehaviour
    {
        private Health health;
        private CursedEnergyController cursedEnergy;
        private PrototypeCharacterController characterController;
        private CombatActionGate actionGate;
        private PrototypePlayerTeamController teamController;

        public static PlayerCombatHudDataSource GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            PlayerCombatHudDataSource source = owner.GetComponent<PlayerCombatHudDataSource>();
            return source != null ? source : owner.AddComponent<PlayerCombatHudDataSource>();
        }

        public PlayerCombatHudSnapshot Snapshot
        {
            get
            {
                RefreshReferences();
                if (health == null || characterController == null)
                {
                    return default;
                }

                PrototypeCharacterId activeCharacter = characterController.ActiveCharacter;
                CharacterPresentationProfile profile = characterController.PresentationProfile;
                bool teamMode = teamController != null && teamController.enabled;
                PrototypeCharacterId reserveCharacter = teamMode
                    ? teamController.ReserveCharacter
                    : activeCharacter;
                bool hasLivingReserve = teamMode && teamController.HasLivingReserve;
                float tagCooldown = teamMode ? teamController.ManualTagCooldownRemaining : 0f;

                CombatActionState actionState = actionGate != null
                    ? actionGate.CurrentState
                    : CombatActionState.Normal;
                bool burnedOut = actionGate != null && actionGate.TechniqueBurnedOut;

                PlayerTeamMemberHudSnapshot activeMember = BuildMemberSnapshot(activeCharacter, true);
                PlayerTeamMemberHudSnapshot reserveMember = teamMode
                    ? BuildMemberSnapshot(reserveCharacter, false)
                    : default;
                PlayerTagHudState tagState = ResolveTagState(
                    teamMode,
                    reserveMember,
                    tagCooldown,
                    actionState
                );

                return new PlayerCombatHudSnapshot(
                    true,
                    health.IsDead,
                    activeCharacter,
                    profile,
                    health.CurrentHealth,
                    health.MaxHealth,
                    cursedEnergy != null ? cursedEnergy.CurrentEnergy : 0f,
                    cursedEnergy != null ? cursedEnergy.MaxEnergy : 0f,
                    cursedEnergy != null,
                    actionState,
                    burnedOut,
                    actionGate == null || actionGate.CanStartTechnique,
                    actionGate == null || actionGate.CanStartUltimate,
                    actionGate == null || actionGate.CanStartDomain,
                    teamMode,
                    reserveCharacter,
                    hasLivingReserve,
                    tagCooldown,
                    tagState,
                    activeMember,
                    reserveMember
                );
            }
        }

        private void Awake()
        {
            RefreshReferences();
        }

        private PlayerTeamMemberHudSnapshot BuildMemberSnapshot(
            PrototypeCharacterId characterId,
            bool isActive
        )
        {
            CharacterPresentationProfile profile = CharacterPresentationProfiles.Get(characterId);
            if (isActive)
            {
                return new PlayerTeamMemberHudSnapshot(
                    true,
                    true,
                    characterId,
                    profile,
                    true,
                    health != null && health.IsDead,
                    health != null ? health.CurrentHealth : 0f,
                    cursedEnergy != null ? cursedEnergy.CurrentEnergy : 0f
                );
            }

            if (
                teamController != null
                && teamController.TryGetStoredMemberState(
                    characterId,
                    out bool initialized,
                    out float storedHealth,
                    out float storedEnergy,
                    out bool knockedOut
                )
            )
            {
                return new PlayerTeamMemberHudSnapshot(
                    true,
                    false,
                    characterId,
                    profile,
                    initialized,
                    knockedOut,
                    storedHealth,
                    storedEnergy
                );
            }

            return new PlayerTeamMemberHudSnapshot(
                false,
                false,
                characterId,
                profile,
                false,
                false,
                0f,
                0f
            );
        }

        private static PlayerTagHudState ResolveTagState(
            bool teamMode,
            PlayerTeamMemberHudSnapshot reserveMember,
            float tagCooldown,
            CombatActionState actionState
        )
        {
            if (!teamMode)
            {
                return PlayerTagHudState.Hidden;
            }
            if (reserveMember.IsValid && reserveMember.KnockedOut)
            {
                return PlayerTagHudState.ReserveKnockedOut;
            }
            if (tagCooldown > 0f)
            {
                return PlayerTagHudState.Cooldown;
            }
            if (actionState != CombatActionState.Normal)
            {
                return PlayerTagHudState.ActionLocked;
            }
            return PlayerTagHudState.Ready;
        }

        private void RefreshReferences()
        {
            health ??= GetComponent<Health>();
            cursedEnergy ??= GetComponent<CursedEnergyController>();
            characterController ??= GetComponent<PrototypeCharacterController>();
            actionGate ??= GetComponent<CombatActionGate>();
            teamController ??= GetComponent<PrototypePlayerTeamController>();
        }
    }
}
