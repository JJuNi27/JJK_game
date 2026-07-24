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
        self.result_message = "V 키를 눌러 영역전개를 준비하세요"

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
            "영역 준비: 마우스 오른쪽 버튼을 누르고 유지하세요",
        )

    def _fail(self, message: str) -> None:
        self._change_state(GameState.FAILED, message)

    def _activate_domain(self) -> None:
        self._change_state(
            GameState.DOMAIN_ACTIVE,
            "영역전개 · 무량공처",
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
                    "오른쪽 버튼을 유지한 채 왼쪽 버튼을 클릭하세요",
                )

            # 좌클릭: 장인 결합 입력
            elif event.button == 1 and self.state == GameState.WAIT_LEFT_CLICK:
                if not pygame.mouse.get_pressed(num_buttons=3)[2]:
                    self._fail("실패: 오른쪽 버튼을 유지하지 않았습니다")
                    return

                if self.right_pressed_at is None:
                    self._fail("실패: 오른쪽 버튼 입력 시간이 없습니다")
                    return

                elapsed = now - self.right_pressed_at
                if elapsed > self.config.right_to_left_timeout:
                    self._fail("실패: 왼쪽 클릭이 너무 늦었습니다")
                    return

                self.left_clicked_at = now
                self._change_state(
                    GameState.RELEASE_TIMING,
                    "초록색 타이밍 구간에서 오른쪽 버튼을 놓으세요",
                )

        if event.type == pygame.MOUSEBUTTONUP and event.button == 3:
            if self.state == GameState.WAIT_LEFT_CLICK:
                self._fail("실패: 왼쪽 클릭 전에 오른쪽 버튼을 놓았습니다")
                return

            if self.state == GameState.RELEASE_TIMING:
                if self.left_clicked_at is None:
                    self._fail("실패: 왼쪽 클릭 시간이 없습니다")
                    return

                release_time = now - self.left_clicked_at
                error = abs(release_time - self.config.target_release_time)
                self.last_release_error = error

                if error <= self.config.release_tolerance:
                    self._activate_domain()
                else:
                    self._fail(
                        f"타이밍 실패: {release_time:.2f}초에 버튼을 놓았습니다"
                    )

    def update(self) -> None:
        now = time.perf_counter()
        elapsed = now - self.state_started_at

        if (
            self.state == GameState.DOMAIN_READY
            and elapsed > self.config.domain_ready_timeout
        ):
            self._fail("실패: 장인 입력 제한시간을 초과했습니다")

        elif self.state == GameState.WAIT_LEFT_CLICK:
            if self.right_pressed_at is not None:
                if now - self.right_pressed_at > self.config.right_to_left_timeout:
                    self._fail("실패: 왼쪽 클릭이 너무 늦었습니다")

        elif self.state == GameState.RELEASE_TIMING:
            if self.left_clicked_at is not None:
                max_wait = (
                    self.config.target_release_time
                    + self.config.release_tolerance
                    + 0.5
                )
                if now - self.left_clicked_at > max_wait:
                    self._fail("실패: 오른쪽 버튼을 너무 늦게 놓았습니다")

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
