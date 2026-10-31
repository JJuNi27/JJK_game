using System;
using UnityEngine;
namespace JJKGame.Shibuya
{
    public enum ShibuyaAttackGrade { [InspectorName("일반")] Normal, [InspectorName("치명타")] Critical, [InspectorName("흑섬")] BlackFlash }
    public interface IShibuyaCombatRandom { float Next01(); }
    public interface IShibuyaHitRandom : IShibuyaCombatRandom { void BeginHit(); }
    public sealed class ShibuyaSeededRandom : IShibuyaCombatRandom
    {
        readonly System.Random random;
        public ShibuyaSeededRandom(int seed) { random=new System.Random(seed); }
        public float Next01() => (float)random.NextDouble();
    }
    public readonly struct ShibuyaGradeProbabilities
    {
        public readonly float Critical,RawConditional,EffectiveConditional,FinalBlackFlash;
        public ShibuyaGradeProbabilities(float critical,float raw,float effective) {
            Critical=critical;RawConditional=raw;EffectiveConditional=effective;FinalBlackFlash=critical*effective;
        }
    }
    public static class ShibuyaAttackGrades
    {
        public static ShibuyaGradeProbabilities Probabilities(ShibuyaAttackGradeProfile p,ShibuyaAttackMetadata attack,
            bool zone,int stacks,float criticalAdjustment=0,bool capable=false,bool counter=false,float counterConditional=50)
        {
            if(p==null)return default;
            float c=counter?1:attack!=null&&attack.criticalAllowed?Mathf.Clamp01((p.criticalChance+criticalAdjustment)*.01f):0;
            float raw=Mathf.Clamp01(((zone?p.zoneConditional:p.normalConditional)+Mathf.Max(0,stacks)*p.stackConditional)*.01f);
            bool allowed=capable&&attack!=null&&attack.CanBlackFlash;
            float conditional=!allowed||c<=0?0:counter?Mathf.Clamp01(counterConditional*.01f)
                :Mathf.Min(raw,Mathf.Clamp01((zone?p.zoneFinalCap:p.normalFinalCap)*.01f)/c);
            return new ShibuyaGradeProbabilities(c,raw,conditional);
        }
        public static ShibuyaAttackGrade Roll(ShibuyaGradeProbabilities p,IShibuyaCombatRandom rng)
        {
            // A second draw exists only after the first succeeds. No independent black-flash draw.
            if(p.Critical<=0||rng.Next01()>=p.Critical)return ShibuyaAttackGrade.Normal;
            return p.EffectiveConditional>0&&rng.Next01()<p.EffectiveConditional?ShibuyaAttackGrade.BlackFlash:ShibuyaAttackGrade.Critical;
        }
    }
    // Temporary damage quote, consumed once even if blocked. Rewards are a separate Applied-only commit.
    public sealed class ShibuyaDamageTicket
    {
        public readonly ShibuyaAttackGrade Grade;
        public readonly float Damage;
        public readonly int Sequence,Event,Target;
        public float AppliedHealthLoss {get;internal set;}
        internal bool consumed;
        internal readonly ShibuyaFighter owner;
        internal readonly ShibuyaFighter victim;
        internal readonly float healthBefore;
        internal ShibuyaDamageTicket(ShibuyaFighter source,ShibuyaAttackGrade grade,float damage,int sequence,int hit,ShibuyaFighter target) {
            owner=source;victim=target;healthBefore=target!=null?target.Health.CurrentHealth:0;
            Grade=grade;Damage=damage;Sequence=sequence;Event=hit;Target=target!=null?target.GetInstanceID():0;
        }
    }
}
