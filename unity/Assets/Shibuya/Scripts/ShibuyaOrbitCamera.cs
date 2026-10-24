using UnityEngine;

namespace JJKGame.Shibuya
{
    [RequireComponent(typeof(Camera))]
    public sealed partial class ShibuyaOrbitCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private ShibuyaCombatProfile profile;
        private float yaw;
        private float pitch = 16f;
        private Vector3 focus;
        private bool initialized;
        public void Configure(Transform actor, ShibuyaCombatProfile data) { target = actor; profile = data; RegisterView(); }
        public void ApplyLook(Vector2 look)
        {
            if (IsCinematicView || returningView) return;
            if (look.sqrMagnitude > .000001f) lastManualLook = Time.unscaledTimeAsDouble;
            yaw = Mathf.Repeat(yaw + look.x * profile.mouseSensitivity, 360f);
            pitch = Mathf.Clamp(pitch - look.y * profile.mouseSensitivity, profile.minimumPitch, profile.maximumPitch);
        }
        private void LateUpdate()
        {
            if (target == null || profile == null) return;
            RegisterView();
            if (UpdateCinematicView()) return;
            Vector3 wanted = target.position + Vector3.up * profile.cameraHeight;
            focus = initialized ? Vector3.Lerp(focus, wanted,
                1f - Mathf.Exp(-profile.cameraFollowSharpness * Time.deltaTime)) : wanted;
            initialized = true;
            AssistLockOnCamera();
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.SetPositionAndRotation(CombatCameraPosition(focus, rotation), rotation);
        }
    }
}
