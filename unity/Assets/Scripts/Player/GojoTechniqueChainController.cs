using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.Player
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(GojoTechniqueController))]
    public sealed class GojoTechniqueChainController : MonoBehaviour
    {
        [Header("Blue To Red Chain")]
        [SerializeField, Min(0.1f)] private float blueMarkRadius = 8f;
        [SerializeField, Min(0.1f)] private float redConfirmRadius = 7f;
        [SerializeField, Min(0.1f)] private float chainWindow = 2.2f;
        [SerializeField, Min(0f)] private float bonusDamage = 12f;
        [SerializeField, Min(0f)] private float empoweredPushSpeed = 28f;
        [SerializeField, Min(0f)] private float empoweredHitStun = 0.72f;
        [SerializeField, Min(0.1f)] private float noticeDuration = 1.15f;

        private readonly Dictionary<Health, float> blueMarkedUntil =
            new Dictionary<Health, float>();
        private readonly List<Health> expiredMarks = new List<Health>();

        private Health ownHealth;
        private GojoTechniqueController techniqueController;
        private bool blueWasReady;
        private bool redWasReady;
        private float chainNoticeUntil;
        private GUIStyle primedStyle;
        private GUIStyle chainStyle;
        private int styledForHeight = -1;

        public bool HasPrimedTarget => FindPrimedTarget(false) != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneBootstrap()
        {
            SceneManager.sceneLoaded -= AttachToGojoPlayers;
            SceneManager.sceneLoaded += AttachToGojoPlayers;
        }

        private static void AttachToGojoPlayers(Scene _, LoadSceneMode __)
        {
            GojoTechniqueController[] techniques =
                Object.FindObjectsByType<GojoTechniqueController>(FindObjectsSortMode.None);

            foreach (GojoTechniqueController technique in techniques)
            {
                if (technique.GetComponent<GojoTechniqueChainController>() == null)
                {
                    technique.gameObject.AddComponent<GojoTechniqueChainController>();
                }
            }
        }

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
            techniqueController = GetComponent<GojoTechniqueController>();
            blueWasReady = techniqueController != null && techniqueController.BlueReady;
            redWasReady = techniqueController != null && techniqueController.RedReady;
        }

        private void LateUpdate()
        {
            if (techniqueController == null || ownHealth == null || ownHealth.IsDead)
            {
                return;
            }

            RemoveExpiredMarks();

            bool blueReadyNow = techniqueController.BlueReady;
            bool redReadyNow = techniqueController.RedReady;

            if (blueWasReady && !blueReadyNow)
            {
                MarkTargetsHitByBlue();
            }

            if (redWasReady && !redReadyNow)
            {
                TryConfirmBlueToRedChain();
            }

            blueWasReady = blueReadyNow;
            redWasReady = redReadyNow;
        }

        private void MarkTargetsHitByBlue()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, blueMarkRadius);
            HashSet<Health> markedThisCast = new HashSet<Health>();

            foreach (Collider hit in hits)
            {
                Health target = hit.GetComponentInParent<Health>();
                if (
                    target == null
                    || target == ownHealth
                    || target.IsDead
                    || !markedThisCast.Add(target)
                )
                {
                    continue;
                }

                blueMarkedUntil[target] = Time.time + chainWindow;
            }
        }

        private void TryConfirmBlueToRedChain()
        {
            Health target = FindPrimedTarget(true);
            if (target == null)
            {
                return;
            }

            blueMarkedUntil.Remove(target);

            bool bonusApplied = target.TakeDamage(bonusDamage);
            ApplyEmpoweredPush(target);

            if (bonusApplied || target.IsDead)
            {
                chainNoticeUntil = Time.time + noticeDuration;
            }
        }

        private Health FindPrimedTarget(bool requireRedRange)
        {
            Health bestTarget = null;
            float bestDistanceSqr = float.MaxValue;
            float redRangeSqr = redConfirmRadius * redConfirmRadius;

            foreach (KeyValuePair<Health, float> mark in blueMarkedUntil)
            {
                Health target = mark.Key;
                if (target == null || target.IsDead || Time.time > mark.Value)
                {
                    continue;
                }

                Vector3 offset = target.transform.position - transform.position;
                offset.y = 0f;
                float distanceSqr = offset.sqrMagnitude;
                if (requireRedRange && distanceSqr > redRangeSqr)
                {
                    continue;
                }

                if (distanceSqr < bestDistanceSqr)
                {
                    bestDistanceSqr = distanceSqr;
                    bestTarget = target;
                }
            }

            return bestTarget;
        }

        private void ApplyEmpoweredPush(Health target)
        {
            Vector3 direction = target.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = target.transform.forward;
            }

            Vector3 impulse = direction.normalized * empoweredPushSpeed;
            MonoBehaviour[] behaviours = target.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IHitReactable hitReactable)
                {
                    hitReactable.ApplyHitReaction(impulse, empoweredHitStun);
                    break;
                }
            }
        }

        private void RemoveExpiredMarks()
        {
            expiredMarks.Clear();

            foreach (KeyValuePair<Health, float> mark in blueMarkedUntil)
            {
                if (mark.Key == null || mark.Key.IsDead || Time.time > mark.Value)
                {
                    expiredMarks.Add(mark.Key);
                }
            }

            foreach (Health target in expiredMarks)
            {
                blueMarkedUntil.Remove(target);
            }
        }

        private void OnGUI()
        {
            if (ownHealth == null || ownHealth.IsDead)
            {
                return;
            }

            EnsureStyles();

            if (Time.time <= chainNoticeUntil)
            {
                float width = Mathf.Min(500f, Screen.width - 48f);
                Rect noticeRect = new Rect(
                    (Screen.width - width) * 0.5f,
                    Screen.height * 0.22f,
                    width,
                    68f
                );
                DrawRect(noticeRect, new Color(0.16f, 0.015f, 0.22f, 0.94f));
                DrawBorder(noticeRect, new Color(0.82f, 0.28f, 1f, 0.98f), 3f);
                GUI.Label(noticeRect, "BLUE → RED CHAIN  ·  BONUS +12", chainStyle);
                return;
            }

            if (!HasPrimedTarget)
            {
                return;
            }

            float panelWidth = Mathf.Min(350f, Screen.width - 48f);
            Rect primedRect = new Rect(24f, 239f, panelWidth, 34f);
            DrawRect(primedRect, new Color(0.09f, 0.025f, 0.13f, 0.92f));
            DrawBorder(primedRect, new Color(0.72f, 0.30f, 1f, 0.96f), 2f);
            GUI.Label(primedRect, "BLUE MARK · E로 혁 연계", primedStyle);
        }

        private void EnsureStyles()
        {
            if (styledForHeight == Screen.height)
            {
                return;
            }

            styledForHeight = Screen.height;
            int baseSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 48f, 14f, 20f));

            primedStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = baseSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            primedStyle.normal.textColor = new Color(0.90f, 0.78f, 1f);

            chainStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.Max(22, baseSize + 8),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            chainStyle.normal.textColor = new Color(0.94f, 0.82f, 1f);
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
            DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }
    }
}
