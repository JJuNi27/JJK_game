using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.Player
{
    [DefaultExecutionOrder(2000)]
    [DisallowMultipleComponent]
    public sealed class PrototypeHollowPurplePresentationRuntime : MonoBehaviour
    {
        private const string LegacyRootName = "HollowPurplePrototypeVisual";
        private const string TargetSceneName = "CombatMVP";
        private const float ScanInterval = 0.08f;

        private readonly Dictionary<int, TrackedSource> trackedSources =
            new Dictionary<int, TrackedSource>();
        private readonly List<int> staleSourceIds = new List<int>();
        private readonly List<OrbSequence> sequences = new List<OrbSequence>();

        private float nextScanAt;

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
                || FindFirstObjectByType<PrototypeHollowPurplePresentationRuntime>() != null
            )
            {
                return;
            }

            GameObject runner = new GameObject("PrototypeHollowPurplePresentationRuntime");
            runner.AddComponent<PrototypeHollowPurplePresentationRuntime>();
        }

        private void Update()
        {
            if (Time.unscaledTime >= nextScanAt)
            {
                nextScanAt = Time.unscaledTime + ScanInterval;
                RefreshTrackedSources();
            }

            UpdateTrackedSources();
            UpdateSequences();
        }

        private void OnDestroy()
        {
            foreach (OrbSequence sequence in sequences)
            {
                sequence?.Dispose();
            }
            sequences.Clear();
        }

        private void RefreshTrackedSources()
        {
            GojoTechniqueChainController[] controllers =
                FindObjectsByType<GojoTechniqueChainController>(FindObjectsSortMode.None);

            HashSet<int> liveIds = new HashSet<int>();
            foreach (GojoTechniqueChainController controller in controllers)
            {
                if (controller == null)
                {
                    continue;
                }

                int id = controller.GetInstanceID();
                liveIds.Add(id);
                if (!trackedSources.ContainsKey(id))
                {
                    trackedSources.Add(id, new TrackedSource(controller));
                }
            }

            staleSourceIds.Clear();
            foreach (KeyValuePair<int, TrackedSource> pair in trackedSources)
            {
                if (pair.Value == null || pair.Value.Controller == null || !liveIds.Contains(pair.Key))
                {
                    staleSourceIds.Add(pair.Key);
                }
            }

            foreach (int id in staleSourceIds)
            {
                trackedSources.Remove(id);
            }
        }

        private void UpdateTrackedSources()
        {
            foreach (TrackedSource tracked in trackedSources.Values)
            {
                if (tracked == null || tracked.Controller == null)
                {
                    continue;
                }

                if (tracked.SourceRoot == null)
                {
                    tracked.SourceRoot = tracked.Controller.transform.Find(LegacyRootName);
                    tracked.WasActive = false;
                }

                if (tracked.SourceRoot == null)
                {
                    continue;
                }

                DisableLegacyBeam(tracked.SourceRoot);

                bool sourceActive = tracked.SourceRoot.gameObject.activeInHierarchy;
                if (sourceActive && !tracked.WasActive)
                {
                    StartOrbSequence(tracked.Controller.transform);
                }

                tracked.WasActive = sourceActive;
            }
        }

        private static void DisableLegacyBeam(Transform sourceRoot)
        {
            LineRenderer[] lines = sourceRoot.GetComponentsInChildren<LineRenderer>(true);
            foreach (LineRenderer line in lines)
            {
                if (line != null)
                {
                    line.enabled = false;
                }
            }

            Light[] lights = sourceRoot.GetComponentsInChildren<Light>(true);
            foreach (Light light in lights)
            {
                if (light != null)
                {
                    light.enabled = false;
                }
            }
        }

        private void StartOrbSequence(Transform fighter)
        {
            OrbSequence sequence = CreateCanonicalOrbSequence(
                transform,
                fighter,
                Time.unscaledTime
            );
            if (sequence != null)
            {
                sequences.Add(sequence);
            }
        }

        public static OrbSequence CreateCanonicalOrbSequence(
            Transform runtimeRoot,
            Transform fighter,
            float startTime
        )
        {
            if (runtimeRoot == null || fighter == null)
            {
                return null;
            }

            Vector3 direction = fighter.forward;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = Vector3.forward;
            }
            direction.Normalize();

            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
            Vector3 start = fighter.position + Vector3.up * 1.05f + direction * 1.10f;
            return new OrbSequence(runtimeRoot, start, direction, right, startTime);
        }

        private void UpdateSequences()
        {
            for (int index = sequences.Count - 1; index >= 0; index--)
            {
                OrbSequence sequence = sequences[index];
                if (sequence == null)
                {
                    sequences.RemoveAt(index);
                    continue;
                }

                if (sequence.Update(Time.unscaledTime, Time.unscaledDeltaTime))
                {
                    continue;
                }

                sequence.Dispose();
                sequences.RemoveAt(index);
            }
        }

        private sealed class TrackedSource
        {
            public GojoTechniqueChainController Controller { get; }
            public Transform SourceRoot { get; set; }
            public bool WasActive { get; set; }

            public TrackedSource(GojoTechniqueChainController controller)
            {
                Controller = controller;
            }
        }

        public sealed class OrbSequence
        {
            private const float MergeDuration = 0.24f;
            private const float LaunchDuration = 0.78f;
            private const float TravelDistance = 18f;

            private readonly GameObject root;
            private readonly Transform blueOrb;
            private readonly Transform redOrb;
            private readonly Transform purpleOrb;
            private readonly Transform purpleCore;
            private readonly Light purpleLight;
            private readonly List<Material> runtimeMaterials = new List<Material>();
            private readonly List<Color> materialColors = new List<Color>();
            private readonly ParticleSystem blueInward;
            private readonly ParticleSystem redOutward;
            private readonly ParticleSystem purpleFragments;
            private readonly ParticleSystem purpleTrail;
            private readonly Vector3 start;
            private readonly Vector3 direction;
            private readonly Vector3 right;
            private readonly float startedAt;

            private bool launched;

            public OrbSequence(
                Transform runtimeRoot,
                Vector3 sequenceStart,
                Vector3 sequenceDirection,
                Vector3 sequenceRight,
                float startTime
            )
            {
                start = sequenceStart;
                direction = sequenceDirection;
                right = sequenceRight;
                startedAt = startTime;

                root = new GameObject("HollowPurpleCanonicalOrbSequence");
                root.transform.SetParent(runtimeRoot, true);

                blueOrb = new GameObject("HollowPurpleBlueOrbRoot").transform;
                blueOrb.SetParent(root.transform, true);
                ProductionSignatureVfxFactory.CreateSphere(
                    blueOrb, "BlueDenseCore", Vector3.zero, 0.72f,
                    new Color(0.015f, 0.07f, 0.68f, 0.99f), runtimeMaterials,
                    materialColors, 1.45f
                );
                ProductionSignatureVfxFactory.CreateSphere(
                    blueOrb, "BlueCyanRim", Vector3.zero, 1.02f,
                    new Color(0.035f, 0.42f, 1f, 0.62f), runtimeMaterials,
                    materialColors, 1.70f
                );
                ProductionSignatureVfxFactory.CreateSphere(
                    blueOrb, "BlueOuterShell", Vector3.zero, 1.28f,
                    new Color(0.08f, 0.82f, 1f, 0.16f), runtimeMaterials,
                    materialColors, 1.25f
                );
                blueInward = ProductionSignatureVfxFactory.CreateParticleSystem(
                    blueOrb, "BlueMergeInwardMotes", new Color(0.08f, 0.62f, 1f, 0.75f),
                    runtimeMaterials, materialColors, true, MergeDuration, 0.10f, 0.18f,
                    -5.2f, -3.2f, 0.025f, 0.055f, ParticleSystemShapeType.Sphere,
                    1.18f, true, ParticleSystemSimulationSpace.Local, 14, 34f
                );
                ParticleSystem.ShapeModule blueShape = blueInward.shape;
                blueShape.radiusThickness = 0.08f;

                redOrb = new GameObject("HollowPurpleRedOrbRoot").transform;
                redOrb.SetParent(root.transform, true);
                ProductionSignatureVfxFactory.CreateSphere(
                    redOrb, "RedDenseCore", Vector3.zero, 0.72f,
                    new Color(0.58f, 0.006f, 0.022f, 0.99f), runtimeMaterials,
                    materialColors, 1.50f
                );
                ProductionSignatureVfxFactory.CreateSphere(
                    redOrb, "RedCrimsonRim", Vector3.zero, 1.02f,
                    new Color(1f, 0.025f, 0.045f, 0.64f), runtimeMaterials,
                    materialColors, 1.72f
                );
                ProductionSignatureVfxFactory.CreateSphere(
                    redOrb, "RedUnstableShell", Vector3.zero, 1.30f,
                    new Color(1f, 0.12f, 0.04f, 0.15f), runtimeMaterials,
                    materialColors, 1.20f
                );
                redOutward = ProductionSignatureVfxFactory.CreateParticleSystem(
                    redOrb, "RedMergeOutwardFragments", new Color(1f, 0.035f, 0.055f, 0.72f),
                    runtimeMaterials, materialColors, true, MergeDuration, 0.08f, 0.16f,
                    1.8f, 4.6f, 0.025f, 0.055f, ParticleSystemShapeType.Sphere,
                    0.48f, true, ParticleSystemSimulationSpace.Local, 12, 32f
                );

                purpleOrb = new GameObject("HollowPurpleDenseBody").transform;
                purpleOrb.SetParent(root.transform, true);
                purpleOrb.rotation = Quaternion.LookRotation(direction, Vector3.up);
                purpleCore = ProductionSignatureVfxFactory.CreateSphere(
                    purpleOrb, "VioletCoreSphere", Vector3.zero, 2.35f,
                    new Color(0.32f, 0.008f, 0.62f, 0.99f), runtimeMaterials,
                    materialColors, 1.55f
                );
                ProductionSignatureVfxFactory.CreateSphere(
                    purpleOrb, "DeepPurpleOuterShell", Vector3.zero, 3.15f,
                    new Color(0.54f, 0.018f, 0.92f, 0.66f), runtimeMaterials,
                    materialColors, 1.75f
                );
                ProductionSignatureVfxFactory.CreateSphere(
                    purpleOrb, "MagentaHighlight", new Vector3(0.38f, 0.28f, -0.22f),
                    0.88f, new Color(1f, 0.16f, 0.76f, 0.52f), runtimeMaterials,
                    materialColors, 1.60f
                );
                ProductionSignatureVfxFactory.CreateSphere(
                    purpleOrb, "RestrainedEnergyShell", Vector3.zero, 3.72f,
                    new Color(0.72f, 0.12f, 1f, 0.12f), runtimeMaterials,
                    materialColors, 1.25f
                );
                purpleFragments = ProductionSignatureVfxFactory.CreateParticleSystem(
                    purpleOrb, "PurpleOrbitFragments", new Color(0.86f, 0.18f, 1f, 0.62f),
                    runtimeMaterials, materialColors, true, LaunchDuration, 0.12f, 0.24f,
                    0.18f, 0.65f, 0.035f, 0.075f, ParticleSystemShapeType.Sphere,
                    1.72f, false, ParticleSystemSimulationSpace.World, 16, 24f
                );
                purpleTrail = ProductionSignatureVfxFactory.CreateParticleSystem(
                    purpleOrb, "PurpleShortResidualTrail", new Color(0.48f, 0.025f, 0.82f, 0.58f),
                    runtimeMaterials, materialColors, true, LaunchDuration, 0.10f, 0.20f,
                    1.5f, 3.8f, 0.04f, 0.09f, ParticleSystemShapeType.Cone,
                    0.42f, true, ParticleSystemSimulationSpace.World, 8, 30f
                );
                purpleTrail.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                ParticleSystem.ShapeModule trailShape = purpleTrail.shape;
                trailShape.angle = 10f;
                trailShape.length = 0.18f;

                GameObject lightObject = new GameObject("HollowPurpleOrbLight");
                lightObject.transform.SetParent(purpleOrb, false);
                purpleLight = lightObject.AddComponent<Light>();
                purpleLight.type = LightType.Point;
                purpleLight.color = new Color(0.58f, 0.06f, 1f);
                purpleLight.range = 8f;
                purpleLight.intensity = 4.2f;
                purpleLight.shadows = LightShadows.None;

                purpleOrb.gameObject.SetActive(false);
                blueOrb.position = start - right * 1.45f;
                redOrb.position = start + right * 1.45f;
                blueOrb.localScale = Vector3.one * 0.92f;
                redOrb.localScale = Vector3.one * 0.92f;
                blueInward.Play(true);
                redOutward.Play(true);
            }

            public bool Update(float now, float unscaledDeltaTime)
            {
                if (root == null)
                {
                    return false;
                }

                float elapsed = now - startedAt;
                if (elapsed < MergeDuration)
                {
                    UpdateMerge(elapsed / MergeDuration, unscaledDeltaTime);
                    return true;
                }

                float launchElapsed = elapsed - MergeDuration;
                if (launchElapsed <= LaunchDuration)
                {
                    UpdateLaunch(launchElapsed / LaunchDuration, launchElapsed, unscaledDeltaTime);
                    return true;
                }

                return false;
            }

            private void UpdateMerge(float normalized, float unscaledDeltaTime)
            {
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(normalized));
                float separation = Mathf.Lerp(1.45f, 0.03f, t);
                float arc = Mathf.Sin(t * Mathf.PI) * 0.20f;

                blueOrb.position = start - right * separation + Vector3.up * arc;
                redOrb.position = start + right * separation - Vector3.up * arc * 0.35f;

                float scale = Mathf.Lerp(0.92f, 1.18f, t);
                blueOrb.localScale = Vector3.one * scale;
                redOrb.localScale = Vector3.one * scale;

                blueOrb.Rotate(Vector3.up, 620f * unscaledDeltaTime, Space.World);
                redOrb.Rotate(Vector3.up, -620f * unscaledDeltaTime, Space.World);
            }

            private void UpdateLaunch(float normalized, float launchElapsed, float unscaledDeltaTime)
            {
                if (!launched)
                {
                    launched = true;
                    blueOrb.gameObject.SetActive(false);
                    redOrb.gameObject.SetActive(false);
                    purpleOrb.gameObject.SetActive(true);
                    purpleOrb.position = start;
                    purpleFragments.Play(true);
                    purpleTrail.Play(true);
                }

                float t = Mathf.Clamp01(normalized);
                float travel = Mathf.Lerp(0.10f, TravelDistance, t);
                purpleOrb.position = start + direction * travel;

                float ignition = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.Clamp01(launchElapsed / 0.09f)
                );
                float growth = Mathf.Lerp(0.34f, 1.12f, ignition);
                float pulse = 1f + Mathf.Sin(launchElapsed * 20f) * 0.035f;
                purpleOrb.localScale = Vector3.one * growth * pulse;
                purpleCore.Rotate(Vector3.up, 120f * unscaledDeltaTime, Space.Self);

                if (purpleLight != null)
                {
                    purpleLight.intensity =
                        ignition * (4.2f + Mathf.Sin(launchElapsed * 18f) * 0.55f);
                    purpleLight.range = Mathf.Lerp(7f, 10f, t);
                }
            }

            public void Dispose()
            {
                if (root != null)
                {
                    Object.Destroy(root);
                }

                foreach (Material material in runtimeMaterials)
                {
                    if (material != null)
                    {
                        Object.Destroy(material);
                    }
                }
                runtimeMaterials.Clear();
                materialColors.Clear();
            }
        }
    }
}
