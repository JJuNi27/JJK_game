# 현재 작업 — 3차 폴리싱 실플레이 피드백 정리

상태: **사용자 구현 승인 완료 — 다음 Codex 구현 pass 시작 가능**

사용자가 현재까지 정리된 실플레이 피드백과 Cosmic Eye 분석을 기준으로 다음 구현 pass 진행을 승인했다.

이번 pass의 범위는 이 문서의 P0~P6 수정이다.
`docs/design/GAME_VISION.md`와 `docs/architecture/COMBAT_DATA_DRIVEN.md`는 장기 방향 참고용이며,
**이번 pass에서 전투 전체를 Data-driven 구조로 대규모 migration하지 않는다.**

현재 USER VERIFIED인 Blue와 Unlimited Void 푸른 성운 배경은 잠금 상태로 보존한다.

---

## P0 — 최우선 실제 버그: Domain 종료 후 평타가 여전히 안 나감

### 사용자 실제 검증
CombatMVP에서 Unlimited Void 종료 후:
**basic melee / physical attack이 여전히 실행되지 않음.**

이전 자동 test 21/21은 실제 버그를 잡지 못했다.

### 다음 구현 원칙
- technique burnout은 의도대로 유지 가능
- basic melee만 정상 복구해야 함
- broad reset 금지
- 실제 input → BasicAttack → state/action lock 경로를 추적
- 내부 bool만 보고 "fixed"라고 판단 금지

### 반드시 확인할 후보
- `BasicAttack`
- `TechniqueBurnoutController`
- Domain presentation/session lock
- player action lock
- restoration timing/state

### Done
실제 CombatMVP에서:
- Domain 종료
- technique skill은 burnout 규칙대로 동작
- basic melee 입력 시 실제 공격 sequence 시작

---

## LOCKED — Blue / 아오

### 사용자 판정
**1차 완성.**

새로운 아이디어가 생기기 전까지 수정하지 않는다.

보존:
- 현재 Blue visual
- 현재 debris
- current attraction/singularity
- 4-hit gameplay

---

## P1 — Red / 아카

### 버그
Enemy에 맞아 impact가 발생했는데도 projectile visual이 계속 날아감.

### 다음 구조
대상별 Collision Response로 분리:
- Enemy/Character → impact + damage + projectile 종료
- Destructible Building/Object → 파괴/충격 전달 가능 + Red는 계속 통과 가능
- Hard World Solid → impact + 종료
- Trigger/Ignore → 통과
- Max Range → 자연 소멸

미래의 넓은 도쿄 도시맵 / 건물 파괴를 고려해 확장 가능한 구조로 설계.

### Impact VFX
현재 smoke / vapor가 너무 약함.

강화:
- 더 큰 volume
- 더 강한 initial pressure burst
- 방사형/수평으로 밀려나는 회백색 air/dust/vapor
- 약간 더 긴 residue
- cartoon smoke puff 금지
- fire explosion 금지

---

## P2 — Hollow Purple

### 문제 A — 중앙 정렬
caster 가림을 해결하려다 Purple이 Gojo 오른쪽에 생김.

수정:
- Purple은 Gojo 정중앙 축에서 완성
- lateral X offset = 0 원칙
- 가림은 forward offset / vertical offset / framing으로 해결

### 문제 B — scar 폭
사용자 체감상 바닥 잔상이 다시 좁아짐.

권장:
- scar width를 Purple actual visual diameter에 비례하도록 설계
- width multiplier를 Inspector/Data에서 조절 가능하게
- Purple scale 변경 시 scar만 가늘어지는 regression 방지

### 보존
- current fusion
- 0.32 s hold
- current long-range direction
- lightning / distortion direction

---

## P3 — Unlimited Void White Blood 리듬

### 현재 문제
그냥 빠르게 순서대로 spawn되는 느낌.

### 원하는 리듬
**1차 퉁 → pause → 2차 투둥 → pause → 3차 퉁**

### 구조
White Blood를 3개 spatial group으로 배분:
- Group 1
- Group 2
- Group 3

각 그룹은:
- 앞/뒤/좌/우/위/아래에 골고루
- near/mid/far depth에 골고루
- 특정 방향에 몰리지 않음

### Inspector/Data tuning
최소:
- Group1 Time
- Group2 Time
- Group2 SubBeat Gap
- Group3 Time

나중에 실제 음성에 맞게 사용자가 조절 가능해야 함.

---

## P4 — 사운드 / 음성 구조

사용자 선호:
**타격별/행동별 SFX를 분리하는 방향 선호.**

합의된 설계:
- 짧은 punch/kick/whoosh/impact → event별 clip
- 연속된 voice line → 하나의 clip 유지 가능
- animation과 audio 모두 공통 Beat/Timeline에 동기화
- 한쪽이 다른 쪽에 hard-bind되지 않음

자세한 기준:
`docs/architecture/AUDIO_PRESENTATION.md`

---

## P5 — Cosmic Eye

### 사용자 판정
현재 eye는 아직 reference를 충분히 따라하지 못함.

다음에는 자유로운 재해석보다:
`docs/references/gojo/unlimited_void/cosmic_eye_ref_01.png`
를 **매우 가까운 구조/비율/색층 목표**로 취급한다.

전경 캐릭터는 구현 대상이 아니다.

### Astra 분석 결과 — 다음 구현 기준
분석 원문:
`docs/references/gojo/unlimited_void/COSMIC_EYE_REFERENCE_ANALYSIS.md`

