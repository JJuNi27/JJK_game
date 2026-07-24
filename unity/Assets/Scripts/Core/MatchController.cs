using JJKGame.Enemy;
using JJKGame.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.Core
{
    public sealed class MatchController : MonoBehaviour
    {
        [SerializeField] private Health playerHealth;
        [SerializeField] private Health enemyHealth;
        [SerializeField] private GojoDomainController gojoDomain;

        private string resultText = string.Empty;
        private bool matchFinished;

        private GUIStyle headerStyle;
        private GUIStyle healthValueStyle;
        private GUIStyle domainStyle;
        private GUIStyle hintStyle;
        private GUIStyle resultStyle;
        private GUIStyle resultHintStyle;
        private int styledForHeight = -1;

        public void Configure(
            Health newPlayerHealth,
            Health newEnemyHealth,
            GojoDomainController newGojoDomain
        )
        {
            playerHealth = newPlayerHealth;
            enemyHealth = newEnemyHealth;
            gojoDomain = newGojoDomain;
        }

        private void Awake()
        {
            if (playerHealth == null || enemyHealth == null)
            {
                Debug.LogError("MatchController에 Player/Enemy Health를 연결해야 합니다.");
                enabled = false;
                return;
            }

            playerHealth.Died += HandlePlayerDeath;
            enemyHealth.Died += HandleEnemyDeath;
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= HandlePlayerDeath;
            }

            if (enemyHealth != null)
            {
                enemyHealth.Died -= HandleEnemyDeath;
            }
        }

        private void Update()
        {
            if (matchFinished && Input.GetKeyDown(KeyCode.Return))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void HandlePlayerDeath(Health _)
        {
            FinishMatch("DEFEAT");
        }

        private void HandleEnemyDeath(Health _)
        {
            FinishMatch("VICTORY");
        }

        private void FinishMatch(string result)
        {
            if (matchFinished)
            {
                return;
            }

            matchFinished = true;
            resultText = result;
            StopCombatActors();
        }

        private void StopCombatActors()
        {
            if (playerHealth != null)
            {
                ThirdPersonPlayerController movement =
                    playerHealth.GetComponent<ThirdPersonPlayerController>();
                BasicAttack attack = playerHealth.GetComponent<BasicAttack>();

                if (movement != null)
                {
                    movement.enabled = false;
                }

                if (attack != null)
                {
                    attack.enabled = false;
                }
            }

            if (gojoDomain != null)
            {
                gojoDomain.ResetCommand();
                gojoDomain.enabled = false;
            }

            if (enemyHealth != null)
            {
                CurseBotController bot = enemyHealth.GetComponent<CurseBotController>();
                if (bot != null)
                {
                    bot.enabled = false;
                }
            }
        }

        private void OnGUI()
        {
            if (playerHealth == null || enemyHealth == null)
            {
                return;
            }

            EnsureStyles();
            DrawCombatHud();

            if (matchFinished)
            {
                DrawResultOverlay();
            }
        }

        private void DrawCombatHud()
        {
            const float margin = 24f;
            const float panelHeight = 84f;
            float availableHalfWidth = (Screen.width - margin * 3f) * 0.5f;
            float panelWidth = Mathf.Clamp(availableHalfWidth, 250f, 440f);

            Rect playerRect = new Rect(margin, margin, panelWidth, panelHeight);
            Rect enemyRect = new Rect(
                Screen.width - margin - panelWidth,
                margin,
                panelWidth,
                panelHeight
            );

            DrawHealthPanel(
                playerRect,
                "PLAYER · GOJO SATORU",
                playerHealth,
                new Color(0.18f, 0.66f, 1f),
                TextAnchor.UpperLeft
            );
            DrawHealthPanel(
                enemyRect,
                "CURSE BOT",
                enemyHealth,
                new Color(0.92f, 0.16f, 0.20f),
                TextAnchor.UpperRight
            );

            float domainWidth = Mathf.Min(720f, Screen.width - margin * 2f);
            Rect domainRect = new Rect(
                (Screen.width - domainWidth) * 0.5f,
                Screen.height - 92f,
                domainWidth,
                68f
            );
            DrawRect(domainRect, new Color(0.025f, 0.035f, 0.075f, 0.88f));
            DrawBorder(domainRect, new Color(0.24f, 0.55f, 1f, 0.9f), 2f);

            string domainText = gojoDomain != null
                ? gojoDomain.StatusText
                : "영역 시스템 연결 안 됨";
            GUI.Label(
                new Rect(domainRect.x + 18f, domainRect.y + 9f, domainRect.width - 36f, 28f),
                domainText,
                domainStyle
            );
            GUI.Label(
                new Rect(domainRect.x + 18f, domainRect.y + 39f, domainRect.width - 36f, 20f),
                "WASD 이동  ·  좌클릭 공격  ·  V 영역 준비  ·  R 입력 초기화",
                hintStyle
            );
        }

        private void DrawHealthPanel(
            Rect panelRect,
            string title,
            Health health,
            Color fillColor,
            TextAnchor titleAlignment
        )
        {
            DrawRect(panelRect, new Color(0.018f, 0.022f, 0.035f, 0.9f));
            DrawBorder(panelRect, new Color(fillColor.r, fillColor.g, fillColor.b, 0.9f), 2f);

            headerStyle.alignment = titleAlignment;
            GUI.Label(
                new Rect(panelRect.x + 14f, panelRect.y + 8f, panelRect.width - 28f, 24f),
                title,
                headerStyle
            );

            Rect barBackground = new Rect(
                panelRect.x + 14f,
                panelRect.y + 38f,
                panelRect.width - 28f,
                24f
            );
            DrawRect(barBackground, new Color(0.09f, 0.10f, 0.14f, 1f));

            float ratio = health.MaxHealth > 0f
                ? Mathf.Clamp01(health.CurrentHealth / health.MaxHealth)
                : 0f;
            Rect barFill = new Rect(
                barBackground.x + 2f,
                barBackground.y + 2f,
                Mathf.Max(0f, (barBackground.width - 4f) * ratio),
                barBackground.height - 4f
            );
            DrawRect(barFill, fillColor);
            DrawBorder(barBackground, new Color(1f, 1f, 1f, 0.18f), 1f);

            GUI.Label(
                barBackground,
                $"{health.CurrentHealth:0} / {health.MaxHealth:0}",
                healthValueStyle
            );
        }

        private void DrawResultOverlay()
        {
            DrawRect(
                new Rect(0f, 0f, Screen.width, Screen.height),
                new Color(0f, 0f, 0f, 0.64f)
            );

            bool victory = resultText == "VICTORY";
            Color accent = victory
                ? new Color(0.20f, 0.78f, 1f)
                : new Color(0.95f, 0.18f, 0.22f);

            Rect resultPanel = new Rect(
                Screen.width * 0.5f - 250f,
                Screen.height * 0.5f - 105f,
                500f,
                210f
            );
            DrawRect(resultPanel, new Color(0.018f, 0.022f, 0.04f, 0.96f));
            DrawBorder(resultPanel, accent, 4f);

            resultStyle.normal.textColor = accent;
            GUI.Label(
                new Rect(resultPanel.x + 20f, resultPanel.y + 28f, resultPanel.width - 40f, 82f),
                resultText,
                resultStyle
            );
            GUI.Label(
                new Rect(resultPanel.x + 20f, resultPanel.y + 112f, resultPanel.width - 40f, 34f),
                victory ? "주령을 퇴치했습니다" : "전투에서 패배했습니다",
                domainStyle
            );
            GUI.Label(
                new Rect(resultPanel.x + 20f, resultPanel.y + 158f, resultPanel.width - 40f, 28f),
                "ENTER 키로 다시 시작",
                resultHintStyle
            );
        }

        private void EnsureStyles()
        {
            if (styledForHeight == Screen.height)
            {
                return;
            }

            styledForHeight = Screen.height;
            int baseSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 45f, 15f, 22f));

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = baseSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft,
            };
            headerStyle.normal.textColor = Color.white;

            healthValueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(14, baseSize - 1),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            healthValueStyle.normal.textColor = Color.white;

            domainStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = baseSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            domainStyle.normal.textColor = Color.white;

            hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(12, baseSize - 4),
                alignment = TextAnchor.MiddleCenter,
            };
            hintStyle.normal.textColor = new Color(0.75f, 0.80f, 0.92f);

            resultStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 16f, 42f, 72f)),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };

            resultHintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(14, baseSize - 1),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            resultHintStyle.normal.textColor = new Color(0.84f, 0.86f, 0.94f);
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        private static void DrawBorder(Rect rect, Color color, float thickness)
        {
            DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            DrawRect(
                new Rect(rect.x, rect.yMax - thickness, rect.width, thickness),
                color
            );
            DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawRect(
                new Rect(rect.xMax - thickness, rect.y, thickness, rect.height),
                color
            );
        }
    }
}
