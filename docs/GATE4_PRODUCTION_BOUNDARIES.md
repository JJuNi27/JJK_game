# Gate 4 — Prototype / Production Boundary Review

작성 기준일: 2026-08-23

## 상태

```text
Gate 4A: USER VERIFIED
Gate 4B: USER VERIFIED
Gate 4C: USER VERIFIED
Gate 4D: USER VERIFIED
Gate 4E: USER VERIFIED
Gate 4F: USER VERIFIED

Boundary Review: COMPLETE
Gate 4 Final Regression: PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

---

# 목적

Gate 4에서 만든 모든 코드를 무조건 production 시스템으로 승격하지 않는다.

다음 두 범주를 명확히 나눈다.

```text
PRODUCTION-CANDIDATE CONTRACT
- 다음 캐릭터/최종 자산이 재사용해야 할 경계

PROTOTYPE-ONLY IMPLEMENTATION
- 현재 검증용 표현/런타임
- Gate 5B에서 실제 자산 기반 구현으로 교체 가능
```

핵심 원칙:

```text
Contract는 재사용한다.
Prototype renderer/runtime은 필요하면 버린다.
Gameplay 고유 규칙은 캐릭터별로 유지한다.
```

---

# 1. Production-candidate contracts

## Character Presentation

```text
CharacterPresentationProfile
CharacterPresentationProfiles
CharacterSkillPresentation
```

소유:

```text
Character ID
Display / HUD / Short Name
Variant label
HUD / CE accent
Q/E/R/V presentation label + accent
```

소유하지 않음:

```text
Damage
CE amount
Cooldown
Domain gameplay rule
캐릭터 고유 술식 조건
```

## HUD Data

```text
PlayerCombatHudDataSource
PlayerCombatHudSnapshot
PlayerTeamMemberHudSnapshot
OpponentCombatHudDataSource
OpponentCombatHudSnapshot
OpponentTeamMemberHudSnapshot
```

원칙:

```text
HUD는 gameplay component를 직접 조작하지 않는다.
UI는 read-only snapshot을 소비한다.
```

현재 IMGUI renderer는 contract가 아니다.

## Technique Presentation

```text
TechniquePresentationRequest
TechniquePresentationId
TechniquePresentationPhase
TechniquePresentationRequests
```

Gameplay/observer는 다음 의미만 전달한다.

```text
Technique identity
Anticipation / Release / Impact / Active / End 계열 phase
Origin
Direction
Amplified 여부
```

Camera shake 수치, FOV, VFX renderer 같은 구현값은 producer가 소유하지 않는다.

## VFX Lifecycle

```text
PresentationVfxSpawnRequest
PresentationVfxTimePolicy
PresentationVfxStopMode
IPresentationVfxInstance
PresentationVfxHandle
IPresentationVfxRuntime
PresentationVfxRuntime
```

향후:

```text
Particle System
VFX Graph
Prefab
Object Pool
```

등으로 runtime 교체 가능해야 한다.

## Animation-facing contract

```text
FighterAnimationStateSnapshot
FighterAnimationCue
FighterAnimationCues
FighterAnimationStateSource
```

실제 Animator Controller의 parameter 이름/layer 구조는 실제 rigged asset이 들어오기 전까지 contract로 고정하지 않는다.

## Audio Event

```text
CombatAudioEventId
CombatAudioEvent
CombatAudioEvents
```

Gameplay는 다음을 모른다.

```text
AudioClip
AudioSource
Resources path
Mixer routing
FMOD/Wwise 여부
```

---

# 2. Prototype-only implementations

다음은 현재 플레이 검증에는 유효하지만 production 품질 자체가 아니다.

```text
GojoPrototypeAvatar
SukunaPrototypeAvatar
PrototypeFighterPresentationController의 procedural body pose
PrototypeBeautyArenaPresentation
현재 IMGUI HUD / F1 Help overlay
PrototypePresentationVfxRuntime
PrototypeSignatureSpatialVfx 계열 procedural LineRenderer 표현
PrototypeHollowPurplePresentationRuntime의 임시 renderer
UnlimitedVoidPrototypeVisual
SukunaMalevolentShrineVisual
SukunaDomainFugaBlast의 procedural visual
PrototypeCombatAudio fallback waveform / local Resources override runtime
PrototypeCombatAudioEventBridge
PrototypeCombatAudioEventSource
PrototypeSignatureTechniqueFeedbackController의 prototype observation 방식
```

이 코드를 Gate 5B에서 그대로 유지할 의무는 없다.

중요한 것은 이 구현들이 소비/발행하는 Gate 4 contract다.

---

# 3. Gameplay는 별도 유지

다음은 Gate 4에서 범용 Ability System으로 통합하지 않았다.

```text
Gojo Infinity rule
창 / 혁 / 허식 자 gameplay
무량공처 gameplay state
Sukuna 해 / 팔 / 푸가 조건
복마어주자 gameplay state
DomainSureHit
CE cost / regen
Damage / hit reaction
Tag / KO / reserve
Target Lock handoff
```

이것은 의도된 결정이다.

캐릭터 고유 규칙을 공통 Presentation contract와 혼동하지 않는다.

---

# 4. Known prototype debt

Gate 4 완료를 막지는 않지만 이후 단계에서 추적한다.

```text
Hollow Purple
- visual = 이동 구체
- gameplay damage = 기존 즉시 Capsule
- visual/damage timing 불일치

