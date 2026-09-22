---
name: jjk-reference-breakdown
description: JJK VFX reference 영상·이미지의 관찰, 시각 목표, 현재 candidate와의 차이를 timestamp 근거로 분석한다. Unity 구현 제안은 사용자가 요청한 경우에만 별도 단계로 작성한다.
---

# JJK Reference Breakdown

## 범위와 입력

[AGENTS.md](../../../AGENTS.md)와 [Reference README](../../../docs/references/README.md)에서 자료 취급 규칙을 읽는다.
[CURRENT_TASK](../../../docs/active/CURRENT_TASK.md) 또는 사용자 요청에서 source, 분석 구간, candidate 버전, 비교 질문을 확인한다.
모델 선택과 해석상 결정은 [AI Development Workflow](../../../docs/workflows/AI_DEVELOPMENT_WORKFLOW.md)를 참조한다.

기존 의견은 관찰을 대신하지 않는다. source를 직접 보지 못했으면 그 한계를 명시하며 관찰 완료를 주장하지 않는다.
이 Skill은 구현·튜닝·runtime asset 변환을 수행하지 않는다.

분석은 길게 쓰는 것이 목적이 아니다. 결론을 바꿀 수 있는 evidence를 우선한다.
모든 분석 축을 확인하되 질문과 무관하거나 관찰할 수 없는 항목은 `NOT MATERIAL` 또는 `NOT OBSERVABLE`로 짧게 기록할 수 있다.

## A. SOURCE CHECK

Reference와 candidate 각각 다음을 기록한다.

| 항목 | 기록할 내용 |
|---|---|
| Source file | 정확한 로컬 경로 또는 URL, 버전 식별 정보 |
| Timestamp range | 원본 기준 시작/끝, 선택한 beat |
| FPS | 확인값 또는 unknown; 필요하면 가변 FPS 여부 |
| Playback speed | 원속도/반속 등 분석 재생 배율, 원본 자체 slow motion 여부 |
| Resolution / compression | 해상도, 압축·재인코딩·crop으로 잃은 정보 |
| 관찰 한계 | occlusion, motion blur, camera cut, exposure saturation 등 |

재생 시간을 원본 timestamp와 혼동하지 않는다.
정지 이미지는 FPS·velocity·lifetime을 추정하지 않고 해당 항목을 `NOT OBSERVABLE`로 표시한다.
빠른 사건은 원속도 motion과 필요한 소수의 프레임을 함께 본다.

### A-1. PHASE MATCH GATE

직접 비교 전에 반드시 확인한다.

- 같은 technique인가?
- 같은 phase / beat인가?
- 화면상 역할이 직접 대응되는가?
- camera / framing / environment 차이가 비교를 무효화할 정도인가?

각 source 또는 구간을 다음 중 하나로 분류한다.

- `PRIMARY`: 같은 technique·phase 또는 직접 대응되는 사건. form / motion / timing의 직접 gap 근거로 사용 가능.
- `SECONDARY`: technique/phase는 다르지만 제한된 시각 특성만 참고할 가치가 있음. 예: temporal instability, luminance transition, screen-space violence.
- `NOT DIRECTLY COMPARABLE`: 직접적인 form/motion gap의 근거로 사용하지 않음.

`SECONDARY`를 사용할 때는 어떤 특성에만 참고하는지 명시한다.
서로 다른 phase를 같은 현상으로 합쳐 결론내리지 않는다.
phase가 불명확하면 confidence를 낮추고, 필요한 경우 사용자에게 범위 확인을 요청한다.

## B. REFERENCE OBSERVATION

먼저 화면에서 무엇이 보이는지만 기록한다.
이 단계에는 Unity component, shader 기법, 생성 방법 제안을 넣지 않는다.

기본 표:
`관찰 ID | 원본 timestamp/beat | 분석 축 | 직접 관찰 | 추정(있으면) | confidence 및 근거 | 관찰 한계`

Confidence는 `high / medium / low`로 표시한다.
직접 측정값, 상대 인상, 추정을 구분한다.

### B-1. FRAME EVIDENCE

timestamp / frame 기반으로 확인할 수 있는 것을 기록한다.

- 형태 변화
- 위치 변화
- 가림 관계
- 밝기 변화
- 색 변화
- silhouette의 출현/소실

프레임 증거만으로 원속도 체감 강도·무게·속도를 자동 결론내리지 않는다.

### B-2. REAL-TIME PERCEPTION

가능하면 정상 재생 속도에서도 다음을 별도로 본다.

- speed impression
- weight / force impression
- readability
- violence / instability
- confusion / visual noise
- impact timing

원속도 검토를 하지 않았으면 `NOT RUN`으로 남긴다.
frame-by-frame 분석을 했다는 이유로 real-time perception까지 검증했다고 주장하지 않는다.

### B-3. TRACKING CONFIDENCE

동일 visual element의 lifetime / direction / trajectory를 추적할 때 다음 요인을 확인한다.

- occlusion
- motion blur
- repeated / duplicate-looking frames
- saturation / clipping
- compression artifacts
- camera movement / cut
- 다른 효과와의 중첩

동일 요소 추적이 끊기면 정확한 lifetime이나 trajectory를 사실처럼 단정하지 않는다.
필요하면 `tracking confidence: high / medium / low`를 별도로 기록한다.

### B-4. 분석 축

