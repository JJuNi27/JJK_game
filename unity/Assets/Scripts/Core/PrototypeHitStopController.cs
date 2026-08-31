using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.Core
{
    public sealed class PrototypeHitStopController : MonoBehaviour
    {
        private static PrototypeHitStopController instance;

        private bool hitStopActive;
        private float hitStopEndsAt;
        private float restoreTimeScale = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            instance = null;
        }

        public static void Request(float realTimeDuration, float relativeTimeScale)
        {
            if (realTimeDuration <= 0f)
            {
                return;
            }

            PrototypeHitStopController controller = GetOrCreate();
            controller.ApplyRequest(realTimeDuration, relativeTimeScale);
        }

        private static PrototypeHitStopController GetOrCreate()
        {
            if (instance != null)
            {
                return instance;
            }

            PrototypeHitStopController existing = FindFirstObjectByType<PrototypeHitStopController>();
            if (existing != null)
            {
                instance = existing;
                return existing;
            }

            GameObject host = new GameObject("PrototypeHitStopRuntime");
            instance = host.AddComponent<PrototypeHitStopController>();
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
        }

        private void Update()
        {
            if (!hitStopActive || Time.unscaledTime < hitStopEndsAt)
            {
                return;
            }

            RestoreTimeScale();
        }

        private void OnDestroy()
        {
            if (instance != this)
            {
                return;
            }

            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            RestoreTimeScale();
            instance = null;
        }

        private void OnDisable()
        {
            if (instance == this)
            {
                RestoreTimeScale();
            }
        }

        private void OnApplicationQuit()
        {
            RestoreTimeScale();
        }

        private void HandleSceneUnloaded(Scene _)
        {
            RestoreTimeScale();
        }

        private void ApplyRequest(float realTimeDuration, float relativeTimeScale)
        {
            if (!hitStopActive)
            {
                restoreTimeScale = Mathf.Max(0.0001f, Time.timeScale);
                hitStopActive = true;
            }

            hitStopEndsAt = Mathf.Max(hitStopEndsAt, Time.unscaledTime + realTimeDuration);
            float requestedScale = restoreTimeScale * Mathf.Clamp(relativeTimeScale, 0.01f, 1f);
            Time.timeScale = Mathf.Min(Time.timeScale, requestedScale);
        }

        private void RestoreTimeScale()
        {
            if (!hitStopActive)
            {
                return;
            }

            Time.timeScale = restoreTimeScale;
            hitStopActive = false;
            hitStopEndsAt = 0f;
            restoreTimeScale = 1f;
        }
    }
}
