from __future__ import annotations

import pygame

from game.character import CHARACTER_SLOTS, CharacterProfile
from ui.common import draw_text, draw_wrapped_text


NUMBER_KEYS = (
    pygame.K_1,
    pygame.K_2,
    pygame.K_3,
    pygame.K_4,
    pygame.K_5,
    pygame.K_6,
    pygame.K_7,
    pygame.K_8,
    pygame.K_9,
)


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
    card_font: pygame.font.Font,
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
        "고죠: 한 손 · 스쿠나: 양손 · 메구미: 그림자 궤적 장인",
        (60, 175),
        (145, 158, 184),
    )

    mouse_position = pygame.mouse.get_pos()

    for index, character in enumerate(CHARACTER_SLOTS):
        rect = character_card_rect(index)
        hovered = rect.collidepoint(mouse_position)

        if character.available:
            background = (35, 43, 65) if hovered else (31, 38, 58)
            border = (126, 164, 222) if hovered else (102, 138, 194)
            name_color = (235, 240, 252)
            detail_color = (175, 187, 211)
        else:
            background = (26, 29, 41) if hovered else (24, 27, 38)
            border = (69, 74, 91) if hovered else (55, 60, 76)
            name_color = (145, 151, 170)
            detail_color = (112, 118, 136)

        pygame.draw.rect(surface, background, rect, border_radius=18)
        pygame.draw.rect(surface, border, rect, width=2, border_radius=18)

        content_x = rect.x + 22
        content_width = rect.width - 44

        draw_wrapped_text(
            surface,
            body_font,
            character.name,
            (content_x, rect.y + 25),
            content_width,
            name_color,
            line_gap=2,
            max_lines=2,
        )
        draw_wrapped_text(
            surface,
            card_font,
            f"생득술식: {character.technique}",
            (content_x, rect.y + 94),
            content_width,
            detail_color,
            max_lines=2,
        )
        draw_wrapped_text(
            surface,
            card_font,
            f"영역전개: {character.domain}",
            (content_x, rect.y + 145),
            content_width,
            detail_color,
            max_lines=2,
        )

        status_color = (153, 191, 227) if character.available else (132, 138, 157)
        draw_wrapped_text(
            surface,
            card_font,
            character.status,
            (content_x, rect.y + 198),
            content_width,
            status_color,
            max_lines=2,
        )

        if character.available:
            key_label = str(index + 1)
            action_text = f"{key_label} / 클릭으로 선택"
            if index == 0:
                action_text = "1 / ENTER / 클릭으로 선택"
            action_color = (220, 229, 247)
        else:
            action_text = "아직 선택 불가"
            action_color = (105, 110, 126)

        draw_text(
            surface,
            card_font,
            action_text,
            (content_x, rect.bottom - 42),
            action_color,
        )

    draw_text(surface, small_font, "ESC: 종료", (60, 646), (145, 158, 184))


def select_character_from_event(event: pygame.event.Event) -> CharacterProfile | None:
    if event.type == pygame.KEYDOWN:
        if event.key in (pygame.K_RETURN, pygame.K_KP_ENTER):
            return next(
                (character for character in CHARACTER_SLOTS if character.available),
                None,
            )

        if event.key in NUMBER_KEYS:
            index = NUMBER_KEYS.index(event.key)
            if index < len(CHARACTER_SLOTS):
                character = CHARACTER_SLOTS[index]
                return character if character.available else None

    if event.type == pygame.MOUSEBUTTONDOWN and event.button == 1:
        for index, character in enumerate(CHARACTER_SLOTS):
            if character.available and character_card_rect(index).collidepoint(event.pos):
                return character

    return None
