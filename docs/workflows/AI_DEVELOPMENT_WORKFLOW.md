# AI Development Workflow

## 목적과 책임

이 문서는 JJK_game의 Astra / Luna routing 단일 기준이다.
모델 구분은 프로젝트 운영 정책이며 자동 모델 전환 설정이 아니다.
현재 VFX 값, 승인 상태, 미해결 시각 문제는 이 문서에 복제하지 않는다.

| 기준 문서 | 소유하는 내용 |
|---|---|
| [AGENTS.md](../../AGENTS.md) | 절대 작업 규칙, 권한, 문서 우선순위 |
| [CURRENT_HANDOFF](../CURRENT_HANDOFF.md) / [CURRENT_TASK](../active/CURRENT_TASK.md) | 현재 상태, 실행 범위, 다음 작업 |
| [USER_VERIFIED_SETTINGS](../locked/USER_VERIFIED_SETTINGS.md) | 사용자 승인 값과 동작 |
| [GOJO_VFX](../architecture/GOJO_VFX.md) 등 architecture | 실제 기술 구조와 소유권 |
| [VFX_DIRECTION](../design/VFX_DIRECTION.md) | 장기 시각 목표 |
| [UNITY_VALIDATION](UNITY_VALIDATION.md) | Unity 실행·검증 절차 |
| 이 문서 | 모델 routing, 작업 전달, 도구 연결 경계 |

과거 Gate 완료와 최신 candidate 승인 상태를 섞지 않는다.
문서의 오래된 “최신 작업” 문구보다 AGENTS의 우선순위와 실제 현재 상태를 확인한다.
이 workflow를 도입했다는 이유로 기존 technical checkpoint가 최종 visual baseline이 되지는 않는다.

## 모델 routing

| 모델 | 맡길 작업 |
|---|---|
| Astra | 모호한 문제 분석, architecture, shader / VFX 구조 결정, reference 해석, 원인 불명 버그, 위험한 수정, 중요한 review |
| Luna | 명세가 확정된 구현, 반복 수정, repository 탐색, 테스트 / QA 실행, 로그 / 보고서 정리, 단순 wiring |

QA 자료 수집과 확정된 기준 적용은 Luna로 진행할 수 있다.
기준 자체의 해석이나 상충하는 art direction 결정은 Astra 대상으로 분리한다.

실제 모델 선택은 User / ChatGPT 단계에서 결정한다.
Skill 실행을 모델 전환 완료로 표현하지 않는다.

지정된 모델이 현재 Codex 환경에서 제공되지 않는 경우 존재한다고 가정하거나 임의 대체하지 않는다.
사용자에게 unavailable 상태와 가능한 대안을 보고하고 선택을 기다린다.

운영 흐름:

```text
User
→ ChatGPT 방향 / spec / 모델 선택
→ Codex Astra 또는 Luna
→ Unity
→ QA
→ User 최종 시각 검수
```

분석·문서 전용 작업은 Unity 실행 단계 없이 끝낼 수 있다.
자동 모델 전환, subagent hierarchy, Luna orchestrator를 추가하지 않는다.

## Luna → Astra escalation

Luna가 명세만으로 결정할 수 없는 구조적 문제를 발견하면 해당 변경을 보류하고 다음을 보고한다.

1. 관찰 근거: 실제 코드 경로, 로그, timestamp 등.
2. 결정이 필요한 지점: 확정된 명세로 해결되지 않는 이유.
3. 선택지: 가능한 최소 대안.
4. 각 선택지의 영향: LOCK, architecture, visual, regression 범위.

이를 **ASTRA ESCALATION**으로 표시한다.
이미 승인된 구조를 임의로 재설계하지 않는다.

결정과 독립적으로 가능한 읽기·검증·정리는 기존 허용 범위에서 이어갈 수 있다.
결정이 내려지면 그 결과와 acceptance criteria를 작업 명세에 반영하고 구현을 재개한다.

## Astra → Luna hand-back

Astra가 다음을 확정한 뒤 이후 작업이 bounded implementation / QA가 되면 다음 실행은 원칙적으로 Luna 대상으로 되돌린다.

