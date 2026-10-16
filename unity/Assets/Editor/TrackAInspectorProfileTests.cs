using System.Collections;
using System.Linq;
using System.Reflection;
using JJKGame.Core;
using JJKGame.Dev.VFXLab;
using JJKGame.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace JJKGame.EditorTools
{
    public sealed class TrackAInspectorProfileTests
    {
        private const string CharacterProfilePath =
            "Assets/Characters/Gojo/Data/Gojo VFXLab Character Profile.asset";
        private const string AudioProfilePath =
            "Assets/Characters/Gojo/Data/Gojo Combat Audio Profile.asset";

        [UnityTearDown]
        public IEnumerator ExitPlayModeAfterRuntimeProfileCheck()
        {
            if (EditorApplication.isPlaying)
            {
                yield return new ExitPlayMode();
            }
        }

        [TestCase("characterProfile", "캐릭터 프로필")]
        [TestCase("walkSpeed", "걷기 속도")]
        [TestCase("runSpeed", "달리기 속도")]
        [TestCase("basicAttack1Trigger", "평타 1타 트리거")]
        [TestCase("blueVoice", "아오 음성")]
        [TestCase("redVoice", "아카 음성")]
        [TestCase("purpleVoice", "무라사키 음성")]
        [TestCase("domainVoice", "무량공처 음성")]
        [TestCase("basicSwingSound", "평타 휘두르기 효과음")]
        [TestCase("sfxVolume", "효과음 볼륨")]
        [TestCase("voiceVolume", "음성 볼륨")]
        [TestCase("musicVolume", "음악 볼륨")]
        public void ExplicitEditorLabelTableUsesKoreanFieldLabels(
            string propertyName,
            string expected)
        {
            Assert.That(
                KoreanInspectorLabels.TextFor(propertyName, "English fallback"),
                Is.EqualTo(expected));
        }

        [Test]
        public void TargetInspectorsAreScopedAndUseTheSharedSerializedPropertyEditor()
        {
            GameObject owner = new GameObject("InspectorProfileTest");
            try
            {
                AssertEditor<VfxLabPreviewCharacter, VfxLabPreviewCharacterEditor>(
                    owner.AddComponent<VfxLabPreviewCharacter>());
                AssertEditor<VfxLabPreviewSequence, VfxLabPreviewSequenceEditor>(
                    owner.AddComponent<VfxLabPreviewSequence>());
                AssertEditor<PrototypeCombatAudio, PrototypeCombatAudioEditor>(
                    owner.AddComponent<PrototypeCombatAudio>());
                AssertEditor<TechniqueBurnoutController, TechniqueBurnoutControllerEditor>(
                    owner.AddComponent<TechniqueBurnoutController>());
                AssertEditor<BasicAttack, BasicAttackEditor>(owner.AddComponent<BasicAttack>());

                VfxLabCharacterProfile character =
                    ScriptableObject.CreateInstance<VfxLabCharacterProfile>();
                CombatAudioProfile audio = ScriptableObject.CreateInstance<CombatAudioProfile>();
                try
                {
                    AssertEditor<VfxLabCharacterProfile, VfxLabCharacterProfileEditor>(character);
                    AssertEditor<CombatAudioProfile, CombatAudioProfileEditor>(audio);
                }
                finally
                {
                    Object.DestroyImmediate(character);
                    Object.DestroyImmediate(audio);
                }
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void KoreanContentKeepsSerializedTooltipAndRangeMetadata()
        {
            GameObject owner = new GameObject("InspectorMetadataTest");
            try
            {
                PrototypeCombatAudio audio = owner.AddComponent<PrototypeCombatAudio>();
                SerializedObject serialized = new SerializedObject(audio);
                SerializedProperty volume = serialized.FindProperty("sfxVolume");
                GUIContent content = KoreanInspectorLabels.Content(volume);
                Assert.That(content.text, Is.EqualTo("효과음 볼륨"));
                Assert.That(content.tooltip, Does.Contain("전투 효과음"));

                FieldInfo volumeField = typeof(PrototypeCombatAudio).GetField(
                    "sfxVolume",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(volumeField.GetCustomAttribute<UnityEngine.RangeAttribute>(), Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void EveryRequestedUserFacingFieldHasAnExplicitKoreanLabel()
        {
            System.Type[] requestedTypes =
            {
                typeof(VfxLabPreviewCharacter),
                typeof(VfxLabPreviewSequence),
                typeof(VfxLabCharacterProfile),
                typeof(CharacterMovementProfile),
                typeof(EvadeProfile),
                typeof(CombatAudioProfile),
                typeof(TechniqueAudioProfile),
                typeof(PrototypeCombatAudio),
                typeof(DomainPresentationSettings),
                typeof(TechniqueBurnoutController),
                typeof(BasicAttack),
            };

            foreach (System.Type type in requestedTypes)
            {
                FieldInfo[] serializedFields = type.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    | BindingFlags.DeclaredOnly);
                foreach (FieldInfo field in serializedFields)
                {
                    bool serialized = !field.IsStatic && !field.IsNotSerialized
                        && (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null);
                    if (!serialized)
                    {
                        continue;
                    }

                    Assert.That(
                        KoreanInspectorLabels.TextFor(field.Name, null),
                        Is.Not.Null,
                        $"{type.Name}.{field.Name}");
                }
            }
        }

        [Test]
        public void GojoProfileIsEditModeAssetAndPreservesSceneValues()
        {
            VfxLabCharacterProfile profile =
                AssetDatabase.LoadAssetAtPath<VfxLabCharacterProfile>(CharacterProfilePath);
            CombatAudioProfile audio =
                AssetDatabase.LoadAssetAtPath<CombatAudioProfile>(AudioProfilePath);
            Assert.That(profile, Is.Not.Null);
            Assert.That(audio, Is.Not.Null);
            Assert.That(profile.CharacterId, Is.EqualTo(PrototypeCharacterId.GojoModern));
            Assert.That(profile.Movement, Is.Not.Null);
            Assert.That(profile.Movement.walkSpeed, Is.EqualTo(3f));
            Assert.That(profile.Movement.runSpeed, Is.EqualTo(14f));
            Assert.That(profile.Movement.evade.burstSpeed, Is.EqualTo(32f));
            Assert.That(profile.Movement.evade.movementDuration, Is.EqualTo(0.2f));
            Assert.That(profile.CombatAudio, Is.SameAs(audio));
            Assert.That(audio.skill1.voice, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(audio.skill1.voice),
                Is.EqualTo("Assets/Resources/LocalAudio/Gojo_Blue.ogg"));

            EditorSceneManager.OpenScene("Assets/Scenes/VFXLab.unity");
            VfxLabPreviewCharacter preview = Object.FindFirstObjectByType<VfxLabPreviewCharacter>();
            Assert.That(preview, Is.Not.Null);
            SerializedObject serializedPreview = new SerializedObject(preview);
            Assert.That(
                serializedPreview.FindProperty("characterProfile").objectReferenceValue,
                Is.SameAs(profile));
            Assert.That(preview.AudioProfile, Is.SameAs(audio));

            SerializedProperty legacyMovement = serializedPreview.FindProperty("movementProfile");
            Assert.That(legacyMovement.FindPropertyRelative("walkSpeed").floatValue, Is.EqualTo(3f));
            Assert.That(legacyMovement.FindPropertyRelative("runSpeed").floatValue, Is.EqualTo(14f));
        }

        [UnityTest]
        public IEnumerator GojoProfileFlowsIntoRuntimeCreatedPreviewAudio()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/VFXLab.unity");
            yield return new EnterPlayMode();
            yield return null;

            VfxLabCharacterProfile profile =
                AssetDatabase.LoadAssetAtPath<VfxLabCharacterProfile>(CharacterProfilePath);
            CombatAudioProfile audioProfile =
                AssetDatabase.LoadAssetAtPath<CombatAudioProfile>(AudioProfilePath);

            VfxLabPreviewCharacter preview = Object.FindFirstObjectByType<VfxLabPreviewCharacter>();
            VfxLabPreviewSequence sequence = Object.FindFirstObjectByType<VfxLabPreviewSequence>();
            PrototypeCombatAudio audio = Object.FindFirstObjectByType<PrototypeCombatAudio>();
            Assert.That(preview, Is.Not.Null);
            Assert.That(sequence, Is.Not.Null, "VfxLabController must still create the runtime sequence");
            Assert.That(audio, Is.Not.Null, "VfxLabPreviewSequence must still create the runtime audio host");
            Assert.That(preview.AudioProfile, Is.SameAs(audioProfile));

            FieldInfo presentationProfile = typeof(PrototypeCombatAudio).GetField(
                "presentationProfile",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(presentationProfile.GetValue(audio), Is.SameAs(audioProfile));

            PropertyInfo activeMovement = typeof(VfxLabPreviewCharacter).GetProperty(
                "ActiveMovementProfile",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(activeMovement.GetValue(preview), Is.SameAs(profile.Movement));
            Assert.That(profile.Movement.walkSpeed, Is.EqualTo(3f));
            Assert.That(profile.Movement.runSpeed, Is.EqualTo(14f));

            yield return new ExitPlayMode();
        }

        [Test]
        public void GenericSlotsAndLegacyAudioFallbackFieldsRemainAvailable()
        {
            CombatAudioProfile audio =
                AssetDatabase.LoadAssetAtPath<CombatAudioProfile>(AudioProfilePath);
            Assert.That(audio.GetTechnique(CharacterPresentationSkillSlot.Skill1), Is.SameAs(audio.skill1));
            Assert.That(audio.GetTechnique(CharacterPresentationSkillSlot.Skill2), Is.SameAs(audio.skill2));
            Assert.That(audio.GetTechnique(CharacterPresentationSkillSlot.Ultimate), Is.SameAs(audio.ultimate));
            Assert.That(audio.GetTechnique(CharacterPresentationSkillSlot.Domain), Is.SameAs(audio.domain));

            string[] fallbackFields =
            {
                "blueVoice", "redVoice", "purpleVoice", "domainVoice",
                "basicSwingSound", "basicHitSound", "sfxVolume", "voiceVolume", "musicVolume",
            };
            FieldInfo[] fields = typeof(PrototypeCombatAudio).GetFields(
                BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (string name in fallbackFields)
            {
                Assert.That(fields.Any(field => field.Name == name), Is.True, name);
            }

            MethodInfo loadFallbacks = typeof(PrototypeCombatAudio).GetMethod(
                "LoadLocalOverrides",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(loadFallbacks, Is.Not.Null);
        }

        private static void AssertEditor<TTarget, TEditor>(TTarget target)
            where TTarget : Object
            where TEditor : UnityEditor.Editor
        {
            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(target);
            try
            {
                Assert.That(editor, Is.TypeOf<TEditor>());
            }
            finally
            {
                Object.DestroyImmediate(editor);
            }
        }
    }
}
