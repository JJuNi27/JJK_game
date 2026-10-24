using System;
using UnityEngine;

namespace JJKGame.Shibuya
{
    public interface IShibuyaAimSource
    {
        // Query at emission time; the returned value has no projectile/homing ownership.
        ShibuyaAimSolution GetAim(Vector3 emissionOrigin);
        ShibuyaAimSolution GetAim(Vector3 emissionOrigin, bool useLockOn);
    }
    public readonly struct ShibuyaAimSolution
    {
        public readonly Vector3 Origin, Point, Direction;
        public readonly ShibuyaFighter Target;
        public ShibuyaAimSolution(Vector3 origin, Vector3 point, Vector3 fallback, ShibuyaFighter target)
        { Origin = origin; Point = point; Target = target; Direction = (point-origin).sqrMagnitude > .000001f ? (point-origin).normalized : fallback.normalized; }
    }

    // Selection validity and environmental detection are independent of camera/UI/melee rules.
    public interface IShibuyaTargetDetection
    {
        bool IsValid(ShibuyaFighter owner, ShibuyaFighter target);
        Vector3 AimPoint(ShibuyaFighter target, ShibuyaLockOnProfile settings);
        bool IsVisible(ShibuyaFighter owner, ShibuyaFighter target, Vector3 origin, ShibuyaLockOnProfile settings);
    }
    public sealed class ShibuyaVisibleTargetDetection : IShibuyaTargetDetection
    {
        public bool IsValid(ShibuyaFighter owner, ShibuyaFighter target) => target != null && target != owner
            && target.isActiveAndEnabled && !target.Health.IsDead && target.State != ShibuyaFighterState.Finished;
        public Vector3 AimPoint(ShibuyaFighter target, ShibuyaLockOnProfile settings)
            => target.GetComponent<CharacterController>().bounds.center + settings.aimOffset;
        public bool IsVisible(ShibuyaFighter owner, ShibuyaFighter target, Vector3 origin, ShibuyaLockOnProfile settings)
        {
            foreach (var hit in Physics.RaycastAll(origin, AimPoint(target, settings)-origin,
                Vector3.Distance(origin, AimPoint(target, settings)), settings.environmentMask, QueryTriggerInteraction.Ignore))
                if (hit.collider.GetComponentInParent<ShibuyaFighter>() == null) return false;
            return true;
        }
    }

    [DefaultExecutionOrder(100), DisallowMultipleComponent]
    public sealed class ShibuyaLockOn : MonoBehaviour, IShibuyaAimSource
    {
        [SerializeField] private ShibuyaLockOnProfile settings;
        private ShibuyaFighter owner;
        private Camera view;
        private bool held, beganLocked, hasSelection;
        private double pressedAt, obscuredAt = -1, lostUntil;
        private IShibuyaTargetDetection detection = new ShibuyaVisibleTargetDetection();
        public ShibuyaLockOnProfile Settings => settings;
        public ShibuyaFighter Owner => owner;
        public ShibuyaFighter LockedTarget { get; private set; }
        public ShibuyaFighter Candidate { get; private set; }
        public bool IsSearching => held;
        public bool IsOccluded => LockedTarget != null && obscuredAt >= 0;
        public bool HasLostFeedback => Time.unscaledTimeAsDouble < lostUntil;
        public string LossReason { get; private set; }
        public float Distance => LockedTarget != null ? Vector3.Distance(owner.transform.position, LockedTarget.transform.position) : 0;
        public event Action Changed;

