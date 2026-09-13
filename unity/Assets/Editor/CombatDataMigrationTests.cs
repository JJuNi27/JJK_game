using System.Reflection;
using JJKGame.Core;
using JJKGame.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace JJKGame.EditorTools
{
    public sealed class CombatDataMigrationTests
    {
        [Test]
        public void CatalogLoadsGojoDefinitionWithProtectedValues()
        {
            CharacterCombatDefinition gojo = CharacterCombatCatalog.FindDefault(PrototypeCharacterId.GojoModern);
            Assert.That(gojo, Is.Not.Null);
            Assert.That(gojo.Stats.MaxHealth, Is.EqualTo(100f));
            Assert.That(gojo.CursedEnergy.MaxEnergy, Is.EqualTo(100f));
            Assert.That(gojo.CursedEnergy.RegenerationPerSecond, Is.EqualTo(12f));
            Assert.That(gojo.BasicAttack.StepCount, Is.EqualTo(3));
            Assert.That(gojo.BasicAttack.GetStep(0).Damage, Is.EqualTo(12f));
            Assert.That(gojo.BasicAttack.GetStep(1).Damage, Is.EqualTo(14f));
            Assert.That(gojo.BasicAttack.GetStep(2).Damage, Is.EqualTo(24f));
            Assert.That(gojo.GojoTechniques.Blue.Damage, Is.EqualTo(8f));
            Assert.That(gojo.GojoTechniques.Purple.Damage, Is.EqualTo(55f));
            Assert.That(gojo.Domain.CaptureRadius, Is.EqualTo(30f));
            Assert.That(gojo.Movement.walkSpeed, Is.EqualTo(3f));
            Assert.That(gojo.Movement.runSpeed, Is.EqualTo(14f));
        }

        [Test]
        public void BasicAttackProfileDamageReachesRuntimeHit()
        {
            BasicAttackProfile profile = ScriptableObject.CreateInstance<BasicAttackProfile>();
            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty steps = serialized.FindProperty("steps");
            steps.arraySize = 1;
            SerializedProperty step = steps.GetArrayElementAtIndex(0);
            step.FindPropertyRelative("damage").floatValue = 37f;
            step.FindPropertyRelative("cooldown").floatValue = 0.05f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            GameObject attacker = new GameObject("DataDrivenAttacker");
            GameObject target = new GameObject("DataDrivenTarget");
            try
            {
                attacker.AddComponent<Health>();
                attacker.AddComponent<TargetLockController>();
                BasicAttack attack = attacker.AddComponent<BasicAttack>();
                attack.ApplyProfile(profile);
                attack.Configure(attacker.transform, null);
                target.transform.position = attacker.transform.position + Vector3.forward * 0.5f;
                Health targetHealth = target.AddComponent<Health>();
                targetHealth.SetCurrentHealth(100f);
                target.AddComponent<SphereCollider>();
                Physics.SyncTransforms();

                MethodInfo performStep = typeof(BasicAttack).GetMethod(
                    "PerformAttackChainStep",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(performStep, Is.Not.Null);
                performStep.Invoke(attack, new object[] { 0 });
                Assert.That(targetHealth.CurrentHealth, Is.EqualTo(63f).Within(0.001f));
                Assert.That(attack.ChainLabel, Does.Contain("1 / 1"));
            }
            finally
            {
                Object.DestroyImmediate(attacker);
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(profile);
            }
        }

        [Test]
        public void EnergyAndPurpleProfilesReachRuntimeConsumers()
        {
            CursedEnergyProfile energyProfile = ScriptableObject.CreateInstance<CursedEnergyProfile>();
            SetFloat(energyProfile, "maxEnergy", 175f);
            SetFloat(energyProfile, "startingEnergy", 140f);
            GameObject owner = new GameObject("ProfileConsumer");
            GojoTechniqueGameplayProfile techniques = ScriptableObject.CreateInstance<GojoTechniqueGameplayProfile>();
            SerializedObject techniqueData = new SerializedObject(techniques);
            techniqueData.FindProperty("purple").FindPropertyRelative("damage").floatValue = 73f;
            techniqueData.ApplyModifiedPropertiesWithoutUndo();
            try
            {
                owner.AddComponent<Health>();
                CursedEnergyController energy = owner.AddComponent<CursedEnergyController>();
                energy.ApplyProfile(energyProfile);
                GojoTechniqueController technique = owner.AddComponent<GojoTechniqueController>();
                GojoTechniqueChainController chain = owner.AddComponent<GojoTechniqueChainController>();
                technique.ApplyProfile(techniques);
                chain.ApplyProfile(techniques);

                Assert.That(energy.MaxEnergy, Is.EqualTo(175f));
                Assert.That(energy.CurrentEnergy, Is.EqualTo(140f));
                Assert.That(ReadFloat(chain, "purpleDamage"), Is.EqualTo(73f));
            }
            finally
            {
                Object.DestroyImmediate(owner);
                Object.DestroyImmediate(energyProfile);
                Object.DestroyImmediate(techniques);
            }
        }

        private static void SetFloat(Object target, string property, float value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(property).floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static float ReadFloat(object target, string field)
        {
            return (float)target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        }
    }
}
