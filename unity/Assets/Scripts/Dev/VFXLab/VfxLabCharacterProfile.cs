using JJKGame.Core;
using JJKGame.Player;
using UnityEngine;

namespace JJKGame.Dev.VFXLab
{
    /// <summary>
    /// Optional character binding for the shared lab. A null profile preserves the
    /// current Gojo setup; future fighters can provide their own animator/audio data
    /// without changing the scene composition root or reusing Gojo technique effects.
    /// </summary>
    [CreateAssetMenu(fileName = "VfxLabCharacterProfile", menuName = "JJK/VFXLab/캐릭터 프리뷰 프로필")]
    public sealed class VfxLabCharacterProfile : ScriptableObject
    {
        [Header("캐릭터")]
        [SerializeField, InspectorName("캐릭터 식별자")]
        [Tooltip("캐릭터 선택 화면과 동일한 캐릭터 식별자입니다.")]
        private PrototypeCharacterId characterId = PrototypeCharacterId.GojoModern;

        [SerializeField, InspectorName("표시 이름 재정의")]
        [Tooltip("기본 캐릭터 표시명을 덮어쓸 때만 입력합니다.")]
        private string displayNameOverride;

        [Header("프리뷰 연결")]
        [SerializeField, InspectorName("애니메이터 컨트롤러")]
        [Tooltip("선택 캐릭터의 선택적 애니메이터 컨트롤러입니다. 비워 두면 씬의 기존 연결을 유지합니다.")]
        private RuntimeAnimatorController animatorController;

        [SerializeField]
        [Tooltip("선택 캐릭터의 걷기, 달리기, 회피 설정입니다. 비워 두면 씬의 기존 연결을 유지합니다.")]
        private CharacterMovementProfile movement = new CharacterMovementProfile();

        [SerializeField, InspectorName("전투 오디오 프로필")]
        [Tooltip("선택 캐릭터의 기술 슬롯/공통 전투 오디오 프로필입니다.")]
        private CombatAudioProfile combatAudio;

        public PrototypeCharacterId CharacterId => characterId;
        public CharacterPresentationProfile Presentation => CharacterPresentationProfiles.Get(characterId);
        public string DisplayName => string.IsNullOrWhiteSpace(displayNameOverride)
            ? Presentation.DisplayName
            : displayNameOverride;
        public RuntimeAnimatorController AnimatorController => animatorController;
        public CharacterMovementProfile Movement => movement;
        public CombatAudioProfile CombatAudio => combatAudio;
    }
}
