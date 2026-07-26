using System;
using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    public sealed class BlueConvergenceField : MonoBehaviour
    {
        private readonly HashSet<Health> damagedTargets = new HashSet<Health>();
        private readonly HashSet<Health> pulseTargets = new HashSet<Health>();
        private readonly List<LineRenderer> rings = new List<LineRenderer>();
        private readonly List<Color> ringColors = new List<Color>();

        private Health owner;
        private float radius;
        private float duration;
        private float pulseInterval;
        private float damage;
        private float pullSpeed;
        private float hitStun;
        private float startedAt;
        private float nextPulseAt;
        private Action<Health> onTargetHit;
        private Action onFirstImpact;
        private bool impactPlayed;
        private Light fieldLight;

        public void Configure(
            Health newOwner,
            float newRadius,
            float newDuration,
            float newPulseInterval,
            float newDamage,
            float newPullSpeed,
            float newHitStun,
            Action<Health> newOnTargetHit,
            Action newOnFirstImpact
        )
        {
            owner = newOwner;
            radius = Mathf.Max(0.1f, newRadius);
            duration = Mathf.Max(0.1f, newDuration);
            pulseInterval = Mathf.Max(0.03f, newPulseInterval);
            damage = Mathf.Max(0f, newDamage);
            pullSpeed = Mathf.Max(0f, newPullSpeed);
            hitStun = Mathf.Max(0f, newHitStun);
            onTargetHit = newOnTargetHit;
            onFirstImpact = newOnFirstImpact;
            startedAt = Time.time;
            nextPulseAt = Time.time;

            BuildVisual();
            ApplyPulse();
        }

        private void Update()
        {
            float elapsed = Time.time - startedAt;
            if (elapsed >= duration)
            {
                Destroy(gameObject);
                return;
            }

            if (Time.time >= nextPulseAt)
            {
                ApplyPulse();
            }

            UpdateVisual(elapsed / duration);
        }

        private void ApplyPulse()
        {
            nextPulseAt = Time.time + pulseInterval;
            pulseTargets.Clear();

            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider hit in hits)
            {
                Health target = hit.GetComponentInParent<Health>();
                if (
                    target == null
                    || target == owner
                    || target.IsDead
                    || !pulseTargets.Add(target)
                )
                {
                    continue;
                }

                bool firstSuccessfulHit = !damagedTargets.Contains(target);
                if (firstSuccessfulHit)
                {
                    if (!target.TakeDamage(damage))
                    {
                        continue;
                    }

                    damagedTargets.Add(target);
                    onTargetHit?.Invoke(target);
                    if (!impactPlayed)
                    {
                        impactPlayed = true;
                        onFirstImpact?.Invoke();
                    }
                }

                Vector3 direction = transform.position - target.transform.position;
                direction.y = 0f;
                if (direction.sqrMagnitude <= 0.001f)
                {
                    direction = owner != null
                        ? owner.transform.position - target.transform.position
                        : -target.transform.forward;
                    direction.y = 0f;
                }

                if (direction.sqrMagnitude <= 0.001f)
                {
                    direction = -target.transform.forward;
                }

                float stun = firstSuccessfulHit ? hitStun : Mathf.Min(0.08f, hitStun);
                ApplyHitReaction(target, direction.normalized * pullSpeed, stun);
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
                "BlueOuterConvergence",
                radius,
                0.13f,
                new Color(0.08f, 0.65f, 1f, 0.96f),
                RingPlane.XZ,
                Quaternion.identity
            );
            CreateRing(
                "BlueInnerConvergence",
                radius * 0.48f,
                0.09f,
                new Color(0.48f, 0.94f, 1f, 0.94f),
                RingPlane.XY,
                Quaternion.Euler(68f, 0f, 18f)
            );

            GameObject lightObject = new GameObject("BlueConvergenceLight");
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.localPosition = Vector3.up * 0.55f;
            fieldLight = lightObject.AddComponent<Light>();
            fieldLight.type = LightType.Point;
            fieldLight.color = new Color(0.10f, 0.55f, 1f);
            fieldLight.range = radius * 2f;
            fieldLight.intensity = 4.2f;
            fieldLight.shadows = LightShadows.None;
        }

        private void CreateRing(
            string objectName,
            float ringRadius,
            float width,
            Color color,
            RingPlane plane,
            Quaternion rotation
        )
        {
            GameObject ringObject = new GameObject(objectName);
            ringObject.transform.SetParent(transform, false);
            ringObject.transform.localRotation = rotation;

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
                line.material = new Material(shader) { color = color };
            }

            for (int index = 0; index < line.positionCount; index++)
            {
                float angle = (float)index / line.positionCount * Mathf.PI * 2f;
                float cosine = Mathf.Cos(angle) * ringRadius;
                float sine = Mathf.Sin(angle) * ringRadius;
                line.SetPosition(
                    index,
                    plane == RingPlane.XY
                        ? new Vector3(cosine, sine, 0f)
                        : new Vector3(cosine, 0f, sine)
                );
            }

            rings.Add(line);
            ringColors.Add(color);
        }

        private void UpdateVisual(float progress)
        {
            float normalized = Mathf.Clamp01(progress);
            float contraction = Mathf.Lerp(1.12f, 0.62f, normalized);
            float pulse = 1f + Mathf.Sin(Time.time * 24f) * 0.055f;
            transform.localScale = Vector3.one * contraction * pulse;
            transform.Rotate(Vector3.up, 150f * Time.deltaTime, Space.World);

            float fade = Mathf.Clamp01((1f - normalized) * 3.5f);
            for (int index = 0; index < rings.Count; index++)
            {
                Color color = ringColors[index];
                color.a *= fade;
                rings[index].startColor = color;
                rings[index].endColor = color;
            }

            if (fieldLight != null)
            {
                fieldLight.intensity = Mathf.Lerp(4.2f, 0f, normalized);
            }
        }

        private enum RingPlane
        {
            XZ,
            XY,
        }
    }
}
