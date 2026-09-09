# Cosmic Eye Reference 분석 — Astra 시각 분석 기준

상태: **분석 완료 / 다음 구현 기준 자료**
구현 작업이 아니라 reference 분석 결과를 보존하는 문서다.

## 분석 대상
- `cosmic_eye_ref_01.png`
- `interior_anime_ref_01.png`
- 현재 3차 polish Domain preview
- 현재 `UnlimitedVoidPrototypeVisual.cs`
- 현재 `UnlimitedVoidIris.shader`
- 현재 `DomainPresentationSettings.cs`

## 핵심 결론
Reference의 정체성은 단순히 "큰 검은 원 + 밝은 소용돌이"가 아니다.

핵심 관계:
**큰 검은 pupil + 어두운 내부 여백 + 비대칭 cloud mass + 강한 outer rim + 오른쪽 tail**

현재 구현은 밝은 반복 섬유 무늬가 iris 전체를 차지하여 이 관계가 약하다.

또한 다음 구현에서는 단순히 Eye 크기를 더 키우는 것보다:
1. 명암 구조 분리
2. 큰 cloud 형태
3. warm rim
4. rightward tail
을 먼저 맞추는 것이 reference 유사도에 더 큰 영향을 준다.

---

## A. 전체 실루엣

Eye 본체는 거의 원형.
강한 비대칭은 pupil 변형이 아니라:
- cloud density
- cloud brightness
- cloud thickness
- rightward tail
에서 발생한다.

특징:
- pupil과 main rim은 거의 같은 center
- 오른쪽 위 cloud가 넓고 밝음
- 아래쪽은 dark breathing room이 많음
- 왼쪽에는 blue/violet cloud + thin outer glow
- 오른쪽으로 긴 nebula stream이 연결됨

Unlimited Void 안에서 Eye는 공간 규모를 잡아주는 **primary focal landmark**다.
주변 nebula가 Eye만큼 밝고 복잡해져 focal hierarchy를 깨면 안 된다.

---

## B. Pupil

Reference pupil:
- 거의 완전한 black
- 내부 star/glow/fiber 없음
- stable circular silhouette
- edge에는 아주 얇은 blue-gray transition 가능
- strongest bright rim은 pupil 바로 옆에 붙지 않음

중요:
**pupil → dark gap/cloud → outer rim**
구조.

균일한 neon outline을 pupil 바로 둘레에 붙이지 않는다.

---

## C. Iris / Ring 구조

Reference는 크게 세 영역으로 읽힌다.

1. 넓고 불균일한 inner cloud
2. 그 사이의 dark breathing room
3. 바깥쪽의 얇고 강한 luminous rim

iris 전체가 같은 brightness가 아니다.

"눈"으로 읽히는 핵심은 반복 radial line 수가 아니라:
**stable black pupil을 둘러싼 넓은 명암 구조와 cloud mass**다.

---

## D. Reference 비율

Eye outer corona radius = **1.00** 기준 추정값:

| 요소 | 정규화 값 |
|---|---:|
| Pupil radius | **0.42~0.45** |
| 권장 시작 Pupil | **0.43** |
| Pupil edge transition | **0.01~0.02** |
| Main iris | **0.45~0.83** |
| Bright rim center | **0.84~0.89** |
| Bright rim strong width | **0.015~0.04** |
| Rim glow spread | **0.04~0.09** |
| Outer corona | **0.90~1.00** |
| Pupil/rim center offset | **0~0.03** |
| Visible right-tail end | center 기준 **2.6~2.9** |

핵심 관계:
**Pupil은 Eye 외곽 반경의 약 43%.**

이 값은 최종 locked value가 아니라 다음 구현의 reference-matching 시작점이다.

---

## E. 색

Reference에 실제로 존재하는 색:
- near pure white
- warm ivory
- pale gold / pale orange
- cyan
- pale blue
- violet
- subtle spectral fringe

### 색 역할
**White / near-white**
- 일부 outer rim / highlight

**Warm ivory**
- 위쪽 bright arc의 중요한 색

**Pale gold/orange**
- 주색은 아니지만 reference identity에 중요한 warm accent
- fire/explosion orange처럼 사용 금지

**Pale blue**
- 넓은 cloud body의 주요 색

**Cyan**
- 일부 edge / thin light membrane

**Violet**
- 깊이/그림자층

**Spectral fringe**
- 균일 rainbow band가 아니라 얇고 불규칙한 color separation

Warm gold/orange는 제거하지 않는다.

---

## F. Cloud / Flow

Reference는 single smoke layer보다 여러 밀도/선명도의 cloud layer로 읽힌다.

