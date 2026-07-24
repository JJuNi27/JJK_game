from __future__ import annotations

from collections.abc import Callable

from characters.base import CharacterProfile, CharacterRuntime
from characters.gojo import PROFILE as GOJO, build_runtime as build_gojo_runtime
from characters.megumi import PROFILE as MEGUMI_DRAFT
from characters.sukuna_itadori import (
    PROFILE as SUKUNA_ITADORI,
    build_runtime as build_sukuna_runtime,
)


RuntimeBuilder = Callable[[], CharacterRuntime]

CHARACTER_SLOTS: tuple[CharacterProfile, ...] = (
    GOJO,
    SUKUNA_ITADORI,
    MEGUMI_DRAFT,
)

_RUNTIME_BUILDERS: dict[str, RuntimeBuilder] = {
    GOJO.character_id: build_gojo_runtime,
    SUKUNA_ITADORI.character_id: build_sukuna_runtime,
}


def create_character_runtime(character: CharacterProfile) -> CharacterRuntime:
    """등록된 캐릭터의 런타임을 생성한다.

    새 캐릭터를 추가할 때 기존 캐릭터 조건문을 수정하지 않고,
    해당 캐릭터 패키지의 builder를 이 등록소에 한 줄 추가한다.
    """

    builder = _RUNTIME_BUILDERS.get(character.character_id)
    if builder is None:
        raise ValueError(f"지원하지 않거나 잠긴 캐릭터입니다: {character.character_id}")
    return builder()
