using JJKGame.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.Core
{
    public enum PrototypeCharacterId
    {
        GojoModern,
        SukunaShibuyaYujiBody,
    }

    [DefaultExecutionOrder(10000)]
    [DisallowMultipleComponent]
    public sealed class PrototypeCharacterRuntime : MonoBehaviour
    {
        public static PrototypeCharacterId SelectedCharacter { get; private set; } =
            PrototypeCharacterId.GojoModern;

        private Health health;
        private CursedEnergyController cursedEnergy;
        private GUIStyle switchStyle;
        private int styledForHeight = -1;

        public bool IsSukuna => SelectedCharacter == PrototypeCharacterId.SukunaShibuyaYujiBody;
        public string DisplayName => IsSukuna
            ? "RYOMEN SUKUNA · 시부야 · 이타도리 육체"
            : "GOJO SATORU · 현대 · 교사";

        public static PrototypeCharacterRuntime GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            PrototypeCharacterRuntime runtime = owner.GetComponent<PrototypeCharacterRuntime>();
            return runtime != null ? runtime : owner.AddComponent<PrototypeCharacterRuntime>();
        }

        private void Awake()
        {
            health = GetComponent<Health>();
            cursedEnergy = CursedEnergyController.GetOrCreate(gameObject);
        }

        private void Start()
        {
            ApplySelectedCharacter();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                SelectedCharacter = IsSukuna
                    ? PrototypeCharacterId.GojoModern
                    : PrototypeCharacterId.SukunaShibuyaYujiBody;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void ApplySelectedCharacter()
        {
            SetGojoComponentsEnabled(!IsSukuna);

            Transform gojoVisual = transform.Find("PrototypeGojoAvatar");
            if (gojoVisual != null)
            {
                gojoVisual.gameObject.SetActive(!IsSukuna);
            }

            if (IsSukuna)
            {
                cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SukunaShibuyaReserve, true);
                SukunaPrototypeAvatar.GetOrCreate(gameObject).SetVisible(true);
                SukunaTechniqueController.GetOrCreate(gameObject).enabled = true;
            }
            else
            {
                cursedEnergy?.ApplyProfile(CursedEnergyProfileId.SixEyesEfficiency, true);
                SukunaTechniqueController sukuna = GetComponent<SukunaTechniqueController>();
                if (sukuna != null)
                {
                    sukuna.enabled = false;
                }
                SukunaPrototypeAvatar sukunaAvatar = GetComponent<SukunaPrototypeAvatar>();
                sukunaAvatar?.SetVisible(false);
            }
        }

        private void SetGojoComponentsEnabled(bool value)
        {
            SetEnabled<GojoTechniqueController>(value);
            SetEnabled<GojoTechniqueChainController>(value);
            SetEnabled<GojoDomainController>(value);
            SetEnabled<GojoInfinityDefense>(value);
            SetEnabled<TechniqueBurnoutController>(value);
        }

        private void SetEnabled<T>(bool value) where T : Behaviour
        {
            T component = GetComponent<T>();
            if (component != null)
            {
                component.enabled = value;
            }
        }

        private void OnGUI()
        {
            EnsureStyle();

            Rect switchChip = new Rect(Screen.width * 0.5f - 130f, Screen.height - 52f, 260f, 24f);
            DrawRect(switchChip, new Color(0.018f, 0.020f, 0.032f, 0.96f));
            DrawBorder(
                switchChip,
                IsSukuna ? new Color(1f, 0.22f, 0.16f) : new Color(0.24f, 0.62f, 1f),
                1f
            );
            GUI.Label(
                switchChip,
                IsSukuna ? "F2 · 고죠로 전환" : "F2 · 스쿠나로 전환",
                switchStyle
            );

            if (!IsSukuna)
            {
                return;
            }

            float width = Mathf.Min(680f, Screen.width - 24f);
            Rect domainCover = new Rect((Screen.width - width) * 0.5f, Screen.height - 88f, width, 36f);
            DrawRect(domainCover, new Color(0.055f, 0.012f, 0.018f, 0.98f));
            DrawBorder(domainCover, new Color(1f, 0.26f, 0.14f), 2f);
            GUI.Label(
                domainCover,
                "R · 푸가 — 해와 팔 사용 상태 기록 중 · 발동은 Milestone 2",
                switchStyle
            );
        }

        private void EnsureStyle()
        {
            if (styledForHeight == Screen.height)
            {
                return;
            }

            styledForHeight = Screen.height;
            switchStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 68f, 11f, 15f)),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            switchStyle.normal.textColor = Color.white;
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
