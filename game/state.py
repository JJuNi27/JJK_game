from enum import Enum, auto


class GameState(Enum):
    """무량공처 장인 프로토타입에서 사용하는 상태."""

    NORMAL = auto()
    DOMAIN_READY = auto()
    WAIT_LEFT_CLICK = auto()
    RELEASE_TIMING = auto()
    DOMAIN_ACTIVE = auto()
    FAILED = auto()
