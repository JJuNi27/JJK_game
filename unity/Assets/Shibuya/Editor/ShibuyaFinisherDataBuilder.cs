using System;
using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    public static class ShibuyaFinisherDataBuilder
    {
        public const string Path = "Assets/Shibuya/Data/Shibuya Ground Finishers.asset";
        public static void CreateAndConnect()
        {
            if (EditorApplication.isPlaying || UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Play Mode 종료 및 Scene 저장 상태 확인이 필요합니다.");
            var profile = AssetDatabase.LoadAssetAtPath<ShibuyaCombatProfile>("Assets/Shibuya/Data/Shibuya Combat Profile.asset");
            var set = AssetDatabase.LoadAssetAtPath<ShibuyaFinisherSet>(Path);
            if (profile.finishers != null && profile.finishers != set)
                throw new InvalidOperationException("기존 마무리 연결을 덮어쓸 수 없습니다.");
            if (set == null) { set = Create(); AssetDatabase.CreateAsset(set, Path); }
            var serialized = new SerializedObject(profile);
            serialized.FindProperty("finishers").objectReferenceValue = set;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(profile); // Never save unrelated assets or scenes.
        }
        public static ShibuyaFinisherSet Create()
        {
            var data = ScriptableObject.CreateInstance<ShibuyaFinisherSet>();
            data.uppercut = Step(ShibuyaStrikeMotion.Uppercut, ShibuyaHitOutcome.Launch, 2.5f, .80f);
            data.downFinisher = Step(ShibuyaStrikeMotion.DownSlam, ShibuyaHitOutcome.Knockdown, 2f, .65f);
            return data;
        }
        private static ShibuyaMovesetStep Step(ShibuyaStrikeMotion motion, ShibuyaHitOutcome outcome, float speed, float pitch)
            => new ShibuyaMovesetStep {
                hits = new[] { new ShibuyaHitEvent {
                    strike = new ShibuyaStrikeDefinition { damage = 25, startup = .18f, activeTime = .10f,
                        recovery = .32f, knockbackSpeed = speed, hitStun = .45f, alignVictimOnHit = false,
                        motion = motion, presentationIntensity = 1.5f, powerWindupTime = .10f,
                        hitStopDuration = .065f, soundPitch = pitch },
                    outcome = outcome, heavyReaction = true
                } },
                movements = new[] { new ShibuyaMovementDefinition { kind = ShibuyaMovementKind.Forward,
                    startTime = .10f, duration = .16f, maxDistance = .55f, speed = 5 }
                }
            };
    }
}
