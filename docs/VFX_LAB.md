# VFXLab — Production VFX Preview Stage

작성 기준: 2026-09-03

## 목적

`VFXLab`은 Gate 5B production VFX를 전투 한 판을 시작하지 않고 반복 제작·검수하기 위한
developer-only presentation sandbox다.

```text
캐릭터 이동
+ prototype/future authored motion
+ 실제 production VFX
+ orbit camera
+ scene-owned lighting / Bloom
+ replay / loop / pause / slow motion
```

ParticleSystem만 따로 보는 viewer가 아니며 production 시작 scene 또는 CharacterSelect → CombatMVP
흐름의 일부가 아니다. Build Settings도 변경하지 않는다.

## Production VFX와의 관계

Blue renderer를 VFXLab용으로 복제하지 않는다.

```text
GojoBluePresentationPreset
        ↓ PresentationVfxSpawnRequest
ProductionParticleVfxRuntime
        ↓ PresentationVfxStyleId.GojoBlue
GojoBlueVfxInstance
        ├─ CombatMVP / BlueConvergenceField
        └─ VFXLab / VfxLabPreviewSequence
```

`GojoBluePresentationPreset`이 field와 impact request의 색, 반경 mapping, duration metadata,
style id를 한 곳에서 만든다. CombatMVP와 VFXLab은 동일 shader, shared material library,
particle, arc, temporary light, distortion-source hook 및 lifecycle 구현을 사용한다.

VFXLab은 어떤 presentation을 언제 보여줄지만 소유한다. Blue가 어떻게 렌더링되는지는 계속
production `GojoBlueVfxInstance`가 소유한다.

## Scene 열기

1. Unity Project 창에서 `Assets/Scenes/VFXLab.unity`를 연다.
2. 이 scene이 활성 scene인지 확인한다.
3. Play를 누른다.
4. Game view를 한 번 클릭해 키보드와 마우스 입력 focus를 준다.

Build Settings에 추가할 필요가 없고 기본 시작 scene으로 설정하지 않는다.

Play Mode hierarchy의 주요 scene-owned object:

```text
VFXLab
├─ PreviewStage
├─ PreviewCharacter
├─ VFXPreviewPoint
├─ ProductionVfxRuntime
├─ PreviewCamera
├─ Lighting
└─ VfxLabController
```

## Controls

| 입력 | 기능 |
|---|---|
| `WASD` | preview character 이동 |
| `1` | Blue full sequence |
| `2` | Blue field only |
| `3` | Blue impact/collapse only |
| `R` | 마지막 선택 preview 즉시 replay |
| `L` | loop on/off |
| `P` | preview pause/resume |
| `[` / `]` | playback speed 0.125x~2x 감소/증가 |
| `Backspace` | 현재 preview 즉시 clear |
| `RMB drag` | camera horizontal/vertical orbit |
| `Mouse wheel` | zoom in/out |
| `Home` | camera framing reset |

좌측 상단 developer IMGUI overlay는 selected preview, current phase, loop, speed, pause,
animation source와 control 요약을 표시한다. Production Combat HUD는 사용하지 않는다.

## Movement와 character presentation

현재 저장소에는 authored Gojo rig/Animator/AnimationClip이 없다. 기존 production/prototype movement와
`FighterAnimationStateSource`는 `Health`, `BasicAttack`, CE 및 combat action state에 연결되어 있으므로
VFXLab에 통째로 가져오지 않았다.

대신 `VfxLabPreviewCharacter`가 combat dependency 없는 `CharacterController` 이동과 Gojo 식별용
procedural fallback character를 제공한다. Idle/Move와 다음 technique motion 단계가 보인다.

```text
Idle → Anticipation → Cast → Release → Recover → Idle
```

이 fallback은 final animation이라는 뜻이 아니다. 실제 Animator가 추가되면 PreviewCharacter 아래에
Animator를 연결하고 다음 optional parameter를 제공할 수 있다.

```text
float PlanarSpeed
trigger TechniqueAnticipation
trigger TechniqueCast
trigger TechniqueRelease
trigger TechniqueRecover
```

Animator Controller와 parameter가 존재할 때만 hook을 호출하므로 없는 parameter warning을 만들지 않는다.
Gate 4E의 gameplay-owned state/cue 경계를 바꾸지 않으며 CombatMVP animation ownership도 변경하지 않는다.

## Blue preview 구성

Full preview는 gameplay cast가 아닌 VFXLab-local presentation sequence다.

```text
Anticipation
→ Cast
→ production Blue field
→ production Blue impact collapse
→ Recover
→ Immediate handle cleanup
```

Field radius `4.5`, field lifetime `0.95`, impact lifetime `0.28`은 현재 production request와 같은
presentation input을 사용한다. Full preview의 단계 배치와 loop wait만 developer preview sequencing이다.

Field와 impact 모두 VFXLab에서는 `Scaled` presentation time을 사용해 pause와 playback speed에
반응한다. CombatMVP의 field/impact time policy는 기존 `Scaled`/`Unscaled` 의미를 그대로 유지한다.

