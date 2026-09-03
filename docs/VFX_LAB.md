# VFXLab · Character Presentation / Production VFX Workbench

작성 기준: 2026-09-03

## 목적

`VFXLab`은 프로젝트 전 캐릭터의 모델, 이동, animation hook, action motion과 production VFX를 전투 규칙 없이 빠르게 반복 검수하는 developer-only workbench다.

```text
Preview Character
  + movement / idle / locomotion
  + basic attack / dodge / technique / domain motion
  + procedural fallback 또는 future authored model + Animator
Production Presentation VFX
  + presentation-only local sequence
Camera Inspection
  + replay / loop / pause / speed / clear
```

이 scene은 gameplay simulation이 아니다. Enemy AI, match state, damage, hitbox, CE, cooldown, target lock, pull/push/stun을 만들거나 실행하지 않는다. CharacterSelect, production HUD, CombatMVP camera/lighting에도 관여하지 않는다.

현재 첫 profile은 Gojo이며, 캐릭터 선택 UI나 범용 ability framework는 아직 만들지 않는다.

## Scene 열기

1. Unity Project 창에서 `Assets/Scenes/VFXLab.unity`를 직접 연다.
2. 활성 scene이 `VFXLab`인지 확인한다.
3. Play 후 Game view를 한 번 클릭해 입력 focus를 준다.

Build Settings 변경이나 시작 scene 지정은 필요하지 않다.

주요 scene-owned hierarchy:

```text
VFXLab
├─ PreviewStage
├─ PreviewCharacter
│  └─ AuthoredModelRoot
├─ VFXPreviewPoint
├─ ProductionVfxRuntime
├─ PreviewCamera
├─ Lighting
└─ VfxLabController
```

## Controls

### Production-facing character controls

| 입력 | Preview action |
|---|---|
| `WASD` | Movement |
| `LMB` | 클릭마다 Basic Attack 1 → 2 → Finisher presentation |
| `Space` | Dodge presentation |
| `Q` | Cursed Technique Lapse: Blue |
| `E` | Cursed Technique Reversal: Red |
| `R` | Hollow Purple |
| `V` | Unlimited Void direct presentation |
| `X` | 현재 action/domain presentation cancel |

`V`를 한 번 누르면 짧은 `DomainAnticipation` motion 뒤 production `UnlimitedVoidProductionVisual`이 바로 실행된다. VFXLab에서는 RMB/LMB artisan timing을 요구하지 않으며 CE, domain gameplay state, radius query, release 성공 판정, stun 또는 physics overlap을 실행하지 않는다.

### Developer controls

| 입력 | 기능 |
|---|---|
| `Shift+R` | 마지막 선택 action replay |
| `L` | Loop on/off |
| `P` | Pause/resume |
| `[` / `]` | Playback speed 0.125x~2x |
| `Backspace` | 즉시 hard clear |
| `Shift+2` | Blue field-only debug |
| `Shift+3` | Blue impact-only debug |
| `MMB drag` | Camera orbit |
| `Mouse wheel` | Zoom |
| `Home` | Camera framing reset |
| `F1` | Compact HUD / expanded help 전환 |

`R`은 Purple 전용이며 replay가 점유하지 않는다. `RMB`는 예약 입력으로 비워 두고 camera orbit은 `MMB`가 소유한다. 기본 overlay는 Action/Phase와 핵심 기술 키만 보이는 compact HUD이며 `F1`로 전체 조작 도움말을 열고 닫는다.

## Supported Gojo preview actions

### Basic Attack

`LMB` 한 번은 한 step만 실행한다. 첫 클릭은 `BasicAttack1`, 다음 클릭은 `BasicAttack2`, 세 번째 클릭은 `BasicAttackFinisher` motion과 해당 production BasicHit renderer를 보여 준다. 다음 입력 없이 0.9초가 지나면 다시 step 1로 reset한다. 한 클릭으로 다음 step을 자동 실행하지 않는다.

실제 `BasicAttack` gameplay component는 실행하지 않으므로 Physics overlap, damage, knockback, hit stun 또는 gameplay combo state가 없다.

### Dodge

`Space` 입력 시 현재 WASD 방향, 입력이 없으면 캐릭터 forward를 사용해 짧은 presentation-only 이동과 body pose를 보여 준다. Invincibility, stamina, collision immunity 또는 combat state는 없다.

### Blue

`Q`는 기존 full sequence를 실행한다.

```text
Anticipation → Cast → production Blue field → impact collapse → Recover
```

각 시작/replay 시 캐릭터의 현재 position/forward로 anchor를 다시 계산하고, 재생 중에는 world anchor를 고정한다. `Shift+2`, `Shift+3`으로 field/impact를 개별 검수할 수 있다.

### Red

`E`는 VFXLab-local anticipation/cast/travel/impact/recover 순서에서 production `GojoRedPresentationPreset` request와 `GojoRedVfxInstance` renderer를 실행한다. CombatMVP와 VFXLab은 `GojoRedProductionDefaults`의 range/speed/radius 및 같은 preset을 공유하므로 preview anchor도 canonical 11m를 22m/s로 이동한다. Projectile gameplay actor는 생성하지 않는다.

### Hollow Purple

`R`은 readiness, CE, cooldown 없이 `PrototypeHollowPurplePresentationRuntime.OrbSequence`의 canonical presentation을 직접 실행한다. CombatMVP와 VFXLab 모두 동일한 factory/type을 사용하므로 Blue orb + Red orb merge 뒤 동일한 Purple orb가 launch된다. canonical `MergeDuration=0.24`, `LaunchDuration=0.78`, `TravelDistance=18`은 한 구현에만 존재하며 VFXLab이 복제하지 않는다. Damage capsule은 실행하지 않는다.

