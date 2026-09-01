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
            ApplyMaterialFade(Mathf.Min(naturalFade, stopFade));

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

        private void ApplyMaterialFade(float fade)
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

    internal sealed class GojoBlueVfxInstance : GojoSignatureVfxInstance
    {
        private readonly List<Transform> convergenceSegments = new List<Transform>(4);
        private Transform coreRoot;
        private bool impactCue;

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
            float orbDiameter = Mathf.Clamp(inner * 1.35f, 0.68f, 1.15f);
            float boundaryRadius = Mathf.Max(1.2f, outer * 0.94f);

            coreRoot = new GameObject("BlueCoreRoot").transform;
            coreRoot.SetParent(transform, false);
            ProductionSignatureVfxFactory.CreateSphere(
                coreRoot, "DenseCoreSphere", Vector3.zero, orbDiameter * 0.72f,
                new Color(0.015f, 0.055f, 0.62f, 0.98f), RuntimeMaterials,
                MaterialColors, 1.45f
            );
            ProductionSignatureVfxFactory.CreateSphere(
                coreRoot, "ElectricBlueRim", Vector3.zero, orbDiameter,
                new Color(0.025f, 0.32f, 1f, 0.62f), RuntimeMaterials,
                MaterialColors, 1.70f
            );
            ProductionSignatureVfxFactory.CreateSphere(
                coreRoot, "CyanCompressionShell", Vector3.zero, orbDiameter * 1.34f,
                new Color(0.08f, 0.78f, 1f, 0.18f), RuntimeMaterials,
                MaterialColors, 1.25f
            );

            Track(ProductionSignatureVfxFactory.CreateParticleSystem(
                coreRoot, "BlueCorona", new Color(0.10f, 0.72f, 1f, 0.72f),
                RuntimeMaterials, MaterialColors, true, Duration, 0.10f, 0.20f,
                -0.28f, -0.08f, 0.025f, 0.055f, ParticleSystemShapeType.Sphere,
                orbDiameter * 0.68f, false, ParticleSystemSimulationSpace.Local, 10, 24f
            ));

            Transform boundary = new GameObject("ConvergenceBoundary").transform;
            boundary.SetParent(transform, false);
            boundary.localPosition = Vector3.down * 0.28f;
            for (int index = 0; index < 4; index++)
            {
                LineRenderer arc = ProductionSignatureVfxFactory.CreateArc(
                    boundary, $"InwardArc_{index + 1}", boundaryRadius,
                    index * 88f + 12f, index % 2 == 0 ? 76f : 52f,
                    Mathf.Clamp(outer * 0.012f, 0.035f, 0.065f),
                    new Color(0.04f, 0.54f, 1f, index % 2 == 0 ? 0.62f : 0.38f),
                    false, RuntimeMaterials, MaterialColors
                );
                convergenceSegments.Add(arc.transform);
            }

            ParticleSystem inward = ProductionSignatureVfxFactory.CreateParticleSystem(
                transform, "InwardParticles", new Color(0.04f, 0.48f, 1f, 0.78f),
                RuntimeMaterials, MaterialColors, true, Duration,
                impactCue ? 0.20f : 0.38f, impactCue ? 0.38f : 0.68f,
                impactCue ? -11f : -7.5f, impactCue ? -7f : -4.2f,
                0.035f, 0.075f, ParticleSystemShapeType.Sphere, boundaryRadius,
                true, ParticleSystemSimulationSpace.Local, impactCue ? 34 : 18,
                impactCue ? 20f : 58f
            );
            ParticleSystem.ShapeModule inwardShape = inward.shape;
            inwardShape.radiusThickness = 0.08f;
            ParticleSystem.SizeOverLifetimeModule inwardSize = inward.sizeOverLifetime;
            inwardSize.enabled = true;
            inwardSize.size = new ParticleSystem.MinMaxCurve(
                1f,
                new AnimationCurve(
                    new Keyframe(0f, 1f),
                    new Keyframe(0.72f, 0.72f),
                    new Keyframe(1f, 0.05f)
                )
            );
            Track(inward);

            ParticleSystem compression = ProductionSignatureVfxFactory.CreateParticleSystem(
                transform, "CompressionMotes", new Color(0.10f, 0.82f, 1f, 0.56f),
                RuntimeMaterials, MaterialColors, true, Duration, 0.32f, 0.55f,
                -4.2f, -2.2f, 0.025f, 0.050f, ParticleSystemShapeType.Sphere,
                boundaryRadius * 0.60f, false, ParticleSystemSimulationSpace.Local, 12, 30f
            );
            ParticleSystem.ShapeModule compressionShape = compression.shape;
            compressionShape.radiusThickness = 0.12f;
            Track(compression);
        }

        protected override void Tick(float elapsed, float normalized, float deltaTime)
        {
            if (coreRoot != null)
            {
                coreRoot.localScale = Vector3.one * (1f + Mathf.Sin(elapsed * 18f) * 0.045f);
                coreRoot.Rotate(Vector3.up, 55f * deltaTime, Space.Self);
            }
            for (int index = 0; index < convergenceSegments.Count; index++)
            {
                Transform segment = convergenceSegments[index];
                if (segment == null)
                {
                    continue;
                }
                float cycle = Mathf.Repeat(
                    elapsed * (impactCue ? 4.8f : 2.2f) + index * 0.23f,
                    1f
                );
                float scale = Mathf.Lerp(1f, 0.22f, Mathf.SmoothStep(0f, 1f, cycle));
                segment.localScale = Vector3.one * scale;
                segment.Rotate(
                    Vector3.up,
                    (index % 2 == 0 ? 34f : -26f) * deltaTime,
                    Space.Self
                );
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
