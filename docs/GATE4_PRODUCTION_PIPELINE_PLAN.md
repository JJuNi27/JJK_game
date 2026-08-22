# Gate 4 — Production Pipeline Extraction Plan

작성 기준일: 2026-08-23

## 상태

```text
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED
Gate 4 Production Pipeline Extraction: STARTED
Gate 4A Character Presentation Contract · Pass 1: REMOTE IMPLEMENTED / USER TEST PENDING
```

2026-08-23 Gate 3 최종 회귀를 사용자가 `다 정상!`으로 확인하여 Gate 4 실행 단계로 전환했다.

## 목적

Gate 1~3에서는 빠르게 실제 플레이를 검증하기 위해 Character/Presentation 기능이 여러 Prototype Controller에 분산되어 있다.

Gate 4의 목적은 단순한 코드 미관 개선이 아니라:

```text
세 번째 캐릭터를 추가할 때
고죠/스쿠나 코드를 복사하지 않고도
캐릭터 고유 규칙은 유지하면서
공통 제작 절차를 반복 가능하게 만드는 것
```

이다.

## 핵심 원칙

```text
1. 실제로 두 번 이상 반복된 것만 공통화
2. 캐릭터 고유 Gameplay Rule은 억지로 범용화하지 않음
3. Data와 Runtime 책임 분리
4. Presentation 요청은 Gameplay 결과를 바꾸지 않음
5. 한 번에 전체 재작성하지 않고 단계별 Migration
6. 각 Migration 뒤 Gojo/Sukuna 회귀 검증
```

---

# 4A — Character Presentation Contract · PASS 1 IMPLEMENTED

새 공통 데이터 파일:

```text
unity/Assets/Scripts/Player/CharacterPresentationProfile.cs
```

현재 한 곳에서 제공하는 정보:

```text
Character ID
Display Name
HUD Name
Short Name
Variant Label
Compact Variant Label
HUD Accent
CE Accent
Q / E / R / V Skill Label
Q / E / R / V Presentation Accent
```

현재 Profile 등록:

```text
GojoModern
SukunaShibuyaYujiBody
```

이 Profile에는 의도적으로 다음을 넣지 않는다.

```text
HP / CE 수치
Damage
Cooldown
Cast Time
Domain Rule
Character-specific Gameplay Rule
```

즉 `Presentation identity data`만 분리한다.

## Pass 1 Migration

다음 소비자를 Profile 기반으로 이관했다.

```text
PrototypeCharacterController
- DisplayName
- character switch chip identity/accent
- Sukuna player/domain/help presentation labels

PrototypePlayerTeamController
- Active fighter accent
- CE accent
- Active/Reserve short name
- fighter display/variant label
- 기존 CharacterAccent / CharacterShortName / CharacterEraLabel 중복 제거

PrototypeSkillDeckHud
- Gojo/Sukuna 분기형 하드코딩 skill label/color 제거
- Active CharacterPresentationProfile에서 Q/E/R/V label/accent 읽음
- Tag 시 Profile만 바뀌면 Skill Deck identity도 자동 변경
```

상세:

```text
docs/GATE4A_CHARACTER_PRESENTATION_CONTRACT.md
```

## Asset Hook 보류

Portrait / Icon / model prefab / Animator reference는 이번 Pass에 억지로 넣지 않는다.

현재 저장소에는 실제 rigged Gojo/Sukuna model/Animator asset이 없기 때문에, 존재하지 않는 asset contract를 추측해 고정하면 오히려 재작업이 커진다.

실제 asset이 들어오는 시점에 Profile을 ScriptableObject 또는 asset-backed registry로 확장하는 방향을 검토한다.

## Pass 1 성공 조건

```text
[ ] Unity compile error 없음
[ ] Gojo Active HUD 이름/색상 동일
[ ] Sukuna Tag 후 이름/색상 동일
[ ] Skill Deck: Gojo Q 창 / E 혁 / R 허식 자 / V 무량공처
[ ] Skill Deck: Sukuna Q 해 / E 팔 / R 푸가 / V 복마어주자
[ ] Skill pulse / availability / state label 정상
[ ] Tag HP / CE 보존 정상
[ ] Gojo/Sukuna gameplay 변화 없음
```

통과하면 4A Pass 1을 USER VERIFIED로 닫고 남은 Presentation identity 하드코딩을 조사한 뒤 4A를 마무리한다.

