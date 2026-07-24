from __future__ import annotations

import pygame


WIDTH = 1100
HEIGHT = 700
FPS = 60


def load_korean_font(size: int) -> pygame.font.Font:
    """운영체제에 설치된 한글 폰트를 우선순위대로 찾는다."""

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


def wrap_text(
    font: pygame.font.Font,
    text: str,
    max_width: int,
    max_lines: int | None = None,
) -> list[str]:
    """문자 단위로 줄바꿈해 한글 문장을 지정 폭 안에 넣는다."""

    if not text:
        return []

    lines: list[str] = []
    current = ""

    for character in text:
        candidate = current + character
        if current and font.size(candidate)[0] > max_width:
            lines.append(current.rstrip())
            current = character.lstrip()
            if max_lines is not None and len(lines) >= max_lines:
                break
        else:
            current = candidate

    if current and (max_lines is None or len(lines) < max_lines):
        lines.append(current.rstrip())

    if max_lines is not None and len(lines) == max_lines:
        consumed = "".join(lines)
        if len(consumed.replace(" ", "")) < len(text.replace(" ", "")):
            while lines[-1] and font.size(lines[-1] + "…")[0] > max_width:
                lines[-1] = lines[-1][:-1]
            lines[-1] = lines[-1].rstrip() + "…"

    return lines


def draw_wrapped_text(
    surface: pygame.Surface,
    font: pygame.font.Font,
    text: str,
    position: tuple[int, int],
    max_width: int,
    color: tuple[int, int, int] = (235, 238, 245),
    line_gap: int = 5,
    max_lines: int | None = None,
) -> int:
    """줄바꿈한 문장을 그리고 사용한 세로 높이를 반환한다."""

    x, y = position
    line_height = font.get_linesize() + line_gap
    lines = wrap_text(font, text, max_width, max_lines=max_lines)

    for index, line in enumerate(lines):
        draw_text(surface, font, line, (x, y + index * line_height), color)

    return len(lines) * line_height
