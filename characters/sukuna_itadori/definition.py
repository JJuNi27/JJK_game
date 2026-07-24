from __future__ import annotations

from characters.base import CharacterProfile, CharacterRuntime
from characters.sukuna_itadori.controller import SukunaDomainController
from characters.sukuna_itadori.effect import SukunaDomainEffect
from characters.sukuna_itadori.input import SukunaTwoHandSealInput


PROFILE = CharacterProfile(
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
    status="양손 장인 프로토타입",
)


def build_runtime() -> CharacterRuntime:
    """스쿠나(이타도리) 전용 모듈만 조립해 런타임을 만든다."""

    return CharacterRuntime(
        controller=SukunaDomainController(),
        seal_input=SukunaTwoHandSealInput(),
        effect=SukunaDomainEffect(),
    )
