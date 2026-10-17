using UnityEngine;

namespace JJKGame.Shibuya
{
    [RequireComponent(typeof(Camera))]
    public sealed class ShibuyaOrbitCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private ShibuyaCombatProfile profile;
        private float yaw;
        private float pitch = 16f;
        private Vector3 focus;
        private bool initialized;
        public void Configure(Transform actor, ShibuyaCombatProfile data) { target = actor; profile = data; }
        public void ApplyLook(Vector2 look)
        {
            yaw = Mathf.Repeat(yaw + look.x * profile.mouseSensitivity, 360f);
            pitch = Mathf.Clamp(pitch - look.y * profile.mouseSensitivity, profile.minimumPitch, profile.maximumPitch);
        }
        private void LateUpdate()
        {
            if (target == null || profile == null) return;
            Vector3 wanted = target.position + Vector3.up * profile.cameraHeight;
            focus = initialized ? Vector3.Lerp(focus, wanted,
                1f - Mathf.Exp(-profile.cameraFollowSharpness * Time.deltaTime)) : wanted;
            initialized = true;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 direction = rotation * Vector3.back;
            float distance = profile.cameraDistance;
            foreach (RaycastHit hit in Physics.SphereCastAll(focus, profile.cameraCollisionRadius,
                direction, distance, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.GetComponentInParent<ShibuyaFighter>() != null) continue;
                distance = Mathf.Min(distance, Mathf.Max(0.05f, hit.distance - 0.08f));
            }
            transform.SetPositionAndRotation(focus + direction * distance, rotation);
        }
    }
}
