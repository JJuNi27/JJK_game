using UnityEngine;
namespace JJKGame.Shibuya
{
    [CreateAssetMenu(menuName="JJK/시부야/공격형 훈련 AI 설정")]
    public sealed class ShibuyaTrainingAIProfile : ScriptableObject
    {
        public bool trainingAllowed=true;
        [Min(.1f)] public float detectionRange=10;
        [Min(.1f)] public float approachStartDistance=2.3f;
        [Min(.1f)] public float approachStopDistance=1.55f;
        [Min(.01f)] public float decisionInterval=.15f;
        [Range(0,90)] public float facingTolerance=20;
        [Min(0)] public float reactionDelay=.20f;
        [Min(0)] public float minimumRest=.9f;
        [Min(0)] public float maximumRest=1.5f;
        [Min(0)] public float oneHitWeight=25;
        [Min(0)] public float twoHitWeight=35;
        [Min(0)] public float threeHitWeight=25;
        [Min(0)] public float fourHitWeight=15;
        public bool useRunning;
        [Min(0)] public float maximumAttackHeightDifference=1.4f;
        public bool useFixedSeed;
        public int randomSeed=1729;
        public ShibuyaTrainingMode initialMode=ShibuyaTrainingMode.Offensive;
        public ShibuyaTrainingDefenseProfile defense;
    }
}
