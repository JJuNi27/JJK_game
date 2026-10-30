# 시부야 PASS4F.1 — 방어형 훈련 AI MVP 임시 수용 체크포인트

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

작성: 2026-10-30. 브랜치 `feat/shibuya-combat-mvp`.

## 이번 체크포인트 저장 대상 — 19개

- `docs/CURRENT_HANDOFF.md`
- `docs/active/CURRENT_TASK.md`
- `docs/locked/USER_VERIFIED_SETTINGS.md`
- `docs/SHIBUYA_PASS4F1_REVIEW.md`
- `unity/Assets/Shibuya/Data/Shibuya Training AI.asset`
- `unity/Assets/Shibuya/Data/Shibuya Training Defense.asset`
- `unity/Assets/Shibuya/Data/Shibuya Training Defense.asset.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaTrainingAIInspector.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaTrainingAITool.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaTrainingDefenseInspector.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaTrainingDefenseInspector.cs.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4F1Tests.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4F1Tests.cs.meta`
- `unity/Assets/Shibuya/Scripts/ShibuyaTrainingAI.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaTrainingAIProfile.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaTrainingDefenseProfile.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaTrainingDefenseProfile.cs.meta`
- `unity/Assets/Shibuya/Scripts/ShibuyaTrainingDefenseRuntime.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaTrainingDefenseRuntime.cs.meta`

## PHASE A — 완료된 USER VERIFIED 체크포인트

- 커밋: `9568855d806c29ad66375ac470a6d69c3482205c`
- 링크: https://github.com/JJuNi27/JJK_game/commit/9568855d806c29ad66375ac470a6d69c3482205c
- 메시지: `feat(shibuya): checkpoint offensive training AI through PASS 4F.0`
- 저장20개 파일, 일반 push 성공. 실제 GitHub refs/heads/feat/shibuya-combat-mvp와 로컬 HEAD 일치 확인 후 PHASE B를 시작했다.
- 승인: 공격형 AI의 플레이어 접근·정렬·기존 Fighter의1~4타·휴식/재접근·기본 조작감. 장애물 우회·장시간 안정성·전체 경계·최종 난이도는 제외한다.
- `.slnx`와 로컬 QA/미디어/모델/오디오/레퍼런스/캐시는 제외했다. 한글 `?`로 저장됐던 새4F.0 문서 부분만 UTF-8로 바로잡고 이전 PASS 본문을 보존했다. 코드 게임플레이는 체크포인트 단계에서 변경하지 않았다.

4F.0 QA는 원본 XML을 다시 읽어 최신 고유34 PASS/0 FAIL/0 SKIP(7회 실행)을 확인해 재사용했다. 누적1928건의 분류:

| 유형 | 수 | 성격과 영향 |
|---|---:|---|
| 미등록 `script recompile` 호출 | 2 | CLI/서버 명령 이름 호환 오류, 게임플레이 예외 아님 |
| 미등록 `script recompile_status` 호출 | 17 | 같은 CLI 호출 문제, 실제 컴파일 실패 아님 |
| `discovery 0개` 콜백 예외 반복 | 1909 | 잘못된 필터와 예외 뒤 구독 해제 누락 경로. 해당 호출은 검사 미시작 |

이후 지원되는 컴파일/검증 경로와 콜백 정리 후 결과를 확인했다. 미시작 탐색을34개 성공 검사에 넣지 않았다. 실제 초기 예약 결함·촬영 중 사망 실패는 별도 XML/trace로 남고 관련 재검증 통과와 구분한다. 중대 미해결 오류로34개 최신 결과를 무효화할 근거는 원본 대조에서 확인하지 못했으며, 모든 장시간 경계 조건의 해결을 뜻하지 않는다. 마지막4F.0 Editor Console0오류/1폰트경고 기록과 누적1928건 모두 보존했다.

## PHASE B — 구현과 우선순위

상태: **LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기**.

기존 AI를 partial로 확장해 작은 방어 판단 경로를 앞단에 연결했다. Offensive 모드는 즉시 기존 경로로 돌아가며 원래 접근·정렬·그룹·휴식 알고리즘/값을 재작성하지 않는다. 새 `ShibuyaTrainingDefenseProfile`을 기존 AI Profile에서 참조한다. 기본 initialMode는 Offensive, 실제 AI는 여전히 기본 OFF다.

