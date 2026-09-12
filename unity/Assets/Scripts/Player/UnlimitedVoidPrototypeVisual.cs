using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;
using UnityEngine.Events;
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
        [Header("Arrival Choreography (seconds from activation)")]
        [SerializeField, Min(0.01f)] private float suppressionDuration = 0.22f;
        [SerializeField, Min(0.1f)] private float tunnelDuration = 0.72f;
        [SerializeField, Min(0.1f)] private float voidReadDuration = 0.34f;
        [SerializeField, Min(0.1f)] private float cloudBeatInterval = 0.24f;
        [SerializeField, Min(0.1f)] private float cloudBloomDuration = 0.42f;
        [SerializeField] private UnityEvent<int> onArrivalBeat = new UnityEvent<int>();
        // 0 suppression, 1 tunnel, 2 black void, 3/4/5 cloud blooms, 6 complete.
        public event System.Action<int> ArrivalBeat;
        public string PhaseLabel { get; private set; } = "SUPPRESSION";
        public float PresentationElapsed { get; private set; }
        public bool Paused { get; set; }
        // Four shared timeline cues: group 1, group 2a, group 2b, group 3.
        public event System.Action<int> WhiteBloodBeat;
        private readonly PresentationBeatClock whiteBloodBeats = new PresentationBeatClock();
        private readonly PresentationBeatClock beats = new PresentationBeatClock();
        private readonly List<LineRenderer> tunnelLines = new List<LineRenderer>();
        private readonly List<Transform> clouds = new List<Transform>();
        private readonly List<Material> cloudMaterials = new List<Material>();
        private Transform tunnelRoot;
        private Transform suppressionRoot;
        private Transform suppressionFloor;
        private Material suppressionMaterial;
        private Material cloudTemplate;
        private Camera viewingCamera;
        private Health presentationOwner;
        private float VoidAt => suppressionDuration + tunnelDuration;
        private float CloudsAt => VoidAt + voidReadDuration;
        private float CompleteAt => CloudsAt + cloudBeatInterval * 2f + cloudBloomDuration;

        private readonly List<Material> runtimeMaterials = new List<Material>(12);
        private readonly List<Color> materialColors = new List<Color>(12);
        private readonly List<Material> starMaterials = new List<Material>(4);
        private readonly List<Color> starColors = new List<Color>(4);
        private readonly List<ParticleSystem> starFields = new List<ParticleSystem>(3);

        private Transform starRoot;
        private Transform focalRoot;
        private Light domainLight;
        private Vector3 anchorPosition;
        private Quaternion anchorRotation;
        private float visualRadius = 30f;
        private bool built;
        private DomainPresentationSettings tuning = new DomainPresentationSettings();
        private Material irisMaterial, tailMaterial;
        private readonly List<Material> nebulaMaterials = new List<Material>();
        private bool releasing;
        public float TunnelDuration => tunnelDuration;

        public void Configure(DomainPresentationSettings settings)
        {
            tuning = settings ?? new DomainPresentationSettings();
            visualRadius = Mathf.Max(60f, tuning.interiorRadius);
            suppressionDuration = Mathf.Max(0.01f, tuning.barrierCloseDuration);
            voidReadDuration = tuning.voidReadDuration;
            cloudBeatInterval = tuning.cloudBeatInterval;
            cloudBloomDuration = tuning.cloudBloomDuration;
            BuildVisual();
        }

        public void SetPresentationDelay(float delay) { PresentationElapsed = -Mathf.Max(0f, delay); }

        public void AnchorAt(Vector3 position, Quaternion rotation)
        {
            anchorPosition = position;
            anchorRotation = rotation;
            transform.SetPositionAndRotation(position, rotation);
        }

        public void Configure(float interiorRadius)
        {
            Configure(new DomainPresentationSettings { interiorRadius = Mathf.Max(60f, interiorRadius) });
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
            PresentationElapsed = 0f;
            releasing = false;
            foreach (Material nebula in nebulaMaterials) nebula.SetFloat("_Visibility", 0f);
            beats.Reset(0f, suppressionDuration, VoidAt, CloudsAt,
                CloudsAt + cloudBeatInterval, CloudsAt + cloudBeatInterval * 2f, CompleteAt);
            whiteBloodBeats.Reset(CloudsAt + tuning.Group1Time, CloudsAt + tuning.Group2Time,
                CloudsAt + tuning.Group2Time + tuning.Group2SubBeatGap, CloudsAt + tuning.Group3Time);
            viewingCamera = Camera.main;
            presentationOwner = GetComponentInParent<Health>();
            if (domainLight != null) domainLight.intensity = 0f;
            // The lab supplies a world anchor; gameplay supplies a caster-local root.
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
            RenderArrival(0f);
        }

        protected virtual void Update()
        {
            transform.SetPositionAndRotation(anchorPosition, anchorRotation);
            if (releasing) return;
            if (Paused) return;
            PresentationElapsed += Time.deltaTime;
            float elapsed = PresentationElapsed;
            float eased = Mathf.SmoothStep(0f, 1f, (elapsed - CloudsAt) / Mathf.Max(0.1f, CompleteAt - CloudsAt));
            ApplyStarFade(eased);
            RenderArrival(elapsed);
            if (irisMaterial != null) irisMaterial.SetFloat("_Age", elapsed);
            if (tailMaterial != null) tailMaterial.SetFloat("_Age", elapsed);
            foreach (Material nebula in nebulaMaterials)
            {
                nebula.SetFloat("_Age", elapsed);
                nebula.SetFloat("_Visibility", Mathf.SmoothStep(0f, 1f, (elapsed - VoidAt) / 1.4f));
            }
            while (whiteBloodBeats.TryConsume(elapsed, out int cloudBeat)) WhiteBloodBeat?.Invoke(cloudBeat);
            while (beats.TryConsume(elapsed, out int beat))
            {
                ArrivalBeat?.Invoke(beat);
                onArrivalBeat.Invoke(beat);
                if (presentationOwner != null && (beat == 2 || beat == 6))
                    TechniquePresentationRequests.Raise(TechniquePresentationRequest.AtWorldPoint(
                        presentationOwner, TechniquePresentationId.UnlimitedVoid,
                        beat == 2 ? TechniquePresentationPhase.Release : TechniquePresentationPhase.Culmination,
                        focalRoot.position));
            }

            if (focalRoot != null)
            {
                float arrival = Mathf.Lerp(0.72f, 1f, Mathf.SmoothStep(0f, 1f, (elapsed - VoidAt) / 0.22f));
                float pulse = 1f + Mathf.Sin(elapsed * 1.8f) * 0.018f;
                focalRoot.localScale = Vector3.one * arrival * pulse * tuning.centralFocalScale;
            }
            if (starRoot != null)
            {
                starRoot.Rotate(
                    Vector3.up,
                    0.45f * Time.deltaTime,
                    Space.Self
                );
            }
            if (domainLight != null)
            {
                domainLight.intensity = eased * tuning.interiorBrightness * (1.5f + Mathf.Sin(elapsed * 1.4f) * 0.10f);
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
            DestroyMaterials(cloudMaterials);
            if (cloudTemplate != null) Destroy(cloudTemplate);
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
            suppressionRoot = new GameObject("SuppressedReality").transform;
            suppressionRoot.SetParent(transform, false);
            BuildBackdrop();
            BuildNebula();
            BuildFloorSuppression();
            BuildDepthField();
            BuildFocalElement();
            BuildVisibilityLight();
            BuildTunnel();
            BuildClouds();
        }

        private void BuildBackdrop()
        {
            ProductionSignatureVfxFactory.CreateSphere(
                suppressionRoot, "VoidBackdropEnclosure", Vector3.up * 4f,
                visualRadius * 2.25f, new Color(0.002f, 0.004f, 0.018f, 1f),
                runtimeMaterials, materialColors, 0.15f, CullMode.Front, true
            );
            suppressionMaterial = runtimeMaterials[runtimeMaterials.Count - 1];
            // Draw enclosure behind transparent stars, tunnel and ink layers.
            suppressionMaterial.renderQueue = 2990;
        }

        private void BuildFloorSuppression()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "VoidFloorSuppression";
            suppressionFloor = floor.transform;
            floor.transform.SetParent(suppressionRoot, false);
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

        private void BuildNebula()
        {
            Shader shader = Resources.Load<Shader>("VFX/UnlimitedVoidNebula");
            if (shader == null) return;
            for (int i = 0; i < 2; i++)
            {
                Transform shell = ProductionSignatureVfxFactory.CreateSphere(suppressionRoot,
                    "BlueBlackNebulaDepth_" + i, Vector3.zero, visualRadius * (i == 0 ? 2.12f : 1.8f),
                    Color.white, runtimeMaterials, materialColors, 0f, CullMode.Front, true);
                Material material = shell.GetComponent<Renderer>().sharedMaterial;
                material.shader = shader;
                material.renderQueue = 2992 + i;
                material.SetFloat("_Layer", i);
                material.SetFloat("_Intensity", tuning.nebulaIntensity);
                material.SetFloat("_Visibility", 0f);
                shell.localRotation = Quaternion.Euler(i * 73f, i * 119f, i * 47f);
                nebulaMaterials.Add(material);
            }
        }

        public void HideForWorldRelease()
        {
            releasing = true;
            PhaseLabel = "DOMAIN RELEASE · CENTER-OUTWARD";
            suppressionRoot.gameObject.SetActive(false);
            starRoot.gameObject.SetActive(false);
            focalRoot.gameObject.SetActive(false);
            tunnelRoot.gameObject.SetActive(false);
            foreach (Transform cloud in clouds) cloud.gameObject.SetActive(false);
            domainLight.intensity = 0f;
        }

        private void BuildDepthField()
        {
            starRoot = new GameObject("VoidDepthField").transform;
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
                starRoot, name, color, starMaterials, starColors, true, 6f, 8f, 12f,
                minSpeed, maxSpeed, minSize, maxSize, ParticleSystemShapeType.Sphere,
                radius, false, ParticleSystemSimulationSpace.Local, count, count / 10f
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
            focalRoot.localPosition = Vector3.forward * (visualRadius * 0.62f)
                + Vector3.up * 7.5f;

            GameObject iris = GameObject.CreatePrimitive(PrimitiveType.Quad);
            iris.name = "CosmicIris";
            Destroy(iris.GetComponent<Collider>());
            iris.transform.SetParent(focalRoot, false);
            iris.transform.localScale = Vector3.one * 14f;
            Shader shader = Resources.Load<Shader>("VFX/UnlimitedVoidIris");
            if (shader != null)
            {
                irisMaterial = new Material(shader);
                irisMaterial.SetFloat("_Intensity", tuning.centralFocalIntensity);
                runtimeMaterials.Add(irisMaterial);
                materialColors.Add(Color.white);
                Renderer renderer = iris.GetComponent<Renderer>();
                renderer.sharedMaterial = irisMaterial;
                renderer.sortingOrder = 3; // keep the pitch-black pupil in front of the depth stars
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tail.name = "CosmicEye_RightwardNebulaTail";
                Destroy(tail.GetComponent<Collider>());
                tail.transform.SetParent(focalRoot, false);
                tail.transform.localPosition = new Vector3(12.075f, 0f, .45f);
                tail.transform.localScale = new Vector3(15.75f, 10.5f, 1f);
                tailMaterial = new Material(irisMaterial);
                tailMaterial.SetFloat("_Layer", 1f);
                runtimeMaterials.Add(tailMaterial); materialColors.Add(Color.white);
                var tailRenderer = tail.GetComponent<Renderer>();
                tailRenderer.sharedMaterial = tailMaterial;
                tailRenderer.sortingOrder = 3;
                tailRenderer.shadowCastingMode = ShadowCastingMode.Off;
                tailRenderer.receiveShadows = false;
            }
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

        private void BuildTunnel()
        {
            tunnelRoot = new GameObject("PurpleConvergenceTunnel").transform;
            tunnelRoot.SetParent(transform, false);
            for (int i = 0; i < 72; i++)
            {
                var line = ProductionSignatureVfxFactory.CreateArc(tunnelRoot,
                    "InwardPerspectiveStreak", 1f, 0f, 45f, i % 7 == 0 ? 0.055f : 0.025f,
                    i % 5 == 0 ? new Color(2.8f, 1.15f, 2.5f, 0.9f)
                        : new Color(1.25f, 0.045f, 2.6f, 0.8f),
                    true, runtimeMaterials, materialColors);
                line.positionCount = 2;
                tunnelLines.Add(line);
            }
        }

        private void BuildClouds()
        {
            Shader shader = Resources.Load<Shader>("VFX/UnlimitedVoidInk");
            if (shader == null) return;
            cloudTemplate = new Material(shader);
            for (int i = 0; i < tuning.whiteBloodDensity; i++)
            {
                var cloud = GameObject.CreatePrimitive(PrimitiveType.Quad);
                cloud.name = "SuspendedWhiteBlood_" + i;
                Destroy(cloud.GetComponent<Collider>());
                cloud.transform.SetParent(transform, false);
                float angle = i * 2.399963f;
                float radius = Mathf.Lerp(16f, visualRadius * tuning.whiteBloodDepth,
                    ((i / 3) % 3) * 0.36f + 0.08f * Mathf.Sin(i * 7.1f));
                // Equal-area spherical distribution includes zenith and nadir, not a horizon belt.
                float height = 1f - 2f * (i + .5f) / tuning.whiteBloodDensity;
                float horizontal = Mathf.Sqrt(1f - height * height);
                cloud.transform.localPosition = Vector3.up * 3f + new Vector3(
                    Mathf.Cos(angle) * horizontal, height * tuning.whiteBloodVerticalSpread,
                    Mathf.Sin(angle) * horizontal) * radius;
                cloud.transform.rotation = Quaternion.LookRotation(cloud.transform.position - transform.position);
                cloud.transform.Rotate(Vector3.forward, i * 137.5f, Space.Self);
                var material = new Material(cloudTemplate);
                material.SetFloat("_Seed", i * 7.13f);
                var renderer = cloud.GetComponent<Renderer>();
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                cloudMaterials.Add(material);
                clouds.Add(cloud.transform);
            }
        }

        private void RenderArrival(float elapsed)
        {
            suppressionRoot.gameObject.SetActive(true);
            starRoot.gameObject.SetActive(true);
            PhaseLabel = elapsed < suppressionDuration ? "SUPPRESSION"
                : elapsed < VoidAt ? "PURPLE TUNNEL · SUCTION"
                : elapsed < CloudsAt ? "BLACK VOID · ARRIVAL"
                : elapsed < CompleteAt ? "WHITE INK · BLOOM " + (1 + Mathf.Min(2, (int)((elapsed - CloudsAt) / cloudBeatInterval)))
                : "UNLIMITED VOID · COMPLETE";
            Color dark = new Color(0.002f, 0.004f, 0.018f,
                Mathf.SmoothStep(0f, 1f, elapsed / Mathf.Max(0.01f, suppressionDuration)));
            ProductionSignatureVfxFactory.SetMaterialColor(suppressionMaterial, dark, 0f);
            // Floor is introduced only once reality is suppressed.
            suppressionFloor.gameObject.SetActive(!tuning.hideVisualFloor && elapsed >= suppressionDuration);
            focalRoot.gameObject.SetActive(elapsed >= VoidAt);
            tunnelRoot.gameObject.SetActive(elapsed >= suppressionDuration && elapsed < VoidAt + 0.12f);
            if (viewingCamera == null) viewingCamera = Camera.main;
            if (viewingCamera != null)
            {
                // Preserve the accepted view-local tunnel geometry across the space transition.
                tunnelRoot.SetPositionAndRotation(viewingCamera.transform.position,
                    Quaternion.LookRotation(focalRoot.position - viewingCamera.transform.position));
                focalRoot.rotation = Quaternion.LookRotation(focalRoot.position - viewingCamera.transform.position);
            }
            float t = Mathf.Clamp01((elapsed - suppressionDuration) / Mathf.Max(0.1f, tunnelDuration));
            float envelope = Mathf.SmoothStep(0f, 1f, t * 8f) * (1f - Mathf.Clamp01((elapsed - VoidAt) / 0.12f));
            for (int i = 0; i < tunnelLines.Count; i++)
            {
                // Decouple angular spacing and travel phase to fill the whole view.
                float phase = Mathf.Repeat(Mathf.Sin(i * 127.1f + 3.7f) * 43758.5453f, 1f);
                float cycle = Mathf.Repeat(phase + t * (0.75f + t * 1.6f), 1f);
                float angle = i * 2.399963f;
                float radius = Mathf.Lerp(9f, 0.18f, cycle);
                float depth = Mathf.Lerp(1f, 24f, cycle);
                var radial = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                tunnelLines[i].SetPosition(0, radial * radius + Vector3.forward * depth);
                tunnelLines[i].SetPosition(1, radial * (radius + 1.4f) + Vector3.forward * (depth - 3.2f));
                tunnelLines[i].startColor = new Color(1f, 1f, 1f, envelope * Mathf.Sin(cycle * Mathf.PI));
                tunnelLines[i].endColor = new Color(1f, 1f, 1f, 0f);
            }
            for (int i = 0; i < clouds.Count; i++)
            {
                int group = i % 3;
                float time = group == 0 ? tuning.Group1Time : group == 1
                    ? tuning.Group2Time + ((i / 3) % 2) * tuning.Group2SubBeatGap : tuning.Group3Time;
                float age = elapsed - CloudsAt - time;
                float bloom = Mathf.SmoothStep(0f, 1f, age / .18f);
                clouds[i].gameObject.SetActive(age >= 0f);
                clouds[i].localScale = new Vector3(1.1f + (i % 4) * 0.22f, 0.8f + (i % 5) * 0.12f, 1f)
                    * tuning.whiteBloodScale * Mathf.Lerp(0.1f, 3.5f + i % 3, bloom);
                cloudMaterials[i].SetFloat("_Bloom", bloom);
                cloudMaterials[i].SetFloat("_Age", Mathf.Max(0f, age));
            }
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
