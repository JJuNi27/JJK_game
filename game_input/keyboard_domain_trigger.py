from __future__ import annotations

import pygame

from game.domain_protocol import DomainControllerProtocol


class KeyboardDomainTrigger:
    """키보드로 영역 준비와 연습 제어 입력을 처리한다.

    V 키는 음성인식의 대체 입력이고, R은 현재 장인 시도만 초기화한다.
    T는 누적 연습 통계 초기화를 요청하며, 실제 통계 객체는 이 모듈이 직접
    알지 않도록 bool 값으로 Main에 전달한다.
    """

    def handle_event(
        self,
        event: pygame.event.Event,
        controller: DomainControllerProtocol,
    ) -> bool:
        """통계 초기화 요청이 발생했으면 True를 반환한다."""
        if event.type != pygame.KEYDOWN:
            return False

        if event.key == pygame.K_v:
            controller.request_domain()
        elif event.key == pygame.K_r:
            controller.reset()
        elif event.key == pygame.K_t:
            return True

        return False
