using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    [RequireComponent(typeof(Health))]
    public sealed class GojoTechniqueController : MonoBehaviour
    {
        [Header("Cursed Technique Lapse: Blue")]
        [SerializeField, Min(0.1f)] private float blueRadius = 8f;
        [SerializeField, Min(0f)] private float blueDamage = 8f;
        [SerializeField, Min(0f)] private float bluePullSpeed = 13f;
        [SerializeField, Min(0f)] private float blueHitStun = 0.42f;
        [SerializeField, Min(0.1f)] private float blueCooldown = 3.2f;
        [SerializeField, Min(0.1f)] private float visualDuration = 0.65f;

        private readonly List<LineRenderer> visualRings = new List<LineRenderer>();
        private Health ownHealth;
        private GojoDomainController domainController;
        private GameObject visualRoot;
        private Light blueLight;
        private float nextBlueAt;
        private float visualStartedAt;

        public bool BlueReady => Time.time >= nextBlueAt;
        public float BlueCooldownRemaining => Mathf.Max(0f, nextBlueAt - Time.time);
        public float BlueCooldownProgress => blueCooldown <= 0f
            ? 1f
            : Mathf.Clamp01(1f - BlueCooldownRemaining / blueCooldown);
        public string BlueStatusText => BlueReady
            ? "Q · 술식순전 「창」  READY"
            : $"Q · 술식순전 「창」  {BlueCooldownRemaining:0.0}s";

        private void Awake()
        {
            ownHealth = GetComponent<Health>();
            domainController = GetComponent<GojoDomainController>();
            BuildRuntimeVisual();
            visualRoot.SetActive(false);
        }

        private void Update()
        {
            UpdateVisual();

            if (!Input.GetKeyDown(KeyCode.Q) || !CanUseBlue())
            {
                return;
            }

            ActivateBlue();
        }

        private bool CanUseBlue()
        {
            if (!BlueReady || ownHealth == null || ownHealth.IsDead)
            {
                return false;
            }

            return domainController == null
                || domainController.State == GojoDomainController.DomainState.Normal;
        }

        private void ActivateBlue()
        {
            nextBlueAt = Time.time + blueCooldown;
            ShowVisual();

            Collider[] hits = Physics.OverlapSphere(transform.position, blueRadius);
            HashSet<Health> affectedTargets = new HashSet<Health>();

            foreach (Collider hit in hits)
            {
                Health targetHealth = hit.GetComponentInParent<Health>();
                if (
                    targetHealth == null
                    || targetHealth == ownHealth
                    || targetHealth.IsDead
                    || !affectedTargets.Add(targetHealth)
                )
                {
                    continue;
                }

                targetHealth.TakeDamage(blueDamage);
                ApplyPull(targetHealth);
            }
        }

        private void ApplyPull(Health targetHealth)
        {
            Vector3 direction = transform.position - targetHealth.transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = -targetHealth.transform.forward;
            }

            Vector3 impulse = direction.normalized * bluePullSpeed;
            MonoBehaviour[] behaviours = targetHealth.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IHitReactable hitReactable)
                {
                    hitReactable.ApplyHitReaction(impulse, blueHitStun);
                    break;
                }
            }
        }

        private void BuildRuntimeVisual()
        {
            visualRoot = new GameObject("BluePrototypeVisual");
            visualRoot.transform.SetParent(transform, false);
            visualRoot.transform.localPosition = Vector3.up * 0.15f;

            CreateRing(
                "BlueOuterRing",
                blueRadius,
                0.13f,
                new Color(0.10f, 0.72f, 1f, 0.95f),
                Quaternion.identity,
                RingPlane.XZ
            );
            CreateRing(
                "BlueInnerRing",
                blueRadius * 0.42f,
                0.10f,
                new Color(0.32f, 0.92f, 1f, 0.90f),
                Quaternion.Euler(72f, 0f, 18f),
                RingPlane.XY
            );

            GameObject lightObject = new GameObject("BlueLight");
            lightObject.transform.SetParent(visualRoot.transform, false);
            lightObject.transform.localPosition = Vector3.up * 1.4f;
            blueLight = lightObject.AddComponent<Light>();
            blueLight.type = LightType.Point;
            blueLight.color = new Color(0.12f, 0.55f, 1f);
            blueLight.range = blueRadius * 1.5f;
            blueLight.intensity = 3.4f;
            blueLight.shadows = LightShadows.None;
        }

        private void CreateRing(
            string objectName,
            float radius,
            float width,
            Color color,
            Quaternion localRotation,
            RingPlane plane
        )
        {
            GameObject ringObject = new GameObject(objectName);
            ringObject.transform.SetParent(visualRoot.transform, false);
            ringObject.transform.localRotation = localRotation;

            LineRenderer line = ringObject.AddComponent<LineRenderer>();
            line.loop = true;
            line.useWorldSpace = false;
            line.positionCount = 96;
            line.startWidth = width;
            line.endWidth = width;
            line.startColor = color;
            line.endColor = color;
            line.numCornerVertices = 4;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader != null)
            {
                line.material = new Material(shader)
                {
                    color = color,
                };
            }

            for (int index = 0; index < line.positionCount; index++)
            {
                float angle = (float)index / line.positionCount * Mathf.PI * 2f;
                float cosine = Mathf.Cos(angle) * radius;
                float sine = Mathf.Sin(angle) * radius;
                Vector3 point = plane switch
                {
                    RingPlane.XY => new Vector3(cosine, sine, 0f),
                    _ => new Vector3(cosine, 0f, sine),
                };
                line.SetPosition(index, point);
            }

            visualRings.Add(line);
        }

        private void ShowVisual()
        {
            visualStartedAt = Time.time;
            visualRoot.transform.localScale = Vector3.one * 0.08f;
            visualRoot.SetActive(true);
        }

        private void UpdateVisual()
        {
            if (visualRoot == null || !visualRoot.activeSelf)
            {
                return;
            }

            float elapsed = Time.time - visualStartedAt;
            float normalized = Mathf.Clamp01(elapsed / visualDuration);
            float expansion = 1f - Mathf.Pow(1f - normalized, 3f);
            float pulse = 1f + Mathf.Sin(elapsed * 18f) * 0.045f;
            visualRoot.transform.localScale = Vector3.one * Mathf.Max(0.08f, expansion * pulse);
            visualRoot.transform.Rotate(Vector3.up, 90f * Time.deltaTime, Space.Self);

            float alpha = 1f - normalized;
            foreach (LineRenderer ring in visualRings)
            {
                Color start = ring.startColor;
                start.a = alpha;
                ring.startColor = start;
                ring.endColor = start;
            }

            if (blueLight != null)
            {
                blueLight.intensity = Mathf.Lerp(3.4f, 0f, normalized);
            }

            if (elapsed >= visualDuration)
            {
                visualRoot.SetActive(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, blueRadius);
        }

        private enum RingPlane
        {
            XZ,
            XY,
        }
    }
}
