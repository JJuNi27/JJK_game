using UnityEngine;

namespace JJKGame.Dev.VFXLab
{
    /// <summary>
    /// Developer inspection camera. RMB drag orbits, wheel zooms, and Home resets.
    /// It uses unscaled time so inspection remains available while preview is paused.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public sealed class VfxLabOrbitCamera : MonoBehaviour
    {
        private const float DefaultYaw = 0f;
        private const float DefaultPitch = 22f;
        private const float DefaultDistance = 10f;

        private Transform characterTarget;
        private Transform vfxTarget;
        private float yaw = DefaultYaw;
        private float pitch = DefaultPitch;
        private float distance = DefaultDistance;
        private Vector3 smoothedFocus;
        private bool hasFocus;

        public void Configure(Transform character, Transform previewPoint)
        {
            characterTarget = character;
            vfxTarget = previewPoint;
            ResetView();
        }

        private void LateUpdate()
        {
            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * 4.2f;
                pitch = Mathf.Clamp(
                    pitch - Input.GetAxis("Mouse Y") * 3.2f,
                    8f,
                    72f
                );
            }

            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.001f)
            {
                distance = Mathf.Clamp(distance - scroll * 0.85f, 2.6f, 22f);
            }
            if (Input.GetKeyDown(KeyCode.Home))
            {
                ResetView();
            }

            Vector3 focus = ResolveFocus();
            float smoothing = 1f - Mathf.Exp(-10f * Time.unscaledDeltaTime);
            smoothedFocus = hasFocus
                ? Vector3.Lerp(smoothedFocus, focus, smoothing)
                : focus;
            hasFocus = true;

            Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = smoothedFocus - orbit * Vector3.forward * distance;
            transform.rotation = Quaternion.LookRotation(
                smoothedFocus - transform.position,
                Vector3.up
            );
        }

        private Vector3 ResolveFocus()
        {
            Vector3 characterPoint = characterTarget != null
                ? characterTarget.position + Vector3.up * 1.1f
                : Vector3.up;
            Vector3 effectPoint = vfxTarget != null
                ? vfxTarget.position + Vector3.up * 0.35f
                : characterPoint + Vector3.forward * 3.5f;
            return Vector3.Lerp(characterPoint, effectPoint, 0.48f);
        }

        private void ResetView()
        {
            yaw = DefaultYaw;
            pitch = DefaultPitch;
            distance = DefaultDistance;
            smoothedFocus = ResolveFocus();
            hasFocus = true;
        }
    }
}
