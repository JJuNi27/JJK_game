using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JJKGame.Dev.VFXLab
{
    /// <summary>
    /// Scene-owned VFXLab composition root. It creates only presentation stage,
    /// lighting, camera, movement, overlay, and preview sequencing helpers.
    /// </summary>
    [DefaultExecutionOrder(1500)]
    [DisallowMultipleComponent]
    public sealed class VfxLabController : MonoBehaviour
    {
        private readonly List<Material> runtimeMaterials = new List<Material>(8);

        private Transform labRoot;
        private Transform previewStage;
        private Transform previewCharacterRoot;
        private Transform previewPoint;
        private Transform lightingRoot;
        private Camera previewCamera;
        private VfxLabPreviewCharacter previewCharacter;
        private VfxLabOrbitCamera orbitCamera;
        private VfxLabPreviewSequence sequence;
        private VolumeProfile runtimeVolumeProfile;
        private bool originalFog;
        private FogMode originalFogMode;
        private Color originalFogColor;
        private float originalFogStart;
        private float originalFogEnd;
        private AmbientMode originalAmbientMode;
        private Color originalAmbientSky;
        private Color originalAmbientEquator;
        private Color originalAmbientGround;
        private float originalAmbientIntensity;
        private GUIStyle titleStyle;
        private GUIStyle lineStyle;
        private int styledForHeight = -1;

        private void Awake()
        {
            labRoot = transform.parent != null ? transform.parent : transform;
            previewStage = RequireChild(labRoot, "PreviewStage");
            previewCharacterRoot = RequireChild(labRoot, "PreviewCharacter");
            previewPoint = RequireChild(labRoot, "VFXPreviewPoint");
            lightingRoot = RequireChild(labRoot, "Lighting");
            Transform cameraRoot = RequireChild(labRoot, "PreviewCamera");

            previewCharacter = previewCharacterRoot.GetComponent<VfxLabPreviewCharacter>();
            if (previewCharacter == null)
            {
                previewCharacter = previewCharacterRoot.gameObject.AddComponent<VfxLabPreviewCharacter>();
            }

            previewCamera = cameraRoot.GetComponent<Camera>();
            if (previewCamera == null)
            {
                previewCamera = cameraRoot.gameObject.AddComponent<Camera>();
            }
            cameraRoot.gameObject.tag = "MainCamera";
            if (cameraRoot.GetComponent<AudioListener>() == null)
            {
                cameraRoot.gameObject.AddComponent<AudioListener>();
            }
            orbitCamera = cameraRoot.GetComponent<VfxLabOrbitCamera>();
            if (orbitCamera == null)
            {
                orbitCamera = cameraRoot.gameObject.AddComponent<VfxLabOrbitCamera>();
            }

            sequence = GetComponent<VfxLabPreviewSequence>();
            if (sequence == null)
            {
                sequence = gameObject.AddComponent<VfxLabPreviewSequence>();
            }

            CaptureRenderSettings();
            BuildStage();
            BuildLightingAndPostProcess();
        }

        private void Start()
        {
            previewCharacter.Configure(previewCamera.transform);
            orbitCamera.Configure(previewCharacterRoot, previewPoint);
            sequence.Configure(previewPoint, previewCharacter);
        }

        private void CaptureRenderSettings()
        {
            originalFog = RenderSettings.fog;
            originalFogMode = RenderSettings.fogMode;
            originalFogColor = RenderSettings.fogColor;
            originalFogStart = RenderSettings.fogStartDistance;
            originalFogEnd = RenderSettings.fogEndDistance;
            originalAmbientMode = RenderSettings.ambientMode;
            originalAmbientSky = RenderSettings.ambientSkyColor;
            originalAmbientEquator = RenderSettings.ambientEquatorColor;
            originalAmbientGround = RenderSettings.ambientGroundColor;
            originalAmbientIntensity = RenderSettings.ambientIntensity;
        }

        private void BuildStage()
        {
            if (previewStage.Find("NeutralFloor") != null)
            {
                return;
            }

            Material floorMaterial = CreateLitMaterial(new Color(0.060f, 0.078f, 0.115f, 1f), 0.12f, 0.48f);
            Material backdropMaterial = CreateLitMaterial(new Color(0.032f, 0.047f, 0.078f, 1f), 0.02f, 0.55f);
            Material accentMaterial = CreateUnlitMaterial(new Color(0.030f, 0.20f, 0.46f, 0.30f), 1.2f);
            Material markerMaterial = CreateUnlitMaterial(new Color(0.06f, 0.48f, 0.76f, 0.46f), 1.45f);

            CreateBox(
                previewStage,
                "NeutralFloor",
                new Vector3(0f, -0.14f, 2.5f),
                new Vector3(18f, 0.24f, 18f),
                floorMaterial,
                true
            );
            CreateBox(
                previewStage,
                "DarkBackdrop",
                new Vector3(0f, 4f, 11f),
                new Vector3(18f, 8f, 0.25f),
                backdropMaterial,
                false
            );
            CreateBox(
                previewStage,
                "LeftSeparationPanel",
                new Vector3(-8.5f, 2.4f, 2.5f),
                new Vector3(0.20f, 4.8f, 17f),
                backdropMaterial,
                false
            );
            CreateBox(
                previewStage,
                "RightSeparationPanel",
                new Vector3(8.5f, 2.4f, 2.5f),
                new Vector3(0.20f, 4.8f, 17f),
                backdropMaterial,
                false
            );

            CreateRing(previewStage, "StageGuideRing", new Vector3(0f, 0.015f, 2.5f), 5.8f, 0.026f, accentMaterial, 80);
            CreateRing(previewStage, "CharacterGuide", new Vector3(0f, 0.020f, 0f), 1.25f, 0.020f, accentMaterial, 48);

            Transform markerRoot = new GameObject("PreviewMarkerVisual").transform;
            markerRoot.SetParent(previewPoint, false);
            CreateRing(markerRoot, "BlueRadiusGuide", new Vector3(0f, -0.98f, 0f), 4.5f, 0.018f, markerMaterial, 96);
            CreateRing(markerRoot, "CoreGuide", Vector3.up * 0.35f, 0.34f, 0.024f, markerMaterial, 48);
            CreateMarkerAxis(markerRoot, markerMaterial);
        }

        private void BuildLightingAndPostProcess()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.024f, 0.036f, 0.065f, 1f);
            RenderSettings.fogStartDistance = 24f;
            RenderSettings.fogEndDistance = 58f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.115f, 0.155f, 0.245f, 1f);
            RenderSettings.ambientEquatorColor = new Color(0.064f, 0.088f, 0.142f, 1f);
            RenderSettings.ambientGroundColor = new Color(0.028f, 0.036f, 0.060f, 1f);
            RenderSettings.ambientIntensity = 1.05f;

            CreateDirectionalLight(
                "CoolKey",
                Quaternion.Euler(46f, -34f, 0f),
                new Color(0.78f, 0.88f, 1f),
                1.22f,
                LightShadows.Soft
            );
            CreatePointLight(
                "CoolFill",
                new Vector3(-4.2f, 3.4f, 1.8f),
                new Color(0.20f, 0.44f, 1f),
                4.1f,
                13f
            );
            CreatePointLight(
                "WarmRim",
                new Vector3(4.5f, 2.8f, 5.5f),
                new Color(1f, 0.32f, 0.16f),
                2.8f,
                12f
            );

            Volume volume = lightingRoot.GetComponent<Volume>();
            if (volume == null)
            {
                volume = lightingRoot.gameObject.AddComponent<Volume>();
            }
            volume.isGlobal = true;
            volume.priority = 110f;
            volume.weight = 1f;

            runtimeVolumeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            runtimeVolumeProfile.name = "VFXLab_RuntimeVolume";
            runtimeVolumeProfile.hideFlags = HideFlags.HideAndDontSave;
            volume.profile = runtimeVolumeProfile;

            Tonemapping tonemapping = runtimeVolumeProfile.Add<Tonemapping>(true);
            tonemapping.mode.Override(TonemappingMode.ACES);

            Bloom bloom = runtimeVolumeProfile.Add<Bloom>(true);
            bloom.threshold.Override(1.0f);
            bloom.intensity.Override(0.20f);
            bloom.scatter.Override(0.48f);
            bloom.clamp.Override(5f);
            bloom.tint.Override(new Color(0.92f, 0.97f, 1f, 1f));
            bloom.highQualityFiltering.Override(false);

            ColorAdjustments color = runtimeVolumeProfile.Add<ColorAdjustments>(true);
            color.postExposure.Override(0.12f);
            color.contrast.Override(7f);
            color.colorFilter.Override(new Color(0.97f, 0.985f, 1f, 1f));
            color.saturation.Override(-2f);

            WhiteBalance whiteBalance = runtimeVolumeProfile.Add<WhiteBalance>(true);
            whiteBalance.temperature.Override(-4f);
            whiteBalance.tint.Override(1f);

            Vignette vignette = runtimeVolumeProfile.Add<Vignette>(true);
            vignette.color.Override(new Color(0.004f, 0.008f, 0.020f, 1f));
            vignette.intensity.Override(0.07f);
            vignette.smoothness.Override(0.55f);

            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0.022f, 0.034f, 0.062f, 1f);
            previewCamera.fieldOfView = 55f;
            previewCamera.nearClipPlane = 0.1f;
            previewCamera.farClipPlane = 120f;
            previewCamera.allowHDR = true;

            UniversalAdditionalCameraData cameraData =
                previewCamera.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData == null)
            {
                cameraData = previewCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }
            cameraData.renderPostProcessing = true;
            cameraData.volumeLayerMask = ~0;
        }

        private void CreateDirectionalLight(
            string name,
            Quaternion rotation,
            Color color,
            float intensity,
            LightShadows shadows
        )
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.SetParent(lightingRoot, false);
            lightObject.transform.rotation = rotation;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = shadows;
            light.shadowStrength = 0.62f;
        }

        private void CreatePointLight(
            string name,
            Vector3 position,
            Color color,
            float intensity,
            float range
        )
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.SetParent(lightingRoot, false);
            lightObject.transform.localPosition = position;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        private static Transform RequireChild(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child != null)
            {
                return child;
            }
            Transform created = new GameObject(childName).transform;
            created.SetParent(parent, false);
            return created;
        }

        private static void CreateBox(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Material material,
            bool keepCollider
        )
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = position;
            box.transform.localScale = scale;
            Renderer renderer = box.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
            if (!keepCollider)
            {
                Collider collider = box.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }
            }
        }

        private static void CreateRing(
            Transform parent,
            string name,
            Vector3 position,
            float radius,
            float width,
            Material material,
            int segments
        )
        {
            GameObject ringObject = new GameObject(name);
            ringObject.transform.SetParent(parent, false);
            ringObject.transform.localPosition = position;
            LineRenderer line = ringObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = Mathf.Max(16, segments);
            line.startWidth = width;
            line.endWidth = width;
            line.sharedMaterial = material;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            for (int index = 0; index < line.positionCount; index++)
            {
                float angle = index * Mathf.PI * 2f / line.positionCount;
                line.SetPosition(index, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }

        private static void CreateMarkerAxis(Transform parent, Material material)
        {
            GameObject axisObject = new GameObject("PreviewPointAxis");
            axisObject.transform.SetParent(parent, false);
            LineRenderer line = axisObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.SetPosition(0, Vector3.down * 0.95f);
            line.SetPosition(1, Vector3.up * 0.95f);
            line.startWidth = 0.025f;
            line.endWidth = 0.008f;
            line.sharedMaterial = material;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
        }

        private Material CreateLitMaterial(
            Color color,
            float metallic,
            float smoothness
        )
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            shader ??= Shader.Find("Standard");
            if (shader == null)
            {
                return null;
            }
            Material material = new Material(shader) { color = color };
            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }
            runtimeMaterials.Add(material);
            return material;
        }

        private Material CreateUnlitMaterial(Color color, float emission)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            shader ??= Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }
            Material material = new Material(shader) { color = color };
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emission);
            }
            runtimeMaterials.Add(material);
            return material;
        }

        private void OnGUI()
        {
            if (sequence == null)
            {
                return;
            }
            EnsureStyles();
            const float margin = 14f;
            float width = Mathf.Min(430f, Screen.width - margin * 2f);
            Rect panel = new Rect(margin, margin, width, 180f);
            Color previous = GUI.color;
            GUI.color = new Color(0.008f, 0.015f, 0.035f, 0.92f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = previous;
            GUI.Box(panel, GUIContent.none);

            GUI.Label(new Rect(panel.x + 12f, panel.y + 8f, panel.width - 24f, 24f), "VFX LAB · GOJO BLUE", titleStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 36f, panel.width - 24f, 20f), $"Preview  {sequence.SelectedPreviewLabel}", lineStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 57f, panel.width - 24f, 20f), $"Phase    {sequence.CurrentPhaseLabel}", lineStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 78f, panel.width - 24f, 20f), $"Loop     {(sequence.LoopEnabled ? "ON" : "OFF")}    Speed  {sequence.PlaybackSpeed:0.###}x    {(sequence.Paused ? "PAUSED" : "PLAYING")}", lineStyle);
            GUI.Label(new Rect(panel.x + 12f, panel.y + 99f, panel.width - 24f, 20f), $"Motion   {previewCharacter.AnimationSourceLabel}", lineStyle);
            if (!sequence.RuntimeReady)
            {
                Color oldColor = lineStyle.normal.textColor;
                lineStyle.normal.textColor = new Color(1f, 0.28f, 0.22f);
                GUI.Label(new Rect(panel.x + 12f, panel.y + 120f, panel.width - 24f, 20f), "Production VFX runtime is not registered", lineStyle);
                lineStyle.normal.textColor = oldColor;
            }
            GUI.Label(
                new Rect(panel.x + 12f, panel.y + 121f, panel.width - 24f, 50f),
                "1 Full  ·  2 Field  ·  3 Impact  ·  R Replay  ·  L Loop\nP Pause  ·  [ / ] Speed  ·  Backspace Clear  ·  RMB Orbit  ·  Wheel Zoom  ·  Home Camera",
                lineStyle
            );
        }

        private void EnsureStyles()
        {
            if (styledForHeight == Screen.height)
            {
                return;
            }
            styledForHeight = Screen.height;
            int baseSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height / 64f, 12f, 17f));
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = baseSize + 2,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
            };
            titleStyle.normal.textColor = new Color(0.30f, 0.82f, 1f);
            lineStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = baseSize,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
            };
            lineStyle.normal.textColor = new Color(0.88f, 0.94f, 1f);
        }

        private void OnDestroy()
        {
            RenderSettings.fog = originalFog;
            RenderSettings.fogMode = originalFogMode;
            RenderSettings.fogColor = originalFogColor;
            RenderSettings.fogStartDistance = originalFogStart;
            RenderSettings.fogEndDistance = originalFogEnd;
            RenderSettings.ambientMode = originalAmbientMode;
            RenderSettings.ambientSkyColor = originalAmbientSky;
            RenderSettings.ambientEquatorColor = originalAmbientEquator;
            RenderSettings.ambientGroundColor = originalAmbientGround;
            RenderSettings.ambientIntensity = originalAmbientIntensity;

            if (runtimeVolumeProfile != null)
            {
                for (int index = runtimeVolumeProfile.components.Count - 1; index >= 0; index--)
                {
                    VolumeComponent component = runtimeVolumeProfile.components[index];
                    if (component != null)
                    {
                        Destroy(component);
                    }
                }
                Destroy(runtimeVolumeProfile);
                runtimeVolumeProfile = null;
            }
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
