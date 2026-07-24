from __future__ import annotations

from characters.base import CharacterProfile, CharacterRuntime
from characters.yuta.controller import YutaDomainController
from characters.yuta.effect import YutaDomainEffect
from characters.yuta.input import YutaSwordDrawInput


PROFILE = CharacterProfile(
    character_id="yuta",
    name="옷코츠 유타",
    technique="모방 · 리카",
    domain="진안상애",
    voice_command="료이키 텐카이",
    seal_steps=(
        "왼손으로 C 키를 누르고 반지 연결을 유지합니다",
        "화면 아래쪽을 좌클릭한 채 검을 뽑듯 위로 드래그합니다",
        "C를 유지한 채 좌클릭을 놓습니다",
    ),
    available=True,
    status="반지 연결 · 검 뽑기 장인 프로토타입",
    progress_state_label="검 뽑기",
    show_timing_bar=False,
    show_error_stats=False,
)


def build_runtime() -> CharacterRuntime:
    """유타 전용 모듈만 조립해 독립적인 런타임을 만든다."""

    return CharacterRuntime(
        controller=YutaDomainController(),
        seal_input=YutaSwordDrawInput(),
        effect=YutaDomainEffect(),
    )
