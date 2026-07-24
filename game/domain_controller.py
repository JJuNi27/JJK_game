"""고죠 모듈의 이전 import 경로를 위한 임시 호환 파일.

새 코드는 `characters.gojo.controller`를 직접 사용한다.
"""

from characters.gojo.controller import (
    GojoDomainConfig as DomainConfig,
    GojoDomainController as DomainController,
)

__all__ = ["DomainConfig", "DomainController"]
