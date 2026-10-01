# Purple Tempest Layers — local exploration

2026-10-01 KST. **CODEX VALIDATED in targeted scope / PENDING USER VISUAL REVIEW / NOT USER VERIFIED.** Default OFF, not a production promotion. This preserved candidate and the later TempestPolish are included in a separately authorized technical checkpoint on 2026-10-02.

## Change Contract

- Target gap: previous PressureStorm used pale purple floating strokes; it lacked white/gray lower wind and contrasting overlapping currents around the sphere.
- Allowed scope: separate outer companion with lower wind, bright white/violet discharges and dark violet/black-edged currents; Korean profile Inspector; bright Side/Caster and distant observer QA.
- Preserve: current sphere, every older candidate including PressureStorm, Production/Final2, size, trajectory, formation/fusion/travel timing, ConvergenceDiagnostic, camera ownership, gameplay and terminal.
- Expected visible change: lower-space swirling wind, multiple overlapping electrical currents with different contrast and rhythms, stronger distant visibility.
- Failure conditions: floating scraps, fixed orbit rings, persistent flat ribbons, missing bright-background contrast, protected behavior changes.
- Evidence: labeled normal-speed A/B videos, sampled sequences, source hashes, opt-in and lifecycle assertions, pre-body/terminal pixels and pose equality.

## Implementation and controls

Candidate: `PurpleTempestLayersProfile.asset`, saved `candidateEnabled: 0`.

- Eight broad, partial white/gray wind sweeps below the sphere. Their radii expand, heights vary, angular velocities differ, and the mask moves through the strokes. Travel gives the field a rearward bias.
- Twelve white-centered/violet discharges curve around varied tilted planes. Staggered events renew the shape, visibility and angular position rather than all firing radially from the core.
- Eight thicker violet currents have near-black borders and a different event rhythm. They cross the bright currents and parts of the projected body.
- Analytic sphere occlusion hides rear portions of the new layers without modifying the body shader. Line widths account for the inherited sphere scale.
- One owned dynamic wind mesh, three owned materials, twenty LineRenderers with reusable position arrays. No new camera, gameplay, hit-stop or exposure controls.
- While this candidate is ON, it takes precedence over PressureStorm and suppresses the original outer lightning. OFF preserves the previous branches. Existing halo, debris, distortion and body settings remain untouched.

## Files in this pass

Existing shared hook:

- `unity/Assets/Scripts/Player/ProductionPurpleOuterRuntime.cs`

New files, each with a new `.meta`:

- `unity/Assets/Scripts/Player/PurpleTempestLayersProfile.cs`
- `unity/Assets/Scripts/Player/PurpleTempestLayersRuntime.cs`
- `unity/Assets/Resources/VFX/PurpleTempestLayersProfile.asset`
- `unity/Assets/Resources/VFX/PurpleTempestWind.shader`
- `unity/Assets/Resources/VFX/PurpleTempestDischarge.shader`
- `unity/Assets/Editor/PurpleTempestLayersProfileEditor.cs`
- `unity/Assets/Editor/PurpleTempestLayersCapture.cs`

This report is new. Local helpers, raw frames and logs are under the gitignored `unity/Logs/PurpleTempestLayersQA/` folder.

## Final evidence

`D:\JJK_game\unity\Logs\PurpleTempestLayersQA\AB\20260930_171200_297`

- `Side_AB_Full.mp4`
- `Caster_AB_Full.mp4`
- `Observer_AB_Full.mp4`
- `Side_ReviewSheet.jpg`, `Caster_ReviewSheet.jpg`, `Observer_ReviewSheet.jpg`
- `Caster_MotionSequence.jpg`, `Observer_MotionSequence.jpg`
- `comparison_validation.json`

Videos: 1920x540, 60 fps, 385 frames (~6.417 seconds), normal playback speed. Burned-in labels: **LEFT BASELINE = Wrapped + PressureStorm; RIGHT CANDIDATE = Wrapped + TempestLayers.** Common FlowFirst/ConvergenceDiagnostic settings match.

Main QA uses the bright neutral ground/background and daytime lighting. Side/Caster retain the established camera setup. The fixed distant observer uses a corridor-centered framing; it was shifted from the previous observer framing to include the charge as well as passage, identically for A/B.

