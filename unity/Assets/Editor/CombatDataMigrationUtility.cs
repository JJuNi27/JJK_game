using JJKGame.Core;
using JJKGame.Enemy;
using JJKGame.Player;
using UnityEditor;
using UnityEngine;

namespace JJKGame.EditorTools
{
    [InitializeOnLoad]
    public static class CombatDataMigrationUtility
    {
        private const string GojoData = "Assets/Characters/Gojo/Data";
        private const string SharedData = "Assets/Characters/Common/Data";
        private const string CatalogData = "Assets/Resources/CombatData";

        static CombatDataMigrationUtility()
        {
            EditorApplication.delayCall += EnsureInitialMigration;
        }

        private static void EnsureInitialMigration()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (AssetDatabase.LoadAssetAtPath<CharacterCombatCatalog>($"{CatalogData}/Character Combat Catalog.asset") == null)
                CreateOrRepairDefaultProfiles();
        }

        [MenuItem("JJK Game/Combat Data/Create Or Repair Default Profiles")]
        public static void CreateOrRepairDefaultProfiles()
        {
            EnsureFolder(GojoData); EnsureFolder(SharedData); EnsureFolder(CatalogData);

            CharacterStatsProfile stats = Create<CharacterStatsProfile>($"{GojoData}/Gojo Character Stats.asset");
            CursedEnergyProfile energy = Create<CursedEnergyProfile>($"{GojoData}/Gojo Cursed Energy Profile.asset");
            BasicAttackProfile basic = Create<BasicAttackProfile>($"{GojoData}/Gojo Basic Attack Profile.asset");
            GojoTechniqueGameplayProfile technique = Create<GojoTechniqueGameplayProfile>($"{GojoData}/Gojo Technique Gameplay Profile.asset");
            DomainGameplayProfile domain = Create<DomainGameplayProfile>($"{GojoData}/Gojo Unlimited Void Gameplay.asset");
            BurnoutPolicyProfile burnout = Create<BurnoutPolicyProfile>($"{GojoData}/Gojo Burnout Policy.asset");
            TargetingProfile targeting = Create<TargetingProfile>($"{GojoData}/Gojo Targeting Profile.asset");
            TrainingBotProfile normalBot = Create<TrainingBotProfile>($"{SharedData}/Training Bot Normal Profile.asset");
            TrainingBotProfile amplifiedBot = Create<TrainingBotProfile>($"{SharedData}/Training Bot Domain Amplification Profile.asset");
            Set(amplifiedBot, "attackMode", (int)TrainingAttackMode.DomainAmplification);

            CharacterCombatDefinition gojo = Create<CharacterCombatDefinition>($"{GojoData}/Gojo Combat Definition.asset");
            SerializedObject definition = new SerializedObject(gojo);
            definition.FindProperty("characterId").enumValueIndex = (int)PrototypeCharacterId.GojoModern;
            definition.FindProperty("displayName").stringValue = "GOJO SATORU";
            definition.FindProperty("stats").objectReferenceValue = stats;
            definition.FindProperty("cursedEnergy").objectReferenceValue = energy;
            definition.FindProperty("basicAttack").objectReferenceValue = basic;
            definition.FindProperty("gojoTechniques").objectReferenceValue = technique;
            definition.FindProperty("domain").objectReferenceValue = domain;
            definition.FindProperty("burnout").objectReferenceValue = burnout;
            definition.FindProperty("targeting").objectReferenceValue = targeting;
            SerializedProperty movement = definition.FindProperty("movement");
            movement.FindPropertyRelative("walkSpeed").floatValue = 3f;
            movement.FindPropertyRelative("runSpeed").floatValue = 14f;
            SerializedProperty traits = definition.FindProperty("traits");
            traits.arraySize = 2;
            traits.GetArrayElementAtIndex(0).enumValueIndex = (int)CharacterTraitId.SixEyes;
            traits.GetArrayElementAtIndex(1).enumValueIndex = (int)CharacterTraitId.Infinity;
            SerializedProperty passives = definition.FindProperty("passives");
            passives.arraySize = 2;
            passives.GetArrayElementAtIndex(0).enumValueIndex = (int)CharacterPassiveId.InfinityDefense;
            passives.GetArrayElementAtIndex(1).enumValueIndex = (int)CharacterPassiveId.TechniqueBurnoutRecovery;
            definition.ApplyModifiedPropertiesWithoutUndo();

            ConfigureGojoBasicAttack(basic);
            CharacterCombatCatalog catalog = Create<CharacterCombatCatalog>($"{CatalogData}/Character Combat Catalog.asset");
            SerializedObject catalogObject = new SerializedObject(catalog);
            SerializedProperty characters = catalogObject.FindProperty("characters");
            characters.arraySize = 1;
            characters.GetArrayElementAtIndex(0).objectReferenceValue = gojo;
            catalogObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(gojo); EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("Combat data profiles are ready. Existing assets were preserved; catalog links were repaired.");
        }

        private static void ConfigureGojoBasicAttack(BasicAttackProfile profile)
        {
            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty steps = serialized.FindProperty("steps");
            if (steps.arraySize == 0)
            {
                steps.arraySize = 3;
                SetStep(steps.GetArrayElementAtIndex(0), 12f, 0.24f, 4.5f, 0.12f, false, "gojo.basic.1");
                SetStep(steps.GetArrayElementAtIndex(1), 14f, 0.28f, 6f, 0.17f, false, "gojo.basic.2");
                SetStep(steps.GetArrayElementAtIndex(2), 24f, 0.52f, 11f, 0.38f, true, "gojo.basic.finisher");
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(profile);
            }
        }

        private static void SetStep(SerializedProperty step, float damage, float cooldown, float knockback, float stun, bool finisher, string tag)
        {
            step.FindPropertyRelative("damage").floatValue = damage;
            step.FindPropertyRelative("cooldown").floatValue = cooldown;
            step.FindPropertyRelative("knockback").floatValue = knockback;
            step.FindPropertyRelative("hitStun").floatValue = stun;
            step.FindPropertyRelative("finisher").boolValue = finisher;
            step.FindPropertyRelative("presentationTag").stringValue = tag;
        }

        private static T Create<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void Set(Object target, string propertyName, int enumValue)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).enumValueIndex = enumValue;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void EnsureFolder(string path)
        {
            string current = "Assets";
            string[] segments = path.Substring("Assets/".Length).Split('/');
            foreach (string segment in segments)
            {
                string next = $"{current}/{segment}";
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, segment);
                current = next;
            }
        }
    }
}