우선순위는 사망/전투 종료 → 기존 시네마틱·피격·다운·잡기·방향 기상·히트스톱 등 → 현재 정상 콤보 → 자신의 가드/회피 → 관측된 위협의 지연 반응 → 일반 공격형 판단이다. 새 위협 때문에 콤보를 캔슬하지 않는다. 진행 중 회피는 기존 Fighter가 끝낸다. 가드 종료/모드 변경/OFF는 자신이 소유한 홀드만 해제한다. 모드 변경은 자신의 미래 공격 예약만 철회하고 현재 유효 타격은 유지한다.

관측은 현재 target.CurrentStrike/ComboStep/AcceptedAttackCount, 실제 위치·바라보기·장애물을 사용한다. HasBufferedAttack·BufferedFinisher·실제 키/마우스는 AI 코드에서 읽지 않는다. 같은 실제 공격 단계는1회만 관측/판단하고, 미래에 예약된 단계는 실제로 시작돼 AcceptedAttackCount가 바뀐 후 관측한다. 관측 시각/반응 예정 시각/실행 시각을 구분한다. 지연 중 피격·공격 종료·상태 이탈이면 늦은 반응을 취소하며 방어를 소급하지 않는다.

### 모드

| 모드 | 동작 |
|---|---|
| 공격형 | 승인된4F.0 경로 |
| 가드 연습 | 관측 후 일반 가드, 유한 유지 후 해제 |
| 회피 연습 | 가능한 좌/우/후방 대시. 카메라 기준 사용 안 함 |
| 패링 연습 | 가능한 새 가드로 패링 시도, 재시도 불가 시 일반 가드 대체. 성공 보장 없음 |
| 종합 | 가능한 방어 가중치 선택 + 기존 공격형. 데이터에서 공격 혼합을 끌 수 있음 |

일반 가드도 기존 새 입력의 패링 창을 그대로 가진다. 이를 별도로 비활성화하거나 패링 성공을 강제하지 않는다. 가드 성공은 GuardBlocked, 패링 성공은 ParrySucceeded의 실제 결과로 집계한다. 가드 체력은 기존 내부 수치를 사용하고 HUD에 추가하지 않는다. 유효하지 않은 방향/가드 파괴/피격은 기존 결과를 따른다.

회피는 상대 방향 또는 자신의 몸 방향을 수평 기준으로 계산해 기존 TryGroundDash에 좌/우/후방 입력을1회 제출한다. 시작 방향은 기존 API가 고정한다. CapsuleCast로 막힌 후보를 제외하며 실패 시 같은 프레임 재시도하지 않는다. 실제 이동/충돌/거리/무적/쿨타임은 기존 Fighter가 소유한다. 카메라·CharacterController.Move·전역 Time.timeScale을 AI가 조작하지 않는다.

회피 지표는 **실제 대시 시작**과 **무적 구간의 실제 Guarded 피해 판정**을 구분한다. 위치 이탈로 단순 미적중한 경우를 피해 차단 성공 수에 넣지 않는다. 특수 IDamageGuard를 향후 더 붙이면 차단 원인 태그를 더 세분할 수 있으며 현재 지표 이름은 관측한 사실만 표시한다.

### 한국어 Data Asset

`Assets/Shibuya/Data/Shibuya Training Defense.asset` + 기존 Training AI의 방어 설정 참조/초기 모드. 모든 실제 필드를 SerializedProperty 기반 한국어 Inspector로 편집하며 Undo/객체 선택/기본 직렬화 편집을 유지한다. 모드는 런타임 선택과 저장 initialMode를 구분하고, 고정 난수/시드는 공통 Training Profile을 사용한다. 방어 난수는 공격 난수와 별도 System.Random 흐름이다.

| 설정 | 저장 기본값 |
|---|---|
| 관측 주기 | 0.10초 |
| 반응 지연 | 0.16~0.30초 |
| 방어 판단 최소 간격 | 0.75초 |
| 가드 유지 | 0.35~0.75초 |
| 종합 가드/회피/패링/무반응 | 50/25/10/15 가중치 |
| 위협 관측 거리/공격자 방향 반각 | 4m / 75도 |
| 회피 기준/좌·우·후방 가중치 | 현재 상대 방향 / 1·1·1 |
| 종합에서 공격형 허용 | true |