The open user scene was VFXLab, with Purple_UserPrototype present. CombatMVP was loaded additively only in Play Mode. Capture explicitly selects CombatMVP's main camera, avoiding the first preview's incorrect selection of VFXLab PreviewCamera. Existing VFXLab global Volume remained common to both A/B sides; it was not edited. No Scene asset was saved.

## Validation

- C# recompile: 0 errors / 0 warnings.
- Both shaders imported/supported: 0 compiler errors.
- Final console: 0 errors; 1 existing Input Manager deprecation warning. No input-system migration performed.
- Direct Editor Pipeline assertion harness: **7 check groups PASS / 0 FAIL in the final run** — asset/shader/Inspector check, three lifecycle repetitions, three cleanup checks. This is not a Unity Test Runner discovery/suite claim. Full regression not run.
- With Tempest OFF, the preserved PressureStorm companion still spawns. With both profiles enabled, only Tempest spawns. Charge uses Wrapped body; travel uses D2-R3 body. Terminal contains neither the new companion nor ProductionPurpleBodyRuntime.
- Each repetition samples 120 charge frames and travel with no growing material/mesh count. After real player-loop destruction, monitored materials/meshes return to **0 / 0**. Candidate roots also return to zero.
- All three views: frames 0–103 RGB identical; first difference frame 104 (1.733333s), when the new outer presentation becomes visible. All 385 pose/scale/active-state samples identical across A/B. Terminal 7.0s RGB max difference **0** in all three views.
- Start-snapshot check of **362 existing files**: only the authorized `ProductionPurpleOuterRuntime.cs` hook changed. Previous candidates, body shaders, Scene/ProjectSettings and unrelated dirty files retain their starting bytes.
- Final Editor: Edit Mode, `runInBackground=False`, candidate OFF; original VFXLab/Purple_UserPrototype present, temporary CombatMVP unloaded. Staged files: 0.

The first preview attempt stopped because CombatMVP GojoPlayer was absent from the currently open VFXLab scene. This was corrected by additive runtime-only QA loading, not by altering the user's Scene. The first camera-selection preview is superseded by the final folder above.

Logs: `targeted-final.log`, `console-final.json`, `console-entries-final.json`, `start_snapshot.json`, `outer_before.cs.txt`, `final-git-status.txt` in the QA directory.

Repository evidence copies: [targeted results](qa/purple-tempest/layers/targeted-final.json), [comparison results](qa/purple-tempest/layers/comparison-validation.json), [final console](qa/purple-tempest/layers/console-final.json), and [original direct assertion harness](qa/purple-tempest/layers/Validation.cs). Raw logs, videos and reference media remain local-only. Later improvements and user feedback are recorded in [TempestPolish review](PURPLE_TEMPEST_POLISH_REVIEW.md); that candidate is also not USER VERIFIED.

## Visual assessment and limits

**Technical: PASS in checked scope. Regression: PASS in checked scope. Reference similarity: MIXED. Art direction: MIXED, with stronger layer readability.**

- Caster 2.7/3.5s: the lower white wind is distinguishable from purple energy and occupies the character's surrounding space. It is a more direct response to the requested white/gray wind than PressureStorm's airborne purple fragments. Confidence high for this visible difference.
- Caster 3.433–3.600s: wind sweeps change position while white discharges and black-bordered currents replace and overlap one another. Layer motion is visibly present in adjacent frames. Counter-evidence: several curves still hold a recognizable arc shape for multiple frames.
- Observer 5.667s: dark borders and white wind survive the bright background more clearly than the former faint pressure strips. Confidence medium for greater readability; distant oppressive weight is not established merely by greater contrast.
- Some dark currents read as flat graphic strips or thick outlined lines. Their black border is deliberately strong, but they do not yet have the reference's fully integrated, constantly exposed/occluded energy texture.
- Lower wind can still read as clean circular streaks in some frames. It is not a complete hurricane cloud/air-pressure effect, and it does not physically displace the environment.
- The protected central flare/body remains recognizable. This pass has not replaced its structure or proven that all layers now read as one unified destructive phenomenon.

AI visual review used final stills and adjacent-frame sequences. Normal-speed videos are supplied for the user's temporal/weight judgment; frame inspection is not presented as equivalent to human real-time viewing. Deterministic capture is not a live performance benchmark.

Keep this as a comparison candidate. No automatic further polish after A/B or production promotion. The original implementation pass did not commit/push/merge; the later checkpoint has separate explicit commit/push authorization.
