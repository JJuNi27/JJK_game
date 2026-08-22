# Gate 4 — Production Pipeline Extraction Plan

작성 기준일: 2026-08-23

## 상태

```text
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED
Gate 4 Production Pipeline Extraction: STARTING
NEXT: 4A Character Presentation Contract
```

2026-08-23 Gate 3 최종 회귀를 사용자가 `다 정상!`으로 확인하여 Gate 4 실행 단계로 전환한다.

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

# 4A — Character Presentation Contract · NEXT

먼저 추출할 공통 정보:

```text
Character ID
Display Name
Short Name
Era / Variant Label
HUD Accent
Skill Labels
Optional Portrait / Icon hooks
Presentation Prefab / Animator hooks
```

현재 이 정보는 Player Team HUD, Skill Deck, Character Controller 등 여러 파일에 문자열/Color로 중복되어 있다.

목표:

```text
CharacterPresentationProfile
```

같은 데이터 단위에서 HUD와 Presentation이 공통으로 읽게 한다.

주의:

```text
HP / CE 수치
고유 술식 Gameplay Rule
Domain Rule
Damage
```

은 Presentation Profile에 넣지 않는다.

4A 첫 migration 성공 조건:

```text
[ ] Gojo/Sukuna identity data가 한 곳에서 제공됨
[ ] 기존 HUD 표시가 그대로 유지됨
[ ] Skill Deck 이름/색상이 동일하게 표시됨
[ ] Tag 시 Active identity 정상 전환
[ ] Gameplay 값 변화 없음
```

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
