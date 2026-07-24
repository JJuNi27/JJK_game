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


GOJO = CharacterProfile(
    character_id="gojo",
    name="고죠 사토루",
    technique="무하한 주술",
    domain="무량공처",
    voice_command="료이키 텐카이",
)


CHARACTER_SLOTS = (
    GOJO,
    CharacterProfile(
        character_id="locked_1",
        name="준비 중",
        technique="설정 카드 승인 후 추가",
        domain="영역 미정",
        voice_command="-",
        available=False,
    ),
    CharacterProfile(
        character_id="locked_2",
        name="준비 중",
        technique="설정 카드 승인 후 추가",
        domain="영역 미정",
        voice_command="-",
        available=False,
    ),
)
