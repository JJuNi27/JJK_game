# Gojo domain space and technique presentation review — 2026-09-08

Status: implementation and automated runtime validation completed; USER VISUAL VERIFICATION PENDING. No commit, push, merge, license modification, or scene save is part of this pass.

## Starting state and preserved work

Branch: `feat/gojo-blue-screen-distortion`; starting HEAD: `b2eaaeb`.

The working tree already contained modified `VFXLab.unity`, `ProductionCombatFeedbackDirector`, `VfxLabPreviewSequence`, `ProductionParticleVfxRuntime`, `ProductionSignatureVfxDirector`, `PrototypeHollowPurplePresentationRuntime`, and `UnlimitedVoidPrototypeVisual`. It also contained the choreography report, hand diagnostic scripts, `TechniqueChargeVisual`, the ink shader, and beat clock/tests. Those changes were treated as the starting implementation, not discarded or attributed wholesale to this pass.

The existing `GOJO_PRESENTATION_CHOREOGRAPHY.md` records an earlier pass. Its earlier validation results do not validate the new domain transition or this revision.

The scene, imported FBX files, Animator controllers, `ThirdPersonPlayerController`, `VfxLabPreviewCharacter`, `BlueConvergenceField`, and `GojoTechniqueController` were hash checked against the start of this task. All 11 files in that protected inventory remained byte-identical at the check. Local models/audio and the three hand diagnostic scripts were not edited. Walk 3 / Run 14, verified blend-tree speeds, evade, Blue's four-hit logic, and Purple damage/range scheduling were not changed.

## Architecture found and chosen

`GojoDomainController.ActivateDomain` is the gameplay capture owner. It queries `Physics.OverlapSphere(transform.position, domainRadius)`, deduplicates `IDomainStunnable` components and applies `domainDuration` stun. This query, input state machine, energy rules, 30 m default radius and 3 s default duration remain authoritative.

Previously, the controller passed `domainRadius` into `UnlimitedVoidProductionVisual.Configure`, coupling environment scale to capture scale. The presenter built a procedural enclosure, a floor without collision, stars, a forward focal sphere, view-local purple tunnel, and ink quads concentrated around the focal sphere. Actors stayed on the normal map. Combat uses `SimpleCameraFollow`; VFXLab uses its independent orbit camera. Red preview also inadvertently spawned a new charge object on every Update.

The chosen implementation is a **same-scene, far-away isolated interior instance**, behind `IDomainInteriorTransition`. `DomainInteriorSpace` implements `Enter`, `Exit`, `Dispose`, `IsEntered`, and `Translation`. Default origin is `(0, 2048, 0)` and the independent interior radius is 90 m. A temporary physical floor supports the real actors; their object identity, parents, component references, lock-on references and relative displacement are retained. This is actual participant relocation, not a skybox swap.

This is smaller than an additive-scene conversion for the current prototype: the actual `CurseBotController` uses CharacterController movement and timed freeze, with no NavMesh-driven AI. Keeping actors in their existing scene avoids transferring their gameplay ownership or rebuilding references. A future additive environment can replace the transition implementation. The optional NavMeshAgent safeguard suspends an agent during transfer and restores its enabled state at its original position; it is not a general adapter for NavMesh-driven combat in the pocket space.

The original world remains loaded. Existing independent projectiles, summons and non-captured actors are not transferred. Exterior/interior combat, barrier damage and terrain destruction remain outside this implementation. Future sustained domain combat with arbitrary spawned entities needs explicit ownership/transition adapters.

## Capture, barrier and interior separation

1. Gameplay captures/stuns using the existing radius, and supplies the already captured transforms to presentation.
2. `DomainPresentationSession` selects the presentation victim and builds an exterior-only black sphere, detached from the moving caster.
3. Detail shots precede rapid closure. A screen occlusion covers the actual spatial discontinuity.
4. Each participant is saved and moved by the same translation; the interior presenter is anchored in the new space. The barrier stays behind.
5. The accepted purple tunnel's 72-line construction and converging line trajectories are retained. It reveals the independent interior.
6. Completion, cancellation, disable/destruction, caster death and presentation-scene unload call the idempotent exit path.

`barrierVisualDiameterMeters` defaults to 10 m and never enters the capture query. Optional `fitBarrierToCapturedParticipants` defaults to true: the configured diameter is a minimum and the effective diameter grows to enclose captured actors. Disable fitting for a strictly fixed visual diameter. Even a 120 m visual barrier cannot capture an opponent outside the gameplay overlap. Interior radius has no dependency on either barrier fitting or capture radius.

