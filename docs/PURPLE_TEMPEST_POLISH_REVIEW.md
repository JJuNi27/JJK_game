# Purple Tempest Polish — separate local candidate

2026-10-01 implementation; 2026-10-02 checkpoint preparation. **CODEX VALIDATED in targeted scope / PENDING USER VISUAL REVIEW / NOT USER VERIFIED.** Saved default OFF. No production promotion. Commit/push of this technical checkpoint was separately authorized on 2026-10-02.

## Change Contract

- Target gap: uniform black stroke/closed violet bands; the charging wind rides unchanged with the projectile; three layers remain visually separate.
- Allowed scope: separate Tempest polish companion/profile/shaders and QA, plus an opt-in hook in the existing outer sampler.
- Preserve: sphere/body shaders and profiles, previous Tempest, every older candidate, Production/Final2, size/path/formation/contact/fusion/release clocks, ConvergenceDiagnostic, gameplay/camera/terminal and unrelated local art.
- Expected visible change: black and emission invade the current rather than outlining it uniformly; charge wind dissipates at launch while travel uses directional shear; neon overlaps changing current/pressure locations.
- Failure conditions: persistent outlined noodle/closed strip, charging cyclone following the projectile, neon identity disappearing, body or timing changes.
- Evidence: same-condition labeled bright Side/Caster A/B, motion/release frame sequences, direct targeted assertions, resource return, pose/terminal pixels and starting file hashes.

## Implementation

Candidate: `PurpleTempestPolishProfile.asset`, `candidateEnabled: 0`. It takes priority over Tempest/PressureStorm only when explicitly enabled. OFF retains their prior branches. No existing profile or body shader was edited.

- Dark current: eight open partial currents, with two intersecting non-camera-facing surfaces each. Runtime removes actual face spans. The mask fades the ends of surviving spans. Moving local-space fields distribute near-black and violet emission independently of lateral borders. This is not the prior black stroke shader.
- Wind: eight widened charge sweeps include diffuse gray coverage and sharp crests, with irregular width/height. At release their matrix is anchored at the launch location and fades over the profile's 0.24 seconds. Four same-layer travel sweeps emerge along the projectile's direction; they do not reuse its charging orbit. No extra wake/particle effect type was added; existing production travel halo/wake remains common.
- Neon: retains twelve lines, white/violet palette, previous width/emission defaults, and event cadence. Three shared moving spatial disturbances influence dark emission/coverage, wind width/height/tint and neon path penetration/exposure. This affects these outer layers only, not the locked body material.
- Phase consumes existing `ageSinceRelease` and velocity, without changing a clock or adding camera/gameplay ownership.
- Ownership: three materials, one pressure mesh, one dark-current mesh, twelve neon LineRenderers. Final assertion counters include the `PurpleTempestPolish_` resource prefix. Performance was not profiled; additional mesh updates are a remaining cost.

## Changed files

Existing shared hook only:

- `unity/Assets/Scripts/Player/ProductionPurpleOuterRuntime.cs`

New files, each with a new `.meta`:

- `unity/Assets/Scripts/Player/PurpleTempestPolishProfile.cs`
- `unity/Assets/Scripts/Player/PurpleTempestPolishRuntime.cs`
- `unity/Assets/Resources/VFX/PurpleTempestPolishProfile.asset`
- `unity/Assets/Resources/VFX/PurpleTempestPolishWind.shader`
- `unity/Assets/Resources/VFX/PurpleTempestPolishDischarge.shader`
- `unity/Assets/Editor/PurpleTempestPolishProfileEditor.cs` — scoped Korean labels.
- `unity/Assets/Editor/PurpleTempestPolishCapture.cs`
- `unity/Assets/Editor/PurpleTempestPolishValidation.cs`

This report is new. Helpers/logs/raw renders are under ignored `unity/Logs/PurpleTempestPolishQA/`.

The checkpoint includes the existing Purple-only dependencies needed by this candidate and its A/B setup: Wrapped/Identity and PressureStorm, FlowFirst/earlier ingredient opt-ins, Convergence opt-ins, their assets/Inspector editors, and Purple-specific shared hooks. These are preserved exploration implementations, not newly approved production visuals. Standalone Blue/Red changes, Scene/ProjectSettings/package changes and unrelated local work are excluded. No new handoff document is part of this checkpoint.

## Evidence

Final directory:

`D:\JJK_game\unity\Logs\PurpleTempestPolishQA\AB\20261001_133007_863`

- `Side_AB_Full.mp4`
- `Caster_AB_Full.mp4`
- `Side_ReviewSheet.jpg`, `Caster_ReviewSheet.jpg`
- `Side_MotionSequence.jpg`, `Caster_MotionSequence.jpg` — 3.433–3.600 seconds.
- `Side_ReleaseSequence.jpg`, `Caster_ReleaseSequence.jpg` — 5.100–5.350 seconds.
- `comparison_validation.json`, `format.json` and per-view `.phase.csv`/`.poses.txt`.

Small machine-readable evidence is also preserved in the repository: [targeted results](qa/purple-tempest/polish/targeted-final.json) and [comparison results](qa/purple-tempest/polish/comparison-validation.json). The MP4s, raw frames, references and full logs remain local-only; a GitHub checkout alone does not provide these videos.

