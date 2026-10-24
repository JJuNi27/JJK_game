using UnityEngine;

namespace JJKGame.Shibuya
{
    // Presentation only. No fighter facing, aim source, input ownership or FOV writes.
    public sealed partial class ShibuyaOrbitCamera
    {
        private float lockYawVelocity, lockPitchVelocity, lockShoulder, lockHeight, lockDistance;
        private int lockShoulderSide = 1;
        private ShibuyaFighter framedTarget;
        private double lockResumeAt;
        private float collisionArmDistance = -1;
        private const float MaxCameraFrameStep = .05f; // Bound a background Editor stall's one-frame correction.

        private void ResetLockOnFraming()
        {
            ResetScreenRecovery();
            lockYawVelocity = lockPitchVelocity = lockShoulder = lockHeight = lockDistance = 0;
            framedTarget = null; lockShoulderSide = 1; lockResumeAt = 0; collisionArmDistance = -1;
        }
        private void ResumeLockOnFraming()
        {
            ResetScreenRecovery();
            lockYawVelocity = lockPitchVelocity = 0;
            lockResumeAt = Time.unscaledTimeAsDouble + (lockOn != null && lockOn.Settings != null ? lockOn.Settings.cameraReturnGrace : 0);
        }
        private void AssistLockOnCamera()
        {
            // LateUpdate already yields to the cinematic and return owner before reaching here.
            if (IsCinematicView || IsReturningView) return;
            float dt = Mathf.Min(Time.unscaledDeltaTime, MaxCameraFrameStep);
            var p = lockOn != null ? lockOn.Settings : null;
            bool active = p != null && p.IsValid && p.cameraAssistEnabled && p.cameraStrength > 0
                && lockOn.isActiveAndEnabled && lockOn.Owner != null && lockOn.Owner.transform == target
                && lockOn.LockedTarget != null;
            if (!active) {
                ResetScreenRecovery();
                lockYawVelocity = lockPitchVelocity = 0; framedTarget = null;
                float speed = p != null && p.IsValid ? p.cameraFramingSpeed : profile.cameraFollowSharpness;
                MoveLockFraming(0, 0, 0, speed * dt); return;
            }
            if (lockOn.IsSearching || Time.unscaledTimeAsDouble-lastManualLook < p.manualLookGrace
                || Time.unscaledTimeAsDouble < lockResumeAt) {
                ResetScreenRecovery();
                lockYawVelocity = lockPitchVelocity = 0; return; // Freeze framing while the user owns the view.
            }
            var camera = GetComponent<Camera>();
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            // Use the collision-shortened arm too: a wall changes projected size/dead-zone
            // coverage even though the requested orbit distance has not changed.
            float visibleArm = collisionArmDistance < 0 ? profile.cameraDistance+lockDistance : collisionArmDistance;
            Vector3 anchor = CombatCameraSegment(focus,focus+LockFocusOffset(rotation));
            Vector3 position = CombatCameraSegment(anchor,anchor+rotation*Vector3.back*visibleArm);
            Vector3 screen = ProjectLockPoint(lockOn.TargetPoint, position, rotation, camera);
            float x = Mathf.Clamp(screen.x, .5f-p.cameraDeadZone.x, .5f+p.cameraDeadZone.x);
            float y = Mathf.Clamp(screen.y, .5f-p.cameraDeadZone.y, .5f+p.cameraDeadZone.y);
            float tan = Mathf.Tan(camera.fieldOfView*.5f*Mathf.Deg2Rad);
            float yawError, pitchError;
            if (screen.z > .01f) {
                yawError = (Mathf.Atan((screen.x-.5f)*2*tan*camera.aspect)-Mathf.Atan((x-.5f)*2*tan*camera.aspect))*Mathf.Rad2Deg;
                pitchError = -(Mathf.Atan((screen.y-.5f)*2*tan)-Mathf.Atan((y-.5f)*2*tan))*Mathf.Rad2Deg;
            } else {
                Vector3 direction = lockOn.TargetPoint-position;
                yawError = Mathf.DeltaAngle(yaw, Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg);
                pitchError = -Mathf.Atan2(direction.y,new Vector2(direction.x,direction.z).magnitude)*Mathf.Rad2Deg-pitch;
            }
            var mine = target.GetComponent<CharacterController>().bounds;
            var theirs = lockOn.LockedTarget.GetComponent<CharacterController>().bounds;
            float overlap = LockScreenOverlap(mine, theirs, position, rotation, camera);
            if (framedTarget != lockOn.LockedTarget) {
                framedTarget = lockOn.LockedTarget;
                float ownerX = ProjectLockPoint(mine.center, position, rotation, camera).x;
                lockShoulderSide = screen.x >= ownerX ? 1 : -1; // Latch per selection; no per-frame side flips.
            }
            float range = Vector3.Distance(mine.center,theirs.center);
            float near = p.cameraFramingRange <= 0 ? 0 : 1-Mathf.Clamp01(range/p.cameraFramingRange);
            bool recovering = AssistScreenRecovery(camera, p, ref yawError, ref pitchError);
            if (!recovering && overlap > .15f && near > 0 && p.cameraOverlapStrength > 0) {
                Vector3 flat = theirs.center-mine.center; flat.y = 0;
                if (flat.sqrMagnitude > .000001f) {
                    float sideYaw = Mathf.Atan2(flat.x,flat.z)*Mathf.Rad2Deg
                        -lockShoulderSide*p.cameraOverlapOrbitAngle;
                    float sideError = Mathf.DeltaAngle(yaw, sideYaw)*p.cameraOverlapStrength*near;
                    // Only resolve the overlap while the opponent is otherwise visible.
                    // Screen-edge recovery takes precedence over the side composition.
                    if (Mathf.Abs(yawError) < .001f) yawError = sideError;
                }
            }
            yaw = Mathf.Repeat(yaw+LockRotationStep(yawError,ref lockYawVelocity,p,dt),360);
            pitch = Mathf.Clamp(pitch+LockRotationStep(pitchError,ref lockPitchVelocity,p,dt),profile.minimumPitch,profile.maximumPitch);

            // Keep the evaluated orbit stable during recovery. Ordinary framing resumes after
            // actual output visibility has held, rather than fighting recovery with new offsets.
            if (recovering) return;

            // Use the unshifted pose for offset demand: the correction cannot oscillate simply
            // because its own offset cleared the overlap. Yaw is allowed to settle off axis.
            rotation = Quaternion.Euler(pitch,yaw,0);
            Vector3 basePosition = focus + rotation*Vector3.back*profile.cameraDistance;
            float baseOverlap = LockScreenOverlap(mine,theirs,basePosition,rotation,camera);
            float coverage = baseOverlap*p.cameraOverlapStrength*near*p.cameraStrength;
            float height = Mathf.Clamp((theirs.center.y-mine.center.y)*p.cameraVerticalFocusWeight,
                -p.cameraVerticalFocusLimit,p.cameraVerticalFocusLimit)*p.cameraStrength;
            MoveLockFraming(lockShoulderSide*p.cameraShoulderOffset*coverage,height,
                p.cameraExtraDistance*coverage,p.cameraFramingSpeed*dt);
        }
        private static float LockRotationStep(float error, ref float velocity, ShibuyaLockOnProfile p, float dt)
        {
            float wanted = Mathf.Clamp(error*p.cameraSharpness*p.cameraStrength,-p.cameraDegreesPerSecond,p.cameraDegreesPerSecond);
            velocity = Mathf.MoveTowards(velocity,wanted,p.cameraAcceleration*dt);
            // No residual drift into a dead zone, or overshoot after a target reverses direction.
            if (Mathf.Abs(error) < .001f || Mathf.Sign(velocity) != Mathf.Sign(error)) return 0;
            return Mathf.Sign(error)*Mathf.Min(Mathf.Abs(error),Mathf.Abs(velocity)*dt);
        }
        private void MoveLockFraming(float shoulder, float height, float distance, float step)
        {
            lockShoulder = Mathf.MoveTowards(lockShoulder,shoulder,step);
            lockHeight = Mathf.MoveTowards(lockHeight,height,step);
            lockDistance = Mathf.MoveTowards(lockDistance,distance,step);
        }
        private Vector3 LockFocusOffset(Quaternion rotation) => rotation*Vector3.right*lockShoulder + Vector3.up*lockHeight;
        private Vector3 CombatCameraPosition(Vector3 baseFocus, Quaternion rotation)
        {
            var position = EvaluateCombatCameraPosition(baseFocus, rotation, out var arm);
            collisionArmDistance = arm;
            return position;
        }
        // Pure pose evaluation: candidate queries must not advance the collision recovery clock.
        private Vector3 EvaluateCombatCameraPosition(Vector3 baseFocus, Quaternion rotation, out float arm)
        {
            Vector3 anchor = CombatCameraSegment(baseFocus,baseFocus+LockFocusOffset(rotation));
            Vector3 position = CombatCameraSegment(anchor,anchor+rotation*Vector3.back*(profile.cameraDistance+lockDistance));
            float clearance = Vector3.Distance(anchor,position);
            float speed = lockOn != null && lockOn.Settings != null && lockOn.Settings.IsValid
                ? lockOn.Settings.cameraFramingSpeed : profile.cameraFollowSharpness;
            arm = collisionArmDistance < 0 ? clearance
                : Mathf.Min(clearance,Mathf.MoveTowards(collisionArmDistance,clearance,speed*Mathf.Min(Time.unscaledDeltaTime,MaxCameraFrameStep)));
            position = anchor+rotation*Vector3.back*arm;
            // Also check the diagonal from the unshifted character anchor at corners.
            return LockFocusOffset(rotation).sqrMagnitude>.000001f ? CombatCameraSegment(baseFocus,position) : position;
        }
        private Vector3 CombatCameraSegment(Vector3 origin, Vector3 wanted)
        {
            Vector3 line = wanted-origin; float distance = line.magnitude;
            if(distance < .000001f) return origin;
            foreach(var hit in Physics.SphereCastAll(origin,profile.cameraCollisionRadius,line/distance,distance,~0,QueryTriggerInteraction.Ignore))
                if(hit.collider.GetComponentInParent<ShibuyaFighter>()==null)
                    distance=Mathf.Min(distance,Mathf.Max(0,hit.distance-.08f));
            return origin+line.normalized*distance;
        }
        // Evaluate the desired orbit pose, not a stale transform left by a cinematic or collision.
        private static Vector3 ProjectLockPoint(Vector3 world, Vector3 position, Quaternion rotation, Camera camera)
        {
            Vector3 local = Quaternion.Inverse(rotation)*(world-position);
            float z = Mathf.Max(.01f,local.z), tan = Mathf.Max(.0001f,Mathf.Tan(camera.fieldOfView*.5f*Mathf.Deg2Rad));
            return new Vector3(.5f+local.x/(2*z*tan*Mathf.Max(.01f,camera.aspect)),.5f+local.y/(2*z*tan),local.z);
        }
        private static bool ProjectLockBounds(Bounds bounds, Vector3 position, Quaternion rotation, Camera camera, out Rect rect)
        {
            Vector2 min = new Vector2(float.PositiveInfinity,float.PositiveInfinity), max = -min;
            for(int i=0;i<8;i++) {
                Vector3 corner=bounds.center+Vector3.Scale(bounds.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
                Vector3 point=ProjectLockPoint(corner,position,rotation,camera);
                if(point.z<=.01f) {rect=default;return false;}
                min=Vector2.Min(min,point);max=Vector2.Max(max,point);
            }
            rect=Rect.MinMaxRect(min.x,min.y,max.x,max.y);return true;
        }
        private static float LockScreenOverlap(Bounds mine, Bounds theirs, Vector3 position, Quaternion rotation, Camera camera)
        {
            if ((Quaternion.Inverse(rotation)*(theirs.center-mine.center)).z<=0
                || !ProjectLockBounds(mine,position,rotation,camera,out var a)
                || !ProjectLockBounds(theirs,position,rotation,camera,out var b)) return 0;
            float x=Mathf.Max(0,Mathf.Min(a.xMax,b.xMax)-Mathf.Max(a.xMin,b.xMin));
            float y=Mathf.Max(0,Mathf.Min(a.yMax,b.yMax)-Mathf.Max(a.yMin,b.yMin));
            return Mathf.Clamp01(x*y/Mathf.Max(.000001f,Mathf.Min(a.width*a.height,b.width*b.height)));
        }
    }
}
