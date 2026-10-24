using UnityEngine;

namespace JJKGame.Shibuya
{
    [CreateAssetMenu(menuName="JJK/시부야/공중 추격·체술 설정")]
    public sealed class ShibuyaAerialCombatDefinition : ScriptableObject
    {
        public bool chaseEnabled=true;
        [Min(.01f)] public float chaseInputWindow=.70f;
        [Min(.01f)] public float chaseInputBuffer=.20f;
        [Min(.1f)] public float chaseMaxRange=9f;
        [Min(.1f)] public float chaseMaxTravel=9f;
        [Min(.01f)] public float chaseMaxDuration=.80f;
        [Min(.1f)] public float chaseSpeed=24f;
        [Min(.1f)] public float chaseDeceleration=65f;
        [Min(.1f)] public float chaseArrivalDistance=1.25f;
        [Min(0)] public float chaseArrivalTolerance=.12f;
        [Min(.01f)] public float chaseVerticalTolerance=.45f;
        [Min(0)] public float chaseMinimumRise=2.5f;
        [Min(0)] public float chasePredictionTime=.05f;
        [Min(0)] public float chasePredictionLimit=.60f;
        [Range(0,180)] public float chaseMaxTurnAngle=100f;
        [Min(0)] public float chaseTurnSpeed=720f;
        [Min(.01f)] public float chaseStallTimeout=.08f;
        [Min(0)] public float chaseLandingSeparationSpeed=8f;
        [Min(0)] public float chaseContactHoldTime=.18f;
        [Min(.01f)] public float aerialInputWindow=.55f;
        [Min(0)] public float aerialMaxHeightDifference=1.1f;
        [Range(0,1)] public float aerialAirControl=.25f;
        [Min(0)] public float failureFallSpeedLimit=6f;
        [Range(0,45)] public float chaseLeanAngle=18f;
        public bool aerialHoverEnabled=true;
        [Min(.01f)] public float aerialMaxHoverTime=1.2f;
        [Min(0)] public float aerialLinkInputWindow=.12f;
        public bool holdVictimOnAirHit=true;
        public ShibuyaMovesetStep aerialAttack=new ShibuyaMovesetStep();

        public bool IsValid {
            get {
                if(!ShibuyaMovesetDefinition.ValidStep(aerialAttack)) return false;
                foreach(var hit in aerialAttack.hits) if(hit.outcome!=ShibuyaHitOutcome.Normal) return false;
                float[] positive={chaseInputWindow,chaseInputBuffer,chaseMaxRange,chaseMaxTravel,chaseMaxDuration,
                    chaseSpeed,chaseDeceleration,chaseArrivalDistance,chaseVerticalTolerance,chaseStallTimeout,aerialInputWindow,aerialMaxHoverTime};
                float[] nonnegative={chasePredictionTime,chasePredictionLimit,chaseMaxTurnAngle,chaseTurnSpeed,
                    chaseLandingSeparationSpeed,chaseContactHoldTime,aerialMaxHeightDifference,aerialAirControl,failureFallSpeedLimit,chaseLeanAngle,chaseMinimumRise,chaseArrivalTolerance,aerialLinkInputWindow};
                foreach(float x in positive) if(float.IsNaN(x)||float.IsInfinity(x)||x<=0) return false;
                foreach(float x in nonnegative) if(float.IsNaN(x)||float.IsInfinity(x)||x<0) return false;
                return chaseMaxTurnAngle<=180 && aerialAirControl<=1;
            }
        }
    }
}
