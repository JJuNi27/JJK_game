from __future__ import annotations

import time
from dataclasses import dataclass

from game.state import GameState


@dataclass(frozen=True)
class MegumiDomainConfig:
    """감합암예정 그림자 궤적 입력의 초기 판정 수치."""

    domain_ready_timeout: float = 3.0
    q_to_drag_timeout: float = 0.80
    downward_min_distance: float = 130.0
    downward_horizontal_tolerance: float = 80.0
    horizontal_min_distance: float = 180.0
    horizontal_vertical_tolerance: float = 100.0
    total_gesture_timeout: float = 1.80
    domain_active_duration: float = 2.5
    failed_duration: float = 1.2

    # 공통 UI Protocol 호환용 값. 메구미는 타이밍 바를 표시하지 않는다.
    target_release_time: float = 0.0
    release_tolerance: float = 0.0


class MegumiDomainController:
    """Q 키와 마우스 드래그로 감합암예정의 그림자 궤적을 판정한다."""

    def __init__(self, config: MegumiDomainConfig | None = None) -> None:
        self.config = config or MegumiDomainConfig()
        self.reset()

    def reset(self) -> None:
        self.state = GameState.NORMAL
        self.state_started_at = time.perf_counter()
        self.q_started_at: float | None = None
        self.drag_started_at: float | None = None
        self.drag_start: tuple[int, int] | None = None
        self.spread_anchor: tuple[int, int] | None = None
        self.horizontal_distance = 0.0
        self.last_release_error: float | None = None
        self.result_message = "V 키 또는 음성으로 영역전개를 준비하세요"

    def _change_state(self, state: GameState, message: str) -> None:
        self.state = state
        self.state_started_at = time.perf_counter()
        self.result_message = message

    def request_domain(self) -> None:
        if self.state != GameState.NORMAL:
            return

        self.q_started_at = None
        self.drag_started_at = None
        self.drag_start = None
        self.spread_anchor = None
        self.horizontal_distance = 0.0
        self.last_release_error = None
        self._change_state(
            GameState.DOMAIN_READY,
            "영역 준비: 왼손으로 Q 키를 누르고 유지하세요",
        )

    def begin_left_hand(self) -> None:
        if self.state != GameState.DOMAIN_READY:
            return

        self.q_started_at = time.perf_counter()
        self._change_state(
            GameState.WAIT_LEFT_CLICK,
            "Q를 유지한 채 좌클릭하고 마우스를 아래로 드래그하세요",
        )

    def cancel_left_hand(self) -> None:
        if self.state in (GameState.WAIT_LEFT_CLICK, GameState.RELEASE_TIMING):
            self._fail("실패: 감합암예정이 완성되기 전에 Q 키를 놓았습니다")

    def begin_drag(
        self,
        position: tuple[int, int],
        *,
        left_hand_is_held: bool,
    ) -> None:
        if self.state != GameState.WAIT_LEFT_CLICK:
            return

        if not left_hand_is_held:
            self._fail("실패: Q 키를 유지하지 않았습니다")
            return

        if self.q_started_at is None:
            self._fail("실패: 왼손 장인 입력 시간이 없습니다")
            return

        if time.perf_counter() - self.q_started_at > self.config.q_to_drag_timeout:
            self._fail("실패: 그림자 드래그 시작이 너무 늦었습니다")
            return

        self.drag_started_at = time.perf_counter()
        self.drag_start = position
        self.result_message = "좌클릭을 유지한 채 마우스를 아래로 드래그하세요"

    def update_drag(self, position: tuple[int, int]) -> None:
        if self.drag_start is None or self.drag_started_at is None:
            return

        if time.perf_counter() - self.drag_started_at > self.config.total_gesture_timeout:
            self._fail("실패: 그림자 궤적 입력 시간이 너무 길었습니다")
            return

        if self.state == GameState.WAIT_LEFT_CLICK:
            dx = position[0] - self.drag_start[0]
            dy = position[1] - self.drag_start[1]

            if (
                dy >= self.config.downward_min_distance
                and abs(dx) <= self.config.downward_horizontal_tolerance
            ):
                self.spread_anchor = position
                self.horizontal_distance = 0.0
                self._change_state(
                    GameState.RELEASE_TIMING,
                    "그림자를 왼쪽이나 오른쪽으로 충분히 펼치세요",
                )
            return

        if self.state == GameState.RELEASE_TIMING and self.spread_anchor is not None:
            horizontal = abs(position[0] - self.spread_anchor[0])
            vertical_drift = abs(position[1] - self.spread_anchor[1])

            if vertical_drift <= self.config.horizontal_vertical_tolerance:
                self.horizontal_distance = max(self.horizontal_distance, float(horizontal))
                self.result_message = (
                    f"그림자 펼침: {self.horizontal_distance:.0f} / "
                    f"{self.config.horizontal_min_distance:.0f}px"
                )

    def finish_drag(self, *, left_hand_is_held: bool) -> None:
        if self.drag_start is None:
            return

        if not left_hand_is_held:
            self._fail("실패: Q 키를 유지하지 않았습니다")
            return

        if self.state == GameState.WAIT_LEFT_CLICK:
            self._fail("실패: 마우스를 아래로 충분히 내리지 않았습니다")
            return

        if self.state != GameState.RELEASE_TIMING:
            return

        if self.horizontal_distance < self.config.horizontal_min_distance:
            self._fail("실패: 그림자를 좌우로 충분히 펼치지 않았습니다")
            return

        self._change_state(GameState.DOMAIN_ACTIVE, "영역전개 · 감합암예정")

    def _fail(self, message: str) -> None:
        self._change_state(GameState.FAILED, message)

    def update(self) -> None:
        now = time.perf_counter()
        elapsed = now - self.state_started_at

        if self.state == GameState.DOMAIN_READY and elapsed > self.config.domain_ready_timeout:
            self._fail("실패: Q 키 입력 제한시간을 초과했습니다")

        elif self.state == GameState.WAIT_LEFT_CLICK:
            if (
                self.drag_started_at is None
                and self.q_started_at is not None
                and now - self.q_started_at > self.config.q_to_drag_timeout
            ):
                self._fail("실패: 그림자 드래그 시작이 너무 늦었습니다")
            elif (
                self.drag_started_at is not None
                and now - self.drag_started_at > self.config.total_gesture_timeout
            ):
                self._fail("실패: 그림자 궤적 입력 시간이 너무 길었습니다")

        elif self.state == GameState.RELEASE_TIMING:
            if (
                self.drag_started_at is not None
                and now - self.drag_started_at > self.config.total_gesture_timeout
            ):
                self._fail("실패: 그림자 궤적 입력 시간이 너무 길었습니다")

        elif self.state == GameState.DOMAIN_ACTIVE and elapsed > self.config.domain_active_duration:
            self.reset()

        elif self.state == GameState.FAILED and elapsed > self.config.failed_duration:
            self.reset()

    def release_elapsed(self) -> float:
        if self.drag_started_at is None:
            return 0.0
        return max(0.0, time.perf_counter() - self.drag_started_at)

    def release_progress(self) -> float:
        if self.config.horizontal_min_distance <= 0:
            return 0.0
        return min(1.0, self.horizontal_distance / self.config.horizontal_min_distance)
