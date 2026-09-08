# JJK_game — Codex Project Guide

This file is a short project map, not a full specification.
Read only the subsystem docs relevant to the current task.

## Core workflow
- Inspect the actual working tree before editing.
- Prefer the smallest safe patch over broad rewrites.
- Preserve unrelated local changes.
- Do not reset, checkout, clean, or overwrite user-authored scene / Animator / FBX work.
- Do not commit, push, merge, or modify master unless the user explicitly approves it.
- Do not stop for non-blocking confirmations. Ask only when continuing would be destructive or a real design decision is required.
- Treat USER VERIFIED values as locked unless the user explicitly requests a change.
- During iteration, run targeted validation first. Run the full required regression suite once after the implementation is stable.
- Never describe subjective visual quality as USER VERIFIED. Only the user can visually approve Unity Play Mode results.

## Visual direction
This is a game, and strong readability / spectacle is a project goal.
Do not automatically under-design VFX because an effect seems "too flashy" in isolation.
In actual gameplay, effects often read smaller and weaker than expected.
When two safe choices are otherwise equivalent, prefer the version with stronger impact, aura, hit feel, depth, and readable motion — while still preserving character/gameplay readability.

## Project priorities
1. Visual impact / aura
2. Hit feel
3. Sound
4. Premium VFX
5. Character-specific source-material feel
6. Maintainability and Inspector/Data-driven tuning

## Protected Gojo baseline
Read: `docs/locked/USER_VERIFIED_SETTINGS.md`

## If touching Unlimited Void / Domain systems
Read: `docs/architecture/DOMAIN_SYSTEM.md`

## If touching cameras / cinematic ownership
Read: `docs/architecture/CAMERA_PRESENTATION.md`

## If touching Gojo Blue / Red / Purple presentation
Read: `docs/architecture/GOJO_VFX.md`

## Before running Unity validation
Read: `docs/workflows/UNITY_VALIDATION.md`

## Current active task
Read: `docs/active/CURRENT_TASK.md`

## Visual reference files
Reference images/videos used only to communicate visual intent live under:
`docs/references/`

Read `docs/references/README.md` before using them.

If CURRENT_TASK points to a reference image:
- inspect that image before implementing the related visual change;
- extract shape, composition, color, motion-language, depth, and mood;
- do not blindly copy unrelated characters or foreground subjects;
- do not move reference media into runtime Unity assets unless explicitly requested.

Reference media may be local-only and intentionally ignored by Git.

## Important asset / Git safety
Do not add or rewrite LocalModels, LocalAudio, imported FBX, textures, audio files, or `.blend` files unless the user explicitly requests it.
Do not discard local `VFXLab.unity` changes.
