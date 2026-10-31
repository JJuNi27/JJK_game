using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass4D1DataTests
    {
        [Test] public void ParryProfileIndependentKoreanFieldsAndUndo()
        {
            var c=AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root+"/Data/Shibuya Combat Profile.asset");
            var p=Object.Instantiate(c.guard.parry);
            try {
                Assert.That(p.windowDuration,Is.EqualTo(.12f));Assert.That(p.retryInterval,Is.EqualTo(.5f));Assert.That(p.halfAngle,Is.EqualTo(60));
                Assert.That(p.attackerStun,Is.EqualTo(.5f));Assert.That(p.pushDistance,Is.EqualTo(1.2f));Assert.That(p.successMotionDuration,Is.EqualTo(.1f));
                Assert.That(p.localHitStop,Is.Zero);Assert.That(p.effectShader,Is.Not.Null);Assert.That(ShaderUtil.ShaderHasError(p.effectShader),Is.False);
                Assert.That(c.strikes.All(s=>s.parryable&&s.guardable),Is.True);
                var so=new SerializedObject(p);var it=so.GetIterator();bool child=true;
                while(it.NextVisible(child)){child=false;if(it.name!="m_Script")Assert.That(ShibuyaParryInspector.Labels.ContainsKey(it.name),Is.True,it.name);}
                Undo.IncrementCurrentGroup();so.FindProperty("windowDuration").floatValue=.2f;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();Undo.PerformUndo();
                Assert.That(p.windowDuration,Is.EqualTo(.12f));Assert.That(c.guard.parry.windowDuration,Is.EqualTo(.12f));
            }finally{Undo.ClearUndo(p);Object.DestroyImmediate(p);}
        }
    }
    public sealed class ShibuyaPass4D1Tests : ShibuyaPass4CFixture
    {
        private IEnumerator Up()
        {
            frame.guardHeld=false;yield return Until(()=>!(bool)typeof(ShibuyaFighter).GetField("guardInputHeld",Private).GetValue(player));
        }
        private IEnumerator Down(bool ready=true)
        {
            yield return Up();if(ready)yield return Until(()=>player.ParryRetryRemaining<=0&&!player.IsParryRecovering);
            frame.guardHeld=true;yield return Until(()=>player.IsGuarding);
        }
        private DamageResolution Hit(float angle=0,bool parryable=true,bool guardable=true,DamageDeliveryType type=DamageDeliveryType.PhysicalStrike,DamageTraits traits=DamageTraits.None)
        {
            Place(dummy,player.transform.position+Quaternion.AngleAxis(angle,Vector3.up)*player.transform.forward*2);
            return player.ReceiveStrike(new DamageContext(1,dummy.gameObject,type,traits,"PASS4D1 QA"),new ShibuyaStrikeDefinition{guardDamage=16,guardable=guardable,parryable=parryable});
        }
        private void ActualAttack(bool multi=false)
        {
            Place(player,new Vector3(0,.03f,-1.4f));Place(dummy,new Vector3(0,.03f,0));dummy.transform.rotation=Quaternion.Euler(0,180,0);
            var s=combat.strikes[0];s.startup=multi?.3f:.1f;s.activeTime=.03f;s.recovery=.05f;s.advanceDistance=0;s.hitStopDuration=multi?0:.067f;
            var m=ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();m.name="PASS4B3 QA parry actual attack";
            var cine=Clone(AssetDatabase.LoadAssetAtPath<ShibuyaCinematicProfile>(ShibuyaSceneBuilder.Root+"/Data/Shibuya Cinematic Example.asset"));cine.cinematicEnabled=true;
            m.finisherStepIndex=multi?1:0;
            m.steps=multi?new[]{new ShibuyaMovesetStep{hits=new[]{new ShibuyaHitEvent{strike=s},new ShibuyaHitEvent{strike=s,startTime=.01f}}},
                new ShibuyaMovesetStep{hits=new[]{new ShibuyaHitEvent{strike=s,cinematic=cine}}}}
                :new[]{new ShibuyaMovesetStep{hits=new[]{new ShibuyaHitEvent{strike=s,cinematic=cine}}}};
            Assert.That(player.SetMoveset(m),Is.True);Physics.SyncTransforms();
        }
        [UnityTest] public IEnumerator FreshPressWindowExpiryCooldownAndHeldGuard()
        {
            yield return Down();double start=Time.timeAsDouble;
            Assert.That(player.IsParryWindowOpen,Is.True);Assert.That(player.ParryAttemptCount,Is.EqualTo(1));
            yield return Wait(.05f);Assert.That(player.IsParryWindowOpen,Is.True);
            yield return Wait(.09f);Assert.That(player.IsParryWindowOpen,Is.False);Assert.That(player.IsGuarding,Is.True);
            float hp=player.Health.CurrentHealth;Assert.That(Hit(),Is.EqualTo(DamageResolution.Guarded));
            Assert.That(player.ParrySuccessCount,Is.Zero);Assert.That(player.CurrentGuard,Is.EqualTo(84));Assert.That(player.Health.CurrentHealth,Is.EqualTo(hp));
            yield return Down(false);Assert.That(player.IsParryWindowOpen,Is.False);Assert.That(player.ParryAttemptCount,Is.EqualTo(1));
            yield return Wait(.5f);Assert.That(player.IsParryWindowOpen,Is.False);Assert.That(player.ParryAttemptCount,Is.EqualTo(1));
            yield return Down();Assert.That(player.IsParryWindowOpen,Is.True);Assert.That(player.ParryAttemptCount,Is.EqualTo(2));
            Record("4D1-window-cooldown",new{elapsed=Time.timeAsDouble-start,attempts=player.ParryAttemptCount,guard=player.CurrentGuard});
        }
        [UnityTest] public IEnumerator RapidReleasePressCannotExtendOrAutomaticallyRearm()
        {
            var starts=new List<double>();int last=0;double began=Time.timeAsDouble;
            while(Time.timeAsDouble-began<.85) {
                yield return Down(false);
                if(player.ParryAttemptCount!=last){last=player.ParryAttemptCount;starts.Add(Time.timeAsDouble);}
                yield return Wait(.035f);
            }
            Assert.That(starts.Count,Is.InRange(1,2));
            for(int i=1;i<starts.Count;i++)Assert.That(starts[i]-starts[i-1],Is.GreaterThanOrEqualTo(.48));
            yield return Wait(.6f);Assert.That(player.ParryAttemptCount,Is.EqualTo(last));Assert.That(player.IsParryWindowOpen,Is.False);
            Record("4D1-spam",new{starts,attempts=last});
        }
        [UnityTest] public IEnumerator ConeAndIndependentGuardParryFlagsAndSpecialDamage()
        {
            var samples=new List<object>();
            foreach(float angle in new[]{0f,59f,61f,180f}) {
                yield return Down();camera.transform.rotation=Quaternion.Euler(0,180,0);
                float hp=player.Health.CurrentHealth,g=player.CurrentGuard;int successes=player.ParrySuccessCount;
                var result=Hit(angle);bool valid=angle<60;
                Assert.That(result,Is.EqualTo(valid?DamageResolution.Guarded:DamageResolution.Applied));
                Assert.That(player.ParrySuccessCount,Is.EqualTo(successes+(valid?1:0)));Assert.That(player.CurrentGuard,Is.EqualTo(g));
                Assert.That(player.Health.CurrentHealth,Is.EqualTo(valid?hp:hp-1));samples.Add(new{angle,valid,result=result.ToString()});
            }
            yield return Down();float before=player.CurrentGuard;int parries=player.ParrySuccessCount;
            Assert.That(Hit(parryable:false),Is.EqualTo(DamageResolution.Guarded));Assert.That(player.CurrentGuard,Is.EqualTo(before-16));Assert.That(player.ParrySuccessCount,Is.EqualTo(parries));
            yield return Down();before=player.CurrentGuard;Assert.That(Hit(guardable:false),Is.EqualTo(DamageResolution.Guarded));Assert.That(player.ParrySuccessCount,Is.EqualTo(parries+1));Assert.That(player.CurrentGuard,Is.EqualTo(before));
            foreach(var type in new[]{DamageDeliveryType.Environmental,DamageDeliveryType.DomainSureHit,DamageDeliveryType.CursedTechnique}) {
                yield return Down();parries=player.ParrySuccessCount;Assert.That(Hit(type:type),Is.EqualTo(DamageResolution.Applied));Assert.That(player.ParrySuccessCount,Is.EqualTo(parries));
            }
            yield return Down();parries=player.ParrySuccessCount;Assert.That(Hit(traits:DamageTraits.Unblockable),Is.EqualTo(DamageResolution.Applied));Assert.That(player.ParrySuccessCount,Is.EqualTo(parries));
            Record("4D1-policy",samples);
        }
        [UnityTest] public IEnumerator ActualParryCancelsBufferedMultiEventsWithoutHpOrAutoCounter()
        {
            ActualAttack(true);var extra=new GameObject("PASS4D1 duplicate collider");extra.transform.SetParent(dummy.transform,false);extra.AddComponent<BoxCollider>().center=Vector3.up;
            int events=0;player.HitEventStarted+=_=>events++;
            Vector3 start=player.transform.position,atParry=start;
            dummy.ParrySucceeded+=_=>atParry=player.transform.position;
            Assert.That(player.TryAttack(),Is.True);yield return Until(()=>player.AttackElapsed>=.27f);
            Assert.That(player.TryAttack(),Is.True);Assert.That(player.HasBufferedAttack,Is.True);dummy.SetGuardHeld(true);
            yield return Until(()=>dummy.ParrySuccessCount==1);
            Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(1000));Assert.That(player.Health.CurrentHealth,Is.EqualTo(100));Assert.That(dummy.CurrentGuard,Is.EqualTo(100));
            Assert.That(dummy.GuardBlockCount,Is.Zero);Assert.That(dummy.GuardBreakCount,Is.Zero);Assert.That(player.ConfirmedHitCount,Is.Zero);Assert.That(player.HasBufferedAttack,Is.False);
            Assert.That(player.State,Is.EqualTo(ShibuyaFighterState.Hitstunned));Assert.That(dummy.IsParryRecovering,Is.True);Assert.That(dummy.TryAttack(),Is.False);
            float stunRemaining=(float)typeof(ShibuyaFighter).GetField("stunnedUntil",Private).GetValue(player)-player.CombatTime;
            Assert.That(stunRemaining,Is.InRange(.45f,.5001f));
            yield return Wait(.5f);
            float moved=Vector3.Distance(atParry,player.transform.position);
            Assert.That(moved,Is.EqualTo(1.2f).Within(.04f));Assert.That(events,Is.EqualTo(1));Assert.That(player.AcceptedAttackCount,Is.EqualTo(1));
            Assert.That(dummy.ParrySuccessCount,Is.EqualTo(1));Assert.That(dummy.AcceptedAttackCount,Is.Zero);Assert.That(player.IsInCinematic||dummy.IsInCinematic,Is.False);
            Assert.That(dummy.TryAttack(),Is.True);
            Record("4D1-real-multi",new{moved,preparationAdvance=Vector3.Distance(start,atParry),stunRemaining,events,sourceHp=player.Health.CurrentHealth,defenderHp=dummy.Health.CurrentHealth,parries=dummy.ParrySuccessCount,sourceHits=player.ConfirmedHitCount});
        }
        [UnityTest] public IEnumerator ActualFinisherParryDoesNotStartCinematicOrHitFeedback()
        {
            ActualAttack();Assert.That(player.TryAttack(),Is.True);yield return Until(()=>player.AttackElapsed>=.04f);dummy.SetGuardHeld(true);
            yield return Until(()=>dummy.ParrySuccessCount==1);yield return Wait(.03f);
            Assert.That(player.ConfirmedHitCount,Is.Zero);Assert.That(player.HitStopCount,Is.Zero);Assert.That(dummy.GuardBlockCount,Is.Zero);
            Assert.That(player.IsInCinematic||dummy.IsInCinematic,Is.False);Assert.That(dummy.GetComponent<ShibuyaParryVisual>().IsVisible,Is.True);
            Assert.That(dummy.GetComponent<ShibuyaGuardVisual>().IsVisible,Is.False);
        }
        [UnityTest] public IEnumerator RecoilUsesCollisionAndConsumesBlockedDistance()
        {
            ActualAttack();var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=new Vector3(0,1.5f,-2);wall.transform.localScale=new Vector3(4,3,.2f);Physics.SyncTransforms();
            Assert.That(player.TryAttack(),Is.True);yield return Until(()=>player.AttackElapsed>=.04f);dummy.SetGuardHeld(true);yield return Until(()=>dummy.ParrySuccessCount==1);
            yield return Wait(.15f);float blocked=player.transform.position.z;
            float remaining=(float)typeof(ShibuyaFighter).GetField("distanceKnockbackRemaining",Private).GetValue(player);
            Assert.That(blocked,Is.GreaterThan(-1.58f));Object.Destroy(wall);yield return Wait(.3f);
            float after=blocked-player.transform.position.z;Assert.That(after,Is.LessThanOrEqualTo(remaining+.04f));
            Assert.That(player.Health.CurrentHealth,Is.EqualTo(100));Record("4D1-wall",new{blocked,remaining,after});
        }
        [UnityTest] public IEnumerator InvalidPressAndFocusLossDoNotQueueParry()
        {
            Assert.That(player.TryAttack(),Is.True);frame.guardHeld=true;yield return Wait(.1f);Assert.That(player.ParryAttemptCount,Is.Zero);
            yield return Until(()=>player.ComboStep==0);yield return Wait(.1f);Assert.That(player.ParryAttemptCount,Is.Zero);
            yield return Down();Assert.That(player.IsParryWindowOpen,Is.True);input.SendMessage("OnApplicationFocus",false);
            yield return Wait(.15f);Assert.That(player.IsGuarding||player.IsParryWindowOpen,Is.False);Assert.That(player.ParryAttemptCount,Is.EqualTo(1));
            yield return Wait(.5f);Assert.That(player.ParryAttemptCount,Is.EqualTo(1));yield return Down();Assert.That(player.ParryAttemptCount,Is.EqualTo(2));
        }
        [UnityTest] public IEnumerator OptionalLocalStopLeavesGameTimeRunning()
        {
            combat.guard=Clone(combat.guard);combat.guard.parry=Clone(combat.guard.parry);combat.guard.parry.localHitStop=.08f;
            yield return Down();Assert.That(Hit(),Is.EqualTo(DamageResolution.Guarded));float clock=player.CombatTime;double began=Time.timeAsDouble;
            Assert.That(player.IsInHitStop&&dummy.IsInHitStop,Is.True);yield return Wait(.025f);
            Assert.That(player.CombatTime,Is.EqualTo(clock).Within(.001f));Assert.That(Time.timeAsDouble-began,Is.GreaterThan(.02));Assert.That(Time.timeScale,Is.EqualTo(1));
            yield return Wait(.1f);Assert.That(player.IsInHitStop||dummy.IsInHitStop,Is.False);
        }
        [UnityTest] public IEnumerator ParryPresentationExcludesAuraAndCleansUp()
        {
            yield return Down();Assert.That(Hit(),Is.EqualTo(DamageResolution.Guarded));yield return Wait(.025f);
            var visual=player.GetComponent<ShibuyaParryVisual>();var surface=visual.SurfaceRenderer;
            Assert.That(visual.IsVisible,Is.True);Assert.That(surface.transform.IsChildOf(player.transform),Is.False);
            int mesh=surface.GetComponent<MeshFilter>().sharedMesh.GetInstanceID(),material=surface.sharedMaterial.GetInstanceID();
            yield return Wait(.3f);Assert.That(visual.IsVisible,Is.False);Assert.That(player.AcceptedAttackCount,Is.Zero);
            frame.guardHeld=false;frame.auraHeld=true;yield return Wait(.15f);
            Assert.That(input.EnergyCharge.IsCharging,Is.True);Assert.That(aura.Held,Is.True);Assert.That(aura.Visual.GetComponentsInChildren<Transform>().Any(t=>t.name=="AuraRim_ParryPresentation"),Is.False);
            Object.Destroy(visual);yield return Wait(.12f);Assert.That(surface==null,Is.True);
            Assert.That(Resources.FindObjectsOfTypeAll<Mesh>().Any(m=>m.GetInstanceID()==mesh),Is.False);Assert.That(Resources.FindObjectsOfTypeAll<Material>().Any(m=>m.GetInstanceID()==material),Is.False);
        }
        [UnityTest] public IEnumerator OriginalSpeedSuccessEarlyHoldAndLatePress()
        {
            foreach(string mode in new[]{"parry-success","early-guard","late-hit"}) {
                yield return Up();yield return Until(()=>player.ParryRetryRemaining<=0&&!player.IsParryRecovering);
                Place(player,new Vector3(0,.03f,-8));Place(dummy,new Vector3(0,.03f,-6.6f));dummy.transform.rotation=Quaternion.Euler(0,180,0);player.transform.rotation=Quaternion.identity;
                var m=ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();m.name="PASS4B3 QA parry media";
                var s=new ShibuyaStrikeDefinition{startup=.3f,activeTime=.03f,recovery=.08f,advanceDistance=0,knockbackSpeed=0};m.steps=new[]{new ShibuyaMovesetStep{hits=new[]{new ShibuyaHitEvent{strike=s}}}};
                Assert.That(dummy.SetMoveset(m),Is.True);Pose(new Vector3(2.8f,2,4));
                if(mode=="early-guard"){frame.guardHeld=true;yield return Wait(.2f);}
                var rows=new List<object>();double start=Time.timeAsDouble;bool attacked=false,pressed=mode=="early-guard";int index=0;
                while(Time.timeAsDouble-start<1.6) {
                    double t=Time.timeAsDouble-start;
                    if(!attacked&&t>=.25){Assert.That(dummy.TryAttack(),Is.True);attacked=true;}
                    if(!pressed&&t>=(mode=="parry-success"?.47:.7)){frame.guardHeld=true;pressed=true;}
                    Capture("4D1-media/"+mode+"/frame-"+index.ToString("D4"));
                    rows.Add(new{index,time=t,hp=player.Health.CurrentHealth,guard=player.CurrentGuard,parries=player.ParrySuccessCount,blocks=player.GuardBlockCount,window=player.IsParryWindowOpen,pose=player.IsParryRecovering});
                    index++;yield return Wait(1f/40);
                }
                Record("4D1-media/"+mode+"-timestamps",rows);
            }
        }
        [UnityTest] public IEnumerator PracticeToolUsesRealAttackAndClearsPendingOnFinish()
        {
            var type=typeof(ShibuyaGuardTestWindow);var flags=System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic;
            var original=dummy.Moveset;
            yield return Down();
            type.GetField("practiceLoop",flags).SetValue(null,true);
            type.GetMethod("Schedule",flags).Invoke(null,new object[]{0f,16f,false});
            try {
                yield return Until(()=>dummy.AcceptedAttackCount>0,6);
                yield return Until(()=>player.GuardBlockCount>0);
                Assert.That(player.Health.CurrentHealth,Is.EqualTo(100));Assert.That(player.ParrySuccessCount,Is.Zero);
                Assert.That(player.GetComponent<ShibuyaParryPracticeCue>(),Is.Not.Null);
                player.StopCombat();yield return Wait(.5f);
                Assert.That((bool)type.GetField("practiceLoop",flags).GetValue(null),Is.False);
                Assert.That((bool)type.GetField("pending",flags).GetValue(null),Is.False);
                Assert.That(dummy.Moveset,Is.SameAs(original));
                Record("4D1-practice",new{actualAttacks=dummy.AcceptedAttackCount,blocks=player.GuardBlockCount,hp=player.Health.CurrentHealth,cleared=true});
            }finally{type.GetField("practiceLoop",flags).SetValue(null,false);type.GetField("pending",flags).SetValue(null,false);}
        }
    }
}