가중치는 현재 가능한 행동의 합으로 정규화한다. 음수/비정상 값은0, 합계0은 무반응이다. 연습 모드는 해당 행동을 우선하고 종합 모드에 확률을 적용한다. 캐릭터의 공격/가드/패링/대시 실제 밸런스는 각 기존 Profile에서 계속 관리한다.

## 검증과 실제 수치

QA 루트: `unity/Logs/ShibuyaPass4F1QA/20261030-defense-ai/`.

| 실행 | PASS | FAIL | SKIP | 해석 |
|---|---:|---:|---:|---|
| first-targeted | 4 | 0 | 0 | 가드·늦은 피격·실제 패링·3방향 회피 |
| remaining-targeted | 17 | 1 | 0 | 원본 결과. 실패 뒤6 PASS는 증거 미완료로 일단 보류 |
| buffer-diagnostic | 0 | 1 | 0 | 실제 QA 입력 만료 재현·원본 trace |
| buffer-input-fix | 1 | 0 | 0 | 동일 기대값, 유효한 QA 입력 예약으로 통과·본문 완료 기록 |
| post-failure-body-proof | 6 | 0 | 0 | 앞서 증거 없던6건을 완료 기록·영상 포함 재검증 |
| selected-regression | 17 | 0 | 0 | 기존 선택16 + 실제 AI 가드 잡기 신규1 |

최신 고유 **39 PASS/0 FAIL/0 SKIP = 신규23 + 기존 선택 회귀16**. 6회 실행의 케이스별 최신 집계이며 단일39개 전체 성공 실행이 아니다. 원본 XML/실패/미시작 상태·소스 사본을 보존했다. `latest-unique-results.json`에 케이스별 원본 실행을 기록한다. PASS4A 과거 미해결18건은 별도 유지한다.

관측 결과:

- 가드 반응 약0.2167초, HP1000 유지/가드16 소비. 유지 종료 후 입력 홀드 해제.
- 짧은 준비0.05초 공격은 반응 전에 HP1000→995, 뒤늦은 가드/패링 성공 없음.
- QA 격리 공격 준비0.30초에 실제 패링1회, 기존 창0.12초/재시도0.50초 유지. 공격 취소·넉백 약0.90m·자동 반격 없음.
- 창이 끝난 패링 시도와 parryable=false 타격은 정상 가드. Unblockable/환경 피해는 정상 적용.
- 가드 파괴 공격은 원래 HP 피해1회, 기존0.8초 잠금. 뒤쪽 타격은 방어 성공으로 처리하지 않음.
- 좌/우/후방 거리3.60/3.60/3.30m. 플레이어 카메라 회전 후 방향 불변. 기존0.04~0.14초 회피와 종료 후0.45초 쿨타임 유지.
- 무적 구간 실제 Guarded는 차단1, 무적 밖 공격은 HP995·차단0. 단순 사거리 이탈은 차단 성공으로 집계하지 않음.
- 판단 최소0.75초 설정에서 실제 새 방어 요청 간격 약0.7997초.
- 시드1729/2,000선택: 가드48.55%, 회피26.25%, 패링9.70%, 무반응15.50%. 동일 시드 재현/전역 Unity 난수 불변/무효 합계 안전 처리.
- 종합의 현재4타는 위협이 생겨도4단계·85피해를 마치며 방어로 강제 취소하지 않음.
- 실제 AI 유지 가드는 플레이어 잡기에 뚫리며15피해·잡기 상태가 정상 적용됨. 다운·방향 기상·잡기·시네마틱·사망/비활성 중 새 방어 명령 충돌 없음.
- 기존 공격형 기본OFF/접근/정렬/1~4타/요청 철회/휴식/난수/재시작, 기존 대시/공중 추격/시네마틱 카메라/F 충전·아우라·HUD/패링·잡기·방향 기상 선택 회귀 통과.

QA 공격의 피해·긴 준비·넓은 판정 등은 복제 데이터에만 설정했다. 사용자 승인 실제 공격/대시/패링 값은 바꾸지 않았다.

