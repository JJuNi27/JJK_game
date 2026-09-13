using System;
using JJKGame.Enemy;
using JJKGame.Player;
using UnityEngine;

namespace JJKGame.Core
{
    public enum CharacterTraitId { SixEyes, Infinity, ZeroCursedEnergy, HeavenlyRestriction, CanUseRct, OpenBarrierDomain, CursedSpirit, AdaptivePhenomenon, SpecialPhysiology }
    public enum CharacterPassiveId { InfinityDefense, TechniqueBurnoutRecovery, ReverseCursedTechnique, AdaptationProgress }

    [CreateAssetMenu(menuName = "JJK Game/Combat/Character Combat Definition", fileName = "Character Combat Definition")]
    public sealed class CharacterCombatDefinition : ScriptableObject
    {
        [Header("캐릭터 식별")]
        [SerializeField, InspectorName("캐릭터 식별자")] private PrototypeCharacterId characterId;
        [SerializeField, InspectorName("표시 이름")] private string displayName;
        [Header("전투 데이터")]
        [SerializeField, InspectorName("캐릭터 능력치")] private CharacterStatsProfile stats;
        [SerializeField, InspectorName("주력 프로필")] private CursedEnergyProfile cursedEnergy;
        [SerializeField, InspectorName("평타 프로필")] private BasicAttackProfile basicAttack;
        [SerializeField, InspectorName("Gojo 술식 프로필")] private GojoTechniqueGameplayProfile gojoTechniques;
        [SerializeField, InspectorName("영역 프로필")] private DomainGameplayProfile domain;
        [SerializeField, InspectorName("번아웃 정책")] private BurnoutPolicyProfile burnout;
        [SerializeField, InspectorName("타겟팅 프로필")] private TargetingProfile targeting;
        [SerializeField, InspectorName("이동 프로필")] private CharacterMovementProfile movement = new CharacterMovementProfile();
        [SerializeField, InspectorName("훈련 봇 프로필")] private TrainingBotProfile trainingBot;
        [Header("미래 Trait / Passive 연결")]
        [SerializeField, InspectorName("Trait 목록")] private CharacterTraitId[] traits = Array.Empty<CharacterTraitId>();
        [SerializeField, InspectorName("Passive 목록")] private CharacterPassiveId[] passives = Array.Empty<CharacterPassiveId>();
        public PrototypeCharacterId CharacterId => characterId; public string DisplayName => displayName;
        public CharacterStatsProfile Stats => stats; public CursedEnergyProfile CursedEnergy => cursedEnergy;
        public BasicAttackProfile BasicAttack => basicAttack; public GojoTechniqueGameplayProfile GojoTechniques => gojoTechniques;
        public DomainGameplayProfile Domain => domain; public BurnoutPolicyProfile Burnout => burnout;
        public TargetingProfile Targeting => targeting; public CharacterMovementProfile Movement => movement;
        public TrainingBotProfile TrainingBot => trainingBot; public CharacterTraitId[] Traits => traits; public CharacterPassiveId[] Passives => passives;
    }
}
