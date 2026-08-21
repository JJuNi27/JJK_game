using System;
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
        private float entryNoticeUntil;
        private GUIStyle titleStyle;
        private GUIStyle rowStyle;
        private GUIStyle chipStyle;
        private int styledForHeight = -1;

        public PrototypeEncounterMode Mode => requestedMode;
        public bool IsTeamBattle => Mode == PrototypeEncounterMode.TeamBattle;
        public static bool TeamBattleModeRequested => requestedMode == PrototypeEncounterMode.TeamBattle;
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapAfterSceneLoad()
        {
            MatchController match = FindFirstObjectByType<MatchController>();
            if (match == null)
            {
                return;
            }

            PrototypeOpponentTeamController existing = FindFirstObjectByType<PrototypeOpponentTeamController>();
            if (existing != null)
            {
                return;
            }

            GameObject host = new GameObject("PrototypeOpponentTeamRuntime");
            PrototypeOpponentTeamController controller = host.AddComponent<PrototypeOpponentTeamController>();
            controller.InitializeFromScene();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += HandleSceneLoaded;
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
            InitializeFromScene();
        }

        private void Update()
        {
            if (!initialized || switchingMode)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.F2))
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
            if (!initialized)
            {
                return;
            }

            EnsureStyles();
            DrawModeChip();
            if (IsTeamBattle)
            {
                DrawOpponentTeamPanel();
            }
        }

        private void DrawModeChip()
        {
            float width = Mathf.Min(270f, Screen.width - 24f);
            Rect rect = new Rect(Screen.width * 0.5f - width * 0.5f, 35f, width, 25f);
            Color accent = IsTeamBattle
                ? new Color(1f, 0.32f, 0.14f)
                : new Color(0.25f, 0.78f, 1f);
            DrawRect(rect, new Color(0.020f, 0.025f, 0.040f, 0.94f));
            DrawBorder(rect, accent, 2f);
            chipStyle.normal.textColor = accent;
            string modeName = IsTeamBattle ? "TEAM BATTLE" : "TRAINING · MULTI CURSE";
            GUI.Label(rect, $"F2 · MODE · {modeName}", chipStyle);
        }

        private void DrawOpponentTeamPanel()
        {
            float width = Mathf.Min(310f, Screen.width - 24f);
            Rect panel = new Rect(Screen.width - width - 12f, 12f, width, 83f);
            DrawRect(panel, new Color(0.025f, 0.016f, 0.020f, 0.96f));
            DrawBorder(panel, new Color(1f, 0.34f, 0.14f, 0.95f), 2f);

            string notice = Time.time < entryNoticeUntil ? " · RESERVE ENTRY" : string.Empty;
            GUI.Label(
                new Rect(panel.x + 8f, panel.y + 2f, panel.width - 16f, 22f),
                $"OPPONENT TEAM · {LivingMemberCount}/2{notice}",
                titleStyle
            );

            DrawMemberRow(
                new Rect(panel.x + 8f, panel.y + 27f, panel.width - 16f, 23f),
                activeIndex,
                true
            );
            DrawMemberRow(
                new Rect(panel.x + 8f, panel.y + 54f, panel.width - 16f, 23f),
                1 - activeIndex,
                false
            );
        }

        private void DrawMemberRow(Rect rect, int index, bool active)
        {
            Health member = members[index];
            bool knockedOut = member == null || member.IsDead;
            Color accent = knockedOut
                ? new Color(0.42f, 0.42f, 0.46f)
                : index == 0
                    ? new Color(0.94f, 0.15f, 0.20f)
                    : new Color(0.95f, 0.34f, 0.10f);

            DrawRect(rect, new Color(0.040f, 0.035f, 0.045f, 0.98f));
            DrawBorder(rect, accent, active ? 2f : 1f);

            string role = active ? "ACTIVE" : "RESERVE";
            string hp = member != null ? $"HP {member.CurrentHealth:0}/{member.MaxHealth:0}" : "MISSING";
            string ko = knockedOut ? " · KO" : string.Empty;
            rowStyle.normal.textColor = knockedOut ? new Color(0.62f, 0.62f, 0.66f) : Color.white;
            GUI.Label(rect, $"{role} · CURSE {(char)('A' + index)} · {hp}{ko}", rowStyle);
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
                fontSize = baseSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
            };
            titleStyle.normal.textColor = Color.white;

            rowStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(10, baseSize - 1),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            rowStyle.normal.textColor = Color.white;

            chipStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(10, baseSize - 1),
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
