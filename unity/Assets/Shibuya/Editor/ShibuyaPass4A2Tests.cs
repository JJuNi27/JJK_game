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
    public sealed class ShibuyaPass4A2Tests
    {
        private ShibuyaFighter p,d;
        private ShibuyaOrbitCamera orbit;
        private Camera view;
        private ShibuyaLockOn selection;
        private ShibuyaLockOnProfile settings;
        private ShibuyaInputFrame input;
        private static string Root => ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass4A2QA");
        [UnitySetUp] public IEnumerator Setup()
        {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);yield return new EnterPlayMode();
            Application.runInBackground=true;yield return null;
            Directory.CreateDirectory(Root);
            var match=Object.FindFirstObjectByType<ShibuyaMatch>();p=match.Player;d=match.Dummy;
            orbit=Array.Find(Object.FindObjectsByType<ShibuyaOrbitCamera>(FindObjectsSortMode.None),x=>x.LocalOwner==p);
            view=orbit.GetComponent<Camera>();selection=p.GetComponent<ShibuyaPlayerInput>().LockOn;
            settings=Object.Instantiate(selection.Settings);settings.name="PASS4A2 QA Settings";selection.Configure(p,view,settings);
            input=default;p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>input;
            Place(p,new Vector3(0,.03f,0));Place(d,new Vector3(0,.03f,1.4f));
            p.transform.rotation=Quaternion.identity;Set("yaw",0f);Set("pitch",16f);yield return Wait(.1f);
        }
        [UnityTearDown] public IEnumerator Leave(){if(EditorApplication.isPlaying)yield return new ExitPlayMode();}
        [TearDown] public void Cleanup()
        {
            foreach(var a in Resources.FindObjectsOfTypeAll<ScriptableObject>())
                if(a.name.StartsWith("PASS4A2 QA",StringComparison.Ordinal)&&!AssetDatabase.Contains(a))Object.DestroyImmediate(a);
        }
        [UnityTest] public IEnumerator ClosePairActuallyReducesProjectedOverlapAndKeepsScale()
        {
            float before=Overlap();ScreenCapture.CaptureScreenshot(Path.Combine(Root,"overlap-before.png"));
            Select(d);yield return Wait(1.3f);float after=Overlap();
            ScreenCapture.CaptureScreenshot(Path.Combine(Root,"overlap-after.png"));
            Record("overlap",new{before,after,yaw=Read("yaw"),shoulder=Read("lockShoulder"),distance=Read("lockDistance")});
            Assert.That(before,Is.GreaterThan(.3f));Assert.That(after,Is.LessThan(before*.75f));
            Assert.That(Read("lockDistance"),Is.InRange(0,settings.cameraExtraDistance));
            Assert.That(Read("lockShoulder"),Is.GreaterThan(0));Assert.That(view.fieldOfView,Is.EqualTo(60));
        }
        [UnityTest] public IEnumerator ManualLookHasGraceAndSearchDoesNotRecenter()
        {
            // At 1.4m an orbit can still see the opponent even after 180 degrees. Use a
            // genuinely off-screen opponent, rather than demanding the removed recenter rule.
            Place(d,new Vector3(0,.03f,15));
            Select(d);orbit.ApplyLook(new Vector2(70,0));float yaw=Read("yaw");
            yield return Wait(.45f);Assert.That(Read("yaw"),Is.EqualTo(yaw).Within(.001f));
            var offscreen=view.WorldToViewportPoint(selection.TargetPoint);
            Record("manual-outside",new{yaw,point=new[]{offscreen.x,offscreen.y,offscreen.z}});
            Assert.That(offscreen.z,Is.LessThan(0),"automatic recovery is only expected when the opponent is actually outside the view");
            yield return Wait(.5f);Assert.That(Mathf.Abs(Mathf.DeltaAngle(yaw,Read("yaw"))),Is.GreaterThan(.01f));
            input.lockHeld=true;yield return null;orbit.ApplyLook(new Vector2(-8,2));yaw=Read("yaw");
            yield return Wait(.9f);Assert.That(Read("yaw"),Is.EqualTo(yaw).Within(.001f));
            Assert.That(selection.IsSearching);input.lockHeld=false;selection.CancelSearch();yield return null;
        }
        [UnityTest] public IEnumerator VisibleDeadZoneDoesNotPullTargetToCenter()
        {
            settings.cameraOverlapStrength=0;settings.cameraVerticalFocusWeight=0;
            Place(d,new Vector3(3,.03f,8));Select(d);yield return Wait(.1f);
            Vector3 screen=view.WorldToViewportPoint(selection.TargetPoint);float yaw=Read("yaw"),pitch=Read("pitch");
            Assert.That(screen.x,Is.InRange(.5f-settings.cameraDeadZone.x,.5f+settings.cameraDeadZone.x));
            Assert.That(screen.y,Is.InRange(.5f-settings.cameraDeadZone.y,.5f+settings.cameraDeadZone.y));
            yield return Wait(.7f);Assert.That(Read("yaw"),Is.EqualTo(yaw).Within(.001f));Assert.That(Read("pitch"),Is.EqualTo(pitch).Within(.001f));
        }
        [UnityTest] public IEnumerator EdgeTrackingAcceleratesWithBoundedFrameSteps()
        {
            settings.cameraOverlapStrength=0;Place(d,new Vector3(14,.03f,3));Select(d);
            float yaw=Read("yaw"),before=yaw,maxStep=0;double end=Time.unscaledTimeAsDouble+.7;
            while(Time.unscaledTimeAsDouble<end){yield return null;float now=Read("yaw");maxStep=Mathf.Max(maxStep,Mathf.Abs(Mathf.DeltaAngle(yaw,now)));yaw=now;}
            Record("edge",new{before,after=yaw,maxStep});Assert.That(Mathf.DeltaAngle(before,yaw),Is.GreaterThan(1));
            Assert.That(maxStep,Is.LessThanOrEqualTo(settings.cameraDegreesPerSecond*.05f+.001f));
        }
        [UnityTest] public IEnumerator ShoulderSideDoesNotFlipWhenTargetCrossesCenter()
        {
            Select(d);yield return Wait(.5f);int side=ReadInt("lockShoulderSide");
            for(int i=0;i<6;i++) {Place(d,new Vector3((i%2==0?-.2f:.2f),.03f,1.4f));yield return Wait(.12f);Assert.That(ReadInt("lockShoulderSide"),Is.EqualTo(side));Assert.That(Read("lockShoulder")*side,Is.GreaterThanOrEqualTo(0));}
        }
        [UnityTest] public IEnumerator AirHeightTracksWithinPitchAndFocusLimits()
        {
            settings.cameraOverlapStrength=0;Place(d,new Vector3(0,9,6));Select(d);float before=Read("pitch");
            yield return Wait(.5f);float up=Read("pitch");
            Assert.That(up,Is.LessThan(before));Assert.That(Mathf.Abs(Read("lockHeight")),Is.LessThanOrEqualTo(settings.cameraVerticalFocusLimit));
            // A ground target at 1.5m remained inside the vertical dead zone in the first
            // run. A lower target must actually leave that zone before downward correction.
            Place(d,new Vector3(0,-7,6));var below=view.WorldToViewportPoint(selection.TargetPoint);
            Record("lower-outside",new{pitch=up,point=new[]{below.x,below.y,below.z}});
            Assert.That(below.y,Is.LessThan(.5f-settings.cameraDeadZone.y));
            yield return Wait(.8f);
            Assert.That(Read("pitch"),Is.GreaterThan(up));Assert.That(Read("pitch"),Is.InRange(p.Profile.minimumPitch,p.Profile.maximumPitch));
            Record("height",new{before,up,down=Read("pitch"),focusHeight=Read("lockHeight")});
        }
        [UnityTest] public IEnumerator DistanceIsBoundedAndFarTargetDoesNotCauseHugeZoomOut()
        {
            Select(d);yield return Wait(.5f);float close=Read("lockDistance");Assert.That(close,Is.GreaterThan(0));
            Place(d,new Vector3(0,.03f,45));yield return Wait(.9f);
            Assert.That(Read("lockDistance"),Is.LessThan(close));Assert.That(Read("lockDistance"),Is.InRange(0,settings.cameraExtraDistance));
            Assert.That(Vector3.Distance(view.transform.position,p.transform.position),Is.LessThan(p.Profile.cameraDistance+settings.cameraExtraDistance+settings.cameraShoulderOffset+3));
            Assert.That(settings.acquireRange,Is.EqualTo(50));Assert.That(settings.retainRange,Is.EqualTo(60));
        }
        [UnityTest] public IEnumerator WallColumnCeilingClipCameraAndRecoveryIsGradual()
        {
            Select(d);yield return Wait(.6f);
            Vector3 anchor=p.transform.position+Vector3.up*p.Profile.cameraHeight;
            var wall=Wall(Vector3.Lerp(anchor,view.transform.position,.65f),new Vector3(8,5,.4f));
            yield return Wait(.15f);AssertCameraClear();float blocked=Vector3.Distance(view.transform.position,anchor);
            Object.Destroy(wall);yield return null;float first=Vector3.Distance(view.transform.position,anchor);
            Assert.That(first-blocked,Is.LessThan(.6f));yield return Wait(.6f);
            Assert.That(Vector3.Distance(view.transform.position,anchor),Is.GreaterThan(first));
            var column=Wall(view.transform.position+view.transform.right*.6f,new Vector3(.5f,5,.5f));
            orbit.ApplyLook(new Vector2(4,0));yield return Wait(.2f);AssertCameraClear();Object.Destroy(column);
            var ceiling=Wall(anchor+Vector3.up*2,new Vector3(12,.3f,12));orbit.ApplyLook(new Vector2(0,25));
            yield return Wait(.2f);AssertCameraClear();Object.Destroy(ceiling);
        }
        [UnityTest] public IEnumerator UnlockRelaxesOffsetsAndFreeCameraDoesNotDrift()
        {
            Select(d);yield return Wait(.8f);selection.Clear();float yaw=Read("yaw");
            yield return Wait(1.2f);Assert.That(Read("lockShoulder"),Is.EqualTo(0).Within(.001f));Assert.That(Read("lockDistance"),Is.EqualTo(0).Within(.001f));
            Assert.That(Read("yaw"),Is.EqualTo(yaw).Within(.001f));orbit.ApplyLook(new Vector2(7,0));Assert.That(Read("yaw"),Is.Not.EqualTo(yaw));
        }
        [UnityTest] public IEnumerator CinematicOwnsViewAndReturnsToFramingWithoutJump()
        {
            Select(d);yield return Wait(.8f);float yaw=Read("yaw"),pitch=Read("pitch"),offset=Read("lockShoulder"),fov=view.fieldOfView;
            var film=Object.Instantiate(AssetDatabase.LoadAssetAtPath<ShibuyaCinematicProfile>(ShibuyaCinematicExample.Path));film.name="PASS4A2 QA Film";film.cinematicEnabled=true;
            var session=ShibuyaCinematicSession.TryStartAfterApplied(p,d,film,DamageResolution.Applied,ShibuyaCinematicTrigger.Default,7001,0);Assert.That(session,Is.Not.Null);
            yield return Wait(.1f);orbit.ApplyLook(new Vector2(25,25));
            Assert.That(Read("yaw"),Is.EqualTo(yaw));Assert.That(Read("pitch"),Is.EqualTo(pitch));Assert.That(Read("lockShoulder"),Is.EqualTo(offset));
            session.Finish("PASS4A2 check");yield return Until(()=>!orbit.IsReturningView);
            Vector3 returned=view.transform.position;Quaternion rotation=view.transform.rotation;
            yield return null;
            Record("cinematic-return",new{jump=Vector3.Distance(returned,view.transform.position),angle=Quaternion.Angle(rotation,view.transform.rotation)});
            Assert.That(Vector3.Distance(returned,view.transform.position),Is.LessThan(.03f));Assert.That(Quaternion.Angle(rotation,view.transform.rotation),Is.LessThan(.1f));
            Assert.That(view.fieldOfView,Is.EqualTo(fov));Assert.That(Read("yaw"),Is.EqualTo(yaw));
            Assert.That(Time.timeScale,Is.EqualTo(1));
        }
        [UnityTest] public IEnumerator ProfileKoreanUndoAndAttackTurnSettingsUnchanged()
        {
            foreach(var f in typeof(ShibuyaLockOnProfile).GetFields(BindingFlags.Instance|BindingFlags.Public))Assert.That(ShibuyaLockOnProfileInspector.Labels.ContainsKey(f.Name),f.Name);
            var so=new SerializedObject(settings);float before=settings.cameraShoulderOffset;
            Undo.IncrementCurrentGroup();so.FindProperty("cameraShoulderOffset").floatValue=1.1f;so.ApplyModifiedProperties();Undo.FlushUndoRecordObjects();
            Assert.That(settings.cameraShoulderOffset,Is.EqualTo(1.1f));Undo.PerformUndo();Assert.That(settings.cameraShoulderOffset,Is.EqualTo(before));
            Assert.That(settings.attackTurnSpeed,Is.EqualTo(2400));Assert.That(settings.attackMaxTurnAngle,Is.EqualTo(180));Assert.That(settings.attackPreparationTime,Is.EqualTo(.12f));
            settings.cameraDeadZone.x=float.NaN;Assert.That(settings.IsValid,Is.False);yield return null;
        }
        private void Select(ShibuyaFighter target)
        {
            orbit.enabled=false;view.transform.rotation=Quaternion.LookRotation(target.GetComponent<CharacterController>().bounds.center-view.transform.position);
            selection.Clear();selection.HandleButton(true,Time.unscaledTimeAsDouble);selection.HandleButton(false,Time.unscaledTimeAsDouble);
            Assert.That(selection.LockedTarget,Is.EqualTo(target));orbit.enabled=true;
        }
        private float Overlap()
        {
            Rect a=ScreenRect(p),b=ScreenRect(d);float w=Mathf.Max(0,Mathf.Min(a.xMax,b.xMax)-Mathf.Max(a.xMin,b.xMin)),h=Mathf.Max(0,Mathf.Min(a.yMax,b.yMax)-Mathf.Max(a.yMin,b.yMin));
            return w*h/Mathf.Max(.000001f,Mathf.Min(a.width*a.height,b.width*b.height));
        }
        private Rect ScreenRect(ShibuyaFighter actor)
        {
            var b=actor.GetComponent<CharacterController>().bounds;Vector2 min=Vector2.one*float.PositiveInfinity,max=Vector2.one*float.NegativeInfinity;
            for(int i=0;i<8;i++){var v=view.WorldToViewportPoint(b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));min=Vector2.Min(min,v);max=Vector2.Max(max,v);}
            return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
        }
        private void AssertCameraClear()
        {foreach(var c in Physics.OverlapSphere(view.transform.position,p.Profile.cameraCollisionRadius*.95f,~0,QueryTriggerInteraction.Ignore))Assert.That(c.GetComponentInParent<ShibuyaFighter>(),Is.Not.Null,c.name);}
        private static GameObject Wall(Vector3 at,Vector3 scale){var w=GameObject.CreatePrimitive(PrimitiveType.Cube);w.transform.position=at;w.transform.localScale=scale;Physics.SyncTransforms();return w;}
        private static void Place(ShibuyaFighter f,Vector3 at){var c=f.GetComponent<CharacterController>();c.enabled=false;f.transform.position=at;c.enabled=true;Physics.SyncTransforms();}
        private static FieldInfo Field(string n)=>typeof(ShibuyaOrbitCamera).GetField(n,BindingFlags.NonPublic|BindingFlags.Instance);
        private void Set(string n,float v)=>Field(n).SetValue(orbit,v);
        private float Read(string n)=>(float)Field(n).GetValue(orbit);
        private int ReadInt(string n)=>(int)Field(n).GetValue(orbit);
        private static IEnumerator Wait(float seconds){double end=Time.unscaledTimeAsDouble+seconds;while(Time.unscaledTimeAsDouble<end)yield return null;}
        private static IEnumerator Until(Func<bool> ready){double end=Time.unscaledTimeAsDouble+4;while(!ready()&&Time.unscaledTimeAsDouble<end)yield return null;Assert.That(ready(),"PASS4A2 condition timed out");}
        private static void Record(string beat,object data){Directory.CreateDirectory(Root);File.AppendAllText(Path.Combine(Root,"camera-metrics.jsonl"),Newtonsoft.Json.JsonConvert.SerializeObject(new{beat,data})+Environment.NewLine);}
    }
}