        public void Configure(ShibuyaFighter actor, Camera camera, ShibuyaLockOnProfile profile,
            IShibuyaTargetDetection policy = null)
        {
            Clear(false, null); owner = actor; view = camera; settings = profile;
            detection = policy ?? new ShibuyaVisibleTargetDetection();
        }
        private bool Ready => owner != null && owner.isActiveAndEnabled && !owner.Health.IsDead
            && owner.State != ShibuyaFighterState.Finished && view != null && settings != null && settings.IsValid;
        public void HandleButton(bool down, double now)
        {
            if (!Ready) { Clear(false, null); return; }
            if (down && !held) { held = true; pressedAt = now; beganLocked = LockedTarget != null; Candidate = FindCandidate(); Changed?.Invoke(); }
            else if (!down && held) {
                bool shortUnlock = beganLocked && now-pressedAt < settings.reselectHoldTime;
                var selected = FindCandidate(); held = false; Candidate = null;
                if (shortUnlock) Clear(true, "수동 해제");
                else if (selected == null) Clear(true, "선택 가능한 대상 없음");
                else {
                    LockedTarget = selected; hasSelection = selected != null; obscuredAt = -1; lostUntil = 0;
                    LossReason = null; Changed?.Invoke();
                }
            }
        }
        public void CancelSearch() { held = beganLocked = false; Candidate = null; Changed?.Invoke(); }
        public void Clear(bool feedback = false, string reason = null)
        {
            bool had = hasSelection;
            hasSelection = false;
            LockedTarget = Candidate = null; held = beganLocked = false; obscuredAt = -1;
            LossReason = reason; lostUntil = feedback && had && settings != null ? Time.unscaledTimeAsDouble+settings.lostFeedbackTime : 0;
            Changed?.Invoke();
        }
        private void LateUpdate() => Refresh(Time.unscaledTimeAsDouble);
        public void Refresh(double now)
        {
            if (!Ready) { Clear(false, null); return; }
            if (hasSelection) {
                if (!detection.IsValid(owner, LockedTarget)) Clear(true, "대상 사망/비활성/종료");
                else if (Distance > settings.retainRange) Clear(true, "유지 거리 초과");
                else if (detection.IsVisible(owner, LockedTarget, view.transform.position, settings)) obscuredAt = -1;
                else {
                    if (obscuredAt < 0) obscuredAt = now;
                    if (now-obscuredAt >= settings.sightGraceTime) Clear(true, "시야 상실");
                }
            }
            if (held) Candidate = FindCandidate();
        }
        private ShibuyaFighter FindCandidate()
        {
            ShibuyaFighter best = null; float score = float.PositiveInfinity;
            foreach (var target in FindObjectsByType<ShibuyaFighter>(FindObjectsSortMode.None)) {
                if (!detection.IsValid(owner, target) || Vector3.Distance(owner.transform.position, target.transform.position) > settings.acquireRange) continue;
                Vector3 screen = view.WorldToViewportPoint(detection.AimPoint(target, settings));
                if (screen.z <= 0 || screen.x < 0 || screen.x > 1 || screen.y < 0 || screen.y > 1) continue;
                float radius = new Vector2(screen.x-.5f, screen.y-.5f).magnitude;
                if (radius > settings.screenCenterRadius || !detection.IsVisible(owner, target, view.transform.position, settings)) continue;
                if (radius < score || (Mathf.Approximately(radius, score) && best != null && target.GetInstanceID() < best.GetInstanceID())) { best = target; score = radius; }
            }
            return best;
        }
        public ShibuyaAimSolution GetAim(Vector3 emissionOrigin)
            => GetAim(emissionOrigin, true);
        public ShibuyaAimSolution GetAim(Vector3 emissionOrigin, bool useLockOn)
        {
            // An emission query also validates loss; no stale target between Update and firing.
            Refresh(Time.unscaledTimeAsDouble);
            var target = useLockOn ? LockedTarget : null;
            Vector3 fallback = view != null ? view.transform.forward : transform.forward;
            return new ShibuyaAimSolution(emissionOrigin, target != null ? detection.AimPoint(target, settings) : emissionOrigin+fallback, fallback, target);
        }
        public Vector3 TargetPoint => LockedTarget != null ? detection.AimPoint(LockedTarget, settings) : transform.position;
        private void OnDisable() => Clear();
    }
}
