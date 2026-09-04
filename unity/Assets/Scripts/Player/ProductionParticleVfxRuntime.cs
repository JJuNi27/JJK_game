using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace JJKGame.Player
{
    /// <summary>
    /// Production ParticleSystem implementation of the Gate 4 VFX runtime boundary.
    /// The host and every spawned effect are scene-owned so reloads cannot retain
    /// registrations, particles, or runtime materials. CombatMVP installs it
    /// automatically; VFXLab opts in with an explicit scene-owned component.
    /// </summary>
    [DefaultExecutionOrder(1450)]
    [DisallowMultipleComponent]
    public sealed class ProductionParticleVfxRuntime : MonoBehaviour, IPresentationVfxRuntime
    {
        private const string TargetSceneName = "CombatMVP";
        private const string DeveloperPreviewSceneName = "VFXLab";
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
                !SupportsScene(SceneManager.GetActiveScene().name)
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

        private static bool SupportsScene(string sceneName)
        {
            return sceneName == TargetSceneName || sceneName == DeveloperPreviewSceneName;
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
        private const string EnergyResourcePath = "VFX/GojoBlueEnergy";
        private const string EnergyShaderName = "JJKGame/VFX/Gojo Blue Energy";
        private const string DistortionResourcePath = "VFX/GojoBlueDistortion";
        private const string DistortionShaderName = "JJKGame/VFX/Gojo Blue Distortion";
        private const string ParticleResourcePath = "VFX/GojoBlueParticle";
        private const string ParticleShaderName = "JJKGame/VFX/Gojo Blue Particle";

        private Material energyMaterial;
        private Material distortionMaterial;
        private Material particleMaterial;

        public Material EnergyMaterial
        {
            get
            {
                EnsureEnergyMaterial();
                return energyMaterial;
            }
        }

        public Material DistortionMaterial
        {
            get
            {
                EnsureDistortionMaterial();
                return distortionMaterial;
            }
        }

        public Material ParticleMaterial
        {
            get
            {
                EnsureParticleMaterial();
                return particleMaterial;
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
            library.EnsureEnergyMaterial();
            return library;
        }

        private void EnsureEnergyMaterial()
        {
            if (energyMaterial != null)
            {
                return;
            }

            Shader shader = Resources.Load<Shader>(EnergyResourcePath);
            if (shader == null)
            {
                shader = Shader.Find(EnergyShaderName);
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

        private void EnsureDistortionMaterial()
        {
            if (distortionMaterial != null)
            {
                return;
            }

            Shader shader = Resources.Load<Shader>(DistortionResourcePath);
            if (shader == null)
            {
                shader = Shader.Find(DistortionShaderName);
            }
            if (shader == null)
            {
                return;
            }

            distortionMaterial = new Material(shader)
            {
                name = "GojoBlueDistortion_RuntimeTemplate",
                hideFlags = HideFlags.HideAndDontSave,
                renderQueue = (int)RenderQueue.Transparent - 20,
            };
        }

        private void EnsureParticleMaterial()
        {
            if (particleMaterial != null)
            {
                return;
            }

            Shader shader = Resources.Load<Shader>(ParticleResourcePath);
            if (shader == null)
            {
                shader = Shader.Find(ParticleShaderName);
            }
            if (shader == null)
            {
                return;
            }

            particleMaterial = new Material(shader)
            {
                name = "GojoBlueParticle_RuntimeTemplate",
                hideFlags = HideFlags.HideAndDontSave,
                renderQueue = (int)RenderQueue.Transparent,
            };
        }

        private void OnDestroy()
        {
            if (energyMaterial != null)
            {
                Destroy(energyMaterial);
                energyMaterial = null;
            }
            if (distortionMaterial != null)
            {
                Destroy(distortionMaterial);
                distortionMaterial = null;
            }
            if (particleMaterial != null)
            {
                Destroy(particleMaterial);
                particleMaterial = null;
            }
        }
    }

    /// <summary>
    /// Presentation-only Blue distortion metadata and renderer binding. The localized
    /// shell samples the camera opaque texture without changing gameplay or renderer assets.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GojoBlueDistortionSource : MonoBehaviour
    {
        private static readonly int StrengthId = Shader.PropertyToID("_Strength");
        private static readonly int WorldRadiusId = Shader.PropertyToID("_WorldRadius");
        private static readonly int ImpactId = Shader.PropertyToID("_Impact");

        private MaterialPropertyBlock properties;
        private Renderer distortionRenderer;

        public float WorldRadius { get; private set; }
        public float NormalizedStrength { get; private set; }
        public bool IsImpactCue { get; private set; }

        internal void Configure(
            float worldRadius,
            float strength,
            bool impactCue,
            Renderer renderer
        )
        {
            WorldRadius = Mathf.Max(0f, worldRadius);
            NormalizedStrength = Mathf.Clamp01(strength);
            IsImpactCue = impactCue;
            distortionRenderer = renderer;
            ApplyRendererState();
        }

        internal void SetStrength(float strength)
        {
            NormalizedStrength = Mathf.Clamp01(strength);
            ApplyRendererState();
        }

        private void ApplyRendererState()
        {
            if (distortionRenderer == null)
            {
                return;
            }

            properties ??= new MaterialPropertyBlock();
            properties.SetFloat(StrengthId, NormalizedStrength);
            properties.SetFloat(WorldRadiusId, WorldRadius);
            properties.SetFloat(ImpactId, IsImpactCue ? 1f : 0f);
            distortionRenderer.SetPropertyBlock(properties);
        }
    }

    internal sealed class GojoBlueVfxInstance : GojoSignatureVfxInstance
    {
        private enum BlueParticleMaskMode
        {
            SoftMote = 0,
            TaperedStreak = 1,
            BrokenWisp = 2,
            DarkFragment = 3,
            AirflowWisp = 4,
            WindRibbon = 5,
        }

        private enum BlueEnergyLayerKind
        {
            CompressionPoint,
            DenseBody,
            FresnelShell,
            OuterShell,
        }

        private enum BlueParticleLayerKind
        {
            Corona,
            FastSpiral,
            SlowSpiral,
            GroundDust,
            DarkDebris,
            TidalDebris,
            AirflowWisp,
            WindRibbon,
        }

        private sealed class ParticleLayerBinding
        {
            private static readonly int ModeId = Shader.PropertyToID("_Mode");
            private static readonly int FadeId = Shader.PropertyToID("_Fade");
            private static readonly int EmissionId = Shader.PropertyToID("_Emission");
            private static readonly int BreakupId = Shader.PropertyToID("_Breakup");

            private readonly ParticleSystemRenderer renderer;
            private readonly MaterialPropertyBlock properties = new MaterialPropertyBlock();
            private readonly float mode;
            private readonly float emission;
            private readonly float breakup;

            public BlueParticleLayerKind Kind { get; }

            public ParticleLayerBinding(
                ParticleSystemRenderer targetRenderer,
                BlueParticleLayerKind layerKind,
                BlueParticleMaskMode maskMode,
                float emissionMultiplier,
                float breakupAmount
            )
            {
                renderer = targetRenderer;
                Kind = layerKind;
                mode = (float)maskMode;
                emission = emissionMultiplier;
                breakup = breakupAmount;
            }

            public void Apply(float fade)
            {
                if (renderer == null)
                {
                    return;
                }

                properties.SetFloat(ModeId, mode);
                properties.SetFloat(FadeId, Mathf.Clamp01(fade));
                properties.SetFloat(EmissionId, emission);
                properties.SetFloat(BreakupId, breakup);
                renderer.SetPropertyBlock(properties);
            }
        }

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

            public BlueEnergyLayerKind Kind { get; }

            public EnergyLayerBinding(
                Renderer targetRenderer,
                BlueEnergyLayerKind layerKind,
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
                Kind = layerKind;
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
            new List<EnergyLayerBinding>(4);
        private readonly List<ConvergenceArcBinding> convergenceArcs =
            new List<ConvergenceArcBinding>(8);
        private readonly List<ParticleLayerBinding> particleLayers =
            new List<ParticleLayerBinding>(8);

        private Transform coreRoot;
        private Light compressionLight;
        private GojoBlueDistortionSource distortionSource;
        private ParticleSystem groundDustSuction;
        private ParticleSystem darkDebrisFragments;
        private ParticleSystem tidalDebrisTrails;
        private ParticleSystem airflowSuctionWisps;
        private ParticleSystem windRibbonConvergence;
        private bool impactCue;
        private float shaderCompression;
        private float lightPulse;
        private float baseLightIntensity;
        private float baseDistortionStrength;
        private float impactCollapse;
        private float denseEnergyWeight;
        private float fresnelEnergyWeight;
        private float outerEnergyWeight;
        private float coronaWeight;
        private float fastSpiralWeight;
        private float slowSpiralWeight;
        private float groundDustWeight;
        private float darkDebrisWeight;
        private float tidalDebrisWeight;
        private float airflowWispWeight;
        private float windRibbonWeight;
        private float convergenceArcWeight;
        private float distortionWeight;
        private float lightWeight;
        private float fieldPulseAccent;

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
            UpdatePresentationTiming(0f);
            float inner = Mathf.Max(0.08f, request.StartRadius);
            float outer = Mathf.Max(inner * 2f, request.EndRadius);
            float orbDiameter = Mathf.Clamp(inner * 1.42f, 0.72f, 1.22f);
            float boundaryRadius = Mathf.Max(1.2f, outer * 0.94f);
            GojoBlueMaterialLibrary materialLibrary =
                GojoBlueMaterialLibrary.GetOrCreate(transform.parent);

            baseDistortionStrength = impactCue ? 0.30f : 0.26f;
            float distortionWorldRadius =
                impactCue
                    ? orbDiameter * 1.95f
                    : Mathf.Max(orbDiameter * 2.15f, boundaryRadius * 0.98f);
            Renderer distortionRenderer = CreateDistortionShell(
                materialLibrary.DistortionMaterial,
                distortionWorldRadius
            );

            coreRoot = new GameObject("BlueCoreRoot").transform;
            coreRoot.SetParent(transform, false);
            CreateEnergySphere(
                coreRoot,
                "DenseEnergyBody",
                BlueEnergyLayerKind.DenseBody,
                orbDiameter * 0.74f,
                40,
                materialLibrary.EnergyMaterial,
                new Color(0.001f, 0.004f, 0.055f, 1f),
                new Color(0.004f, 0.035f, 0.30f, 1f),
                new Color(0.018f, 0.28f, 0.92f, 1f),
                0.99f,
                0f,
                3.8f,
                0.22f,
                11.5f,
                -0.48f,
                3.4f,
                0.16f,
                0.92f,
                7.2f,
                0.08f,
                0.17f
            );
            CreateEnergySphere(
                coreRoot,
                "FresnelEnergyShell",
                BlueEnergyLayerKind.FresnelShell,
                orbDiameter * 1.04f,
                41,
                materialLibrary.EnergyMaterial,
                new Color(0.002f, 0.025f, 0.24f, 1f),
                new Color(0.018f, 0.24f, 0.96f, 1f),
                new Color(0.62f, 0.97f, 1f, 1f),
                0.64f,
                1f,
                4.9f,
                -0.31f,
                15.0f,
                0.62f,
                2.25f,
                0.42f,
                1.68f,
                5.2f,
                0.12f,
                0.49f
            );
            CreateEnergySphere(
                coreRoot,
                "ThinOuterDistortionShell",
                BlueEnergyLayerKind.OuterShell,
                orbDiameter * 1.58f,
                42,
                materialLibrary.EnergyMaterial,
                new Color(0.002f, 0.028f, 0.18f, 1f),
                new Color(0.02f, 0.30f, 0.82f, 1f),
                new Color(0.28f, 0.94f, 1f, 1f),
                0.24f,
                2f,
                6.2f,
                0.17f,
                19.0f,
                -0.78f,
                1.55f,
                0.63f,
                1.02f,
                3.8f,
                0.14f,
                0.83f
            );
            CreateEnergySphere(
                coreRoot,
                "WhiteHotCompressionPoint",
                BlueEnergyLayerKind.CompressionPoint,
                orbDiameter * 0.18f,
                43,
                materialLibrary.EnergyMaterial,
                new Color(0.36f, 0.88f, 1f, 1f),
                new Color(0.78f, 0.98f, 1f, 1f),
                Color.white,
                1f,
                0f,
                2.6f,
                0.36f,
                8.0f,
                -0.64f,
                1.2f,
                0.04f,
                2.45f,
                10.5f,
                0.05f,
                0.31f
            );

            BuildCorona(orbDiameter, materialLibrary.ParticleMaterial);
            BuildSpiralFlow(boundaryRadius, materialLibrary.ParticleMaterial);
            BuildEnvironmentSuction(boundaryRadius, materialLibrary.ParticleMaterial);
            BuildSuctionWind(boundaryRadius, materialLibrary.ParticleMaterial);
            BuildConvergenceField(outer, boundaryRadius);
            BuildCompressionLight(orbDiameter);

            distortionSource = gameObject.AddComponent<GojoBlueDistortionSource>();
            distortionSource.Configure(
                distortionWorldRadius,
                baseDistortionStrength * distortionWeight,
                impactCue,
                distortionRenderer
            );
            ApplyEnergyLayers(1f);
            ApplyParticleLayers(1f);
            ApplyConvergenceArcFade(1f);
            ApplyEnvironmentEmissionRates();
        }

        private Renderer CreateDistortionShell(
            Material sharedMaterial,
            float worldRadius
        )
        {
            if (sharedMaterial == null || worldRadius <= 0f)
            {
                return null;
            }

            GameObject shell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shell.name = "BlueScreenDistortionShell";
            shell.transform.SetParent(transform, false);
            shell.transform.localScale = Vector3.one * (worldRadius * 2f);
            Collider collider = shell.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                Destroy(collider);
            }

            Renderer renderer = shell.GetComponent<Renderer>();
            if (renderer == null)
            {
                Destroy(shell);
                return null;
            }

            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sortingOrder = 30;
            renderer.sharedMaterial = sharedMaterial;
            return renderer;
        }

        private void CreateEnergySphere(
            Transform parent,
            string name,
            BlueEnergyLayerKind layerKind,
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
                    layerKind,
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

        private void BuildCorona(float orbDiameter, Material particleMaterial)
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
                20f,
                particleMaterial
            );
            ParticleSystem.MainModule main = corona.main;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            ParticleSystem.ShapeModule shape = corona.shape;
            shape.radiusThickness = 0.18f;
            BindParticleLayer(
                corona,
                particleMaterial,
                BlueParticleLayerKind.Corona,
                BlueParticleMaskMode.BrokenWisp,
                1.30f,
                0.24f
            );
            Track(corona);
        }

        private void BuildSpiralFlow(float boundaryRadius, Material particleMaterial)
        {
            ParticleSystem fastSpiral = ProductionSignatureVfxFactory.CreateParticleSystem(
                transform,
                "FastClockwiseInwardStreaks",
                new Color(0.025f, 0.38f, 1f, 0.72f),
                RuntimeMaterials,
                MaterialColors,
                true,
                Duration,
                impactCue ? 0.16f : 0.30f,
                impactCue ? 0.26f : 0.48f,
                impactCue ? -10.5f : -7.2f,
                impactCue ? -7.4f : -4.4f,
                0.022f,
                0.060f,
                ParticleSystemShapeType.Sphere,
                boundaryRadius,
                true,
                ParticleSystemSimulationSpace.Local,
                impactCue ? 32 : 20,
                impactCue ? 22f : 56f,
                particleMaterial
            );
            fastSpiral.transform.localPosition = Vector3.up * 0.13f;
            ParticleSystem.ShapeModule fastShape = fastSpiral.shape;
            fastShape.radiusThickness = 0.06f;
            ParticleSystem.VelocityOverLifetimeModule fastVelocity =
                fastSpiral.velocityOverLifetime;
            fastVelocity.enabled = true;
            fastVelocity.space = ParticleSystemSimulationSpace.Local;
            // Unity requires all orbital velocity axes to use the same
            // MinMaxCurve mode. Keep X/Y/Z in TwoConstants mode even when an axis is zero.
            fastVelocity.orbitalX = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);
            fastVelocity.orbitalY = new ParticleSystem.MinMaxCurve(4.2f, 6.4f);
            fastVelocity.orbitalZ = new ParticleSystem.MinMaxCurve(0f, 0f);
            ParticleSystem.NoiseModule fastNoise = fastSpiral.noise;
            fastNoise.enabled = true;
            fastNoise.strength = 0.10f;
            fastNoise.frequency = 0.28f;
            ConfigureInwardSize(fastSpiral, 0.04f);
            ParticleSystemRenderer fastRenderer =
                fastSpiral.GetComponent<ParticleSystemRenderer>();
            if (fastRenderer != null)
            {
                fastRenderer.velocityScale = 0.035f;
                fastRenderer.lengthScale = 0.52f;
            }
            BindParticleLayer(
                fastSpiral,
                particleMaterial,
                BlueParticleLayerKind.FastSpiral,
                BlueParticleMaskMode.TaperedStreak,
                1.36f,
                0.16f
            );
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
                impactCue ? 14f : 32f,
                particleMaterial
            );
            slowSpiral.transform.localPosition = Vector3.down * 0.11f;
            ParticleSystem.ShapeModule slowShape = slowSpiral.shape;
            slowShape.radiusThickness = 0.14f;
            ParticleSystem.VelocityOverLifetimeModule slowVelocity =
                slowSpiral.velocityOverLifetime;
            slowVelocity.enabled = true;
            slowVelocity.space = ParticleSystemSimulationSpace.Local;
            // Match the orbital curve mode on every axis (TwoConstants).
            slowVelocity.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
            slowVelocity.orbitalY = new ParticleSystem.MinMaxCurve(-3.8f, -2.2f);
            slowVelocity.orbitalZ = new ParticleSystem.MinMaxCurve(-0.22f, 0.22f);
            ParticleSystem.NoiseModule slowNoise = slowSpiral.noise;
            slowNoise.enabled = true;
            slowNoise.strength = 0.055f;
            slowNoise.frequency = 0.20f;
            ConfigureInwardSize(slowSpiral, 0.08f);
            BindParticleLayer(
                slowSpiral,
                particleMaterial,
                BlueParticleLayerKind.SlowSpiral,
                BlueParticleMaskMode.SoftMote,
                1.16f,
                0.10f
            );
            Track(slowSpiral);
        }

        private void BindParticleLayer(
            ParticleSystem system,
            Material sharedMaterial,
            BlueParticleLayerKind layerKind,
            BlueParticleMaskMode maskMode,
            float emission,
            float breakup
        )
        {
            if (system == null || sharedMaterial == null)
            {
                return;
            }

            ParticleSystemRenderer renderer =
                system.GetComponent<ParticleSystemRenderer>();
            if (renderer == null)
            {
                return;
            }

            renderer.sharedMaterial = sharedMaterial;
            ParticleLayerBinding binding = new ParticleLayerBinding(
                renderer,
                layerKind,
                maskMode,
                emission,
                breakup
            );
            binding.Apply(ResolveParticleLayerWeight(layerKind));
            particleLayers.Add(binding);
        }

        private void BuildEnvironmentSuction(
            float boundaryRadius,
            Material particleMaterial
        )
        {
            groundDustSuction = ProductionSignatureVfxFactory.CreateParticleSystem(
                transform,
                "GroundDustSuction",
                new Color(0.15f, 0.19f, 0.24f, 0.58f),
                RuntimeMaterials,
                MaterialColors,
                !impactCue,
                Duration,
                impactCue ? 0.18f : 0.62f,
                impactCue ? 0.30f : 0.90f,
                0f,
                0f,
                0.075f,
                0.180f,
                ParticleSystemShapeType.Sphere,
                boundaryRadius * 1.28f,
                false,
                ParticleSystemSimulationSpace.Local,
                impactCue ? 20 : 8,
                impactCue ? 0f : 18f,
                particleMaterial
            );
            ParticleSystem.ShapeModule dustShape = groundDustSuction.shape;
            dustShape.position = Vector3.down
                * Mathf.Clamp(boundaryRadius * 0.065f, 0.22f, 0.30f);
            dustShape.radiusThickness = 0.16f;
            dustShape.scale = new Vector3(1f, 0.065f, 1f);
            ParticleSystem.MainModule dustMain = groundDustSuction.main;
            dustMain.startColor = Color.white;
            dustMain.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            ParticleSystem.VelocityOverLifetimeModule dustVelocity =
                groundDustSuction.velocityOverLifetime;
            dustVelocity.enabled = true;
            dustVelocity.space = ParticleSystemSimulationSpace.Local;
            // Unity requires the X/Y/Z velocity curves to use the same MinMaxCurve mode.
            // Keep the unused axes in TwoConstants mode to match Y.
            dustVelocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
            dustVelocity.y = new ParticleSystem.MinMaxCurve(
                impactCue ? 0.55f : 0.34f,
                impactCue ? 1.00f : 0.76f
            );
            dustVelocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            dustVelocity.radial = CreateAcceleratingInwardCurve(
                impactCue ? 5.2f : 1.35f,
                impactCue ? 7.4f : 18.0f
            );
            dustVelocity.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
            dustVelocity.orbitalY = new ParticleSystem.MinMaxCurve(
                impactCue ? 2.2f : 0.75f,
                impactCue ? 3.4f : 1.45f
            );
            dustVelocity.orbitalZ = new ParticleSystem.MinMaxCurve(0f, 0f);
            ParticleSystem.NoiseModule dustNoise = groundDustSuction.noise;
            dustNoise.enabled = true;
            dustNoise.strength = impactCue ? 0.075f : 0.045f;
            dustNoise.frequency = 0.24f;
            ConfigureInwardSize(groundDustSuction, 0.015f);
            BindParticleLayer(
                groundDustSuction,
                particleMaterial,
                BlueParticleLayerKind.GroundDust,
                BlueParticleMaskMode.SoftMote,
                0.94f,
                0.16f
            );
            Track(groundDustSuction);

            darkDebrisFragments = ProductionSignatureVfxFactory.CreateParticleSystem(
                transform,
                "DarkDebrisFragments",
                new Color(0.075f, 0.12f, 0.22f, 0.90f),
                RuntimeMaterials,
                MaterialColors,
                !impactCue,
                Duration,
                impactCue ? 0.18f : 0.58f,
                impactCue ? 0.30f : 0.86f,
                0f,
                0f,
                0.080f,
                0.220f,
                ParticleSystemShapeType.Sphere,
                boundaryRadius * 1.34f,
                false,
                ParticleSystemSimulationSpace.Local,
                impactCue ? 12 : 5,
                impactCue ? 0f : 10f,
                particleMaterial
            );
            ParticleSystem.ShapeModule debrisShape = darkDebrisFragments.shape;
            debrisShape.position = Vector3.down
                * Mathf.Clamp(boundaryRadius * 0.035f, 0.08f, 0.18f);
            debrisShape.radiusThickness = 0.12f;
            debrisShape.scale = new Vector3(1f, 0.28f, 1f);
            ParticleSystem.MainModule debrisMain = darkDebrisFragments.main;
            debrisMain.startColor = Color.white;
            debrisMain.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            ParticleSystem.VelocityOverLifetimeModule debrisVelocity =
                darkDebrisFragments.velocityOverLifetime;
            debrisVelocity.enabled = true;
            debrisVelocity.space = ParticleSystemSimulationSpace.Local;
            // Match the linear velocity curve mode on every axis (TwoConstants).
            debrisVelocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
            debrisVelocity.y = new ParticleSystem.MinMaxCurve(
                impactCue ? 0.25f : 0.16f,
                impactCue ? 0.62f : 0.48f
            );
            debrisVelocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            debrisVelocity.radial = CreateAcceleratingInwardCurve(
                impactCue ? 6.2f : 1.45f,
                impactCue ? 9.2f : 21.0f
            );
            debrisVelocity.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
            debrisVelocity.orbitalY = new ParticleSystem.MinMaxCurve(
                impactCue ? 2.8f : 1.0f,
                impactCue ? 4.2f : 1.9f
            );
            debrisVelocity.orbitalZ = new ParticleSystem.MinMaxCurve(0f, 0f);
            ParticleSystem.NoiseModule debrisNoise = darkDebrisFragments.noise;
            debrisNoise.enabled = true;
            debrisNoise.strength = impactCue ? 0.065f : 0.035f;
            debrisNoise.frequency = 0.20f;
            ConfigureInwardSize(darkDebrisFragments, 0.01f);
            BindParticleLayer(
                darkDebrisFragments,
                particleMaterial,
                BlueParticleLayerKind.DarkDebris,
                BlueParticleMaskMode.DarkFragment,
                0.90f,
                0.20f
            );
            Track(darkDebrisFragments);

            BuildTidalDebrisTrails(boundaryRadius, particleMaterial);
        }

        private void BuildTidalDebrisTrails(
            float boundaryRadius,
            Material particleMaterial
        )
        {
            tidalDebrisTrails = ProductionSignatureVfxFactory.CreateParticleSystem(
                transform,
                "TidalDebrisTrails",
                new Color(0.045f, 0.10f, 0.20f, 0.86f),
                RuntimeMaterials,
                MaterialColors,
                !impactCue,
                Duration,
                impactCue ? 0.16f : 0.62f,
                impactCue ? 0.28f : 0.90f,
                0f,
                0f,
                0.055f,
                0.130f,
                ParticleSystemShapeType.Sphere,
                boundaryRadius * 1.38f,
                true,
                ParticleSystemSimulationSpace.Local,
                impactCue ? 7 : 4,
                impactCue ? 0f : 7f,
                particleMaterial
            );
            tidalDebrisTrails.transform.localPosition = Vector3.up * 0.08f;
            ParticleSystem.ShapeModule tidalShape = tidalDebrisTrails.shape;
            tidalShape.radiusThickness = 0.08f;
            tidalShape.scale = new Vector3(1f, 0.32f, 1f);
            ParticleSystem.MainModule tidalMain = tidalDebrisTrails.main;
            tidalMain.startColor = Color.white;
            tidalMain.startRotation = new ParticleSystem.MinMaxCurve(
                -0.28f,
                0.28f
            );
            ParticleSystem.VelocityOverLifetimeModule tidalVelocity =
                tidalDebrisTrails.velocityOverLifetime;
            tidalVelocity.enabled = true;
            tidalVelocity.space = ParticleSystemSimulationSpace.Local;
            tidalVelocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
            tidalVelocity.y = new ParticleSystem.MinMaxCurve(0.12f, 0.58f);
            tidalVelocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            tidalVelocity.radial = CreateAcceleratingInwardCurve(1.35f, 24.0f);
            tidalVelocity.orbitalX = new ParticleSystem.MinMaxCurve(-0.10f, 0.10f);
            tidalVelocity.orbitalY = new ParticleSystem.MinMaxCurve(0.45f, 1.15f);
            tidalVelocity.orbitalZ = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
            ParticleSystem.NoiseModule tidalNoise = tidalDebrisTrails.noise;
            tidalNoise.enabled = true;
            tidalNoise.strength = 0.025f;
            tidalNoise.frequency = 0.16f;
            ConfigureInwardSize(tidalDebrisTrails, 0.004f);
            ParticleSystemRenderer tidalRenderer =
                tidalDebrisTrails.GetComponent<ParticleSystemRenderer>();
            if (tidalRenderer != null)
            {
                tidalRenderer.velocityScale = 0.12f;
                tidalRenderer.lengthScale = 1.65f;
                tidalRenderer.sortingOrder = 31;
            }
            BindParticleLayer(
                tidalDebrisTrails,
                particleMaterial,
                BlueParticleLayerKind.TidalDebris,
                BlueParticleMaskMode.DarkFragment,
                0.72f,
                0.18f
            );
            Track(tidalDebrisTrails);
        }

        private void BuildSuctionWind(
            float boundaryRadius,
            Material particleMaterial
        )
        {
            airflowSuctionWisps = ProductionSignatureVfxFactory.CreateParticleSystem(
                transform,
                "AirflowSuctionWisps",
                new Color(0.30f, 0.54f, 0.68f, 0.34f),
                RuntimeMaterials,
                MaterialColors,
                !impactCue,
                Duration,
                impactCue ? 0.16f : 0.60f,
                impactCue ? 0.28f : 0.90f,
                0f,
                0f,
                0.120f,
                0.280f,
                ParticleSystemShapeType.Sphere,
                boundaryRadius * 1.40f,
                true,
                ParticleSystemSimulationSpace.Local,
                impactCue ? 14 : 5,
                impactCue ? 0f : 18f,
                particleMaterial
            );
            airflowSuctionWisps.transform.localPosition = Vector3.up * 0.06f;
            ParticleSystem.ShapeModule airflowShape = airflowSuctionWisps.shape;
            airflowShape.radiusThickness = 0.20f;
            airflowShape.scale = new Vector3(1f, 0.48f, 1f);
            ParticleSystem.MainModule airflowMain = airflowSuctionWisps.main;
            airflowMain.startColor = Color.white;
            airflowMain.startRotation = new ParticleSystem.MinMaxCurve(
                -0.18f,
                0.18f
            );
            ParticleSystem.VelocityOverLifetimeModule airflowVelocity =
                airflowSuctionWisps.velocityOverLifetime;
            airflowVelocity.enabled = true;
            airflowVelocity.space = ParticleSystemSimulationSpace.Local;
            airflowVelocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
            airflowVelocity.y = new ParticleSystem.MinMaxCurve(
                impactCue ? 0.12f : 0.05f,
                impactCue ? 0.34f : 0.24f
            );
            airflowVelocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            airflowVelocity.radial = CreateAcceleratingInwardCurve(
                impactCue ? 5.0f : 1.10f,
                impactCue ? 8.2f : 18.5f
            );
            airflowVelocity.orbitalX = new ParticleSystem.MinMaxCurve(-0.18f, 0.18f);
            airflowVelocity.orbitalY = new ParticleSystem.MinMaxCurve(
                impactCue ? 2.0f : 0.75f,
                impactCue ? 3.0f : 1.45f
            );
            airflowVelocity.orbitalZ = new ParticleSystem.MinMaxCurve(-0.14f, 0.14f);
            ParticleSystem.NoiseModule airflowNoise = airflowSuctionWisps.noise;
            airflowNoise.enabled = true;
            airflowNoise.strength = impactCue ? 0.045f : 0.025f;
            airflowNoise.frequency = 0.18f;
            ConfigureInwardSize(airflowSuctionWisps, 0.05f);
            ParticleSystemRenderer airflowRenderer =
                airflowSuctionWisps.GetComponent<ParticleSystemRenderer>();
            if (airflowRenderer != null)
            {
                airflowRenderer.velocityScale = 0.018f;
                airflowRenderer.lengthScale = 0.54f;
                airflowRenderer.sortingOrder = 32;
            }
            BindParticleLayer(
                airflowSuctionWisps,
                particleMaterial,
                BlueParticleLayerKind.AirflowWisp,
                BlueParticleMaskMode.AirflowWisp,
                0.62f,
                0.24f
            );
            Track(airflowSuctionWisps);

            windRibbonConvergence = ProductionSignatureVfxFactory.CreateParticleSystem(
                transform,
                "WindRibbonConvergence",
                new Color(0.34f, 0.62f, 0.78f, 0.26f),
                RuntimeMaterials,
                MaterialColors,
                !impactCue,
                Duration,
                impactCue ? 0.18f : 0.58f,
                impactCue ? 0.30f : 0.86f,
                0f,
                0f,
                0.145f,
                0.320f,
                ParticleSystemShapeType.Sphere,
                boundaryRadius * 1.52f,
                true,
                ParticleSystemSimulationSpace.Local,
                impactCue ? 8 : 3,
                impactCue ? 0f : 4.5f,
                particleMaterial
            );
            windRibbonConvergence.transform.localPosition = Vector3.up * 0.12f;
            ParticleSystem.ShapeModule ribbonShape = windRibbonConvergence.shape;
            ribbonShape.radiusThickness = 0.10f;
            ribbonShape.scale = new Vector3(1f, 0.62f, 1f);
            ParticleSystem.MainModule ribbonMain = windRibbonConvergence.main;
            ribbonMain.startColor = Color.white;
            ribbonMain.startRotation = new ParticleSystem.MinMaxCurve(-0.12f, 0.12f);
            ParticleSystem.VelocityOverLifetimeModule ribbonVelocity =
                windRibbonConvergence.velocityOverLifetime;
            ribbonVelocity.enabled = true;
            ribbonVelocity.space = ParticleSystemSimulationSpace.Local;
            ribbonVelocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
            ribbonVelocity.y = new ParticleSystem.MinMaxCurve(
                impactCue ? -0.10f : -0.08f,
                impactCue ? 0.24f : 0.18f
            );
            ribbonVelocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            ribbonVelocity.radial = CreateAcceleratingInwardCurve(
                impactCue ? 5.8f : 1.40f,
                impactCue ? 9.0f : 20.0f
            );
            ribbonVelocity.orbitalX = new ParticleSystem.MinMaxCurve(-0.28f, 0.28f);
            ribbonVelocity.orbitalY = new ParticleSystem.MinMaxCurve(
                impactCue ? 2.2f : 0.95f,
                impactCue ? 3.2f : 1.75f
            );
            ribbonVelocity.orbitalZ = new ParticleSystem.MinMaxCurve(-0.22f, 0.22f);
            ParticleSystem.NoiseModule ribbonNoise = windRibbonConvergence.noise;
            ribbonNoise.enabled = true;
            ribbonNoise.strength = impactCue ? 0.038f : 0.020f;
            ribbonNoise.frequency = 0.16f;
            ConfigureInwardSize(windRibbonConvergence, 0.035f);
            ParticleSystemRenderer ribbonRenderer =
                windRibbonConvergence.GetComponent<ParticleSystemRenderer>();
            if (ribbonRenderer != null)
            {
                ribbonRenderer.velocityScale = 0.012f;
                ribbonRenderer.lengthScale = 0.68f;
                ribbonRenderer.sortingOrder = 33;
            }
            BindParticleLayer(
                windRibbonConvergence,
                particleMaterial,
                BlueParticleLayerKind.WindRibbon,
                BlueParticleMaskMode.WindRibbon,
                0.46f,
                0.30f
            );
            Track(windRibbonConvergence);
        }

        private static ParticleSystem.MinMaxCurve CreateAcceleratingInwardCurve(
            float initialSpeed,
            float finalSpeed
        )
        {
            float safeFinalSpeed = Mathf.Max(0.01f, finalSpeed);
            float initialRatio = Mathf.Clamp01(initialSpeed / safeFinalSpeed);
            float middleRatio = Mathf.Lerp(initialRatio, 1f, 0.24f);
            float nearCoreRatio = Mathf.Lerp(initialRatio, 1f, 0.72f);
            return new ParticleSystem.MinMaxCurve(
                safeFinalSpeed,
                new AnimationCurve(
                    new Keyframe(0f, -initialRatio),
                    new Keyframe(0.58f, -middleRatio),
                    new Keyframe(0.86f, -nearCoreRatio),
                    new Keyframe(1f, -1f)
                )
            );
        }

        private static void ConfigureInwardSize(ParticleSystem system, float finalSize)
        {
            ParticleSystem.SizeOverLifetimeModule size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(
                1f,
                new AnimationCurve(
                    new Keyframe(0f, 0.55f),
                    new Keyframe(0.16f, 1f),
                    new Keyframe(0.68f, 0.74f),
                    new Keyframe(0.82f, 0.34f),
                    new Keyframe(0.91f, 0.10f),
                    new Keyframe(0.97f, 0.025f),
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
            compressionLight.intensity = baseLightIntensity * lightWeight;
            compressionLight.shadows = LightShadows.None;
        }

        protected override void Tick(float elapsed, float normalized, float deltaTime)
        {
            UpdatePresentationTiming(normalized);
            fieldPulseAccent = impactCue
                ? 0f
                : EvaluateFieldPulseAccent(normalized);
            shaderCompression = impactCue
                ? Mathf.Lerp(0.25f, 1f, impactCollapse)
                : 0.42f
                    + Mathf.Sin(elapsed * 8.6f) * 0.055f
                    + fieldPulseAccent * 0.38f;
            lightPulse = 0.88f + Mathf.Sin(elapsed * 10.2f) * 0.12f;

            float environmentSimulationSpeed = impactCue
                ? Mathf.Lerp(1.05f, 1.65f, impactCollapse)
                : 1f
                    + SmoothRamp(0.76f, 1f, normalized) * 0.24f
                    + fieldPulseAccent * 0.62f;
            SetSimulationSpeed(groundDustSuction, environmentSimulationSpeed);
            SetSimulationSpeed(darkDebrisFragments, environmentSimulationSpeed);
            SetSimulationSpeed(tidalDebrisTrails, environmentSimulationSpeed);
            SetSimulationSpeed(airflowSuctionWisps, environmentSimulationSpeed);
            SetSimulationSpeed(windRibbonConvergence, environmentSimulationSpeed);
            ApplyEnvironmentEmissionRates();

            if (coreRoot != null)
            {
                float scale = impactCue
                    ? Mathf.Lerp(1.24f, 0.58f, impactCollapse)
                    : 1f
                        + Mathf.Sin(elapsed * 8.6f) * 0.018f
                        - fieldPulseAccent * 0.18f;
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
            ApplyConvergenceArcFade(fade);
            if (compressionLight != null)
            {
                compressionLight.intensity =
                    baseLightIntensity
                    * lightPulse
                    * (1f + fieldPulseAccent * 0.44f)
                    * lightWeight
                    * fade;
            }
            if (distortionSource != null)
            {
                float compressionBoost = impactCue
                    ? Mathf.Lerp(1.08f, 1.28f, impactCollapse)
                    : 0.90f
                        + shaderCompression * 0.24f
                        + fieldPulseAccent * 0.38f;
                distortionSource.SetStrength(
                    baseDistortionStrength * compressionBoost * distortionWeight * fade
                );
            }
            ApplyParticleLayers(fade);
        }

        private static void SetSimulationSpeed(ParticleSystem system, float speed)
        {
            if (system == null)
            {
                return;
            }

            ParticleSystem.MainModule main = system.main;
            main.simulationSpeed = speed;
        }

        private void ApplyEnergyLayers(float fade)
        {
            foreach (EnergyLayerBinding layer in energyLayers)
            {
                layer?.Apply(
                    fade * ResolveEnergyLayerWeight(layer.Kind),
                    shaderCompression
                );
            }
        }

        private void ApplyParticleLayers(float fade)
        {
            foreach (ParticleLayerBinding layer in particleLayers)
            {
                layer?.Apply(fade * ResolveParticleLayerWeight(layer.Kind));
            }
        }

        private void ApplyConvergenceArcFade(float fade)
        {
            float fieldAlphaScale = impactCue ? 1f : 0.80f;
            foreach (ConvergenceArcBinding arc in convergenceArcs)
            {
                arc?.ApplyFade(fade * convergenceArcWeight * fieldAlphaScale);
            }
        }

        private void ApplyEnvironmentEmissionRates()
        {
            SetEmissionRateMultiplier(
                groundDustSuction,
                impactCue ? 0f : groundDustWeight
            );
            SetEmissionRateMultiplier(
                darkDebrisFragments,
                impactCue ? 0f : darkDebrisWeight
            );
            SetEmissionRateMultiplier(
                tidalDebrisTrails,
                impactCue ? 0f : tidalDebrisWeight
            );
            SetEmissionRateMultiplier(
                airflowSuctionWisps,
                impactCue ? 0f : airflowWispWeight
            );
            SetEmissionRateMultiplier(
                windRibbonConvergence,
                impactCue ? 0f : windRibbonWeight
            );
        }

        private static void SetEmissionRateMultiplier(
            ParticleSystem system,
            float multiplier
        )
        {
            if (system == null)
            {
                return;
            }

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTimeMultiplier = Mathf.Clamp01(multiplier);
        }

        private void UpdatePresentationTiming(float normalized)
        {
            if (impactCue)
            {
                impactCollapse = SmoothRamp(0.20f, 0.75f, normalized);
                denseEnergyWeight = 1f;
                fresnelEnergyWeight = 1f;
                outerEnergyWeight = 1f;
                coronaWeight = 1f;
                fastSpiralWeight = 1f;
                slowSpiralWeight = 1f;
                groundDustWeight = 1f;
                darkDebrisWeight = 1f;
                tidalDebrisWeight = 1f;
                airflowWispWeight = 1f;
                windRibbonWeight = 1f;
                convergenceArcWeight = 1f;
                distortionWeight = Mathf.Lerp(
                    0.58f,
                    1f,
                    SmoothRamp(0f, 0.20f, normalized)
                );
                lightWeight = 1f;
                return;
            }

            impactCollapse = 0f;
            denseEnergyWeight = SmoothRamp(0f, 0.10f, normalized);
            fresnelEnergyWeight = SmoothRamp(0.08f, 0.22f, normalized);
            outerEnergyWeight = SmoothRamp(0.12f, 0.28f, normalized);
            coronaWeight = SmoothRamp(0.12f, 0.28f, normalized);
            groundDustWeight = SmoothRamp(0.18f, 0.34f, normalized);
            darkDebrisWeight = SmoothRamp(0.16f, 0.31f, normalized);
            tidalDebrisWeight = SmoothRamp(0.20f, 0.36f, normalized);
            airflowWispWeight = SmoothRamp(0.12f, 0.28f, normalized);
            slowSpiralWeight = SmoothRamp(0.24f, 0.42f, normalized);
            windRibbonWeight = SmoothRamp(0.18f, 0.38f, normalized);
            convergenceArcWeight = SmoothRamp(0.30f, 0.50f, normalized);
            fastSpiralWeight = SmoothRamp(0.42f, 0.58f, normalized);
            distortionWeight = SmoothRamp(0.08f, 0.22f, normalized);
            lightWeight = SmoothRamp(0.05f, 0.30f, normalized);
        }

        private float ResolveEnergyLayerWeight(BlueEnergyLayerKind layerKind)
        {
            return layerKind switch
            {
                BlueEnergyLayerKind.CompressionPoint => denseEnergyWeight,
                BlueEnergyLayerKind.DenseBody => denseEnergyWeight,
                BlueEnergyLayerKind.FresnelShell => fresnelEnergyWeight,
                BlueEnergyLayerKind.OuterShell => outerEnergyWeight,
                _ => 1f,
            };
        }

        private float ResolveParticleLayerWeight(BlueParticleLayerKind layerKind)
        {
            return layerKind switch
            {
                BlueParticleLayerKind.Corona => coronaWeight,
                BlueParticleLayerKind.FastSpiral => fastSpiralWeight,
                BlueParticleLayerKind.SlowSpiral => slowSpiralWeight,
                BlueParticleLayerKind.GroundDust => groundDustWeight,
                BlueParticleLayerKind.DarkDebris => darkDebrisWeight,
                BlueParticleLayerKind.TidalDebris => tidalDebrisWeight,
                BlueParticleLayerKind.AirflowWisp => airflowWispWeight,
                BlueParticleLayerKind.WindRibbon => windRibbonWeight,
                _ => 1f,
            };
        }

        private static float EvaluateFieldPulseAccent(float normalized)
        {
            float accent = 0f;
            for (
                int pulseIndex = 0;
                pulseIndex < GojoBluePulseSchedule.HitCount;
                pulseIndex++
            )
            {
                float pulseTime = GojoBluePulseSchedule.GetNormalizedTime(pulseIndex);
                float attack = SmoothRamp(
                    pulseTime - 0.026f,
                    pulseTime,
                    normalized
                );
                float release = 1f - SmoothRamp(
                    pulseTime,
                    pulseTime + 0.105f,
                    normalized
                );
                accent = Mathf.Max(accent, attack * release);
            }
            return accent;
        }

        private static float SmoothRamp(float start, float end, float normalized)
        {
            return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(start, end, normalized));
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
            float rateOverTime,
            Material materialOverride = null
        )
        {
            GameObject child = new GameObject(name, typeof(ParticleSystem));
            child.transform.SetParent(parent, false);
            ParticleSystem system = child.GetComponent<ParticleSystem>();
            // A ParticleSystem component can begin playing as soon as it is created on
            // an active GameObject. Stop and clear it before mutating duration/module
            // settings; Unity rejects duration changes while a system is already playing.
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

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
            Material material = materialOverride;
            if (material == null)
            {
                material = CreateMaterial(
                    color, materials, colors, true, true, 1.55f, CullMode.Off
                );
            }
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
