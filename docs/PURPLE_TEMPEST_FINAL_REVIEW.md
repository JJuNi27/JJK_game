# PurpleTempestFinal — separate final polish candidate

2026-10-02 (KST). **CODEX VALIDATED in targeted compile/shader/lifecycle scope / PENDING USER VISUAL REVIEW / NOT USER VERIFIED.** Overall technical clearance is incomplete because runtime HUD console errors remain. No production promotion; saved default OFF; no commit/push.

## Change Contract

- Target gap: sparse/outlined dark energy, clean white wind arcs, insufficient release transition and detached neon.
- Allowed scope: clone/refine the existing TempestPolish companion, profile and two shaders; add one opt-in outer hook and scoped capture/validation helpers.
- Must preserve: Production, Final2, all earlier candidates, sphere/body shaders, size, formation/contact/fusion/release clocks, movement, gameplay, camera ownership, terminal and USER VERIFIED settings.
- Expected visible change: broader internally mixed dark/emissive torn energy; charging air wraps lower space, compresses/reorients at release and becomes projectile-oriented pressure/shear/wake; neon crosses shared disturbances at different depths.
- Failure condition: dominant noodles/stroke, clean wind ribbons, carrying the charging cyclone in flight, generic central neon decoration, or any protected change.
- Required evidence: bright neutral original-speed labeled Side/Caster/Observer A/B; 385 pose/scale/active samples per view; terminal images; repeated resource checks and starting hashes.

## Implementation and ownership

Candidate `PurpleTempestFinalProfile.asset`, `candidateEnabled: 0`. It is independent of TempestPolish and takes priority only when explicitly enabled. OFF preserves the previous branch. No body material/profile, old candidate asset/shader, camera, timing or gameplay was edited.

- Reuses the same eight dark currents, eight charging air sheets, four travel air sheets and twelve white-violet LineRenderers; no new effect category or event-system architecture.
- Dark currents use shorter spans, wider irregular coverage and actual dropped faces. Drifting black/emissive fields mix inside each current rather than outlining its lateral boundaries. Currents and neon reference shared moving pressure sites. A sparse preview exposed hard flat fragments; boundaries/depth offsets were adjusted before full A/B. No visual tuning after full A/B.
- Charge air moves inward across lower space. Its saved launch matrix stays fixed. A release envelope compresses and redirects that anchored air forward before fading. The four travel sheets separately represent front compression, lateral shear and short rear pressure. Existing production wake is untouched.
- Neon palette and count remain unchanged. Independent cadence/length variation and transverse pressure-site paths replace a purely circumferential arrangement; outer emission/bloom/body settings are not raised to compensate for the bright environment.
- Three owned materials and two meshes, released by existing root lifecycle. Existing bright-neutral QA lighting/floor and Side/Caster framing are reused. Observer uses the prior fixed far-view setup. No Scene asset was saved.
- Tuning lives in the separate profile with a Korean scoped CustomEditor. Procedural path coefficients remain presentation debt (AGENTS category B); sample resolution/hash/clamp/index mechanics remain category C. No gameplay values were introduced.

## Changed files

Existing hook: `unity/Assets/Scripts/Player/ProductionPurpleOuterRuntime.cs`.

New files, each with its own `.meta`:

- `unity/Assets/Scripts/Player/PurpleTempestFinalProfile.cs`
- `unity/Assets/Scripts/Player/PurpleTempestFinalRuntime.cs`
- `unity/Assets/Resources/VFX/PurpleTempestFinalProfile.asset`
- `unity/Assets/Resources/VFX/PurpleTempestFinalWind.shader`
- `unity/Assets/Resources/VFX/PurpleTempestFinalDischarge.shader`
- `unity/Assets/Editor/PurpleTempestFinalProfileEditor.cs`
- `unity/Assets/Editor/PurpleTempestFinalCapture.cs`
- `unity/Assets/Editor/PurpleTempestFinalValidation.cs`

This report and ignored `unity/Logs/PurpleTempestFinalQA/` helpers/evidence are also new. Unity incidentally reordered two generated solution entries; verified pre-task bytes (including line endings) were put back. No original dirty file was overwritten. Protected-file hash audit permits only the explicit outer hook to change.

## Exact evidence

Final directory: `D:\JJK_game\unity\Logs\PurpleTempestFinalQA\AB\20261002_063321_437`

- `Side_AB_Full.mp4`
- `Caster_AB_Full.mp4`
- `Observer_AB_Full.mp4`
- Per-view `*_ReviewSheet.jpg`, `.poses.txt`, `.phase.csv`, protected terminal stills and `comparison_validation.json`.

