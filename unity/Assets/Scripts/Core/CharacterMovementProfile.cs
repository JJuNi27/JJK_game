using System;
using UnityEngine;

namespace JJKGame.Core
{
    [Serializable]
    public sealed class CharacterMovementProfile
    {
        [Min(0.1f)] public float walkSpeed = 4f;
        [Min(0.1f)] public float runSpeed = 14f;
        public EvadeProfile evade = EvadeProfile.CreateGojo();
    }

    /// <summary>Character data only. Curve X is normalized movement time, Y is speed / burstSpeed.</summary>
    [Serializable]
    public sealed class EvadeProfile
    {
        [Min(0f)] public float burstSpeed = 12f;
        [Min(0.01f)] public float movementDuration = 0.24f;
        public AnimationCurve speedCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        [Min(0f)] public float recoveryDuration;
        [Min(0f)] public float cooldown = 0.75f;
        [Min(0f)] public float invulnerabilityStart;
        [Min(0f)] public float invulnerabilityDuration = 0.30f;
        [Tooltip("Presentation metadata only; never controls movement or invulnerability.")]
        public string styleId = "evasive-step";
        public string animationTrigger = "Dodge";
        public string animationState = "";
        public string vfxCue = "";
        public string sfxCue = "Dodge";

        public float MovementDuration => Mathf.Max(0.01f, movementDuration);
        public float ActionDuration => MovementDuration + Mathf.Max(0f, recoveryDuration);

        public static EvadeProfile CreateGojo()
        {
            return new EvadeProfile
            {
                burstSpeed = 32f,
                movementDuration = 0.20f,
                recoveryDuration = 0.04f,
                // Piecewise linear: 32 at 0, 28 at .06, 14 at .14, zero at .20 seconds.
                speedCurve = new AnimationCurve(
                    new Keyframe(0f, 1f, 0f, -0.41666667f),
                    new Keyframe(0.3f, 0.875f, -0.41666667f, -1.09375f),
                    new Keyframe(0.7f, 0.4375f, -1.09375f, -1.45833333f),
                    new Keyframe(1f, 0f, -1.45833333f, 0f)),
                styleId = "gojo-blue-burst",
                vfxCue = "gojo.evade.spatial-streak",
            };
        }
    }
}
