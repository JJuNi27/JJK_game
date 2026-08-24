using UnityEngine;

namespace JJKGame.Core
{
    public static class CombatInputBindings
    {
        public const KeyCode Skill1 = KeyCode.Q;
        public const KeyCode Skill2 = KeyCode.E;
        public const KeyCode Ultimate = KeyCode.R;
        public const KeyCode Domain = KeyCode.V;
        public const KeyCode CancelCommand = KeyCode.X;
        public const KeyCode Dodge = KeyCode.Space;
        public const KeyCode TargetLock = KeyCode.Tab;

        // Gate 5B production-facing reserve-slot commands.
        public const KeyCode Reserve1Tag = KeyCode.Alpha1;
        public const KeyCode Reserve2Tag = KeyCode.Alpha2;

        // Prototype/developer compatibility input. Final player tag flow uses Reserve1/2.
        public const KeyCode Tag = KeyCode.T;

        public const string Skill1Label = "Q";
        public const string Skill2Label = "E";
        public const string UltimateLabel = "R";
        public const string DomainLabel = "V";
        public const string CancelCommandLabel = "X";
        public const string DodgeLabel = "SPACE";
        public const string TargetLockLabel = "TAB";
        public const string Reserve1TagLabel = "1";
        public const string Reserve2TagLabel = "2";
        public const string TagLabel = "T";
    }
}
