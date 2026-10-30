# CURRENT TASK — 잡기·긴급 탈출 USER VERIFIED 체크포인트 / 훈련 AI 설계 합의 대기

2026-10-30. `feat/shibuya-combat-mvp`. 이전 기준 `bd5acdcb22c35002317a438a2b0fd28986b53607`; 이 문서가 포함된 새 체크포인트는 Git 이력에서 확인한다. 사용자 승인에 따라 PASS4D.2+4E 코드/데이터/meta/테스트/보고서만 선택적으로 저장한다.

USER VERIFIED는 잡기의 입력·조작감/성공·실패 기본 동작/전투 연계와 긴급 탈출의 기본 방향 기상·입력·조작이다. 잡기15 HP/1.4m는 공통 MVP 초기값이다. 캐릭터별 잡기 모션·후속 피해/던지기·카메라/시네마틱 정책 및 기상 최종 애니메이션/VFX는 후속 설계다. 자동 검사 경계까지 사용자 수동 검증으로 확대하지 않는다.

기존 결과를 확인해 재사용했다: Grab 최신 고유41 PASS(4회 실행), Recovery 최신 고유37 PASS(7회 실행). 단일 전체 회귀 성공으로 합산하지 않는다. 원본 실패·플랫폼 수정·초기 쿨타임0 원인 미확정·이후 계측 통과와 PASS4A 미해결18건을 보존한다. 이번 저장 작업에서는 Unity 실행/전체 재검증/게임플레이 수치 변경을 하지 않는다.

자동 생성 `.slnx` 프로젝트 순서 변경은 저장 대상에서 제외해 로컬에 보존한다. QA 원본과 대용량 미디어/모델/오디오/레퍼런스/캐시는 커밋하지 않는다. 기존 F 아우라·카메라·고죠 자산, 대시·패링 승인과 가드 체력 비공개 정책을 유지한다.

**다음 작업 후보: 훈련 AI 설계.** 행동 규칙을 사용자와 합의하고 별도 PASS로 승인받은 뒤 구현한다. 체크포인트 저장 후 신규 기능은 시작하지 않는다. 아래는 이전 PASS 작성 당시 기록이다.

## 이전 PASS4E 구현·검증 기록

2026-10-30. `feat/shibuya-combat-mvp`, HEAD `bd5acdcb22c35002317a438a2b0fd28986b53607`. PASS4D.2와4E 모두 LOCAL UNCOMMITTED, staging·commit·push 없음.

PASS4E CODEX VALIDATED(targeted) / USER VERIFIED 대기. 기존 실제 다운·착지/minimum0.45/natural0.85/getup0.22를 재사용한다. 새 방향+Space만2.5m/0.30초 회피 기상, 지면0.04~0.18초 무적, 종료·중단 후 쿨타임1.0초. 카메라 수평 방향을 시작 시 고정하고 기존 Fighter motor에서 이동한다. 조기 입력/홀드는 버퍼링하지 않고 점프·추격과 분리한다. 이미 시작한 유한 수평 동작은 중력과 함께 끝나며 공중에서 새로 시작하지 못한다.

최신 고유37 PASS/0 FAIL/0 SKIP(신규20+선택 회귀17)는7회 실행 집계다. 원본 실패·계측·API 응답 시간 초과를 보존했다. 플랫폼 가장자리 중단을 보완했으며 임의 중간 높이 가정 대신 실제 중력/컨트롤러 이동 한도를 검증했다. 초기 쿨타임0 관측은 정확한 원인 미확정으로 남기고 이후 계측 통과와 구분한다. PASS4A18건은 해결로 소급하지 않는다.

현재 Console0/0, 과거 Pipeline error4는 별도. Play stopped/씬 clean/fps -1/Time.timeScale1/AutoTick true16ms/PlayUnfocused/runtime Mesh·Material0/0. 기존 미커밋 잡기/승인 기능/QA 보존, unity.slnx 자동 줄 순서 변경은 임의 복구하지 않았다.

사용자는 PASS4D.2 **구조·조작감**을 승인했다. 잡기 최종 연출/피해 구성/밀려남/모션과 PASS4E 시각·조작감은 새로 승인된 것이 아니다. `docs/SHIBUYA_PASS4E_REVIEW.md`, `unity/Logs/ShibuyaPass4EQA/20261030-quick-recovery/` 참조. 직접 Play 후 `JJK Game/시부야 PASS4E/긴급 탈출 검수 도구`에서 검수한다. 사용자 피드백을 기다리며 추가 구현/Git 저장은 시작하지 않는다.

