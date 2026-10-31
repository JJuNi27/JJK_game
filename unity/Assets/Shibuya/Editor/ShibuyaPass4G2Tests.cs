using System;
using System.Collections;
using System.Linq;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass4G2DataTests
    {
        [Test] public void CounterProfileIndependentTimersKoreanFieldsAndUndo()
        {
            var c=AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root+"/Data/Shibuya Combat Profile.asset");
            Assert.That(c.guard.parry.attackerStun,Is.EqualTo(.5f));Assert.That(c.guard.parry.pushDistance,Is.EqualTo(1.2f));Assert.That(c.guard.parry.successMotionDuration,Is.EqualTo(.1f));
            var p=UnityEngine.Object.Instantiate(c.counter);try{
                Assert.That(p.IsValid,Is.True);Assert.That(p.inputOpportunity,Is.EqualTo(.5f));Assert.That(p.counterBlackFlashConditional,Is.EqualTo(50));
                foreach(var f in typeof(ShibuyaCounterProfile).GetFields())if(f.DeclaringType==typeof(ShibuyaCounterProfile))Assert.That(ShibuyaInspector.KoreanLabel(f.Name),Is.Not.EqualTo(f.Name));
                var so=new SerializedObject(p);Undo.IncrementCurrentGroup();so.FindProperty("counterStep").FindPropertyRelative("hits").GetArrayElementAtIndex(0).FindPropertyRelative("strike").FindPropertyRelative("damage").floatValue=45;
                so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();Undo.PerformUndo();Assert.That(p.counterStep.hits[0].strike.damage,Is.EqualTo(20));
            }finally{Undo.ClearUndo(p);UnityEngine.Object.DestroyImmediate(p);}
        }
        [Test] public void CounterGuaranteedCriticalAndExplicitFiftyPercentCapException()
        {
            var p=ScriptableObject.CreateInstance<ShibuyaAttackGradeProfile>();try{
                foreach(bool zone in new[]{false,true})foreach(int stacks in new[]{0,14,99}){
                    var m=ShibuyaAttackMetadata.Basic();m.origin=ShibuyaAttackOrigin.Counter;
                    var result=ShibuyaAttackGrades.Probabilities(p,m,zone,stacks,capable:true,counter:true);
                    Assert.That(result.Critical,Is.EqualTo(1));Assert.That(result.FinalBlackFlash,Is.EqualTo(.5f));
                    Assert.That(ShibuyaAttackGrades.Roll(result,new ShibuyaGradeTestRandom(.99f,.4999f)),Is.EqualTo(ShibuyaAttackGrade.BlackFlash));
                    Assert.That(ShibuyaAttackGrades.Roll(result,new ShibuyaGradeTestRandom(.99f,.5f)),Is.EqualTo(ShibuyaAttackGrade.Critical));
                    Assert.That(ShibuyaAttackGrades.Probabilities(p,m,zone,stacks,capable:false,counter:true).FinalBlackFlash,Is.Zero);
                }
            }finally{UnityEngine.Object.DestroyImmediate(p);}
        }
    }
    public sealed class ShibuyaPass4G2Tests : ShibuyaPass4CFixture
    {
        ShibuyaCounterProfile counter;ShibuyaAttackGradeProfile grades;bool complete;
        [UnitySetUp] public IEnumerator CounterSetup(){
            grades=Clone(combat.attackGrades);combat.attackGrades=grades;counter=Clone(combat.counter);combat.counter=counter;
            Place(dummy,player.transform.position+Vector3.forward*1.4f);dummy.transform.rotation=Quaternion.Euler(0,180,0);
            player.SetCombatRandom(new ShibuyaGradeTestRandom(0,.999f));complete=false;yield return null;
        }
        [TearDown] public void CompletionWitness()=>Assert.That(complete,Is.True,"counter test body completion is required");
        void Done(string name,object data){Record(name,data);complete=true;}
        void Parry(){
            player.SetGuardHeld(false);player.SetGuardHeld(true);Assert.That(player.IsParryWindowOpen,Is.True);
            var r=player.ReceiveStrike(new DamageContext(1,dummy.gameObject,DamageDeliveryType.PhysicalStrike,DamageTraits.None,"4G2 parry"),new ShibuyaStrikeDefinition());
            Assert.That(r,Is.EqualTo(DamageResolution.Guarded));Assert.That(player.HasCounterOpportunity,Is.True);Assert.That(player.IsGuarding,Is.False);
        }
        [UnityTest] public IEnumerator SuccessNewClickReplacesNormalAndAppliesOneGuaranteedCritical()
        {
            Parry();frame.guardHeld=true;frame.attack=true;yield return Until(()=>player.CounterStartedCount==1);frame.attack=false;
            yield return Until(()=>player.ComboStep==0);Assert.That(player.CounterInputCount,Is.EqualTo(1));Assert.That(player.AcceptedAttackCount,Is.EqualTo(1));
            Assert.That(player.CounterAppliedCount,Is.EqualTo(1));Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(970));Assert.That(player.LastAttackGrade,Is.EqualTo(ShibuyaAttackGrade.Critical));
            Assert.That(dummy.HasCounterReservation||player.IsCounterBusy,Is.False);Assert.That(player.GrabAttemptCount,Is.Zero);
            Done("4G2-success",new{player.CounterInputCount,player.CounterAppliedCount,player.LastCounterReason,hp=dummy.Health.CurrentHealth});
        }
        [UnityTest] public IEnumerator NoAutomaticAttackExpiryNormalAttackAndFreeDash()
        {
            Parry();yield return Wait(.15f);Assert.That(player.AcceptedAttackCount,Is.Zero);Assert.That(player.TryGroundDash(Vector2.left,Vector3.forward,Vector3.right),Is.True);
            yield return Until(()=>!player.IsGroundDashing);yield return Wait(.3f);Assert.That(player.HasCounterOpportunity,Is.False);Assert.That(player.CounterStartedCount,Is.Zero);
            frame.attack=true;yield return Until(()=>player.AcceptedAttackCount==1);frame.attack=false;Assert.That(player.IsCounterBusy,Is.False);
            Done("4G2-expiry-free-dash",new{player.GroundDashCount,player.CounterStartedCount,player.AcceptedAttackCount});
        }
        [UnityTest] public IEnumerator LongPreparationReservationOutlivesParryStunAndOrdinaryRange()
        {
            counter.counterStep.hits[0].strike.startup=.8f;counter.counterStep.movements=Array.Empty<ShibuyaMovementDefinition>();
            Parry();Assert.That(player.TryCounterInput(),Is.True);yield return Until(()=>player.CounterStartedCount==1);
            // A reserved actor cannot escape via ordinary guard, dodge or a later finished recoil.
            yield return Wait(.55f);Assert.That(dummy.HasCounterReservation,Is.True);dummy.SetGuardHeld(true);
            Assert.That(dummy.IsGuarding,Is.False);Assert.That(dummy.TryGroundDash(Vector2.left,Vector3.forward,Vector3.right),Is.False);Assert.That(dummy.TryAttack(),Is.False);
            Place(dummy,player.transform.position+Vector3.forward*8);float distance=Vector3.Distance(player.transform.position,dummy.transform.position);
            yield return Until(()=>player.CounterAppliedCount==1);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(970));
            yield return Until(()=>!player.IsCounterBusy);Assert.That(dummy.HasCounterReservation,Is.False);Assert.That(player.TryCounterInput(),Is.False);
            Done("4G2-long-lease",new{distance,startup=.8f,player.CounterAppliedCount,hp=dummy.Health.CurrentHealth});
        }
        [UnityTest] public IEnumerator CounterBlackFlashRewardAndHeldClickCannotRepeat()
        {
            grades.blackFlashCapable=true;player.SetCombatRandom(new ShibuyaGradeTestRandom(0,0));input.EnergyCharge.Resource.SetCurrentEnergy(100);
            Parry();frame.attack=true;yield return Until(()=>player.CounterAppliedCount==1);yield return Wait(.75f);
            Assert.That(player.AcceptedAttackCount,Is.EqualTo(1));Assert.That(player.BlackFlashCount,Is.EqualTo(1));Assert.That(player.ZoneStacks,Is.EqualTo(1));
            Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(950));Assert.That(input.EnergyCharge.Resource.CurrentEnergy,Is.EqualTo(110));Assert.That(player.ZoneActive,Is.True);
            Done("4G2-black-flash",new{player.BlackFlashCount,player.ZoneStacks,hp=dummy.Health.CurrentHealth,energy=input.EnergyCharge.Resource.CurrentEnergy});
        }
        [UnityTest] public IEnumerator GuardAndFailedParryNeverOfferCounterAndGrabStillOwnsGuardClick()
        {
            frame.guardHeld=true;yield return Wait(.16f);Assert.That(player.IsGuarding,Is.True);
            var r=player.ReceiveStrike(new DamageContext(1,dummy.gameObject,DamageDeliveryType.PhysicalStrike,DamageTraits.None,"4G2 normal guard"),new ShibuyaStrikeDefinition());
            Assert.That(r,Is.EqualTo(DamageResolution.Guarded));Assert.That(player.HasCounterOpportunity,Is.False);
            frame.attack=true;yield return Until(()=>player.GrabAttemptCount==1);frame.attack=false;Assert.That(player.CounterStartedCount,Is.Zero);
            Done("4G2-grab-priority",new{player.GrabAttemptCount,player.CounterStartedCount,player.GuardBlockCount});
        }
        [UnityTest] public IEnumerator WallBeforeStartRejectsWithoutTeleportOrDamage()
        {
            Parry();var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=(player.transform.position+dummy.transform.position)*.5f+Vector3.up;
            wall.transform.localScale=new Vector3(4,3,.2f);Physics.SyncTransforms();Vector3 before=player.transform.position;
            Assert.That(player.TryCounterInput(),Is.True);Assert.That(player.CounterStartedCount,Is.Zero);Assert.That(player.LastCounterReason,Is.EqualTo("world-obstruction"));
            Assert.That(player.transform.position,Is.EqualTo(before));Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(1000));UnityEngine.Object.Destroy(wall);
            Done("4G2-wall-before",new{player.LastCounterReason,player.CounterStartedCount});yield return null;
        }
        [UnityTest] public IEnumerator WallAppearingDuringReservedStartupCancelsAndReleasesTarget()
        {
            counter.counterStep.hits[0].strike.startup=.6f;Parry();player.TryCounterInput();yield return Until(()=>player.CounterStartedCount==1);
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=(player.transform.position+dummy.transform.position)*.5f+Vector3.up;wall.transform.localScale=new Vector3(4,3,.2f);Physics.SyncTransforms();
            yield return Until(()=>!player.IsCounterBusy);Assert.That(dummy.HasCounterReservation,Is.False);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(1000));
            Assert.That(player.LastCounterReason,Is.EqualTo("world-obstruction"));UnityEngine.Object.Destroy(wall);Done("4G2-wall-during",new{player.LastCounterReason});
        }
        [UnityTest] public IEnumerator SpecialProtectionStillBlocksGuaranteedCounterAndRewards()
        {
            grades.blackFlashCapable=true;player.SetCombatRandom(new ShibuyaGradeTestRandom(0,0));Parry();player.TryCounterInput();
            var special=dummy.gameObject.AddComponent<ShibuyaGradeSpecialGuard>();yield return Until(()=>!player.IsCounterBusy);
            Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(1000));Assert.That(player.CounterAppliedCount+player.BlackFlashCount+player.ZoneStacks,Is.Zero);
            Assert.That(player.LastCounterReason,Is.EqualTo("damage-rejected-Guarded"));Assert.That(dummy.HasCounterReservation,Is.False);UnityEngine.Object.Destroy(special);
            Done("4G2-special-protection",new{player.LastCounterReason});
        }
        [UnityTest] public IEnumerator CaptureLossDamageDisableAndBattleEndReleaseOpportunityAndLease()
        {
            Parry();input.SendMessage("OnApplicationFocus",false);Assert.That(player.HasCounterOpportunity,Is.False);yield return Wait(.6f);
            Place(dummy,player.transform.position+Vector3.forward*1.4f);Parry();counter.counterStep.hits[0].strike.startup=.8f;player.TryCounterInput();
            player.Health.ReceiveDamage(new DamageContext(1,dummy.gameObject,DamageDeliveryType.PhysicalStrike));Assert.That(player.IsCounterBusy||dummy.HasCounterReservation,Is.False);
            yield return Wait(.6f);Place(dummy,player.transform.position+Vector3.forward*1.4f);Parry();player.TryCounterInput();
            player.enabled=false;Assert.That(dummy.HasCounterReservation,Is.False);player.enabled=true;yield return Wait(.6f);
            Place(dummy,player.transform.position+Vector3.forward*1.4f);Parry();player.TryCounterInput();player.StopCombat();Assert.That(player.IsCounterBusy||dummy.HasCounterReservation,Is.False);
            Assert.That(Time.timeScale,Is.EqualTo(1));Done("4G2-cancel-paths",new{player.LastCounterReason});
        }
        [UnityTest] public IEnumerator MultiHitCounterIndependentlyGradesOnlyItsReservedTarget()
        {
            grades.blackFlashCapable=true;player.SetCombatRandom(new ShibuyaGradeTestRandom(0,0,0,0));input.EnergyCharge.Resource.SetCurrentEnergy(100);
            var first=counter.counterStep.hits[0];first.strike.knockbackSpeed=0;first.strike.hitStopDuration=0;
            counter.counterStep.hits=new[]{first,new ShibuyaHitEvent{startTime=.3f,strike=JsonUtility.FromJson<ShibuyaStrikeDefinition>(JsonUtility.ToJson(first.strike))}};
            Parry();player.TryCounterInput();yield return Until(()=>player.CounterAppliedCount==2);yield return Until(()=>!player.IsCounterBusy);
            Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(895));Assert.That(player.BlackFlashCount,Is.EqualTo(2));Assert.That(player.ZoneStacks,Is.EqualTo(2));Assert.That(input.EnergyCharge.Resource.CurrentEnergy,Is.EqualTo(120));
            Done("4G2-multihit",new{player.CounterAppliedCount,player.BlackFlashCount,player.ZoneStacks});
        }
        [UnityTest] public IEnumerator ExactInputDeadlineIsExclusiveAndTimersAreIndependent()
        {
            Parry();yield return Wait(.12f);float now=Time.time;Set(player,"counterOpportunityUntil",now+.0001f);
            Assert.That(player.HasCounterOpportunity,Is.True);Assert.That(player.TryCounterInput(),Is.True);Assert.That(player.CounterTarget,Is.SameAs(dummy));
            player.CancelCounter("boundary-test-reset");yield return Wait(.5f);Place(dummy,player.transform.position+Vector3.forward*1.4f);Parry();yield return Wait(.12f);
            Set(player,"counterOpportunityUntil",Time.time);Assert.That(player.HasCounterOpportunity,Is.False);Assert.That(player.TryCounterInput(),Is.False);Assert.That(player.TryAttack(),Is.True);
            Assert.That(player.CounterStartedCount,Is.LessThanOrEqualTo(1));Done("4G2-boundary",new{policy="strictly-before deadline accepts; equal deadline expires",player.CounterInputCount});
        }
        [UnityTest] public IEnumerator InvulnerabilityCinematicAndTargetDeathAreExplicitExceptions()
        {
            Parry();dummy.Health.GrantInvulnerability(.1f);Assert.That(player.TryCounterInput(),Is.True);Assert.That(player.LastCounterReason,Is.EqualTo("target-special-invulnerable"));
            yield return Wait(.6f);Place(dummy,player.transform.position+Vector3.forward*1.4f);Parry();player.TryCounterInput();
            var film=Clone(AssetDatabase.LoadAssetAtPath<ShibuyaCinematicProfile>(ShibuyaCinematicExample.Path));film.cinematicEnabled=true;
            var session=ShibuyaCinematicSession.TryStartAfterApplied(player,dummy,film,DamageResolution.Applied,ShibuyaCinematicTrigger.Default,700,0);
            Assert.That(session,Is.Not.Null);Assert.That(player.IsCounterBusy||dummy.HasCounterReservation,Is.False);session.Finish("counter ownership QA");
            yield return Wait(.65f);Place(dummy,player.transform.position+Vector3.forward*1.4f);Parry();player.TryCounterInput();dummy.Health.Kill();
            Assert.That(player.IsCounterBusy||dummy.HasCounterReservation,Is.False);Assert.That(player.CounterAppliedCount,Is.Zero);
            Done("4G2-top-protections",new{player.LastCounterReason,player.CounterAppliedCount});
        }
        [UnityTest] public IEnumerator OtherCombatPairContinuesAndReviewToolRestoresRuntimeCopies()
        {
            var originalProfile=player.Profile;var originalStats=player.StatsProfile;
            var window=ScriptableObject.CreateInstance<ShibuyaCombatGradeReview>();
            typeof(ShibuyaCombatGradeReview).GetMethod("Bind",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).Invoke(window,new object[]{player});
            Assert.That(player.Profile,Is.Not.SameAs(originalProfile));Assert.That(player.StatsProfile,Is.SameAs(originalStats));UnityEngine.Object.DestroyImmediate(window);
            Assert.That(player.Profile,Is.SameAs(originalProfile));Assert.That(player.StatsProfile,Is.SameAs(originalStats));
            counter.counterStep.hits[0].strike.startup=.8f;Parry();player.TryCounterInput();
            var c=UnityEngine.Object.Instantiate(dummy.gameObject,new Vector3(10,.03f,-8),Quaternion.identity).GetComponent<ShibuyaFighter>();
            var d=UnityEngine.Object.Instantiate(dummy.gameObject,new Vector3(10,.03f,-6.6f),Quaternion.Euler(0,180,0)).GetComponent<ShibuyaFighter>();
            c.SetCombatRandom(new ShibuyaGradeTestRandom(.99f));yield return Wait(.1f);Assert.That(c.TryAttack(),Is.True);
            yield return Until(()=>c.ConfirmedHitCount==1);Assert.That(d.Health.CurrentHealth,Is.EqualTo(980));Assert.That(c.HasCounterReservation||d.HasCounterReservation,Is.False);
            Assert.That(Time.timeScale,Is.EqualTo(1));Done("4G2-third-pair-and-tool",new{otherHits=c.ConfirmedHitCount,otherHp=d.Health.CurrentHealth,restored=true});
        }
        [UnityTest] public IEnumerator OriginalSpeedParryCounterAndZoneEvidence()
        {
            grades.blackFlashCapable=true;player.SetCombatRandom(new ShibuyaGradeTestRandom(0,0));input.EnergyCharge.Resource.SetCurrentEnergy(100);
            Pose(new Vector3(3.5f,2.1f,-4.5f));var rows=new System.Collections.Generic.List<object>();double start=Time.timeAsDouble;bool parried=false,clicked=false;int index=0;
            while(Time.timeAsDouble-start<2.2){double t=Time.timeAsDouble-start;
                if(!parried&&t>=.3){Parry();parried=true;}
                if(parried&&!clicked&&t>=.48){frame.attack=true;clicked=true;}else if(clicked)frame.attack=false;
                Capture("4G2-media/parry-counter/frame-"+index.ToString("D4"));
                rows.Add(new{index,time=t,player.CounterOpportunityRemaining,player.CounterStartedCount,player.CounterAppliedCount,player.LastAttackGrade,hp=dummy.Health.CurrentHealth,energy=input.EnergyCharge.Resource.CurrentEnergy,player.ZoneStacks,player.ZoneRemaining});
                index++;yield return Wait(1f/40);
            }
            Assert.That(player.CounterAppliedCount,Is.EqualTo(1));Assert.That(player.BlackFlashCount,Is.EqualTo(1));Done("4G2-media-timestamps",rows);
        }
    }
}
