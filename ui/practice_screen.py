from __future__ import annotations

import pygame

from game.character import CharacterProfile
from game.domain_protocol import DomainControllerProtocol
from game.practice_stats import PracticeStats
from game.state import GameState
from ui.common import HEIGHT, WIDTH, draw_text


STATE_LABELS = {
    GameState.NORMAL: "대기",
    GameState.DOMAIN_READY: "영역 준비",
    GameState.WAIT_LEFT_CLICK: "장인 결합 대기",
    GameState.RELEASE_TIMING: "해제 타이밍",
    GameState.DOMAIN_ACTIVE: "영역 발동",
    GameState.FAILED: "실패",
}


def draw_timing_bar(
    surface: pygame.Surface,
    controller: DomainControllerProtocol,
) -> None:
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


def build_practice_instructions(character: CharacterProfile) -> list[str]:
    instructions = [
        f'1. V 또는 음성 "{character.voice_command}"로 영역을 준비합니다'
    ]
    instructions.extend(
        f"{index}. {step}" for index, step in enumerate(character.seal_steps, start=2)
    )
    instructions.extend(
        [
            "R: 현재 시도 초기화    T: 연습 통계 초기화",
            "B: 캐릭터 선택    ESC: 종료",
        ]
    )
    return instructions


def draw_practice_screen(
    surface: pygame.Surface,
    character: CharacterProfile,
    controller: DomainControllerProtocol,
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

    for index, line in enumerate(build_practice_instructions(character)):
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
