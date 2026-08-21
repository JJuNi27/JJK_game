using System.Collections.Generic;
using UnityEngine;

namespace JJKGame.Player
{
    public sealed class PrototypeSignatureSpatialVfx : MonoBehaviour
    {
        private readonly List<Material> runtimeMaterials = new List<Material>();

        private Transform followTarget;
        private Vector3 followOffset;
        private Color primaryColor;
        private Color secondaryColor;
        private float startRadius;
        private float endRadius;
        private float duration;
        private float startedAt;
        private float spinSpeed;
        private LineRenderer outerRing;
        private LineRenderer innerRing;
        private Light pulseLight;

        public static void SpawnFollowAura(
            Transform target,
            Vector3 localOffset,
            Color primary,
            Color secondary,
            float startRadius,
            float endRadius,
            float duration,
            float spinSpeed = 120f
        )
        {
            if (target == null)
            {
                return;
            }

            PrototypeSignatureSpatialVfx effect = CreateInstance(
                target.position + target.TransformDirection(localOffset),
                primary,
                secondary,
                startRadius,
                endRadius,
                duration,
                spinSpeed
            );
            if (effect == null)
            {
                return;
            }

            effect.followTarget = target;
            effect.followOffset = localOffset;
        }

        public static void SpawnWorldBurst(
            Vector3 worldPosition,
            Color primary,
            Color secondary,
            float startRadius,
            float endRadius,
            float duration,
            float spinSpeed = 160f
        )
        {
            CreateInstance(
                worldPosition,
                primary,
                secondary,
                startRadius,
                endRadius,
                duration,
                spinSpeed
            );
        }

        private static PrototypeSignatureSpatialVfx CreateInstance(
            Vector3 worldPosition,
            Color primary,
            Color secondary,
            float initialRadius,
            float finalRadius,
            float effectDuration,
            float effectSpinSpeed
        )
        {
            if (effectDuration <= 0f || finalRadius <= 0f)
            {
                return null;
            }

            GameObject root = new GameObject("PrototypeSignatureSpatialVfx");
            root.transform.position = worldPosition;
            PrototypeSignatureSpatialVfx effect = root.AddComponent<PrototypeSignatureSpatialVfx>();
            effect.Configure(
                primary,
                secondary,
                Mathf.Max(0.05f, initialRadius),
                Mathf.Max(initialRadius, finalRadius),
                effectDuration,
                effectSpinSpeed
            );
            return effect;
        }

        private void Configure(
            Color primary,
            Color secondary,
            float initialRadius,
            float finalRadius,
            float effectDuration,
            float effectSpinSpeed
        )
        {
            primaryColor = primary;
            secondaryColor = secondary;
            startRadius = initialRadius;
            endRadius = finalRadius;
            duration = Mathf.Max(0.05f, effectDuration);
            spinSpeed = effectSpinSpeed;
            startedAt = Time.unscaledTime;

            outerRing = CreateRing(
                "OuterRing",
                0.095f,
                primaryColor,
                Quaternion.Euler(90f, 0f, 0f)
            );
            innerRing = CreateRing(
                "InnerRing",
                0.055f,
                secondaryColor,
                Quaternion.Euler(68f, 18f, 26f)
            );

            GameObject lightObject = new GameObject("PulseLight");
            lightObject.transform.SetParent(transform, false);
            pulseLight = lightObject.AddComponent<Light>();
            pulseLight.type = LightType.Point;
            pulseLight.color = primaryColor;
            pulseLight.range = Mathf.Max(4f, endRadius * 1.7f);
            pulseLight.intensity = 4.5f;
            pulseLight.shadows = LightShadows.None;

            ApplyVisual(0f);
        }

        private void Update()
        {
            if (followTarget != null)
            {
                transform.position =
                    followTarget.position + followTarget.TransformDirection(followOffset);
            }

            float elapsed = Time.unscaledTime - startedAt;
            float progress = Mathf.Clamp01(elapsed / duration);
            ApplyVisual(progress);

            transform.Rotate(Vector3.up, spinSpeed * Time.unscaledDeltaTime, Space.World);
            if (innerRing != null)
            {
                innerRing.transform.Rotate(
                    Vector3.forward,
                    -spinSpeed * 1.35f * Time.unscaledDeltaTime,
                    Space.Self
                );
            }

            if (progress >= 1f)
            {
                Destroy(gameObject);
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

        private void ApplyVisual(float progress)
        {
            float eased = 1f - (1f - progress) * (1f - progress);
            float radius = Mathf.Lerp(startRadius, endRadius, eased);
            float alpha = Mathf.Clamp01(1f - progress);
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 26f) * 0.06f;

            if (outerRing != null)
            {
                outerRing.transform.localScale = Vector3.one * radius * pulse;
                SetRingColor(outerRing, WithAlpha(primaryColor, primaryColor.a * alpha));
            }

            if (innerRing != null)
            {
                innerRing.transform.localScale = Vector3.one * radius * 0.72f;
                SetRingColor(innerRing, WithAlpha(secondaryColor, secondaryColor.a * alpha));
            }

            if (pulseLight != null)
            {
                pulseLight.range = Mathf.Lerp(
                    Mathf.Max(3f, startRadius * 2f),
                    Mathf.Max(5f, endRadius * 1.8f),
                    eased
                );
                pulseLight.intensity = Mathf.Lerp(5.5f, 0f, progress);
            }
        }

        private LineRenderer CreateRing(
            string objectName,
            float width,
            Color color,
            Quaternion localRotation
        )
        {
            GameObject ringObject = new GameObject(objectName);
            ringObject.transform.SetParent(transform, false);
            ringObject.transform.localRotation = localRotation;

            LineRenderer line = ringObject.AddComponent<LineRenderer>();
            line.loop = true;
            line.useWorldSpace = false;
            line.positionCount = 64;
            line.startWidth = width;
            line.endWidth = width;
            line.startColor = color;
            line.endColor = color;
            line.numCornerVertices = 4;
            line.numCapVertices = 4;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.material = CreateMaterial(color);

            for (int index = 0; index < line.positionCount; index++)
            {
                float angle = (float)index / line.positionCount * Mathf.PI * 2f;
                line.SetPosition(index, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f));
            }

            return line;
        }

        private Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                return null;
            }

            Material material = new Material(shader) { color = color };
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2.2f);
            }
            runtimeMaterials.Add(material);
            return material;
        }

        private static void SetRingColor(LineRenderer ring, Color color)
        {
            ring.startColor = color;
            ring.endColor = color;
            if (ring.material != null)
            {
                ring.material.color = color;
                if (ring.material.HasProperty("_EmissionColor"))
                {
                    ring.material.SetColor("_EmissionColor", color * 2.2f);
                }
            }
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }
    }
}
