# Gojo locomotion / reactive evade / animation integration

2026-09-07 · `feat/gojo-blue-screen-distortion` · Implementation; user Play Mode movement review passed.

## Code findings and Gojo evade tuning

Before this change, `VfxLabPreviewCharacter.ApplyMovement` used Run 14 and an unrelated
hardcoded dodge: 9.5 → 5.5 over 0.36 seconds. `VfxLabPreviewSequence.TickDodge` had a second
0.36 / 0.58-second timeline. Combat used `ThirdPersonPlayerController`: constant speed 12,
movement/action duration 0.24, cooldown 0.75, immediate invulnerability 0.30 seconds.
`CombatActionGate` derives Dodging from the movement controller's `IsDodging`; the movement
controller owns Space acceptance and cooldown. VFXLab is a presentation sandbox, without Health,
real invulnerability, or production cooldown enforcement; its replay/cancel shortcuts stay available.

Continuous-time distances, in Unity world units (treated as metres):

| Movement | First 0.10 s | First 0.20 s | Full evade distance |
|---|---:|---:|---:|
| Run 14 | 1.40 | 2.80 | — |
| Old VFXLab dodge | 0.894 | 1.678 | 2.70 over 0.36 s |
| Old combat dodge | 1.20 | 2.40 | 2.88 over 0.24 s |
| Proposed/applied Gojo burst | 2.78 | 3.90 | 3.90 over 0.20 s |

The old preview dodge was slower than Run at every point. The new burst clears about twice
the running distance in the first 0.10 seconds. It deliberately does not stay faster than Run
throughout recovery. The default speed curve is piecewise linear:

| Elapsed | Speed |
|---|---:|
| 0.00 s | 32 |
| 0.06 s | 28 |
| 0.14 s | 14 |
| 0.20 s | 0 |

Movement lasts 0.20 seconds; stationary horizontal recovery lasts 0.04 seconds. Gravity continues.
Action occupancy totals 0.24 seconds. Cooldown remains 0.75 seconds measured from accepted input.
Invulnerability remains a separate [0.00, 0.30)-second window. Thus the pre-existing 0.06 seconds
of invulnerability after action unlock remains; shortening it would be a separate balance change.
The theoretical maximum invulnerable duty cycle remains 0.30 / 0.75 = 40%, rather than increasing
with burst speed. No additional invulnerability is granted by presentation or movement completion.

Scale references: VFXLab's floor is 64 × 64; its guide ring radius is 5.8 and Blue guide radius 4.5.
The MVP scene builder creates a plane at scale 3 (30 × 30). A 3.9m evade is 13% of that arena width;
it is a short escape step, not a cross-arena teleport. It does not guarantee escape from the centre
of a 4.5m-radius field, especially when accounting for the character capsule. These are initial
tuning values for arena testing, not a claim that balance or subjective responsiveness is verified.

`EvadeMotion` integrates a fixed 100-interval curve table at start and returns differences in
cumulative distance, so changing FPS or crossing the end on a long frame cannot extend range.
The default curve knots coincide with table samples. Custom curves are sampled approximations;
Y is clamped to [0, 1], with `burstSpeed` specifying the maximum speed. Live curve edits affect the
next evade, not a motion already in progress. Duration and recovery are also captured at start.

Movement is applied through `CharacterController.Move` in steps no larger than half its radius.
There are no transform position warps, disabled colliders, or accumulated blocked distances.
Wall sliding and grounding remain CharacterController responsibilities; map colliders and layer
settings still need scene review. Unity documents collision-constrained displacement and separate
gravity handling in [CharacterController.Move](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/CharacterController.Move.html).

## Movement and presentation boundaries

`ProductionCombatInput.RunHeld` owns both Shift keys. VFXLab and production Gojo use Walk 4 /
Run 14. Existing other-character serialized movement/dodge values remain the fallback. Applying
Gojo through `PrototypeCharacterController.ApplyCharacter` supplies Gojo's movement profile to
the shared controller; switching away clears that profile and cancels pending evade motion without
resetting the owner's cooldown. Other characters can use the same `ConfigureMovement(profile)`
entry point; no new character-specific Dodge controller is needed.

