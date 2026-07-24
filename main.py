from __future__ import annotations

import sys

import pygame

from effects.muryang_effect import MuryangEffect
from game.domain_controller import DomainController
from game.state import GameState


WIDTH = 1100
HEIGHT = 700
FPS = 60


def draw_text(
    surface: pygame.Surface,
    font: pygame.font.Font,
    text: str,
    position: tuple[int, int],
    color: tuple[int, int, int] = (235, 238, 245),
) -> None:
    rendered = font.render(text, True, color)
    surface.blit(rendered, position)


def draw_timing_bar(
    surface: pygame.Surface,
    controller: DomainController,
) -> None:
    """우클릭 해제 타이밍을 확인하는 연습용 UI."""

    x, y, width, height = 190, 500, 720, 34
    pygame.draw.rect(surface, (38, 42, 56), (x, y, width, height), border_radius=10)

    total = (
        controller.config.target_release_time
        + controller.config.release_tolerance
        + 0.5
    )
    target_start = (
        controller.config.target_release_time
        - controller.config.release_tolerance
    ) / total
    target_end = (
        controller.config.target_release_time
        + controller.config.release_tolerance
    ) / total

    zone_x = x + int(width * target_start)
    zone_width = max(1, int(width * (target_end - target_start)))
    pygame.draw.rect(
        surface,
        (74, 150, 110),
        (zone_x, y, zone_width, height),
        border_radius=8,
    )

    marker_x = x + int(width * controller.release_progress())
    pygame.draw.line(
        surface,
        (245, 247, 255),
        (marker_x, y - 12),
        (marker_x, y + height + 12),
        width=4,
    )


def draw_practice_screen(
    surface: pygame.Surface,
    controller: DomainController,
    title_font: pygame.font.Font,
    body_font: pygame.font.Font,
    small_font: pygame.font.Font,
) -> None:
    surface.fill((18, 21, 31))

    draw_text(surface, title_font, "Unlimited Void Seal Practice", (54, 44))
    draw_text(surface, body_font, f"STATE: {controller.state.name}", (58, 135))
    draw_text(surface, body_font, controller.result_message, (58, 185))

    instructions = [
        "1. Press V to enter DOMAIN_READY",
        "2. Hold RIGHT mouse button",
        "3. While holding RIGHT, click LEFT",
        "4. Release RIGHT inside the green timing zone",
        "R: reset    ESC: quit",
    ]

    for index, line in enumerate(instructions):
        draw_text(surface, small_font, line, (62, 280 + index * 38), (190, 197, 214))

    if controller.state == GameState.RELEASE_TIMING:
        draw_timing_bar(surface, controller)
        elapsed = controller.release_elapsed()
        draw_text(
            surface,
            small_font,
            f"release timer: {elapsed:.2f}s",
            (190, 552),
        )

    if controller.state == GameState.FAILED:
        overlay = pygame.Surface((WIDTH, HEIGHT), pygame.SRCALPHA)
        overlay.fill((115, 20, 25, 55))
        surface.blit(overlay, (0, 0))


def main() -> int:
    pygame.init()
    pygame.display.set_caption("JJK Game - Unlimited Void Prototype")
    screen = pygame.display.set_mode((WIDTH, HEIGHT))
    clock = pygame.time.Clock()

    title_font = pygame.font.Font(None, 54)
    body_font = pygame.font.Font(None, 35)
    small_font = pygame.font.Font(None, 27)

    controller = DomainController()
    effect = MuryangEffect()
    was_domain_active = False

    running = True
    while running:
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False
            elif event.type == pygame.KEYDOWN and event.key == pygame.K_ESCAPE:
                running = False
            else:
                controller.handle_event(event)

        controller.update()

        is_domain_active = controller.state == GameState.DOMAIN_ACTIVE
        if is_domain_active and not was_domain_active:
            effect.restart()

        if is_domain_active:
            effect.draw(screen, title_font)
        else:
            draw_practice_screen(
                screen,
                controller,
                title_font,
                body_font,
                small_font,
            )

        was_domain_active = is_domain_active
        pygame.display.flip()
        clock.tick(FPS)

    pygame.quit()
    return 0


if __name__ == "__main__":
    sys.exit(main())
