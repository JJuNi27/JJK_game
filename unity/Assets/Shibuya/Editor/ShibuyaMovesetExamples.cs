using JJKGame.Core;
using UnityEditor;
using UnityEngine;

namespace JJKGame.Shibuya.EditorTools
{
    public static class ShibuyaMovesetExamples
    {
        public const string MultiPath = "Assets/Shibuya/Data/PASS3A Multi Hit.asset";
        public const string ApproachPath = "Assets/Shibuya/Data/PASS3A Knockback Approach.asset";
        public static ShibuyaMovesetDefinition Multi()
        {
            var data = ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();
            data.steps = new[] { new ShibuyaMovesetStep { hits = new[] {
                Hit(7, 0, .1f), Hit(11, .23f, .08f)
            } } };
            data.steps[0].hits[1].strike.motion = ShibuyaStrikeMotion.LeftJab;
            return data;
        }
        public static ShibuyaMovesetDefinition Approach()
        {
            var data = ScriptableObject.CreateInstance<ShibuyaMovesetDefinition>();
            var launch = Hit(9, 0, .1f);
            launch.strike.recovery = .18f; // Authored follow-up fits the finite confirmed-link budget.
            launch.knockbackKind = ShibuyaKnockbackKind.Distance;
            launch.knockbackDistance = 4; launch.knockbackDuration = .45f;
            var finish = Hit(23, .5f, .1f);
            finish.requiredMovement = 0; finish.heavyReaction = true;
            finish.strike.hitStopDuration = .065f;
            finish.strike.knockbackSpeed = 11;
            finish.strike.motion = ShibuyaStrikeMotion.HeavyCross;
            data.steps = new[] {
                new ShibuyaMovesetStep { hits = new[] { launch } },
                new ShibuyaMovesetStep { hits = new[] { finish }, movements = new[] {
                    new ShibuyaMovementDefinition { kind = ShibuyaMovementKind.ApproachLastHit,
                        duration = .65f, speed = 16, maxDistance = 7, targetRange = 9, arrivalDistance = 1.3f }
                } }
            };
            return data;
        }
        private static ShibuyaHitEvent Hit(float damage, float start, float startup) => new ShibuyaHitEvent {
            startTime = start, strike = new ShibuyaStrikeDefinition { damage = damage, startup = startup,
                activeTime = .06f, recovery = .25f, knockbackSpeed = 0, hitStun = .25f,
                alignVictimOnHit = false, advanceDistance = 0 }
        };
        // Explicit creation only; never regenerate or overwrite an existing asset.
        public static void CreateMissingExamples()
        {
            if (AssetDatabase.LoadAssetAtPath<ShibuyaMovesetDefinition>(MultiPath) == null)
                AssetDatabase.CreateAsset(Multi(), MultiPath);
            if (AssetDatabase.LoadAssetAtPath<ShibuyaMovesetDefinition>(ApproachPath) == null)
                AssetDatabase.CreateAsset(Approach(), ApproachPath);
        }
        [MenuItem("JJK Game/시부야 PASS3A/Play Mode 멀티 히트 예제")]
        public static void ApplyMulti() => Apply(MultiPath);
        [MenuItem("JJK Game/시부야 PASS3A/Play Mode 넉백 후 접근 예제")]
        public static void ApplyApproach() => Apply(ApproachPath);
        [MenuItem("JJK Game/시부야 PASS3A/Play Mode 기존 4타 복귀")]
        public static void ApplyDefault() => Apply(null);
        private static void Apply(string path)
        {
            if (!Application.isPlaying) { Debug.LogWarning("Play Mode에서만 적용됩니다. 씬은 저장하지 않습니다."); return; }
            var match = Object.FindFirstObjectByType<ShibuyaMatch>();
            if (match != null && !match.Finished)
                match.Player.SetMoveset(path == null ? null : AssetDatabase.LoadAssetAtPath<ShibuyaMovesetDefinition>(path));
        }
    }
}
