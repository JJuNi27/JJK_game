from __future__ import annotations

import time
from dataclasses import dataclass

from game.state import GameState


@dataclass(frozen=True)
class GojoDomainConfig:
    """고죠 무량공처 장인 프로토타입 판정 수치."""

    domain_ready_timeout: float = 3.0
    right_to_left_timeout: float = 0.65
    target_release_time: float = 0.90
    release_tolerance: float = 0.22
    domain_active_duration: float = 2.5
    failed_duration: float = 1.2


class GojoDomainController:
    """무량공처 한 손 장인의 상태 전이와 성공·실패 판정을 관리한다."""

    def __init__(self, config: GojoDomainConfig | None = None) -> None:
        self.config = config or GojoDomainConfig()
        self.reset()

    def reset(self) -> None:
        self.state = GameState.NORMAL
        self.state_started_at = time.perf_counter()
        self.right_pressed_at: float | None = None
        self.left_clicked_at: float | None = None
        self.last_release_error: float | None = None
        self.result_message = "V 키 또는 음성으로 영역전개를 준비하세요"

    def _change_state(self, state: GameState, message: str) -> None:
        self.state = state
        self.state_started_at = time.perf_counter()
        self.result_message = message

    def request_domain(self) -> None:
        if self.state != GameState.NORMAL:
            return

        self.right_pressed_at = None
        self.left_clicked_at = None
        self.last_release_error = None
        self._change_state(
            GameState.DOMAIN_READY,
            "영역 준비: 마우스 오른쪽 버튼을 누르고 유지하세요",
        )

    def begin_seal(self) -> None:
        if self.state != GameState.DOMAIN_READY:
            return

        self.right_pressed_at = time.perf_counter()
        self._change_state(
            GameState.WAIT_LEFT_CLICK,
            "오른쪽 버튼을 유지한 채 왼쪽 버튼을 클릭하세요",
        )

    def combine_seal(self, *, right_is_held: bool) -> None:
        if self.state != GameState.WAIT_LEFT_CLICK:
            return

        if not right_is_held:
            self._fail("실패: 오른쪽 버튼을 유지하지 않았습니다")
            return

        if self.right_pressed_at is None:
            self._fail("실패: 오른쪽 버튼 입력 시간이 없습니다")
            return

        now = time.perf_counter()
        if now - self.right_pressed_at > self.config.right_to_left_timeout:
            self._fail("실패: 왼쪽 클릭이 너무 늦었습니다")
            return

        self.left_clicked_at = now
        self._change_state(
            GameState.RELEASE_TIMING,
            "초록색 타이밍 구간에서 오른쪽 버튼을 놓으세요",
        )

    def release_seal(self) -> None:
        if self.state == GameState.WAIT_LEFT_CLICK:
            self._fail("실패: 왼쪽 클릭 전에 오른쪽 버튼을 놓았습니다")
            return

        if self.state != GameState.RELEASE_TIMING:
            return

        if self.left_clicked_at is None:
            self._fail("실패: 왼쪽 클릭 시간이 없습니다")
            return

        release_time = time.perf_counter() - self.left_clicked_at
        error = abs(release_time - self.config.target_release_time)
        self.last_release_error = error

        if error <= self.config.release_tolerance:
            self._change_state(GameState.DOMAIN_ACTIVE, "영역전개 · 무량공처")
        else:
            self._fail(f"타이밍 실패: {release_time:.2f}초에 버튼을 놓았습니다")

    def _fail(self, message: str) -> None:
        self._change_state(GameState.FAILED, message)

    def update(self) -> None:
        now = time.perf_counter()
        elapsed = now - self.state_started_at

        if self.state == GameState.DOMAIN_READY and elapsed > self.config.domain_ready_timeout:
            self._fail("실패: 장인 입력 제한시간을 초과했습니다")
        elif self.state == GameState.WAIT_LEFT_CLICK and self.right_pressed_at is not None:
            if now - self.right_pressed_at > self.config.right_to_left_timeout:
                self._fail("실패: 왼쪽 클릭이 너무 늦었습니다")
        elif self.state == GameState.RELEASE_TIMING and self.left_clicked_at is not None:
            max_wait = self.config.target_release_time + self.config.release_tolerance + 0.5
            if now - self.left_clicked_at > max_wait:
                self._fail("실패: 오른쪽 버튼을 너무 늦게 놓았습니다")
        elif self.state == GameState.DOMAIN_ACTIVE and elapsed > self.config.domain_active_duration:
            self.reset()
        elif self.state == GameState.FAILED and elapsed > self.config.failed_duration:
            self.reset()

    def release_elapsed(self) -> float:
        if self.left_clicked_at is None:
            return 0.0
        return max(0.0, time.perf_counter() - self.left_clicked_at)

    def release_progress(self) -> float:
        total = self.config.target_release_time + self.config.release_tolerance + 0.5
        return min(1.0, self.release_elapsed() / total)
