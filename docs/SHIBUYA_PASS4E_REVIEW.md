# PASS 4E — 지상 다운 후 방향 긴급 탈출 MVP

## 체크포인트 승인 범위 — 2026-10-30

사용자가 다운 후 방향 기상의 기본 기능·방향 입력·조작을 직접 확인해 **USER VERIFIED**로 승인했다. 모든 극단적 충돌/저프레임/무적 경계를 직접 검증했다는 뜻은 아니며 최종 캐릭터별 기상 애니메이션/VFX는 후속 작업이다. 아래 대기/LOCAL UNCOMMITTED 표시는 구현 당시 기록이다.7회 실행 최신 고유37 PASS와 최초 실패·플랫폼 수정·초기 쿨타임0 원인 미확정·이후 계측 통과를 보존했다. 이번 저장을 위해 Unity 테스트를 다시 실행하지 않았다.

2026-10-30 KST. `feat/shibuya-combat-mvp`, HEAD `bd5acdcb22c35002317a438a2b0fd28986b53607`.
LOCAL UNCOMMITTED / **CODEX VALIDATED(targeted) / USER VERIFIED 대기**. 기존 미커밋 PASS4D.2와 원본 QA를 보존했다. staging·commit·push 없음.

## 기존 다운 상태와 입력 연결

기존 `downed/downLanded/gettingUp`, `TryLandingSurface`, `CanRequestDownRecovery`, `UpdateRecoveryState`를 사용한다. 최소 다운 **0.45초**, 자연 기상 시작 **0.85초**, 기상 동작 **0.22초**와 원본 DownSettings는 변경하지 않았다. 별도 최소 대기 타이머나 두 번째 다운 시스템을 만들지 않았다.

`ShibuyaPlayerInput`은 현재 다운/탈출 상태에서 새 Space를 긴급 탈출 경로로 먼저 소비한다. 그 프레임에는 SetIntent의 점프·QueueAirChase에 전달하지 않는다. 자격 미충족 입력은 저장하지 않으며, 방향만 누르거나 Space만 누르면 실행되지 않는다. `SetIntent` 자체도 다운 중 jumpIntent를 저장하지 않도록 처리했다. 정상 지상 점프·어퍼컷 후 선택 추격 경로는 유지한다.

착지 완료, 최소 대기 경과, 자연 기상 전, 새 방향 입력, 현재 피격 경직/히트스톱/시네마틱/사망/종료/재사용 제한 없음이 필요하다. 시작 시 일반 공격 버퍼·점프·다운 기상 소유권을 정리하고, 가드·패링·잡기·F 충전이 겹치지 않게 한다. Space 홀드를 이어가도 재실행되거나 기상 직후 점프하지 않는다.

## 방향·이동·회피

별도 `ShibuyaQuickRecoveryRuntime`이 기존 Fighter의 단일 CharacterController에 수평 변위만 제공한다. transform.position을 바꾸거나 별도 Move를 호출하지 않는다. 이동 방향은 카메라 수평 Forward/Right와 정규화한 WASD로 시작 순간 고정한다. 수직 카메라에서는 Right로부터 수평 Forward를 복원한다. 락온 여부/타깃 위치를 방향 계산에 사용하지 않으며, 기존 PASS4C.2 대시 소스·데이터와 OrbitCamera/LockOn은 수정하지 않았다.

| 독립 Quick Recovery Data Asset 항목 | 초기 실험값 |
|---|---:|
| 이동 거리 / 시간 | 2.5m / 0.30초 |
| 무적 시작 / 종료 | 0.04 / 0.18초 |
| 종료·중단 후 재사용 제한 | 1.0초 |
| 임시 포즈 강도 / 복구 시간 | 1.0 / 0.30초 |
| 주력 / 가드 소모 | 없음 |

이동 곡선을 시작 시 누적 적분한 뒤 총 거리를 정규화한다. 막힌 시도 거리도 즉시 소비하여 벽 제거 후 누적 이동하지 않는다. 종료 후 쿨타임은 다음 긴급 탈출만 제한하며 일반 이동/공격/점프/가드/대시를 막지 않는다.