`CharacterMovementProfile` and nested `EvadeProfile` are inline serializable data. This is the
smallest boundary compatible with today's runtime character switching. Inspector tuning locations:

- VFXLab preview character → Movement → Movement Profile.
- Production player → Prototype Character Controller → Character Movement → Gojo Movement.

Both start from the same Gojo defaults. Their serialized Inspector overrides are separate: tuning
one does not silently rewrite the other. A shared ScriptableObject asset can replace these inline
profiles when multiple authored character assets actually exist; no asset registry is required now.

The reusable split is:

```text
ProductionCombatInput → existing owner / action gate / cooldown
                                     ↓
CharacterMovementProfile.Evade → EvadeMotion → CharacterController.Move
                                     ↓
EvadePresentationCues: Started / Recovery / Completed / Cancelled
                                     ↓
character animation / VFX / SFX adapter
```

Evade cues carry the owner transform, direction, profile metadata, and phase. `styleId`,
`animationTrigger`, `animationState`, `vfxCue`, and `sfxCue` are presentation contracts; they never
set gameplay timing. Listeners must filter by owner and unsubscribe on disable. The VFXLab adapter
already consumes an existing matching Trigger safely. State-name playback and custom VFX/SFX
adapters remain to be connected when assets exist. The current Dodge audio path is retained, so a
future audio adapter must replace that fallback, not play a second sound on top of it.

Gojo keeps the existing relaxed Idle presentation during burst instead of the prototype dive/lean.
No new roll, slide, pocket pose animation, spatial streak, or distortion asset was invented.
Yuta slide/step/roll, Itadori explosive step, and Sukuna-specific presentation can supply their own
profiles and subscribe to the same lifecycle. Their final numeric tuning is intentionally unset.

The local Gojo Animator has Idle/Run transitions driven by `PlanarSpeed`; there is no Walk clip.
Transitions and clips remain intact, so walking currently uses the existing Run animation at the
slower gameplay speed. A future 1D Idle/Walk/Run blend tree is appropriate when Walk is available:
one continuous planar-speed input, thresholds corresponding to authored clip speeds, and no
combinatorial transitions. Creating a fake Walk state now would not improve animation quality.
The preview binding refreshes parameter types when its Animator/controller changes, rejects stale
external animator references, and enforces Apply Root Motion OFF. Visual scale 0.5 is preserved.

## Technique choreography decision

Use a hybrid: explicit presentation release callback with a per-cast token, plus a central reusable
fallback clock. `TechniqueReleaseClock` owns exactly-once release arbitration; gameplay controllers
retain CE, cooldown, target capture, damage and action ownership. `TechniqueChoreographyCues`
provide Began / Released / Cancelled and optional animation trigger/state metadata to presentation.

| Candidate | Decision |
|---|---|
| Animator normalized time | Useful inside an authored adapter, but clip transitions/loops need explicit identity and cancellation. |
| FBX Animation Event | Convenient per-clip cue; keep event data in Unity import settings or adapter configuration rather than modifying source gameplay. |
| StateMachineBehaviour | Suitable adapter for a known controller; capture the cast token on entry and reject stale exits. |
| Large Timeline/playable system | Defer: no final technique clips exist, and it would duplicate current timing/feedback ownership. |
| Explicit callback + fallback clock | Applied to Blue/Red now; reusable for future characters and clip adapters. |

Blue/Red begin emits a cue with owner, semantic technique ID, cast token, animation binding and
the existing cast duration. An adapter captures the token then calls
`ITechniqueReleaseReceiver.RequestPresentationRelease(capturedToken)` at its release point.
It must never read a new cast's token when an old animation finishes. Duplicate, cancelled and
old-token requests are rejected. Accepted release is consumed in the gameplay Update, after
existing combat/domain checks, at most once. Animator callbacks do not spawn gameplay directly.

