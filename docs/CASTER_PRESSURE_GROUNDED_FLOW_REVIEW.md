# CasterPressureGroundedFlow — review

Status: CODEX VALIDATED (scoped technical checks only) / PENDING USER VISUAL REVIEW / NOT USER VERIFIED.
Visual goal is not fully met. Candidate remains opt-in, saved default OFF; no Production promotion.

## Change Contract

- Target: connect pressure to caster feet/lower body; replace trackable full contours with moving local sections; separate broad-flow and crest cadence.
- Scope: new GroundedFlow runtime/profile/shader, scoped Korean Inspector, QA capture/validation, minimal optional canonical-sequence hook.
- Preserve: previous CasterPressure/3DFlow, PairPolish/Final2, Blue/Red and Purple bodies, formation/contact/fusion/release timing, trajectory, gameplay and camera ownership.
- Failure: long white paper/ribbon, distant upper-body-only wind, or pressure becoming too weak to read from Caster.
- Evidence: bright-neutral Side/Caster, original-speed labeled A/B, adjacent frames, scoped assertions, resource cleanup and protection hashes.

## Changes

- Uses CharacterController feet (center minus half-height), with a profile fallback for preview actors. The QA anchor is now (512, 0, 512), rather than caster center y=1.
- Feet-relative guide passes around front/side/back and concentrates below waist. Existing guide is not rotated as a complete object.
- Broad, inner-compression and sharp-crest sections traverse only a part of the guide. Their geometry attachment and visible span change between packets, beyond shader flicker.
- Packet cycle settings: broad 0.62s, inner 0.37s, crest 0.14s. These are implementation choices, NOT measured reference lifetimes.
- Shader advection differs across those roles; local coverage moves across curved sections. Ground clearance uses smooth compression rather than a hard flat clamp.
- Wind density builds toward fusion and settles during hold without changing the existing sequence timeline.
- Release remains a 0.32s pinned-origin forward sweep/fade, not a charge vortex attached to the projectile.
- First preview had over-masked crest visibility; corrected the new candidate's local crest span/coverage before the final A/B. No additional tuning after final A/B.

## Actual visual review

AI reviewed time-ordered frames and adjacent-frame evidence. Original-speed MP4s are provided for user review; AI does not claim human-equivalent continuous perception.

| Area | Finding | Confidence / counter-evidence |
|---|---|---|
| Reference placement | CLOSER: candidate pressure is near feet/lower legs, instead of crossing the two upper orbs. Visible at 0.35, 0.65, 0.95, 1.10, 1.35s. | High for location; source camera and actor proportions differ. |
| Contour persistence | Local sections replace the previous complete upper S-shaped contour. 0.933–1.017s shows local footprint/crest change while broad near-foot presence remains. | Medium: screenshots cannot establish the full subjective original-speed impression; some short curved spans are still recognizable. |
| Paper/ribbon | Long exposed white sheet impression is reduced. | Medium: much of that reduction comes from lower placement and smaller visible coverage; it is not proof of convincing volumetric air. |
| Scale / pressure | WEAKER in Caster: thin low gray/white patches blend into the bright floor. Side reads lower pressure better, but neither reaches reference hurricane density/occupation. | High for sampled frames; bright isolated crest moments can read better between sampled frames. |
| Overall | Visual similarity MIXED; art-direction quality MIXED. Placement and contour behavior improve, but spatial presence is insufficient. Not production-ready. | No automatic visual PASS. |

Primary reference: `_local_refs/videos/고죠 무라사키 날리는 장면.mp4`, particularly ingredient pressure around 1.20–1.73s and 2.867–3.233s. Reference has a connected near-foot density AND larger surrounding wisps; moving all visible pressure low does not reproduce that complete relationship. Reference camera movement, exposure and duplicated frames limit precise speed estimates.

Remaining limits:

1. Caster pressure is still too faint/small against neutral ground; broad occupation was lost while shortening contours.
2. Some sections still read as soft gray arcs or haze, not forcibly moving compressed air. No claim that paper/ribbon has been conclusively solved.
3. Fast local replacement can feel like patch replacement; it needs user original-speed review to distinguish velocity from flicker. Existing Purple-wrapper wind is unchanged and remains visible later in the sequence.

