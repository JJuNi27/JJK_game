using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass4G3DeliveryTests : ShibuyaPass4CFixture
    {
        readonly List<object> health=new List<object>();
        readonly List<ShibuyaCombatDamageResult> received=new List<ShibuyaCombatDamageResult>();
        readonly List<ShibuyaCombatDamageResult> presented=new List<ShibuyaCombatDamageResult>();
        readonly List<object> checkpoints=new List<object>();
        readonly List<object> frameTrace=new List<object>();
        readonly List<bool> visibleNewHits=new List<bool>();
        ShibuyaDamageNumbers numbers;bool complete;
        ShibuyaCombatGradeReview reviewWindow;
        [UnitySetUp] public IEnumerator DeliverySetup(){
            health.Clear();received.Clear();presented.Clear();checkpoints.Clear();frameTrace.Clear();visibleNewHits.Clear();complete=false;
            numbers=input.DamageNumbers;Place(dummy,player.transform.position+Vector3.forward*1.4f);dummy.transform.rotation=Quaternion.Euler(0,180,0);
            Pose(new Vector3(3.4f,2.1f,-4.5f));
            float hp=dummy.Health.CurrentHealth;
            dummy.Health.DamageResolved+=(_,context,resolution)=>{if(resolution==DamageResolution.Applied){
                float loss=hp-dummy.Health.CurrentHealth;hp=dummy.Health.CurrentHealth;
                health.Add(new{loss,hp,requested=context.Amount,action=player.AcceptedAttackCount,combo=player.ComboStep,
                    comboToken=(int)typeof(ShibuyaFighter).GetField("sequenceToken",Private).GetValue(player),
                    hitEvent=(int)typeof(ShibuyaFighter).GetField("resolvingEvent",Private).GetValue(player),time=Time.unscaledTimeAsDouble});}};
            ShibuyaCombatDamageResults.Resolved+=Receive;numbers.Presented+=Presented;
            yield return null;
        }
        void Receive(ShibuyaCombatDamageResult r){if(r.Attacker==player)received.Add(r);}
        void Presented(ShibuyaCombatDamageResult r){if(r.Attacker==player)presented.Add(r);}
        [UnityTearDown] public IEnumerator DeliveryTeardown(){ShibuyaCombatDamageResults.Resolved-=Receive;if(numbers!=null)numbers.Presented-=Presented;if(reviewWindow!=null)UnityEngine.Object.DestroyImmediate(reviewWindow);yield return null;}
        [TearDown] public void CompletionWitness()=>Assert.That(complete,Is.True,"delivery trace must reach final assertions");
        object Result(ShibuyaCombatDamageResult r)=>new{r.Key.Source,r.Key.Sequence,r.Key.HitEvent,r.Key.Target,r.Key.Action,r.Grade,r.Resolution,r.HealthLoss};
        int LastSlot(){if(presented.Count==0)return -1;var pool=(Array)typeof(ShibuyaDamageNumbers).GetField("pool",Private).GetValue(numbers);
            for(int i=0;i<pool.Length;i++){object p=pool.GetValue(i);var t=p.GetType();if((bool)t.GetField("active").GetValue(p)&&((ShibuyaCombatDamageResult)t.GetField("result").GetValue(p)).Key.Equals(presented.Last().Key))return i;}return -1;}
        IEnumerator Screen(string name){string path=Path.Combine(Root,TestContext.CurrentContext.Test.Name,name+".png");Directory.CreateDirectory(Path.GetDirectoryName(path));ScreenCapture.CaptureScreenshot(path);yield return Until(()=>File.Exists(path));yield return null;}
        IEnumerator Checkpoint(string name){yield return null;int slot=LastSlot();bool visible=numbers.TryGetLayout(slot,out var layout,out string reason);
            visibleNewHits.Add(presented.Count==health.Count&&visible&&layout.Alpha>0);
            checkpoints.Add(new{name,health=health.Count,received=received.Count,created=presented.Count,newPopupForEveryHit=presented.Count==health.Count,active=numbers.ActiveCount,slot,visible,reason,alpha=layout.Alpha,
                rectangle=new{x=layout.Rect.x,y=layout.Rect.y,width=layout.Rect.width,height=layout.Rect.height},observerCount=ShibuyaCombatDamageResults.ObserverCount,playerId=player.GetInstanceID(),victimId=dummy.GetInstanceID(),displayId=numbers.GetInstanceID(),
                text=slot>=0?numbers.GetPopupText(slot):"",lastReason=numbers.LastReason});yield return Screen(name);}
        void Save(string name){Record(name,new{health,received=received.Select(Result).ToArray(),created=presented.Select(Result).ToArray(),checkpoints,frameTrace,visibleNewHits,
            healthCount=health.Count,receivedCount=received.Count,createdCount=presented.Count,gradeCommits=player.GradeAppliedCount,
            currentHp=dummy.Health.CurrentHealth,capacity=numbers.Capacity,lifetime=numbers.Profile.displayDuration});complete=true;}
        IEnumerator AirWait(string phase,Func<bool> condition){
            double until=Time.unscaledTimeAsDouble+3;var samples=new List<object>();
            do{samples.Add(new{time=Time.unscaledTimeAsDouble,dt=Time.unscaledDeltaTime,player.ComboStep,player.AcceptedAttackCount,player.ConfirmedHitCount,
                player.AttackElapsed,player.AttackDuration,player.IsInHitStop,player.CanAirAttack,player.IsAirAttacking,player.IsAerialHovering,
                px=player.transform.position.x,py=player.transform.position.y,pz=player.transform.position.z,dx=dummy.transform.position.x,dy=dummy.transform.position.y,dz=dummy.transform.position.z,
                health=health.Count,received=received.Count,created=presented.Count});
                if(condition())break;yield return null;
            }while(Time.unscaledTimeAsDouble<until);
            Record("air-wait-"+phase,new{success=condition(),samples});Assert.That(condition(),Is.True,phase);
        }
        IEnumerator AirClick(int action){
            // Editor test continuations can run several times before one Player Update. Hold this
            // edge until the real input consumer accepts it, then observe that it consumed release.
            frame.attack=true;yield return AirWait("input-accepted-"+action,()=>player.AcceptedAttackCount==action);
            frame.attack=false;yield return AirWait("input-released-"+action,
                ()=>!(bool)typeof(ShibuyaPlayerInput).GetField("previousAttack",Private).GetValue(input));
        }
        IEnumerator Four(ShibuyaFinisher finisher=ShibuyaFinisher.Default,bool moving=false){
            Assert.That(player.TryAttack(),Is.True);
            for(int hit=1;hit<=4;hit++){
                double deadline=Time.unscaledTimeAsDouble+3;
                while(player.ConfirmedHitCount<hit&&Time.unscaledTimeAsDouble<deadline){if(moving)Pose(new Vector3(Mathf.Sin(Time.time*.7f)*2.2f,2.4f,-4.8f));yield return null;}
                Assert.That(player.ConfirmedHitCount,Is.GreaterThanOrEqualTo(hit),"bounded actual combo hit");
                yield return Checkpoint("four-step-hit-"+hit);
                if(hit<4){yield return Until(()=>player.AttackElapsed>=player.AttackDuration-.12f);Assert.That(player.TryAttack(finisher),Is.True);int next=hit+1;yield return Until(()=>player.ComboStep==next);}
            }
        }
        [UnityTest] public IEnumerator FourAcceptedStepsEachPublishAndRenderOneReceipt()
        {
            yield return Four();Assert.That(player.ConfirmedHitCount,Is.EqualTo(4),"four-step body completed");
            Save("four-step-delivery");Assert.That(health.Count,Is.EqualTo(4));Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(915));
            Assert.That(received.Count,Is.EqualTo(4),"four positive HP transactions must reach the result bus");Assert.That(presented.Count,Is.EqualTo(4));
            Assert.That(received.Select(r=>r.Key).Distinct().Count(),Is.EqualTo(4));
            Assert.That(visibleNewHits,Is.EqualTo(new[]{true,true,true,true}));
        }
        [UnityTest] public IEnumerator MixedGradesAcrossStepsKeepFourDisplaysAndTwoBlackFlashRewards()
        {
            combat.attackGrades=Clone(combat.attackGrades);combat.attackGrades.blackFlashCapable=true;
            player.SetCombatRandom(new ShibuyaGradeTestRandom(.99f,0,.99f,0,0,0,0));input.EnergyCharge.Resource.SetCurrentEnergy(100);
            yield return Four();Assert.That(player.ConfirmedHitCount,Is.EqualTo(4));Save("mixed-four-step-delivery");
            Assert.That(health.Count,Is.EqualTo(4));Assert.That(received.Count,Is.EqualTo(4));Assert.That(presented.Count,Is.EqualTo(4));
            Assert.That(received.Select(r=>r.HealthLoss),Is.EqualTo(new[]{20f,30,50,68.75f}));
            Assert.That(received.Select(r=>r.Grade),Is.EqualTo(new[]{ShibuyaAttackGrade.Normal,ShibuyaAttackGrade.Critical,ShibuyaAttackGrade.BlackFlash,ShibuyaAttackGrade.BlackFlash}));
            Assert.That(player.BlackFlashCount,Is.EqualTo(2));Assert.That(player.ZoneStacks,Is.EqualTo(2));Assert.That(input.EnergyCharge.Resource.CurrentEnergy,Is.EqualTo(120));
            Assert.That(visibleNewHits,Is.EqualTo(new[]{true,true,true,true}));
        }
        [UnityTest] public IEnumerator CameraAndKnockbackMovementDoNotDropVisibleNewHits()
        {
            yield return Four(moving:true);Assert.That(player.ConfirmedHitCount,Is.EqualTo(4));Save("moving-four-step-delivery");
            Assert.That(received.Count,Is.EqualTo(4));Assert.That(presented.Count,Is.EqualTo(4));
            Assert.That(dummy.transform.position.z,Is.GreaterThan(-6.6f));
            Assert.That(numbers.Profile.displayDuration,Is.EqualTo(.85f));Assert.That(visibleNewHits,Is.EqualTo(new[]{true,true,true,true}));
        }
        [UnityTest] public IEnumerator OffOnAcrossOneComboOnlySuppressesTheOffStepAndSameReceiptNeverRepeats()
        {
            Assert.That(player.TryAttack(),Is.True);
            for(int hit=1;hit<=4;hit++){
                yield return Until(()=>player.ConfirmedHitCount==hit);if(hit==2)Assert.That(numbers.ActiveCount,Is.Zero);
                if(hit<4){numbers.SetDisplayEnabled(hit!=1);yield return Until(()=>player.AttackElapsed>=player.AttackDuration-.12f);Assert.That(player.TryAttack(),Is.True);int next=hit+1;yield return Until(()=>player.ComboStep==next);}
            }
            Save("off-on-in-one-combo");Assert.That(health.Count,Is.EqualTo(4));Assert.That(received.Count,Is.EqualTo(4));Assert.That(presented.Count,Is.EqualTo(3));
            Assert.That(presented.Select(x=>x.Key.Sequence),Is.EqualTo(new[]{1,3,4}));int count=numbers.PresentedCount;numbers.Present(received.Last());Assert.That(numbers.PresentedCount,Is.EqualTo(count));
            Assert.That(numbers.LastReason,Is.EqualTo("중복 타격 결과"));Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(915));
        }
        [UnityTest] public IEnumerator AirThreeClicksEachPublishAllFiveHits()
        {
            Place(player,new Vector3(0,.03f,-1.4f));Place(dummy,new Vector3(0,.03f,0));player.transform.rotation=Quaternion.identity;
            // Use the existing pursuit QA's input cadence; screenshot I/O must not delay its tight airborne window.
            Assert.That(player.TryAttack(),Is.True);
            for(int i=1;i<=3;i++){yield return AirWait("ground-buffer-"+i,()=>player.ComboStep==i&&player.AttackElapsed>=player.AttackDuration-.12f);Assert.That(player.TryAttack(ShibuyaFinisher.Uppercut),Is.True);int step=i+1;yield return AirWait("ground-start-"+step,()=>player.ComboStep==step);}
            yield return AirWait("ground-four-hits",()=>player.ConfirmedHitCount==4);Record("air-phase-before-space",new{player.ComboStep,player.IsInHitStop,player.HasAirChaseOpportunity,player.AirChaseCount});player.SetIntent(Vector3.zero,false,true);
            yield return AirWait("chase-arrival",()=>player.CanAirAttack);Record("air-phase-arrived",new{player.AirChaseCount,player.AirChaseContactCount,player.AcceptedAttackCount});int beforeHealth=health.Count,beforeReceived=received.Count,beforeCreated=presented.Count;
            yield return AirClick(5);
            yield return AirWait("first-air-hit",()=>player.ConfirmedHitCount>=5);
            Record("air-phase-first-click",new{player.AcceptedAttackCount,player.ConfirmedHitCount,player.AttackElapsed,player.AttackDuration,health=health.Count,received=received.Count,created=presented.Count});
            yield return AirWait("second-input-window",()=>player.AttackElapsed>=player.AttackDuration-player.AerialDefinition.aerialLinkInputWindow+.015f);
            yield return AirClick(6);
            Record("air-phase-second-click",new{player.AcceptedAttackCount,player.ConfirmedHitCount});
            yield return AirWait("second-ended",()=>!player.IsAirAttacking);yield return Wait(.035f);yield return AirClick(7);
            yield return AirWait("hover-ended",()=>!player.IsAerialHovering);
            Save("air-five-hit-delivery");Assert.That(health.Count-beforeHealth,Is.EqualTo(5));Assert.That(received.Count-beforeReceived,Is.EqualTo(5));Assert.That(presented.Count-beforeCreated,Is.EqualTo(5));
            Assert.That(received.Skip(beforeReceived).Select(r=>r.Key).Distinct().Count(),Is.EqualTo(5));
        }
        [UnityTest] public IEnumerator TwoAcceptedMultihitStepsMayReuseEventIndicesWithoutBeingDuplicates()
        {
            var m=ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();m.name="PASS4B3 QA delivery multi-step";
            Func<ShibuyaMovesetStep> step=()=>new ShibuyaMovesetStep{hits=Enumerable.Range(0,2).Select(i=>new ShibuyaHitEvent{startTime=i*.14f,
                strike=new ShibuyaStrikeDefinition{attack=ShibuyaAttackMetadata.Basic(),damage=20,startup=.03f,activeTime=.03f,recovery=.15f,
                    hitStun=.12f,knockbackSpeed=0,advanceDistance=0}}).ToArray()};
            m.steps=new[]{step(),step()};Assert.That(player.SetMoveset(m),Is.True);
            Assert.That(player.TryAttack(),Is.True);yield return Until(()=>player.AttackElapsed>=player.AttackDuration-.10f);Assert.That(player.TryAttack(),Is.True);
            yield return Until(()=>player.ConfirmedHitCount==4);Save("two-step-two-event-delivery");
            Assert.That(health.Count,Is.EqualTo(4));Assert.That(received.Count,Is.EqualTo(4));Assert.That(presented.Count,Is.EqualTo(4));
            Assert.That(received.Select(x=>x.Key.HitEvent),Is.EqualTo(new[]{0,1,0,1}));Assert.That(received.Select(x=>x.Key.Sequence),Is.EqualTo(new[]{1,1,2,2}));
        }
        [UnityTest] public IEnumerator BehindCameraIsCreatedOnceThenRevealedWhenCameraReturns()
        {
            var old=camera.transform.rotation;camera.transform.rotation=old*Quaternion.Euler(0,180,0);Assert.That(player.TryAttack(),Is.True);yield return Until(()=>player.ConfirmedHitCount==1);
            yield return null;int slot=LastSlot();Assert.That(presented.Count,Is.EqualTo(1));Assert.That(numbers.TryGetLayout(slot,out _,out string reason),Is.False);Assert.That(reason,Is.EqualTo("카메라 뒤/근접면"));
            camera.transform.rotation=old;yield return Checkpoint("camera-return-reveal");Assert.That(numbers.TryGetLayout(slot,out _,out _),Is.True);Assert.That(presented.Count,Is.EqualTo(1));Save("hidden-reveal-delivery");
        }
        [UnityTest] public IEnumerator QaWindowStaleReferenceAfterRestartDoesNotOwnDamageDelivery()
        {
            var window=ScriptableObject.CreateInstance<ShibuyaCombatGradeReview>();reviewWindow=window;window.name="PASS4G3 Delivery QA Window";var bind=typeof(ShibuyaCombatGradeReview).GetMethod("Bind",BindingFlags.NonPublic|BindingFlags.Instance);bind.Invoke(window,new object[]{player});
            var actor=typeof(ShibuyaCombatGradeReview).GetField("actor",BindingFlags.NonPublic|BindingFlags.Instance);
            Assert.That(actor.GetValue(window),Is.SameAs(player));yield return Four();Assert.That(presented.Count,Is.EqualTo(4));
            Record("qa-window-before-restart",new{health=health.Count,received=received.Count,created=presented.Count});
            var oldMatch=UnityEngine.Object.FindFirstObjectByType<ShibuyaMatch>();dummy.Health.Kill();Assert.That(oldMatch.Finished,Is.True);oldMatch.RestartBattle();yield return Until(()=>UnityEngine.Object.FindFirstObjectByType<ShibuyaMatch>()!=oldMatch);
            var stale=(ShibuyaFighter)actor.GetValue(window);Assert.That(stale==null,Is.True,"Editor window still holds the destroyed old actor");
            var match=UnityEngine.Object.FindFirstObjectByType<ShibuyaMatch>();player=match.Player;dummy=match.Dummy;input=player.GetComponent<ShibuyaPlayerInput>();yield return Until(()=>input.DamageNumbers!=null);numbers=input.DamageNumbers;
            health.Clear();received.Clear();presented.Clear();checkpoints.Clear();visibleNewHits.Clear();numbers.Presented+=Presented;
            float beforeHp=dummy.Health.CurrentHealth;dummy.Health.DamageResolved+=(_,c,r)=>{if(r==DamageResolution.Applied){health.Add(new{loss=beforeHp-dummy.Health.CurrentHealth});beforeHp=dummy.Health.CurrentHealth;}};
            input.TestInput=()=>default;player.SetCombatRandom(new ShibuyaGradeTestRandom(.99f));Place(player,new Vector3(0,.03f,-1.4f));Place(dummy,new Vector3(0,.03f,0));player.transform.rotation=Quaternion.identity;
            Assert.That(player.TryAttack(),Is.True);yield return Until(()=>numbers.PresentedCount==1);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(980));
            bind.Invoke(window,new object[]{player});Assert.That(actor.GetValue(window),Is.SameAs(player));UnityEngine.Object.DestroyImmediate(window);Save("qa-window-restart-reference");
            Assert.That(health.Count,Is.EqualTo(1));Assert.That(received.Count,Is.EqualTo(1));Assert.That(presented.Count,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator OriginalSpeedFourStepScreenTrace()
        {
            double begin=Time.unscaledTimeAsDouble;int clicks=0,index=0,lastSeen=0;float until=(float)(begin+2.8);
            while(Time.unscaledTimeAsDouble<until){
                if(clicks==0){Assert.That(player.TryAttack(),Is.True);clicks++;}
                else if(clicks<4&&player.ComboStep==clicks&&player.AttackElapsed>=player.AttackDuration-.12f){Assert.That(player.TryAttack(),Is.True);clicks++;}
                int slot=LastSlot();bool visible=numbers.TryGetLayout(slot,out var layout,out string reason);
                string path=Path.Combine(Root,"four-step-media/frame-"+index.ToString("D4")+".png");Directory.CreateDirectory(Path.GetDirectoryName(path));ScreenCapture.CaptureScreenshot(path);
                frameTrace.Add(new{index,time=Time.unscaledTimeAsDouble-begin,health=health.Count,received=received.Count,created=presented.Count,visible,reason,text=numbers.GetPopupText(slot),newHit=health.Count!=lastSeen});
                lastSeen=health.Count;index++;yield return Wait(1f/30);
            }
            yield return null;Save("four-step-media-timestamps");Assert.That(health.Count,Is.EqualTo(4));Assert.That(received.Count,Is.EqualTo(4));Assert.That(presented.Count,Is.EqualTo(4));
        }
    }
}
