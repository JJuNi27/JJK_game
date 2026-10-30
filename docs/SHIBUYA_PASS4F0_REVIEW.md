# 시부야 PASS4F.0 — 공격형 훈련 AI

2026-10-30. 브랜치 `feat/shibuya-combat-mvp`, 구현 기준 HEAD `0cbaed7ea07a254a252e09e36d3072a7913da678`.

## 사용자 승인과 구현 범위

USER VERIFIED: 플레이어에게 접근, 방향 정렬, 기존 Fighter의 1~4타 공격, 공격 후 휴식·재접근 및 기본 조작감. 장애물 우회, 장시간 안정성, 전체 경계 조건과 최종 난이도는 미승인이다. 자동 검증 경계 조건은 CODEX VALIDATED와 구분한다.

`ShibuyaTrainingAIProfile`이 판단 수치를 소유하고 `ShibuyaTrainingAI`는 의도만 제출한다. `ShibuyaTrainingFighterRuntime`의 좁은 API가 안전한 정지 회전, 지상 예약 가능 조회, 새 묶음 시작과 자신이 소유한 예약 철회를 제공한다. 기존 Fighter Update에는 회전 의도 처리1줄을 추가했다. 실제 CharacterController 이동, 충돌, 피해, 피격은 기존 Fighter/Health가 소유한다. AI의 직접 위치/회전/체력 조작이나 입력 읽기는 없다.

상태는 Disabled/Idle/Approach/FaceTarget/Attack/Recovery/UnableToAct/Blocked다. 새 Play/재시작은 AI OFF/수동 더미다. Editor 메뉴 ON에서만 런타임 컴포넌트를 추가한다. OFF는 미래 예약과 이동·회전 의도를 철회하되 현재 타격·피격은 유지한다. 비활성화/파괴 시 이벤트 구독을 해제한다.

## 데이터와 실제 경로

Data Asset: `unity/Assets/Shibuya/Data/Shibuya Training AI.asset`.

| 설정 | 기본값 |
|---|---|
| 사용 허용 / 실제 ON | true / 기본 OFF |
| 탐색 | 10m |
| 접근 시작 / 종료 | 2.30 / 1.55m |
| 거리·경로 판단 | 0.15초 |
| 방향 허용각 / 반응 | 20도 / 0.20초 |
| 묶음 후 휴식 | 0.90~1.50초 |
| 1/2/3/4타 가중치 | 25/35/25/15 |
| 달리기 | OFF |
| 지상 공격 높이 차 | 1.40m |
| 고정 난수 / 시드 | OFF / 1729 |

실제 필드는 한국어 CustomEditor/SerializedProperty/Undo로 편집한다. 확률은 가중치 합으로 정규화하고 유효하지 않은 합계는1타로 대체한다. System.Random을 사용해 UnityEngine.Random을 변경하지 않는다. 직선 Raycast/CapsuleCast가 막히면 정지하며 NavMesh/우회는 없다. 실제 도착 제한과 안전 상태는 매 프레임, 거리·경로 판단은 설정 주기로 갱신한다.

실제 공격은 Fighter.TryAttack(Default) → 기존 판정 → Health/DamageContext/가드/패링이다. 공개된 기존 예약 창의 중간에서 한 단계당 후속 요청1회만 제출한다. 기존 피해·모션·입력 버퍼·추적·넉백을 변경하지 않는다. 국소 히트스톱은 예약을 보존하고, 피격·다운·잡기·긴급 탈출·시네마틱 등은 기존 상태가 우선한다. AI가 방어·패링·잡기·대시·술식·공중 추격을 스스로 실행하는 기능은4F.0에 없다.

## 원본 검증 결과

QA 루트: `unity/Logs/ShibuyaPass4F0QA/20261030-training-ai/`. 각 실행의 results.xml/execution.jsonl/discovery.json과 최초 실패 trace를 보존한다.

