from __future__ import annotations

import time
from dataclasses import dataclass

from game.state import GameState


@dataclass(frozen=True)
class SukunaDomainConfig:
    """스쿠나(이타도리) 복마어주자 장인의 초기 판정 수치."""

    domain_ready_timeout: float = 3.0
    left_hand_to_mouse_timeout: float = 0.70
    mouse_press_sync_tolerance: float = 0.18
    target_release_time: float = 0.85
    release_tolerance: float = 0.22
    mouse_release_sync_tolerance: float = 0.18
    domain_active_duration: float = 2.5
    failed_duration: float = 1.2


class SukunaDomainController:
    """왼손 키보드 + 오른손 마우스로 복마어주자 장인을 판정한다."""

    def __init__(self, config: SukunaDomainConfig | None = None) -> None:
        self.config = config or SukunaDomainConfig()
        self.reset()

    def reset(self) -> None:
        self.state = GameState.NORMAL
        self.state_started_at = time.perf_counter()
        self.left_hand_started_at: float | None = None
        self.mouse_seal_completed_at: float | None = None
        self.last_release_error: float | None = None
        self.result_message = "V 키 또는 음성으로 영역전개를 준비하세요"

    def _change_state(self, state: GameState, message: str) -> None:
        self.state = state
        self.state_started_at = time.perf_counter()
        self.result_message = message

    def request_domain(self) -> None:
        if self.state != GameState.NORMAL:
            return

        self.left_hand_started_at = None
        self.mouse_seal_completed_at = None
        self.last_release_error = None
        self._change_state(
            GameState.DOMAIN_READY,
            "영역 준비: 왼손으로 E 키를 누르고 유지하세요",
        )

    def begin_left_hand(self) -> None:
        if self.state != GameState.DOMAIN_READY:
            return

        self.left_hand_started_at = time.perf_counter()
        self._change_state(
            GameState.WAIT_LEFT_CLICK,
            "E를 유지한 채 좌·우클릭을 거의 동시에 누르고 유지하세요",
        )

    def cancel_left_hand(self) -> None:
        if self.state in (GameState.WAIT_LEFT_CLICK, GameState.RELEASE_TIMING):
            self._fail("실패: 영역 발동 전에 E 키를 놓았습니다")

    def complete_mouse_seal(
        self,
        *,
        left_hand_is_held: bool,
        press_gap: float,
    ) -> None:
        if self.state != GameState.WAIT_LEFT_CLICK:
            return

        if not left_hand_is_held:
            self._fail("실패: E 키를 유지하지 않았습니다")
            return

        if self.left_hand_started_at is None:
            self._fail("실패: 왼손 장인 입력 시간이 없습니다")
            return

        now = time.perf_counter()
        if now - self.left_hand_started_at > self.config.left_hand_to_mouse_timeout:
            self._fail("실패: 마우스 장인 결합이 너무 늦었습니다")
            return

        if press_gap > self.config.mouse_press_sync_tolerance:
            self._fail("실패: 좌·우클릭을 더 동시에 눌러야 합니다")
            return

        self.mouse_seal_completed_at = now
        self._change_state(
            GameState.RELEASE_TIMING,
            "초록색 구간에서 좌·우클릭을 거의 동시에 놓으세요",
        )

    def incomplete_mouse_release(self) -> None:
        if self.state == GameState.WAIT_LEFT_CLICK:
            self._fail("실패: 좌·우클릭 장인이 완성되기 전에 버튼을 놓았습니다")

    def complete_mouse_release(
        self,
        *,
        left_hand_is_held: bool,
        release_gap: float,
    ) -> None:
        if self.state != GameState.RELEASE_TIMING:
            return

        if not left_hand_is_held:
            self._fail("실패: E 키를 유지하지 않았습니다")
            return

        if self.mouse_seal_completed_at is None:
            self._fail("실패: 마우스 장인 완성 시간이 없습니다")
            return

        if release_gap > self.config.mouse_release_sync_tolerance:
            self._fail("실패: 좌·우클릭을 더 동시에 놓아야 합니다")
            return

        release_time = time.perf_counter() - self.mouse_seal_completed_at
        error = abs(release_time - self.config.target_release_time)
        self.last_release_error = error

        if error <= self.config.release_tolerance:
            self._change_state(GameState.DOMAIN_ACTIVE, "영역전개 · 복마어주자")
        else:
            self._fail(f"타이밍 실패: {release_time:.2f}초에 버튼을 놓았습니다")

    def _fail(self, message: str) -> None:
        self._change_state(GameState.FAILED, message)

    def update(self) -> None:
        now = time.perf_counter()
        elapsed = now - self.state_started_at

        if self.state == GameState.DOMAIN_READY and elapsed > self.config.domain_ready_timeout:
            self._fail("실패: E 키 입력 제한시간을 초과했습니다")
        elif self.state == GameState.WAIT_LEFT_CLICK and self.left_hand_started_at is not None:
            if now - self.left_hand_started_at > self.config.left_hand_to_mouse_timeout:
                self._fail("실패: 마우스 장인 결합이 너무 늦었습니다")
        elif self.state == GameState.RELEASE_TIMING and self.mouse_seal_completed_at is not None:
            max_wait = self.config.target_release_time + self.config.release_tolerance + 0.5
            if now - self.mouse_seal_completed_at > max_wait:
                self._fail("실패: 좌·우클릭을 너무 늦게 놓았습니다")
        elif self.state == GameState.DOMAIN_ACTIVE and elapsed > self.config.domain_active_duration:
            self.reset()
        elif self.state == GameState.FAILED and elapsed > self.config.failed_duration:
            self.reset()

    def release_elapsed(self) -> float:
        if self.mouse_seal_completed_at is None:
            return 0.0
        return max(0.0, time.perf_counter() - self.mouse_seal_completed_at)

    def release_progress(self) -> float:
        total = self.config.target_release_time + self.config.release_tolerance + 0.5
        return min(1.0, self.release_elapsed() / total)
