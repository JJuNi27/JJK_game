using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JJKGame.Player
{
    /// <summary>
    /// Gate 5B functional Character / Team Select front-end.
    /// CharacterSelect is the dedicated pre-match host scene. SampleScene remains accepted
    /// only as a developer compatibility host. This UI writes MatchTeamSelectionStore and
    /// never touches battle GameObjects directly.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    [DisallowMultipleComponent]
    public sealed class CharacterTeamSelectFrontEnd : MonoBehaviour
    {
        private const string HostSceneName = "CharacterSelect";
        private const string LegacyHostSceneName = "SampleScene";
        private const string BattleSceneName = "CombatMVP";
        private const int MaxTeamSize = 3;

        private static readonly PrototypeCharacterId[] Roster =
        {
            PrototypeCharacterId.GojoModern,
            PrototypeCharacterId.SukunaShibuyaYujiBody,
            PrototypeCharacterId.MegumiStudent,
        };

        private readonly List<PrototypeCharacterId> selectedTeam =
            new List<PrototypeCharacterId>(MaxTeamSize);

        private readonly Button[] rosterButtons = new Button[Roster.Length];
        private readonly Image[] rosterButtonImages = new Image[Roster.Length];
        private readonly Text[] slotTexts = new Text[MaxTeamSize];

        private Font uiFont;
        private Text previewName;
        private Text previewVariant;
        private Text previewSkills;
        private Text previewHint;
        private Text statusText;
        private Button startButton;
        private Image startButtonImage;
        private PrototypeCharacterId previewCharacter = PrototypeCharacterId.GojoModern;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (
                !IsSelectHostScene(SceneManager.GetActiveScene().name)
                || FindFirstObjectByType<CharacterTeamSelectFrontEnd>() != null
            )
            {
                return;
            }

            GameObject runner = new GameObject("CharacterTeamSelectFrontEnd");
            runner.AddComponent<CharacterTeamSelectFrontEnd>();
        }

        private void Awake()
        {
            if (!IsSelectHostScene(SceneManager.GetActiveScene().name))
            {
                enabled = false;
                return;
            }

            uiFont = CreateRuntimeFont();
            EnsureEventSystem();
            BuildUi();
            PreviewCharacter(previewCharacter);
            RefreshUi();
        }

        private void Update()
        {
            if (!enabled)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SelectRosterCharacter(Roster[0]);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectRosterCharacter(Roster[1]);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SelectRosterCharacter(Roster[2]);
            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                RemoveLastSelection();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                ClearSelection();
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                StartBattle();
            }
        }

        private void BuildUi()
        {
            GameObject canvasObject = new GameObject(
                "CharacterSelectCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            StretchFull(canvasRect);

            Image background = CreatePanel(
                canvasRect,
                "Background",
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                new Color(0.010f, 0.014f, 0.026f, 1f)
            );

            Image topBand = CreatePanel(
                background.rectTransform,
                "TopBand",
                new Vector2(0f, 0.82f),
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                new Color(0.025f, 0.035f, 0.065f, 0.98f)
            );

            CreateText(
                topBand.rectTransform,
                "Title",
                "CHARACTER / TEAM SELECT",
                42,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.92f, 0.95f, 1f),
                new Vector2(0.04f, 0.36f),
                new Vector2(0.70f, 0.90f),
                Vector2.zero,
                Vector2.zero
            );

            CreateText(
                topBand.rectTransform,
                "SubTitle",
                "1~3 fighters · MAIN + RESERVE 1 + RESERVE 2",
                22,
                FontStyle.Normal,
                TextAnchor.MiddleLeft,
                new Color(0.48f, 0.60f, 0.82f),
                new Vector2(0.04f, 0.06f),
                new Vector2(0.70f, 0.40f),
                Vector2.zero,
                Vector2.zero
            );

            CreateText(
                topBand.rectTransform,
                "Controls",
                "1 / 2 / 3 ADD   ·   BACKSPACE UNDO   ·   C CLEAR   ·   ENTER BATTLE",
                17,
                FontStyle.Bold,
                TextAnchor.MiddleRight,
                new Color(0.60f, 0.68f, 0.82f),
                new Vector2(0.56f, 0.08f),
                new Vector2(0.96f, 0.44f),
                Vector2.zero,
                Vector2.zero
            );

            Image rosterPanel = CreatePanel(
                background.rectTransform,
                "RosterPanel",
                new Vector2(0.035f, 0.28f),
                new Vector2(0.60f, 0.79f),
                Vector2.zero,
                Vector2.zero,
                new Color(0.016f, 0.022f, 0.040f, 0.98f)
            );

            CreateText(
                rosterPanel.rectTransform,
                "RosterHeader",
                "ROSTER",
                25,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.78f, 0.84f, 0.96f),
                new Vector2(0.035f, 0.84f),
                new Vector2(0.96f, 0.97f),
                Vector2.zero,
                Vector2.zero
            );

            float cardWidth = 0.285f;
            for (int index = 0; index < Roster.Length; index++)
            {
                PrototypeCharacterId characterId = Roster[index];
                CharacterPresentationProfile profile = CharacterPresentationProfiles.Get(characterId);
                float xMin = 0.035f + index * (cardWidth + 0.035f);
                float xMax = xMin + cardWidth;

                Button card = CreateButton(
                    rosterPanel.rectTransform,
                    $"Roster_{profile.HudName}",
                    BuildRosterCardText(index, profile),
                    21,
                    new Vector2(xMin, 0.12f),
                    new Vector2(xMax, 0.80f),
                    profile.HudAccent
                );

                int captured = index;
                card.onClick.AddListener(() => SelectRosterCharacter(Roster[captured]));
                rosterButtons[index] = card;
                rosterButtonImages[index] = card.GetComponent<Image>();
            }

            Image previewPanel = CreatePanel(
                background.rectTransform,
                "PreviewPanel",
                new Vector2(0.63f, 0.28f),
                new Vector2(0.965f, 0.79f),
                Vector2.zero,
                Vector2.zero,
                new Color(0.018f, 0.023f, 0.042f, 0.98f)
            );

            previewName = CreateText(
                previewPanel.rectTransform,
                "PreviewName",
                string.Empty,
                34,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Color.white,
                new Vector2(0.07f, 0.72f),
                new Vector2(0.94f, 0.92f),
                Vector2.zero,
                Vector2.zero
            );

            previewVariant = CreateText(
                previewPanel.rectTransform,
                "PreviewVariant",
                string.Empty,
                21,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.60f, 0.68f, 0.82f),
                new Vector2(0.07f, 0.61f),
                new Vector2(0.94f, 0.73f),
                Vector2.zero,
                Vector2.zero
            );

            previewSkills = CreateText(
                previewPanel.rectTransform,
                "PreviewSkills",
                string.Empty,
                22,
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                Color.white,
                new Vector2(0.07f, 0.27f),
                new Vector2(0.94f, 0.60f),
                Vector2.zero,
                Vector2.zero
            );

            previewSkills.horizontalOverflow = HorizontalWrapMode.Wrap;
            previewSkills.verticalOverflow = VerticalWrapMode.Overflow;

            previewHint = CreateText(
                previewPanel.rectTransform,
                "PreviewHint",
                "Click card or press its number to add it to the next open slot.",
                17,
                FontStyle.Normal,
                TextAnchor.LowerLeft,
                new Color(0.48f, 0.56f, 0.70f),
                new Vector2(0.07f, 0.07f),
                new Vector2(0.94f, 0.25f),
                Vector2.zero,
                Vector2.zero
            );

            Image teamPanel = CreatePanel(
                background.rectTransform,
                "TeamFormationPanel",
                new Vector2(0.035f, 0.055f),
                new Vector2(0.965f, 0.245f),
                Vector2.zero,
                Vector2.zero,
                new Color(0.018f, 0.024f, 0.044f, 0.98f)
            );

            CreateText(
                teamPanel.rectTransform,
                "TeamHeader",
                "TEAM FORMATION",
                21,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.72f, 0.80f, 0.94f),
                new Vector2(0.02f, 0.72f),
                new Vector2(0.23f, 0.96f),
                Vector2.zero,
                Vector2.zero
            );

            string[] slotNames = { "MAIN", "RESERVE 1", "RESERVE 2" };
            for (int slot = 0; slot < MaxTeamSize; slot++)
            {
                float xMin = 0.02f + slot * 0.205f;
                float xMax = xMin + 0.19f;
                Button slotButton = CreateButton(
                    teamPanel.rectTransform,
                    $"Slot_{slot}",
                    $"{slotNames[slot]}\n— EMPTY —",
                    18,
                    new Vector2(xMin, 0.12f),
                    new Vector2(xMax, 0.69f),
                    new Color(0.22f, 0.28f, 0.42f)
                );
                int capturedSlot = slot;
                slotButton.onClick.AddListener(() => RemoveSelectionAt(capturedSlot));
                slotTexts[slot] = slotButton.GetComponentInChildren<Text>();
            }

            Button undoButton = CreateButton(
                teamPanel.rectTransform,
                "UndoButton",
                "UNDO",
                18,
                new Vector2(0.65f, 0.12f),
                new Vector2(0.74f, 0.69f),
                new Color(0.30f, 0.36f, 0.52f)
            );
            undoButton.onClick.AddListener(RemoveLastSelection);

            Button clearButton = CreateButton(
                teamPanel.rectTransform,
                "ClearButton",
                "CLEAR",
                18,
                new Vector2(0.75f, 0.12f),
                new Vector2(0.84f, 0.69f),
                new Color(0.34f, 0.28f, 0.34f)
            );
            clearButton.onClick.AddListener(ClearSelection);

            startButton = CreateButton(
                teamPanel.rectTransform,
                "BattleButton",
                "BATTLE",
                23,
                new Vector2(0.855f, 0.12f),
                new Vector2(0.98f, 0.69f),
                new Color(0.18f, 0.66f, 1f)
            );
            startButton.onClick.AddListener(StartBattle);
            startButtonImage = startButton.GetComponent<Image>();

            statusText = CreateText(
                teamPanel.rectTransform,
                "Status",
                string.Empty,
                16,
                FontStyle.Bold,
                TextAnchor.MiddleRight,
                new Color(0.62f, 0.72f, 0.90f),
                new Vector2(0.52f, 0.73f),
                new Vector2(0.98f, 0.96f),
                Vector2.zero,
                Vector2.zero
            );
        }

        private void SelectRosterCharacter(PrototypeCharacterId characterId)
        {
            PreviewCharacter(characterId);

            if (selectedTeam.Contains(characterId))
            {
                SetStatus("Already assigned · click its slot to remove.");
                RefreshUi();
                return;
            }

            if (selectedTeam.Count >= MaxTeamSize)
            {
                SetStatus("Team full · remove a slot before adding another fighter.");
                RefreshUi();
                return;
            }

            selectedTeam.Add(characterId);
            SetStatus($"{CharacterPresentationProfiles.Get(characterId).HudName} assigned.");
            RefreshUi();
        }

        private void PreviewCharacter(PrototypeCharacterId characterId)
        {
            previewCharacter = characterId;
            CharacterPresentationProfile profile = CharacterPresentationProfiles.Get(characterId);

            if (previewName != null)
            {
                previewName.text = profile.DisplayName;
                previewName.color = profile.HudAccent;
            }
            if (previewVariant != null)
            {
                previewVariant.text = $"VARIANT · {profile.VariantLabel}";
            }
            if (previewSkills != null)
            {
                previewSkills.text =
                    $"Q  {profile.Skill1.Label}\n"
                    + $"E  {profile.Skill2.Label}\n"
                    + $"R  {profile.Ultimate.Label}\n"
                    + $"V  {profile.Domain.Label}";
            }
        }

        private void RemoveLastSelection()
        {
            if (selectedTeam.Count <= 0)
            {
                SetStatus("No fighter to remove.");
                RefreshUi();
                return;
            }

            selectedTeam.RemoveAt(selectedTeam.Count - 1);
            SetStatus("Last slot removed.");
            RefreshUi();
        }

        private void RemoveSelectionAt(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= selectedTeam.Count)
            {
                return;
            }

            selectedTeam.RemoveAt(slotIndex);
            SetStatus("Team slots compacted.");
            RefreshUi();
        }

        private void ClearSelection()
        {
            selectedTeam.Clear();
            SetStatus("Team cleared.");
            RefreshUi();
        }

        private void StartBattle()
        {
            if (selectedTeam.Count <= 0)
            {
                SetStatus("Select at least one fighter.");
                RefreshUi();
                return;
            }

            MatchTeamSelection selection = selectedTeam.Count switch
            {
                1 => MatchTeamSelection.Solo(selectedTeam[0]),
                2 => MatchTeamSelection.Duo(selectedTeam[0], selectedTeam[1]),
                _ => MatchTeamSelection.Trio(
                    selectedTeam[0],
                    selectedTeam[1],
                    selectedTeam[2]
                ),
            };

            MatchTeamSelectionStore.SetPlayerTeam(selection);
            SceneManager.LoadScene(BattleSceneName);
        }

        private void RefreshUi()
        {
            for (int index = 0; index < Roster.Length; index++)
            {
                PrototypeCharacterId characterId = Roster[index];
                bool assigned = selectedTeam.Contains(characterId);
                CharacterPresentationProfile profile = CharacterPresentationProfiles.Get(characterId);
                if (rosterButtonImages[index] != null)
                {
                    rosterButtonImages[index].color = assigned
                        ? Color.Lerp(new Color(0.045f, 0.055f, 0.085f, 1f), profile.HudAccent, 0.42f)
                        : new Color(0.045f, 0.055f, 0.085f, 1f);
                }
            }

            string[] slotNames = { "MAIN", "RESERVE 1", "RESERVE 2" };
            for (int slot = 0; slot < MaxTeamSize; slot++)
            {
                if (slotTexts[slot] == null)
                {
                    continue;
                }

                if (slot < selectedTeam.Count)
                {
                    CharacterPresentationProfile profile =
                        CharacterPresentationProfiles.Get(selectedTeam[slot]);
                    slotTexts[slot].text = $"{slotNames[slot]}\n{profile.HudName} · {profile.CompactVariantLabel}";
                    slotTexts[slot].color = profile.HudAccent;
                }
                else
                {
                    slotTexts[slot].text = $"{slotNames[slot]}\n— EMPTY —";
                    slotTexts[slot].color = new Color(0.55f, 0.60f, 0.70f);
                }
            }

            bool canStart = selectedTeam.Count > 0;
            if (startButton != null)
            {
                startButton.interactable = canStart;
            }
            if (startButtonImage != null)
            {
                startButtonImage.color = canStart
                    ? new Color(0.10f, 0.48f, 0.88f, 1f)
                    : new Color(0.16f, 0.19f, 0.26f, 1f);
            }

            if (statusText != null && string.IsNullOrEmpty(statusText.text))
            {
                statusText.text =
                    selectedTeam.Count <= 0
                        ? "Select 1~3 fighters."
                        : $"{selectedTeam.Count}/3 fighters ready.";
            }
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private string BuildRosterCardText(int index, CharacterPresentationProfile profile)
        {
            return
                $"{index + 1}   {profile.HudName}\n"
                + $"{profile.CompactVariantLabel}\n\n"
                + $"Q {profile.Skill1.Label}\n"
                + $"E {profile.Skill2.Label}\n"
                + $"R {profile.Ultimate.Label}\n"
                + $"V {profile.Domain.Label}";
        }

        private Button CreateButton(
            RectTransform parent,
            string objectName,
            string label,
            int fontSize,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color accent
        )
        {
            GameObject buttonObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button)
            );
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = buttonObject.GetComponent<Image>();
            image.color = Color.Lerp(new Color(0.045f, 0.055f, 0.085f, 1f), accent, 0.14f);

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.Lerp(Color.white, accent, 0.25f);
            colors.pressedColor = Color.Lerp(Color.white, accent, 0.48f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.45f, 0.45f, 0.50f, 0.55f);
            colors.colorMultiplier = 1f;
            button.colors = colors;

            Text text = CreateText(
                rect,
                "Label",
                label,
                fontSize,
                FontStyle.Bold,
                TextAnchor.MiddleCenter,
                Color.white,
                Vector2.zero,
                Vector2.one,
                new Vector2(10f, 8f),
                new Vector2(-10f, -8f)
            );
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            return button;
        }

        private Image CreatePanel(
            RectTransform parent,
            string objectName,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax,
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
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Image image = panelObject.GetComponent<Image>();
            image.color = color;
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
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax
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
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            Text text = textObject.GetComponent<Text>();
            text.font = uiFont;
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.supportRichText = true;
            return text;
        }

        private static Font CreateRuntimeFont()
        {
            Font font = Font.CreateDynamicFontFromOSFont(
                new[] { "Malgun Gothic", "Arial", "Liberation Sans" },
                24
            );

            if (font != null)
            {
                return font;
            }

            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule)
            );
            DontDestroyOnLoad(eventSystemObject);
        }

        private static bool IsSelectHostScene(string sceneName)
        {
            return sceneName == HostSceneName || sceneName == LegacyHostSceneName;
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