## 이전 PASS4D.2 기록 (작성 당시 상태)

2026-10-30. `feat/shibuya-combat-mvp`, 실제 HEAD `bd5acdcb22c35002317a438a2b0fd28986b53607`.
LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기. 이번 사용자 요청으로 공통 잡기를 구현했으며 Git staging·commit·push는 하지 않았다.

유효 가드+새 LMB로만 잡기, 동시 버튼 우선, 패링 성공 후 RMB 유지 LMB 기본 반격 보존. 짧은 몸 전방 판정→접촉→피해1회→기존 motor 밀려남/경직→성공·실패 회복. 가드·패링만 우회하고 대시 무적/특수 방어를 존중한다. 기존 카메라·전투·아우라·잠금 데이터·씬·GUID 보존.

최신 고유41 PASS/0 FAIL/0 SKIP는 신규23+선택 회귀18의 4회 실행 집계다. 최초 캡처 기록 실패1건과 테스트 컴파일 오류2건은 수정됐고 원본을 보존했다. 전체41개 단일 성공 실행으로 표현하지 않는다. 최종 Console 현재 error0/warning1, 버퍼 과거 error2. Play stopped/씬 clean/fps -1/Time.timeScale1/AutoTick true16ms/PlayUnfocused, runtime Mesh/Material0/0. PASS4A 과거18건은 별도 미해결 이력이다.

`docs/SHIBUYA_PASS4D2_REVIEW.md`, `unity/Logs/ShibuyaPass4D2QA/20261030-grab/` 참조. 직접 Play 후 `JJK Game/시부야 PASS4D/잡기 검수 도구`에서 상황을 준비하고 Game View에서 직접 RMB 홀드+LMB로 검수한다. 손 접촉·모션/최종 VFX 품질은 USER VERIFIED 아님. 사용자 피드백을 기다리며 추가 기능/반전술식/Git 저장은 시작하지 않는다.

## 이전 체크포인트 및 PASS4D.1 기록 (작성 당시 상태)

2026-10-30. `feat/shibuya-combat-mvp`, 이전 기준 `1202e940176daec9fbb0b8027702f600b2f677d4`.
이번 체크포인트는 대시·가드·패링 관련 코드/데이터/meta/테스트/문서를 함께 보존한다. 새 HEAD는 Git 이력에서 확인한다. QA 영상·원본 로그/캐시·로컬 모델·오디오·레퍼런스는 로컬에 보존하며 커밋하지 않는다.

사용자가 직접 승인한 범위: **PASS4C.2 방향 대시·조작감**, **PASS4D.1 패링 구조·조작감 USER VERIFIED**. 가드 원통은 MVP 임시 수용이며 최종 VFX 승인이 아니다. 패링 최종 VFX·캐릭터별 모션은 후속 작업이다. 기존 가드 전체/자동 경계 검사를 사용자 검증으로 확대하지 않는다.
가드 체력 비공개/시각 위험 표현 정책 유지. 최신4D.1 고유30 PASS는 기존 복수 실행 결과를 재사용했다. 체크포인트 작업에서 Unity 테스트를 재실행하지 않았다. 원본 실패 기록과 PASS4A 미해결18건 유지.

**현재 추가 구현 없음. 잡기는 다음 PASS에서 별도 요청 후 시작한다.** 반전술식/회복 보너스/신체 강화/최종 캐릭터 연출도 자동으로 착수하지 않는다. 아래는4D.1 구현 당시 기록이다.

2026-10-30 KST. LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기.
새 우클릭0.12초/재시도0.50초/전방120도, 패링 우선→일반 가드. 성공은 HP·가드·주력 소모 없이 공격자0.35초 경직/1.2m 밀려남과 현재·예약 공격 취소. 방어자는 가드를 끝내고0.10초 뒤 수동 후속 행동을 선택한다. 홀드 자동 재패링/재가드/자동 반격 없음. Core Health는 변경하지 않았다.

최신 고유30 PASS/0 FAIL/0 SKIP는 신규12+기존 가드13+선택한 기존 회귀5의 복수 실행 결과. 단일 전체 실행이 아니다. 최초 거리 측정 실패(XML 보존)는 준비 전진0.52m를 분리해 실제 반동1.20000017m로 확인했다. 현재 C#/Shader 오류 없음, Console 현재 error0/warning1(font), Play stopped/씬 clean/fps -1/Time.timeScale1/패링 리소스0. 기존 PASS4A18건을 해결로 소급하지 않는다.

