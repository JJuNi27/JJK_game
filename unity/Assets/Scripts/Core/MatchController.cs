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
        }

        private void OnGUI()
        {
            if (playerHealth == null || enemyHealth == null)
            {
                return;
            }

            const float panelWidth = 360f;
            GUI.Box(new Rect(16f, 16f, panelWidth, 118f), "JJK Combat MVP");
            GUI.Label(
                new Rect(32f, 46f, panelWidth - 32f, 22f),
                $"Player HP: {playerHealth.CurrentHealth:0} / {playerHealth.MaxHealth:0}"
            );
            GUI.Label(
                new Rect(32f, 70f, panelWidth - 32f, 22f),
                $"Curse HP: {enemyHealth.CurrentHealth:0} / {enemyHealth.MaxHealth:0}"
            );

            string domainState = gojoDomain != null ? gojoDomain.State.ToString() : "Not Connected";
            GUI.Label(
                new Rect(32f, 94f, panelWidth - 32f, 22f),
                $"Domain: {domainState}"
            );

            if (!matchFinished)
            {
                return;
            }

            GUI.Box(
                new Rect(Screen.width / 2f - 170f, Screen.height / 2f - 55f, 340f, 110f),
                resultText
            );
            GUI.Label(
                new Rect(Screen.width / 2f - 125f, Screen.height / 2f, 250f, 24f),
                "Press ENTER to restart"
            );
        }
    }
}