권장 기술:
- FBM: 적합
- domain warping: 적합
- multiple flow layers with different speed: 적합
- asymmetric thickness: 적합
- subtle chromatic dispersion: 적합
- polar-coordinate flow: **부분 적합**

Polar flow는 iris 내부 접선 흐름에 유용하지만,
전체 Eye를 균일한 swirl/radial pattern으로 만들면 reference와 멀어진다.

큰 cloud mass를 먼저 맞추고 미세 flow는 그 다음.

---

## G. Rightward Nebula Tail

Tail은 선택 장식이 아니라 **reference 전체 실루엣의 핵심**이다.

Reference 느낌:
Eye body
→ wide bright/cloud connector
→ separated mid cloud masses
→ faint distant luminous traces
→ frame 밖으로 continuation

권장 hybrid:
1. main iris shader와 자연스럽게 연결되는 wide cloud connector
2. 소수의 world-space cloud layer
3. 끝부분의 희미한 streak/particle assist

Particle-only tail은 분사 smoke / speed line처럼 보일 위험이 크므로 피한다.

---

## H. 공간 구조

현재 구현은 camera-facing Quad 하나에 모든 Eye 요소를 합성.

Reference fidelity를 높이는 권장 방향:
**existing focalRoot 내부의 volumetric-like hybrid**

예:
- main pupil/iris layer
- near cloud
- far cloud
- outer corona
- rightward tail

실제 volumetric renderer를 새로 도입할 필요는 없음.

중요:
- 기존 Domain architecture 유지
- 기존 생성/정리 ownership 유지
- 별도 병렬 BlackHoleEye 시스템을 새로 만들지 않음

---

## I. 현재 구현과 Reference 차이 — 우선순위

1. **iris 전체가 너무 밝고 촘촘한 fiber**
2. **rightward tail 없음**
3. **large cloud mass보다 반복 wave pattern이 강함**
4. **cold white가 지배하고 warm ivory/gold 분리가 약함**
5. **bright rim과 inner iris 역할이 겹침**
6. **outer corona가 별도 depth로 읽히지 않음**
7. **single billboard의 평면성 / framing 문제**

현재 shader의 pupil은 대략 `r=0.35` 수준으로,
reference 시작 목표 약 `0.43`보다 작다.

그러나 pupil만 크게 키우는 것으로 해결되지 않는다.
**brightness distribution과 negative space가 더 중요하다.**

---

## J. 다음 구현 Top 5

### 1. 명암 구조 재설계
반복 bright fiber의 지배를 줄이고:
black pupil
→ dark gap/cloud
→ outer bright rim
을 명확하게 분리.

### 2. Rightward cloud/tail 추가
오른쪽 위 cloud가 body에서 자연스럽게 tail로 이어져야 함.

### 3. Reference 비율 적용
시작:
- pupil ≈ 0.43
- bright rim ≈ 0.86
- corona ≈ 0.90~1.00

pupil은 stable, cloud thickness는 asymmetric.

### 4. Warm / Cold 색층 분리
- inner cloud: pale blue/violet
- highlight: near-white
- selected outer arcs: warm ivory/gold

모두 흰색으로 합쳐지지 않게 함.

### 5. Limited depth + comparison framing
corona/cloud/tail에 제한적인 앞뒤 depth를 만들고,
Eye body + tail 전체가 보이는 비교용 시점을 확보.

---

## K. 다음 Codex가 반드시 지킬 시각 규칙

- Reference 자체가 목표다. 다른 cosmic-eye 디자인으로 자유 재해석하지 않는다.
- pupil 내부는 순수하게 어둡게 유지.
- strongest rim은 pupil 바로 둘레가 아니라 iris outer region.
- iris 전체 동일 brightness 금지.
- dark breathing room을 반드시 보존.
- 큰 cloud mass를 먼저 만들고 micro flow는 나중.
- periodic radial line / uniform wave를 주인공으로 만들지 않음.
- asymmetry는 cloud/tail에 주고 pupil은 안정적으로 유지.
- warm ivory / pale gold-orange를 제거하지 않음.
- tail을 별도 장식처럼 붙이지 않고 body cloud에서 연결.
- corona는 hard outer circle이 아니라 dark space로 fade.
- 기존 USER VERIFIED blue nebula background를 훼손하지 않음.
- rotation direction/speed는 reference에서 확정된 사실처럼 취급하지 않음.

---

## L. USER VISUAL REVIEW가 필요한 것

- rotation direction / speed
- 가려진 right-inner 구조
- tail 실제 길이
- corona intensity
- warm color 최종 비중
- tail orientation을 world-fixed로 둘지 presentation 방향에 맞출지
- 실제 combat camera에서의 Eye 크기/높이
- layered parallax가 정면 reference fidelity를 해치지 않는지

위 값은 자동으로 USER VERIFIED 처리하지 않는다.
