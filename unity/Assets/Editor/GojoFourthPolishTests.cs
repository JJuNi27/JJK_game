using System.Collections;
using System.Reflection;
using JJKGame.Core;
using JJKGame.Player;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace JJKGame.EditorTools
{
    public sealed class GojoFourthPolishTests
    {
        private static void Call(object o, string method, params object[] args) =>
            o.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(o, args);
        private static void Set(object o, string field, object value) =>
            o.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(o, value);
        private static IEnumerator Wait(float seconds)
        {
            float end = Time.time + seconds;
            double deadline = UnityEditor.EditorApplication.timeSinceStartup + 30;
            while (Time.time < end)
            {
                Assert.That(UnityEditor.EditorApplication.timeSinceStartup, Is.LessThan(deadline));
                yield return null;
            }
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            ProductionCombatInput.BasicAttackReplay = null;
            if (UnityEditor.EditorApplication.isPlaying) yield return new ExitPlayMode();
        }

        [UnityTest] public IEnumerator LaterLoadedCombatSceneInputStartsDamageAndPoseAfterDomain()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            yield return VerifyLaterLoadedScene();
            yield return new ExitPlayMode();
        }

        private static IEnumerator VerifyLaterLoadedScene()
        {
            // Reproduce entering CombatMVP after the initial scene bootstrap has already run.
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/CombatMVP.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            yield return Wait(.3f);
            var player = GameObject.Find("GojoPlayer");
            Assert.That(player.GetComponent<PrototypeFighterPresentationController>(), Is.Not.Null);
            var domain = player.GetComponent<GojoDomainController>();
            Set(domain, "domainEnergyCost", 0f);
            Call(domain, "ActivateDomain");
            var session = player.GetComponentInChildren<DomainPresentationSession>();
            Call(session, "Tick", 5f);
            Call(session, "Tick", 100f);
            var target = new GameObject("PhysicalTarget", typeof(Health), typeof(BoxCollider));
            Transform origin = player.transform.Find("AttackOrigin");
            target.transform.position = origin.position;
            Physics.SyncTransforms();
            int frame = Time.frameCount + 1;
            ProductionCombatInput.BasicAttackReplay = () => Time.frameCount == frame;
            yield return Wait(.08f);
            Assert.That(target.GetComponent<Health>().CurrentHealth, Is.LessThan(target.GetComponent<Health>().MaxHealth));
            Assert.That(player.GetComponent<BasicAttack>().DisplayChainStep, Is.EqualTo(1));
            var arm = player.transform.Find("PrototypeGojoAvatar/RightArm");
            Assert.That(Quaternion.Angle(arm.localRotation, Quaternion.identity), Is.GreaterThan(10f));
            Assert.That(player.GetComponent<CombatActionGate>().CanStartTechnique, Is.False);
            Assert.That(player.GetComponent<TechniqueBurnoutController>().IsBurnedOut, Is.True);
            ProductionCombatInput.BasicAttackReplay = null;
        }

        [UnityTest] public IEnumerator RedStopsAtCharacterAndWallButPassesDestructibleAndTrigger()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            for (int kind = 0; kind < 4; kind++)
            {
                var owner = new GameObject("Owner", typeof(Health));
                var obstacle = new GameObject("Obstacle", typeof(BoxCollider));
                obstacle.transform.position = Vector3.forward * 3;
                int impacts = 0;
                if (kind == 0) obstacle.AddComponent<Health>();
                if (kind == 2)
                {
                    var response = obstacle.AddComponent<RedCollisionResponse>();
                    response.onImpact.AddListener(_ => impacts++);
                }
                if (kind == 3) obstacle.GetComponent<Collider>().isTrigger = true;
                var behind = new GameObject("Behind", typeof(Health), typeof(BoxCollider));
                behind.transform.position = Vector3.forward * 6;
                var shot = new GameObject("Red", typeof(RedTechniqueProjectile)).GetComponent<RedTechniqueProjectile>();
                shot.Configure(owner.GetComponent<Health>(), Vector3.forward, 1000f, 10f, .5f, 10f, 0, 0, null, null);
                Physics.SyncTransforms();
                yield return Wait(.12f);
                Assert.That(shot == null, Is.True);
                Assert.That(behind.GetComponent<Health>().CurrentHealth < behind.GetComponent<Health>().MaxHealth,
                    Is.EqualTo(kind >= 2), "Response " + kind);
                if (kind == 2) Assert.That(impacts, Is.EqualTo(1));
                Object.Destroy(owner); Object.Destroy(obstacle); Object.Destroy(behind);
                yield return null;
            }
            yield return new ExitPlayMode();
        }

        [UnityTest] public IEnumerator PurpleScarTracksChangingVisibleDiameter()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.position = Vector3.down;
            ground.transform.localScale = new Vector3(50,1,50);
            var host = new GameObject("PurpleAftermath", typeof(PurpleTravelAftermath));
            var aftermath = host.GetComponent<PurpleTravelAftermath>();
            aftermath.Configure(Vector3.up, Vector3.forward, 4f);
            Physics.SyncTransforms();
            aftermath.Render(Vector3.up + Vector3.forward * .1f, 0f, 1f, true, 4f);
            float small = host.transform.Find("GroundSpaceWound_0").GetComponent<LineRenderer>().widthMultiplier;
            aftermath.Render(Vector3.up + Vector3.forward * 1f, .1f, 1f, true, 8f);
            float large = host.transform.Find("GroundSpaceWound_1").GetComponent<LineRenderer>().widthMultiplier;
            Assert.That(small, Is.GreaterThanOrEqualTo(4f * .9f));
            Assert.That(large / small, Is.InRange(1.95f,2.1f), "Doubling the visible orb must double the scar width");
            Object.Destroy(host); Object.Destroy(ground);
            yield return new ExitPlayMode();
        }

        [UnityTest] public IEnumerator WhiteBloodSubbeatAndWorldReleaseMoveWithWorldCoordinates()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            yield return new EnterPlayMode();
            var cameraHost = new GameObject("Camera", typeof(Camera)); cameraHost.tag = "MainCamera";
            cameraHost.transform.position = new Vector3(0,3,-7);
            var owner = new GameObject("Gojo", typeof(Health), typeof(GojoDomainController));
            owner.transform.position = new Vector3(3,0,2);
            var domain = owner.GetComponent<GojoDomainController>();
            Set(domain, "domainEnergyCost", 0f); Call(domain, "ActivateDomain");
            var session = owner.GetComponentInChildren<DomainPresentationSession>();
            var visual = owner.GetComponentInChildren<UnlimitedVoidProductionVisual>();
            Call(session, "Tick", 5f);
            Call(visual, "RenderArrival", 1.87f + .85f);
            Assert.That(visual.transform.Find("SuspendedWhiteBlood_1").gameObject.activeSelf, Is.True);
            Assert.That(visual.transform.Find("SuspendedWhiteBlood_4").gameObject.activeSelf, Is.False);
            Call(visual, "RenderArrival", 1.87f + 1f);
            Assert.That(visual.transform.Find("SuspendedWhiteBlood_4").gameObject.activeSelf, Is.True);
            float lifetime = (float)typeof(DomainPresentationSession).GetField("lifetime",
                BindingFlags.NonPublic | BindingFlags.Instance).GetValue(session);
            Call(session, "Tick", lifetime - .8f);
            Vector3 origin = session.ReleaseOrigin;
            Assert.That(origin, Is.EqualTo(new Vector3(3,0,2)));
            cameraHost.transform.position += Vector3.right * 4;
            Vector3 moved = cameraHost.transform.position;
            Call(session, "Tick", lifetime - .5f);
            Assert.That(session.ReleaseOrigin, Is.EqualTo(origin));
            Assert.That(cameraHost.transform.position, Is.EqualTo(moved));
            Assert.That(JJKGame.CameraSystem.DomainCameraOverride.IsOwned(cameraHost.GetComponent<Camera>()), Is.False);
            session.End();
            Object.Destroy(owner); Object.Destroy(cameraHost);
            yield return new ExitPlayMode();
        }
    }
}
