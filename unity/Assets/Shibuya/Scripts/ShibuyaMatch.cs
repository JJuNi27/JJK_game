using JJKGame.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.Shibuya
{
    public sealed class ShibuyaMatch : MonoBehaviour
    {
        [SerializeField] private ShibuyaFighter player;
        [SerializeField] private ShibuyaFighter dummy;
        private Font font;
        public ShibuyaFighter Player => player;
        public ShibuyaFighter Dummy => dummy;
        // Deliberately owner-specific: never enumerate opponents' resource controllers for HUD.
        public ShibuyaEnergyCharge PlayerCharge => player != null ? player.GetComponent<ShibuyaEnergyCharge>() : null;
        public CursedEnergyController PlayerEnergy => PlayerCharge != null ? PlayerCharge.Resource : null;
        public bool Finished { get; private set; }
        public bool Victory { get; private set; }
        public void Configure(ShibuyaFighter playerActor, ShibuyaFighter dummyActor) { player = playerActor; dummy = dummyActor; }
        private void Start()
        {
            ShibuyaBlackFlashPresentation.Attach(this);
            player.Health.Died += OnDeath;
            dummy.Health.Died += OnDeath;
            font = Font.CreateDynamicFontFromOSFont("Malgun Gothic", 18);
        }
        private void OnDeath(Health dead)
        {
            if (Finished) return;
            Finished = true;
            Victory = dead == dummy.Health;
            player.StopCombat();
            dummy.StopCombat();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        public void RestartBattle()
        {
            if (!Finished) return;
            string scenePath = gameObject.scene.path;
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(scenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
#else
            SceneManager.LoadScene(scenePath);
#endif
        }
#if UNITY_EDITOR
        [ContextMenu("검증/플레이어 패배 (Play Mode 전용)")]
        public void EditorDefeatPlayer()
        {
            if (!Application.isPlaying || Finished) return;
            player.Health.ReceiveDamage(new DamageContext(player.Health.CurrentHealth, gameObject,
                DamageDeliveryType.Environmental, DamageTraits.Unblockable, "EDITOR DEFEAT TEST"));
        }
#endif
        private void OnDestroy()
        {
            if (player != null) player.Health.Died -= OnDeath;
            if (dummy != null) dummy.Health.Died -= OnDeath;
            if (font != null) Destroy(font);
        }
        private void OnGUI()
        {
            if (player == null || dummy == null) return;
            float scale = Mathf.Clamp(Screen.height / 900f, 0.75f, 1.5f);
            GUIStyle label = new GUIStyle(GUI.skin.label) { font = font, fontSize = Mathf.RoundToInt(18 * scale) };
            DrawHealth(new Rect(24, Screen.height - 80 * scale, 280 * scale, 28 * scale), player.Health, "플레이어", new Color(0.15f, 0.8f, 0.28f), label);
            DrawEnergy(new Rect(24, Screen.height - 46 * scale, 280 * scale, 26 * scale), label);
            DrawHealth(new Rect(24, 24, 240 * scale, 24 * scale), dummy.Health, "훈련 더미", new Color(0.9f, 0.36f, 0.2f), label);
            GUI.Label(new Rect(24, 58 * scale, Screen.width - 48, 70 * scale),
                "WASD 이동 · Shift 달리기 · Space 점프 · 마우스 시점 · 좌클릭 타격\nF 홀드 주력 충전 · ESC 마우스 해제 · 화면 클릭으로 조작 재개", label);
            if (!Finished) return;
            Rect panel = new Rect(Screen.width * 0.5f - 200, Screen.height * 0.5f - 90, 400, 180);
            GUI.Box(panel, GUIContent.none);
            GUIStyle title = new GUIStyle(label) { fontSize = 30, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(panel.x, panel.y + 20, panel.width, 55), Victory ? "승리" : "패배", title);
            GUIStyle button = new GUIStyle(GUI.skin.button) { font = font, fontSize = 20 };
            if (GUI.Button(new Rect(panel.x + 90, panel.y + 100, 220, 48), "새 전투 재시작", button)) RestartBattle();
        }
        private static void DrawHealth(Rect rect, Health health, string title, Color fill, GUIStyle label)
        {
            Color old = GUI.color;
            GUI.color = new Color(0.06f, 0.07f, 0.09f, 0.9f); GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = fill;
            GUI.DrawTexture(new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * health.CurrentHealth / health.MaxHealth, rect.height - 4), Texture2D.whiteTexture);
            GUI.color = old;
            GUI.Label(rect, $" {title}  {health.CurrentHealth:0} / {health.MaxHealth:0}", label);
        }
        private void DrawEnergy(Rect rect, GUIStyle label)
        {
            var energy = PlayerEnergy;
            if (energy == null) return;
            Color old = GUI.color;
            GUI.color = new Color(.06f, .07f, .09f, .9f); GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = new Color(.12f, .42f, .95f);
            GUI.DrawTexture(new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * energy.Normalized, rect.height - 4), Texture2D.whiteTexture);
            GUI.color = old;
            string status = PlayerCharge.IsCharging ? energy.Normalized >= 1 ? " · 최대 유지" : " · 충전 중" : "";
            GUI.Label(rect, $" 주력 {energy.CurrentEnergy:0} / {energy.MaxEnergy:0}{status}", label);
        }
    }
}
