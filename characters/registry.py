from __future__ import annotations

from collections.abc import Callable

from characters.base import CharacterProfile, CharacterRuntime
from characters.gojo import PROFILE as GOJO, build_runtime as build_gojo_runtime
from characters.megumi import PROFILE as MEGUMI, build_runtime as build_megumi_runtime
from characters.sukuna_itadori import (
    PROFILE as SUKUNA_ITADORI,
    build_runtime as build_sukuna_runtime,
)


RuntimeBuilder = Callable[[], CharacterRuntime]

CHARACTER_SLOTS: tuple[CharacterProfile, ...] = (
    GOJO,
    SUKUNA_ITADORI,
    MEGUMI,
)

_RUNTIME_BUILDERS: dict[str, RuntimeBuilder] = {
    GOJO.character_id: build_gojo_runtime,
    SUKUNA_ITADORI.character_id: build_sukuna_runtime,
    MEGUMI.character_id: build_megumi_runtime,
}


def create_character_runtime(character: CharacterProfile) -> CharacterRuntime:
    """등록된 캐릭터의 독립 런타임을 생성한다."""

    builder = _RUNTIME_BUILDERS.get(character.character_id)
    if builder is None or not character.available:
        raise ValueError(f"지원하지 않거나 잠긴 캐릭터입니다: {character.character_id}")
    return builder()