시작은 실제 바닥 검증을 요구한다. 지상에서 시작한 현재0.30초의 유한한 수평 변위는 가장자리에서도 기존 중력과 함께 마친다. 첫 지지 상실에 수평 속도를 즉시0으로 만들면 둥근 캡슐이 모서리에 걸쳐 멈출 수 있어, 남은 유한 동작을 유지하며 자연스럽게 가장자리를 벗어나게 했다. 공중에서는 새 탈출을 시작할 수 없고, 시간/거리를 추가하거나 고도를 유지하지 않는다. 피격/포획/시네마틱/포커스 상실 등은 즉시 취소한다. 지면으로 스냅하지 않으며 회피 무적은 유효 지면/행동 조건 안에서만 적용한다.

회피 방어는 PhysicalStrike이고 Unblockable이 아닌 경우에만 Block을 반환한다. Health.GrantInvulnerability나 Core 피해 코드를 변경하지 않았다. 환경·영역 필중·Unblockable 전역 면역은 없다. 실제 Health/ResolveHits 경로에서 HP·피격·정상 적중 피드백·가드/패링 이벤트가 발생하지 않음을 검사한다. 무적 밖의 피해·잡기/시네마틱/사망/비활성화/포커스 상실은 탈출 이동을 취소한다.

PASS4D.2 GrabRuntime에는 `IsQuickRecoveryEvading` 대상 제외와, 무적 밖에서 포획한 대상의 탈출 취소만 추가했다. 잡기의 원래 거리·준비·피해·밀려남·실패 페널티·입력 정책은 보존했다. 잡기가 기본 설정 그대로 방어 창에서 거절되고 창 밖에서는 포획함을 실제 경로로 확인한다. 이 상성 분리 검사에서는 이동에 의한 사거리 이탈과 면역을 구분하기 위해 테스트 복제본의 탈출 거리만0.05m로 사용하며 실제 저장값2.5m는 유지한다.

## 포즈·지면 균열·Inspector

BlockVisual은 다운 자세에서 기존 각도를 이어받아 몸을 복구하고 입력 방향으로 짧게 기울인다. 일반 다운·자연 기상·가드·패링·잡기 포즈와 분리한다. 기존 지면 균열은 탈출 시작 시 숨기며 새 Play를 호출하지 않는다. 신규 렌더러/셰이더/Mesh/Material/대형 VFX/시네마틱은 없다. F 아우라 자체는 수정하지 않는다.

`Shibuya Quick Recovery.asset`을 Combat Profile의 `quickRecovery`가 참조한다. 사용 여부·거리·시간·속도 Curve·무적 시작/종료·쿨타임·임시 포즈를 한국어 SerializedProperty Inspector로 편집한다. Undo·객체 선택·Curve·Min/Range 편집을 유지한다. 최소 대기·자연 기상 값은 기존 DownSettings가 계속 단독 소유한다.

## QA 원본과 실패 이력

`unity/Logs/ShibuyaPass4EQA/20261030-quick-recovery/`에 실행별 discovery/execution/results.xml, 진단 trace/checkpoint, 변경 전 파일 SHA-256, 기존 Grab QA 해시를 보존한다. 전체 구형 회귀는 반복하지 않았다. PASS4A 과거 미해결18건을 해결로 소급하지 않는다.

| 실행 | PASS | FAIL | SKIP |
|---|---:|---:|---:|
| initial | 11 | 2 | 0 |
| diagnostic | 0 | 2 | 0 |
| action-checkpoints | 1 | 0 | 0 |
| edges-media | 8 | 1 | 0 |
| focused-final | 20 | 1 | 0 |
| edge-carry-final | 5 | 1 | 0 |
| platform-trajectory-final | 1 | 0 | 0 |

**최신 고유37 PASS / 0 FAIL / 0 SKIP**: 신규20개와 선택한 기존 회귀17개. 위7회 실행의 최신 케이스별 집계이며 단일37개 전체 성공 실행이 아니다. 전체 목록은 `test-summary-final.json`에 있다. 통과한 구형 전체 검사를 무조건 다시 돌리지 않았으며, 소스 수정 후에는 영향받는 움직임/상성 검사만 재실행했다.

