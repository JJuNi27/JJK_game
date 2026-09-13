using System;
using JJKGame.Player;
using UnityEngine;

namespace JJKGame.Core
{
    [CreateAssetMenu(menuName = "JJK Game/Combat/Character Combat Catalog", fileName = "Character Combat Catalog")]
    public sealed class CharacterCombatCatalog : ScriptableObject
    {
        private const string ResourcePath = "CombatData/Character Combat Catalog";
        [SerializeField, InspectorName("캐릭터 정의 목록")] private CharacterCombatDefinition[] characters = Array.Empty<CharacterCombatDefinition>();
        public static CharacterCombatCatalog Load() => Resources.Load<CharacterCombatCatalog>(ResourcePath);
        public CharacterCombatDefinition Find(PrototypeCharacterId id)
        {
            foreach (CharacterCombatDefinition definition in characters)
                if (definition != null && definition.CharacterId == id) return definition;
            return null;
        }
        public static CharacterCombatDefinition FindDefault(PrototypeCharacterId id) => Load()?.Find(id);
    }
}
