from __future__ import annotations

import sys

import pygame
from effects.muryang_effect import MuryangEffect
from game.domain_controller import DomainController
from game.state import GameState
from game_input.keyboard_domain_trigger import KeyboardDomainTrigger
from game_input.mouse_seal_input import MouseSealInput


WIDTH = 1100
HEIGHT = 700
FPS = 60

STATE_LABELS = {
    GameState.NORMAL: "대기",
    GameState.DOMAIN_READY: "영역 준비",
    GameState.WAIT_LEFT_CLICK: "장인 결합 대기",
    GameState.RELEASE_TIMING: "해제 타이밍",
    GameState.DOMAIN_ACTIVE: "무량공처 발동",
    GameState.FAILED: "실패",
}


def load_korean_font(size: int) -> pygame.font.Font:
    """운영체제에 설치된 한글 폰트를 찾아 반환한다.

    Windows에서는 기본 설치된 맑은 고딕을 우선 사용한다.
    """

    candidates = [
        "malgungothic",
        "malgun gothic",
        "nanumgothic",
        "nanum gothic",
        "noto sans cjk kr",
        "applegothic",
    ]

    for name in candidates:
        font_path = pygame.font.match_font(name)
        if font_path:
            return pygame.font.Font(font_path, size)

    # 한글 폰트를 찾지 못한 경우 기본 폰트로 실행은 계속한다.
    return pygame.font.Font(None, size)


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

    draw_text(surface, title_font, "무량공처 장인 연습", (54, 44))
    state_label = STATE_LABELS.get(controller.state, controller.state.name)
    draw_text(surface, body_font, f"현재 상태: {state_label}", (58, 135))
    draw_text(surface, body_font, controller.result_message, (58, 185))

    instructions = [
        "1. V 키를 눌러 영역전개 준비 상태로 들어갑니다",
        "2. 마우스 오른쪽 버튼을 누르고 유지합니다",
        "3. 오른쪽 버튼을 유지한 채 왼쪽 버튼을 클릭합니다",
        "4. 초록색 타이밍 구간에서 오른쪽 버튼을 놓습니다",
        "R: 초기화    ESC: 종료",
    ]

    for index, line in enumerate(instructions):
        draw_text(surface, small_font, line, (62, 280 + index * 38), (190, 197, 214))

    if controller.state == GameState.RELEASE_TIMING:
        draw_timing_bar(surface, controller)
        elapsed = controller.release_elapsed()
        draw_text(
            surface,
            small_font,
            f"해제 타이머: {elapsed:.2f}초",
            (190, 552),
        )

    if controller.state == GameState.FAILED:
        overlay = pygame.Surface((WIDTH, HEIGHT), pygame.SRCALPHA)
        overlay.fill((115, 20, 25, 55))
        surface.blit(overlay, (0, 0))


def main() -> int:
    pygame.init()
    pygame.display.set_caption("JJK 게임 - 무량공처 프로토타입")
    screen = pygame.display.set_mode((WIDTH, HEIGHT))
    clock = pygame.time.Clock()

    title_font = load_korean_font(54)
    body_font = load_korean_font(35)
    small_font = load_korean_font(27)

    controller = DomainController()
    keyboard_trigger = KeyboardDomainTrigger()
    mouse_seal_input = MouseSealInput()
    effect = MuryangEffect()
    was_domain_active = False

    running = True
    while running:
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                running = False
                continue

            if event.type == pygame.KEYDOWN and event.key == pygame.K_ESCAPE:
                running = False
                continue

            # 실제 입력 장치는 각 모듈이 처리하고, Controller에는 의미만 전달한다.
            keyboard_trigger.handle_event(event, controller)
            mouse_seal_input.handle_event(event, controller)

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
