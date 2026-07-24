from __future__ import annotations

from typing import Protocol

from game.state import GameState


class DomainConfigProtocol(Protocol):
    target_release_time: float
    release_tolerance: float


class DomainControllerProtocol(Protocol):
    """메인 화면과 공통 입력 모듈이 의존하는 영역 컨트롤러 규약."""

    state: GameState
    result_message: str
    last_release_error: float | None
    config: DomainConfigProtocol

    def reset(self) -> None: ...

    def request_domain(self) -> None: ...

    def update(self) -> None: ...

    def release_elapsed(self) -> float: ...

    def release_progress(self) -> float: ...
