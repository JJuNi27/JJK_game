using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace JJKGame.Core
{
    /// <summary>
    /// CombatMVP-only production-facing owner for arena RenderSettings, lighting mood,
    /// and URP post processing. It owns presentation state only and is destroyed with
    /// the scene so combat reloads cannot accumulate Volumes, Lights, or profiles.
    /// </summary>
    [DefaultExecutionOrder(-1100)]
    [DisallowMultipleComponent]
    public sealed class ProductionArenaMoodController : MonoBehaviour
    {
        private const string TargetSceneName = "CombatMVP";
        private const string RuntimeProfileName = "CombatMVP_RuntimeMoodProfile";

        private static readonly Color NightFog = new Color(0.018f, 0.032f, 0.060f, 1f);
        private static readonly Color AmbientSky = new Color(0.080f, 0.120f, 0.205f, 1f);
        private static readonly Color AmbientEquator = new Color(0.045f, 0.065f, 0.110f, 1f);
        private static readonly Color AmbientGround = new Color(0.018f, 0.022f, 0.038f, 1f);

        private Volume globalVolume;
        private VolumeProfile runtimeProfile;
        private GameObject moodLightRoot;
        private Camera mainCamera;
        private UniversalAdditionalCameraData cameraData;
        private Light mainDirectional;
        private bool initialized;

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

        private CameraClearFlags originalClearFlags;
        private Color originalBackgroundColor;
        private bool originalPostProcessing;
        private LayerMask originalVolumeLayerMask;

        private Color originalDirectionalColor;
        private float originalDirectionalIntensity;
        private LightShadows originalDirectionalShadows;
        private float originalDirectionalShadowStrength;

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
                || FindFirstObjectByType<ProductionArenaMoodController>() != null
            )
            {
                return;
            }

            GameObject host = new GameObject("ProductionArenaMoodController");
            host.AddComponent<ProductionArenaMoodController>();
        }

        private void Awake()
        {
            if (SceneManager.GetActiveScene().name != TargetSceneName)
            {
                enabled = false;
                return;
            }

            CaptureRenderSettings();
            CaptureCamera();
            CaptureMainDirectional();
            BuildGlobalVolume();
            BuildMoodLights();
            initialized = true;
            ApplyMood();
        }

        private void OnEnable()
        {
            if (initialized)
            {
                ApplyMood();
            }
        }

        private void OnDisable()
        {
            if (initialized)
            {
                RestorePresentationState();
            }
        }

        private void OnDestroy()
        {
            RestorePresentationState();
            DestroyRuntimeProfile();
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

        private void CaptureCamera()
        {
            mainCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            if (mainCamera == null)
            {
                return;
            }

            originalClearFlags = mainCamera.clearFlags;
            originalBackgroundColor = mainCamera.backgroundColor;
            cameraData = mainCamera.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData == null)
            {
                cameraData = mainCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }

            originalPostProcessing = cameraData.renderPostProcessing;
            originalVolumeLayerMask = cameraData.volumeLayerMask;
        }

        private void CaptureMainDirectional()
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light != null && light.type == LightType.Directional)
                {
                    mainDirectional = light;
                    break;
                }
            }

            if (mainDirectional == null)
            {
                return;
            }

            originalDirectionalColor = mainDirectional.color;
            originalDirectionalIntensity = mainDirectional.intensity;
            originalDirectionalShadows = mainDirectional.shadows;
            originalDirectionalShadowStrength = mainDirectional.shadowStrength;
        }

        private void BuildGlobalVolume()
        {
            globalVolume = gameObject.AddComponent<Volume>();
            globalVolume.isGlobal = true;
            globalVolume.priority = 100f;
            globalVolume.weight = 1f;

            runtimeProfile = ScriptableObject.CreateInstance<VolumeProfile>();
            runtimeProfile.name = RuntimeProfileName;
            runtimeProfile.hideFlags = HideFlags.HideAndDontSave;
            globalVolume.profile = runtimeProfile;

            Tonemapping tonemapping = AddOverride<Tonemapping>();
            tonemapping.mode.Override(TonemappingMode.ACES);

            Bloom bloom = AddOverride<Bloom>();
            bloom.threshold.Override(0.95f);
            bloom.intensity.Override(0.24f);
            bloom.scatter.Override(0.52f);
            bloom.clamp.Override(6f);
            bloom.tint.Override(new Color(0.92f, 0.97f, 1f, 1f));
            bloom.highQualityFiltering.Override(false);

            ColorAdjustments color = AddOverride<ColorAdjustments>();
            color.postExposure.Override(-0.10f);
            color.contrast.Override(11f);
            color.colorFilter.Override(new Color(0.94f, 0.97f, 1f, 1f));
            color.hueShift.Override(0f);
            color.saturation.Override(-2f);

            WhiteBalance whiteBalance = AddOverride<WhiteBalance>();
            whiteBalance.temperature.Override(-4f);
            whiteBalance.tint.Override(1f);

            Vignette vignette = AddOverride<Vignette>();
            vignette.color.Override(new Color(0.005f, 0.010f, 0.025f, 1f));
            vignette.center.Override(new Vector2(0.5f, 0.5f));
            vignette.intensity.Override(0.13f);
            vignette.smoothness.Override(0.55f);
            vignette.rounded.Override(false);
        }

        private T AddOverride<T>() where T : VolumeComponent
        {
            T component = runtimeProfile.Add<T>(true);
            component.hideFlags = HideFlags.HideAndDontSave;
            return component;
        }

        private void BuildMoodLights()
        {
            moodLightRoot = new GameObject("ProductionMoodLights");
            moodLightRoot.transform.SetParent(transform, false);

            CreatePointLight(
                "CoolArenaFill",
                new Vector3(-7f, 6f, -4f),
                new Color(0.24f, 0.52f, 1f),
                1.45f,
                19f
            );
            CreatePointLight(
                "WarmArenaRim",
                new Vector3(8f, 5.2f, 5f),
                new Color(1f, 0.30f, 0.13f),
                1.15f,
                16f
            );
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
            lightObject.transform.SetParent(moodLightRoot.transform, false);
            lightObject.transform.position = position;

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        private void ApplyMood()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = NightFog;
            RenderSettings.fogStartDistance = 26f;
            RenderSettings.fogEndDistance = 80f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = AmbientSky;
            RenderSettings.ambientEquatorColor = AmbientEquator;
            RenderSettings.ambientGroundColor = AmbientGround;
            RenderSettings.ambientIntensity = 0.82f;

            if (mainCamera != null)
            {
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = NightFog;
            }
            if (cameraData != null)
            {
                cameraData.renderPostProcessing = true;
                cameraData.volumeLayerMask = ~0;
            }
            if (mainDirectional != null)
            {
                mainDirectional.color = new Color(0.66f, 0.78f, 1f);
                mainDirectional.intensity = 0.90f;
                mainDirectional.shadows = LightShadows.Soft;
                mainDirectional.shadowStrength = 0.55f;
            }
            if (globalVolume != null)
            {
                globalVolume.enabled = true;
            }
            if (moodLightRoot != null)
            {
                moodLightRoot.SetActive(true);
            }
        }

        private void RestorePresentationState()
        {
            if (!initialized)
            {
                return;
            }

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

            if (mainCamera != null)
            {
                mainCamera.clearFlags = originalClearFlags;
                mainCamera.backgroundColor = originalBackgroundColor;
            }
            if (cameraData != null)
            {
                cameraData.renderPostProcessing = originalPostProcessing;
                cameraData.volumeLayerMask = originalVolumeLayerMask;
            }
            if (mainDirectional != null)
            {
                mainDirectional.color = originalDirectionalColor;
                mainDirectional.intensity = originalDirectionalIntensity;
                mainDirectional.shadows = originalDirectionalShadows;
                mainDirectional.shadowStrength = originalDirectionalShadowStrength;
            }
            if (globalVolume != null)
            {
                globalVolume.enabled = false;
            }
            if (moodLightRoot != null)
            {
                moodLightRoot.SetActive(false);
            }
        }

        private void DestroyRuntimeProfile()
        {
            if (runtimeProfile == null)
            {
                return;
            }

            if (globalVolume != null)
            {
                globalVolume.profile = null;
            }

            for (int index = runtimeProfile.components.Count - 1; index >= 0; index--)
            {
                VolumeComponent component = runtimeProfile.components[index];
                if (component != null)
                {
                    Destroy(component);
                }
            }
            runtimeProfile.components.Clear();
            Destroy(runtimeProfile);
            runtimeProfile = null;
        }
    }
}
