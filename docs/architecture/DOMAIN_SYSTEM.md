# Unlimited Void / Domain System Architecture

## Core rule: three separate concepts
Never collapse these into one value or one physical volume.

### 1. Gameplay Capture Radius
Determines who is caught when Unlimited Void activates.
This is gameplay logic.

### 2. Visual Barrier Radius / Diameter
The black spherical barrier visible in the original world.
It is presentation-only and must remain independently tunable.
Changing barrier size must not change capture semantics.

### 3. Domain Interior Space
The Unlimited Void interior is a separate supernatural space / pocket-dimension experience.
It is not physically the same size as the exterior black barrier.

A 10 m barrier does **not** imply a 10 m interior room.

## Intended flow
`Original World`
→ Domain cast succeeds
→ Capture targets using Gameplay Capture Radius
→ Black barrier closes around Gojo + captured participants
→ outside world disappears / transition boundary
→ existing purple tunnel
→ participants enter Unlimited Void interior
→ Domain active state
→ Domain ends with a barrier-release / dissolve presentation
→ every participant restores to their own original-world state

## Current prototype strategy
Current implemented approach: **same-scene isolated far-away Domain Interior space**.
Reason: preserves existing references, CharacterController-based enemy movement, camera ownership, combat state, and avoids forcing an unnecessary Additive Scene rewrite.

Do not replace this strategy unless a real blocking defect requires it.

## Per-participant restoration
Restoration state must be stored per participant, never as one global transform.

Restore each participant to its own saved state on:
- normal completion
- cancellation
- early end
- death if relevant
- scene unload if relevant

Restoration must include the gameplay/control state required to resume normal combat correctly.

### Current USER-reported regression
After Unlimited Void ends in CombatMVP, normal basic melee / physical attacks can remain unavailable.

Important:
- intended cursed-technique burnout may remain if the current gameplay rules require it;
- **basic melee / physical combat must recover normally** after Domain exit;
- do not "fix" this by removing intended technique-burnout rules.

This is a real gameplay bug and has priority over purely visual polish.

## Current second-polish CODEX VALIDATED tuning
These are not immutable USER VERIFIED values.

- Domain arrival / cinematic sequence: **5.87 s**
- Domain interior dwell: **10 s**
- Victim stun / overwhelmed duration: **16 s**
- Empty-domain arrival sequence: **4.72 s**
- White-liquid clusters: **54**, spherical distribution including above/below
- Visible floor: hidden by default
- Collision / gameplay floor: retained

## Interior visual language — updated user direction
Preserve:
- visually hidden floor while collision remains
- 360° enclosure
- orientation ambiguity
- existing purple tunnel
- separate pocket-space feeling

Current issue:
The interior is too close to **dark empty space + too many bright White Blood splashes**.

Desired next direction:
- reduce White Blood dominance and visual clutter
- make the environment itself visually rich
- introduce a deeper blue / blue-white cosmic nebula field
- add subtle star / cosmic depth where appropriate
- use black abyss as the base, but not an empty black room
- White Blood remains an accent distributed through depth, not the main background
- no clearly readable normal floor / sky split

Desired read:
**an infinite blue-black cosmic void / nebula filled with impossible depth, with suspended white organic liquid splashes.**

## Cosmic Eye / black-hole focal
The focal landmark should be larger and more dominant than the current version.

Desired:
- large pitch-black pupil / abyss
- broad dreamlike iris / accretion structure
- pale blue, white, violet, and subtle spectral / iridescent layers
- multiple translucent flows at different speeds
- hypnotic / sublime / cosmic-eye impression
- not a small clean neon ring

Use local reference images from:
`docs/references/gojo/unlimited_void/`
when they are present and referenced by CURRENT_TASK.

## Domain exit presentation
Current return to the original world feels too abrupt.

Desired:
- do not instantly snap the environment away visually
- present the Domain/barrier as releasing or peeling away
- prefer a center-outward dissolve / reveal so the original world gradually returns
- keep gameplay restoration safe and deterministic
- presentation timing must not strand participants or camera state

This remains presentation, not barrier HP/destruction gameplay.

## Out of scope
Do not implement unless explicitly requested:
- barrier HP
- barrier damage / destruction
- outside ally rescue
- simultaneous interior/exterior combat
- attacks passing through the barrier
