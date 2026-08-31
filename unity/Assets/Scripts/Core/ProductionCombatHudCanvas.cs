using JJKGame.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JJKGame.Core
{
    /// <summary>
    /// Shared presentation switch so the Gate 5B Canvas HUD can replace prototype IMGUI
    /// without deleting the old regression/debug HUD implementation.
    /// </summary>
    public static class CombatHudPresentationMode
    {
        public static bool ProductionCanvasActive { get; internal set; }
    }

    /// <summary>
    /// Gate 5B production-facing battle HUD shell. It consumes the Gate 4 read-only HUD
    /// snapshots only; no gameplay state or combat command is owned here.
    /// </summary>
    [DefaultExecutionOrder(1500)]
    [DisallowMultipleComponent]
    public sealed class ProductionCombatHudCanvas : MonoBehaviour
    {
        private const string TargetSceneName = "CombatMVP";

        private PlayerCombatHudDataSource playerSource;
        private OpponentCombatHudDataSource opponentSource;
        private MatchController matchController;
        private TargetLockController targetLock;
        private Font uiFont;
        private Canvas combatCanvas;
        private GameObject combatHudRoot;
        private GameObject skillDeckRoot;
        private GameObject domainInputRoot;
        private GameObject domainTimingRoot;
        private GameObject controlHelpRoot;
        private GameObject resultOverlayRoot;

        private Text playerName;
        private Text playerVariant;
        private Text playerHpText;
        private Text playerEnergyText;
        private Image playerHpFill;
        private Image playerEnergyFill;

        private Text opponentName;
        private Text opponentMeta;
        private Text opponentHpText;
        private Image opponentHpFill;

        private readonly Text[] teamSlotTexts = new Text[3];
        private readonly Image[] teamSlotPanels = new Image[3];

        private readonly Text[] skillTexts = new Text[4];
        private readonly Image[] skillPanels = new Image[4];

        private Text dodgeText;
        private Text comboText;
        private Text encounterText;
        private Image targetLockPanel;
        private Text targetLockText;
        private Image attackWarningPanel;
        private Image attackWarningFill;
        private Text attackWarningText;

        private Text domainStatusText;
        private Text domainInstructionText;
        private Image domainProgressFill;
        private Image domainReleaseWindow;
        private Image domainReleaseCursor;

        private Text controlHelpTitle;
        private Text controlHelpBody;

        private Image resultPanel;
        private Text resultTitle;
        private Text resultDescription;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            InstallForCurrentScene();
        }

        private static void HandleSceneLoaded(Scene _, LoadSceneMode __)
        {
            InstallForCurrentScene();
        }

        private static void InstallForCurrentScene()
        {
            if (
                SceneManager.GetActiveScene().name != TargetSceneName
                || FindFirstObjectByType<ProductionCombatHudCanvas>() != null
            )
            {
                return;
            }

            GameObject runner = new GameObject("ProductionCombatHudCanvas");
            runner.AddComponent<ProductionCombatHudCanvas>();
        }

        private void Awake()
        {
            if (SceneManager.GetActiveScene().name != TargetSceneName)
            {
                enabled = false;
                return;
            }

            CombatHudPresentationMode.ProductionCanvasActive = true;
            uiFont = CreateRuntimeFont();
            BuildUi();
            RefreshSources();
        }

        private void OnEnable()
        {
            if (SceneManager.GetActiveScene().name != TargetSceneName)
            {
                return;
            }

            CombatHudPresentationMode.ProductionCanvasActive = true;
            if (combatCanvas != null)
            {
                combatCanvas.enabled = true;
            }
        }

        private void OnDisable()
        {
            CombatHudPresentationMode.ProductionCanvasActive = false;
            if (combatCanvas != null)
            {
                combatCanvas.enabled = false;
            }
        }

        private void OnDestroy()
        {
            CombatHudPresentationMode.ProductionCanvasActive = false;
        }

        private void Update()
        {
            RefreshSources();

            bool matchFinished = matchController != null && matchController.MatchFinished;
            if (combatHudRoot != null)
            {
                combatHudRoot.SetActive(!matchFinished);
            }
            if (resultOverlayRoot != null)
            {
                resultOverlayRoot.SetActive(matchFinished);
            }
            if (matchFinished)
            {
                SetOverlayActive(controlHelpRoot, false);
                SetOverlayActive(domainInputRoot, false);
                RefreshResultOverlay();
                return;
            }

            PlayerCombatHudSnapshot player = playerSource != null
                ? playerSource.Snapshot
                : default;
            OpponentCombatHudSnapshot opponent = opponentSource != null
                ? opponentSource.Snapshot
                : default;

            if (player.IsValid)
            {
                RefreshPlayer(player);
                RefreshTeam(player);
                RefreshSkills(player);
            }

            bool controlHelpVisible =
                player.IsValid
                && matchController != null
                && matchController.ControlHelpVisible;
            SetOverlayActive(controlHelpRoot, controlHelpVisible);
            if (skillDeckRoot != null)
            {
                skillDeckRoot.SetActive(!controlHelpVisible);
            }
            if (controlHelpVisible)
            {
                RefreshControlHelp(player);
            }

            bool domainInputVisible = player.IsValid && player.DomainInputActive;
            SetOverlayActive(domainInputRoot, domainInputVisible);
            if (domainInputVisible)
            {
                RefreshDomainInput(player);
            }

            if (opponent.IsValid)
            {
                RefreshOpponent(opponent);
            }

            RefreshTargetLock(opponent);
            RefreshAttackWarning(opponent);
        }

        private void RefreshSources()
        {
            if (playerSource == null)
            {
                playerSource = FindFirstObjectByType<PlayerCombatHudDataSource>();
            }

            if (opponentSource == null)
            {
                opponentSource = FindFirstObjectByType<OpponentCombatHudDataSource>();
            }

            if (matchController == null)
            {
                matchController = FindFirstObjectByType<MatchController>();
            }

            if (targetLock == null)
            {
                targetLock = FindFirstObjectByType<TargetLockController>();
            }
        }

        private void BuildUi()
        {
            GameObject canvasObject = new GameObject(
                "CombatHudCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
            canvasObject.transform.SetParent(transform, false);

            combatCanvas = canvasObject.GetComponent<Canvas>();
            combatCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            combatCanvas.sortingOrder = 250;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform root = canvasObject.GetComponent<RectTransform>();
            StretchFull(root);

            RectTransform hudRoot = CreateContainer(root, "CombatHudRoot");
            combatHudRoot = hudRoot.gameObject;
            BuildPlayerPlate(hudRoot);
            BuildOpponentPlate(hudRoot);
            BuildTeamPlate(hudRoot);
            BuildSkillDeck(hudRoot);
            BuildCenterState(hudRoot);
            BuildDomainInputOverlay(root);
            BuildControlHelpOverlay(root);
            BuildResultOverlay(root);
        }

        private void BuildPlayerPlate(RectTransform root)
        {
            Image panel = CreatePanel(
                root,
                "PlayerPlate",
                new Vector2(0.025f, 0.82f),
                new Vector2(0.39f, 0.965f),
                new Color(0.012f, 0.020f, 0.040f, 0.92f)
            );

            CreatePanel(
                panel.rectTransform,
                "Accent",
                Vector2.zero,
                new Vector2(0.012f, 1f),
                new Color(0.18f, 0.66f, 1f, 1f)
            );

            playerName = CreateText(
                panel.rectTransform,
                "Name",
                "FIGHTER",
                28,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Color.white,
                new Vector2(0.045f, 0.67f),
                new Vector2(0.62f, 0.95f)
            );

            playerVariant = CreateText(
                panel.rectTransform,
                "Variant",
                string.Empty,
                15,
                FontStyle.Bold,
                TextAnchor.MiddleRight,
                new Color(0.60f, 0.68f, 0.82f),
                new Vector2(0.55f, 0.70f),
                new Vector2(0.96f, 0.94f)
            );

            BuildValueBar(
                panel.rectTransform,
                "HP",
                new Vector2(0.045f, 0.37f),
                new Vector2(0.96f, 0.59f),
                out playerHpFill,
                out playerHpText
            );

            BuildValueBar(
                panel.rectTransform,
                "CE",
                new Vector2(0.045f, 0.10f),
                new Vector2(0.78f, 0.29f),
                out playerEnergyFill,
                out playerEnergyText
            );

            dodgeText = CreateText(
                panel.rectTransform,
                "Dodge",
                "DODGE READY",
                14,
                FontStyle.Bold,
                TextAnchor.MiddleRight,
                new Color(0.30f, 0.82f, 1f),
                new Vector2(0.79f, 0.08f),
                new Vector2(0.96f, 0.30f)
            );
        }

        private void BuildOpponentPlate(RectTransform root)
        {
            Image panel = CreatePanel(
                root,
                "OpponentPlate",
                new Vector2(0.61f, 0.82f),
                new Vector2(0.975f, 0.965f),
                new Color(0.040f, 0.012f, 0.018f, 0.92f)
            );

            CreatePanel(
                panel.rectTransform,
                "Accent",
                new Vector2(0.988f, 0f),
                Vector2.one,
                new Color(0.96f, 0.22f, 0.14f, 1f)
            );

            opponentName = CreateText(
                panel.rectTransform,
                "Name",
                "OPPONENT",
                25,
                FontStyle.Bold,
                TextAnchor.MiddleRight,
                Color.white,
                new Vector2(0.34f, 0.66f),
                new Vector2(0.955f, 0.94f)
            );

            opponentMeta = CreateText(
                panel.rectTransform,
                "Meta",
                string.Empty,
                14,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.88f, 0.48f, 0.34f),
                new Vector2(0.045f, 0.68f),
                new Vector2(0.36f, 0.94f)
            );

            BuildValueBar(
                panel.rectTransform,
                "HP",
                new Vector2(0.045f, 0.18f),
                new Vector2(0.955f, 0.50f),
                out opponentHpFill,
                out opponentHpText
            );
        }

        private void BuildTeamPlate(RectTransform root)
        {
            Image panel = CreatePanel(
                root,
                "TeamPlate",
                new Vector2(0.025f, 0.035f),
                new Vector2(0.42f, 0.17f),
                new Color(0.010f, 0.017f, 0.032f, 0.90f)
            );

            CreateText(
                panel.rectTransform,
                "Header",
                "TEAM",
                14,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.50f, 0.62f, 0.82f),
                new Vector2(0.025f, 0.72f),
                new Vector2(0.18f, 0.96f)
            );

            for (int index = 0; index < 3; index++)
            {
                float xMin = 0.025f + index * 0.32f;
                float xMax = xMin + 0.30f;
                Image slot = CreatePanel(
                    panel.rectTransform,
                    $"Slot_{index}",
                    new Vector2(xMin, 0.12f),
                    new Vector2(xMax, 0.70f),
                    new Color(0.035f, 0.045f, 0.070f, 0.92f)
                );
                teamSlotPanels[index] = slot;
                teamSlotTexts[index] = CreateText(
                    slot.rectTransform,
                    "Text",
                    index == 0 ? "ACTIVE" : $"R{index}",
                    14,
                    FontStyle.Bold,
                    TextAnchor.MiddleCenter,
                    Color.white,
                    Vector2.zero,
                    Vector2.one
                );
            }
        }

        private void BuildSkillDeck(RectTransform root)
        {
            Image panel = CreatePanel(
                root,
                "SkillDeck",
                new Vector2(0.58f, 0.035f),
                new Vector2(0.975f, 0.19f),
                new Color(0.010f, 0.017f, 0.032f, 0.90f)
            );
            skillDeckRoot = panel.gameObject;

            CreateText(
                panel.rectTransform,
                "Header",
                "TECHNIQUES",
                14,
                FontStyle.Bold,
                TextAnchor.MiddleRight,
                new Color(0.50f, 0.62f, 0.82f),
                new Vector2(0.68f, 0.76f),
                new Vector2(0.975f, 0.96f)
            );

            string[] keys = { "Q", "E", "R", "V" };
            for (int index = 0; index < 4; index++)
            {
                float xMin = 0.025f + index * 0.24f;
                float xMax = xMin + 0.225f;
                Image slot = CreatePanel(
                    panel.rectTransform,
                    $"Skill_{keys[index]}",
                    new Vector2(xMin, 0.12f),
                    new Vector2(xMax, 0.72f),
                    new Color(0.035f, 0.045f, 0.070f, 0.94f)
                );
                skillPanels[index] = slot;
                skillTexts[index] = CreateText(
                    slot.rectTransform,
                    "Text",
                    keys[index],
                    15,
                    FontStyle.Bold,
                    TextAnchor.MiddleCenter,
                    Color.white,
                    Vector2.zero,
                    Vector2.one
                );
            }
        }

        private void BuildCenterState(RectTransform root)
        {
            encounterText = CreateText(
                root,
                "Encounter",
                string.Empty,
                14,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(0.74f, 0.80f, 0.90f),
                new Vector2(0.40f, 0.925f),
                new Vector2(0.60f, 0.965f)
            );

            comboText = CreateText(
                root,
                "Combo",
                string.Empty,
                22,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(1f, 0.80f, 0.24f),
                new Vector2(0.39f, 0.74f),
                new Vector2(0.61f, 0.81f)
            );

            targetLockPanel = CreatePanel(
                root,
                "TargetLockChip",
                new Vector2(0.425f, 0.845f),
                new Vector2(0.575f, 0.895f),
                new Color(0.055f, 0.040f, 0.010f, 0.92f)
            );
            targetLockText = CreateText(
                targetLockPanel.rectTransform,
                "Text",
                string.Empty,
                14,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(1f, 0.86f, 0.36f),
                Vector2.zero,
                Vector2.one
            );
            targetLockPanel.gameObject.SetActive(false);

            attackWarningPanel = CreatePanel(
                root,
                "AttackWarning",
                new Vector2(0.405f, 0.625f),
                new Vector2(0.595f, 0.705f),
                new Color(0.090f, 0.010f, 0.012f, 0.92f)
            );
            attackWarningText = CreateText(
                attackWarningPanel.rectTransform,
                "Text",
                string.Empty,
                16,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(1f, 0.38f, 0.24f),
                new Vector2(0.03f, 0.28f),
                new Vector2(0.97f, 0.96f)
            );
            Image warningBar = CreatePanel(
                attackWarningPanel.rectTransform,
                "ProgressBackground",
                new Vector2(0.06f, 0.10f),
                new Vector2(0.94f, 0.24f),
                new Color(0.16f, 0.035f, 0.035f, 0.98f)
            );
            attackWarningFill = CreatePanel(
                warningBar.rectTransform,
                "ProgressFill",
                Vector2.zero,
                Vector2.one,
                new Color(1f, 0.22f, 0.12f, 0.92f)
            );
            attackWarningPanel.gameObject.SetActive(false);
        }

        private void BuildDomainInputOverlay(RectTransform root)
        {
            Image panel = CreatePanel(
                root,
                "DomainInputOverlay",
                new Vector2(0.23f, 0.205f),
                new Vector2(0.77f, 0.345f),
                new Color(0.012f, 0.022f, 0.055f, 0.96f)
            );
            domainInputRoot = panel.gameObject;

            CreatePanel(
                panel.rectTransform,
                "Accent",
                Vector2.zero,
                new Vector2(0.012f, 1f),
                new Color(0.24f, 0.62f, 1f, 1f)
            );

            domainStatusText = CreateText(
                panel.rectTransform,
                "Status",
                string.Empty,
                20,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(0.78f, 0.90f, 1f),
                new Vector2(0.045f, 0.67f),
                new Vector2(0.955f, 0.94f)
            );
            ConfigureBestFit(domainStatusText, 13, 20);

            Image progressBackground = CreatePanel(
                panel.rectTransform,
                "ReleaseTiming",
                new Vector2(0.055f, 0.34f),
                new Vector2(0.945f, 0.59f),
                new Color(0.055f, 0.070f, 0.115f, 1f)
            );
            domainTimingRoot = progressBackground.gameObject;

            domainProgressFill = CreatePanel(
                progressBackground.rectTransform,
                "ProgressFill",
                Vector2.zero,
                Vector2.one,
                new Color(0.20f, 0.52f, 1f, 0.42f)
            );
            domainReleaseWindow = CreatePanel(
                progressBackground.rectTransform,
                "ReleaseWindow",
                new Vector2(0.45f, 0f),
                new Vector2(0.65f, 1f),
                new Color(0.12f, 0.88f, 0.38f, 0.90f)
            );
            domainReleaseCursor = CreatePanel(
                progressBackground.rectTransform,
                "ReleaseCursor",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 1f),
                Color.white
            );
            domainReleaseCursor.rectTransform.offsetMin = new Vector2(-2f, -3f);
            domainReleaseCursor.rectTransform.offsetMax = new Vector2(2f, 3f);

            domainInstructionText = CreateText(
                panel.rectTransform,
                "Instruction",
                string.Empty,
                15,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(0.68f, 0.76f, 0.90f),
                new Vector2(0.045f, 0.07f),
                new Vector2(0.955f, 0.29f)
            );
            ConfigureBestFit(domainInstructionText, 11, 15);
            domainInputRoot.SetActive(false);
        }

        private void BuildControlHelpOverlay(RectTransform root)
        {
            Image panel = CreatePanel(
                root,
                "ControlHelpOverlay",
                new Vector2(0.64f, 0.37f),
                new Vector2(0.97f, 0.76f),
                new Color(0.010f, 0.017f, 0.032f, 0.97f)
            );
            controlHelpRoot = panel.gameObject;

            CreatePanel(
                panel.rectTransform,
                "Accent",
                Vector2.zero,
                new Vector2(0.012f, 1f),
                new Color(0.30f, 0.72f, 1f, 1f)
            );
            controlHelpTitle = CreateText(
                panel.rectTransform,
                "Title",
                "CONTROLS  ·  F1 CLOSE",
                22,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.70f, 0.86f, 1f),
                new Vector2(0.055f, 0.84f),
                new Vector2(0.95f, 0.97f)
            );
            ConfigureBestFit(controlHelpTitle, 13, 22);

            controlHelpBody = CreateText(
                panel.rectTransform,
                "Body",
                string.Empty,
                17,
                FontStyle.Bold,
                TextAnchor.UpperLeft,
                Color.white,
                new Vector2(0.055f, 0.08f),
                new Vector2(0.95f, 0.82f)
            );
            controlHelpBody.lineSpacing = 1.15f;
            ConfigureBestFit(controlHelpBody, 11, 17);
            controlHelpRoot.SetActive(false);
        }

        private void BuildResultOverlay(RectTransform root)
        {
            Image dimmer = CreatePanel(
                root,
                "MatchResultOverlay",
                Vector2.zero,
                Vector2.one,
                new Color(0f, 0f, 0f, 0.76f)
            );
            resultOverlayRoot = dimmer.gameObject;

            resultPanel = CreatePanel(
                dimmer.rectTransform,
                "ResultPanel",
                new Vector2(0.27f, 0.32f),
                new Vector2(0.73f, 0.68f),
                new Color(0.018f, 0.024f, 0.045f, 0.99f)
            );
            resultTitle = CreateText(
                resultPanel.rectTransform,
                "Result",
                string.Empty,
                58,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Color.white,
                new Vector2(0.06f, 0.54f),
                new Vector2(0.94f, 0.90f)
            );
            ConfigureBestFit(resultTitle, 26, 58);

            resultDescription = CreateText(
                resultPanel.rectTransform,
                "Description",
                string.Empty,
                22,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Color.white,
                new Vector2(0.06f, 0.35f),
                new Vector2(0.94f, 0.55f)
            );
            ConfigureBestFit(resultDescription, 14, 22);

            Text controls = CreateText(
                resultPanel.rectTransform,
                "Controls",
                "ENTER  ·  REMATCH     ESC  ·  TEAM SELECT",
                18,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                new Color(0.72f, 0.78f, 0.88f),
                new Vector2(0.06f, 0.10f),
                new Vector2(0.94f, 0.30f)
            );
            ConfigureBestFit(controls, 11, 18);
            resultOverlayRoot.SetActive(false);
        }

        private void RefreshDomainInput(PlayerCombatHudSnapshot snapshot)
        {
            domainStatusText.text = string.IsNullOrEmpty(snapshot.DomainStatusLabel)
                ? "영역 입력"
                : snapshot.DomainStatusLabel;

            bool timing = snapshot.DomainReleaseTimingActive;
            SetOverlayActive(domainTimingRoot, timing);
            domainInstructionText.text = timing
                ? "초록 구간에서 RMB 해제  ·  X 입력 취소"
                : "RMB 유지 → LMB 입력  ·  X 입력 취소";
            if (!timing)
            {
                return;
            }

            float progress = Mathf.Clamp01(snapshot.DomainReleaseProgressNormalized);
            SetHorizontalAnchors(domainProgressFill.rectTransform, 0f, progress);

            float windowStart = Mathf.Clamp01(snapshot.DomainReleaseWindowStartNormalized);
            float windowEnd = Mathf.Clamp(
                snapshot.DomainReleaseWindowEndNormalized,
                windowStart,
                1f
            );
            SetHorizontalAnchors(domainReleaseWindow.rectTransform, windowStart, windowEnd);

            RectTransform cursor = domainReleaseCursor.rectTransform;
            cursor.anchorMin = new Vector2(progress, 0f);
            cursor.anchorMax = new Vector2(progress, 1f);
            cursor.offsetMin = new Vector2(-2f, -3f);
            cursor.offsetMax = new Vector2(2f, 3f);
        }

        private void RefreshControlHelp(PlayerCombatHudSnapshot snapshot)
        {
            CharacterPresentationProfile profile = snapshot.PresentationProfile;
            controlHelpTitle.color = profile.HudAccent;

            string teamControls = snapshot.TeamMode
                ? "\nTEAM   1 = R1 교대   2 = R2 교대"
                : string.Empty;
            controlHelpBody.text =
                "WASD  이동     SPACE  회피\n"
                + "TAB  Target Lock     LMB  기본 공격\n\n"
                + $"Q  {profile.Skill1.Label}\n"
                + $"E  {profile.Skill2.Label}\n"
                + $"R  {profile.Ultimate.Label}\n"
                + $"V  {profile.Domain.Label}\n"
                + "X  영역 입력 취소"
                + teamControls;
        }

        private void RefreshResultOverlay()
        {
            if (matchController == null)
            {
                return;
            }

            string result = matchController.ResultText;
            bool victory = result == "VICTORY";
            Color accent = victory
                ? new Color(0.20f, 0.78f, 1f)
                : new Color(0.95f, 0.18f, 0.22f);
            resultTitle.text = result;
            resultTitle.color = accent;
            resultDescription.text = matchController.ResultDescription;
            resultPanel.color = Color.Lerp(
                new Color(0.018f, 0.024f, 0.045f, 0.99f),
                accent,
                0.10f
            );
        }

        private void RefreshPlayer(PlayerCombatHudSnapshot snapshot)
        {
            CharacterPresentationProfile profile = snapshot.PresentationProfile;
            playerName.text = profile.HudName;
            playerName.color = profile.HudAccent;
            playerVariant.text = profile.VariantLabel;

            SetBar(
                playerHpFill,
                playerHpText,
                snapshot.CurrentHealth,
                snapshot.MaxHealth,
                profile.HudAccent,
                $"HP  {snapshot.CurrentHealth:0} / {snapshot.MaxHealth:0}"
            );

            SetBar(
                playerEnergyFill,
                playerEnergyText,
                snapshot.CurrentEnergy,
                snapshot.MaxEnergy,
                profile.EnergyAccent,
                snapshot.HasEnergy
                    ? $"{snapshot.EnergyProfileLabel}  {snapshot.CurrentEnergy:0} / {snapshot.MaxEnergy:0}"
                    : "CE  —"
            );

            if (snapshot.IsDodging)
            {
                dodgeText.text = "DODGING";
                dodgeText.color = new Color(0.46f, 0.96f, 1f);
            }
            else if (snapshot.DodgeReady)
            {
                dodgeText.text = "DODGE READY";
                dodgeText.color = new Color(0.30f, 0.82f, 1f);
            }
            else
            {
                dodgeText.text = $"DODGE {snapshot.DodgeCooldownRemaining:0.0}s";
                dodgeText.color = new Color(0.52f, 0.58f, 0.68f);
            }

            if (snapshot.DisplayHitComboCount > 0)
            {
                comboText.text = snapshot.HitComboLabel;
            }
            else if (snapshot.DisplayChainStep > 0)
            {
                comboText.text = snapshot.ChainLabel;
            }
            else
            {
                comboText.text = string.Empty;
            }
        }

        private void RefreshOpponent(OpponentCombatHudSnapshot snapshot)
        {
            OpponentTeamMemberHudSnapshot active = snapshot.ActiveMember;
            opponentName.text = active.IsValid ? active.DisplayName : "OPPONENT";
            opponentMeta.text = snapshot.ReserveEntryNotice
                ? "RESERVE ENTRY"
                : snapshot.ModeLabel;

            float current = active.IsValid ? active.CurrentHealth : 0f;
            float max = active.IsValid ? active.MaxHealth : 1f;
            SetBar(
                opponentHpFill,
                opponentHpText,
                current,
                max,
                new Color(0.96f, 0.22f, 0.14f),
                $"HP  {current:0} / {max:0}"
            );

            encounterText.text = snapshot.IsTeamBattle
                ? $"TEAM BATTLE · {snapshot.LivingMemberCount}/{snapshot.TeamSize}"
                : snapshot.ModeLabel;
        }

        private void RefreshTeam(PlayerCombatHudSnapshot snapshot)
        {
            PlayerTeamMemberHudSnapshot[] members =
            {
                snapshot.ActiveMember,
                snapshot.ReserveMember,
                snapshot.Reserve2Member,
            };

            for (int index = 0; index < 3; index++)
            {
                bool available = index < snapshot.TeamSize && members[index].IsValid;
                teamSlotPanels[index].gameObject.SetActive(available);
                if (!available)
                {
                    continue;
                }

                PlayerTeamMemberHudSnapshot member = members[index];
                CharacterPresentationProfile profile = member.PresentationProfile;
                string role = index == 0 ? "A" : $"R{index}";
                string state = index == 0
                    ? "ACTIVE"
                    : BuildTagStatus(
                        index,
                        index == 1 ? snapshot.Reserve1TagState : snapshot.Reserve2TagState,
                        snapshot.TagCooldownRemaining
                    );
                teamSlotTexts[index].text =
                    $"{role}  {profile.HudName}\nHP {member.Health:0}\n{state}";
                teamSlotTexts[index].color = member.KnockedOut
                    ? new Color(0.48f, 0.50f, 0.56f)
                    : profile.HudAccent;
                teamSlotPanels[index].color = member.KnockedOut
                    ? new Color(0.035f, 0.035f, 0.042f, 0.90f)
                    : Color.Lerp(
                        new Color(0.030f, 0.040f, 0.065f, 0.92f),
                        profile.HudAccent,
                        index == 0 ? 0.22f : 0.12f
                    );
            }
        }

        private void RefreshSkills(PlayerCombatHudSnapshot snapshot)
        {
            CharacterPresentationProfile profile = snapshot.PresentationProfile;
            CharacterSkillPresentation[] skills =
            {
                profile.Skill1,
                profile.Skill2,
                profile.Ultimate,
                profile.Domain,
            };
            string[] keys = { "Q", "E", "R", "V" };
            bool[] usable =
            {
                snapshot.CanUseTechnique,
                snapshot.CanUseTechnique,
                snapshot.CanUseUltimate,
                snapshot.CanUseDomain,
            };

            for (int index = 0; index < 4; index++)
            {
                string state = BuildSkillState(snapshot, index, usable[index]);
                bool emphasized = usable[index]
                    || (index == 3
                        && (snapshot.ActionState == CombatActionState.DomainInput
                            || snapshot.ActionState == CombatActionState.DomainActive));
                skillTexts[index].text = $"{keys[index]}\n{skills[index].Label}\n{state}";
                skillTexts[index].color = emphasized
                    ? skills[index].Accent
                    : new Color(0.42f, 0.44f, 0.50f);
                skillPanels[index].color = emphasized
                    ? Color.Lerp(
                        new Color(0.030f, 0.040f, 0.065f, 0.94f),
                        skills[index].Accent,
                        0.16f
                    )
                    : new Color(0.030f, 0.032f, 0.040f, 0.88f);
            }
        }

        private void RefreshTargetLock(OpponentCombatHudSnapshot opponent)
        {
            Health currentTarget = targetLock != null ? targetLock.CurrentTarget : null;
            bool visible = currentTarget != null;
            targetLockPanel.gameObject.SetActive(visible);
            if (!visible)
            {
                return;
            }

            string displayName = opponent.IsValid && opponent.ActiveMember.IsValid
                ? opponent.ActiveMember.DisplayName
                : currentTarget.gameObject.name.ToUpperInvariant();
            targetLockText.text = $"TARGET LOCK  ·  {displayName}";
        }

        private void RefreshAttackWarning(OpponentCombatHudSnapshot snapshot)
        {
            bool visible = snapshot.IsValid && snapshot.AttackTelegraphCount > 0;
            attackWarningPanel.gameObject.SetActive(visible);
            if (!visible)
            {
                return;
            }

            attackWarningText.text = snapshot.AttackTelegraphCount > 1
                ? $"DANGER  ·  ATTACK INCOMING × {snapshot.AttackTelegraphCount}"
                : "DANGER  ·  ATTACK INCOMING";

            float progress = Mathf.Clamp01(snapshot.AttackTelegraphProgress);
            RectTransform fillRect = attackWarningFill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(progress, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }

        private static string BuildTagStatus(
            int reserveIndex,
            PlayerTagHudState state,
            float cooldownRemaining
        )
        {
            string key = $"[{reserveIndex}]";
            return state switch
            {
                PlayerTagHudState.Ready => $"{key} READY",
                PlayerTagHudState.Cooldown => $"{key} COOLDOWN {cooldownRemaining:0.0}s",
                PlayerTagHudState.ActionLocked => $"{key} LOCKED",
                PlayerTagHudState.ReserveKnockedOut => $"{key} KO",
                _ => string.Empty,
            };
        }

        private static string BuildSkillState(
            PlayerCombatHudSnapshot snapshot,
            int skillIndex,
            bool usable
        )
        {
            if (snapshot.TechniqueBurnedOut)
            {
                return "BURNOUT";
            }
            if (usable)
            {
                return "READY";
            }
            if (skillIndex == 3 && snapshot.ActionState == CombatActionState.DomainActive)
            {
                return "ACTIVE";
            }
            if (skillIndex == 3 && snapshot.ActionState == CombatActionState.DomainInput)
            {
                return "INPUT";
            }
            return "LOCKED";
        }

        private void BuildValueBar(
            RectTransform parent,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            out Image fill,
            out Text valueText
        )
        {
            Image background = CreatePanel(
                parent,
                $"{label}_Background",
                anchorMin,
                anchorMax,
                new Color(0.050f, 0.060f, 0.082f, 0.98f)
            );

            fill = CreatePanel(
                background.rectTransform,
                $"{label}_Fill",
                Vector2.zero,
                Vector2.one,
                Color.white
            );
            fill.type = Image.Type.Simple;
            fill.raycastTarget = false;

            valueText = CreateText(
                background.rectTransform,
                $"{label}_Text",
                label,
                14,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Color.white,
                Vector2.zero,
                Vector2.one
            );
        }

        private static void SetBar(
            Image fill,
            Text text,
            float value,
            float max,
            Color color,
            string label
        )
        {
            if (fill != null)
            {
                float ratio = max > 0f ? Mathf.Clamp01(value / max) : 0f;
                RectTransform rect = fill.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = new Vector2(ratio, 1f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                Color fillColor = color;
                fillColor.a = 0.82f;
                fill.color = fillColor;
            }

            if (text != null)
            {
                text.text = label;
            }
        }

        private static void SetOverlayActive(GameObject overlay, bool active)
        {
            if (overlay != null && overlay.activeSelf != active)
            {
                overlay.SetActive(active);
            }
        }

        private static void SetHorizontalAnchors(RectTransform rect, float min, float max)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(Mathf.Clamp01(min), 0f);
            rect.anchorMax = new Vector2(Mathf.Clamp01(max), 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void ConfigureBestFit(Text text, int minimumSize, int maximumSize)
        {
            if (text == null)
            {
                return;
            }

            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = minimumSize;
            text.resizeTextMaxSize = maximumSize;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
        }

        private static RectTransform CreateContainer(RectTransform parent, string objectName)
        {
            GameObject container = new GameObject(objectName, typeof(RectTransform));
            container.transform.SetParent(parent, false);
            RectTransform rect = container.GetComponent<RectTransform>();
            StretchFull(rect);
            return rect;
        }

        private Image CreatePanel(
            RectTransform parent,
            string objectName,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color
        )
        {
            GameObject panelObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(Image)
            );
            panelObject.transform.SetParent(parent, false);
            RectTransform rect = panelObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = panelObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private Text CreateText(
            RectTransform parent,
            string objectName,
            string value,
            int fontSize,
            FontStyle style,
            TextAnchor alignment,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax
        )
        {
            GameObject textObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(Text)
            );
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Text text = textObject.GetComponent<Text>();
            text.font = uiFont;
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Font CreateRuntimeFont()
        {
            Font font = Font.CreateDynamicFontFromOSFont(
                new[] { "Malgun Gothic", "Arial", "Liberation Sans" },
                24
            );
            return font != null
                ? font
                : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
