# Gojo third polish and Domain recovery

2026-09-09. Implemented the approved P0–P6 active task from the existing working tree. No commit, push, merge, scene save, asset replacement, or architecture rewrite. **CODEX VALIDATED; PENDING USER VISUAL REVIEW.**

**Implemented items**

| Item | Result |
|---|---|
| P0 — Physical combat recovery | Reproduced a presentation-end path leaving `GojoDomainController.State` at `Active`, which keeps the basic-attack gate closed. Presentation completion now notifies the controller, and controller disable resets its Domain command. Burnout begins in that terminal transaction, including disable paths, and continues to block techniques. BasicAttack exposes the same gated attack operation used by mouse input for testing actual physical damage. No health, cooldown, combo, target-lock, or unrelated combat reset was added. |
| P1 — Blue | Disabled the optional small pre-cast decoration in the shared settings, with an early return that avoids creating its objects. Rubble now uses split vertices for flat fractured faces, dark brown-gray color, near-zero smoothness, no metallic response, and much weaker neutral emission. Default count 24 → 32, overall scale 1.4 → 1.2, fewer giant chunks, more varied small pieces, faster inward trajectories and accelerating radial collapse. Core and existing four-hit field gameplay retained. |
| P2 — Red | Shared combat/lab defaults: speed 22 → **42 m/s**, range 11 → **26 m**. The final swept collision step is clamped to the remaining range so a long frame cannot extend the shot. Flash intensity .30 → .38, shock-ring scale 1.3 → 1.65. Aftermath becomes torn elongated gray-white pressure sheets: faster outward motion, less damping, early .025 s onset, and a filament mask replacing round puffs. Existing .95 s aftermath lifetime, charge ownership, release cleanup and repulsion damage semantics retained. |
| P3 — Purple | Range 18 → **48 m**, travel .78 → **1.6 s**. Combat presentation reads the controller's range/duration; lab uses the same defaults. Completed hold stays **.32 s**, visual scale **1.65**, gameplay radius **3.2 m**. Added a cast-local hold offset `(2.5, 1.3, 1.2)` metres; the last part of fusion translates toward it, and release returns smoothly to the original corridor within .18 s. Fusion sources and their relative paths remain. Release pressure front expands farther, lightning increases 12 → 24 with paired branches, and a localized distortion shell forms an elongated traveling wake. Scar sampling now covers the longer path within a bounded 96-sample limit, with varied tapered edges. |
| P4 — Unlimited Void interior | Two procedural blue nebula shells add continuous 360° cloud depth without a UV seam or horizon. Larger black gaps retain the abyss base. White Blood density 54 → **32**, scale 1.8 → **1.4**, opacity/brightness reduced; near/mid/far and above/below spherical placement retained. Stars now replenish during the full residence rather than expiring after the arrival sequence. Visible floor stays hidden; gameplay collision and the isolated interior are unchanged. |
| P5 — Cosmic Eye | Focal scale 2.2 → **3.5**, distance .72 → .62 of interior radius. Broader irregular accretion, counter-flowing mist and fine iris threads combine pale blue, white, violet and subtle spectral color. Pupil stays black, with eye sorting above depth stars. This is a procedural shader interpretation of the reference, not an imported image. |
| P6 — Domain release | The final **1.15 s** presents an irregular center-outward reveal. One environment-only interior snapshot overlays the restored world; actor renderers are omitted from that snapshot and their previous visibility is restored immediately. Participants restore before the opening reveals the map. The existing camera lease holds the return view until handoff. Release cannot re-enter the interior on a stale tick. Snapshot/material/overlay/barrier cleanup is owned by the existing session. Cancellation, disable, death and unload retain immediate safe restoration. No barrier HP/destruction or second gameplay scene was added. |

**References inspected before Domain art changes**

All three local files were inspected: `interior_anime_ref_01.png`, `interior_concept_ref_01.png`, and `cosmic_eye_ref_01.png`. The concept needed a temporary JPEG decoding copy under the ignored QA folder; the source PNG is unchanged. The anime's dark gaps and blue cloud structure guided the atmosphere, the exaggerated concept informed layered depth, and the eye reference informed black pupil scale and the broad spectral halo. Foreground characters and the concept's bright floor/horizon were not implementation targets. Reference media was not copied into Unity Assets.

**Validation**

