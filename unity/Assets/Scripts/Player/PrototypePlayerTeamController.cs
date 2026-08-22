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

        [Header("Gate 2A · Active / Reserve")]
        [SerializeField, Min(0f)] private float manualTagCooldown = 1.25f;
        [SerializeField, Min(0f)] private float manualTagInvulnerability = 0.30f;
        [SerializeField, Min(0f)] private float koTagInvulnerability = 0.80f;

        private TeamMemberState[] members;

        private Health health;
        private CursedEnergyController cursedEnergy;
        private PrototypeCharacterController characterController;
        private CombatActionGate actionGate;
        private BasicAttack basicAttack;
        private PlayerCombatHudDataSource hudDataSource;
        private int activeIndex;
        private float nextManualTagAt;
        private bool switchingMember;
        private GUIStyle titleStyle;
        private GUIStyle metaStyle;
        private GUIStyle rowStyle;
        private GUIStyle chipStyle;
        private int styledForHeight = -1;

        public static bool MegumiStressRosterRequested => megumiStressRosterRequested;
        public PrototypeCharacterId ActiveCharacter => members[activeIndex].CharacterId;
        public PrototypeCharacterId ReserveCharacter => members[1 - activeIndex].CharacterId;
        public bool HasLivingReserve => !members[1 - activeIndex].KnockedOut;
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
            PrototypeCharacterId currentCharacter = characterController != null
                ? characterController.ActiveCharacter
                : PrototypeCharacterId.GojoModern;
            activeIndex = ResolveIndex(currentCharacter);
            if (activeIndex < 0)
            {
                activeIndex = 0;
                characterController?.ApplyCharacter(members[0].CharacterId, true);
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
            if (Input.GetKeyDown(KeyCode.F3))
            {
                megumiStressRosterRequested = !megumiStressRosterRequested;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }

            if (
                health == null
                || health.IsDead
                || switchingMember
                || !Input.GetKeyDown(CombatInputBindings.Tag)
            )
            {
                return;
            }

            TryManualTag();
        }

        private void BuildRoster()
        {
            members = new[]
            {
                new TeamMemberState(PrototypeCharacterId.GojoModern),
                new TeamMemberState(
                    megumiStressRosterRequested
                        ? PrototypeCharacterId.MegumiStudent
                        : PrototypeCharacterId.SukunaShibuyaYujiBody
                ),
            };
        }

        private void TryManualTag()
        {
            if (Time.time < nextManualTagAt)
            {
                return;
            }

            actionGate ??= CombatActionGate.GetOrCreate(gameObject);
            if (actionGate == null || actionGate.CurrentState != CombatActionState.Normal)
            {
                return;
            }

            int reserveIndex = 1 - activeIndex;
            TeamMemberState reserve = members[reserveIndex];
            if (reserve.KnockedOut)
            {
                return;
            }

            CaptureActiveState();
            SwitchTo(reserveIndex, false);
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
            int reserveIndex = 1 - activeIndex;
            if (members[reserveIndex].KnockedOut)
            {
                return;
            }

            SwitchTo(reserveIndex, true);
        }

        private void SwitchTo(int nextIndex, bool fromKnockout)
        {
            if (nextIndex < 0 || nextIndex >= members.Length || nextIndex == activeIndex)
            {
                return;
            }

            TeamMemberState next = members[nextIndex];
            if (next.KnockedOut)
            {
                return;
            }

            switchingMember = true;
            bool firstActivation = !next.Initialized;

            characterController ??= GetComponent<PrototypeCharacterController>();
            characterController?.ApplyCharacter(next.CharacterId, firstActivation);

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            if (firstActivation)
            {
                next.Initialized = true;
                next.Health = health != null ? health.CurrentHealth : 0f;
                next.Energy = cursedEnergy != null ? cursedEnergy.CurrentEnergy : 0f;
            }
            else
            {
                health?.SetCurrentHealth(next.Health);
                cursedEnergy?.SetCurrentEnergy(next.Energy);
            }

            activeIndex = nextIndex;
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
            if (members == null || activeIndex < 0 || activeIndex >= members.Length)
            {
                return;
            }

            TeamMemberState active = members[activeIndex];
            active.Initialized = true;
            active.Health = health != null ? health.CurrentHealth : active.Health;
            active.Energy = cursedEnergy != null ? cursedEnergy.CurrentEnergy : active.Energy;
        }

        private int ResolveIndex(PrototypeCharacterId characterId)
        {
            if (members == null)
            {
                return -1;
            }

            for (int index = 0; index < members.Length; index++)
            {
                if (members[index].CharacterId == characterId)
                {
                    return index;
                }
            }

            return -1;
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
            DrawRect(new Rect(rect.x + 5f, rect.y, rect.width - 5f, 2f), new Color(accent.r, accent.g, accent.b, 0.75f));

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
            float width = Mathf.Min(336f, Screen.width - 24f);
            float panelY = Mathf.Max(298f, Screen.height - 176f);
            panelY = Mathf.Min(panelY, Screen.height - 86f);
            Rect panel = new Rect(12f, panelY, width, 76f);
            Color activeAccent = snapshot.PresentationProfile.HudAccent;

            DrawHudPlate(panel, new Color(0.46f, 0.55f, 0.76f), false);
            DrawRect(new Rect(panel.x, panel.y, 3f, panel.height), new Color(activeAccent.r, activeAccent.g, activeAccent.b, 0.78f));

            string tagStatus = BuildTagStatus(snapshot);
            GUI.Label(
                new Rect(panel.x + 10f, panel.y + 2f, panel.width * 0.52f, 18f),
                megumiStressRosterRequested ? "TEAM · G/M" : "TEAM · G/S",
                titleStyle
            );
            metaStyle.alignment = TextAnchor.MiddleRight;
            metaStyle.normal.textColor = snapshot.TagState == PlayerTagHudState.Ready
                ? activeAccent
                : new Color(0.68f, 0.72f, 0.80f);
            GUI.Label(
                new Rect(panel.x + panel.width * 0.42f, panel.y + 2f, panel.width * 0.54f - 8f, 18f),
                $"{CombatInputBindings.TagLabel} TAG · {tagStatus}",
                metaStyle
            );
            metaStyle.alignment = TextAnchor.MiddleLeft;

            DrawMemberRow(
                new Rect(panel.x + 9f, panel.y + 23f, panel.width - 18f, 23f),
                snapshot.ActiveMember
            );
            DrawMemberRow(
                new Rect(panel.x + 9f, panel.y + 49f, panel.width - 18f, 21f),
                snapshot.ReserveMember
            );
        }

        private void DrawMemberRow(Rect rect, PlayerTeamMemberHudSnapshot member)
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
            DrawBorder(rect, new Color(accent.r, accent.g, accent.b, member.IsActive ? 0.72f : 0.34f), 1f);

            string role = member.IsActive ? "A" : "R";
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

        private static string BuildTagStatus(PlayerCombatHudSnapshot snapshot)
        {
            return snapshot.TagState switch
            {
                PlayerTagHudState.ReserveKnockedOut => "RESERVE KO",
                PlayerTagHudState.Cooldown => $"{snapshot.TagCooldownRemaining:0.0}s",
                PlayerTagHudState.ActionLocked => "ACTION LOCK",
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
            DrawRect(rect, stronger
                ? new Color(0.006f, 0.010f, 0.020f, 0.94f)
                : new Color(0.008f, 0.012f, 0.022f, 0.88f));
            DrawRect(
                new Rect(rect.x + 5f, rect.y + 4f, rect.width - 10f, rect.height - 8f),
                new Color(accent.r * 0.05f, accent.g * 0.05f, accent.b * 0.05f, 0.36f)
            );
            DrawBorder(rect, new Color(accent.r, accent.g, accent.b, stronger ? 0.55f : 0.30f), 1f);
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
