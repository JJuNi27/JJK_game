from __future__ import annotations

import pygame

from characters.yuta.controller import YutaDomainController


class YutaSwordDrawInput:
    """C 키 반지 연결과 좌클릭 위쪽 드래그를 진안상애 입력으로 변환한다."""

    def __init__(self) -> None:
        self._c_held = False
        self._left_held = False

    def reset(self) -> None:
        self._c_held = False
        self._left_held = False

    def handle_event(
        self,
        event: pygame.event.Event,
        controller: YutaDomainController,
    ) -> None:
        if event.type == pygame.KEYDOWN and event.key == pygame.K_c:
            if not self._c_held:
                self._c_held = True
                controller.begin_ring_connection()
            return

        if event.type == pygame.KEYUP and event.key == pygame.K_c:
            self._c_held = False
            controller.cancel_ring_connection()
            return

        if event.type == pygame.MOUSEBUTTONDOWN and event.button == 1:
            self._left_held = True
            controller.begin_draw(event.pos, ring_is_connected=self._c_held)
            return

        if event.type == pygame.MOUSEMOTION and self._left_held:
            controller.update_draw(event.pos)
            return

        if event.type == pygame.MOUSEBUTTONUP and event.button == 1:
            self._left_held = False
            controller.finish_draw(ring_is_connected=self._c_held)
