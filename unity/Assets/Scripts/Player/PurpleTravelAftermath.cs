using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Player
{
    /// <summary>Bounded travel-only lightning and sampled ground scars; the sequence owns lifetime.</summary>
    public sealed class PurpleTravelAftermath : MonoBehaviour
    {
        private readonly List<Material> materials = new List<Material>();
        private readonly List<Color> colors = new List<Color>();
        private readonly List<LineRenderer> lightning = new List<LineRenderer>();
        private readonly List<LineRenderer> scars = new List<LineRenderer>();
        private Vector3 start, forward, right;
        private int sampled;
        private float scarWidth, visualScale;

        public void Configure(Vector3 origin, Vector3 direction, float visualDiameter)
        {
            start = origin;
            scarWidth = visualDiameter * GojoPolishSettings.Current.purpleScarWidthMultiplier;
            visualScale = GojoPolishSettings.Current.purpleVisualScale;
            forward = direction.normalized;
            right = Vector3.Cross(Vector3.up, forward).normalized;
            for (int i = 0; i < 24; i++)
            {
                var arc = ProductionSignatureVfxFactory.CreateArc(transform, "TravelBranch_" + i,
                    1f, 0f, 45f, i % 3 == 0 ? .055f : .025f,
                    i % 2 == 0 ? new Color(1.8f,.22f,2.5f,.85f) : new Color(.8f,.3f,2f,.9f),
                    true, materials, colors);
                arc.useWorldSpace = true;
                arc.positionCount = 7;
                lightning.Add(arc);
                arc.gameObject.SetActive(false);
            }
        }

        public void Render(Vector3 orb, float elapsed, float fade, bool travelling, float visualDiameter = 0f)
        {
            if (travelling && visualDiameter > 0f)
                scarWidth = visualDiameter * GojoPolishSettings.Current.purpleScarWidthMultiplier;
            int frame = Mathf.FloorToInt(elapsed * 24f);
            for (int i = 0; i < lightning.Count; i++)
            {
                var arc = lightning[i];
                bool visible = travelling && (frame + i * 7) % 5 < 3;
                arc.gameObject.SetActive(visible);
                if (!visible) continue;
                int trunk = i / 2;
                float angle = trunk * 2.399963f + frame * .71f;
                Vector3 radial = right * Mathf.Cos(angle) + Vector3.up * Mathf.Sin(angle);
                for (int j = 0; j < 7; j++)
                {
                    float t = j / 6f;
                    float fork = i % 2 == 1 ? Mathf.Max(0f, t - .33f) : 0f;
                    float jag = Mathf.Sin(j * 17.3f + frame * 7.1f + trunk) * .38f;
                    arc.SetPosition(j, orb + radial * (1.2f + t * (trunk % 3 == 0 ? 2.8f : 1.5f)) * visualScale
                        - forward * (t * 4.5f + jag) + Vector3.up * jag
                        + (right * Mathf.Sin(angle) - Vector3.up * Mathf.Cos(angle)) * fork * 3.4f);
                }
            }
            float travelled = Vector3.Dot(orb - start, forward);
            while (travelling && sampled < 96 && sampled * .65f < travelled)
            { AddScar(sampled); sampled++; }
            for (int i = 0; i < scars.Count; i++)
            {
                Color tint = Color.white; tint.a = fade;
                scars[i].startColor = scars[i].endColor = tint;
            }
        }

        private void AddScar(int index)
        {
            Vector3 sample = start + forward * (index * .65f);
            Vector3 surface = sample - Vector3.up * .8f;
            bool ground = false;
            float closest = float.PositiveInfinity;
            foreach (RaycastHit hit in Physics.RaycastAll(sample + Vector3.up * 2f, Vector3.down, 8f,
                ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.GetComponentInParent<Health>() != null || hit.distance >= closest) continue;
                closest = hit.distance; surface = hit.point + hit.normal * .035f; ground = true;
            }
            var scar = ProductionSignatureVfxFactory.CreateArc(transform,
                ground ? "GroundSpaceWound_" + index : "SuspendedSpaceResidue_" + index,
                1f, 0f, 45f, scarWidth * (.94f + (index % 5) * .015f),
                ground ? new Color(.075f,.007f,.14f,.82f) : new Color(.5f,.04f,1f,.45f),
                false, materials, colors);
            scar.useWorldSpace = true;
            scar.widthCurve = new AnimationCurve(new Keyframe(0,.92f), new Keyframe(.28f,1f),
                new Keyframe(.66f,.98f), new Keyframe(1,.9f));
            // Assigning a normalized widthCurve replaces the factory's start/end widths.
            // Keep metre width in the multiplier so the curve cannot collapse it to 1 m.
            scar.widthMultiplier = scarWidth * (.94f + (index % 5) * .015f);
            scar.positionCount = 5;
            if (ground) { scar.alignment = LineAlignment.TransformZ; scar.transform.rotation = Quaternion.Euler(90f,0f,0f); }
            for (int j = 0; j < 5; j++) scar.SetPosition(j, surface + forward * (j * .19f)
                + right * Mathf.Sin(index * 7.1f + j * 3.4f) * .12f);
            scars.Add(scar);
            var seam = ProductionSignatureVfxFactory.CreateArc(transform, "ResidualVioletSeam_" + index,
                1f, 0f, 45f, .025f, new Color(.68f,.06f,1.4f,.7f), true, materials, colors);
            seam.useWorldSpace = true; seam.positionCount = 5;
            for (int j = 0; j < 5; j++) seam.SetPosition(j, scar.GetPosition(j) + Vector3.up * .01f
                + right * scarWidth * (index % 2 == 0 ? .4f : -.4f));
            scars.Add(seam);
        }

        private void OnDestroy()
        {
            foreach (Material material in materials) if (material != null) Destroy(material);
        }
    }
}
