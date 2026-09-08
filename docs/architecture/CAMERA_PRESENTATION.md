# Camera Presentation / Domain Cinematic Ownership

## Ownership principle
Do not fight the normal combat camera every frame.
Use a temporary cinematic override / shot-director layer.

`Normal Combat Camera`
→ `Domain Cinematic Override`
→ temporary shots
→ complete / cancel
→ exact restore of normal camera state

## Existing presentation ownership to preserve
The project already owns presentation effects such as:
- shake
- FOV
- focus
- flash
- hit-stop-related feedback

Do not duplicate these systems unnecessarily.

## Restore requirements
On completion or interruption, restore:
- FOV
- follow target
- camera target
- camera mode/state
- any temporary transform changes

Avoid permanent transform drift.

## Full cinematic condition
Full victim-focused cinematic only when at least one valid enemy is captured.
No enemy → shorter Gojo/domain-only sequence.

## Primary cinematic victim
Deterministic priority:
1. existing lock-on target if captured and valid
2. nearest captured opponent
3. stable deterministic fallback

Selection is presentation-only. Gameplay still affects all captured enemies.

## Intended shot language
- Hand / seal detail
- Blindfold / side-face close-up hook
- Six Eyes extreme close-up hook
- Gojo hero front shot
- Barrier closure wide
- existing Purple tunnel abduction
- victim reaction close-up
- Domain arrival / reveal

Do not fake destructive blindfold mesh animation if authored animation does not exist.
If facial rigging is unavailable, use stunned pose / head orientation / camera framing instead of pretending facial animation exists.

## Tone
Unlimited Void camera language should be:
- calm
- ominous
- overwhelming
- unhurried

Avoid frantic hyper-cut pacing.
Final timing should be revisited when authored blindfold / Six Eyes / domain-seal animation and voice are available.

## Domain exit
If a center-outward Domain dissolve / barrier-release presentation is used:
- camera ownership must remain stable during the dissolve;
- restore combat camera only at a safe handoff point;
- never leave the camera in the isolated interior after participant restoration.

## VFXLab safety
VFXLab manual/orbit camera controls must survive preview use.
Repeated cinematic previews must restore the preview camera correctly.
