using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass4H0DataTests
    {
        [Test] public void SerializedKoreanProfileUndoAndShaderReferences()
        {
            var original=Resources.Load<ShibuyaBlackFlashProfile>("Shibuya Black Flash");Assert.That(original,Is.Not.Null);
            Assert.That(original.inkShader,Is.Not.Null);Assert.That(original.lightShader,Is.Not.Null);
            Assert.That(ShaderUtil.ShaderHasError(original.inkShader),Is.False);Assert.That(ShaderUtil.ShaderHasError(original.lightShader),Is.False);
            var p=Object.Instantiate(original);
            try{var so=new SerializedObject(p);var it=so.GetIterator();bool child=true;
                while(it.NextVisible(child)){child=false;Assert.That(ShibuyaBlackFlashInspector.Labels.ContainsKey(it.name),Is.True,it.name);}
                Undo.IncrementCurrentGroup();so.FindProperty("branchLength").floatValue=3;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();
                Assert.That(p.branchLength,Is.EqualTo(3));Undo.PerformUndo();Assert.That(p.branchLength,Is.EqualTo(original.branchLength));
            }finally{Undo.ClearUndo(p);Object.DestroyImmediate(p);}
        }
        [Test] public void AllLayersFiniteNondegenerateAndThreeDimensional()
        {
            var p=Resources.Load<ShibuyaBlackFlashProfile>("Shibuya Black Flash");var builder=new ShibuyaBlackFlashGeometry();var mesh=new Mesh();
            try{for(int layer=0;layer<5;layer++)for(int seed=0;seed<3;seed++){
                builder.Build(mesh,layer,p,seed);var v=mesh.vertices;var t=mesh.triangles;Assert.That(v.Length,Is.GreaterThan(50));
                Assert.That(v.All(x=>float.IsFinite(x.x)&&float.IsFinite(x.y)&&float.IsFinite(x.z)),Is.True);
                Assert.That(mesh.bounds.size.x,Is.GreaterThan(.2f));Assert.That(mesh.bounds.size.z,Is.GreaterThan(.2f));
                for(int i=0;i<t.Length;i+=3)Assert.That(Vector3.Cross(v[t[i+1]]-v[t[i]],v[t[i+2]]-v[t[i]]).sqrMagnitude,Is.GreaterThan(1e-16f),"layer "+layer);
            }}finally{Object.DestroyImmediate(mesh);}
        }
    }
    public sealed class ShibuyaPass4H0Tests : ShibuyaPass4CFixture
    {
        ShibuyaBlackFlashPresentation vfx;ShibuyaAttackGradeProfile grades;bool complete;
        readonly List<ShibuyaCombatDamageResult> receipts=new List<ShibuyaCombatDamageResult>();
        [UnitySetUp] public IEnumerator Prepare()
        {
            complete=false;receipts.Clear();vfx=Object.FindFirstObjectByType<ShibuyaBlackFlashPresentation>();Assert.That(vfx,Is.Not.Null);
            grades=Clone(combat.attackGrades);combat.attackGrades=grades;grades.blackFlashCapable=true;
            Place(dummy,player.transform.position+Vector3.forward*1.4f);dummy.transform.rotation=Quaternion.Euler(0,180,0);Pose(new Vector3(3,2,-4));
            ShibuyaCombatDamageResults.Resolved+=Receive;yield return null;
        }
        void Receive(ShibuyaCombatDamageResult r){if(r.Attacker==player)receipts.Add(r);}
        [UnityTearDown] public IEnumerator Unsubscribe(){ShibuyaCombatDamageResults.Resolved-=Receive;yield return null;}
        [TearDown] public void CompleteWitness()=>Assert.That(complete,Is.True,"test must reach final assertions");
        void Done(string name,object data){Record(name,data);complete=true;}
        ShibuyaMovesetDefinition Single(int hits=1)
        {
            var m=ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();m.name="PASS4B3 QA black flash sequence";
            m.steps=new[]{new ShibuyaMovesetStep{hits=Enumerable.Range(0,hits).Select(i=>new ShibuyaHitEvent{startTime=i*.1f,strike=new ShibuyaStrikeDefinition{
                attack=ShibuyaAttackMetadata.Basic(),damage=20,startup=.04f,activeTime=.035f,recovery=.1f,hitStun=.08f,knockbackSpeed=0,advanceDistance=0,hitStopDuration=.035f}}).ToArray()}};return m;
        }
        IEnumerator Hit(){int old=player.ConfirmedHitCount;Assert.That(player.TryAttack(),Is.True);yield return Until(()=>player.ConfirmedHitCount>old);yield return Until(()=>player.ComboStep==0);}
        object Result(ShibuyaCombatDamageResult r)=>new{r.Key.Sequence,r.Key.HitEvent,r.Key.Target,r.Grade,r.HealthLoss,r.Resolution};
        [UnityTest] public IEnumerator ActualGradesNumbersOffAndIndependentFiveHitEvents()
        {
            Assert.That(player.SetMoveset(Single()),Is.True);
            player.SetCombatRandom(new ShibuyaGradeTestRandom(.99f));yield return Hit();Assert.That(vfx.AcceptedCount,Is.Zero);
            player.SetCombatRandom(new ShibuyaGradeTestRandom(0,.99f));yield return Hit();Assert.That(vfx.AcceptedCount,Is.Zero);
            input.DamageNumbers.SetDisplayEnabled(false);player.SetCombatRandom(new ShibuyaGradeTestRandom(0,0));yield return Hit();Assert.That(vfx.AcceptedCount,Is.EqualTo(1));
            Assert.That(receipts[0].HealthLoss,Is.EqualTo(20));Assert.That(receipts[1].HealthLoss,Is.EqualTo(30));Assert.That(receipts[2].HealthLoss,Is.EqualTo(50));
            Assert.That(vfx.Present(receipts[2]),Is.False);Assert.That(vfx.DuplicateCount,Is.EqualTo(1));
            Assert.That(player.SetMoveset(Single(5)),Is.True);player.SetCombatRandom(new ShibuyaGradeTestRandom(0,0,0,0,0,0,0,0,0,0));yield return Hit();
            Assert.That(receipts.Count,Is.EqualTo(8));Assert.That(vfx.AcceptedCount,Is.EqualTo(6));Assert.That(receipts.Count(r=>r.Grade==ShibuyaAttackGrade.BlackFlash),Is.EqualTo(6));
            Assert.That(receipts.Select(r=>r.Key).Distinct().Count(),Is.EqualTo(8));
            Done("4H0-actual-events",new{results=receipts.Select(Result).ToArray(),vfx.AcceptedCount,vfx.DuplicateCount,numberDisplay=input.DamageNumbers.DisplayEnabled,player.BlackFlashCount,player.ZoneStacks});
        }
        [UnityTest] public IEnumerator BlockedMissAndLethalStillOnlyPresentAppliedBlackFlash()
        {
            Assert.That(player.SetMoveset(Single()),Is.True);player.SetCombatRandom(new ShibuyaGradeTestRandom(0,0,0,0,0,0,0,0));
            dummy.SetGuardHeld(true);yield return Wait(.16f);Assert.That(player.TryAttack(),Is.True);yield return Until(()=>player.ComboStep==0);
            Assert.That(vfx.AcceptedCount,Is.Zero);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(1000));dummy.SetGuardHeld(false);
            yield return Wait(.55f);dummy.Health.GrantInvulnerability(.5f);Assert.That(player.TryAttack(),Is.True);yield return Until(()=>player.ComboStep==0);Assert.That(vfx.AcceptedCount,Is.Zero);
            yield return Wait(.55f);Place(dummy,player.transform.position+Vector3.forward*20);Assert.That(player.TryAttack(),Is.True);yield return Until(()=>player.ComboStep==0);Assert.That(vfx.AcceptedCount,Is.Zero);
            Place(dummy,player.transform.position+Vector3.forward*1.4f);
            dummy.Health.ReceiveDamage(new DamageContext(999,player.gameObject,DamageDeliveryType.Environmental,DamageTraits.Unblockable,"QA lethal setup"));
            player.SetCombatRandom(new ShibuyaGradeTestRandom(0,0));Assert.That(player.TryAttack(),Is.True);yield return Until(()=>dummy.Health.CurrentHealth==0);
            Assert.That(vfx.AcceptedCount,Is.EqualTo(1));Assert.That(vfx.LastResult.HealthLoss,Is.EqualTo(1));Assert.That(vfx.ActiveCount,Is.EqualTo(1));
            Assert.That(Object.FindFirstObjectByType<ShibuyaMatch>().Finished,Is.True);yield return Wait(.9f);Assert.That(vfx.ActiveCount,Is.Zero);
            Done("4H0-block-and-lethal",new{results=receipts.Select(Result).ToArray(),vfx.AcceptedCount,finalHp=dummy.Health.CurrentHealth});
        }
        [UnityTest] public IEnumerator PoolPreviewObserverAndOwnedResourcesCleanup()
        {
            var settings=Clone(vfx.Profile);settings.simultaneousLimit=3;vfx.Configure(settings);int observers=ShibuyaCombatDamageResults.ObserverCount;
            float hp=dummy.Health.CurrentHealth,energy=input.EnergyCharge.Resource.CurrentEnergy;int stacks=player.ZoneStacks;
            var watch=System.Diagnostics.Stopwatch.StartNew();long allocations=GC.GetAllocatedBytesForCurrentThread();
            for(int i=0;i<30;i++)vfx.Preview(dummy.transform.position+Vector3.up,Vector3.forward,i);
            watch.Stop();long burstAllocated=GC.GetAllocatedBytesForCurrentThread()-allocations;
            Assert.That(vfx.Capacity,Is.EqualTo(3));Assert.That(vfx.ActiveCount,Is.EqualTo(3));Assert.That(vfx.RecycledLiveCount,Is.EqualTo(27));Assert.That(vfx.AcceptedCount,Is.Zero);
            Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(hp));Assert.That(input.EnergyCharge.Resource.CurrentEnergy,Is.EqualTo(energy));Assert.That(player.ZoneStacks,Is.EqualTo(stacks));
            int meshCount=Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name.StartsWith("BlackFlash pooled"));Assert.That(meshCount,Is.EqualTo(15));
            for(int i=0;i<3;i++){vfx.enabled=false;Assert.That(ShibuyaCombatDamageResults.ObserverCount,Is.EqualTo(observers-1));vfx.enabled=true;Assert.That(ShibuyaCombatDamageResults.ObserverCount,Is.EqualTo(observers));}
            vfx.Preview(dummy.transform.position+Vector3.up,Vector3.forward,44);yield return Wait(1);Assert.That(vfx.ActiveCount,Is.Zero);
            Object.Destroy(vfx);yield return Until(()=>vfx==null);yield return Wait(.1f);
            Assert.That(ShibuyaCombatDamageResults.ObserverCount,Is.EqualTo(observers-1),"observer removed after runtime destruction");
            Assert.That(Resources.FindObjectsOfTypeAll<Material>().Count(m=>m.name.StartsWith("BlackFlash owned")),Is.Zero,"owned materials removed after runtime destruction");
            // Match remains alive, so presentation must explicitly dispose its child pool too.
            Assert.That(Resources.FindObjectsOfTypeAll<Mesh>().Count(m=>m.name.StartsWith("BlackFlash pooled")),Is.Zero,"owned meshes removed after runtime destruction");
            Done("4H0-pool-cleanup",new{meshCount,previewCalls=30,capacity=3,recycled=27,burstMilliseconds=watch.Elapsed.TotalMilliseconds,burstAllocatedBytes=burstAllocated,timeScale=Time.timeScale});
        }
        [UnityTest] public IEnumerator ActualEvadeSpecialDefenseAndInvalidReceiptsNeverPresent()
        {
            var strike=Single().steps[0].hits[0].strike;
            player.SetCombatRandom(new ShibuyaGradeTestRandom(0,0,0,0));
            Assert.That(dummy.TryGroundDash(Vector2.left,Vector3.forward,Vector3.right),Is.True);
            yield return Until(()=>dummy.IsGroundDashEvading);
            var evade=player.PrepareGradedDamage(strike,dummy,1000,0);
            var context=new DamageContext(evade.Damage,player.gameObject,DamageDeliveryType.PhysicalStrike);
            var first=dummy.ReceiveStrike(context,strike,out float loss);player.CommitGradedDamage(evade,first,loss,dummy.transform.position+Vector3.up);
            Assert.That(first,Is.EqualTo(DamageResolution.Guarded));Assert.That(loss,Is.Zero);
            yield return Until(()=>!dummy.IsGroundDashing);
            var guard=dummy.gameObject.AddComponent<ShibuyaGradeSpecialGuard>();
            var special=player.PrepareGradedDamage(strike,dummy,1000,1);
            var second=dummy.ReceiveStrike(new DamageContext(special.Damage,player.gameObject,DamageDeliveryType.PhysicalStrike),strike,out loss);
            player.CommitGradedDamage(special,second,loss,dummy.transform.position+Vector3.up);
            Assert.That(second,Is.EqualTo(DamageResolution.Guarded));Object.Destroy(guard);
            foreach(float value in new[]{0f,-1,float.NaN,float.PositiveInfinity}){
                var key=new ShibuyaDamageResultKey(player.GetInstanceID(),dummy.GetInstanceID(),1001,0,ShibuyaDamageAction.Strike);
                Assert.That(vfx.Present(new ShibuyaCombatDamageResult(player,dummy,key,DamageResolution.Applied,value,ShibuyaAttackGrade.BlackFlash,dummy.transform.position)),Is.False);
            }
            Assert.That(vfx.AcceptedCount,Is.Zero);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(1000));Assert.That(player.BlackFlashCount+player.ZoneStacks,Is.Zero);
            Done("4H0-evade-and-special",new{evade=first,special=second,vfx.AcceptedCount,actualHp=dummy.Health.CurrentHealth,note="special defense uses the existing IDamageGuard test double; not a Gojo Infinity scene run"});
        }
        [UnityTest] public IEnumerator SameTimeMultiAngleBrightDarkAndResidueEvidence()
        {
            Vector3 point=dummy.transform.position+Vector3.up;
            var v=vfx.Preview(point,Vector3.forward,71);
            var poses=new[]{new Vector3(1.8f,1.6f,-2.8f),new Vector3(0,2,-6),new Vector3(4,2,-3),new Vector3(4,2,1),new Vector3(1,5,-1)};
            string[] names={"close","combat","diagonal","side","high"};
            for(int i=0;i<poses.Length;i++){camera.transform.position=point+poses[i];camera.transform.LookAt(point);foreach(float age in new[]{.02f,.09f,.21f,.43f}){v.Sample(age);Capture("4H0-views/"+names[i]+"-"+age.ToString("F2",System.Globalization.CultureInfo.InvariantCulture));}}
            // A camera-only dark background contrast probe, no saved lighting/scene edits.
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.008f,.008f,.012f);camera.cullingMask=1<<30;
            foreach(Transform child in v.GetComponentsInChildren<Transform>(true))child.gameObject.layer=30;
            v.Sample(.09f);Capture("4H0-views/dark-isolated-peak");
            Assert.That(v.VertexCount,Is.LessThan(20000));Done("4H0-visual-geometry",new{v.VertexCount,views=5,ages=new[]{.02,.09,.21,.43},note="deterministic visual samples, not continuous playback"});yield return null;
        }
        [UnityTest] public IEnumerator OriginalSpeedActualNormalCriticalBlackFlashAndMovingCamera()
        {
            Assert.That(player.SetMoveset(Single()),Is.True);var samples=new List<object>();
            for(int grade=0;grade<3;grade++){
                Place(dummy,player.transform.position+Vector3.forward*1.4f);
                player.SetCombatRandom(grade==0?new ShibuyaGradeTestRandom(.99f):grade==1?new ShibuyaGradeTestRandom(0,.99f):new ShibuyaGradeTestRandom(0,0));
                Assert.That(player.TryAttack(),Is.True);double start=Time.unscaledTimeAsDouble;int frameIndex=0;
                while(Time.unscaledTimeAsDouble-start<1.1){
                    float t=(float)(Time.unscaledTimeAsDouble-start);Pose(new Vector3(3+Mathf.Sin(t*2)*.7f,2,-4));
                    string path="4H0-video/grade"+grade+"-"+frameIndex.ToString("D4");Capture(path);
                    samples.Add(new{grade,index=frameIndex,time=Time.unscaledTimeAsDouble-start,dt=Time.unscaledDeltaTime,active=vfx.ActiveCount,accepted=vfx.AcceptedCount,hp=dummy.Health.CurrentHealth});frameIndex++;yield return null;
                }
            }
            Assert.That(vfx.AcceptedCount,Is.EqualTo(1));Assert.That(receipts.Select(x=>x.Grade),Is.EqualTo(new[]{ShibuyaAttackGrade.Normal,ShibuyaAttackGrade.Critical,ShibuyaAttackGrade.BlackFlash}));
            Done("4H0-video-timestamps",new{samples,receipts=receipts.Select(Result).ToArray(),vfx.AcceptedCount,Time.timeScale});
        }
    }
}
