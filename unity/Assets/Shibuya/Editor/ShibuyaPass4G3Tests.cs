using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass4G3DataTests
    {
        [TestCase(20f,"20")][TestCase(1.25f,"1.25")][TestCase(0f,"")][TestCase(-1f,"")]
        [TestCase(float.NaN,"")][TestCase(float.PositiveInfinity,"")]
        public void DamageTextSanitizesAndPreservesFractions(float value,string text)=>Assert.That(ShibuyaDamageNumbers.FormatDamage(value),Is.EqualTo(text));
        [Test] public void TinyFractionAndKoreanInspectorSerializedUndo()
        {
            float value=.0000001f;Assert.That(float.Parse(ShibuyaDamageNumbers.FormatDamage(value),CultureInfo.InvariantCulture),Is.EqualTo(value));
            var original=Resources.Load<ShibuyaDamageNumberProfile>("Shibuya Damage Numbers");var p=UnityEngine.Object.Instantiate(original);
            try{Assert.That(p.displayEnabled,Is.True);foreach(var f in typeof(ShibuyaDamageNumberProfile).GetFields())if(f.DeclaringType==typeof(ShibuyaDamageNumberProfile))Assert.That(ShibuyaDamageNumberInspector.Label(f.Name),Is.Not.EqualTo(f.Name));
                var so=new SerializedObject(p);Undo.IncrementCurrentGroup();so.FindProperty("normalSize").intValue=48;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();Undo.PerformUndo();Assert.That(p.normalSize,Is.EqualTo(32));
            }finally{Undo.ClearUndo(p);UnityEngine.Object.DestroyImmediate(p);}
        }
        [TestCase(0)][TestCase(1)][TestCase(2)]
        public void HealthMeasurementPrecedesHealingNestedDamageAndDeathCallbacks(int mode)
        {
            var go=new GameObject("4G3 transaction unit");var health=go.AddComponent<Health>();bool invoked=false;float nested=0;
            try{health.SetCurrentHealth(100);health.HealthChanged+=(_,__)=>{if(invoked)return;invoked=true;
                    if(mode==0)health.Restore(10);else if(mode==1)health.ReceiveDamage(new DamageContext(5,null,DamageDeliveryType.PhysicalStrike),out nested);else health.Kill();};
                var result=health.ReceiveDamage(new DamageContext(20,null,DamageDeliveryType.PhysicalStrike),out float actual);
                Assert.That(result,Is.EqualTo(DamageResolution.Applied));Assert.That(actual,Is.EqualTo(20));
                Assert.That(health.CurrentHealth,Is.EqualTo(mode==0?90:mode==1?75:0));if(mode==1)Assert.That(nested,Is.EqualTo(5));
            }finally{UnityEngine.Object.DestroyImmediate(go);}
        }
    }
    public sealed class ShibuyaPass4G3Tests : ShibuyaPass4CFixture
    {
        ShibuyaDamageNumbers numbers;ShibuyaDamageNumberProfile display;ShibuyaAttackGradeProfile grades;
        List<ShibuyaCombatDamageResult> shown=new List<ShibuyaCombatDamageResult>();bool complete;
        [UnitySetUp] public IEnumerator DisplaySetup(){
            numbers=input.DamageNumbers;Assert.That(numbers,Is.Not.Null);display=Clone(numbers.Profile);display.displayDuration=1.2f;
            numbers.Configure(player,camera,display);numbers.Presented+=r=>shown.Add(r);shown.Clear();
            grades=Clone(combat.attackGrades);combat.attackGrades=grades;Pose(new Vector3(3.4f,2.1f,-4.5f));
            Place(dummy,player.transform.position+Vector3.forward*1.4f);dummy.transform.rotation=Quaternion.Euler(0,180,0);
            UseSingle(player);complete=false;yield return Wait(.04f);
        }
        [TearDown] public void BodyCompleted()=>Assert.That(complete,Is.True,"all damage display test assertions must execute");
        void Done(string name,object data){Record(name,data);complete=true;}
        ShibuyaMovesetDefinition Single(){var m=ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();m.name="PASS4B3 QA 4G3 strike";
            m.steps=new[]{new ShibuyaMovesetStep{hits=new[]{new ShibuyaHitEvent{strike=new ShibuyaStrikeDefinition{
                attack=ShibuyaAttackMetadata.Basic(),damage=20,startup=.06f,activeTime=.04f,recovery=.12f,hitStun=.08f,knockbackSpeed=0,advanceDistance=0,hitStopDuration=.03f}}}}};return m;}
        void UseSingle(ShibuyaFighter actor){Assert.That(actor.SetMoveset(Single()),Is.True);}
        IEnumerator Hit(ShibuyaFighter actor=null){actor=actor??player;int before=actor.ConfirmedHitCount;Assert.That(actor.TryAttack(),Is.True);yield return Until(()=>actor.ConfirmedHitCount>before);yield return Until(()=>actor.ComboStep==0);}
        IEnumerator Screen(string name){string path=Path.Combine(Root,name+".png");Directory.CreateDirectory(Path.GetDirectoryName(path));ScreenCapture.CaptureScreenshot(path);yield return Until(()=>File.Exists(path));yield return null;}
        object Receipt(ShibuyaCombatDamageResult r)=>new{r.HealthLoss,r.Grade,r.EnergyRestored,r.Zone,r.Key.Sequence,r.Key.HitEvent,r.Key.Target};
        [UnityTest] public IEnumerator ActualNormalCriticalBlackFlashAndZoneAreShownWithoutRecalculation()
        {
            foreach(int mode in new[]{0,1,2,2}){
                grades.blackFlashCapable=mode==2;player.SetCombatRandom(mode==0?new ShibuyaGradeTestRandom(.99f):mode==1?new ShibuyaGradeTestRandom(0):new ShibuyaGradeTestRandom(0,0));
                input.EnergyCharge.Resource.SetCurrentEnergy(100);yield return Hit();yield return Screen("4G3-grade-"+shown.Count);
                int slot=Enumerable.Range(0,numbers.Capacity).First(i=>numbers.GetPopupText(i)==numbers.LastText);
                Assert.That(numbers.TryGetLayout(slot,out var layout,out _),Is.True);
                Assert.That(layout.Color,Is.EqualTo(mode==0?display.normalColor:mode==1?display.criticalColor:display.blackFlashColor));
            }
            Assert.That(shown.Select(x=>x.HealthLoss),Is.EqualTo(new[]{20f,30,50,55}));Assert.That(shown.Select(x=>x.Grade),Is.EqualTo(new[]{ShibuyaAttackGrade.Normal,ShibuyaAttackGrade.Critical,ShibuyaAttackGrade.BlackFlash,ShibuyaAttackGrade.BlackFlash}));
            Assert.That(shown[2].Zone,Is.EqualTo(ShibuyaZoneFeedback.Entered));Assert.That(shown[3].Zone,Is.EqualTo(ShibuyaZoneFeedback.Refreshed));Assert.That(numbers.LastText,Does.Contain("흑섬!\n55"));
            Done("4G3-grades",shown.Select(Receipt).ToArray());
        }
        [UnityTest] public IEnumerator ActualFiveHitsSeparateAndDuplicateColliderAndReplayCannotAddText()
        {
            grades.blackFlashCapable=true;player.SetCombatRandom(new ShibuyaGradeTestRandom(.99f,0,0,0,.99f,0,0,.99f));
            var m=Single();var template=m.steps[0].hits[0].strike;
            m.steps[0].hits=Enumerable.Range(0,5).Select(i=>new ShibuyaHitEvent{startTime=.17f*i,strike=JsonUtility.FromJson<ShibuyaStrikeDefinition>(JsonUtility.ToJson(template))}).ToArray();player.SetMoveset(m);
            var extra=new GameObject("4G3 extra collider");extra.transform.SetParent(dummy.transform,false);extra.AddComponent<BoxCollider>().center=Vector3.up;
            yield return Hit();Assert.That(shown.Count,Is.EqualTo(5));Assert.That(shown.Select(x=>x.HealthLoss),Is.EqualTo(new[]{20f,50,33,55,22}));
            Assert.That(player.BlackFlashCount,Is.EqualTo(2));Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(820));int count=numbers.PresentedCount;numbers.Present(shown[4]);
            Assert.That(numbers.PresentedCount,Is.EqualTo(count));Assert.That(numbers.LastReason,Is.EqualTo("중복 타격 결과"));Assert.That(numbers.ActiveCount,Is.EqualTo(5));
            var layouts=Enumerable.Range(0,numbers.Capacity).Where(i=>numbers.TryGetLayout(i,out _,out _)).Select(i=>{numbers.TryGetLayout(i,out var r,out _);return r.Rect.x;}).ToArray();Assert.That(layouts.Distinct().Count(),Is.EqualTo(5));
            yield return Screen("4G3-five-hits");Done("4G3-five-hits",shown.Select(Receipt).ToArray());
        }
        [UnityTest] public IEnumerator OffClearsImmediatelyButIdenticalDrawsKeepDamageZoneEnergyAndHitStop()
        {
            grades.blackFlashCapable=true;var rngA=new ShibuyaGradeTestRandom(0,0);player.SetCombatRandom(rngA);input.EnergyCharge.Resource.SetCurrentEnergy(100);yield return Hit();
            float damageA=1000-dummy.Health.CurrentHealth;int before=numbers.PresentedCount;Assert.That(numbers.ActiveCount,Is.GreaterThan(0));numbers.SetDisplayEnabled(false);Assert.That(numbers.ActiveCount,Is.Zero);
            var c=UnityEngine.Object.Instantiate(dummy.gameObject,new Vector3(8,.03f,-8),Quaternion.identity).GetComponent<ShibuyaFighter>();
            var d=UnityEngine.Object.Instantiate(dummy.gameObject,new Vector3(8,.03f,-6.6f),Quaternion.Euler(0,180,0)).GetComponent<ShibuyaFighter>();
            c.Configure(combat,c.StatsProfile);UseSingle(c);var rngB=new ShibuyaGradeTestRandom(0,0);c.SetCombatRandom(rngB);var reserve=CursedEnergyController.GetOrCreate(c.gameObject);reserve.ApplyProfile(combat.cursedEnergy);reserve.SetCurrentEnergy(100);
            yield return Wait(.1f);yield return Hit(c);Assert.That(1000-d.Health.CurrentHealth,Is.EqualTo(damageA));Assert.That(c.BlackFlashCount,Is.EqualTo(1));Assert.That(c.ZoneStacks,Is.EqualTo(1));Assert.That(reserve.CurrentEnergy,Is.EqualTo(110));
            Assert.That(c.HitStopCount,Is.EqualTo(player.HitStopCount));Assert.That(rngA.Draws,Is.EqualTo(rngB.Draws));Assert.That(numbers.PresentedCount,Is.EqualTo(before));Assert.That(numbers.ActiveCount,Is.Zero);
            yield return Screen("4G3-display-off");
            for(int i=0;i<12;i++){numbers.SetDisplayEnabled(true);numbers.SetDisplayEnabled(false);}numbers.SetDisplayEnabled(true);Assert.That(numbers.ActiveCount,Is.Zero);
            player.SetCombatRandom(new ShibuyaGradeTestRandom(0,0));yield return Hit();Assert.That(numbers.PresentedCount,Is.EqualTo(before+1));Assert.That(numbers.LastDisplayedDamage,Is.EqualTo(55));
            yield return Screen("4G3-display-on-again");
            Done("4G3-off-equivalence",new{damageA,drawsOn=rngA.Draws,drawsOff=rngB.Draws,energyOff=reserve.CurrentEnergy,c.ZoneStacks,c.HitStopCount});
        }
        DamageResolution Direct(DamageContext context,int id){var s=Single().steps[0].hits[0].strike;var t=player.PrepareGradedDamage(s,dummy,500,id);var r=dummy.ReceiveStrike(context,s,out float loss);player.CommitGradedDamage(t,r,loss,dummy.transform.position+Vector3.up);return r;}
        [UnityTest] public IEnumerator MissGuardParryEvadeSpecialDefenseDeadAndZeroNeverShowSuccess()
        {
            Place(dummy,new Vector3(8,.03f,8));Assert.That(player.TryAttack(),Is.True);yield return Until(()=>player.ComboStep==0);Assert.That(numbers.PresentedCount,Is.Zero);
            Place(dummy,player.transform.position+Vector3.forward*1.4f);dummy.SetGuardHeld(true);yield return Wait(.15f);
            var context=new DamageContext(20,player.gameObject,DamageDeliveryType.PhysicalStrike);Assert.That(Direct(context,0),Is.EqualTo(DamageResolution.Guarded));dummy.SetGuardHeld(false);yield return Wait(.55f);
            dummy.SetGuardHeld(true);Assert.That(Direct(context,1),Is.EqualTo(DamageResolution.Guarded));Assert.That(dummy.ParrySuccessCount,Is.EqualTo(1));dummy.SetGuardHeld(false);
            yield return Until(()=>!dummy.IsParryRecovering);Assert.That(dummy.TryGroundDash(Vector2.left,Vector3.forward,Vector3.right),Is.True);yield return Until(()=>dummy.IsGroundDashEvading);
            Assert.That(Direct(context,6),Is.EqualTo(DamageResolution.Guarded));yield return Until(()=>!dummy.IsGroundDashing);
            dummy.Health.GrantInvulnerability(.05f);Assert.That(Direct(context,2),Is.EqualTo(DamageResolution.Invulnerable));yield return Wait(.07f);
            var special=dummy.gameObject.AddComponent<ShibuyaGradeSpecialGuard>();Assert.That(Direct(context,3),Is.EqualTo(DamageResolution.Guarded));UnityEngine.Object.DestroyImmediate(special);
            Assert.That(Direct(new DamageContext(0,player.gameObject,DamageDeliveryType.PhysicalStrike),4),Is.EqualTo(DamageResolution.Invalid));
            dummy.Health.SetCurrentHealth(0);Assert.That(Direct(context,5),Is.EqualTo(DamageResolution.TargetDead));Assert.That(numbers.PresentedCount,Is.Zero);Assert.That(player.BlackFlashCount+player.ZoneStacks,Is.Zero);
            Done("4G3-blockers",new{numbers.PresentedCount,numbers.LastReason,player.BlackFlashCount});
        }
        [UnityTest] public IEnumerator GuardClickGrabDisplaysActualFifteenOnceWithoutCriticalOrBlackFlash()
        {
            grades.blackFlashCapable=true;player.SetCombatRandom(new ShibuyaGradeTestRandom(0,0));frame.guardHeld=true;yield return Until(()=>player.IsGuarding);frame.attack=true;
            yield return Until(()=>player.GrabSuccessCount==1);frame=default;Assert.That(numbers.PresentedCount,Is.EqualTo(1));Assert.That(numbers.LastDisplayedDamage,Is.EqualTo(15));
            Assert.That(numbers.LastDisplayedGrade,Is.EqualTo(ShibuyaAttackGrade.Normal));Assert.That(player.BlackFlashCount,Is.Zero);Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(985));
            Done("4G3-grab",Receipt(shown[0]));
        }
        [UnityTest] public IEnumerator ActualLethalBlackFlashShowsTwelveNotFiftyAndCallbackHealDoesNotContaminate()
        {
            bool healed=false;Action<Health,float> heal=(h,_)=>{if(healed)return;healed=true;h.Restore(10);};dummy.Health.HealthChanged+=heal;
            player.SetCombatRandom(new ShibuyaGradeTestRandom(.99f));yield return Hit();dummy.Health.HealthChanged-=heal;Assert.That(numbers.LastDisplayedDamage,Is.EqualTo(20));Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(990));
            dummy.Health.SetCurrentHealth(12);grades.blackFlashCapable=true;player.SetCombatRandom(new ShibuyaGradeTestRandom(0,0));int before=numbers.PresentedCount;Assert.That(player.TryAttack(),Is.True);
            yield return Until(()=>numbers.PresentedCount==before+1);Assert.That(numbers.LastDisplayedDamage,Is.EqualTo(12));Assert.That(numbers.LastDisplayedGrade,Is.EqualTo(ShibuyaAttackGrade.BlackFlash));Assert.That(numbers.LastText,Does.Contain("흑섬!\n12"));
            Assert.That(UnityEngine.Object.FindFirstObjectByType<ShibuyaMatch>().Finished,Is.True);yield return Screen("4G3-overkill-twelve");Done("4G3-overkill",new{numbers.LastDisplayedDamage,numbers.LastText,matchEnded=true});
        }
        [UnityTest] public IEnumerator OneHitMultipleTargetsEachReceivesItsOwnNumber()
        {
            Place(dummy,player.transform.position+new Vector3(-.55f,0,1.4f));var d=UnityEngine.Object.Instantiate(dummy.gameObject,player.transform.position+new Vector3(.55f,0,1.4f),Quaternion.identity).GetComponent<ShibuyaFighter>();
            player.SetCombatRandom(new ShibuyaGradeTestRandom(.99f,.99f));yield return Hit();Assert.That(shown.Count,Is.EqualTo(2));Assert.That(shown.Select(x=>x.Key.Target).Distinct().Count(),Is.EqualTo(2));
            Assert.That(dummy.Health.CurrentHealth,Is.EqualTo(980));Assert.That(d.Health.CurrentHealth,Is.EqualTo(980));yield return Screen("4G3-multiple-targets");Done("4G3-multiple-targets",shown.Select(Receipt).ToArray());
        }
        [UnityTest] public IEnumerator ProjectionColorsAnimationExpiryAndPoolCleanup()
        {
            display.displayDuration=.65f;display.maximumVisible=2;player.SetCombatRandom(new ShibuyaGradeTestRandom(.99f));yield return Hit();int slot=Enumerable.Range(0,numbers.Capacity).First(i=>numbers.GetPopupText(i)!="");
            Assert.That(numbers.TryGetLayout(slot,out var initial,out _),Is.True);Assert.That(initial.Color,Is.EqualTo(display.normalColor));Quaternion rotation=camera.transform.rotation;
            camera.transform.rotation=rotation*Quaternion.Euler(0,180,0);Assert.That(numbers.TryGetLayout(slot,out _,out string reason),Is.False);Assert.That(reason,Is.EqualTo("카메라 뒤/근접면"));
            camera.transform.rotation=rotation*Quaternion.Euler(0,75,0);Assert.That(numbers.TryGetLayout(slot,out _,out _),Is.False,"side-facing camera must hide offscreen popup");camera.transform.rotation=rotation;
            yield return Wait(.25f);Assert.That(numbers.TryGetLayout(slot,out var later,out _),Is.True);Assert.That(later.Rect.y,Is.LessThan(initial.Rect.y));Assert.That(later.Alpha,Is.LessThan(1));
            yield return Wait(.4f);Assert.That(numbers.ActiveCount,Is.Zero,"popup lifetime must expire");int objects=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Length;
            var basis=shown[0];for(int i=0;i<180;i++)numbers.Present(new ShibuyaCombatDamageResult(player,dummy,new ShibuyaDamageResultKey(player.GetInstanceID(),dummy.GetInstanceID(),700,i,ShibuyaDamageAction.Strike),DamageResolution.Applied,20,ShibuyaAttackGrade.Normal,basis.WorldPoint));
            Assert.That(numbers.Capacity,Is.EqualTo(2));Assert.That(numbers.ActiveCount,Is.EqualTo(2));Assert.That(UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Length,Is.EqualTo(objects));
            int observers=ShibuyaCombatDamageResults.ObserverCount;numbers.enabled=false;Assert.That(ShibuyaCombatDamageResults.ObserverCount,Is.EqualTo(observers-1));numbers.enabled=true;Assert.That(ShibuyaCombatDamageResults.ObserverCount,Is.EqualTo(observers));
            var owned=numbers.OwnedFont;Assert.That(owned,Is.Not.Null);int fontId=owned.GetInstanceID();
            int beforeDestroyFrame=Time.frameCount;UnityEngine.Object.Destroy(numbers);
            // EditMode UnityTest null continuations can run inside one game frame. Destroy is end-of-frame,
            // then the owned font is destroyed at the next end-of-frame; wait for actual disposal, not two Editor ticks.
            yield return Until(()=>numbers==null&&owned==null,1);Assert.That(ShibuyaCombatDamageResults.ObserverCount,Is.EqualTo(observers-1));
            Record("4G3-font-lifecycle-probe",new{frame=Time.frameCount,fontId,fontDestroyed=owned==null,numberDestroyed=numbers==null,fonts=Resources.FindObjectsOfTypeAll<Font>().Select(f=>new{id=f.GetInstanceID(),f.name,persistent=EditorUtility.IsPersistent(f),flags=f.hideFlags}).ToArray()});
            Assert.That(Resources.FindObjectsOfTypeAll<Font>().Any(f=>f.GetInstanceID()==fontId),Is.False,"owned font instance must be released after presenter destruction");Done("4G3-pool-cleanup",new{objects,capacity=2,submitted=180,observersAfter=ShibuyaCombatDamageResults.ObserverCount,fontId,beforeDestroyFrame,afterDestroyFrame=Time.frameCount});
        }
        [UnityTest] public IEnumerator OriginalSpeedFiveHitOverlayEvidence()
        {
            display.displayDuration=1.05f;grades.blackFlashCapable=true;player.SetCombatRandom(new ShibuyaGradeTestRandom(.99f,0,0,0,.99f,0,0,.99f));input.EnergyCharge.Resource.SetCurrentEnergy(100);
            var m=Single();var s=m.steps[0].hits[0].strike;m.steps[0].hits=Enumerable.Range(0,5).Select(i=>new ShibuyaHitEvent{startTime=i*.20f,strike=JsonUtility.FromJson<ShibuyaStrikeDefinition>(JsonUtility.ToJson(s))}).ToArray();player.SetMoveset(m);
            var rows=new List<object>();double begin=Time.unscaledTimeAsDouble;bool started=false;int i=0;
            while(Time.unscaledTimeAsDouble-begin<2.5){double t=Time.unscaledTimeAsDouble-begin;if(!started&&t>=.25){Assert.That(player.TryAttack(),Is.True);started=true;}
                string path=Path.Combine(Root,"4G3-media/frame-"+i.ToString("D4")+".png");Directory.CreateDirectory(Path.GetDirectoryName(path));ScreenCapture.CaptureScreenshot(path);
                rows.Add(new{index=i,time=t,numbers.PresentedCount,numbers.ActiveCount,numbers.LastDisplayedDamage,numbers.LastDisplayedGrade});i++;yield return Wait(1f/30);
            }
            yield return null;Assert.That(shown.Count,Is.EqualTo(5));Assert.That(shown.Select(x=>x.HealthLoss),Is.EqualTo(new[]{20f,50,33,55,22}));Done("4G3-media-timestamps",rows);
        }
    }
}
