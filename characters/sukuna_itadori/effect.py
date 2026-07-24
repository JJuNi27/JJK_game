from __future__ import annotations

import math
import time

import pygame


class SukunaDomainEffect:
    """에셋 없이 코드로 그린 임시 복마어주자 연출."""

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
        surface.fill((20, 7, 9))

        pulse = (math.sin(elapsed * 4.0) + 1.0) * 0.5
        horizon_y = int(height * 0.68)

        fog = pygame.Surface((width, height), pygame.SRCALPHA)
        fog.fill((120, 12, 18, int(35 + pulse * 25)))
        surface.blit(fog, (0, 0))

        center_x = width // 2
        shrine_color = (62, 18, 21)
        edge_color = (151, 48, 48)

        pygame.draw.rect(
            surface,
            shrine_color,
            (center_x - 190, horizon_y - 150, 380, 150),
        )
        pygame.draw.polygon(
            surface,
            shrine_color,
            [
                (center_x - 240, horizon_y - 150),
                (center_x, horizon_y - 275),
                (center_x + 240, horizon_y - 150),
            ],
        )
        pygame.draw.line(
            surface,
            edge_color,
            (center_x - 240, horizon_y - 150),
            (center_x, horizon_y - 275),
            width=4,
        )
        pygame.draw.line(
            surface,
            edge_color,
            (center_x, horizon_y - 275),
            (center_x + 240, horizon_y - 150),
            width=4,
        )

        for offset in (-125, -45, 45, 125):
            pygame.draw.rect(
                surface,
                (11, 6, 7),
                (center_x + offset - 22, horizon_y - 105, 44, 105),
            )

        slash_layer = pygame.Surface((width, height), pygame.SRCALPHA)
        for index in range(11):
            phase = elapsed * 330 + index * 127
            x = int((phase % (width + 360)) - 180)
            y = 95 + (index * 47) % 470
            length = 165 + (index % 4) * 35
            alpha = 95 + (index % 3) * 35
            pygame.draw.line(
                slash_layer,
                (235, 72, 72, alpha),
                (x - length // 2, y + 36),
                (x + length // 2, y - 36),
                width=3,
            )
        surface.blit(slash_layer, (0, 0))

        title = title_font.render("영역전개 · 복마어주자", True, (245, 222, 220))
        surface.blit(title, title.get_rect(center=(center_x, 72)))
