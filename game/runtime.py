from __future__ import annotations

from dataclasses import dataclass
from typing import Protocol

import pygame

from effects.fukuma_effect import FukumaEffect
from effects.muryang_effect import MuryangEffect
from game.character import CharacterProfile
from game.domain_controller import DomainController
from game.domain_protocol import DomainControllerProtocol
from game.sukuna_domain_controller import SukunaDomainController
from game_input.mouse_seal_input import MouseSealInput
from game_input.sukuna_two_hand_input import SukunaTwoHandSealInput


class SealInputProtocol(Protocol):
    def handle_event(
        self,
        event: pygame.event.Event,
        controller: DomainControllerProtocol,
    ) -> None: ...


class EffectProtocol(Protocol):
    def restart(self) -> None: ...

    def draw(
        self,
        surface: pygame.Surface,
        title_font: pygame.font.Font,
    ) -> None: ...


@dataclass
class CharacterRuntime:
    controller: DomainControllerProtocol
    seal_input: SealInputProtocol
    effect: EffectProtocol


def create_character_runtime(character: CharacterProfile) -> CharacterRuntime:
    """선택한 캐릭터에 맞는 판정·입력·임시 이펙트를 묶어 반환한다."""

    if character.character_id == "gojo":
        return CharacterRuntime(
            controller=DomainController(),
            seal_input=MouseSealInput(),
            effect=MuryangEffect(),
        )

    if character.character_id == "sukuna_itadori":
        return CharacterRuntime(
            controller=SukunaDomainController(),
            seal_input=SukunaTwoHandSealInput(),
            effect=FukumaEffect(),
        )

    raise ValueError(f"지원하지 않는 캐릭터입니다: {character.character_id}")