최초 공중 다운 실패는 실제 다운/착지까지 성공했으나, 테스트가 Space Up을1번 yield한 뒤 다시 Down으로 바꿔 새 입력이 접수되지 않은 경우다. 보완 검사에서는 실제 previousJump=false가 관측된 뒤 새 Down을 보냈고 통과했다. 게임플레이/기대값은 바꾸지 않았다.

행동 제한 검사에서는 초기2회에 종료 후 cooldown0이 관측됐다. 추가 체크포인트 실행에서 탈출 시작→0.0837초 행동 차단→약0.3005초 종료/쿨타임1.0초가 기록됐고 통과했다. edges-media에서도 같은 기대값으로 통과했다. 초기0 관측의 정확한 원인은 **미확정**이며 프레임 지연 또는 런타임 결함으로 단정하지 않는다. 원본과 재검증을 함께 보존한다.

플랫폼 실패는 실제 새 런타임의 이동 중단 문제였다. 시작 확인용 중심 레이로 진행을 멈췄을 때 Y2.0244m, CharacterController의 첫 지지 상실로 중단해도 둥근 캡슐이 모서리에 걸쳐 Y1.8304m에 남았다. 시작 지면 검증과 중력은 유지하고, 이미 지상에서 시작한 유한 수평 동작을 중력과 함께 마치는 것으로 보완했다. 그 후 실제 낙하는 진행됐지만 테스트의 임의 가정인 ‘동작 종료 시 높이>1m’가 실패했다(Y0.5456m). 이 가정을 맞추려고 보호된 중력을 바꾸지 않았다. 대신 각 시간 단계의 실제 낙하량이 기존 중력·컨트롤러 step/skin 이동 한도 안인지, 공중에 새 무적이 없는지, 최종 착지하는지를 직접 검증했다. 최종0.30초 동작 종료 높이0.5508m→착지0.0300m, 시간 진행19구간 중 최대 낙하0.1432m, 이동 한도 위반0으로 통과했다. 이전 실패와 기대 가정 수정 이유를 모두 보존했다.

두 최종 실행 시작 요청은 Pipeline 메인 스레드 응답5초 제한에 걸렸으나 execution/results가 실제로 생성되고 끝난 것을 확인했다. 같은 요청을 중복 시작하지 않았다. 이 API 오류와 테스트 결과를 구분한다.

초기 거리 계측은 전후좌우·대각선 모두 **2.5m**, 관측 총 시간0.3163~0.3338초(입력 수락/관측 프레임 포함), 자연 기상 시작0.8626초였다. 동작 설정0.30초·자연 기상 설정0.85초를 변경한 것이 아니다. 벽 앞 실제 이동0.4200m, 차단된 동안 요청 거리2.4080m 소비 후 벽 제거 뒤 이동0.0920m로 누적되지 않았다.

## 원속도 자료·직접 검수

`media/directional.mp4`, `natural.mp4`, `early-hold.mp4`, `recovery-comparison.jpg`와 원본 프레임/timestamps. 960×720 H.264 VFR 원속도이며 가속·슬로모션·보간 없음. 각44/42/42개 표본, 마지막 시간1.346/1.325/1.336초. QA 고정 구도이며 일반 자유 카메라 품질의 수동 승인을 대신하지 않는다. 평지 영상은 플랫폼 수정의 영향이 없는 코드 경로에서 촬영했다.

1. `ShibuyaCombatMVP`에서 Play.
2. `JJK Game → 시부야 PASS4E → 긴급 탈출 검수 도구`.
3. 기존 RMB/F/Space를 놓고 상황을 예약한 뒤 Game View로 돌아가 입력을 잡는다. 2초 후 실제 더미 공격으로 플레이어가 다운된다(개발용 피해1, 저장값 변경 없음). 여전히 가드·회피 중이면 실제 방어/사거리 판정에 따라 막히거나 빗나갈 수 있으며 도구가 다운을 강제하지 않는다.
4. 착지 후0.45초 이전 Space/홀드, 0.45~0.85초 새 WASD+Space, 아무 입력 없는 자연 기상을 비교한다. MMB 락온도 직접 선택할 수 있다.
5. 초반/후반 체술, 잡기, 벽 근처 상황을 선택해 상성을 확인한다. 초반 공격은 짧은 실제 공격으로 준비하지만 프레임·거리 차이가 있으므로 도구가 성공/회피를 강제하지 않는다.

