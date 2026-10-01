# PurpleIdentityCoupled exploration review

Status: CODEX TECHNICALLY CHECKED / PENDING USER VISUAL REVIEW / NOT USER VERIFIED. Candidate saved OFF. Do not promote to Production.

## Change contract

- Target gap: PurpleIdentityMass retains a coherent centre but its front surface and existing outer lightning read as separate layers around a complete orb.
- Allowed: a separate candidate body shader/corona/profile and candidate-only routing in the existing charge body/outer runtimes.
- Preserve: Production, PurpleIdentityMass, ingredients, formation, release, travel, terminal, gameplay, camera, and timing.
- Expected visible change: brief local interruptions on the near surface and spatial discharge driven by the same event clock, while the main mass remains stable.
- Failure: a dome, jelly motion, detached neon sticks, or no perceptible coupling.
- Evidence: labeled Side/Caster A/B, exact protected-frame comparison, compile/shader check, and lifecycle check.

## Actual result

An early broad corona looked like a translucent half-sphere shield; it was removed. The final preview retains the stable mass and avoids that shield. Local body interruption and 3D discharge are synchronized in code, but the screen result is weak: the orb is still continuously readable, the central radial star persists, and some new fragments look detached. This is a diagnostic candidate, not an art-direction approval or a recommendation for Production.

## Evidence

`unity/Logs/PurpleIdentityCoupledQA/AB/20260930_045157_900/`

- `Side_AB_Full.mp4` and `Caster_AB_Full.mp4`: 60 fps, left BASELINE = PurpleIdentityMass, right CANDIDATE = PurpleIdentityCoupled.
- `Side_*` and `Caster_*` stills and raw frame files.
- `evidence.json`: exact raw RGB equality in formation frames 0–95 and travel frames 320–384; terminal PNG bytes and root pose records equal in both views. Charge frames changed: 206 per view.

Unity C# recompile completed with zero errors. Both candidate shaders passed `ShaderUtil` error checks. Candidate profile is saved OFF. A direct static test method passed. Manual repeated spawn/routing/travel/cleanup passed three times, with owned materials and meshes returning from 0/0 to 0/0. Console ground truth reported zero errors. The Unity Test Runner run for `PurpleIdentityCoupledTests` did not return a result within 300 seconds and was cancelled, so automated test-suite status is **INCONCLUSIVE**, not PASS.

No commit, push, or Production promotion was performed.
