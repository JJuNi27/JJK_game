using System;
using JJKGame.Core;
using UnityEngine;
namespace JJKGame.Shibuya
{
    public enum ShibuyaDamageAction { Strike, Grab }
    public enum ShibuyaZoneFeedback { None, Entered, Refreshed }
    public readonly struct ShibuyaDamageResultKey : IEquatable<ShibuyaDamageResultKey>
    {
        public readonly int Source,Target,Sequence,HitEvent;
        public readonly ShibuyaDamageAction Action;
        public ShibuyaDamageResultKey(int source,int target,int sequence,int hit,ShibuyaDamageAction action){Source=source;Target=target;Sequence=sequence;HitEvent=hit;Action=action;}
        public bool Equals(ShibuyaDamageResultKey v)=>Source==v.Source&&Target==v.Target&&Sequence==v.Sequence&&HitEvent==v.HitEvent&&Action==v.Action;
        public override bool Equals(object obj)=>obj is ShibuyaDamageResultKey key&&Equals(key);
        public override int GetHashCode()=>HashCode.Combine(Source,Target,Sequence,HitEvent,(int)Action);
    }
    // Immutable observer data. Rendering never rolls a grade, applies damage or grants rewards.
    public readonly struct ShibuyaCombatDamageResult
    {
        public readonly ShibuyaFighter Attacker,Victim;
        public readonly ShibuyaDamageResultKey Key;
        public readonly DamageResolution Resolution;
        public readonly ShibuyaAttackGrade Grade;
        public readonly float HealthLoss,EnergyRestored;
        public readonly Vector3 WorldPoint;
        public readonly ShibuyaZoneFeedback Zone;
        public ShibuyaCombatDamageResult(ShibuyaFighter attacker,ShibuyaFighter victim,ShibuyaDamageResultKey key,
            DamageResolution resolution,float loss,ShibuyaAttackGrade grade,Vector3 point,float energy=0,ShibuyaZoneFeedback zone=ShibuyaZoneFeedback.None)
        {Attacker=attacker;Victim=victim;Key=key;Resolution=resolution;HealthLoss=loss;Grade=grade;WorldPoint=point;EnergyRestored=energy;Zone=zone;}
    }
    public static class ShibuyaCombatDamageResults
    {
        public static event Action<ShibuyaCombatDamageResult> Resolved;
        public static int ObserverCount=>Resolved?.GetInvocationList().Length??0;
        internal static void Publish(ShibuyaCombatDamageResult result)
        {
            var observers=Resolved;if(observers==null)return;
            foreach(Action<ShibuyaCombatDamageResult> observer in observers.GetInvocationList())
                try{observer(result);}catch(Exception error){Debug.LogException(error);} // An observer cannot abort a combat transaction.
        }
    }
}
