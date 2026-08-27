using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JJKGame.Player
{
    /// <summary>
    /// Gate 5B Character / Team Select front-end.
    /// CharacterSelect is the dedicated pre-match host scene. SampleScene remains accepted
    /// only as a developer compatibility host. The UI writes MatchTeamSelectionStore and
    /// never touches battle-scene fighter GameObjects directly.
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
        private readonly Text[] rosterBadgeTexts = new Text[Roster.Length];
        private readonly Text[] slotTexts = new Text[MaxTeamSize];
        private readonly Image[] slotImages = new Image[MaxTeamSize];

        private Font uiFont;
        private Text previewName;
        private Text previewVariant;
        private Text previewRole;
        private Text previewDescription;
        private Text previewSkills;
        private Text previewMonogram;
        private Text teamCounterText;
        private Text statusText;
        private Image previewAccentBar;
        private Image previewGlow;
        private Image backgroundGlow;
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

            AnimateVisualShell();
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
                new Color(0.006f, 0.009f, 0.020f, 1f)
            );

            backgroundGlow = CreatePanel(
                background.rectTransform,
                "BackgroundGlow",
                new Vector2(0.48f, 0.08f),
                new Vector2(1.03f, 0.96f),
                Vector2.zero,
                Vector2.zero,
                new Color(0.05f, 0.12f, 0.24f, 0.18f)
            );

            Image leftRail = CreatePanel(
                background.rectTransform,
                "LeftRail",
                new Vector2(0f, 0f),
                new Vector2(0.012f, 1f),
                Vector2.zero,
                Vector2.zero,
                new Color(0.10f, 0.46f, 0.84f, 1f)
            );

            CreatePanel(
                background.rectTransform,
                "TopDivider",
                new Vector2(0.035f, 0.805f),
                new Vector2(0.965f, 0.809f),
                Vector2.zero,
                Vector2.zero,
                new Color(0.18f, 0.28f, 0.48f, 0.78f)
            );

            BuildHeader(background.rectTransform);
            BuildRoster(background.rectTransform);
            BuildPreview(background.rectTransform);
            BuildTeamFormation(background.rectTransform);

            leftRail.raycastTarget = false;
            backgroundGlow.raycastTarget = false;
        }

        private void BuildHeader(RectTransform parent)
        {
            CreateText(
                parent,
                "Eyebrow",
                "JJK ARENA / PRE-MATCH",
                16,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.30f, 0.58f, 0.90f),
                new Vector2(0.045f, 0.925f),
                new Vector2(0.44f, 0.965f),
                Vector2.zero,
                Vector2.zero
            );

            Text title = CreateText(
                parent,
                "Title",
                "CHARACTER / TEAM SELECT",
                44,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.94f, 0.97f, 1f),
                new Vector2(0.045f, 0.855f),
                new Vector2(0.62f, 0.935f),
                Vector2.zero,
                Vector2.zero
            );
            AddOutline(title, new Color(0f, 0f, 0f, 0.65f), new Vector2(1.5f, -1.5f));

            CreateText(
                parent,
                "SubTitle",
                "팀을 선택한 순서가 MAIN → RESERVE 1 → RESERVE 2가 됩니다.",
                19,
                FontStyle.Normal,
                TextAnchor.MiddleLeft,
                new Color(0.55f, 0.64f, 0.78f),
                new Vector2(0.045f, 0.815f),
                new Vector2(0.62f, 0.865f),
                Vector2.zero,
                Vector2.zero
            );

            CreateText(
                parent,
                "Controls",
                "1 / 2 / 3 선택   ·   BACKSPACE 되돌리기   ·   C 초기화   ·   ENTER 전투 시작",
                17,
                FontStyle.Bold,
                TextAnchor.MiddleRight,
                new Color(0.56f, 0.64f, 0.78f),
                new Vector2(0.53f, 0.885f),
                new Vector2(0.955f, 0.945f),
                Vector2.zero,
                Vector2.zero
            );
        }

        private void BuildRoster(RectTransform parent)
        {
            Image rosterPanel = CreatePanel(
                parent,
                "RosterPanel",
                new Vector2(0.045f, 0.30f),
                new Vector2(0.565f, 0.785f),
                Vector2.zero,
                Vector2.zero,
                new Color(0.014f, 0.020f, 0.037f, 0.98f)
            );

            CreatePanel(
                rosterPanel.rectTransform,
                "RosterAccent",
                new Vector2(0f, 0f),
                new Vector2(0.006f, 1f),
                Vector2.zero,
                Vector2.zero,
                new Color(0.20f, 0.48f, 0.84f, 0.84f)
            );

            CreateText(
                rosterPanel.rectTransform,
                "RosterHeader",
                "FIGHTER ROSTER",
                24,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.80f, 0.86f, 0.96f),
                new Vector2(0.035f, 0.84f),
                new Vector2(0.96f, 0.96f),
                Vector2.zero,
                Vector2.zero
            );

            CreateText(
                rosterPanel.rectTransform,
                "RosterHelp",
                "카드에 마우스를 올리면 상세 정보 · 클릭하면 다음 팀 슬롯에 배치",
                15,
                FontStyle.Normal,
                TextAnchor.MiddleLeft,
                new Color(0.44f, 0.52f, 0.66f),
                new Vector2(0.035f, 0.77f),
                new Vector2(0.96f, 0.85f),
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
                    20,
                    new Vector2(xMin, 0.10f),
                    new Vector2(xMax, 0.73f),
                    profile.HudAccent
                );

                int captured = index;
                card.onClick.AddListener(() => SelectRosterCharacter(Roster[captured]));
                AddPointerEnter(card.gameObject, () => PreviewCharacter(Roster[captured]));

                rosterButtons[index] = card;
                rosterButtonImages[index] = card.GetComponent<Image>();

                Text badge = CreateText(
                    card.GetComponent<RectTransform>(),
                    "SelectionBadge",
                    string.Empty,
                    14,
                    FontStyle.Bold,
                    TextAnchor.MiddleCenter,
                    Color.white,
                    new Vector2(0.58f, 0.87f),
                    new Vector2(0.94f, 0.97f),
                    Vector2.zero,
                    Vector2.zero
                );
                badge.gameObject.SetActive(false);
                rosterBadgeTexts[index] = badge;
            }
        }

        private void BuildPreview(RectTransform parent)
        {
            Image previewPanel = CreatePanel(
                parent,
                "PreviewPanel",
                new Vector2(0.59f, 0.30f),
                new Vector2(0.955f, 0.785f),
                Vector2.zero,
                Vector2.zero,
                new Color(0.014f, 0.019f, 0.035f, 0.985f)
            );

            previewGlow = CreatePanel(
                previewPanel.rectTransform,
                "PreviewGlow",
                new Vector2(0.52f, 0f),
                new Vector2(1f, 1f),
                Vector2.zero,
                Vector2.zero,
                new Color(0.12f, 0.38f, 0.72f, 0.10f)
            );
            previewGlow.raycastTarget = false;

            previewAccentBar = CreatePanel(
                previewPanel.rectTransform,
                "PreviewAccentBar",
                new Vector2(0f, 0f),
                new Vector2(0.012f, 1f),
                Vector2.zero,
                Vector2.zero,
                new Color(0.18f, 0.66f, 1f, 1f)
            );

            previewMonogram = CreateText(
                previewPanel.rectTransform,
                "PreviewMonogram",
                "GOJO",
                74,
                FontStyle.Bold,
                TextAnchor.MiddleRight,
                new Color(1f, 1f, 1f, 0.055f),
                new Vector2(0.34f, 0.58f),
                new Vector2(0.94f, 0.96f),
                Vector2.zero,
                Vector2.zero
            );

            CreateText(
                previewPanel.rectTransform,
                "PreviewHeader",
                "FIGHTER PROFILE",
                15,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.48f, 0.62f, 0.84f),
                new Vector2(0.065f, 0.88f),
                new Vector2(0.50f, 0.96f),
                Vector2.zero,
                Vector2.zero
            );

            previewName = CreateText(
                previewPanel.rectTransform,
                "PreviewName",
                string.Empty,
                31,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                Color.white,
                new Vector2(0.065f, 0.73f),
                new Vector2(0.92f, 0.88f),
                Vector2.zero,
                Vector2.zero
            );

            previewVariant = CreateText(
                previewPanel.rectTransform,
                "PreviewVariant",
                string.Empty,
                18,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.60f, 0.68f, 0.82f),
                new Vector2(0.065f, 0.65f),
                new Vector2(0.92f, 0.74f),
                Vector2.zero,
                Vector2.zero
            );

            previewRole = CreateText(
                previewPanel.rectTransform,
                "PreviewRole",
                string.Empty,
                17,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.82f, 0.86f, 0.94f),
                new Vector2(0.065f, 0.55f),
                new Vector2(0.92f, 0.65f),
                Vector2.zero,
                Vector2.zero
            );

            previewDescription = CreateText(
                previewPanel.rectTransform,
                "PreviewDescription",
                string.Empty,
                17,
                FontStyle.Normal,
                TextAnchor.UpperLeft,
                new Color(0.68f, 0.73f, 0.82f),
                new Vector2(0.065f, 0.38f),
                new Vector2(0.92f, 0.55f),
                Vector2.zero,
                Vector2.zero
            );
            previewDescription.horizontalOverflow = HorizontalWrapMode.Wrap;
            previewDescription.verticalOverflow = VerticalWrapMode.Overflow;

            CreateText(
                previewPanel.rectTransform,
                "SkillHeader",
                "TECHNIQUE LOADOUT",
                14,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.48f, 0.62f, 0.84f),
                new Vector2(0.065f, 0.30f),
                new Vector2(0.92f, 0.38f),
                Vector2.zero,
                Vector2.zero
            );

            previewSkills = CreateText(
                previewPanel.rectTransform,
                "PreviewSkills",
                string.Empty,
                18,
                FontStyle.Bold,
                TextAnchor.UpperLeft,
                Color.white,
                new Vector2(0.065f, 0.07f),
                new Vector2(0.92f, 0.30f),
                Vector2.zero,
                Vector2.zero
            );
            previewSkills.horizontalOverflow = HorizontalWrapMode.Wrap;
            previewSkills.verticalOverflow = VerticalWrapMode.Overflow;
        }

        private void BuildTeamFormation(RectTransform parent)
        {
            Image teamPanel = CreatePanel(
                parent,
                "TeamFormationPanel",
                new Vector2(0.045f, 0.055f),
                new Vector2(0.955f, 0.255f),
                Vector2.zero,
                Vector2.zero,
                new Color(0.014f, 0.020f, 0.038f, 0.985f)
            );

            CreateText(
                teamPanel.rectTransform,
                "TeamHeader",
                "TEAM FORMATION",
                20,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.72f, 0.80f, 0.94f),
                new Vector2(0.02f, 0.76f),
                new Vector2(0.23f, 0.96f),
                Vector2.zero,
                Vector2.zero
            );

            teamCounterText = CreateText(
                teamPanel.rectTransform,
                "TeamCounter",
                "0 / 3 READY",
                15,
                FontStyle.Bold,
                TextAnchor.MiddleLeft,
                new Color(0.42f, 0.58f, 0.82f),
                new Vector2(0.20f, 0.76f),
                new Vector2(0.35f, 0.96f),
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
                    17,
                    new Vector2(xMin, 0.12f),
                    new Vector2(xMax, 0.70f),
                    new Color(0.20f, 0.28f, 0.44f)
                );
                int capturedSlot = slot;
                slotButton.onClick.AddListener(() => RemoveSelectionAt(capturedSlot));
                slotTexts[slot] = slotButton.GetComponentInChildren<Text>();
                slotImages[slot] = slotButton.GetComponent<Image>();
            }

            Button undoButton = CreateButton(
                teamPanel.rectTransform,
                "UndoButton",
                "UNDO",
                16,
                new Vector2(0.65f, 0.12f),
                new Vector2(0.74f, 0.70f),
                new Color(0.26f, 0.34f, 0.50f)
            );
            undoButton.onClick.AddListener(RemoveLastSelection);

            Button clearButton = CreateButton(
                teamPanel.rectTransform,
                "ClearButton",
                "CLEAR",
                16,
                new Vector2(0.75f, 0.12f),
                new Vector2(0.84f, 0.70f),
                new Color(0.38f, 0.22f, 0.30f)
            );
            clearButton.onClick.AddListener(ClearSelection);

            startButton = CreateButton(
                teamPanel.rectTransform,
                "BattleButton",
                "BATTLE",
                22,
                new Vector2(0.855f, 0.12f),
                new Vector2(0.98f, 0.70f),
                new Color(0.18f, 0.66f, 1f)
            );
            startButton.onClick.AddListener(StartBattle);
            startButtonImage = startButton.GetComponent<Image>();

            statusText = CreateText(
                teamPanel.rectTransform,
                "Status",
                string.Empty,
                15,
                FontStyle.Bold,
                TextAnchor.MiddleRight,
                new Color(0.58f, 0.68f, 0.86f),
                new Vector2(0.48f, 0.76f),
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
                SetStatus("이미 팀에 배치됨 · 아래 팀 슬롯을 클릭하면 제거됩니다.");
                RefreshUi();
                return;
            }

            if (selectedTeam.Count >= MaxTeamSize)
            {
                SetStatus("팀이 가득 찼습니다 · 슬롯을 비운 뒤 다시 선택하세요.");
                RefreshUi();
                return;
            }

            selectedTeam.Add(characterId);
            CharacterPresentationProfile profile = CharacterPresentationProfiles.Get(characterId);
            SetStatus($"{profile.HudName} → {GetSlotLabel(selectedTeam.Count - 1)} 배치");
            RefreshUi();
        }

        private void PreviewCharacter(PrototypeCharacterId characterId)
        {
            previewCharacter = characterId;
            CharacterPresentationProfile profile = CharacterPresentationProfiles.Get(characterId);
            CharacterSelectPresentationData selectData =
                CharacterSelectPresentationProfiles.Get(characterId);

            if (previewName != null)
            {
                previewName.text = profile.DisplayName;
                previewName.color = profile.HudAccent;
            }

            if (previewVariant != null)
            {
                previewVariant.text = $"VARIANT · {profile.VariantLabel}";
            }

            if (previewRole != null)
            {
                previewRole.text = $"{selectData.RoleLabel}   /   {selectData.StyleLabel}";
            }

            if (previewDescription != null)
            {
                previewDescription.text = selectData.Description;
            }

            if (previewSkills != null)
            {
                previewSkills.text =
                    $"Q   {profile.Skill1.Label}\n"
                    + $"E   {profile.Skill2.Label}\n"
                    + $"R   {profile.Ultimate.Label}\n"
                    + $"V   {profile.Domain.Label}";
            }

            if (previewMonogram != null)
            {
                previewMonogram.text = profile.HudName;
            }

            if (previewAccentBar != null)
            {
                previewAccentBar.color = profile.HudAccent;
            }

            if (previewGlow != null)
            {
                Color glow = profile.HudAccent;
                glow.a = 0.12f;
                previewGlow.color = glow;
            }
        }

        private void RemoveLastSelection()
        {
            if (selectedTeam.Count <= 0)
            {
                SetStatus("제거할 파이터가 없습니다.");
                RefreshUi();
                return;
            }

            CharacterPresentationProfile removed =
                CharacterPresentationProfiles.Get(selectedTeam[selectedTeam.Count - 1]);
            selectedTeam.RemoveAt(selectedTeam.Count - 1);
            SetStatus($"{removed.HudName} 배치 취소");
            RefreshUi();
        }

        private void RemoveSelectionAt(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= selectedTeam.Count)
            {
                return;
            }

            CharacterPresentationProfile removed =
                CharacterPresentationProfiles.Get(selectedTeam[slotIndex]);
            selectedTeam.RemoveAt(slotIndex);
            SetStatus($"{removed.HudName} 제거 · 뒤 슬롯 자동 정렬");
            RefreshUi();
        }

        private void ClearSelection()
        {
            selectedTeam.Clear();
            SetStatus("팀 편성을 초기화했습니다.");
            RefreshUi();
        }

        private void StartBattle()
        {
            if (selectedTeam.Count <= 0)
            {
                SetStatus("최소 1명의 파이터를 선택하세요.");
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
                int selectedIndex = selectedTeam.IndexOf(characterId);
                bool assigned = selectedIndex >= 0;
                CharacterPresentationProfile profile = CharacterPresentationProfiles.Get(characterId);

                if (rosterButtonImages[index] != null)
                {
                    rosterButtonImages[index].color = assigned
                        ? Color.Lerp(new Color(0.035f, 0.045f, 0.072f, 1f), profile.HudAccent, 0.40f)
                        : new Color(0.035f, 0.045f, 0.072f, 0.98f);
                }

                if (rosterBadgeTexts[index] != null)
                {
                    rosterBadgeTexts[index].gameObject.SetActive(assigned);
                    rosterBadgeTexts[index].text = assigned ? GetSlotLabel(selectedIndex) : string.Empty;
                    rosterBadgeTexts[index].color = assigned ? profile.HudAccent : Color.white;
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
                    slotTexts[slot].text =
                        $"{slotNames[slot]}\n{profile.HudName} · {profile.CompactVariantLabel}\nCLICK TO REMOVE";
                    slotTexts[slot].color = profile.HudAccent;

                    if (slotImages[slot] != null)
                    {
                        slotImages[slot].color = Color.Lerp(
                            new Color(0.035f, 0.045f, 0.072f, 1f),
                            profile.HudAccent,
                            0.26f
                        );
                    }
                }
                else
                {
                    slotTexts[slot].text = $"{slotNames[slot]}\n— EMPTY —";
                    slotTexts[slot].color = new Color(0.50f, 0.56f, 0.68f);

                    if (slotImages[slot] != null)
                    {
                        slotImages[slot].color = new Color(0.035f, 0.045f, 0.072f, 0.98f);
                    }
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
                    ? new Color(0.08f, 0.50f, 0.92f, 1f)
                    : new Color(0.14f, 0.17f, 0.24f, 1f);
            }

            if (teamCounterText != null)
            {
                teamCounterText.text = $"{selectedTeam.Count} / {MaxTeamSize} READY";
                teamCounterText.color = canStart
                    ? new Color(0.28f, 0.72f, 1f)
                    : new Color(0.42f, 0.52f, 0.68f);
            }

            if (statusText != null && string.IsNullOrEmpty(statusText.text))
            {
                statusText.text =
                    selectedTeam.Count <= 0
                        ? "1~3명의 파이터를 선택하세요."
                        : $"{selectedTeam.Count}명 편성 완료 · BATTLE 준비";
            }
        }

        private void AnimateVisualShell()
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 1.6f);

            if (backgroundGlow != null)
            {
                Color color = backgroundGlow.color;
                color.a = Mathf.Lerp(0.10f, 0.19f, pulse);
                backgroundGlow.color = color;
            }

            if (previewGlow != null)
            {
                CharacterPresentationProfile profile =
                    CharacterPresentationProfiles.Get(previewCharacter);
                Color color = profile.HudAccent;
                color.a = Mathf.Lerp(0.07f, 0.14f, pulse);
                previewGlow.color = color;
            }
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private static string GetSlotLabel(int slotIndex)
        {
            return slotIndex switch
            {
                0 => "MAIN",
                1 => "R1",
                2 => "R2",
                _ => string.Empty,
            };
        }

        private static string BuildRosterCardText(
            int index,
            CharacterPresentationProfile profile
        )
        {
            return
                $"0{index + 1}\n"
                + $"{profile.HudName}\n"
                + $"{profile.CompactVariantLabel}\n\n"
                + $"{profile.Skill1.Label}  /  {profile.Ultimate.Label}";
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
            image.color = Color.Lerp(new Color(0.035f, 0.045f, 0.072f, 1f), accent, 0.13f);

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.Lerp(Color.white, accent, 0.22f);
            colors.pressedColor = Color.Lerp(Color.white, accent, 0.42f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.40f, 0.42f, 0.48f, 0.52f);
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

        private static void AddOutline(Text text, Color color, Vector2 distance)
        {
            Outline outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        private static void AddPointerEnter(GameObject target, UnityEngine.Events.UnityAction action)
        {
            EventTrigger trigger = target.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = target.AddComponent<EventTrigger>();
            }

            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter,
            };
            entry.callback.AddListener(_ => action());
            trigger.triggers.Add(entry);
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
