using System;
using UnityEngine;

namespace JJKGame.Player
{
    [Serializable]
    public sealed class BlueGameplayData
    {
        [SerializeField, InspectorName("시전시간"), Min(0.01f)] private float castTime = 0.24f;
        [SerializeField, InspectorName("시전 거리"), Min(0.1f)] private float castDistance = 10f;
        [SerializeField, InspectorName("적중 반경"), Min(0.1f)] private float radius = 4.5f;
        [SerializeField, InspectorName("필드 지속시간"), Min(0.1f)] private float fieldDuration = 0.95f;
        [SerializeField, InspectorName("다단 적중 간격"), Min(0.03f)] private float pulseInterval = 0.10f;
        [SerializeField, InspectorName("다단 적중 피해량"), Min(0f)] private float damage = 8f;
        [SerializeField, InspectorName("끌어당기는 힘"), Min(0f)] private float pullSpeed = 16f;
        [SerializeField, InspectorName("경직 시간"), Min(0f)] private float hitStun = 0.42f;
        [SerializeField, InspectorName("재사용 대기시간"), Min(0.1f)] private float cooldown = 3.2f;
        [SerializeField, InspectorName("주력 소모량"), Min(0f)] private float energyCost = 16f;
        [SerializeField, InspectorName("락온 대상 오프셋"), Min(0f)] private float lockedTargetOffset = 1.8f;
        public float CastTime => castTime; public float CastDistance => castDistance; public float Radius => radius;
        public float FieldDuration => fieldDuration; public float PulseInterval => pulseInterval; public float Damage => damage;
        public float PullSpeed => pullSpeed; public float HitStun => hitStun; public float Cooldown => cooldown;
        public float EnergyCost => energyCost; public float LockedTargetOffset => lockedTargetOffset;
    }

    [Serializable]
    public sealed class RedGameplayData
    {
        [SerializeField, InspectorName("시전시간"), Min(0.01f)] private float castTime = 0.30f;
        [SerializeField, InspectorName("사거리"), Min(0.1f)] private float range = 26f;
        [SerializeField, InspectorName("투사체 속도"), Min(0.1f)] private float projectileSpeed = 42f;
        [SerializeField, InspectorName("적중 반경"), Min(0.1f)] private float radius = 1.7f;
        [SerializeField, InspectorName("피해량"), Min(0f)] private float damage = 18f;
        [SerializeField, InspectorName("밀어내는 힘"), Min(0f)] private float pushSpeed = 23f;
        [SerializeField, InspectorName("경직 시간"), Min(0f)] private float hitStun = 0.52f;
        [SerializeField, InspectorName("재사용 대기시간"), Min(0.1f)] private float cooldown = 4.5f;
        [SerializeField, InspectorName("주력 소모량"), Min(0f)] private float energyCost = 24f;
        [SerializeField, InspectorName("생성 높이"), Min(0f)] private float spawnHeight = 1f;
        [SerializeField, InspectorName("전방 생성 오프셋"), Min(0f)] private float spawnForwardOffset = 0.9f;
        public float CastTime => castTime; public float Range => range; public float ProjectileSpeed => projectileSpeed;
        public float Radius => radius; public float Damage => damage; public float PushSpeed => pushSpeed; public float HitStun => hitStun;
        public float Cooldown => cooldown; public float EnergyCost => energyCost; public float SpawnHeight => spawnHeight;
        public float SpawnForwardOffset => spawnForwardOffset;
    }

    [Serializable]
    public sealed class PurpleGameplayData
    {
        [SerializeField, InspectorName("준비 유지시간"), Min(0.1f)] private float preparationDuration = 8f;
        [SerializeField, InspectorName("재사용 대기시간"), Min(0.1f)] private float cooldown = 10f;
        [SerializeField, InspectorName("사거리"), Min(0.1f)] private float range = 48f;
        [SerializeField, InspectorName("적중 반경"), Min(0.1f)] private float radius = 3.2f;
        [SerializeField, InspectorName("피해량"), Min(0f)] private float damage = 55f;
        [SerializeField, InspectorName("밀어내는 힘"), Min(0f)] private float pushSpeed = 34f;
        [SerializeField, InspectorName("경직 시간"), Min(0f)] private float hitStun = 1f;
        [SerializeField, InspectorName("주력 소모량"), Min(0f)] private float energyCost = 45f;
        public float PreparationDuration => preparationDuration; public float Cooldown => cooldown; public float Range => range;
        public float Radius => radius; public float Damage => damage; public float PushSpeed => pushSpeed; public float HitStun => hitStun;
        public float EnergyCost => energyCost;
    }

    [Serializable]
    public sealed class BlueRedSynergyData
    {
        [SerializeField, InspectorName("Blue 표식 지속시간"), Min(0.1f)] private float blueMarkDuration = 2.2f;
        [SerializeField, InspectorName("추가 피해량"), Min(0f)] private float bonusDamage = 12f;
        [SerializeField, InspectorName("강화 밀어내는 힘"), Min(0f)] private float empoweredPushSpeed = 28f;
        [SerializeField, InspectorName("강화 경직 시간"), Min(0f)] private float empoweredHitStun = 0.72f;
        [SerializeField, InspectorName("알림 표시시간"), Min(0.1f)] private float noticeDuration = 1.15f;
        public float BlueMarkDuration => blueMarkDuration; public float BonusDamage => bonusDamage;
        public float EmpoweredPushSpeed => empoweredPushSpeed; public float EmpoweredHitStun => empoweredHitStun;
        public float NoticeDuration => noticeDuration;
    }

    [CreateAssetMenu(menuName = "JJK Game/Combat/Gojo Technique Gameplay Profile", fileName = "Gojo Technique Gameplay Profile")]
    public sealed class GojoTechniqueGameplayProfile : ScriptableObject
    {
        [SerializeField, InspectorName("Blue 게임플레이")] private BlueGameplayData blue = new BlueGameplayData();
        [SerializeField, InspectorName("Red 게임플레이")] private RedGameplayData red = new RedGameplayData();
        [SerializeField, InspectorName("Purple 게임플레이")] private PurpleGameplayData purple = new PurpleGameplayData();
        [SerializeField, InspectorName("Blue→Red 연계")] private BlueRedSynergyData blueRedSynergy = new BlueRedSynergyData();
        public BlueGameplayData Blue => blue; public RedGameplayData Red => red;
        public PurpleGameplayData Purple => purple; public BlueRedSynergyData BlueRedSynergy => blueRedSynergy;
    }
}
