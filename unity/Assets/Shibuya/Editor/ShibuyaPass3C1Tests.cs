using System;
using System.Collections;
using System.Collections.Generic;
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
    public sealed class ShibuyaPass3C1Tests
    {
        private ShibuyaFighter p,d;
        private ShibuyaInputFrame input;
        private readonly List<object> samples = new List<object>();
        private int steps,events,applied;
        private double began;
        private float hp;
        private const BindingFlags Private = BindingFlags.Instance|BindingFlags.NonPublic;
        private static string Root => ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass3C1QA");
        private static object Value(ShibuyaFighter f,string name) => typeof(ShibuyaFighter).GetField(name,Private).GetValue(f);

        [UnitySetUp] public IEnumerator Setup()
        {
            samples.Clear();steps=events=applied=0;input=default;began=0;hp=0;
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);
            yield return new EnterPlayMode();yield return null;
            var match=Object.FindFirstObjectByType<ShibuyaMatch>();p=match.Player;d=match.Dummy;
            var profile=Object.Instantiate(p.Profile);profile.name="PASS3C1 QA Profile";
            profile.aerialCombat=Object.Instantiate(profile.aerialCombat);profile.aerialCombat.name="PASS3C1 QA Air";
            p.Configure(profile,AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>("Assets/Shibuya/Data/Player Stats.asset"));
            p.GetComponent<ShibuyaPlayerInput>().TestInput=()=>input;
            Place(p,new Vector3(0,.03f,-1.4f));Place(d,new Vector3(0,.03f,0));p.transform.rotation=Quaternion.identity;
            yield return Wait(.1f);
        }
        [UnityTearDown] public IEnumerator Leave()
        {
            if(EditorApplication.isPlaying) {
                Save();yield return new ExitPlayMode();
            }
        }
        [TearDown] public void Cleanup()
        {
            foreach(var a in Resources.FindObjectsOfTypeAll<ScriptableObject>())
                if(a.name.StartsWith("PASS3C1 QA",StringComparison.Ordinal)&&!AssetDatabase.Contains(a))Object.DestroyImmediate(a);
        }
        private void Sample(string beat)
        {
            samples.Add(new {beat,time=Time.unscaledTimeAsDouble,dt=Time.unscaledDeltaTime,
                py=p.transform.position.y,dy=d.transform.position.y,pv=p.VerticalVelocity,dv=d.VerticalVelocity,
                hover=p.IsAerialHovering,held=d.IsHeldByAerialHit,grace=Value(p,"airGrace"),
                source=Value(d,"airHoldSource")!=null,lease=Value(p,"heldAirVictim")!=null,
                stunUntil=Value(d,"stunnedUntil"),dc=d.CombatTime,pc=p.CombatTime,
                deadline=Value(p,"airHoverDeadline"),elapsed=p.AttackElapsed,steps,events,applied,hp=d.Health.CurrentHealth});
        }
        private void Save()
        {
            Sample("final");Directory.CreateDirectory(Root);
            File.WriteAllText(Path.Combine(Root,TestContext.CurrentContext.Test.Name+".json"),
                Newtonsoft.Json.JsonConvert.SerializeObject(new{began,startHp=hp,steps,events,applied,samples},Newtonsoft.Json.Formatting.Indented));
        }
        private IEnumerator Pursuit()
        {
            Assert.That(p.TryAttack());
            for(int i=1;i<=3;i++) {
                yield return Until(()=>p.ComboStep==i&&p.AttackElapsed>=p.AttackDuration-.12f);
                Assert.That(p.TryAttack(ShibuyaFinisher.Uppercut));yield return Until(()=>p.ComboStep==i+1);
            }
            yield return Until(()=>p.ConfirmedHitCount==4);p.SetIntent(Vector3.zero,false,true);
            yield return Until(()=>p.CanAirAttack);Assert.That(p.AirChaseContactCount,Is.EqualTo(1));
        }
        private void Track()
        {
            began=Time.unscaledTimeAsDouble;hp=d.Health.CurrentHealth;
            p.AttackStarted+=_=>{steps++;Sample("step");};
            p.HitEventStarted+=_=>{events++;Sample("event");};
            d.Health.DamageResolved+=(h,c,r)=>{if(c.Source==p.gameObject&&r==DamageResolution.Applied){applied++;Sample("damage");}};
        }
        private IEnumerator Click() {input.attack=true;yield return null;input.attack=false;yield return null;}
        private IEnumerator FirstHit()
        {
            yield return Pursuit();Track();yield return Click();yield return Until(()=>applied==1);
            Assert.That(d.IsHeldByAerialHit);Sample("first-held");
        }
        private IEnumerator StableUntil(Func<bool> end)
        {
            float py=p.transform.position.y,dy=d.transform.position.y;double timeout=Time.unscaledTimeAsDouble+2;
            while(!end()&&Time.unscaledTimeAsDouble<timeout) {
                yield return null;if(end())break;Sample("height");
                Assert.That(p.transform.position.y,Is.EqualTo(py).Within(.001f));
                Assert.That(d.transform.position.y,Is.EqualTo(dy).Within(.001f));
            }
            Assert.That(end(),Is.True,"bounded height observation did not finish");
        }
        private void AssertReleased()
        {
            Assert.That(d.IsHeldByAerialHit,Is.False);
            Assert.That(Value(d,"airHoldSource"),Is.Null);Assert.That(Value(p,"heldAirVictim"),Is.Null);
        }

        [UnityTest] public IEnumerator FirstClickGapKeepsActualVictimHeight()
        {
            yield return FirstHit();yield return Until(()=>!p.IsAirAttacking);
            float py=p.transform.position.y,dy=d.transform.position.y;Sample("gap-start");
            yield return Wait(.06f);Sample("gap-end");
            Assert.That(d.transform.position.y,Is.EqualTo(dy).Within(.001f),"victim fell during valid inter-click grace");
            Assert.That(p.transform.position.y,Is.EqualTo(py).Within(.001f));Assert.That(d.IsHeldByAerialHit);
        }
        [UnityTest] public IEnumerator ThreeClicksPreserveFiveHitsAndBothLinkWindows()
        {
            yield return FirstHit();
            yield return StableUntil(()=>p.AttackElapsed>=p.AttackDuration-p.AerialDefinition.aerialLinkInputWindow+.015f);
            yield return Click();yield return Until(()=>steps==2);
            yield return StableUntil(()=>!p.IsAirAttacking);
            Assert.That(applied,Is.EqualTo(4));Assert.That(p.IsAerialHovering&&d.IsHeldByAerialHit);
            double next=Time.unscaledTimeAsDouble+.035;
            yield return StableUntil(()=>Time.unscaledTimeAsDouble>=next);
            yield return Click();yield return Until(()=>steps==3);
            yield return Until(()=>!p.IsAerialHovering);Sample("budget-end");
            Assert.That(events,Is.EqualTo(5));Assert.That(applied,Is.EqualTo(5));Assert.That(hp-d.Health.CurrentHealth,Is.EqualTo(62));
            Assert.That((double)Value(p,"airHoverDeadline")-began,Is.EqualTo(1.2).Within(.03));
            AssertReleased();float py=p.transform.position.y,dy=d.transform.position.y;
            yield return Wait(.1f);Assert.That(p.transform.position.y,Is.LessThan(py-.01f));Assert.That(d.transform.position.y,Is.LessThan(dy-.01f));
        }
        [UnityTest] public IEnumerator NoNextClickExpiresAndRestoresGravity()
        {
            yield return FirstHit();yield return Until(()=>!p.IsAirAttacking);
            Assert.That(p.IsAerialHovering&&d.IsHeldByAerialHit);
            float py=p.transform.position.y,dy=d.transform.position.y;
            yield return Until(()=>!p.IsAerialHovering);yield return null;AssertReleased();
            yield return Wait(.1f);Sample("fall-after-grace");
            Assert.That(p.transform.position.y,Is.LessThan(py-.01f));Assert.That(d.transform.position.y,Is.LessThan(dy-.01f));
            Assert.That(p.TryAttack(),Is.False);Assert.That(applied,Is.EqualTo(2));
        }
        [UnityTest] public IEnumerator ShortStunDoesNotBreakAirHeightOrCreateGroundStun()
        {
            foreach(var h in p.AerialDefinition.aerialAttack.hits){h.strike.hitStun=.025f;h.strike.hitStopDuration=0;}
            yield return FirstHit();
            yield return StableUntil(()=>!p.IsAirAttacking);
            Assert.That(d.CombatTime,Is.GreaterThan((float)Value(d,"stunnedUntil")));
            Assert.That(d.IsHeldByAerialHit);Assert.That(d.TryAttack(),Is.False);
            yield return Until(()=>!p.IsAerialHovering);yield return null;AssertReleased();
        }
        [UnityTest] public IEnumerator RangeExitReleasesWithoutPullingVictimBack()
        {
            yield return FirstHit();Vector3 at=d.transform.position+Vector3.right*5;Place(d,at);
            yield return null;yield return null;AssertReleased();float y=d.transform.position.y;
            yield return Wait(.08f);Assert.That(d.transform.position.x,Is.EqualTo(at.x).Within(.03f));Assert.That(d.transform.position.y,Is.LessThan(y-.01f));
        }
        [UnityTest] public IEnumerator HeightExitAndLandingClearTheLease()
        {
            yield return FirstHit();Place(d,new Vector3(d.transform.position.x,.03f,d.transform.position.z));
            yield return null;yield return null;AssertReleased();
            // The applied hit still owns its original local hit-stop. Let physics resume
            // and cover the .03m floor gap before demanding a grounded controller.
            yield return Until(()=>!d.IsInHitStop&&d.IsGrounded);Sample("landed-after-release");
            Assert.That(d.transform.position.y,Is.GreaterThan(-.1f));Assert.That(d.IsGrounded);
        }
        [UnityTest] public IEnumerator WallBetweenParticipantsReleasesHoldAndBlocksDamage()
        {
            yield return FirstHit();float hpNow=d.Health.CurrentHealth;
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=(p.transform.position+d.transform.position)*.5f+Vector3.up;
            wall.transform.localScale=new Vector3(4,3,.15f);Physics.SyncTransforms();
            yield return null;yield return null;AssertReleased();
            yield return Until(()=>!p.IsAirAttacking);Assert.That(d.Health.CurrentHealth,Is.EqualTo(hpNow));
            Object.Destroy(wall);
        }
        [UnityTest] public IEnumerator ThirdPartyDamageRevokesPreviousOwnership()
        {
            yield return FirstHit();var third=new GameObject("PASS3C1 third party");
            Assert.That(d.Health.ReceiveDamage(new DamageContext(1,third,DamageDeliveryType.PhysicalStrike)),Is.EqualTo(DamageResolution.Applied));
            AssertReleased();Object.Destroy(third);
            yield return null;Sample("third-party-revoked");
        }
        [UnityTest] public IEnumerator HoverBudgetStillExpiresDuringHitStopAndLongFrame()
        {
            yield return FirstHit();double deadline=(double)Value(p,"airHoverDeadline");
            while(Time.unscaledTimeAsDouble<deadline-.15) {
                p.TryAttack();yield return null;
                Assert.That((double)Value(p,"airHoverDeadline"),Is.EqualTo(deadline));
            }
            var pause=typeof(ShibuyaFighter).GetMethod("ApplyLocalHitStop",Private);
            pause.Invoke(p,new object[]{.3f});pause.Invoke(d,new object[]{.3f});
            System.Threading.Thread.Sleep(180);yield return null;yield return null;Sample("long-frame-expiry");
            Assert.That(p.IsAerialHovering,Is.False);AssertReleased();Assert.That(p.TryAttack(),Is.False);
            Assert.That(Time.timeScale,Is.EqualTo(1));
            yield return Wait(.25f);float y=d.transform.position.y;yield return Wait(.08f);Assert.That(d.transform.position.y,Is.LessThan(y-.01f));
        }
        private static void Place(ShibuyaFighter f,Vector3 at){var cc=f.GetComponent<CharacterController>();cc.enabled=false;f.transform.position=at;cc.enabled=true;Physics.SyncTransforms();}
        private static IEnumerator Wait(float seconds){double until=Time.unscaledTimeAsDouble+seconds;while(Time.unscaledTimeAsDouble<until)yield return null;}
        private static IEnumerator Until(Func<bool> condition){double until=Time.unscaledTimeAsDouble+6;while(!condition()&&Time.unscaledTimeAsDouble<until)yield return null;Assert.That(condition(),"PASS3C1 condition timed out");}
    }
}
