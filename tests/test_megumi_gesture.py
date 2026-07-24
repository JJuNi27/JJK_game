from __future__ import annotations

import unittest

from characters.megumi.controller import MegumiDomainController
from game.state import GameState


class MegumiGestureTests(unittest.TestCase):
    def test_shadow_gesture_activates_domain(self) -> None:
        controller = MegumiDomainController()

        controller.request_domain()
        controller.begin_left_hand()
        controller.begin_drag((300, 180), left_hand_is_held=True)
        controller.update_drag((310, 320))
        controller.update_drag((500, 330))
        controller.finish_drag(left_hand_is_held=True)

        self.assertEqual(controller.state, GameState.DOMAIN_ACTIVE)
        self.assertEqual(controller.result_message, "영역전개 · 감합암예정")

    def test_releasing_q_before_completion_fails(self) -> None:
        controller = MegumiDomainController()

        controller.request_domain()
        controller.begin_left_hand()
        controller.cancel_left_hand()

        self.assertEqual(controller.state, GameState.FAILED)

    def test_short_downward_drag_fails(self) -> None:
        controller = MegumiDomainController()

        controller.request_domain()
        controller.begin_left_hand()
        controller.begin_drag((300, 180), left_hand_is_held=True)
        controller.update_drag((305, 240))
        controller.finish_drag(left_hand_is_held=True)

        self.assertEqual(controller.state, GameState.FAILED)


if __name__ == "__main__":
    unittest.main()