## Per-participant restoration

Each `DomainInteriorSpace.ReturnState` stores one actor's Transform, position, rotation, CharacterController enabled state, optional NavMeshAgent enabled state, and optional Rigidbody kinematic/linear/angular velocity state. Duplicate actors are saved once. Actors are not reparented. Teleport temporarily disables the motor, then restores it; exit restores each record individually, skips destroyed actors, synchronizes physics, and removes the floor. Death does not resurrect an actor or undo damage.

Positions are captured immediately before entering the pocket space, so movement during the opening shots does not overwrite them with another participant's position. Repeated Exit calls do nothing after the first restoration.

## Camera and future cues

`DomainCameraOverride` is a single-owner lease on the existing Camera. The normal combat and lab controllers explicitly yield transform/FOV writes while the lease is held; their targets, offsets, orbit parameters and enabled states are not replaced. Existing combat feedback continues to own flash, shake and FOV requests. Flash decay continues during the cinematic.

The lease saves camera position, rotation and FOV, shifts its return position with the space transition, and restores it when shots finish or are cancelled. The session also shifts combat focus coordinates / lab smoothed focus and restores the original-world camera pose when the domain ends. A destroyed lease owner has a recovery path. In VFXLab, MMB immediately releases cinematic ownership to the existing inspection orbit controls; pause and speed remain on the lab's scaled clock.

Victim priority is valid captured lock-on target, then nearest captured actor, then stable scene/hierarchy path. An empty capture uses hand/hero shots and omits face/eye/victim shots. The full sequence has hand, side face, eye, hero, barrier, tunnel, victim upper-body and arrival shots. No facial bones, blend shapes, blindfold meshes or Animator assets are modified. The existing gameplay freeze supplies the overwhelmed fallback. A short local face light supports detail framing.

Optional hand/face/eye anchors and `onShot` allow future authored animation/voice integration. Unassigned anchors now resolve the actual humanoid or named Mixamo bones and scale default framing to the actor?s head height. This corrected a captured hand shot that previously framed the legs. The current authored model keeps its hand in its pocket; this pass does not invent a domain-seal pose. `DomainOverwhelmed` is an emitted shot hook. `TryAuthoredDetailCue(capturedCastToken, cue)` accepts only the current cast's hand/face/eye/hero presentation cue during the detail window; timers remain the fallback and these cues never activate gameplay. Blue/Red retain their existing choreography/release-token architecture.

Default full capture timing: detail 0–0.44 s; closure completes 0.66 s; black boundary through 0.76 s; existing tunnel through 1.48 s; white-blood beats 1.82 / 2.06 / 2.30 s; full environment 2.72 s; gameplay ends at 3 s. Empty capture is 0.20 s shorter. These are intentionally brief cuts within the existing gameplay duration, not an extended stun.

## Presentation tuning

`DomainPresentationSettings` appears on `GojoDomainController` and `VfxLabPreviewSequence`. It exposes barrier diameter/fitting/closure/opacity, black boundary duration, interior origin/radius/brightness, white-blood count/scale/depth, cinematic enable, detail/victim/return shot durations, hand/face/detail/hero offsets, detail/eye/wide FOV, authored anchors and shot event. Runtime-generated domain visual retains its existing serialized tunnel/arrival beat durations. Geometry count/radius settings are construction-time settings; they are not a live rebuild system.

`ProductionParticleVfxRuntime` additionally exposes Blue torn-chunk count (6–48, default 24). No gameplay radius or damage value is an artistic tuning input to the new capture system.

## Technique changes

- **Unlimited Void:** actual separate space, opaque transition boundary, deterministic cinematic, three-depth 360° white-liquid distribution (default 36 clusters), crisp torn liquid/droplets rather than soft fog, irregular layered pale-blue/violet event horizon around a black center, increased controlled blue character lighting. The central landmark remains directional; the surrounding ink/star environment supplies other views.
- **Red:** one charge per preview, rotating/shrinking formation filaments converging into the kernel, immediate consume on release/cancel, stronger outward fragments, delayed wider pressure front, outward directional refraction using the existing distortion shader with a sign parameter, and stronger white-red flash/impulse through the existing audio-feedback owner. Generic/Nue impact feedback is preserved; the stronger mode is selected only for Gojo Red and its preview.
- **Blue:** the active body receives a denser cyan palette plus 24 lit angular mesh chunks in large/medium/small tiers, distinct inclined/eccentric trajectories, variable speeds and late inward collapse. Existing shader layers, localized lensing and gameplay pulses remain. This changes the active technique, not just anticipation.
- **Purple:** merge/launch durations (0.24 / 0.78 s) and 18 m visual travel are retained, as is existing formation/fusion. Travel gets intermittent branched arcs and up to 28 sampled ground/spatial scar segments with violet seams. The travelling body hides after travel; the presentation-only residue fades over 1.6 s. Sequence cancellation/disposal owns all related geometry/materials. No terrain is modified.

