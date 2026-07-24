from __future__ import annotations

import time
from dataclasses import dataclass

import pygame

from game.state import GameState


@dataclass(frozen=True)
class DomainConfig:
    """프로토타입 판정 수치.

    실제 플레이 후 어렵거나 쉽다면 이 값들만 조절하면 된다.
    """

    domain_ready_timeout: float = 3.0
    right_to_left_timeout: float = 0.65
    target_release_time: float = 0.90
    release_tolerance: float = 0.22
    domain_active_duration: float = 2.5
    failed_duration: float = 1.2


class DomainController:
    """무량공처 장인 커맨드의 상태와 성공/실패를 관리한다."""

    def __init__(self, config: DomainConfig | None = None) -> None:
        self.config = config or DomainConfig()
        self.reset()

    def reset(self) -> None:
        self.state = GameState.NORMAL
        self.state_started_at = time.perf_counter()
        self.right_pressed_at: float | None = None
        self.left_clicked_at: float | None = None
        self.last_release_error: float | None = None
        self.result_message = "Press V to prepare Domain Expansion"

    def _change_state(self, state: GameState, message: str) -> None:
        self.state = state
        self.state_started_at = time.perf_counter()
        self.result_message = message

    def start_domain_ready(self) -> None:
        self.right_pressed_at = None
        self.left_clicked_at = None
        self.last_release_error = None
        self._change_state(
            GameState.DOMAIN_READY,
            "Domain ready: hold RIGHT mouse button",
        )

    def _fail(self, message: str) -> None:
        self._change_state(GameState.FAILED, message)

    def _activate_domain(self) -> None:
        self._change_state(
            GameState.DOMAIN_ACTIVE,
            "UNLIMITED VOID ACTIVATED",
        )

    def handle_event(self, event: pygame.event.Event) -> None:
        now = time.perf_counter()

        if event.type == pygame.KEYDOWN:
            if event.key == pygame.K_v and self.state == GameState.NORMAL:
                self.start_domain_ready()
            elif event.key == pygame.K_r:
                self.reset()

        if event.type == pygame.MOUSEBUTTONDOWN:
            # 우클릭: 주력 집중 시작
            if event.button == 3 and self.state == GameState.DOMAIN_READY:
                self.right_pressed_at = now
                self._change_state(
                    GameState.WAIT_LEFT_CLICK,
                    "Keep holding RIGHT and click LEFT",
                )

            # 좌클릭: 장인 결합 입력
            elif event.button == 1 and self.state == GameState.WAIT_LEFT_CLICK:
                if not pygame.mouse.get_pressed(num_buttons=3)[2]:
                    self._fail("Failed: RIGHT mouse button was not held")
                    return

                if self.right_pressed_at is None:
                    self._fail("Failed: missing RIGHT press time")
                    return

                elapsed = now - self.right_pressed_at
                if elapsed > self.config.right_to_left_timeout:
                    self._fail("Failed: LEFT click was too late")
                    return

                self.left_clicked_at = now
                self._change_state(
                    GameState.RELEASE_TIMING,
                    "Release RIGHT inside the target timing zone",
                )

        if event.type == pygame.MOUSEBUTTONUP and event.button == 3:
            if self.state == GameState.WAIT_LEFT_CLICK:
                self._fail("Failed: RIGHT was released before LEFT click")
                return

            if self.state == GameState.RELEASE_TIMING:
                if self.left_clicked_at is None:
                    self._fail("Failed: missing LEFT click time")
                    return

                release_time = now - self.left_clicked_at
                error = abs(release_time - self.config.target_release_time)
                self.last_release_error = error

                if error <= self.config.release_tolerance:
                    self._activate_domain()
                else:
                    self._fail(
                        f"Failed timing: released at {release_time:.2f}s"
                    )

    def update(self) -> None:
        now = time.perf_counter()
        elapsed = now - self.state_started_at

        if (
            self.state == GameState.DOMAIN_READY
            and elapsed > self.config.domain_ready_timeout
        ):
            self._fail("Failed: command input timed out")

        elif self.state == GameState.WAIT_LEFT_CLICK:
            if self.right_pressed_at is not None:
                if now - self.right_pressed_at > self.config.right_to_left_timeout:
                    self._fail("Failed: LEFT click was too late")

        elif self.state == GameState.RELEASE_TIMING:
            if self.left_clicked_at is not None:
                max_wait = (
                    self.config.target_release_time
                    + self.config.release_tolerance
                    + 0.5
                )
                if now - self.left_clicked_at > max_wait:
                    self._fail("Failed: RIGHT release was too late")

        elif (
            self.state == GameState.DOMAIN_ACTIVE
            and elapsed > self.config.domain_active_duration
        ):
            self.reset()

        elif (
            self.state == GameState.FAILED
            and elapsed > self.config.failed_duration
        ):
            self.reset()

    def release_elapsed(self) -> float:
        """좌클릭 이후 현재까지 흐른 시간을 반환한다."""
        if self.left_clicked_at is None:
            return 0.0
        return max(0.0, time.perf_counter() - self.left_clicked_at)

    def release_progress(self) -> float:
        """타이밍 바 그리기에 사용할 0~1 진행률."""
        total = self.config.target_release_time + self.config.release_tolerance + 0.5
        return min(1.0, self.release_elapsed() / total)
