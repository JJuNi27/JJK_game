# 무량공처 / Domain 시스템 아키텍처

## 핵심 규칙: 세 개념은 반드시 분리
절대로 하나의 값이나 하나의 물리 공간으로 합치지 않는다.

### 1. Gameplay Capture Radius
영역 전개 시 누가 잡히는지 결정하는 gameplay logic.

### 2. Visual Barrier Radius / Diameter
원래 세계에서 보이는 검은 구형 결계.
Presentation용이며 독립적으로 조절 가능해야 한다.
Barrier 크기를 바꿔도 capture 판정은 바뀌지 않아야 한다.

### 3. Domain Interior Space
무량공처 내부는 별도의 초자연적 pocket-dimension 경험이다.
외부 검은 결계와 물리적으로 같은 크기가 아니다.

## 의도한 흐름
`Original World`
→ Domain cast 성공
→ Gameplay Capture Radius로 대상 포획
→ black barrier closure
→ world transition
→ 기존 purple tunnel
→ participants enter Unlimited Void
→ Domain active
→ Domain release
→ participant별 원래 상태 복구

## 현재 prototype 전략
**같은 Scene의 멀리 떨어진 isolated Domain Interior 공간**.

실제 막히는 문제가 없는 한 이 구조를 교체하지 않는다.

## Participant별 복구
복구 상태는 participant별로 저장한다.
normal / cancel / early end / relevant death / scene unload 경로 모두에서 안전하게 복구한다.

### 아직 해결되지 않은 실제 버그
Codex 자동 검증에서는 수정 완료로 나왔지만,
사용자가 실제 CombatMVP에서 확인한 결과:
**Domain 종료 뒤 basic melee / physical attack이 여전히 실행되지 않는다.**

따라서 다음 작업에서는:
- 단순 flag 값만 검사하지 말 것
- 실제 input → BasicAttack → action/combat lock 경로를 추적
- `BasicAttack`
- `TechniqueBurnoutController`
- Domain presentation/session lock
- player action lock / state restoration
중 누가 실제 입력을 막는지 진단

의도:
- technique burnout은 필요하면 유지
- basic melee는 반드시 복구

---

## Unlimited Void 푸른 성운 배경

### 상태
**USER VERIFIED — 1차 완성**

사용자가 현재 blue-black nebula / cosmic background를 매우 만족스럽다고 확인함.

앞으로 다음을 수정하더라도 성운 배경 자체는 보호:
- White Blood
- Cosmic Eye
- exit/release transition
- audio timing

---

## White Blood choreography

### 현재 문제
White Blood가 그냥 `숑숑숑` 순차 spawn되는 느낌.
사용자가 원하는 원작 느낌은 **박자감 있는 등장**.

### 사용자 의도
대략적인 체감:
**1차: 퉁 → pause → 2차: 투둥 → pause → 3차: 퉁**

### 권장 구조
White Blood를 공간적으로 골고루 섞인 **3개 팀**으로 사전 배분.

- Group 1
- Group 2
- Group 3

팀 배분 시:
- 앞/뒤/좌/우/위/아래가 특정 팀에 몰리지 않게
- near/mid/far depth도 골고루
- 각 팀이 화면/공간 전체에 흩어져 있어야 함

### Timing / Inspector
사용자가 나중에 음성/연출에 맞게 직접 조절 가능하도록 최소한 다음을 Inspector/Data에서 노출:
- Group1 Time
- Group2 Time
- Group2 SubBeat Gap
- Group3 Time

2차 `투둥`은 필요하면 sub-beat 2회로 표현.

중요:
- 이 값은 hardcode하지 말 것
- 나중에 실제 voice / SFX timing에 맞춰 조절할 수 있어야 함
- 관련 사운드 구조는 `docs/architecture/AUDIO_PRESENTATION.md` 참고

---

## Cosmic Eye

### 현재 사용자 판정
현재 결과는 여전히 reference와 충분히 비슷하지 않음.

다음 pass에서는 자유로운 재해석을 줄이고,
`docs/references/gojo/unlimited_void/cosmic_eye_ref_01.png`
를 **구도/비율/형태 기준에 매우 가깝게 맞추는 목표 reference**로 취급한다.

중요하게 따라갈 요소:
- 거대한 pitch-black pupil
- pupil 대비 충분히 넓은 luminous iris / accretion band
- 한 덩어리 흰 smoke가 아니라 여러 개의 얇은 layered flow
- white + pale blue + cyan + violet + subtle spectral fringe
- outer corona의 흐림 / 깊이
- 명확한 "눈동자" 인상
- reference의 scale relationship

전경 캐릭터는 구현 대상이 아니다.

목표:
**블랙홀에 흰 연기가 감긴 모습이 아니라, 거대한 우주적 눈동자 자체로 읽혀야 한다.**

---

## Domain exit / release

### 현재 문제
현재 구현은 카메라 화면에 붙은 screen-space curtain/wipe처럼 느껴짐.

### 사용자 목표
해제 중심은 **카메라가 아니라 Domain을 전개한 캐릭터(Gojo)의 world position**.

권장 방향:
- caster/world-space `releaseOrigin`
- Gojo를 중심으로 spherical / radial boundary가 확장되거나 벗겨짐
- 원래 맵이 그 world-space 중심을 기준으로 점진적으로 드러남
- 카메라를 움직여도 release center가 screen center에 붙어 있지 않음

현재 same-scene isolated interior 구조를 유지하면서 가능한 가장 작은 안전한 방법을 우선한다.

기술적 한계가 실제로 확인되면 대안을 선택하되,
단순 screen-space wipe로 되돌아가지는 않는다.

### 반드시 보존
- participant restoration 안전성
- camera handoff
- repeated Domain cleanup
- barrier HP/destruction은 여전히 범위 밖

---

## 범위 밖
명시적으로 요청하지 않는 한:
- barrier HP
- barrier destruction gameplay
- outside ally rescue
- simultaneous inside/outside combat
- barrier piercing attacks
