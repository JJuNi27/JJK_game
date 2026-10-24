using System;
using System.Collections;
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
    // Small input-to-hit checks. No physics/reflection/file IO in live hit callbacks.
    public sealed class ShibuyaPass4A1Tests
    {
        private ShibuyaFighter p, d;
        private ShibuyaOrbitCamera orbit;
        private Camera view;
        private ShibuyaLockOn selection;
        private ShibuyaLockOnProfile settings;
        private ShibuyaInputFrame frame;
        [UnitySetUp] public IEnumerator Setup()
        {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);
            yield return new EnterPlayMode();
            Application.runInBackground = true;
            yield return null;
            var match = Object.FindFirstObjectByType<ShibuyaMatch>(); p = match.Player; d = match.Dummy;
            orbit = Array.Find(Object.FindObjectsByType<ShibuyaOrbitCamera>(FindObjectsSortMode.None), c => c.LocalOwner == p);
            orbit.enabled = false; view = orbit.GetComponent<Camera>();
            p.GetComponent<ShibuyaPlayerInput>().TestInput = () => frame;
            frame = default; selection = p.GetComponent<ShibuyaPlayerInput>().LockOn;
            settings = Object.Instantiate(selection.Settings); settings.name = "PASS4A1 QA Settings";
            selection.Configure(p, view, settings);
            Place(p, new Vector3(0,.03f,-1.4f)); Place(d, new Vector3(0,.03f,0));
            p.transform.rotation = Quaternion.identity;
            yield return null;
        }
        [UnityTearDown] public IEnumerator Leave() { if (EditorApplication.isPlaying) yield return new ExitPlayMode(); }
        [TearDown] public void Cleanup()
        {
            foreach (var obj in Resources.FindObjectsOfTypeAll<ScriptableObject>())
                if (obj.name.StartsWith("PASS4A1 QA", StringComparison.Ordinal) && !AssetDatabase.Contains(obj)) Object.DestroyImmediate(obj);
        }
        [UnityTest] public IEnumerator LockedFrontRealClick() { yield return Heading(0,0); }
        [UnityTest] public IEnumerator LockedCameraAndBodyNinetyDegrees() { yield return Heading(90,90); }
        [UnityTest] public IEnumerator LockedCameraAndBodyOpposite() { yield return Heading(180,180); }
        private IEnumerator Heading(float body, float camera)
        {
            Select(d); p.transform.rotation = Quaternion.Euler(0,body,0);
            view.transform.rotation = Quaternion.Euler(0,camera,0);
            yield return Click(); yield return Finished();
            Assert.That(p.AcceptedAttackCount,Is.EqualTo(1)); Assert.That(p.ConfirmedHitCount,Is.EqualTo(1));
            Assert.That(d.Health.CurrentHealth,Is.EqualTo(980));
            Assert.That(Vector3.Angle(p.transform.forward,Vector3.forward),Is.LessThan(10));
            Assert.That(selection.LockedTarget,Is.EqualTo(d));
            Assert.That(p.TrackingTurnApplied,Is.LessThanOrEqualTo(settings.attackMaxTurnAngle+.01f));
        }
        [UnityTest] public IEnumerator SlowTurnDoesNotHitBehindOrExtendStartup()
        {
            Select(d); p.transform.rotation=Quaternion.Euler(0,180,0); settings.attackTurnSpeed=10;
            float startup=p.Profile.strikes[0].Startup;
            yield return Click(); yield return Finished();
            Assert.That(p.ConfirmedHitCount,Is.Zero); Assert.That(d.Health.CurrentHealth,Is.EqualTo(1000));
            Assert.That(p.TrackingTurnApplied,Is.LessThanOrEqualTo(startup*10+.01f));
            Assert.That(p.Profile.strikes[0].Startup,Is.EqualTo(startup));
        }
        [UnityTest] public IEnumerator FarLockTurnsButCannotReach()
        {
            Place(d,new Vector3(0,.03f,12)); Select(d);p.transform.rotation=Quaternion.Euler(0,180,0);
            yield return Click();yield return Finished();
            Assert.That(Vector3.Angle(p.transform.forward,Vector3.forward),Is.LessThan(10));
            Assert.That(p.ConfirmedHitCount,Is.Zero); Assert.That(d.Health.CurrentHealth,Is.EqualTo(1000));
            Assert.That(p.TrackingAdvanceRequested,Is.Zero);
        }
        [UnityTest] public IEnumerator WallAndImmunityStillBlockActualDamage()
        {
            Select(d);
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=new Vector3(0,1,-.7f);
            wall.transform.localScale=new Vector3(4,4,.15f);Physics.SyncTransforms();
            yield return Click();Assert.That(Target(0),Is.EqualTo(d));
            yield return Finished();Assert.That(p.ConfirmedHitCount,Is.Zero);
            Object.Destroy(wall);yield return null;d.Health.GrantInvulnerability(2);
            yield return Click();yield return Finished();Assert.That(p.ConfirmedHitCount,Is.Zero);
            Assert.That(d.Health.CurrentHealth,Is.EqualTo(1000));
        }
        [UnityTest] public IEnumerator NoLockAndUnlockPreserveSoftMelee()
        {
            Select(d);selection.Clear();yield return Click();yield return Finished();
            Assert.That(p.ConfirmedHitCount,Is.EqualTo(1)); Assert.That(p.ComboTarget,Is.EqualTo(d));
            Assert.That(Target(0),Is.EqualTo(d)); Assert.That(Explicit(0),Is.False);
        }
        [UnityTest] public IEnumerator ReselectionCannotHijackAcceptedEventButNextAttackUsesNewLock()
        {
            var other=Object.Instantiate(d.gameObject,new Vector3(1.4f,.03f,-1.4f),Quaternion.identity).GetComponent<ShibuyaFighter>();
            Select(d); Assert.That(p.TryAttack()); Select(other);
            Assert.That(Target(0),Is.EqualTo(d),"snapshot at input acceptance");
            yield return Finished(); Assert.That(p.ConfirmedHitCount,Is.EqualTo(1));
            Assert.That(other.Health.CurrentHealth,Is.EqualTo(1000));
            Assert.That(p.TryAttack());Assert.That(Target(0),Is.EqualTo(other));
            yield return Finished(); Assert.That(other.Health.CurrentHealth,Is.EqualTo(980));
        }
        [UnityTest] public IEnumerator OrdinaryJumpAirAttackUsesLockedHeadingWithoutRootPitch()
        {
            Select(d); p.SetIntent(Vector3.zero,false,true);d.SetIntent(Vector3.zero,false,true);
            yield return Until(()=>p.transform.position.y>.7f && d.transform.position.y>.7f);
            p.transform.rotation=Quaternion.Euler(0,180,0);view.transform.rotation=Quaternion.Euler(0,90,0);
            Assert.That(p.TryAttack()); Assert.That(p.IsAirAttacking);Assert.That(Target(0),Is.EqualTo(d));Assert.That(Explicit(0));
            yield return Until(()=>p.ConfirmedHitCount>0);
            Assert.That(p.transform.eulerAngles.x,Is.EqualTo(0).Within(.01f));
            Assert.That(p.transform.eulerAngles.z,Is.EqualTo(0).Within(.01f));
        }
        [UnityTest] public IEnumerator EstablishedAirContactOutranksNewExplicitLock()
        {
            p.SetIntent(Vector3.zero,false,true);d.SetIntent(Vector3.zero,false,true);
            yield return Until(()=>p.transform.position.y>.7f && d.transform.position.y>.7f);
            Assert.That(p.TryAttack());yield return Until(()=>p.ConfirmedHitCount>0);
            var other=Object.Instantiate(d.gameObject,d.transform.position+Vector3.right*2,Quaternion.identity).GetComponent<ShibuyaFighter>();
            Select(other);
            // Inspect a later event's real startup, not a replacement attack engine.
            var step=(ShibuyaMovesetStep)Field("activeStep").GetValue(p);
            Assert.That(step.hits.Length,Is.EqualTo(2));
            yield return Until(()=>((bool[])Field("aimStarted").GetValue(p))[1]);
            Assert.That(Target(1),Is.EqualTo(d));Assert.That(Explicit(1),Is.False);
            Assert.That(selection.LockedTarget,Is.EqualTo(other));
        }
        [UnityTest] public IEnumerator AimUsesActualOriginXYZCameraIndependentSnapshotAndOptOut()
        {
            Place(d,new Vector3(0,8,8));Select(d);view.transform.rotation=Quaternion.Euler(0,180,0);
            var origin=p.transform.position+new Vector3(.4f,1.7f,.2f);var rotation=p.transform.rotation;
            IShibuyaAimSource source=selection;var shot=source.GetAim(origin);
            Assert.That(shot.Target,Is.EqualTo(d));Assert.That(shot.Origin,Is.EqualTo(origin));
            Assert.That(Vector3.Angle(shot.Direction,selection.TargetPoint-origin),Is.LessThan(.01f));
            Assert.That(shot.Direction.y,Is.GreaterThan(.4f));var direction=shot.Direction;
            Place(d,new Vector3(5,10,8));Assert.That(shot.Direction,Is.EqualTo(direction));
            Assert.That(source.GetAim(origin,false).Target,Is.Null);
            Assert.That(Vector3.Angle(source.GetAim(origin,false).Direction,view.transform.forward),Is.LessThan(.01f));
            Assert.That(p.transform.rotation,Is.EqualTo(rotation));yield return null;
        }
        [UnityTest] public IEnumerator SettingsHaveKoreanSerializedUndoAndProtectedData()
        {
            foreach(var field in typeof(ShibuyaLockOnProfile).GetFields(BindingFlags.Public|BindingFlags.Instance))
                Assert.That(ShibuyaLockOnProfileInspector.Labels.ContainsKey(field.Name),field.Name);
            var so=new SerializedObject(settings);float old=settings.attackTurnSpeed;
            Undo.IncrementCurrentGroup();so.FindProperty("attackTurnSpeed").floatValue=900;so.ApplyModifiedProperties();
            Undo.FlushUndoRecordObjects();Assert.That(settings.attackTurnSpeed,Is.EqualTo(900));Undo.PerformUndo();Assert.That(settings.attackTurnSpeed,Is.EqualTo(old));
            CollectionAssert.AreEqual(new[]{20f,20f,20f,25f},Array.ConvertAll(p.Profile.strikes,x=>x.damage));
            Assert.That(p.Health.MaxHealth,Is.EqualTo(100));Assert.That(d.Health.MaxHealth,Is.EqualTo(1000));
            Assert.That(Time.timeScale,Is.EqualTo(1));yield return null;
        }
        [UnityTest] public IEnumerator LockedDefaultFourClicksPreserveDamage() { yield return LockedFour(0); }
        [UnityTest] public IEnumerator LockedWHeldUsesUppercut() { yield return LockedFour(1); }
        [UnityTest] public IEnumerator LockedSHeldUsesDown() { yield return LockedFour(-1); }
        private IEnumerator LockedFour(int command)
        {
            Select(d);p.transform.rotation=Quaternion.Euler(0,180,0);view.transform.rotation=Quaternion.Euler(0,180,0);
            frame.forwardKey=command>0;frame.backwardKey=command<0;
            ShibuyaFinisher applied=ShibuyaFinisher.Default;
            p.HitConfirmed+=(point,strength)=> { if(p.ConfirmedHitCount==4) applied=p.CurrentFinisher; };
            yield return Click();
            for(int i=1;i<=3;i++) {
                yield return Until(()=>p.AcceptedAttackCount==i && p.AttackElapsed>=p.AttackDuration-.12f);
                yield return Click();yield return Until(()=>p.AcceptedAttackCount==i+1);
            }
            yield return Until(()=>p.ConfirmedHitCount==4);
            Assert.That(p.AcceptedAttackCount,Is.EqualTo(4));Assert.That(d.Health.CurrentHealth,Is.EqualTo(915));
            Assert.That(applied,Is.EqualTo(command>0?ShibuyaFinisher.Uppercut:command<0?ShibuyaFinisher.Down:ShibuyaFinisher.Default));
            if(command>0) Assert.That(d.IsAirborne);else if(command<0) Assert.That(d.IsDowned);
        }
        [UnityTest] public IEnumerator PursuitAndFirstAirEventCannotBeHijackedByAnotherLock()
        {
            Assert.That(p.TryAttack());
            for(int i=1;i<=3;i++) {
                yield return Until(()=>p.AcceptedAttackCount==i && p.AttackElapsed>=p.AttackDuration-.12f);
                Assert.That(p.TryAttack(ShibuyaFinisher.Uppercut));yield return Until(()=>p.AcceptedAttackCount==i+1);
            }
            yield return Until(()=>p.ConfirmedHitCount==4);
            var other=Object.Instantiate(d.gameObject,p.transform.position+new Vector3(4,0,2),Quaternion.identity).GetComponent<ShibuyaFighter>();
            Select(other);p.SetIntent(Vector3.zero,false,true);
            yield return Until(()=>p.CanAirAttack);
            Assert.That(p.AirChaseTarget,Is.EqualTo(d));Assert.That(p.AirChaseContactCount,Is.EqualTo(1));
            Assert.That(p.TryAttack());Assert.That(Target(0),Is.EqualTo(d));Assert.That(Explicit(0),Is.False);
            Assert.That(selection.LockedTarget,Is.EqualTo(other));
            yield return Until(()=>p.ConfirmedHitCount>4);Assert.That(other.Health.CurrentHealth,Is.EqualTo(1000));
        }
        private void Select(ShibuyaFighter target)
        {
            view.transform.position=p.transform.position+new Vector3(0,1.2f,-5);
            view.transform.rotation=Quaternion.LookRotation(target.GetComponent<CharacterController>().bounds.center-view.transform.position);
            selection.Clear();selection.HandleButton(true,Time.unscaledTimeAsDouble);selection.HandleButton(false,Time.unscaledTimeAsDouble);
            Assert.That(selection.LockedTarget,Is.EqualTo(target));
        }
        private IEnumerator Click(){frame.attack=true;yield return null;frame.attack=false;yield return null;}
        private IEnumerator Finished(){yield return Until(()=>p.ComboStep==0);}
        private static IEnumerator Until(Func<bool> ready)
        {double end=Time.unscaledTimeAsDouble+3;while(!ready()&&Time.unscaledTimeAsDouble<end)yield return null;Assert.That(ready(),"PASS4A1 scenario timed out");}
        private static FieldInfo Field(string name)=>typeof(ShibuyaFighter).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic);
        private ShibuyaFighter Target(int i)=>((ShibuyaFighter[])Field("eventAim").GetValue(p))[i];
        private bool Explicit(int i)=>((bool[])Field("aimIsExplicit").GetValue(p))[i];
        private static void Place(ShibuyaFighter actor,Vector3 position)
        {var cc=actor.GetComponent<CharacterController>();cc.enabled=false;actor.transform.position=position;cc.enabled=true;Physics.SyncTransforms();}
    }
}
