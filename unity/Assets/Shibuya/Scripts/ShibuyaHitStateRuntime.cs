using UnityEngine;

namespace JJKGame.Shibuya
{
    public sealed partial class ShibuyaFighter
    {
        private bool airborne, downed, gettingUp, downLanded, apexVisited;
        private ShibuyaAirSettings airSettings;
        private ShibuyaDownSettings downSettings;
        private float apexUntil, downStartedAt, getUpStartedAt;
        private Vector3 airDirection;
        private float airDistanceRemaining, airTravelTime;
        private ShibuyaGroundImpact groundImpact;
        public bool IsAirborne => airborne;
        public bool IsDowned => downed;
        public bool IsGettingUp => gettingUp;
        public float VerticalVelocity => verticalSpeed;
        public float LandingSeparationRequested { get; private set; }
        public bool CanRequestDownRecovery => downed && downLanded && !gettingUp
            && !Health.IsDead && !stopped && combatTime >= downStartedAt + downSettings.minimumDownTime;
        public float DownPoseWeight => !downed ? 0f : !gettingUp ? 1f
            : 1f - Mathf.SmoothStep(0, 1, (combatTime - getUpStartedAt) / Mathf.Max(.01f, downSettings.getUpDuration));
        public float DownPoseAngle => downSettings == null ? 0f : downSettings.downPoseAngle;
        public float AirReactionLean => airborne ? airSettings.airReactionLean : 0f;
        private bool IsActionLocked => IsStunned || airborne || downed;

        private void ApplyHitOutcome(ShibuyaHitEvent hit, Vector3 hitDirection)
        {
            if (Health.IsDead || stopped || hit.outcome == ShibuyaHitOutcome.Normal) return;
            CancelCombo(); ClearVictimAlignment(); ClearIncomingLink(); jumpIntent = false;
            if (hit.outcome == ShibuyaHitOutcome.Launch) {
                downed = gettingUp = downLanded = false;
                airSettings = hit.air;
                airborne = true; apexVisited = false; apexUntil = 0;
                verticalSpeed = hit.air.usePeakHeight
                    ? Mathf.Sqrt(2f * hit.air.airGravity * hit.air.peakHeight) : Mathf.Max(.1f, hit.air.launchSpeed);
                hitDirection.y = 0;
                airDirection = hitDirection.sqrMagnitude > .000001f ? hitDirection.normalized : -transform.forward;
                airDistanceRemaining = hit.air.launchHorizontalDistance;
                airTravelTime = 0;
                LandingSeparationRequested = 0;
                knockback = Vector3.zero;
            } else {
                airborne = false;
                if (downed) return; // Additional hits must not indefinitely restart the down timer.
                downSettings = hit.down; downed = true; gettingUp = false;
                downLanded = false; // A grounded CC is not proof of a valid floor (it can be another fighter).
                downStartedAt = combatTime;
                verticalSpeed = Mathf.Min(verticalSpeed, -Mathf.Max(.1f, hit.down.fallSpeed));
            }
        }

        private Vector3 AdvanceAirHorizontal(float delta)
        {
            if (!airborne) return Vector3.zero;
            float travelDelta = Mathf.Min(delta, Mathf.Max(0, airSettings.launchHorizontalDuration-airTravelTime));
            airTravelTime += delta;
            float amount = Mathf.Min(airDistanceRemaining,
                airSettings.launchHorizontalDistance/Mathf.Max(.01f,airSettings.launchHorizontalDuration)*travelDelta);
            airDistanceRemaining -= amount;
            if (verticalSpeed < 0) amount += airSettings.descendingHorizontalSpeed*delta;
            return airDirection*amount;
        }

