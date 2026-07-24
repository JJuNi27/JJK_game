"""하위 호환용 캐릭터 메타데이터 진입점.

새 코드는 `characters` 패키지를 사용한다. 기존 UI import가 깨지지 않도록
이 파일은 프로필과 슬롯을 다시 내보낸다.
"""

from characters.base import CharacterProfile
from characters.gojo import PROFILE as GOJO
from characters.megumi import PROFILE as MEGUMI
from characters.registry import CHARACTER_SLOTS
from characters.sukuna_itadori import PROFILE as SUKUNA_ITADORI

# 과거 이름을 사용하는 외부 import가 당장 깨지지 않도록 잠시 유지한다.
MEGUMI_DRAFT = MEGUMI

__all__ = [
    "CharacterProfile",
    "GOJO",
    "SUKUNA_ITADORI",
    "MEGUMI",
    "MEGUMI_DRAFT",
    "CHARACTER_SLOTS",
]
