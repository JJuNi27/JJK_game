"""하위 호환용 런타임 진입점.

실제 캐릭터별 런타임 구성은 `characters/<character>/definition.py`와
`characters/registry.py`에서 관리한다.
"""

from characters.base import CharacterRuntime, EffectProtocol, SealInputProtocol
from characters.registry import create_character_runtime

__all__ = [
    "CharacterRuntime",
    "EffectProtocol",
    "SealInputProtocol",
    "create_character_runtime",
]
