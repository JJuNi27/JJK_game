# CURRENT TASK — PASS4F.1 MVP 임시 수용 체크포인트 / AI 개선 보류

## PASS4F.1 체크포인트 — USER VERIFIED / MVP 임시 수용 (2026-10-31)

브랜치 `feat/shibuya-combat-mvp`. 이전 공격형 AI 체크포인트는 `9568855d806c29ad66375ac470a6d69c3482205c`다. 이번 문서가 포함된 새 방어형 AI 체크포인트의 SHA는 현재 브랜치 Git 이력에서 확인한다. 이번 작업은 관련 파일의 선택적 commit/push와 문서 정리만 수행하며 게임플레이·AI·밸런스는 수정하지 않는다.

사용자는 직접 플레이한 뒤 움직임이 다소 어색하지만 AI는 추후 개선하기로 하고 **현재 MVP 수준을 임시 수용**했다. USER VERIFIED 범위는 **AI 모드 선택·기본 동작, 공격형/방어형 AI의 현재 기본 구조, 현재 구현을 보존하고 다음 작업으로 넘어가는 결정**이다. 자연스러운 이동, 공격·방어 전환의 완성도, 대전 난이도·재미, 패링 성공률/반응 정확도 최종 밸런스, 종합 모드 공격 적극성, 캐릭터별 성향, 장시간 안정성과 전체 경계 조건은 최종 승인하지 않았다. 자동 검증 세부 행동을 사용자 직접 승인으로 확대하지 않는다.

**검수 UX:** AI는 기본 OFF이며 모드 선택과 ON/OFF는 별도다. Play → `JJK Game/시부야 PASS4F/모드`에서 선택 → 기존 `공격형 훈련 AI ON·OFF`로 실제 활성화한다. 이름과 무관하게 선택한 모드로 동작한다. `훈련 AI 현재 상태·설정`에서 실제 ON/Running과 선택 모드를 구분해 확인한다. 초기 “AI가 움직이지 않는다” 보고를 모드 선택만으로 자동 활성화된 것으로 해석하지 않으며, 확인되지 않은 고장 원인을 단정하지 않는다. 상태 창을 닫으면 연결 AI OFF, 새 Play/재시작도 기본 OFF다.

기존 검증을 재사용했다. PASS4F.1 **최신 고유39 PASS/0 FAIL/0 SKIP는6회 실행 집계**(신규23+선택 회귀16)이며 단일 전체 실행 성공이 아니다. 최초 실패·버퍼 만료 진단·QA 입력 시점 수정·증거 미완료6건의 별도 재검증을 원본 XML/trace/영상과 함께 유지한다. 마지막 구현 검증 기록은 컴파일 정상/Editor Console error0·폰트warning1이다. 이는 이번 Git 작업에서 새로 실행한 Unity 검사 결과가 아니다.

PASS4F.0의 최신 고유34 PASS(7회 실행), 이전 도구/QA 오류1928건(CLI 명령 호환19+탐색 콜백 반복1909), PASS4F.1 QA 코루틴 CS0161 수정 이력1건을 구분해 보존한다. PASS4A 과거 미해결18건은 해결로 소급하지 않는다. 기존 QA565개를 포함한 로컬 원본과 실패 기록은 삭제·변경하지 않는다. `.slnx` 자동 순서 변경은 staging 없이 로컬에 보존하고 QA/미디어/캐시/로컬 모델·오디오·레퍼런스는 커밋에서 제외한다.

### 후속 AI 개선 목록 — 이번에는 구현하지 않음

1. 접근·거리 조절의 자연스러움
2. 공격과 방어 사이 행동 전환
3. 종합 모드의 과도한 수동성
4. 패링·가드·회피 반응 타이밍
5. 반복적이고 기계적인 행동 패턴
6. 캐릭터별 전투 성향과 난이도
7. 장애물 회피 및 고급 이동
8. 공격 예고 가독성과 실제 전투 밸런스

위 목록은 후속 과제이며 신규 구현 승인이 아니다. 기존 공격형 AI와 고죠/VFX·승인 아우라·카메라·전투 Data Asset·사용자 조절값을 보존한다. 저장 후 추가 기능은 시작하지 않는다. 상세 구현·실행별 결과는 `docs/SHIBUYA_PASS4F1_REVIEW.md`를 따른다. 아래 LOCAL UNCOMMITTED/USER VERIFIED 대기 표기는 이전 구현 당시 기록이다.

## 최신 로컬 — PASS4F.1 방어형 훈련 AI / 사용자 검수 대기

2026-10-30. `feat/shibuya-combat-mvp`, 로컬·원격 체크포인트 HEAD `9568855d806c29ad66375ac470a6d69c3482205c`. PASS4F.0은 사용자 승인 범위를 문서화해20개 파일로 일반 commit/push했고 실제 원격 HEAD 일치를 확인했다. 현재 PASS4F.1은 **LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기**이며 staging·commit·push하지 않았다.

공격형/가드 연습/회피 연습/패링 연습/종합 모드를 제공한다. 현재 실제 공격 관측→0.16~0.30초 반응→가능 행동의 가중치 선택→기존 SetGuardHeld/TryGroundDash 실행이다. 미래 입력/예약이나 플레이어 마우스·키를 읽지 않는다. 진행 중 정상 콤보와 기존 상태 소유권이 우선한다. 공격형의 기존 경로·수치는 보존했다. 새 방어 설정은 별도 한국어 Defense Profile이며 기존 AI Profile이 참조한다.

