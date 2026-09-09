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

### 목표 reference
`docs/references/gojo/unlimited_void/cosmic_eye_ref_01.png`

분석 원문:
`docs/references/gojo/unlimited_void/COSMIC_EYE_REFERENCE_ANALYSIS.md`

이 reference는 자유로운 영감용이 아니라 **형태/비율/색층/실루엣의 직접 목표**다.

### Astra 분석 핵심
현재 구현이 reference처럼 안 보이는 가장 큰 이유는 단순 크기가 아니다.

현재 문제 우선순위:
1. iris 전체가 너무 밝고 촘촘한 섬유 무늬로 차 있음
2. 오른쪽 tail이 없음
3. 큰 비정형 cloud mass보다 반복 물결/선이 강함
4. warm ivory/gold가 cold white와 충분히 분리되지 않음
5. bright rim이 reference보다 안쪽 역할과 겹침
6. outer corona가 별도 depth로 읽히지 않음
7. 단일 camera-facing quad의 평면성이 보임

### Reference 비율
Eye outer radius = 1.0 기준:
- pupil: **0.42~0.45** / 시작 0.43
- main iris: **0.45~0.83**
- bright rim center: **0.84~0.89**
- bright rim strong width: **0.015~0.04**
- rim glow: **0.04~0.09**
- corona: **0.90~1.00**
- pupil/rim center offset: **0~0.03**
- visible right tail: center 기준 약 **2.6~2.9**

### 명암 구조
반드시:
**black pupil → dark breathing room + cloud mass → outer bright rim → fading corona**

금지:
- pupil 바로 옆의 균일 neon ring
- iris 전체 동일 brightness
- 규칙적인 radial fiber가 주인공이 되는 구성
- pupil 내부 발광/별/무늬

### 색
사용:
- white / near-white
- warm ivory
- pale gold/orange accent
- pale blue
- cyan
- violet
- subtle spectral fringe

Warm gold/orange는 reference에 실제로 존재하므로 제거하지 않는다.
다만 fire/explosion 색이 아니라 outer rim 일부의 spectral warm arc로 사용한다.

### Cloud / Flow
- 큰 cloud mass를 먼저 맞춘 뒤 미세한 flow를 추가
- asymmetric thickness
- multiple flow layers
- FBM / domain warping 적합
- polar-coordinate flow는 iris 내부 한정으로 유용
- pupil silhouette는 안정적으로 유지

### Rightward tail
**필수 실루엣 요소.**

본체 cloud에서 자연스럽게 연결:
wide cloud connector
→ a few world-space cloud layers
→ faint distant streak/particle assist

particle-only tail 금지.

### 공간 구현
현재 Domain architecture / focal ownership을 유지한다.

권장:
**existing focalRoot 내부의 volumetric-like hybrid**
- pupil/main iris
- 제한된 layered cloud planes
- outer corona
- rightward tail

새 Domain system 또는 별도 병렬 black-hole system을 만들지 않는다.

목표:
**블랙홀에 흰 연기가 감긴 모습이 아니라, reference의 거대한 우주적 눈동자로 읽히는 것.**

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