`docs/SHIBUYA_PASS4D1_REVIEW.md`, `unity/Logs/ShibuyaPass4D1QA/20261030-parry/`. 기존 메뉴 `JJK Game/시부야 PASS4D/가드 피격 검수 도구`의 반복 패링 연습을 켜면 Game View 예고 후 실제 더미가 공격한다. 창 닫기/중단/전투 종료 시 예약을 정리한다. 가드 잔량 HUD는 없다. 원속도 성공/이른 가드/늦은 피격 영상을 남겼다.

PASS4C.2 방향·속도 USER VERIFIED 유지. PASS4D.0 전체를 USER VERIFIED로 확대하지 않고 원통 외형의 임시 MVP 수용만 기록한다. 주력 아우라/카메라/대시·피해·HP/기존 작업/QA 보존. 잡기/반전술식/회복 보너스/신체 강화 및 Git staging/commit/push는 하지 않는다. 사용자 직접 검수 대기.

## 이전 PASS4D.0 기록 (아래 상태는 작성 당시)

2026-10-30 KST. LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기.
우클릭 홀드·몸 전방120도·체력100·종료 후1.2초 대기/20/s 회복·파괴0.8초·기본4타 가드 피해16/16/18/35. 가드 중 걷기40%, 공격/점프/대시/F 차단. 중단/실패 후 계속 누르는 입력은 자동 재가드하지 않는다. 별도 곡면 방어막의 안정/중간/위험 균열/파괴로 위험을 표현하며 게임 HUD 잔량 UI는 없다.

최신 고유33 PASS/0 FAIL/0 SKIP는 기존 대시20 + 가드13의 복수 실행 결과다. 단일 전체 성공으로 합산하지 않는다. 원본 실패 기록, 입력 Up 관측 동기화 근거, 수정한 프로필 제거 시 자세 NullReference를 보존했다. 전체 과거 전투 회귀/PASS4A18건은 재검증 완료로 선언하지 않는다.
현재 C#/Shader 오류 없음, Console 현재 error0/warning1, Pipeline 과거 error6 원인은 보고서 참조. Play stopped/씬 clean/fps -1/Time.timeScale1. Mesh/Material 정리 확인. Computer Use/포커스/AutoTick 변경 없음.

보고서 `docs/SHIBUYA_PASS4D_REVIEW.md`, QA·원속도 자료 `unity/Logs/ShibuyaPass4DQA/20261030-guard/`. 메뉴 `JJK Game/시부야 PASS4D/가드 피격 검수 도구` → 실제 더미 전/측/후방·파괴 공격을2초 뒤 실행한다. 사용자는 Game View로 돌아가 RMB로 검수한다. 가드 내부 수치는 Fighter 개발용 Inspector/QA 로그에서만 확인한다.

PASS4C.2 방향·속도는 사용자 직접 승인. 기존 대시5.25/3.60/3.30m、쿨타임0.45s、무적0.04~0.14s、카메라/락온/아우라/공중전/시네마틱/HP/피해·기존 미커밋 작업 보존. 패링·잡기·반전술식·신체 강화·후속 기능은 구현하지 않는다. staging/commit/push 등 Git 변경 작업 없이 직접 Play 검수를 기다린다.

## 이전 PASS4C.2 작업 이력 (아래 검수 대기 표시는 작성 당시)

2026-10-30 KST. 브랜치 `feat/shibuya-combat-mvp`.
보호 기준 HEAD `1202e940176daec9fbb0b8027702f600b2f677d4` (기존 PASS4B.4a USER VERIFIED).
현재 **LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기**.

## 구현 범위

