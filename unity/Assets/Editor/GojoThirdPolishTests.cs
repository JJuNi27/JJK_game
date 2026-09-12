using System.Collections;
using System.Reflection;
using JJKGame.Core;
using JJKGame.Player;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace JJKGame.EditorTools
{
    public sealed class GojoThirdPolishTests
    {
        private static void Call(object target, string method, params object[] args) =>
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
        private static void Set(object target, string field, object value) =>
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        [UnityTearDown]
        public IEnumerator LeavePlayMode()
        {
            if (UnityEditor.EditorApplication.isPlaying) yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator DomainPresentationEndMustReleaseMeleeGate()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            var owner = new GameObject("Gojo", typeof(Health), typeof(GojoDomainController));
            var domain = owner.GetComponent<GojoDomainController>();
            var gate = owner.GetComponent<CombatActionGate>();
            Set(domain, "domainEnergyCost", 0f);
            Call(domain, "ActivateDomain");
            var session = owner.GetComponentInChildren<DomainPresentationSession>();
            Assert.That(gate.CanStartBasicAttack, Is.False);
            // The presentation owns its own terminal paths (expiry, disable, destruction).
            // Reproduce a presentation ending before the gameplay Update sees its deadline.
            session.End();
            Assert.That(domain.State, Is.EqualTo(GojoDomainController.DomainState.Normal));
            Assert.That(gate.CanStartBasicAttack, Is.True, "Restored world must not retain DomainActive input lock");
            Assert.That(owner.GetComponent<TechniqueBurnoutController>().IsBurnedOut, Is.True);
            Assert.That(gate.CanStartTechnique, Is.False);
            Object.Destroy(owner);
            yield return null;
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator DomainTerminalPathsRestorePhysicalDamageAndKeepBurnout()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            for (int path = 0; path < 5; path++)
            {
                var owner = new GameObject("Gojo", typeof(Health), typeof(GojoDomainController), typeof(BasicAttack));
                var target = new GameObject("MeleeTarget", typeof(Health), typeof(BoxCollider));
                target.transform.position = Vector3.forward;
                var domain = owner.GetComponent<GojoDomainController>();
                var attack = owner.GetComponent<BasicAttack>();
                Set(domain, "domainEnergyCost", 0f);
                Call(domain, "ActivateDomain");
                var session = owner.GetComponentInChildren<DomainPresentationSession>();
                Assert.That(attack.TryAttack(), Is.False);
                if (path == 0) { Set(domain, "activeLifetime", -1f); Call(domain, "UpdateStateTimeouts"); }
                if (path == 1) domain.ResetCommand();
                if (path == 2) { domain.enabled = false; domain.enabled = true; }
                if (path == 3) session.gameObject.SetActive(false);
                if (path == 4) Call(session, "Tick", 100f);
                Assert.That(domain.State, Is.EqualTo(GojoDomainController.DomainState.Normal), "Path " + path);
                Assert.That(domain.CapturesMouseInput, Is.False);
                var gate = owner.GetComponent<CombatActionGate>();
                Assert.That(gate.TechniqueBurnedOut, Is.True);
                Assert.That(gate.CanStartTechnique, Is.False);
                Physics.SyncTransforms();
                float hp = target.GetComponent<Health>().CurrentHealth;
                Assert.That(attack.TryAttack(), Is.True, "Path " + path);
                Assert.That(target.GetComponent<Health>().CurrentHealth, Is.LessThan(hp));
                Object.Destroy(owner); Object.Destroy(target);
                yield return null;
            }
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator RedReachesExtendedRangeWithoutFinalStepOvershoot()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            var owner = new GameObject("Gojo", typeof(Health));
            var near = new GameObject("At24m", typeof(Health), typeof(BoxCollider));
            var beyond = new GameObject("BeyondRange", typeof(Health), typeof(BoxCollider));
            near.transform.position = Vector3.forward * 24f;
            beyond.transform.position = Vector3.forward * 30f;
            var shot = new GameObject("Red", typeof(RedTechniqueProjectile)).GetComponent<RedTechniqueProjectile>();
            shot.Configure(owner.GetComponent<Health>(), Vector3.forward, GojoRedProductionDefaults.ProjectileSpeed,
                GojoRedProductionDefaults.Range, GojoRedProductionDefaults.Radius, 18f, 0f, 0f, null, null);
            Physics.SyncTransforms();
            double deadline = UnityEditor.EditorApplication.timeSinceStartup + 15;
            while (shot != null)
            {
                Assert.That(UnityEditor.EditorApplication.timeSinceStartup, Is.LessThan(deadline));
                yield return null;
            }
            Assert.That(near.GetComponent<Health>().CurrentHealth, Is.LessThan(near.GetComponent<Health>().MaxHealth));
            Assert.That(beyond.GetComponent<Health>().CurrentHealth, Is.EqualTo(beyond.GetComponent<Health>().MaxHealth));
            // Simulate a long frame: its swept query must stop at the same endpoint.
            var fast = new GameObject("RedLongFrame", typeof(RedTechniqueProjectile)).GetComponent<RedTechniqueProjectile>();
            fast.Configure(owner.GetComponent<Health>(), Vector3.forward, 10000f,
                GojoRedProductionDefaults.Range, GojoRedProductionDefaults.Radius, 18f, 0f, 0f, null, null);
            Call(fast, "Update");
            Assert.That(beyond.GetComponent<Health>().CurrentHealth, Is.EqualTo(beyond.GetComponent<Health>().MaxHealth));
            Object.Destroy(fast.gameObject);
            Object.Destroy(owner); Object.Destroy(near); Object.Destroy(beyond);
            yield return null;
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator CenterOutwardReleaseRestoresBeforeRevealAndCleansOnInterruption()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            var cameraHost = new GameObject("Camera", typeof(Camera)); cameraHost.tag = "MainCamera";
            cameraHost.transform.position = new Vector3(0,3,-7);
            Camera view = cameraHost.GetComponent<Camera>();
            for (int repeat = 0; repeat < 3; repeat++)
            {
                var owner = new GameObject("Gojo", typeof(Health), typeof(GojoDomainController));
                var domain = owner.GetComponent<GojoDomainController>();
                Set(domain, "domainEnergyCost", 0f);
                Call(domain, "ActivateDomain");
                var session = owner.GetComponentInChildren<DomainPresentationSession>();
                Call(session, "Tick", 4f);
                Assert.That(session.IsEntered, Is.True);
                float lifetime = (float)typeof(DomainPresentationSession).GetField("lifetime",
                    BindingFlags.Instance | BindingFlags.NonPublic).GetValue(session);
                Call(session, "Tick", lifetime - .65f);
                Assert.That(session.IsReleasing, Is.True);
                Assert.That(session.IsEntered, Is.False);
                Assert.That(owner.transform.position, Is.EqualTo(Vector3.zero));
                Assert.That(cameraHost.transform.position, Is.EqualTo(new Vector3(0,3,-7)));
                Assert.That(session.ReleaseProgress, Is.InRange(.1f,.9f));
                Assert.That(JJKGame.CameraSystem.DomainCameraOverride.IsOwned(view), Is.False);
                if (repeat == 0) Call(session, "Tick", lifetime + .1f);
                else if (repeat == 1) domain.ResetCommand();
                else { owner.GetComponent<Health>().Kill(); Call(domain, "Update"); }
                Assert.That(JJKGame.CameraSystem.DomainCameraOverride.IsOwned(view), Is.False);
                Assert.That(owner.transform.position, Is.EqualTo(Vector3.zero));
                Assert.That(domain.State, Is.EqualTo(GojoDomainController.DomainState.Normal));
                Object.Destroy(owner);
                yield return null;
                Assert.That(GameObject.Find("DomainTransitionOcclusion"), Is.Null);
                foreach (RenderTexture texture in Resources.FindObjectsOfTypeAll<RenderTexture>())
                    Assert.That(texture.name, Is.Not.EqualTo("DomainReleaseInteriorFrame"));
            }
            Object.Destroy(cameraHost);
            yield return null;
            yield return new ExitPlayMode();
        }
    }
}
