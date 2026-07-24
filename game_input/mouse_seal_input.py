from __future__ import annotations

import pygame

from game.domain_controller import DomainController


class MouseSealInput:
    """마우스 장인 입력을 DomainController의 의미 단위 명령으로 변환한다.

    실제 버튼 이벤트는 이 클래스만 알고 있다. 나중에 카메라 장인 인식을
    추가할 때는 같은 controller 메서드를 호출하는 CameraSealInput을 만들면 된다.
    """

    def __init__(self) -> None:
        self._right_held = False

    def handle_event(
        self,
        event: pygame.event.Event,
        controller: DomainController,
    ) -> None:
        if event.type == pygame.MOUSEBUTTONDOWN:
            if event.button == 3:
                self._right_held = True
                controller.begin_seal()
            elif event.button == 1:
                controller.combine_seal(right_is_held=self._right_held)

        elif event.type == pygame.MOUSEBUTTONUP and event.button == 3:
            self._right_held = False
            controller.release_seal()