| 분석 축 | 관찰 질문 |
|---|---|
| primary silhouette | 가장 먼저 읽히는 외곽 형태와 끊김은 무엇인가? |
| body mass | 중심 질량의 응집·빈 공간·무게 인상은 어떤가? |
| secondary energy | 본체 밖 보조 에너지가 어떤 역할과 비중을 갖는가? |
| motion direction | 내향/외향/접선 흐름의 시작과 끝이 어디인가? |
| velocity impression | 가속·감속·충격·정체가 어떻게 읽히는가? |
| lifetime | 개별 사건과 잔광이 어느 구간 동안 유지되는가? |
| density | 동시에 보이는 사건 수와 채워진 면적은 어떤가? |
| scale | 본체·캐릭터·화면에 대한 상대 크기는 어떤가? |
| asymmetry | 위치·밝기·빈도·형태의 비대칭은 무엇인가? |
| depth / parallax | 앞뒤 가림과 상대 이동이 깊이를 어떻게 보여주는가? |
| luminance | 밝기 peak와 시간 변화, 포화 영역은 어디인가? |
| contrast | 명부·암부 경계와 negative space는 어떻게 분포하는가? |
| color hierarchy | 주색·보조색·강조색이 어느 면적과 밝기를 차지하는가? |
| screen-space contribution | 화면 점유율과 시선 집중에 얼마나 기여하는가? |
| environmental interaction | 지면·물체·배경에 실제로 보이는 반응은 무엇인가? |
| camera contribution | camera 이동·shake·framing과 효과 자체 움직임을 구분할 수 있는가? |
| impact punctuation | 어떤 순간적 명암·형태·속도 변화가 사건을 강조하는가? |
| relationship / coupling | body와 secondary energy가 하나의 사건으로 읽히는가, 독립 부품처럼 읽히는가? |
| state change | body / secondary energy / luminance / readability가 함께 상태를 바꾸는가? |
| what must NOT be copied literally | 관련 없는 인물·UI·구도, 매체 특유 표현 중 그대로 옮기면 부적합한 것은 무엇인가? |
| uncertain observations | 가림·압축·포화 때문에 확정할 수 없는 것은 무엇인가? |

화면상 이동을 실제 world velocity로 단정하지 않는다.
밝기 clipping만 보고 실제 광원 구조를 확정하지 않는다.

## C. VISUAL TARGET

관찰 ID를 근거로 구현 독립적인 목표를 쓴다.
각 목표는 `보여야 할 현상 | 피해야 할 인상 | 확인할 beat/view | 근거 ID`로 정리한다.

Visual Target은 화면에서 보여야 하는 현상을 설명한다.
“번개를 늘린다”, “mesh를 줄인다”, “LineRenderer를 추가한다”처럼 구현 수단이나 수량으로 목표를 대신하지 않는다.

예시는 특정 기술의 답을 유도하지 않도록 중립적으로 쓴다.
예: “주요 사건과 보조 효과가 시간적으로 따로 놀지 않고 하나의 beat로 읽혀야 한다.”
예시를 실제 source 관찰 사실로 취급하지 않는다.

## D. REFERENCE VS CANDIDATE GAP ANALYSIS

Candidate도 B의 동일 분석 축으로 별도 관찰한 뒤 대응 beat를 비교한다.
서로 다른 길이의 영상은 의미상 같은 사건을 맞추고 원본 timestamp를 각각 유지한다.
A-1의 Phase Match 결과를 적용하며 `SECONDARY`와 `NOT DIRECTLY COMPARABLE` 구간의 사용 범위를 넘지 않는다.

Camera·화면 점유율·재생 속도·배경 차이가 판단에 끼치는 영향을 적는다.

기본 표:
`분석 축 | reference 관찰/시간 | candidate 관찰/시간 | 지각적 gap | 왜 다르게 느껴지는지(추정과 confidence) | 비교 한계`

특정 결론을 유도하는 고정 예시는 사용하지 않는다.
Gap은 실제 source evidence에서 도출하고 “더 화려하게”, “더 강하게” 같은 총평으로 대체하지 않는다.

### D-1. RELATIONSHIP / STATE CHANGE GAP

다음을 별도로 확인한다.

- body와 secondary energy가 한 사건으로 결합되어 읽히는가?
- 외곽 요소가 독립 object처럼 오래 추적되는가?
- body / secondary energy / luminance가 함께 상태를 바꾸는가?
- 개별 요소의 위치만 교체되는가?
- 형상이 다른 발광이나 가림에 묻혔다 다시 나타나는가?
- 전체 readability가 시간에 따라 어떻게 변하는가?

이 축은 특정 Purple 사례의 결론을 일반화하지 않고 모든 VFX reference에 적용 가능한 관계 분석으로 사용한다.

### D-2. COUNTER-EVIDENCE

중요한 가설마다 다음을 기록한다.

- `Supporting evidence`
- `Counter-evidence`
- `Final confidence`

가설과 맞지 않는 장면이나 조건을 의도적으로 찾는다.
counter-evidence가 없으면 `none found in reviewed evidence`라고 명시한다.
counter-evidence를 찾지 못했다는 사실을 가설 확정으로 취급하지 않는다.

Candidate가 없으면 Gap Analysis를 `NOT EVALUATED`로 남긴다.
과거 보고나 코드만 보고 화면 차이를 만들어내지 않는다.

## E. IMPLEMENTATION PROPOSAL — 요청된 경우만

사용자가 구현 제안을 요청하지 않았으면 D까지 보고하고 끝낸다.
제안 요청은 실제 수정 권한과 구분한다.

이 단계에서 처음 shader / mesh / particle / line / screen effect 등의 수단을 논의한다.

각 제안에 다음을 붙인다.

- 해결할 gap ID
- 구현 후보
- 기존 구조 재사용
- 예상 시각 효과
- trade-off
- LOCK 영향
- 검증 방법

관찰 결과를 구현 가설에 맞춰 소급 수정하지 않는다.
현재 프로젝트에서 실제 사용 가능한 기술을 확인한 뒤 제안한다.
