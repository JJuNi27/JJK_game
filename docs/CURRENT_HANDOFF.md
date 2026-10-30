# JJK_game — Current Project Handoff

## 현재 — PASS4D.2 + PASS4E USER VERIFIED 체크포인트 / 훈련 AI 설계 합의 대기

2026-10-30. 브랜치 `feat/shibuya-combat-mvp`. 이전 원격 기준은 `bd5acdcb22c35002317a438a2b0fd28986b53607`이며, 이 문서가 포함된 새 저장 커밋의 SHA는 현재 브랜치 Git 이력에서 확인한다. 이번 요청은 관련 파일의 선택적 commit/push와 문서 정리만 수행한다.

사용자 직접 승인: **잡기 입력·조작감/성공·실패 기본 동작/기존 전투 연계**, **다운 후 방향 기상 기본 기능·방향 입력·조작**. 잡기15 HP/1.4m는 공통 MVP 초기값이지 모든 캐릭터의 고정 규칙이 아니다. 캐릭터별 잡기 성공 연출, 후속 타격 횟수·피해, 밀쳐내기/던지기, 시네마틱 여부, 짧은 즉시 실행형은 후속 설계 대상이다. 잡기 최종 모션·카메라와 기상 최종 애니메이션/VFX, 자동 검사만 한 극단적 경계 조건은 USER VERIFIED로 확대하지 않는다. 정확한 승인 범위는 locked/USER_VERIFIED_SETTINGS의 이번 승인 절을 따른다.

PASS4D.2 최신 고유41 PASS/0 FAIL/0 SKIP는4회 실행, PASS4E 최신 고유37 PASS/0 FAIL/0 SKIP는7회 실행의 케이스별 집계다. 기존 XML을 확인하고 재사용했으며 이번 저장을 위해 Unity를 실행하거나 테스트를 반복하지 않았다. 최초 실패·컴파일 수정·플랫폼 중단 보완·API 응답 시간 초과를 보존한다. 초기 쿨타임0 관측의 근본 원인은 미확정이며 이후 계측/재검증 통과와 구분한다. PASS4A 과거 미해결18건은 그대로다.

기존 PASS4C.2/4D.1 승인, 임시 MVP 가드 원통, 가드 체력 HUD 비공개, F 아우라·카메라·고죠 자산과 잠금값을 유지한다. QA 원본/영상/이미지/XML/trace, 로컬 모델·오디오·레퍼런스, Library/Temp/Cache는 로컬 보존이며 이번 커밋 대상이 아니다. `unity/unity.slnx`는 기존2개 프로젝트 순서만 바뀐 자동 생성 변경으로, 기능에 필요하지 않아 staging하지 않고 그대로 보존한다.

**다음 후보는 훈련 AI 설계**다. 사용자와 행동 규칙을 합의한 뒤 별도 PASS로 진행한다. 이번 체크포인트에서 훈련 AI나 신규 전투 기능을 구현하지 않는다. 아래는 이전 구현·검증 당시의 기록이며 LOCAL UNCOMMITTED/검수 대기/Console 상태를 현재 체크포인트 승인과 혼동하지 않는다.

## 최신 로컬 — PASS4E 방향 긴급 탈출 / 사용자 검수 대기

2026-10-30. `feat/shibuya-combat-mvp`, HEAD `bd5acdcb22c35002317a438a2b0fd28986b53607` 유지. PASS4D.2와 새 PASS4E는 LOCAL UNCOMMITTED이며 staging·commit·push 없음. 사용자는 이번 요청에서 **잡기 구조·조작감 정상**을 확인했다. 잡기 캐릭터별 연출·피해 구성·밀려남·모션의 최종 승인은 포함하지 않는다. 아래4D.2 검수 대기 표시는 이전 작성 시점이다.

