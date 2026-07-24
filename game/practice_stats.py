from __future__ import annotations

from dataclasses import dataclass


@dataclass
class PracticeStats:
    """자유 연습장에서 장인 입력 결과를 누적한다.

    DomainController의 상태 판정과 분리해 두어, 나중에 다른 영역이나 Unity
    연습장에서도 같은 형태의 통계 구조를 재사용할 수 있다.
    """

    attempts: int = 0
    successes: int = 0
    failures: int = 0
    current_streak: int = 0
    best_streak: int = 0
    last_release_error: float | None = None
    total_release_error: float = 0.0
    measured_release_count: int = 0

    def record_success(self, release_error: float | None) -> None:
        self.attempts += 1
        self.successes += 1
        self.current_streak += 1
        self.best_streak = max(self.best_streak, self.current_streak)
        self._record_release_error(release_error)

    def record_failure(self, release_error: float | None) -> None:
        self.attempts += 1
        self.failures += 1
        self.current_streak = 0
        self._record_release_error(release_error)

    def _record_release_error(self, release_error: float | None) -> None:
        self.last_release_error = release_error
        if release_error is None:
            return

        self.total_release_error += release_error
        self.measured_release_count += 1

    @property
    def success_rate(self) -> float:
        if self.attempts == 0:
            return 0.0
        return self.successes / self.attempts

    @property
    def average_release_error(self) -> float | None:
        if self.measured_release_count == 0:
            return None
        return self.total_release_error / self.measured_release_count

    def reset(self) -> None:
        self.attempts = 0
        self.successes = 0
        self.failures = 0
        self.current_streak = 0
        self.best_streak = 0
        self.last_release_error = None
        self.total_release_error = 0.0
        self.measured_release_count = 0
