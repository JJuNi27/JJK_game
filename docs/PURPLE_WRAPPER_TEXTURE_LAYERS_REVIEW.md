# PurpleWrapperTextureLayers — source principles exploration

2026-10-06. LOCAL / CODEX VALIDATED in scoped compile, shader and lifecycle checks / PENDING USER VISUAL REVIEW / NOT USER VERIFIED.

Saved default OFF. No Production integration, staging, commit or push. This is a projectile-wrapper experiment, not a caster-pressure refinement.

## Source fact versus project transfer

Input is the user's supplied **Gemini verified analysis** of [the Roblox production video](https://youtu.be/7T9gmPp6vD0). The attachment is `C:/Users/p4com/.codex/attachments/a156ae94-fdf6-4f55-9d56-0da9751e7ff0/붙여넣은 텍스트.txt`.

YouTube direct open failed (cache miss). The source video/UI was not independently verified by Codex. CONFIRMED/LIKELY below are **Gemini's supplied source labels**, not Codex's independent certification. No asset was extracted/copied from Roblox. The local anime reference remains the game's final form authority.

- Source fact supplied: a projectile, with stable core plus Dark/Wind/Lightning/Trail; different brush and curve textures; multi-axis orientation/depth; contrast hierarchy. This does NOT establish caster-foot wind placement.
- Source hypothesis supplied: short repeated replacement helps obscure complete silhouettes. Gemini labels replacement LIKELY in one section; stronger wording elsewhere does not remove that uncertainty.
- Source timing limitation: the attachment alternates between <0.05s and 0.05–0.1s for lightning. Exact lifetimes are not treated as measured truths. Candidate cycles are tunable implementation choices.
- Project transfer: test compact alpha-shape families and staggered depth/packet envelopes in the existing wrapper companion. This is not a Roblox reconstruction or a new claim about caster pressure.

## A / B / C comparison before implementation

Baseline: `PurpleTempestFinalCleanup`, actual runtime/two shaders/profile and its Side/Caster/Observer evidence. Current report: `docs/PURPLE_TEMPEST_FINAL_CLEANUP_REVIEW.md`.

| Principle | A. Already Present | B. Missing / Weak | C. Not Appropriate |
|---|---|---|---|
| Different shape/texture families | Wind and dark already use different procedural noise fields; neon is narrow. | Broad surface masks still reveal similar strip boundaries. A clear thick short brush / slender curve contrast was weak. | Do not mandate Roblox emitters or texture assets as the only solution. |
| Small/medium overlap | 12 wind lanes, 8 dark currents with 2 crossings, 12 lines, not one global surface. | Several lanes still expose long complete faces; independence in code is not proof of visual layering. | Count increases alone. |
| XYZ orientation | Dark/neon already use tilted 3D paths. | Wind is heavily lower/horizontal, with a few large sheets. | Random orientation without a shared field. |
| Depth | Near/far paths and sphere-ray hiding exist. | Some planar side views and shared strip shapes remain recognizable. | Deform the sphere to simulate depth. |
| Core/Dark/Wind/Lightning hierarchy | Strong white center, black-violet contrast and sharp neon. | Core/body bloom can conceal weak wrapper masks; some samples flatten toward magenta. | Replace the protected sphere or brighten every layer equally. |
| Conceal/reveal | 40 finite cover proxies, 12 hot sites; nearer wind hides dark/neon, dark hides neon. | Wind did not reciprocally hide behind the dark proxies. Proxy coverage is approximate, not actual per-pixel depth. | Global ZWrite or renderer architecture replacement. |
| Broken alpha | Advected erosion and localized face removal already exist. | Partial card/rectangular faces persist. | Hard cutout everywhere; smooth bloom shoulders are still useful. |
| Local replacement | Existing staggered dark/neon event cycles and moving wind masks. | Long wind faces can remain trackable despite moving internal noise. | Adopt an exact source lifetime without evidence. |
| Contrast | Already present. | Perceived wind role can become weaker at game distance. | Additional Orb/Cross Flare (unconfirmed in this source), literal very long Roblox trail, jelly/macro deformation. |

Current Cleanup Caster samples at 2.7/3.5s show broad white floor/side strips; Side 5.2–5.3s still shows long diagonal white spans. Those are the actual project gap, not simply a different engine feature list.

## Change Contract

- Target gap: trackable full wrapper faces and weak contrast between compact brush and curved sweep; long cable exposure.
- Allowed scope: separate wrapper runtime/profile/two shaders, Korean scoped Inspector, isolated QA capture/validation; small OFF-preserving priority hook in `ProductionPurpleOuterRuntime`.
- Must preserve: Production/Final2/TempestPolish/Final/Cleanup and all older candidates; sphere shader/size/identity, trajectory/formation/contact/fusion/release timing, gameplay, camera ownership, terminal, caster-pressure candidates and USER VERIFIED data.
- Expected visible change: shorter independently replaced masks around the stable sphere at different depths; rapid sharp discharge, reciprocal partial reveal/hiding; core remains brightest.
- Failure: sheet/card/noodle persistence, lost wind role/continuous occupation, flat uniform brightness, sphere deformation or any protected change.
- Required evidence: neutral-bright original-speed Side/Caster/Observer A/B, adjacent/temporal frames, executed assertion checks, cleanup and starting-file hashes.