`acceptPresentationRelease` defaults OFF. Existing Blue 0.24 / Red 0.30 timers and scene overrides
remain effective. If enabled, a valid event may release earlier; if absent, the configured timer
still releases. This is an explicit fallback deadline, not an unlimited wait for an animation.
When final clips arrive, set the intended release/deadline together and validate them. No new
recovery lock is imposed before a real recovery clip and gameplay decision exist.

Existing `CombatAudioEvents`, `FighterAnimationStateSource` and `PresentationVfxRuntime` remain
the downstream presentation paths. The new anticipation contract does not move existing Blue
voice or VFX timing. VFXLab's isolated Blue/Red presentation timeline stays at its existing timing;
it does not invoke combat release callbacks or claim to simulate damage.

Purple keeps its existing unscaled 0.24 merge / 0.78 travel / 18m range and pending-hit schedule.
Domain keeps its gesture acceptance, release timing window, radius, duration and gameplay states.
Their existing technique presentation requests remain the integration points until final gestures
and clips exist; they are not rerouted through an early-release clock in this pass. Future adapters
can use the shared token contract once their actual release semantics are authored. Pass 7 camera,
FOV, focus, flash and hit-stop owners were not changed. Accepted Blue four-hit and Red effect code
were not changed.

## Blender tool

Load `scripts/blender_retarget_mixamo.py` in Blender's Text Editor, run it, then call:

```python
bake_retarget('SOURCE_ARMATURE_NAME', 'MASTER_ARMATURE_NAME', 1, 30,
              'Gojo_NewAction', apply=False)  # inspect map first
bake_retarget('SOURCE_ARMATURE_NAME', 'MASTER_ARMATURE_NAME', 1, 30,
              'Gojo_NewAction', apply=True)
```

Use the actual Object names, source frame range and a new Action name. The script matches Mixamo
names with/without namespace separators, checks required humanoid bones and rejects ambiguous
duplicates. Unmatched target bones are listed. It copies all matched bone rotations in WORLD space
and Hips location in WORLD space, bakes selected poses with visual keying, creates a new Action,
and removes only its own constraints. Existing target constraints/drivers/active NLA tracks are
rejected because silently baking over them risks double transforms. Source Action and old target
Action datablocks remain. Selection, active object, mode and current frame are restored.

