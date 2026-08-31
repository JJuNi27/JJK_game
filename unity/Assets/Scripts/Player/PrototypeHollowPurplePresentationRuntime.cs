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
            if (fighter == null)
            {
                return;
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
            sequences.Add(new OrbSequence(start, direction, right, Time.unscaledTime));
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

        private sealed class OrbSequence
        {
            private const float MergeDuration = 0.24f;
            private const float LaunchDuration = 0.78f;
            private const float TravelDistance = 18f;

            private readonly GameObject root;
            private readonly Transform blueOrb;
            private readonly Transform redOrb;
            private readonly Transform purpleOrb;
            private readonly Transform purpleCore;
            private readonly LineRenderer[] orbitRings;
            private readonly Light purpleLight;
            private readonly List<Material> runtimeMaterials = new List<Material>();
            private readonly Vector3 start;
            private readonly Vector3 direction;
            private readonly Vector3 right;
            private readonly float startedAt;

            private bool launched;

            public OrbSequence(Vector3 sequenceStart, Vector3 sequenceDirection, Vector3 sequenceRight, float startTime)
            {
                start = sequenceStart;
                direction = sequenceDirection;
                right = sequenceRight;
                startedAt = startTime;

                root = new GameObject("HollowPurpleCanonicalOrbSequence");

                blueOrb = CreateSphere("HollowPurpleBlue", new Color(0.08f, 0.30f, 1f, 1f));
                redOrb = CreateSphere("HollowPurpleRed", new Color(1f, 0.05f, 0.04f, 1f));
                purpleOrb = CreateSphere("HollowPurpleOrb", new Color(0.56f, 0.04f, 1f, 1f));
                purpleCore = CreateSphere("HollowPurpleCore", new Color(0.94f, 0.76f, 1f, 1f));

                blueOrb.SetParent(root.transform, true);
                redOrb.SetParent(root.transform, true);
                purpleOrb.SetParent(root.transform, true);
                purpleCore.SetParent(purpleOrb, false);
                purpleCore.localScale = Vector3.one * 0.34f;

                orbitRings = new[]
                {
                    CreateOrbitRing("HollowPurpleOrbitA", Quaternion.identity, 0.70f, 0.030f),
                    CreateOrbitRing("HollowPurpleOrbitB", Quaternion.Euler(67f, 0f, 19f), 0.84f, 0.025f),
                    CreateOrbitRing("HollowPurpleOrbitC", Quaternion.Euler(24f, 48f, 72f), 0.96f, 0.020f),
                };
                foreach (LineRenderer ring in orbitRings)
                {
                    ring.transform.SetParent(purpleOrb, false);
                }

                GameObject lightObject = new GameObject("HollowPurpleOrbLight");
                lightObject.transform.SetParent(purpleOrb, false);
                purpleLight = lightObject.AddComponent<Light>();
                purpleLight.type = LightType.Point;
                purpleLight.color = new Color(0.58f, 0.06f, 1f);
                purpleLight.range = 10f;
                purpleLight.intensity = 8f;
                purpleLight.shadows = LightShadows.None;

                purpleOrb.gameObject.SetActive(false);
                blueOrb.position = start - right * 1.45f;
                redOrb.position = start + right * 1.45f;
                blueOrb.localScale = Vector3.one * 0.95f;
                redOrb.localScale = Vector3.one * 0.95f;
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

                float scale = Mathf.Lerp(0.95f, 1.35f, t);
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
                }

                float t = Mathf.Clamp01(normalized);
                float travel = Mathf.Lerp(0.10f, TravelDistance, t);
                purpleOrb.position = start + direction * travel;

                float growth = Mathf.Lerp(
                    3.8f,
                    5.0f,
                    Mathf.SmoothStep(0f, 1f, Mathf.Min(1f, t * 1.9f))
                );
                float pulse = 1f + Mathf.Sin(launchElapsed * 22f) * 0.045f;
                purpleOrb.localScale = Vector3.one * growth * pulse;
                purpleOrb.Rotate(direction, 190f * unscaledDeltaTime, Space.World);

                for (int index = 0; index < orbitRings.Length; index++)
                {
                    LineRenderer ring = orbitRings[index];
                    if (ring == null)
                    {
                        continue;
                    }

                    float sign = index % 2 == 0 ? 1f : -1f;
                    ring.transform.Rotate(
                        Vector3.forward,
                        sign * (145f + index * 40f) * unscaledDeltaTime,
                        Space.Self
                    );
                }

                if (purpleLight != null)
                {
                    purpleLight.intensity = 8f + Mathf.Sin(launchElapsed * 20f) * 1.4f;
                    purpleLight.range = Mathf.Lerp(9f, 14f, t);
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
            }

            private Transform CreateSphere(string objectName, Color color)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = objectName;

                Collider collider = sphere.GetComponent<Collider>();
                if (collider != null)
                {
                    Object.Destroy(collider);
                }

                Renderer renderer = sphere.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material material = CreateMaterial(color);
                    if (material != null)
                    {
                        renderer.material = material;
                    }
                }

                return sphere.transform;
            }

            private LineRenderer CreateOrbitRing(
                string objectName,
                Quaternion localRotation,
                float radius,
                float width
            )
            {
                GameObject ringObject = new GameObject(objectName);
                ringObject.transform.localRotation = localRotation;

                LineRenderer line = ringObject.AddComponent<LineRenderer>();
                line.loop = true;
                line.useWorldSpace = false;
                line.positionCount = 72;
                line.startWidth = width;
                line.endWidth = width;
                line.startColor = new Color(0.96f, 0.78f, 1f, 0.96f);
                line.endColor = new Color(0.62f, 0.12f, 1f, 0.84f);
                line.numCornerVertices = 4;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;

                Material material = CreateMaterial(new Color(0.78f, 0.34f, 1f, 1f));
                if (material != null)
                {
                    line.material = material;
                }

                for (int index = 0; index < line.positionCount; index++)
                {
                    float angle = (float)index / line.positionCount * Mathf.PI * 2f;
                    line.SetPosition(
                        index,
                        new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f)
                    );
                }

                return line;
            }

            private Material CreateMaterial(Color color)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
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
                if (material.HasProperty("_EmissionColor"))
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", color * 2.8f);
                }
                runtimeMaterials.Add(material);
                return material;
            }
        }
    }
}
