---
name: jjk-visual-qa
description: JJK VFX candidate의 기술 정확성, 회귀 안전성, reference 유사도, art direction을 분리 평가한다. 구현 후 검수나 기존 결과 검토에 사용하며 자동 재튜닝이나 사용자 시각 승인을 대신하지 않는다.
---

# JJK Visual QA

## 입력과 평가 범위

[AGENTS.md](../../../AGENTS.md)의 보호·권한 규칙, [CURRENT_TASK](../../../docs/active/CURRENT_TASK.md)의 LOCK과 요청된 검수 범위를 확인한다.
[USER_VERIFIED_SETTINGS](../../../docs/locked/USER_VERIFIED_SETTINGS.md)는 기존 사용자 승인 범위의 기준이다.
검증 실행 절차는 [Unity Validation](../../../docs/workflows/UNITY_VALIDATION.md), 모델 routing은 [AI Development Workflow](../../../docs/workflows/AI_DEVELOPMENT_WORKFLOW.md)를 참조한다.

Baseline/candidate의 branch·commit 또는 로컬 변경 상태, 사용한 profile/toggle, reference, 기존 QA 증거를 식별한다.
A/B 영상의 `left/right`, `previous/candidate` 정체는 화면 표기나 보고서 근거로 먼저 확인하며 추측하지 않는다.
이미 유효한 검사를 반복하기 전에 구현·실행 조건이 달라졌는지 확인한다.
결과 검토만 요청됐으면 그 범위에서 끝낸다.

## 비교 조건

Camera, framing, resolution, 시간 기준, 재생 속도, 배경, 노출/post-process, 가능한 경우 random seed를 일치시킨다.
일치시킬 수 없는 항목은 비교 한계로 남긴다.
비교를 맞추려고 보호된 global 설정을 변경하지 않는다.

변경 구간과 보호 구간을 분리하고 필요한 최소 still/motion을 선택한다.
흐름은 원속도 motion으로 먼저 보고 반속·grayscale·close-up은 판단 질문에 필요할 때 사용한다.
Reference 관찰이나 candidate gap이 필요하면 [Reference Breakdown](../jjk-reference-breakdown/SKILL.md)을 사용한다.

## 네 가지 독립 평가

객관적 기술 검증과 주관적 시각 평가는 서로 다른 판정 언어를 사용한다.

### 1. Technical correctness

판정:
- `PASS`
- `FAIL`
- `NOT RUN`
- `INCONCLUSIVE`

변경에 필요한 C# compile, shader, missing script/shader/asset reference 검사를 선택한다.
Test 실행은 실제 discovery/execution과 executed count, failures, skipped, XML 또는 동등한 machine-readable 결과로 확인한다.
Process exit code 0이나 compile 성공만으로 테스트 PASS를 선언하지 않는다.
실행 0건은 실행한 테스트 PASS가 아니다.

실행 전 harness의 project/output 경로와 캡처 부작용을 확인한다.
별도 worktree라도 절대 출력 경로가 원본을 가리킬 수 있다.
대상 Editor/project와 실제 실행 명령을 기록하고 기존 사용자 Editor의 unsaved 작업을 방해하지 않는 실행 경로를 선택한다.
로그/결과에는 구별되는 run 식별자를 사용하고 임시 작업공간 정리 전에 필요한 근거를 보존한다.

### 2. Regression safety

판정:
- `PASS`
- `FAIL`
- `NOT RUN`
- `INCONCLUSIVE`

작업별 LOCK과 시작 snapshot/diff를 대조한다.
공유 runtime에 영향이 있으면 관련 gameplay 호출 경로도 검사한다.

변경에 관련된 spawn/cleanup, cancel, repeated cast, material/mesh 증가, camera ownership/restore를 평가한다.
Pixel diff가 필요하면 비교 구간, 시점, frame 수, alignment, tolerance, 측정 채널을 기록한다.
일부 구간의 차이 0을 전체 게임 회귀 없음으로 일반화하지 않는다.

전체 regression 필요성은 Unity Validation과 사용자 요청에 따라 판단하고 미실행 범위를 명시한다.

### 3. Visual similarity

판정:
- `CLOSER`
- `MIXED`
- `FARTHER`
- `INCONCLUSIVE`

Reference와 candidate의 다음 항목을 실제 화면에서 비교한다.

- motion language
- mass
- depth
- timing
- silhouette
- readability
- contrast
- asymmetry
- camera contribution
- environmental reaction

코드나 레이어 수가 다른 것을 시각 차이의 증거로 쓰지 않는다.
더 밝거나 더 화려한 것만으로 reference에 가까워졌다고 판정하지 않는다.
Reference/candidate가 없거나 특정 view가 가려져 있으면 해당 판단을 유보한다.

주요 판정마다 다음을 기록한다.

- evidence / timestamp
- confidence
- 가능한 counter-evidence

### 4. Art-direction quality

판정:
- `STRONGER`
- `MIXED`
- `WEAKER`
- `INCONCLUSIVE`

[VFX Direction](../../../docs/design/VFX_DIRECTION.md)의 정체성·임팩트 목표와 이번 visual target에 따라 판단한다.
실제 플레이 거리에서 주력/보조 요소의 위계, 본체 가림, 힘의 방향, 사건의 강조, 장면 적합성을 본다.

Reference 유사도와 게임에서의 가독성이 충돌하면 trade-off를 명시한다.
취향상의 새로운 방향을 QA 과정에서 임의 적용하지 않는다.
“더 reference와 비슷하다”를 자동으로 “게임 연출로 더 좋다”고 취급하지 않는다.

주요 판정마다 다음을 기록한다.

- evidence / timestamp
- confidence
- 가능한 counter-evidence

## 보고와 승인 상태

보고에는 네 영역의 판정·근거·미실행 범위와 다음을 각각 분리해 적는다.

- 개선점
- 퇴보점
- trade-off
- 미확인 사항

정확한 파일 경로, 비교 조건, 남은 문제, 다음 사용자 판단 지점을 포함한다.

Technical PASS는 Visual similarity나 Art-direction의 긍정 판정을 의미하지 않는다.
`CLOSER`나 `STRONGER`도 USER VERIFIED를 의미하지 않는다.
AI visual review는 USER VERIFIED가 아니다.

사용자가 해당 범위의 Play Mode / 영상을 직접 확인하고 명시적으로 승인한 경우에만 USER VERIFIED 근거가 된다.
기존 승인 항목을 일괄 pending으로 되돌리지 않고, 미승인 candidate도 기존 승인 범위에 합치지 않는다.
CODEX VALIDATED는 수행한 기술 검증 범위와 함께 표시하고 현재 handoff/task의 사용자 승인 상태는 근거 없이 승격하지 않는다.
