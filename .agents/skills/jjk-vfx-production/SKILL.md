---
name: jjk-vfx-production
description: JJK_game의 승인된 VFX 구현·refinement·integration 작업을 기존 구조와 보호 범위 안에서 수행한다. Reference 분석만 또는 QA만 요청된 경우에는 해당 전용 Skill을 사용한다.
---

# JJK VFX Production

## 시작점과 규칙 출처

[AGENTS.md](../../../AGENTS.md)를 작업 안전·권한·문서 우선순위의 기준으로 사용한다.
아래는 그 규칙을 적용하는 제작 절차이며 별도의 정책 원본이 아니다.
모델 선택이나 구조적 판단의 escalation은 [AI Development Workflow](../../../docs/workflows/AI_DEVELOPMENT_WORKFLOW.md)를 따른다.

## 1. 실제 상태와 작업 범위 복구

- 실제 작업 디렉터리, repository root, branch, HEAD, `git status -sb`, staged/unstaged diff를 확인한다. 이전 완료 결과가 있으면 재구현 전에 읽는다.
- [CURRENT_HANDOFF](../../../docs/CURRENT_HANDOFF.md), [CURRENT_TASK](../../../docs/active/CURRENT_TASK.md), [USER_VERIFIED_SETTINGS](../../../docs/locked/USER_VERIFIED_SETTINGS.md)를 순서대로 읽고, AGENTS가 안내하는 관련 architecture/design 문서만 추가로 읽는다. 고죠 기술이면 [GOJO_VFX](../../../docs/architecture/GOJO_VFX.md)를 포함한다.
- 이번 요청의 변경 허용 대상, LOCK, 기존 사용자 승인 범위, 미승인 후보, 완료된 단계, 산출물을 짧게 추출한다. 값은 해당 문서·실제 자산에서 읽고 이 Skill에 저장하지 않는다.
- unrelated dirty 파일과 이번 작업 파일을 구분한다. 같은 파일에 기존 변경이 섞여 있으면 시작 diff를 기준으로 보존 범위를 식별한다. 필요에 맞게 보호 파일 hash 또는 diff를 확보한다.
- 문서의 과거 이력이나 테스트 통과를 새 변경 권한으로 해석하지 않는다. 사용자 요청과 현재 상태로 작업 범위를 확정한다.

## 2. 구현보다 먼저 화면과 구조 확인

현재 runtime/profile/shader 연결과 실제 렌더를 함께 확인한다. 코드에 기능이 있다는 사실만으로 화면에서 읽힌다고 판단하지 않는다.
관련 reference가 지정되면 [Reference Breakdown](../jjk-reference-breakdown/SKILL.md)의 관찰·시각 목표·candidate gap 결과를 활용한다. 분석이 이미 있으면 source와 후보 버전이 일치하는지 확인해 재사용한다.

관찰된 gap마다 이번에 바꿀 변수, 보존할 요소, 비교 방법을 정한다.
[VFX_DIRECTION](../../../docs/design/VFX_DIRECTION.md)의 장기 목표를 참고하되 현재 요청 밖의 camera, environment, audio 작업까지 확장하지 않는다.

## 3. CHANGE CONTRACT

모호한 visual 작업은 구현 전에 짧은 Change Contract를 작성한다.

- `Target gap`: 이번 변경이 해결할 관찰된 gap
- `Allowed scope`: 수정 가능한 파일/시스템/표현 범위
- `Must preserve`: LOCK, USER VERIFIED 범위, 회귀 금지 항목
- `Expected visible change`: 실제 화면에서 달라져야 하는 현상
- `Failure condition`: 실패로 판단할 시각/기술적 결과
- `Required evidence`: A/B, view, timestamp, targeted test 등

Contract는 현재 문서와 사용자 요청에서 파생한다.
Skill 안에 특정 Purple 수치나 임시 후보값을 하드코딩하지 않는다.

Contract가 불충분해 구현 방향이 여러 갈래로 갈리면 임의로 선택하지 않고 AI Development Workflow의 escalation 절차를 따른다.

## 4. 기존 구조 안에서 최소 변경

기존 sampling, profile, particle/mesh, lifecycle 구조를 먼저 재사용한다.
smallest safe change는 시각 목표를 실제로 충족하면서 무관한 구조 변경을 최소화하는 뜻이다.

Exploration은 기존 baseline을 덮어쓰지 않는 별도 후보로 구성한다.
opt-in이 필요한 후보는 저장 상태 기본 OFF와 preview 활성 상태를 구분한다.
승인된 production integration에는 승인 범위를 따른다.
새 profile이 필요하면 AGENTS의 Data ownership·Inspector 규칙을 적용한다.

### STOP / ASTRA ESCALATION

다음 중 하나가 발생하면 해당 변경을 중단하고 임의로 확장하지 않는다.

- 기존 Gap 설명과 충돌하는 새 evidence 발견
- LOCK 변경 필요
- architecture 변경 필요
- 허용 범위 밖 subsystem 변경 필요
- acceptance criteria만으로 결정할 수 없는 art-direction 선택 필요
- 해결책이 기존 Change Contract를 벗어남

이 경우 [AI Development Workflow](../../../docs/workflows/AI_DEVELOPMENT_WORKFLOW.md)의 `ASTRA ESCALATION` 형식으로 다음을 보고한다.

1. 관찰 근거
2. 결정이 필요한 지점
3. 최소 선택지
4. 각 선택지의 영향

결정과 독립적으로 가능한 읽기·검증·정리는 기존 허용 범위에서 계속할 수 있다.

## 5. 비교와 검증

동일 카메라·시점·재생 조건의 baseline/candidate A/B를 준비한다.
가독성 질문에 필요한 뷰와 구간만 선택하고 기존 유효 결과를 재사용한다.
[Visual QA](../jjk-visual-qa/SKILL.md)에 따라 기술·회귀·시각 유사도·art direction을 각각 평가한다.

실행 범위는 [Unity Validation](../../../docs/workflows/UNITY_VALIDATION.md)과 이번 요청을 따른다.
변경을 반증할 targeted 검사를 먼저 선택하고 필요한 regression은 안정된 결과에 수행한다.

변경 영향에 따라 spawn/dispose, cancel, repeated cast, material/mesh, camera ownership/restore를 확인한다.
변경하지 않은 gameplay도 공유 경로에 영향이 있을 때 검사한다.

## 6. 종료와 인계

변경 파일, 실제 비교 자료, 실행된 검사와 실패 수, 보호 범위 확인, 개선·퇴보·한계를 보고한다.
기술 검증 상태와 사용자 시각 승인 상태를 분리하고 승인 표현은 Visual QA의 상태 기록 절차를 따른다.

Git 전달 작업을 요청받았다면 AGENTS의 명시 승인 조건과 실제 승인 범위를 확인한다.
구현/QA 완료를 commit/push 승인으로 취급하지 않는다.

완료 후 Change Contract의 각 항목이 실제 결과와 일치했는지 짧게 대조한다.