The script supports the Blender 5.x PoseBone selection API and a 4.x selection fallback, consistent
with the [Blender 5.0 API change](https://developer.blender.org/docs/release_notes/5.0/python_api/).
Visual baking uses the documented [NLA bake operator](https://docs.blender.org/api/5.3/bpy.ops.nla.html).
It performs no automatic file save/export or transform application. FBX export → Unity Humanoid →
Copy From Other Avatar → `Gojo_Blender_MasterAvatar` remains the established manual finishing path.
Root-motion cleanup and artistic timing still require inspection on the real source animation.

## Changed files and validation

| File | Reason |
|---|---|
| `Core/CombatInputBindings.cs` | Central Run intent; both Shift keys. |
| `Core/CharacterMovementProfile.cs` (+ meta) | Serializable walk/run/evade data and Gojo curve default. |
| `Core/EvadeMotion.cs` (+ meta) | Shared curve integration, collision movement, presentation lifecycle. |
| `Player/ThirdPersonPlayerController.cs` | Consume profile, preserve gate/cooldown/Health ownership, cancel on disable/death. |
| `Player/PrototypeCharacterController.cs` | Supply movement configuration during character switching. |
| `Dev/VFXLab/VfxLabPreviewCharacter.cs` | Walk/run split, shared evade, relaxed Gojo presentation, safer Animator binding. |
| `Dev/VFXLab/VfxLabPreviewSequence.cs` | Read shared evade completion/recovery instead of duplicate dodge timers. |
| `Player/PrototypeFighterPresentationController.cs` | Use existing relaxed Gojo stance during evade. |
| `Core/TechniqueReleaseClock.cs` (+ meta) | Reusable exactly-once release/cancel arbitration. |
| `Core/TechniqueChoreographyContract.cs` (+ meta) | Optional authored binding and token-bearing presentation cues. |
| `Core/TechniquePresentationRequest.cs` | Append Blue/Red semantic IDs without changing existing enum values. |
| `Player/GojoTechniqueController.cs` | Blue/Red callback integration with existing timer defaults. |
| `Editor/LocomotionChoreographyTests.cs` (+ meta) | Frame partitions, hitch/cancel, collision and release regression coverage. |
| `scripts/blender_retarget_mixamo.py` | Reusable retarget/bake tool. |
| `tests/test_blender_retarget.py` | Mapping ambiguity and missing-rig validation. |
| `tests/blender_retarget_smoke.py` | Real Blender API verification on synthetic armatures. |
| `docs/GOJO_MOVEMENT_CHOREOGRAPHY.md` | Findings, tuning rationale, integration and review instructions. |

Paths beginning with Core/Player/Dev above are relative to `unity/Assets/Scripts`; Editor is
relative to `unity/Assets`. The existing dirty `VFXLab.unity` is the user's pre-existing local
model/Animator attachment; it was not edited. No LocalModels, model, FBX, texture or .blend was
added to Git. The local scene remains excluded from this change set.

Validation performed:

- `dotnet build unity/Assembly-CSharp-Editor.csproj --no-restore -v:q`: runtime and Editor build
  passed, 0 warnings / 0 errors. New source files are included in the regenerated Unity projects.
- Unity 6000.3.20f1 isolated EditMode fixture: **8 passed / 0 failed**, including 30/60/144 FPS,
  early displacement/recovery, hitch/pause/cancel, a thin-wall collision with a full-burst hitch,
  and stale/duplicate/missing release events. Results:
  `unity/Temp/LocomotionValidation/editmode-results.xml` (local ignored artifact).
  The fixture copies only the shared motion/clock sources and tests; it does not validate the
  real combat scene, input ordering, actual Health damage timing, or authored model presentation.
- `python -m pytest -q tests/test_blender_retarget.py`: **2 passed**.
- Blender 5.2.1 background synthetic-rig smoke: **passed**. WORLD rotation / Hips location,
  parented bones with a rotated/scaled target, dry-run, Action preservation and constraint cleanup.
- User Unity Play Mode review: **passed** for Gojo Walk 4, Run 14 and the Dodge burst feel.
- `git diff --check` on authored source/docs/tests: passed. The pre-existing local scene diff has
  Unity YAML trailing spaces; this pass did not rewrite that user-owned scene.

The movement presentation has direct user acceptance. The remaining static build, automated
EditMode and synthetic Blender checks do not replace combat-scene gameplay validation.

## Remaining Unity review

1. Test wall fronts, corners, slopes, thin colliders, arena edges and low FPS. Confirm no stored
   displacement after blocking, penetration, or excessive camera discomfort. Camera tuning is unchanged.
2. In combat, verify cooldown 0.75, action lock through burst/recovery, existing 0.30 invulnerability,
   death/disable/tag cancellation, and unchanged Space/technique/domain priority. VFXLab cannot
   verify invulnerability or cooldown because it has no combat Health/action gate.
3. Check Blue four hits, Red effects/audio, Purple timing/range and Domain gesture/activation.
   With default bindings, no missing animation event may prevent normal cast completion.
4. Verify local Animator/Avatar replacement, Idle↔Run, scale 0.5 and root motion OFF. Walk clip,
   authored evade clip, technique gestures/recovery, cyan streak/distortion and custom evade sound
   remain intentionally pending. Test the Blender tool on a working copy of a real source animation
   before using its baked Action in the verified master pipeline.
