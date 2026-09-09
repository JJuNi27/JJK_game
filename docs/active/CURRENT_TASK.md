# 현재 작업 — 고죠 3차 폴리싱 + Domain 종료 복구

상태: **CODEX 구현/자동검증 완료 — 사용자 시각 검토 대기**

중요:
현재 구현은 **로컬 working tree에만 존재**하며 아직 commit / push되지 않았다.
GitHub의 실제 Unity 코드와 이 문서의 구현 상태가 일시적으로 다를 수 있다.

## 목표
1. 무량공처 종료 후 기본 melee가 잠기는 실제 gameplay regression 수정
2. Blue / Red / Purple / Unlimited Void presentation을 한 단계 더 강하게 개선

사용자는 화려함을 원한다.
VFX가 강하다는 이유만으로 자동으로 약하게 만들지 않는다.

---

## P0 — Domain 종료 후 기본 melee 복구

### 요청
- intended cursed-technique burnout은 필요하면 유지
- 영역 종료 후 basic melee / physical attack은 정상 복구
- 관련 없는 combat state를 광범위하게 reset하지 않음
- normal completion / 관련 cancel/end path 검증

### Codex 보고
**구현 완료**
- stale Domain melee lock 수정
- physical attack 복구
- technique burnout 유지

상태: 자동검증 완료, 실제 Play Mode 확인 필요.

---

## P1 — Blue

### 사용자 요청
- 고죠 앞의 의미 없는 작은 pre-cast blue decoration 제거
- singularity / attraction 정체성 유지
- 밝고 매끈한 얼음 파편 대신 어둡고 거친 rubble
- 큰 파편 비중 약간 감소
- medium/small debris 다양성 증가
- core 가독성 유지
- inward suction / collapse 강화
- 강한 spectacle 허용

### 반드시 보존
- Blue 4-hit gameplay

### Codex 보고
**구현 완료**
- 작은 pre-cast decoration 비활성화
- 더 어둡고 거친 debris
- inward collapse 가독성 강화
- 4-hit gameplay 유지

상태: 사용자 시각 검토 대기.

---

## P2 — Red

### 사용자 요청
- projectile 속도 증가
- 사거리 증가
- round smoke puff 대신 압력에 밀려 찢기는 air / dust / vapor
- flash + shockwave + residue를 하나의 강한 `BANG`으로
- 반복 cast cleanup 유지
- fire explosion처럼 만들지 않음

### Codex 보고
**구현 완료**
- **26 m**
- **42 m/s**
- flash / shockwave 강화
- torn outward pressure residue 강화

상태: 사용자 시각 검토 대기.

---

## P3 — Hollow Purple

### 사용자 요청
Purple 핵심 fusion은 매우 좋으므로 광범위하게 재설계하지 않는다.

변경:
- travel range / lifetime 증가
- gameplay range도 일치
- hold 중 Gojo를 덜 가리도록 offset 조정
- release 강화
- irregular violet lightning 강화
- spatial distortion / warped-space wake 강화
- 큰 visual/gameplay size 유지
- 가능하면 scar edge를 조금 더 불규칙하게

### Codex 보고
**구현 완료**
- **48 m / 1.6 s**
- **0.32 s hold 유지**
- hold offset 조정
- release 강화
- branching lightning
- distortion wake 강화

상태: 사용자 시각 검토 대기.

---

## P4 — Unlimited Void interior

### 기존 문제
어두운 빈 공간 + 너무 많은 밝은 White Blood가 화면을 지배함.

### 사용자 목표
- black abyss 기반 유지
- richer deep-blue / blue-white nebula / cosmic depth
- subtle star 허용
- White Blood clutter 감소
- White Blood는 near/mid/far의 보조 accent
- 360° / above-below 분포 유지
- visible floor는 숨기고 collision floor 유지

목표:
**infinite blue-black cosmic nebula + impossible depth + suspended white organic liquid splashes.**

### Codex 보고
**구현 완료**
- layered blue-black nebula
- persistent stars
- White Blood clutter 감소

상태: 사용자 시각 검토 대기.

### 레퍼런스
구현 전에 아래 로컬 파일을 직접 확인:
`docs/references/gojo/unlimited_void/`

- `interior_anime_ref_01.png`
- `interior_concept_ref_01.png`
- `cosmic_eye_ref_01.png`

전경 캐릭터는 구현 대상이 아니다.

---

## P5 — Unlimited Void Cosmic Eye

### 사용자 요청
- 현재보다 훨씬 크게
- pitch-black center
- broad iris / accretion / luminous structure
- pale blue + white + violet + subtle iridescent spectral layer
- 여러 translucent layer와 서로 다른 flow speed
- hypnotic / sublime / eye-like
- simple clean ring 금지

### Codex 보고
**구현 완료**
- larger cosmic eye
- black pupil
- broader spectral flows

상태: 사용자 시각 검토 대기.

---

## P6 — Domain exit / barrier release

### 사용자 요청
기존의 갑작스러운 맵 복귀를 개선.

원하는 흐름:
Domain active
→ Domain release 시작
→ interior/barrier가 중심에서 바깥으로 dissolve
→ original world가 점진적으로 reveal
→ normal combat camera/state로 안전하게 handoff

### Codex 보고
**구현 완료**
- **1.15 s center-outward reveal**
- participant restoration / camera handoff 자동검증 완료

상태: 사용자 시각 검토 대기.

---

## 절대 광범위하게 변경하지 말 것
- Walk Speed 3
- Run Speed 14
- Walk Time Scale 1.15
- current locomotion / Animator / evade
- Blue 4-hit gameplay
- successful Purple fusion foundation
- Unlimited Void purple tunnel
- Gameplay Capture Radius / Visual Barrier / Interior 분리
- same-scene far-away isolated Domain Interior
- participant별 restoration
- hidden visual floor + collision floor
- existing camera ownership / restore system

## 이번 Codex 검증 결과
- focused checks: **4/4**
- combined targeted lab checks: **5/5**
- full Unity regression: **21/21**
- Python: **13/13**
- full regression은 마지막에 1회 수행
- 3개 레퍼런스 모두 확인했다고 보고
- commit / push / merge 없음

## 이제 사용자에게 남은 검토
Play Mode / preview 영상에서 직접 판단:
- Blue debris가 아직 얼음처럼 보이는지
- Red 속도/사거리와 `BANG` 타격감
- Purple 사거리, hold 가독성, 발사 힘, lightning / distortion
- Unlimited Void nebula와 White Blood 비율
- Cosmic Eye 크기/외형
- Domain exit이 실제로 끊기지 않고 자연스럽게 이어지는지
- Domain 종료 후 basic melee가 실제로 정상 동작하는지

사용자가 확인하기 전까지 위 항목은 **PENDING USER VISUAL REVIEW**다.
