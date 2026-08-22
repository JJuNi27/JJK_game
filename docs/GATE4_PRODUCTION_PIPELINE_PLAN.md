# Gate 4 — Production Pipeline Extraction Plan

작성 기준일: 2026-08-23

## 상태

```text
Gate 3 Beauty Corner: FINAL REGRESSION
Gate 4 Production Pipeline Extraction: PREPARED / NOT STARTED
```

Gate 3 최종 회귀가 통과한 뒤 이 문서를 실행 기준으로 사용한다.

## 목적

Gate 1~3에서는 빠르게 실제 플레이를 검증하기 위해 Character/Presentation 기능이 여러 Prototype Controller에 분산되어 있다.

Gate 4의 목적은 `예쁘게 리팩터링`하는 것이 아니라:

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
3. Data와 Runtime 책임을 분리
4. Presentation 요청은 Gameplay 결과를 바꾸지 않음
5. 한 번에 전체 재작성하지 않고 단계별 Migration
6. 각 Migration 뒤 Gojo/Sukuna 회귀 검증
```

---

# 4A — Character Presentation Contract

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

현재 이 정보가 여러 파일에 문자열/Color로 중복되어 있다.

목표:

```text
CharacterPresentationProfile
```

같은 데이터 단위에서 HUD/Presentation이 읽게 한다.

주의:

```text
HP / CE 수치나 고유 술식 로직까지 이 Profile에 밀어 넣지 않는다.
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

Gate 4에서는 먼저 `정보 공급 계약`을 분리한다.

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

그 다음 최종 UI 자산이 준비되면 IMGUI → Canvas/TMP로 교체한다.

즉 Gate 4 초반 목표는 `예쁜 Canvas 제작`보다 UI가 특정 Prototype Controller 구현에 직접 묶이지 않게 하는 것이다.

---

# 4C — Technique Presentation Request

Gate 3에서 반복된 Presentation 종류:

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

목표는 Gameplay Controller가 Camera/VFX 구현 세부사항을 직접 알지 않도록 요청 단위를 만든다.

예시 개념:

```text
TechniquePresentationRequest
- fighter
- technique id
- phase: Anticipation / Release / Impact / Active / End
- world point / direction optional
- intensity profile
```

실제 타입/이름은 코드 조사 후 결정한다.

거대한 모든 기술 범용 Ability System은 만들지 않는다.

---

# 4D — VFX Lifecycle Contract

Gate 3에서 실제로 필요했던 lifecycle:

```text
Spawn
Follow caster/projectile/world point
Play with unscaled/scaled timing policy
Stop/Fade
Destroy
```

Prototype의 procedural LineRenderer VFX는 나중에 실제 VFX Graph/Particle Prefab으로 교체할 수 있어야 한다.

따라서 Gameplay code가 특정 LineRenderer hierarchy 이름을 직접 찾는 구조를 줄인다.

---

# 4E — Animation Contract

실제 rigged model/animation asset이 들어오면 필요한 공통 이벤트를 정의한다.

우선 후보:

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

Gameplay timing과 Animation presentation timing이 필요한 지점만 Animation Event 또는 명시적 state signal로 연결한다.

현재 procedural avatar pose를 최종 Animator 구조로 그대로 확장하지 않는다.

---

# 4F — Audio Event Contract

현재:

```text
PrototypeCombatAudio
SukunaCombatAudio
Resources LocalAudio override
procedural fallback clips
```

Production 목표:

```text
Voice Event
Technique SFX Event
Hit SFX Event
Domain SFX Event
Match Result Event
```

실제 AudioClip 자산을 바꿔도 Gameplay Controller 변경이 최소화되도록 한다.

---

# 4G — Migration / Regression

한 번에 전부 바꾸지 않는다.

권장 순서:

```text
4A Character Presentation Data
→ Gojo/Sukuna 확인

4B HUD Binding
→ Team/Tag/KO 확인

4C Camera/Technique Presentation Request
→ Purple/Fuga/Domain 확인

4D VFX Lifecycle
→ Signature/Hit VFX 확인

4E Animation Contract
→ 실제 모델 자산 준비 상태에 맞춰 진행

4F Audio Contract
→ 실제 Audio 자산 준비 상태에 맞춰 진행
```

각 단계에서 기존 Prototype Gameplay 값은 유지한다.

---

# Codex / Agent 사용 포인트

Gate 4는 여러 파일에 흩어진 중복을 찾아 단계적으로 옮기는 작업이 많아 agent 활용 효율이 높다.

적합 작업:

```text
- Character 문자열/색상/Skill label 중복 탐색
- Presentation 호출 위치 전체 검색
- Interface/Profile 도입에 따른 다중 파일 Migration
- 오래된 Prototype 경로/미사용 코드 탐색
- 테스트/회귀용 코드 검색 및 정리
- Diff 단위 코드 리뷰
```

원칙:

```text
Agent가 대량 수정
→ Git diff 검토
→ 작은 migration 단위로 merge
→ 사용자 Unity 검증
```

사용자 로컬 Unity AI Gateway 또는 Codex가 연결되지 않은 상태에서는 이 ChatGPT가 직접 로컬 Editor를 조작했다고 주장하지 않는다.

---

# Gate 4 완료 조건

세 번째 캐릭터 Gate 5를 시작하기 전에 다음이 성립해야 한다.

```text
[ ] 새 Fighter의 identity/HUD data를 기존 HUD 코드 복사 없이 등록 가능
[ ] 기존 Gojo/Sukuna Gameplay Controller는 그대로 동작
[ ] Technique Presentation 호출 방식이 공통 요청 구조로 정리됨
[ ] Camera/Hit Stop/VFX/SFX의 재사용 경로가 명확함
[ ] Tag/KO/Team HUD가 Fighter 종류에 하드코딩되지 않음
[ ] 실제 Animator/Model을 꽂을 명확한 contract가 존재
[ ] Prototype-only 코드와 Production candidate 코드가 구분됨
```

통과 후:

```text
Gate 5 Third Character Stress Test
```

에서 메구미 또는 유타처럼 구조가 다른 Fighter로 실제 파이프라인을 깨뜨려 본다.
