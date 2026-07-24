from __future__ import annotations

import math
import time

import pygame


class YutaDomainEffect:
    """에셋 없이 코드로 그린 임시 진안상애 연출."""

    def __init__(self) -> None:
        self.started_at = time.perf_counter()

    def restart(self) -> None:
        self.started_at = time.perf_counter()

    def draw(
        self,
        surface: pygame.Surface,
        title_font: pygame.font.Font,
    ) -> None:
        elapsed = time.perf_counter() - self.started_at
        width, height = surface.get_size()
        center_x = width // 2
        horizon_y = int(height * 0.67)

        surface.fill((15, 8, 24))

        # 영역 전체를 감싸는 붉은 보랏빛 맥동
        pulse = (math.sin(elapsed * 3.2) + 1.0) * 0.5
        aura = pygame.Surface((width, height), pygame.SRCALPHA)
        aura.fill((90, 25, 105, int(28 + pulse * 28)))
        surface.blit(aura, (0, 0))

        # 중앙 십자형 구조물
        structure = pygame.Surface((width, height), pygame.SRCALPHA)
        structure_color = (120, 72, 128, 150)
        pygame.draw.rect(
            structure,
            structure_color,
            (center_x - 24, 125, 48, horizon_y - 115),
            border_radius=14,
        )
        pygame.draw.rect(
            structure,
            structure_color,
            (center_x - 185, 230, 370, 44),
            border_radius=14,
        )
        surface.blit(structure, (0, 0))

        # 땅에 꽂힌 수많은 검. 각 검은 서로 다른 복사 술식을 암시한다.
        sword_layer = pygame.Surface((width, height), pygame.SRCALPHA)
        for index in range(34):
            column = index % 17
            row = index // 17
            x = 55 + column * 62 + (row * 25)
            base_y = horizon_y + row * 82 + int(math.sin(elapsed * 2.0 + index) * 4)
            length = 62 + (index % 5) * 9
            angle = -0.16 + (index % 7) * 0.055

            tip_x = x + int(math.sin(angle) * length)
            tip_y = base_y - int(math.cos(angle) * length)
            alpha = 145 + (index % 4) * 22

            pygame.draw.line(
                sword_layer,
                (214, 194, 225, alpha),
                (x, base_y),
                (tip_x, tip_y),
                width=3,
            )
            pygame.draw.line(
                sword_layer,
                (151, 105, 170, alpha),
                (x - 10, base_y - 10),
                (x + 10, base_y - 10),
                width=3,
            )

            # 검마다 들어 있는 술식이 다르다는 점을 작은 광점으로 표현
            glow_radius = 2 + (index % 3)
            pygame.draw.circle(
                sword_layer,
                (238, 172, 225, min(255, alpha + 25)),
                (tip_x, tip_y),
                glow_radius,
            )

        surface.blit(sword_layer, (0, 0))

        # 검을 뽑을 때 위로 솟는 잔광
        draw_progress = min(1.0, elapsed / 0.7)
        glow_height = int(260 * draw_progress)
        pygame.draw.line(
            surface,
            (242, 212, 244),
            (center_x, horizon_y),
            (center_x, horizon_y - glow_height),
            width=6,
        )

        title = title_font.render("영역전개 · 진안상애", True, (248, 231, 246))
        surface.blit(title, title.get_rect(center=(center_x, 72)))
