from __future__ import annotations

from characters.base import CharacterProfile, CharacterRuntime
from characters.gojo.controller import GojoDomainController
from characters.gojo.effect import GojoDomainEffect
from characters.gojo.input import GojoSealInput


PROFILE = CharacterProfile(
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


def build_runtime() -> CharacterRuntime:
    """고죠 전용 모듈만 조립해 독립적인 런타임을 만든다."""

    return CharacterRuntime(
        controller=GojoDomainController(),
        seal_input=GojoSealInput(),
        effect=GojoDomainEffect(),
    )
