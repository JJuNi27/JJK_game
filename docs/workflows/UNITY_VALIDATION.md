# Unity 검증 Workflow

목표: 작은 수정마다 전체 검증을 반복해서 Codex 사용량을 낭비하지 않으면서 품질을 유지한다.

## 수정 전
실행:
- `git status -sb`
- `git diff --stat`

기존 로컬 변경사항을 먼저 확인한다.

## 작업 중
현재 변경을 반증할 수 있는 가장 싼 targeted validation부터 사용한다.

예:
- C# compile / static check
- focused Editor test
- 변경한 ability 하나의 preview
- cleanup / repeat-cast 단일 체크

작은 비주얼 수정마다 전체 Unity suite를 반복하지 않는다.

## 구현이 안정된 뒤
필요한 전체 regression을 **마지막에 한 번** 수행한다.

### 이번 Gojo 3차 polish의 Codex 보고
- focused checks: **4/4**
- combined targeted lab checks: **5/5**
- full Unity regression: **21/21**, 마지막에 1회 수행
- Python: **13/13**

이 결과는 현재 로컬 working tree에 대한 **CODEX VALIDATED** 기록이며 사용자 시각 승인을 의미하지 않는다.

## 다음 검토에서 특히 확인할 것
- Domain 종료 후 basic melee 실제 복구
- Red 속도 / 사거리 / cleanup / impact feel
- Purple travel range / hit / cleanup / caster readability
- Domain exit transition이 실제 영상에서 자연스럽게 이어지는지
- Unlimited Void nebula / White Blood / Cosmic Eye balance

## 시각 검증 상태 용어
사용자가 직접 확인하기 전에는 `USER VERIFIED`를 사용하지 않는다.

사용:
- CODEX VALIDATED
- AUTOMATED TEST PASSED
- CODEX PREVIEW
- PENDING USER VISUAL REVIEW

## Unity license 이력
Unity Personal은 활성화되어 있었지만 한 번 자동 shell에서
`No valid Unity Editor license found`
오류가 발생했다.

Hub-launched Editor가 authentication을 갱신한 뒤 자동 Unity 실행이 정상화됐다.

다시 발생하면:
- license를 자동 delete / return / reactivate하지 않는다.
- 정확한 failure log를 보존한다.
- Hub / Editor authentication 확인 후 재시도한다.

## Editor가 열려 있을 때
실제 Unity Editor가 이미 열려 있다면 unsaved work를 방해하지 않는다.
필요하면 safe test copy / 별도 validation path를 사용한다.

## 최종 보고에 포함
- 변경 파일
- targeted validation
- full test count / result
- 실패가 있다면 정확한 이유
- 사용자 시각 검토 대기 항목
- 명시적 승인 없는 commit / push 금지
