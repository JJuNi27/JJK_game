using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JJKGame.CameraSystem
{
    public sealed class SimpleCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 6.5f, -8.5f);
        [SerializeField, Min(0.1f)] private float followSmoothness = 8f;
        [SerializeField, Min(0.1f)] private float lookHeight = 1.4f;

        [Header("Combat Presentation Feedback")]
        [SerializeField, Range(0f, 1f)] private float maximumShakeAmplitude = 0.75f;
        [SerializeField, Range(0f, 0.75f)] private float maximumFocusStrength = 0.58f;
        [SerializeField, Min(1f)] private float maximumFocusDistance = 22f;
        [SerializeField, Range(8f, 40f)] private float shakeNoiseFrequency = 24f;
        [SerializeField, Range(0f, 12f)] private float maximumOutwardFovKick = 10f;
        [SerializeField, Range(0f, 8f)] private float maximumInwardFovKick = 5.5f;

        private readonly List<TimedImpulse> shakeImpulses = new List<TimedImpulse>(6);
        private readonly List<TimedImpulse> fovImpulses = new List<TimedImpulse>(6);

        private float flashStartedAt;
        private float flashEndsAt;
        private float flashPeakAlpha;
        private Color flashColor = Color.white;
        private GameObject flashOverlayRoot;
        private RawImage flashOverlay;

        private Camera controlledCamera;
        private float baseFieldOfView;

        private float focusStartedAt;
        private float focusEndsAt;
        private float focusStrength;
        private float currentFocusStrength;
        private Vector3 focusWorldPoint;

        private readonly struct TimedImpulse
        {
            public TimedImpulse(float magnitude, float startedAt, float endsAt)
            {
                Magnitude = magnitude;
                StartedAt = startedAt;
                EndsAt = endsAt;
            }

            public float Magnitude { get; }
            public float StartedAt { get; }
            public float EndsAt { get; }
        }

        private void Awake()
        {
            controlledCamera = GetComponent<Camera>();
            if (controlledCamera != null)
            {
                baseFieldOfView = controlledCamera.fieldOfView;
            }

            BuildFlashOverlay();
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void AddShake(float amplitude, float duration)
        {
            if (amplitude <= 0f || duration <= 0f)
            {
                return;
            }

            float now = Time.unscaledTime;
            shakeImpulses.Add(
                new TimedImpulse(
                    Mathf.Clamp(amplitude, 0f, maximumShakeAmplitude),
                    now,
                    now + duration
                )
            );
        }

        public void Flash(Color color, float peakAlpha, float duration)
        {
            if (peakAlpha <= 0f || duration <= 0f)
            {
                return;
            }

            float now = Time.unscaledTime;
            flashColor = color;
            flashPeakAlpha = Mathf.Clamp01(Mathf.Max(flashPeakAlpha, peakAlpha));
            flashStartedAt = now;
            flashEndsAt = Mathf.Max(flashEndsAt, now + duration);
            if (flashOverlayRoot != null)
            {
                flashOverlayRoot.SetActive(true);
            }
        }

        public void AddFovKick(float delta, float duration)
        {
            if (Mathf.Approximately(delta, 0f) || duration <= 0f)
            {
                return;
            }

            controlledCamera ??= GetComponent<Camera>();
            if (controlledCamera == null)
            {
                return;
            }

            if (baseFieldOfView <= 0f)
            {
                baseFieldOfView = controlledCamera.fieldOfView;
            }

            float now = Time.unscaledTime;
            fovImpulses.Add(new TimedImpulse(delta, now, now + duration));
        }

        public void AddWorldFocus(Vector3 worldPoint, float strength, float duration)
        {
            if (strength <= 0f || duration <= 0f)
            {
                return;
            }

            if (!IsFinite(worldPoint))
            {
                return;
            }

            Vector3 safeWorldPoint = ClampFocusPoint(worldPoint);
            float now = Time.unscaledTime;
            focusStartedAt = now;
            focusEndsAt = now + duration;
            focusStrength = Mathf.Clamp(strength, 0f, maximumFocusStrength);
            focusWorldPoint = currentFocusStrength > 0.001f
                ? Vector3.Lerp(focusWorldPoint, safeWorldPoint, 0.65f)
                : safeWorldPoint;
        }

        private void LateUpdate()
        {
            ApplyFovKick();
            UpdateFlashOverlay();

            if (target == null)
            {
                return;
            }

            // Keep the camera on a stable world-space offset instead of rotating
            // the offset with the player. This prevents movement/camera feedback loops.
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(
                transform.position,
                desiredPosition,
                1f - Mathf.Exp(-followSmoothness * Time.deltaTime)
            );

            transform.position = smoothedPosition + BuildShakeOffset();
            Vector3 lookPoint = BuildLookPoint();
            Vector3 lookDirection = lookPoint - transform.position;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            }
        }

        private Vector3 BuildLookPoint()
        {
            Vector3 baseLookPoint = target.position + Vector3.up * lookHeight;
            float now = Time.unscaledTime;
            float desiredStrength = 0f;
            if (now < focusEndsAt && focusStrength > 0f)
            {
                float duration = Mathf.Max(0.01f, focusEndsAt - focusStartedAt);
                float progress = Mathf.Clamp01((now - focusStartedAt) / duration);
                desiredStrength = focusStrength * Mathf.Sin(progress * Mathf.PI);
            }

            currentFocusStrength = Mathf.MoveTowards(
                currentFocusStrength,
                desiredStrength,
                3.5f * Time.unscaledDeltaTime
            );
            if (currentFocusStrength <= 0.0001f)
            {
                currentFocusStrength = 0f;
                if (now >= focusEndsAt)
                {
                    focusStrength = 0f;
                }
                return baseLookPoint;
            }

            focusWorldPoint = ClampFocusPoint(focusWorldPoint);
            return Vector3.Lerp(baseLookPoint, focusWorldPoint, currentFocusStrength);
        }

        private void ApplyFovKick()
        {
            controlledCamera ??= GetComponent<Camera>();
            if (controlledCamera == null || baseFieldOfView <= 0f)
            {
                return;
            }

            float now = Time.unscaledTime;
            float combinedOffset = 0f;
            for (int index = fovImpulses.Count - 1; index >= 0; index--)
            {
                TimedImpulse impulse = fovImpulses[index];
                if (now >= impulse.EndsAt)
                {
                    fovImpulses.RemoveAt(index);
                    continue;
                }

                float duration = Mathf.Max(0.01f, impulse.EndsAt - impulse.StartedAt);
                float progress = Mathf.Clamp01((now - impulse.StartedAt) / duration);
                combinedOffset += impulse.Magnitude * Mathf.Sin(progress * Mathf.PI);
            }

            combinedOffset = Mathf.Clamp(
                combinedOffset,
                -maximumInwardFovKick,
                maximumOutwardFovKick
            );
            controlledCamera.fieldOfView = Mathf.Clamp(
                baseFieldOfView + combinedOffset,
                25f,
                100f
            );
        }

        private Vector3 BuildShakeOffset()
        {
            float now = Time.unscaledTime;
            float accumulatedEnergy = 0f;
            for (int index = shakeImpulses.Count - 1; index >= 0; index--)
            {
                TimedImpulse impulse = shakeImpulses[index];
                if (now >= impulse.EndsAt)
                {
                    shakeImpulses.RemoveAt(index);
                    continue;
                }

                float duration = Mathf.Max(0.01f, impulse.EndsAt - impulse.StartedAt);
                float progress = Mathf.Clamp01((now - impulse.StartedAt) / duration);
                float attack = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / 0.12f));
                float decay = 1f - progress;
                float contribution = impulse.Magnitude * attack * decay * decay;
                accumulatedEnergy += contribution * contribution;
            }

            float amplitude = Mathf.Min(Mathf.Sqrt(accumulatedEnergy), maximumShakeAmplitude);
            if (amplitude <= 0.0001f)
            {
                return Vector3.zero;
            }

            float sample = now * shakeNoiseFrequency;
            float noiseX = Mathf.PerlinNoise(13.37f, sample) * 2f - 1f;
            float noiseY = Mathf.PerlinNoise(47.11f, sample + 19.7f) * 2f - 1f;
            Vector2 continuousNoise = Vector2.ClampMagnitude(new Vector2(noiseX, noiseY), 1f);
            return transform.right * continuousNoise.x * amplitude
                + Vector3.up * continuousNoise.y * amplitude;
        }

        private void OnDisable()
        {
            shakeImpulses.Clear();
            fovImpulses.Clear();
            focusStrength = 0f;
            currentFocusStrength = 0f;
            flashPeakAlpha = 0f;
            flashEndsAt = 0f;
            if (flashOverlayRoot != null)
            {
                flashOverlayRoot.SetActive(false);
            }
            if (controlledCamera != null && baseFieldOfView > 0f)
            {
                controlledCamera.fieldOfView = baseFieldOfView;
            }
        }

        private Vector3 ClampFocusPoint(Vector3 worldPoint)
        {
            if (target == null)
            {
                return worldPoint;
            }

            Vector3 baseLookPoint = target.position + Vector3.up * lookHeight;
            Vector3 offsetFromPlayer = worldPoint - baseLookPoint;
            if (offsetFromPlayer.sqrMagnitude > maximumFocusDistance * maximumFocusDistance)
            {
                offsetFromPlayer = offsetFromPlayer.normalized * maximumFocusDistance;
            }
            return baseLookPoint + offsetFromPlayer;
        }

        private static bool IsFinite(Vector3 value)
        {
            return float.IsFinite(value.x)
                && float.IsFinite(value.y)
                && float.IsFinite(value.z);
        }

        private void BuildFlashOverlay()
        {
            flashOverlayRoot = new GameObject(
                "CombatFlashOverlay",
                typeof(RectTransform),
                typeof(Canvas)
            );
            flashOverlayRoot.transform.SetParent(transform, false);

            Canvas canvas = flashOverlayRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            GameObject imageObject = new GameObject("Flash", typeof(RectTransform), typeof(RawImage));
            imageObject.transform.SetParent(flashOverlayRoot.transform, false);
            RectTransform imageRect = imageObject.GetComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero;

            flashOverlay = imageObject.GetComponent<RawImage>();
            flashOverlay.texture = Texture2D.whiteTexture;
            flashOverlay.raycastTarget = false;
            flashOverlayRoot.SetActive(false);
        }

        private void UpdateFlashOverlay()
        {
            if (flashOverlay == null || flashOverlayRoot == null)
            {
                return;
            }

            float now = Time.unscaledTime;
            if (now >= flashEndsAt)
            {
                flashPeakAlpha = 0f;
                flashOverlayRoot.SetActive(false);
                return;
            }

            float duration = Mathf.Max(0.01f, flashEndsAt - flashStartedAt);
            float progress = Mathf.Clamp01((now - flashStartedAt) / duration);
            float attack = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress / 0.12f));
            float decay = 1f - progress;
            Color current = flashColor;
            current.a = flashPeakAlpha * attack * decay;
            flashOverlay.color = current;
        }
    }
}