## Technical / regression evidence

- Final Unity recompile completed: no C# errors. New shader supported, ShaderUtil error count 0.
- Explicit NUnit-assertion harness, NOT Unity Test Runner discovery: `targeted.xml` = 9 PASS / 0 FAIL; `play-canonical.xml` = 1 PASS / 0 FAIL. Actual assertion methods executed.
- PairPolish/FlowFirst direction and repeated resource assertions passed. Canonical construction performed in Play Mode.
- 385 samples per view: Blue/Red/Purple position, scale and active state identical; contact JSON and diagnostic flash/hold/pulse CSV identical.
- Side protected terminal t=7.0 RGB max difference 0: PASS for that sample only.
- Caster terminal t=7.0 max RGB difference 161, 737 changed pixels: INCONCLUSIVE. The mismatch is not attributed to the candidate without further evidence. No full-sequence pixel-regression PASS claimed.
- Post-render owned PurpleIngredient material/mesh counts 0/0, no candidate roots; repeated cancellation passed.
- Pipeline console history retains 9 old errors from previous EditMode canonical QA/timeout. No new logged runtime errors during this pass; final groundTruth consoleErrors=0. History was not cleared. Initial pre-import Environment entry failed type resolution; preserved separately as StateBefore.json, resolved after project import.
- Original VFXLab returned, Play Mode OFF, scene dirty=False, background flag restored=False. No scene asset saved.
- SHA256 check of preexisting working-tree files: only `PrototypeHollowPurplePresentationRuntime.cs` changed in this pass (new optional hook). Previous candidates, protected/unrelated dirty files preserved. Index unchanged; no staging/commit/push.

## Outputs

All comparison media: `unity/Logs/CasterPressureGroundedFlowQA/AB/20261005_191817_457/`

- `Side_AB_Full.mp4`
- `Caster_AB_Full.mp4`
- `Side_Charge_Adjacent.jpg`, `Caster_Charge_Adjacent.jpg`
- `Side_Local_Contour_Adjacent.jpg`, `Caster_Local_Contour_Adjacent.jpg`
- `Side_Release_Adjacent.jpg`, `Caster_Release_Adjacent.jpg`
- `Side_Flow_Sequence.jpg`, `Caster_Flow_Sequence.jpg`
- `comparison_validation.json`

MP4s: 1920×540, 60fps, 385 frames (continuous source 0.0–6.4s). LEFT BASELINE = previous CasterPressure3DFlowPrototype; RIGHT CANDIDATE = CasterPressureGroundedFlow. The shared Blue/Red/Purple settings and bright-neutral scene are identical. No environment-driven emission/Bloom adjustment.

QA logs under `unity/Logs/CasterPressureGroundedFlowQA/`: `CompileFinal.json`, `ShaderFinal.json`, `TargetedFinal.json`, `targeted.xml`, `play-canonical.xml`, `ResourcesFinal.json`, `ConsoleFinal.json`, `RestoredFinal.json`, `start-hashes.json`.

## Files added / changed in this pass

New (and their `.meta`):

- `unity/Assets/Scripts/Player/CasterPressureGroundedFlow.cs`
- `unity/Assets/Scripts/Player/CasterPressureGroundedFlowProfile.cs`
- `unity/Assets/Resources/VFX/CasterPressureGroundedFlow.shader`
- `unity/Assets/Resources/VFX/CasterPressureGroundedFlowProfile.asset`
- `unity/Assets/Editor/CasterPressureGroundedFlowProfileEditor.cs`
- `unity/Assets/Editor/CasterPressureGroundedFlowCapture.cs`
- `unity/Assets/Editor/CasterPressureGroundedFlowValidation.cs`
- This report; local-only QA harness/results/media under the QA folder above.

Existing file changed: `unity/Assets/Scripts/Player/PrototypeHollowPurplePresentationRuntime.cs` — 10 opt-in hook lines added; prior dirty edits retained.

No automatic further polish after A/B. No USER VERIFIED declaration, Production integration, commit or push.