PASS4E는 CODEX VALIDATED(targeted) / USER VERIFIED 대기. 실제 다운 착지 후 기존 minimumDownTime0.45초부터 자연 기상 전 새 WASD+Space로 카메라 기준 방향 탈출을 한다. 기존 자연 기상0.85초·기상0.22초는 유지했다. 새 Quick Recovery Data는2.5m/0.30초, 지면에서0.04~0.18초 회피, 종료·중단 후 쿨타임1.0초다. 조기/홀드/단독 Space는 저장하지 않으며 점프·공중 추격에 중복 전달하지 않는다. 지상에서 시작한 유한 수평 동작은 가장자리에서도 기존 중력과 함께 마치며, 공중 시작/체공/지면 스냅은 없다. 무적 밖의 피격·잡기·시네마틱·사망·포커스 상실 등은 취소한다.

최신 고유37 PASS(신규20+선택 회귀17)는 7회 실행의 케이스별 집계다. 단일 전체 성공 실행이 아니다. 초기 입력 Up 관측/쿨타임 관측 실패와 플랫폼 중단 문제·검증의 중간 높이 가정을 구분해 원본 XML과 계측을 남겼다. 플랫폼 중력 인계를 보완했고, 초기 쿨타임0 관측의 정확한 원인은 미확정으로 유지한다. PASS4A 과거18건은 별도 미해결 이력이다.

보고서 `docs/SHIBUYA_PASS4E_REVIEW.md`, 원본 `unity/Logs/ShibuyaPass4EQA/20261030-quick-recovery/`, 원속도3비교 `media/`. 최종 Editor Console error0/warning0, Pipeline 누적 error4(과거 컴파일2+API 시작 응답 timeout2)는 별도다. Play stopped/씬 clean/fps -1/Time.timeScale1/AutoTick true16ms/PlayUnfocused, runtime Mesh/Material0/0. 기존 Grab QA55개 해시, 승인 다운/대시/카메라/아우라·씬·GUID·고죠 자산 보존. Unity가 unity.slnx 두 Project 줄 순서만 자동 변경한 것은 별도 보존했다.

직접 검수: Play → `JJK Game/시부야 PASS4E/긴급 탈출 검수 도구` → 실제 다운 공격 예약 → Game View에서 직접 WASD+새 Space. 조기 홀드/무입력 자연 기상, 락온/고각, 벽·체술·잡기를 비교한다. 이후 추가 기능이나 Git 저장을 자동 진행하지 않는다. 아래는 이전 기록이다.

## 최신 로컬 — PASS4D.2 공통 잡기 MVP / 사용자 검수 대기

2026-10-30. 실제 HEAD `bd5acdcb22c35002317a438a2b0fd28986b53607`, `feat/shibuya-combat-mvp`. PASS4C~4D.1 체크포인트는 일반 push 완료 상태이며 아래1202e940 표기는 이전 기준이다. 이번 잡기는 LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기이고 staging·commit·push하지 않았다.

실제 가드 중 새 LMB로 잡기, 동시 RMB+LMB 잡기 우선, 패링 성공 후 RMB 유지 LMB는 기존 평타 반격이다. 전방1.55m/반각50도, 준비0.18+유효0.08초, 짧은 접촉0.10초 후 피해15/밀려남1.4m/경직0.40초, 성공 회복0.25·실패 회복0.50초다. `Shibuya Grab.asset`과 한국어 Inspector로 조절한다. 가드·패링만 제한적으로 우회하고 대시 무적·Infinity·Health 판정을 유지한다. 강제 위치 스냅/별도 이동 모터/시네마틱/주력 소모 없음. 기존 승인 카메라·대시·패링·아우라 설정·피해·HP·GUID·씬은 보존했다.

최신 고유41 PASS(신규23+선택 회귀18)는 4회 실행 집계이지 단일 전체 성공 실행이 아니다. 원본 실행13/0, 7/1, 18/0, 5/0을 보존했다. 실패1은 QA Vector3 JSON 순환 기록으로 좌표 배열로 수정 후 통과했다. 테스트 컴파일 오류2건도 수정·원본 보존. 최종 Editor Console error0/warning1(font), Pipeline 버퍼의 과거 error2는 별도다. Play stopped/씬 clean/fps -1/Time.timeScale1/AutoTick true16ms/PlayUnfocused 유지, runtime Mesh/Material0/0. 기존 PASS4A18건을 해결로 소급하지 않는다.

