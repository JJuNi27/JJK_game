using JJKGame.Core;
using UnityEngine;

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

        [Header("Gate 2A · Active / Reserve")]
        [SerializeField, Min(0f)] private float manualTagCooldown = 1.25f;
        [SerializeField, Min(0f)] private float manualTagInvulnerability = 0.30f;
        [SerializeField, Min(0f)] private float koTagInvulnerability = 0.80f;

        private readonly TeamMemberState[] members =
        {
            new TeamMemberState(PrototypeCharacterId.GojoModern),
            new TeamMemberState(PrototypeCharacterId.SukunaShibuyaYujiBody),
        };

        private Health health;
        private CursedEnergyController cursedEnergy;
        private PrototypeCharacterController characterController;
        private CombatActionGate actionGate;
        private BasicAttack basicAttack;
        private int activeIndex;
        private float nextManualTagAt;
        private bool switchingMember;
        private GUIStyle titleStyle;
        private GUIStyle rowStyle;
        private GUIStyle chipStyle;
        private int styledForHeight = -1;

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

        private void Awake()
        {
            health = GetComponent<Health>();
            cursedEnergy = CursedEnergyController.GetOrCreate(gameObject);
            characterController = GetComponent<PrototypeCharacterController>();
            actionGate = CombatActionGate.GetOrCreate(gameObject);
            basicAttack = GetComponent<BasicAttack>();

            if (health != null)
            {
                health.DamageResolved += HandleDamageResolved;
            }
        }

        private void Start()
        {
            activeIndex = ResolveIndex(characterController != null
                ? characterController.ActiveCharacter
                : PrototypeCharacterId.GojoModern);
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
            if (activeIndex < 0 || activeIndex >= members.Length)
            {
                return;
            }

            TeamMemberState active = members[activeIndex];
            active.Initialized = true;
            active.Health = health != null ? health.CurrentHealth : active.Health;
            active.Energy = cursedEnergy != null ? cursedEnergy.CurrentEnergy : active.Energy;
        }

        private static int ResolveIndex(PrototypeCharacterId characterId)
        {
            return characterId == PrototypeCharacterId.SukunaShibuyaYujiBody ? 1 : 0;
        }

        private void OnGUI()
        {
            if (health == null || characterController == null)
            {
                return;
            }

            EnsureStyles();
            DrawActivePlayerOverlay();
            DrawTeamPanel();
        }

        private void DrawActivePlayerOverlay()
        {
            const float margin = 12f;
            float panelWidth = Mathf.Clamp((Screen.width - margin * 3f) * 0.36f, 230f, 340f);
            Rect rect = new Rect(margin, margin, panelWidth, 62f);
            bool sukuna = ActiveCharacter == PrototypeCharacterId.SukunaShibuyaYujiBody;
            Color accent = sukuna
                ? new Color(0.96f, 0.20f, 0.12f)
                : new Color(0.18f, 0.66f, 1f);

            DrawRect(rect, sukuna
                ? new Color(0.040f, 0.010f, 0.012f, 0.995f)
                : new Color(0.012f, 0.018f, 0.032f, 0.995f));
            DrawBorder(rect, accent, 2f);
            GUI.Label(
                new Rect(rect.x + 10f, rect.y + 3f, rect.width - 20f, 18f),
                $"ACTIVE · {CharacterName(ActiveCharacter)}",
                titleStyle
            );

            DrawValueBar(
                new Rect(rect.x + 10f, rect.y + 23f, rect.width - 20f, 18f),
                health.CurrentHealth,
                health.MaxHealth,
                accent,
                $"HP {health.CurrentHealth:0}/{health.MaxHealth:0}"
            );

            cursedEnergy ??= CursedEnergyController.GetOrCreate(gameObject);
            if (cursedEnergy != null)
            {
                DrawValueBar(
                    new Rect(rect.x + 10f, rect.y + 44f, rect.width - 20f, 12f),
                    cursedEnergy.CurrentEnergy,
                    cursedEnergy.MaxEnergy,
                    sukuna ? new Color(0.72f, 0.12f, 0.20f) : new Color(0.34f, 0.20f, 0.96f),
                    $"CE {cursedEnergy.CurrentEnergy:0}/{cursedEnergy.MaxEnergy:0}"
                );
            }
        }

        private void DrawTeamPanel()
        {
            float width = Mathf.Min(340f, Screen.width - 24f);
            Rect panel = new Rect(12f, Screen.height - 118f, width, 92f);
            DrawRect(panel, new Color(0.012f, 0.016f, 0.025f, 0.96f));
            DrawBorder(panel, new Color(0.58f, 0.68f, 0.88f, 0.90f), 2f);

            string tagStatus = BuildTagStatus();
            GUI.Label(
                new Rect(panel.x + 8f, panel.y + 3f, panel.width - 16f, 20f),
                $"TEAM · {CombatInputBindings.TagLabel} TAG · {tagStatus}",
                titleStyle
            );

            DrawMemberRow(
                new Rect(panel.x + 8f, panel.y + 26f, panel.width - 16f, 27f),
                activeIndex,
                true
            );
            DrawMemberRow(
                new Rect(panel.x + 8f, panel.y + 57f, panel.width - 16f, 27f),
                1 - activeIndex,
                false
            );
        }

        private void DrawMemberRow(Rect rect, int index, bool active)
        {
            TeamMemberState member = members[index];
            bool sukuna = member.CharacterId == PrototypeCharacterId.SukunaShibuyaYujiBody;
            Color accent = member.KnockedOut
                ? new Color(0.40f, 0.40f, 0.44f)
                : sukuna
                    ? new Color(0.96f, 0.24f, 0.14f)
                    : new Color(0.20f, 0.70f, 1f);
            DrawRect(rect, new Color(0.032f, 0.036f, 0.050f, 0.98f));
            DrawBorder(rect, accent, active ? 2f : 1f);

            string role = active ? "ACTIVE" : "RESERVE";
            string hpText = member.Initialized
                ? $"HP {member.Health:0}"
                : "HP READY";
            string energyText = member.Initialized
                ? $"CE {member.Energy:0}"
                : "CE START";
            string down = member.KnockedOut ? " · KO" : string.Empty;
            rowStyle.normal.textColor = member.KnockedOut ? new Color(0.60f, 0.60f, 0.64f) : Color.white;
            GUI.Label(
                rect,
                $"{role} · {CharacterShortName(member.CharacterId)} · {hpText} · {energyText}{down}",
                rowStyle
            );
        }

        private string BuildTagStatus()
        {
            TeamMemberState reserve = members[1 - activeIndex];
            if (reserve.KnockedOut)
            {
                return "RESERVE KO";
            }
            if (ManualTagCooldownRemaining > 0f)
            {
                return $"{ManualTagCooldownRemaining:0.0}s";
            }

            actionGate ??= CombatActionGate.GetOrCreate(gameObject);
            if (actionGate != null && actionGate.CurrentState != CombatActionState.Normal)
            {
                return "ACTION LOCK";
            }
            return "READY";
        }

        private static string CharacterName(PrototypeCharacterId characterId)
        {
            return characterId == PrototypeCharacterId.SukunaShibuyaYujiBody
                ? "RYOMEN SUKUNA · 시부야 사변"
                : "GOJO SATORU · 현대 · 교사";
        }

        private static string CharacterShortName(PrototypeCharacterId characterId)
        {
            return characterId == PrototypeCharacterId.SukunaShibuyaYujiBody
                ? "스쿠나"
                : "고죠";
        }

        private void DrawValueBar(Rect rect, float value, float max, Color fill, string text)
        {
            DrawRect(rect, new Color(0.075f, 0.082f, 0.115f));
            float ratio = max > 0f ? Mathf.Clamp01(value / max) : 0f;
            DrawRect(
                new Rect(rect.x + 1f, rect.y + 1f, (rect.width - 2f) * ratio, rect.height - 2f),
                fill
            );
            DrawBorder(rect, new Color(1f, 1f, 1f, 0.16f), 1f);
            GUI.Label(rect, text, chipStyle);
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
