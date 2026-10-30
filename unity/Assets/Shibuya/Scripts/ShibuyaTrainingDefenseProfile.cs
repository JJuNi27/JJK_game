using UnityEngine;
namespace JJKGame.Shibuya
{
    public enum ShibuyaTrainingMode
    {
        [InspectorName("공격형")] Offensive,
        [InspectorName("가드 연습")] GuardPractice,
        [InspectorName("회피 연습")] DodgePractice,
        [InspectorName("패링 연습")] ParryPractice,
        [InspectorName("종합")] Mixed
    }
    public enum ShibuyaTrainingDefenseAction { None, Guard, Dodge, Parry }
    public enum ShibuyaTrainingDodgeBasis { [InspectorName("현재 상대 방향")] Opponent, [InspectorName("자신의 몸 방향")] Body }
    [CreateAssetMenu(menuName="JJK/시부야/훈련 AI 방어 판단 설정")]
    public sealed class ShibuyaTrainingDefenseProfile : ScriptableObject
    {
        public bool defenseEnabled=true;
        [Min(.01f)] public float observationInterval=.10f;
        [Min(0)] public float minimumReaction=.16f;
        [Min(0)] public float maximumReaction=.30f;
        [Min(0)] public float minimumDecisionInterval=.75f;
        [Min(0)] public float minimumGuardHold=.35f;
        [Min(0)] public float maximumGuardHold=.75f;
        [Min(0)] public float guardWeight=50;
        [Min(0)] public float dodgeWeight=25;
        [Min(0)] public float parryWeight=10;
        [Min(0)] public float noResponseWeight=15;
        [Min(.1f)] public float threatRange=4;
        [Range(0,180)] public float attackerFacingHalfAngle=75;
        public ShibuyaTrainingDodgeBasis dodgeBasis=ShibuyaTrainingDodgeBasis.Opponent;
        [Min(0)] public float leftWeight=1;
        [Min(0)] public float rightWeight=1;
        [Min(0)] public float backWeight=1;
        public bool mixedAllowsOffense=true;
    }
}
