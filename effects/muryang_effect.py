from __future__ import annotations

import math
import time

import pygame


class MuryangEffect:
    """에셋 없이 코드로 그리는 임시 무량공처 효과.

    최종 연출이 아니라 입력 성공이 실제 화면 변화로 이어지는지 확인하기 위한
    프로토타입용 효과다.
    """

    def __init__(self) -> None:
        self.started_at = time.perf_counter()

    def restart(self) -> None:
        self.started_at = time.perf_counter()

    def draw(self, surface: pygame.Surface, font: pygame.font.Font) -> None:
        width, height = surface.get_size()
        center = (width // 2, height // 2)
        elapsed = time.perf_counter() - self.started_at

        surface.fill((5, 7, 20))

        # 중심에서 퍼져나가는 원형 파장
        for index in range(8):
            phase = (elapsed * 180 + index * 85) % 680
            radius = int(30 + phase)
            alpha = max(0, 190 - int(phase * 0.24))

            ring_layer = pygame.Surface((width, height), pygame.SRCALPHA)
            pygame.draw.circle(
                ring_layer,
                (160, 190, 255, alpha),
                center,
                radius,
                width=3,
            )
            surface.blit(ring_layer, (0, 0))

        # 별처럼 흩어진 점
        for index in range(110):
            angle = index * 2.399 + elapsed * 0.08
            distance = 40 + ((index * 47 + elapsed * 35) % 420)
            x = center[0] + math.cos(angle) * distance
            y = center[1] + math.sin(angle) * distance * 0.58
            size = 1 + index % 3
            pygame.draw.circle(surface, (220, 230, 255), (int(x), int(y)), size)

        title = font.render("UNLIMITED VOID", True, (245, 247, 255))
        surface.blit(title, title.get_rect(center=(center[0], center[1] - 10)))
