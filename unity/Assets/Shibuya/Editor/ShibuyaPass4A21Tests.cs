using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass4A21Tests
    {
        private ShibuyaFighter player, victim;
        private ShibuyaOrbitCamera orbit;
        private Camera view;
        private ShibuyaLockOn selection;
        private ShibuyaInputFrame input;
        private ShibuyaLockOnProfile settings;
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static string Root => ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass4A21QA");

        [UnitySetUp] public IEnumerator Setup()
        {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);
            yield return new EnterPlayMode();
            yield return null;
            var match = Object.FindFirstObjectByType<ShibuyaMatch>();
            player = match.Player; victim = match.Dummy;
            orbit = Array.Find(Object.FindObjectsByType<ShibuyaOrbitCamera>(FindObjectsSortMode.None), c => c.LocalOwner == player);
            view = orbit.GetComponent<Camera>();
            selection = player.GetComponent<ShibuyaPlayerInput>().LockOn;
            settings = Object.Instantiate(selection.Settings); settings.name = "PASS4A21 QA Settings";
            selection.Configure(player, view, settings);
            input = default; player.GetComponent<ShibuyaPlayerInput>().TestInput = () => input;
            Place(player, new Vector3(0,.03f,0)); Place(victim, new Vector3(0,.03f,5.6f));
            Set("yaw", 0f); Set("pitch", 16f);
            yield return Wait(.15f);
            orbit.enabled = false;
            view.transform.rotation = Quaternion.LookRotation(selectionPoint - view.transform.position);
            selection.HandleButton(true, Time.unscaledTimeAsDouble); selection.HandleButton(false, Time.unscaledTimeAsDouble);
            Assert.That(selection.LockedTarget, Is.EqualTo(victim)); orbit.enabled = true;
        }
        [UnityTearDown] public IEnumerator Leave() { if(EditorApplication.isPlaying) yield return new ExitPlayMode(); }
        [TearDown] public void Cleanup()
        {
            foreach(var a in Resources.FindObjectsOfTypeAll<ScriptableObject>())
                if(a.name.StartsWith("PASS4A21 QA",StringComparison.Ordinal) && !AssetDatabase.Contains(a)) Object.DestroyImmediate(a);
        }
        private Vector3 selectionPoint => victim.GetComponent<CharacterController>().bounds.center;
        private Vector3 ScreenPoint => view.WorldToViewportPoint(selection.TargetPoint);
        private bool Visible(float margin = .04f) { var v = ScreenPoint; return v.z > .01f && v.x >= margin && v.x <= 1-margin && v.y >= margin && v.y <= 1-margin; }

        [UnityTest] public IEnumerator MaximumPitchRearRecoversAndStaysVisible()
        {
            Place(victim,new Vector3(0,.03f,-5.6f)); Set("yaw",0f); Set("pitch",player.Profile.maximumPitch);
            Set("lastManualLook",Time.unscaledTimeAsDouble);
            yield return Wait(.25f);
            Assert.That(Visible(0),Is.False,"Reproduction must start outside actual rendered viewport");
            var before = Snapshot();
            yield return Wait(6f);
            Record("maximum-rear",new { before, after=Snapshot() });
            Assert.That(selection.LockedTarget,Is.EqualTo(victim));
            Assert.That(Visible(),Is.True,"Final rendered target must return inside the screen, not merely change yaw");
            double end=Time.unscaledTimeAsDouble+.6;
            while(Time.unscaledTimeAsDouble<end) { yield return null; Assert.That(Visible(.02f),Is.True,"Recovered target left screen again"); }
        }
        [UnityTest] public IEnumerator MaximumPitchFrontDiagonalAndSymmetricRecovery()
        {
            var positions=new[]{new Vector3(0,.03f,5.6f),new Vector3(3.96f,.03f,-3.96f),new Vector3(0,.03f,-5.6f)};
            foreach(var at in positions) {
                Place(victim,at); orbit.enabled=false;orbit.enabled=true;
                Set("yaw",0f);Set("pitch",player.Profile.maximumPitch);Set("lastManualLook",double.NegativeInfinity);
                yield return null; // Read the new output pose, not the preceding target's frame.
                float previous=Read("yaw"),sum=0;int direction=0;double end=Time.unscaledTimeAsDouble+5;
                while(!Visible() && Time.unscaledTimeAsDouble<end) {
                    yield return null;float current=Read("yaw"),step=Mathf.DeltaAngle(previous,current);previous=current;
                    if(Mathf.Abs(step)>.001f) { if(direction==0) direction=step>0?1:-1;Assert.That(Mathf.Sign(step),Is.EqualTo((float)direction),"Recovery reversed its orbit direction"); }
                    sum+=Mathf.Abs(step);Assert.That(Mathf.Abs(step),Is.LessThanOrEqualTo(settings.cameraDegreesPerSecond*.05f+.01f));
                }
                // Evaluate after a rendered frame even if the preceding pose was visible.
                yield return Wait(.5f);
                if(!Visible()) yield return Wait(4f);
                Record("angle-"+at.x+"-"+at.z,new{after=Snapshot(),sum,direction});
                Assert.That(Visible(),Is.True,at.ToString());
                end=Time.unscaledTimeAsDouble+.4;
                while(Time.unscaledTimeAsDouble<end){yield return null;Assert.That(Visible(.02f),Is.True);}
            }
        }
        [UnityTest] public IEnumerator RecoveryYieldsToManualLookSearchAndOriginalGrace()
        {
            Place(victim,new Vector3(0,.03f,-5.6f));Set("yaw",0f);Set("pitch",70f);
            yield return Wait(.4f);
            orbit.ApplyLook(new Vector2(4,0));float manualYaw=Read("yaw");
            yield return Wait(.6f);
            Assert.That(Read("yaw"),Is.EqualTo(manualYaw).Within(.001f),"0.8s manual grace must be preserved");
            input.lockHeld=true;yield return null;
            yield return Wait(.9f);Assert.That(Read("yaw"),Is.EqualTo(manualYaw).Within(.001f));
            selection.CancelSearch();input.lockHeld=false;
            yield return Wait(4.5f);Assert.That(Visible(),Is.True);
        }
        [UnityTest] public IEnumerator RecoveryNearColumnAndAirTargetUsesFinalViewport()
        {
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position=new Vector3(2.8f,2,-1);wall.transform.localScale=new Vector3(.6f,4,.6f);Physics.SyncTransforms();
            Place(victim,new Vector3(0,.03f,-5.6f));Set("yaw",0f);Set("pitch",70f);
            yield return Wait(4.5f);
            Assert.That(Visible(),Is.True);AssertCameraClear();Object.Destroy(wall);
            Place(victim,new Vector3(0,7,-5.6f));FreezeAirFixture();
            orbit.enabled=false;orbit.enabled=true;Set("yaw",0f);Set("pitch",70f);
            yield return Wait(4.5f);
            Record("air-recovery",Snapshot());Assert.That(Visible(),Is.True);
            yield return Wait(.5f);Assert.That(Visible(.02f),Is.True);AssertCameraClear();
        }
        [UnityTest] public IEnumerator ImpossiblePoseStopsAndUnlockDeathClearRecovery()
        {
            Place(victim,new Vector3(0,35,0));FreezeAirFixture();
            Set("yaw",0f);Set("pitch",70f);
            yield return Wait(4.5f);
            Assert.That(Visible(),Is.False,"Directly overhead target is infeasible with the protected pitch and focus limits");
            Assert.That(orbit.ScreenRecoveryStatus,Does.StartWith("복구 제한"));
            float yaw=Read("yaw"),pitch=Read("pitch");yield return Wait(.5f);
            Assert.That(Read("yaw"),Is.EqualTo(yaw).Within(.001f));Assert.That(Read("pitch"),Is.EqualTo(pitch).Within(.001f));
            selection.Clear();yield return null;yield return null;
            Assert.That(orbit.ScreenRecoveryStatus,Is.EqualTo("일반 구도"));
        }
        [UnityTest] public IEnumerator TargetDeathDuringRecoveryClearsWithoutCameraDrift()
        {
            Place(victim,new Vector3(0,.03f,-5.6f));Set("yaw",0f);Set("pitch",70f);
            yield return Wait(.2f);
            victim.Health.ReceiveDamage(new JJKGame.Core.DamageContext(2000,player.gameObject,JJKGame.Core.DamageDeliveryType.Environmental));
            yield return Wait(.2f);Assert.That(selection.LockedTarget,Is.Null);
            Assert.That(orbit.ScreenRecoveryStatus,Is.EqualTo("일반 구도"));
        }
        [UnityTest] public IEnumerator CinematicInterruptsRecoveryAndReturnsWithoutJump()
        {
            Place(victim,new Vector3(0,.03f,-5.6f));Set("yaw",0f);Set("pitch",70f);
            yield return Wait(.2f);
            float yaw=Read("yaw"),pitch=Read("pitch");
            var film=Object.Instantiate(AssetDatabase.LoadAssetAtPath<ShibuyaCinematicProfile>(ShibuyaCinematicExample.Path));
            film.name="PASS4A21 QA Film";film.cinematicEnabled=true;
            var session=ShibuyaCinematicSession.TryStartAfterApplied(player,victim,film,JJKGame.Core.DamageResolution.Applied,ShibuyaCinematicTrigger.Default,7002,0);
            Assert.That(session,Is.Not.Null);
            yield return Wait(.1f);orbit.ApplyLook(new Vector2(25,25));
            Assert.That(Read("yaw"),Is.EqualTo(yaw));Assert.That(Read("pitch"),Is.EqualTo(pitch));
            session.Finish("PASS4A21 check");double end=Time.unscaledTimeAsDouble+3;
            while(orbit.IsReturningView && Time.unscaledTimeAsDouble<end)yield return null;
            Assert.That(orbit.IsReturningView,Is.False);
            Vector3 returned=view.transform.position;Quaternion rotation=view.transform.rotation;
            yield return null;
            Record("cinematic-return",new{jump=Vector3.Distance(returned,view.transform.position),angle=Quaternion.Angle(rotation,view.transform.rotation)});
            Assert.That(Vector3.Distance(returned,view.transform.position),Is.LessThan(.03f));
            Assert.That(Quaternion.Angle(rotation,view.transform.rotation),Is.LessThan(.1f));
            // ResumeLockOnFraming starts its grace on the return-completion frame. Waiting
            // another full .5s after the checks above would assert AFTER that grace expired.
            double resumeAt=(double)Field("lockResumeAt").GetValue(orbit);int graceFrames=0;
            while(Time.unscaledTimeAsDouble<resumeAt) {
                Assert.That(Read("yaw"),Is.EqualTo(yaw));
                Assert.That(Read("pitch"),Is.EqualTo(pitch));
                graceFrames++;yield return null;
            }
            Record("cinematic-grace",new{graceFrames,resumeAt,observedAt=Time.unscaledTimeAsDouble});
            Assert.That(graceFrames,Is.GreaterThan(0));
            yield return Wait(4.5f);Assert.That(Visible(),Is.True);
            Assert.That(Time.timeScale,Is.EqualTo(1));
        }
        private void AssertCameraClear()
        {
            foreach(var c in Physics.OverlapSphere(view.transform.position,player.Profile.cameraCollisionRadius*.95f,~0,QueryTriggerInteraction.Ignore))
                Assert.That(c.GetComponentInParent<ShibuyaFighter>(),Is.Not.Null,c.name);
        }
        // Isolate camera framing from falling-target gameplay, without changing saved combat data.
        private void FreezeAirFixture() => typeof(ShibuyaFighter).GetMethod("ApplyLocalHitStop",Private).Invoke(victim,new object[]{12f});
        private object Snapshot()
        {
            var v=ScreenPoint;
            var pos=view.transform.position;
            float tan=Mathf.Tan(view.fieldOfView*.5f*Mathf.Deg2Rad);
            float clampedY=Mathf.Clamp(v.y,.5f-settings.cameraDeadZone.y,.5f+settings.cameraDeadZone.y);
            float requestedPitch=-(Mathf.Atan((v.y-.5f)*2*tan)-Mathf.Atan((clampedY-.5f)*2*tan))*Mathf.Rad2Deg;
            return new { yaw=Read("yaw"),pitch=Read("pitch"),limit=player.Profile.maximumPitch,
                viewport=new[]{v.x,v.y,v.z},position=new[]{pos.x,pos.y,pos.z},shoulder=Read("lockShoulder"),arm=Read("collisionArmDistance"),
                oldRequestedPitch=requestedPitch,oldClampedPitchChange=Mathf.Clamp(Read("pitch")+requestedPitch,player.Profile.minimumPitch,player.Profile.maximumPitch)-Read("pitch"),
                manualAge=Time.unscaledTimeAsDouble-(double)Field("lastManualLook").GetValue(orbit),occluded=selection.IsOccluded };
        }
        private static FieldInfo Field(string name)=>typeof(ShibuyaOrbitCamera).GetField(name,Private);
        private void Set(string name,object value)=>Field(name).SetValue(orbit,value);
        private float Read(string name)=>(float)Field(name).GetValue(orbit);
        private static void Place(ShibuyaFighter f,Vector3 at) { var cc=f.GetComponent<CharacterController>();cc.enabled=false;f.transform.position=at;cc.enabled=true;Physics.SyncTransforms(); }
        private static IEnumerator Wait(float seconds) { double end=Time.unscaledTimeAsDouble+seconds;while(Time.unscaledTimeAsDouble<end) yield return null; }
        private static void Record(string name,object data) { Directory.CreateDirectory(Root);File.WriteAllText(Path.Combine(Root,name+".json"),Newtonsoft.Json.JsonConvert.SerializeObject(data,Newtonsoft.Json.Formatting.Indented)); }
    }
}
