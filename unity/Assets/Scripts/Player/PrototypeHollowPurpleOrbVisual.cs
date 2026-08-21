using UnityEngine;

namespace JJKGame.Player
{
    [DefaultExecutionOrder(1700)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BasicAttack))]
    public sealed class PrototypeHollowPurpleOrbVisual : MonoBehaviour
    {
        private const string SourceRootName = "HollowPurplePrototypeVisual";
        private const float MergeDuration = 0.18f;
        private const float LaunchDuration = 0.67f;
        private const float TravelDistance = 18f;

        private Transform sourceRoot;
        private GameObject visualRoot;
        private Transform blueOrb;
        private Transform redOrb;
        private Transform purpleOrb;
        private TrailRenderer purpleTrail;
        private Light purpleLight;
        private LineRenderer[] orbitRings;

        private bool sourceWasActive;
        private bool sequenceActive;
        private float sequenceStartedAt;
        private Vector3 sequenceStart;
        private Vector3 sequenceDirection;
        private Vector3 sequenceRight;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BootstrapAfterSceneLoad()
        {
            BasicAttack[] attacks = FindObjectsByType<BasicAttack>(FindObjectsSortMode.None);
            foreach (BasicAttack attack in attacks)
            {
                if (
                    attack == null
                    || attack.GetComponent<PrototypeHollowPurpleOrbVisual>() != null
                )
                {
                    continue;
                }

                attack.gameObject.AddComponent<PrototypeHollowPurpleOrbVisual>();
            }
        }

        private void Awake()
        {
            ResolveSourceRoot();
            BuildVisual();
            DisableLegacyBeamVisual();
        }

        private void Update()
        {
            ResolveSourceRoot();
            DisableLegacyBeamVisual();

            bool sourceActive = sourceRoot != null && sourceRoot.gameObject.activeInHierarchy;
            if (sourceActive && !sourceWasActive)
            {
                BeginSequence();
            }

            if (sequenceActive)
            {
                UpdateSequence();
            }

            sourceWasActive = sourceActive;
        }

        private void OnDestroy()
        {
            if (visualRoot != null)
            {
                Destroy(visualRoot);
            }
        }

        private void ResolveSourceRoot()
        {
            if (sourceRoot == null)
            {
                sourceRoot = transform.Find(SourceRootName);
            }
        }

        private void DisableLegacyBeamVisual()
        {
            if (sourceRoot == null)
            {
                return;
            }

            LineRenderer[] legacyLines = sourceRoot.GetComponentsInChildren<LineRenderer>(true);
            foreach (LineRenderer line in legacyLines)
            {
                if (line != null)
                {
                    line.enabled = false;
                }
            }

            Light[] legacyLights = sourceRoot.GetComponentsInChildren<Light>(true);
            foreach (Light light in legacyLights)
            {
                if (light != null)
                {
                    light.enabled = false;
                }
            }
        }

        private void BeginSequence()
        {
            sequenceDirection = transform.forward;
            sequenceDirection.y = 0f;
            if (sequenceDirection.sqrMagnitude <= 0.001f)
            {
                sequenceDirection = Vector3.forward;
            }
            sequenceDirection.Normalize();

            sequenceRight = Vector3.Cross(Vector3.up, sequenceDirection).normalized;
            sequenceStart = transform.position + Vector3.up * 1.05f + sequenceDirection * 1.05f;
            sequenceStartedAt = Time.unscaledTime;
            sequenceActive = true;

            visualRoot.SetActive(true);
            blueOrb.gameObject.SetActive(true);
            redOrb.gameObject.SetActive(true);
            purpleOrb.gameObject.SetActive(false);

            blueOrb.position = sequenceStart - sequenceRight * 1.35f;
            redOrb.position = sequenceStart + sequenceRight * 1.35f;
            blueOrb.localScale = Vector3.one * 0.78f;
            redOrb.localScale = Vector3.one * 0.78f;

            if (purpleTrail != null)
            {
                purpleTrail.Clear();
                purpleTrail.emitting = false;
            }
        }

        private void UpdateSequence()
        {
            float elapsed = Time.unscaledTime - sequenceStartedAt;
            if (elapsed < MergeDuration)
            {
                UpdateMerge(elapsed / MergeDuration);
                return;
            }

            float launchElapsed = elapsed - MergeDuration;
            if (launchElapsed <= LaunchDuration)
            {
                UpdateLaunch(launchElapsed / LaunchDuration, launchElapsed);
                return;
            }

            sequenceActive = false;
            visualRoot.SetActive(false);
        }

        private void UpdateMerge(float normalized)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(normalized));
            float approach = Mathf.Lerp(1.35f, 0.08f, t);
            float lift = Mathf.Sin(t * Mathf.PI) * 0.18f;

            blueOrb.position = sequenceStart - sequenceRight * approach + Vector3.up * lift;
            redOrb.position = sequenceStart + sequenceRight * approach - Vector3.up * lift * 0.35f;

            float mergeScale = Mathf.Lerp(0.78f, 1.02f, t);
            blueOrb.localScale = Vector3.one * mergeScale;
            redOrb.localScale = Vector3.one * mergeScale;

            blueOrb.Rotate(Vector3.up, 540f * Time.unscaledDeltaTime, Space.World);
            redOrb.Rotate(Vector3.up, -540f * Time.unscaledDeltaTime, Space.World);
        }

        private void UpdateLaunch(float normalized, float launchElapsed)
        {
            if (blueOrb.gameObject.activeSelf)
            {
                blueOrb.gameObject.SetActive(false);
                redOrb.gameObject.SetActive(false);
                purpleOrb.gameObject.SetActive(true);
                if (purpleTrail != null)
                {
                    purpleTrail.Clear();
                    purpleTrail.emitting = true;
                }
            }

            float t = Mathf.Clamp01(normalized);
            float travel = Mathf.Lerp(0.15f, TravelDistance, t);
            purpleOrb.position = sequenceStart + sequenceDirection * travel;

            float grow = Mathf.Lerp(2.9f, 4.15f, Mathf.SmoothStep(0f, 1f, Mathf.Min(1f, t * 2.2f)));
            float pulse = 1f + Mathf.Sin(launchElapsed * 24f) * 0.055f;
            purpleOrb.localScale = Vector3.one * grow * pulse;
            purpleOrb.Rotate(sequenceDirection, 220f * Time.unscaledDeltaTime, Space.World);

            if (orbitRings != null)
            {
                for (int index = 0; index < orbitRings.Length; index++)
                {
                    LineRenderer ring = orbitRings[index];
                    if (ring == null)
                    {
                        continue;
                    }

                    float direction = index % 2 == 0 ? 1f : -1f;
                    ring.transform.Rotate(
                        Vector3.forward,
                        direction * (160f + index * 45f) * Time.unscaledDeltaTime,
                        Space.Self
                    );
                }
            }

            if (purpleLight != null)
            {
                purpleLight.intensity = 7.5f + Mathf.Sin(launchElapsed * 22f) * 1.6f;
                purpleLight.range = Mathf.Lerp(8f, 13f, t);
            }
        }

        private void BuildVisual()
        {
            visualRoot = new GameObject("HollowPurpleOrbOverrideVisual");
            visualRoot.SetActive(false);

            blueOrb = CreateSphere(
                "PurpleMergeBlue",
                new Color(0.10f, 0.34f, 1f, 1f)
            );
            redOrb = CreateSphere(
                "PurpleMergeRed",
                new Color(1f, 0.08f, 0.06f, 1f)
            );
            purpleOrb = CreateSphere(
                "PurpleOrb",
                new Color(0.57f, 0.06f, 1f, 1f)
            );

            blueOrb.SetParent(visualRoot.transform, true);
            redOrb.SetParent(visualRoot.transform, true);
            purpleOrb.SetParent(visualRoot.transform, true);

            purpleTrail = purpleOrb.gameObject.AddComponent<TrailRenderer>();
            purpleTrail.time = 0.18f;
            purpleTrail.minVertexDistance = 0.08f;
            purpleTrail.startWidth = 2.0f;
            purpleTrail.endWidth = 0.06f;
            purpleTrail.startColor = new Color(0.70f, 0.18f, 1f, 0.72f);
            purpleTrail.endColor = new Color(0.28f, 0.02f, 0.48f, 0f);
            purpleTrail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            purpleTrail.receiveShadows = false;
            purpleTrail.material = CreateMaterial(new Color(0.62f, 0.08f, 1f, 0.82f));
            purpleTrail.emitting = false;

            orbitRings = new[]
            {
                CreateOrbitRing("PurpleOrbitA", Quaternion.identity, 0.76f, 0.035f),
                CreateOrbitRing("PurpleOrbitB", Quaternion.Euler(68f, 0f, 18f), 0.88f, 0.028f),
                CreateOrbitRing("PurpleOrbitC", Quaternion.Euler(24f, 52f, 76f), 1.00f, 0.022f),
            };

            foreach (LineRenderer ring in orbitRings)
            {
                ring.transform.SetParent(purpleOrb, false);
            }

            GameObject lightObject = new GameObject("PurpleOrbLight");
            lightObject.transform.SetParent(purpleOrb, false);
            purpleLight = lightObject.AddComponent<Light>();
            purpleLight.type = LightType.Point;
            purpleLight.color = new Color(0.58f, 0.08f, 1f);
            purpleLight.range = 10f;
            purpleLight.intensity = 8f;
            purpleLight.shadows = LightShadows.None;
        }

        private Transform CreateSphere(string objectName, Color color)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = objectName;

            Collider collider = sphere.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            Renderer renderer = sphere.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = CreateMaterial(color);
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
            line.positionCount = 64;
            line.startWidth = width;
            line.endWidth = width;
            line.startColor = new Color(0.92f, 0.68f, 1f, 0.95f);
            line.endColor = new Color(0.62f, 0.14f, 1f, 0.72f);
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.material = CreateMaterial(new Color(0.78f, 0.34f, 1f, 0.90f));

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

        private static Material CreateMaterial(Color color)
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

            Material material = new Material(shader) { color = color };
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 2.6f);
            }
            return material;
        }
    }
}
