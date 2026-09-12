using System.Collections;
using System.Reflection;
using JJKGame.Core;
using JJKGame.Player;
using JJKGame.Dev.VFXLab;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace JJKGame.EditorTools
{
    public sealed class DomainCaptureProbe : MonoBehaviour, IDomainStunnable
    {
        public int Calls;
        public float Duration;
        public void ApplyDomainStun(float duration) { Calls++; Duration = duration; }
    }

    public sealed class DomainPresentationRuntimeTests
    {
        [UnityTearDown]
        public IEnumerator LeavePlayModeAfterFailure()
        {
            if (UnityEditor.EditorApplication.isPlaying) yield return new ExitPlayMode();
        }

        private static void Invoke(object target, string name, params object[] args)
        {
            Assert.That(target, Is.Not.Null, name + " receiver missing");
            var method = target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, name + " method missing");
            method.Invoke(target, args);
        }

        // EditMode's coroutine runner does not process WaitForSeconds, even after EnterPlayMode.
        private static IEnumerator WaitScaled(float seconds)
        {
            float until = Time.time + seconds;
            double timeout = UnityEditor.EditorApplication.timeSinceStartup + 30;
            while (Time.time < until)
            {
                Assert.That(Application.isPlaying, Is.True);
                Assert.That(UnityEditor.EditorApplication.timeSinceStartup, Is.LessThan(timeout), "Play clock did not advance");
                yield return null;
            }
        }

        private static IEnumerator WaitReal(float seconds)
        {
            double until = UnityEditor.EditorApplication.timeSinceStartup + seconds;
            while (UnityEditor.EditorApplication.timeSinceStartup < until) yield return null;
        }

        [UnityTest]
        public IEnumerator CaptureBarrierSeparationMultipleReturnsDeathAndUnload()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            var cameraHost = new GameObject("TestCamera", typeof(Camera));
            cameraHost.tag = "MainCamera";
            cameraHost.transform.position = new Vector3(0,4,-8);
            Camera camera = cameraHost.GetComponent<Camera>(); camera.fieldOfView = 57f;
            var caster = new GameObject("Caster");
            caster.AddComponent<Health>();
            var domain = caster.AddComponent<GojoDomainController>();
            var settings = (DomainPresentationSettings)typeof(GojoDomainController)
                .GetField("domainPresentation", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(domain);
            settings.fitBarrierToCapturedParticipants = false;
            var a = MakeTarget("A", new Vector3(4,0,0));
            var b = MakeTarget("B", new Vector3(0,0,8));
            var outside = MakeTarget("Outside", new Vector3(40,0,0));
            caster.GetComponent<TargetLockController>().TryLockTarget(b.GetComponent<Health>());
            typeof(GojoDomainController).GetField("domainEnergyCost", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(domain, 0f);
            for (int repeat = 0; repeat < 2; repeat++)
            {
                settings.barrierVisualDiameterMeters = repeat == 0 ? 1f : 120f;
                Invoke(domain, "ActivateDomain");
                Assert.That(a.Calls, Is.EqualTo(repeat + 1));
                Assert.That(b.Calls, Is.EqualTo(repeat + 1));
                Assert.That(outside.Calls, Is.Zero);
                Assert.That(a.Duration, Is.EqualTo(domain.DomainVictimStunDuration));
                var session = caster.GetComponentInChildren<DomainPresentationSession>();
                Assert.That(session.FullCinematic, Is.True);
                Assert.That(DomainPresentationSession.SelectVictim(caster.transform,
                    new[] {a.transform,b.transform}, b.GetComponent<Health>()), Is.EqualTo(b.transform));
                yield return WaitScaled(settings.DetailDuration(true) + settings.barrierCloseDuration + .12f);
                Assert.That(session.IsEntered, Is.True);
                Assert.That(a.transform.position - caster.transform.position, Is.EqualTo(new Vector3(4,0,0)));
                Assert.That(b.transform.position - caster.transform.position, Is.EqualTo(new Vector3(0,0,8)));
                Assert.That(outside.transform.position.y, Is.Zero);
                if (repeat == 0) domain.ResetCommand();
                else yield return WaitScaled(settings.ArrivalDuration(true) + domain.DomainActiveDuration
                    - settings.DetailDuration(true) - settings.barrierCloseDuration);
                yield return null;
                Assert.That(caster.transform.position, Is.EqualTo(Vector3.zero));
                Assert.That(a.transform.position, Is.EqualTo(new Vector3(4,0,0)));
                Assert.That(b.transform.position, Is.EqualTo(new Vector3(0,0,8)));
                Assert.That(camera.fieldOfView, Is.EqualTo(57f));
                Assert.That(cameraHost.transform.position, Is.EqualTo(new Vector3(0,4,-8)));
            }
            Invoke(domain, "ActivateDomain");
            yield return WaitScaled(settings.DetailDuration(true) + settings.barrierCloseDuration + .12f);
            caster.GetComponent<Health>().Kill();
            yield return null;
            Assert.That(a.transform.position, Is.EqualTo(new Vector3(4,0,0)));
            Assert.That(b.transform.position, Is.EqualTo(new Vector3(0,0,8)));
            Object.Destroy(caster);
            yield return null;

            // A temporary presentation scene can disappear while the participants survive elsewhere.
            var scene = SceneManager.CreateScene("DomainSessionUnloadTest");
            var root = new GameObject("Presentation"); root.SetActive(false);
            SceneManager.MoveGameObjectToScene(root, scene);
            var visual = root.AddComponent<UnlimitedVoidProductionVisual>(); visual.Configure(new DomainPresentationSettings());
            root.SetActive(true);
            var transition = root.AddComponent<DomainPresentationSession>();
            transition.Begin(a.transform, new[] {b.transform}, visual, new DomainPresentationSettings(), 10f);
            yield return WaitScaled(settings.DetailDuration(true) + settings.barrierCloseDuration + .12f);
            yield return SceneManager.UnloadSceneAsync(scene);
            Assert.That(a.transform.position, Is.EqualTo(new Vector3(4,0,0)));
            Assert.That(b.transform.position, Is.EqualTo(new Vector3(0,0,8)));
            yield return new ExitPlayMode();
        }

        private static DomainCaptureProbe MakeTarget(string name, Vector3 position)
        {
            var host = new GameObject(name, typeof(Health), typeof(BoxCollider));
            host.transform.position = position;
            var extra = new GameObject("ExtraCollider", typeof(BoxCollider)); extra.transform.SetParent(host.transform, false);
            var probe = host.AddComponent<DomainCaptureProbe>();
            Physics.SyncTransforms();
            return probe;
        }

        [UnityTest]
        public IEnumerator VfxLabRepeatedPreviewsCleanUpAndAllowInteriorOrbit()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/VFXLab.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null; yield return null;
            var sequence = Object.FindFirstObjectByType<VfxLabPreviewSequence>();
            var character = Object.FindFirstObjectByType<VfxLabPreviewCharacter>();
            Assert.That(sequence, Is.Not.Null);
            Vector3 original = character.transform.position;
            var inspection = Object.FindFirstObjectByType<VfxLabOrbitCamera>();
            SetYaw(inspection, 65f);
            for (int repeat = 0; repeat < 3; repeat++)
            {
                Invoke(sequence, "Begin", VfxLabPreviewAction.Red);
                yield return WaitScaled(.2f);
                Assert.That(Object.FindObjectsByType<TechniqueChargeVisual>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
                yield return WaitScaled(.3f);
                Assert.That(Object.FindObjectsByType<TechniqueChargeVisual>(FindObjectsSortMode.None).Length, Is.Zero);
                if (repeat < 2)
                {
                    if (repeat == 0) yield return Capture("Red_travel");
                    float timeout = Time.time + 2f;
                    while (sequence.CurrentPhaseLabel != "RED IMPACT")
                    {
                        Assert.That(Time.time, Is.LessThan(timeout), "Red never reached its impact phase");
                        yield return null;
                    }
                    Assert.That(GameObject.Find("RedCoreRoot"), Is.Not.Null);
                    if (repeat == 1)
                    {
                        var impactPoint = (Vector3)typeof(VfxLabPreviewSequence)
                            .GetField("travelEnd", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sequence);
                        yield return Capture("Red_impact", impactPoint);
                        yield return WaitScaled(.4f);
                        var vapor = GameObject.Find("RepulsivePressureAftermath");
                        Assert.That(vapor, Is.Not.Null);
                        Assert.That(vapor.GetComponent<ParticleSystem>().particleCount, Is.GreaterThan(0));
                        yield return Capture("Red_aftermath", impactPoint);
                        yield return WaitScaled(GojoPolishSettings.Current.redAftermathDuration + .4f);
                        Assert.That(GameObject.Find("RepulsivePressureAftermath"), Is.Null);
                    }
                }
                Invoke(sequence, "CancelPreview", "TEST CANCEL");
                yield return null;
                Assert.That(GameObject.Find("RepulsivePressureAftermath"), Is.Null);
            }
            for (int repeat = 0; repeat < 2; repeat++)
            {
                Invoke(sequence, "Begin", VfxLabPreviewAction.Blue);
                yield return WaitScaled(.16f);
                Assert.That(Object.FindObjectsByType<TechniqueChargeVisual>(FindObjectsSortMode.None).Length, Is.Zero);
                if (repeat == 0) yield return Capture("Blue_compression");
                yield return WaitScaled(.94f);
                Assert.That(Object.FindObjectsByType<BlueTornDebris>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
                if (repeat == 0) yield return Capture("Blue_active");
                if (repeat == 1) yield return WaitScaled(2.2f);
                Invoke(sequence, "CancelPreview", "TEST CANCEL"); yield return null;
                Assert.That(Object.FindObjectsByType<BlueTornDebris>(FindObjectsSortMode.None).Length, Is.Zero);
                Invoke(sequence, "Begin", VfxLabPreviewAction.HollowPurple);
                yield return WaitScaled(.98f);
                var held = GameObject.Find("HollowPurpleDenseBody");
                Assert.That(held, Is.Not.Null);
                Vector3 heldPosition = held.transform.position;
                if (repeat == 0) yield return Capture("Purple_complete_hold");
                yield return WaitScaled(.08f);
                Assert.That(Vector3.Distance(held.transform.position, heldPosition), Is.LessThan(.01f), "Purple moved during completion hold");
                yield return WaitScaled(.30f);
                Assert.That(Vector3.Distance(held.transform.position, heldPosition), Is.GreaterThan(1f));
                if (repeat == 0) yield return Capture("Purple_travel");
                yield return WaitScaled(1.5f);
                if (repeat == 0) yield return Capture("Purple_scar");
                if (repeat == 1) yield return WaitScaled(2f);
                Invoke(sequence, "CancelPreview", "TEST CANCEL"); yield return null;
                Assert.That(Object.FindObjectsByType<PurpleTravelAftermath>(FindObjectsSortMode.None).Length, Is.Zero);
            }
            SetYaw(inspection, 0f);
            yield return WaitScaled(.15f); // save after the existing lab motor has settled onto its stage
            original = character.transform.position;
            for (int repeat = 0; repeat < 2; repeat++)
            {
                Invoke(sequence, "Begin", VfxLabPreviewAction.UnlimitedVoid);
                yield return WaitScaled(.37f);
                if (repeat == 0) yield return Capture("Domain_hand");
                yield return WaitScaled(1.65f);
                if (repeat == 0) yield return Capture("Domain_barrier");
                yield return WaitScaled(.45f);
                if (repeat == 0) yield return Capture("Domain_tunnel");
                var session = Object.FindFirstObjectByType<DomainPresentationSession>();
                Assert.That(session.FullCinematic, Is.False);
                Assert.That(session.IsEntered, Is.True);
                yield return WaitScaled(3f);
                Invoke(sequence, "SetPaused", true);
                var visual = Object.FindFirstObjectByType<UnlimitedVoidProductionVisual>();
                float clock = visual.PresentationElapsed;
                var orbit = Object.FindFirstObjectByType<VfxLabOrbitCamera>();
                for (int angle = 0; angle < 360; angle += 45)
                {
                    SetYaw(orbit, angle);
                    yield return WaitReal(.15f);
                    if (repeat == 0) yield return Capture("Domain_" + angle);
                }
                if (repeat == 0)
                {
                    // Inspect beside the actor, not from inside the oversized imported mesh.
                    Vector3 polarView = character.transform.position + Vector3.up*3 + Vector3.right*7;
                    yield return Capture("Domain_zenith", polarView, Vector3.up);
                    yield return Capture("Domain_nadir", polarView, Vector3.down);
                    Camera eyeView = Camera.main;
                    Vector3 savedEyePosition = eyeView.transform.position;
                    Quaternion savedEyeRotation = eyeView.transform.rotation;
                    bool orthographic = eyeView.orthographic;
                    float size = eyeView.orthographicSize;
                    Transform focal = visual.transform.Find("InfiniteSpaceFocalElement");
                    Quaternion focalRotation = focal.rotation;
                    var hiddenForComparison = new System.Collections.Generic.List<Renderer>();
                    foreach (Renderer renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                        if (renderer.enabled && (renderer.name.StartsWith("SuspendedWhiteBlood_")
                            || (renderer.transform.IsChildOf(character.transform)
                                && renderer.GetComponentInParent<UnlimitedVoidProductionVisual>() == null)))
                        { hiddenForComparison.Add(renderer); renderer.enabled = false; }
                    try
                    {
                        eyeView.orthographic = true; eyeView.orthographicSize = 28f;
                        eyeView.transform.SetPositionAndRotation(visual.transform.TransformPoint(new Vector3(22f,7.5f,0f)),
                            visual.transform.rotation);
                        focal.rotation = visual.transform.rotation;
                        yield return Capture("CosmicEye_reference_orthographic");
                    }
                    finally
                    {
                        eyeView.orthographic = orthographic; eyeView.orthographicSize = size;
                        eyeView.transform.SetPositionAndRotation(savedEyePosition, savedEyeRotation);
                        focal.rotation = focalRotation;
                        foreach (Renderer renderer in hiddenForComparison)
                            if (renderer != null) renderer.enabled = true;
                    }
                }
                Assert.That(GameObject.Find("VoidFloorSuppression"), Is.Null, "Visual floor must not occlude lower ink");
                Assert.That(visual.PresentationElapsed, Is.EqualTo(clock));
                if (repeat == 0)
                {
                    float lifetime = (float)typeof(DomainPresentationSession).GetField("lifetime",
                        BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);
                    Invoke(session, "Tick", lifetime - 1.10f);
                    yield return Capture("Domain_release_start");
                    Invoke(session, "Tick", lifetime - .57f);
                    yield return Capture("Domain_release_middle");
                    Invoke(session, "Tick", lifetime - .06f);
                    yield return Capture("Domain_release_end");
                }
                Invoke(sequence, "CancelPreview", "TEST CANCEL");
                Invoke(sequence, "SetPaused", false);
                yield return null;
                Assert.That(Vector3.Distance(character.transform.position, original), Is.LessThan(.1f));
                Assert.That(JJKGame.CameraSystem.DomainCameraOverride.IsOwned(Camera.main), Is.False);
            }
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator CombatMvpPolishUsesRealDamageAndRestoresDomain()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/CombatMVP.unity", OpenSceneMode.Single);
            yield return new EnterPlayMode();
            // Create captured event-counter closures AFTER the Play Mode domain reload.
            yield return VerifyCombatMvp();
            yield return new ExitPlayMode();
        }

        private IEnumerator VerifyCombatMvp()
        {
            yield return WaitScaled(.3f);
            var gojo = GameObject.Find("GojoPlayer");
            Assert.That(gojo, Is.Not.Null);
            var technique = gojo.GetComponent<GojoTechniqueController>();
            var chain = gojo.GetComponent<GojoTechniqueChainController>();
            var domain = gojo.GetComponent<GojoDomainController>();
            var bot = Object.FindFirstObjectByType<JJKGame.Enemy.CurseBotController>();
            Assert.That(technique, Is.Not.Null);
            Assert.That(bot, Is.Not.Null);
            Assert.That(chain, Is.Not.Null, "Combat chain missing");
            Assert.That(domain, Is.Not.Null, "Combat domain missing");
            Debug.Log("POLISH COMBAT ready: " + gojo.name + " / " + bot.name);
            // Deterministic training placement; actual actor, colliders, damage and presenters remain in use.
            bot.enabled = false;
            var health = bot.GetComponent<Health>();
            Assert.That(health, Is.Not.Null, "Combat bot health missing");
            Vector3 forward = Vector3.forward;
            gojo.transform.rotation = Quaternion.identity;
            Place(bot.transform, gojo.transform.position + forward * 8f);
            var targetLock = gojo.GetComponent<TargetLockController>();
            Assert.That(targetLock, Is.Not.Null, "Combat target lock missing");
            Assert.That(targetLock.TryLockTarget(health), Is.True, "Training target could not be locked");
            int blueHits = 0, redHits = 0;
            technique.BlueHit += target => { if (target == health) blueHits++; };
            technique.RedHit += target => { if (target == health) redHits++; };
            var castType = typeof(GojoTechniqueController).GetNestedType("CastState", BindingFlags.NonPublic);
            Assert.That(castType, Is.Not.Null, "Casting enum missing");
            Debug.Log("POLISH COMBAT starting Blue");
            Invoke(technique, "TryBeginTechnique", System.Enum.Parse(castType, "Blue"));
            yield return WaitScaled(.1f);
            yield return Capture("Combat_Blue_compression", gojo.transform.position + Vector3.up);
            yield return WaitScaled(.35f);
            Assert.That(Object.FindFirstObjectByType<BlueTornDebris>(), Is.Not.Null);
            yield return Capture("Combat_Blue_gameplay_camera");
            yield return Capture("Combat_Blue_active", bot.transform.position + Vector3.up);
            float fieldDuration = (float)typeof(GojoTechniqueController).GetField("blueFieldDuration",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(technique);
            yield return WaitScaled(fieldDuration);
            Assert.That(blueHits, Is.EqualTo(4), "Real active Blue must retain its four damage pulses");

            Place(bot.transform, gojo.transform.position + forward * 5f);
            Invoke(technique, "TryBeginTechnique", System.Enum.Parse(castType, "Red"));
            float redDeadline = Time.time + 3f;
            while (redHits == 0)
            {
                Assert.That(Time.time, Is.LessThan(redDeadline), "Combat Red did not hit its target");
                yield return null;
            }
            Assert.That(Object.FindObjectsByType<TechniqueChargeVisual>(FindObjectsSortMode.None).Length, Is.Zero);
            yield return Capture("Combat_Red_impact", bot.transform.position + Vector3.up);
            yield return WaitScaled(.4f);
            Assert.That(GameObject.Find("RepulsivePressureAftermath"), Is.Not.Null);
            yield return Capture("Combat_Red_aftermath", bot.transform.position + Vector3.up);
            yield return WaitScaled(1.1f);
            Assert.That(GameObject.Find("RepulsivePressureAftermath"), Is.Null);

            // Lock a centre-line target, then place the real enemy outside the old 2.2m capsule.
            var centre = MakeTarget("PurpleCentreFixture", gojo.transform.position + forward * 12f);
            var outside = MakeTarget("PurpleOutsideFixture", gojo.transform.position + forward * 9f + Vector3.right * 5f);
            var far = MakeTarget("PurpleFarFixture", gojo.transform.position + forward * 44f);
            var beyond = MakeTarget("PurpleBeyondFixture", gojo.transform.position + forward * 55f);
            targetLock.TryLockTarget(centre.GetComponent<Health>());
            Place(bot.transform, gojo.transform.position + forward * 9f + Vector3.right * 3f);
            Assert.That(chain.PurpleGameplayRadius, Is.EqualTo(3.2f));
            float beforePurple = health.CurrentHealth;
            typeof(GojoTechniqueChainController).GetField("purpleEnergyCost", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(chain, 0f);
            Invoke(chain, "ActivatePurple");
            yield return WaitScaled(.42f);
            Assert.That(health.CurrentHealth, Is.EqualTo(beforePurple), "No damage during fusion hold");
            var orb = GameObject.Find("HollowPurpleDenseBody");
            Assert.That(orb, Is.Not.Null);
            yield return Capture("Combat_Purple_hold", orb.transform.position);
            yield return WaitScaled(.4f);
            yield return Capture("Combat_Purple_travel", orb.transform.position);
            yield return WaitScaled(.5f);
            Assert.That(health.CurrentHealth, Is.LessThan(beforePurple), "New outer gameplay corridor must hit");
            Assert.That(outside.GetComponent<Health>().CurrentHealth, Is.EqualTo(outside.GetComponent<Health>().MaxHealth));
            Assert.That(far.GetComponent<Health>().CurrentHealth, Is.EqualTo(far.GetComponent<Health>().MaxHealth), "Far hit must wait for travel");
            yield return WaitScaled(1f);
            Assert.That(far.GetComponent<Health>().CurrentHealth, Is.LessThan(far.GetComponent<Health>().MaxHealth));
            Assert.That(beyond.GetComponent<Health>().CurrentHealth, Is.EqualTo(beyond.GetComponent<Health>().MaxHealth));
            yield return Capture("Combat_Purple_scar", gojo.transform.position + forward * 9f);
            yield return WaitScaled(2.2f);
            Assert.That(Object.FindFirstObjectByType<PurpleTravelAftermath>(), Is.Null);
            Object.Destroy(centre.gameObject); Object.Destroy(outside.gameObject);
            Object.Destroy(far.gameObject); Object.Destroy(beyond.gameObject);
            yield return null;

            Place(bot.transform, gojo.transform.position + forward * 6f);
            bot.enabled = true;
            Vector3 originalGojo = gojo.transform.position, originalBot = bot.transform.position;
            float originalFov = Camera.main.fieldOfView;
            typeof(GojoDomainController).GetField("domainEnergyCost", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(domain, 0f);
            Invoke(domain, "ActivateDomain");
            yield return WaitScaled(.75f);
            yield return Capture("Combat_Domain_face");
            yield return WaitScaled(3.1f);
            var session = gojo.GetComponentInChildren<DomainPresentationSession>();
            Assert.That(session.IsEntered, Is.True);
            Assert.That(session.FullCinematic, Is.True);
            yield return WaitScaled(2.3f);
            yield return Capture("Combat_Domain_interior");
            float frozenUntil = (float)typeof(JJKGame.Enemy.CurseBotController).GetField("frozenUntil",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(bot);
            Assert.That(frozenUntil - Time.time, Is.GreaterThan(8f), "Victim duration must outlast arrival");
            Assert.That(domain.State, Is.EqualTo(GojoDomainController.DomainState.Active));
            domain.ResetCommand();
            Assert.That(Vector3.Distance(gojo.transform.position, originalGojo), Is.LessThan(.01f));
            Assert.That(Vector3.Distance(bot.transform.position, originalBot), Is.LessThan(.01f));
            Assert.That(Camera.main.fieldOfView, Is.EqualTo(originalFov).Within(.01f));
            Assert.That(JJKGame.CameraSystem.DomainCameraOverride.IsOwned(Camera.main), Is.False);
            Assert.That(gojo.GetComponent<CombatActionGate>().CanStartBasicAttack, Is.True);
            Assert.That(gojo.GetComponent<CombatActionGate>().CanStartTechnique, Is.False, "Burnout stays technique-only");
            Assert.That(gojo.GetComponent<BasicAttack>().TryAttack(), Is.True, "CombatMVP physical attack must recover");
        }

        private static void Place(Transform actor, Vector3 position)
        {
            var motor = actor.GetComponent<CharacterController>();
            bool enabled = motor != null && motor.enabled;
            if (motor != null) motor.enabled = false;
            actor.position = position;
            if (motor != null) motor.enabled = enabled;
            Physics.SyncTransforms();
        }

        private static void SetYaw(VfxLabOrbitCamera orbit, float angle) =>
            typeof(VfxLabOrbitCamera).GetField("yaw", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(orbit, angle);

        private static IEnumerator Capture(string label, Vector3? focus = null, Vector3? lookDirection = null)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) yield break;
            string output = System.IO.Path.Combine(Application.dataPath, "D:/JJK_game/unity/Logs/FourthPolishQA/CODEX_PREVIEW");
            System.IO.Directory.CreateDirectory(output);
            var target = new RenderTexture(1280, 720, 24, RenderTextureFormat.ARGB32);
            target.Create();
            RenderTexture previous = RenderTexture.active;
            var texture = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            Camera camera = Camera.main;
            RenderTexture previousTarget = camera.targetTexture;
            camera.targetTexture = target;
            Vector3 cameraPosition = camera.transform.position;
            Quaternion cameraRotation = camera.transform.rotation;
            bool asyncCompilation = UnityEditor.ShaderUtil.allowAsyncCompilation;
            var overlay = GameObject.Find("DomainTransitionOcclusion")?.GetComponent<Canvas>();
            if (overlay != null)
            {
                overlay.renderMode = RenderMode.ScreenSpaceCamera;
                overlay.worldCamera = camera;
                overlay.planeDistance = camera.nearClipPlane + .01f;
                Canvas.ForceUpdateCanvases();
            }
            try
            {
                UnityEditor.ShaderUtil.allowAsyncCompilation = false;
                foreach (Renderer renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material == null || material.shader == null) continue;
                    Assert.That(UnityEditor.ShaderUtil.ShaderHasError(material.shader), Is.False,
                        renderer.name + " shader failed: " + material.shader.name);
                }
                if (lookDirection.HasValue)
                {
                    camera.transform.position = focus.Value;
                    camera.transform.rotation = Quaternion.LookRotation(lookDirection.Value, Vector3.forward);
                }
                else if (focus.HasValue)
                {
                    camera.transform.position = focus.Value + new Vector3(4f, 3f, -6f);
                    camera.transform.LookAt(focus.Value);
                }
                UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,
                    new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest { destination = target });
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); texture.Apply();
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(output, label + ".png"), texture.EncodeToPNG());
            }
            finally
            {
                UnityEditor.ShaderUtil.allowAsyncCompilation = asyncCompilation;
                if (overlay != null) { overlay.renderMode = RenderMode.ScreenSpaceOverlay; overlay.worldCamera = null; }
                camera.targetTexture = previousTarget;
                camera.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
                RenderTexture.active = previous; target.Release(); Object.Destroy(target); Object.Destroy(texture);
            }
            yield return null;
        }
    }
}
