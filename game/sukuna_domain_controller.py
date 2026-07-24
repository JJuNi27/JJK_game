"""스쿠나 모듈의 이전 import 경로를 위한 임시 호환 파일.

새 코드는 `characters.sukuna_itadori.controller`를 직접 사용한다.
"""

from characters.sukuna_itadori.controller import (
    SukunaDomainConfig,
    SukunaDomainController,
)

__all__ = ["SukunaDomainConfig", "SukunaDomainController"]
