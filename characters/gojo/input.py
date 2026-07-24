from __future__ import annotations

import pygame

from characters.gojo.controller import GojoDomainController


class GojoSealInput:
    """마우스 입력을 고죠 무량공처 장인 명령으로 변환한다."""

    def __init__(self) -> None:
        self._right_held = False

    def reset(self) -> None:
        self._right_held = False

    def handle_event(
        self,
        event: pygame.event.Event,
        controller: GojoDomainController,
    ) -> None:
        if event.type == pygame.MOUSEBUTTONDOWN:
            if event.button == 3:
                self._right_held = True
                controller.begin_seal()
            elif event.button == 1:
                controller.combine_seal(right_is_held=self._right_held)

        elif event.type == pygame.MOUSEBUTTONUP and event.button == 3:
            self._right_held = False
            controller.release_seal()
