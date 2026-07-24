from __future__ import annotations

import sys

import pygame

from effects.muryang_effect import MuryangEffect
from game.app_scene import AppScene
from game.character import CHARACTER_SLOTS, CharacterProfile
from game.domain_controller import DomainController
from game.practice_stats import PracticeStats
from game.state import GameState
from game_input.keyboard_domain_trigger import KeyboardDomainTrigger
from game_input.mouse_seal_input import MouseSealInput
from game_input.voice_domain_trigger import VoiceDomainTrigger


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
    """운영체제에 설치된 한글 폰트를 찾아 반환한다."""

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


def character_card_rect(index: int) -> pygame.Rect:
    card_width = 300
    gap = 35
    start_x = 65
    return pygame.Rect(start_x + index * (card_width + gap), 245, card_width, 285)


def draw_character_select_screen(
    surface: pygame.Surface,
    title_font: pygame.font.Font,
    body_font: pygame.font.Font,
    small_font: pygame.font.Font,
) -> None:
    surface.fill((15, 18, 28))

    draw_text(surface, title_font, "캐릭터 선택", (54, 42))
    draw_text(
        surface,
        body_font,
        "연습할 캐릭터를 선택하세요",
        (58, 120),
        (196, 207, 228),
    )
    draw_text(
        surface,
        small_font,
        "현재는 설정 검토가 끝난 고죠 사토루만 선택할 수 있습니다.",
        (60, 175),
        (145, 158, 184),
    )

    for index, character in enumerate(CHARACTER_SLOTS):
        rect = character_card_rect(index)
        if character.available:
            background = (31, 38, 58)
            border = (102, 138, 194)
            name_color = (235, 240, 252)
        else:
            background = (24, 27, 38)
            border = (55, 60, 76)
            name_color = (124, 130, 148)

        pygame.draw.rect(surface, background, rect, border_radius=18)
        pygame.draw.rect(surface, border, rect, width=2, border_radius=18)

        draw_text(
            surface,
            body_font,
            character.name,
            (rect.x + 24, rect.y + 28),
            name_color,
        )
        draw_text(
            surface,
            small_font,
            f"생득술식: {character.technique}",
            (rect.x + 24, rect.y + 92),
            (175, 187, 211) if character.available else (105, 110, 126),
        )
        draw_text(
            surface,
            small_font,
            f"영역전개: {character.domain}",
            (rect.x + 24, rect.y + 128),
            (175, 187, 211) if character.available else (105, 110, 126),
        )

        if character.available:
            draw_text(
                surface,
                small_font,
                '음성: "료이키 텐카이"',
                (rect.x + 24, rect.y + 164),
                (153, 191, 227),
            )
            draw_text(
                surface,
                small_font,
                "1 또는 ENTER / 클릭으로 선택",
                (rect.x + 24, rect.y + 225),
                (210, 221, 242),
            )
        else:
            draw_text(
                surface,
                small_font,
                "잠김",
                (rect.x + 24, rect.y + 225),
                (110, 115, 130),
            )

    draw_text(
        surface,
        small_font,
        "ESC: 종료",
        (60, 646),
        (145, 158, 184),
    )


def select_character_from_event(
    event: pygame.event.Event,
) -> CharacterProfile | None:
    if event.type == pygame.KEYDOWN and event.key in (
        pygame.K_1,
        pygame.K_RETURN,
        pygame.K_KP_ENTER,
    ):
        return next(
            (character for character in CHARACTER_SLOTS if character.available),
            None,
        )

    if event.type == pygame.MOUSEBUTTONDOWN and event.button == 1:
        for index, character in enumerate(CHARACTER_SLOTS):
            if character.available and character_card_rect(index).collidepoint(event.pos):
                return character

    return None


def draw_timing_bar(
    surface: pygame.Surface,
    controller: DomainController,
) -> None:
    """우클릭 해제 타이밍을 확인하는 연습용 UI."""

    x, y, width, height = 190, 525, 720, 34
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


def format_release_error(value: float | None) -> str:
    if value is None:
        return "측정 없음"
    return f"{value:.3f}초"


def draw_stats_panel(
    surface: pygame.Surface,
    stats: PracticeStats,
    body_font: pygame.font.Font,
    small_font: pygame.font.Font,
) -> None:
    """연습 세션의 성공률과 타이밍 기록을 표시한다."""

    panel_rect = pygame.Rect(750, 250, 300, 255)
    pygame.draw.rect(surface, (27, 31, 44), panel_rect, border_radius=16)
    pygame.draw.rect(surface, (55, 63, 82), panel_rect, width=2, border_radius=16)

    draw_text(surface, body_font, "연습 기록", (780, 270), (220, 228, 245))

    lines = [
        f"시도: {stats.attempts}회",
        f"성공 / 실패: {stats.successes} / {stats.failures}",
        f"성공률: {stats.success_rate:.1%}",
        f"현재 연속 성공: {stats.current_streak}회",
        f"최고 연속 성공: {stats.best_streak}회",
        f"최근 해제 오차: {format_release_error(stats.last_release_error)}",
        f"평균 해제 오차: {format_release_error(stats.average_release_error)}",
    ]

    for index, line in enumerate(lines):
        draw_text(
            surface,
            small_font,
            line,
            (778, 320 + index * 25),
            (184, 194, 215),
        )