Videos are 1920x540, 60 fps, 385 frames (~6.417 seconds), original speed. Burned-in labels: **LEFT BASELINE = Wrapped + TempestLayers; RIGHT CANDIDATE = Wrapped + TempestPolish.** FlowFirst/ConvergenceDiagnostic and the bright neutral ground, daytime light, background and cameras match within A/B. Earlier failed/preview folders are not final evidence.

## Targeted validation

- C# compile: 0 errors. Two shaders imported/supported: 0 shader compiler errors. Final console ground truth: 0 errors / 0 warnings after the corrected implementation; initial NaN errors are not counted as a successful preview.
- Direct Editor Pipeline NUnit assertion harness: **7 check groups PASS / 0 FAIL** (asset/Inspector check, three lifecycle cycles, three deferred cleanup checks). This is not a Unity Test Runner discovery/suite claim. Machine-readable output: `unity/Logs/PurpleTempestPolishQA/targeted-final.json`.
- OFF uses previous Tempest. ON spawns one polish companion and excludes duplicate previous companions. Charge/travel body shader identity preserved; terminal has neither the new companion nor the production body runtime.
- Each lifecycle samples 120 charge frames without monitored material/mesh growth; verifies finite mesh vertices, real dark face gaps, fixed launch anchor, charge/release/travel transitions. Deferred destruction returns monitored material/mesh counts to **0 / 0**, three times.
- Both views: all 385 body/ingredient pose, scale and active-state samples match A/B. First RGB difference is frame 104 (1.733333 seconds). Frames 0–103 match. Terminal at 7.0 seconds RGB maximum difference **0**.
- Phase logs: Release at sampled wall 5.133333 seconds, Travel by 5.383333 seconds. Anchor remains at (512, ..., 515.3) during flight; charge wind weight becomes zero and travel weight 0.48.
- Starting snapshot: **978 existing files checked; only the authorized shared outer hook changed**. Body, prior Tempest/profile/shaders/capture, Final2/other candidates, Scene/ProjectSettings and unrelated changes retain their starting bytes.
- Historical full-image equality is **INCONCLUSIVE**. Previous-session images differ from frame zero; a separate current original-harness sparse preview also differs from full capture (a cyan actor-area outline appears in the full capture). These comparisons are recorded in `comparison_validation.json`, not counted as regression PASS. Within the final A/B the outline is common. Starting-file hashes and focused branch/body/pose/terminal checks remain valid; full visual equality to an older session is not claimed.
- No full regression or real gameplay damage test was rerun. No claim of whole-project regression clearance.
- Final Editor: Edit Mode; `runInBackground=False`, timeScale 1; saved candidate OFF. QA CombatMVP was runtime-only and no Scene asset was saved.

## Visual review

Technical: **PASS in checked scope**. Regression: **PASS in checked branch/resource/pose/terminal/file scope**, historical pixel comparison **INCONCLUSIVE**. Reference similarity: **MIXED**. Art direction: **MIXED**.

### User feedback — 2026-10-02

The following is the user's supplied assessment, distinct from the AI frame-based assessment above:

- Overall direction: **CLOSER**.
- Sphere: **KEEP**.
- Neon: **KEEP + POLISH**.
- Separating Charge and Travel wind is a successful change.
- Wind visual quality remains insufficient.
- Dark cursed energy has less noodle-like contour, but its weight has become too weak.
- This is feedback for further refinement, **not promotion or USER VERIFIED**. Final acceptance remains pending; all candidates stay default OFF.

- Caster 3.433–3.600s: the thick uniformly black-outlined closed strips are gone. Dark coverage and violet exposure change between neighbouring frames. Confidence high for stroke reduction. Counter-evidence: smaller dark/violet pieces can still read as floating scraps; the heavy dark layer is substantially less dominant than baseline.
- Side/Caster 5.100–5.350s: wind remains at the launch region while the sphere departs, then fades instead of carrying the charging swirl down the corridor. Confidence high for this phase change, supported by fixed-anchor/weight logs. Counter-evidence: the directional travel wind is faint at distance and is not proven to add threatening pressure.
- Charge at 2.7/3.5s: gray coverage is broader, neon survives, and current disturbances can overlap the body edge. Confidence medium for increased spatial interaction. Counter-evidence: white wind still has clean ribbon/crest shapes in several frames; neon can remain identifiable as separate long lines. Shared fields have not proven complete storm unity.
- Trade-off: removing the dominant dark contour makes the image less cartoon-like but also lighter and less imposing. This candidate is not declared reference-matched or superior in every view.
- AI review used synchronized stills/adjacent-frame sequences. Original-speed videos are supplied, but frame inspection is not equivalent to human real-time judgment. No live frame-time benchmark.

## Failures corrected and remaining decisions

An initial fractional power at the final dark-mesh node generated NaN vertices and invalid bounds. The endpoint sine is now clamped before the power; finite-vertex checks cover it. The first missing-dark previews are superseded. The initial QA resource prefix omitted the new resources; final checks were rerun with the corrected prefix. Initial MCP calls after domain reload occasionally failed transport/scene availability; retrying the existing runtime-only scene load resolved this without VFX workarounds.

No structural/reference conflict requiring **ASTRA ESCALATION** arose. However, a future larger change to turn the remaining pieces into volumetric pressure or a different current representation requires a fresh contract; it was not added after A/B. No automatic extra polish. User visual review remains pending.
