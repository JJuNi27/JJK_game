from __future__ import annotations

import unittest

from characters.yuta.controller import YutaDomainController
from game.state import GameState


class YutaGestureTests(unittest.TestCase):
    def test_sword_draw_activates_domain(self) -> None:
        controller = YutaDomainController()

        controller.request_domain()
        controller.begin_ring_connection()
        controller.begin_draw((420, 520), ring_is_connected=True)
        controller.update_draw((430, 310))
        controller.finish_draw(ring_is_connected=True)

        self.assertEqual(controller.state, GameState.DOMAIN_ACTIVE)
        self.assertEqual(controller.result_message, "영역전개 · 진안상애")

    def test_releasing_ring_connection_before_completion_fails(self) -> None:
        controller = YutaDomainController()

        controller.request_domain()
        controller.begin_ring_connection()
        controller.cancel_ring_connection()

        self.assertEqual(controller.state, GameState.FAILED)

    def test_short_upward_drag_fails(self) -> None:
        controller = YutaDomainController()

        controller.request_domain()
        controller.begin_ring_connection()
        controller.begin_draw((420, 520), ring_is_connected=True)
        controller.update_draw((425, 430))
        controller.finish_draw(ring_is_connected=True)

        self.assertEqual(controller.state, GameState.FAILED)

    def test_wide_horizontal_drift_does_not_count(self) -> None:
        controller = YutaDomainController()

        controller.request_domain()
        controller.begin_ring_connection()
        controller.begin_draw((420, 520), ring_is_connected=True)
        controller.update_draw((650, 270))
        controller.finish_draw(ring_is_connected=True)

        self.assertEqual(controller.state, GameState.FAILED)


if __name__ == "__main__":
    unittest.main()
