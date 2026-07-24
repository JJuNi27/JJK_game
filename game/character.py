from __future__ import annotations

from dataclasses import dataclass


@dataclass(frozen=True)
class CharacterProfile:
    """캐릭터 선택 화면과 이후 전투 시스템이 공유할 최소 정보."""

    character_id: str
    name: str
    technique: str
    domain: str
    voice_command: str
    available: bool = True
    status: str = "선택 가능"


GOJO = CharacterProfile(
    character_id="gojo",
    name="고죠 사토루",
    technique="무하한 주술",
    domain="무량공처",
    voice_command="료이키 텐카이",
    status='음성: "료이키 텐카이"',
)

SUKUNA_DRAFT = CharacterProfile(
    character_id="sukuna_draft",
    name="료멘 스쿠나",
    technique="설정 카드 검토 중",
    domain="복마어주자",
    voice_command="료이키 텐카이",
    available=False,
    status="설정 카드 초안 검토 필요",
)

FUTURE_SLOT = CharacterProfile(
    character_id="future_slot",
    name="다음 캐릭터",
    technique="후보 미정",
    domain="미정",
    voice_command="-",
    available=False,
    status="스쿠나 이후 추가 예정",
)


CHARACTER_SLOTS = (
    GOJO,
    SUKUNA_DRAFT,
    FUTURE_SLOT,
)
