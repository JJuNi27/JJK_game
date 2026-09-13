using System;
using JJKGame.Player;
using UnityEngine;

namespace JJKGame.Core
{
    [Serializable]
    public sealed class TechniqueAudioProfile
    {
        [InspectorName("음성")]
        [Tooltip("기술 시전과 함께 재생할 연속 음성 클립입니다.")]
        public AudioClip voice;

        [InspectorName("시전 효과음")]
        [Tooltip("기술의 시전 시작 박자에 재생할 효과음입니다.")]
        public AudioClip castSfx;

        [InspectorName("방출 효과음")]
        [Tooltip("기술이 실제로 방출되는 박자에 재생할 효과음입니다.")]
        public AudioClip releaseSfx;

        [InspectorName("이동 효과음")]
        [Tooltip("투사체나 기술 본체가 이동하는 동안 사용할 효과음입니다.")]
        public AudioClip travelSfx;

        [InspectorName("충돌 효과음")]
        [Tooltip("기술이 대상이나 월드에 적중하는 박자에 재생할 효과음입니다.")]
        public AudioClip impactSfx;

        [InspectorName("비트 훅 식별자")]
        [Tooltip("연출 비트 시계와 연결할 선택적 의미 식별자입니다. 비워 두면 기존 즉시 재생을 유지합니다.")]
        public string beatHookId;
    }

    /// <summary>
    /// Character-neutral audio seam. Existing prototype fields remain the fallback,
    /// so adopting this asset can be incremental and does not require moving clips.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatAudioProfile", menuName = "JJK/프레젠테이션/전투 오디오 프로필")]
    public sealed class CombatAudioProfile : ScriptableObject
    {
        [Header("기술 슬롯")]
        [InspectorName("기술 1"), Tooltip("캐릭터의 기술 1 슬롯에 사용할 음성과 효과음입니다.")]
        public TechniqueAudioProfile skill1 = new TechniqueAudioProfile();

        [InspectorName("기술 2"), Tooltip("캐릭터의 기술 2 슬롯에 사용할 음성과 효과음입니다.")]
        public TechniqueAudioProfile skill2 = new TechniqueAudioProfile();

        [InspectorName("필살기"), Tooltip("캐릭터의 필살기 슬롯에 사용할 음성과 효과음입니다.")]
        public TechniqueAudioProfile ultimate = new TechniqueAudioProfile();

        [InspectorName("영역"), Tooltip("캐릭터의 영역 슬롯에 사용할 음성과 효과음입니다.")]
        public TechniqueAudioProfile domain = new TechniqueAudioProfile();

        [Header("공통 전투 효과음")]
        [InspectorName("평타 휘두르기 효과음"), Tooltip("평타를 휘두를 때 재생할 공통 효과음입니다.")]
        public AudioClip basicSwing;

        [InspectorName("평타 적중 효과음"), Tooltip("평타가 적중할 때 재생할 공통 효과음입니다.")]
        public AudioClip basicHit;

        [InspectorName("평타 마무리 효과음"), Tooltip("평타 마무리 타격이 적중할 때 재생할 효과음입니다.")]
        public AudioClip basicFinisher;

        [InspectorName("회피 효과음"), Tooltip("회피할 때 재생할 공통 효과음입니다.")]
        public AudioClip dodge;

        public TechniqueAudioProfile GetTechnique(CharacterPresentationSkillSlot slot)
        {
            return slot switch
            {
                CharacterPresentationSkillSlot.Skill2 => skill2,
                CharacterPresentationSkillSlot.Ultimate => ultimate,
                CharacterPresentationSkillSlot.Domain => domain,
                _ => skill1,
            };
        }
    }
}
