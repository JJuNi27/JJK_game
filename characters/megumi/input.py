from __future__ import annotations

import pygame

from characters.megumi.controller import MegumiDomainController


class MegumiShadowGestureInput:
    """Q 키와 좌클릭 드래그를 감합암예정 그림자 궤적으로 변환한다."""

    def __init__(self) -> None:
        self._q_held = False
        self._left_held = False

    def reset(self) -> None:
        self._q_held = False
        self._left_held = False

    def handle_event(
        self,
        event: pygame.event.Event,
        controller: MegumiDomainController,
    ) -> None:
        if event.type == pygame.KEYDOWN and event.key == pygame.K_q:
            if not self._q_held:
                self._q_held = True
                controller.begin_left_hand()
            return

        if event.type == pygame.KEYUP and event.key == pygame.K_q:
            self._q_held = False
            controller.cancel_left_hand()
            return

        if event.type == pygame.MOUSEBUTTONDOWN and event.button == 1:
            self._left_held = True
            controller.begin_drag(event.pos, left_hand_is_held=self._q_held)
            return

        if event.type == pygame.MOUSEMOTION and self._left_held:
            controller.update_drag(event.pos)
            return

        if event.type == pygame.MOUSEBUTTONUP and event.button == 1:
            self._left_held = False
            controller.finish_drag(left_hand_is_held=self._q_held)
