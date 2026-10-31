using System;
using System.Collections.Generic;
using JJKGame.Core;
using UnityEngine;
namespace JJKGame.Shibuya
{
    public sealed partial class ShibuyaFighter : ICursedEnergyCostModifier
    {
        IShibuyaCombatRandom gradeRandom;
        readonly HashSet<(int,int,int)> gradeCommits=new HashSet<(int,int,int)>();
        float zoneEndsAt;
        public ShibuyaAttackGradeProfile GradeProfile => profile!=null?profile.attackGrades:null;
        // Transient hooks for future buffs/debuffs; no passive framework or persistent growth is installed.
        public float CriticalChanceIncrease {get;set;}
        public float CriticalChanceReduction {get;set;}
        public float CriticalVulnerability {get;set;}
        public float CriticalMultiplierBonus {get;set;}
        public int ZoneStacks {get;private set;}
        public int BlackFlashCount {get;private set;}
        public int CriticalHitCount {get;private set;}
        public int GradeAppliedCount {get;private set;}
        public bool ZoneActive => GradeProfile!=null&&isActiveAndEnabled&&!Health.IsDead&&!stopped&&Time.time<zoneEndsAt;
        public float ZoneRemaining => ZoneActive?Mathf.Max(0,zoneEndsAt-Time.time):0;
        public float ZoneDamageMultiplier => ZoneActive?1+GradeProfile.zoneDamageBonus*.01f:1;
        public float EnergyCostFactor => ZoneActive?1-Mathf.Clamp01(GradeProfile.zoneCostReduction*.01f):1;
        public float EnergyChargeFactor => ZoneActive?1+GradeProfile.zoneChargeBonus*.01f:1;
        public ShibuyaAttackGrade LastAttackGrade {get;private set;}
        public float LastAppliedGradeDamage {get;private set;}
        public event Action<ShibuyaFighter,ShibuyaDamageTicket> AttackGradeApplied;
        public void SetCombatRandom(IShibuyaCombatRandom random) { gradeRandom=random; }
        public ShibuyaGradeProbabilities GetGradeProbabilities(ShibuyaAttackMetadata metadata,ShibuyaFighter target=null,bool counter=false,float counterConditional=50)
        {
            float modifier=GradeProfile!=null&&GradeProfile.allowCriticalModifiers
                ?CriticalChanceIncrease-CriticalChanceReduction+(target!=null?target.CriticalVulnerability:0):0;
            return ShibuyaAttackGrades.Probabilities(GradeProfile,metadata,ZoneActive,ZoneStacks,modifier,
                GradeProfile!=null&&GradeProfile.blackFlashCapable,counter,counterConditional);
        }
        public ShibuyaDamageTicket PrepareGradedDamage(ShibuyaStrikeDefinition strike,ShibuyaFighter target,int sequence,int hit,
            bool counter=false,float counterConditional=50)
        {
            var p=GradeProfile;
            if(gradeRandom==null)gradeRandom=new ShibuyaSeededRandom(Guid.NewGuid().GetHashCode());
            if(gradeRandom is IShibuyaHitRandom perHit)perHit.BeginHit();
            var grade=ShibuyaAttackGrades.Roll(GetGradeProbabilities(strike.attack,target,counter,counterConditional),gradeRandom);
            float multiplier=p==null?1:grade==ShibuyaAttackGrade.BlackFlash?p.blackFlashMultiplier:
                grade==ShibuyaAttackGrade.Critical?Mathf.Max(1,p.criticalMultiplier+(p.allowCriticalModifiers?CriticalMultiplierBonus:0)):1;
            return new ShibuyaDamageTicket(this,grade,Mathf.Max(.1f,strike.damage)*ZoneDamageMultiplier*multiplier,
                sequence,hit,target);
        }
        public bool CommitGradedDamage(ShibuyaDamageTicket ticket,DamageResolution resolution,float? measuredHealthLoss=null,Vector3? hitPoint=null)
        {
            if(ticket==null||ticket.owner!=this||ticket.consumed)return false;
            ticket.consumed=true;
            if(!gradeCommits.Add((ticket.Sequence,ticket.Event,ticket.Target)))return false;
            Vector3 point=hitPoint??(ticket.victim!=null?ticket.victim.transform.position+Vector3.up:transform.position);
            var key=new ShibuyaDamageResultKey(GetInstanceID(),ticket.Target,ticket.Sequence,ticket.Event,ShibuyaDamageAction.Strike);
            if(resolution!=DamageResolution.Applied){
                if(measuredHealthLoss.HasValue)ShibuyaCombatDamageResults.Publish(new ShibuyaCombatDamageResult(this,ticket.victim,key,resolution,0,ShibuyaAttackGrade.Normal,point));
                return false;
            }
            ticket.AppliedHealthLoss=measuredHealthLoss??(ticket.victim!=null?Mathf.Max(0,ticket.healthBefore-ticket.victim.Health.CurrentHealth):ticket.Damage);
            LastAttackGrade=ticket.Grade;LastAppliedGradeDamage=ticket.AppliedHealthLoss;GradeAppliedCount++;
            if(ticket.Grade!=ShibuyaAttackGrade.Normal)CriticalHitCount++;
            float restoredEnergy=0;var zoneFeedback=ShibuyaZoneFeedback.None;
            if(ticket.Grade==ShibuyaAttackGrade.BlackFlash&&GradeProfile!=null){
                bool wasZone=ZoneActive;
                BlackFlashCount++;ZoneStacks++;
                zoneEndsAt=Time.time+Mathf.Max(0,GradeProfile.zoneDuration);
                var resource=GetComponent<CursedEnergyController>();
                if(resource==null&&profile.cursedEnergy!=null){resource=CursedEnergyController.GetOrCreate(gameObject);resource.ApplyProfile(profile.cursedEnergy);}
                if(resource!=null){float before=resource.CurrentEnergy;resource.Restore(GradeProfile.blackFlashEnergy);restoredEnergy=resource.CurrentEnergy-before;}
                if(ZoneActive)zoneFeedback=wasZone?ShibuyaZoneFeedback.Refreshed:ShibuyaZoneFeedback.Entered;
            }
            AttackGradeApplied?.Invoke(this,ticket);
            if(measuredHealthLoss.HasValue)ShibuyaCombatDamageResults.Publish(new ShibuyaCombatDamageResult(this,ticket.victim,key,
                resolution,ticket.AppliedHealthLoss,ticket.Grade,point,restoredEnergy,zoneFeedback));
            return true;
        }
    }
}
