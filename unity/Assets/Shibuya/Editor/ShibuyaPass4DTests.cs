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
    public sealed class ShibuyaPass4DDataTests
    {
        [Test] public void GuardDataKoreanInspectorShaderAndUndo()
        {
            var combat=AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root+"/Data/Shibuya Combat Profile.asset");
            var p=Object.Instantiate(combat.guard);
            try {
                Assert.That(p.maximumGuard,Is.EqualTo(100));Assert.That(p.halfAngle,Is.EqualTo(60));Assert.That(p.movementMultiplier,Is.EqualTo(.4f));
                Assert.That(p.recoveryDelay,Is.EqualTo(1.2f));Assert.That(p.recoveryPerSecond,Is.EqualTo(20));Assert.That(p.breakLockDuration,Is.EqualTo(.8f));
                Assert.That(p.shieldShader,Is.Not.Null);Assert.That(ShaderUtil.ShaderHasError(p.shieldShader),Is.False);
                Assert.That(Array.ConvertAll(combat.strikes,s=>s.guardDamage),Is.EqualTo(new[]{16f,16f,18f,35f}));
                Assert.That(Array.ConvertAll(combat.strikes,s=>s.damage),Is.EqualTo(new[]{20f,20f,20f,25f}));
                var so=new SerializedObject(p);var it=so.GetIterator();bool child=true;
                while(it.NextVisible(child)){child=false;if(it.name!="m_Script")Assert.That(ShibuyaGuardInspector.Labels.ContainsKey(it.name),Is.True,it.name);}
                Undo.IncrementCurrentGroup();so.FindProperty("maximumGuard").floatValue=150;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();Undo.PerformUndo();
                Assert.That(p.maximumGuard,Is.EqualTo(100));Assert.That(combat.guard.maximumGuard,Is.EqualTo(100));
            } finally{Undo.ClearUndo(p);Object.DestroyImmediate(p);}
        }
    }
    public sealed class ShibuyaPass4DTests : ShibuyaPass4CFixture
    {
        private IEnumerator Guard()
        {
            frame.guardHeld=false;
            yield return Until(() => !(bool)typeof(ShibuyaFighter).GetField("guardInputHeld",Private).GetValue(player));
            Record("4D-rearm-observation",new {latched=(bool)typeof(ShibuyaFighter).GetField("guardInputHeld",Private).GetValue(player),
                state=player.State.ToString(),ground=player.IsGrounded,guard=player.CurrentGuard,clock=player.CombatTime,
                stunnedUntil=(float)typeof(ShibuyaFighter).GetField("stunnedUntil",Private).GetValue(player),
                broken=player.IsGuardBroken,charging=input.EnergyCharge.IsCharging,auraHeld=aura.Held});
            frame.guardHeld=true;yield return Until(()=>player.IsGuarding);
            yield return Wait((player.ParrySettings!=null?player.ParrySettings.windowDuration:0)+.03f);
        }
        private DamageResolution Hit(float angle=0,float cost=16,float damage=20,DamageDeliveryType type=DamageDeliveryType.PhysicalStrike,DamageTraits traits=DamageTraits.None,bool blockable=true)
        {
            Vector3 direction=Quaternion.AngleAxis(angle,Vector3.up)*player.transform.forward;
            Place(dummy,player.transform.position+direction*2);
            return player.ReceiveStrike(new DamageContext(damage,dummy.gameObject,type,traits,"PASS4D QA"),
                new ShibuyaStrikeDefinition{guardDamage=cost,guardable=blockable});
        }
        [UnityTest] public IEnumerator HeldInputMovementRestrictionsAndFocusRelease()
        {
            yield return Guard();Assert.That(player.TryAttack(),Is.False);Assert.That(player.TryGroundDash(Vector2.up,Vector3.forward,Vector3.right),Is.False);
            Vector3 start=player.transform.position;frame.move=Vector2.right;frame.run=true;frame.jump=true;frame.auraHeld=true;
            double began=Time.timeAsDouble;yield return Wait(.3f);double dt=Time.timeAsDouble-began;
            float distance=Vector3.ProjectOnPlane(player.transform.position-start,Vector3.up).magnitude;
            Assert.That(distance,Is.EqualTo(combat.walkSpeed*.4f*dt).Within(.10f));Assert.That(player.IsGrounded,Is.True);
            Assert.That(input.EnergyCharge.IsCharging,Is.False);Assert.That(aura.Held,Is.False);
            frame.move=Vector2.zero;frame.jump=frame.auraHeld=false;
            input.SendMessage("OnApplicationFocus",false);yield return Wait(.15f);Assert.That(player.IsGuarding,Is.False);
            yield return Guard();Assert.That(player.IsGuarding,Is.True);frame.guardHeld=false;yield return Until(()=>!player.IsGuarding);
            Record("4D-input",new{distance,dt,speed=distance/dt,focusRelease=true});
        }
        [UnityTest] public IEnumerator DirectionConeHpAndUnblockablePolicy()
        {
            var samples=new List<object>();
            foreach(float angle in new[]{0f,59f,61f,180f}) {
                yield return Guard();float hp=player.Health.CurrentHealth,g=player.CurrentGuard;
                var result=Hit(angle);bool block=angle<60;
                Assert.That(result,Is.EqualTo(block?DamageResolution.Guarded:DamageResolution.Applied));
                Assert.That(player.Health.CurrentHealth,Is.EqualTo(block?hp:hp-20));Assert.That(player.CurrentGuard,Is.EqualTo(block?g-16:g));
                samples.Add(new{angle,result=result.ToString(),hp=player.Health.CurrentHealth,guard=player.CurrentGuard});
            }
            yield return Guard();Assert.That(Hit(traits:DamageTraits.Unblockable),Is.EqualTo(DamageResolution.Applied));
            yield return Guard();Assert.That(Hit(type:DamageDeliveryType.Environmental),Is.EqualTo(DamageResolution.Applied));
            Record("4D-direction",samples);
        }
        [UnityTest] public IEnumerator RecoveryNormalizesVisualTierWithoutHud()
        {
            yield return Guard();Hit(cost:32);Assert.That(player.CurrentGuard,Is.EqualTo(68));Assert.That(player.GuardVisualState,Is.EqualTo(ShibuyaGuardVisualState.Strained));
            Hit(cost:32);Assert.That(player.CurrentGuard,Is.EqualTo(36));Assert.That(player.GuardVisualState,Is.EqualTo(ShibuyaGuardVisualState.Critical));
            yield return Wait(.2f);Assert.That(player.GetComponent<ShibuyaGuardVisual>().IsVisible,Is.True);
            frame.guardHeld=false;yield return null;yield return Wait(1.05f);Assert.That(player.CurrentGuard,Is.EqualTo(36));
            yield return Wait(.4f);Assert.That(player.CurrentGuard,Is.GreaterThan(36));
            yield return Wait(3.3f);Assert.That(player.CurrentGuard,Is.EqualTo(100));
            yield return Guard();Assert.That(player.GuardVisualState,Is.EqualTo(ShibuyaGuardVisualState.Stable));
            Record("4D-recovery",new{guard=player.CurrentGuard,state=player.GuardVisualState.ToString(),hp=player.Health.CurrentHealth});
        }
        [UnityTest] public IEnumerator BreakAppliesFullDamageOnceAndHeldInputCannotRearm()
        {
            yield return Guard();double start=Time.timeAsDouble;int applied=0;
            player.Health.DamageResolved+=(_,__,result)=>{if(result==DamageResolution.Applied)applied++;};
            Assert.That(Hit(cost:100,damage:25),Is.EqualTo(DamageResolution.Applied));
            Assert.That(player.Health.CurrentHealth,Is.EqualTo(75));Assert.That(applied,Is.EqualTo(1));Assert.That(player.CurrentGuard,Is.Zero);
            Assert.That(player.IsGuardBroken,Is.True);Assert.That(player.IsGuarding,Is.False);Assert.That(player.GuardBreakCount,Is.EqualTo(1));
            frame.guardHeld=false;yield return null;frame.guardHeld=true;yield return Wait(.4f);Assert.That(player.IsGuarding,Is.False);
            yield return Wait(.5f);Assert.That(player.IsGuardBroken,Is.False);Assert.That(player.IsGuarding,Is.False);
            yield return Wait(.65f);Assert.That(player.CurrentGuard,Is.GreaterThan(0));Assert.That(player.IsGuarding,Is.False);
            yield return Guard();Assert.That(player.IsGuarding,Is.True);
            Record("4D-break",new{hp=player.Health.CurrentHealth,applied,guard=player.CurrentGuard,elapsed=Time.timeAsDouble-start});
        }
        [UnityTest] public IEnumerator ActualMultiColliderHitBlocksFeedbackAndBreakHitsOnlyOnce()
        {
            Place(player,new Vector3(0,.03f,-1.4f));Place(dummy,new Vector3(0,.03f,0));dummy.transform.rotation=Quaternion.Euler(0,180,0);yield return Wait(.08f);
            var extra=new GameObject("PASS4D duplicate hurt collider");extra.transform.SetParent(dummy.transform,false);extra.AddComponent<BoxCollider>().center=Vector3.up;
            dummy.SetGuardHeld(true);yield return Wait((dummy.ParrySettings!=null?dummy.ParrySettings.windowDuration:0)+.03f);
            var moves=ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();moves.name="PASS4B3 QA guard actual-hit";
            var strike=combat.strikes[0];strike.hitStopDuration=.065f;
            var cine=Clone(AssetDatabase.LoadAssetAtPath<ShibuyaCinematicProfile>(ShibuyaSceneBuilder.Root+"/Data/Shibuya Cinematic Example.asset"));cine.cinematicEnabled=true;
            moves.steps=new[]{new ShibuyaMovesetStep{hits=new[]{new ShibuyaHitEvent{strike=strike,cinematic=cine,outcome=ShibuyaHitOutcome.Knockdown}}}};
            Assert.That(player.SetMoveset(moves),Is.True);Assert.That(player.TryAttack(),Is.True);yield return Until(()=>player.ComboStep==0);
            Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(1000));Assert.That(dummy.CurrentGuard,Is.EqualTo(84));Assert.That(dummy.GuardBlockCount,Is.EqualTo(1));
            Assert.That(player.ConfirmedHitCount,Is.Zero);Assert.That(player.HitStopCount,Is.Zero);Assert.That(dummy.IsDowned||dummy.IsInCinematic,Is.False);
            strike.guardDamage=120;moves.steps[0].hits[0].outcome=ShibuyaHitOutcome.Normal;
            Assert.That(player.TryAttack(),Is.True);yield return Until(()=>player.ConfirmedHitCount==1);
            Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(980));Assert.That(dummy.GuardBreakCount,Is.EqualTo(1));Assert.That(player.HitStopCount,Is.EqualTo(1));
            Record("4D-actual-hit",new{hp=dummy.Health.CurrentHealth,blocks=dummy.GuardBlockCount,breaks=dummy.GuardBreakCount,hits=player.ConfirmedHitCount});
        }
        [UnityTest] public IEnumerator GuardOnlyMetadataDoesNotChangeActualDashEvade()
        {
            Place(player,new Vector3(0,.03f,-1.4f));Place(dummy,new Vector3(0,.03f,0));dummy.transform.rotation=Quaternion.Euler(0,180,0);yield return Wait(.1f);
            var defender=Clone(dummy.Profile);defender.groundDash=Clone(dash);defender.groundDash.sideDistance=.01f;
            dummy.Configure(defender,AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(ShibuyaSceneBuilder.Root+"/Data/Dummy Stats.asset"));
            var moves=ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();moves.name="PASS4B3 QA guard-only metadata";
            var strike=combat.strikes[0];strike.guardable=false;strike.hitStopDuration=0;
            moves.steps=new[]{new ShibuyaMovesetStep{hits=new[]{new ShibuyaHitEvent{strike=strike}}}};Assert.That(player.SetMoveset(moves),Is.True);
            dummy.SetGuardHeld(true);yield return Wait((dummy.ParrySettings!=null?dummy.ParrySettings.windowDuration:0)+.03f);Assert.That(player.TryAttack(),Is.True);yield return Until(()=>player.ComboStep==0);
            Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(980));Assert.That(dummy.CurrentGuard,Is.EqualTo(100));
            yield return Wait(.4f);dummy.SetGuardHeld(false);
            Place(player,new Vector3(0,.03f,-1.4f));Place(dummy,new Vector3(0,.03f,0));yield return Wait(.08f);
            Assert.That(player.TryAttack(),Is.True);yield return Until(()=>player.AttackElapsed>=.035f);
            Assert.That(dummy.TryGroundDash(Vector2.right,Vector3.forward,Vector3.right),Is.True);yield return Until(()=>player.ComboStep==0);
            Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(980));Assert.That(player.ConfirmedHitCount,Is.EqualTo(1));
            Record("4D-guard-only-metadata",new{hp=dummy.Health.CurrentHealth,hits=player.ConfirmedHitCount,guard=dummy.CurrentGuard});
        }
        [UnityTest] public IEnumerator InvalidStatesAndFailedHoldDoNotAutoGuard()
        {
            Assert.That(player.TryAttack(),Is.True);frame.guardHeld=true;yield return null;Assert.That(player.IsGuarding,Is.False);
            yield return Until(()=>player.ComboStep==0);yield return Wait(.1f);Assert.That(player.IsGuarding,Is.False);frame.guardHeld=false;yield return null;
            Assert.That(player.TryGroundDash(Vector2.right,Vector3.forward,Vector3.right),Is.True);frame.guardHeld=true;yield return Wait(.08f);
            Assert.That(player.IsGuarding,Is.False);float g=player.CurrentGuard;
            Assert.That(Hit(),Is.EqualTo(DamageResolution.Guarded));Assert.That(player.CurrentGuard,Is.EqualTo(g));
            yield return Until(()=>!player.IsGroundDashing);Assert.That(player.IsGuarding,Is.False);frame.guardHeld=false;yield return null;
            foreach(string field in new[]{"downed","gettingUp","airborne","airSession","chasing"}) {
                Set(player,field,true);player.SetGuardHeld(true);Assert.That(player.IsGuarding,Is.False,field);player.SetGuardHeld(false);Set(player,field,false);
            }
            Set(player,"hitStopUntil",Time.unscaledTimeAsDouble+.1);player.SetGuardHeld(true);Assert.That(player.IsGuarding,Is.False);player.SetGuardHeld(false);Set(player,"hitStopUntil",0d);
            player.ApplyHitReaction(Vector3.zero,.2f);player.SetGuardHeld(true);Assert.That(player.IsGuarding,Is.False);player.SetGuardHeld(false);yield return Wait(.25f);
            frame.jump=true;yield return Until(()=>!player.IsGrounded);frame.jump=false;frame.guardHeld=true;yield return null;Assert.That(player.IsGuarding,Is.False);
            Record("4D-invalid-states",new{guard=player.CurrentGuard,blocks=player.GuardBlockCount});
        }
        [UnityTest] public IEnumerator FinishDisableAndDeathClearGuard()
        {
            yield return Guard();player.enabled=false;Assert.That(player.IsGuarding,Is.False);player.enabled=true;
            frame.guardHeld=false;yield return null;yield return Guard();player.Health.ReceiveDamage(new DamageContext(500,dummy.gameObject,DamageDeliveryType.Environmental));
            Assert.That(player.IsGuarding,Is.False);Assert.That(player.Health.IsDead,Is.True);Assert.That(player.IsGuardBroken,Is.False);
            Assert.That(dummy.State,Is.EqualTo(ShibuyaFighterState.Finished));
        }
        [UnityTest] public IEnumerator RepeatedGuardReusesResourcesAndFResumes()
        {
            yield return Guard();var visual=player.GetComponent<ShibuyaGuardVisual>();var surface=visual.SurfaceRenderer.transform;
            int mesh=surface.GetComponent<MeshFilter>().sharedMesh.GetInstanceID(),material=surface.GetComponent<Renderer>().sharedMaterial.GetInstanceID();
            for(int i=0;i<8;i++){frame.guardHeld=false;yield return Wait(.04f);yield return Guard();}
            Assert.That(surface.GetComponent<MeshFilter>().sharedMesh.GetInstanceID(),Is.EqualTo(mesh));Assert.That(surface.GetComponent<Renderer>().sharedMaterial.GetInstanceID(),Is.EqualTo(material));
            frame.guardHeld=false;frame.auraHeld=true;yield return Wait(.1f);Assert.That(input.EnergyCharge.IsCharging,Is.True);Assert.That(aura.Held,Is.True);Assert.That(visual.IsVisible,Is.False);
            Record("4D-resources",new{mesh,material,charging=input.EnergyCharge.IsCharging});
        }
        [UnityTest] public IEnumerator GuardRenderSurfaceIsExcludedFromAuraAndDisposesResources()
        {
            frame.auraHeld=true;yield return Wait(.2f);Assert.That(aura.Visual,Is.Not.Null);
            var visual=player.GetComponent<ShibuyaGuardVisual>();var surface=visual.SurfaceRenderer;
            Assert.That(surface.transform.IsChildOf(player.transform),Is.False);
            Assert.That(player.GetComponentsInChildren<Renderer>().Contains(surface),Is.False);
            Assert.That(aura.Visual.GetComponentsInChildren<Transform>().Any(t=>t.name=="AuraRim_GuardPresentation"),Is.False);
            int mesh=surface.GetComponent<MeshFilter>().sharedMesh.GetInstanceID(),material=surface.sharedMaterial.GetInstanceID();
            Object.Destroy(visual);yield return Wait(.12f);
            Assert.That(surface==null,Is.True);
            Assert.That(Resources.FindObjectsOfTypeAll<Mesh>().Any(m=>m.GetInstanceID()==mesh),Is.False);
            Assert.That(Resources.FindObjectsOfTypeAll<Material>().Any(m=>m.GetInstanceID()==material),Is.False);
            Record("4D-aura-isolation-cleanup",new{excluded=true,meshDisposed=true,materialDisposed=true});
        }
        [UnityTest] public IEnumerator GuardCapacityClampAndDisabledProfileDoNotRearmHeldInput()
        {
            var p=Clone(combat.guard);combat.guard=p;Assert.That(player.CurrentGuard,Is.EqualTo(100));
            p.maximumGuard=50;Assert.That(player.CurrentGuard,Is.EqualTo(50));p.maximumGuard=100;
            yield return Guard();p.guardEnabled=false;yield return Wait(.05f);Assert.That(player.IsGuarding,Is.False);
            p.guardEnabled=true;yield return Wait(.05f);Assert.That(player.IsGuarding,Is.False);
            yield return Guard();Assert.That(player.CurrentGuard,Is.InRange(0,100));
            combat.guard=null;yield return Wait(.05f);Assert.That(player.IsGuarding,Is.False);
            Assert.That(player.CurrentGuard,Is.Zero);Assert.That(player.GetComponent<ShibuyaGuardVisual>().IsVisible,Is.False);
            combat.guard=p;yield return Wait(.05f);Assert.That(player.IsGuarding,Is.False);
            yield return Guard();
            Record("4D-capacity-clamp",new{guard=player.CurrentGuard,max=player.MaximumGuard});
        }
        [UnityTest] public IEnumerator OriginalSpeedGuardStableStrainedCriticalBreakAndRecovery()
        {
            yield return Guard();Pose(new Vector3(2.8f,2,4));var samples=new List<object>();double start=Time.timeAsDouble;int phase=0,index=0;
            while(Time.timeAsDouble-start<2.4) {
                double t=Time.timeAsDouble-start;
                if(phase==0&&t>.5){Hit(cost:32);phase++;}
                else if(phase==1&&t>1){Hit(cost:32);phase++;}
                else if(phase==2&&t>1.5){Hit(cost:40);phase++;}
                Capture("4D-media/frame-"+index.ToString("D4"));samples.Add(new{index,time=t,guard=player.CurrentGuard,state=player.GuardVisualState.ToString(),active=player.IsGuarding});index++;yield return Wait(1f/30);
            }
            Record("4D-media-timestamps",samples);frame.guardHeld=false;yield return Wait(5.5f);yield return Guard();yield return Wait(.1f);Capture("4D-recovered");
            Assert.That(player.GuardVisualState,Is.EqualTo(ShibuyaGuardVisualState.Stable));
        }
    }
}
