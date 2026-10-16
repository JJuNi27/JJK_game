# Gojo second polish pass — completed automated validation

2026-09-09 KST. Continued the existing uncommitted implementation. No commit, push, merge, reset, checkout or clean. This report describes this polish pass, not all changes already present in the working tree. Visual approval remains with the user.

## Validation

- Unity 6000.3.20f1: **17 tests passed, 0 failed, 0 skipped**, including the new CombatMVP test. Final XML end time: `2026-09-08 15:23:58Z` (2026-09-09 00:23:58 KST).
- Runtime and Editor C# compilation passed. One existing CS0649 warning remains for the optional `ProductionSignatureVfxDirector.techniqueCastingPoint` socket. Final Unity log contains no C# errors, shader errors, NullReferenceException or MissingReferenceException.
- Existing Python tests: **13 passed** — 11 using the repository virtual environment's unittest runner, 2 Blender retarget tests using pytest.
- Tests ran against an isolated copy in `unity/Logs/PolishQA/Project`, because the user's actual project was open in Unity. Changed source files were byte-compared with that copy. No scene was saved over the user's VFXLab or CombatMVP scene. License configuration/files were not changed.
- All 11 prior protected file hashes match: VFXLab scene, Animator, five model/animation FBXs, locomotion controller, preview character, Blue gameplay field and technique controller. Walk 3 / Run 14 / Walk Time Scale 1.15 and evade were preserved. Domain-space and camera-lease architecture were preserved.
- The accepted purple tunnel's builder and trajectory code match the pre-polish snapshot exactly; its duration remains 0.72 seconds.
- `git diff --check` reports only pre-existing whitespace in the protected VFXLab scene; that scene was left untouched.

Evidence: `unity/Logs/PolishQA/unity-results.xml`, `unity-tests.log`, `validation-summary.json`, `CODEX_PREVIEW/index.html`. The gallery contains **31 PNGs at 1280×720**: 21 VFXLab captures and 10 CombatMVP captures.

The interrupted 16/17 run failed in the new CombatMVP fixture: a closure holding event counters was allocated before EnterPlayMode and lost during domain reload. The fixture now allocates it after the reload. A later forced compilation of every material pass stalled capture long enough to expire an unscaled Red effect; the excessive compilation step was removed. Captures now disable asynchronous compilation while rendering and check for shader errors. The initial pink Blue capture did not recur in the final captures. Polar capture positions were moved outside the imported character mesh. No failing tests remain.

## Techniques and Inspector controls

Shared Q/E/R art settings are in `Assets/Resources/VFX/GojoPolishSettings.asset`, used by both combat and VFXLab. Domain settings remain on `GojoDomainController` and `VfxLabPreviewSequence`.

| Technique | Final change and defaults | Verification |
|---|---|---|
| Blue preparation | Reworked curved decoration into straight inward compression streaks surrounding a shrinking dark kernel. `blueCompressionEnabled=true`, strength **0.7**. | Preparation captured in both scenes; interpretation and subtlety need the user's motion review. |
| Blue active | Darker core contrast; irregular lit mesh rubble has stronger visibility, **1.4×** scale, modest cyan emission and wider hero orbits. Existing runtime chunk count **24**, tunable 6–48. | Mesh rubble visible in the ordinary CombatMVP gameplay camera as well as closer inspection. Actual enemy receives **4 Blue hits**. Repeated cleanup passes. |
| Red | Stronger white-red feedback through the existing audio/feedback owner; flash **0.30**, impulse 0.58 for 0.24s; shock-ring scale **1.3** and thicker front. Added soft gray-white pressure vapor, density **32**, aftermath duration **0.95s**. Burst lifetime remains 0.26s; owned effect root lasts up to 1.21s. | Charge is created once and absent after release. Actual combat impact, live aftermath particles, natural expiry and repeated cancellation cleanup pass. Captures show pressure bloom and vapor. |
| Purple | Existing fusion retained, followed by a stationary **0.32s** completed-fusion hold. Same hold added to the existing damage schedule. Visual scale **1.65×**. Gameplay capsule radius **3.2m**, previously 2.2m, independently exposed on `GojoTechniqueChainController`. | Lab checks stationary hold then movement. Actual combat checks no damage during hold, a hit on an enemy outside the old corridor, and no hit on a farther outside target. |
| Purple path | Temporary wound width **4.5m**, varied along the path, with seams near alternating edges. Residue duration **1.8s** after travel. Lightning reach scales with the larger body. | Hold, travel lightning and broad scar captured in both scenes; repeated/natural cleanup passes. |

Purple range **18m**, merge **0.24s**, travel **0.78s**, damage and existing fusion materials/motion remain unchanged except for the explicitly requested hold and size. Damage still uses the existing scheduled capsule architecture and presentation-start slack; this pass does not replace it with a live projectile collision system. Visual size, damage radius and scar width can be tuned independently.

## Unlimited Void

The same-scene isolated interior remains at `(0,2048,0)`, radius 90. Capture radius remains 30m, independent of the exterior visual barrier. Per-participant transform/motor/rigidbody restoration, target selection, cancellation, death and scene-unload handling remain in the existing transaction. Tests cover multiple participants, duplicate colliders, fixed small/large visual barriers, repeated returns, cancellation, natural end, death, scene unload and camera pose/FOV restoration.

