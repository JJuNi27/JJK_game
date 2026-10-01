# Purple Pressure Storm — exploration review

2026-10-01 KST. LOCAL, default OFF. Technical checks passed; visual direction remains MIXED / PENDING USER VISUAL REVIEW. Not USER VERIFIED; not a production replacement.

## Change Contract

- Target gap: Wrapped reads as a sphere with nearby decorative lightning; strengthen surrounding wind-pressure motion and spatial reach.
- Allowed scope: new outer-pressure companion, pressure-boundary discharge, opt-in profile, Korean Inspector, bright neutral QA and distant observer.
- Must preserve: Wrapped body and all older candidates, Production/Final2, size, formation/trajectory/timing, ConvergenceDiagnostic, camera ownership, gameplay and terminal.
- Expected visible change: broad flows sweep around and away from the sphere; lightning roots move with those flows, rather than originate only at the center.
- Failure conditions: long luminous ribbons, floating scraps, pressure too faint at distance, merely more decoration, protected behavior changes.
- Required evidence: labeled Side/Caster/Observer A/B, protected phase/pose comparison, default-OFF/shader/lifecycle checks and protected-file hashes.

## Implementation

New candidate: `PurplePressureStorm`.

- Nine swept 3D pressure lanes use staggered expansion/shedding phases. Travel direction biases the flow rearward. One reusable dynamic mesh; no per-frame material/mesh creation.
- Eroded violet pressure strokes use bright and dark contrast against a bright background. First preview's long luminous spine was removed and the path length shortened; pressure strokes were widened.
- Eight main/branch discharge pairs are sampled from actual pressure-path positions and tangents. Short staggered events change branch shape and direction; tapered lines replace Wrapped's rectangular segment lightning **only while this candidate exists**.
- Existing body, halo, debris, distortion and convergence presentation are not retuned. There is no new physical force, scene-object displacement or global exposure/bloom change.
- Data controls are in `PurplePressureStormProfile.asset`, saved `candidateEnabled: 0`, with a scoped Korean CustomEditor.

## Changed files in this pass

Existing shared file, limited opt-in hook only:

- `unity/Assets/Scripts/Player/ProductionPurpleOuterRuntime.cs`

New files, each with its own `.meta`:

- `unity/Assets/Scripts/Player/PurplePressureStormProfile.cs`
- `unity/Assets/Scripts/Player/PurplePressureStormRuntime.cs`
- `unity/Assets/Resources/VFX/PurplePressureStormProfile.asset`
- `unity/Assets/Resources/VFX/PurplePressureStormFlow.shader`
- `unity/Assets/Resources/VFX/PurplePressureStormArc.shader`
- `unity/Assets/Editor/PurplePressureStormProfileEditor.cs`
- `unity/Assets/Editor/PurplePressureStormCapture.cs`

This report is new. Local-only helpers/evidence are under `unity/Logs/PurplePressureStormQA/`. Other pre-existing dirty/untracked work is preserved.

## Comparison evidence

Directory: `D:\JJK_game\unity\Logs\PurplePressureStormQA\AB\20260930_155355_450`

- `Side_AB_Full.mp4`
- `Caster_AB_Full.mp4`
- `Observer_AB_Full.mp4`
- `Side_ReviewSheet.jpg`, `Caster_ReviewSheet.jpg`, `Observer_ReviewSheet.jpg`
- `Caster_MotionSequence.jpg`, `Observer_MotionSequence.jpg`
- `comparison_validation.json`

All videos are 1920x540, 60 fps, 385 frames (~6.417s), normal speed. LEFT = BASELINE (Wrapped), RIGHT = CANDIDATE (Wrapped + PressureStorm), burned-in labels. Common FlowFirst/ConvergenceDiagnostic settings are identical. Side/Caster viewpoints retain the previous capture setup. The additional observer is stationary ~35m to the side of the travel corridor.

Bright neutral QA: warm-gray Lit ground, blue-gray background, daylight directional light and neutral ambient light. This is the default for this capture path and should remain the basis for future Purple comparisons. No Scene asset is saved. Temporary lighting/camera/background/actor/profile changes are restored after capture. The observer framing prioritizes passage; its early charge is near the left edge and is not a substitute for Side/Caster.

