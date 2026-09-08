# CURRENT TASK — Gojo Third Polish + Domain Recovery

Status: **USER APPROVED FOR THE NEXT CODEX PASS**
Do not implement until the user explicitly starts Codex after pulling these docs and placing any desired local reference images.

## Goal
Perform a focused third polish pass without rewriting accepted architecture.

This pass has two priorities:
1. fix the real post-Domain combat-state regression;
2. push Blue / Red / Purple / Unlimited Void presentation harder and more clearly.

The user explicitly wants spectacle.
Do not undershoot VFX intensity merely because a layer looks aggressive in isolation.

---

## P0 — Gameplay bug: basic melee must recover after Domain

USER VERIFIED issue in CombatMVP:
After Unlimited Void ends, normal basic melee / physical attacks can remain unavailable.

Fix this first.

Requirements:
- preserve intended cursed-technique burnout behavior if it exists;
- restore normal basic melee / physical attack availability after Domain exit;
- do not broadly reset unrelated combat state;
- validate normal completion and relevant cancellation/end paths.

Success:
After Domain ends, Gojo can perform normal physical/basic attacks again even if cursed-technique burnout still intentionally blocks techniques.

---

## P1 — Blue

USER feedback:
The remaining small blue pre-cast effects in front of Gojo still have no meaningful read.

Change:
- remove the small ambiguous pre-cast blue decoration unless technically required;
- keep the singularity / attraction identity;
- make debris darker, rougher, and more like broken environment / rock;
- reduce ice-like bright/smooth chunks;
- slightly reduce dominant giant chunks;
- increase medium/small debris variation;
- keep the core visible;
- make inward suction/collapse clearer;
- stronger spectacle is allowed.

Preserve:
- existing Blue 4-hit gameplay.

---

## P2 — Red

USER feedback:
- projectile feels too slow;
- range feels too short;
- impact still lacks a convincing violent repulsive aftermath.

Change:
- increase projectile travel speed;
- increase effective travel range;
- preserve controllable / deterministic gameplay behavior;
- replace round smoke-puff read with pressure-displaced air / dust / vapor tearing outward;
- strengthen flash + shockwave + outward pressure residue as one `BANG` event;
- it is acceptable to make the hit more visually aggressive than the previous pass.

Preserve:
- charge created once;
- charge removed on release;
- repeated casts do not leak stale visuals;
- Red remains repulsion, not generic fire explosion.

---

## P3 — Hollow Purple

USER feedback:
Purple is already very strong. Do not redesign the successful fusion.

Change only targeted items:
- increase travel range / lifetime; current travel ends too soon;
- ensure gameplay hit/travel behavior matches the increased practical range;
- move/offset the completed Purple during the hold so it does not hide too much of Gojo;
- make release feel stronger;
- increase irregular violet lightning / branching arcs;
- strengthen spatial distortion / warped-space wake while traveling;
- retain the existing large visual/gameplay size;
- optionally break up scar edges slightly so it reads less like a flat carpet.

Preserve:
- Blue+Red fusion foundation;
- current major Purple identity;
- current 0.32 s hold unless there is a technical reason or explicit user request to change it.

---

## P4 — Unlimited Void interior art direction

Current problem:
The Domain reads too much like a dark empty space covered with too many bright White Blood splashes.

Desired:
- keep the black abyss base;
- add much richer deep-blue / blue-white nebula / cosmic depth;
- subtle stars / space depth are allowed;
- make the Domain environment itself visually beautiful, ominous, surreal, and rich;
- do not be afraid of a more visually impressive result;
- reduce White Blood dominance and clutter;
- keep White Blood as an organic accent across near/mid/far depth;
- preserve above/below/360 distribution;
- preserve hidden visible floor + gameplay collision.

Desired read:
**infinite blue-black cosmic nebula + impossible depth + suspended white organic liquid splashes.**

### Reference images
Before implementing this section, inspect any files present under:
`docs/references/gojo/unlimited_void/`

Expected local filenames if the user supplies them:
- `interior_anime_ref_01.png`
- `interior_concept_ref_01.png`
- `cosmic_eye_ref_01.png`

Important:
Foreground characters in reference screenshots are NOT implementation targets.
Use the references for:
- background structure
- color language
- nebula depth
- White Blood balance
- cosmic-eye shape / scale / halo / mood

---

## P5 — Unlimited Void Cosmic Eye

USER feedback:
The eye/focal is still too small and not close enough to the desired dreamlike cosmic-eye reference.

Change:
- increase focal scale noticeably;
- maintain a pitch-black center;
- create a broader iris / accretion / luminous structure;
- use pale blue + white + violet + subtle iridescent spectral layers;
- use layered transparency and different flow speeds;
- make it more hypnotic / sublime / eye-like;
- avoid a simple clean ring.

Use `cosmic_eye_ref_01.png` if present.
Do not reproduce unrelated characters from the reference image.

---

## P6 — Domain exit / barrier-release transition

Current issue:
Returning to the original map is visually too abrupt.

Desired sequence:
Domain active
→ Domain begins to release
→ interior / barrier dissolves from the center outward
→ original world is progressively revealed
→ safe handoff back to normal combat camera/state

Requirements:
- presentation-only; do not implement barrier HP/destruction;
- keep participant restoration safe;
- no camera/transform drift;
- no participant left in isolated Domain space;
- repeated Domain use must clean up correctly.

---

## Protect / do not rewrite
- Walk Speed 3
- Run Speed 14
- Walk Time Scale 1.15
- current locomotion / Animator / evade
- current Blue 4-hit gameplay
- current successful Purple fusion foundation
- current Unlimited Void purple tunnel
- Gameplay Capture Radius vs Visual Barrier vs Interior separation
- same-scene isolated far-away Domain Interior architecture
- per-participant restoration architecture
- hidden visual floor with collision floor retained
- existing camera ownership / restore system

## Validation strategy
During implementation:
- use targeted checks only for the subsystem being changed;
- do not rerun the entire Unity suite after every visual tweak.

After all requested items are stable:
- run the full relevant Unity regression once;
- run existing Python tests once if still relevant;
- preview Q / E / R / V in VFXLab and CombatMVP as appropriate.

## Done when
- post-Domain basic melee recovery is fixed;
- Blue no longer uses meaningless small pre-cast decoration and debris reads less like ice;
- Red travels faster/farther and impact reads as violent repulsive pressure;
- Purple travels meaningfully farther, hides Gojo less during hold, and has stronger release/travel distortion;
- Unlimited Void has richer blue cosmic/nebula depth with reduced White Blood clutter;
- Cosmic Eye is larger and closer to the supplied visual language;
- Domain exit has a readable center-outward release/dissolve instead of an abrupt snap;
- protected systems remain intact;
- full regression passes;
- final subjective quality remains PENDING USER VISUAL REVIEW.

Do not commit or push until the user explicitly approves.
