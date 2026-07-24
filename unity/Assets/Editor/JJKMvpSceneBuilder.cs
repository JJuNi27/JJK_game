using System.Linq;
using JJKGame.CameraSystem;
using JJKGame.Core;
using JJKGame.Enemy;
using JJKGame.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.EditorTools
{
    public static class JJKMvpSceneBuilder
    {
        private const string SceneFolder = "Assets/Scenes";
        private const string GeneratedFolder = "Assets/Generated";
        private const string ScenePath = SceneFolder + "/CombatMVP.unity";

        [MenuItem("Tools/JJK Game/Build Combat MVP Scene")]
        public static void BuildScene()
        {
            EnsureFolder(SceneFolder);
            EnsureFolder(GeneratedFolder);

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single
            );

            Material groundMaterial = CreateMaterial(
                GeneratedFolder + "/Ground.mat",
                new Color(0.12f, 0.13f, 0.16f)
            );
            Material playerMaterial = CreateMaterial(
                GeneratedFolder + "/GojoPrototype.mat",
                new Color(0.35f, 0.58f, 0.95f)
            );
            Material enemyMaterial = CreateMaterial(
                GeneratedFolder + "/CursePrototype.mat",
                new Color(0.62f, 0.12f, 0.16f)
            );

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "ArenaGround";
            ground.transform.localScale = new Vector3(3f, 1f, 3f);
            ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;

            GameObject player = CreateCharacterCapsule(
                "GojoPlayer",
                new Vector3(0f, 1f, -4f),
                playerMaterial
            );
            player.tag = "Player";
            Health playerHealth = player.AddComponent<Health>();
            player.AddComponent<ThirdPersonPlayerController>();
            GojoDomainController domainController = player.AddComponent<GojoDomainController>();
            BasicAttack basicAttack = player.AddComponent<BasicAttack>();

            GameObject attackOriginObject = new GameObject("AttackOrigin");
            attackOriginObject.transform.SetParent(player.transform);
            attackOriginObject.transform.localPosition = new Vector3(0f, 0.9f, 1.15f);
            basicAttack.Configure(attackOriginObject.transform, domainController);

            GameObject enemy = CreateCharacterCapsule(
                "CurseBot",
                new Vector3(0f, 1f, 6f),
                enemyMaterial
            );
            Health enemyHealth = enemy.AddComponent<Health>();
            enemy.AddComponent<CurseBotController>();

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 7f, -12f);
            cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            SimpleCameraFollow cameraFollow = cameraObject.AddComponent<SimpleCameraFollow>();
            cameraFollow.SetTarget(player.transform);

            GameObject lightObject = new GameObject("Directional Light");
            lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            Light directionalLight = lightObject.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.intensity = 1.25f;

            GameObject matchObject = new GameObject("MatchController");
            MatchController matchController = matchObject.AddComponent<MatchController>();
            matchController.Configure(playerHealth, enemyHealth, domainController);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = player;
            EditorUtility.DisplayDialog(
                "JJK Combat MVP",
                "CombatMVP 씬을 만들었습니다. Play 버튼을 눌러 테스트하세요.",
                "확인"
            );
        }

        private static GameObject CreateCharacterCapsule(
            string objectName,
            Vector3 position,
            Material material
        )
        {
            GameObject character = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            character.name = objectName;
            character.transform.position = position;
            character.GetComponent<Renderer>().sharedMaterial = material;

            Collider primitiveCollider = character.GetComponent<Collider>();
            if (primitiveCollider != null)
            {
                Object.DestroyImmediate(primitiveCollider);
            }

            CharacterController characterController = character.AddComponent<CharacterController>();
            characterController.height = 2f;
            characterController.radius = 0.5f;
            characterController.center = Vector3.zero;
            return character;
        }

        private static Material CreateMaterial(string path, Color color)
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                existing.color = color;
                EditorUtility.SetDirty(existing);
                return existing;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader)
            {
                color = color,
            };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string[] segments = path.Split('/');
            string current = segments[0];

            for (int index = 1; index < segments.Length; index++)
            {
                string next = current + "/" + segments[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[index]);
                }
                current = next;
            }
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] currentScenes = EditorBuildSettings.scenes;
            if (currentScenes.Any(scene => scene.path == scenePath))
            {
                return;
            }

            EditorBuildSettings.scenes = currentScenes
                .Concat(new[] { new EditorBuildSettingsScene(scenePath, true) })
                .ToArray();
        }
    }
}
