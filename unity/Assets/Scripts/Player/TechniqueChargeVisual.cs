using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Player
{
    /// <summary>Small pose-following anticipation, shared by combat and the lab.
    /// The host owns release/cancellation; this component never starts an attack.</summary>
    public sealed class TechniqueChargeVisual : MonoBehaviour
    {
        private readonly List<Material> materials = new List<Material>();
        private readonly List<Color> colors = new List<Color>();
        private readonly LineRenderer[] filaments = new LineRenderer[12];
        private Transform anchor;
        private Transform core;
        private Light glow;
        private float elapsed, duration;
        private bool inward;
        private float strength;
        private Vector3 offset;

        public static TechniqueChargeVisual Spawn(Transform anchor, bool blue, float duration,
            Vector3? localOffset = null)
        {
            if (anchor == null) return null;
            if (blue && !GojoPolishSettings.Current.blueCompressionEnabled) return null;
            var host = new GameObject(blue ? "BlueAnticipation" : "RedAnticipation");
            var visual = host.AddComponent<TechniqueChargeVisual>();
            visual.anchor = anchor;
            visual.inward = blue;
            visual.strength = blue ? (GojoPolishSettings.Current.blueCompressionEnabled
                ? GojoPolishSettings.Current.blueCompressionStrength : 0f) : 1f;
            visual.offset = localOffset ?? new Vector3(0.35f, 1.25f, 0.8f);
            visual.duration = Mathf.Max(0.01f, duration);
            Color hue = blue ? new Color(0.08f, 0.55f, 2.4f) : new Color(2.6f, 0.035f, 0.08f);
            visual.core = ProductionSignatureVfxFactory.CreateSphere(host.transform,
                "CondensedCastingPoint", Vector3.zero, 0.12f, hue,
                visual.materials, visual.colors, 2f);
            Transform hot = ProductionSignatureVfxFactory.CreateSphere(visual.core, "HotCenter", Vector3.zero,
                0.62f, new Color(2.8f, 2.8f, 3f), visual.materials, visual.colors, 2f);
            hot.GetComponent<Renderer>().sharedMaterial.renderQueue = 3010;
            if (blue)
                ProductionSignatureVfxFactory.ApplyEnergySurface(visual.core,
                    new Color(.001f,.004f,.012f), new Color(.015f,.13f,.32f),
                    new Color(.15f,.65f,1.3f), false);
            for (int i = 0; i < visual.filaments.Length; i++)
            {
                var line = ProductionSignatureVfxFactory.CreateArc(host.transform,
                    blue ? "InwardCompressionStreak" : "PressureFilament", 1f, 0f, 45f, blue ? .009f : .018f, hue, true,
                    visual.materials, visual.colors);
                line.positionCount = 8;
                visual.filaments[i] = line;
            }
            visual.glow = host.AddComponent<Light>();
            visual.glow.type = LightType.Point;
            visual.glow.color = blue ? new Color(0.1f, 0.5f, 1f) : new Color(1f, 0.04f, 0.08f);
            visual.glow.range = 3f;
            visual.glow.shadows = UnityEngine.LightShadows.None;
            visual.Render(0f);
            return visual;
        }

        private void Update()
        {
            if (anchor == null) { Destroy(gameObject); return; }
            elapsed += Time.deltaTime;
            Render(Mathf.Clamp01(elapsed / duration));
            if (elapsed >= duration) Consume();
        }

        public void Consume()
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private void Render(float progress)
        {
            transform.SetPositionAndRotation(anchor.TransformPoint(offset), anchor.rotation);
            // A small dark kernel condenses while straight tidal streaks visibly converge.
            core.localScale = Vector3.one * (inward ? Mathf.Lerp(.24f, .13f, progress)
                : Mathf.Lerp(0.08f, 0.32f, progress * progress)) * strength;
            glow.intensity = Mathf.Lerp(0.05f, inward ? .8f : 2.4f, progress * progress) * strength;
            for (int i = 0; i < filaments.Length; i++)
            {
                float cycle = Mathf.Repeat(elapsed * (inward ? 2.8f : 4.5f) + i * 0.137f, 1f);
                float convergence = Mathf.SmoothStep(0f, 1f, progress);
                float radius = inward ? Mathf.Lerp(0.95f, 0.12f, cycle)
                    : Mathf.Lerp(0.95f + 0.16f * Mathf.Sin(i * 2.1f), 0.035f, convergence);
                for (int j = 0; j < 8; j++)
                {
                    float tail = j / 7f;
                    float angle = i * 2.399963f + (inward ? 0f
                        : elapsed * (9f + (i % 3) * 2f) + tail * 1.5f);
                    float r = radius + tail * (inward ? 0.30f : 0.3f * (1f - convergence));
                    filaments[i].SetPosition(j, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle),
                        Mathf.Sin(i * 1.7f) * 0.35f) * r);
                }
                Color tint = Color.white;
                tint.a = inward ? Mathf.Sin(cycle * Mathf.PI) * Mathf.Lerp(0.25f, 0.9f, progress)
                    : (0.7f + 0.25f * Mathf.Sin(elapsed * 22f + i))
                        * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.8f, 1f, progress)));
                tint.a *= strength;
                filaments[i].startColor = tint;
                tint.a = 0f;
                filaments[i].endColor = tint;
            }
        }

        private void OnDestroy()
        {
            foreach (var material in materials) if (material != null) Destroy(material);
        }
    }
}
