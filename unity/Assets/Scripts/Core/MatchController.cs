using System.Collections.Generic;
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

        [Header("Prototype Encounter")]
        [SerializeField, Min(1)] private int prototypeEnemyCount = 2;
        [SerializeField, Min(3f)] private float arenaGroundScale = 6.5f;
        [SerializeField] private bool createSafetyWalls = true;
        [SerializeField, Min(0.2f)] private float safetyWallThickness = 0.8f;
        [SerializeField, Min(0.2f)] private float safetyWallHeight = 1.2f;

        private readonly List<Health> enemyHealths = new List<Health>();
        private readonly List<CurseBotController> enemyBots = new List<CurseBotController>();

        private string resultText = string.Empty;
        private bool matchFinished;
        private bool showControlHelp;
        private BasicAttack playerAttack;
        private ThirdPersonPlayerController playerMovement;
        private TargetLockController targetLock;
        private CursedEnergyController cursedEnergy;
        private GojoVariantController gojoVariant;
        private PrototypeCharacterController prototypeCharacter;
        private PrototypePlayerTeamController playerTeam;

        private GUIStyle headerStyle;
        private GUIStyle valueStyle;
        private GUIStyle smallStyle;
        private GUIStyle centerStyle;
        private GUIStyle warningStyle;
        private GUIStyle resultStyle;
        private int styledForHeight = -1;

        public int LivingEnemyCount
        {
            get
            {
                int living = 0;
                foreach (Health health in enemyHealths)
                {
                    if (health != null && !health.IsDead)
                    {
                        living += 1;
                    }
                }
                return living;
            }
        }

        private bool TeamHudActive
        {
            get
            {
                if (playerTeam == null && playerHealth != null)
                {
                    playerTeam = playerHealth.GetComponent<PrototypePlayerTeamController>();
                }

                return playerTeam != null && playerTeam.enabled;
            }
        }

        private bool OpponentTeamHudActive => PrototypeOpponentTeamController.TeamBattleModeRequested;

        private bool IsSukunaActive
        {
            get
            {
                if (prototypeCharacter == null && playerHealth != null)
                {
                    prototypeCharacter = playerHealth.GetComponent<PrototypeCharacterController>();
                }

                return prototypeCharacter != null && prototypeCharacter.IsSukuna;
            }
        }

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

            ExpandPrototypeArena();
            BuildEnemyRoster();

            playerAttack = playerHealth.GetComponent<BasicAttack>();
            playerMovement = playerHealth.GetComponent<ThirdPersonPlayerController>();
            targetLock = playerHealth.GetComponent<TargetLockController>();
            cursedEnergy = CursedEnergyController.GetOrCreate(playerHealth.gameObject);
            gojoVariant = GojoVariantController.GetOrCreate(playerHealth.gameObject);
            prototypeCharacter = playerHealth.GetComponent<PrototypeCharacterController>();
            playerTeam = playerHealth.GetComponent<PrototypePlayerTeamController>();
            GojoPrototypeAvatar.GetOrCreate(playerHealth.gameObject);

            playerHealth.Died += HandlePlayerDeath;
            foreach (Health health in enemyHealths)
            {
                if (health != null)
                {
                    health.Died += HandleEnemyDeath;
                }
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= HandlePlayerDeath;
            }

            foreach (Health health in enemyHealths)
            {
                if (health != null)
                {
                    health.Died -= HandleEnemyDeath;
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                showControlHelp = !showControlHelp;
            }

            if (matchFinished && Input.GetKeyDown(KeyCode.Return))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void BuildEnemyRoster()
        {
            enemyHealths.Clear();
            enemyBots.Clear();
            RegisterEnemy(enemyHealth);

            Health[] sceneHealth = FindObjectsByType<Health>(FindObjectsSortMode.None);
            foreach (Health health in sceneHealth)
            {
                if (health != null && health != playerHealth && health != enemyHealth)
                {
                    RegisterEnemy(health);
                }
            }

            int desiredCount = Mathf.Max(1, prototypeEnemyCount);
            while (enemyHealths.Count < desiredCount)
            {
                int newIndex = enemyHealths.Count;
                GameObject clone = Instantiate(enemyHealth.gameObject);
                clone.name = $"CurseBot_{(char)('A' + newIndex)}";
                clone.transform.position = GetPrototypeEnemySpawnPosition(newIndex);
                Vector3 facing = playerHealth.transform.position - clone.transform.position;
                facing.y = 0f;
                if (facing.sqrMagnitude > 0.001f)
                {
                    clone.transform.rotation = Quaternion.LookRotation(facing, Vector3.up);
                }

                Health cloneHealth = clone.GetComponent<Health>();
                if (cloneHealth == null)
                {
                    Destroy(clone);
                    break;
                }

                cloneHealth.ResetHealth();
                RegisterEnemy(cloneHealth);
            }
        }

        private void RegisterEnemy(Health health)
        {
            if (health == null || health == playerHealth || enemyHealths.Contains(health))
            {
                return;
            }

            enemyHealths.Add(health);
            CurseBotController bot = health.GetComponent<CurseBotController>();
            if (bot != null)
            {
                enemyBots.Add(bot);
            }
        }

        private Vector3 GetPrototypeEnemySpawnPosition(int enemyIndex)
        {
            Vector3 basePosition = enemyHealth.transform.position;
            if (enemyIndex <= 0)
            {
                return basePosition;
            }

            int row = (enemyIndex - 1) / 2;
            float side = enemyIndex % 2 == 1 ? -1f : 1f;
            return basePosition + new Vector3(side * (7f + row * 2f), 0f, 3f + row * 4f);
        }

        private void ExpandPrototypeArena()
        {
            GameObject ground = GameObject.Find("ArenaGround");
            if (ground == null)
            {
                Debug.LogWarning("ArenaGround를 찾지 못해 런타임 맵 확장을 건너뜁니다.");
                return;
            }

            Vector3 scale = ground.transform.localScale;
            scale.x = Mathf.Max(scale.x, arenaGroundScale);
            scale.z = Mathf.Max(scale.z, arenaGroundScale);
            ground.transform.localScale = scale;

            if (!createSafetyWalls || GameObject.Find("PrototypeArenaBoundary") != null)
            {
                return;
            }

            Renderer groundRenderer = ground.GetComponent<Renderer>();
            if (groundRenderer != null)
            {
                CreateSafetyWalls(groundRenderer.bounds);
            }
        }

        private void CreateSafetyWalls(Bounds arenaBounds)
        {
            GameObject root = new GameObject("PrototypeArenaBoundary");
            float thickness = safetyWallThickness;
            float height = safetyWallHeight;
            float y = arenaBounds.max.y + height * 0.5f;
            float fullWidth = arenaBounds.size.x + thickness * 2f;
            float fullDepth = arenaBounds.size.z + thickness * 2f;

            CreateSafetyWall(root.transform, "NorthWall", new Vector3(arenaBounds.center.x, y, arenaBounds.max.z + thickness * 0.5f), new Vector3(fullWidth, height, thickness));
            CreateSafetyWall(root.transform, "SouthWall", new Vector3(arenaBounds.center.x, y, arenaBounds.min.z - thickness * 0.5f), new Vector3(fullWidth, height, thickness));
            CreateSafetyWall(root.transform, "EastWall", new Vector3(arenaBounds.max.x + thickness * 0.5f, y, arenaBounds.center.z), new Vector3(thickness, height, fullDepth));
            CreateSafetyWall(root.transform, "WestWall", new Vector3(arenaBounds.min.x - thickness * 0.5f, y, arenaBounds.center.z), new Vector3(thickness, height, fullDepth));
        }

        private static void CreateSafetyWall(Transform parent, string objectName, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = objectName;
            wall.transform.SetParent(parent, false);
            wall.transform.position = position;
            wall.transform.localScale = scale;

            Renderer renderer = wall.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            if (renderer != null && shader != null)
            {
                renderer.material = new Material(shader)
                {
                    color = new Color(0.055f, 0.10f, 0.18f, 1f),
                };
            }
        }

        private void HandlePlayerDeath(Health _)
        {
            FinishMatch("DEFEAT");
        }

        private void HandleEnemyDeath(Health _)
        {
            if (LivingEnemyCount <= 0)
            {
                FinishMatch("VICTORY");
            }
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
            playerMovement ??= playerHealth != null
                ? playerHealth.GetComponent<ThirdPersonPlayerController>()
                : null;
            playerAttack ??= playerHealth != null
                ? playerHealth.GetComponent<BasicAttack>()
                : null;
            targetLock ??= playerHealth != null
                ? playerHealth.GetComponent<TargetLockController>()
                : null;

            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }
            if (playerAttack != null)
            {
                playerAttack.enabled = false;
            }
            if (targetLock != null)
            {
                targetLock.enabled = false;
            }

            if (playerHealth != null)
            {
                GojoTechniqueChainController chain = playerHealth.GetComponent<GojoTechniqueChainController>();
                GojoTechniqueController technique = playerHealth.GetComponent<GojoTechniqueController>();
                if (chain != null)
                {
                    chain.enabled = false;
                }
                if (technique != null)
                {
                    technique.enabled = false;
                }
            }

            if (gojoDomain != null)
            {
                gojoDomain.ResetCommand();
                gojoDomain.enabled = false;
            }

            foreach (CurseBotController bot in enemyBots)
            {
                if (bot != null)
                {
                    bot.enabled = false;
                }
            }
        }

        private void OnGUI()
        {
            if (playerHealth == null || enemyHealths.Count == 0)
            {
                return;
            }

            EnsureStyles();
            DrawCompactCombatHud();

            if (showControlHelp && !matchFinished)
            {
                DrawControlHelp();
            }
            if (matchFinished)
            {
                DrawResultOverlay();
            }
        }

        private void DrawCompactCombatHud()
        {
            const float margin = 12f;
            float panelWidth = Mathf.Clamp((Screen.width - margin * 3f) * 0.36f, 230f, 340f);
            Rect playerRect = new Rect(margin, margin, panelWidth, 62f);
            if (!TeamHudActive)
            {
                DrawPlayerPanel(playerRect);
            }

            if (!OpponentTeamHudActive)
            {
                float enemyWidth = Mathf.Clamp(panelWidth * 0.92f, 220f, 320f);
                for (int index = 0; index < enemyHealths.Count; index++)
                {
                    Health health = enemyHealths[index];
                    if (health == null)
                    {
                        continue;
                    }

                    Rect rect = new Rect(
                        Screen.width - margin - enemyWidth,
                        margin + index * 48f,
                        enemyWidth,
                        43f
                    );
                    DrawEnemyPanel(rect, health, index);
                }

                DrawEnemyCount();
            }

            DrawDodgeChip(playerRect);
            DrawAttackIndicators();
            DrawEnemyAttackWarning();
            if (!IsSukunaActive)
            {
                DrawDomainPanel();
            }
        }

        private void DrawPlayerPanel(Rect rect)
        {
            DrawRect(rect, new Color(0.012f, 0.018f, 0.032f, 0.90f));
            DrawBorder(rect, new Color(0.18f, 0.66f, 1f, 0.92f), 2f);

            string title = gojoVariant != null
                ? $"PLAYER · {gojoVariant.DisplayName}"
                : "PLAYER · GOJO SATORU";
            GUI.Label(new Rect(rect.x + 10f, rect.y + 3f, rect.width - 20f, 18f), title, headerStyle);

            DrawValueBar(
                new Rect(rect.x + 10f, rect.y + 23f, rect.width - 20f, 18f),
                playerHealth.CurrentHealth,
                playerHealth.MaxHealth,
                new Color(0.18f, 0.66f, 1f),
                $"HP  {playerHealth.CurrentHealth:0} / {playerHealth.MaxHealth:0}"
            );

            cursedEnergy ??= CursedEnergyController.GetOrCreate(playerHealth.gameObject);
            if (cursedEnergy != null)
            {
                DrawValueBar(
                    new Rect(rect.x + 10f, rect.y + 44f, rect.width - 20f, 12f),
                    cursedEnergy.CurrentEnergy,
                    cursedEnergy.MaxEnergy,
                    new Color(0.34f, 0.20f, 0.96f),
                    $"CE {cursedEnergy.CurrentEnergy:0}/{cursedEnergy.MaxEnergy:0} · {cursedEnergy.ProfileLabel}"
                );
            }
        }

        private void DrawEnemyPanel(Rect rect, Health health, int index)
        {
            Color accent = index % 2 == 0
                ? new Color(0.94f, 0.15f, 0.20f)
                : new Color(0.95f, 0.34f, 0.10f);
            DrawRect(rect, new Color(0.018f, 0.020f, 0.030f, 0.90f));
            DrawBorder(rect, accent, 2f);
            string defeated = health.IsDead ? " · DOWN" : string.Empty;
            GUI.Label(new Rect(rect.x + 9f, rect.y + 2f, rect.width - 18f, 17f), $"CURSE {(char)('A' + index)}{defeated}", headerStyle);
            DrawValueBar(
                new Rect(rect.x + 9f, rect.y + 21f, rect.width - 18f, 16f),
                health.CurrentHealth,
                health.MaxHealth,
                accent,
                $"{health.CurrentHealth:0} / {health.MaxHealth:0}"
            );
        }

        private void DrawEnemyCount()
        {
            Rect rect = new Rect(Screen.width * 0.5f - 82f, 7f, 164f, 23f);
            Color accent = LivingEnemyCount > 0
                ? new Color(1f, 0.55f, 0.18f)
                : new Color(0.20f, 0.78f, 1f);
            DrawRect(rect, new Color(0.045f, 0.025f, 0.012f, 0.90f));
            DrawBorder(rect, accent, 2f);
            centerStyle.normal.textColor = accent;
            GUI.Label(rect, $"CURSES  {LivingEnemyCount}/{enemyHealths.Count}", centerStyle);
        }

        private void DrawDodgeChip(Rect playerRect)
        {
            playerMovement ??= playerHealth.GetComponent<ThirdPersonPlayerController>();
            if (playerMovement == null || matchFinished)
            {
                return;
            }

            string text;
            Color accent;
            if (playerMovement.IsDodging)
            {
                text = "DODGING";
                accent = new Color(0.35f, 0.95f, 1f);
            }
            else if (playerMovement.DodgeReady)
            {
                text = "SPACE · DODGE READY";
                accent = new Color(0.22f, 0.82f, 1f);
            }
            else
            {
                text = $"DODGE {playerMovement.DodgeCooldownRemaining:0.0}s";
                accent = new Color(0.48f, 0.56f, 0.68f);
            }

            Rect rect = new Rect(playerRect.x, playerRect.yMax + 5f, 160f, 22f);
            DrawRect(rect, new Color(0.018f, 0.025f, 0.045f, 0.90f));
            DrawBorder(rect, accent, 1f);
            smallStyle.normal.textColor = accent;
            GUI.Label(rect, text, smallStyle);
        }

        private void DrawAttackIndicators()
        {
            playerAttack ??= playerHealth.GetComponent<BasicAttack>();
            if (playerAttack == null || matchFinished)
            {
                return;
            }

            float y = 34f;
            if (playerAttack.DisplayChainStep > 0)
            {
                bool finisher = playerAttack.DisplayChainStep == 3;
                Rect rect = new Rect(Screen.width * 0.5f - 115f, y, 230f, 26f);
                Color accent = finisher
                    ? new Color(0.72f, 0.38f, 1f)
                    : new Color(0.20f, 0.76f, 1f);
                DrawRect(rect, new Color(0.018f, 0.025f, 0.055f, 0.88f));
                DrawBorder(rect, accent, 2f);
                centerStyle.normal.textColor = accent;
                GUI.Label(rect, playerAttack.ChainLabel, centerStyle);
                y += 30f;
            }

            if (playerAttack.DisplayHitComboCount > 0)
            {
                Rect rect = new Rect(Screen.width * 0.5f - 90f, y, 180f, 24f);
                Color accent = new Color(1f, 0.80f, 0.22f);
                DrawRect(rect, new Color(0.055f, 0.038f, 0.012f, 0.90f));
                DrawBorder(rect, accent, 1f);
                centerStyle.normal.textColor = accent;
                GUI.Label(rect, playerAttack.HitComboLabel, centerStyle);
            }
        }

        private void DrawEnemyAttackWarning()
        {
            if (matchFinished)
            {
                return;
            }

            int count = 0;
            float progress = 0f;
            foreach (CurseBotController bot in enemyBots)
            {
                if (bot != null && bot.gameObject.activeInHierarchy && bot.IsAttackTelegraphing)
                {
                    count += 1;
                    progress = Mathf.Max(progress, bot.AttackWindupProgress);
                }
            }
            if (count == 0)
            {
                return;
            }

            float width = Mathf.Min(300f, Screen.width - 24f);
            Rect rect = new Rect((Screen.width - width) * 0.5f, Screen.height * 0.31f, width, 54f);
            Color accent = new Color(1f, 0.40f, 0.08f);
            DrawRect(rect, new Color(0.10f, 0.025f, 0.008f, 0.90f));
            DrawBorder(rect, accent, 2f);
            warningStyle.normal.textColor = accent;
            GUI.Label(new Rect(rect.x, rect.y + 2f, rect.width, 31f), count > 1 ? $"DODGE! × {count}" : "DODGE!", warningStyle);
            Rect bar = new Rect(rect.x + 18f, rect.y + 38f, rect.width - 36f, 8f);
            DrawRect(bar, new Color(0.18f, 0.08f, 0.025f));
            DrawRect(new Rect(bar.x, bar.y, bar.width * progress, bar.height), accent);
        }

        private void DrawDomainPanel()
        {
            bool timing = gojoDomain != null && gojoDomain.IsReleaseTiming;
            float width = Mathf.Min(680f, Screen.width - 24f);
            float height = timing ? 76f : 36f;
            Rect rect = new Rect((Screen.width - width) * 0.5f, Screen.height - height - 12f, width, height);
            DrawRect(rect, new Color(0.018f, 0.026f, 0.060f, 0.88f));
            DrawBorder(rect, new Color(0.24f, 0.55f, 1f, 0.92f), 2f);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 4f, rect.width - 20f, 24f), gojoDomain != null ? gojoDomain.StatusText : "영역 시스템 연결 안 됨", centerStyle);

            if (timing)
            {
                Rect bar = new Rect(rect.x + 24f, rect.y + 34f, rect.width - 48f, 16f);
                DrawReleaseTimingBar(bar);
                GUI.Label(new Rect(rect.x + 10f, rect.y + 54f, rect.width - 20f, 17f), $"초록 구간에서 우클릭 해제 · {gojoDomain.ReleaseElapsed:0.00}s", smallStyle);
            }
            else
            {
                smallStyle.normal.textColor = new Color(0.60f, 0.67f, 0.82f);
                GUI.Label(new Rect(rect.x + 10f, rect.y + 20f, rect.width - 20f, 14f), "F1 · 조작 도움말", smallStyle);
            }
        }

        private void DrawReleaseTimingBar(Rect rect)
        {
            DrawRect(rect, new Color(0.07f, 0.08f, 0.12f));
            float start = rect.x + rect.width * gojoDomain.ReleaseWindowStartNormalized;
            float end = rect.x + rect.width * gojoDomain.ReleaseWindowEndNormalized;
            DrawRect(new Rect(start, rect.y + 1f, Mathf.Max(2f, end - start), rect.height - 2f), new Color(0.16f, 0.88f, 0.38f));
            float cursor = rect.x + rect.width * gojoDomain.ReleaseProgressNormalized;
            DrawRect(new Rect(cursor - 2f, rect.y - 2f, 4f, rect.height + 4f), Color.white);
        }

        private void DrawControlHelp()
        {
            bool teamMode = TeamHudActive;
            bool sukuna = IsSukunaActive;
            float width = 370f;
            float height = sukuna ? 154f : 148f;
            Rect rect = new Rect(Screen.width - width - 12f, Screen.height - height - 60f, width, height);
            Color accent = sukuna
                ? new Color(0.96f, 0.22f, 0.12f)
                : new Color(0.24f, 0.55f, 1f);
            DrawRect(rect, sukuna
                ? new Color(0.040f, 0.010f, 0.012f, 0.98f)
                : new Color(0.012f, 0.018f, 0.032f, 0.95f));
            DrawBorder(rect, accent, 2f);

            string teamLine = teamMode ? "T 팀 교대 · 현재 2인 팀 프로토타입\n" : string.Empty;
            string text;
            if (sukuna)
            {
                text =
                    "F1 · 닫기\n"
                    + teamLine
                    + "WASD 이동 · SPACE 회피 · TAB 타깃\n"
                    + "LMB 기본 공격 · Q 해 · E 팔\n"
                    + "R 푸가: 해·팔 사용 후 영역 밖 적 1명\n"
                    + "V 복마어주자: 짧은 준비 후 개방형 영역";
            }
            else
            {
                text =
                    "F1 · 닫기\n"
                    + teamLine
                    + "WASD 이동 · SPACE 회피 · TAB 타깃\n"
                    + "LMB 기본 공격 · Q 창 · E 혁 · R 허식 자\n"
                    + "V 영역 준비 · X 영역 입력 취소\n"
                    + "영역: RMB 유지 → LMB → 초록 구간 RMB 해제";
            }

            smallStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rect.x + 12f, rect.y + 8f, rect.width - 24f, rect.height - 16f), text, smallStyle);
        }

        private void DrawResultOverlay()
        {
            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.64f));
            bool victory = resultText == "VICTORY";
            Color accent = victory ? new Color(0.20f, 0.78f, 1f) : new Color(0.95f, 0.18f, 0.22f);
            Rect panel = new Rect(Screen.width * 0.5f - 210f, Screen.height * 0.5f - 82f, 420f, 164f);
            DrawRect(panel, new Color(0.018f, 0.022f, 0.04f, 0.97f));
            DrawBorder(panel, accent, 3f);
            resultStyle.normal.textColor = accent;
            GUI.Label(new Rect(panel.x, panel.y + 18f, panel.width, 65f), resultText, resultStyle);
            centerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(panel.x + 12f, panel.y + 88f, panel.width - 24f, 24f), victory ? "모든 주령을 퇴치했습니다" : "전투에서 패배했습니다", centerStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 124f, panel.width - 24f, 24f), "ENTER · 다시 시작", centerStyle);
        }

        private void DrawValueBar(Rect rect, float value, float max, Color fill, string text)
        {
            DrawRect(rect, new Color(0.075f, 0.082f, 0.115f));
            float ratio = max > 0f ? Mathf.Clamp01(value / max) : 0f;
            DrawRect(new Rect(rect.x + 1f, rect.y + 1f, (rect.width - 2f) * ratio, rect.height - 2f), fill);
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
            smallStyle.normal.textColor = new Color(0.82f, 0.86f, 0.95f);

            centerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = baseSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            centerStyle.normal.textColor = Color.white;

            warningStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 30f, 24f, 36f)),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };

            resultStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 18f, 38f, 60f)),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
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
