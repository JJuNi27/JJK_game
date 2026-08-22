namespace JJKGame.Player
{
    public enum MatchTeamSlot
    {
        Main = 0,
        Reserve1 = 1,
        Reserve2 = 2,
    }

    /// <summary>
    /// Gate 5B production-candidate pre-match team selection data.
    /// It contains identity/slot choice only and owns no battle-scene GameObjects.
    /// </summary>
    public sealed class MatchTeamSelection
    {
        private readonly PrototypeCharacterId[] members;

        private MatchTeamSelection(PrototypeCharacterId[] selectedMembers)
        {
            members = selectedMembers;
        }

        public int TeamSize => members.Length;
        public PrototypeCharacterId Main => members[0];
        public bool HasReserve1 => members.Length >= 2;
        public PrototypeCharacterId Reserve1 => HasReserve1 ? members[1] : default;
        public bool HasReserve2 => members.Length >= 3;
        public PrototypeCharacterId Reserve2 => HasReserve2 ? members[2] : default;

        public static MatchTeamSelection Solo(PrototypeCharacterId main)
        {
            return new MatchTeamSelection(new[] { main });
        }

        public static MatchTeamSelection Duo(
            PrototypeCharacterId main,
            PrototypeCharacterId reserve1
        )
        {
            return new MatchTeamSelection(new[] { main, reserve1 });
        }

        public static MatchTeamSelection Trio(
            PrototypeCharacterId main,
            PrototypeCharacterId reserve1,
            PrototypeCharacterId reserve2
        )
        {
            return new MatchTeamSelection(new[] { main, reserve1, reserve2 });
        }

        public bool TryGet(MatchTeamSlot slot, out PrototypeCharacterId characterId)
        {
            int index = (int)slot;
            if (index < 0 || index >= members.Length)
            {
                characterId = default;
                return false;
            }

            characterId = members[index];
            return true;
        }

        public PrototypeCharacterId GetRequired(MatchTeamSlot slot)
        {
            int index = (int)slot;
            if (index < 0 || index >= members.Length)
            {
                throw new System.InvalidOperationException(
                    $"Team slot {slot} is not available for team size {members.Length}."
                );
            }

            return members[index];
        }
    }

    /// <summary>
    /// Temporary cross-scene holder for the upcoming Character Select front-end.
    /// Gate 5B will replace F3/Alpha roster shortcuts by writing selection data here,
    /// then the battle bootstrap will consume it.
    /// </summary>
    public static class MatchTeamSelectionStore
    {
        private static MatchTeamSelection playerTeam = MatchTeamSelection.Duo(
            PrototypeCharacterId.GojoModern,
            PrototypeCharacterId.SukunaShibuyaYujiBody
        );

        public static MatchTeamSelection PlayerTeam => playerTeam;

        public static void SetPlayerTeam(MatchTeamSelection selection)
        {
            playerTeam = selection
                ?? MatchTeamSelection.Duo(
                    PrototypeCharacterId.GojoModern,
                    PrototypeCharacterId.SukunaShibuyaYujiBody
                );
        }

        public static void ResetPrototypeDefault()
        {
            playerTeam = MatchTeamSelection.Duo(
                PrototypeCharacterId.GojoModern,
                PrototypeCharacterId.SukunaShibuyaYujiBody
            );
        }
    }
}
