using JJKGame.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.Player
{
    [DefaultExecutionOrder(1100)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(PrototypeCharacterController))]
    public sealed class PrototypePlayerTeamController : MonoBehaviour
    {
        private sealed class TeamMemberState
        {
            public TeamMemberState(PrototypeCharacterId characterId)
            {
                CharacterId = characterId;
            }

            public PrototypeCharacterId CharacterId { get; }
            public bool Initialized { get; set; }
            public float Health { get; set; }
            public float Energy { get; set; }
            public bool KnockedOut => Initialized && Health <= 0f;
        }

        private static bool megumiStressRosterRequested;

        [Header("Gate 5B · Active / Reserve Slots")]
        [SerializeField, Min(0f)] private float manualTagCooldown = 1.25f;
        [SerializeField, Min(0f)] private float manualTagInvulnerability = 0.30f;
        [SerializeField, Min(0f)] private float koTagInvulnerability = 0.80f;
        [SerializeField] private bool enableDeveloperHarness = true;

        private TeamMemberState[] members;

        private Health health;
        private CursedEnergyController cursedEnergy;
        private PrototypeCharacterController characterController;
        private CombatActionGate actionGate;
        private BasicAttack basicAttack;
        private PlayerCombatHudDataSource hudDataSource;
        private float nextManualTagAt;
        private bool switchingMember;
        private GUIStyle titleStyle;
        private GUIStyle metaStyle;
        private GUIStyle rowStyle;
        private GUIStyle chipStyle;
        private int styledForHeight = -1;

        public static bool MegumiStressRosterRequested => megumiStressRosterRequested;
        public int TeamSize => members != null ? members.Length : 0;
        public bool IsTeamBattle => TeamSize > 1;
        public bool HasReserve1 => TeamSize >= 2;
        public bool HasReserve2 => TeamSize >= 3;
        public PrototypeCharacterId ActiveCharacter =>
            TeamSize > 0 ? members[0].CharacterId : PrototypeCharacterId.GojoModern;
        public PrototypeCharacterId ReserveCharacter =>
            HasReserve1 ? members[1].CharacterId : ActiveCharacter;
        public PrototypeCharacterId Reserve2Character =>
            HasReserve2 ? members[2].CharacterId : ActiveCharacter;
        public bool HasLivingReserve => FindFirstLivingReserveIndex() >= 1;
        public float ManualTagCooldownRemaining => Mathf.Max(0f, nextManualTagAt - Time.time);

        public static PrototypePlayerTeamController GetOrCreate(GameObject owner)
        {
            if (owner == null)
            {
                return null;
            }

            PrototypePlayerTeamController team = owner.GetComponent<PrototypePlayerTeamController>();
            return team != null ? team : owner.AddComponent<PrototypePlayerTeamController>();
        }

        public bool TryGetStoredMemberState(
            PrototypeCharacterId characterId,
            out bool initialized,
            out float storedHealth,
            out float storedEnergy,
            out bool knockedOut
        )
        {
            if (members != null)
            {
                for (int index = 0; index < members.Length; index++)
                {
                    TeamMemberState member = members[index];
                    if (member.CharacterId != characterId)
                    {
                        continue;
                    }

                    initialized = member.Initialized;
                    storedHealth = member.Health;
                    storedEnergy = member.Energy;
                    knockedOut = member.KnockedOut;
                    return true;
                }
            }

            initialized = false;
            storedHealth = 0f;
            storedEnergy = 0f;
            knockedOut = false;
            return false;
        }

        public bool TryGetSlotMemberState(
            int slotIndex,
            out PrototypeCharacterId characterId,
            out bool initialized,
            out float storedHealth,
            out float storedEnergy,
            out bool knockedOut
        )
        {
            if (members != null && slotIndex >= 0 && slotIndex < members.Length)
            {
                TeamMemberState member = members[slotIndex];
                characterId = member.CharacterId;
                initialized = member.Initialized;
                storedHealth = member.Health;
                storedEnergy = member.Energy;
                knockedOut = member.KnockedOut;
                return true;
            }

            characterId = PrototypeCharacterId.GojoModern;
            initialized = false;
            storedHealth = 0f;
            storedEnergy = 0f;
            knockedOut = false;
            return false;
        }

        private void Awake()
        {
            BuildRoster();

            health = GetComponent<Health>();
            cursedEnergy = CursedEnergyController.GetOrCreate(gameObject);
            characterController = GetComponent<PrototypeCharacterController>();
            actionGate = CombatActionGate.GetOrCreate(gameObject);
            basicAttack = GetComponent<BasicAttack>();
            hudDataSource = PlayerCombatHudDataSource.GetOrCreate(gameObject);

            if (health != null)
            {
                health.DamageResolved += HandleDamageResolved;
            }
        }

        private void Start()
        {
            if (TeamSize <= 0)
            {
                BuildPrototypeDefaultRoster();
            }

            characterController ??= GetComponent<PrototypeCharacterController>();
            PrototypeCharacterId mainCharacter = members[0].CharacterId;
            if (characterController != null && characterController.ActiveCharacter != mainCharacter)
            {
                characterController.ApplyCharacter(mainCharacter, true);
            }

            CaptureActiveState();
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.DamageResolved -= HandleDamageResolved;
            }
        }

        private void Update()
        {
            if (enableDeveloperHarness && Input.GetKeyDown(KeyCode.F3))
            {
                ToggleMegumiStressRoster();
                return;
            }

            if (enableDeveloperHarness && Input.GetKeyDown(KeyCode.F4))
            {
                CycleRuntimeTeamSizeHarness();
                return;
            }

            if (health == null || health.IsDead || switchingMember || TeamSize <= 1)
            {
                return;
            }

            bool reserve1Pressed = Input.GetKeyDown(CombatInputBindings.Reserve1Tag);
            bool reserve2Pressed = Input.GetKeyDown(CombatInputBindings.Reserve2Tag);
            bool legacyTagPressed =
                enableDeveloperHarness && Input.GetKeyDown(CombatInputBindings.Tag);

            if (!reserve1Pressed && !reserve2Pressed && !legacyTagPressed)
            {
                return;
            }

            int reserveSlotIndex = reserve2Pressed ? 2 : 1;
            TryManualTag(reserveSlotIndex);
        }

        private void BuildRoster()
        {
            MatchTeamSelection selection = MatchTeamSelectionStore.PlayerTeam;
            if (selection == null || selection.TeamSize < 1 || selection.TeamSize > 3)
            {
                BuildPrototypeDefaultRoster();
                return;
            }

            members = new TeamMemberState[selection.TeamSize];
            for (int index = 0; index < members.Length; index++)
            {
                PrototypeCharacterId characterId = selection.GetRequired((MatchTeamSlot)index);
                members[index] = new TeamMemberState(characterId);
            }
        }

        private void BuildPrototypeDefaultRoster()
        {
            MatchTeamSelectionStore.ResetPrototypeDefault();
            MatchTeamSelection selection = MatchTeamSelectionStore.PlayerTeam;
            members = new[]
            {
                new TeamMemberState(selection.Main),
                new TeamMemberState(selection.Reserve1),
            };
            megumiStressRosterRequested = false;
        }

        private void TryManualTag(int reserveSlotIndex)
        {
            if (
                reserveSlotIndex <= 0
                || reserveSlotIndex >= TeamSize
                || Time.time < nextManualTagAt
            )
            {
                return;
            }

            actionGate ??= CombatActionGate.GetOrCreate(gameObject);
            if (actionGate == null || actionGate.CurrentState != CombatActionState.Normal)
            {
                return;
            }

            TeamMemberState reserve = members[reserveSlotIndex];
            if (reserve.KnockedOut)
            {
                return;
            }

            SwitchWithReserveSlot(reserveSlotIndex, false);
        }

        private void HandleDamageResolved(
            Health resolvedHealth,
            DamageContext _,
            DamageResolution resolution
        )
        {
            if (
                switchingMember
                || resolvedHealth != health
                || resolution != DamageResolution.Applied
                || health == null
                || !health.IsDead
            )
            {
                return;
            }

            CaptureActiveState();
            int reserveSlotIndex = FindFirstLivingReserveIndex();
            if (reserveSlotIndex < 1)
            {
                return;
            }

            SwitchWithReserveSlot(reserveSlotIndex, true);
        }

        private void SwitchWithReserveSlot(int reserveSlotIndex, bool fromKnockout)
        {
            if (
                reserveSlotIndex <= 0
                || reserveSlotIndex >= TeamSize
                || members[reserveSlotIndex].KnockedOut
            )
            {
                return;
            }

            switchingMember = true;
            CaptureActiveState();

            TeamMemberState outgoing = members[0];
            TeamMemberState incoming = members[reserveSlotIndex];
            bool firstActivation = !incoming.Initialized;

            characterController ??= GetComponent<PrototypeCharacterController>();
            characterController?.ApplyCharacter(incoming.CharacterId, firstActivation);

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            if (firstActivation)
            {
                incoming.Initialized = true;
                incoming.Health = health != null ? health.CurrentHealth : 0f;
                incoming.Energy = cursedEnergy != null ? cursedEnergy.CurrentEnergy : 0f;
            }
            else
            {
                health?.SetCurrentHealth(incoming.Health);
                cursedEnergy?.SetCurrentEnergy(incoming.Energy);
            }

            members[0] = incoming;
            members[reserveSlotIndex] = outgoing;

            basicAttack ??= GetComponent<BasicAttack>();
            basicAttack?.ResetCombatSequence();

            float invulnerability = fromKnockout
                ? koTagInvulnerability
                : manualTagInvulnerability;
            health?.GrantInvulnerability(invulnerability);
            nextManualTagAt = Time.time + manualTagCooldown;
            switchingMember = false;
        }

        private void CaptureActiveState()
        {
            if (TeamSize <= 0)
            {
                return;
            }

            TeamMemberState active = members[0];
            active.Initialized = true;
            active.Health = health != null ? health.CurrentHealth : active.Health;
            active.Energy = cursedEnergy != null ? cursedEnergy.CurrentEnergy : active.Energy;
        }

        private int FindFirstLivingReserveIndex()
        {
            if (members == null)
            {
                return -1;
            }

            for (int index = 1; index < members.Length; index++)
            {
                if (!members[index].KnockedOut)
                {
                    return index;
                }
            }
            return -1;
        }

        private void ToggleMegumiStressRoster()
        {
            megumiStressRosterRequested = !megumiStressRosterRequested;
            MatchTeamSelectionStore.SetPlayerTeam(
                megumiStressRosterRequested
                    ? MatchTeamSelection.Duo(
                        PrototypeCharacterId.GojoModern,
                        PrototypeCharacterId.MegumiStudent
                    )
                    : MatchTeamSelection.Duo(
                        PrototypeCharacterId.GojoModern,
                        PrototypeCharacterId.SukunaShibuyaYujiBody
                    )
            );
            ReloadActiveScene();
        }

        private void CycleRuntimeTeamSizeHarness()
        {
            megumiStressRosterRequested = false;
            MatchTeamSelection current = MatchTeamSelectionStore.PlayerTeam;
            int currentSize = current != null ? current.TeamSize : 2;

            if (currentSize == 2)
            {
                MatchTeamSelectionStore.SetPlayerTeam(
                    MatchTeamSelection.Trio(
                        PrototypeCharacterId.GojoModern,
                        PrototypeCharacterId.SukunaShibuyaYujiBody,
                        PrototypeCharacterId.MegumiStudent
                    )
                );
            }
            else if (currentSize == 3)
            {
                MatchTeamSelectionStore.SetPlayerTeam(
                    MatchTeamSelection.Solo(PrototypeCharacterId.GojoModern)
                );
            }
            else
            {
                MatchTeamSelectionStore.ResetPrototypeDefault();
            }

            ReloadActiveScene();
        }

        private static void ReloadActiveScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnGUI()
        {
            hudDataSource ??= PlayerCombatHudDataSource.GetOrCreate(gameObject);
            PlayerCombatHudSnapshot snapshot = hudDataSource != null
                ? hudDataSource.Snapshot
                : default;
            if (!snapshot.IsValid || !snapshot.TeamMode)
            {
                return;
            }

            EnsureStyles();
            DrawActivePlayerOverlay(snapshot);
            DrawTeamPanel(snapshot);
        }

        private void DrawActivePlayerOverlay(PlayerCombatHudSnapshot snapshot)
        {
            CharacterPresentationProfile profile = snapshot.PresentationProfile;
            const float margin = 12f;
            float panelWidth = Mathf.Clamp((Screen.width - margin * 3f) * 0.37f, 255f, 370f);
            Rect rect = new Rect(margin, margin, panelWidth, 76f);
            Color accent = profile.HudAccent;
            Color secondary = profile.EnergyAccent;

            DrawHudPlate(rect, accent, true);
            DrawRect(new Rect(rect.x, rect.y, 5f, rect.height), accent);
            DrawRect(
                new Rect(rect.x + 5f, rect.y, rect.width - 5f, 2f),
                new Color(accent.r, accent.g, accent.b, 0.75f)
            );

            GUI.Label(
                new Rect(rect.x + 14f, rect.y + 5f, rect.width - 92f, 18f),
                profile.ShortName.ToUpperInvariant(),
                titleStyle
            );
            metaStyle.alignment = TextAnchor.MiddleRight;
            metaStyle.normal.textColor = accent;
            GUI.Label(
                new Rect(rect.x + rect.width - 84f, rect.y + 5f, 72f, 18f),
                "ACTIVE",
                metaStyle
            );
            metaStyle.alignment = TextAnchor.MiddleLeft;
            metaStyle.normal.textColor = new Color(0.68f, 0.73f, 0.84f);
            GUI.Label(
                new Rect(rect.x + 14f, rect.y + 21f, rect.width - 28f, 15f),
                profile.DisplayName,
                metaStyle
            );

            DrawValueBar(
                new Rect(rect.x + 14f, rect.y + 39f, rect.width - 28f, 18f),
                snapshot.CurrentHealth,
                snapshot.MaxHealth,
                accent,
                $"HP  {snapshot.CurrentHealth:0} / {snapshot.MaxHealth:0}"
            );

            if (snapshot.HasEnergy)
            {
                DrawValueBar(
                    new Rect(rect.x + 14f, rect.y + 60f, rect.width - 28f, 10f),
                    snapshot.CurrentEnergy,
                    snapshot.MaxEnergy,
                    secondary,
                    $"CE {snapshot.CurrentEnergy:0}/{snapshot.MaxEnergy:0}"
                );
            }
        }

        private void DrawTeamPanel(PlayerCombatHudSnapshot snapshot)
        {
            float width = Mathf.Min(352f, Screen.width - 24f);
            float height = snapshot.TeamSize >= 3 ? 102f : 78f;
            float panelY = Mathf.Max(270f, Screen.height - height - 98f);
            panelY = Mathf.Min(panelY, Screen.height - height - 10f);
            Rect panel = new Rect(12f, panelY, width, height);
            Color activeAccent = snapshot.PresentationProfile.HudAccent;

            DrawHudPlate(panel, new Color(0.46f, 0.55f, 0.76f), false);
            DrawRect(
                new Rect(panel.x, panel.y, 3f, panel.height),
                new Color(activeAccent.r, activeAccent.g, activeAccent.b, 0.78f)
            );

            GUI.Label(
                new Rect(panel.x + 10f, panel.y + 2f, panel.width * 0.35f, 18f),
                $"TEAM · {snapshot.TeamSize}",
                titleStyle
            );

            string reserveStatus = BuildReserveControlStatus(snapshot);
            metaStyle.alignment = TextAnchor.MiddleRight;
            metaStyle.normal.textColor = new Color(0.72f, 0.78f, 0.90f);
            GUI.Label(
                new Rect(panel.x + panel.width * 0.30f, panel.y + 2f, panel.width * 0.66f - 8f, 18f),
                reserveStatus,
                metaStyle
            );
            metaStyle.alignment = TextAnchor.MiddleLeft;

            DrawMemberRow(
                new Rect(panel.x + 9f, panel.y + 23f, panel.width - 18f, 23f),
                snapshot.ActiveMember,
                "A"
            );
            DrawMemberRow(
                new Rect(panel.x + 9f, panel.y + 49f, panel.width - 18f, 21f),
                snapshot.ReserveMember,
                "R1"
            );
            if (snapshot.TeamSize >= 3)
            {
                DrawMemberRow(
                    new Rect(panel.x + 9f, panel.y + 73f, panel.width - 18f, 21f),
                    snapshot.Reserve2Member,
                    "R2"
                );
            }
        }

        private void DrawMemberRow(
            Rect rect,
            PlayerTeamMemberHudSnapshot member,
            string role
        )
        {
            if (!member.IsValid || member.PresentationProfile == null)
            {
                return;
            }

            CharacterPresentationProfile profile = member.PresentationProfile;
            Color accent = member.KnockedOut
                ? new Color(0.38f, 0.39f, 0.44f)
                : profile.HudAccent;
            Color background = member.IsActive
                ? new Color(accent.r * 0.10f, accent.g * 0.10f, accent.b * 0.10f, 0.94f)
                : new Color(0.025f, 0.030f, 0.045f, 0.84f);

            DrawRect(rect, background);
            DrawRect(new Rect(rect.x, rect.y, member.IsActive ? 4f : 2f, rect.height), accent);
            DrawBorder(
                rect,
                new Color(accent.r, accent.g, accent.b, member.IsActive ? 0.72f : 0.34f),
                1f
            );

            string hpText = member.Initialized ? $"HP {member.Health:0}" : "HP READY";
            string energyText = member.Initialized ? $"CE {member.Energy:0}" : "CE START";
            string down = member.KnockedOut ? " · KO" : string.Empty;
            rowStyle.normal.textColor = member.KnockedOut
                ? new Color(0.58f, 0.59f, 0.64f)
                : Color.white;
            GUI.Label(
                rect,
                $"{role}  {profile.ShortName}   {hpText}   {energyText}{down}",
                rowStyle
            );
        }

        private static string BuildReserveControlStatus(PlayerCombatHudSnapshot snapshot)
        {
            string reserve1 =
                $"{CombatInputBindings.Reserve1TagLabel} R1 {BuildTagStatus(snapshot.Reserve1TagState, snapshot.TagCooldownRemaining)}";
            if (snapshot.TeamSize < 3)
            {
                return reserve1;
            }

            string reserve2 =
                $"{CombatInputBindings.Reserve2TagLabel} R2 {BuildTagStatus(snapshot.Reserve2TagState, snapshot.TagCooldownRemaining)}";
            return $"{reserve1} · {reserve2}";
        }

        private static string BuildTagStatus(PlayerTagHudState state, float cooldownRemaining)
        {
            return state switch
            {
                PlayerTagHudState.ReserveKnockedOut => "KO",
                PlayerTagHudState.Cooldown => $"{cooldownRemaining:0.0}s",
                PlayerTagHudState.ActionLocked => "LOCK",
                PlayerTagHudState.Ready => "READY",
                _ => "--",
            };
        }

        private void DrawValueBar(Rect rect, float value, float max, Color fill, string text)
        {
            DrawRect(rect, new Color(0.055f, 0.065f, 0.090f, 0.96f));
            float ratio = max > 0f ? Mathf.Clamp01(value / max) : 0f;
            DrawRect(
                new Rect(rect.x + 1f, rect.y + 1f, (rect.width - 2f) * ratio, rect.height - 2f),
                fill
            );
            DrawRect(
                new Rect(rect.x + 1f, rect.y + 1f, (rect.width - 2f) * ratio, 2f),
                new Color(1f, 1f, 1f, 0.18f)
            );
            DrawBorder(rect, new Color(1f, 1f, 1f, 0.14f), 1f);
            GUI.Label(rect, text, chipStyle);
        }

        private static void DrawHudPlate(Rect rect, Color accent, bool stronger)
        {
            DrawRect(
                rect,
                stronger
                    ? new Color(0.006f, 0.010f, 0.020f, 0.94f)
                    : new Color(0.008f, 0.012f, 0.022f, 0.88f)
            );
            DrawRect(
                new Rect(rect.x + 5f, rect.y + 4f, rect.width - 10f, rect.height - 8f),
                new Color(accent.r * 0.05f, accent.g * 0.05f, accent.b * 0.05f, 0.36f)
            );
            DrawBorder(
                rect,
                new Color(accent.r, accent.g, accent.b, stronger ? 0.55f : 0.30f),
                1f
            );
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
            metaStyle.normal.textColor = new Color(0.68f, 0.73f, 0.84f);

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