### Unlimited Void

`V` 한 번은 0.34초 character anticipation motion 뒤 추가 anticipation particle 없이 production `UnlimitedVoidProductionVisual`을 preview-owned host에서 실행한다. Host는 cancel/replay/complete 시 제거된다. Domain gameplay component, artisan input, radius query, stun, duration state, burnout은 실행하지 않는다.

## Production presentation 재사용

VFXLab 전용 Blue/Red/Purple renderer 또는 Purple recreation은 없다.

```text
VfxLabPreviewSequence
  └─ 무엇을 / 언제 / 어디에서 보여 줄지만 결정
       ↓
PresentationVfxRuntime
       ↓
ProductionParticleVfxRuntime
       ├─ GojoBlueVfxInstance
       ├─ GojoRedPresentationPreset → GojoRedVfxInstance
       └─ ProductionParticleVfxInstance
            └─ BasicHit styles

Canonical Hollow Purple
  └─ PrototypeHollowPurplePresentationRuntime.OrbSequence
       ├─ CombatMVP
       └─ VFXLab

Unlimited Void
  └─ UnlimitedVoidProductionVisual
```

Blue는 `GojoBluePresentationPreset`을 그대로 사용하고 Red는 CombatMVP와 `GojoRedPresentationPreset`을 공유한다. 모든 short-lived preview는 `PresentationVfxHandle`을 보관하고 cancel/replay/loop 전 `Immediate`로 정리한다. Canonical Purple sequence, Red 이동용 임시 anchor와 Domain host도 VFXLab이 명시적으로 제거한다.

`TechniquePresentationRequest`는 production gameplay owner(`Health`)를 기준으로 camera/animation consumer에 전달된다. VFXLab에는 gameplay owner를 만들지 않으므로 해당 event를 위조하지 않고 renderer-facing production runtime을 직접 호출한다.

## Character model / Animator hook

`PreviewCharacter/AuthoredModelRoot` 아래에 future character prefab 또는 model을 넣는다. Renderer 또는 Animator가 발견되면 `PreviewGojoFallback`은 생성되지 않거나 비활성화된다. Authored model이 없으면 현재 procedural Gojo가 자동으로 유지된다.

Inspector의 `Authored Model Root`, `Animator`와 parameter 이름은 교체 가능하다. 실제 Animator Controller가 있고 해당 parameter가 존재할 때만 값을 쓰므로 없는 parameter warning을 만들지 않는다.

지원 hook:

```text
float   PlanarSpeed
trigger Idle
trigger BasicAttack1
trigger BasicAttack2
trigger BasicAttackFinisher
trigger Dodge
trigger TechniqueAnticipation
trigger TechniqueCast
trigger TechniqueRelease
trigger TechniqueRecover
trigger DomainAnticipation
trigger DomainRelease
```

이 hook은 VFXLab-local adapter이며 Gate 4E의 `FighterAnimationStateSource`, `FighterAnimationCue`, CombatMVP animation ownership을 변경하지 않는다.

## Stage / lighting / camera

- 64×64 neutral cool-gray 단일 floor
- wall/backdrop geometry 없음
- fog 없음
- neutral gray camera background
- 전 구역을 커버하는 inspection directional key/fill
- Neutral tonemapping과 약한 Bloom
- subtle Stage/Character/Blue guide
- MMB orbit, wheel zoom, Home reset

Stage/lighting 값은 VFXLab scene-owned이며 `ProductionArenaMoodController`와 CombatMVP lighting을 변경하지 않는다.

## Unity 사용자 테스트 순서

1. `VFXLab.unity`를 열고 Play한다. Console red error/MissingReference가 없는지 확인한다.
2. `WASD` 이동 후 procedural idle/locomotion과 camera follow를 확인한다.
3. `LMB`를 세 번 나눠 눌러 각 클릭이 step 1, 2, finisher 하나만 실행하는지 확인한다. 0.9초 이상 기다린 뒤 다음 클릭이 step 1인지 확인하고 `Space` dodge도 확인한다.
4. 이동/회전 후 `Q`, `E`, `R`을 각각 실행하고 캐릭터 motion, anchor, production renderer를 확인한다.
5. `R`에서 Blue/Red orb merge 후 canonical Purple orb가 18m launch되는지 확인한다.
6. `V`를 한 번 눌러 짧은 anticipation 뒤 Unlimited Void가 바로 활성화되고 추가 RMB/LMB 입력을 요구하지 않는지 확인한다.
7. `Shift+2`, `Shift+3`으로 Blue field/impact debug를 확인한다.
8. 각 action에서 `Shift+R`, `L`, `P`, `[`, `]`, `X`, `Backspace`를 확인한다.
9. replay/loop/cancel 후 effect, Light, travel anchor, Domain host가 누적되지 않는지 Hierarchy에서 확인한다.
10. 기본 compact overlay가 시야를 가리지 않는지, `F1`로 expanded help를 열고 닫는지, `AuthoredModelRoot` hook이 유지되는지 확인한다.

## 검증 상태

```text
VFXLab Generalized Workbench
REMOTE IMPLEMENTED / USER TEST PENDING
```

정적 compile 성공은 Unity Play Mode visual/input/lifecycle 또는 Console 검증을 대신하지 않는다.