도구는 실제 TryAttack/HitOutcome/Health/GuardHeld/TryGrab 경로를 사용하며 downed 플래그나 성공 카운터를 조작하지 않는다. 임시 무브셋·개발용 벽은 종료 시 정리한다. 사용자의 공간/거리/입력 타이밍·기상 동작의 느낌은 직접 검수 대기다. 원작 캐릭터의 최종 구르기/리깅/VFX는 제작하지 않았다.

## 보호·Git·남은 한계

PASS4D.2 잡기 **구조·조작감 USER VERIFIED**는 이번 사용자 요청에서 확인한 사실로 기록한다. 잡기 캐릭터별 연출·피해 구성·밀려남·모션 최종 승인은 포함하지 않는다. PASS4E 자동 검증을 사용자 승인으로 확대하지 않는다.

기존 Grab Data/테스트/메타/QA, 대시 프로필·소스, 카메라/락온, USER_VERIFIED_SETTINGS, Gojo/VFX, 주력 아우라, Scene/Prefab, Packages/ProjectSettings, 피해20/20/20/25·HP100/1000을 보존한다. 통합을 위해 기존 로컬 Fighter/Input/BlockVisual/CombatProfile/HitState/Cinematic/EnergyCharge와 GrabRuntime에 최소 연결을 추가했다. QA 러너의 출력 경로 허용에4E를 추가했다.

Unity가 tracked `unity/unity.slnx`의 기존2개 Project 줄 순서를 자동 변경했다. 경로 추가/삭제는 없고 실행 기능 변화는 없다. 임의 복구하지 않고 별도 생성 변경으로 보존한다. 이번 요청에는 Git 저장을 수행하지 않는다.

최종 Git은 HEAD/브랜치 유지, staged0, 전체 로컬 working tree40개(기존 잡기 작업 포함, tracked modified14/untracked26)다. PASS4E 시작 후의 변경 목록은 `changed-files.txt`: 기존 파일14개+신규13개, 합27개이며 이 중1개가 위 자동 생성 솔루션 변경이다. 이 목록을 전체 WIP40개와 혼동하지 않는다. 기존 Grab QA55개 외에도 기존 Grab Data/Inspector/테스트/meta와 보호 자산은 시작 해시와 비교했다. `git diff --check` 통과, 사용자 승인 없는 Git mutation 없음.

최종 컴파일 오류0/scriptCompilationFailed=false, Editor Console 현재 **error0/warning0**. Pipeline 누적 기록은 **error4/warning130**이며 error4는 이전 PASS4D.2 테스트 컴파일 오류2건과 이번 요청 시작 응답 시간 초과2건이다. 이를 전체 이력 error0으로 표현하지 않는다. Console을 수동으로 지우지 않았다. Play stopped/씬 clean/fps -1/Time.timeScale1/AutoTick true·16ms/PlayUnfocused. 가드·패링·지면 균열 런타임 Mesh/Material 잔존0/0이며 긴급 탈출에는 새 시각 리소스 생성이 없다. 기존 Grab QA XML/JSON/JSONL55개를 SHA-256으로 보존 확인했다.

움직이는 플랫폼 추종, 임의 복잡한 경사/저프레임 전체 조합, 네트워크 동기화와 GPU 성능 전체 검증은 하지 않았다. 초기 쿨타임0 관측의 정확한 원인은 여전히 미확정이며 이후 여러 계측/재검증 통과와 함께 기록한다. 원속도 시각 자료의 표본 수가 짧은 무적 전체를 매 프레임 표현하지는 못하므로 시간/피해 검사는 자동 결과와 구분한다. 사용자 기상 모션·방향·심리전/입력 가능 시간의 체감은 직접 검수가 필요하다.
