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

10 m짜리 결계라고 해서 내부 공간도 10 m 방이어서는 안 된다.

## 의도한 흐름
`Original World`
→ Domain cast 성공
→ Gameplay Capture Radius로 대상 포획
→ Gojo + captured participants를 검은 결계가 감쌈
→ 원래 세계가 사라지고 transition boundary 형성
→ 기존 보라색 터널
→ participants가 Unlimited Void 내부로 이동
→ Domain active
→ Domain이 해제되는 연출
→ 각 participant가 자신의 원래 세계 상태로 복구

## 현재 prototype 전략
현재 구현: **같은 Scene의 멀리 떨어진 isolated Domain Interior 공간**.

이 방식은 기존 reference, CharacterController 기반 enemy 이동, camera ownership, combat state를 보존하면서 불필요한 Additive Scene 재작성을 피한다.

실제 막히는 문제가 없는 한 이 구조를 교체하지 않는다.

## Participant별 복구
복구 상태는 하나의 global transform이 아니라 participant별로 저장한다.

다음 모든 종료 경로에서 각 participant를 자신의 저장 상태로 복구:
- 정상 종료
- cancel
- early end
- 관련 있는 death
- 관련 있는 scene unload

정상 전투로 돌아가기 위해 필요한 gameplay/control state도 함께 복구해야 한다.

### P0 버그 — 현재 로컬 working tree에서 Codex 수정 완료
문제:
Unlimited Void 종료 뒤 CombatMVP에서 기본 melee / physical attack까지 잠긴 채 남을 수 있었음.

의도:
- 저주술식 burnout이 gameplay 규칙상 필요하면 그대로 유지 가능
- **기본 melee / physical combat는 영역 종료 후 반드시 복구**

Codex 보고상 stale Domain melee lock을 수정했고 targeted/full regression을 통과했다.
실제 feel과 정상 복구는 사용자 Play Mode 확인이 남아 있다.

## 현재 Domain 시간 기준
이 값들은 자동 검증 기준이며 사용자 잠금값이 아니다.
- Domain arrival / cinematic sequence: **5.87 s**
- Domain interior dwell: **10 s**
- Victim stun / overwhelmed: **16 s**
- Empty-domain arrival: **4.72 s**

## Interior 비주얼 방향

### 반드시 보존
- 보이는 floor는 숨기고 collision floor는 유지
- 360° enclosure
- 위/아래 방향 감각 모호함
- 기존 보라색 터널
- 별도의 pocket-space 느낌

### 사용자 목표
기존의 **어두운 빈 공간 + 너무 많은 White Blood** 느낌을 줄인다.

원하는 방향:
- black abyss를 기반으로 유지
- deep blue / blue-white nebula / cosmic depth 강화
- 필요하면 subtle star / 우주 깊이 추가
- White Blood는 환경 전체가 아니라 보조 accent
- near / mid / far에 분산하되 clutter 감소
- 정상적인 바닥/하늘 구분이 보이면 안 됨

목표:
**무한한 blue-black cosmic void / nebula + 불가능한 깊이감 + 공간에 떠 있는 흰 유기성 액체 구조.**

### 현재 로컬 working tree Codex 구현 결과
- layered blue-black nebula depth
- persistent stars
- White Blood clutter 감소

시각 품질은 **사용자 시각 검토 대기**.

## Cosmic Eye / black-hole focal

원하는 방향:
- 크고 지배적인 pitch-black pupil / abyss
- 넓은 iris / accretion / luminous structure
- pale blue, white, violet, subtle spectral / iridescent layers
- 서로 다른 속도의 translucent flow
- hypnotic / sublime / cosmic-eye 느낌
- 작은 깔끔한 neon ring처럼 보이면 안 됨

현재 로컬 working tree에서 Codex가 더 큰 eye와 넓은 spectral flow를 구현했다고 보고했다.
최종 외형은 **사용자 시각 검토 대기**.

레퍼런스가 존재하고 CURRENT_TASK에서 지정했다면:
`docs/references/gojo/unlimited_void/`
를 직접 확인한다.

## Domain exit
기존 문제:
원래 맵으로 복귀할 때 너무 갑자기 휙 전환됨.

목표:
- 환경을 즉시 snap하지 않음
- Domain/barrier가 풀리거나 벗겨지는 느낌
- center-outward dissolve / reveal 우선
- gameplay restoration은 deterministic하고 안전해야 함
- camera/participant를 interior에 남기면 안 됨

현재 로컬 working tree Codex 구현:
- **1.15 s center-outward reveal**
- participant restoration / camera handoff 검증 보고

시각적으로 자연스러운지는 **사용자 시각 검토 대기**.

## 범위 밖
명시적으로 요청하지 않는 한 구현 금지:
- barrier HP
- barrier damage / destruction
- outside ally rescue
- interior/exterior 동시 전투
- barrier를 관통하는 공격