보고서 `docs/SHIBUYA_PASS4D2_REVIEW.md`, 원본 `unity/Logs/ShibuyaPass4D2QA/20261030-grab/`, 원속도3비교 `media-final/`. 직접 확인: Play → `JJK Game/시부야 PASS4D/잡기 검수 도구` → 상황 선택 → Game View 복귀 → 직접 RMB 유지+새 LMB. 최종 손 맞춤/캐릭터 모션/VFX·조작감은 사용자 검수 대기다. 반전술식·새 후속 기능은 시작하지 않는다. 아래는 이전 체크포인트/구현 당시 기록이다.

작성 기준: **2026-10-30 KST**
저장소: `JJuNi27/JJK_game` · 브랜치: `feat/shibuya-combat-mvp`
USER VERIFIED 체크포인트: `1202e940176daec9fbb0b8027702f600b2f677d4` (PASS 4B.4a까지, 10-25 일반 push 완료).
위 커밋은 이번 PASS4C~4D.1 체크포인트의 이전 기준이다. 이 문서가 포함된 새 체크포인트의 SHA는 현재 브랜치 Git 이력에서 확인한다. QA 원본/미디어와 로컬 모델·오디오·레퍼런스는 별도 로컬 보존이며 GitHub에 포함된 것으로 해석하지 않는다.

## 현재 — PASS4C~4D.1 체크포인트 / 후속 요청 대기

2026-10-30 사용자가 체크포인트 저장을 승인했다. PASS4C 지상 대시,4C.1 거리·쿨타임 튜닝,4C.2 카메라 기준 방향 통일,4D.0 가드·비공개 체력·파괴·임시 방어막,4D.1 패링·취소/반동·연습 도구와 필요한 코드/데이터/meta/테스트/보고서를 함께 보존한다.

USER VERIFIED는 **4C.2 방향 대시·조작감**, **4D.1 패링 구조·조작감**이다. 가드 원통은 임시 MVP 수용이며 최종 방어 VFX 승인이 아니다. 패링 최종 VFX와 캐릭터별 모션도 후속 작업이다. 가드 전체·자동 경계 검사까지 사용자 수동 검증으로 확대하지 않는다. 정확한 승인 범위는 `docs/locked/USER_VERIFIED_SETTINGS.md`의 이번 승인 절을 따른다.

기존 검증 결과를 재사용했으며 체크포인트를 위해 Unity를 실행하거나 전체 회귀를 다시 돌리지 않았다. 최신4D.1 고유30 PASS는 복수 실행 집계이고 단일 전체 성공 실행이 아니다. 최초 실패 XML/원인과 PASS4A 과거 미해결18건을 유지한다. 대용량 QA/Library/Temp/로컬 전용 자산은 커밋 대상에서 제외한다.

저장 후 추가 구현은 하지 않는다. **잡기는 다음 PASS에서 별도 승인 후 진행**한다. 패링/가드 최종 연출, 반전술식, 신체 강화 등도 자동 착수하지 않는다.

아래는 각 PASS 구현·자동 검증 당시의 기록이다. 당시의 LOCAL UNCOMMITTED/검수 대기/Console 상태를 최신 승인과 혼동하지 않는다.

## 0. 현재 — PASS 4D.1 타이밍 패링 MVP 검수 대기

우클릭 새 가드 진입 시0.12초 패링 창, 시작 사이0.50초 제한, 몸 정면120도. 실패/쿨타임은 일반 가드 유지. 성공은 가드를 종료하고0.10초 동작 후 수동 후속 행동 선택(자동 반격/홀드 재가드 없음), 공격자에게 가짜 HP 피해 없이0.35초 경직/1.2m 거리 넉백/현재·예약 공격 취소. Core Health 수정 없이 Guarded+전용 ParrySucceeded 이벤트로 구분한다. 가드/패링 가능 플래그는 독립이다. 기존 방어막/F 아우라/카메라/승인 대시 수치 보존.

LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기. 최신 고유30 PASS는 복수 실행 집계이며 단일30개 실행이 아니다. 최초 순변위 측정 실패는 준비 전진0.52m를 분리한 실제 반동1.20000017m로 확인했으며 원본을 보존했다. 보고서 `docs/SHIBUYA_PASS4D1_REVIEW.md`, QA/원속도3비교 `unity/Logs/ShibuyaPass4D1QA/20261030-parry/`. 기존 PASS4D 검수 메뉴의 반복 패링 연습/실제 공격 예고를 사용한다. 현재 Console error0/warning1, Play stopped/씬 clean/fps -1/Time.timeScale1/패링 리소스0. commit/push 없음.

PASS4C.2는 사용자 직접 승인. PASS4D.0 전체는 CODEX VALIDATED, 원통 외형만 임시 MVP 수용이며 새 패링의 승인으로 확대하지 않는다. 잡기/반전술식/주력 회복 보너스/신체 강화는 미구현이다. 아래는 이전4D.0 기록이다.

PASS4C.2 카메라 기준 대시 방향·속도는 사용자 직접 승인(LOCAL UNCOMMITTED)이다. 새 PASS4D.0은 우클릭 홀드/전방120도/독립 가드 체력100/회복1.2초 후20/s/파괴0.8초/타격별 가드 피해16·16·18·35와 별도 위험 방어막을 구현했다. 게임 HUD에는 가드 잔량을 표시하지 않는다. 기존 Health 방어 확장 재사용, 이동은 Fighter 단독 소유. F 아우라/카메라/대시 수치 보존. CODEX VALIDATED(targeted) / USER VERIFIED 대기. 최신 고유33 PASS는 복수 실행 집계이며 단일33개 성공 실행이 아니다. 원본 실패/테스트 관측 오류/수정한 프로필 생명주기 예외를 모두 보존했다.

상세·실제 피격 검수 메뉴·영상: `docs/SHIBUYA_PASS4D_REVIEW.md`. QA `unity/Logs/ShibuyaPass4DQA/20261030-guard/`. 메뉴 `JJK Game/시부야 PASS4D/가드 피격 검수 도구`는 Play Mode에서만2초 지연 실제 더미 공격을 제공한다. 현재 Console error0/warning1, 보존 버퍼 error6의 원인은 보고서에 구분했다. Play stopped/씬 clean/fps -1/Time.timeScale1. 패링/잡기/반전술식/신체 강화는 미구현. commit/push 없음.

아래는 PASS4C.2 구현 당시 기록이며 사용자 승인은 위 최신 기록을 따른다.

최신 PASS4C.2: 락온·비락온 모두 시작 시 카메라의 수평 WASD 방향을 저장해 대시한다. 타깃 위치는 이동 벡터에 사용하지 않으며 기존 몸 바라보기는 별도 유지한다. PASS4C.1 거리/지속시간/쿨타임/무적/Curve, 락온 선택·공격·카메라 코드는 보존했다. 단일 targeted20 PASS/0 FAIL/0 SKIP. 보고서·원속도5영상: `docs/SHIBUYA_PASS4C2_REVIEW.md`. LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기. 사용자는4C.1 거리·속도·쿨타임/비락온 방향에 만족한다고 피드백했지만 새4C.2 방향은 직접 검수 전이다. 최종 Play stopped/씬 clean/fps -1, 현재 Console error0/warning1(font), 이전 timeout은 버퍼 보존.

아래 PASS4C.1/4C 수치·검증·Console은 당시의 이력이다.

