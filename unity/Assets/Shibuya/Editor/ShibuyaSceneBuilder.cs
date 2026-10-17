using System.IO;
using JJKGame.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace JJKGame.Shibuya.EditorTools
{
    public static class ShibuyaSceneBuilder
    {
        public const string Root = "Assets/Shibuya";
        public const string ScenePath = Root + "/Scenes/ShibuyaCombatMVP.unity";

        [MenuItem("JJK Game/시부야 PASS1/전투 씬 열기")]
        public static void Open()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("JJK Game/시부야 PASS1/최초 씬 생성")]
        public static void Build()
        {
            if (File.Exists(ScenePath)) throw new System.InvalidOperationException("기존 시부야 씬을 덮어쓰지 않습니다. 전투 씬 열기를 사용하세요.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EnsureFolder(Root + "/Data"); EnsureFolder(Root + "/Materials");
            EnsureFolder(Root + "/Prefabs"); EnsureFolder(Root + "/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.72f, 0.76f, 0.82f);
            RenderSettings.fog = false;
            Light sun = new GameObject("Daylight").AddComponent<Light>();
            sun.type = LightType.Directional; sun.intensity = 1.25f; sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(48, -35, 0);

            Material road = MaterialAsset("Road", new Color(0.24f, 0.27f, 0.30f));
            Material pavement = MaterialAsset("Pavement", new Color(0.57f, 0.59f, 0.61f));
            Material marking = MaterialAsset("Marking", new Color(0.88f, 0.88f, 0.78f));
            Material building = MaterialAsset("Building", new Color(0.43f, 0.47f, 0.53f));
            Material window = MaterialAsset("Window", new Color(0.22f, 0.34f, 0.40f));
            Material playerColor = MaterialAsset("Player", new Color(0.18f, 0.43f, 0.70f));
            Material dummyColor = MaterialAsset("Dummy", new Color(0.76f, 0.33f, 0.19f));
            Material skin = MaterialAsset("BlockFace", new Color(0.78f, 0.72f, 0.62f));
            Material impact = MaterialAsset("Impact", new Color(1f, 0.84f, 0.4f));
            Transform map = new GameObject("ShibuyaBlockout").transform;
            Cube("RoadGround", map, new Vector3(0, -0.25f, 0), new Vector3(64, 0.5f, 64), road);
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                {
                    Cube("Sidewalk", map, new Vector3(x * 21, 0.08f, z * 21), new Vector3(26, 0.16f, 26), pavement);
                    float height = x == z ? 13 : 9;
                    Cube("Building", map, new Vector3(x * 23, height * 0.5f + 0.16f, z * 23), new Vector3(15, height, 15), building);
                    Cube("BuildingFacade", map, new Vector3(x * 15.45f, 3.5f, z * 23), new Vector3(0.06f, 5, 12), window, false);
                }
            for (int side = -1; side <= 1; side += 2)
                for (int stripe = -5; stripe <= 5; stripe++)
                {
                    Cube("Crosswalk", map, new Vector3(stripe, 0.015f, side * 10), new Vector3(0.52f, 0.02f, 3), marking, false);
                    Cube("Crosswalk", map, new Vector3(side * 10, 0.015f, stripe), new Vector3(3, 0.02f, 0.52f), marking, false);
                }
            for (int side = -1; side <= 1; side += 2)
            {
                Cube("MapBoundary", map, new Vector3(side * 32, 1.5f, 0), new Vector3(0.4f, 3, 64), pavement);
                Cube("MapBoundary", map, new Vector3(0, 1.5f, side * 32), new Vector3(64, 3, 0.4f), pavement);
            }

            ShibuyaCombatProfile profile = LoadOrCreate<ShibuyaCombatProfile>(Root + "/Data/Shibuya Combat Profile.asset");
            CharacterStatsProfile playerStats = Stats("Player Stats", 100);
            CharacterStatsProfile dummyStats = Stats("Dummy Stats", 80);
            ShibuyaFighter player = CreateActor("ShibuyaPlayer", profile, playerStats, playerColor, skin, true);
            ShibuyaFighter dummy = CreateActor("ShibuyaDummy", profile, dummyStats, dummyColor, skin, false);
            player.transform.position = new Vector3(0, 0.05f, -3.2f);
            dummy.transform.position = new Vector3(0, 0.05f, 0);
            dummy.transform.rotation = Quaternion.Euler(0, 180, 0);
            Camera camera = new GameObject("ShibuyaCamera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera"; camera.nearClipPlane = 0.08f; camera.farClipPlane = 150;
            camera.fieldOfView = 60; camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.65f, 0.72f, 0.80f);
            camera.transform.SetPositionAndRotation(new Vector3(0, 3.2f, -9.2f), Quaternion.Euler(16, 0, 0));
            ShibuyaOrbitCamera orbit = camera.gameObject.AddComponent<ShibuyaOrbitCamera>();
            orbit.Configure(player.transform, profile);
            player.GetComponent<ShibuyaPlayerInput>().Configure(orbit);
            PrefabUtility.RecordPrefabInstancePropertyModifications(player.transform);
            PrefabUtility.RecordPrefabInstancePropertyModifications(dummy.transform);
            PrefabUtility.RecordPrefabInstancePropertyModifications(player.GetComponent<ShibuyaPlayerInput>());
            ShibuyaMatch match = new GameObject("ShibuyaMatch").AddComponent<ShibuyaMatch>();
            match.Configure(player, dummy);
            ShibuyaHitFeedback feedback = new GameObject("ShibuyaHitFeedback").AddComponent<ShibuyaHitFeedback>();
            feedback.Configure(player, impact);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("SHIBUYA_PASS1_SCENE_CREATED " + ScenePath);
        }

        private static ShibuyaFighter CreateActor(string name, ShibuyaCombatProfile profile,
            CharacterStatsProfile stats, Material body, Material skin, bool isPlayer)
        {
            string path = Root + "/Prefabs/" + name + ".prefab";
            if (!File.Exists(path))
            {
                GameObject root = new GameObject(name);
                CharacterController motor = root.AddComponent<CharacterController>();
                motor.height = 1.8f; motor.center = Vector3.up * 0.9f; motor.radius = 0.35f;
                motor.stepOffset = 0.25f; motor.skinWidth = 0.03f; motor.minMoveDistance = 0;
                Health health = root.AddComponent<Health>();
                SerializedObject h = new SerializedObject(health);
                h.FindProperty("statsProfile").objectReferenceValue = stats; h.ApplyModifiedPropertiesWithoutUndo();
                ShibuyaFighter actor = root.AddComponent<ShibuyaFighter>(); actor.Configure(profile, stats);
                Transform visual = new GameObject("BlockVisual").transform; visual.SetParent(root.transform, false);
                Cube("Torso", visual, new Vector3(0, 1.13f, 0), new Vector3(0.65f, 0.65f, 0.36f), body, false);
                Cube("Head", visual, new Vector3(0, 1.7f, 0), new Vector3(0.44f, 0.44f, 0.44f), skin, false);
                Cube("FaceDirection", visual, new Vector3(0, 1.7f, 0.225f), new Vector3(0.30f, 0.08f, 0.025f), body, false);
                Transform armL = Limb("LeftArm", visual, new Vector3(-0.45f, 1.42f, 0), new Vector3(0.23f, 0.65f, 0.28f), body);
                Transform armR = Limb("RightArm", visual, new Vector3(0.45f, 1.42f, 0), new Vector3(0.23f, 0.65f, 0.28f), body);
                Transform legL = Limb("LeftLeg", visual, new Vector3(-0.18f, 0.8f, 0), new Vector3(0.27f, 0.75f, 0.32f), body);
                Transform legR = Limb("RightLeg", visual, new Vector3(0.18f, 0.8f, 0), new Vector3(0.27f, 0.75f, 0.32f), body);
                root.AddComponent<ShibuyaBlockVisual>().Configure(visual, armL, armR, legL, legR);
                if (isPlayer) root.AddComponent<ShibuyaPlayerInput>();
                PrefabUtility.SaveAsPrefabAsset(root, path); Object.DestroyImmediate(root);
            }
            return ((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path))).GetComponent<ShibuyaFighter>();
        }
        private static Transform Limb(string name, Transform parent, Vector3 position, Vector3 size, Material material)
        {
            Transform pivot = new GameObject(name).transform;
            pivot.SetParent(parent, false); pivot.localPosition = position;
            Cube("Block", pivot, Vector3.down * size.y * 0.5f, size, material, false);
            return pivot;
        }
        private static GameObject Cube(string name, Transform parent, Vector3 position, Vector3 size, Material material, bool collision = true)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube); cube.name = name;
            cube.transform.SetParent(parent, false); cube.transform.localPosition = position; cube.transform.localScale = size;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            if (!collision) Object.DestroyImmediate(cube.GetComponent<Collider>());
            return cube;
        }
        private static Material MaterialAsset(string name, Color color)
        {
            string path = Root + "/Materials/" + name + ".mat";
            Material result = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (result != null) return result;
            result = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            result.SetFloat("_Smoothness", 0.15f);
            AssetDatabase.CreateAsset(result, path); return result;
        }
        private static CharacterStatsProfile Stats(string name, float hp)
        {
            string path = Root + "/Data/" + name + ".asset";
            CharacterStatsProfile existing = AssetDatabase.LoadAssetAtPath<CharacterStatsProfile>(path);
            if (existing != null) return existing;
            CharacterStatsProfile result = LoadOrCreate<CharacterStatsProfile>(path);
            SerializedObject data = new SerializedObject(result); data.FindProperty("maxHealth").floatValue = hp;
            data.ApplyModifiedPropertiesWithoutUndo(); return result;
        }
        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T result = AssetDatabase.LoadAssetAtPath<T>(path);
            if (result != null) return result;
            result = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(result, path); return result;
        }
        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder).Replace('\\', '/'); EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