| Control | Default / meaning |
|---|---|
| Hand / face / eye / hero holds | **0.55 / 0.65 / 0.50 / 0.80s**; full detail sequence 2.50s, empty-domain sequence 1.35s |
| Barrier close / black transition | **0.55 / 0.20s** |
| Preserved purple tunnel | **0.72s** |
| Empty-void read / cloud interval / cloud bloom | **0.60 / 0.40 / 0.70s** |
| Victim camera / camera return blend | **1.20 / 0.65s** |
| `domainActiveDuration` | **10s of residence after arrival completes**, on gameplay controller and lab preview |
| `domainVictimStunDuration` | **16s from capture**, independently tunable, including the cinematic |
| Total presentation + residence | Default full arrival **5.87s + 10s = 15.87s**; no-victim lab arrival **4.72s + 10s = 14.72s**, plus the lab's existing 0.34s anticipation |
| `hideVisualFloor` | **true**; disables the visual suppression plane, retains the separate BoxCollider support floor |
| White liquid | Density **54**, scale **1.8**, distance depth **0.85**, new vertical spread **1.0** |
| Cosmic focal | Scale **2.2**, intensity **1.25** |

The old single `domainDuration` field was replaced by the new active-residence and victim-stun controls. The legacy value serialized in CombatMVP was not rewritten into the user's scene. Stun is measured from capture independently of residence; tuning it beyond the complete domain lifetime can intentionally leave some stun after return.

The central focal now uses `UnlimitedVoidIris.shader`: an opaque abyssal pupil with a broad pale blue/white/violet iris, warped radial filaments, slow swirl and fading outer glow. It replaces the thin stacked neon-like event-horizon arcs. The existing ink shader is unchanged in this pass.

White-liquid positions use an equal-area spherical distribution rather than a height-modulated horizontal belt, retaining near/middle/far layers. Eight horizontal views at 45° intervals plus zenith/nadir captures show surrounding white structures and no rendered floor clipping the lower structures. The lab's moving preview marker guides are hidden while inside the domain and return afterward. The collision floor remains unchanged and supports the real character controllers throughout the runtime tests.

The existing cinematic lease still yields to the lab's manual orbit control and returns the camera to its saved state. A small final correction enforces minimum detail-shot distance based on near clip and actor scale, reducing clipping on the small CombatMVP prototype avatar. It does not change lens settings, ownership or restoration. Full facial composition remains a manual review item.

## Capture scope and remaining manual review

Q/E/R/V previews were invoked through the same lab sequence and actual combat controller methods used by the scenes. These were automated calls, not manual keyboard playtesting. Combat uses the current `GojoPlayer`, `CurseBot` actors, colliders, presenters and actual damage. The selected bot was held stationary for deterministic Q/E/R checks, then enabled for the domain; centre/outside fixture targets verify Purple's enlarged corridor. Only test fixtures bypass energy cost for the direct ultimate/domain checks.

CombatMVP currently renders its existing procedural prototype Gojo avatar and capsule enemies. VFXLab uses the imported Gojo character. This pass does not replace the CombatMVP avatar. Its very tight facial shots therefore do not prove finished Gojo facial acting or composition.

URP camera render requests produced the PNGs. They demonstrate rendered geometry, color, positions and selected phases. They omit screen-space overlay UI/flash and do not establish continuous blackout transitions, audio quality, tactile camera impulse, animation acting, or subjective visual approval.

Manual Unity Play Mode review is still needed for Blue's compression meaning at combat distance, debris/core balance, Red's violent impact and vapor texture, Purple's hold/size/scar appearance, and the domain's slower mood, iris motion, white-liquid brightness/density, close-up framing and orientation loss while moving/orbiting. The white structures are still procedural quads with liquid masks; they are not simulated volumetric fluid. Current authored hand poses/Animator were preserved, including the lab's pocket-hand pose.

## Files changed in this second pass

Existing files changed (14; some were already untracked from the first pass):

- `unity/Assets/Scripts/Core/PrototypeCombatAudio.cs`
- `unity/Assets/Scripts/Dev/VFXLab/VfxLabController.cs`
- `unity/Assets/Scripts/Dev/VFXLab/VfxLabPreviewSequence.cs`
- `unity/Assets/Scripts/Player/BlueTornDebris.cs`
- `unity/Assets/Scripts/Player/DomainPresentationSettings.cs`
- `unity/Assets/Scripts/Player/DomainPresentationSession.cs`
- `unity/Assets/Scripts/Player/GojoDomainController.cs`
- `unity/Assets/Scripts/Player/GojoTechniqueChainController.cs`
- `unity/Assets/Scripts/Player/ProductionParticleVfxRuntime.cs`
- `unity/Assets/Scripts/Player/PrototypeHollowPurplePresentationRuntime.cs`
- `unity/Assets/Scripts/Player/PurpleTravelAftermath.cs`
- `unity/Assets/Scripts/Player/TechniqueChargeVisual.cs`
- `unity/Assets/Scripts/Player/UnlimitedVoidPrototypeVisual.cs`
- `unity/Assets/Editor/DomainPresentationRuntimeTests.cs`

New assets, each with a `.meta` file:

- `unity/Assets/Scripts/Player/GojoPolishSettings.cs`
- `unity/Assets/Resources/VFX/GojoPolishSettings.asset`
- `unity/Assets/Resources/VFX/RepulsiveVapor.shader`
- `unity/Assets/Resources/VFX/UnlimitedVoidIris.shader`

New report: `docs/GOJO_SECOND_POLISH_REVIEW.md`. Generated tests/captures and the isolated validation copy remain Git-ignored under `unity/Logs/PolishQA`.
