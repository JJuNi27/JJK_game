using System;
using System.Collections;
using System.Collections.Generic;
using JJKGame.Core;
using JJKGame.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.Enemy
{
    public enum PrototypeEncounterMode
    {
        TrainingMultiCurse,
        TeamBattle,
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeOpponentTeamController : MonoBehaviour
    {
        private static PrototypeEncounterMode requestedMode = PrototypeEncounterMode.TrainingMultiCurse;

        private readonly Health[] members = new Health[2];
        private readonly Vector3[] memberSpawnPositions = new Vector3[2];
        private readonly Quaternion[] memberSpawnRotations = new Quaternion[2];

        private int activeIndex;
        private bool initialized;
        private bool switchingMode;
        private Coroutine sceneInitializationRoutine;
        private float entryNoticeUntil;
        private OpponentCombatHudDataSource hudDataSource;
        private GUIStyle titleStyle;
        private GUIStyle metaStyle;
        private GUIStyle rowStyle;
        private GUIStyle chipStyle;
        private int styledForHeight = -1;

        public PrototypeEncounterMode Mode => requestedMode;
        public bool IsTeamBattle => Mode == PrototypeEncounterMode.TeamBattle;
        public static bool TeamBattleModeRequested => requestedMode == PrototypeEncounterMode.TeamBattle;
        public bool IsInitialized => initialized;
        public int ActiveMemberIndex => activeIndex;
        public int TeamSize => members.Length;
        public bool EntryNoticeActive => initialized && Time.time < entryNoticeUntil;
        public Health ActiveMember => initialized ? members[activeIndex] : null;
        public Health ReserveMember => initialized ? members[1 - activeIndex] : null;
        public int LivingMemberCount
        {
            get
            {
                int living = 0;
                for (int index = 0; index < members.Length; index++)
                {
                    if (members[index] != null && !members[index].IsDead)
                    {
                        living += 1;
                    }
                }
                return living;
            }
        }

        public Health GetMember(int index)
        {
            return index >= 0 && index < members.Length ? members[index] : null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapAfterSceneLoad()
        {
            SceneManager.sceneLoaded -= HandleBootstrapSceneLoaded;
            SceneManager.sceneLoaded += HandleBootstrapSceneLoaded;
            InstallForCurrentScene();
        }

        private static void HandleBootstrapSceneLoaded(Scene _, LoadSceneMode __)
        {
            InstallForCurrentScene();
        }

        private static void InstallForCurrentScene()
        {
            MatchController match = FindFirstObjectByType<MatchController>();
            if (match == null)
            {
                return;
            }

            PrototypeOpponentTeamController existing =
                FindFirstObjectByType<PrototypeOpponentTeamController>();
            if (existing != null)
            {
                existing.ScheduleSceneInitialization();
                return;
            }

            GameObject host = new GameObject("PrototypeOpponentTeamRuntime");
            PrototypeOpponentTeamController controller =
                host.AddComponent<PrototypeOpponentTeamController>();
            controller.ScheduleSceneInitialization();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += HandleSceneLoaded;
            hudDataSource = OpponentCombatHudDataSource.GetOrCreate(gameObject);
        }

        private void InitializeFromScene()
        {
            DetachMemberEvents();
            initialized = false;

            CurseBotController[] bots = FindObjectsByType<CurseBotController>(FindObjectsSortMode.None);
            if (bots == null || bots.Length < 2)
            {
                Debug.LogWarning("Gate 2B 상대 팀 프로토타입은 CurseBot 2개 이상이 필요합니다.");
                switchingMode = false;
                return;
            }

            List<CurseBotController> ordered = new List<CurseBotController>(bots);
            ordered.Sort(CompareBotsForTeamOrder);

            for (int index = 0; index < 2; index++)
            {
                Health health = ordered[index] != null ? ordered[index].GetComponent<Health>() : null;
                if (health == null)
                {
                    Debug.LogWarning("Gate 2B 상대 팀원 Health를 찾지 못했습니다.");
                    switchingMode = false;
                    return;
                }

                members[index] = health;
                memberSpawnPositions[index] = health.transform.position;
                memberSpawnRotations[index] = health.transform.rotation;
                health.Died += HandleMemberDeath;
            }

            activeIndex = 0;
            initialized = true;
            switchingMode = false;
            ApplyRequestedMode();
        }

        private static int CompareBotsForTeamOrder(CurseBotController left, CurseBotController right)
        {
            bool leftIsReservePrototype = left != null && left.name.Contains("_B");
            bool rightIsReservePrototype = right != null && right.name.Contains("_B");
            if (leftIsReservePrototype != rightIsReservePrototype)
            {
                return leftIsReservePrototype ? 1 : -1;
            }

            string leftName = left != null ? left.name : string.Empty;
            string rightName = right != null ? right.name : string.Empty;
            return string.Compare(leftName, rightName, StringComparison.Ordinal);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            if (sceneInitializationRoutine != null)
            {
                StopCoroutine(sceneInitializationRoutine);
                sceneInitializationRoutine = null;
            }
            DetachMemberEvents();
        }

        private void DetachMemberEvents()
        {
            for (int index = 0; index < members.Length; index++)
            {
                if (members[index] != null)
                {
                    members[index].Died -= HandleMemberDeath;
                }
                members[index] = null;
            }
        }

        private void HandleSceneLoaded(Scene _, LoadSceneMode __)
        {
            if (FindFirstObjectByType<MatchController>() == null)
            {
                if (sceneInitializationRoutine != null)
                {
                    StopCoroutine(sceneInitializationRoutine);
                    sceneInitializationRoutine = null;
                }

                DetachMemberEvents();
                initialized = false;
                switchingMode = false;
                return;
            }

            ScheduleSceneInitialization();
        }

        private void ScheduleSceneInitialization()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            if (sceneInitializationRoutine != null)
            {
                StopCoroutine(sceneInitializationRoutine);
            }

            sceneInitializationRoutine = StartCoroutine(InitializeAfterSceneReady());
        }

        private IEnumerator InitializeAfterSceneReady()
        {
            // MatchController creates the prototype enemy clone during scene startup.
            // sceneLoaded callbacks can race that setup on reload, so bind on the next frame.
            yield return null;

            sceneInitializationRoutine = null;
            if (FindFirstObjectByType<MatchController>() == null)
            {
                yield break;
            }

            InitializeFromScene();
        }

        private void Update()
        {
            if (!initialized || switchingMode)
            {
                return;
            }

            if (PrototypeDeveloperInput.OpponentModeTogglePressed)
            {
                requestedMode = IsTeamBattle
                    ? PrototypeEncounterMode.TrainingMultiCurse
                    : PrototypeEncounterMode.TeamBattle;

                switchingMode = true;
                initialized = false;
                DetachMemberEvents();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void ApplyRequestedMode()
        {
            if (!initialized)
            {
                return;
            }

            for (int index = 0; index < members.Length; index++)
            {
                Health member = members[index];
                if (member == null)
                {
                    continue;
                }

                member.transform.position = memberSpawnPositions[index];
                member.transform.rotation = memberSpawnRotations[index];
            }

            if (!IsTeamBattle)
            {
                for (int index = 0; index < members.Length; index++)
                {
                    if (members[index] != null && !members[index].gameObject.activeSelf)
                    {
                        members[index].gameObject.SetActive(true);
                    }
                }
                return;
            }

            activeIndex = ResolveStartingActiveIndex();
            int reserveIndex = 1 - activeIndex;

            Health active = members[activeIndex];
            Health reserve = members[reserveIndex];
            if (active != null && !active.gameObject.activeSelf)
            {
                active.gameObject.SetActive(true);
            }
            if (reserve != null && reserve.gameObject.activeSelf)
            {
                reserve.gameObject.SetActive(false);
            }
        }

        private int ResolveStartingActiveIndex()
        {
            if (members[0] != null && !members[0].IsDead)
            {
                return 0;
            }
            if (members[1] != null && !members[1].IsDead)
            {
                return 1;
            }
            return 0;
        }

        private void HandleMemberDeath(Health deadMember)
        {
            if (!initialized || !IsTeamBattle || deadMember == null)
            {
                return;
            }

            if (members[activeIndex] != deadMember)
            {
                return;
            }

            int reserveIndex = 1 - activeIndex;
            Health reserve = members[reserveIndex];
            if (reserve == null || reserve.IsDead)
            {
                return;
            }

            deadMember.gameObject.SetActive(false);

            activeIndex = reserveIndex;
            reserve.transform.position = memberSpawnPositions[reserveIndex];
            reserve.transform.rotation = memberSpawnRotations[reserveIndex];
            reserve.gameObject.SetActive(true);

            CurseBotController reserveBot = reserve.GetComponent<CurseBotController>();
            if (reserveBot != null)
            {
                reserveBot.enabled = true;
            }

            TransferTargetLockTo(reserve);
            entryNoticeUntil = Time.time + 1.5f;
        }

        private static void TransferTargetLockTo(Health newActive)
        {
            if (newActive == null || newActive.IsDead || !newActive.gameObject.activeInHierarchy)
            {
                return;
            }

            TargetLockController targetLock = FindFirstObjectByType<TargetLockController>();
            if (targetLock == null || !targetLock.enabled)
            {
                return;
            }

            targetLock.TryLockTarget(newActive);
        }

        private void OnGUI()
        {
            if (CombatHudPresentationMode.ProductionCanvasActive)
            {
                return;
            }

            hudDataSource ??= OpponentCombatHudDataSource.GetOrCreate(gameObject);
            OpponentCombatHudSnapshot snapshot = hudDataSource != null ? hudDataSource.Snapshot : default;
            if (!snapshot.IsValid)
            {
                return;
            }

            EnsureStyles();
            DrawModeChip(snapshot);
            if (snapshot.IsTeamBattle)
            {
                DrawOpponentTeamPanel(snapshot);
            }
        }

        private void DrawModeChip(OpponentCombatHudSnapshot snapshot)
        {
            float width = Mathf.Min(220f, Screen.width - 24f);
            Rect rect = new Rect(Screen.width * 0.5f - width * 0.5f, 8f, width, 21f);
            Color accent = snapshot.IsTeamBattle
                ? new Color(1f, 0.30f, 0.12f)
                : new Color(0.20f, 0.72f, 1f);

            DrawRect(rect, new Color(0.006f, 0.010f, 0.018f, 0.86f));
            DrawRect(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), new Color(accent.r, accent.g, accent.b, 0.62f));
            chipStyle.normal.textColor = new Color(0.82f, 0.86f, 0.94f);
            string developerKey = PrototypeDeveloperInput.BuildAllowsDeveloperHarness
                ? "F2   "
                : string.Empty;
            GUI.Label(rect, $"{developerKey}{snapshot.ModeLabel}", chipStyle);
        }

        private void DrawOpponentTeamPanel(OpponentCombatHudSnapshot snapshot)
        {
            float width = Mathf.Min(336f, Screen.width - 24f);
            Rect panel = new Rect(Screen.width - width - 12f, 12f, width, 76f);
            Color accent = new Color(1f, 0.30f, 0.12f);

            DrawHudPlate(panel, accent);
            DrawRect(new Rect(panel.xMax - 4f, panel.y, 4f, panel.height), accent);

            string notice = snapshot.ReserveEntryNotice ? "RESERVE ENTRY" : string.Empty;
            GUI.Label(
                new Rect(panel.x + 10f, panel.y + 3f, panel.width * 0.55f, 18f),
                "OPPONENT",
                titleStyle
            );
            metaStyle.alignment = TextAnchor.MiddleRight;
            metaStyle.normal.textColor = notice.Length > 0
                ? new Color(1f, 0.56f, 0.22f)
                : accent;
            GUI.Label(
                new Rect(panel.x + panel.width * 0.42f, panel.y + 3f, panel.width * 0.54f - 9f, 18f),
                notice.Length > 0 ? notice : $"{snapshot.LivingMemberCount} / {snapshot.TeamSize}",
                metaStyle
            );
            metaStyle.alignment = TextAnchor.MiddleLeft;

            DrawMemberRow(
                new Rect(panel.x + 9f, panel.y + 24f, panel.width - 18f, 23f),
                snapshot.ActiveMember
            );
            DrawMemberRow(
                new Rect(panel.x + 9f, panel.y + 50f, panel.width - 18f, 20f),
                snapshot.ReserveMember
            );
        }

        private void DrawMemberRow(Rect rect, OpponentTeamMemberHudSnapshot member)
        {
            bool knockedOut = !member.IsValid || member.KnockedOut;
            Color accent = knockedOut
                ? new Color(0.40f, 0.40f, 0.45f)
                : member.MemberIndex == 0
                    ? new Color(0.94f, 0.15f, 0.20f)
                    : new Color(1f, 0.38f, 0.10f);
            Color background = member.IsActive
                ? new Color(accent.r * 0.09f, accent.g * 0.07f, accent.b * 0.06f, 0.94f)
                : new Color(0.032f, 0.026f, 0.032f, 0.84f);

            DrawRect(rect, background);
            DrawRect(new Rect(rect.xMax - (member.IsActive ? 4f : 2f), rect.y, member.IsActive ? 4f : 2f, rect.height), accent);
            DrawBorder(rect, new Color(accent.r, accent.g, accent.b, member.IsActive ? 0.72f : 0.34f), 1f);

            string role = member.IsActive ? "A" : "R";
            string hp = member.IsValid ? $"HP {member.CurrentHealth:0}/{member.MaxHealth:0}" : "MISSING";
            string ko = knockedOut ? " · KO" : string.Empty;
            rowStyle.normal.textColor = knockedOut ? new Color(0.62f, 0.62f, 0.66f) : Color.white;
            GUI.Label(rect, $"{role}   {member.DisplayName}   {hp}{ko}", rowStyle);
        }

        private static void DrawHudPlate(Rect rect, Color accent)
        {
            DrawRect(rect, new Color(0.012f, 0.008f, 0.014f, 0.91f));
            DrawRect(
                new Rect(rect.x + 5f, rect.y + 4f, rect.width - 10f, rect.height - 8f),
                new Color(accent.r * 0.04f, accent.g * 0.025f, accent.b * 0.02f, 0.36f)
            );
            DrawBorder(rect, new Color(accent.r, accent.g, accent.b, 0.42f), 1f);
        }

        private void EnsureStyles()
        {
            if (styledForHeight == Screen.height)
            {
                return;
            }

            styledForHeight = Screen.height;
            int baseSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 64f, 11f, 16f));
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = baseSize + 1,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
            };
            titleStyle.normal.textColor = Color.white;

            metaStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(9, baseSize - 2),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
            };
            metaStyle.normal.textColor = new Color(0.76f, 0.78f, 0.84f);

            rowStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(10, baseSize - 1),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            rowStyle.normal.textColor = Color.white;

            chipStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(9, baseSize - 2),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
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
