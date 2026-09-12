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
        public const KeyCode Run = KeyCode.LeftShift;
        public const KeyCode RunAlternate = KeyCode.RightShift;
        public const KeyCode TargetLock = KeyCode.Tab;

        // Gate 5B production-facing reserve-slot commands.
        public const KeyCode Reserve1Tag = KeyCode.Alpha1;
        public const KeyCode Reserve2Tag = KeyCode.Alpha2;

        public const string Skill1Label = "Q";
        public const string Skill2Label = "E";
        public const string UltimateLabel = "R";
        public const string DomainLabel = "V";
        public const string CancelCommandLabel = "X";
        public const string DodgeLabel = "SPACE";
        public const string TargetLockLabel = "TAB";
        public const string Reserve1TagLabel = "1";
        public const string Reserve2TagLabel = "2";
    }

    /// <summary>
    /// Gate 5B production command boundary. Gameplay reads command intent here instead
    /// of binding itself to keyboard/mouse polling. Current legacy Input Manager
    /// semantics are intentionally preserved; future device mappings belong here.
    /// </summary>
    public static class ProductionCombatInput
    {
        public static Vector2 Move
        {
            get
            {
#if UNITY_EDITOR
                if (MoveReplay != null) return MoveReplay();
#endif
                return new Vector2(
                    Input.GetAxisRaw("Horizontal"),
                    Input.GetAxisRaw("Vertical")
                );
            }
        }

#if UNITY_EDITOR
        // Automated device-boundary replay; gameplay Update and all gates remain live.
        public static System.Func<Vector2> MoveReplay;
        public static System.Func<bool> BasicAttackReplay;
        public static System.Func<bool> DodgeReplay;
        public static System.Func<bool> DomainReplay;
        public static System.Func<bool> DomainModifierPressedReplay;
        public static System.Func<bool> DomainModifierHeldReplay;
        public static System.Func<bool> DomainModifierReleasedReplay;
#endif
        public static bool BasicAttackPressed
        {
            get
            {
#if UNITY_EDITOR
                if (BasicAttackReplay != null) return BasicAttackReplay();
#endif
                return Input.GetMouseButtonDown(0);
            }
        }
        public static bool DodgePressed
        {
            get
            {
#if UNITY_EDITOR
                if (DodgeReplay != null) return DodgeReplay();
#endif
                return Input.GetKeyDown(CombatInputBindings.Dodge);
            }
        }
        public static bool RunHeld => Input.GetKey(CombatInputBindings.Run)
            || Input.GetKey(CombatInputBindings.RunAlternate);
        public static bool TargetLockPressed =>
            Input.GetKeyDown(CombatInputBindings.TargetLock);
        public static bool Skill1Pressed => Input.GetKeyDown(CombatInputBindings.Skill1);
        public static bool Skill2Pressed => Input.GetKeyDown(CombatInputBindings.Skill2);
        public static bool UltimatePressed => Input.GetKeyDown(CombatInputBindings.Ultimate);
        public static bool DomainPressed
        {
            get
            {
#if UNITY_EDITOR
                if (DomainReplay != null) return DomainReplay();
#endif
                return Input.GetKeyDown(CombatInputBindings.Domain);
            }
        }
        public static bool CancelPressed =>
            Input.GetKeyDown(CombatInputBindings.CancelCommand);
        public static bool Reserve1TagPressed =>
            Input.GetKeyDown(CombatInputBindings.Reserve1Tag);
        public static bool Reserve2TagPressed =>
            Input.GetKeyDown(CombatInputBindings.Reserve2Tag);

        // Gojo's existing domain gesture stays device-neutral at the gameplay call site.
        public static bool DomainModifierPressed
        {
            get
            {
#if UNITY_EDITOR
                if (DomainModifierPressedReplay != null) return DomainModifierPressedReplay();
#endif
                return Input.GetMouseButtonDown(1);
            }
        }
        public static bool DomainModifierHeld
        {
            get
            {
#if UNITY_EDITOR
                if (DomainModifierHeldReplay != null) return DomainModifierHeldReplay();
#endif
                return Input.GetMouseButton(1);
            }
        }
        public static bool DomainModifierReleased
        {
            get
            {
#if UNITY_EDITOR
                if (DomainModifierReleasedReplay != null) return DomainModifierReleasedReplay();
#endif
                return Input.GetMouseButtonUp(1);
            }
        }
    }

    /// <summary>
    /// Production match/UI commands that are not combat actions.
    /// </summary>
    public static class ProductionMatchInput
    {
        public static bool ControlHelpPressed => Input.GetKeyDown(KeyCode.F1);
        public static bool RematchPressed => Input.GetKeyDown(KeyCode.Return);
        public static bool CharacterSelectPressed => Input.GetKeyDown(KeyCode.Escape);
    }

    /// <summary>
    /// Prototype regression shortcuts. They remain available in the Unity Editor and
    /// Development Builds, but compile to inactive command reads in production builds.
    /// </summary>
    public static class PrototypeDeveloperInput
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const bool BuildAllowsDeveloperInput = true;
#else
        private const bool BuildAllowsDeveloperInput = false;
#endif

        public static bool BuildAllowsDeveloperHarness => BuildAllowsDeveloperInput;

        public static bool OpponentModeTogglePressed =>
            BuildAllowsDeveloperHarness && Input.GetKeyDown(KeyCode.F2);
        public static bool StressRosterTogglePressed =>
            BuildAllowsDeveloperHarness && Input.GetKeyDown(KeyCode.F3);
        public static bool TeamSizeCyclePressed =>
            BuildAllowsDeveloperHarness && Input.GetKeyDown(KeyCode.F4);
        public static bool LegacyReserve1TagPressed =>
            BuildAllowsDeveloperHarness && Input.GetKeyDown(KeyCode.T);

        // Older direct character reload shortcuts are also prototype-only.
        public static bool SelectGojoPressed =>
            BuildAllowsDeveloperHarness && Input.GetKeyDown(KeyCode.Alpha1);
        public static bool SelectSukunaPressed =>
            BuildAllowsDeveloperHarness && Input.GetKeyDown(KeyCode.Alpha2);
        public static bool SelectMegumiPressed =>
            BuildAllowsDeveloperHarness && Input.GetKeyDown(KeyCode.Alpha3);
    }
}