def draw_practice_screen(
    surface: pygame.Surface,
    character: CharacterProfile,
    controller: DomainController,
    stats: PracticeStats,
    voice_status: str,
    title_font: pygame.font.Font,
    body_font: pygame.font.Font,
    small_font: pygame.font.Font,
) -> None:
    surface.fill((18, 21, 31))

    draw_text(
        surface,
        title_font,
        f"{character.name} · {character.domain} 장인 연습",
        (54, 44),
    )
    state_label = STATE_LABELS.get(controller.state, controller.state.name)
    draw_text(surface, body_font, f"현재 상태: {state_label}", (58, 135))
    draw_text(surface, body_font, controller.result_message, (58, 185))

    instructions = [
        f'1. V 키 또는 음성 "{character.voice_command}"로 영역을 준비합니다',
        "2. 마우스 오른쪽 버튼을 누르고 유지합니다",
        "3. 오른쪽 버튼을 유지한 채 왼쪽 버튼을 클릭합니다",
        "4. 초록색 타이밍 구간에서 오른쪽 버튼을 놓습니다",
        "R: 현재 시도 초기화    T: 연습 통계 초기화",
        "B: 캐릭터 선택    ESC: 종료",
    ]

    for index, line in enumerate(instructions):
        draw_text(surface, small_font, line, (62, 270 + index * 34), (190, 197, 214))

    draw_stats_panel(surface, stats, body_font, small_font)

    if controller.state == GameState.RELEASE_TIMING:
        draw_timing_bar(surface, controller)
        elapsed = controller.release_elapsed()
        draw_text(
            surface,
            small_font,
            f"해제 타이머: {elapsed:.2f}초",
            (190, 575),
        )

    draw_text(surface, small_font, voice_status, (58, 642), (145, 173, 205))

    if controller.state == GameState.FAILED:
        overlay = pygame.Surface((WIDTH, HEIGHT), pygame.SRCALPHA)
        overlay.fill((115, 20, 25, 55))
        surface.blit(overlay, (0, 0))


def record_finished_attempt(
    previous_state: GameState,
    controller: DomainController,
    stats: PracticeStats,
) -> None:
    """상태가 성공 또는 실패로 새로 전환된 순간에만 통계를 기록한다."""

    if controller.state == previous_state:
        return

    if controller.state == GameState.DOMAIN_ACTIVE:
        stats.record_success(controller.last_release_error)
    elif controller.state == GameState.FAILED:
        stats.record_failure(controller.last_release_error)


def enter_practice(
    character: CharacterProfile,
    controller: DomainController,
    stats: PracticeStats,
    voice_trigger: VoiceDomainTrigger,
) -> None:
    controller.reset()
    stats.reset()
    voice_trigger.start()
    pygame.display.set_caption(
        f"JJK 게임 - {character.name} {character.domain} 프로토타입"
    )


def return_to_character_select(
    controller: DomainController,
    stats: PracticeStats,
    voice_trigger: VoiceDomainTrigger,
) -> VoiceDomainTrigger:
    voice_trigger.stop()
    controller.reset()
    stats.reset()
    pygame.display.set_caption("JJK 게임 - 캐릭터 선택")
    return VoiceDomainTrigger()


def main() -> int:
    pygame.init()
    pygame.display.set_caption("JJK 게임 - 캐릭터 선택")
    screen = pygame.display.set_mode((WIDTH, HEIGHT))
    clock = pygame.time.Clock()

    title_font = load_korean_font(54)
    body_font = load_korean_font(35)
    small_font = load_korean_font(23)

    scene = AppScene.CHARACTER_SELECT
    selected_character: CharacterProfile | None = None

    controller = DomainController()
    stats = PracticeStats()
    keyboard_trigger = KeyboardDomainTrigger()
    mouse_seal_input = MouseSealInput()
    voice_trigger = VoiceDomainTrigger()
    effect = MuryangEffect()

    previous_state = controller.state
    was_domain_active = False

    try:
        running = True
        while running:
            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    running = False
                    continue

                if event.type == pygame.KEYDOWN and event.key == pygame.K_ESCAPE:
                    running = False
                    continue

                if scene == AppScene.CHARACTER_SELECT:
                    selected = select_character_from_event(event)
                    if selected is not None:
                        selected_character = selected
                        scene = AppScene.PRACTICE
                        enter_practice(
                            selected_character,
                            controller,
                            stats,
                            voice_trigger,
                        )
                        previous_state = controller.state
                        was_domain_active = False
                    continue

                if (
                    scene == AppScene.PRACTICE
                    and event.type == pygame.KEYDOWN
                    and event.key == pygame.K_b
                ):
                    scene = AppScene.CHARACTER_SELECT
                    selected_character = None
                    voice_trigger = return_to_character_select(
                        controller,
                        stats,
                        voice_trigger,
                    )
                    previous_state = controller.state
                    was_domain_active = False
                    continue

                should_reset_stats = keyboard_trigger.handle_event(event, controller)
                if should_reset_stats:
                    stats.reset()

                mouse_seal_input.handle_event(event, controller)

            if scene == AppScene.PRACTICE:
                voice_trigger.update(controller)
                controller.update()
                record_finished_attempt(previous_state, controller, stats)

                is_domain_active = controller.state == GameState.DOMAIN_ACTIVE
                if is_domain_active and not was_domain_active:
                    effect.restart()

                if is_domain_active:
                    effect.draw(screen, title_font)
                elif selected_character is not None:
                    draw_practice_screen(
                        screen,
                        selected_character,
                        controller,
                        stats,
                        voice_trigger.status_message,
                        title_font,
                        body_font,
                        small_font,
                    )

                previous_state = controller.state
                was_domain_active = is_domain_active
            else:
                draw_character_select_screen(
                    screen,
                    title_font,
                    body_font,
                    small_font,
                )

            pygame.display.flip()
            clock.tick(FPS)
    finally:
        voice_trigger.stop()
        pygame.quit()

    return 0


if __name__ == "__main__":
    sys.exit(main())
