using System;
using UnityEngine;

namespace JJKGame.Core
{
    public enum EvadePresentationPhase { Started, Recovery, Completed, Cancelled }

    public readonly struct EvadePresentationCue
    {
        public EvadePresentationCue(Transform owner, Vector3 direction, EvadeProfile profile,
            EvadePresentationPhase phase)
        {
            Owner = owner;
            Direction = direction;
            Profile = profile;
            Phase = phase;
        }
        public Transform Owner { get; }
        public Vector3 Direction { get; }
        public EvadeProfile Profile { get; }
        public EvadePresentationPhase Phase { get; }
    }

    public static class EvadePresentationCues
    {
        public static event Action<EvadePresentationCue> Raised;
        public static void Raise(EvadePresentationCue cue) => Raised?.Invoke(cue);
    }

    /// <summary>
    /// Shared displacement clock for combat and preview. No input, Health, animation or teleport.
    /// Integrates a fixed curve table once per evade; frame partitions cannot change total distance.
    /// Consumers pass the returned displacement to CharacterController.Move.
    /// Blocked movement is consumed, never saved up for a later jump through an obstacle.
    /// </summary>
    public sealed class EvadeMotion
    {
        private const int Samples = 100;
        private readonly float[] distance = new float[Samples + 1];
        private float movementDuration;
        private float actionDuration;
        private float elapsed;
        public bool IsActive { get; private set; }
        public bool IsRecovering => IsActive && elapsed >= movementDuration;
        public float Progress => IsActive ? Mathf.Clamp01(elapsed / actionDuration) : 0f;
        public float Elapsed => elapsed;

        public void Begin(EvadeProfile profile)
        {
            movementDuration = profile.MovementDuration;
            actionDuration = profile.ActionDuration;
            elapsed = 0f;
            distance[0] = 0f;
            float previous = Speed(profile, 0f);
            for (int i = 1; i <= Samples; i++)
            {
                float speed = Speed(profile, (float)i / Samples);
                distance[i] = distance[i - 1] + (previous + speed) * 0.5f * movementDuration / Samples;
                previous = speed;
            }
            IsActive = true;
        }

        public float Advance(float deltaTime)
        {
            if (!IsActive) return 0f;
            float before = DistanceAt(elapsed);
            elapsed = Mathf.Min(actionDuration, elapsed + Mathf.Max(0f, deltaTime));
            float displacement = DistanceAt(elapsed) - before;
            IsActive = elapsed < actionDuration;
            return Mathf.Max(0f, displacement);
        }

        public void Cancel() => IsActive = false;

        public static void Move(CharacterController motor, Vector3 direction, float displacement,
            float verticalDisplacement)
        {
            int steps = Mathf.Max(1, Mathf.CeilToInt(displacement / Mathf.Max(0.05f, motor.radius * 0.5f)));
            Vector3 delta = (direction * displacement + Vector3.up * verticalDisplacement) / steps;
            for (int i = 0; i < steps; i++) motor.Move(delta);
        }

        private float DistanceAt(float time)
        {
            float sample = Mathf.Clamp01(time / movementDuration) * Samples;
            int index = Mathf.Min(Samples - 1, Mathf.FloorToInt(sample));
            return Mathf.Lerp(distance[index], distance[index + 1], sample - index);
        }

        private static float Speed(EvadeProfile profile, float time)
        {
            float weight = profile.speedCurve == null || profile.speedCurve.length == 0
                ? 1f : profile.speedCurve.Evaluate(time);
            return Mathf.Max(0f, profile.burstSpeed) * Mathf.Clamp01(weight);
        }
    }
}
