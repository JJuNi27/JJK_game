using UnityEngine;

namespace JJKGame.Player
{
    public enum CharacterPresentationSkillSlot
    {
        Skill1,
        Skill2,
        Ultimate,
        Domain,
    }

    public readonly struct CharacterSkillPresentation
    {
        public CharacterSkillPresentation(string label, Color accent)
        {
            Label = label;
            Accent = accent;
        }

        public string Label { get; }
        public Color Accent { get; }
    }

    /// <summary>
    /// Gate 4A production-candidate identity/presentation data.
    /// This deliberately contains display metadata only: no HP, CE, damage,
    /// cooldown, domain rules, or character-specific gameplay logic.
    /// </summary>
    public sealed class CharacterPresentationProfile
    {
        public CharacterPresentationProfile(
            PrototypeCharacterId characterId,
            string displayName,
            string hudName,
            string shortName,
            string variantLabel,
            string compactVariantLabel,
            Color hudAccent,
            Color energyAccent,
            CharacterSkillPresentation skill1,
            CharacterSkillPresentation skill2,
            CharacterSkillPresentation ultimate,
            CharacterSkillPresentation domain
        )
        {
            CharacterId = characterId;
            DisplayName = displayName;
            HudName = hudName;
            ShortName = shortName;
            VariantLabel = variantLabel;
            CompactVariantLabel = compactVariantLabel;
            HudAccent = hudAccent;
            EnergyAccent = energyAccent;
            Skill1 = skill1;
            Skill2 = skill2;
            Ultimate = ultimate;
            Domain = domain;
        }

        public PrototypeCharacterId CharacterId { get; }
        public string DisplayName { get; }
        public string HudName { get; }
        public string ShortName { get; }
        public string VariantLabel { get; }
        public string CompactVariantLabel { get; }
        public Color HudAccent { get; }
        public Color EnergyAccent { get; }
        public CharacterSkillPresentation Skill1 { get; }
        public CharacterSkillPresentation Skill2 { get; }
        public CharacterSkillPresentation Ultimate { get; }
        public CharacterSkillPresentation Domain { get; }

        public CharacterSkillPresentation GetSkill(CharacterPresentationSkillSlot slot)
        {
            return slot switch
            {
                CharacterPresentationSkillSlot.Skill2 => Skill2,
                CharacterPresentationSkillSlot.Ultimate => Ultimate,
                CharacterPresentationSkillSlot.Domain => Domain,
                _ => Skill1,
            };
        }
    }

    /// <summary>
    /// Central registry for presentation metadata proven to repeat across fighters.
    /// Asset-backed portrait/model/Animator hooks remain deferred until real assets exist.
    /// </summary>
    public static class CharacterPresentationProfiles
    {
        private static readonly CharacterPresentationProfile GojoModern =
            new CharacterPresentationProfile(
                PrototypeCharacterId.GojoModern,
                "GOJO SATORU · 현대 · 교사",
                "GOJO",
                "고죠",
                "현대 · 교사",
                "현대",
                new Color(0.18f, 0.66f, 1f),
                new Color(0.20f, 0.24f, 0.90f),
                new CharacterSkillPresentation("창", new Color(0.12f, 0.55f, 1f)),
                new CharacterSkillPresentation("혁", new Color(1f, 0.12f, 0.10f)),
                new CharacterSkillPresentation("허식 자", new Color(0.68f, 0.18f, 1f)),
                new CharacterSkillPresentation("무량공처", new Color(0.36f, 0.72f, 1f))
            );

        private static readonly CharacterPresentationProfile SukunaShibuya =
            new CharacterPresentationProfile(
                PrototypeCharacterId.SukunaShibuyaYujiBody,
                "RYOMEN SUKUNA · 시부야 사변",
                "SUKUNA",
                "스쿠나",
                "시부야 사변",
                "시부야",
                new Color(0.96f, 0.20f, 0.12f),
                new Color(0.66f, 0.08f, 0.12f),
                new CharacterSkillPresentation("해", new Color(0.90f, 0.18f, 0.12f)),
                new CharacterSkillPresentation("팔", new Color(0.92f, 0.26f, 0.12f)),
                new CharacterSkillPresentation("푸가", new Color(1f, 0.46f, 0.08f)),
                new CharacterSkillPresentation("복마어주자", new Color(0.78f, 0.035f, 0.025f))
            );

        private static readonly CharacterPresentationProfile MegumiStudent =
            new CharacterPresentationProfile(
                PrototypeCharacterId.MegumiStudent,
                "FUSHIGURO MEGUMI · 도쿄고 학생",
                "MEGUMI",
                "메구미",
                "도쿄고 학생",
                "학생",
                new Color(0.12f, 0.42f, 0.52f),
                new Color(0.10f, 0.30f, 0.38f),
                new CharacterSkillPresentation("옥견", new Color(0.22f, 0.62f, 0.68f)),
                new CharacterSkillPresentation("누에", new Color(0.46f, 0.66f, 0.84f)),
                new CharacterSkillPresentation("만상", new Color(0.28f, 0.46f, 0.58f)),
                new CharacterSkillPresentation("감합암예정", new Color(0.06f, 0.16f, 0.22f))
            );

        public static CharacterPresentationProfile Get(PrototypeCharacterId characterId)
        {
            return characterId switch
            {
                PrototypeCharacterId.SukunaShibuyaYujiBody => SukunaShibuya,
                PrototypeCharacterId.MegumiStudent => MegumiStudent,
                _ => GojoModern,
            };
        }
    }
}
