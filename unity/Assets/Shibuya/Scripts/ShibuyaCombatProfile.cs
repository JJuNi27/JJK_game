using UnityEngine;

namespace JJKGame.Shibuya
{
    [CreateAssetMenu(menuName = "JJK Game/시부야/기본 전투 설정", fileName = "Shibuya Combat Profile")]
    public sealed class ShibuyaCombatProfile : ScriptableObject
    {
        [Header("이동")]
        [Min(0.1f)] public float walkSpeed = 4.8f;
        [Min(0.1f)] public float runSpeed = 8.5f;
        [Min(0.1f)] public float turnSpeed = 720f;
        [Min(0.1f)] public float jumpHeight = 1.4f;
        [Min(0.1f)] public float gravity = 25f;
        [Range(0f, 1f)] public float attackMoveMultiplier = 0.25f;
        [Header("단일 체술")]
        [Min(0.1f)] public float damage = 20f;
        [Min(0.1f)] public float attackRadius = 0.85f;
        [Min(0.1f)] public float attackForwardOffset = 1.1f;
        [Min(0f)] public float attackHeight = 1f;
        [Min(0.01f)] public float startup = 0.10f;
        [Min(0.01f)] public float activeTime = 0.08f;
        [Min(0.01f)] public float recovery = 0.22f;
        [Min(0f)] public float knockbackSpeed = 6f;
        [Min(0f)] public float hitStun = 0.25f;
        [Min(0.1f)] public float knockbackDamping = 22f;
        [Header("카메라")]
        [Min(0.5f)] public float cameraDistance = 6.5f;
        [Min(0f)] public float cameraHeight = 1.25f;
        [Min(0.1f)] public float mouseSensitivity = 3f;
        [Range(-80f, 0f)] public float minimumPitch = -25f;
        [Range(1f, 85f)] public float maximumPitch = 70f;
        [Range(0.05f, 0.5f)] public float cameraCollisionRadius = 0.2f;
        [Min(0.1f)] public float cameraFollowSharpness = 16f;
        [Header("기본 피격 표현")]
        [Min(0.01f)] public float hitFlashDuration = 0.09f;
        [Range(0f, 1f)] public float hitSoundVolume = 0.35f;

        public float AttackDuration => Mathf.Max(0.01f, startup) + Mathf.Max(0.01f, activeTime)
            + Mathf.Max(0.01f, recovery);
    }
}
