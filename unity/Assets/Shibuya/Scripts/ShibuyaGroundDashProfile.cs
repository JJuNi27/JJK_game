using UnityEngine;

namespace JJKGame.Shibuya
{
    [CreateAssetMenu(menuName = "JJK Game/시부야/지상 방향 대시 설정", fileName = "Shibuya Ground Dash")]
    public sealed class ShibuyaGroundDashProfile : ScriptableObject
    {
        public bool dashEnabled = true;
        [Min(0)] public float forwardDistance = 3.5f;
        [Min(.01f)] public float forwardDuration = .22f;
        [Min(0)] public float sideDistance = 2.4f;
        [Min(.01f)] public float sideDuration = .18f;
        [Min(0)] public float backwardDistance = 2.2f;
        [Min(.01f)] public float backwardDuration = .20f;
        [Min(0)] public float evadeStartDelay = .04f;
        [Min(0)] public float evadeDuration = .10f;
        [Min(0)] public float cooldownAfterEnd = .65f;
        public AnimationCurve speedCurve = new AnimationCurve(
            new Keyframe(0, 0, 0, 10), new Keyframe(.12f, 1.5f, 0, 0),
            new Keyframe(.65f, 1, -2, -2), new Keyframe(1, 0, -3, 0));
        [Range(0, 40)] public float stepLeanAngle = 18;
    }
}
