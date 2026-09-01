using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace JJKGame.Player
{
    /// <summary>
    /// CombatMVP-only ParticleSystem implementation of the Gate 4 VFX runtime boundary.
    /// The host and every spawned effect are scene-owned so reloads cannot retain
    /// registrations, particles, or runtime materials.
    /// </summary>
    [DefaultExecutionOrder(1450)]
    [DisallowMultipleComponent]
    public sealed class ProductionParticleVfxRuntime : MonoBehaviour, IPresentationVfxRuntime
    {
        private const string TargetSceneName = "CombatMVP";
        private static ProductionParticleVfxRuntime activeInstance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            activeInstance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            InstallForCurrentScene();
        }

        private static void HandleSceneLoaded(Scene _, LoadSceneMode __)
        {
            InstallForCurrentScene();
        }

        private static void InstallForCurrentScene()
        {
            if (
                SceneManager.GetActiveScene().name != TargetSceneName
                || FindFirstObjectByType<ProductionParticleVfxRuntime>() != null
            )
            {
                return;
            }

            new GameObject("ProductionParticleVfxRuntime")
                .AddComponent<ProductionParticleVfxRuntime>();
        }

        private void Awake()
        {
            if (
                SceneManager.GetActiveScene().name != TargetSceneName
                || (activeInstance != null && activeInstance != this)
            )
            {
                enabled = false;
                Destroy(gameObject);
                return;
            }

            activeInstance = this;
            PresentationVfxRuntime.Register(this);
        }

        private void OnEnable()
        {
            if (activeInstance == this)
            {
                PresentationVfxRuntime.Register(this);
            }
        }

        private void OnDisable()
        {
            PresentationVfxRuntime.Unregister(this);
        }

        private void OnDestroy()
        {
            PresentationVfxRuntime.Unregister(this);
            if (activeInstance == this)
            {
                activeInstance = null;
            }
        }

        public PresentationVfxHandle Spawn(PresentationVfxSpawnRequest request)
        {
            IPresentationVfxInstance instance = request.StyleId switch
            {
                PresentationVfxStyleId.GojoBlue =>
                    GojoBlueVfxInstance.Spawn(request, transform),
                PresentationVfxStyleId.GojoRed =>
                    GojoRedVfxInstance.Spawn(request, transform),
                _ => ProductionParticleVfxInstance.Spawn(request, transform),
            };
            return instance != null ? new PresentationVfxHandle(instance) : default;
        }
    }

    /// <summary>
    /// Shared scene-owned lifecycle for orb-first Gojo signature presentations.
    /// These instances consume renderer metadata only; gameplay and feedback stay
    /// with their existing owners.
    /// </summary>
    internal abstract class GojoSignatureVfxInstance : MonoBehaviour, IPresentationVfxInstance
    {
        protected readonly List<Material> RuntimeMaterials = new List<Material>(12);
        protected readonly List<Color> MaterialColors = new List<Color>(12);
        protected readonly List<ParticleSystem> ParticleSystems = new List<ParticleSystem>(5);

        private Transform followTarget;
        private Vector3 followOffset;
        private float duration;
        private float startedAt;
        private float stopStartedAt;
        private bool useUnscaledTime;
        private bool stopping;
        private bool destroying;

        public bool IsAlive => !destroying && this != null && gameObject != null;
        protected float CurrentTime => useUnscaledTime ? Time.unscaledTime : Time.time;
        protected float Duration => duration;

        protected void Initialize(PresentationVfxSpawnRequest request)
        {
            followTarget = request.FollowsTarget ? request.FollowTarget : null;
            followOffset = request.FollowLocalOffset;
            duration = Mathf.Max(0.05f, request.Duration);
            useUnscaledTime = request.TimePolicy == PresentationVfxTimePolicy.Unscaled;
            startedAt = CurrentTime;
            Build(request);

            foreach (ParticleSystem system in ParticleSystems)
            {
                if (system != null)
                {
                    system.Play(true);
                }
            }
        }

        public void Stop(PresentationVfxStopMode mode = PresentationVfxStopMode.FadeOut)
        {
            if (destroying)
            {
                return;
            }
            if (mode == PresentationVfxStopMode.Immediate)
            {
                destroying = true;
                Destroy(gameObject);
                return;
            }
            if (stopping)
            {
                return;
            }

            stopping = true;
            stopStartedAt = CurrentTime;
            foreach (ParticleSystem system in ParticleSystems)
            {
                if (system != null)
                {
                    system.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
        }

        protected virtual void Update()
        {
            if (followTarget != null)
            {
                transform.position =
                    followTarget.position + followTarget.TransformDirection(followOffset);
                transform.rotation = followTarget.rotation;
            }

            float now = CurrentTime;
            float elapsed = now - startedAt;
            float normalized = Mathf.Clamp01(elapsed / duration);
            Tick(elapsed, normalized, useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);

            float fadeWindow = Mathf.Min(0.10f, duration);
            float naturalFade = 1f - Mathf.Clamp01(
                (elapsed - Mathf.Max(0f, duration - fadeWindow)) / fadeWindow
            );
            float stopFade = stopping
                ? 1f - Mathf.Clamp01((now - stopStartedAt) / 0.14f)
                : 1f;
            ApplyVisualFade(Mathf.Min(naturalFade, stopFade));

            if (elapsed >= duration || (stopping && stopFade <= 0f))
            {
                destroying = true;
                Destroy(gameObject);
            }
        }

        protected abstract void Build(PresentationVfxSpawnRequest request);

        protected virtual void Tick(float elapsed, float normalized, float deltaTime)
        {
        }

        protected void Track(ParticleSystem system)
        {
            if (system != null)
            {
                ParticleSystems.Add(system);
            }
        }

        protected virtual void ApplyVisualFade(float fade)
        {
            for (int index = 0; index < RuntimeMaterials.Count; index++)
            {
                Material material = RuntimeMaterials[index];
                if (material == null)
                {
                    continue;
                }
                Color color = MaterialColors[index];
                color.a *= fade;
                ProductionSignatureVfxFactory.SetMaterialColor(material, color);
            }
        }

        protected virtual void OnDestroy()
        {
            destroying = true;
            foreach (Material material in RuntimeMaterials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
            RuntimeMaterials.Clear();
            MaterialColors.Clear();
            ParticleSystems.Clear();
        }

        protected static GameObject CreateHost(
            string name,
            PresentationVfxSpawnRequest request,
            Transform runtimeRoot
        )
        {
            GameObject host = new GameObject(name);
            host.transform.SetParent(runtimeRoot, true);
            host.transform.position = ProductionSignatureVfxFactory.ResolvePosition(request);
            Vector3 direction = request.HasDirection ? request.Direction : Vector3.forward;
            direction = direction.sqrMagnitude > 0.0001f
                ? direction.normalized
                : Vector3.forward;
            host.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            return host;
        }
    }

    [DisallowMultipleComponent]
    internal sealed class GojoBlueMaterialLibrary : MonoBehaviour
    {
        private const string ResourcePath = "VFX/GojoBlueEnergy";
        private const string ShaderName = "JJKGame/VFX/Gojo Blue Energy";

        private Material energyMaterial;

        public Material EnergyMaterial
        {
            get
            {
                EnsureMaterial();
                return energyMaterial;
            }
        }

        public bool UsesCustomShader { get; private set; }

        public static GojoBlueMaterialLibrary GetOrCreate(Transform runtimeRoot)
        {
            GojoBlueMaterialLibrary library =
                runtimeRoot.GetComponent<GojoBlueMaterialLibrary>();
            if (library == null)
            {
                library = runtimeRoot.gameObject.AddComponent<GojoBlueMaterialLibrary>();
            }
            library.EnsureMaterial();
            return library;
        }

        private void EnsureMaterial()
        {
            if (energyMaterial != null)
            {
                return;
            }

            Shader shader = Resources.Load<Shader>(ResourcePath);
            if (shader == null)
            {
                shader = Shader.Find(ShaderName);
            }
            UsesCustomShader = shader != null;
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }
            if (shader == null)
            {
                return;
            }

            energyMaterial = new Material(shader)
            {
                name = "GojoBlueEnergy_RuntimeTemplate",
                hideFlags = HideFlags.HideAndDontSave,
            };
            if (energyMaterial.HasProperty("_Surface"))
            {
                energyMaterial.SetFloat("_Surface", 1f);
            }
            if (energyMaterial.HasProperty("_SrcBlend"))
            {
                energyMaterial.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            }
            if (energyMaterial.HasProperty("_DstBlend"))
            {
                energyMaterial.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            }
            if (energyMaterial.HasProperty("_ZWrite"))
            {
                energyMaterial.SetFloat("_ZWrite", 0f);
            }
            energyMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            energyMaterial.DisableKeyword("_SURFACE_TYPE_OPAQUE");
            energyMaterial.renderQueue = (int)RenderQueue.Transparent;
        }

        private void OnDestroy()
        {
            if (energyMaterial != null)
            {
                Destroy(energyMaterial);
                energyMaterial = null;
            }
        }
    }

    /// <summary>
    /// Future full-screen distortion integrations can consume this presentation-only
    /// anchor without changing Blue gameplay or the current renderer asset.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GojoBlueDistortionSource : MonoBehaviour
    {
        public float WorldRadius { get; private set; }
        public float NormalizedStrength { get; private set; }
        public bool IsImpactCue { get; private set; }

        internal void Configure(float worldRadius, float strength, bool impactCue)
        {
            WorldRadius = Mathf.Max(0f, worldRadius);
            NormalizedStrength = Mathf.Clamp01(strength);
            IsImpactCue = impactCue;
        }

        internal void SetStrength(float strength)
        {
            NormalizedStrength = Mathf.Clamp01(strength);
        }
    }

    internal sealed class GojoBlueVfxInstance : GojoSignatureVfxInstance
    {
        private sealed class EnergyLayerBinding
        {
            private readonly Renderer renderer;
            private readonly MaterialPropertyBlock properties = new MaterialPropertyBlock();
            private readonly Color bodyColor;
            private readonly Color midColor;
            private readonly Color edgeColor;
            private readonly float baseOpacity;
            private readonly float layerMode;
            private readonly float noiseScale;
            private readonly float noiseSpeed;
            private readonly float detailScale;
            private readonly float detailSpeed;
            private readonly float fresnelPower;
            private readonly float breakup;
            private readonly float emission;
            private readonly float pulseSpeed;
            private readonly float pulseAmount;
            private readonly float phaseOffset;

            public EnergyLayerBinding(
                Renderer targetRenderer,
                Color newBodyColor,
                Color newMidColor,
                Color newEdgeColor,
                float newBaseOpacity,
                float newLayerMode,
                float newNoiseScale,
                float newNoiseSpeed,
                float newDetailScale,
                float newDetailSpeed,
                float newFresnelPower,
                float newBreakup,
                float newEmission,
                float newPulseSpeed,
                float newPulseAmount,
                float newPhaseOffset
            )
            {
                renderer = targetRenderer;
                bodyColor = newBodyColor;
                midColor = newMidColor;
                edgeColor = newEdgeColor;
                baseOpacity = newBaseOpacity;
                layerMode = newLayerMode;
                noiseScale = newNoiseScale;
                noiseSpeed = newNoiseSpeed;
                detailScale = newDetailScale;
                detailSpeed = newDetailSpeed;
                fresnelPower = newFresnelPower;
                breakup = newBreakup;
                emission = newEmission;
                pulseSpeed = newPulseSpeed;
                pulseAmount = newPulseAmount;
                phaseOffset = newPhaseOffset;
            }

            public void Apply(float fade, float compression)
            {
                if (renderer == null)
                {
                    return;
                }

                Color fallback = midColor;
                fallback.a = baseOpacity * fade;
                properties.SetColor("_BaseColor", fallback);
                properties.SetColor("_Color", fallback);
                properties.SetColor("_BodyColor", bodyColor);
                properties.SetColor("_MidColor", midColor);
                properties.SetColor("_EdgeColor", edgeColor);
                properties.SetFloat("_Opacity", baseOpacity * fade);
                properties.SetFloat("_LayerMode", layerMode);
                properties.SetFloat("_NoiseScale", noiseScale);
                properties.SetFloat("_NoiseSpeed", noiseSpeed);
                properties.SetFloat("_DetailScale", detailScale);
                properties.SetFloat("_DetailSpeed", detailSpeed);
                properties.SetFloat("_FresnelPower", fresnelPower);
                properties.SetFloat("_Breakup", breakup);
                properties.SetFloat("_Emission", emission);
                properties.SetFloat("_PulseSpeed", pulseSpeed);
                properties.SetFloat("_PulseAmount", pulseAmount);
                properties.SetFloat("_PhaseOffset", phaseOffset);
                properties.SetFloat("_Compression", compression);
                renderer.SetPropertyBlock(properties);
            }
        }

        private sealed class ConvergenceArcBinding
        {
            private readonly Transform arcTransform;
            private readonly LineRenderer line;
            private readonly Color color;
            private readonly float baseAlpha;
            private readonly float phase;
            private readonly float shrinkSpeed;
            private readonly float rotationSpeed;
            private readonly float minimumScale;
            private readonly float verticalAmplitude;
            private readonly Vector3 baseLocalPosition;
            private float animatedAlpha;

            public ConvergenceArcBinding(
                LineRenderer arcLine,
                Color arcColor,
                float newBaseAlpha,
                float newPhase,
                float newShrinkSpeed,
                float newRotationSpeed,
                float newMinimumScale,
                float newVerticalAmplitude
            )
            {
                line = arcLine;
                arcTransform = arcLine != null ? arcLine.transform : null;
                color = arcColor;
                baseAlpha = newBaseAlpha;
                phase = newPhase;
                shrinkSpeed = newShrinkSpeed;
                rotationSpeed = newRotationSpeed;
                minimumScale = newMinimumScale;
                verticalAmplitude = newVerticalAmplitude;
                baseLocalPosition = arcTransform != null
                    ? arcTransform.localPosition
                    : Vector3.zero;
            }

            public void Tick(float elapsed, float deltaTime, bool impactCue)
            {
                if (arcTransform == null || line == null)
                {
                    return;
                }

                float speed = shrinkSpeed * (impactCue ? 2.15f : 1f);
                float cycle = Mathf.Repeat(elapsed * speed + phase, 1f);
                float eased = Mathf.SmoothStep(0f, 1f, cycle);
                float scale = Mathf.Lerp(1f, minimumScale, eased);
                arcTransform.localScale = Vector3.one * scale;
                arcTransform.localPosition = baseLocalPosition
                    + Vector3.up * (Mathf.Sin(elapsed * 3.7f + phase * 11f) * verticalAmplitude);
                arcTransform.Rotate(
                    Vector3.up,
                    rotationSpeed * (impactCue ? 1.8f : 1f) * deltaTime,
                    Space.Self
                );

                float appear = Mathf.Pow(Mathf.Sin(cycle * Mathf.PI), 0.72f);
                float irregular = 0.45f
                    + 0.55f * Mathf.Clamp01(
                        Mathf.Sin(elapsed * (2.1f + phase) + phase * 19f) * 0.5f + 0.5f
                    );
                animatedAlpha = baseAlpha * appear * irregular;
            }

            public void ApplyFade(float fade)
            {
                if (line == null)
                {
                    return;
                }
                Color faded = color;
                faded.a = animatedAlpha * fade;
                line.startColor = faded;
                faded.a *= 0.55f;
                line.endColor = faded;
            }
        }

        private readonly List<EnergyLayerBinding> energyLayers =
            new List<EnergyLayerBinding>(3);
        private readonly List<ConvergenceArcBinding> convergenceArcs =
            new List<ConvergenceArcBinding>(8);

        private Transform coreRoot;
        private Light compressionLight;
        private GojoBlueDistortionSource distortionSource;
        private bool impactCue;
        private float shaderCompression;
        private float lightPulse;
        private float baseLightIntensity;
        private float baseDistortionStrength;

        public static GojoBlueVfxInstance Spawn(
            PresentationVfxSpawnRequest request,
            Transform runtimeRoot
        )
        {
            if (request.Duration <= 0f)
            {
                return null;
            }
            GameObject host = CreateHost("GojoBlueSignatureVfx", request, runtimeRoot);
            GojoBlueVfxInstance instance = host.AddComponent<GojoBlueVfxInstance>();
            instance.Initialize(request);
            return instance;
        }

        protected override void Build(PresentationVfxSpawnRequest request)
        {
            impactCue = !request.FollowsTarget;
            float inner = Mathf.Max(0.08f, request.StartRadius);
            float outer = Mathf.Max(inner * 2f, request.EndRadius);
            float orbDiameter = Mathf.Clamp(inner * 1.42f, 0.72f, 1.22f);
            float boundaryRadius = Mathf.Max(1.2f, outer * 0.94f);
            GojoBlueMaterialLibrary materialLibrary =
                GojoBlueMaterialLibrary.GetOrCreate(transform.parent);

            coreRoot = new GameObject("BlueCoreRoot").transform;
            coreRoot.SetParent(transform, false);
            CreateEnergySphere(
                coreRoot,
                "DenseEnergyBody",
                orbDiameter * 0.74f,
                40,
                materialLibrary.EnergyMaterial,
                new Color(0.003f, 0.018f, 0.24f, 1f),
                new Color(0.015f, 0.10f, 0.78f, 1f),
                new Color(0.08f, 0.48f, 1f, 1f),
                0.97f,
                0f,
                3.8f,
                0.22f,
                11.5f,
                -0.48f,
                3.4f,
                0.16f,
                1.28f,
                7.2f,
                0.08f,
                0.17f
            );
            CreateEnergySphere(
                coreRoot,
                "FresnelEnergyShell",
                orbDiameter * 1.04f,
                41,
                materialLibrary.EnergyMaterial,
                new Color(0.004f, 0.045f, 0.42f, 1f),
                new Color(0.025f, 0.26f, 1f, 1f),
                new Color(0.24f, 0.90f, 1f, 1f),
                0.56f,
                1f,
                4.9f,
                -0.31f,
                15.0f,
                0.62f,
                2.25f,
                0.42f,
                1.34f,
                5.2f,
                0.12f,
                0.49f
            );
            CreateEnergySphere(
                coreRoot,
                "ThinOuterDistortionShell",
                orbDiameter * 1.42f,
                42,
                materialLibrary.EnergyMaterial,
                new Color(0.002f, 0.028f, 0.18f, 1f),
                new Color(0.02f, 0.30f, 0.82f, 1f),
                new Color(0.28f, 0.94f, 1f, 1f),
                0.19f,
                2f,
                6.2f,
                0.17f,
                19.0f,
                -0.78f,
                1.55f,
                0.63f,
                0.82f,
                3.8f,
                0.14f,
                0.83f
            );

            BuildCorona(orbDiameter);
            BuildSpiralFlow(boundaryRadius);
            BuildConvergenceField(outer, boundaryRadius);
            BuildCompressionLight(orbDiameter);

            distortionSource = gameObject.AddComponent<GojoBlueDistortionSource>();
            baseDistortionStrength = impactCue ? 0.24f : 0.15f;
            distortionSource.Configure(
                orbDiameter * (impactCue ? 1.75f : 1.45f),
                baseDistortionStrength,
                impactCue
            );
            ApplyEnergyLayers(1f);
        }

        private void CreateEnergySphere(
            Transform parent,
            string name,
            float diameter,
            int sortingOrder,
            Material sharedMaterial,
            Color bodyColor,
            Color midColor,
            Color edgeColor,
            float opacity,
            float layerMode,
            float noiseScale,
            float noiseSpeed,
            float detailScale,
            float detailSpeed,
            float fresnelPower,
            float breakup,
            float emission,
            float pulseSpeed,
            float pulseAmount,
            float phaseOffset
        )
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = name;
            sphere.transform.SetParent(parent, false);
            sphere.transform.localScale = Vector3.one * diameter;
            Collider collider = sphere.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer = sphere.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = sortingOrder;
            if (sharedMaterial != null)
            {
                renderer.sharedMaterial = sharedMaterial;
            }
            energyLayers.Add(
                new EnergyLayerBinding(
                    renderer,
                    bodyColor,
                    midColor,
                    edgeColor,
                    opacity,
                    layerMode,
                    noiseScale,
                    noiseSpeed,
                    detailScale,
                    detailSpeed,
                    fresnelPower,
                    breakup,
                    emission,
                    pulseSpeed,
                    pulseAmount,
                    phaseOffset
                )
            );
        }

        private void BuildCorona(float orbDiameter)
        {
            ParticleSystem corona = ProductionSignatureVfxFactory.CreateParticleSystem(
                coreRoot,
                "BlueCorona",
                new Color(0.08f, 0.58f, 1f, 0.58f),
                RuntimeMaterials,
                MaterialColors,
                true,
                Duration,
                0.09f,
                0.18f,
                -0.32f,
                -0.10f,
                0.018f,
                0.048f,
                ParticleSystemShapeType.Sphere,
                orbDiameter * 0.72f,
                false,
                ParticleSystemSimulationSpace.Local,
                10,
                20f
            );
            ParticleSystem.ShapeModule shape = corona.shape;
            shape.radiusThickness = 0.18f;
            Track(corona);
        }

        private void BuildSpiralFlow(float boundaryRadius)
        {
            ParticleSystem fastSpiral = ProductionSignatureVfxFactory.CreateParticleSystem(
                transform,
                "FastClockwiseInwardStreaks",
                new Color(0.025f, 0.38f, 1f, 0.72f),
                RuntimeMaterials,
                MaterialColors,
                true,
                Duration,
                impactCue ? 0.20f : 0.42f,
                impactCue ? 0.34f : 0.66f,
                impactCue ? -10.5f : -7.2f,
                impactCue ? -7.4f : -4.4f,
                0.022f,
                0.060f,
                ParticleSystemShapeType.Sphere,
                boundaryRadius,
                true,
                ParticleSystemSimulationSpace.Local,
                impactCue ? 30 : 16,
                impactCue ? 18f : 48f
            );
            fastSpiral.transform.localPosition = Vector3.up * 0.13f;
            ParticleSystem.ShapeModule fastShape = fastSpiral.shape;
            fastShape.radiusThickness = 0.06f;
            ParticleSystem.VelocityOverLifetimeModule fastVelocity =
                fastSpiral.velocityOverLifetime;
            fastVelocity.enabled = true;
            fastVelocity.space = ParticleSystemSimulationSpace.Local;
            fastVelocity.orbitalY = new ParticleSystem.MinMaxCurve(4.2f, 6.4f);
            fastVelocity.orbitalX = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);
            ParticleSystem.NoiseModule fastNoise = fastSpiral.noise;
            fastNoise.enabled = true;
            fastNoise.strength = 0.10f;
            fastNoise.frequency = 0.28f;
            ConfigureInwardSize(fastSpiral, 0.04f);
            Track(fastSpiral);

            ParticleSystem slowSpiral = ProductionSignatureVfxFactory.CreateParticleSystem(
                transform,
                "SlowCounterSpiralMotes",
                new Color(0.10f, 0.72f, 1f, 0.48f),
                RuntimeMaterials,
                MaterialColors,
                true,
                Duration,
                impactCue ? 0.22f : 0.48f,
                impactCue ? 0.38f : 0.76f,
                impactCue ? -7.2f : -4.4f,
                impactCue ? -4.8f : -2.2f,
                0.018f,
                0.044f,
                ParticleSystemShapeType.Sphere,
                boundaryRadius * 0.68f,
                false,
                ParticleSystemSimulationSpace.Local,
                impactCue ? 24 : 12,
                impactCue ? 14f : 32f
            );
            slowSpiral.transform.localPosition = Vector3.down * 0.11f;
            ParticleSystem.ShapeModule slowShape = slowSpiral.shape;
            slowShape.radiusThickness = 0.14f;
            ParticleSystem.VelocityOverLifetimeModule slowVelocity =
                slowSpiral.velocityOverLifetime;
            slowVelocity.enabled = true;
            slowVelocity.space = ParticleSystemSimulationSpace.Local;
            slowVelocity.orbitalY = new ParticleSystem.MinMaxCurve(-3.8f, -2.2f);
            slowVelocity.orbitalZ = new ParticleSystem.MinMaxCurve(-0.22f, 0.22f);
            ParticleSystem.NoiseModule slowNoise = slowSpiral.noise;
            slowNoise.enabled = true;
            slowNoise.strength = 0.055f;
            slowNoise.frequency = 0.20f;
            ConfigureInwardSize(slowSpiral, 0.08f);
            Track(slowSpiral);
        }

        private static void ConfigureInwardSize(ParticleSystem system, float finalSize)
        {
            ParticleSystem.SizeOverLifetimeModule size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(
                1f,
                new AnimationCurve(
                    new Keyframe(0f, 0.55f),
                    new Keyframe(0.18f, 1f),
                    new Keyframe(0.76f, 0.64f),
                    new Keyframe(1f, finalSize)
                )
            );
        }

        private void BuildConvergenceField(float outer, float boundaryRadius)
        {
            Transform boundary = new GameObject("ConvergenceBoundaryManager").transform;
            boundary.SetParent(transform, false);
            boundary.localPosition = Vector3.down * 0.28f;

            for (int index = 0; index < 8; index++)
            {
                bool intermediate = index >= 5;
                float radiusFactor = intermediate
                    ? 0.46f + (index - 5) * 0.105f
                    : 0.80f + index * 0.047f;
                float arcRadius = boundaryRadius * radiusFactor;
                float arcLength = 40f + (index * 29) % 69;
                float width = Mathf.Clamp(
                    outer * (0.0065f + (index % 3) * 0.0022f),
                    0.022f,
                    intermediate ? 0.042f : 0.058f
                );
                float alpha = intermediate
                    ? 0.24f + (index - 5) * 0.055f
                    : 0.34f + (index % 3) * 0.09f;
                Color color = intermediate
                    ? new Color(0.08f, 0.64f, 1f, alpha)
                    : new Color(0.025f, 0.38f + index * 0.045f, 1f, alpha);
                LineRenderer arc = ProductionSignatureVfxFactory.CreateArc(
                    boundary,
                    intermediate ? $"MidCompressionArc_{index - 4}" : $"OuterBrokenArc_{index + 1}",
                    arcRadius,
                    13f + index * 61f,
                    arcLength,
                    width,
                    color,
                    false,
                    RuntimeMaterials,
                    MaterialColors
                );
                arc.transform.localPosition = Vector3.up * ((index % 3 - 1) * 0.055f);
                convergenceArcs.Add(
                    new ConvergenceArcBinding(
                        arc,
                        color,
                        alpha,
                        Mathf.Repeat(0.11f + index * 0.137f, 1f),
                        1.35f + (index % 4) * 0.31f,
                        (index % 2 == 0 ? 1f : -1f) * (12f + index * 3.8f),
                        intermediate ? 0.10f : 0.18f + (index % 2) * 0.07f,
                        intermediate ? 0.025f : 0.045f
                    )
                );
            }
        }

        private void BuildCompressionLight(float orbDiameter)
        {
            GameObject lightObject = new GameObject("BlueCompressionLight");
            lightObject.transform.SetParent(coreRoot, false);
            compressionLight = lightObject.AddComponent<Light>();
            compressionLight.type = LightType.Point;
            compressionLight.color = new Color(0.025f, 0.22f, 1f);
            compressionLight.range = Mathf.Clamp(orbDiameter * 4.4f, 3.0f, 5.2f);
            baseLightIntensity = impactCue ? 0.52f : 0.66f;
            compressionLight.intensity = baseLightIntensity;
            compressionLight.shadows = LightShadows.None;
        }

        protected override void Tick(float elapsed, float normalized, float deltaTime)
        {
            shaderCompression = impactCue
                ? Mathf.SmoothStep(0.25f, 1f, normalized)
                : 0.34f + Mathf.Sin(elapsed * 8.6f) * 0.08f;
            lightPulse = 0.88f + Mathf.Sin(elapsed * 10.2f) * 0.12f;

            if (coreRoot != null)
            {
                float scale = impactCue
                    ? Mathf.Lerp(1.24f, 0.58f, Mathf.SmoothStep(0f, 1f, normalized))
                    : 1f + Mathf.Sin(elapsed * 8.6f) * 0.035f;
                coreRoot.localScale = Vector3.one * scale;
                coreRoot.Rotate(Vector3.up, (impactCue ? 95f : 42f) * deltaTime, Space.Self);
            }

            foreach (ConvergenceArcBinding arc in convergenceArcs)
            {
                arc?.Tick(elapsed, deltaTime, impactCue);
            }
        }

        protected override void ApplyVisualFade(float fade)
        {
            base.ApplyVisualFade(fade);
            ApplyEnergyLayers(fade);
            foreach (ConvergenceArcBinding arc in convergenceArcs)
            {
                arc?.ApplyFade(fade);
            }
            if (compressionLight != null)
            {
                compressionLight.intensity = baseLightIntensity * lightPulse * fade;
            }
            if (distortionSource != null)
            {
                float compressionBoost = impactCue
                    ? Mathf.Lerp(1.35f, 0.35f, shaderCompression)
                    : 0.90f + shaderCompression * 0.20f;
                distortionSource.SetStrength(
                    baseDistortionStrength * compressionBoost * fade
                );
            }
        }

        private void ApplyEnergyLayers(float fade)
        {
            foreach (EnergyLayerBinding layer in energyLayers)
            {
                layer?.Apply(fade, shaderCompression);
            }
        }
    }

    internal sealed class GojoRedVfxInstance : GojoSignatureVfxInstance
    {
        private Transform coreRoot;
        private Transform pulseShell;
        private LineRenderer shockFront;
        private bool impactCue;
        private float orbDiameter;
        private float pulseDiameter;

        public static GojoRedVfxInstance Spawn(
            PresentationVfxSpawnRequest request,
            Transform runtimeRoot
        )
        {
            if (request.Duration <= 0f)
            {
                return null;
            }
            GameObject host = CreateHost("GojoRedSignatureVfx", request, runtimeRoot);
            GojoRedVfxInstance instance = host.AddComponent<GojoRedVfxInstance>();
            instance.Initialize(request);
            return instance;
        }

        protected override void Build(PresentationVfxSpawnRequest request)
        {
            impactCue = !request.FollowsTarget;
            float inner = Mathf.Max(0.08f, request.StartRadius);
            float outer = Mathf.Max(inner * 2f, request.EndRadius);
            orbDiameter = Mathf.Clamp(inner * 1.8f, 0.72f, 1.18f);
            pulseDiameter = impactCue
                ? Mathf.Clamp(outer * 1.55f, 2.8f, 7.2f)
                : Mathf.Clamp(outer * 0.95f, 1.8f, 3.6f);

            coreRoot = new GameObject("RedCoreRoot").transform;
            coreRoot.SetParent(transform, false);
            ProductionSignatureVfxFactory.CreateSphere(
                coreRoot, "DenseRedSphere", Vector3.zero, orbDiameter * 0.74f,
                new Color(0.54f, 0.008f, 0.018f, 0.99f), RuntimeMaterials,
                MaterialColors, 1.55f
            );
            ProductionSignatureVfxFactory.CreateSphere(
                coreRoot, "CrimsonGlowShell", Vector3.zero, orbDiameter,
                new Color(1f, 0.025f, 0.035f, 0.66f), RuntimeMaterials,
                MaterialColors, 1.75f
            );
            ProductionSignatureVfxFactory.CreateSphere(
                coreRoot, "WarmRedRim", Vector3.zero, orbDiameter * 1.28f,
                new Color(1f, 0.12f, 0.025f, 0.17f), RuntimeMaterials,
                MaterialColors, 1.15f
            );

            pulseShell = ProductionSignatureVfxFactory.CreateSphere(
                transform, "RepulsionPulse", Vector3.zero, orbDiameter,
                new Color(1f, 0.035f, 0.045f, impactCue ? 0.16f : 0.10f),
                RuntimeMaterials, MaterialColors, 1.25f
            );
            shockFront = ProductionSignatureVfxFactory.CreateArc(
                transform, "ThinShockFront", 0.5f, 0f, 360f,
                impactCue ? 0.065f : 0.045f,
                new Color(1f, 0.10f, 0.08f, impactCue ? 0.78f : 0.58f),
                true, RuntimeMaterials, MaterialColors
            );

            ParticleSystem fragments = ProductionSignatureVfxFactory.CreateParticleSystem(
                transform,
                impactCue ? "ImpactOutwardStreaks" : "ShortTrailingFragments",
                new Color(1f, 0.035f, 0.04f, 0.76f), RuntimeMaterials,
                MaterialColors, !impactCue, Duration, 0.10f,
                impactCue ? 0.18f : 0.22f, impactCue ? 6.5f : 1.0f,
                impactCue ? 11f : 3.2f, 0.030f, 0.070f,
                ParticleSystemShapeType.Sphere, orbDiameter * 0.48f, true,
                ParticleSystemSimulationSpace.World, impactCue ? 22 : 8,
                impactCue ? 0f : 32f
            );
            ParticleSystem.ShapeModule fragmentShape = fragments.shape;
            fragmentShape.radiusThickness = impactCue ? 0.1f : 0.7f;
            Track(fragments);

            if (!impactCue)
            {
                ParticleSystem trail = ProductionSignatureVfxFactory.CreateParticleSystem(
                    transform, "CompactResidualTrail",
                    new Color(0.72f, 0.008f, 0.018f, 0.58f), RuntimeMaterials,
                    MaterialColors, true, Duration, 0.10f, 0.20f, 1.2f, 3.0f,
                    0.035f, 0.070f, ParticleSystemShapeType.Cone,
                    orbDiameter * 0.16f, true, ParticleSystemSimulationSpace.World,
                    6, 24f
                );
                trail.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                ParticleSystem.ShapeModule trailShape = trail.shape;
                trailShape.angle = 8f;
                trailShape.length = 0.10f;
                Track(trail);
            }
        }

        protected override void Tick(float elapsed, float normalized, float deltaTime)
        {
            if (coreRoot != null)
            {
                coreRoot.localScale = Vector3.one * (1f + Mathf.Sin(elapsed * 24f) * 0.035f);
                coreRoot.Rotate(Vector3.forward, 95f * deltaTime, Space.Self);
            }

            float cycle = impactCue ? normalized : Mathf.Repeat(elapsed / 0.30f, 1f);
            float expansion = Mathf.SmoothStep(0f, 1f, cycle);
            float diameter = Mathf.Lerp(orbDiameter * 0.92f, pulseDiameter, expansion);
            if (pulseShell != null)
            {
                pulseShell.localScale = Vector3.one * diameter;
            }
            if (shockFront != null)
            {
                shockFront.transform.localScale = Vector3.one * Mathf.Lerp(
                    orbDiameter * 0.82f,
                    pulseDiameter,
                    expansion
                );
                Color lineColor = new Color(
                    1f,
                    0.08f,
                    0.065f,
                    (1f - cycle) * (impactCue ? 0.82f : 0.58f)
                );
                shockFront.startColor = lineColor;
                shockFront.endColor = lineColor;
            }
        }
    }

    /// <summary>
    /// Primitive/material helpers shared by dedicated Gojo presenters. Callers own
    /// every returned runtime material and destroy it with their visual lifecycle.
    /// </summary>
    internal static class ProductionSignatureVfxFactory
    {
        public static Vector3 ResolvePosition(PresentationVfxSpawnRequest request)
        {
            return request.FollowsTarget && request.FollowTarget != null
                ? request.FollowTarget.position
                    + request.FollowTarget.TransformDirection(request.FollowLocalOffset)
                : request.WorldPosition;
        }

        public static Transform CreateSphere(
            Transform parent,
            string name,
            Vector3 localPosition,
            float diameter,
            Color color,
            List<Material> materials,
            List<Color> colors,
            float emissionMultiplier,
            CullMode cullMode = CullMode.Back,
            bool transparent = true
        )
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = name;
            sphere.transform.SetParent(parent, false);
            sphere.transform.localPosition = localPosition;
            sphere.transform.localScale = Vector3.one * Mathf.Max(0.01f, diameter);

            Collider collider = sphere.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }

            Renderer renderer = sphere.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                Material material = CreateMaterial(
                    color,
                    materials,
                    colors,
                    false,
                    transparent,
                    emissionMultiplier,
                    cullMode
                );
                if (material != null)
                {
                    renderer.sharedMaterial = material;
                }
            }
            return sphere.transform;
        }

        public static LineRenderer CreateArc(
            Transform parent,
            string name,
            float radius,
            float startDegrees,
            float arcDegrees,
            float width,
            Color color,
            bool xyPlane,
            List<Material> materials,
            List<Color> colors
        )
        {
            GameObject arcObject = new GameObject(name);
            arcObject.transform.SetParent(parent, false);
            LineRenderer line = arcObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = arcDegrees >= 359.5f;
            line.positionCount = Mathf.Max(8, Mathf.CeilToInt(Mathf.Abs(arcDegrees) / 6f));
            line.startWidth = width;
            line.endWidth = width * 0.62f;
            line.numCornerVertices = 3;
            line.numCapVertices = 3;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.sortingOrder = 34;

            Material material = CreateMaterial(
                color, materials, colors, false, true, 1.45f, CullMode.Off
            );
            if (material != null)
            {
                line.sharedMaterial = material;
            }

            float denominator = line.loop ? line.positionCount : line.positionCount - 1f;
            for (int index = 0; index < line.positionCount; index++)
            {
                float angle = (startDegrees + arcDegrees * index / denominator) * Mathf.Deg2Rad;
                Vector3 point = xyPlane
                    ? new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f)
                    : new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                line.SetPosition(index, point);
            }
            return line;
        }

        public static ParticleSystem CreateParticleSystem(
            Transform parent,
            string name,
            Color color,
            List<Material> materials,
            List<Color> colors,
            bool loop,
            float duration,
            float lifetimeMin,
            float lifetimeMax,
            float speedMin,
            float speedMax,
            float sizeMin,
            float sizeMax,
            ParticleSystemShapeType shapeType,
            float shapeRadius,
            bool stretched,
            ParticleSystemSimulationSpace simulationSpace,
            int burstCount,
            float rateOverTime
        )
        {
            GameObject child = new GameObject(name, typeof(ParticleSystem));
            child.transform.SetParent(parent, false);
            ParticleSystem system = child.GetComponent<ParticleSystem>();

            ParticleSystem.MainModule main = system.main;
            main.loop = loop;
            main.playOnAwake = false;
            main.duration = Mathf.Max(0.05f, duration);
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifetimeMin, lifetimeMax);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            main.startColor = color;
            main.maxParticles = 180;
            main.simulationSpace = simulationSpace;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = Mathf.Max(0f, rateOverTime);
            if (burstCount > 0)
            {
                emission.SetBursts(new[]
                {
                    new ParticleSystem.Burst(
                        0f,
                        (short)Mathf.Clamp(burstCount, 1, 160)
                    ),
                });
            }

            ParticleSystem.ShapeModule shape = system.shape;
            shape.enabled = true;
            shape.shapeType = shapeType;
            shape.radius = Mathf.Max(0.01f, shapeRadius);

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(color, 1f),
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(color.a, 0.08f),
                    new GradientAlphaKey(color.a * 0.70f, 0.72f),
                    new GradientAlphaKey(0f, 1f),
                }
            );
            colorOverLifetime.color = gradient;

            ParticleSystemRenderer renderer = child.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = stretched
                ? ParticleSystemRenderMode.Stretch
                : ParticleSystemRenderMode.Billboard;
            renderer.velocityScale = stretched ? 0.10f : 0f;
            renderer.lengthScale = stretched ? 1.45f : 1f;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = 36;
            Material material = CreateMaterial(
                color, materials, colors, true, true, 1.55f, CullMode.Off
            );
            if (material != null)
            {
                renderer.sharedMaterial = material;
            }
            return system;
        }

        public static Material CreateMaterial(
            Color color,
            List<Material> materials,
            List<Color> colors,
            bool particle,
            bool transparent,
            float emissionMultiplier,
            CullMode cullMode = CullMode.Back
        )
        {
            Shader shader = particle
                ? Shader.Find("Universal Render Pipeline/Particles/Unlit")
                : Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null && particle)
            {
                shader = Shader.Find("Particles/Standard Unlit");
            }
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }
            if (shader == null)
            {
                return null;
            }

            Material material = new Material(shader) { color = color };
            if (material.HasProperty("_Cull"))
            {
                material.SetFloat("_Cull", (float)cullMode);
            }
            if (transparent)
            {
                if (material.HasProperty("_Surface"))
                {
                    material.SetFloat("_Surface", 1f);
                }
                if (material.HasProperty("_SrcBlend"))
                {
                    material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                }
                if (material.HasProperty("_DstBlend"))
                {
                    material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                }
                if (material.HasProperty("_ZWrite"))
                {
                    material.SetFloat("_ZWrite", 0f);
                }
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.DisableKeyword("_SURFACE_TYPE_OPAQUE");
                material.renderQueue = (int)RenderQueue.Transparent;
            }
            SetMaterialColor(material, color, emissionMultiplier);
            materials?.Add(material);
            colors?.Add(color);
            return material;
        }

        public static void SetMaterialColor(
            Material material,
            Color color,
            float emissionMultiplier = 1.55f
        )
        {
            if (material == null)
            {
                return;
            }
            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            if (material.HasProperty("_EmissionColor"))
            {
                Color emission = color * emissionMultiplier;
                emission.a = color.a;
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission);
            }
        }
    }
}