- 방향+C의 지상 스텝. C만 누름/홀드/쿨타임 중 예약 반복 없음.
- 전방5.25m/0.22s, 측면3.60m/0.18s, 후방3.30m/0.20s. PASS4C.1에서 거리만1.5배 적용했다.
- 앞 성분 대각선은 전방(무적 없음), 후방 대각선은 후방 회피. 정규화하여 추가 이동 거리 없음.
- 측면/후방 일반 체술 회피는 시작 후0.04~0.14s 유지. 종료/중단 후 공통0.45s 쿨타임.
- 위 값은 새 `Shibuya Ground Dash.asset`의 한글 Inspector에서 조절하는 **실험값**이다.
- 락온·비락온 모두 시작 시 카메라의 수평 forward/right 기준. 타깃 위치는 이동 벡터에 사용하지 않으며 대시 중 카메라/대상 변화에도 경로 재추적 없음.
- 몸 바라보기는 별도: 비락온은 시작 몸 방향, 락온은 시작 타깃 방향 유지. 락온 선택/공격 보정/카메라 코드는 변경하지 않았다.
- 기존 ShibuyaFighter만 CharacterController 이동을 처리한다. 피해 guard는 Applied 피드백 앞에서 일반 체술만 차단한다.
- 공격/피격/다운/공중/추격/히트스톱/시네마틱/사망/종료 상태 금지.
- F 홀드 중 C 금지. 대시 중 새 F는 대시를 종료해 기존 충전을 우선한다.
- 주력 소모/공중 대시/대시 공격/공격 캔슬/고급 애니메이션/신규 주력 VFX 없음.

## 검증

PASS4C.2 최신: 단일 targeted20 PASS/0 FAIL/0 SKIP. 8방향 락온/비락온 일치, 고피치85/90도·후방 카메라, 근거리/원거리 대상 이동·실제 카메라 조작 중 경로 고정, 대시 후 락온 공격, 기존 대시 회귀. PASS4C.1 에셋 수치/Curve byte 동일. 최종 Play stopped/씬 clean/fps -1/Time.timeScale1, Console 현재 error0/warning1(font), 이전 timeout1건은 버퍼 보존. 보고서 `docs/SHIBUYA_PASS4C2_REVIEW.md`, QA·원속도5영상 `unity/Logs/ShibuyaPass4C2QA/20261030-camera-basis/`. 새 방향 USER VERIFIED 대기. 아래는 이전4C.1/4C 결과이며 합산하지 않는다.

PASS4C.1: `targeted/results.xml` 단일16 PASS/0 FAIL/0 SKIP, 별도 `far-framing/results.xml` 1 PASS. 실측 W5.25/A3.60/D3.60/S3.30000019m. 정상 종료/F 중단 쿨타임0.45s. 기존 런타임·방향·카메라 코드 SHA-256 동일. 최종 Play stopped/씬 clean/fps -1/Time.timeScale1, 현재 Console error0/warning0(이전 timeout1건·누적 font 경고는 Pipeline 버퍼 보존). 보고서: `docs/SHIBUYA_PASS4C1_REVIEW.md`. QA·원속도 영상: `unity/Logs/ShibuyaPass4C1QA/20261030-tuning/`. 아래20케이스는 이전 PASS4C 결과로 새 실행과 합산하지 않는다.

최초11개10 PASS/1 FAIL(테스트의 세션 반환형 검사 오류), 후속9 PASS, 리소스1 PASS, 공중/촬영2 PASS.
중복을 제외한 **최신 고유20 PASS / 0 FAIL / 0 SKIP**, 미디어 검사 포함. 단일 전체 회귀 실행이 아니다. 실패 XML과 기존 촬영은 삭제하지 않았다.
기본4타/Space 추격/공중 체술/락온 공격/시네마틱 복귀/F 충전의 관련 기존 검사만 재사용·targeted 검증했다.
과거 PASS4A 회귀18건의 전체 원인 해소를 선언하지 않는다.

상세: `docs/SHIBUYA_PASS4C_REVIEW.md`.
QA: `unity/Logs/ShibuyaPass4CQA/20261029-ground-dash/`.
최종 원속도 영상: `media-framed/front-free-W.mp4`, `diagonal-free-A.mp4`, `locked-D.mp4`, `runtime-camera-W.mp4`.
첫3개는 기술 촬영, 마지막은 실제 OrbitCamera 사용. 화면 캡처 표본의 한계와 타임스탬프를 기록했다.

## 보호 / 다음 단계

기존 승인 전투값, Gojo/VFX/애니메이션, 혼령 아우라·색·100→1.0/150→1.5/200→2.0 곡선, 주력/HUD 코드, 기존 Scene/Prefab/Packages/ProjectSettings/GUID를 보존한다.
신체 강화 패시브는 사용자 결정으로 계속 보류한다. 자동 테스트로 C 기능을 USER VERIFIED 처리하지 않는다.
이번에는 staging/commit/push/reset/clean/merge/worktree를 실행하지 않는다.

사용자가 기존 ShibuyaCombatMVP를 직접 Play하여 짧은 스텝의 거리/속도/회피 타이밍/카메라 체감/입력 우선순위를 확인할 때까지 추가 기능 구현 없이 대기한다.
