using UnityEngine;

namespace JJKGame.CameraSystem
{
    /// <summary>Explicit camera lease. Normal controllers retain their own targets and settings.</summary>
    [DisallowMultipleComponent]
    public sealed class DomainCameraOverride : MonoBehaviour
    {
        private Object owner;
        private Camera view;
        private Vector3 savedPosition;
        private Quaternion savedRotation;
        private float savedFov;
        private bool held;
        public bool IsActive => held;
        public bool IsOwnedBy(Object requester) => held && owner == requester;
        public Vector3 ReturnPosition => savedPosition;
        public Quaternion ReturnRotation => savedRotation;
        public float ReturnFov => savedFov;
        public static bool IsOwned(Camera camera) => camera != null &&
            camera.TryGetComponent(out DomainCameraOverride lease) && lease.IsActive;

        public bool Acquire(Object requester)
        {
            if (IsActive || requester == null) return false;
            view = GetComponent<Camera>();
            if (view == null) return false;
            owner = requester;
            held = true;
            savedPosition = transform.position;
            savedRotation = transform.rotation;
            savedFov = view.fieldOfView;
            return true;
        }

        public void ShiftReturn(Vector3 delta) { savedPosition += delta; }

        public void Pose(Object requester, Vector3 position, Quaternion rotation, float fov)
        {
            if (owner != requester || !IsActive) return;
            transform.SetPositionAndRotation(position, rotation);
            view.fieldOfView = fov;
        }

        public void Release(Object requester)
        {
            if (owner != requester || !IsActive) return;
            Restore();
        }

        private void Restore()
        {
            if (view != null)
            {
                transform.SetPositionAndRotation(savedPosition, savedRotation);
                view.fieldOfView = savedFov;
            }
            owner = null;
            held = false;
        }

        private void LateUpdate() { if (held && owner == null) Restore(); }

        private void OnDisable() { if (IsActive) Restore(); }
        private void OnDestroy() { if (IsActive) Restore(); }
    }
}
