from __future__ import annotations

import time
from dataclasses import dataclass

from game.state import GameState


@dataclass(frozen=True)
class YutaDomainConfig:
    """진안상애의 반지 연결 + 검 뽑기 궤적 초기 판정 수치."""

    domain_ready_timeout: float = 3.0
    ring_to_draw_timeout: float = 0.80
    upward_min_distance: float = 190.0
    upward_horizontal_tolerance: float = 100.0
    total_gesture_timeout: float = 1.60
    domain_active_duration: float = 2.5
    failed_duration: float = 1.2

    # 공통 UI Protocol 호환용 값. 유타는 타이밍 바를 표시하지 않는다.
    target_release_time: float = 0.0
    release_tolerance: float = 0.0


class YutaDomainController:
    """C 키 반지 연결과 위쪽 마우스 드래그로 진안상애를 판정한다."""

    def __init__(self, config: YutaDomainConfig | None = None) -> None:
        self.config = config or YutaDomainConfig()
        self.reset()

    def reset(self) -> None:
        self.state = GameState.NORMAL
        self.state_started_at = time.perf_counter()
        self.ring_started_at: float | None = None
        self.drag_started_at: float | None = None
        self.drag_start: tuple[int, int] | None = None
        self.upward_distance = 0.0
        self.last_release_error: float | None = None
        self.result_message = "V 키 또는 음성으로 영역전개를 준비하세요"

    def _change_state(self, state: GameState, message: str) -> None:
        self.state = state
        self.state_started_at = time.perf_counter()
        self.result_message = message

    def request_domain(self) -> None:
        if self.state != GameState.NORMAL:
            return

        self.ring_started_at = None
        self.drag_started_at = None
        self.drag_start = None
        self.upward_distance = 0.0
        self.last_release_error = None
        self._change_state(
            GameState.DOMAIN_READY,
            "영역 준비: 왼손으로 C 키를 누르고 반지 연결을 유지하세요",
        )

    def begin_ring_connection(self) -> None:
        if self.state != GameState.DOMAIN_READY:
            return

        self.ring_started_at = time.perf_counter()
        self._change_state(
            GameState.WAIT_LEFT_CLICK,
            "C를 유지한 채 화면 아래쪽을 좌클릭하고 위로 드래그하세요",
        )

    def cancel_ring_connection(self) -> None:
        if self.state in (GameState.WAIT_LEFT_CLICK, GameState.RELEASE_TIMING):
            self._fail("실패: 진안상애가 완성되기 전에 C 키를 놓았습니다")

    def begin_draw(
        self,
        position: tuple[int, int],
        *,
        ring_is_connected: bool,
    ) -> None:
        if self.state != GameState.WAIT_LEFT_CLICK:
            return

        if not ring_is_connected:
            self._fail("실패: C 키로 반지 연결을 유지하지 않았습니다")
            return

        if self.ring_started_at is None:
            self._fail("실패: 반지 연결 입력 시간이 없습니다")
            return

        if time.perf_counter() - self.ring_started_at > self.config.ring_to_draw_timeout:
            self._fail("실패: 검 뽑기 시작이 너무 늦었습니다")
            return

        self.drag_started_at = time.perf_counter()
        self.drag_start = position
        self.upward_distance = 0.0
        self._change_state(
            GameState.RELEASE_TIMING,
            "좌클릭을 유지한 채 검을 뽑듯 마우스를 위로 올리세요",
        )

    def update_draw(self, position: tuple[int, int]) -> None:
        if (
            self.state != GameState.RELEASE_TIMING
            or self.drag_start is None
            or self.drag_started_at is None
        ):
            return

        if time.perf_counter() - self.drag_started_at > self.config.total_gesture_timeout:
            self._fail("실패: 검 뽑기 입력 시간이 너무 길었습니다")
            return

        dx = position[0] - self.drag_start[0]
        upward = self.drag_start[1] - position[1]

        if abs(dx) <= self.config.upward_horizontal_tolerance:
            self.upward_distance = max(self.upward_distance, float(upward))
            self.result_message = (
                f"검 뽑기: {self.upward_distance:.0f} / "
                f"{self.config.upward_min_distance:.0f}px"
            )

    def finish_draw(self, *, ring_is_connected: bool) -> None:
        if self.drag_start is None:
            return

        if not ring_is_connected:
            self._fail("실패: C 키로 반지 연결을 유지하지 않았습니다")
            return

        if self.state != GameState.RELEASE_TIMING:
            return

        if self.upward_distance < self.config.upward_min_distance:
            self._fail("실패: 검을 위로 충분히 뽑지 않았습니다")
            return

        self._change_state(GameState.DOMAIN_ACTIVE, "영역전개 · 진안상애")

    def _fail(self, message: str) -> None:
        self._change_state(GameState.FAILED, message)

    def update(self) -> None:
        now = time.perf_counter()
        elapsed = now - self.state_started_at

        if self.state == GameState.DOMAIN_READY and elapsed > self.config.domain_ready_timeout:
            self._fail("실패: C 키 입력 제한시간을 초과했습니다")

        elif self.state == GameState.WAIT_LEFT_CLICK:
            if (
                self.ring_started_at is not None
                and now - self.ring_started_at > self.config.ring_to_draw_timeout
            ):
                self._fail("실패: 검 뽑기 시작이 너무 늦었습니다")

        elif self.state == GameState.RELEASE_TIMING:
            if (
                self.drag_started_at is not None
                and now - self.drag_started_at > self.config.total_gesture_timeout
            ):
                self._fail("실패: 검 뽑기 입력 시간이 너무 길었습니다")

        elif self.state == GameState.DOMAIN_ACTIVE and elapsed > self.config.domain_active_duration:
            self.reset()

        elif self.state == GameState.FAILED and elapsed > self.config.failed_duration:
            self.reset()

    def release_elapsed(self) -> float:
        if self.drag_started_at is None:
            return 0.0
        return max(0.0, time.perf_counter() - self.drag_started_at)

    def release_progress(self) -> float:
        if self.config.upward_min_distance <= 0:
            return 0.0
        return min(1.0, self.upward_distance / self.config.upward_min_distance)
