# Gate 4E — Animation Contract

작성 기준일: 2026-08-23

## 상태

```text
Gate 4A Character Presentation Contract: USER VERIFIED
Gate 4B HUD Data Binding Extraction: USER VERIFIED
Gate 4C Technique Presentation Request: USER VERIFIED
Gate 4D VFX Lifecycle Contract: USER VERIFIED
Gate 4E Pass 1 · Animation State/Cue Boundary: REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 목적

현재 저장소에는 실제 rigged Gojo/Sukuna 모델과 production Animator/Animation Clip이 아직 없다.

따라서 Gate 4E에서 임의의 Animator Controller를 만들어 최종 구조인 것처럼 굳히지 않는다.

대신 현재 procedural prototype pose가 직접 읽던 Gameplay 구현을 먼저 `animation-facing contract` 뒤로 옮긴다.

기존:

```text
PrototypeFighterPresentationController
→ BasicAttack 직접 읽음
→ ThirdPersonPlayerController 직접 읽음
→ CharacterController velocity 직접 읽음
→ prototype child name으로 캐릭터 추론
```

목표:

```text
Gameplay Components
→ FighterAnimationStateSource
→ FighterAnimationStateSnapshot / FighterAnimationCue
→ Prototype pose renderer 또는 미래 Animator adapter
```

---

# Pass 1 — Fighter Animation Contract

새 파일:

```text
unity/Assets/Scripts/Core/FighterAnimationContract.cs
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

현재 semantic cue:

```text
CharacterEntered
DodgeStarted
BasicAttackStarted (variant = 1/2/3)
TechniquePhase
Knockout
```

Technique cue는 Gate 4C의 동일한:

```text
TechniquePresentationId
TechniquePresentationPhase
```

를 재사용한다.

즉 Animation만을 위해 허식 자/푸가/영역 이름 체계를 따로 복제하지 않는다.

---

# FighterAnimationStateSource

새 파일:

```text
unity/Assets/Scripts/Player/FighterAnimationStateSource.cs
```

이 컴포넌트가 기존 Gameplay Component를 읽어 animation-facing 상태로 변환한다.

읽는 대상:

```text
Health
BasicAttack
ThirdPersonPlayerController
CharacterController
PrototypeCharacterController
CombatActionGate
TechniquePresentationRequests
```

하지만 다음은 하지 않는다.

```text
이동 실행
공격 실행
회피 실행
데미지 적용
Tag 실행
Technique 실행
Animator parameter 직접 조작
```

따라서 실제 Animator adapter가 들어와도 Gameplay ownership은 이동하지 않는다.

---

# Prototype Fighter Presentation Migration

변경:

```text
unity/Assets/Scripts/Player/PrototypeFighterPresentationController.cs
```

기존 direct Gameplay 참조를 제거하고:

```text
FighterAnimationStateSource
→ FighterAnimationStateSnapshot
```

만 읽도록 변경했다.

현재 procedural pose는 그대로 유지한다.

```text
Gojo/Sukuna locomotion stance
Dodge pose
Basic Attack 1/2/3 pose
Tag/character entry pulse
```

다만 이제 캐릭터 판단도 prototype child name 문자열이 아니라 `CharacterId`를 기준으로 한다.

현재 child transform 이름/LeftArm/RightArm 접근은 명백히 prototype renderer 전용이므로 남겨 두었다. 실제 rigged model이 들어오면 이 consumer 자체를 Animator adapter로 교체한다.

---

# Production Animator가 들어왔을 때의 연결 방향

현재 확보된 경계로 다음 구현을 추가할 수 있다.

```text
FighterAnimationStateSource
   ├→ Continuous State
   │   ├ speed
   │   ├ dodge progress
   │   ├ action state
   │   └ dead
   │
   └→ FighterAnimationCue
       ├ basic 1/2/3
       ├ technique anticipation/release/active/end
       ├ domain phase
       ├ character entry
       └ knockout

→ Animator Adapter
→ Character-specific Animator Controller / clips
```

Clip 이름, Animator parameter hash, layer 구조, Avatar Mask는 실제 asset이 들어오기 전에는 확정하지 않는다.

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

---

# Pass 1 사용자 테스트

```text
1. cd D:\GitHub\JJK_game
2. git pull origin master
3. Unity compile 완료 대기
4. Console 빨간 오류 확인
5. CombatMVP Play
```

빠른 확인:

```text
[ ] 고죠 idle/movement 몸 기울기 기존처럼 정상
[ ] 고죠 기본 1/2/3타 procedural pose 정상
[ ] 고죠 Dodge pose 정상
[ ] T → 스쿠나 시 entry pulse + visual 전환 정상
[ ] 스쿠나 movement stance 정상
[ ] 스쿠나 기본 1/2/3타 pose 정상
[ ] 스쿠나 Dodge pose 정상
[ ] 다시 T → 고죠 정상
[ ] HP/CE/Tag/공격/회피 Gameplay 회귀 없음
[ ] 허식 자/푸가/영역 presentation 회귀 없음
[ ] Console 빨간 오류 없음
```

통과하면:

```text
Gate 4E Pass 1: USER VERIFIED
```

그 뒤 실제 animation asset이 없는 현재 상황에서 추가 Animator 구현을 억지로 만들지 판단하고, 충분하면 Gate 4E를 닫아 Gate 4F Audio Event Contract로 넘어간다.
