using System.Collections;
using System.Linq;
using JJKGame.Core;
using JJKGame.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace JJKGame.Shibuya.EditorTools
{
    public sealed class ShibuyaPass1Tests
    {
        [Test]
        public void SceneAssetsAndKoreanEditorsAreConnected()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(ShibuyaSceneBuilder.ScenePath), Is.Not.Null);
            var profile = AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>(ShibuyaSceneBuilder.Root + "/Data/Shibuya Combat Profile.asset");
            Assert.That(profile.strikes[0].damage, Is.GreaterThan(0));
            Assert.That(profile.strikes[0].Duration, Is.GreaterThan(profile.strikes[0].startup + profile.strikes[0].activeTime));
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ShibuyaSceneBuilder.Root + "/Prefabs/ShibuyaPlayer.prefab");
            Assert.That(prefab.GetComponent<Health>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<ShibuyaFighter>().Profile, Is.SameAs(profile));
            Assert.That(prefab.GetComponent<BasicAttack>(), Is.Null);
            UnityEditor.Editor inspector = UnityEditor.Editor.CreateEditor(profile);
            try { Assert.That(inspector, Is.TypeOf<ShibuyaCombatProfileEditor>()); }
            finally { Object.DestroyImmediate(inspector); }
        }

        [UnityTest]
        public IEnumerator RealPlayLoopMovementHitVictoryDefeatAndRestart()
        {
            EditorSceneManager.OpenScene(ShibuyaSceneBuilder.ScenePath);
            yield return new EnterPlayMode();
            int firstFrame = Time.frameCount;
            var match = Object.FindFirstObjectByType<ShibuyaMatch>();
            Assert.That(match, Is.Not.Null);
            var player = match.Player;
            var dummy = match.Dummy;
            dummy.Health.SetCurrentHealth(80f); // Runtime fixture only; preserve user HP asset.
            var input = player.GetComponent<ShibuyaPlayerInput>();
            var camera = Object.FindFirstObjectByType<ShibuyaOrbitCamera>();
            input.TestInput = () => default;
            yield return Wait(0.35f);
            Assert.That(Time.frameCount - firstFrame, Is.GreaterThan(5), "Play Mode must advance actual frames");
            AssertNoLegacyActors();
            Capture("01-Initial.png");

            // This enters through the same input boundary and Updates as keyboard/mouse gameplay.
            Place(player, new Vector3(0, 0.03f, -12));
            input.TestInput = () => new ShibuyaInputFrame { move = Vector2.up };
            Vector3 start = player.transform.position;
            yield return Wait(0.5f);
            float walk = Horizontal(player.transform.position - start);
            input.TestInput = () => new ShibuyaInputFrame { move = Vector2.up, run = true };
            start = player.transform.position;
            yield return Wait(0.5f);
            float run = Horizontal(player.transform.position - start);
            Assert.That(walk, Is.GreaterThan(1.5f));
            Assert.That(run, Is.GreaterThan(walk * 1.3f));
            input.TestInput = () => default;
            yield return Wait(0.15f);

            float floor = player.transform.position.y;
            input.TestInput = () => new ShibuyaInputFrame { jump = true };
            yield return null;
            input.TestInput = () => default;
            yield return Wait(0.18f);
            Assert.That(player.transform.position.y - floor, Is.GreaterThan(0.3f));
            yield return Wait(1.3f);
            Assert.That(player.IsGrounded, Is.True, "jump must land");
            Assert.That(Mathf.Abs(player.transform.position.y - floor), Is.LessThan(0.12f));

            Quaternion oldView = camera.transform.rotation;
            input.TestInput = () => new ShibuyaInputFrame { look = new Vector2(4, 1) };
            yield return null;
            yield return null;
            input.TestInput = () => default;
            yield return null;
            Assert.That(Quaternion.Angle(oldView, camera.transform.rotation), Is.GreaterThan(5f));
            camera.ApplyLook(new Vector2(-8, -2));
            yield return null;

            // Wall collision uses the actor's single motor. No teleports through blocked displacement.
            Place(player, new Vector3(0, 0.03f, -12));
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "QA_Wall"; wall.transform.position = new Vector3(2, 1.5f, -12);
            wall.transform.localScale = new Vector3(0.4f, 3, 10);
            Physics.SyncTransforms();
            input.TestInput = () => new ShibuyaInputFrame { move = Vector2.right, run = true };
            yield return Wait(0.6f);
            input.TestInput = () => default;
            Assert.That(player.transform.position.x, Is.LessThan(1.6f));
            Object.Destroy(wall); yield return null;

            Place(player, new Vector3(0, 0.03f, -12));
            wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = new Vector3(0, 2, -14);
            wall.transform.localScale = new Vector3(8, 6, 0.4f);
            Physics.SyncTransforms();
            yield return Wait(0.4f);
            Assert.That(camera.transform.position.z, Is.GreaterThan(-13.8f), "camera must stay in front of an intervening wall");
            Object.Destroy(wall); yield return null;

            Place(player, new Vector3(0, 0.03f, -1.35f));
            player.transform.rotation = Quaternion.identity;
            Place(dummy, new Vector3(0, 0.03f, 0));
            // Extra colliders exercise per-attack Health deduplication.
            var extra = new GameObject("QA_ExtraHurtboxes"); extra.transform.SetParent(dummy.transform, false);
            extra.transform.localPosition = Vector3.up;
            extra.AddComponent<SphereCollider>().radius = 0.4f;
            extra.AddComponent<BoxCollider>().size = Vector3.one * 0.5f;
            Physics.SyncTransforms();
            int attacks = player.AcceptedAttackCount;
            input.TestInput = () => new ShibuyaInputFrame { attack = true };
            yield return null;
            input.TestInput = () => default;
            yield return Wait(player.Profile.strikes[0].startup + 0.03f);
            Assert.That(dummy.Health.CurrentHealth, Is.EqualTo(60f).Within(0.01f));
            Assert.That(dummy.State, Is.EqualTo(ShibuyaFighterState.Hitstunned));
            Assert.That(dummy.TryAttack(), Is.False, "hitstun blocks actions on the common fighter");
            float hitZ = dummy.transform.position.z;
            Capture("02-Hit.png");
            yield return Wait(0.5f);
            Assert.That(dummy.transform.position.z, Is.GreaterThan(hitZ + 0.1f));
            Assert.That(dummy.Health.CurrentHealth, Is.EqualTo(60f).Within(0.01f));
            Assert.That(player.AcceptedAttackCount, Is.EqualTo(attacks + 1));
            Assert.That(player.ConfirmedHitCount, Is.EqualTo(1));
            Object.Destroy(extra); yield return null;

            // Both combatants use the same reaction/motor, including the player.
            player.Health.ReceiveDamage(new DamageContext(5f, dummy.gameObject, DamageDeliveryType.PhysicalStrike));
            IHitReactable playerReaction = player;
            playerReaction.ApplyHitReaction(Vector3.right * 4f, 0.2f);
            Assert.That(player.State, Is.EqualTo(ShibuyaFighterState.Hitstunned));
            Assert.That(player.TryAttack(), Is.False);
            yield return Wait(0.4f);

            // A world collider must stop the same attack path.
            Place(player, new Vector3(0, 0.03f, -1.6f)); player.transform.rotation = Quaternion.identity;
            Place(dummy, new Vector3(0, 0.03f, 0));
            wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = new Vector3(0, 1, -0.8f); wall.transform.localScale = new Vector3(3, 2, 0.15f);
            Physics.SyncTransforms();
            Assert.That(player.TryAttack(), Is.True);
            yield return Wait(0.5f);
            Assert.That(dummy.Health.CurrentHealth, Is.EqualTo(60f).Within(0.01f));
            Object.Destroy(wall); yield return null;

            for (int i = 0; i < 3; i++)
            {
                Place(player, new Vector3(0, 0.03f, -1.35f)); player.transform.rotation = Quaternion.identity;
                Place(dummy, new Vector3(0, 0.03f, 0));
                input.TestInput = () => new ShibuyaInputFrame { attack = true };
                yield return null;
                input.TestInput = () => default;
                yield return Wait(0.5f);
            }
            Assert.That(match.Finished && match.Victory, Is.True);
            Assert.That(dummy.Health.CurrentHealth, Is.Zero);
            Assert.That(player.TryAttack(), Is.False);
            Vector3 finishedPosition = player.transform.position;
            input.TestInput = () => new ShibuyaInputFrame { move = Vector2.up, jump = true, attack = true };
            yield return Wait(0.2f);
            Assert.That(Vector3.Distance(player.transform.position, finishedPosition), Is.LessThan(0.001f));
            Capture("03-Victory.png");

            match.RestartBattle();
            yield return Wait(0.5f);
            match = Object.FindFirstObjectByType<ShibuyaMatch>();
            Assert.That(match.Finished, Is.False);
            Assert.That(match.Player.Health.CurrentHealth, Is.EqualTo(100));
            Assert.That(match.Dummy.Health.CurrentHealth, Is.EqualTo(match.Dummy.Health.MaxHealth));
            match.Player.GetComponent<ShibuyaPlayerInput>().TestInput = () => default;
            match.EditorDefeatPlayer();
            yield return null;
            Assert.That(match.Finished && !match.Victory, Is.True);
            Assert.That(match.Player.Health.CurrentHealth, Is.Zero);
            Capture("04-Defeat.png");
            match.RestartBattle();
            yield return Wait(0.5f);
            match = Object.FindFirstObjectByType<ShibuyaMatch>();
            match.Player.GetComponent<ShibuyaPlayerInput>().TestInput = () => default;
            Assert.That(match.Finished, Is.False);
            Assert.That(Object.FindObjectsByType<ShibuyaFighter>(FindObjectsSortMode.None).Length, Is.EqualTo(2));
            Assert.That(Object.FindObjectsByType<ShibuyaMatch>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(Resources.FindObjectsOfTypeAll<AudioClip>().Count(c => c.name == "Shibuya temporary impact"), Is.EqualTo(1));
            AssertNoLegacyActors();
            Debug.Log("SHIBUYA_PASS1_PLAYMODE: movement/run/jump/landing/look/wall/hit/dedup/stun/knockback/victory/defeat/restart/resource checks completed");
            yield return new ExitPlayMode();
        }

        private static IEnumerator Wait(float seconds)
        {
            float until = Time.time + seconds;
            while (Time.time < until) yield return null;
        }
        private static float Horizontal(Vector3 delta) => new Vector2(delta.x, delta.z).magnitude;
        private static void Place(ShibuyaFighter actor, Vector3 position)
        {
            var motor = actor.GetComponent<CharacterController>(); motor.enabled = false;
            actor.transform.position = position; motor.enabled = true; Physics.SyncTransforms();
        }
        private static void AssertNoLegacyActors()
        {
            Assert.That(Object.FindObjectsByType<BasicAttack>(FindObjectsSortMode.None), Is.Empty);
            Assert.That(Object.FindObjectsByType<GojoDomainController>(FindObjectsSortMode.None), Is.Empty);
            Assert.That(Object.FindObjectsByType<PrototypePlayerTeamController>(FindObjectsSortMode.None), Is.Empty);
            Assert.That(Object.FindObjectsByType<PrototypeCharacterController>(FindObjectsSortMode.None), Is.Empty);
            Assert.That(Object.FindObjectsByType<MatchController>(FindObjectsSortMode.None), Is.Empty);
            Assert.That(Object.FindObjectsByType<ProductionCombatHudCanvas>(FindObjectsSortMode.None), Is.Empty);
            Assert.That(Object.FindObjectsByType<GojoTechniqueController>(FindObjectsSortMode.None), Is.Empty);
            Assert.That(Object.FindObjectsByType<JJKGame.Enemy.PrototypeOpponentTeamController>(FindObjectsSortMode.None), Is.Empty);
            // Approved exception: the two pre-existing global runners remain dormant.
            var deck = Object.FindFirstObjectByType<PrototypeSkillDeckHud>();
            var beauty = Object.FindFirstObjectByType<PrototypeBeautyArenaPresentation>();
            Assert.That(deck, Is.Not.Null);
            Assert.That(beauty, Is.Not.Null);
            Assert.That(deck.GetComponentsInChildren<Canvas>(true), Is.Empty);
            Assert.That(GameObject.Find("__BeautyArenaPresentation"), Is.Null);
            Assert.That(Object.FindObjectsByType<PlayerCombatHudDataSource>(FindObjectsSortMode.None), Is.Empty);
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }
        private static void Capture(string name)
        {
            var camera = Camera.main;
            var rt = RenderTexture.GetTemporary(960, 600, 24);
            var texture = new Texture2D(960, 600, TextureFormat.RGB24, false);
            var old = camera.targetTexture; var active = RenderTexture.active;
            try
            {
                camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt;
                texture.ReadPixels(new Rect(0, 0, 960, 600), 0, 0); texture.Apply();
                System.IO.Directory.CreateDirectory(ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass1QA"));
                System.IO.File.WriteAllBytes(ShibuyaPass3CTestRunner.OutputPath("Logs/ShibuyaPass1QA/") + name, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = old; RenderTexture.active = active;
                RenderTexture.ReleaseTemporary(rt); Object.Destroy(texture);
            }
        }
    }
}
