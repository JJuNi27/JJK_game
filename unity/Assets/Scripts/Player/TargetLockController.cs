using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [RequireComponent(typeof(Health))]
    public sealed class TargetLockController : MonoBehaviour
    {
        [Header("Target Lock")]
        [SerializeField, Min(1f)] private float maxLockDistance = 30f;
        [SerializeField, Min(0.1f)] private float indicatorRadius = 0.95f;
        [SerializeField, Min(0f)] private float indicatorHeight = 0.08f;
        [SerializeField, Min(0f)] private float indicatorRotationSpeed = 110f;

        private readonly List<Health> candidates = new List<Health>();

        private Health ownHealth;
        private Health currentTarget;
        private GameObject indicatorRoot;
        private LineRenderer indicatorRing;
        private GUIStyle lockStyle;
        private int styledForHeight = -1;

        public Health CurrentTarget => IsValidTarget(currentTarget) ? currentTarget : null;
        public bool HasTarget => CurrentTarget != null;

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
            BuildIndicator();
            ClearTarget();
        }

        private void Update()
        {
            if (ownHealth == null || ownHealth.IsDead)
            {
                ClearTarget();
                return;
            }

            if (currentTarget != null && !IsValidTarget(currentTarget))
            {
                ClearTarget();
            }

            if (Input.GetKeyDown(CombatInputBindings.TargetLock))
            {
                SelectNextTargetOrUnlock();
            }

            UpdateIndicator();
        }

        private void OnDisable()
        {
            if (indicatorRoot != null)
            {
                indicatorRoot.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (indicatorRoot != null)
            {
                Destroy(indicatorRoot);
            }
        }

        public bool TryGetAimDirection(out Vector3 direction)
        {
            Health target = CurrentTarget;
            if (target == null)
            {
                direction = transform.forward;
                return false;
            }

            direction = target.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = transform.forward;
                return false;
            }

            direction.Normalize();
            return true;
        }

        public void FaceTargetInstant()
        {
            if (TryGetAimDirection(out Vector3 direction))
            {
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }

        private void SelectNextTargetOrUnlock()
        {
            RefreshCandidates();
            if (candidates.Count == 0)
            {
                ClearTarget();
                return;
            }

            if (currentTarget == null)
            {
                SetTarget(candidates[0]);
                return;
            }

            int currentIndex = candidates.IndexOf(currentTarget);
            if (currentIndex >= 0 && currentIndex < candidates.Count - 1)
            {
                SetTarget(candidates[currentIndex + 1]);
            }
            else
            {
                ClearTarget();
            }
        }

        private void RefreshCandidates()
        {
            candidates.Clear();
            Health[] healthObjects = FindObjectsByType<Health>(FindObjectsSortMode.None);

            foreach (Health health in healthObjects)
            {
                if (IsValidTarget(health))
                {
                    candidates.Add(health);
                }
            }

            candidates.Sort((left, right) =>
            {
                float leftDistance = HorizontalDistanceSqr(left.transform.position);
                float rightDistance = HorizontalDistanceSqr(right.transform.position);
                return leftDistance.CompareTo(rightDistance);
            });
        }

        private bool IsValidTarget(Health target)
        {
            if (target == null || target == ownHealth || target.IsDead)
            {
                return false;
            }

            return HorizontalDistanceSqr(target.transform.position)
                <= maxLockDistance * maxLockDistance;
        }

        private float HorizontalDistanceSqr(Vector3 position)
        {
            Vector3 offset = position - transform.position;
            offset.y = 0f;
            return offset.sqrMagnitude;
        }

        private void SetTarget(Health target)
        {
            currentTarget = IsValidTarget(target) ? target : null;
            UpdateIndicator();
        }

        private void ClearTarget()
        {
            currentTarget = null;
            if (indicatorRoot != null)
            {
                indicatorRoot.SetActive(false);
            }
        }

        private void BuildIndicator()
        {
            indicatorRoot = new GameObject("TargetLockPrototypeIndicator");
            indicatorRing = indicatorRoot.AddComponent<LineRenderer>();
            indicatorRing.loop = true;
            indicatorRing.useWorldSpace = false;
            indicatorRing.positionCount = 64;
            indicatorRing.startWidth = 0.09f;
            indicatorRing.endWidth = 0.09f;
            indicatorRing.startColor = new Color(1f, 0.78f, 0.16f, 0.98f);
            indicatorRing.endColor = indicatorRing.startColor;
            indicatorRing.numCornerVertices = 4;
            indicatorRing.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            indicatorRing.receiveShadows = false;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader != null)
            {
                indicatorRing.material = new Material(shader)
                {
                    color = indicatorRing.startColor,
                };
            }

            for (int index = 0; index < indicatorRing.positionCount; index++)
            {
                float angle = (float)index / indicatorRing.positionCount * Mathf.PI * 2f;
                indicatorRing.SetPosition(
                    index,
                    new Vector3(
                        Mathf.Cos(angle) * indicatorRadius,
                        0f,
                        Mathf.Sin(angle) * indicatorRadius
                    )
                );
            }

            indicatorRoot.SetActive(false);
        }

        private void UpdateIndicator()
        {
            Health target = CurrentTarget;
            if (indicatorRoot == null || target == null)
            {
                if (indicatorRoot != null)
                {
                    indicatorRoot.SetActive(false);
                }
                return;
            }

            indicatorRoot.transform.position =
                target.transform.position + Vector3.up * indicatorHeight;
            indicatorRoot.transform.Rotate(
                Vector3.up,
                indicatorRotationSpeed * Time.deltaTime,
                Space.World
            );
            indicatorRoot.SetActive(true);
        }

        private void OnGUI()
        {
            Health target = CurrentTarget;
            if (target == null || ownHealth == null || ownHealth.IsDead)
            {
                return;
            }

            EnsureStyle();
            float width = Mathf.Min(360f, Screen.width - 48f);
            Rect panel = new Rect(Screen.width - width - 24f, 115f, width, 34f);
            DrawRect(panel, new Color(0.055f, 0.040f, 0.010f, 0.92f));
            DrawBorder(panel, new Color(1f, 0.76f, 0.16f, 0.98f), 2f);
            GUI.Label(
                panel,
                $"TARGET LOCK · {target.gameObject.name} · {CombatInputBindings.TargetLockLabel} 전환/해제",
                lockStyle
            );
        }

        private void EnsureStyle()
        {
            if (styledForHeight == Screen.height)
            {
                return;
            }

            styledForHeight = Screen.height;
            lockStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 52f, 13f, 19f)),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            lockStyle.normal.textColor = new Color(1f, 0.88f, 0.44f);
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
