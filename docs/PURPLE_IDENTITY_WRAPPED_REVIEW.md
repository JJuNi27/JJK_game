# Purple Identity Wrapped — exploration review

Status: **CODEX VALIDATED / PENDING USER VISUAL REVIEW**. This is an opt-in visual candidate, saved OFF. It is not a new Production or USER VERIFIED baseline.

## Change Contract

- **Target gap:** The stable Purple sphere reads as a separate object because the surrounding energy is too sparse to interrupt its edge.
- **Allowed scope:** Candidate-only charge body edge damping; candidate-only halo, sparks and short 3D discharge during charge and travel.
- **Must preserve:** Production, Final2 and older candidates; sphere scale and trajectory; fusion timing; gameplay and camera; terminal explosion. The existing travel body, travel halo and wake remain intact.
- **Expected visible change:** A legible spherical mass with brighter, irregular energy events extending across and beyond parts of its edge.
- **Failure condition:** Jelly-like macro deformation, an opaque cloud hiding the sphere, a regular protective shell, or long neon sticks standing apart from the mass.
- **Required evidence:** 60 fps labelled Side/Caster A/B, before/after travel frame, profile saved OFF, shader/compile/console check, pose/terminal comparison, and repeated cleanup.

## Implementation

`PurpleIdentityWrappedProfile` is a separate Resource profile with `candidateEnabled: 0` and Korean Inspector labels. Charge uses an IdentityMass-derived body shader that damps only the moving outer edge. A broken, sparse corona and short-lived 3D arc events surround the sphere. During travel, the original D2-R3 body, halo and wake are retained; only candidate sparks/arcs are added. Terminal uses its existing implementation.

The first visual preview was much too bright and hid the sphere. The dense plume version was discarded. The final candidate retains patchy perimeter emission and more frequent event-like arcs. No existing candidate asset was overwritten.

## Evidence and validation

Final A/B session: `unity/Logs/PurpleIdentityWrappedQA/AB/20260930_105148_847/`.

- `Side_AB_Full.mp4` and `Caster_AB_Full.mp4`: labelled BASELINE / CANDIDATE, 960×540 each view, 60 fps, 385 frames, 6.417 s.
- `Side_AB_Travel_5p7.png` and `Caster_AB_Travel_5p7.png`: travel body comparison.
- `Side_Baseline_144.png` through `Side_Candidate_151.png` and matching Caster files: local frame evidence.
- `evidence.json`: formation identical in both views; body/ingredient poses identical; terminal still identical; charge changed as intended; travel differs in the opt-in outer layer.

Unity C# compile: 0 errors, 0 warnings. Three candidate shaders: 0 compiler errors. Console ground truth: 0 errors. Profile OFF and Korean Inspector check: PASS. Three charge → travel → terminal routing/cleanup cycles: PASS, owned materials and meshes 0/0 before and after. Capture completed and restored its temporary toggles and camera state. Editor returned to Edit Mode.

## Visual judgment

The outer events clearly occupy more space in Side and Caster, and the sphere remains a sphere. At travel, the existing bright body is preserved. **This is not yet a convincing reference match.** Several arcs still read as clean angular neon lines, and the perimeter energy remains less layered and less violent than the reference. The candidate is worth reviewing as a direction test, not promoting as final art. User Play Mode review is pending.

No commit or push was made.