핵심 결론:
**더 크게 만드는 것보다 명암 구조, 큰 구름 덩어리, warm rim, 오른쪽 tail을 먼저 맞추는 것이 reference 재현 효과가 크다.**

#### Reference 비율 시작값
Eye outer radius = `1.00` 기준:
- Pupil radius: **0.42~0.45**, 시작값 **0.43**
- Main iris: **0.45~0.83**
- Bright rim 중심: **0.84~0.89**
- Bright rim 강한 띠 두께: **0.015~0.04**
- Rim 주변 glow: **0.04~0.09**
- Outer corona: **0.90~1.00**
- Pupil/rim 중심 편차: **0~0.03**
- 오른쪽 tail visible extent: 중심에서 약 **2.6~2.9**

#### 반드시 맞출 명암 구조
`pitch-black pupil`
→ `어두운 iris 여백 + 큰 비정형 cloud`
→ `강한 바깥 bright rim`
→ `부드럽게 사라지는 outer corona`

금지:
- pupil 바로 둘레에 강한 네온 ring
- iris 전체를 같은 밝기로 채우기
- 촘촘한 반복 섬유/방사선이 화면을 지배
- pupil 내부에 star/glow/fiber 넣기

#### 색 규칙
- near-white
- warm ivory
- **pale gold/orange accent 유지**
- pale blue
- cyan
- violet
- subtle spectral fringe

Gold/orange는 fire color가 아니라 outer rim 일부의 따뜻한 spectral accent로 사용.

#### 구조 / 구현 방향
기존 Domain / focal ownership을 유지하면서:
- main pupil/iris shader
- 제한된 수의 layered cloud/corona plane
- 오른쪽으로 연결되는 nebula tail
을 조합한 **volumetric-like hybrid**를 우선 검토.

Claude가 제안한 기술 중 다음은 적극 활용 가능:
- FBM noise
- domain warping
- multiple flow layers with different speeds
- asymmetric cloud thickness
- subtle chromatic dispersion
- outer corona
- hybrid shader + secondary cloud/streak layers

부분적으로만 사용할 것:
- polar-coordinate swirl: iris flow에는 유용하지만 전체를 규칙적인 소용돌이로 만들지 말 것
- Einstein-like rim: outer rim 개념에는 유용하지만 pupil에 붙은 균일 neon ring으로 만들지 말 것

#### Rightward nebula tail
선택 장식이 아니라 reference silhouette의 핵심.

권장:
main iris의 넓은 cloud connection
→ 소수의 world-space cloud layer
→ 먼 부분의 희미한 streak/particle 보조층

particle만으로 분사 연기처럼 만들지 않는다.

### 다음 구현 Top 5
1. 촘촘한 밝은 반복선 제거/약화 + 명암 구조 재분리
2. 오른쪽 대형 cloud + tail 연결
3. pupil 약 0.43 / bright rim 약 0.86 기준으로 비율과 비대칭 수정
4. ivory/gold와 cold blue/violet cloud 색층 분리
5. corona / cloud / tail에 제한적 depth를 주고 reference 비교 시점 확보

목표:
**가능한 한 reference 자체의 eye 구조를 충실하게 재현.**

---

## LOCKED — Unlimited Void 푸른 성운 배경

### 사용자 판정
**현재 배경 매우 만족. 1차 완성.**

보존:
- 현재 blue-black nebula
- cosmic depth
- star/depth language
- invisible floor / orientation ambiguity

White Blood / eye / exit 수정 중 이 배경을 같이 바꾸지 않는다.

---

## P6 — Domain exit를 caster-centered world-space release로

### 현재 문제
현재는 카메라 화면에 붙은 curtain/wipe처럼 보임.

### 원하는 연출
Domain을 전개한 Gojo의 world position을 release center로 사용.

`Domain active`
→ caster 중심 release 시작
→ world-space radial/spherical boundary 확장
→ original map이 그 중심 기준으로 드러남
→ safe camera/combat handoff

카메라를 움직여도 해제 중심이 screen center에 붙지 않아야 함.

### 제약
- same-scene isolated Domain architecture 유지
- participant restoration 안전성 유지
- camera drift 금지
- repeated use cleanup
- barrier HP/destruction gameplay는 구현하지 않음

---

## 현재 사용자 승인 상태

### 잠금 / 더 이상 건드리지 않음
- Blue 1차 완성
- Unlimited Void 푸른 성운 배경 1차 완성

### 다음 수정 필요
- Domain 종료 후 basic melee
- Red impact projectile termination / collision response / smoke
- Purple 중앙 정렬 / scar width
- White Blood 3-beat choreography
- sound/voice beat architecture
- Cosmic Eye fidelity
- caster-centered Domain release

## 다음 단계
**Codex 구현 시작 가능.**

작업 순서 권장:
1. P0 실제 평타 복구 버그를 먼저 원인 진단/수정
2. Red
3. Purple
4. White Blood
5. Cosmic Eye
6. Domain release
7. 관련 targeted validation
8. 구현 안정 후 full regression 1회
9. 사용자 Play Mode 시각 검토 대기

사운드/음성은 이번 pass에서 새 audio asset을 만들지 않는다.
White Blood timing과 기존 beat 구조가 향후 audio sync에 맞게 조절 가능한 상태만 유지한다.

명시적 승인 없는 commit / push / merge 금지.