| Check | Result |
|---|---|
| P0 reproducer before the fix | **Failed as expected:** presentation ended, but controller state remained Active. `p0-before.xml` records the failure. |
| Same P0 reproducer after the fix | **1/1 passed.** |
| Final focused recovery / Red range / Domain release tests | **4/4 passed**, including physical damage after five terminal paths, burnout preservation, a long-frame Red range bound, release interruption and snapshot cleanup. |
| Targeted scene iteration | CombatMVP and normal completion/death/unload passed. An initial lab run found stale-tick re-entry after release. After the one-way release fix, the combined focused/lab run passed **5/5**. A separate initial fixture timing assertion was corrected to derive the actual session lifetime instead of relying on a historical duration. |
| Final full Unity regression — run once | **21 passed, 0 failed, 0 skipped**, Unity 6000.3.20f1. Includes all existing locomotion/beat-clock tests, Domain participant/camera safety, CombatMVP and repeated Q/E/R/V lab previews, plus four new tests. |
| Existing Python regression — run once | **13 passed:** 11 unittest cases in the repository virtual environment and 2 Blender-retarget pytest cases using the existing system pytest installation. No dependency installation. |
| Compilation / runtime logs | C# compilation succeeded during targeted validation. No C# errors, shader errors, NullReferenceException or MissingReferenceException in final logs. |
| Preservation | All **49** checked protected files match their starting hashes. All **291** Assets files in the isolated test copy match the working tree. The original purple tunnel builder and trajectory blocks match exactly. Source/resource/editor whitespace check passed. |

Full results: [Unity XML](../unity/Logs/ThirdPolishQA/final-full.xml), [Unity log](../unity/Logs/ThirdPolishQA/final-full.log), [validation summary](../unity/Logs/ThirdPolishQA/validation-summary.json), [Python unittest](../unity/Logs/ThirdPolishQA/python-unittest.log), [Python pytest](../unity/Logs/ThirdPolishQA/python-pytest.log).

Validation used the existing isolated project at `unity/Logs/PolishQA/Project`; no user scene was saved. Walk 3 / Run 14 / Walk Time Scale 1.15, Animator, evade, FBXs, LocalModels, LocalAudio, the pre-existing VFXLab scene edits, and unrelated local source/doc changes were preserved. Capture radius, visual barrier sizing, same-scene isolated interior, per-participant return storage and camera ownership architecture remain separate and intact.

**Files changed in this pass**

These are relative to the starting working tree, including files that were already untracked. They do not claim ownership of the earlier uncommitted pass.

| Directory | Existing files edited |
|---|---|
| `unity/Assets/Scripts/Core/` | `TechniqueBurnoutController.cs` |
| `unity/Assets/Scripts/Player/` | `BasicAttack.cs`, `BlueTornDebris.cs`, `DomainPresentationSession.cs`, `DomainPresentationSettings.cs`, `GojoDomainController.cs`, `GojoPolishSettings.cs`, `GojoTechniqueChainController.cs`, `ProductionParticleVfxRuntime.cs`, `PrototypeHollowPurplePresentationRuntime.cs`, `PurpleTravelAftermath.cs`, `RedTechniqueProjectile.cs`, `TechniqueChargeVisual.cs`, `UnlimitedVoidPrototypeVisual.cs` |
| `unity/Assets/Resources/VFX/` | `GojoPolishSettings.asset`, `RepulsiveVapor.shader`, `UnlimitedVoidInk.shader`, `UnlimitedVoidIris.shader` |
| `unity/Assets/Editor/` | `DomainPresentationRuntimeTests.cs` |

New files: `unity/Assets/Editor/GojoThirdPolishTests.cs`, `unity/Assets/Resources/VFX/UnlimitedVoidNebula.shader`, `unity/Assets/Resources/VFX/UnlimitedVoidRelease.shader`, and their three `.meta` files; this report. Total: 19 existing files edited and 7 files added. Generated QA snapshots, logs and captures remain ignored under `unity/Logs/ThirdPolishQA`.

**PENDING USER VISUAL REVIEW**

The [34-image capture index](../unity/Logs/ThirdPolishQA/CODEX_PREVIEW/index.md) includes CombatMVP and VFXLab Q/E/R/V, eight horizontal Domain directions, zenith/nadir, and three release stages. Selected views: [Blue gameplay](../unity/Logs/ThirdPolishQA/CODEX_PREVIEW/Combat_Blue_gameplay_camera.png), [Red aftermath](../unity/Logs/ThirdPolishQA/CODEX_PREVIEW/Combat_Red_aftermath.png), [Purple hold](../unity/Logs/ThirdPolishQA/CODEX_PREVIEW/Purple_complete_hold.png), [Domain interior](../unity/Logs/ThirdPolishQA/CODEX_PREVIEW/Domain_0.png), [release midpoint](../unity/Logs/ThirdPolishQA/CODEX_PREVIEW/Domain_release_middle.png).

User Play Mode review is still needed for debris/core balance, Red speed/pressure feel, Purple hold framing and release force, lightning/wake/scar appearance, nebula/White Blood balance, eye scale/color/motion, and the continuous exit/camera handoff. No subjective quality is labeled USER VERIFIED.

Previews invoke the actual scene/controller methods rather than manually pressing keys. CombatMVP retains its existing prototype avatar and capsule enemies; VFXLab retains the imported Gojo model. Captures demonstrate selected rendered phases, not final audio, input feel, performance across hardware, or continuous motion quality. The release uses a frozen environment snapshot for 1.15 s while the live original world is revealed; it is not a live render of two simultaneous gameplay spaces. Purple retains the existing scheduled capsule-hit architecture, so targets are selected at cast time and resolved at the travel delay; this pass does not replace it with continuous projectile collision.