QA의 정확한 시점 분리를 위해 AI 복제 데이터도 조절했다. 일반 검사·영상의 관측 주기는0.01초(저장 기본0.10초), 반응은 보통0.20초 고정, 회피 예시는0.02초, 가드 유지는0.65초 고정이다. 종합 영상의 반응은0.16~0.30초로 되돌렸지만 관측 주기는0.01초다. 따라서 이 영상은 API 연결·성공/실패 사례의 근거이며 저장 기본 난이도의 대표 플레이 영상으로 해석하지 않는다. 실제 기본값에서는 관측 지연 때문에 더 늦게 반응할 수 있다. 저장 Asset의 기본값은 위 표 그대로 보존했다.

## 실패·수정과 Console

1. 신규 QA IEnumerator 메뉴 검사에 yield가 없어 CS0161 컴파일 오류1건. 테스트에 yield를 추가해 수정했다. 런타임 컴파일 결함이 아니다. `compile-first-failure.txt`와 Console 원본 보존.
2. 미래 입력 비관측 검사의 QA 공격자가 창의 첫 순간에 후속 입력을 예약했다. 만료1.2298초보다 실제 종료 처리1.2397초가 늦어2타 자체가 나오지 않았다. 원본 trace/소스 보존 후 **QA 입력만 기존 허용 창 중간으로 이동**했다. 기대값·원래 버퍼·플레이어 공격·방어 로직은 변경하지 않았다. 실제2타 시작 뒤에만 두 번째 관측/무반응이 생기는 검사 통과.
3. 실패 뒤의6건은 XML상 PASS인데 예정된 계측/영상이 없었다. 정확한 프레임워크 내부 원인은 확정하지 않는다. 해당6건을 즉시 검증 완료로 쓰지 않고, 본문 완료 sentinel/파일 근거를 추가해 새 실행6 PASS와 영상408프레임을 확인했다. 원래17/1 XML을 덮어쓰지 않았다.
4. 재컴파일 뒤 Pipeline 연결 실패1회. 미시작/Editor 상태 확인 후 복구된 연결에서1회 시작했다. 패키지 재설치·MCP 재설정은 하지 않았다.

현재 컴파일 정상, Editor Console **error0/warning1(Deleting invalid font reference)**. 이번 Pipeline 세션 누적error1은 위 QA CS0161이며 수정 이력을 보존한다. 이전4F.0의 CLI/QA1928건과 합쳐 새 게임플레이 오류로 표현하지 않는다. 테스트 실패 XML은 Console error 카운터와 별도로 기록한다.

최종 Play stopped/씬 clean/fps -1/Time.timeScale1/AutoTick true16ms/GameView PlayUnfocused, 런타임 AI0/Shibuya Mesh·Material0/0. 임시 프레임률은 복구했고 AutoTick/프로젝트 패키지는 변경하지 않았다. Computer Use·Windows 직접 입력·강제 포커스 전환 없음. 새로운 방어용 VFX/메시/머티리얼을 만들지 않았고 기존 가드/패링/대시 표현을 재사용했다.

## 직접 플레이와 영상

1. ShibuyaCombatMVP Play. 새 세션은 AI OFF.
2. `JJK Game → 시부야 PASS4F → 모드`에서 공격형/가드 연습/회피 연습/패링 연습/종합 중 선택. 선택만으로 켜지지 않는다.
3. 기존 `공격형 훈련 AI ON·OFF` 메뉴로 ON. 이름은 호환성을 위해 유지했으며 현재 선택 모드로 동작한다.
4. `훈련 AI 현재 상태·설정`에서 런타임 모드, 마지막 판단·결과, 실제 관측/시도/성공/실패를 확인한다. Training AI Asset의 방어 설정 링크로 Defense Asset을 열어 한국어 Inspector를 조절한다. 상태 창을 닫으면 연결 AI OFF.
5. 가드 연습은 긴 준비 타격·뒤쪽 공격·잡기를 비교한다. 짧은 평타는 AI 반응 전에 맞을 수 있다. 회피 연습은 근접 공격 뒤의 실제 옆/후방 이동을 확인한다. 패링 연습은 시도하는 모드이며 성공 보장이 아니다.
6. 종합에서 공격 사이 잠시 쉬어 AI의 기존 접근/공격도 관찰한다. OFF는 가드 홀드를 정리하고 현재 실행 중인 타격·대시는 기존 엔진이 마무리한다. 재시작은 다시 수동 더미다.