All three MP4s: 1920x540, 60 fps, 385 frames (~6.417 seconds), original speed. Burned-in **LEFT BASELINE = TempestPolish / RIGHT CANDIDATE = TempestFinal**. Common Wrapped sphere, FlowFirst and ConvergenceDiagnostic remain the comparison setup; the opt-in body is not being redesigned.

The initial Side-baseline eval response timed out at the API's 5-second dispatch limit, but all 385 raw frames, pose/phase files and terminal still existed. It was not re-rendered. Subsequent captures used the existing `run_script` entry with a longer dispatcher budget. No exit-code-only validation claim.

## Technical correctness

- Unity 6000.3.20f1, existing Editor Pipeline; C# recompile completed with **0 errors**, both shaders imported/supported with **0 compiler errors**.
- Direct Editor Pipeline NUnit assertion harness: **7 executed check groups PASS / 0 FAIL** (Begin plus three lifecycle/cleanup pairs). This is not Unity Test Runner discovery or a full-suite result. Results: `unity/Logs/PurpleTempestFinalQA/targeted-final.json`.
- Checks include OFF baseline/ON single companion, same charge/travel body shader identity, 120 finite-vertex samples without resource growth, actual dark face gaps, fixed charge anchor, release/travel weights and terminal exclusion.
- Deferred destruction returned monitored materials/meshes to **0 / 0** in three repetitions.
- **Console FAIL / unresolved:** final runtime snapshot reported **94,750 cumulative console errors**. The latest 100 entries all had `NullReferenceException` from `ProductionCombatHudCanvas.RefreshTeam`, line 961 (`members[index].IsValid`), called from Update line 180. Compilation was not failed. This is a HUD call stack, not a shader diagnostic; its pre-existing/candidate causality was not established, so it is not dismissed as unrelated or counted as zero. HUD source was not changed. See `unity/Logs/PurpleTempestFinalQA/Console.json`.
- Editor returned to Edit Mode, original VFXLab scene, dirty=False, background=False; saved candidates restored OFF.

## Regression safety

- All **385 samples per view** have identical ingredient/body positions, scales and active states.
- Production body/earlier candidate/Scene/ProjectSettings/gameplay/camera files retain starting bytes; only the authorized outer opt-in hook changed among pre-existing files.
- Terminal at 7.0 s: **Side RGB max difference 0 (PASS)**. **Caster max 219 / Observer max 201 (INCONCLUSIVE)**. Caster comparison visibly contains a cyan actor-area outline only on baseline; minor other screen differences also remain. This is evidence of unmatched ancillary rendering, not proof that all differences have that cause. The terminal tests independently exclude the new companion/body runtime.
- Full capture differs from frame zero, before the new outer appears. Do not interpret full-image difference as candidate-only change. Frame-aligned pose/source/branch checks remain usable; entire-image regression clearance is **INCONCLUSIVE**.
- Historical whole-image comparison remains **INCONCLUSIVE**, not superseded. No whole-game damage regression/full suite was rerun. No frame-time benchmark.

## Visual review and limits

Reference: local `고죠 무라사키 날리는 장면.mp4`, including 6.0 s pressure/energy frame, and existing TempestPolish evidence. Source UI/actors obscure portions of the reference. This AI review uses time-aligned frames/adjacent-frame evidence; original-speed movies are supplied for human perception. It does not claim a human real-time viewing result.

**Visual similarity: MIXED. Art direction: MIXED.**

- Caster 2.7–3.5 s: lower-space wind is broader and less dominated by a razor-thin bright crest; dark coverage is heavier without a uniform black border. Confidence medium. Counter-evidence: broad white areas still look like layered sheets, and dark fragments can look flat.
- Caster 5.2–5.3 s: the launch-region air becomes a forward-slanted release burst; phase/anchor tests confirm charging wind vanishes rather than travelling with the sphere. Confidence high for phase separation, medium for pressure impression. Counter-evidence: the strongest white release sheet can still read as a diagonal ribbon, so this failure criterion is not fully cleared.
- Observer 3.5/5.67 s: dark/neon/wind overlap is visible at distance; the sphere remains identifiable. Confidence medium. Counter-evidence: extra coverage alone does not prove menace; the remote view still loses fine wind detail.
- Neon crosses shared sites/depths instead of only orbiting the centre. Counter-evidence: some frames show parallel zigzag lines as a separate bundle; complete layer unity is not achieved.

Remaining top three: white-sheet/ribbon wind moments, occasional planar dark fragments, conspicuous long neon bundles. Preserve this as a comparison candidate; do not promote on these results. No automatic additional polish. No VFX architecture escalation was needed; the HUD failure requires a separately scoped diagnostic rather than an unrelated fix in this pass.
