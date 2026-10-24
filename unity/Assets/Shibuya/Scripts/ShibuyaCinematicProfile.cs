using System;
using UnityEngine;

namespace JJKGame.Shibuya
{
    [Flags] public enum ShibuyaCinematicTrigger { [InspectorName("기본 마무리")] Default = 1, [InspectorName("어퍼컷")] Uppercut = 2, [InspectorName("다운")] Down = 4, [InspectorName("별도 연출 이벤트")] Authored = 8 }
    public enum ShibuyaShotAnchor { [InspectorName("공격자")] Attacker, [InspectorName("피격자")] Victim, [InspectorName("중간 지점")] Midpoint }
    [Serializable] public sealed class ShibuyaCinematicShot
    {
        public ShibuyaShotAnchor anchor = ShibuyaShotAnchor.Midpoint;
        [Min(.01f)] public float duration = .18f;
        [Min(.1f)] public float distance = 4f;
        public float height = 1.5f;
        public float angle = 65f;
        public Vector3 positionOffset;
        public Vector3 lookOffset = new Vector3(0, 1f, 0);
        public Vector3 rotationOffset;
        [Range(15, 100)] public float fieldOfView = 48;
        [Min(0)] public float blendTime = .045f;
    }

    [CreateAssetMenu(menuName = "JJK/시부야/2인 시네마틱 설정")]
    public sealed class ShibuyaCinematicProfile : ScriptableObject
    {
        public bool cinematicEnabled = true;
        public ShibuyaCinematicTrigger triggers = ShibuyaCinematicTrigger.Default;
        [Min(.01f)] public float duration = .5f;
        public ShibuyaCinematicShot[] shots = { new ShibuyaCinematicShot() };
        [Min(0)] public float blendIn = .06f;
        [Min(0)] public float blendOut = .12f;
        [Min(.1f)] public float cameraCollisionRadius = .18f;
        [Min(.1f)] public float maximumSeparation = 8f;
        public bool holdMotion = true;
        public bool holdFacing = true;
        public bool blockThirdPartyDamage = true;
        public bool allowAuthoredDamage = true;
        public bool cancelOnAttackInput;
        public bool cancelOnMoveInput;
        public bool cancelOnObstruction = true;
        public bool IsValid {
            get {
                if (!Finite(duration) || duration <= 0 || !Finite(blendIn) || blendIn < 0 || !Finite(blendOut) || blendOut < 0
                    || !Finite(cameraCollisionRadius) || cameraCollisionRadius <= 0 || !Finite(maximumSeparation) || maximumSeparation <= 0
                    || shots == null || shots.Length == 0) return false;
                foreach (var s in shots) if (s == null || !Finite(s.duration) || s.duration <= 0 || !Finite(s.distance) || s.distance <= 0
                    || !Finite(s.height) || !Finite(s.angle) || !Finite(s.fieldOfView) || s.fieldOfView < 15 || s.fieldOfView > 100
                    || !Finite(s.blendTime) || s.blendTime < 0 || !Finite(s.positionOffset) || !Finite(s.lookOffset) || !Finite(s.rotationOffset)
                    || !Enum.IsDefined(typeof(ShibuyaShotAnchor), s.anchor)) return false;
                return true;
            }
        }
        private static bool Finite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
        private static bool Finite(Vector3 v) => Finite(v.x) && Finite(v.y) && Finite(v.z);
    }
}
