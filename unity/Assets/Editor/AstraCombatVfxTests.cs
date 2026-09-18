using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JJKGame.Core;
using JJKGame.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace JJKGame.EditorTools
{
    public sealed class AstraCombatVfxTests
    {
        private const string Output = "D:/JJK_game/unity/Logs/AstraCombatQA";
        private static object Call(object instance, string name, params object[] args) =>
            instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance, args);
        private static IEnumerator Wait(float seconds)
        {
            float end = Time.time + seconds;
            double deadline = EditorApplication.timeSinceStartup + 30;
            while (Time.time < end)
            {
                Assert.That(EditorApplication.timeSinceStartup, Is.LessThan(deadline), "Clock stalled");
                yield return null;
            }
        }
        private static GameObject Target(string name, Vector3 position)
        {
            var target = new GameObject(name, typeof(Health), typeof(BoxCollider));
            target.transform.position = position;
            return target;
        }
        private static void Runtime()
        {
            var scene = SceneManager.CreateScene("VFXLab");
            SceneManager.SetActiveScene(scene);
            new GameObject("ProductionParticleVfxRuntime", typeof(ProductionParticleVfxRuntime));
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.timeScale = 1f;
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
        }

        [UnityTest] public IEnumerator PurpleHoldReleaseIsContinuousAndDamageUsesSameCorridor()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            yield return PurpleCases();
            yield return new ExitPlayMode();
        }
        private static IEnumerator PurpleCases()
        {
            Runtime();
            var owner = new GameObject("PurpleCaster", typeof(GojoTechniqueChainController));
            var chain = owner.GetComponent<GojoTechniqueChainController>();
            for (int variant = 0; variant < 4; variant++)
            {
                owner.transform.rotation = Quaternion.Euler(0, variant * 67f, 0);
                Vector3 forward = owner.transform.forward;
                GameObject aim = null;
                if (variant > 0)
                {
                    aim = Target("Aim", forward * (variant == 1 ? 5f : 28f));
                    Assert.That(owner.GetComponent<TargetLockController>().TryLockTarget(aim.GetComponent<Health>()), Is.True);
                }
                Vector3 direction = (Vector3)Call(chain, "FindPurpleAimDirection");
                Assert.That(direction.y, Is.EqualTo(0f).Within(.0001f));
                Vector3 origin = PrototypeHollowPurplePresentationRuntime.ReleaseOrigin(owner.transform.position, direction);
                var host = new GameObject("PurpleTestSequence");
                var sequence = PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform, owner.transform, 0f);
                float hold = GojoPolishSettings.Current.purpleFusionHoldDuration;
                Assert.That(hold, Is.EqualTo(3f));
                Assert.That(chain.PurpleGameplayRadius, Is.EqualTo(3.2f));
                sequence.Update(GojoPolishSettings.Current.PurpleHoldStart + hold * .5f, 0f);
                var orb = GameObject.Find("HollowPurpleDenseBody").transform;
                Assert.That(Vector3.Distance(orb.position, origin), Is.LessThan(.001f));
                for (int frame = 0; frame <= 12; frame++)
                {
                    float travel = frame / 60f;
                    sequence.Update(GojoPolishSettings.Current.PurpleReleaseTime + travel, 1f / 60f);
                    Assert.That(orb.position.y, Is.EqualTo(origin.y).Within(.001f), "Initial release must not descend");
                    Assert.That(Vector3.Distance(orb.position, origin + direction * (chain.PurpleRange * travel / chain.PurpleLaunchDuration)),
                        Is.LessThan(.003f));
                }
                if (aim != null) aim.GetComponent<Collider>().enabled = false;
                var near = Target("Near", origin + direction * 2f);
                var far = Target("Far", origin + direction * 44f);
                var outside = Target("Outside", origin + direction * 12f + Vector3.Cross(Vector3.up, direction) * 4.2f);
                var beyond = Target("Beyond", origin + direction * 54f);
                Physics.SyncTransforms();
                Call(chain, "QueuePurpleDamage", direction);
                yield return Wait(.4f);
                Assert.That(near.GetComponent<Health>().CurrentHealth, Is.EqualTo(100f), "No damage during formation/hold");
                yield return Wait(GojoPolishSettings.Current.PurpleReleaseTime);
                Assert.That(near.GetComponent<Health>().CurrentHealth, Is.EqualTo(45f));
                Assert.That(far.GetComponent<Health>().CurrentHealth, Is.EqualTo(100f));
                yield return Wait(1.55f);
                Assert.That(far.GetComponent<Health>().CurrentHealth, Is.EqualTo(100f), "First contact stops damage to targets behind it");
                Assert.That(outside.GetComponent<Health>().CurrentHealth, Is.EqualTo(100f));
                Assert.That(beyond.GetComponent<Health>().CurrentHealth, Is.EqualTo(100f));
                sequence.Update(20f, 0f); sequence.Dispose();
                Object.Destroy(host); Object.Destroy(near); Object.Destroy(far);
                Object.Destroy(outside); Object.Destroy(beyond); if (aim != null) Object.Destroy(aim);
                yield return null;
                Assert.That(Object.FindFirstObjectByType<PurpleTravelAftermath>(), Is.Null);
                var subscriptions=typeof(GojoTechniqueChainController).GetField("PurpleTerminated",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(chain) as Delegate;
                Assert.That(subscriptions,Is.Null,"Disposing one cast must remove its termination subscription before the next cast");
            }
            Object.Destroy(owner);
        }

        [UnityTest] public IEnumerator BlueFourHitsResidueAndRepeatedCleanup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            yield return BlueCases();
            yield return new ExitPlayMode();
        }
        private static IEnumerator BlueCases()
        {
            Runtime();
            for (int repeat = 0; repeat < 3; repeat++)
            {
                var target = Target("BlueTarget", Vector3.forward);
                var owner = new GameObject("Owner", typeof(Health));
                var field = new GameObject("BlueField", typeof(BlueConvergenceField)).GetComponent<BlueConvergenceField>();
                int count = 0; var times = new List<float>(); float started = Time.time;
                field.Configure(owner.GetComponent<Health>(), 4.5f, .95f, .1f, 8f, 16f, .42f,
                    _ => { count++; times.Add(Time.time - started); }, null);
                Physics.SyncTransforms();
                yield return Wait(1.01f);
                Assert.That(field == null, Is.True);
                Assert.That(count, Is.EqualTo(4));
                Assert.That(target.GetComponent<Health>().CurrentHealth, Is.EqualTo(92f));
                float[] expected = { .171f, .361f, .551f, .741f };
                for (int i = 0; i < 4; i++) Assert.That(times[i], Is.InRange(expected[i] - .002f, expected[i] + .1f));
                var visual = GameObject.Find("GojoBlueSignatureVfx");
                Assert.That(visual, Is.Not.Null, "Presentation must outlive gameplay");
                Assert.That(visual.transform.Find("BlueCoreRoot").gameObject.activeSelf, Is.False);
                yield return Wait(.5f);
                Assert.That(count, Is.EqualTo(4), "No fifth aftermath hit");
                Assert.That(GameObject.Find("GojoBlueSignatureVfx"), Is.Null);
                Assert.That(Object.FindFirstObjectByType<BlueTornDebris>(), Is.Null);
                Object.Destroy(target); Object.Destroy(owner);
                yield return null;
            }
            var cancelled = new GameObject("CancelledBlue", typeof(BlueConvergenceField)).GetComponent<BlueConvergenceField>();
            cancelled.Configure(null, 4.5f, .95f, .1f, 8f, 16f, .42f, null, null);
            Object.Destroy(cancelled.gameObject);
            yield return null; yield return null;
            Assert.That(GameObject.Find("GojoBlueSignatureVfx"), Is.Null, "Cancel must not leave residue");
        }

        [UnityTest] public IEnumerator RedSurfaceOutwardMotionAndRepeatedCleanup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            yield return RedCases();
            yield return new ExitPlayMode();
        }
        private static IEnumerator RedCases()
        {
            Runtime();
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.position = Vector3.down * .5f; ground.transform.localScale = new Vector3(40,1,40);
            Physics.SyncTransforms();
            for (int repeat = 0; repeat < 3; repeat++)
            {
                var handle = PresentationVfxRuntime.Spawn(GojoRedPresentationPreset.CreateImpactRequest(Vector3.up, 1.7f, Vector3.forward));
                yield return Wait(.08f);
                var root = GameObject.Find("RedSurfaceReaction"); Assert.That(root, Is.Not.Null);
                Assert.That(root.transform.position.y, Is.EqualTo(.035f).Within(.005f));
                Assert.That(root.GetComponentsInChildren<Collider>().Length, Is.Zero);
                Assert.That(root.GetComponentsInChildren<Rigidbody>().Length, Is.Zero);
                Transform chunk = root.transform.Find("RepelledSurfaceChunk");
                Vector3 first = chunk.localPosition;
                yield return Wait(.2f);
                Vector3 second = chunk.localPosition;
                Assert.That(new Vector2(second.x, second.y).magnitude, Is.GreaterThan(new Vector2(first.x, first.y).magnitude));
                var front = GameObject.Find("ThinShockFront").GetComponent<LineRenderer>();
                Assert.That(front.loop, Is.False);
                yield return Wait(1.5f);
                Assert.That(handle.IsAlive, Is.False);
                Assert.That(GameObject.Find("RedSurfaceReaction"), Is.Null);
                Assert.That(GameObject.Find("RepulsivePressureAftermath"), Is.Null);
            }
            Object.Destroy(ground);
        }

        [Test] public void PolishInspectorUsesKoreanLabelsAndPreservesSerializedTuning()
        {
            var settings = Resources.Load<GojoPolishSettings>("VFX/GojoPolishSettings");
            var editor = UnityEditor.Editor.CreateEditor(settings);
            Assert.That(editor.GetType().Name, Is.EqualTo("GojoPolishSettingsEditor"));
            var serialized = new SerializedObject(settings); var p = serialized.GetIterator();
            bool children = true;
            while (p.NextVisible(children))
            {
                children = false;
                if (p.name == "m_Script") continue;
                string label = KoreanInspectorLabels.Content(p).text;
                Assert.That(System.Text.RegularExpressions.Regex.IsMatch(label, "[가-힣]"), Is.True, p.name);
            }
            Assert.That(settings.purpleFusionHoldDuration, Is.EqualTo(3f));
            Assert.That(settings.purpleHoldOffset, Is.EqualTo(new Vector3(0,1.3f,2.2f)));
            Assert.That(settings.redAftermathDuration, Is.EqualTo(1.25f));
            Object.DestroyImmediate(editor);
        }

        [UnityTest] public IEnumerator RenderReviewInVfxLabAndCombatCamera()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            yield return RenderScenes();
            yield return new ExitPlayMode();
        }
        private static IEnumerator RenderScenes()
        {
            foreach (string scene in new[] { "VFXLab", "CombatMVP" })
            {
                yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/" + scene + ".unity",
                    new LoadSceneParameters(LoadSceneMode.Single));
                yield return Wait(.3f);
                var camera = Camera.main; Assert.That(camera, Is.Not.Null);
                var actor = scene == "CombatMVP" ? GameObject.Find("GojoPlayer").transform
                    : Object.FindFirstObjectByType<JJKGame.Dev.VFXLab.VfxLabPreviewCharacter>().transform;
                Time.timeScale = 0f;
                Vector3 centre = actor.position + actor.forward * 5f + Vector3.up * .55f;
                var anchor = new GameObject("ReviewBlueAnchor"); anchor.transform.position = centre;
                var blue = PresentationVfxRuntime.Spawn(GojoBluePresentationPreset.CreateFieldRequest(anchor.transform, 4.5f, .95f));
                var root = GameObject.Find("GojoBlueSignatureVfx");
                MonoBehaviour presenter = null;
                foreach (var component in root.GetComponents<MonoBehaviour>())
                    if (component.GetType().Name == "GojoBlueVfxInstance") presenter = component;
                float[] beats = { .10f,.171f,.27f,.361f,.46f,.551f,.65f,.741f,1.05f };
                for (int i = 0; i < beats.Length; i++)
                {
                    Call(presenter, "Tick", beats[i], beats[i] / 1.3f, .016f);
                    foreach (var ps in root.GetComponentsInChildren<ParticleSystem>()) ps.Simulate(beats[i], false, true, false);
                    Call(presenter, "ApplyVisualFade", 1f);
                    Capture(scene + "_Blue_" + i, camera, centre, i == 3);
                }
                blue.Stop(PresentationVfxStopMode.Immediate); Object.Destroy(anchor);
                yield return null;
                var red = PresentationVfxRuntime.Spawn(GojoRedPresentationPreset.CreateImpactRequest(centre, 1.7f, actor.forward));
                root = GameObject.Find("GojoRedSignatureVfx");
                foreach (var component in root.GetComponents<MonoBehaviour>())
                    if (component.GetType().Name == "GojoRedVfxInstance") presenter = component;
                presenter.enabled = false;
                foreach (float age in new[] { .055f,.18f,.45f,.9f })
                {
                    Call(presenter, "Tick", age, age / 1.51f, .016f);
                    foreach (var ps in root.GetComponentsInChildren<ParticleSystem>()) ps.Simulate(age, false, true, false);
                    Call(presenter, "ApplyVisualFade", 1f);
                    Capture(scene + "_Red_" + age.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture), camera, centre, false);
                }
                red.Stop(PresentationVfxStopMode.Immediate); yield return null;
                var host = new GameObject("PurpleReview");
                var purple = PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform, actor, 0f);
                Vector3 origin = PrototypeHollowPurplePresentationRuntime.ReleaseOrigin(actor.position, actor.forward);
                foreach (float age in new[] { .40f,.56f,.60f,.68f,.75f,2.5f })
                {
                    purple.Update(age, .016f);
                    Capture(scene + "_Purple_" + age.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture), camera, origin + actor.forward * 2f, false,
                        actor.right * 14f + Vector3.up * 2f);
                }
                purple.Dispose(); Object.Destroy(host); Time.timeScale = 1f;
                yield return null;
            }
        }
        [UnityTest] public IEnumerator PurpleProductionRenderReview()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            foreach (string scene in new[] { "VFXLab", "CombatMVP" })
            {
                yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/" + scene + ".unity",
                    new LoadSceneParameters(LoadSceneMode.Single));
                yield return Wait(.3f);
                var camera = Camera.main;
                var actor = scene == "CombatMVP" ? GameObject.Find("GojoPlayer").transform
                    : Object.FindFirstObjectByType<JJKGame.Dev.VFXLab.VfxLabPreviewCharacter>().transform;
                Time.timeScale = 0f;
                var host = new GameObject("PurpleProductionReview");
                var sequence = PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform, actor, 0f);
                Vector3 origin = PrototypeHollowPurplePresentationRuntime.ReleaseOrigin(actor.position, actor.forward);
                var beatList = new List<float>();
                for (int frame=0; frame<=330; frame+=6) beatList.Add(frame/60f);
                for (int frame=0; frame<8; frame++) beatList.Add(5f+frame/60f+.0001f);
                beatList.AddRange(new[]{6.2f,6.734f,6.774f,6.824f,6.95f,7.15f,7.5f,8.55f});
                beatList.Sort(); float[] beats=beatList.ToArray();
                float previous = 0;
                foreach (float age in beats)
                {
                    for (float t = previous + .016f; t < age; t += .016f) sequence.Update(t,.016f);
                    sequence.Update(age,.016f); previous = age;
                    Canvas.ForceUpdateCanvases();
                    string label = scene + "_PurpleThird_" + age.ToString("0.000",System.Globalization.CultureInfo.InvariantCulture);
                    Capture(label + "_shot",camera,origin,true);
                    if (age == 3f || age == 4.5f || age == 5.5f)
                    {
                        Vector3 focus = origin + actor.forward * Mathf.Max(0,age-GojoPolishSettings.Current.PurpleReleaseTime)*30f;
                        Capture(label+"_front",camera,focus,false,actor.forward*13f+Vector3.up*1.4f);
                        Capture(label+"_side",camera,focus,false,actor.right*14f+Vector3.up*2f);
                        Capture(label+"_low",camera,focus,false,-actor.forward*12f+actor.right*8f-Vector3.up*.7f);
                    }
                    if (age == 6.2f || age == 7.15f)
                        Capture(label+"_scar",camera,origin+actor.forward*(age==6.2f?32f:48f)-Vector3.up*2f,false,
                            actor.right*9f-actor.forward*5f+Vector3.up*9f);
                }
                sequence.Dispose(); Object.Destroy(host);
                yield return null; yield return null;
                host=new GameObject("PurpleObserverReview");
                sequence=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform,actor,0f,null,false);
                Vector3 observerPosition=camera.transform.position; Quaternion observerRotation=camera.transform.rotation;
                previous=0;
                foreach(float age in beats)
                {
                    for(float t=previous+.016f;t<age;t+=.016f) sequence.Update(t,.016f);
                    sequence.Update(age,.016f); previous=age;
                    Assert.That(JJKGame.CameraSystem.DomainCameraOverride.IsOwned(camera),Is.False,"Observer cannot be forced into caster shot");
                    Assert.That(GameObject.Find("PurpleImpactFrame"),Is.Null);
                    Assert.That(Vector3.Distance(camera.transform.position,observerPosition),Is.LessThan(.0001f));
                    Assert.That(Quaternion.Angle(camera.transform.rotation,observerRotation),Is.LessThan(.001f));
                    float travel=Mathf.Clamp(age-GojoPolishSettings.Current.PurpleReleaseTime,0,1.6f)*30f;
                    Capture(scene+"_PurpleThirdObserver_"+age.ToString("0.000",System.Globalization.CultureInfo.InvariantCulture),
                        camera,origin+actor.forward*travel,false,actor.right*14f-actor.forward*9f+Vector3.up*3f);
                }
                sequence.Dispose(); Object.Destroy(host); Time.timeScale=1f;
                yield return null; yield return null;
            }
            foreach(string shaderName in new[]{"HollowPurpleVolume","HollowPurpleFilament","HollowPurpleImpact","HollowPurpleScar","HollowPurpleIngredient","HollowPurpleDust"})
            {
                var shader=Resources.Load<Shader>("VFX/"+shaderName);
                Assert.That(shader.isSupported,Is.True,shaderName);
                foreach(var message in ShaderUtil.GetShaderMessages(shader))
                    Assert.That(message.severity,Is.Not.EqualTo(UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),message.message);
            }
            yield return new ExitPlayMode();
        }

        [UnityTest] public IEnumerator PurpleCameraCancellationAndResourceCleanup()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            var cameraHost=new GameObject("PurpleCameraFixture",typeof(Camera)); cameraHost.tag="MainCamera";
            var camera=cameraHost.GetComponent<Camera>(); camera.fieldOfView=63;
            camera.transform.SetPositionAndRotation(new Vector3(2,6,-10),Quaternion.Euler(22,-8,0));
            Vector3 position=camera.transform.position; Quaternion rotation=camera.transform.rotation;
            int baseline=PurpleMaterialCount();
            for(int repeat=0;repeat<4;repeat++)
            {
                var host=new GameObject("PurpleLifetimeFixture"); var caster=new GameObject("Caster");
                var sequence=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform,caster.transform,0f,camera);
                var sequenceRoot=host.transform.Find("HollowPurpleCanonicalOrbSequence");
                sequence.Update(.12f,.016f);
                Assert.That(sequenceRoot.Find("HollowPurpleBlueOrbRoot").gameObject.activeSelf,Is.True);
                Assert.That(sequenceRoot.Find("HollowPurpleRedOrbRoot").gameObject.activeSelf,Is.False,"Blue appears first");
                sequence.Update(.80f,.016f);
                Assert.That(sequenceRoot.Find("HollowPurpleRedOrbRoot").gameObject.activeSelf,Is.True);
                Assert.That(sequenceRoot.Find("HollowPurpleDenseBody").gameObject.activeSelf,Is.False,"Opposition must precede fusion");
                sequence.Update(3.0f,.016f);
                var shot=host.GetComponentInChildren<PurpleReleasePresentation>();
                Assert.That(shot.OwnsCamera,Is.True);
                Assert.That(Vector3.Distance(camera.transform.position,position),Is.GreaterThan(.1f));
                sequence.Update(GojoPolishSettings.Current.PurpleImpactStart+.001f,.016f); Assert.That(shot.ImpactVisible,Is.True);
                if(repeat==0)
                {
                    Assert.That(GojoPolishSettings.Current.PurpleReleaseTime,Is.EqualTo(5.1333333f).Within(.0001f));
                    var graphic=host.GetComponentInChildren<UnityEngine.UI.RawImage>(true);
                    for(int frame=0;frame<8;frame++)
                    {
                        sequence.Update(GojoPolishSettings.Current.PurpleImpactStart+frame/60f+.0001f,.016f);
                        Assert.That(shot.ImpactVisible,Is.True,"Eight frames with four distinct cuts");
                        Assert.That(graphic.material.GetFloat("_Stage"),Is.EqualTo(frame/2));
                    }
                    sequence.Update(GojoPolishSettings.Current.PurpleReleaseTime+.02f,.016f);
                    Assert.That(shot.ImpactVisible,Is.False);
                    sequence.Update(GojoPolishSettings.Current.PurpleReleaseTime+.8f,.016f);
                }
                else if(repeat==1) shot.enabled=false;
                else if(repeat==2) { caster.SetActive(false); Assert.That(sequence.Update(GojoPolishSettings.Current.PurpleImpactStart+.03f,.016f),Is.False); }
                sequence.Dispose();
                Assert.That(JJKGame.CameraSystem.DomainCameraOverride.IsOwned(camera),Is.False);
                Assert.That(camera.fieldOfView,Is.EqualTo(63));
                Assert.That(Vector3.Distance(camera.transform.position,position),Is.LessThan(.0001f));
                Assert.That(Quaternion.Angle(camera.transform.rotation,rotation),Is.LessThan(.001f));
                Object.Destroy(host); Object.Destroy(caster); yield return null; yield return null;
                Assert.That(Object.FindFirstObjectByType<PurpleEnergyBody>(),Is.Null);
                Assert.That(Object.FindFirstObjectByType<PurpleTravelAftermath>(),Is.Null);
                Assert.That(GameObject.Find("PurpleImpactFrame"),Is.Null);
                Assert.That(PurpleMaterialCount(),Is.EqualTo(baseline),"Owned materials must be destroyed");
            }
            var other=new GameObject("OtherCameraOwner");
            var lease=camera.GetComponent<JJKGame.CameraSystem.DomainCameraOverride>(); Assert.That(lease.Acquire(other),Is.True);
            var blockedHost=new GameObject("BlockedPurple",typeof(PurpleReleasePresentation));
            var blocked=blockedHost.GetComponent<PurpleReleasePresentation>(); blocked.Configure(Vector3.forward*4,Vector3.forward,.56f,camera);
            blocked.Sample(.561f); Assert.That(blocked.OwnsCamera,Is.False); Assert.That(blocked.ImpactVisible,Is.False);
            Object.Destroy(blockedHost); lease.Release(other); Object.Destroy(other);
            var displacedHost=new GameObject("DisplacedPurple",typeof(PurpleReleasePresentation));
            var displaced=displacedHost.GetComponent<PurpleReleasePresentation>(); displaced.Configure(Vector3.forward*4,Vector3.forward,.56f,camera);
            lease.Release(displaced);
            var takeover=new GameObject("NewCinematicOwner"); Assert.That(lease.Acquire(takeover),Is.True);
            displaced.Sample(.45f);
            Assert.That(displaced.OwnsCamera,Is.False); Assert.That(displaced.ImpactVisible,Is.False);
            Assert.That(lease.IsOwnedBy(takeover),Is.True,"A lost Purple lease must not affect the new owner");
            lease.Release(takeover); Object.Destroy(takeover); Object.Destroy(displacedHost);
            var hitchHost=new GameObject("PurpleHitch",typeof(PurpleReleasePresentation));
            var hitch=hitchHost.GetComponent<PurpleReleasePresentation>(); hitch.Configure(Vector3.forward*4,Vector3.forward,.56f,camera);
            hitch.Sample(.42f); hitch.Sample(.68f); Assert.That(hitch.ImpactVisible,Is.True,"A hitch crossing release must still produce the peak graphic");
            hitch.Sample(.84f); Assert.That(hitch.ImpactVisible,Is.False); hitch.Restore();
            Object.Destroy(hitchHost); Object.Destroy(cameraHost);
            yield return new ExitPlayMode();
        }
        private static int PurpleMaterialCount()
        {
            int count=0;
            foreach(var m in Resources.FindObjectsOfTypeAll<Material>()) if(m.name.StartsWith("Purple") && m.name.Contains("_Runtime")) count++;
            return count;
        }

        [UnityTest] public IEnumerator PurpleRealCombatCastAndInterruptedPreviewRestore()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            yield return new EnterPlayMode();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/CombatMVP.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return Wait(.3f);
            foreach(var bot in Object.FindObjectsByType<JJKGame.Enemy.CurseBotController>(FindObjectsSortMode.None)) bot.enabled=false;
            var actor=GameObject.Find("GojoPlayer"); var chain=actor.GetComponent<GojoTechniqueChainController>();
            var camera=Camera.main; float fov=camera.fieldOfView;
            // The production scene has a nearer bot. Isolate the two probes for the new first-contact rule.
            foreach(var health in Object.FindObjectsByType<Health>(FindObjectsSortMode.None))
                if(health.gameObject!=actor)
                    foreach(var collider in health.GetComponentsInChildren<Collider>()) collider.enabled=false;
            // Use the production activation, source detection, camera, damage coroutine and cleanup path.
            var target=Target("ActualPurpleDamageProbe",actor.transform.position+Vector3.forward*12f+Vector3.up);
            var behind=Target("BehindFirstContact",actor.transform.position+Vector3.forward*30f+Vector3.up*2.35f);
            actor.transform.rotation=Quaternion.identity;
            Assert.That(actor.GetComponent<TargetLockController>().TryLockTarget(target.GetComponent<Health>()),Is.True);
            Physics.SyncTransforms(); Call(chain,"ActivatePurple");
            yield return Wait(3f);
            Assert.That(Object.FindFirstObjectByType<PurpleEnergyBody>(),Is.Not.Null);
            Assert.That(JJKGame.CameraSystem.DomainCameraOverride.IsOwned(camera),Is.True);
            Assert.That(target.GetComponent<Health>().CurrentHealth,Is.EqualTo(100));
            Capture("CombatMVP_PurpleThird_REAL_hold",camera,actor.transform.position,true);
            yield return Wait(GojoPolishSettings.Current.PurpleReleaseTime - 3f + .22f);
            Assert.That(GameObject.Find("PurpleImpactFrame"),Is.Null,"Impact graphic already inactive after its short peak");
            Capture("CombatMVP_PurpleThird_REAL_travel",camera,actor.transform.position,true);
            yield return Wait(.7f);
            Assert.That(target.GetComponent<Health>().CurrentHealth,Is.EqualTo(45));
            Assert.That(behind.GetComponent<Health>().CurrentHealth,Is.EqualTo(100));
            var terminal=Object.FindFirstObjectByType<PurpleTerminalBurst>();
            Assert.That(terminal,Is.Not.Null,"The first actual contact must produce a world-space terminal rupture");
            Assert.That(terminal.transform.position.z,Is.EqualTo(actor.transform.position.z+12f).Within(.01f));
            Capture("CombatMVP_PurpleThird_REAL_first_hit_terminal",camera,terminal.transform.position,false,new Vector3(12,4,-10));
            Assert.That(JJKGame.CameraSystem.DomainCameraOverride.IsOwned(camera),Is.False);
            Assert.That(camera.fieldOfView,Is.EqualTo(fov).Within(.01f));
            yield return Wait(3f);
            Assert.That(Object.FindFirstObjectByType<PurpleEnergyBody>(),Is.Null);
            Assert.That(Object.FindFirstObjectByType<PurpleTravelAftermath>(),Is.Null);
            Object.Destroy(target);
            Object.Destroy(behind);

            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/VFXLab.unity",new LoadSceneParameters(LoadSceneMode.Single));
            yield return Wait(.3f);
            var preview=Object.FindFirstObjectByType<JJKGame.Dev.VFXLab.VfxLabPreviewSequence>();
            camera=Camera.main; Vector3 p=camera.transform.position; Quaternion q=camera.transform.rotation; fov=camera.fieldOfView;
            Call(preview,"Begin",JJKGame.Dev.VFXLab.VfxLabPreviewAction.HollowPurple);
            double deadline=EditorApplication.timeSinceStartup+8;
            while(!JJKGame.CameraSystem.DomainCameraOverride.IsOwned(camera))
            {
                Assert.That(EditorApplication.timeSinceStartup,Is.LessThan(deadline)); yield return null;
            }
            // Begin positions the preview anchor before fusion; restore the camera at lease acquisition.
            var previewLease=camera.GetComponent<JJKGame.CameraSystem.DomainCameraOverride>();
            p=previewLease.ReturnPosition; q=previewLease.ReturnRotation; fov=previewLease.ReturnFov;
            Call(preview,"CancelPreview","PURPLE CAMERA CANCEL TEST");
            Assert.That(JJKGame.CameraSystem.DomainCameraOverride.IsOwned(camera),Is.False);
            Assert.That(camera.fieldOfView,Is.EqualTo(fov));
            Assert.That(Vector3.Distance(camera.transform.position,p),Is.LessThan(.001f));
            Assert.That(Quaternion.Angle(camera.transform.rotation,q),Is.LessThan(.01f));
            yield return null; yield return null;
            Assert.That(Object.FindFirstObjectByType<PurpleReleasePresentation>(),Is.Null);
            yield return new ExitPlayMode();
        }

        [UnityTest] public IEnumerator PurpleThirdCancellationAndRangeTerminal()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            yield return new EnterPlayMode();
            var cameraObject=new GameObject("Observer",typeof(Camera)); cameraObject.tag="MainCamera";
            var camera=cameraObject.GetComponent<Camera>();
            camera.transform.SetPositionAndRotation(new Vector3(3,4,-9),Quaternion.Euler(15,10,0));
            Vector3 position=camera.transform.position;
            int baseline=PurpleMaterialCount();
            for(int variant=0;variant<3;variant++)
            {
                var owner=new GameObject("CancelCaster",typeof(GojoTechniqueChainController));
                var chain=owner.GetComponent<GojoTechniqueChainController>();
                var host=new GameObject("CancellationSequence");
                var sequence=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform,owner.transform,Time.unscaledTime);
                Assert.That(host.GetComponentInChildren<PurpleReleasePresentation>().OwnsCamera,Is.False,"Unrelated main camera is an observer");
                var origin=PrototypeHollowPurplePresentationRuntime.ReleaseOrigin(owner.transform.position,Vector3.forward);
                if(variant<2)
                {
                    var target=Target("CancelledDamageProbe",origin+Vector3.forward*3);
                    Physics.SyncTransforms(); Call(chain,"QueuePurpleDamage",Vector3.forward);
                    float castStart=chain.PurplePresentationStartedAt;
                    yield return Wait(variant==0?.2f:3.4f);
                    sequence.Update(Time.unscaledTime,.016f);
                    Assert.That(GameObject.Find("PurpleImpactFrame"),Is.Null);
                    chain.enabled=false;
                    Assert.That(sequence.Update(Time.unscaledTime,.016f),Is.False);
                    sequence.Dispose();
                    yield return Wait(Mathf.Max(.1f,castStart+GojoPolishSettings.Current.PurpleReleaseTime+.4f-Time.unscaledTime));
                    Assert.That(target.GetComponent<Health>().CurrentHealth,Is.EqualTo(100),"Cancelled formation/charge cannot leave delayed damage");
                    Object.Destroy(target);
                }
                else
                {
                    float start=Time.unscaledTime;
                    // Recreate at a known clock, then sample the endpoint and all residual cleanup.
                    sequence.Dispose(); yield return null; yield return null;
                    sequence=PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform,owner.transform,start,null,false);
                    float end=GojoPolishSettings.Current.PurpleReleaseTime+chain.PurpleLaunchDuration;
                    sequence.Update(start+end+.001f,.016f);
                    Assert.That(sequence.HasTerminated,Is.True);
                    Assert.That(Vector3.Distance(sequence.TerminalPosition,origin+Vector3.forward*chain.PurpleRange),Is.LessThan(.001f));
                    Assert.That(host.GetComponentInChildren<PurpleTerminalBurst>(),Is.Not.Null);
                    Assert.That(sequence.Update(start+end+2.1f,.016f),Is.False);
                    sequence.Dispose();
                }
                Object.Destroy(host); Object.Destroy(owner); yield return null; yield return null;
                Assert.That(Object.FindFirstObjectByType<PurpleTerminalBurst>(),Is.Null);
                Assert.That(Object.FindFirstObjectByType<PurpleEnergyBody>(),Is.Null);
                Assert.That(PurpleMaterialCount(),Is.EqualTo(baseline));
                Assert.That(Vector3.Distance(camera.transform.position,position),Is.LessThan(.001f));
            }
            Object.Destroy(cameraObject);
            yield return new ExitPlayMode();
        }

        private static void Capture(string label, Camera camera, Vector3 focus, bool gameplay, Vector3? offset = null)
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) Assert.Fail("Graphics required for visual review");
            Directory.CreateDirectory(Output + "/CODEX_PREVIEW");
            var target = new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32); target.Create();
            var texture = new Texture2D(1280,720,TextureFormat.RGB24,false);
            var previous = RenderTexture.active; var previousTarget = camera.targetTexture;
            Vector3 position = camera.transform.position; Quaternion rotation = camera.transform.rotation;
            bool async = ShaderUtil.allowAsyncCompilation;
            try
            {
                ShaderUtil.allowAsyncCompilation = false;
                if (!gameplay) { camera.transform.position = focus + (offset ?? new Vector3(8,4,-11)); camera.transform.LookAt(focus); }
                camera.targetTexture = target;
                RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                RenderTexture.active = target; texture.ReadPixels(new Rect(0,0,1280,720),0,0); texture.Apply();
                File.WriteAllBytes(Output + "/CODEX_PREVIEW/" + label + ".png", texture.EncodeToPNG());
            }
            finally
            {
                ShaderUtil.allowAsyncCompilation = async; camera.targetTexture = previousTarget;
                camera.transform.SetPositionAndRotation(position, rotation); RenderTexture.active = previous;
                target.Release(); Object.Destroy(target); Object.Destroy(texture);
            }
        }
    }
}
