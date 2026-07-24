from __future__ import annotations

from characters.base import CharacterProfile, CharacterRuntime
from effects.muryang_effect import MuryangEffect
from game.domain_controller import DomainController
from game_input.mouse_seal_input import MouseSealInput


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
    return CharacterRuntime(
        controller=DomainController(),
        seal_input=MouseSealInput(),
        effect=MuryangEffect(),
    )
