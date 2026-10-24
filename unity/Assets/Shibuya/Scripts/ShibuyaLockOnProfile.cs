using UnityEngine;

namespace JJKGame.Shibuya
{
    [CreateAssetMenu(menuName = "JJK Game/시부야/타깃 락온 설정", fileName = "Shibuya Lock On")]
    public sealed class ShibuyaLockOnProfile : ScriptableObject
    {
        [Min(0)] public float acquireRange = 50f;
        [Min(0)] public float retainRange = 60f;
        [Range(0, .7f)] public float screenCenterRadius = .22f;
        [Min(0)] public float reselectHoldTime = .25f;
        [Min(0)] public float sightGraceTime = 1.5f;
        public LayerMask environmentMask = ~0;
        public Vector3 aimOffset;
        public bool attackFacingEnabled = true;
        [Min(0)] public float attackTurnSpeed = 2400f;
        [Range(0, 180)] public float attackMaxTurnAngle = 180f;
        [Min(0)] public float attackPreparationTime = .12f;
        [Range(0, 1)] public float cameraStrength = .7f;
        [Min(0)] public float cameraSharpness = 5f;
        [Min(0)] public float cameraDegreesPerSecond = 100f;
        [Min(0)] public float manualLookGrace = .8f;
        public bool cameraAssistEnabled = true;
        public Vector2 cameraDeadZone = new Vector2(.28f, .22f);
        [Min(0)] public float cameraAcceleration = 180f;
        [Min(0)] public float cameraShoulderOffset = 2.2f;
        [Range(0, 1)] public float cameraOverlapStrength = 1f;
        [Range(0, 60)] public float cameraOverlapOrbitAngle = 32f;
        [Min(0)] public float cameraFramingSpeed = 3f;
        [Min(0)] public float cameraFramingRange = 12f;
        [Min(0)] public float cameraExtraDistance = 1f;
        [Range(0, 1)] public float cameraVerticalFocusWeight = .2f;
        [Min(0)] public float cameraVerticalFocusLimit = 1f;
        [Min(0)] public float cameraReturnGrace = .5f;
        public bool cameraScreenRecoveryEnabled = true;
        [Range(.01f,.2f)] public float cameraRecoveryMargin = .06f;
        [Min(.05f)] public float cameraRecoveryStallTime = .3f;
        [Min(.05f)] public float cameraRecoveryStableTime = .12f;
        [Min(.1f)] public float cameraRecoveryTimeout = 4f;
        public bool showUI = true;
        [Min(0)] public float lostFeedbackTime = .8f;
        [Min(4)] public float markerSize = 30f;
        public Color candidateColor = Color.yellow;
        public Color lockedColor = Color.cyan;
        public Color occludedColor = new Color(1f, .65f, .2f);
        public bool showAimDebug;
        [Min(0)] public float debugAimLength = 8f;

        public bool IsValid => Finite(acquireRange) && acquireRange >= 0 && Finite(retainRange) && retainRange >= acquireRange
            && Finite(screenCenterRadius) && screenCenterRadius >= 0 && screenCenterRadius <= .7f
            && Finite(attackTurnSpeed) && attackTurnSpeed >= 0
            && Finite(attackMaxTurnAngle) && attackMaxTurnAngle >= 0 && attackMaxTurnAngle <= 180
            && Finite(attackPreparationTime) && attackPreparationTime >= 0
            && Finite(reselectHoldTime) && reselectHoldTime >= 0 && Finite(sightGraceTime) && sightGraceTime >= 0
            && Finite(cameraStrength) && cameraStrength >= 0 && cameraStrength <= 1
            && Finite(cameraSharpness) && cameraSharpness >= 0 && Finite(cameraDegreesPerSecond) && cameraDegreesPerSecond >= 0
            && Finite(manualLookGrace) && manualLookGrace >= 0 && Finite(lostFeedbackTime) && lostFeedbackTime >= 0
            && Finite(cameraDeadZone.x) && cameraDeadZone.x >= .05f && cameraDeadZone.x <= .45f
            && Finite(cameraDeadZone.y) && cameraDeadZone.y >= .05f && cameraDeadZone.y <= .45f
            && Finite(cameraAcceleration) && cameraAcceleration >= 0 && Finite(cameraShoulderOffset) && cameraShoulderOffset >= 0
            && Finite(cameraOverlapStrength) && cameraOverlapStrength >= 0 && cameraOverlapStrength <= 1
            && Finite(cameraOverlapOrbitAngle) && cameraOverlapOrbitAngle >= 0 && cameraOverlapOrbitAngle <= 60
            && Finite(cameraFramingSpeed) && cameraFramingSpeed >= 0 && Finite(cameraFramingRange) && cameraFramingRange >= 0
            && Finite(cameraExtraDistance) && cameraExtraDistance >= 0 && Finite(cameraVerticalFocusWeight) && cameraVerticalFocusWeight >= 0 && cameraVerticalFocusWeight <= 1
            && Finite(cameraVerticalFocusLimit) && cameraVerticalFocusLimit >= 0 && Finite(cameraReturnGrace) && cameraReturnGrace >= 0
            && Finite(cameraRecoveryMargin) && cameraRecoveryMargin >= .01f && cameraRecoveryMargin <= .2f
            && Finite(cameraRecoveryStallTime) && cameraRecoveryStallTime >= .05f
            && Finite(cameraRecoveryStableTime) && cameraRecoveryStableTime >= .05f
            && Finite(cameraRecoveryTimeout) && cameraRecoveryTimeout >= .1f
            && Finite(markerSize) && markerSize >= 4 && Finite(debugAimLength) && debugAimLength >= 0
            && Finite(aimOffset.x) && Finite(aimOffset.y) && Finite(aimOffset.z);
        private static bool Finite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
    }
}
