from __future__ import annotations

from enum import Enum, auto


class AppScene(Enum):
    """프로토타입의 상위 화면 상태."""

    CHARACTER_SELECT = auto()
    PRACTICE = auto()
