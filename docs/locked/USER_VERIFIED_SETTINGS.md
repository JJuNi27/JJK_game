# USER VERIFIED SETTINGS — DO NOT CHANGE WITHOUT EXPLICIT USER REQUEST

This document contains values and behavior the user directly verified in Unity / Blender.
These are stronger than automated-test-only results.

## Gojo locomotion

### Movement Profile
- Walk Speed: **3**
- Run Speed: **14**

### Animator Blend Tree
| Motion | Threshold | Time Scale |
|---|---:|---:|
| Gojo_Idle | 0 | 1.00 |
| Gojo_Walk | 3 | 1.15 |
| Gojo_Run | 14 | 1.00 |

### Input
- Normal movement: Walk
- Shift + movement: Run
- Space: Dodge / Evade

### USER VERIFIED transition flow
`Idle → Walk → Run → Walk → Idle`

## Protected animation assets
- Gojo_Idle
- Gojo_Walk
- Gojo_Run
- Gojo_Walk_Base (backup)

Do not alter locomotion or these accepted animation relationships for unrelated VFX work.

## Protected gameplay / presentation baselines
Unless a task explicitly requests otherwise, preserve:
- current Animator setup
- evade behavior
- Gojo Blue existing **4-hit gameplay**
- current successful Purple formation / fusion foundation
- current successful Unlimited Void purple-line tunnel
- current camera feedback ownership
- current Domain capture / barrier / isolated-interior / participant-restoration architecture

## Status vocabulary
Use these labels consistently:
- **USER VERIFIED**: user directly confirmed in Unity / Blender.
- **CODEX VALIDATED**: compile/test/automated preview passed, but user did not visually approve it.
- **PENDING VISUAL REVIEW**: technically implemented but still needs user Play Mode judgment.