최신 PASS4C.1: 전방5.25m, 좌우3.60m, 후방3.30m, 종료/중단 후 쿨타임0.45s. 지속시간0.22/0.18/0.20s와 무적0.04~0.14s, 속도Curve·방향·락온·카메라·전투 런타임은 보존했다. 실제 Data Asset 네 값만 변경. 단일16 PASS/0 FAIL/0 SKIP와 별도 원거리 촬영1 PASS. 상세/원속도 자료: `docs/SHIBUYA_PASS4C1_REVIEW.md`. CODEX VALIDATED(targeted) / USER VERIFIED 대기 / LOCAL UNCOMMITTED. 아래4C 수치와20케이스·Console 기록은 이전 실행 이력이다.

CODEX VALIDATED(targeted 범위) / USER VERIFIED 대기. C KeyDown 방향 스텝, 기존 Fighter 단일 motor, 시작 방향 스냅샷, 짧은 일반 체술 회피 guard, 한국어 공통 Data Asset을 구현했다. 전방3.5m/0.22s, 측면2.4m/0.18s, 후방2.2m/0.20s, 측면·후방0.04~0.14s 회피, 종료 후0.65s 공통 쿨타임은 **실험값**이다. 전방 성분 대각선은 무적 없다.

공격/경직/다운/공중/추격/히트스톱/시네마틱에서는 시작하지 않는다. F 홀드 중 C를 차단하고, 진행 중 새 F는 대시를 종료해 기존 충전을 우선한다. 주력 소모/공격 캔슬/공중 대시/신체 강화는 없다. 기존 승인된 아우라·크기 곡선·충전/HUD·카메라/전투값을 보존했다.

최신 고유20케이스 PASS(복수 실행, 미디어 포함). 최초 시네마틱 API 반환형 검사 오류1건을 수정했고 원본 XML을 보존했다. 과거 PASS4A 회귀18건 전체 해결을 선언하지 않는다. 상세·원속도 영상·파일 목록: `docs/SHIBUYA_PASS4C_REVIEW.md`. 코드/설정 commit·push는 이번 작업에서 실행하지 않았다. 아래 USER VERIFIED 기록은 기존 승인 기능이며 새로운 C 기능까지 승인한 것이 아니다.

최종 Console은 error1/warning1. 오류는 검증 종료 후 Pipeline 복합 상태 조회의5초 timeout이며 작은 조회로 연결/Play 종료·씬 clean·프레임률 -1 복구를 확인했다. 테스트 중 게임 예외와 구분하고 로그를 지우지 않았다. 아래4B.4a의 Console 기록은 당시의 별도 시점이다.

## 1. 게임 방향과 보호

Unity 6 URP 기반 3D 자유 이동 격투게임. 현재는 ShibuyaCombatMVP의 블록 플레이어/더미로 1:1 공통 전투를 검증한다. 장기 목표는 넓은 도시, 캐릭터 선택, 독립 무브셋·술식·영역전개, 자유 난전·다인전·멀티플레이다.

Gojo는 첫 캐릭터이며 전용 VFX·모델·애니메이션·승인값은 동결 보존한다. 과거 Gojo 백업 기준은 `backup/gojo-vfx-wip-20261017`의 `540272a`; 모든 로컬 QA가 백업에 포함됐다고 가정하지 않는다. 과거 Purple 인수인계는 `6655f19` 시점 문서 및 Purple 보고서를 참조한다.

실제 로컬 작업 상태 → 최신 handoff/current task → locked settings → 관련 설계/보고서 순으로 확인한다. 사용자-facing Inspector는 한국어, 코드는 HOW/Data Asset은 WHAT. D:\JJK_game 하나를 사용하며 사용자 승인 없이 새 worktree·reset/clean/덮어쓰기/commit/push를 하지 않는다.

## 2. 체크포인트 구현 범위

