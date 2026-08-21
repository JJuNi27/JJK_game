using System;
using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    public sealed class SukunaFugaProjectile : MonoBehaviour
    {
        public static event Action<Vector3, bool> Exploded;

        private readonly List<Material> runtimeMaterials = new List<Material>();
        private readonly List<LineRenderer> flightRings = new List<LineRenderer>();
        private readonly List<LineRenderer> explosionRings = new List<LineRenderer>();

        private Health owner;
        private Health target;
        private Vector3 direction;
        private float speed;
        private float maxRange;
        private float collisionRadius;
        private float explosionRadius;
        private float damage;
        private float knockbackSpeed;
        private float hitStun;
        private float travelled;
        private float startedAt;
        private float explodedAt;
        private bool exploded;
        private Action onExploded;

        private Transform visualRoot;
        private Transform core;
        private TrailRenderer trail;
        private Light projectileLight;

        public void Configure(
            Health newOwner,
            Health newTarget,
            Vector3 initialDirection,
            float newSpeed,
            float newMaxRange,
            float newCollisionRadius,
            float newExplosionRadius,
            float newDamage,
            float newKnockbackSpeed,
            float newHitStun,
            Action newOnExploded
        )
        {
            owner = newOwner;
            target = newTarget;
            direction = initialDirection;
            direction.y = 0f;
            direction = direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : Vector3.forward;
            speed = Mathf.Max(0.1f, newSpeed);
            maxRange = Mathf.Max(0.1f, newMaxRange);
            collisionRadius = Mathf.Max(0.1f, newCollisionRadius);
            explosionRadius = Mathf.Max(0.1f, newExplosionRadius);
            damage = Mathf.Max(0f, newDamage);
            knockbackSpeed = Mathf.Max(0f, newKnockbackSpeed);
            hitStun = Mathf.Max(0f, newHitStun);
            onExploded = newOnExploded;
            startedAt = Time.time;

            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            BuildProjectileVisual();
        }

        private void Update()
        {
            if (exploded)
            {
                UpdateExplosionVisual();
                return;
            }

            UpdateDirectionTowardTarget();

            float step = speed * Time.deltaTime;
            Vector3 previous = transform.position;
            Vector3 next = previous + direction * step;
            transform.position = next;
            travelled += step;

            UpdateFlightVisual();

            if (HasReachedTarget(previous, next) || travelled >= maxRange)
            {
                Explode();
            }
        }

        private void OnDestroy()
        {
            foreach (Material material in runtimeMaterials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
        }

        private void UpdateDirectionTowardTarget()
        {
            if (target == null || target.IsDead)
            {
                return;
            }

            Vector3 desired = target.transform.position + Vector3.up * 0.75f - transform.position;
            if (desired.sqrMagnitude <= 0.001f)
            {
                return;
            }

            float maxRadians = 210f * Mathf.Deg2Rad * Time.deltaTime;
            direction = Vector3.RotateTowards(direction, desired.normalized, maxRadians, 0f).normalized;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        private bool HasReachedTarget(Vector3 segmentStart, Vector3 segmentEnd)
        {
            if (target == null || target.IsDead)
            {
                return false;
            }

            Vector3 targetPoint = target.transform.position + Vector3.up * 0.75f;
            Vector3 segment = segmentEnd - segmentStart;
            float denominator = segment.sqrMagnitude;
            float t = denominator > 0.0001f
                ? Mathf.Clamp01(Vector3.Dot(targetPoint - segmentStart, segment) / denominator)
                : 0f;
            Vector3 closest = segmentStart + segment * t;
            return Vector3.Distance(closest, targetPoint) <= collisionRadius + 0.55f;
        }

        private void Explode()
        {
            if (exploded)
            {
                return;
            }

            exploded = true;
            explodedAt = Time.time;
            onExploded?.Invoke();

            if (trail != null)
            {
                trail.emitting = false;
            }

            foreach (LineRenderer ring in flightRings)
            {
                if (ring != null)
                {
                    ring.gameObject.SetActive(false);
                }
            }

            ApplySingleTargetDamage();
            BuildExplosionRings();

            bool domainAmplified = gameObject.name.Contains("Domain");
            Exploded?.Invoke(transform.position, domainAmplified);
        }

        private void ApplySingleTargetDamage()
        {
            if (target == null || target.IsDead)
            {
                return;
            }

            Vector3 targetPoint = target.transform.position + Vector3.up * 0.75f;
            if (Vector3.Distance(transform.position, targetPoint) > explosionRadius + 1.2f)
            {
                return;
            }

            DamageContext context = new DamageContext(
                damage,
                owner != null ? owner.gameObject : gameObject,
                DamageDeliveryType.CursedTechnique,
                DamageTraits.None,
                "푸가",
                targetPoint
            );
            if (target.ReceiveDamage(context) != DamageResolution.Applied)
            {
                return;
            }

            Vector3 pushDirection = target.transform.position - transform.position;
            pushDirection.y = 0f;
            if (pushDirection.sqrMagnitude <= 0.001f)
            {
                pushDirection = direction;
            }
            ApplyHitReaction(target, pushDirection.normalized * knockbackSpeed, hitStun);
        }

        private static void ApplyHitReaction(Health targetHealth, Vector3 impulse, float stun)
        {
            MonoBehaviour[] behaviours = targetHealth.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IHitReactable hitReactable)
                {
                    hitReactable.ApplyHitReaction(impulse, stun);
                    break;
                }
            }
        }

        private void BuildProjectileVisual()
        {
            GameObject rootObject = new GameObject("FugaVisual");
            rootObject.transform.SetParent(transform, false);
            visualRoot = rootObject.transform;

            Material coreMaterial = CreateMaterial(new Color(1f, 0.22f, 0.02f, 1f), true);
            GameObject coreObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            coreObject.name = "FugaCore";
            coreObject.transform.SetParent(visualRoot, false);
            coreObject.transform.localScale = Vector3.one * collisionRadius * 1.25f;
            core = coreObject.transform;
            Collider coreCollider = coreObject.GetComponent<Collider>();
            if (coreCollider != null)
            {
                Destroy(coreCollider);
            }
            Renderer coreRenderer = coreObject.GetComponent<Renderer>();
            if (coreRenderer != null)
            {
                coreRenderer.material = coreMaterial;
            }

            CreateFlightRing("FugaRingA", collisionRadius * 1.35f, 0.10f, Quaternion.identity);
            CreateFlightRing(
                "FugaRingB",
                collisionRadius * 0.90f,
                0.075f,
                Quaternion.Euler(65f, 0f, 25f)
            );

            trail = coreObject.AddComponent<TrailRenderer>();
            trail.time = 0.26f;
            trail.minVertexDistance = 0.04f;
            trail.startWidth = collisionRadius * 0.95f;
            trail.endWidth = 0.02f;
            trail.startColor = new Color(1f, 0.28f, 0.02f, 0.95f);
            trail.endColor = new Color(0.45f, 0.01f, 0.01f, 0f);
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;
            trail.material = CreateMaterial(new Color(1f, 0.15f, 0.01f, 0.90f), true);

            GameObject lightObject = new GameObject("FugaLight");
            lightObject.transform.SetParent(visualRoot, false);
            projectileLight = lightObject.AddComponent<Light>();
            projectileLight.type = LightType.Point;
            projectileLight.color = new Color(1f, 0.16f, 0.02f);
            projectileLight.range = explosionRadius * 1.5f;
            projectileLight.intensity = 5.5f;
            projectileLight.shadows = LightShadows.None;
        }

        private void CreateFlightRing(
            string objectName,
            float radius,
            float width,
            Quaternion localRotation
        )
        {
            LineRenderer ring = CreateRing(
                objectName,
                visualRoot,
                radius,
                width,
                new Color(1f, 0.48f, 0.05f, 0.94f),
                localRotation
            );
            flightRings.Add(ring);
        }

        private void BuildExplosionRings()
        {
            explosionRings.Clear();
            explosionRings.Add(
                CreateRing(
                    "FugaExplosionXZ",
                    transform,
                    1f,
                    0.13f,
                    new Color(1f, 0.18f, 0.01f, 0.98f),
                    Quaternion.Euler(90f, 0f, 0f)
                )
            );
            explosionRings.Add(
                CreateRing(
                    "FugaExplosionXY",
                    transform,
                    1f,
                    0.11f,
                    new Color(1f, 0.62f, 0.06f, 0.94f),
                    Quaternion.identity
                )
            );
            explosionRings.Add(
                CreateRing(
                    "FugaExplosionTilted",
                    transform,
                    1f,
                    0.085f,
                    new Color(1f, 0.90f, 0.38f, 0.90f),
                    Quaternion.Euler(42f, 18f, 35f)
                )
            );
        }

        private LineRenderer CreateRing(
            string objectName,
            Transform parent,
            float radius,
            float width,
            Color color,
            Quaternion localRotation
        )
        {
            GameObject ringObject = new GameObject(objectName);
            ringObject.transform.SetParent(parent, false);
            ringObject.transform.localRotation = localRotation;

            LineRenderer line = ringObject.AddComponent<LineRenderer>();
            line.loop = true;
            line.useWorldSpace = false;
            line.positionCount = 72;
            line.startWidth = width;
            line.endWidth = width;
            line.startColor = color;
            line.endColor = color;
            line.numCornerVertices = 4;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.material = CreateMaterial(color, true);

            for (int index = 0; index < line.positionCount; index++)
            {
                float angle = (float)index / line.positionCount * Mathf.PI * 2f;
                line.SetPosition(
                    index,
                    new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f)
                );
            }
            return line;
        }

        private Material CreateMaterial(Color color, bool emission)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            Material material = new Material(shader) { color = color };
            if (emission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2.4f);
            }
            runtimeMaterials.Add(material);
            return material;
        }

        private void UpdateFlightVisual()
        {
            float elapsed = Time.time - startedAt;
            float pulse = 1f + Mathf.Sin(elapsed * 30f) * 0.10f;
            if (core != null)
            {
                core.localScale = Vector3.one * collisionRadius * 1.25f * pulse;
            }
            if (visualRoot != null)
            {
                visualRoot.Rotate(Vector3.forward, -310f * Time.deltaTime, Space.Self);
            }
            if (projectileLight != null)
            {
                projectileLight.intensity = 5.2f + Mathf.Sin(elapsed * 24f) * 1.3f;
            }
        }

        private void UpdateExplosionVisual()
        {
            const float explosionDuration = 0.58f;
            float normalized = Mathf.Clamp01((Time.time - explodedAt) / explosionDuration);
            float scale = Mathf.Lerp(0.65f, explosionRadius, normalized);
            float alpha = 1f - normalized;

            if (core != null)
            {
                core.localScale = Vector3.one * Mathf.Lerp(collisionRadius * 1.4f, explosionRadius * 0.65f, normalized);
            }

            for (int index = 0; index < explosionRings.Count; index++)
            {
                LineRenderer ring = explosionRings[index];
                if (ring == null)
                {
                    continue;
                }

                float offsetScale = scale * (1f - index * 0.12f);
                ring.transform.localScale = Vector3.one * offsetScale;
                Color color = index switch
                {
                    0 => new Color(1f, 0.15f, 0.01f, 0.98f * alpha),
                    1 => new Color(1f, 0.58f, 0.04f, 0.92f * alpha),
                    _ => new Color(1f, 0.92f, 0.42f, 0.86f * alpha),
                };
                ring.startColor = color;
                ring.endColor = color;
            }

            if (projectileLight != null)
            {
                projectileLight.range = Mathf.Lerp(explosionRadius, explosionRadius * 2.2f, normalized);
                projectileLight.intensity = Mathf.Lerp(10f, 0f, normalized);
            }

            if (normalized >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
