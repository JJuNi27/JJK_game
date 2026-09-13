using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Player
{
    /// <summary>Visual rubble only: deterministic three-dimensional tidal trajectories, no physics bodies.</summary>
    public sealed class BlueTornDebris : MonoBehaviour
    {
        private readonly List<Transform> chunks = new List<Transform>();
        private readonly List<Material> materials = new List<Material>();
        private readonly List<Color> colors = new List<Color>();
        private Mesh mesh;
        private float radius;
        private float visibilityScale;

        public void Configure(float boundaryRadius, int count)
        {
            radius = boundaryRadius;
            visibilityScale = GojoPolishSettings.Current.blueDebrisScale;
            mesh = new Mesh { name = "TornAngularRubble" };
            mesh.vertices = new[] { new Vector3(-.5f,-.3f,-.4f), new Vector3(.35f,-.5f,-.3f),
                new Vector3(.5f,.2f,-.5f), new Vector3(-.2f,.55f,-.3f), new Vector3(-.4f,-.5f,.3f),
                new Vector3(.5f,-.2f,.4f), new Vector3(.24f,.5f,.3f), new Vector3(-.55f,.2f,.5f) };
            mesh.triangles = new[] {0,2,1,0,3,2,4,5,6,4,6,7,0,1,5,0,5,4,3,7,6,3,6,2,1,2,6,1,6,5,0,4,7,0,7,3};
            // Split triangle vertices for fractured, flat-shaded faces rather than smooth ice.
            Vector3[] shared = mesh.vertices;
            int[] triangles = mesh.triangles;
            var facets = new Vector3[triangles.Length];
            for (int i = 0; i < triangles.Length; i++) { facets[i] = shared[triangles[i]]; triangles[i] = i; }
            mesh.vertices = facets; mesh.triangles = triangles;
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            Material material = ProductionSignatureVfxFactory.CreateMaterial(
                new Color(.13f,.20f,.27f,1f), materials, colors, false, false, .48f, CullMode.Back);
            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit != null && material != null)
            {
                material.shader = lit;
                material.SetColor("_BaseColor", new Color(.16f,.145f,.13f,1f));
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(.018f,.016f,.014f));
                material.SetFloat("_Smoothness", .015f);
                material.SetFloat("_Metallic", 0f);
            }
            for (int i = 0; i < Mathf.Clamp(count, 6, 48); i++)
            {
                var chunk = new GameObject("TornChunk_" + i, typeof(MeshFilter), typeof(MeshRenderer));
                chunk.transform.SetParent(transform, false);
                chunk.GetComponent<MeshFilter>().sharedMesh = mesh;
                chunk.GetComponent<MeshRenderer>().sharedMaterial = material;
                chunk.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                chunks.Add(chunk.transform);
            }
            Render(0f, 0f);
        }

        public void Render(float elapsed, float progress)
        {
            float collapse = Mathf.SmoothStep(0f, 1f, (progress - .76f) / .24f);
            for (int i = 0; i < chunks.Count; i++)
            {
                int tier = i % 6 == 0 ? 0 : i % 3 == 0 ? 1 : 2;
                float rate = tier == 0 ? .42f : tier == 1 ? .72f : 1.15f;
                float cycle = Mathf.Repeat(i * .381966f + elapsed * rate, 1f);
                float distance = radius * (tier == 0 ? 1.15f : 1f) * (1f - cycle * cycle) * (1f - collapse);
                float angle = i * 2.399963f + elapsed * (tier == 0 ? 1.9f : 3.2f + tier) + cycle * cycle * 4f;
                Quaternion plane = Quaternion.Euler(i * 43.7f, i * 79.1f, i * 17.3f);
                Vector3 orbit = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle) * (.45f + (i % 5) * .12f),
                    Mathf.Sin(angle * 1.7f + i) * .22f);
                chunks[i].localPosition = plane * orbit * distance;
                chunks[i].localRotation = Quaternion.Euler(elapsed * (40f + i * 7f), i * 47f + elapsed * 81f, i * 53f);
                float size = (tier == 0 ? .68f : tier == 1 ? .37f : .12f + (i % 5) * .046f) * (1f - collapse)
                    * Mathf.SmoothStep(0f, 1f, (1f-cycle) / .16f) * Mathf.SmoothStep(0f, 1f, cycle / .08f);
                chunks[i].localScale = new Vector3(1.2f + (i % 4)*.23f, .5f + (i % 5)*.16f, .8f) * size * visibilityScale;
            }
        }

        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
            foreach (Material material in materials) if (material != null) Destroy(material);
        }
    }
}