- 구조 결정
- Change Contract 또는 동등한 implementation contract
- acceptance criteria
- 보호 범위 / LOCK 영향

Astra를 관성적으로 계속 사용하지 않는다.

다만 구현 중 새로운 구조적 모호성, reference 해석 충돌, art-direction 결정, LOCK 영향이 발견되면 다시 Astra로 escalation할 수 있다.

## Skill 연결과 산출물

| Skill | 입력 → 출력 | 소유하지 않는 일 |
|---|---|---|
| [JJK VFX Production](../../.agents/skills/jjk-vfx-production/SKILL.md) | 승인된 명세·LOCK → 구현 후보·비교 자료·변경 인계 | 독자적인 승인 규칙, 모델 정책 |
| [JJK Reference Breakdown](../../.agents/skills/jjk-reference-breakdown/SKILL.md) | source·candidate → 관찰·시각 목표·gap, 요청 시 별도 구현 제안 | 자동 구현·튜닝 |
| [JJK Visual QA](../../.agents/skills/jjk-visual-qa/SKILL.md) | 구현·reference·검증 결과 → 네 영역의 독립 평가 | 사용자 시각 승인, 자동 polish |

작업에 필요한 Skill만 사용한다.
분석만 요청된 작업에 제작 절차를 강제하지 않는다.

실제 판단 기준과 상세 평가 항목은 각 Skill에 두며 이 문서에는 복제하지 않는다.
재사용 문서와 작업별 결과를 구분한다.
관찰·QA 보고서에는 source, 후보 버전, 실행 조건, 증거 위치를 남기고 미디어는 기존 reference/QA 보존 정책을 따른다.

## Worktree와 증거 경로

분리 작업은 승인된 기준 commit, 대상 branch, worktree 경로를 명시한다.
원본과 새 작업공간의 root / HEAD / index / dirty 상태를 구분하고 필요한 보호 범위의 시작·종료 상태를 비교한다.

Worktree 분리는 코드 경로만 분리한다.
테스트의 절대 출력 경로나 연결된 Editor가 원본을 가리키면 격리가 성립하지 않는다.

실행 전 project path와 출력 위치를 확인하는 절차는 Visual QA Skill을 사용한다.
격리를 해결하지 못했으면 실행을 보류하고 정확한 경로 의존성을 보고한다.

Local-only reference와 QA가 새 worktree에 없으면 누락으로 기록하며 존재를 가정하거나 자동으로 대량 복사하지 않는다.

## 향후 도구 연결 지점

현재 문서는 연결 위치만 정의한다.
Unity MCP, Gemini/Antigravity CLI, 외부 plugin 설치·설정은 포함하지 않는다.

- Unity MCP: Production의 실제 프로젝트 확인/preview 및 Visual QA의 console/test/capture 수집 수단. 연결 시 capability와 대상 project/Editor를 확인하고 기존 CLI 또는 검증된 harness를 대체할 수 있는지 평가한다.
- Gemini/Antigravity: Reference Breakdown의 보조 관찰과 Visual QA의 독립 비교 의견. timestamp 근거와 불확실성을 결과에 유지하며 사용자 승인으로 취급하지 않는다.
- 도구 사용이 AGENTS의 권한이나 작업 범위를 확대하지 않는다. 외부 자료 전달이 필요하면 승인된 자료 범위를 확인한다.
- 설치·인증·비밀값은 Skill 본문에 넣지 않는다.
- 도구가 없어도 가능한 관찰·분석·기존 검증을 수행한다. 연결 실패를 VFX나 Scene을 임의 수정하는 이유로 삼지 않는다.

## 첫 실제 시험 권장 범위

사용자가 선택한 짧은 reference 구간과 기존 candidate motion을 대상으로 Reference Breakdown의 A–D만 수행한다.
같은 원본 timestamp 체계와 분석 축으로 관찰·gap을 만들고 구현 제안/코드 수정/재렌더는 하지 않는다.

이어 Visual QA로 기존 증거를 네 영역에 분류하고, 자료가 없는 항목은 `NOT RUN` 또는 `INCONCLUSIVE`로 남긴다.

Production Skill의 실제 구현 시험은 이 결과에서 작업 명세가 정해진 뒤 별도로 진행한다.
