using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Player
{
    /// <summary>Bounded, surface-sampled visual rubble. No rigidbodies, damage, or destruction.</summary>
    internal sealed class RedSurfaceReaction : MonoBehaviour
    {
        private readonly RaycastHit[] hits = new RaycastHit[32];
        private readonly List<Material> materials = new List<Material>();
        private readonly List<Color> colors = new List<Color>();
        private Transform[] chunks;
        private Vector3[] origins, velocities;
        private Mesh mesh;
        private Vector3 normal;
        private float lifetime, scale;
        private ParticleSystem dust;

        internal static bool TrySurface(Vector3 point, Vector3 direction, RaycastHit[] buffer,
            out Vector3 surface, out Vector3 surfaceNormal)
        {
            surface = point;
            surfaceNormal = Vector3.up;
            // A nearby wall takes priority; otherwise use the actual ground below the contact.
            for (int pass = 0; pass < 2; pass++)
            {
                Vector3 origin = pass == 0 ? point - direction * .3f : point + Vector3.up * .4f;
                Vector3 ray = pass == 0 ? direction : Vector3.down;
                int count = Physics.RaycastNonAlloc(origin, ray, buffer, pass == 0 ? 1.8f : 4f,
                    ~0, QueryTriggerInteraction.Ignore);
                float closest = float.PositiveInfinity;
                bool found = false;
                for (int i = 0; i < count; i++)
                {
                    RaycastHit hit = buffer[i];
                    if (hit.collider.GetComponentInParent<Health>() != null || hit.distance >= closest) continue;
                    closest = hit.distance; surface = hit.point; surfaceNormal = hit.normal; found = true;
                }
                if (found) return true;
            }
            return false;
        }

        public void Configure(Vector3 point, Vector3 direction, float duration, Material vaporMaterial)
        {
            if (!TrySurface(point, direction, hits, out Vector3 surface, out normal))
            {
                enabled = false;
                return;
            }
            lifetime = Mathf.Max(.1f, duration);
            scale = GojoPolishSettings.Current.redSurfaceReactionScale;
            transform.SetPositionAndRotation(surface + normal * .035f, Quaternion.LookRotation(normal));
            int count = Mathf.Clamp(GojoPolishSettings.Current.redSurfaceDebrisCount, 0, 24);
            chunks = new Transform[count]; origins = new Vector3[count]; velocities = new Vector3[count];
            mesh = new Mesh { name = "RedFracturedSurfaceProxy" };
            Vector3[] vertices = { new Vector3(-.5f,-.25f,-.2f), new Vector3(.5f,-.15f,-.15f),
                new Vector3(.1f,.45f,-.15f), new Vector3(-.1f,.05f,.4f) };
            int[] triangles = { 0,2,1,0,1,3,1,2,3,2,0,3 };
            var facets = new Vector3[triangles.Length];
            for (int i = 0; i < facets.Length; i++) { facets[i] = vertices[triangles[i]]; triangles[i] = i; }
            mesh.vertices = facets; mesh.triangles = triangles; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            Material stone = ProductionSignatureVfxFactory.CreateMaterial(new Color(.25f,.23f,.21f),
                materials, colors, false, false, 0f, CullMode.Back);
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit != null) { stone.shader = lit; stone.SetColor("_BaseColor", new Color(.25f,.23f,.21f)); }
            for (int i = 0; i < count; i++)
            {
                var chunk = new GameObject("RepelledSurfaceChunk", typeof(MeshFilter), typeof(MeshRenderer));
                chunk.transform.SetParent(transform, false);
                chunk.GetComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = chunk.GetComponent<MeshRenderer>(); renderer.sharedMaterial = stone;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                float angle = i * 2.399963f;
                Vector3 radial = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                origins[i] = radial * (.12f + (i % 4) * .12f) * scale;
                velocities[i] = radial * (3f + (i % 5) * .85f) * scale;
                chunks[i] = chunk.transform;
            }
            dust = ProductionSignatureVfxFactory.CreateParticleSystem(transform, "OutwardSurfaceDust",
                new Color(.66f,.68f,.71f,.58f), materials, colors, false, lifetime,
                lifetime * .65f, lifetime, 4f * scale, 9f * scale, .3f, .65f,
                ParticleSystemShapeType.Circle, .55f * scale, false,
                ParticleSystemSimulationSpace.World, 22, 0f, vaporMaterial);
            var main = dust.main; main.startDelay = .055f;
            var limit = dust.limitVelocityOverLifetime; limit.enabled = true; limit.limit = 2.5f; limit.dampen = .08f;
            dust.Play(false);
            Render(0f);
        }

        public void Render(float elapsed)
        {
            if (chunks == null) return;
            float t = Mathf.Clamp01(elapsed / lifetime);
            // Decelerating displacement with a low hop, never a rising smoke column.
            float travel = (1f - Mathf.Exp(-elapsed * 3.5f)) / 3.5f;
            for (int i = 0; i < chunks.Length; i++)
            {
                chunks[i].localPosition = origins[i] + velocities[i] * travel
                    + Vector3.forward * Mathf.Sin(t * Mathf.PI) * (.2f + i % 3 * .09f) * scale;
                chunks[i].localRotation = Quaternion.Euler(i * 31f + elapsed * 110f, i * 79f, elapsed * 75f);
                float size = (.12f + i % 4 * .045f) * scale
                    * (1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - .6f) / .4f)));
                chunks[i].localScale = new Vector3(1.4f, .65f, 1f) * size;
            }
        }

        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
            foreach (Material material in materials) if (material != null) Destroy(material);
        }
    }
}
