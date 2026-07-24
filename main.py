from __future__ import annotations

import sys

import pygame

from game.app_scene import AppScene
from game.character import CharacterProfile
from game.domain_protocol import DomainControllerProtocol
from game.practice_stats import PracticeStats
from game.runtime import CharacterRuntime, create_character_runtime
from game.state import GameState
from game_input.keyboard_domain_trigger import KeyboardDomainTrigger
from game_input.voice_domain_trigger import VoiceDomainTrigger
from ui.character_select_screen import (
    draw_character_select_screen,
    select_character_from_event,
)
from ui.common import FPS, HEIGHT, WIDTH, load_korean_font
from ui.practice_screen import draw_practice_screen


def record_finished_attempt(
    previous_state: GameState,
    controller: DomainControllerProtocol,
    stats: PracticeStats,
) -> None:
    """성공 또는 실패 상태로 새로 전환된 순간에만 통계를 기록한다."""

    if controller.state == previous_state:
        return

    if controller.state == GameState.DOMAIN_ACTIVE:
        stats.record_success(controller.last_release_error)
    elif controller.state == GameState.FAILED:
        stats.record_failure(controller.last_release_error)


def enter_practice(
    character: CharacterProfile,
    stats: PracticeStats,
    voice_trigger: VoiceDomainTrigger,
) -> CharacterRuntime:
    runtime = create_character_runtime(character)
    runtime.controller.reset()
    runtime.seal_input.reset()
    stats.reset()
    voice_trigger.start()
    pygame.display.set_caption(
        f"JJK 게임 - {character.name} {character.domain} 프로토타입"
    )
    return runtime


def return_to_character_select(
    runtime: CharacterRuntime,
    stats: PracticeStats,
    voice_trigger: VoiceDomainTrigger,
) -> VoiceDomainTrigger:
    voice_trigger.stop()
    runtime.controller.reset()
    runtime.seal_input.reset()
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
    card_font = load_korean_font(19)

    scene = AppScene.CHARACTER_SELECT
    selected_character: CharacterProfile | None = None
    runtime: CharacterRuntime | None = None

    stats = PracticeStats()
    keyboard_trigger = KeyboardDomainTrigger()
    voice_trigger = VoiceDomainTrigger()

    previous_state = GameState.NORMAL
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
                        runtime = enter_practice(selected, stats, voice_trigger)
                        scene = AppScene.PRACTICE
                        previous_state = runtime.controller.state
                        was_domain_active = False
                    continue

                if runtime is None or selected_character is None:
                    continue

                if event.type == pygame.KEYDOWN and event.key == pygame.K_b:
                    voice_trigger = return_to_character_select(
                        runtime,
                        stats,
                        voice_trigger,
                    )
                    runtime = None
                    selected_character = None
                    scene = AppScene.CHARACTER_SELECT
                    previous_state = GameState.NORMAL
                    was_domain_active = False
                    continue

                if event.type == pygame.KEYDOWN and event.key == pygame.K_r:
                    runtime.seal_input.reset()

                should_reset_stats = keyboard_trigger.handle_event(
                    event,
                    runtime.controller,
                )
                if should_reset_stats:
                    stats.reset()

                runtime.seal_input.handle_event(event, runtime.controller)

            if scene == AppScene.PRACTICE and runtime is not None:
                voice_trigger.update(runtime.controller)
                runtime.controller.update()
                record_finished_attempt(previous_state, runtime.controller, stats)

                is_domain_active = runtime.controller.state == GameState.DOMAIN_ACTIVE
                if is_domain_active and not was_domain_active:
                    runtime.effect.restart()

                if is_domain_active:
                    runtime.effect.draw(screen, title_font)
                elif selected_character is not None:
                    draw_practice_screen(
                        screen,
                        selected_character,
                        runtime.controller,
                        stats,
                        voice_trigger.status_message,
                        title_font,
                        body_font,
                        small_font,
                    )

                previous_state = runtime.controller.state
                was_domain_active = is_domain_active
            else:
                draw_character_select_screen(
                    screen,
                    title_font,
                    body_font,
                    small_font,
                    card_font,
                )

            pygame.display.flip()
            clock.tick(FPS)
    finally:
        voice_trigger.stop()
        pygame.quit()

    return 0


if __name__ == "__main__":
    sys.exit(main())
