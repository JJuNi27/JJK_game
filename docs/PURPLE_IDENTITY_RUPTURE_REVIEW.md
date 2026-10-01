# Purple Identity Rupture — exploration review

Status: **CODEX TECHNICALLY VALIDATED / VISUAL TARGET NOT MET / PENDING USER VISUAL REVIEW**. This is a separate, saved-OFF candidate. It is not a production replacement or a USER VERIFIED baseline.

## Change contract

- Target gap: `PurpleIdentityMass` and `PurpleIdentityCoupled` still read as a complete violet ball with a regular central flare and detached outer decoration.
- Allowed: candidate-only charge body rendering and candidate-only charge outer routing, with a new profile, shader, and QA capture. The underlying mass position and size remain fixed.
- Preserve: Production, Final2, IdentityMass, IdentityCoupled, formation/fusion timing, trajectory, travel, terminal, gameplay, camera, and user-verified values.
- Expected visible change: brief front interruption exposes inner light; one spatial discharge crosses the body boundary so the light and outer reach read as one event.
- Failure: jelly-like macro deformation, white circular sticker, long neon band, separate zigzag ornament, or a fully trackable surface.
- Evidence: labeled 60 fps Side/Caster A/B, eight consecutive paired frames, compile/console/shader checks, protected-segment RGB comparison, and candidate charge/travel routing check.

## Result

The candidate uses one raymarched volume for the stationary mass and the discharge outside it, with a larger proxy only during the opted-in charge phase. It suppresses the separate charge halo, hero debris, and arc renderers for this candidate; travel restores Production rendering. The core has less radial-star geometry. Three event directions, path breaks, and a near-side density opening attempt to connect inner light to exterior discharge.

The result **does not solve the requested visual identity**. At the 144–151 frame peak sequence, the centre becomes a fairly round white patch and the sphere's front surface remains easy to trace. The thinner discharge has more temporal variation than the preceding thick band, but much of it reads as a distinct pink zigzag outside the ball. An intermediate attempt to reduce uniform front density exposed an even larger white circle, so that change was rejected within this candidate before final capture. A prior thick-path preview also looked like pink sausage segments; it was not retained. The stable mass succeeds at avoiding the old jelly motion, yet that stability still dominates the silhouette. **Do not promote this candidate on my visual assessment.**

The actual A/B is `LEFT = BASELINE (IdentityMass)`, `RIGHT = CANDIDATE (IdentityRupture)`. Both retain common FlowFirst and ConvergenceDiagnostic. Outputs:

- `unity/Logs/PurpleIdentityRuptureQA/AB/20260930_063007_125/Side_AB_Full.mp4`
- `unity/Logs/PurpleIdentityRuptureQA/AB/20260930_063007_125/Caster_AB_Full.mp4`
- `unity/Logs/PurpleIdentityRuptureQA/AB/20260930_063007_125/Side_Peak_144_151_AB.png`
- `unity/Logs/PurpleIdentityRuptureQA/AB/20260930_063007_125/Caster_Peak_144_151_AB.png`
- Individual paired PNGs: `Side_{Baseline,Candidate}_144.png` through `_151.png` and corresponding `Caster_` images in the same directory.

## Validation and limits

- Unity 6000.3.20f1 C# recompile: 0 errors. Candidate shader import/compiler messages: 0 errors. Console ground truth after capture: 0 errors and 0 warnings. A later import reported one mixed-line-ending warning in the new shader; its line endings were normalized, and reimport produced no new warnings or errors. The historical warning remains in the Editor console buffer.
- Targeted charge/travel route: PASS. Candidate charge shader selected with the separate halo disabled; Production D2-R3 body and travel halo restored during travel.
- RGB/pose guard (`evidence.json` in the capture directory): Side and Caster formation, travel and terminal identical; corresponding poses identical. Charge changed on 206 sampled frames per view.
- The saved profile has `candidateEnabled: 0`. No production profile, protected candidate, scene, timing, or gameplay value was intentionally changed in this pass.
- Resource check: the custom MCP Play capture does not advance a normal frame after its deferred `Destroy`, so a count taken inside that same Play session retained temporary objects. The temporary QA root was explicitly removed; after leaving Play Mode, the counted Purple materials, meshes, and QA roots were all 0. That establishes Editor-session cleanup, but is not proof of per-cast runtime cleanup under normal frame progression. Treat that subcheck as **INCONCLUSIVE**, not as a clean PASS or a proven leak.
- No commit or push.

The visual failure suggests the front mass and exterior event still need a different relationship. More brightness, path noise, or arc count alone would continue to decorate the ball rather than prevent the ball surface from being read first.
