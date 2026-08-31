using JJKGame.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.Core
{
    /// <summary>
    /// Beauty Corner contextual skill deck.
    /// Gate 4B consumes a PlayerCombatHudSnapshot instead of reaching directly
    /// into character, health, energy and action-gate controllers.
    /// </summary>
    [DefaultExecutionOrder(1900)]
    [DisallowMultipleComponent]
    public sealed class PrototypeSkillDeckHud : MonoBehaviour
    {
        private const string TargetSceneName = "CombatMVP";
        private const float PressPulseDuration = 0.18f;

        private PlayerCombatHudDataSource hudDataSource;
        private float nextReferenceScanAt;

        private float qPulseUntil;
        private float ePulseUntil;
        private float rPulseUntil;
        private float vPulseUntil;

        private GUIStyle headerStyle;
        private GUIStyle chipStyle;
        private GUIStyle keyStyle;
        private GUIStyle statusStyle;
        private int styledForHeight = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindFirstObjectByType<PrototypeSkillDeckHud>() != null)
            {
                return;
            }

            GameObject runner = new GameObject("PrototypeSkillDeckHud");
            DontDestroyOnLoad(runner);
            runner.AddComponent<PrototypeSkillDeckHud>();
        }

        private void Update()
        {
            if (SceneManager.GetActiveScene().name != TargetSceneName)
            {
                return;
            }

            if (Time.unscaledTime >= nextReferenceScanAt)
            {
                RefreshReferences();
                nextReferenceScanAt = Time.unscaledTime + 0.30f;
            }

            float pulseUntil = Time.unscaledTime + PressPulseDuration;
            if (ProductionCombatInput.Skill1Pressed)
            {
                qPulseUntil = pulseUntil;
            }
            if (ProductionCombatInput.Skill2Pressed)
            {
                ePulseUntil = pulseUntil;
            }
            if (ProductionCombatInput.UltimatePressed)
            {
                rPulseUntil = pulseUntil;
            }
            if (ProductionCombatInput.DomainPressed)
            {
                vPulseUntil = pulseUntil;
            }
        }

        private void RefreshReferences()
        {
            if (hudDataSource != null)
            {
                return;
            }

            PrototypeCharacterController characterController =
                FindFirstObjectByType<PrototypeCharacterController>();
            if (characterController != null)
            {
                hudDataSource = PlayerCombatHudDataSource.GetOrCreate(characterController.gameObject);
            }
        }

        private void OnGUI()
        {
            if (CombatHudPresentationMode.ProductionCanvasActive)
            {
                return;
            }

            if (
                SceneManager.GetActiveScene().name != TargetSceneName
                || hudDataSource == null
            )
            {
                return;
            }

            PlayerCombatHudSnapshot snapshot = hudDataSource.Snapshot;
            if (!snapshot.IsValid || snapshot.IsDead)
            {
                return;
            }

            EnsureStyles();

            CharacterPresentationProfile profile = snapshot.PresentationProfile;
            Color fighterAccent = profile.HudAccent;

            float width = Mathf.Min(460f, Screen.width - 24f);
            float height = 58f;
            float x = Screen.width - width - 12f;
            float y = Screen.width < 1500f
                ? Screen.height - 150f
                : Screen.height - height - 12f;
            Rect panel = new Rect(x, y, width, height);

            DrawRect(panel, new Color(0.010f, 0.014f, 0.024f, 0.94f));
            DrawRect(new Rect(panel.x, panel.y, panel.width, 2f), fighterAccent);
            DrawRect(
                new Rect(panel.x, panel.y, 3f, panel.height),
                new Color(fighterAccent.r, fighterAccent.g, fighterAccent.b, 0.82f)
            );

            GUI.Label(
                new Rect(panel.x + 10f, panel.y + 3f, 150f, 18f),
                $"{profile.HudName} · {profile.CompactVariantLabel}",
                headerStyle
            );

            GUI.Label(
                new Rect(panel.x + panel.width - 170f, panel.y + 3f, 160f, 18f),
                BuildStateLabel(snapshot),
                statusStyle
            );

            float gap = 4f;
            float contentX = panel.x + 8f;
            float contentWidth = panel.width - 16f;
            float chipWidth = (contentWidth - gap * 3f) / 4f;
            float chipY = panel.y + 23f;
            float chipHeight = 28f;

            DrawProfileSkillChip(
                profile,
                CharacterPresentationSkillSlot.Skill1,
                new Rect(contentX + (chipWidth + gap) * 0f, chipY, chipWidth, chipHeight),
                CombatInputBindings.Skill1Label,
                snapshot.CanUseTechnique,
                Time.unscaledTime < qPulseUntil
            );
            DrawProfileSkillChip(
                profile,
                CharacterPresentationSkillSlot.Skill2,
                new Rect(contentX + (chipWidth + gap) * 1f, chipY, chipWidth, chipHeight),
                CombatInputBindings.Skill2Label,
                snapshot.CanUseTechnique,
                Time.unscaledTime < ePulseUntil
            );
            DrawProfileSkillChip(
                profile,
                CharacterPresentationSkillSlot.Ultimate,
                new Rect(contentX + (chipWidth + gap) * 2f, chipY, chipWidth, chipHeight),
                CombatInputBindings.UltimateLabel,
                snapshot.CanUseUltimate,
                Time.unscaledTime < rPulseUntil
            );
            DrawProfileSkillChip(
                profile,
                CharacterPresentationSkillSlot.Domain,
                new Rect(contentX + (chipWidth + gap) * 3f, chipY, chipWidth, chipHeight),
                CombatInputBindings.DomainLabel,
                snapshot.CanUseDomain,
                Time.unscaledTime < vPulseUntil
            );
        }

        private void DrawProfileSkillChip(
            CharacterPresentationProfile profile,
            CharacterPresentationSkillSlot slot,
            Rect rect,
            string keyLabel,
            bool available,
            bool pressed
        )
        {
            CharacterSkillPresentation skill = profile.GetSkill(slot);
            DrawSkillChip(rect, keyLabel, skill.Label, skill.Accent, available, pressed);
        }

        private void DrawSkillChip(
            Rect rect,
            string keyLabel,
            string techniqueLabel,
            Color accent,
            bool available,
            bool pressed
        )
        {
            float availability = available ? 1f : 0.42f;
            float pulse = pressed ? 1f : 0f;
            Color plate = Color.Lerp(
                new Color(0.030f, 0.034f, 0.046f, 0.96f),
                new Color(accent.r * 0.34f, accent.g * 0.34f, accent.b * 0.34f, 0.98f),
                0.34f + pulse * 0.46f
            );
            plate.a *= availability;

            Color border = accent;
            border.a = (pressed ? 1f : 0.72f) * availability;
            DrawRect(rect, plate);
            DrawBorder(rect, border, pressed ? 2f : 1f);

            Rect keyRect = new Rect(rect.x + 3f, rect.y + 3f, 24f, rect.height - 6f);
            Color keyPlate = accent;
            keyPlate.a = (pressed ? 0.96f : 0.58f) * availability;
            DrawRect(keyRect, keyPlate);

            Color previousKeyColor = keyStyle.normal.textColor;
            Color previousChipColor = chipStyle.normal.textColor;
            keyStyle.normal.textColor = available
                ? Color.white
                : new Color(0.72f, 0.72f, 0.75f);
            chipStyle.normal.textColor = available
                ? Color.white
                : new Color(0.56f, 0.58f, 0.64f);

            GUI.Label(keyRect, keyLabel, keyStyle);
            GUI.Label(
                new Rect(rect.x + 30f, rect.y + 1f, rect.width - 33f, rect.height - 2f),
                techniqueLabel,
                chipStyle
            );

            keyStyle.normal.textColor = previousKeyColor;
            chipStyle.normal.textColor = previousChipColor;
        }

        private static string BuildStateLabel(PlayerCombatHudSnapshot snapshot)
        {
            if (snapshot.TechniqueBurnedOut)
            {
                return "TECHNIQUE BURNOUT";
            }

            return snapshot.ActionState switch
            {
                CombatActionState.Dodging => "DODGE",
                CombatActionState.TechniqueCasting => "CASTING",
                CombatActionState.DomainInput => "DOMAIN INPUT",
                CombatActionState.DomainActive => "DOMAIN ACTIVE",
                CombatActionState.Disabled => "DISABLED",
                _ => "READY",
            };
        }

        private void EnsureStyles()
        {
            if (styledForHeight == Screen.height)
            {
                return;
            }

            styledForHeight = Screen.height;
            int baseSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 70f, 10f, 15f));

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = baseSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
            };
            headerStyle.normal.textColor = new Color(0.92f, 0.95f, 1f);

            statusStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(9, baseSize - 1),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
            };
            statusStyle.normal.textColor = new Color(0.62f, 0.70f, 0.82f);

            keyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = baseSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            keyStyle.normal.textColor = Color.white;

            chipStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(9, baseSize - 1),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
            };
            chipStyle.normal.textColor = Color.white;
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private static void DrawBorder(Rect rect, Color color, float thickness)
        {
            DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }
    }
}
