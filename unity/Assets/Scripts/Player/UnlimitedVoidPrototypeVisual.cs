using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Player
{
    /// <summary>
    /// Presentation-only Unlimited Void environment. The domain controller owns only
    /// active/inactive state; this component owns its enclosure, floor suppression,
    /// depth field, focal body, runtime materials, and transition.
    /// </summary>
    [DisallowMultipleComponent]
    public class UnlimitedVoidProductionVisual : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float transitionDuration = 0.34f;
        [SerializeField, Min(0.1f)] private float focalDriftSpeed = 2.8f;

        private readonly List<Material> runtimeMaterials = new List<Material>(12);
        private readonly List<Color> materialColors = new List<Color>(12);
        private readonly List<Material> starMaterials = new List<Material>(4);
        private readonly List<Color> starColors = new List<Color>(4);
        private readonly List<ParticleSystem> starFields = new List<ParticleSystem>(3);

        private Transform starRoot;
        private Transform focalRoot;
        private Transform celestialArc;
        private Light domainLight;
        private Vector3 anchorPosition;
        private Quaternion anchorRotation;
        private float activatedAt;
        private float visualRadius = 30f;
        private bool built;

        public void Configure(float domainRadius)
        {
            visualRadius = Mathf.Clamp(domainRadius, 24f, 38f);
            BuildVisual();
        }

        protected virtual void Awake()
        {
            if (gameObject.activeInHierarchy)
            {
                BuildVisual();
            }
        }

        protected virtual void OnEnable()
        {
            BuildVisual();
            activatedAt = Time.unscaledTime;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            anchorPosition = transform.position;
            anchorRotation = transform.rotation;
            if (focalRoot != null)
            {
                focalRoot.localScale = Vector3.one * 0.55f;
            }
            foreach (ParticleSystem field in starFields)
            {
                if (field == null)
                {
                    continue;
                }
                field.Clear(true);
                field.Play(true);
            }
            ApplyStarFade(0f);
        }

        protected virtual void Update()
        {
            transform.SetPositionAndRotation(anchorPosition, anchorRotation);
            float elapsed = Time.unscaledTime - activatedAt;
            float normalized = Mathf.Clamp01(elapsed / transitionDuration);
            float eased = 1f - Mathf.Pow(1f - normalized, 3f);
            ApplyStarFade(eased);

            if (focalRoot != null)
            {
                float arrival = Mathf.Lerp(0.55f, 1f, eased);
                float pulse = 1f + Mathf.Sin(elapsed * 1.8f) * 0.018f;
                focalRoot.localScale = Vector3.one * arrival * pulse;
            }
            if (celestialArc != null)
            {
                celestialArc.Rotate(
                    Vector3.forward,
                    focalDriftSpeed * Time.unscaledDeltaTime,
                    Space.Self
                );
            }
            if (starRoot != null)
            {
                starRoot.Rotate(
                    Vector3.up,
                    0.45f * Time.unscaledDeltaTime,
                    Space.Self
                );
            }
            if (domainLight != null)
            {
                domainLight.intensity = eased * (1.15f + Mathf.Sin(elapsed * 1.4f) * 0.10f);
            }
        }

        protected virtual void OnDisable()
        {
            foreach (ParticleSystem field in starFields)
            {
                if (field == null)
                {
                    continue;
                }
                field.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (transform.parent != null)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
            }
        }

        protected virtual void OnDestroy()
        {
            DestroyMaterials(runtimeMaterials);
            DestroyMaterials(starMaterials);
            materialColors.Clear();
            starColors.Clear();
            starFields.Clear();
        }

        private void BuildVisual()
        {
            if (built)
            {
                return;
            }
            built = true;
            BuildBackdrop();
            BuildFloorSuppression();
            BuildDepthField();
            BuildFocalElement();
            BuildVisibilityLight();
        }

        private void BuildBackdrop()
        {
            ProductionSignatureVfxFactory.CreateSphere(
                transform, "VoidBackdropEnclosure", Vector3.up * 4f,
                visualRadius * 2.25f, new Color(0.002f, 0.004f, 0.018f, 1f),
                runtimeMaterials, materialColors, 0.15f, CullMode.Front, false
            );
        }

        private void BuildFloorSuppression()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "VoidFloorSuppression";
            floor.transform.SetParent(transform, false);
            floor.transform.localPosition = Vector3.up * 0.025f;
            float planeScale = visualRadius * 2.05f / 10f;
            floor.transform.localScale = new Vector3(planeScale, 1f, planeScale);

            Collider collider = floor.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
            Renderer renderer = floor.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                Material material = ProductionSignatureVfxFactory.CreateMaterial(
                    new Color(0.006f, 0.010f, 0.030f, 1f), runtimeMaterials,
                    materialColors, false, false, 0.20f, CullMode.Back
                );
                if (material != null)
                {
                    renderer.sharedMaterial = material;
                }
            }
        }

        private void BuildDepthField()
        {
            starRoot = new GameObject("InfiniteInformationField").transform;
            starRoot.SetParent(transform, false);
            CreateStarLayer(
                "NearStars", new Color(0.40f, 0.82f, 1f, 0.78f), 108,
                visualRadius * 0.46f, 0.032f, 0.085f, 0.010f, 0.035f, 0.48f
            );
            CreateStarLayer(
                "FarStars", new Color(0.48f, 0.40f, 1f, 0.58f), 145,
                visualRadius * 0.88f, 0.018f, 0.050f, 0.003f, 0.012f, 0.64f
            );
            CreateStarLayer(
                "DustPoints", new Color(0.78f, 0.92f, 1f, 0.35f), 92,
                visualRadius * 0.68f, 0.012f, 0.032f, 0.004f, 0.018f, 0.78f
            );
        }

        private void CreateStarLayer(
            string name,
            Color color,
            int count,
            float radius,
            float minSize,
            float maxSize,
            float minSpeed,
            float maxSpeed,
            float radiusThickness
        )
        {
            ParticleSystem field = ProductionSignatureVfxFactory.CreateParticleSystem(
                starRoot, name, color, starMaterials, starColors, false, 6f, 5.5f, 6f,
                minSpeed, maxSpeed, minSize, maxSize, ParticleSystemShapeType.Sphere,
                radius, false, ParticleSystemSimulationSpace.Local, count, 0f
            );
            ParticleSystem.ShapeModule shape = field.shape;
            shape.radiusThickness = radiusThickness;
            ParticleSystem.NoiseModule noise = field.noise;
            noise.enabled = true;
            noise.strength = 0.035f;
            noise.frequency = 0.08f;
            ParticleSystemRenderer renderer = field.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 2;
            starFields.Add(field);
        }

        private void BuildFocalElement()
        {
            focalRoot = new GameObject("InfiniteSpaceFocalElement").transform;
            focalRoot.SetParent(transform, false);
            focalRoot.localPosition = Vector3.forward * (visualRadius * 0.72f)
                + Vector3.up * 7.5f;

            ProductionSignatureVfxFactory.CreateSphere(
                focalRoot, "DarkCentralBody", Vector3.zero, 6.4f,
                new Color(0.001f, 0.002f, 0.012f, 1f), runtimeMaterials,
                materialColors, 0.05f, CullMode.Back, false
            );
            ProductionSignatureVfxFactory.CreateSphere(
                focalRoot, "DistantVioletHalo", Vector3.zero, 8.2f,
                new Color(0.24f, 0.08f, 0.58f, 0.24f), runtimeMaterials,
                materialColors, 1.25f
            );
            ProductionSignatureVfxFactory.CreateSphere(
                focalRoot, "PaleBlueOuterGlow", Vector3.zero, 10.8f,
                new Color(0.20f, 0.58f, 1f, 0.095f), runtimeMaterials,
                materialColors, 1.15f
            );
            LineRenderer arc = ProductionSignatureVfxFactory.CreateArc(
                focalRoot, "SubtleCelestialArc", 6.2f, 205f, 155f, 0.035f,
                new Color(0.48f, 0.72f, 1f, 0.24f), true, runtimeMaterials,
                materialColors
            );
            celestialArc = arc.transform;
        }

        private void BuildVisibilityLight()
        {
            GameObject lightObject = new GameObject("VoidSilhouetteLight");
            lightObject.transform.SetParent(transform, false);
            lightObject.transform.localPosition = Vector3.up * 5f;
            domainLight = lightObject.AddComponent<Light>();
            domainLight.type = LightType.Point;
            domainLight.color = new Color(0.30f, 0.46f, 0.92f);
            domainLight.range = visualRadius * 0.92f;
            domainLight.intensity = 0f;
            domainLight.shadows = LightShadows.None;
        }

        private void ApplyStarFade(float fade)
        {
            for (int index = 0; index < starMaterials.Count; index++)
            {
                Material material = starMaterials[index];
                if (material == null)
                {
                    continue;
                }
                Color color = starColors[index];
                color.a *= fade;
                ProductionSignatureVfxFactory.SetMaterialColor(material, color);
            }
        }

        private static void DestroyMaterials(List<Material> materials)
        {
            foreach (Material material in materials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
            materials.Clear();
        }
    }

    /// <summary>
    /// Serialized-scene compatibility shim. Runtime creation now uses the production
    /// environment above rather than the former three-ring prototype.
    /// </summary>
    public sealed class UnlimitedVoidPrototypeVisual : UnlimitedVoidProductionVisual
    {
    }
}
