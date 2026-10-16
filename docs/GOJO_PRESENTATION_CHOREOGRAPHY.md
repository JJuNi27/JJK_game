# Gojo presentation changes — 2026-09-08

This pass extends the existing presenters. Combat damage, targeting, Blue pull, Red knockback, cooldowns, energy costs, movement speeds, keyboard bindings, Purple hit scheduling and domain stun duration are unchanged. Existing camera emphasis is now timed to the domain arrival. Domain presentation uses scaled time so it respects pause and VFXLab playback speed.

## What was already present

- Blue: a dedicated procedural energy shader, localized screen distortion, inward spirals, dust/debris suction, timed compression pulses and residual collapse.
- Red: a separate travelling projectile, outward fragments and expanding pressure fronts.
- Purple: one canonical Blue/Red merge and travelling orb, with gameplay hits already scheduled against its merge/travel timing.
- Unlimited Void: enclosure, floor suppression, stars, a dark focal sphere and a short uniform entrance fade.
- Blue/Red casts already had `TechniqueReleaseClock`, token validation and optional authored release bindings. The existing audio bridge already plays cast/impact/domain clips.

## Changed files and purpose

Paths below are relative to `unity/Assets/`.

| File | Purpose |
| --- | --- |
| `Scripts/Player/TechniqueChargeVisual.cs` | Adds compact Blue inward filaments and Red outward pressure filaments during anticipation, with a growing hot casting point and local light. Shared by combat and VFXLab. |
| `Scripts/Player/ProductionSignatureVfxDirector.cs` | Consumes existing begin/release/cancel cues; disposes anticipation on release, cancellation or disable. Supports an optional authored hand/finger socket. |
| `Scripts/Player/ProductionParticleVfxRuntime.cs` | Gives Red a white-hot center, turbulent energy surface, fast expanding shock front, local light and longer embers. Adds a small helper to reuse the existing parameterized energy shader with different palettes. Blue's active renderer is retained. |
| `Scripts/Player/PrototypeHollowPurplePresentationRuntime.cs` | Adds a hot core, turbulent violet surfaces, a short fusion pressure front and a 0.36 s visual residue period. Existing merge and travel durations remain intact. |
| `Scripts/Player/UnlimitedVoidPrototypeVisual.cs` | Replaces the uniform arrival with suppression, a view-aligned converging purple tunnel, black event horizon, three white ink bloom groups and delayed depth ambience. Exposes arrival beats and pause state. |
| `Resources/VFX/UnlimitedVoidInk.shader` | Procedural organic ink/mist lobes with soft boundaries, evolving noise and per-cluster bloom. No external texture dependency. |
| `Scripts/Core/PresentationBeatClock.cs` | Reusable ordered presentation beat clock. A frame hitch consumes every crossed beat exactly once; reset starts a new sequence. |
| `Scripts/Core/ProductionCombatFeedbackDirector.cs` | Separates domain activation suppression, black-void arrival impact and completion emphasis using existing feedback facilities. |
| `Scripts/Dev/VFXLab/VfxLabPreviewSequence.cs` | Uses the same Blue/Red anticipation and domain presenter; displays domain phases. Pause no longer disables/re-enables and restarts the domain component. |
| `Editor/PresentationBeatClockTests.cs` | Tests crossed-beat delivery, duplicate prevention, pause and reset. |

New source/shader files include Unity `.meta` files. This report is the only new documentation file. Existing local `VFXLab.unity` edits, local models/audio and hand-pose scripts were not edited by this pass. No commit or push was made.

## Default domain timing

Times are relative to successful domain activation, using the scaled presentation clock.

| Beat | Time | Presentation |
| --- | --- | --- |
| 0 | 0.00 s | Reality suppression starts |
| 1 | 0.22 s | Purple perspective tunnel starts |
| 2 | 0.94 s | Black circular void arrives; arrival camera cue |
| 3 | 1.28 s | First white ink cluster group |
| 4 | 1.52 s | Second cluster group |
| 5 | 1.76 s | Third cluster group |
| 6 | 2.18 s | Full ambience and completion camera cue |

The domain controller still owns the existing three-second gameplay lifetime. The arrival consumes part of that lifetime; this pass does not extend the stun or delay its start. The tunnel suggests transport through converging geometry and camera framing; it does not teleport targets or introduce a new screen-space domain distortion pass.

## Hookup and tuning

- Current keyboard flow needs no manual wiring. Runtime-created effects and the lab use these changes automatically.
- Tune domain stage durations on `UnlimitedVoidProductionVisual`. `ArrivalBeat` and the Inspector's `onArrivalBeat` (`UnityEvent<int>`) expose beats 0–6 for future voice, audio, camera or Timeline adapters. Combat automatically emits `UnlimitedVoid / Release` at beat 2 and `Culmination` at beat 6.
- For an authored hand/finger position, add/configure `ProductionSignatureVfxDirector` on the actor and assign `techniqueCastingPoint`. With no socket, `techniqueCastingOffset` supplies the body-relative fallback. The lab currently uses that fallback pose.
- Blue/Red retain the existing opt-in `acceptPresentationRelease` bindings. A future animation adapter must retain the `CastToken` from `Began` and pass that captured token to `RequestPresentationRelease`; the fallback timer still releases when no authored cue arrives.
- Purple keeps its existing scheduled merge/travel contract. Retiming Purple gameplay release to an animation event remains a separate integration task: update the hit schedule and visual timing together. Domain voice-driven gameplay activation likewise remains on its existing command-success path; the new beats control presentation after activation.
- Existing audio clips and their current cue bindings are retained. New individual ink-beat sounds, final finger-gun/hand-seal clips and voice synchronization are not assigned automatically.

## Fidelity and validation limits

The supplied attachment contained art direction but no Purple reference captures or new production animations. This pass uses existing meshes, particles and the existing energy shader plus procedural ink. The ink is layered transparent geometry with depth offsets, not volumetric fluid simulation. The domain has a fixed 72 tunnel streaks and 12 ink clusters; a target-device performance profile is still needed.

Verification artifacts are under `unity/Temp/GojoPresentationQA-output/`. They come from an isolated project copy with a simple capsule/stage, the same scripts, shaders and URP settings, and real Play Mode rendering. This does not substitute for a full CombatMVP keyboard playthrough, authored-character socket inspection, audio listening or a player-build test.

Verified results:

- Final runtime C# compilation with the project's Unity Roslyn compiler and existing assembly references: passed, no compiler diagnostics.
- Unity EditMode tests: **10 passed, 0 failed, 0 skipped**. This includes the existing locomotion/collision/release-token tests and both new beat-clock tests. See `editmode-results.xml` and `editmode-summary.txt`.
- Play Mode rendering: 16 captured frames covering the baseline stage, domain stages, Blue/Red charges and active visuals, Purple merge/release/travel/residue. The tunnel's initial angular bias and smooth Purple surface were corrected after inspecting captures.
- Domain callback order: `0,1,2,3,4,5,6`, followed by `0` after explicit reactivation. Pause held the clock; resume did not restart the sequence.
- Final harness: **0 runtime errors**, with **1 separately recorded Unity Editor Search indexing exception** (`UnityEditor.Search.SearchDatabase`). The exception comes from the isolated editor's startup indexer; it is not counted as a clean editor log. The VFX renderer and ink shader completed validation.
- Whitespace check passed for changed scripts. The pre-existing local VFXLab scene has whitespace changes outside this pass and was left intact.