| 실행 | PASS | FAIL | SKIP |
|---|---:|---:|---:|
| initial | 10 | 1 | 0 |
| group-diagnostic | 0 | 1 | 0 |
| reservation-final | 3 | 0 | 0 |
| state-and-media | 5 | 1 | 0 |
| media-controlled | 1 | 0 | 0 |
| selected-regression | 16 | 0 | 0 |
| final-decision-contract | 3 | 0 | 0 |

최신 고유 케이스별 **34 PASS/0 FAIL/0 SKIP = 신규18+선택 회귀16**. 7회 실행 집계이지 단일34개 성공 실행이 아니다. `latest-unique-results.json`과 XML을 체크포인트 전에 다시 대조했다. 원본 테스트 기대값을 낮추지 않았다. PASS4A 과거 미해결18건은 별도 이력으로 유지한다.

실측: 그룹별 실제 단계1 / 1,2 / 1,2,3 / 1,2,3,4, 요청 각각1/2/3/4회, 피해20/40/60/85, 거절0. 4타 Fighter 시각 예3.833/4.233/4.584/4.951초. 접근 종료1.55m(검사 허용오차0.04m), 2.0m 유지/2.4m 재접근. 시드1729의2,000표본 비율23.40/34.95/26.15/15.50%, 반복 재현 및 전역 난수 불변. 휴식 검사 복제 데이터0.40~0.55초에서0.421초 관측했으며 저장 기본 휴식0.90~1.50초는 유지했다.

실제 가드 HP 보호, 패링 중단·예약 정리·재접근, 측면 대시 미적중, 실제 다운·방향 기상·잡기·시네마틱 상태 소유권, 재시작 기본OFF와 이벤트 정리를 검사했다. 선택 회귀16건은 기본/W/S4타, 방향 피격, 국소 히트스톱·버퍼, 점프·추격, 시네마틱 카메라 복귀, F 자원·아우라·HUD·승패/재시작, 8방향 대시, 가드 파괴·패링·잡기·자연 기상이다. 전체 대규모 회귀는 반복하지 않았다.

## 실패·누적 오류 분류와 영향

1. **실제 AI 예약 결함:** 기존 창의 첫 순간 예약이 공격 종료 처리 프레임 직전에 만료될 수 있었다. 0.4초 공격이 elapsed0.399925에서 다음 약16ms 프레임으로 넘어가는 경계를 계측했다. AI만 같은 창의 중간 예약으로 변경했고 관련3검사와 마지막 그룹 검사 통과. 기존 플레이어 버퍼/게임플레이 값은 유지했다.
2. **촬영 절차 실패:** HP100 플레이어가 반복 공격에 실제 사망해 다음 촬영 Pair 준비가 타임아웃됐다. 한 성공 그룹 뒤 AI OFF로 촬영을 제어했다. 원래 피해·승패를 바꾸지 않았으며 새 촬영 검사 통과. 원본 실패 영상/XML 보존.
3. **CLI 호출 오류19건:** `script recompile` 미등록 명령2건, `script recompile_status` 미등록 명령17건. 프로젝트의 실제 컴파일 오류가 아닌 CLI/서버 명령 이름 호환 문제다. 이후 지원되는 recompile/MCP로 컴파일 정상 확인.
4. **QA discovery 오류1909건:** 필터 구분자를 잘못 사용해0개 탐색 후 콜백이 예외를 던졌다. Unity TestListJob이 콜백 뒤 구독 해제하는 구조여서 같은 예외가 반복됐다. 검사가 실행 중이지 않음을 확인하고 이번 임시 콜백3개만 해제했다. 후속 탐색/검증 정상. 미시작 폴더 reservation-fix/reservation-fix-v2/reservation-fix-single은 검사 성공 수에 넣지 않는다.

따라서 Pipeline 누적1928건은 위 도구/QA 호출19+1909다. 게임플레이 예외나 C# 컴파일 오류1928건으로 해석하지 않는다. 실패한 탐색은 검사를 시작하지 못했고 성공 실행에 합산하지 않았다. 실제 예약 실패와 촬영 실패는 XML의 실패 이력으로 별도 보존하며 최신 케이스 재검증과 구분한다. 34개 최신 성공을 무효화하는 미해결 중대 오류는 해당 원본 대조에서 확인하지 못했다. 장시간/전체 경계 안정성이 보장된다는 뜻은 아니다.

