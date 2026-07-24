from __future__ import annotations

import pygame

from game.domain_controller import DomainController


class KeyboardDomainTrigger:
    """키보드로 영역 준비와 초기화를 요청한다.

    현재는 V 키가 음성인식의 임시 대체 입력이다. 나중에
    VoiceDomainTrigger를 추가하더라도 DomainController는 수정하지 않는다.
    """

    def handle_event(
        self,
        event: pygame.event.Event,
        controller: DomainController,
    ) -> None:
        if event.type != pygame.KEYDOWN:
            return

        if event.key == pygame.K_v:
            controller.request_domain()
        elif event.key == pygame.K_r:
            controller.reset()