---

# 4B — HUD Data Binding Extraction

현재 Prototype HUD:

```text
MatchController OnGUI
PrototypePlayerTeamController OnGUI
PrototypeOpponentTeamController OnGUI
PrototypeSkillDeckHud OnGUI
```

먼저 정보 공급 계약을 분리한다.

```text
Active Fighter Identity
HP / CE
Reserve State
Tag State
Opponent Team State
Skill Slot Labels
Skill Availability
Combat/Domain/Burnout State
```

Canvas/TMP 최종 HUD 교체는 이 데이터 계약 위에 얹는다.

---

# 4C — Technique Presentation Request

Gate 3에서 반복된 Presentation:

```text
Flash
Camera Shake
FOV Kick
World Focus
Hit Stop
Spatial Burst/Ring
Projectile Release cue
Impact cue
Domain Anticipation / Activation cue
Voice / SFX hook
```

목표는 Gameplay Controller가 Camera/VFX 구현 세부사항을 직접 알지 않도록 Presentation 요청 단위를 만드는 것이다.

개념 후보:

```text
TechniquePresentationRequest
- fighter
- technique id
- phase: Anticipation / Release / Impact / Active / End
- optional world point / direction
- intensity profile
```

실제 타입명은 코드 조사 후 결정한다.

---

# 4D — VFX Lifecycle Contract

```text
Spawn
Follow caster/projectile/world point
scaled/unscaled timing policy
Stop/Fade
Destroy
```

Prototype LineRenderer 구현을 실제 VFX Graph/Particle Prefab으로 교체해도 Gameplay 변경이 최소화되어야 한다.

---

# 4E — Animation Contract

실제 rigged model/animation asset용 공통 이벤트 후보:

```text
Locomotion
Dodge
Basic Attack 1/2/3
Technique Anticipation
Technique Release
Technique Recovery
Domain Start
Domain Active
Tag Out / Tag In
Hit / KO
```

현재 procedural avatar pose를 최종 Animator 구조로 그대로 확장하지 않는다.

---

# 4F — Audio Event Contract

Production 목표:

```text
Voice Event
Technique SFX Event
Hit SFX Event
Domain SFX Event
Match Result Event
```

AudioClip 교체가 Gameplay Controller 수정으로 이어지지 않게 한다.

---

# Migration / Regression 순서

```text
4A Character Presentation Data
→ Gojo/Sukuna identity/HUD regression

4B HUD Binding
→ Team/Tag/KO regression

4C Camera/Technique Presentation Request
→ Purple/Fuga/Domain regression

4D VFX Lifecycle
→ Signature/Hit VFX regression

4E Animation Contract
→ 실제 모델 자산 준비 상태에 맞춰 진행

4F Audio Contract
→ 실제 Audio 자산 준비 상태에 맞춰 진행
```

각 단계에서 기존 Prototype Gameplay 수치를 유지한다.

---

# Codex / Agent 사용 포인트

Gate 4는 여러 파일에 흩어진 중복 탐색과 migration이 많아 agent 활용 효율이 높다.

적합:

```text
Character 문자열/색상/Skill label 중복 탐색
Presentation 호출 위치 전체 검색
Profile/Interface 도입 다중 파일 Migration
오래된 Prototype 경로/미사용 코드 탐색
테스트/회귀 경로 조사
Diff 단위 코드 리뷰
```

원칙:

```text
Agent multi-file work
→ Git diff review
→ 작은 migration 단위 적용
→ 사용자 Unity 검증
```

이 ChatGPT 대화에 별도 로컬 Codex/Unity Editor 제어 도구가 연결되지 않았다면 실행했다고 가장하지 않는다.

---

# Gate 4 완료 조건

```text
[ ] 새 Fighter identity/HUD data를 기존 HUD 코드 복사 없이 등록 가능
[ ] 기존 Gojo/Sukuna Gameplay Controller 정상
[ ] Technique Presentation 공통 요청 구조
[ ] Camera/Hit Stop/VFX/SFX 재사용 경로 명확
[ ] Tag/KO/Team HUD Fighter 하드코딩 축소
[ ] 실제 Animator/Model 연결 contract 존재
[ ] Prototype-only와 Production candidate 코드 구분
```

통과 후 Gate 5 Third Character Stress Test로 간다.
