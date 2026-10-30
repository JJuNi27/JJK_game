using JJKGame.Core;
using UnityEngine;

namespace JJKGame.Shibuya
{
    public enum ShibuyaGroundDashKind { Forward, Side, Backward }

    public sealed partial class ShibuyaFighter
    {
        private bool groundDashing;
        private double dashStartedAt, dashReadyAt;
        private float dashDistance, dashDuration, dashEvadeDelay, dashEvadeTime, dashCooldown, dashConsumed, dashLean;
        private Quaternion dashFacing;
        private ShibuyaGroundDashProfile activeDashProfile;
        private readonly float[] dashIntegral = new float[65];
        public bool IsGroundDashing => groundDashing;
        public ShibuyaGroundDashKind GroundDashKind { get; private set; }
        public Vector3 GroundDashDirection { get; private set; }
        public float GroundDashProgress => groundDashing ? Mathf.Clamp01((float)(Time.timeAsDouble - dashStartedAt) / dashDuration) : 0;
        public float GroundDashCooldownRemaining => Mathf.Max(0, (float)(dashReadyAt - Time.timeAsDouble));
        public float GroundDashRequestedDistance => dashConsumed;
        public float GroundDashLeanAngle => dashLean;
        public int GroundDashCount { get; private set; }
        public bool IsGroundDashEvading => groundDashing && isActiveAndEnabled && !Health.IsDead && !stopped
            && !IsInCinematic && !IsActionLocked && IsGrounded && GroundDashKind != ShibuyaGroundDashKind.Forward
            && Time.timeAsDouble - dashStartedAt >= dashEvadeDelay
            && Time.timeAsDouble - dashStartedAt < Mathf.Min(dashDuration, dashEvadeDelay + dashEvadeTime);

        private bool FocusingEnergy
        {
            get {
                var aura = GetComponent<CursedEnergyAuraController>();
                var charge = GetComponent<ShibuyaEnergyCharge>();
                return (aura != null && aura.Held) || (charge != null && charge.IsCharging);
            }
        }
        public bool TryGroundDash(Vector2 rawDirection, Vector3 cameraForward, Vector3 cameraRight)
        {
            var p = profile != null ? profile.groundDash : null;
            if (!isActiveAndEnabled || p == null || !p.dashEnabled || Health.IsDead || stopped || groundDashing
                || !IsGrounded || IsActionLocked || airSession || chasing || attacking || IsInHitStop || IsInCinematic
                || jumpIntent || chaseQueued || FocusingEnergy || Time.timeAsDouble < dashReadyAt
                || guarding || IsGuardBroken || rawDirection.sqrMagnitude < .000001f) return false;

            Vector2 input = rawDirection.normalized;
            var locked = ExplicitLockTarget;
            if (!IsLiveTarget(locked)) locked = null;
            // Travel is camera-relative in both modes. A lock may affect facing only.
            Vector3 forward = cameraForward;
            forward.y = 0;
            cameraRight.y = 0;
            if (forward.sqrMagnitude < .000001f) forward = Vector3.Cross(cameraRight, Vector3.up);
            if (forward.sqrMagnitude < .000001f) forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (forward.sqrMagnitude < .000001f) forward = Vector3.forward;
            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            if (cameraRight.sqrMagnitude > .000001f) right = cameraRight.normalized;
            Vector3 direction = (forward * input.y + right * input.x).normalized;
            GroundDashKind = input.y > .0001f ? ShibuyaGroundDashKind.Forward
                : input.y < -.0001f ? ShibuyaGroundDashKind.Backward : ShibuyaGroundDashKind.Side;
            float distance = GroundDashKind == ShibuyaGroundDashKind.Forward ? p.forwardDistance
                : GroundDashKind == ShibuyaGroundDashKind.Side ? p.sideDistance : p.backwardDistance;
            float duration = GroundDashKind == ShibuyaGroundDashKind.Forward ? p.forwardDuration
                : GroundDashKind == ShibuyaGroundDashKind.Side ? p.sideDuration : p.backwardDuration;
            if (!float.IsFinite(distance) || distance <= 0 || !float.IsFinite(duration) || duration <= 0) return false;

            // Snapshot the complete path/timing policy once. No target homing or blocked-distance bank.
            CancelCombo(); ClearIncomingLink(); ClearVictimAlignment();
            GroundDashDirection = direction; dashDistance = distance; dashDuration = Mathf.Max(.01f, duration);
            dashEvadeDelay = SafeNonnegative(p.evadeStartDelay); dashEvadeTime = SafeNonnegative(p.evadeDuration);
            dashCooldown = SafeNonnegative(p.cooldownAfterEnd); dashLean = SafeNonnegative(p.stepLeanAngle);
            Vector3 lockFacing = locked != null ? locked.transform.position - transform.position : Vector3.zero;
            lockFacing.y = 0;
            dashFacing = lockFacing.sqrMagnitude > .000001f ? Quaternion.LookRotation(lockFacing) : transform.rotation;
            activeDashProfile = p; dashConsumed = 0; dashStartedAt = Time.timeAsDouble;
            BuildDashIntegral(p.speedCurve); groundDashing = true; GroundDashCount++; jumpIntent = false;
            return true;
        }
        private static float SafeNonnegative(float value) => float.IsFinite(value) ? Mathf.Max(0, value) : 0;
        private void BuildDashIntegral(AnimationCurve curve)
        {
            dashIntegral[0] = 0;
            for (int i = 1; i < dashIntegral.Length; i++) {
                float a = curve != null ? SafeNonnegative(curve.Evaluate((i - 1) / 64f)) : 1;
                float b = curve != null ? SafeNonnegative(curve.Evaluate(i / 64f)) : 1;
                dashIntegral[i] = dashIntegral[i - 1] + (a + b) / 128f;
            }
            float total = dashIntegral[64];
            for (int i = 1; i < dashIntegral.Length; i++) dashIntegral[i] = total > .000001f ? dashIntegral[i] / total : i / 64f;
        }
        private Vector3 AdvanceGroundDash()
        {
            if (!groundDashing) return Vector3.zero;
            if (!IsGrounded || IsActionLocked || IsInCinematic || profile.groundDash != activeDashProfile
                || !activeDashProfile.dashEnabled || FocusingEnergy) { CancelGroundDash(); return Vector3.zero; }
            transform.rotation = dashFacing;
            float phase = GroundDashProgress * 64;
            int i = Mathf.Min(63, Mathf.FloorToInt(phase));
            float target = dashDistance * Mathf.Lerp(dashIntegral[i], dashIntegral[i + 1], phase - i);
            float amount = Mathf.Max(0, target - dashConsumed); dashConsumed = target;
            if (phase >= 64) CancelGroundDash();
            return GroundDashDirection * amount;
        }
        private void CancelGroundDash()
        {
            if (!groundDashing) return;
            groundDashing = false; dashReadyAt = Time.timeAsDouble + dashCooldown;
        }
        private DamageGuardDecision EvaluateGroundDashDamage(DamageContext context) => IsGroundDashEvading
            && context.DeliveryType == DamageDeliveryType.PhysicalStrike && !context.HasTrait(DamageTraits.Unblockable)
                ? DamageGuardDecision.Block : DamageGuardDecision.NoDecision;
    }
}
