using System;
using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    public sealed class RedTechniqueProjectile : MonoBehaviour
    {
        private readonly HashSet<Health> damagedTargets = new HashSet<Health>();
        private readonly HashSet<Health> frameTargets = new HashSet<Health>();
        private readonly List<LineRenderer> rings = new List<LineRenderer>();
        private readonly List<Color> ringColors = new List<Color>();

        private Health owner;
        private Vector3 direction;
        private float speed;
        private float maxRange;
        private float radius;
        private float damage;
        private float pushSpeed;
        private float hitStun;
        private float travelled;
        private float startedAt;
        private Action<Health> onTargetHit;
        private Action onFirstImpact;
        private bool impactPlayed;
        private Light projectileLight;

        public void Configure(
            Health newOwner,
            Vector3 newDirection,
            float newSpeed,
            float newMaxRange,
            float newRadius,
            float newDamage,
            float newPushSpeed,
            float newHitStun,
            Action<Health> newOnTargetHit,
            Action newOnFirstImpact
        )
        {
            owner = newOwner;
            direction = newDirection;
            direction.y = 0f;
            direction = direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : Vector3.forward;
            speed = Mathf.Max(0.1f, newSpeed);
            maxRange = Mathf.Max(0.1f, newMaxRange);
            radius = Mathf.Max(0.1f, newRadius);
            damage = Mathf.Max(0f, newDamage);
            pushSpeed = Mathf.Max(0f, newPushSpeed);
            hitStun = Mathf.Max(0f, newHitStun);
            onTargetHit = newOnTargetHit;
            onFirstImpact = newOnFirstImpact;
            startedAt = Time.time;

            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            BuildVisual();
        }

        private void Update()
        {
            float step = speed * Time.deltaTime;
            Vector3 previous = transform.position;
            Vector3 next = previous + direction * step;

            ApplyHitsBetween(previous, next);
            transform.position = next;
            travelled += step;
            UpdateVisual();

            if (travelled >= maxRange)
            {
                Destroy(gameObject);
            }
        }

        private void ApplyHitsBetween(Vector3 previous, Vector3 next)
        {
            frameTargets.Clear();
            Collider[] hits = Physics.OverlapCapsule(previous, next, radius);
            foreach (Collider hit in hits)
            {
                Health target = hit.GetComponentInParent<Health>();
                if (
                    target == null
                    || target == owner
                    || target.IsDead
                    || damagedTargets.Contains(target)
                    || !frameTargets.Add(target)
                )
                {
                    continue;
                }

                DamageContext context = new DamageContext(
                    damage,
                    owner != null ? owner.gameObject : gameObject,
                    DamageDeliveryType.CursedTechnique,
                    DamageTraits.None,
                    "CURSED TECHNIQUE REVERSAL: RED",
                    target.transform.position + Vector3.up * 0.8f
                );
                if (target.ReceiveDamage(context) != DamageResolution.Applied)
                {
                    continue;
                }

                damagedTargets.Add(target);
                ApplyHitReaction(target, direction * pushSpeed, hitStun);
                onTargetHit?.Invoke(target);

                if (!impactPlayed)
                {
                    impactPlayed = true;
                    onFirstImpact?.Invoke();
                }
            }
        }

        private static void ApplyHitReaction(Health target, Vector3 impulse, float stun)
        {
            MonoBehaviour[] behaviours = target.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IHitReactable hitReactable)
                {
                    hitReactable.ApplyHitReaction(impulse, stun);
                    break;
                }
            }
        }

        private void BuildVisual()
        {
            CreateRing(
                "RedOuterProjectile",
                radius,
                0.15f,
                new Color(1f, 0.10f, 0.14f, 0.98f)
            );
            CreateRing(
                "RedInnerProjectile",
                radius * 0.43f,
                0.10f,
                new Color(1f, 0.60f, 0.14f, 0.96f)
            );

            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "RedProjectileCore";
            core.transform.SetParent(transform, false);
            core.transform.localScale = Vector3.one * radius * 0.34f;
            Collider coreCollider = core.GetComponent<Collider>();
            if (coreCollider != null)
            {
                Destroy(coreCollider);
            }

            Renderer coreRenderer = core.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (coreRenderer != null && shader != null)
            {
                coreRenderer.material = new Material(shader)
                {
                    color = new Color(1f, 0.22f, 0.08f, 1f),
                };
            }

            GameObject lightObject = new GameObject("RedProjectileLight");
            lightObject.transform.SetParent(transform, false);
            projectileLight = lightObject.AddComponent<Light>();
            projectileLight.type = LightType.Point;
            projectileLight.color = new Color(1f, 0.08f, 0.04f);
            projectileLight.range = radius * 4f;
            projectileLight.intensity = 5f;
            projectileLight.shadows = LightShadows.None;
        }

        private void CreateRing(string objectName, float ringRadius, float width, Color color)
        {
            GameObject ringObject = new GameObject(objectName);
            ringObject.transform.SetParent(transform, false);

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

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader != null)
            {
                line.material = new Material(shader) { color = color };
            }

            for (int index = 0; index < line.positionCount; index++)
            {
                float angle = (float)index / line.positionCount * Mathf.PI * 2f;
                line.SetPosition(
                    index,
                    new Vector3(
                        Mathf.Cos(angle) * ringRadius,
                        Mathf.Sin(angle) * ringRadius,
                        0f
                    )
                );
            }

            rings.Add(line);
            ringColors.Add(color);
        }

        private void UpdateVisual()
        {
            float elapsed = Time.time - startedAt;
            float pulse = 1f + Mathf.Sin(elapsed * 32f) * 0.08f;
            transform.localScale = Vector3.one * pulse;
            transform.Rotate(Vector3.forward, -260f * Time.deltaTime, Space.Self);

            float remaining = Mathf.Clamp01(1f - travelled / maxRange);
            float fade = Mathf.Clamp01(remaining * 4f);
            for (int index = 0; index < rings.Count; index++)
            {
                Color color = ringColors[index];
                color.a *= fade;
                rings[index].startColor = color;
                rings[index].endColor = color;
            }

            if (projectileLight != null)
            {
                projectileLight.intensity = 3.5f + Mathf.Sin(elapsed * 28f) * 1.2f;
            }
        }
    }
}