## Implementation

Candidate name: **PurpleWrapperTextureLayers**. Baseline remains Cleanup; selecting the new opt-in companion replaces only the previous wrapper companion at construction. OFF follows the exact prior branch chain.

- Same budgets: 12 wind lanes, 8 dark currents with two shallow curved crossings, 12 LineRenderers; three owned materials and two owned meshes. No particle-count increase, new camera pass or global Bloom/Volume change.
- Wind masks have two **procedural alpha families**: short thick tapered/frayed brush and slender curved sweep. They are analytically shaded masks, **not imported artist-painted textures or a Particle System texture workflow**. This is an initial test of the source principle, not an identical implementation.
- Wind geometry is compact cupped sections instead of long 64-segment ribbons; each lane changes orientation/depth at a masked packet reset, evolves locally during its envelope, and has an independent cycle. 24 segments per lane, rather than 64; 4,900 total owned mesh vertices rather than 12,740. No GPU benchmark was run; shader cover-array cost remains.
- Shell masks occupy near/core and outer radii (profile shellReach 1.65) with XYZ variation. The broad neutral lower-flow wrapper is replaced by compact magenta/violet shell layers in this candidate. **This reduces the visible broad pressure footprint**, rather than preserving all prior hurricane coverage. Caster-foot candidates themselves are untouched.
- Eight lanes remain body-relative through travel; four launch-site sections pin/fade during release. Existing phase clock/charge anchor/0.24s release/travel weights are reused; no sequence timing changes.
- Dark retains black-violet contrast and curved local erosion, adding a blade-shaped asymmetric coverage envelope. No constant black stroke. Residual card views remain possible.
- Neon keeps white/violet palette and emission 7, but uses narrower width (0.045 versus Cleanup 0.095), shorter angular spans and independent fast packet resets (profile cycle 0.105s with per-lane variation). This is a real reduction in continuous line exposure, not a claim of unchanged perceived scale.
- Wind samples existing nearer dark cover patches; dark/neon sample nearer wind; hot-site coupling remains. No global depth-write change. Finite proxy masks do not perfectly match the frayed alpha silhouettes; over-concealment is a limitation.

## Technical and regression evidence

Unity 6000.3.20f1, project `D:/JJK_game/unity`, Editor Pipeline CLI. Editor was initially closed; first recompile command returned no Pipeline instance. Installed matching Editor was launched; import succeeded. No VFX workaround for runner/connection failure.

- C# import/compilationFailed=false, errors 0. Both shaders supported, ShaderUtil errors 0.
- **7 actual assertion check groups PASS / 0 FAIL**: Begin plus three lifecycle/cleanup pairs. Explicit Pipeline NUnit assertions, not Unity Test Runner discovery or seven distinct test methods. Machine result: `unity/Logs/PurpleWrapperTextureLayersQA/targeted-final.json`.
- Checks: saved OFF; scoped Korean Inspector; OFF retains Cleanup, ON creates exactly one new companion; protected charge/travel body shader identity; 120 finite samples per cycle without resource growth; populated dark segments/40 cover proxies/12 neon renderers; release anchor and phase transitions; terminal excludes new companion.
- Three repeated deferred cleanups return material/mesh baseline 0/0. Final inventory new roots/materials/meshes 0/0/0.
- Side/Caster/Observer: 385 samples each, Blue/Red/Purple position/scale/active state identical; contact JSON and diagnostic pulse CSV identical.
- Terminal t=7.0: Side RGB max diff 0 / 0 changed pixels (focused PASS); Caster max 163 / 6,877 changed pixels (INCONCLUSIVE); Observer max 129 / 313 pixels (INCONCLUSIVE). All three images differ before wrapper activation in 120 frames due to ancillary rendering; full-image/pixel regression is still INCONCLUSIVE. Do not attribute all differences to this candidate.
- Final console session: 0 errors / 4 existing warnings, groundTruth consoleErrors=0, compilationFailed=false. No `ProductionCombatHudCanvas.RefreshTeam` NRE observed in this session; no claim that the known issue is fixed.
- SHA256: 1,109 preexisting files preserved; only authorized `ProductionPurpleOuterRuntime.cs` changed. No previous candidate/profile/body/Scene/ProjectSettings or unrelated dirty file changed.
- Play Mode ended in original VFXLab, scene dirty=False; background flag restored=False. Scene asset not saved. Editor left open after this task's launch.
- Full gameplay/damage suite, exact alpha-proxy matching and GPU performance NOT RUN. Core material/texture values are untouched; shader identity tests are not an independent pixel proof of body internals.

