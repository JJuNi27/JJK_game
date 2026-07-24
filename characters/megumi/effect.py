from __future__ import annotations

import math
import time

import pygame


class MegumiDomainEffect:
    """에셋 없이 코드로 그린 임시 감합암예정 연출."""

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

        surface.fill((7, 10, 15))

        # 바닥을 덮는 유동적인 그림자 층
        shadow_layer = pygame.Surface((width, height), pygame.SRCALPHA)
        for index in range(7):
            phase = elapsed * 1.8 + index * 0.85
            wave_y = int(height * 0.58 + math.sin(phase) * 24 + index * 18)
            points = [(0, height)]
            for x in range(0, width + 80, 80):
                y = wave_y + int(math.sin(x * 0.012 + phase) * 18)
                points.append((x, y))
            points.append((width, height))
            alpha = 40 + index * 18
            pygame.draw.polygon(shadow_layer, (12, 20, 30, alpha), points)
        surface.blit(shadow_layer, (0, 0))

        # 식신과 분신을 암시하는 실루엣
        silhouette = (19, 28, 38)
        edge = (76, 101, 125)
        for offset, scale in [(-300, 0.8), (-145, 1.1), (145, 1.1), (300, 0.8)]:
            x = center_x + offset
            body_y = int(height * 0.56)
            pygame.draw.ellipse(
                surface,
                silhouette,
                (x - int(45 * scale), body_y, int(90 * scale), int(135 * scale)),
            )
            pygame.draw.circle(
                surface,
                silhouette,
                (x, body_y - int(20 * scale)),
                int(34 * scale),
            )
            pygame.draw.line(
                surface,
                edge,
                (x - int(30 * scale), body_y + int(45 * scale)),
                (x + int(30 * scale), body_y + int(45 * scale)),
                width=2,
            )

        # 그림자에서 솟는 파동
        pulse_layer = pygame.Surface((width, height), pygame.SRCALPHA)
        for index in range(6):
            radius = int((elapsed * 170 + index * 110) % 720)
            alpha = max(0, 125 - int(radius * 0.14))
            pygame.draw.ellipse(
                pulse_layer,
                (80, 115, 145, alpha),
                (center_x - radius, int(height * 0.55) - radius // 5, radius * 2, max(20, radius // 2)),
                width=2,
            )
        surface.blit(pulse_layer, (0, 0))

        title = title_font.render("영역전개 · 감합암예정", True, (218, 230, 240))
        surface.blit(title, title.get_rect(center=(center_x, 74)))

        subtitle_font = pygame.font.Font(title_font.get_linesize() and title_font.get_height() and None, 24)
        subtitle = subtitle_font.render("미완성 영역", True, (137, 156, 176))
        surface.blit(subtitle, subtitle.get_rect(center=(center_x, 124)))