`console-history-summary.json`, `editor-final.log`, `discovery-editor-tail.txt`, 최초 XML/trace를 보존한다. 마지막 Editor Console error0/warning1(`Deleting invalid font reference.`), Pipeline 누적 error1928을 함께 기록한다. Console 전체 이력을0으로 표현하지 않는다.

## 환경·보호·리소스

승인된 CLI self-update 실제 버전1.0.0-beta.15, 프로젝트 Pipeline0.7.0-exp.1 유지. Play stopped/scene clean/fps -1/Time.timeScale1/AutoTick true16ms/GameView PlayUnfocused. 종료 후 런타임 AI0, 비에셋 Shibuya Mesh/Material0/0 기록 확인. 테스트 fps는 복구했고 AutoTick은 바꾸지 않았다. Computer Use/강제 포커스 전환 없음.

새 AI는 프레임별 GameObject/Mesh/Material을 만들지 않는다. 직선 쿼리를 판단 주기로 수행하지만 다수 AI·군집·도시 경로 CPU 성능은 미검증이다. 기본 피해20/20/20/25, Dummy HP1000/Player HP100, 기존 씬·프리팹·카메라·전투·Gojo/VFX·아우라·GUID·Packages/ProjectSettings는 SHA 대조로 보존 확인한다.

## 직접 검수·영상

Play → `JJK Game/시부야 PASS4F/공격형 훈련 AI ON·OFF`. 상태/한국어 프로필은 같은 경로 `훈련 AI 현재 상태·설정`. 상태 창 종료는 연결 AI OFF다. 필요하면 ESC로 커서를 풀고 메뉴 선택 후 Game View로 돌아간다. 새 Play/전투 재시작은 AI OFF/수동 더미다. 기존 수동 검수 도구는 AI OFF에서 사용한다.

원속도 영상(약5초/960×720/VFR): QA 루트 `media/approach-combo-original-speed.mp4`, `media/parry-resume-original-speed.mp4`, `media/ai-off-original-speed.mp4`. 접근·4타는 HP100→15, 패링 후 복귀 영상은 실제 패링1회 뒤4타를 확인했다. PNG/타임스탬프는 `media-controlled/legacy/ShibuyaPass4B3QA/4F0-media/`다. 실제 Play Mode 자동 카메라 렌더이며 HUD·오디오·사용자 직접 조작 녹화가 아니다. 영상은 마지막 거리 판단 주기 연결 전 촬영, 변경 후 접근·그룹 검사는 별도 통과했다.

## 파일과 체크포인트

신규7개와 각각 meta: Scripts/ShibuyaTrainingAIProfile.cs, ShibuyaTrainingAI.cs, ShibuyaTrainingFighterRuntime.cs; Editor/ShibuyaTrainingAIInspector.cs, ShibuyaTrainingAITool.cs, ShibuyaPass4F0Tests.cs; Data/Shibuya Training AI.asset (`unity/Assets/Shibuya/` 기준).

기존 코드2개는 Fighter 회전 호출1줄, TestRunner의 PASS4F 출력 허용이다. CURRENT_HANDOFF/CURRENT_TASK/이 보고서/USER_VERIFIED_SETTINGS는 이번 승인 범위만 반영한다. 이전 문서의 새4F.0 구간이 한글 `?`로 저장된 것을 체크포인트 검토에서 발견해 UTF-8로 재작성했다. 변경 전 문서 바이트는 로컬 QA에 별도 보존했고 이전 PASS 본문은 유지한다. 전투 로직은 이번 체크포인트에서 변경하지 않았다.

사용자 `.slnx` 자동 순서 변경은 시작 바이트 그대로 제외·보존한다. 대용량 QA/XML/trace/영상·로컬 모델/오디오/레퍼런스/캐시는 커밋하지 않는다. 체크포인트에 PASS4F.1 기능은 포함하지 않는다. 후속4F.1은 push 확인 후 구현·검증하고 별도 승인 전 Git 저장하지 않는다.