## Visual review — honest outcome

**Visual similarity: MIXED. Art direction: MIXED.** Technical PASS is not visual acceptance.

AI inspected time-ordered/adjacent source frames and 0.1s temporal samples. Original-speed 60fps movies are supplied; this is not a claim of human-equivalent continuous playback perception. Similarity against the new source remains conditional on Gemini's supplied breakdown because direct video access failed.

| Finding | Evidence | Confidence / counter-evidence |
|---|---|---|
| Long white sheets reduced | Side/Caster 2.683–2.717, 3.483–3.517s; new shell patches do not expose the old broad white face. | High for these frames; much of the change comes from replacing that pressure coverage, so fewer sheets does not prove better wind. |
| Cable persistence reduced | Same adjacent frames: large common neon contours change into thinner short sharp events. | Medium/high; some angular/transverse events remain fully readable, and fixed jagged line shape can still look graphic in a still. |
| Short replacement improved | Caster temporal samples 2.5–3.9s and Side three-frame sequences show new event positions and different masks. | Medium; some bright details are too fast/small at distance. Real-time snappiness versus flicker needs user review. |
| Wind/body occupation weaker | Caster and Observer 2.7/3.5s: broad gray pressure footprint is gone, and sphere/discharge dominate. | High. This is a regression in wind role and storm continuity, even though it fits a more compact projectile wrapper. |
| Texture families/depth partially read | Side close frames show near/far partial curves and purple masks overlapping dark. | Medium/low across views: body/halo bloom hides many masks, so two families are not consistently identifiable. XYZ in code is not a visual success proof. |
| Dark planar forms remain | Side 3.5 and release 5.167s show black-violet planar/spiky views. | High for existence. Several large panel-like areas occupy the same locations in baseline, suggesting shared locked body/halo contribution; no isolation run proves which whole patch belongs to which layer. Do not blame all on new dark or fix body outside scope. |
| Hierarchy retained incompletely | White core and darker purple remain distinct in charge; thinner neon is not globally brighter. | Medium; common travel body is very magenta/blown-out at 5.67s, and wind becomes too weak to hold its own role. |
| Far-view threat | Observer preview shows visible sphere and intermittent lightning. | INCONCLUSIVE for successful depth/pressure: foreground columns obscure portions, and compact wind detail is weak. |

This experiment demonstrates a cleaner short-discharge tendency; it does **not** prove a convincing integrated layered storm or paper/card elimination. Preserve as a comparison candidate, do not promote. No further visual tuning after final A/B.

## Outputs

`D:/JJK_game/unity/Logs/PurpleWrapperTextureLayersQA/AB/20261006_111132_175/`

- `Side_AB_Full.mp4`
- `Caster_AB_Full.mp4`
- `Observer_AB_Full.mp4`
- Each view: `*_ReviewSheet.jpg`, `*_Charge_Travel_Adjacent.jpg`, `*_Release_Adjacent.jpg`, `*_TemporalEvidence.jpg`
- `comparison_validation.json`, pose/contact/pulse/phase logs, protected terminal stills.

MP4s verified with ffprobe: 1920×540, 60fps, 385 frames / 6.417s, continuous source 0–6.4s. LEFT **BASELINE = PurpleTempestFinalCleanup**, RIGHT **CANDIDATE = PurpleWrapperTextureLayers**, labels burned in. Common Wrapped body, FlowFirst ingredients and ConvergenceDiagnostic match; bright-neutral background and seed match. No new caster-pressure profile enabled.

QA root: `unity/Logs/PurpleWrapperTextureLayersQA/` includes original failed connection log, Editor session log, shader/compiler state, assertion results, resource/console/final state, start hashes and final git status. Prior QA directories preserved.

## Changed files

Existing: `unity/Assets/Scripts/Player/ProductionPurpleOuterRuntime.cs` — optional companion priority/sampling/lightning exclusivity hook only; prior dirty edits retained.

New, each with `.meta`:

- `unity/Assets/Scripts/Player/PurpleWrapperTextureLayersRuntime.cs`
- `unity/Assets/Scripts/Player/PurpleWrapperTextureLayersProfile.cs`
- `unity/Assets/Resources/VFX/PurpleWrapperTextureLayersProfile.asset`
- `unity/Assets/Resources/VFX/PurpleWrapperTextureLayersWind.shader`
- `unity/Assets/Resources/VFX/PurpleWrapperTextureLayersDischarge.shader`
- `unity/Assets/Editor/PurpleWrapperTextureLayersProfileEditor.cs`
- `unity/Assets/Editor/PurpleWrapperTextureLayersCapture.cs`
- `unity/Assets/Editor/PurpleWrapperTextureLayersValidation.cs`

This report and local ignored QA helpers/results. Staged files 0. No commit/push/merge, no Production or USER VERIFIED promotion. No ASTRA ESCALATION: changes remained inside the authorized local wrapper representation scope.
