from __future__ import annotations

from characters.base import CharacterProfile, CharacterRuntime
from characters.megumi.controller import MegumiDomainController
from characters.megumi.effect import MegumiDomainEffect
from characters.megumi.input import MegumiShadowGestureInput


PROFILE = CharacterProfile(
    character_id="megumi",
    name="후시구로 메구미",
    technique="십종영법술",
    domain="감합암예정",
    voice_command="료이키 텐카이",
    seal_steps=(
        "왼손으로 Q 키를 누르고 유지합니다",
        "좌클릭을 유지한 채 마우스를 아래로 드래그합니다",
        "이어서 왼쪽이나 오른쪽으로 그림자를 충분히 펼칩니다",
        "Q를 유지한 채 좌클릭을 놓습니다",
    ),
    available=True,
    status="그림자 궤적 장인 프로토타입",
    progress_state_label="그림자 펼치기",
    show_timing_bar=False,
    show_error_stats=False,
)


def build_runtime() -> CharacterRuntime:
    """메구미 전용 모듈만 조립해 독립적인 런타임을 만든다."""

    return CharacterRuntime(
        controller=MegumiDomainController(),
        seal_input=MegumiShadowGestureInput(),
        effect=MegumiDomainEffect(),
    )
