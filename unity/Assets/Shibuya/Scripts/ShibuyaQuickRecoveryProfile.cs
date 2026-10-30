using UnityEngine;
namespace JJKGame.Shibuya
{
    [CreateAssetMenu(menuName="JJK/시부야/다운 긴급 탈출 설정")]
    public sealed class ShibuyaQuickRecoveryProfile : ScriptableObject
    {
        public bool recoveryEnabled=true;
        [Min(.01f)] public float distance=2.5f;
        [Min(.01f)] public float duration=.30f;
        public AnimationCurve speedCurve=AnimationCurve.EaseInOut(0,1,1,0);
        [Min(0)] public float evadeStart=.04f;
        [Min(0)] public float evadeEnd=.18f;
        [Min(0)] public float cooldownAfterEnd=1;
        [Min(0)] public float poseStrength=1;
        [Min(.01f)] public float poseDuration=.30f;
    }
}