Captures sample the canonical sequence deterministically. They are visual comparison evidence, not live frame-rate/performance measurements. AI review used timestamped stills and adjacent-frame sequences; no claim of human-equivalent continuous real-time video perception is made. User should judge the original-speed MP4s.

## Technical and regression checks

- Unity C# recompile: 0 errors / 0 warnings.
- Two candidate shaders imported, supported, compiler messages: 0 errors.
- Final console groundTruth: 0 errors / 0 warnings. Event cursor retained one historical warning; no current console error.
- Pipeline `run_script` targeted assertion harness: 7 check groups PASS, 0 FAIL in final run (asset/shader/Inspector group; 3 lifecycle groups; 3 deferred-cleanup groups). This is direct NUnit assertions in the existing Editor, **not** a Unity Test Runner suite/discovery claim. No full regression.
- OFF creates no candidate companion. ON creates one. Charge remains Wrapped body shader; travel remains D2-R3 body shader. Terminal creates neither candidate companion nor production body runtime.
- Each lifecycle group samples 120 charge frames and travel, with no growing material/mesh count. Three repetitions return tracked Purple material/mesh resources to **0 / 0**; no candidate roots remain.
- Initial cleanup assertions failed because Editor background execution was stopped (`Time.frameCount` remained 103 despite elapsed wall time). Deferred Destroy calls had not run. Temporarily enabling runtime `Application.runInBackground` advanced frames, drained prior pending destruction, and returned all monitored resources to zero. Final tests then passed. Original `runInBackground=False` restored; no ProjectSettings edit. Earlier failed diagnostics retained, not hidden.
- All three views: first A/B RGB difference at frame 104 (1.733333s); frames 0–103 identical. Every logged body/ingredient pose, scale and active-state sample identical across A/B for all 385 frames. Terminal at 7.0s: RGB max difference **0** in all three views.
- Hash audit of 348 pre-existing files: only the permitted `ProductionPurpleOuterRuntime.cs` hook differs from the start snapshot. Body shaders, prior profiles/candidates, Scene/ProjectSettings and unrelated existing dirty files retain their starting contents.

Artifacts: `targeted-final.log`, `console-final.json`, `start_snapshot.json`, `start.diff`, `Validation.cs`, `Entry.cs` in `unity/Logs/PurplePressureStormQA/`. Initial failed cleanup investigations are in `targeted-results.log` and `playerloop-results.log`.

## Honest visual assessment

| Area | Finding | Evidence / confidence / counter-evidence |
|---|---|---|
| Technical | PASS in targeted scope | Final shader/lifecycle harness and output evidence. Full combat regression/performance not executed. |
| Regression | PASS in checked scope | Unchanged input hashes, OFF gate, pose equality, pre-body and terminal pixels. During active pressure, outer pixels intentionally differ. |
| Reference similarity | MIXED | Spatial reach is larger, but reference-like oppressive wind is not established. Medium confidence; pressure strokes remain visually separate in some frames. |
| Art direction | MIXED | Less rectangular local lightning and more front/back activity, but not yet convincing massive pressure. Medium confidence; distant presence is weaker than close-up improvement. |

At Caster 3.433–3.600s, pressure strokes move across the upper/front space and discharge locations change. At Side 2.7/3.5s, the field occupies substantially more surrounding space without changing the sphere. However, pale torn strokes can still resemble drifting strips; some broad arcs retain a decorative curve. At Observer 5.667s, the sphere is clear but the pressure field is faint. Lightning can extend far without transferring an equally strong sense of weight.

Therefore the user's target — “distant observers feel the surrounding space is being forcibly displaced” — is only partially met. Do not promote this candidate based on increased reach alone. Main remaining issues:

1. Pressure strokes need a stronger collective push/read; current motion can feel individually decorative.
2. Bright-background distant contrast is insufficient for the wind field, despite greater reach.
3. Large flow, sphere and boundary discharge are structurally related but do not consistently read as one heavy event.

A/B is ready for user review; no automatic further polish, promotion, commit or push. Production and prior candidates remain available unchanged.