## Gameplay가 아닌 것

VFXLab에는 다음 component나 상태가 없다.

- enemy AI / Health / DamageReceiver
- match controller / victory / defeat / KO / team
- CE / cooldown / burnout
- hitbox / Physics overlap / damage
- pull / stun / Rigidbody suction target
- Target Lock / production HUD
- gameplay camera feedback / hit-stop director

`BlueConvergenceField`는 overlap, damage, pull, stun, gameplay lifetime을 소유하므로 VFXLab에서 실행하지
않는다. VFXLab은 presentation request만 production runtime에 보낸다.

## Replay와 lifecycle

새 preview, `R`, `Backspace`, loop restart 전에 현재 field/impact `PresentationVfxHandle`을
`Immediate`로 정리한다. 실제 instance 및 그 child ParticleSystem, runtime material, temporary Light,
`GojoBlueDistortionSource`는 scene-owned production lifecycle로 함께 파기된다.

VFXLab의 Global Volume profile, stage materials, marker materials와 preview character materials도
scene-owned이며 Play 종료/scene unload에서 정리된다. `Time.timeScale`과 `Time.fixedDeltaTime`은
VFXLab 진입 값을 캡처하고 component disable/destroy 때 복원한다.

## Stage / lighting / camera

- dark neutral floor와 background separation panel
- character/VFX 위치를 읽기 위한 낮은 밝기의 guide ring
- 현재 CombatMVP readability 기준에 가까운 ACES와 restrained Bloom
- cool key/fill + warm rim으로 character silhouette 분리
- horizontal orbit, 제한된 vertical orbit, zoom, reset
- pause 중에도 unscaled camera inspection 가능

stage 값은 VFXLab scene-owned다. `ProductionArenaMoodController`와 CombatMVP lighting object는 수정하지
않는다. Cinemachine과 VFX Graph도 추가하지 않는다.

## Future Red / Purple / Void 확장

1. production renderer에 이미 존재하는 style/instance를 유지한다.
2. CombatMVP producer에 흩어진 request tuning이 있으면 작은 presentation-only preset/factory로 추출한다.
3. `VfxLabPreviewSequence`에 preview kind와 순서만 추가한다.
4. developer code에 shader, particle, material 또는 effect instance 복제품을 만들지 않는다.
5. technique별 gameplay controller, damage actor, domain state를 VFXLab에 가져오지 않는다.

Timeline은 animation asset과 실제 clip timing 조정 이점이 생길 때 선택적으로 연결한다. 현재는 asset이
없고 짧은 Blue sequence이므로 작은 code state machine이 더 단순하며 Timeline을 억지로 추가하지 않았다.

## Unity 사용자 테스트 체크리스트

### VFXLab

- [ ] `VFXLab.unity`를 직접 열고 Play해도 match가 시작되지 않는다.
- [ ] enemy, HP, CE, cooldown, combat HUD가 없다.
- [ ] Gojo procedural preview character가 보이고 `WASD` 이동/방향 전환이 된다.
- [ ] Idle/Move fallback motion이 보인다.
- [ ] `1`에서 Anticipation → Cast → Field → Impact → Recover가 재생된다.
- [ ] `2`와 `3`으로 field-only / impact-only를 따로 볼 수 있다.
- [ ] `R` 연타 후 이전 effect가 누적되지 않는다.
- [ ] `L` loop가 여러 번 돌아도 effect, Light, material이 누적되지 않는다.
- [ ] `P`, `[`, `]`로 pause/slow motion/speed-up이 particle와 arc timing에 반영된다.
- [ ] pause 중에도 RMB orbit, wheel zoom, Home reset이 된다.
- [ ] `Backspace` 후 production Blue child, Light, distortion source가 남지 않는다.
- [ ] Hierarchy에서 production `GojoBlueVfxInstance` 경로가 사용된다.
- [ ] Bloom에서 deep-blue/electric-blue/cyan 계층이 white blob으로 날아가지 않는다.
- [ ] Console에 red error, shader error, MissingReference가 없다.

### CombatMVP regression

- [ ] 기존 Blue cast/distance/radius/duration/pulse가 동일하다.
- [ ] damage, pull, stun, CE, cooldown, hit timing이 동일하다.
- [ ] 기존 Blue visual이 동일 production preset/instance를 통해 보인다.
- [ ] Red / Hollow Purple / Unlimited Void에 regression이 없다.
- [ ] Pass 7 camera/FOV/focus/flash/hit-stop이 동일하다.
- [ ] CharacterSelect / team / KO / HUD flow가 동일하다.
- [ ] reload/return 후 Console error와 VFX 잔류가 없다.

## 검증 상태

정적 코드 검증과 Unity Editor compile 성공은 Play Mode visual/lifecycle 검증을 대신하지 않는다.

```text
VFXLab: REMOTE IMPLEMENTED / USER TEST PENDING
Blue Pass 8R-2A: REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
```
