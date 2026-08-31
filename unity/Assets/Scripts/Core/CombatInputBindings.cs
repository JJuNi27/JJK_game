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
        public static Vector2 Move => new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        public static bool BasicAttackPressed => Input.GetMouseButtonDown(0);
        public static bool DodgePressed => Input.GetKeyDown(CombatInputBindings.Dodge);
        public static bool TargetLockPressed =>
            Input.GetKeyDown(CombatInputBindings.TargetLock);
        public static bool Skill1Pressed => Input.GetKeyDown(CombatInputBindings.Skill1);
        public static bool Skill2Pressed => Input.GetKeyDown(CombatInputBindings.Skill2);
        public static bool UltimatePressed => Input.GetKeyDown(CombatInputBindings.Ultimate);
        public static bool DomainPressed => Input.GetKeyDown(CombatInputBindings.Domain);
        public static bool CancelPressed =>
            Input.GetKeyDown(CombatInputBindings.CancelCommand);
        public static bool Reserve1TagPressed =>
            Input.GetKeyDown(CombatInputBindings.Reserve1Tag);
        public static bool Reserve2TagPressed =>
            Input.GetKeyDown(CombatInputBindings.Reserve2Tag);

        // Gojo's existing domain gesture stays device-neutral at the gameplay call site.
        public static bool DomainModifierPressed => Input.GetMouseButtonDown(1);
        public static bool DomainModifierHeld => Input.GetMouseButton(1);
        public static bool DomainModifierReleased => Input.GetMouseButtonUp(1);
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
