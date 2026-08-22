using JJKGame.Enemy;
using UnityEngine;

namespace JJKGame.Core
{
    public readonly struct OpponentTeamMemberHudSnapshot
    {
        public OpponentTeamMemberHudSnapshot(
            bool isValid,
            bool isActive,
            int memberIndex,
            float currentHealth,
            float maxHealth,
            bool knockedOut
        )
        {
            IsValid = isValid;
            IsActive = isActive;
            MemberIndex = memberIndex;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
            KnockedOut = knockedOut;
        }

        public bool IsValid { get; }
        public bool IsActive { get; }
        public int MemberIndex { get; }
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
        public bool KnockedOut { get; }
        public string DisplayName => $"CURSE {(char)('A' + MemberIndex)}";
    }

    public readonly struct OpponentCombatHudSnapshot
    {
        public OpponentCombatHudSnapshot(
            bool isValid,
            PrototypeEncounterMode mode,
            int livingMemberCount,
            int teamSize,
            bool reserveEntryNotice,
            OpponentTeamMemberHudSnapshot activeMember,
            OpponentTeamMemberHudSnapshot reserveMember
        )
        {
            IsValid = isValid;
            Mode = mode;
            LivingMemberCount = livingMemberCount;
            TeamSize = teamSize;
            ReserveEntryNotice = reserveEntryNotice;
            ActiveMember = activeMember;
            ReserveMember = reserveMember;
        }

        public bool IsValid { get; }
        public PrototypeEncounterMode Mode { get; }
        public bool IsTeamBattle => Mode == PrototypeEncounterMode.TeamBattle;
        public int LivingMemberCount { get; }
        public int TeamSize { get; }
        public bool ReserveEntryNotice { get; }
        public OpponentTeamMemberHudSnapshot ActiveMember { get; }
        public OpponentTeamMemberHudSnapshot ReserveMember { get; }
        public string ModeLabel => IsTeamBattle ? "TEAM BATTLE" : "TRAINING · MULTI CURSE";
    }

    /// <summary>
    /// Gate 4B read-only opponent HUD binding. It owns no encounter state and
    /// executes no mode switch, KO handoff, target-lock transfer, or match rule.
    /// It only converts PrototypeOpponentTeamController state into a UI snapshot.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PrototypeOpponentTeamController))]
    public sealed class OpponentCombatHudDataSource : MonoBehaviour
    {
        private PrototypeOpponentTeamController teamController;

        public static OpponentCombatHudDataSource GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            OpponentCombatHudDataSource source = owner.GetComponent<OpponentCombatHudDataSource>();
            return source != null ? source : owner.AddComponent<OpponentCombatHudDataSource>();
        }

        public OpponentCombatHudSnapshot Snapshot
        {
            get
            {
                teamController ??= GetComponent<PrototypeOpponentTeamController>();
                if (teamController == null || !teamController.IsInitialized)
                {
                    return default;
                }

                int activeIndex = teamController.ActiveMemberIndex;
                int reserveIndex = 1 - activeIndex;
                return new OpponentCombatHudSnapshot(
                    true,
                    teamController.Mode,
                    teamController.LivingMemberCount,
                    teamController.TeamSize,
                    teamController.EntryNoticeActive,
                    BuildMember(activeIndex, true),
                    BuildMember(reserveIndex, false)
                );
            }
        }

        private OpponentTeamMemberHudSnapshot BuildMember(int index, bool isActive)
        {
            Health member = teamController != null ? teamController.GetMember(index) : null;
            if (member == null)
            {
                return new OpponentTeamMemberHudSnapshot(false, isActive, index, 0f, 0f, true);
            }

            return new OpponentTeamMemberHudSnapshot(
                true,
                isActive,
                index,
                member.CurrentHealth,
                member.MaxHealth,
                member.IsDead
            );
        }
    }
}