- 기존 PASS 1~3C: 자유 이동/점프/카메라, 기본4타, W 업/S 다운, 제한 추적·정렬·방향 피격, 국소 히트스톱, 데이터 기반 무브셋, Space 공중 추격, 공중 LMB 2+2+1과 유한 체공.
- PASS 3D/공중 접촉: 유효 마무리 적중 후 참가자2명 세션·외부 간섭 차단·참가자 카메라, 무피해 추격 도착·자연 낙하, 새 피격의 이동 소유권.
- PASS 3C.1: 공중 후속 입력 유예 동안 피격자 높이 유지 관계. 거리·시간·피격 소유권 경계를 유지하며 전체18건 회귀를 해결했다고 선언하지 않는다.
- PASS 4A/4A.1/4A.2/4A.2.1: MMB 선택/해제, 신규50m/유지60m·시야 상실1.5초 유예, 락온 공격 방향, 3D 조준 제공, 수동 카메라 우선·시네마틱 소유권·최고 pitch 화면 밖 복구.
- PASS 4B~4B.3c: 공통 전신 혼령 아우라, 청록/핑크/초록, 먹물/Interior/분리 화염, 국소 형태 보존과 상하 연결, 최대 주력량 기반 크기.
- PASS 4B.4: 기존 CursedEnergyProfile/Controller 재사용, F 홀드 충전·현재 주력·소비/회복 API·변경 이벤트, 본인 파란 HUD, 최대치에서도 홀드 시간, 정상 해제/중단 이벤트 구분.
- PASS 4B.4a: 승인 후보를 저장 기본·Pink·Green에 직렬화. **QA 메뉴 없이 일반 F에서 최신 표현**.

## 3. USER VERIFIED 범위

기존 PASS 1~3C의 직접 승인 범위를 유지한다. 2026-10-22 직접 승인: W 어퍼컷→Space 무피해 접촉·자연 낙하, 추격 후 공중3클릭, 시네마틱 발동·카메라 복귀. 캐릭터 세부 모션과 시네마틱 구도·연출 품질은 승인에서 제외한다.

PASS 4A의 기본 선택/거리/시야 해제, PASS 4A.1 공격 방향 전환의 속도·자연스러움은 사용자가 직접 확인한 범위다. 자동 검사만 통과한 카메라 경계·모든 해상도·다인전 품질을 승인으로 확대하지 않는다.

**2026-10-25 현재 요청에서 사용자가 확인·승인한 항목:**
- 전신 혼령 주력 아우라의 시각적 방향.
- 별도 QA 메뉴 없이 일반 F 홀드에서 최신 아우라가 출력됨.
- 실제 F 홀드 주력 충전 동작.
- 현재까지의 관련 시부야 구현을 하나의 Git 체크포인트로 저장.

극단적 충돌·무적·입력·저프레임·리소스 경계, 픽셀 비교 등 자동 검증 세부는 CODEX VALIDATED로 구분한다.

## 4. 저장 설정과 미구현 사항

- 기본 지상 피해20/20/20/25, Dummy HP1000, Player HP100 유지.
- 주력 최대치 단일 원천: Shibuya Combat Profile → Shibuya Cursed Energy.MaxEnergy. 현재 저장 최대100, 시작 비율1, 홀드 충전 가능, 초당12. 기존 Gojo의 자동 회복은 별도 기본 모드로 보존.
- 아우라 저장 곡선 **100→1.0 / 150→1.5 / 200→2.0**, 최소0.5/최대3.0. 현재 잔량은 크기에 영향 없음.
- Details OFF, 국소 형태 보존 ON, 추가 확장0.65, 먹물 경계1.30/내부 비중0.80, 분리 화염 ON, 상하 연결1/1. Interior0.65/밀도1, 분리 교체속도0.7 유지.
- **2.7/3.0/3.5/4.0은 비교용 실험이며 최종 기본 배율 미선택.** 기존4.0 임시 미리보기/Inspector 조절 경로 유지.
- 시네마틱 예제는 저장 기본 OFF.
- **신체 강화 패시브는 사용자 결정으로 보류.** 준비 상태·임팩트 프레임·F 해제 버프·강화 아우라 지속·공격/이속 강화·W+F 대시는 구현하지 않음. 홀드/해제 API만 존재.

