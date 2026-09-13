using System;
using System.Reflection;
using JJKGame.Core;
using JJKGame.Enemy;
using JJKGame.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace JJKGame.EditorTools
{
    public sealed class CombatDataInspectorLocalizationTests
    {
        private const string BasicAttackPath =
            "Assets/Characters/Gojo/Data/Gojo Basic Attack Profile.asset";
        private const string TechniquesPath =
            "Assets/Characters/Gojo/Data/Gojo Technique Gameplay Profile.asset";
        private const string DefinitionPath =
            "Assets/Characters/Gojo/Data/Gojo Combat Definition.asset";
        private const string CatalogPath =
            "Assets/Resources/CombatData/Character Combat Catalog.asset";

        [TestCase("attackRadius", "공격 반경")]
        [TestCase("comboResetDelay", "콤보 초기화 시간")]
        [TestCase("hitComboResetDelay", "연속 적중 초기화 시간")]
        [TestCase("steps", "공격 단계")]
        [TestCase("finisher", "마무리 공격")]
        [TestCase("occursAfterDomain", "영역 종료 후 번아웃 발생")]
        [TestCase("baseAttackMultiplier", "기본 공격 배율")]
        [TestCase("gojoTechniques", "술식 게임플레이")]
        [TestCase("blue", "순전 「창」 (Blue)")]
        [TestCase("red", "반전 「혁」 (Red)")]
        [TestCase("purple", "허식 「자」 (Purple)")]
        [TestCase("blueRedSynergy", "창 → 혁 연계")]
        [TestCase("pullSpeed", "끌어당김 속도")]
        [TestCase("pushSpeed", "밀어내기 속도")]
        [TestCase("readyTimeout", "영역 준비 제한 시간")]
        [TestCase("captureRadius", "포획 반경")]
        [TestCase("attackMode", "공격 모드")]
        public void CombatDataFieldsUseExplicitKoreanLabels(string propertyName, string expected)
        {
            Assert.That(KoreanInspectorLabels.TextFor(propertyName, "English fallback"),
                Is.EqualTo(expected));
        }

        [Test]
        public void EveryCombatDataFieldHasAnExplicitKoreanLabel()
        {
            Type[] types =
            {
                typeof(CharacterStatsProfile),
                typeof(CursedEnergyProfile),
                typeof(BasicAttackProfile),
                typeof(BasicAttackStep),
                typeof(GojoTechniqueGameplayProfile),
                typeof(BlueGameplayData),
                typeof(RedGameplayData),
                typeof(PurpleGameplayData),
                typeof(BlueRedSynergyData),
                typeof(DomainGameplayProfile),
                typeof(BurnoutPolicyProfile),
                typeof(TargetingProfile),
                typeof(CharacterCombatDefinition),
                typeof(CharacterCombatCatalog),
                typeof(TrainingBotProfile),
            };

            foreach (Type type in types)
            {
                foreach (FieldInfo field in type.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    | BindingFlags.DeclaredOnly))
                {
                    bool serialized = !field.IsStatic && !field.IsNotSerialized
                        && (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null);
                    if (!serialized)
                    {
                        continue;
                    }

                    Assert.That(KoreanInspectorLabels.TextFor(field.Name, null), Is.Not.Null,
                        $"{type.Name}.{field.Name}");
                }
            }
        }

        [Test]
        public void CombatDataAssetsResolveToScopedSerializedPropertyEditors()
        {
            AssertEditor<CharacterStatsProfile, CharacterStatsProfileEditor>();
            AssertEditor<CursedEnergyProfile, CursedEnergyProfileEditor>();
            AssertEditor<BasicAttackProfile, BasicAttackProfileEditor>();
            AssertEditor<GojoTechniqueGameplayProfile, GojoTechniqueGameplayProfileEditor>();
            AssertEditor<DomainGameplayProfile, DomainGameplayProfileEditor>();
            AssertEditor<BurnoutPolicyProfile, BurnoutPolicyProfileEditor>();
            AssertEditor<TargetingProfile, TargetingProfileEditor>();
            AssertEditor<CharacterCombatDefinition, CharacterCombatDefinitionEditor>();
            AssertEditor<CharacterCombatCatalog, CharacterCombatCatalogEditor>();
            AssertEditor<TrainingBotProfile, TrainingBotProfileEditor>();
        }

        [Test]
        public void ActualAssetsKeepArraysFoldoutsObjectReferencesAndStepTooltip()
        {
            BasicAttackProfile basic = AssetDatabase.LoadAssetAtPath<BasicAttackProfile>(BasicAttackPath);
            GojoTechniqueGameplayProfile techniques =
                AssetDatabase.LoadAssetAtPath<GojoTechniqueGameplayProfile>(TechniquesPath);
            CharacterCombatDefinition definition =
                AssetDatabase.LoadAssetAtPath<CharacterCombatDefinition>(DefinitionPath);
            CharacterCombatCatalog catalog =
                AssetDatabase.LoadAssetAtPath<CharacterCombatCatalog>(CatalogPath);

            Assert.That(basic, Is.Not.Null);
            Assert.That(techniques, Is.Not.Null);
            Assert.That(definition, Is.Not.Null);
            Assert.That(catalog, Is.Not.Null);

            SerializedProperty steps = new SerializedObject(basic).FindProperty("steps");
            Assert.That(steps.isArray, Is.True);
            Assert.That(steps.arraySize, Is.GreaterThan(0));
            SerializedProperty cooldown = steps.GetArrayElementAtIndex(0)
                .FindPropertyRelative("cooldown");
            Assert.That(KoreanInspectorLabels.Content(cooldown, "입력 간격").text,
                Is.EqualTo("입력 간격"));
            Assert.That(KoreanInspectorLabels.Content(steps).tooltip,
                Does.Contain("실제 평타 타수"));

            SerializedObject techniqueData = new SerializedObject(techniques);
            Assert.That(techniqueData.FindProperty("blue").hasVisibleChildren, Is.True);
            Assert.That(techniqueData.FindProperty("red").hasVisibleChildren, Is.True);
            Assert.That(techniqueData.FindProperty("purple").hasVisibleChildren, Is.True);
            Assert.That(techniqueData.FindProperty("blueRedSynergy").hasVisibleChildren, Is.True);

            SerializedObject definitionData = new SerializedObject(definition);
            string[] references =
            {
                "stats", "cursedEnergy", "basicAttack", "gojoTechniques", "domain",
                "burnout", "targeting", "trainingBot",
            };
            foreach (string propertyName in references)
            {
                SerializedProperty reference = definitionData.FindProperty(propertyName);
                Assert.That(reference.propertyType, Is.EqualTo(SerializedPropertyType.ObjectReference),
                    propertyName);
                if (propertyName != "trainingBot")
                {
                    Assert.That(reference.objectReferenceValue, Is.Not.Null, propertyName);
                }
            }

            SerializedProperty characters = new SerializedObject(catalog).FindProperty("characters");
            Assert.That(characters.isArray, Is.True);
            Assert.That(characters.arraySize, Is.GreaterThan(0));
            Assert.That(characters.GetArrayElementAtIndex(0).objectReferenceValue, Is.Not.Null);
        }

        [Test]
        public void EveryRequestedCombatDataAssetUsesTheLocalizedInspector()
        {
            string[] assetPaths =
            {
                "Assets/Characters/Gojo/Data/Gojo Character Stats.asset",
                "Assets/Characters/Gojo/Data/Gojo Cursed Energy Profile.asset",
                BasicAttackPath,
                TechniquesPath,
                "Assets/Characters/Gojo/Data/Gojo Unlimited Void Gameplay.asset",
                "Assets/Characters/Gojo/Data/Gojo Burnout Policy.asset",
                "Assets/Characters/Gojo/Data/Gojo Targeting Profile.asset",
                DefinitionPath,
                "Assets/Characters/Gojo/Data/Gojo Combat Audio Profile.asset",
                "Assets/Characters/Gojo/Data/Gojo VFXLab Character Profile.asset",
                "Assets/Characters/Common/Data/Training Bot Normal Profile.asset",
                "Assets/Characters/Common/Data/Training Bot Domain Amplification Profile.asset",
                CatalogPath,
            };

            foreach (string assetPath in assetPaths)
            {
                UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                Assert.That(asset, Is.Not.Null, assetPath);
                UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(asset);
                try
                {
                    Assert.That(editor, Is.InstanceOf<KoreanSerializedObjectEditor>(), assetPath);
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(editor);
                }
            }
        }

        [TestCase("pulseInterval", "순전 「창」 유지 중 피해 판정이 반복되는 시간 간격입니다.")]
        [TestCase("captureRadius", "영역 발동 시 대상이 포획되는 게임플레이 판정 반경입니다.")]
        public void ConfusingValuesUseConciseKoreanTooltips(string propertyName, string expected)
        {
            Assert.That(KoreanInspectorLabels.TooltipFor(propertyName, null), Is.EqualTo(expected));
        }

        [Test]
        public void NestedCombatDataTypesHaveScopedPropertyDrawers()
        {
            AssertDrawer<BasicAttackStepDrawer>();
            AssertDrawer<BlueGameplayDataDrawer>();
            AssertDrawer<RedGameplayDataDrawer>();
            AssertDrawer<PurpleGameplayDataDrawer>();
            AssertDrawer<BlueRedSynergyDataDrawer>();
        }

        private static void AssertEditor<TTarget, TEditor>()
            where TTarget : ScriptableObject
            where TEditor : UnityEditor.Editor
        {
            TTarget target = ScriptableObject.CreateInstance<TTarget>();
            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(target);
            try
            {
                Assert.That(editor, Is.TypeOf<TEditor>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(editor);
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static void AssertDrawer<TDrawer>() where TDrawer : PropertyDrawer
        {
            Assert.That(typeof(TDrawer).GetCustomAttributes(typeof(CustomPropertyDrawer), false),
                Has.Length.EqualTo(1));
        }
    }
}