Character Art
- 실제 rigged Gojo/Sukuna model 없음
- production Animator/Animation Clip 없음

HUD
- IMGUI 기반
- F1 Help overlay가 일부 HUD를 가릴 수 있음

Environment
- prototype city silhouette / 비충돌 건물
- final art/material/destruction 없음

Audio
- fallback synthesized SFX 중심
- production Voice/SFX/Music 없음

Team prototype
- shared Fighter Shell 기반
- production character prefab/position lifecycle은 Gate 5A/5B에서 재검증
```

---

# 5. Gate 5A에서 검증할 질문

세 번째 캐릭터는 단순히 기술 하나를 추가하는 테스트가 아니다.

```text
1. CharacterPresentationProfile 추가만으로 identity/HUD가 붙는가?
2. HUD renderer 수정 없이 snapshot으로 새 캐릭터를 표현할 수 있는가?
3. 새 술식이 TechniquePresentationRequest를 발행해 camera/VFX consumer를 재사용할 수 있는가?
4. 새 VFX가 lifecycle runtime을 통해 생성/종료 가능한가?
5. Animation state/cue contract가 구조가 다른 Fighter에도 충분한가?
6. Audio Event를 추가해 gameplay가 clip/runtime을 몰라도 되는가?
7. Gojo/Sukuna 코드를 복사해야 하는 부분은 무엇인가?
8. 복붙이 발생했다면 그것은 고유 Gameplay인가, 빠진 공통 contract인가?
```

복붙이 고유 Gameplay라면 허용한다.
공통 Presentation/HUD/asset 연결을 복붙해야 한다면 Gate 4 contract를 보강한다.

---

# 6. Gate 5B에서 교체할 것

Gate 5A가 구조를 통과하면 다음을 production-quality slice로 교체한다.

```text
실제 rigged character model
Animator + production animation set
Particle/VFX Graph/prefab 기반 technique VFX
Canvas/TMP final-style HUD
production SFX / Voice / Music
대표 맵 art / material / lighting / post process
```

목표:

```text
"실제 주술회전 게임처럼 보이고 느껴지는" 첫 한 판
```

---

# 결론

Gate 4의 production 결과물은 현재 prototype renderer 자체가 아니라 다음 **제작 경계**다.

```text
Character Profile
HUD Snapshot
Technique Presentation Request
VFX Lifecycle
Animation State/Cue
Audio Event
```

이 경계를 Gate 5A 세 번째 캐릭터에서 실제로 스트레스 테스트한다.
