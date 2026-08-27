namespace JJKGame.Player
{
    /// <summary>
    /// Character-select-specific copy that is safe for UI to consume.
    /// This is presentation copy only; it owns no combat rules or balance values.
    /// </summary>
    public readonly struct CharacterSelectPresentationData
    {
        public CharacterSelectPresentationData(
            string roleLabel,
            string styleLabel,
            string description
        )
        {
            RoleLabel = roleLabel;
            StyleLabel = styleLabel;
            Description = description;
        }

        public string RoleLabel { get; }
        public string StyleLabel { get; }
        public string Description { get; }
    }

    public static class CharacterSelectPresentationProfiles
    {
        private static readonly CharacterSelectPresentationData Gojo =
            new CharacterSelectPresentationData(
                "ABSOLUTE CONTROL",
                "공간 제압 · 폭발적 술식 연계",
                "창과 혁으로 거리를 지배하고 허식 자와 무량공처로 전장을 끝낸다."
            );

        private static readonly CharacterSelectPresentationData Sukuna =
            new CharacterSelectPresentationData(
                "RELENTLESS OFFENSE",
                "고화력 압박 · 개방형 영역",
                "해와 팔로 빈틈을 깎아내고 푸가와 복마어주자로 전장을 압도한다."
            );

        private static readonly CharacterSelectPresentationData Megumi =
            new CharacterSelectPresentationData(
                "SHIKIGAMI TACTICS",
                "식신 운용 · 공간 통제",
                "옥견을 시작으로 식신과 그림자 기반 전개를 활용하는 전술형 파이터다."
            );

        public static CharacterSelectPresentationData Get(PrototypeCharacterId characterId)
        {
            return characterId switch
            {
                PrototypeCharacterId.SukunaShibuyaYujiBody => Sukuna,
                PrototypeCharacterId.MegumiStudent => Megumi,
                _ => Gojo,
            };
        }
    }
}