## 5. 검증 근거와 남은 이력

| 범위 | 기록 |
|---|---|
| PASS3D 기존 회귀135 + 시네마틱18 | 단일153 PASS / 0 FAIL / 0 SKIP, `unity/Logs/ShibuyaPass3DAirContactQA/20261022/final-regression-153/results.xml` |
| 공중 접촉·이동 신규 | 별도 복수 실행의 최신 고유18 PASS |
| PASS4A | 최초190개171 PASS/19 FAIL, 후속 조사 후 기존 회귀18건 미해결 기록 유지 |
| PASS4B.4 | 별도 실행9 PASS + 6 PASS, 관련2건 재확인. 최신 고유15 PASS |
| PASS4B.4a | 최초5 PASS/2 FAIL, 후속0 PASS/3 FAIL, 검사 구성 보완 후3 PASS. 현행 최신 고유8 PASS, 폐기된 복합 재진입 timeout도 원본 보존 |
| 일반F/승인미리보기 | 같은 설정/위상/카메라의 정지30쌍 RGB 차이0, 전체 움직임 픽셀 보증 아님 |

마지막4B.4a compile 성공, Console 현재 error0/warning1(기존 font reference). 과거 오류·실패 XML은 보존했다. 이번 체크포인트는 파일/참조/이전 증거 검토이며 전체 회귀를 다시 실행하지 않는다.

체크포인트 staged `git diff --check`는 기존 Unity 생성 빈 YAML/meta 필드와 테스트 마지막 빈 줄 때문에 형식 경고60건(24파일)을 냈다. 의미상 문제/충돌 표식은 없고, 원본 자산 바이트 보존을 위해 공백을 자동 정리하지 않았다. 해당2종 공백 규칙만 명령 단위로 제외한 추가 검사는 통과했으며 Git 설정은 변경하지 않았다.

**과거 PASS4A의18건은 해결됐다고 취급하지 않는다.** 후속 targeted PASS로 전체 원인을 소급 확정하지 않는다. 과거 공중 사거리 실패의 PASS3D 유발 여부도 INCONCLUSIVE 유지. 상세: `SHIBUYA_PASS4A_REVIEW.md`, 각 후속 PASS 보고서.

검증 보고서는 Git에 포함하지만 Logs의 원본 XML/trace/이미지/영상은 로컬 전용이다. Library/Temp/UserSettings/LocalModels/LocalAudio/레퍼런스도 제외·보존한다. 아우라 레거시 설정은 `docs/archive/CURSED_ENERGY_AURA_PRE_4B4A.json`에 별도 보존한다.

## 6. 재현과 후속 작업

씬: `unity/Assets/Shibuya/Scenes/ShibuyaCombatMVP.unity`.
- 일반 F: 메뉴 없이 홀드→최신 아우라/충전/본인 HUD, 해제→중단. 기본 시작은 최대치이므로 런타임 ShibuyaEnergyCharge 메뉴의 현재 주력 비우기로 회복을 확인할 수 있다.
- W+4타→새 Space→도착 후 새 LMB3회. 클릭이 없으면 접촉 후 자연 낙하.
- 시네마틱: `JJK Game → 시부야 PASS3D → 시네마틱 예제 ON (Play Mode 전용)`; OFF 또는 Play 종료로 해제.
- 아우라4배 비교: `JJK Game → 시부야 PASS4B3c`의 임시 배율/200기준 곡선 메뉴. 사용 후 저장 프로필 복귀.

현재 지상 방향 대시·회피 PASS4C는 로컬 구현 후 사용자 검수 대기다. 신체 강화는 계속 보류한다. 가드/패링/잡기 등 다음 기능, 캐릭터 모션/시네마틱 구도, 과거18건 QA 원인, 다인전·성능·복잡한 맵 경계는 별도 사용자 지시를 기다린다. 자동으로 구현하거나 승인 범위를 늘리지 않는다.
