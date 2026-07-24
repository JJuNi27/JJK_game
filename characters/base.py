from __future__ import annotations

from dataclasses import dataclass
from typing import Protocol

import pygame

from game.domain_protocol import DomainControllerProtocol


@dataclass(frozen=True)
class CharacterProfile:
    """선택 화면과 연습 UI가 공유하는 캐릭터 메타데이터."""

    character_id: str
    name: str
    technique: str
    domain: str
    voice_command: str
    seal_steps: tuple[str, ...] = ()
    available: bool = True
    status: str = "선택 가능"


class SealInputProtocol(Protocol):
    def reset(self) -> None: ...

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
    """선택한 캐릭터의 판정·입력·이펙트 묶음."""

    controller: DomainControllerProtocol
    seal_input: SealInputProtocol
    effect: EffectProtocol
