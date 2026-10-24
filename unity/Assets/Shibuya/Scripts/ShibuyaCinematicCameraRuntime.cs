using UnityEngine;

namespace JJKGame.Shibuya
{
    public sealed partial class ShibuyaOrbitCamera : IShibuyaCinematicView
    {
        private ShibuyaFighter viewOwner;
        private ShibuyaCinematicSession cinematicView;
        private float savedYaw, savedPitch, savedFov, returnDuration;
        private Vector3 savedFocus, transitionPosition;
        private Quaternion transitionRotation;
        private bool savedInitialized, returningView;
        private float transitionFov;
        private double transitionStarted;
        public bool IsCinematicView => cinematicView != null && cinematicView.Active;
        public ShibuyaFighter LocalOwner => viewOwner;
        public bool IsReturningView => returningView;
        public int CinematicShotIndex { get; private set; } = -1;

        private void RegisterView()
        {
            var owner = target == null ? null : target.GetComponent<ShibuyaFighter>();
            if (viewOwner == owner) return;
            if (IsCinematicView || returningView) RestoreViewImmediately();
            ResetLockOnFraming();
            if (viewOwner != null) viewOwner.UnregisterLocalView(this);
            viewOwner = owner;
            if (viewOwner != null && isActiveAndEnabled) viewOwner.RegisterLocalView(this);
        }
        public bool RequestCinematic(ShibuyaFighter localOwner, ShibuyaCinematicSession session)
        {
            if (!isActiveAndEnabled || session == null || !session.Active || localOwner == null || target != localOwner.transform
                || (session.Attacker != localOwner && session.Victim != localOwner) || IsCinematicView) return false;
            if (!returningView) {
                savedYaw = yaw; savedPitch = pitch; savedFocus = focus; savedInitialized = initialized;
                savedFov = GetComponent<Camera>().fieldOfView;
            }
            returningView = false;
            cinematicView = session; session.Ended += OnCinematicEnded;
            CinematicShotIndex = -1; SnapshotTransition();
            return true;
        }
        private void SnapshotTransition()
        {
            transitionPosition = transform.position; transitionRotation = transform.rotation;
            transitionFov = GetComponent<Camera>().fieldOfView; transitionStarted = Time.unscaledTimeAsDouble;
        }
        private void OnCinematicEnded(ShibuyaCinematicSession session)
        {
            session.Ended -= OnCinematicEnded; cinematicView = null;
            yaw = savedYaw; pitch = savedPitch; focus = savedFocus; initialized = savedInitialized;
            returnDuration = session.Profile == null ? 0 : session.Profile.blendOut;
            returningView = true; SnapshotTransition(); CinematicShotIndex = -1;
        }
        private bool UpdateCinematicView()
        {
            var camera = GetComponent<Camera>();
            if (IsCinematicView) {
                var session = cinematicView; var settings = session.Profile;
                float time = session.Elapsed; int index = 0;
                while (index < settings.shots.Length - 1 && time >= settings.shots[index].duration) { time -= settings.shots[index].duration; index++; }
                var shot = settings.shots[index];
                bool first = CinematicShotIndex < 0;
                if (index != CinematicShotIndex) { if (!first) SnapshotTransition(); CinematicShotIndex = index; }
                Transform a = session.Attacker.transform, b = session.Victim.transform;
                Vector3 anchor = shot.anchor == ShibuyaShotAnchor.Attacker ? a.position
                    : shot.anchor == ShibuyaShotAnchor.Victim ? b.position : (a.position + b.position) * .5f;
                Quaternion basis = Quaternion.Euler(0, a.eulerAngles.y, 0);
                Vector3 lookAt = anchor + basis * shot.lookOffset;
                Vector3 position = anchor + basis * (Quaternion.Euler(0, shot.angle, 0) * Vector3.back * shot.distance
                    + Vector3.up * shot.height + shot.positionOffset);
                position = ResolveCinematicWall(lookAt, position, settings.cameraCollisionRadius);
                float blend = first ? settings.blendIn : shot.blendTime;
                // Index 0 retains the entry envelope throughout its shot.
                if (index == 0) blend = settings.blendIn;
                float t = blend <= 0 ? 1 : Mathf.Clamp01((float)(Time.unscaledTimeAsDouble - transitionStarted) / blend);
                t = Mathf.SmoothStep(0, 1, t);
                Vector3 final = Vector3.Lerp(transitionPosition, position, t);
                final = ResolveCinematicWall(lookAt, final, settings.cameraCollisionRadius);
                foreach (var overlap in Physics.OverlapSphere(final, settings.cameraCollisionRadius * .95f, ~0, QueryTriggerInteraction.Ignore))
                    if (overlap.GetComponentInParent<ShibuyaFighter>() == null) { session.Finish("camera-obstructed"); return true; }
                var direction = lookAt - position;
                Quaternion rotation = direction.sqrMagnitude > .000001f ? Quaternion.LookRotation(direction) * Quaternion.Euler(shot.rotationOffset) : transitionRotation;
                transform.SetPositionAndRotation(final, Quaternion.Slerp(transitionRotation, rotation, t));
                camera.fieldOfView = Mathf.Lerp(transitionFov, shot.fieldOfView, t);
                return true;
            }
            if (!returningView) return false;
            Vector3 wanted = target.position + Vector3.up * profile.cameraHeight;
            Quaternion normalRotation = Quaternion.Euler(savedPitch, savedYaw, 0);
            Vector3 normal = CombatCameraPosition(wanted, normalRotation);
            float fraction = returnDuration <= 0 ? 1 : Mathf.Clamp01((float)(Time.unscaledTimeAsDouble-transitionStarted)/returnDuration);
            float eased = Mathf.SmoothStep(0, 1, fraction);
            transform.SetPositionAndRotation(ResolveCinematicWall(wanted, Vector3.Lerp(transitionPosition, normal, eased), profile.cameraCollisionRadius),
                Quaternion.Slerp(transitionRotation, normalRotation, eased));
            camera.fieldOfView = Mathf.Lerp(transitionFov, savedFov, eased);
            if (fraction >= 1) { returningView = false; focus = wanted; initialized = true; camera.fieldOfView = savedFov; ResumeLockOnFraming(); }
            return true;
        }
        private static Vector3 ResolveCinematicWall(Vector3 origin, Vector3 wanted, float radius)
        {
            Vector3 line = wanted - origin; float distance = line.magnitude;
            if (distance <= .000001f) return origin;
            foreach (var hit in Physics.SphereCastAll(origin, radius, line / distance, distance, ~0, QueryTriggerInteraction.Ignore))
                if (hit.collider.GetComponentInParent<ShibuyaFighter>() == null) distance = Mathf.Min(distance, Mathf.Max(0, hit.distance - .02f));
            return origin + line.normalized * distance;
        }
        private void OnEnable() { RegisterView(); }
        private void OnDisable()
        {
            if (viewOwner != null) viewOwner.UnregisterLocalView(this);
            viewOwner = null;
            RestoreViewImmediately();
            ResetLockOnFraming();
        }
        private void RestoreViewImmediately()
        {
            if (cinematicView != null) { cinematicView.Ended -= OnCinematicEnded; yaw = savedYaw; pitch = savedPitch; focus = savedFocus; initialized = savedInitialized; }
            if (cinematicView != null || returningView) GetComponent<Camera>().fieldOfView = savedFov;
            cinematicView = null; returningView = false; CinematicShotIndex = -1;
        }
    }
}
