using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaGradeTestRandom : IShibuyaCombatRandom
    {
        readonly Queue<float> values;public int Draws {get;private set;}
        public ShibuyaGradeTestRandom(params float[] draws){values=new Queue<float>(draws);}
        public float Next01(){Draws++;return values.Count>0?values.Dequeue():.99999f;}
    }
    public sealed class ShibuyaGradeSpecialGuard : MonoBehaviour,IDamageGuard
    {
        public DamageGuardDecision EvaluateDamage(DamageContext context)=>DamageGuardDecision.Block;
    }
    public sealed class ShibuyaPass4G1DataTests
    {
        [TestCase(.25f,false,0,.0375f)] [TestCase(.25f,true,0,.06f)]
        [TestCase(.25f,false,1,.0475f)] [TestCase(.25f,true,1,.07f)]
        [TestCase(.25f,false,5,.0875f)] [TestCase(.25f,true,5,.11f)]
        [TestCase(.25f,false,10,.1375f)] [TestCase(.25f,true,10,.16f)]
        [TestCase(.25f,false,14,.15f)] [TestCase(.25f,true,14,.20f)]
        [TestCase(.5f,false,0,.075f)] [TestCase(.5f,true,0,.12f)]
        [TestCase(.5f,false,5,.15f)] [TestCase(.5f,true,5,.20f)]
        [TestCase(0f,false,99,0f)] [TestCase(0f,true,99,0f)]
        public void FinalProbabilityTable(float critical,bool zone,int stacks,float expected)
        {
            var p=ScriptableObject.CreateInstance<ShibuyaAttackGradeProfile>();try{
                p.criticalChance=critical*100;var result=ShibuyaAttackGrades.Probabilities(p,ShibuyaAttackMetadata.Basic(),zone,stacks,capable:true);
                Assert.That(result.FinalBlackFlash,Is.EqualTo(expected).Within(.000001));Assert.That(result.FinalBlackFlash,Is.LessThanOrEqualTo(critical));
            }finally{UnityEngine.Object.DestroyImmediate(p);}
        }
        [Test] public void CriticalFirstEligibilityClampAndDeterministicRng()
        {
            var p=ScriptableObject.CreateInstance<ShibuyaAttackGradeProfile>();try{
                var m=ShibuyaAttackMetadata.Basic();var rng=new ShibuyaGradeTestRandom(.9f,0,0);
                Assert.That(ShibuyaAttackGrades.Roll(ShibuyaAttackGrades.Probabilities(p,m,false,0,capable:true),rng),Is.EqualTo(ShibuyaAttackGrade.Normal));Assert.That(rng.Draws,Is.EqualTo(1));
                Assert.That(ShibuyaAttackGrades.Roll(ShibuyaAttackGrades.Probabilities(p,m,false,0,capable:true),rng),Is.EqualTo(ShibuyaAttackGrade.BlackFlash));Assert.That(rng.Draws,Is.EqualTo(3));
                Assert.That(ShibuyaAttackGrades.Probabilities(p,m,false,0,criticalAdjustment:200,capable:true).Critical,Is.EqualTo(1));
                Assert.That(ShibuyaAttackGrades.Probabilities(p,m,false,0,criticalAdjustment:-200,capable:true).FinalBlackFlash,Is.Zero);
                foreach(var nature in new[]{ShibuyaDamageNature.Taijutsu,ShibuyaDamageNature.Hybrid,ShibuyaDamageNature.Technique}){
                    m.nature=nature;foreach(bool direct in new[]{false,true}){m.directHybridHit=direct;
                        Assert.That(ShibuyaAttackGrades.Probabilities(p,m,false,0,capable:true).FinalBlackFlash>0,
                            Is.EqualTo(nature==ShibuyaDamageNature.Taijutsu||nature==ShibuyaDamageNature.Hybrid&&direct));}}
                m.nature=ShibuyaDamageNature.Taijutsu;m.reach=ShibuyaAttackReach.Ranged;Assert.That(ShibuyaAttackGrades.Probabilities(p,m,false,0,capable:true).FinalBlackFlash,Is.Zero);
                m.reach=ShibuyaAttackReach.Melee;m.blackFlashAllowed=false;Assert.That(ShibuyaAttackGrades.Probabilities(p,m,false,0,capable:true).FinalBlackFlash,Is.Zero);
                var a=new ShibuyaSeededRandom(42);var b=new ShibuyaSeededRandom(42);for(int i=0;i<100;i++)Assert.That(a.Next01(),Is.EqualTo(b.Next01()));
            }finally{UnityEngine.Object.DestroyImmediate(p);}
        }
        [Test] public void SavedDefaultsKoreanInspectorUndoAndHpProtection()
        {
            var c=AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root+"/Data/Shibuya Combat Profile.asset");var p=UnityEngine.Object.Instantiate(c.attackGrades);
            try{Assert.That(p.criticalChance,Is.EqualTo(25));Assert.That(p.criticalMultiplier,Is.EqualTo(1.5));Assert.That(p.blackFlashCapable,Is.False);Assert.That(p.zoneDuration,Is.EqualTo(12));
                foreach(var f in typeof(ShibuyaAttackGradeProfile).GetFields())if(f.DeclaringType==typeof(ShibuyaAttackGradeProfile))Assert.That(ShibuyaAttackGradeInspector.Label(f.Name),Is.Not.EqualTo(f.Name));
                var so=new SerializedObject(p);Undo.IncrementCurrentGroup();so.FindProperty("criticalChance").floatValue=50;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();Undo.PerformUndo();Assert.That(p.criticalChance,Is.EqualTo(25));
                Assert.That(c.strikes.Select(s=>s.damage),Is.EqualTo(new[]{20f,20,20,25}));
                Assert.That(AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root+"/Data/Dummy Stats.asset").MaxHealth,Is.EqualTo(1000));
                Assert.That(AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root+"/Data/Player Stats.asset").MaxHealth,Is.EqualTo(100));
            }finally{Undo.ClearUndo(p);UnityEngine.Object.DestroyImmediate(p);}
        }
    }
    public sealed class ShibuyaPass4G1Tests : ShibuyaPass4CFixture
    {
        ShibuyaAttackGradeProfile grades;bool complete;
        [UnitySetUp] public IEnumerator GradeSetup(){grades=Clone(combat.attackGrades);combat.attackGrades=grades;complete=false;yield return null;}
        [TearDown] public void CompletionWitness(){Assert.That(complete,Is.True,"full test body must complete; do not accept a pass after a nested iterator failure");}
        void Done(string name,object value){Record(name,value);complete=true;}
        ShibuyaStrikeDefinition Strike()=>new ShibuyaStrikeDefinition{attack=ShibuyaAttackMetadata.Basic(),damage=20,knockbackSpeed=0,advanceDistance=0,hitStopDuration=0};
        DamageResolution Apply(ShibuyaStrikeDefinition s,int seq,int hit,out ShibuyaDamageTicket ticket){
            ticket=player.PrepareGradedDamage(s,dummy,seq,hit);var result=dummy.ReceiveStrike(new DamageContext(ticket.Damage,player.gameObject,DamageDeliveryType.PhysicalStrike,DamageTraits.None,"4G1 QA"),s);
            player.CommitGradedDamage(ticket,result);return result;
        }
        [UnityTest] public IEnumerator CriticalActorWithoutBlackFlashAndBuffHooks()
        {
            player.SetCombatRandom(new ShibuyaGradeTestRandom(0));Assert.That(Apply(Strike(),1,0,out var ticket),Is.EqualTo(DamageResolution.Applied));
            Assert.That(ticket.Grade,Is.EqualTo(ShibuyaAttackGrade.Critical));Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(970));Assert.That(player.ZoneStacks,Is.Zero);
            player.CriticalChanceIncrease=30;player.CriticalChanceReduction=10;dummy.CriticalVulnerability=5;
            Assert.That(player.GetGradeProbabilities(Strike().attack,dummy).Critical,Is.EqualTo(.5f));grades.allowCriticalModifiers=false;
            Assert.That(player.GetGradeProbabilities(Strike().attack,dummy).Critical,Is.EqualTo(.25f));
            Done("4G1-critical",new{ticket.Damage,ticket.Grade,player.BlackFlashCount,player.CriticalHitCount});yield return null;
        }
        [UnityTest] public IEnumerator FirstBlackFlashThenZoneDamageRewardAndDuplicateCommit()
        {
            grades.blackFlashCapable=true;player.SetCombatRandom(new ShibuyaGradeTestRandom(0,0,0,0));var energy=input.EnergyCharge.Resource;energy.SetCurrentEnergy(100);
            Apply(Strike(),10,0,out var first);Assert.That(first.Damage,Is.EqualTo(50));Assert.That(energy.CurrentEnergy,Is.EqualTo(110));Assert.That(player.ZoneStacks,Is.EqualTo(1));
            Assert.That(player.CommitGradedDamage(first,DamageResolution.Applied),Is.False);Assert.That(player.ZoneStacks,Is.EqualTo(1));
            Apply(Strike(),10,1,out var second);Assert.That(second.Damage,Is.EqualTo(55).Within(.0001));Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(895));Assert.That(energy.CurrentEnergy,Is.EqualTo(120));
            Assert.That(player.ZoneStacks,Is.EqualTo(2));Assert.That(player.ZoneRemaining,Is.EqualTo(12).Within(.02));
            Done("4G1-first-zone",new{first=first.Damage,second=second.Damage,energy=energy.CurrentEnergy,player.ZoneStacks,player.ZoneRemaining});yield return null;
        }
        [UnityTest] public IEnumerator ActualFiveEventsTwoBlackFlashesAndMulticolliderDedup()
        {
            grades.blackFlashCapable=true;player.SetCombatRandom(new ShibuyaGradeTestRandom(.9f,0,0,.9f,0,0,.9f));input.EnergyCharge.Resource.SetCurrentEnergy(100);
            Place(dummy,player.transform.position+Vector3.forward*1.4f);dummy.transform.rotation=Quaternion.Euler(0,180,0);
            var extra=new GameObject("4G1 extra collider");extra.transform.SetParent(dummy.transform,false);extra.AddComponent<BoxCollider>().center=Vector3.up;
            var m=ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();m.name="PASS4B3 QA 4G1 multi";
            m.steps=new[]{new ShibuyaMovesetStep{hits=Enumerable.Range(0,5).Select(i=>{var s=Strike();s.startup=.03f;s.activeTime=.025f;s.recovery=.04f;return new ShibuyaHitEvent{startTime=i*.18f,strike=s};}).ToArray()}};
            Assert.That(player.SetMoveset(m),Is.True);var rows=new List<object>();player.AttackGradeApplied+=(_,t)=>rows.Add(new{t.Sequence,t.Event,t.Target,t.Grade,t.Damage});
            Assert.That(player.TryAttack(),Is.True);yield return Until(()=>player.ComboStep==0);Assert.That(player.ConfirmedHitCount,Is.EqualTo(5));Assert.That(player.BlackFlashCount,Is.EqualTo(2));
            Assert.That(player.ZoneStacks,Is.EqualTo(2));Assert.That(input.EnergyCharge.Resource.CurrentEnergy,Is.EqualTo(120));Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(831).Within(.001));
            Done("4G1-multihit",rows);
        }
        [UnityTest] public IEnumerator InvalidGuardParryInvulnerableAndSpecialDefenseGiveNoRewards()
        {
            grades.blackFlashCapable=true;player.SetCombatRandom(new ShibuyaGradeTestRandom(0,0,0,0,0,0,0,0));input.EnergyCharge.Resource.SetCurrentEnergy(100);
            Place(dummy,player.transform.position+Vector3.forward*1.4f);dummy.transform.rotation=Quaternion.Euler(0,180,0);
            dummy.SetGuardHeld(true);var results=new List<DamageResolution>();results.Add(Apply(Strike(),20,0,out _)); // parry
            dummy.SetGuardHeld(false);yield return Wait(.55f);dummy.SetGuardHeld(true);yield return Wait(.15f);results.Add(Apply(Strike(),20,1,out _)); // normal guard
            dummy.SetGuardHeld(false);dummy.Health.GrantInvulnerability(.05f);results.Add(Apply(Strike(),20,2,out _));yield return Wait(.07f);
            var special=dummy.gameObject.AddComponent<ShibuyaGradeSpecialGuard>();results.Add(Apply(Strike(),20,3,out _));UnityEngine.Object.Destroy(special);
            Assert.That(results.Take(2).All(x=>x==DamageResolution.Guarded),Is.True);Assert.That(results[2],Is.EqualTo(DamageResolution.Invulnerable));Assert.That(results[3],Is.EqualTo(DamageResolution.Guarded));
            Assert.That(player.BlackFlashCount+player.ZoneStacks+player.GradeAppliedCount,Is.Zero);Assert.That(input.EnergyCharge.Resource.CurrentEnergy,Is.EqualTo(100));Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(1000));
            Done("4G1-blocked",results);
        }
        [UnityTest] public IEnumerator ActualZoneCostChargeTwelveSecondsRefreshExpiryAndStackLifetime()
        {
            grades.blackFlashCapable=true;player.SetCombatRandom(new ShibuyaGradeTestRandom(0,0,0,0));var resource=input.EnergyCharge.Resource;resource.SetCurrentEnergy(100);
            Assert.That(player.ZoneStacks,Is.Zero);Apply(Strike(),30,0,out _);Assert.That(resource.ResolveCost(100),Is.EqualTo(85));Assert.That(resource.TrySpend(20),Is.True);Assert.That(resource.CurrentEnergy,Is.EqualTo(93));
            float began=Time.time;frame.auraHeld=true;yield return Until(()=>input.EnergyCharge.IsCharging);yield return Wait(.22f);
            float ratio=(resource.CurrentEnergy-93)/input.EnergyCharge.HoldDuration;Assert.That(ratio,Is.EqualTo(14.4f).Within(.1));frame.auraHeld=false;
            yield return Wait(.3f);float before=player.ZoneRemaining;Apply(Strike(),30,1,out _);Assert.That(player.ZoneRemaining,Is.EqualTo(12).Within(.02));Assert.That(player.ZoneRemaining,Is.GreaterThan(before));
            yield return Until(()=>!player.ZoneActive,13);Assert.That(Time.time-began,Is.GreaterThanOrEqualTo(12));Assert.That(player.ZoneStacks,Is.EqualTo(2));Assert.That(resource.ResolveCost(100),Is.EqualTo(100));Assert.That(player.EnergyChargeFactor,Is.EqualTo(1));
            Done("4G1-zone-lifetime",new{ratio,seconds=Time.time-began,player.ZoneStacks,cost=resource.ResolveCost(100),player.EnergyChargeFactor});
        }
        [UnityTest] public IEnumerator MaximumClampFreshBattleAndLegacyResourceUnchanged()
        {
            Assert.That(player.ZoneStacks+player.BlackFlashCount,Is.Zero);grades.blackFlashCapable=true;player.SetCombatRandom(new ShibuyaGradeTestRandom(0,0));var energy=input.EnergyCharge.Resource;energy.SetCurrentEnergy(195);
            Apply(Strike(),40,0,out _);Assert.That(energy.CurrentEnergy,Is.EqualTo(200));Assert.That(aura.SizeMultiplier,Is.EqualTo(1));
            var go=new GameObject("PASS4B3 QA legacy resource");go.AddComponent<Health>();var legacy=go.AddComponent<CursedEnergyController>();legacy.ApplyProfile(CursedEnergyProfileId.Standard);
            Assert.That(legacy.ResolveCost(100),Is.EqualTo(100));UnityEngine.Object.Destroy(go);Done("4G1-clamp-legacy",new{energy.CurrentEnergy,aura.SizeMultiplier});yield return null;
        }
        [UnityTest] public IEnumerator InterleavedSequenceDuplicatesAndOverkillHealthLossStayExact()
        {
            grades.blackFlashCapable=true;player.SetCombatRandom(new ShibuyaGradeTestRandom(0,0,0,0,0,0,0,0));
            var duplicate=player.PrepareGradedDamage(Strike(),dummy,50,0);Apply(Strike(),50,0,out _);Apply(Strike(),51,0,out _);
            Assert.That(player.CommitGradedDamage(duplicate,DamageResolution.Applied),Is.False);Assert.That(player.BlackFlashCount,Is.EqualTo(2));
            dummy.Health.SetCurrentHealth(10);Apply(Strike(),52,0,out var lethal);Assert.That(lethal.Damage,Is.EqualTo(55).Within(.001));
            Assert.That(lethal.AppliedHealthLoss,Is.EqualTo(10));Assert.That(player.LastAppliedGradeDamage,Is.EqualTo(10));
            Done("4G1-interleaved-overkill",new{lethal.Damage,lethal.AppliedHealthLoss,player.BlackFlashCount});yield return null;
        }
    }
}