## Files changed by this pass

Paths below are relative to `unity/Assets/`. New C# files include `.meta` files.

| File | Purpose |
| --- | --- |
| `Scripts/Player/DomainPresentationSettings.cs` (new) | Shared serialized presentation configuration |
| `Scripts/Player/DomainInteriorSpace.cs` (new) | Transition contract, isolated floor, participant return records |
| `Scripts/Player/DomainPresentationSession.cs` (new) | Barrier, entry/exit transaction, shot director, victim choice, cue hooks |
| `Scripts/Camera/DomainCameraOverride.cs` (new) | Exclusive reversible camera lease |
| `Scripts/Player/GojoDomainController.cs` | Pass captured participants to presentation; end/death cleanup; separate configuration |
| `Scripts/Camera/SimpleCameraFollow.cs` | Yield cinematic ownership and translate focus reference |
| `Scripts/Dev/VFXLab/VfxLabOrbitCamera.cs` | Yield cinematic ownership and translate smoothed focus |
| `Scripts/Dev/VFXLab/VfxLabPreviewSequence.cs` | Shared domain session; Red spawn/consume lifecycle; preview cleanup |
| `Scripts/Player/UnlimitedVoidPrototypeVisual.cs` | Independent interior scale/anchor, all-direction liquid and living horizon |
| `Resources/VFX/UnlimitedVoidInk.shader` | Crisp liquid body, fingers, holes and droplets |
| `Scripts/Player/TechniqueChargeVisual.cs` | Red spiral/convergence and explicit consume |
| `Scripts/Player/ProductionSignatureVfxDirector.cs` | Use immediate charge consume for release/cancel |
| `Scripts/Player/BlueTornDebris.cs` (new) | Bounded deterministic angular rubble |
| `Scripts/Player/ProductionParticleVfxRuntime.cs` | Active Blue rubble/body tuning and Red pressure layers |
| `Resources/VFX/GojoBlueDistortion.shader` | Default-preserving radial direction parameter for Red |
| `Scripts/Core/PrototypeCombatAudio.cs` | Opt-in stronger Red feedback through existing owner |
| `Scripts/Player/PrototypeCombatAudioEventBridge.cs` | Select stronger feedback for Gojo Red only |
| `Scripts/Player/PurpleTravelAftermath.cs` (new) | Travel lightning and ground/spatial scars |
| `Scripts/Player/PrototypeHollowPurplePresentationRuntime.cs` | Attach travel aftermath and own its residual lifetime |
| `Editor/DomainInteriorSafetyTests.cs` (new) | Participant/camera/selection regression tests |
| `Editor/DomainPresentationRuntimeTests.cs` (new) | Gameplay capture/restore and repeated lab tests, optional graphics captures |

This document is also new. `ProductionCombatFeedbackDirector.cs`, `VFXLab.unity` and other pre-existing changed files not listed above were not edited by this pass.

## Validation

Validation artifacts live in `unity/Logs/DomainSpaceQA/` (ignored by Git).

- Runtime C# and Editor/test assemblies compile with Unity 6000.3.20f1's Roslyn compiler and the project's actual assembly references/defines. The existing optional unassigned `techniqueCastingPoint` produces CS0649; no C# errors.
- Existing Python tests: 11 gameplay/registry/gesture tests passed in `.venv`; 2 retarget tests passed using the Python environment with pytest. The initial broad runs exposed split environment dependencies; the complete set was then executed in the environments providing those dependencies.
- Protected inventory hashes and tracked source whitespace check passed.
- Unity execution initially failed on sandbox Package Manager IPC. Outside the sandbox, the first batch process connected to a licensing channel without the required entitlement. The user's Hub Personal license is active; a subsequent Hub-launched Editor log confirms Unity Personal with unlimited expiration. No license files or activation state were altered. Automated execution succeeded after that refresh, using the same Windows user/session and a working version-specific licensing channel.

