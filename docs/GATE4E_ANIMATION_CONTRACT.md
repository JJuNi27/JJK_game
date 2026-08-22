# Gate 4E — Animation Contract

작성 기준일: 2026-08-23

## 상태

```text
Gate 4A Character Presentation Contract: USER VERIFIED
Gate 4B HUD Data Binding Extraction: USER VERIFIED
Gate 4C Technique Presentation Request: USER VERIFIED
Gate 4D VFX Lifecycle Contract: USER VERIFIED
Gate 4E Pass 1 · Animation State/Cue Boundary: USER VERIFIED

Gate 4E Animation Contract: USER VERIFIED
NEXT: Gate 4F Audio Event Contract
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 목적

현재 저장소에는 실제 rigged Gojo/Sukuna 모델과 production Animator/Animation Clip이 아직 없다.

따라서 Gate 4E에서 임의의 Animator Controller를 만들어 최종 구조인 것처럼 굳히지 않는다.

대신 현재 procedural prototype pose가 직접 읽던 Gameplay 구현을 `animation-facing contract` 뒤로 옮겼다.

```text
Gameplay Components
→ FighterAnimationStateSource
→ FighterAnimationStateSnapshot / FighterAnimationCue
→ Prototype pose renderer 또는 미래 Animator adapter
```

---

# Pass 1 — Fighter Animation Contract · USER VERIFIED

새 파일:

```text
unity/Assets/Scripts/Core/FighterAnimationContract.cs
unity/Assets/Scripts/Player/FighterAnimationStateSource.cs
```

## Continuous State

`FighterAnimationStateSnapshot`이 제공한다.

```text
Character ID
Planar speed
Dodge active
Dodge progress
Dodge direction
Basic attack step
CombatActionState
KO state
```

이 값은 presentation이 읽기 위한 상태이며 Gameplay 값을 소유하거나 수정하지 않는다.

## Discrete Cue

`FighterAnimationCue` / `FighterAnimationCues`를 추가했다.

```text
CharacterEntered
DodgeStarted
BasicAttackStarted (variant = 1/2/3)
TechniquePhase
Knockout
```

Technique cue는 Gate 4C의 동일한 `TechniquePresentationId / TechniquePresentationPhase`를 재사용한다.

---

# Prototype Fighter Presentation Migration

변경:

```text
unity/Assets/Scripts/Player/PrototypeFighterPresentationController.cs
```

기존 direct Gameplay 참조를 제거하고 `FighterAnimationStateSource → FighterAnimationStateSnapshot`만 읽도록 변경했다.

현재 procedural pose는 그대로 유지한다.

```text
Gojo/Sukuna locomotion stance
Dodge pose
Basic Attack 1/2/3 pose
Tag/character entry pulse
```

캐릭터 판단도 prototype child name 문자열 추론 대신 `CharacterId` 기준으로 바뀌었다.

현재 child transform 이름/LeftArm/RightArm 접근은 prototype renderer 전용이므로 유지한다. 실제 rigged model이 들어오면 이 consumer 자체를 Animator adapter로 교체한다.

---

# 사용자 검증

2026-08-23 사용자가 다음 범위를 확인했다.

```text
고죠 idle/movement
기본공격 1/2/3 pose
Dodge pose
T Tag → 스쿠나 entry/visual 전환
스쿠나 movement/basic/dodge
다시 고죠 Tag
허식 자/푸가/영역 presentation 회귀 없음
Console 오류 없음
```

사용자 결과:

```text
정상
```

따라서:

```text
Gate 4E Pass 1: USER VERIFIED
Gate 4E Animation Contract: USER VERIFIED
```

실제 Animator/Animation Clip 자산이 저장소에 없는 현재 단계에서는 추가 가짜 Animator 구현을 만들지 않고 Gate 4F로 이동한다.

---

# 바꾸지 않은 Gameplay

```text
Movement speed
Dodge speed/duration/cooldown/invulnerability
Basic attack damage/timing/combo
Technique input/cost/cooldown
Domain rules
Tag / KO / target handoff
Camera / HitStop
VFX
Audio
```
