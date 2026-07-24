from __future__ import annotations

import pygame

from characters.base import CharacterProfile
from characters.registry import CHARACTER_SLOTS
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

GRID_COLUMNS = 2
CARD_WIDTH = 455
CARD_HEIGHT = 180
CARD_GAP_X = 35
CARD_GAP_Y = 22
GRID_START_X = 77
GRID_START_Y = 215


def character_card_rect(index: int) -> pygame.Rect:
    column = index % GRID_COLUMNS
    row = index // GRID_COLUMNS
    x = GRID_START_X + column * (CARD_WIDTH + CARD_GAP_X)
    y = GRID_START_Y + row * (CARD_HEIGHT + CARD_GAP_Y)
    return pygame.Rect(x, y, CARD_WIDTH, CARD_HEIGHT)


def draw_character_select_screen(
    surface: pygame.Surface,
    title_font: pygame.font.Font,
    body_font: pygame.font.Font,
    small_font: pygame.font.Font,
    card_font: pygame.font.Font,
) -> None:
    surface.fill((15, 18, 28))

    draw_text(surface, title_font, "캐릭터 선택", (54, 35))
    draw_text(
        surface,
        body_font,
        "연습할 캐릭터를 선택하세요",
        (58, 105),
        (196, 207, 228),
    )
    draw_text(
        surface,
        small_font,
        "각 캐릭터는 서로 다른 장인 입력과 영역 판정을 사용합니다.",
        (60, 160),
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

        content_x = rect.x + 20
        content_width = rect.width - 40

        draw_wrapped_text(
            surface,
            body_font,
            character.name,
            (content_x, rect.y + 12),
            content_width,
            name_color,
            line_gap=0,
            max_lines=1,
        )
        draw_wrapped_text(
            surface,
            card_font,
            f"생득술식: {character.technique}",
            (content_x, rect.y + 57),
            content_width,
            detail_color,
            line_gap=1,
            max_lines=1,
        )
        draw_wrapped_text(
            surface,
            card_font,
            f"영역전개: {character.domain}",
            (content_x, rect.y + 84),
            content_width,
            detail_color,
            line_gap=1,
            max_lines=1,
        )

        status_color = (153, 191, 227) if character.available else (132, 138, 157)
        draw_wrapped_text(
            surface,
            card_font,
            character.status,
            (content_x, rect.y + 111),
            content_width,
            status_color,
            line_gap=1,
            max_lines=1,
        )

        if character.available:
            key_label = str(index + 1)
            action_text = f"{key_label} / 클릭으로 선택"
            if index == 0:
                action_text = "1 / ENTER / 클릭으로 선택"
            action_color = (220, 229, 247)
        else:
            action_text = "설정 검토 후 해금"
            action_color = (105, 110, 126)

        draw_text(
            surface,
            card_font,
            action_text,
            (content_x, rect.bottom - 35),
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
