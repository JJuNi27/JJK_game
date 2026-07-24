from __future__ import annotations

import unittest

from characters.registry import CHARACTER_SLOTS, create_character_runtime


class CharacterRegistryTests(unittest.TestCase):
    def test_character_ids_are_unique(self) -> None:
        character_ids = [character.character_id for character in CHARACTER_SLOTS]
        self.assertEqual(len(character_ids), len(set(character_ids)))

    def test_available_characters_build_runtime(self) -> None:
        available = [character for character in CHARACTER_SLOTS if character.available]
        self.assertGreaterEqual(len(available), 1)

        for character in available:
            with self.subTest(character=character.character_id):
                runtime = create_character_runtime(character)
                self.assertIsNotNone(runtime.controller)
                self.assertIsNotNone(runtime.seal_input)
                self.assertIsNotNone(runtime.effect)

    def test_available_characters_use_character_local_modules(self) -> None:
        """구현 원본이 공용 폴더로 다시 새어 나가지 않도록 검사한다."""

        available = [character for character in CHARACTER_SLOTS if character.available]

        for character in available:
            with self.subTest(character=character.character_id):
                runtime = create_character_runtime(character)
                expected_prefix = f"characters.{character.character_id}"

                self.assertTrue(
                    type(runtime.controller).__module__.startswith(expected_prefix),
                    type(runtime.controller).__module__,
                )
                self.assertTrue(
                    type(runtime.seal_input).__module__.startswith(expected_prefix),
                    type(runtime.seal_input).__module__,
                )
                self.assertTrue(
                    type(runtime.effect).__module__.startswith(expected_prefix),
                    type(runtime.effect).__module__,
                )

    def test_locked_characters_do_not_build_runtime(self) -> None:
        locked = [character for character in CHARACTER_SLOTS if not character.available]

        for character in locked:
            with self.subTest(character=character.character_id):
                with self.assertRaises(ValueError):
                    create_character_runtime(character)


if __name__ == "__main__":
    unittest.main()
