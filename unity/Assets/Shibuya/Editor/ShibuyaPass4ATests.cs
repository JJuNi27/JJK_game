using System;
using System.Collections;
using System.IO;
using System.Reflection;
using JJKGame.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass4ATests
    {
        private ShibuyaMatch match;
        private ShibuyaFighter p, d;
        private ShibuyaOrbitCamera orbit;
        private Camera camera;
        private ShibuyaLockOn selection;
        private ShibuyaLockOnProfile settings;
        private ShibuyaInputFrame frame;
        [UnitySetUp] public IEnumerator Setup()
        {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);
            yield return new EnterPlayMode();
            Application.runInBackground = true; yield return null;
            match = Object.FindFirstObjectByType<ShibuyaMatch>(); p = match.Player; d = match.Dummy;
            orbit = Array.Find(Object.FindObjectsByType<ShibuyaOrbitCamera>(FindObjectsSortMode.None), c => c.LocalOwner == p);
            camera = orbit.GetComponent<Camera>();
            p.GetComponent<ShibuyaPlayerInput>().TestInput = () => frame;
            frame = default; selection = p.GetComponent<ShibuyaPlayerInput>().LockOn;
            Assert.That(selection, Is.Not.Null, "real scene runtime wiring");
            settings = Object.Instantiate(selection.Settings); settings.name = "PASS4A QA Settings";
            selection.Configure(p, camera, settings); orbit.enabled = false;
            Place(p, new Vector3(0, .03f, -1.4f)); Place(d, new Vector3(0, .03f, 0));
            p.transform.rotation = Quaternion.identity; yield return null; Look(d);
        }
        [UnityTearDown] public IEnumerator Leave() { if (EditorApplication.isPlaying) yield return new ExitPlayMode(); }
        [TearDown] public void Cleanup()
        {
            foreach (var obj in Resources.FindObjectsOfTypeAll<ScriptableObject>())
                if (obj.name.StartsWith("PASS4A QA", StringComparison.Ordinal) && !AssetDatabase.Contains(obj)) Object.DestroyImmediate(obj);
        }

        [UnityTest] public IEnumerator VisibleTargetRealInputPressReleaseAndNoAutoAcquire()
        {
            yield return Wait(.05f); Assert.That(selection.LockedTarget, Is.Null);
            frame.lockHeld = true; yield return null; yield return null;
            Assert.That(selection.Candidate, Is.EqualTo(d)); Assert.That(selection.LockedTarget, Is.Null);
            frame.lockHeld = false; yield return null; Assert.That(selection.LockedTarget, Is.EqualTo(d));
            Assert.That(p.ExplicitLockTarget, Is.EqualTo(d)); Assert.That(p.ComboTarget, Is.Null);
        }
        [UnityTest] public IEnumerator AcquisitionAndRetentionUse3DDistanceAndHysteresis()
        {
            Place(p, new Vector3(0, 100, 0)); Place(d, new Vector3(0, 100, 49.9f)); Look(d); Select();
            Assert.That(selection.LockedTarget, Is.EqualTo(d));
            Place(d, new Vector3(0, 100, 55)); selection.Refresh(Time.unscaledTimeAsDouble); Assert.That(selection.LockedTarget, Is.EqualTo(d));
            Place(d, new Vector3(0, 100, 60.1f)); selection.Refresh(Time.unscaledTimeAsDouble); Assert.That(selection.LockedTarget, Is.Null);
            Place(d, new Vector3(0, 100, 50.1f)); Look(d); Select(); Assert.That(selection.LockedTarget, Is.Null);
            Place(d, new Vector3(0, 140, 31)); Look(d); Select(); Assert.That(selection.LockedTarget, Is.Null, "planar31 but 3D50.6");
            Place(d, new Vector3(0, 130, 39)); Look(d); Select(); Assert.That(selection.LockedTarget, Is.EqualTo(d));
            Place(d, new Vector3(0, 150, 35)); selection.Refresh(Time.unscaledTimeAsDouble); Assert.That(selection.LockedTarget, Is.Null, "3D61m");
            yield return null;
        }
        [UnityTest] public IEnumerator ScreenCenterWinsAmongThreeAndNoOffCenterOrBehindCandidate()
        {
            Place(d, new Vector3(0,.03f,9)); Look(d);
            var a = Actor(new Vector3(3,.03f,8)); var b = Actor(new Vector3(-2,.03f,7));
            Select(); Assert.That(selection.LockedTarget, Is.EqualTo(d));
            selection.Clear(); camera.transform.rotation = Quaternion.LookRotation(Vector3.right); Select(); Assert.That(selection.LockedTarget, Is.Null);
            Look(a); Select(); Assert.That(selection.LockedTarget, Is.EqualTo(a));
            Assert.That(selection.LockedTarget, Is.Not.EqualTo(b)); yield return null;
        }
        [UnityTest] public IEnumerator EnvironmentWallBlocksNewSelectionButFightersDoNot()
        {
            Place(d, new Vector3(0,.03f,8)); Look(d);
            var third = Actor(new Vector3(0,.03f,3)); third.GetComponent<CharacterController>().height = 4;
            Select(); Assert.That(selection.LockedTarget, Is.EqualTo(d));
            selection.Clear(); third.enabled=false;
            Wall(new Vector3(0,1,4), new Vector3(3,5,.3f)); Select(); Assert.That(selection.LockedTarget, Is.Null);
            yield return null;
        }
        [UnityTest] public IEnumerator SightGraceRecoversThenExpiresAtDefaultTimeWithoutSwitch()
        {
            Place(d, new Vector3(0,.03f,8)); Look(d); Select(); Assert.That(settings.sightGraceTime, Is.EqualTo(1.5f));
            var wall = Wall(new Vector3(0,1,4),new Vector3(3,5,.3f)); yield return Wait(.2f);
            Assert.That(selection.IsOccluded); Assert.That(selection.LockedTarget, Is.EqualTo(d));
            Object.Destroy(wall); yield return Wait(.08f); Assert.That(selection.IsOccluded, Is.False);
            wall = Wall(new Vector3(0,1,4),new Vector3(3,5,.3f));
            double began = Time.unscaledTimeAsDouble; yield return Wait(1.3f); Assert.That(selection.LockedTarget, Is.EqualTo(d));
            yield return Wait(.3f); Assert.That(selection.LockedTarget, Is.Null); Assert.That(selection.HasLostFeedback);
            Metric("sight-loss", new { duration = Time.unscaledTimeAsDouble-began, reason=selection.LossReason });
            Object.Destroy(wall); yield return Wait(.1f); Assert.That(selection.LockedTarget, Is.Null);
        }
        [UnityTest] public IEnumerator ShortClickUnlockAndLongHoldReselectAreDistinct()
        {
            Select(); frame.lockHeld = true; yield return null; frame.lockHeld = false; yield return null;
            Assert.That(selection.LockedTarget, Is.Null); yield return Wait(.1f); Assert.That(selection.LockedTarget, Is.Null);
            Select(); var other = Actor(new Vector3(2,.03f,5));
            frame.lockHeld = true; yield return null; Look(other); yield return Wait(settings.reselectHoldTime+.04f);
            Assert.That(selection.Candidate, Is.EqualTo(other)); frame.lockHeld = false; yield return null;
            Assert.That(selection.LockedTarget, Is.EqualTo(other));
            frame.lockHeld=true;yield return null;camera.transform.rotation=Quaternion.LookRotation(Vector3.right);
            yield return Wait(settings.reselectHoldTime+.04f);frame.lockHeld=false;yield return null;
            Assert.That(selection.LockedTarget,Is.Null);Assert.That(selection.HasLostFeedback);
            selection.Clear(); camera.transform.rotation = Quaternion.LookRotation(Vector3.right); Select(); Assert.That(selection.LockedTarget, Is.Null);
        }
        [UnityTest] public IEnumerator DeathDisabledDestroyedAndBattleEndClearLock()
        {
            var extra = Actor(new Vector3(2,.03f,4)); Look(extra); Select();
            extra.enabled = false; yield return null; Assert.That(selection.LockedTarget, Is.Null);
            extra.enabled = true; Look(extra); Select(); Object.Destroy(extra.gameObject); yield return null; yield return null;
            Assert.That(selection.LockedTarget, Is.Null); Assert.That(selection.HasLostFeedback);
            var dead = Actor(new Vector3(2,.03f,5)); Look(dead); Select(); dead.Health.Kill(); yield return null; Assert.That(selection.LockedTarget, Is.Null);
            Look(d); Select(); d.Health.Kill(); yield return null; Assert.That(match.Finished); Assert.That(selection.LockedTarget, Is.Null);
        }
        [UnityTest] public IEnumerator AimUsesEmissionOriginHeightAndFreshPositionWithoutRootPitch()
        {
            Place(d, new Vector3(0,8,8)); Look(d); Select();
            Vector3 origin = p.transform.position + new Vector3(.4f,1.7f,.2f); Quaternion rotation=p.transform.rotation;
            var before=selection.GetAim(origin); Assert.That(before.Target, Is.EqualTo(d)); Assert.That(before.Direction.y, Is.GreaterThan(.4f));
            Assert.That(Vector3.Angle(before.Direction, selection.TargetPoint-origin), Is.LessThan(.01f));
            Place(d, new Vector3(3,10,8)); var after=selection.GetAim(origin);
            Assert.That(Vector3.Angle(before.Direction,after.Direction), Is.GreaterThan(5));
            Assert.That(p.transform.rotation, Is.EqualTo(rotation));
            var snapshot=after.Direction; Place(d, new Vector3(-3,10,8)); Assert.That(after.Direction, Is.EqualTo(snapshot), "returned aim value does not home");
            selection.Clear(); var free=selection.GetAim(origin); Assert.That(free.Target, Is.Null); Assert.That(Vector3.Angle(free.Direction,camera.transform.forward), Is.LessThan(.01f));
            yield return null;
        }
        [UnityTest] public IEnumerator CameraAssistsYawPitchSmoothlyAndRespectsManualSearchAndClamp()
        {
            Place(d,new Vector3(3,3,8)); Look(d); Select(); orbit.enabled=true;
            float yaw=Read(orbit,"yaw"),pitch=Read(orbit,"pitch"); yield return Wait(.18f);
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(yaw,Read(orbit,"yaw"))),Is.GreaterThan(.1f));
            Assert.That(Read(orbit,"pitch"),Is.LessThan(pitch));
            float before=Read(orbit,"yaw"); orbit.ApplyLook(new Vector2(5,0)); float manual=Read(orbit,"yaw");
            Assert.That(Mathf.DeltaAngle(before,manual),Is.EqualTo(5*p.Profile.mouseSensitivity).Within(.01f));
            yield return Wait(.15f); Assert.That(Read(orbit,"yaw"),Is.EqualTo(manual));
            frame.lockHeld=true; yield return null; float search=Read(orbit,"yaw"); yield return Wait(.4f); Assert.That(Read(orbit,"yaw"),Is.EqualTo(search));
            orbit.ApplyLook(new Vector2(0,1000)); Assert.That(Read(orbit,"pitch"),Is.EqualTo(p.Profile.minimumPitch));
            frame.lockHeld=false; selection.Clear(); yield return null; float released=Read(orbit,"yaw"); yield return Wait(.1f); Assert.That(Read(orbit,"yaw"),Is.EqualTo(released));
        }
        [UnityTest] public IEnumerator LockCameraKeepsExistingWallCollision()
        {
            Select(); orbit.enabled=true; var wall=Wall(new Vector3(0,1,-3),new Vector3(4,5,.3f));
            yield return Wait(.15f); Assert.That(wall.GetComponent<Collider>().bounds.Contains(camera.transform.position),Is.False);
            Assert.That(camera.transform.position.z,Is.GreaterThan(-3));
        }
        [UnityTest] public IEnumerator CinematicAndReturnOwnCameraDespiteLockedTarget()
        {
            Select(); orbit.enabled=true; settings.cameraStrength=1;
            var film=Object.Instantiate(AssetDatabase.LoadAssetAtPath<ShibuyaCinematicProfile>(ShibuyaCinematicExample.Path));film.name="PASS4A QA Film";film.cinematicEnabled=true;
            var session=ShibuyaCinematicSession.TryStartAfterApplied(p,d,film,DamageResolution.Applied,ShibuyaCinematicTrigger.Default,991,0);
            Assert.That(session,Is.Not.Null); float yaw=Read(orbit,"yaw"),pitch=Read(orbit,"pitch");
            yield return Wait(.1f); orbit.ApplyLook(new Vector2(20,20));
            Assert.That(Read(orbit,"yaw"),Is.EqualTo(yaw));Assert.That(Read(orbit,"pitch"),Is.EqualTo(pitch));
            session.Finish("PASS4A check"); Assert.That(orbit.IsReturningView);
            yield return null; Assert.That(Read(orbit,"yaw"),Is.EqualTo(yaw)); Assert.That(Read(orbit,"pitch"),Is.EqualTo(pitch));
            yield return Until(()=>!orbit.IsReturningView); Assert.That(selection.LockedTarget,Is.EqualTo(d)); Assert.That(Time.timeScale,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator FarLockDoesNotForceHitOrReachAndUnlockRestoresFallback()
        {
            Place(d,new Vector3(0,.03f,12)); Look(d); Select(); Assert.That(p.TryAttack()); yield return Wait(.5f);
            Assert.That(p.ConfirmedHitCount,Is.Zero);Assert.That(d.Health.CurrentHealth,Is.EqualTo(1000));
            selection.Clear(); Place(p,new Vector3(0,.03f,-1.4f));Place(d,new Vector3(.2f,.03f,0));p.transform.rotation=Quaternion.identity;
            Assert.That(p.TryAttack());yield return Until(()=>p.ConfirmedHitCount==1);Assert.That(p.ComboTarget,Is.EqualTo(d));
        }
        [UnityTest] public IEnumerator GroundMeleePrefersLockWithinExistingPreparationAndHeightRules()
        {
            Place(d,new Vector3(-.5f,.03f,.1f));var locked=Actor(new Vector3(.45f,.03f,.1f));
            Look(locked);Select();Assert.That(selection.LockedTarget,Is.EqualTo(locked));
            Assert.That(p.TryAttack());yield return Wait(.055f);
            Assert.That(Read(p,"aimStarted",0),Is.True); Assert.That(ReadTarget(p,0),Is.EqualTo(locked));
            Assert.That(p.TrackingTurnApplied,Is.LessThanOrEqualTo(p.Profile.trackingMaxTurnAngle+.001f));
            yield return Wait(.5f); Assert.That(p.ConfirmedHitCount,Is.GreaterThanOrEqualTo(1));
        }
        [UnityTest] public IEnumerator AirMeleeCanUseLockWithoutPursuitAndCannotHitHighOutOfReach()
        {
            Place(d,new Vector3(.5f,.03f,0)); Look(d);Select();
            p.SetIntent(Vector3.zero,false,true);d.SetIntent(Vector3.zero,false,true);
            yield return Until(()=>p.transform.position.y>.7f&&d.transform.position.y>.7f);
            Assert.That(p.TryAttack());yield return Until(()=>p.ConfirmedHitCount>=1);
            Assert.That(p.IsAirAttacking || p.IsInAirSession);Assert.That(p.transform.eulerAngles.x,Is.EqualTo(0));
            Metric("air-lock",new{hits=p.ConfirmedHitCount,turn=p.TrackingTurnApplied,height=p.transform.position.y});
        }
        [UnityTest] public IEnumerator MeleeDoesNotIgnoreWallImmunityOrTurnBudget()
        {
            Look(d);Select();d.Health.GrantInvulnerability(2);Assert.That(p.TryAttack());yield return Wait(.5f);Assert.That(p.ConfirmedHitCount,Is.Zero);
            Place(d,new Vector3(0,.03f,3));Look(d);Select();Wall(new Vector3(0,1,.1f),new Vector3(4,4,.25f));
            Assert.That(p.TryAttack());yield return Wait(.5f);Assert.That(p.ConfirmedHitCount,Is.Zero);Assert.That(d.Health.CurrentHealth,Is.EqualTo(1000));
        }
        [UnityTest] public IEnumerator ProfileKoreanSerializedUndoResourceAndProtectedDefaults()
        {
            var asset=Resources.Load<ShibuyaLockOnProfile>("Shibuya Lock On");Assert.That(asset.IsValid);
            foreach(var field in typeof(ShibuyaLockOnProfile).GetFields(BindingFlags.Public|BindingFlags.Instance)) Assert.That(ShibuyaLockOnProfileInspector.Labels.ContainsKey(field.Name),field.Name);
            var so=new SerializedObject(settings);float old=settings.acquireRange;
            Undo.IncrementCurrentGroup();so.FindProperty("acquireRange").floatValue=45;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();
            Assert.That(settings.acquireRange,Is.EqualTo(45));Undo.PerformUndo();Assert.That(settings.acquireRange,Is.EqualTo(old));
            Assert.That(p.Profile.strikes[3].damage,Is.EqualTo(25));Assert.That(p.Health.MaxHealth,Is.EqualTo(100));Assert.That(d.Health.MaxHealth,Is.EqualTo(1000));
            settings.retainRange=10;Assert.That(settings.IsValid,Is.False);selection.Refresh(Time.unscaledTimeAsDouble);Assert.That(selection.LockedTarget,Is.Null);
            yield return null;
        }
        [UnityTest] public IEnumerator WallCornerAndMovingOccludedTargetDoNotSwitchToVisibleBystander()
        {
            Place(d,new Vector3(0,.03f,8));Look(d);Select();
            var wall=Wall(new Vector3(0,1,4),new Vector3(2,5,.4f));var other=Actor(new Vector3(5,.03f,6));
            yield return Wait(.15f);Assert.That(selection.IsOccluded);
            Place(d,new Vector3(4,.03f,8));yield return Wait(.15f);
            Assert.That(selection.LockedTarget,Is.EqualTo(d));Assert.That(selection.IsOccluded,Is.False);
            Place(d,new Vector3(0,.03f,8));
            yield return Until(()=>selection.IsOccluded);
            double detected=Time.unscaledTimeAsDouble;
            while(Time.unscaledTimeAsDouble-detected<1.6) {
                Metric("corner-grace",new{now=Time.unscaledTimeAsDouble,delta=Time.unscaledDeltaTime,occluded=selection.IsOccluded,
                    victim=new[]{d.transform.position.x,d.transform.position.y,d.transform.position.z},
                    camera=new[]{camera.transform.position.x,camera.transform.position.y,camera.transform.position.z},
                    since=typeof(ShibuyaLockOn).GetField("obscuredAt",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(selection)});
                yield return null;
            }
            Assert.That(selection.LockedTarget,Is.Null);Assert.That(other.Health.IsDead,Is.False);
        }
        [UnityTest] public IEnumerator ExplicitAimIsIndependentOfRecentHitRetentionAndSnapshotsPerEvent()
        {
            Assert.That(p.TryAttack());yield return Until(()=>p.ConfirmedHitCount>0);yield return Wait(.45f);
            var old=p.ComboTarget;Assert.That(old,Is.EqualTo(d));
            var combat=Object.Instantiate(p.Profile);combat.name="PASS4A QA Retention";combat.targetRetentionTime=0;
            p.Configure(combat,AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>("Assets/Shibuya/Data/Player Stats.asset"));
            var other=Actor(p.transform.position+new Vector3(.4f,0,1.4f));Look(other);Select();
            Assert.That(p.TryAttack());yield return Wait(.04f);
            Assert.That(ReadTarget(p,0),Is.EqualTo(other));Assert.That(p.ComboTarget,Is.EqualTo(old));
            Assert.That((bool[])typeof(ShibuyaFighter).GetField("aimStopped",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(p),Has.All.False);
            Look(d);Select();yield return Wait(.015f);Assert.That(ReadTarget(p,0),Is.EqualTo(other),"no mid-event target replacement");
        }
        [UnityTest] public IEnumerator LockedHighTargetCannotForceAirHitAndCameraRebindHasNoStaleAssist()
        {
            Place(d,new Vector3(0,12,8));Look(d);Select();float hp=d.Health.CurrentHealth;
            p.SetIntent(Vector3.zero,false,true);yield return Until(()=>p.transform.position.y>.7f);Assert.That(p.TryAttack());
            yield return Wait(.5f);Assert.That(p.ConfirmedHitCount,Is.Zero);Assert.That(d.Health.CurrentHealth,Is.EqualTo(hp));
            var other=Actor(new Vector3(4,.03f,0));orbit.Configure(other.transform,other.Profile);orbit.enabled=true;
            float yaw=Read(orbit,"yaw");yield return Wait(.2f);Assert.That(Read(orbit,"yaw"),Is.EqualTo(yaw));
        }
        private void Look(ShibuyaFighter target)
        {
            camera.transform.position=p.transform.position+new Vector3(0,1.2f,-5);
            camera.transform.rotation=Quaternion.LookRotation(target.GetComponent<CharacterController>().bounds.center-camera.transform.position);
        }
        private void Select() { selection.Clear();selection.HandleButton(true,Time.unscaledTimeAsDouble);selection.HandleButton(false,Time.unscaledTimeAsDouble); }
        private ShibuyaFighter Actor(Vector3 at) => Object.Instantiate(d.gameObject,at,Quaternion.identity).GetComponent<ShibuyaFighter>();
        private static void Place(ShibuyaFighter f,Vector3 at){var cc=f.GetComponent<CharacterController>();cc.enabled=false;f.transform.position=at;cc.enabled=true;Physics.SyncTransforms();}
        private static GameObject Wall(Vector3 at,Vector3 size){var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=at;wall.transform.localScale=size;Physics.SyncTransforms();return wall;}
        private static float Read(object obj,string field)=>(float)obj.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(obj);
        private static bool Read(object obj,string field,int i)=>((bool[])obj.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(obj))[i];
        private static ShibuyaFighter ReadTarget(object obj,int i)=>((ShibuyaFighter[])obj.GetType().GetField("eventAim",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(obj))[i];
        private static IEnumerator Wait(float t){double until=Time.unscaledTimeAsDouble+t;while(Time.unscaledTimeAsDouble<until)yield return null;}
        private static IEnumerator Until(Func<bool> ready){double end=Time.unscaledTimeAsDouble+6;while(!ready()&&Time.unscaledTimeAsDouble<end)yield return null;Assert.That(ready(),"PASS4A scenario timed out");}
        private static void Metric(string beat,object data){string path=ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass4AQA")+"/lock-measurements.jsonl";Directory.CreateDirectory(Path.GetDirectoryName(path));File.AppendAllText(path,Newtonsoft.Json.JsonConvert.SerializeObject(new{beat,data})+Environment.NewLine);}
    }
}
