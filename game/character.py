from __future__ import annotations

from dataclasses import dataclass


@dataclass(frozen=True)
class CharacterProfile:
    """선택 화면과 연습 UI가 공유하는 캐릭터 메타데이터."""

    character_id: str
    name: str
    technique: str
    domain: str
    voice_command: str
    seal_steps: tuple[str, ...] = ()
    available: bool = True
    status: str = "선택 가능"


GOJO = CharacterProfile(
    character_id="gojo",
    name="고죠 사토루",
    technique="무하한 주술",
    domain="무량공처",
    voice_command="료이키 텐카이",
    seal_steps=(
        "마우스 오른쪽 버튼을 누르고 유지합니다",
        "오른쪽 버튼을 유지한 채 왼쪽 버튼을 클릭합니다",
        "초록색 구간에서 오른쪽 버튼을 놓습니다",
    ),
    status='한 손 장인 · 음성 "료이키 텐카이"',
)

SUKUNA_ITADORI = CharacterProfile(
    character_id="sukuna_itadori",
    name="스쿠나(이타도리)",
    technique="주복사 · 해/팔 · 푸가",
    domain="복마어주자",
    voice_command="료이키 텐카이",
    seal_steps=(
        "왼손으로 E 키를 누르고 유지합니다",
        "E를 유지한 채 좌·우클릭을 거의 동시에 누르고 유지합니다",
        "초록색 구간에서 좌·우클릭을 거의 동시에 놓습니다",
    ),
    available=True,
    status="양손 장인 프로토타입",
)

FUTURE_SLOT = CharacterProfile(
    character_id="future_slot",
    name="다음 캐릭터",
    technique="후보 미정",
    domain="미정",
    voice_command="-",
    available=False,
    status="스쿠나 테스트 이후 추가 예정",
)


CHARACTER_SLOTS = (
    GOJO,
    SUKUNA_ITADORI,
    FUTURE_SLOT,
)
