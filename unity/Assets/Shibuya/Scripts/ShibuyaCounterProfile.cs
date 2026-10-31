using UnityEngine;
namespace JJKGame.Shibuya
{
    [CreateAssetMenu(menuName="JJK/시부야/패링 후 반격",fileName="Shibuya Counter")]
    public sealed class ShibuyaCounterProfile : ScriptableObject
    {
        public bool counterEnabled=true;
        public string counterName="패링 반격";
        [Min(0)] public float inputOpportunity=.5f;
        [Min(.1f)] public float maximumStartRange=4;
        [Range(0,180)] public float maximumStartAngle=120;
        [Min(.1f)] public float maximumReservationTime=3;
        [Range(0,100)] public float counterBlackFlashConditional=50;
        public ShibuyaMovesetStep counterStep=new ShibuyaMovesetStep{
            hits=new[]{new ShibuyaHitEvent{heavyReaction=true,strike=new ShibuyaStrikeDefinition{
                attack=new ShibuyaAttackMetadata{reach=ShibuyaAttackReach.Melee,nature=ShibuyaDamageNature.Taijutsu,
                    origin=ShibuyaAttackOrigin.Counter,criticalAllowed=true,blackFlashAllowed=true},
                damage=20,startup=.18f,activeTime=.04f,recovery=.25f,hitStun=.3f,knockbackSpeed=6,
                motion=ShibuyaStrikeMotion.HeavyCross,advanceDistance=0,hitStopDuration=.06f}}},
            movements=new[]{new ShibuyaMovementDefinition{kind=ShibuyaMovementKind.ApproachLastHit,duration=.20f,
                speed=8,maxDistance=1.2f,arrivalDistance=1.3f,targetRange=6,maxTurnAngle=90,turnSpeed=720}}
        };
        public bool IsValid=>counterEnabled&&ShibuyaMovesetDefinition.ValidStep(counterStep)
            &&float.IsFinite(maximumReservationTime)&&maximumReservationTime>counterStep.Duration
            &&float.IsFinite(inputOpportunity)&&inputOpportunity>0&&float.IsFinite(maximumStartRange)&&maximumStartRange>0;
    }
}