최신 고유 **39 PASS/0 FAIL/0 SKIP(신규23+선택 회귀16), 6회 실행 집계**다. 단일39개 전체 실행 성공이 아니다. QA 공격자의 첫 예약 경계에서 발생한 후속 입력 만료를 계측하고 QA 입력만 허용 창 중간으로 옮겼다. 원래 게임플레이 값·기대값은 유지했다. 실패 뒤 PASS였으나 증거가 없던6건은 본문 완료 기록을 붙여 별도6 PASS를 확인했으며 원본 XML을 보존한다. PASS4A 과거 미해결18건은 해결로 변경하지 않는다.

현재 컴파일 정상/Editor Console error0·warning1(font). 이번 Pipeline 세션 error1은 수정한 테스트 코루틴 CS0161이며, 이전4F.0 도구/QA1928건(CLI19+탐색1909)과 구분한다. 재컴파일 중 Pipeline 연결 실패1회는 검사가 미시작임을 확인하고 연결 복구 뒤1회 시작했다. Play stopped/씬 clean/fps -1/Time.timeScale1/AutoTick true16ms/PlayUnfocused, 런타임 AI0 및 Shibuya Mesh·Material0/0이다.

메뉴: Play → `JJK Game/시부야 PASS4F/모드`에서 모드 선택 → 기존 `공격형 훈련 AI ON·OFF`로 ON. 모드 선택만으로 자동 ON하지 않는다. `훈련 AI 현재 상태·설정`에서 모드·마지막 선택·실제 결과·카운터를 확인한다. 가드 체력은 HUD에 표시하지 않는다. 새 Play/재시작은 기존처럼 AI OFF다.

짧은 기본 공격에는 반응이 늦어 맞을 수 있고 패링 모드도 성공을 보장하지 않는다. 종합 모드는 계속 압박받으면 수동적이며, 영상에서도1패링/5반응 실패를 보였다. 무입력 구간에는 기존 공격이 가능함을 별도 검사했다. 자동 난이도 튜닝이나 강제 반격을 추가하지 않는다. 최종 손맛·난이도는 사용자 직접 검수 대상이다.

보고서 `docs/SHIBUYA_PASS4F1_REVIEW.md`, 원본 `unity/Logs/ShibuyaPass4F1QA/20261030-defense-ai/`, 원속도7개 `media/`. 기존4F.0 QA565개·.slnx 바이트·Gojo/VFX·아우라·카메라·전투값·Scene/Prefab/Packages/ProjectSettings/기존 GUID를 보존했다. 아래는 이전 승인/개발 시점의 기록이다.

## PASS4F.0 사용자 승인 체크포인트 — 2026-10-30

브랜치 `feat/shibuya-combat-mvp`, 이전 기준 `0cbaed7ea07a254a252e09e36d3072a7913da678`. 이번 문서를 포함하는 체크포인트 SHA는 Git 이력에서 확인한다.

사용자가 공격형 AI의 **플레이어 접근, 방향 정렬, 기존 Fighter의 1~4타 공격, 공격 후 휴식·재접근, 기본 조작감**을 직접 플레이하고 승인했다. 이 범위는 USER VERIFIED다. 장애물 우회, 장시간 안정성, 전체 경계 조건, 최종 난이도는 승인 범위가 아니다.

기본 OFF, 10m 탐색, 2.3→1.55m 접근, 20도 정렬, 반응0.20초, 묶음1~4타 가중치25/35/25/15, 휴식0.9~1.5초다. 기존 Fighter가 이동·회전·충돌·피해를 소유한다. OFF는 AI 소유 후속 예약만 철회하고 현재 타격·피격은 강제로 취소하지 않는다. 새 Play/재시작은 수동 더미다.

자동 검증은 **최신 고유34 PASS/0 FAIL/0 SKIP(신규18+선택 회귀16), 7회 실행 집계**다. 단일 전체34개 실행이 아니다. 최초 버퍼 만료 경계 결함과 재검증, 촬영 중 실제 사망에 따른 준비 실패, QA 탐색 오류를 원본 XML/trace/영상/로그와 함께 보존했다. PASS4A 과거 미해결18건은 그대로다.

이전 Pipeline 누적 오류1928건은 CLI 명령 호환19건(`script recompile`2, `script recompile_status`17)과 테스트 discovery 콜백 예외 반복1909건이다. 게임플레이/컴파일 오류가 아니며, 미시작 탐색은 성공 검사 수에 넣지 않았다. 마지막 Editor Console error0/warning1(font), Play stopped/씬 clean/fps -1/Time.timeScale1/AutoTick true16ms/PlayUnfocused/런타임 Shibuya Mesh·Material0/0 기록을 재사용했다. 미확인 장시간 안정성을 해결 완료로 확대하지 않는다.

보고서 `docs/SHIBUYA_PASS4F0_REVIEW.md`, 로컬 원본 `unity/Logs/ShibuyaPass4F0QA/20261030-training-ai/`. Play 메뉴는 `JJK Game/시부야 PASS4F/공격형 훈련 AI ON·OFF`, 상태 창은 같은 경로의 `훈련 AI 현재 상태·설정`이다. 상태 창을 닫으면 연결 AI OFF다.

기존 Gojo/VFX·아우라·카메라·전투값·Scene/Prefab/Packages/ProjectSettings와 사용자 `.slnx` 변경을 보존한다. `.slnx`, QA 원본, 미디어와 로컬 자산은 이번 커밋에서 제외한다. PASS4F.1 방어형 AI는 이 체크포인트 push 확인 후 별도 LOCAL UNCOMMITTED로 구현하며 사용자 승인은 아직 없다. 아래는 이전 시점의 기록이다.

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
