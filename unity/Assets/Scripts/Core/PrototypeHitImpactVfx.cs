using System.Collections.Generic;
using UnityEngine;

namespace JJKGame.Core
{
    public sealed class PrototypeHitImpactVfx : MonoBehaviour
    {
        private readonly List<LineRenderer> rays = new List<LineRenderer>();

        private Material rayMaterial;
        private float startedAt;
        private float lifetime;
        private float startScale;
        private float endScale;
        private Color baseColor;

        public static void Spawn(Vector3 worldPosition, int chainStep)
        {
            GameObject host = new GameObject($"PrototypeHitImpact_{Mathf.Clamp(chainStep, 1, 3)}");
            host.transform.position = worldPosition;
            PrototypeHitImpactVfx effect = host.AddComponent<PrototypeHitImpactVfx>();
            effect.Initialize(Mathf.Clamp(chainStep, 1, 3));
        }

        private void Initialize(int chainStep)
        {
            bool finisher = chainStep >= 3;
            int rayCount = chainStep == 1 ? 6 : chainStep == 2 ? 8 : 12;
            float rayLength = chainStep == 1 ? 0.42f : chainStep == 2 ? 0.58f : 0.92f;
            float width = chainStep == 1 ? 0.035f : chainStep == 2 ? 0.045f : 0.065f;

            lifetime = chainStep == 1 ? 0.10f : chainStep == 2 ? 0.12f : 0.18f;
            startScale = 0.72f;
            endScale = finisher ? 1.55f : 1.25f;
            baseColor = finisher
                ? new Color(1f, 0.78f, 0.24f, 1f)
                : chainStep == 2
                    ? new Color(0.66f, 0.88f, 1f, 1f)
                    : new Color(0.86f, 0.95f, 1f, 1f);

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader != null)
            {
                rayMaterial = new Material(shader)
                {
                    color = baseColor,
                };
            }

            for (int index = 0; index < rayCount; index++)
            {
                float angle = (float)index / rayCount * Mathf.PI * 2f;
                float y = Mathf.Sin(angle * 2f + 0.6f) * 0.34f;
                Vector3 direction = new Vector3(Mathf.Cos(angle), y, Mathf.Sin(angle)).normalized;

                GameObject rayObject = new GameObject($"Ray_{index}");
                rayObject.transform.SetParent(transform, false);
                LineRenderer line = rayObject.AddComponent<LineRenderer>();
                line.useWorldSpace = false;
                line.positionCount = 2;
                line.numCapVertices = 2;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;
                line.startWidth = width;
                line.endWidth = width * 0.16f;
                line.startColor = baseColor;
                line.endColor = baseColor;
                if (rayMaterial != null)
                {
                    line.material = rayMaterial;
                }

                float inner = finisher ? 0.10f : 0.07f;
                line.SetPosition(0, direction * inner);
                line.SetPosition(1, direction * rayLength);
                rays.Add(line);
            }

            startedAt = Time.unscaledTime;
            transform.localScale = Vector3.one * startScale;
        }

        private void Update()
        {
            float elapsed = Time.unscaledTime - startedAt;
            float progress = lifetime > 0f ? Mathf.Clamp01(elapsed / lifetime) : 1f;
            float eased = 1f - Mathf.Pow(1f - progress, 3f);
            transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, eased);

            float alpha = 1f - progress;
            Color currentColor = baseColor;
            currentColor.a = alpha;
            foreach (LineRenderer ray in rays)
            {
                if (ray == null)
                {
                    continue;
                }
                ray.startColor = currentColor;
                Color end = currentColor;
                end.a *= 0.20f;
                ray.endColor = end;
            }

            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (rayMaterial != null)
            {
                Destroy(rayMaterial);
            }
        }
    }
}
