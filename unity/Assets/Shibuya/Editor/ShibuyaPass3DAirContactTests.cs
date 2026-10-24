using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass3DAirContactTests
    {
        private static ShibuyaMatch M => Object.FindFirstObjectByType<ShibuyaMatch>();
        private static ShibuyaFighter P => M.Player;
        private static ShibuyaFighter D => M.Dummy;
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static float Number(ShibuyaFighter f, string field) => (float)typeof(ShibuyaFighter).GetField(field, Private).GetValue(f);
        private static object Value(ShibuyaFighter f, string field) => typeof(ShibuyaFighter).GetField(field, Private).GetValue(f);
        private readonly List<object> trace = new List<object>();
        private int clicks, steps, events, applied;
        private ShibuyaInputFrame input;
        private bool pressed;
        private string scenario;
        private static float[] V(Vector3 v) => new[]{v.x,v.y,v.z};
        private void Sample(string beat, int id = -1) {
            if(M == null) return;
            var cc = D.GetComponent<CharacterController>();
            var s = P.IsAirAttacking ? P.CurrentStrike : P.AerialDefinition.aerialAttack.hits[0].strike;
            Vector3 center = P.transform.position + Vector3.up*s.attackHeight + P.transform.forward*s.attackForwardOffset;
            trace.Add(new {beat,id,frame=Time.frameCount,time=Time.unscaledTimeAsDouble,dt=Time.deltaTime,
                player=V(P.transform.position),victim=V(D.transform.position),gap=Vector3.Distance(P.transform.position,D.transform.position),
                pendingLaunch=Number(D,"airDistanceRemaining"),launchTime=Number(D,"airTravelTime"),
                knockback=V(D.KnockbackVelocity), playerVy=P.VerticalVelocity,victimVy=D.VerticalVelocity,
                hover=P.IsAerialHovering,held=D.IsHeldByAerialHit, stun=Value(D,"stunnedUntil"),clock=D.CombatTime,
                center=V(center),radius=s.attackRadius,colliderCenter=V(cc.bounds.center),colliderSize=V(cc.bounds.size),
                colliderDistance=Vector3.Distance(center,cc.ClosestPoint(center)),hp=D.Health.CurrentHealth,clicks,steps,events,applied});
        }
        private IEnumerator Setup(string name) {
            scenario=name; trace.Clear(); clicks=steps=events=applied=0;input=default;pressed=false;
            Application.runInBackground=true;yield return Wait(.1f);
            var p=Object.Instantiate(P.Profile);p.name="AirContact QA Profile";
            p.aerialCombat=Object.Instantiate(p.aerialCombat);p.aerialCombat.name="AirContact QA Air";
            P.Configure(p,AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>("Assets/Shibuya/Data/Player Stats.asset"));
            P.GetComponent<ShibuyaPlayerInput>().TestInput=()=> {if(input.attack&&!pressed)clicks++;pressed=input.attack;return input;};
            Place(P,new Vector3(0,.03f,-1.4f));Place(D,new Vector3(0,.03f,0));P.transform.rotation=Quaternion.identity;
            yield return null;
        }
        private void Track() {
            P.AttackStarted+=OnStep;P.HitEventStarted+=OnEvent;D.Health.DamageResolved+=OnDamage;
        }
        private void OnStep(int _) {steps++;Sample("accepted");}
        private void OnEvent(int id) {events++;Sample("event",id);}
        private void OnDamage(Health h,DamageContext c,DamageResolution r) {if(c.Source==P.gameObject&&r==DamageResolution.Applied){applied++;Sample("applied");}}
        private IEnumerator Press() {input.attack=true;yield return null;input.attack=false;yield return null;}
        private IEnumerator Upper() {
            Assert.That(P.TryAttack());
            for(int i=1;i<=3;i++){yield return Until(()=>P.ComboStep==i&&P.AttackElapsed>=P.AttackDuration-.12f);Assert.That(P.TryAttack(ShibuyaFinisher.Uppercut));yield return Until(()=>P.ComboStep==i+1);}
            yield return Until(()=>P.ConfirmedHitCount==4);Sample("upper-hit");
        }
        private IEnumerator Arrive() {
            P.SetIntent(Vector3.zero,false,true);Sample("space");
            yield return Until(()=>P.CanAirAttack);Sample("arrived");
        }
        private IEnumerator ThreeClicks(bool pursuit) {
            yield return Setup(pursuit?"pursuit-three":"jump-three");
            if(pursuit){yield return Upper();yield return Arrive();}
            else {P.SetIntent(Vector3.zero,false,true);D.SetIntent(Vector3.zero,false,true);yield return Until(()=>P.transform.position.y>.7f&&D.transform.position.y>.7f);}
            Track();float hp=D.Health.CurrentHealth;
            yield return Press();
            for(int i=1;i<=2;i++){yield return Until(()=>steps==i&&P.IsAirAttacking&&P.AttackElapsed>=P.AttackDuration-P.AerialDefinition.aerialLinkInputWindow+.015f);yield return Press();yield return Until(()=>steps==i+1);}
            yield return Wait(1.3f);
            Assert.That(clicks,Is.EqualTo(3));Assert.That(steps,Is.EqualTo(3));Assert.That(events,Is.EqualTo(5));
            Assert.That(applied,Is.EqualTo(5));Assert.That(hp-D.Health.CurrentHealth,Is.EqualTo(62));
        }
        [UnityTest] public IEnumerator JumpThreeClicks() {Open();yield return new EnterPlayMode();yield return ThreeClicks(false);yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator PursuitThreeClicks() {Open();yield return new EnterPlayMode();yield return ThreeClicks(true);yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator ContactHasNoDamageOrAttackAndFallsWithoutOldLaunch() {Open();yield return new EnterPlayMode();yield return Contact();yield return new ExitPlayMode();}
        private IEnumerator Contact() {
            yield return Setup("contact-no-lmb");yield return Upper();
            Assert.That(Number(D,"airDistanceRemaining"),Is.GreaterThan(0));
            float hp=D.Health.CurrentHealth;int accepted=P.AcceptedAttackCount,hits=P.ConfirmedHitCount;
            yield return Arrive();
            Assert.That(P.AirChaseContactCount,Is.EqualTo(1));Assert.That(D.IsChaseContactHeld);
            Assert.That(Number(D,"airDistanceRemaining"),Is.Zero);Assert.That(D.VerticalVelocity,Is.Zero);
            Vector3 at=D.transform.position;yield return Wait(.06f);
            Assert.That(D.transform.position.y,Is.EqualTo(at.y).Within(.001));
            Assert.That(D.transform.position.z,Is.EqualTo(at.z).Within(.001));
            yield return Until(()=>!D.IsChaseContactHeld);yield return Wait(.08f);
            Assert.That(D.transform.position.y,Is.LessThan(at.y-.01));
            Assert.That(D.transform.position.z,Is.EqualTo(at.z).Within(.001));
            Assert.That(D.Health.CurrentHealth,Is.EqualTo(hp));Assert.That(P.AcceptedAttackCount,Is.EqualTo(accepted));
            Assert.That(P.ConfirmedHitCount,Is.EqualTo(hits));Assert.That(P.IsAirAttacking,Is.False);
            yield return Until(()=>D.IsGrounded&&P.IsGrounded);Assert.That(D.IsChaseContactHeld,Is.False);
        }
        [UnityTest] public IEnumerator NoSpacePreservesOriginalLaunch() {Open();yield return new EnterPlayMode();yield return NoContact("no-space");yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator BlockedSpaceCannotContact() {Open();yield return new EnterPlayMode();yield return NoContact("wall");yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator EscapingTargetCannotContact() {Open();yield return new EnterPlayMode();yield return NoContact("range");yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator ImmuneTargetCannotContact() {Open();yield return new EnterPlayMode();yield return NoContact("immune");yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator TimeoutCannotContact() {Open();yield return new EnterPlayMode();yield return NoContact("timeout");yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator InterruptedPursuitCannotContact() {Open();yield return new EnterPlayMode();yield return NoContact("interrupt");yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator DeadTargetCannotContact() {Open();yield return new EnterPlayMode();yield return NoContact("death");yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator LandedTargetCannotContact() {Open();yield return new EnterPlayMode();yield return NoContact("land");yield return new ExitPlayMode();}
        private IEnumerator NoContact(string kind) {
            yield return Setup(kind);yield return Upper();float z=D.transform.position.z;
            if(kind=="timeout")P.AerialDefinition.chaseMaxDuration=.01f;
            if(kind=="range")Place(D,D.transform.position+Vector3.forward*15);
            if(kind=="immune")D.Health.GrantInvulnerability(2);
            if(kind=="death")D.Health.Kill();
            if(kind=="land"){Place(D,new Vector3(8,.03f,0));typeof(ShibuyaFighter).GetField("verticalSpeed",Private).SetValue(D,-1f);yield return Until(()=>!D.IsAirborne);Assert.That(D.IsGrounded);}
            if(kind=="wall") {
                var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.name="AirContact QA Wall";
                wall.transform.position=P.transform.position+Vector3.up*2+Vector3.forward*.5f;
                wall.transform.localScale=new Vector3(8,8,.15f);Physics.SyncTransforms();
            }
            if(kind!="no-space")P.SetIntent(Vector3.zero,false,true);
            if(kind=="interrupt"){yield return Until(()=>P.IsChasing);P.ApplyHitReaction(Vector3.back,.1f);}
            yield return Wait(.45f);Assert.That(P.AirChaseContactCount,Is.Zero);Assert.That(D.IsChaseContactHeld,Is.False);
            if(kind=="no-space"){
                Assert.That(D.IsAirborne);Assert.That(D.transform.position.z-z,Is.GreaterThan(.8f));
                Assert.That((bool)Value(D,"launchMotionActive"));Assert.That(D.Health.CurrentHealth,Is.EqualTo(915));
            }
        }
        [UnityTest] public IEnumerator NewAppliedNormalOwnsAirMotion() {Open();yield return new EnterPlayMode();yield return NewHit(ShibuyaHitOutcome.Normal,false);yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator NewAppliedLaunchStartsNewTrajectory() {Open();yield return new EnterPlayMode();yield return NewHit(ShibuyaHitOutcome.Launch,false);yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator NewAppliedDownEndsAirTrajectory() {Open();yield return new EnterPlayMode();yield return NewHit(ShibuyaHitOutcome.Knockdown,false);yield return new ExitPlayMode();}
        [UnityTest] public IEnumerator RejectedDamageCannotReplaceLaunch() {Open();yield return new EnterPlayMode();yield return NewHit(ShibuyaHitOutcome.Normal,true);yield return new ExitPlayMode();}
        private IEnumerator NewHit(ShibuyaHitOutcome outcome,bool immune) {
            yield return Setup("new-hit-"+outcome+"-"+immune);yield return Upper();
            var go=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Shibuya/Prefabs/ShibuyaPlayer.prefab"));
            go.name="AirContact QA Third";var source=go.GetComponent<ShibuyaFighter>();
            // This fixture uses direct TryAttack; it has no local camera/input owner.
            go.GetComponent<ShibuyaPlayerInput>().enabled=false;
            var profile=Object.Instantiate(P.Profile);profile.name="AirContact QA Third Profile";
            var step=JsonUtility.FromJson<ShibuyaMovesetStep>(JsonUtility.ToJson(P.Profile.finishers.uppercut));
            step.hits[0].outcome=outcome;step.hits[0].startTime=0;step.hits[0].strike.startup=.01f;step.hits[0].strike.powerWindupTime=0;
            if(outcome==ShibuyaHitOutcome.Normal)step.hits[0].strike.knockbackSpeed=P.AerialDefinition.aerialAttack.hits[0].strike.knockbackSpeed;
            var moves=ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();moves.name="AirContact QA Third Moves";
            moves.steps=new[]{step};Assert.That(source.SetMoveset(moves));source.Configure(profile,AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>("Assets/Shibuya/Data/Player Stats.asset"));
            Place(D,new Vector3(4,.10f,0));Place(source,new Vector3(4,.03f,-1.4f));source.transform.rotation=Quaternion.identity;
            if(immune)D.Health.GrantInvulnerability(1);
            float hp=D.Health.CurrentHealth;yield return null;Assert.That(source.TryAttack());
            if(immune){yield return Wait(.1f);Assert.That(D.Health.CurrentHealth,Is.EqualTo(hp));Assert.That((bool)Value(D,"launchMotionActive"));}
            else {
                yield return Until(()=>source.ConfirmedHitCount==1);Sample("new-applied");
                if(outcome==ShibuyaHitOutcome.Normal){Assert.That(D.IsAirborne);Assert.That(Number(D,"airDistanceRemaining"),Is.Zero);Assert.That((bool)Value(D,"launchMotionActive"),Is.False);Assert.That(D.KnockbackVelocity.magnitude,Is.GreaterThan(0));}
                else if(outcome==ShibuyaHitOutcome.Launch){Assert.That(D.IsAirborne);Assert.That((bool)Value(D,"launchMotionActive"));Assert.That(Number(D,"airDistanceRemaining"),Is.GreaterThan(1));Assert.That(D.VerticalVelocity,Is.GreaterThan(0));}
                else {Assert.That(D.IsDowned);Assert.That(D.IsAirborne,Is.False);Assert.That(Number(D,"airDistanceRemaining"),Is.Zero);}
            }
        }
        [UnityTest] public IEnumerator LowFrameContactAndCleanup(){
            Open();yield return new EnterPlayMode();int rate=Application.targetFrameRate,vsync=QualitySettings.vSyncCount;
            try {Application.targetFrameRate=30;QualitySettings.vSyncCount=0;yield return Contact();Assert.That(Time.timeScale,Is.EqualTo(1));}
            finally {Application.targetFrameRate=rate;QualitySettings.vSyncCount=vsync;}
            yield return new ExitPlayMode();
        }
        [UnityTest] public IEnumerator ContactCancellationDeathAndRestartReleaseState() {Open();yield return new EnterPlayMode();yield return ContactCancel();yield return new ExitPlayMode();}
        private IEnumerator ContactCancel(){
            yield return Setup("contact-cancel");yield return Upper();yield return Arrive();Assert.That(D.IsChaseContactHeld);
            P.ApplyHitReaction(Vector3.back,.1f);yield return null;Assert.That(D.IsChaseContactHeld,Is.False);
            D.Health.Kill();yield return null;Assert.That(M.Finished);Assert.That(D.IsChaseContactHeld,Is.False);
            var old=M;M.RestartBattle();yield return Until(()=>M!=old);
            P.GetComponent<ShibuyaPlayerInput>().TestInput=()=>default;
            Assert.That(D.IsChaseContactHeld,Is.False);Assert.That(P.AirChaseContactCount,Is.Zero);
        }
        [Test] public void ContactDataKoreanUndoAndProtectedValues(){
            var p=AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>("Assets/Shibuya/Data/Shibuya Combat Profile.asset");
            var data=Object.Instantiate(p.aerialCombat);data.name="AirContact QA Inspector";
            Assert.That(ShibuyaInspector.KoreanLabel("chaseContactHoldTime"),Is.Not.EqualTo("chaseContactHoldTime"));
            var so=new SerializedObject(data);float before=data.chaseContactHoldTime;
            Undo.IncrementCurrentGroup();int group=Undo.GetCurrentGroup();so.FindProperty("chaseContactHoldTime").floatValue=.23f;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();
            Assert.That(data.chaseContactHoldTime,Is.EqualTo(.23f));Undo.CollapseUndoOperations(group);Undo.PerformUndo();Assert.That(data.chaseContactHoldTime,Is.EqualTo(before));
            data.chaseContactHoldTime=float.NaN;Assert.That(data.IsValid,Is.False);
            Assert.That(p.aerialCombat.aerialMaxHoverTime,Is.EqualTo(1.2f));CollectionAssert.AreEqual(new[]{12f,13f},Array.ConvertAll(p.aerialCombat.aerialAttack.hits,h=>h.strike.damage));
        }
        private static void Open()=>EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);
        private static void Place(ShibuyaFighter f,Vector3 at){var cc=f.GetComponent<CharacterController>();cc.enabled=false;f.transform.position=at;cc.enabled=true;Physics.SyncTransforms();}
        private IEnumerator Wait(float seconds){double end=Time.realtimeSinceStartupAsDouble+seconds;while(Time.realtimeSinceStartupAsDouble<end){Sample("frame");yield return null;}}
        private IEnumerator Until(Func<bool> condition){double end=Time.realtimeSinceStartupAsDouble+6;while(!condition()&&Time.realtimeSinceStartupAsDouble<end){Sample("frame");yield return null;}Assert.That(condition(),Is.True,"air contact wait: "+P.AirChaseReason);}
        [UnityTearDown] public IEnumerator Leave(){
            if(M!=null){P.AttackStarted-=OnStep;P.HitEventStarted-=OnEvent;D.Health.DamageResolved-=OnDamage;}
            if(!string.IsNullOrEmpty(scenario)){
                string dir=ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaAirContactMetrics");Directory.CreateDirectory(dir);
                File.WriteAllText(dir+"/"+scenario+".json",Newtonsoft.Json.JsonConvert.SerializeObject(trace,Newtonsoft.Json.Formatting.Indented));
            }
            if(EditorApplication.isPlaying)yield return new ExitPlayMode();
        }
        [TearDown] public void Clean(){foreach(var a in Resources.FindObjectsOfTypeAll<ScriptableObject>())if(a.name.StartsWith("AirContact QA",StringComparison.Ordinal)&&!AssetDatabase.Contains(a))Object.DestroyImmediate(a);}
    }
}


