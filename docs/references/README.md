# Visual Reference Workspace

This folder is for **local visual references used to communicate art direction to Codex**.

Examples:
- anime screenshots
- game screenshots
- user-created concept images
- generated concept art
- short reference clips

## Important
These files are references, not runtime Unity assets.
Do not copy them into `Assets/` or build dependencies around them unless explicitly requested.

Most binary reference media in this folder is intentionally ignored by Git.
That means:
- the folder structure / instructions stay in GitHub;
- the user can drop reference images into the local workspace after `git pull`;
- Codex can inspect the local file directly;
- the project repository does not need to accumulate every temporary screenshot.

If a reference must be shared through Git later, explicitly decide that first.

## Naming
Recommended:
`<subject>_<purpose>_ref_<nn>.<ext>`

Examples:
- `cosmic_eye_ref_01.png`
- `interior_anime_ref_01.png`
- `red_impact_ref_01.mp4`
- `gojo_melee_motion_ref_02.mp4`

## Workflow
1. User gives visual feedback.
2. ChatGPT converts approved feedback into `docs/active/CURRENT_TASK.md`.
3. If visual communication is difficult, CURRENT_TASK names one or more reference paths.
4. User pulls docs and drops the local reference media into the named folder.
5. Codex inspects those references before implementing.
6. The reference is used for visual language, not blind copying of unrelated foreground subjects.