원속도7개 MP4는 QA `media/`: guard-success, guard-late, dodge, parry-success, parry-late, mixed, ai-off 이름에 `-original-speed.mp4`가 붙는다. 캡처 시각 기반 VFR, 960×720, 일반 사례약2초/종합약5초. PNG/타임스탬프는 `post-failure-body-proof/legacy/ShibuyaPass4B3QA/4F1-media/`다. 자동화 Play Mode 카메라 렌더이며 HUD·오디오·실제 사용자 조작 녹화는 아니다.

### 남은 한계

기본 반응0.16~0.30초와 판단0.75초 때문에 빠른 연타/지속 압박에는 자주 맞는다. 종합 영상에서는6회 관측 중1패링·5반응 실패, AI HP975/플레이어100으로 남았고 영상 구간에 AI 공격은 나오지 않았다. 정상 무입력 구간의 종합4타 공격은 별도 검사했다. 사용자 요청 없이 반응을 단축하거나 숨은 예측·공격 캔슬·강제 탈출을 추가하지 않았다.

원통형 가드와 블록 캐릭터 표현은 기존 MVP다. 난이도/자연스러움/패링 성공률/가드 위험도 직관성은 USER VERIFIED 대기다. 직선 장애물 확인만 있으며 낭떠러지 판단·도시 우회·다수 AI/멀티플레이·장시간 성능은 미검증이다. 새 고급 VFX·잡기 AI·술식·공중 AI·반전술식은 구현하지 않았다.

## 변경과 보존

PHASE B 기존 수정5개: TrainingAI.cs(방어 진입/수명주기 연결·partial), TrainingAIProfile.cs(초기 모드/방어 참조), TrainingAI.asset(동일2필드 추가), TrainingAIInspector.cs, TrainingAITool.cs. 기존 Fighter·Health·가드/패링/대시·카메라·아우라/전투 Profile의 알고리즘이나 값은 바꾸지 않았다.

신규5개와 각각 meta: ShibuyaTrainingDefenseProfile.cs, ShibuyaTrainingDefenseRuntime.cs, ShibuyaTrainingDefenseInspector.cs, ShibuyaPass4F1Tests.cs, Shibuya Training Defense.asset. 문서는 CURRENT_HANDOFF/CURRENT_TASK 및 이 보고서다. PHASE B에서 locked/USER_VERIFIED_SETTINGS는 변경하지 않았다.

기존4F.0 QA565개 SHA-256 불변, 기존 .slnx 바이트 불변, 보호 Scene/Prefab/Gojo/VFX/아우라/카메라/전투값/HP1000·100/기존 GUID/Packages/ProjectSettings 불변을 대조했다. PHASE B staging·commit·push 없음. HEAD는 PHASE A 체크포인트로 유지하고 추가 기능은 시작하지 않는다.

### PHASE A 실제 커밋 파일20개

- `docs/CURRENT_HANDOFF.md`
- `docs/SHIBUYA_PASS4F0_REVIEW.md`
- `docs/active/CURRENT_TASK.md`
- `docs/locked/USER_VERIFIED_SETTINGS.md`
- `unity/Assets/Shibuya/Data/Shibuya Training AI.asset`
- `unity/Assets/Shibuya/Data/Shibuya Training AI.asset.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaPass3CTestRunner.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4F0Tests.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4F0Tests.cs.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaTrainingAIInspector.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaTrainingAIInspector.cs.meta`
- `unity/Assets/Shibuya/Editor/ShibuyaTrainingAITool.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaTrainingAITool.cs.meta`
- `unity/Assets/Shibuya/Scripts/ShibuyaFighter.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaTrainingAI.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaTrainingAI.cs.meta`
- `unity/Assets/Shibuya/Scripts/ShibuyaTrainingAIProfile.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaTrainingAIProfile.cs.meta`
- `unity/Assets/Shibuya/Scripts/ShibuyaTrainingFighterRuntime.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaTrainingFighterRuntime.cs.meta`
