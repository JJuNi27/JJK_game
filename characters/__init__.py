"""캐릭터별 프로필과 런타임 구성을 모아 두는 패키지."""

from characters.base import CharacterProfile, CharacterRuntime
from characters.registry import CHARACTER_SLOTS, create_character_runtime

__all__ = [
    "CharacterProfile",
    "CharacterRuntime",
    "CHARACTER_SLOTS",
    "create_character_runtime",
]
