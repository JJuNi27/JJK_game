using UnityEngine;

namespace JJKGame.CameraSystem
{
    public sealed class SimpleCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 6.5f, -8.5f);
        [SerializeField, Min(0.1f)] private float followSmoothness = 8f;
        [SerializeField, Min(0.1f)] private float lookHeight = 1.4f;

        [Header("Prototype Combat Feedback")]
        [SerializeField, Range(0f, 1f)] private float maximumShakeAmplitude = 0.75f;

        private float shakeStartedAt;
        private float shakeEndsAt;
        private float shakeAmplitude;
        private float flashStartedAt;
        private float flashEndsAt;
        private float flashPeakAlpha;
        private Color flashColor = Color.white;

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
            shakeStartedAt = now;
            shakeEndsAt = Mathf.Max(shakeEndsAt, now + duration);
            shakeAmplitude = Mathf.Clamp(
                Mathf.Max(shakeAmplitude, amplitude),
                0f,
                maximumShakeAmplitude
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
        }

        private void LateUpdate()
        {
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
            Vector3 lookPoint = target.position + Vector3.up * lookHeight;
            transform.rotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
        }

        private Vector3 BuildShakeOffset()
        {
            float now = Time.unscaledTime;
            if (now >= shakeEndsAt)
            {
                shakeAmplitude = 0f;
                return Vector3.zero;
            }

            float duration = Mathf.Max(0.01f, shakeEndsAt - shakeStartedAt);
            float remaining = Mathf.Clamp01((shakeEndsAt - now) / duration);
            Vector2 random = Random.insideUnitCircle * shakeAmplitude * remaining;
            return transform.right * random.x + Vector3.up * random.y;
        }

        private void OnGUI()
        {
            float now = Time.unscaledTime;
            if (now >= flashEndsAt)
            {
                flashPeakAlpha = 0f;
                return;
            }

            float duration = Mathf.Max(0.01f, flashEndsAt - flashStartedAt);
            float remaining = Mathf.Clamp01((flashEndsAt - now) / duration);
            Color previousColor = GUI.color;
            Color current = flashColor;
            current.a = flashPeakAlpha * remaining;
            GUI.color = current;
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previousColor;
        }
    }
}
