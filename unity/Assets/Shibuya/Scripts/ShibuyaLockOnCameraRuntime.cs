using UnityEngine;

namespace JJKGame.Shibuya
{
    public sealed partial class ShibuyaOrbitCamera
    {
        private ShibuyaLockOn lockOn;
        private double lastManualLook = double.NegativeInfinity;
        public void BindLockOn(ShibuyaLockOn value)
        {
            if (lockOn != value) ResetLockOnFraming();
            lockOn = value;
        }
    }
    public sealed partial class ShibuyaFighter
    {
        private ShibuyaLockOn explicitLockOn;
        public void BindLockOn(ShibuyaLockOn value) => explicitLockOn = value;
        public ShibuyaFighter ExplicitLockTarget => explicitLockOn != null && explicitLockOn.isActiveAndEnabled ? explicitLockOn.LockedTarget : null;
        private ShibuyaFighter LockedMeleeTarget(ShibuyaStrikeDefinition strike, Vector3 forward)
        {
            // Explicit selection is not soft acquisition: camera/body yaw and predicted hit reach
            // must not reject it. This chooses a heading, never a damage recipient or teleport.
            if (explicitLockOn == null || !explicitLockOn.isActiveAndEnabled
                || explicitLockOn.Settings == null || !explicitLockOn.Settings.IsValid
                || !explicitLockOn.Settings.attackFacingEnabled) return null;
            explicitLockOn.Refresh(Time.unscaledTimeAsDouble);
            var target = ExplicitLockTarget;
            // Even an occluded/immune locked opponent keeps selection priority. Assistance
            // below may refuse it, but must not acquire a different nearby opponent instead.
            return IsLiveTarget(target) && target != this ? target : null;
        }
        private bool ValidLockedAttackTarget(ShibuyaFighter target, ShibuyaStrikeDefinition strike)
        {
            // An event retains its snapshot across reselection/unlock. Death, immunity, range
            // and world obstruction still stop assistance; another target is never substituted.
            return IsLiveTarget(target) && target != this && !target.Health.IsInvulnerable
                && explicitLockOn != null && explicitLockOn.Settings != null && explicitLockOn.Settings.IsValid
                && Vector3.Distance(transform.position, target.transform.position) <= explicitLockOn.Settings.retainRange
                && !WorldBlocks(transform.position + Vector3.up * strike.attackHeight,
                    target.transform.position + Vector3.up * strike.attackHeight);
        }
        private void TurnLockedAttack(int index, ShibuyaStrikeDefinition strike, float elapsed, float delta)
        {
            if (aimStopped[index]) return;
            var target = eventAim[index];
            if (!ValidLockedAttackTarget(target, strike)) { aimStopped[index] = true; return; }
            var policy = explicitLockOn.Settings;
            float window = Mathf.Min(strike.Startup, policy.attackPreparationTime);
            float dt = Mathf.Max(0, Mathf.Min(elapsed, window) - Mathf.Max(0, elapsed - delta));
            Vector3 flat = target.transform.position - transform.position; flat.y = 0;
            if (dt <= 0 || flat.sqrMagnitude < .000001f) return;
            var old = transform.rotation;
            float turn = Mathf.Min(Mathf.Max(0, policy.attackMaxTurnAngle - aimTurns[index]), policy.attackTurnSpeed * dt);
            transform.rotation = Quaternion.RotateTowards(old, Quaternion.LookRotation(flat), turn);
            float applied = Quaternion.Angle(old, transform.rotation);
            aimTurns[index] += applied; turnConsumed += applied;
        }
    }
}