        // Begin lateral clearance before a descending capsule can land on another capsule's head.
        // This is finite physical motion through the same controller, never a position assignment.
        private Vector3 SeparateLandingCapsules(Vector3 displacement, float delta)
        {
            if ((!airborne && !downed) || verticalSpeed > 0) return displacement;
            float speed = airSettings == null ? profile.walkSpeed : airSettings.landingSeparationSpeed;
            float budget = Mathf.Max(0,speed)*delta;
            Vector3 predicted = motor.bounds.center + displacement;
            foreach(var collider in Physics.OverlapSphere(predicted,motor.height+motor.radius,~0,QueryTriggerInteraction.Ignore)) {
                var other=collider.GetComponentInParent<ShibuyaFighter>();
                if(other==null || other==this || collider!=other.motor) continue;
                if(motor.bounds.min.y+displacement.y>other.motor.bounds.max.y+motor.radius*2f
                    || motor.bounds.max.y+displacement.y<other.motor.bounds.min.y) continue;
                Vector3 flat=predicted-other.motor.bounds.center; flat.y=0;
                float needed=motor.radius+other.motor.radius+motor.skinWidth*2f-flat.magnitude;
                if(needed<=0) continue;
                Vector3 away=flat.sqrMagnitude>.000001f?flat.normalized:airDirection;
                if(away.sqrMagnitude<.000001f) away=transform.right;
                float amount=Mathf.Min(needed,budget);
                Vector3 correction=away*amount;
                if(WorldBlocks(transform.position+Vector3.up*motor.height*.5f,
                    transform.position+correction+Vector3.up*motor.height*.5f)) continue;
                displacement+=correction; predicted+=correction; budget-=amount;
                LandingSeparationRequested+=amount;
            }
            return displacement;
        }

        private bool TryLandingSurface(out RaycastHit surface)
        {
            surface=default; float nearest=float.PositiveInfinity;
            float foot=motor.bounds.min.y;
            foreach(var hit in Physics.RaycastAll(new Vector3(motor.bounds.center.x,foot+.15f,motor.bounds.center.z),
                Vector3.down,.4f+motor.skinWidth,~0,QueryTriggerInteraction.Ignore)) {
                if(hit.collider.GetComponentInParent<ShibuyaFighter>()!=null
                    || Vector3.Dot(hit.normal,Vector3.up)<Mathf.Cos(motor.slopeLimit*Mathf.Deg2Rad)) continue;
                if(hit.distance<nearest) { surface=hit; nearest=hit.distance; }
            }
            return nearest<float.PositiveInfinity;
        }

        private void UpdateVerticalMotion(float delta, bool actionLocked)
        {
            if (motor.isGrounded && verticalSpeed < 0f && !airborne) verticalSpeed = -2f;
            if (!actionLocked && !attacking && jumpIntent && motor.isGrounded)
                verticalSpeed = Mathf.Sqrt(2f * Mathf.Max(.1f, profile.gravity) * Mathf.Max(.1f, profile.jumpHeight));
            jumpIntent = false;
            if (airborne && apexVisited && combatTime < apexUntil) { verticalSpeed = 0; return; }
            float gravity = airborne ? airSettings.airGravity : profile.gravity;
            float previous = verticalSpeed;
            verticalSpeed -= Mathf.Max(.1f, gravity) * delta;
            if (airborne && !apexVisited && previous > 0 && verticalSpeed <= 0) {
                apexVisited = true;
                apexUntil = combatTime + Mathf.Max(0, airSettings.apexHoldTime);
                verticalSpeed = 0;
            }
        }

        private void FinishVerticalMotion(CollisionFlags collisions)
        {
            if ((collisions & CollisionFlags.Above) != 0 && verticalSpeed > 0) {
                verticalSpeed = 0; apexVisited = true; apexUntil = combatTime;
            }
            bool landed = motor.isGrounded || (collisions & CollisionFlags.Below) != 0;
            if (!landed || verticalSpeed > 0 || !TryLandingSurface(out var surface)) return;
            if (airborne) {
                airborne = false; verticalSpeed = -2f;
                // Ground contact ends this launch; it does not preserve a phantom air stun.
            }
            if (downed && !downLanded) {
                downLanded = true; downStartedAt = combatTime;
                if(downSettings.groundCrackEnabled) {
                    if(groundImpact==null) groundImpact=gameObject.AddComponent<ShibuyaGroundImpact>();
                    groundImpact.Play(surface,downSettings);
                }
            }
        }

        private void UpdateRecoveryState()
        {
            if (!downed || !downLanded) return;
            float wake = Mathf.Max(downSettings.minimumDownTime, downSettings.naturalRecoveryTime);
            if (!gettingUp && combatTime >= downStartedAt + wake) {
                gettingUp = true; getUpStartedAt = combatTime;
            }
            if (gettingUp && combatTime >= getUpStartedAt + Mathf.Max(.01f, downSettings.getUpDuration)) {
                downed = gettingUp = downLanded = false;
            }
        }

        private void ClearHitOutcome()
        {
            airborne = downed = gettingUp = downLanded = apexVisited = false;
            apexUntil = downStartedAt = getUpStartedAt = 0;
            verticalSpeed = 0;
            airDistanceRemaining = airTravelTime = 0;
            if(groundImpact!=null) groundImpact.Clear();
        }
    }
}
