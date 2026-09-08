# Unity Validation Workflow

Goal: preserve quality without repeatedly burning Codex usage on full validation during every tiny edit.

## Before editing
Run:
- `git status -sb`
- `git diff --stat`

Identify existing local changes before touching files.

## During iteration
Use the cheapest targeted validation that can disprove the current change.
Examples:
- C# compile / static check
- focused Editor test
- one affected ability preview
- one cleanup/repeat-cast check

Do **not** repeatedly run the entire Unity suite after every small visual tweak.

## After implementation is stable
Run the full required regression suite once.

Latest known successful checkpoint before the next active task:
- Unity: **17/17 passed**, 0 failed, 0 skipped
- Python: **13 passed**
- CombatMVP validation completed
- VFXLab preview completed

These are historical checkpoint results, not proof that later edits are safe.

## Active-task validation priorities
For the current next pass, use targeted checks first for:
- post-Domain basic melee recovery
- Red projectile speed/range and cleanup
- Purple travel range / hit behavior / cleanup
- Domain exit restoration
- visual reference-driven Domain changes

Only after those are stable, run the full regression suite once.

## Visual validation labels
Never use USER VERIFIED unless the user personally checked it.

Use:
- CODEX VALIDATED
- AUTOMATED TEST PASSED
- CODEX PREVIEW
- PENDING USER VISUAL REVIEW

## Unity license issue history
Unity Personal was active, but one automated shell initially failed with `No valid Unity Editor license found`.
After the Hub-launched Editor refreshed authentication, automated Unity execution succeeded.

If this happens again:
- do not delete / return / reactivate the license automatically
- preserve the exact failure log
- verify Hub / Editor authentication and retry

## Editor-open safety
If the real Unity Editor is already open, do not interfere with unsaved work.
Use a safe test copy / separate validation path when appropriate.

## Final report should include
- changed files
- targeted validation
- full test counts/results
- any failures and exact reason
- what remains PENDING USER VISUAL REVIEW
- no commit / push unless explicitly approved
