using System;
using UnityEngine;

namespace JJKGame.Shibuya
{
    public enum ShibuyaMovementKind { [InspectorName("전방 이동")] Forward, [InspectorName("직전 적중 대상 접근")] ApproachLastHit }
    public enum ShibuyaKnockbackKind { [InspectorName("속도·감속")] Velocity, [InspectorName("거리·시간 (충돌 시 감소)")] Distance }

    [Serializable]
    public sealed class ShibuyaHitEvent
    {
        [Min(0)] public float startTime;
        public ShibuyaStrikeDefinition strike = new ShibuyaStrikeDefinition();
        [Min(-1)] public int requiredMovement = -1;
        public bool heavyReaction;
        public ShibuyaKnockbackKind knockbackKind;
        [Min(0)] public float knockbackDistance = 4f;
        [Min(.01f)] public float knockbackDuration = .4f;
        public float ActiveStart => startTime + strike.Startup;
        public float ActiveEnd => startTime + strike.ActiveEnd;
    }

    [Serializable]
    public sealed class ShibuyaMovementDefinition
    {
        public ShibuyaMovementKind kind;
        [Min(0)] public float startTime;
        [Min(.01f)] public float duration = .5f;
        [Min(0)] public float maxDistance = 6f;
        [Min(0)] public float speed = 14f;
        [Min(.1f)] public float targetRange = 8f;
        [Min(0)] public float arrivalDistance = 1.3f;
        [Range(0, 180)] public float maxTurnAngle = 50f;
        [Min(0)] public float turnSpeed = 360f;
        [Min(0)] public float predictionTime = .04f;
    }

    [Serializable]
    public sealed class ShibuyaMovesetStep
    {
        public ShibuyaHitEvent[] hits = { new ShibuyaHitEvent() };
        public ShibuyaMovementDefinition[] movements = Array.Empty<ShibuyaMovementDefinition>();
        public float Duration {
            get {
                float result = .01f;
                foreach (var hit in hits) result = Mathf.Max(result, hit.startTime + hit.strike.Duration);
                foreach (var move in movements) result = Mathf.Max(result, move.startTime + move.duration);
                return result;
            }
        }
        public float ActiveEnd {
            get { float result = 0; foreach (var hit in hits) result = Mathf.Max(result, hit.ActiveEnd); return result; }
        }
    }

    // Optional character data. A null reference retains the verified Combat Profile four-hit data.
    [CreateAssetMenu(menuName = "JJK/시부야/캐릭터 무브셋")]
    public sealed class ShibuyaMovesetDefinition : ScriptableObject
    {
        public ShibuyaMovesetStep[] steps = { new ShibuyaMovesetStep() };
        public bool IsValid {
            get {
                if (steps == null || steps.Length == 0) return false;
                foreach (var step in steps) {
                    if (step == null || step.hits == null || step.hits.Length == 0 || step.movements == null) return false;
                    float previous = -1;
                    foreach (var move in step.movements)
                        if (move == null || !Finite(move.startTime) || move.startTime < 0 || !Finite(move.duration) || move.duration <= 0
                            || !Finite(move.speed) || move.speed < 0 || !Finite(move.maxDistance) || move.maxDistance < 0) return false;
                    foreach (var hit in step.hits) {
                        if (hit == null || hit.strike == null || !Finite(hit.startTime) || hit.startTime < 0
                            || !Finite(hit.strike.Duration) || !Finite(hit.ActiveStart) || hit.ActiveStart < previous
                            || hit.requiredMovement < -1 || hit.requiredMovement >= step.movements.Length) return false;
                        previous = hit.ActiveStart;
                    }
                }
                return true;
            }
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
