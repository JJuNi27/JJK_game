using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.Core
{
    /// <summary>
    /// Beauty Corner-only arena mood scaffold.
    /// It adds non-gameplay urban-night presentation around CombatMVP without
    /// changing the arena colliders, spawn positions, combat ranges, or match rules.
    /// Final environment art will replace this runtime prototype.
    /// </summary>
    [DefaultExecutionOrder(-1200)]
    [DisallowMultipleComponent]
    public sealed class PrototypeBeautyArenaPresentation : MonoBehaviour
    {
        private const string TargetSceneName = "CombatMVP";
        private const string ArenaRootName = "__BeautyArenaPresentation";

        private readonly List<Material> runtimeMaterials = new List<Material>();

        private GameObject arenaRoot;
        private bool renderSettingsCaptured;
        private bool originalFog;
        private FogMode originalFogMode;
        private Color originalFogColor;
        private float originalFogStart;
        private float originalFogEnd;
        private Color originalAmbientSky;
        private Color originalAmbientEquator;
        private Color originalAmbientGround;
        private float originalAmbientIntensity;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (FindFirstObjectByType<PrototypeBeautyArenaPresentation>() != null)
            {
                return;
            }

            GameObject runner = new GameObject("PrototypeBeautyArenaPresentation");
            DontDestroyOnLoad(runner);
            runner.AddComponent<PrototypeBeautyArenaPresentation>();
        }

        private void Awake()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            CaptureRenderSettings();
        }

        private void Start()
        {
            BuildForScene(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            RestoreRenderSettings();
            DestroyArenaRoot();
            DestroyRuntimeMaterials();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode _)
        {
            BuildForScene(scene);
        }

        private void BuildForScene(Scene scene)
        {
            DestroyArenaRoot();
            DestroyRuntimeMaterials();

            if (!scene.IsValid() || scene.name != TargetSceneName)
            {
                RestoreRenderSettings();
                return;
            }

            CaptureRenderSettings();
            ApplyNightMood();

            arenaRoot = new GameObject(ArenaRootName);
            BuildFloorLanguage();
            BuildUrbanSilhouette();
            BuildNeonAccents();
            BuildMoodLights();
        }

        private void CaptureRenderSettings()
        {
            if (renderSettingsCaptured)
            {
                return;
            }

            renderSettingsCaptured = true;
            originalFog = RenderSettings.fog;
            originalFogMode = RenderSettings.fogMode;
            originalFogColor = RenderSettings.fogColor;
            originalFogStart = RenderSettings.fogStartDistance;
            originalFogEnd = RenderSettings.fogEndDistance;
            originalAmbientSky = RenderSettings.ambientSkyColor;
            originalAmbientEquator = RenderSettings.ambientEquatorColor;
            originalAmbientGround = RenderSettings.ambientGroundColor;
            originalAmbientIntensity = RenderSettings.ambientIntensity;
        }

        private void ApplyNightMood()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.028f, 0.045f, 0.085f, 1f);
            RenderSettings.fogStartDistance = 24f;
            RenderSettings.fogEndDistance = 72f;
            RenderSettings.ambientSkyColor = new Color(0.075f, 0.105f, 0.17f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.055f, 0.065f, 0.095f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.025f, 0.028f, 0.04f, 1f);
            RenderSettings.ambientIntensity = 0.72f;
        }

        private void RestoreRenderSettings()
        {
            if (!renderSettingsCaptured)
            {
                return;
            }

            RenderSettings.fog = originalFog;
            RenderSettings.fogMode = originalFogMode;
            RenderSettings.fogColor = originalFogColor;
            RenderSettings.fogStartDistance = originalFogStart;
            RenderSettings.fogEndDistance = originalFogEnd;
            RenderSettings.ambientSkyColor = originalAmbientSky;
            RenderSettings.ambientEquatorColor = originalAmbientEquator;
            RenderSettings.ambientGroundColor = originalAmbientGround;
            RenderSettings.ambientIntensity = originalAmbientIntensity;
        }

        private void BuildFloorLanguage()
        {
            Material coolLine = CreateUnlitMaterial(new Color(0.12f, 0.38f, 0.58f, 1f));
            Material warmLine = CreateUnlitMaterial(new Color(0.52f, 0.16f, 0.12f, 1f));

            CreateRing("ArenaRingOuter", 11.2f, 0.045f, coolLine);
            CreateRing("ArenaRingInner", 7.3f, 0.024f, warmLine);

            Material laneMaterial = CreateLitMaterial(new Color(0.055f, 0.065f, 0.085f, 1f));
            CreateDecorativeCube(
                "StreetLaneX",
                new Vector3(0f, 0.025f, 0f),
                new Vector3(22f, 0.035f, 1.05f),
                laneMaterial
            );
            CreateDecorativeCube(
                "StreetLaneZ",
                new Vector3(0f, 0.028f, 0f),
                new Vector3(1.05f, 0.038f, 22f),
                laneMaterial
            );
        }

        private void BuildUrbanSilhouette()
        {
            Material buildingA = CreateLitMaterial(new Color(0.035f, 0.045f, 0.065f, 1f));
            Material buildingB = CreateLitMaterial(new Color(0.052f, 0.055f, 0.072f, 1f));

            Vector3[] positions =
            {
                new Vector3(-18f, 5.5f, 22f),
                new Vector3(-10f, 8.5f, 24f),
                new Vector3(0f, 6.5f, 26f),
                new Vector3(10f, 10f, 24f),
                new Vector3(18f, 7f, 21f),
                new Vector3(-23f, 7.5f, 8f),
                new Vector3(-24f, 10.5f, -3f),
                new Vector3(-22f, 6f, -14f),
                new Vector3(23f, 8.5f, 8f),
                new Vector3(25f, 6.5f, -4f),
                new Vector3(22f, 11f, -15f),
            };

            Vector3[] scales =
            {
                new Vector3(7f, 11f, 5f),
                new Vector3(6f, 17f, 5f),
                new Vector3(8f, 13f, 6f),
                new Vector3(6f, 20f, 5f),
                new Vector3(7f, 14f, 5f),
                new Vector3(5f, 15f, 7f),
                new Vector3(6f, 21f, 6f),
                new Vector3(5f, 12f, 7f),
                new Vector3(5f, 17f, 7f),
                new Vector3(7f, 13f, 6f),
                new Vector3(6f, 22f, 7f),
            };

            for (int index = 0; index < positions.Length; index++)
            {
                CreateDecorativeCube(
                    $"Skyline_{index + 1:00}",
                    positions[index],
                    scales[index],
                    index % 2 == 0 ? buildingA : buildingB
                );
            }
        }

        private void BuildNeonAccents()
        {
            Material cyan = CreateUnlitMaterial(new Color(0.08f, 0.78f, 1f, 1f));
            Material magenta = CreateUnlitMaterial(new Color(0.88f, 0.08f, 0.36f, 1f));
            Material amber = CreateUnlitMaterial(new Color(1f, 0.46f, 0.08f, 1f));

            CreateDecorativeCube(
                "NeonSign_Cyan",
                new Vector3(-10f, 9.2f, 20.9f),
                new Vector3(0.28f, 5.6f, 0.10f),
                cyan
            );
            CreateDecorativeCube(
                "NeonSign_Magenta",
                new Vector3(9.8f, 10.4f, 21.4f),
                new Vector3(0.32f, 7.2f, 0.10f),
                magenta
            );
            CreateDecorativeCube(
                "NeonSign_Amber",
                new Vector3(20.3f, 6.8f, 7.8f),
                new Vector3(0.12f, 4.6f, 0.28f),
                amber
            );
        }

        private void BuildMoodLights()
        {
            CreatePointLight(
                "CoolStreetLight",
                new Vector3(-8f, 5.5f, -2f),
                new Color(0.22f, 0.52f, 1f),
                2.1f,
                15f
            );
            CreatePointLight(
                "WarmStreetLight",
                new Vector3(8f, 5.2f, 4f),
                new Color(1f, 0.28f, 0.12f),
                1.7f,
                13f
            );
        }

        private void CreateRing(string objectName, float radius, float width, Material material)
        {
            GameObject ringObject = new GameObject(objectName);
            ringObject.transform.SetParent(arenaRoot.transform, false);

            LineRenderer line = ringObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = true;
            line.positionCount = 96;
            line.startWidth = width;
            line.endWidth = width;
            line.material = material;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;

            for (int index = 0; index < line.positionCount; index++)
            {
                float angle = (float)index / line.positionCount * Mathf.PI * 2f;
                line.SetPosition(
                    index,
                    new Vector3(Mathf.Cos(angle) * radius, 0.045f, Mathf.Sin(angle) * radius)
                );
            }
        }

        private void CreateDecorativeCube(
            string objectName,
            Vector3 position,
            Vector3 scale,
            Material material
        )
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(arenaRoot.transform, true);
            cube.transform.position = position;
            cube.transform.localScale = scale;

            Collider collider = cube.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                Destroy(collider);
            }

            Renderer renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private void CreatePointLight(
            string objectName,
            Vector3 position,
            Color color,
            float intensity,
            float range
        )
        {
            GameObject lightObject = new GameObject(objectName);
            lightObject.transform.SetParent(arenaRoot.transform, false);
            lightObject.transform.position = position;

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        private Material CreateLitMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            return CreateMaterial(shader, color);
        }

        private Material CreateUnlitMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }
            return CreateMaterial(shader, color);
        }

        private Material CreateMaterial(Shader shader, Color color)
        {
            Material material = shader != null
                ? new Material(shader)
                : new Material(Shader.Find("Sprites/Default"));

            material.color = color;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            runtimeMaterials.Add(material);
            return material;
        }

        private void DestroyArenaRoot()
        {
            if (arenaRoot != null)
            {
                Destroy(arenaRoot);
                arenaRoot = null;
            }
        }

        private void DestroyRuntimeMaterials()
        {
            foreach (Material material in runtimeMaterials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
            runtimeMaterials.Clear();
        }
    }
}
