using UnityEngine;

namespace JJKGame.Shibuya
{
    public sealed partial class ShibuyaOrbitCamera
    {
        private enum ScreenRecovery { Lazy, Orbit, Blocked }
        private ScreenRecovery screenRecovery;
        private ShibuyaFighter recoveryTarget;
        private Vector3 recoveryTargetPoint;
        private float recoveryYaw, recoveryPitch, recoveryBestError = float.PositiveInfinity;
        private int recoveryDirection, recoveryVisibleFrames;
        private double recoveryStarted, recoveryVisibleSince = -1, recoveryProgressAt;
        public string ScreenRecoveryStatus { get; private set; } = "일반 구도";

        private void ResetScreenRecovery()
        {
            screenRecovery = ScreenRecovery.Lazy; recoveryTarget = null;
            recoveryBestError = float.PositiveInfinity; recoveryProgressAt = Time.unscaledTimeAsDouble;
            recoveryVisibleSince = -1; recoveryVisibleFrames = 0;
            ScreenRecoveryStatus = "일반 구도";
        }
        private static bool InsideRecoveryFrame(Vector3 point, float margin)
            => point.z > .01f && point.x >= margin && point.x <= 1-margin && point.y >= margin && point.y <= 1-margin;

        private Vector3 RecoveryProjection(Camera camera, float candidateYaw, float candidatePitch)
        {
            var rotation = Quaternion.Euler(candidatePitch,candidateYaw,0);
            var position = EvaluateCombatCameraPosition(focus,rotation,out _);
            return ProjectLockPoint(lockOn.TargetPoint,position,rotation,camera);
        }
        private bool AssistScreenRecovery(Camera camera, ShibuyaLockOnProfile p, ref float yawError, ref float pitchError)
        {
            if (!p.cameraScreenRecoveryEnabled) { ResetScreenRecovery(); return false; }
            double now = Time.unscaledTimeAsDouble;
            // Success is judged from the real output camera, including last frame's final
            // diagonal collision correction. LOS validity remains owned by ShibuyaLockOn.
            Vector3 actual = camera.WorldToViewportPoint(lockOn.TargetPoint);
            if (recoveryTarget != lockOn.LockedTarget) {
                ResetScreenRecovery(); recoveryTarget = lockOn.LockedTarget;
            }
            if (screenRecovery == ScreenRecovery.Blocked) {
                // No endless retry or left/right oscillation in an impossible static scene.
                // A meaningful target displacement or new manual input permits a fresh attempt.
                float radius = lockOn.LockedTarget.GetComponent<CharacterController>().radius;
                if (InsideRecoveryFrame(actual,p.cameraRecoveryMargin)
                    || Vector3.Distance(recoveryTargetPoint,lockOn.TargetPoint) > radius) {
                    ResetScreenRecovery(); recoveryTarget=lockOn.LockedTarget;
                } else { yawError=pitchError=0; return true; }
            }
            if (screenRecovery == ScreenRecovery.Lazy) {
                if (InsideRecoveryFrame(actual,0)) {
                    recoveryBestError=float.PositiveInfinity; recoveryProgressAt=now; return false;
                }
                float error = actual.z <= .01f ? 2f : Mathf.Max(0,-actual.x)+Mathf.Max(0,actual.x-1)
                    +Mathf.Max(0,-actual.y)+Mathf.Max(0,actual.y-1);
                if (error < recoveryBestError-.002f) { recoveryBestError=error; recoveryProgressAt=now; }
                bool pitchBlocked = (pitchError > 0 && pitch >= profile.maximumPitch-.01f)
                    || (pitchError < 0 && pitch <= profile.minimumPitch+.01f);
                // Ordinary off-screen tracking gets its existing chance. A saturated axis or
                // sustained lack of output progress activates the bounded fallback.
                if (!pitchBlocked && now-recoveryProgressAt < p.cameraRecoveryStallTime) return false;
                recoveryTargetPoint=lockOn.TargetPoint;
                recoveryStarted=now; recoveryVisibleSince=-1; recoveryVisibleFrames=0;
                if (!PlanScreenRecovery(camera,p)) {
                    screenRecovery=ScreenRecovery.Blocked; ScreenRecoveryStatus="복구 제한: 가능한 구도 없음";
                    yawError=pitchError=0; lockYawVelocity=lockPitchVelocity=0; return true;
                }
                screenRecovery=ScreenRecovery.Orbit; ScreenRecoveryStatus="화면 밖 복구 중";
                lockYawVelocity=lockPitchVelocity=0;
            }
            if (InsideRecoveryFrame(actual,p.cameraRecoveryMargin)) {
                yawError=pitchError=0; lockYawVelocity=lockPitchVelocity=0;
                if (recoveryVisibleSince < 0) recoveryVisibleSince=now;
                recoveryVisibleFrames++;
                if (recoveryVisibleFrames>=3 && now-recoveryVisibleSince>=p.cameraRecoveryStableTime) {
                    ResetScreenRecovery(); recoveryTarget=lockOn.LockedTarget;
                }
                return true;
            }
            recoveryVisibleSince=-1; recoveryVisibleFrames=0;
            if (now-recoveryStarted >= p.cameraRecoveryTimeout) {
                screenRecovery=ScreenRecovery.Blocked; recoveryTargetPoint=lockOn.TargetPoint;
                ScreenRecoveryStatus="복구 제한: 시간 상한";
                yawError=pitchError=0; lockYawVelocity=lockPitchVelocity=0; return true;
            }
            float signed = Mathf.DeltaAngle(yaw,recoveryYaw);
            // Latch the chosen side through a 180-degree tie and never reverse mid-attempt.
            if (Mathf.Abs(signed)>179.9f) signed=recoveryDirection*Mathf.Abs(signed);
            yawError = Mathf.Sign(signed)==recoveryDirection ? signed : 0;
            pitchError = recoveryPitch-pitch;
            return true;
        }
        private bool PlanScreenRecovery(Camera camera, ShibuyaLockOnProfile p)
        {
            Vector3 delta=lockOn.TargetPoint-focus;
            float heading=new Vector2(delta.x,delta.z).sqrMagnitude>.000001f
                ? Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg : yaw;
            float aimedPitch=Mathf.Clamp(-Mathf.Atan2(delta.y,new Vector2(delta.x,delta.z).magnitude)*Mathf.Rad2Deg,
                profile.minimumPitch,profile.maximumPitch);
            float best=float.PositiveInfinity; bool found=false;
            // Ten bounded orbit poses, once per attempt. Prefer current pitch and the smallest
            // angular change; no global optimizer or per-frame scene search.
            for(int vertical=0;vertical<2;vertical++) {
                float cp=vertical==0?pitch:aimedPitch;
                for(int i=0;i<5;i++) {
                    float offset=i==0?0:i==1?-60:i==2?60:i==3?-90:90;
                    float cy=Mathf.Repeat(heading+offset,360);
                    Vector3 screen=RecoveryProjection(camera,cy,cp);
                    if(!InsideRecoveryFrame(screen,p.cameraRecoveryMargin)) continue;
                    float angle=Mathf.DeltaAngle(yaw,cy);
                    int side=Mathf.Abs(angle)>179.9f?lockShoulderSide:(angle>=0?1:-1);
                    float cost=Mathf.Abs(angle)+Mathf.Abs(cp-pitch)*1.5f;
                    if(cost>best+.001f || (Mathf.Abs(cost-best)<=.001f && side!=lockShoulderSide)) continue;
                    best=cost; found=true; recoveryYaw=cy; recoveryPitch=cp; recoveryDirection=side;
                }
            }
            return found;
        }
    }
}
