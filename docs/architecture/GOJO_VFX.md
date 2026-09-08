# Gojo VFX Architecture / Current Baseline

This file records the current accepted baseline so future prompts do not need to repeat the full history.

## Global visual philosophy
This project intentionally values spectacle, aura, strong hit feel, and readable exaggerated VFX.
Do not be overly conservative merely because a VFX layer looks intense in isolation.
The user has explicitly approved pushing the presentation harder than a cautious default, provided the character and gameplay remain readable.

---

## Blue / Ao

### Preserve
- existing 4-hit gameplay
- current attraction / singularity direction
- active debris concept

### Current baseline
- pre-cast decoration was previously reworked toward inward compression
- active Blue uses irregular debris around the singularity

### Current USER feedback / next refinement
- the remaining small blue pre-cast effects in front of Gojo still have no clear meaning to the user
- remove those small ambiguous pre-cast decorations unless they are strictly necessary for the effect to function
- debris still reads too much like bright ice
- shift large debris toward darker, rougher rock / broken-environment fragments
- slightly reduce giant hero-chunk dominance
- add more medium/small debris variation
- keep the core visible
- make inward collapse / suction more obvious

Desired read:
**the surrounding environment is being ripped apart and pulled into Blue.**

---

## Red / Aka

### Preserve
- charge created once
- charge removed on release
- repeated casts must not leak stale charge objects
- repulsion identity; do not turn Red into a generic fireball

### Current CODEX VALIDATED baseline
- stronger flash / shock ring
- gray-white residual aftermath around **0.95 s**

### Current USER feedback / next refinement
- projectile feels too slow
- range feels too short
- increase projectile travel speed and effective range while keeping gameplay controllable
- current aftermath should not look like round smoke puffs
- use pressure-displaced air / dust / vapor / residue tearing outward from the impact center
- integrate flash + shockwave + outward pressure residue into one stronger `BANG`
- strong spectacle is welcome

Desired read:
**compressed repulsion releases, space rejects everything outward, and the pressure leaves a visible aftermath.**

---

## Hollow Purple

### Preserve aggressively
Current formation / Blue+Red fusion foundation is one of the strongest effects.
Do not broadly redesign it.

### Current CODEX VALIDATED tuning
- Fusion-complete hold: **0.32 s**
- Visual size: **1.65×** previous scale
- Gameplay hit radius: **3.2 m**
- Residual scar width: **4.5 m**
- Residual lifetime: **1.8 s**

### Current USER feedback / next refinement
- range is still too short; it feels like Purple launches and ends too soon
- increase travel range / lifetime enough for Purple to feel like a true long-reaching ultimate
- completed Purple currently obscures too much of Gojo; adjust spawn/hold offset or framing so Gojo remains readable before release
- release should feel more violent and premium
- increase irregular violet lightning / branching arcs around the projectile
- strengthen spatial distortion / lensing / warped-space wake during travel
- retain the larger projectile and gameplay-relevant size

### Minor optional scar polish
- scar can look too flat / carpet-like
- make scar edges more irregular
- add breakup / residue variation if cheap
- do not broadly rebuild the scar system

### Hold timing
Current hold is 0.32 s.
Do not change automatically unless the task requests it; user may later compare against roughly 0.45–0.60 s.

Desired read:
**Purple does not merely travel through the map; it violently deforms and wounds the space it crosses.**