The two new test files cover independent multi-participant restoration, repeated exit, destroyed participants, exclusive camera ownership and FOV restoration, deterministic victim choice, capture independence under 1 m versus 120 m barriers, lock preference, cancellation, normal expiration, caster death, presentation-scene unload, repeated Red cleanup, Blue/Purple disposal, and empty-capture lab orbit/pause behavior. The final full run actually executed all 16 tests: **16 passed, 0 failed, 0 skipped** (`editmode-results.xml`, `editmode-final.log`). This comprises 14 EditMode cases and two tests that enter Play Mode. A subsequent focused VFXLab/capture rerun is recorded separately in `capture-results.xml` / `capture-final.log`; it passed **1/1, 0 failed**, and repeats one of those same 16 tests, not an additional distinct test. No tests remain failing in the completed full run. Runtime and shader-error scans of the successful logs found no C# errors, shader errors, NullReferenceException, MissingReferenceException or InvalidOperationException.

The full run after resuming the interrupted work also passed 16/16 before the bone-framing correction, and the corrected implementation then passed the full suite again. The earlier failures were fixed in the test harness: EditMode coroutines needed explicit clock polling after EnterPlayMode; the ordinary MonoBehaviour disable callback needed Play Mode rather than an EditMode assumption; and the lab restore baseline needed to be saved after the existing motor settled on the existing stage. The new camera-framing correction was driven by a real render capture.

The expanded lab test repeats Red three times and Blue/Purple/Domain twice, covering cancellation and natural cleanup for Blue/Purple. It checks a single charge before Red release, zero charges afterward, bounded active Blue/Purple components, complete disposal, an empty-capture short domain, frozen presentation clocks during pause, eight inspection angles, and camera release on exit.

## CODEX PREVIEW / capture results

The actual saved `VFXLab.unity` was loaded in the background Unity process without saving or editing it. The automated test invokes the same preview actions used by Q/E/R/V; it is not a manual keyboard playthrough. Sixteen 1280?720 PNGs are under `unity/Logs/DomainSpaceQA/CODEX_PREVIEW/`:

- `Blue_active.png`: side view confirms mesh chunks at several sizes/depths around the active blue core.
- `Red_travel.png`, `Red_impact.png`: release/travel and an impact-focused view; lifecycle assertions separately establish charge consumption. The impact capture is synchronized to the actual IMPACT phase rather than a guessed timer.
- `Purple_travel.png`, `Purple_scar.png`: visible travel arcs and ground seams/scar after the orb has moved onward; repeat tests verify natural expiration and cancellation cleanup.
- `Domain_hand.png`, `Domain_barrier.png`, `Domain_tunnel.png`: the current authored hand/pocket framing, exterior barrier, and preserved tunnel.
- `Domain_0.png` through `Domain_315.png` in 45? increments: every sampled direction contains distributed white-liquid structures; the original stage geometry is not visible. The lab?s own preview guide moves with its inspection anchor and remains visible.

These are real URP single-camera render requests. They exclude screen-space overlay UI/flash and cannot prove the continuous blackout boundary or camera motion by themselves. The captures show billboard-based liquid clusters and a thin distant focal edge; their density, depth impression, edge treatment, brightness and overall quality still require the user?s judgment. The victim-focused full sequence is exercised by the gameplay fixture, but its final framing with authored enemy/Gojo animation still needs Play Mode review.

Final working-tree inventory: 13 tracked modified files and 29 untracked files (including all pre-existing work and new `.meta` files). Exact paths are saved in `unity/Logs/DomainSpaceQA/git-status-final.txt`. HEAD remains `b2eaaeb`; nothing was committed, pushed or merged.

## User Play Mode review

All subjective appearance remains **USER VISUAL VERIFICATION PENDING**. Any test-generated images are **CODEX PREVIEW**, not approval.

Review the full captured-enemy sequence and the empty lab sequence; hand/face/eye framing with the actual Gojo model; occluded transition and independent spatial scale; four 90° interior views; character readability and liquid edges; return positions/FOV after cancel and normal completion; MMB takeover and repeated preview; Red kernel consumption and repulsion; Blue chunk scale/depth and accepted four-hit timing; Purple preserved fusion, travel lightning and scar fade. Continuous transition/flash timing, audio listening, target-device frame time, arbitrary NavMesh AI, and interactions involving independent projectiles/summons require manual runtime review beyond the automated fixtures and isolated frames.
