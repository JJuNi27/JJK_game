using UnityEngine;

namespace JJKGame.CameraSystem
{
    public sealed class SimpleCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 6.5f, -8.5f);
        [SerializeField, Min(0.1f)] private float followSmoothness = 8f;
        [SerializeField, Min(0.1f)] private float lookHeight = 1.4f;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            // Keep the camera on a stable world-space offset instead of rotating
            // the offset with the player. This prevents feedback loops where
            // moving backward turns the player, which then swings the camera,
            // which changes the camera-relative movement direction again.
            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                1f - Mathf.Exp(-followSmoothness * Time.deltaTime)
            );

            Vector3 lookPoint = target.position + Vector3.up * lookHeight;
            transform.rotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
        }
    }
}
