using JJKGame.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.Player
{
    public enum PrototypeCharacterId
    {
        GojoModern,
        SukunaShibuyaYujiBody,
        MegumiStudent,
    }

    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class PrototypeCharacterController : MonoBehaviour
    {
        private static PrototypeCharacterId selectedCharacter = PrototypeCharacterId.GojoModern;

        private Health health;
        private CursedEnergyController cursedEnergy;
        private SukunaDomainController sukunaDomain;
        private PrototypeCharacterId activeCharacter;
        private bool showSukunaHelp;
        private GUIStyle headerStyle;
        private GUIStyle valueStyle;
        private GUIStyle smallStyle;
        private GUIStyle centerStyle;
        private int styledForHeight = -1;

        public static PrototypeCharacterId SelectedCharacter => selectedCharacter;
        public PrototypeCharacterId ActiveCharacter => activeCharacter;
        public bool IsSukuna => activeCharacter == PrototypeCharacterId.SukunaShibuyaYujiBody;
        public bool IsMegumi => activeCharacter == PrototypeCharacterId.MegumiStudent;
        public CharacterPresentationProfile PresentationProfile => CharacterPresentationProfiles.Get(activeCharacter);
        public string DisplayName => PresentationProfile.DisplayName;

        public static PrototypeCharacterController GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            PrototypeCharacterController controller = owner.GetComponent<PrototypeCharacterController>();
            return controller != null
                ? controller
                : owner.AddComponent<PrototypeCharacterController>();
        }

        private void Awake()
        {
            health = GetComponent<Health>();
            cursedEnergy = CursedEnergyController.GetOrCreate(gameObject);
            activeCharacter = selectedCharacter;
        }

        private void Start()
        {
            ApplyCharacter(selectedCharacter, true);
            PrototypePlayerTeamController.GetOrCreate(gameObject);
        }

        private void Update()
        {
            PrototypePlayerTeamController team = GetComponent<PrototypePlayerTeamController>();
            bool teamMode = team != null && team.enabled;

            if (!teamMode)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    SelectAndReload(PrototypeCharacterId.GojoModern);
                    return;
                }

                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    SelectAndReload(PrototypeCharacterId.SukunaShibuyaYujiBody);
                    return;
                }

                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    SelectAndReload(PrototypeCharacterId.MegumiStudent);
                    return;
                }
            }

            if (!teamMode && IsSukuna && Input.GetKeyDown(KeyCode.F1))
            {
                showSukunaHelp = !showSukunaHelp;
            }
        }

        public void ApplyCharacter(PrototypeCharacterId nextCharacter, bool resetVitals)
        {
            selectedCharacter = nextCharacter;
            activeCharacter = nextCharacter;
            showSukunaHelp = false;

            switch (nextCharacter)
            {
                case PrototypeCharacterId.SukunaShibuyaYujiBody:
                    ApplySukuna(resetVitals);
                    break;
                case PrototypeCharacterId.MegumiStudent:
                    ApplyMegumi(resetVitals);
                    break;
                default:
                    ApplyGojo(resetVitals);
                    break;
            }

            if (resetVitals)
            {
                health?.ResetHealth();
            }
        }

        private void SelectAndReload(PrototypeCharacterId nextCharacter)
        {
            if (selectedCharacter == nextCharacter)
            {
                return;
            }

            selectedCharacter = nextCharacter;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void ApplyGojo(bool refillEnergy)
        {
            SetGojoComponentsEnabled(true);
            DisableSukunaComponents();
            DisableMegumiComponents();

            GojoPrototypeAvatar gojoAvatar = GojoPrototypeAvatar.GetOrCreate(gameObject);
            gojoAvatar.enabled = true;
            SetChildActive("PrototypeGojoAvatar", true);

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SixEyesEfficiency, refillEnergy);
        }

        private void ApplySukuna(bool refillEnergy)
        {
            GojoDomainController gojoDomain = GetComponent<GojoDomainController>();
            if (gojoDomain != null)
            {
                gojoDomain.ResetCommand();
            }
            SetGojoComponentsEnabled(false);
            SetChildActive("PrototypeGojoAvatar", false);
            DisableMegumiComponents();

            SukunaTechniqueController sukunaTechnique = GetComponent<SukunaTechniqueController>();
            if (sukunaTechnique == null)
            {
                sukunaTechnique = gameObject.AddComponent<SukunaTechniqueController>();
            }
            sukunaTechnique.enabled = true;

            sukunaDomain = SukunaDomainController.GetOrCreate(gameObject);
            sukunaDomain.enabled = true;

            SukunaPrototypeAvatar sukunaAvatar = SukunaPrototypeAvatar.GetOrCreate(gameObject);
            sukunaAvatar.enabled = true;
            SetChildActive("PrototypeSukunaAvatar", true);

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SukunaShibuyaReserve, refillEnergy);
        }

        private void ApplyMegumi(bool refillEnergy)
        {
            GojoDomainController gojoDomain = GetComponent<GojoDomainController>();
            if (gojoDomain != null)
            {
                gojoDomain.ResetCommand();
            }
            SetGojoComponentsEnabled(false);
            SetChildActive("PrototypeGojoAvatar", false);
            DisableSukunaComponents();

            MegumiTechniqueController megumiTechnique = MegumiTechniqueController.GetOrCreate(gameObject);
            megumiTechnique.enabled = true;

            MegumiPrototypeAvatar megumiAvatar = MegumiPrototypeAvatar.GetOrCreate(gameObject);
            megumiAvatar.enabled = true;
            SetChildActive(MegumiPrototypeAvatar.VisualRootName, true);

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            cursedEnergy?.ApplyProfile(CursedEnergyProfileId.Standard, refillEnergy);
        }

        private void DisableSukunaComponents()
        {
            SukunaTechniqueController sukunaTechnique = GetComponent<SukunaTechniqueController>();
            if (sukunaTechnique != null)
            {
                sukunaTechnique.enabled = false;
            }

            sukunaDomain = GetComponent<SukunaDomainController>();
            if (sukunaDomain != null)
            {
                sukunaDomain.ResetDomain();
                sukunaDomain.enabled = false;
            }

            SukunaPrototypeAvatar sukunaAvatar = GetComponent<SukunaPrototypeAvatar>();
            if (sukunaAvatar != null)
            {
                sukunaAvatar.enabled = false;
            }
            SetChildActive("PrototypeSukunaAvatar", false);
        }

        private void DisableMegumiComponents()
        {
            MegumiTechniqueController megumiTechnique = GetComponent<MegumiTechniqueController>();
            if (megumiTechnique != null)
            {
                megumiTechnique.enabled = false;
            }

            MegumiPrototypeAvatar megumiAvatar = GetComponent<MegumiPrototypeAvatar>();
            if (megumiAvatar != null)
            {
                megumiAvatar.enabled = false;
            }
            SetChildActive(MegumiPrototypeAvatar.VisualRootName, false);
        }

        private void SetGojoComponentsEnabled(bool enabledState)
        {
            SetEnabled(GetComponent<GojoTechniqueController>(), enabledState);
            SetEnabled(GetComponent<GojoTechniqueChainController>(), enabledState);
            SetEnabled(GetComponent<GojoDomainController>(), enabledState);
            SetEnabled(GetComponent<GojoInfinityDefense>(), enabledState);
            SetEnabled(GetComponent<TechniqueBurnoutController>(), enabledState);
            SetEnabled(GetComponent<GojoVariantController>(), enabledState);

            GojoPrototypeAvatar avatar = GetComponent<GojoPrototypeAvatar>();
            if (avatar != null)
            {
                avatar.enabled = enabledState;
            }
        }

        private static void SetEnabled(Behaviour behaviour, bool enabledState)
        {
            if (behaviour != null)
            {
                behaviour.enabled = enabledState;
            }
        }

        private void SetChildActive(string childName, bool active)
        {
            Transform child = transform.Find(childName);
            if (child != null)
            {
                child.gameObject.SetActive(active);
            }
        }

        private void OnGUI()
        {
            if (CombatHudPresentationMode.ProductionCanvasActive)
            {
                return;
            }

            EnsureStyles();
            DrawCharacterSwitchChip();

            if (!IsSukuna || health == null)
            {
                return;
            }

            PrototypePlayerTeamController team = GetComponent<PrototypePlayerTeamController>();
            bool teamMode = team != null && team.enabled;

            if (!teamMode)
            {
                DrawSukunaPlayerPanel();
            }

            DrawSukunaDomainPanel();
            if (!teamMode && showSukunaHelp)
            {
                DrawSukunaHelp();
            }
        }

        private void DrawCharacterSwitchChip()
        {
            PrototypePlayerTeamController team = GetComponent<PrototypePlayerTeamController>();
            if (team != null && team.enabled)
            {
                return;
            }

            CharacterPresentationProfile profile = PresentationProfile;
            CharacterPresentationProfile gojoProfile = CharacterPresentationProfiles.Get(PrototypeCharacterId.GojoModern);
            CharacterPresentationProfile sukunaProfile = CharacterPresentationProfiles.Get(PrototypeCharacterId.SukunaShibuyaYujiBody);
            CharacterPresentationProfile megumiProfile = CharacterPresentationProfiles.Get(PrototypeCharacterId.MegumiStudent);
            float width = 310f;
            Rect rect = new Rect(Screen.width - width - 12f, 108f, width, 24f);
            Color accent = profile.HudAccent;
            DrawRect(rect, new Color(0.018f, 0.020f, 0.032f, 0.92f));
            DrawBorder(rect, accent, 1f);
            smallStyle.normal.textColor = accent;
            GUI.Label(
                rect,
                $"1 · {gojoProfile.ShortName}    2 · {sukunaProfile.ShortName}    3 · {megumiProfile.ShortName}",
                smallStyle
            );
        }

        private void DrawSukunaPlayerPanel()
        {
            CharacterPresentationProfile profile = PresentationProfile;
            const float margin = 12f;
            float panelWidth = Mathf.Clamp((Screen.width - margin * 3f) * 0.36f, 230f, 340f);
            Rect rect = new Rect(margin, margin, panelWidth, 62f);
            Color accent = profile.HudAccent;

            DrawRect(rect, new Color(0.040f, 0.010f, 0.012f, 0.98f));
            DrawBorder(rect, accent, 2f);
            GUI.Label(
                new Rect(rect.x + 10f, rect.y + 3f, rect.width - 20f, 18f),
                $"PLAYER · {profile.DisplayName}",
                headerStyle
            );

            DrawValueBar(
                new Rect(rect.x + 10f, rect.y + 23f, rect.width - 20f, 18f),
                health.CurrentHealth,
                health.MaxHealth,
                accent,
                $"HP  {health.CurrentHealth:0} / {health.MaxHealth:0}"
            );

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            if (cursedEnergy != null)
            {
                DrawValueBar(
                    new Rect(rect.x + 10f, rect.y + 44f, rect.width - 20f, 12f),
                    cursedEnergy.CurrentEnergy,
                    cursedEnergy.MaxEnergy,
                    profile.EnergyAccent,
                    $"CE {cursedEnergy.CurrentEnergy:0}/{cursedEnergy.MaxEnergy:0} · {cursedEnergy.ProfileLabel}"
                );
            }
        }

        private void DrawSukunaDomainPanel()
        {
            sukunaDomain ??= GetComponent<SukunaDomainController>();
            CharacterPresentationProfile profile = PresentationProfile;
            float width = Mathf.Min(680f, Screen.width - 24f);
            Rect rect = new Rect((Screen.width - width) * 0.5f, Screen.height - 48f, width, 36f);
            Color accent = sukunaDomain != null && sukunaDomain.IsActive
                ? new Color(1f, 0.08f, 0.04f)
                : profile.HudAccent;
            DrawRect(rect, new Color(0.055f, 0.010f, 0.012f, 0.98f));
            DrawBorder(rect, accent, sukunaDomain != null && sukunaDomain.IsActive ? 3f : 2f);
            centerStyle.normal.textColor = Color.white;
            GUI.Label(
                new Rect(rect.x + 10f, rect.y + 2f, rect.width - 20f, 22f),
                sukunaDomain != null ? sukunaDomain.StatusText : $"V · {profile.Domain.Label} · 연결 안 됨",
                centerStyle
            );
            smallStyle.normal.textColor = new Color(0.92f, 0.66f, 0.60f);
            GUI.Label(
                new Rect(rect.x + 10f, rect.y + 20f, rect.width - 20f, 14f),
                "개방형 영역 · 반복 필중 참격 · V 직접 입력은 임시 · F1 도움말",
                smallStyle
            );
        }

        private void DrawSukunaHelp()
        {
            CharacterPresentationProfile profile = PresentationProfile;
            float width = 360f;
            Rect rect = new Rect(Screen.width - width - 12f, Screen.height - 188f, width, 128f);
            DrawRect(rect, new Color(0.040f, 0.010f, 0.012f, 0.98f));
            DrawBorder(rect, profile.HudAccent, 2f);
            string text =
                "F1 · 닫기\n"
                + "WASD 이동 · SPACE 회피 · TAB 타깃\n"
                + $"LMB 기본 공격 · Q {profile.Skill1.Label} · E {profile.Skill2.Label}\n"
                + $"R {profile.Ultimate.Label}: 해·팔 사용 후 영역 밖 적 1명\n"
                + $"V {profile.Domain.Label}: 짧은 준비 후 개방형 영역";
            smallStyle.normal.textColor = Color.white;
            GUI.Label(
                new Rect(rect.x + 12f, rect.y + 8f, rect.width - 24f, rect.height - 16f),
                text,
                smallStyle
            );
        }

        private void DrawValueBar(Rect rect, float value, float max, Color fill, string text)
        {
            DrawRect(rect, new Color(0.075f, 0.045f, 0.055f));
            float ratio = max > 0f ? Mathf.Clamp01(value / max) : 0f;
            DrawRect(
                new Rect(rect.x + 1f, rect.y + 1f, (rect.width - 2f) * ratio, rect.height - 2f),
                fill
            );
            DrawBorder(rect, new Color(1f, 1f, 1f, 0.16f), 1f);
            GUI.Label(rect, text, valueStyle);
        }

        private void EnsureStyles()
        {
            if (styledForHeight == Screen.height)
            {
                return;
            }

            styledForHeight = Screen.height;
            int baseSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 62f, 12f, 17f));
            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = baseSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
            };
            headerStyle.normal.textColor = Color.white;

            valueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(10, baseSize - 2),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            valueStyle.normal.textColor = Color.white;

            smallStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(10, baseSize - 2),
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
            };

            centerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = baseSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            centerStyle.normal.textColor = Color.white;
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
