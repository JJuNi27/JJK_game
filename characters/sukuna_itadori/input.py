from __future__ import annotations

import time

import pygame

from characters.sukuna_itadori.controller import SukunaDomainController
from game.state import GameState


class SukunaTwoHandSealInput:
    """E 키와 마우스 양 버튼을 복마어주자 장인 입력으로 변환한다."""

    def __init__(self) -> None:
        self._e_held = False
        self._mouse_held = {1: False, 3: False}
        self._mouse_down_at: dict[int, float] = {}
        self._first_release_at: float | None = None

    def reset(self) -> None:
        self._e_held = False
        self._mouse_held = {1: False, 3: False}
        self._mouse_down_at.clear()
        self._first_release_at = None

    def handle_event(
        self,
        event: pygame.event.Event,
        controller: SukunaDomainController,
    ) -> None:
        if event.type == pygame.KEYDOWN and event.key == pygame.K_e:
            if not self._e_held:
                self._e_held = True
                controller.begin_left_hand()
            return

        if event.type == pygame.KEYUP and event.key == pygame.K_e:
            self._e_held = False
            controller.cancel_left_hand()
            return

        if event.type == pygame.MOUSEBUTTONDOWN and event.button in (1, 3):
            self._handle_mouse_down(event.button, controller)
            return

        if event.type == pygame.MOUSEBUTTONUP and event.button in (1, 3):
            self._handle_mouse_up(event.button, controller)

    def _handle_mouse_down(
        self,
        button: int,
        controller: SukunaDomainController,
    ) -> None:
        if self._mouse_held[button]:
            return

        self._mouse_held[button] = True
        self._mouse_down_at[button] = time.perf_counter()
        self._first_release_at = None

        if all(self._mouse_held.values()):
            press_gap = abs(self._mouse_down_at[1] - self._mouse_down_at[3])
            controller.complete_mouse_seal(
                left_hand_is_held=self._e_held,
                press_gap=press_gap,
            )

    def _handle_mouse_up(
        self,
        button: int,
        controller: SukunaDomainController,
    ) -> None:
        if not self._mouse_held[button]:
            return

        now = time.perf_counter()
        other_button = 3 if button == 1 else 1
        self._mouse_held[button] = False

        if controller.state == GameState.WAIT_LEFT_CLICK:
            controller.incomplete_mouse_release()
            self._first_release_at = None
            return

        if self._mouse_held[other_button]:
            self._first_release_at = now
            return

        release_gap = 0.0 if self._first_release_at is None else now - self._first_release_at
        controller.complete_mouse_release(
            left_hand_is_held=self._e_held,
            release_gap=release_gap,
        )
        self._first_release_at = None
        self._mouse_down_at.clear()
