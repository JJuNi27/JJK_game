# PASS 3C 추가 — 공통 공중 평타와 완전 체공

기준 HEAD: `3dff1714dd3ed590df3ae7fb199480043288e3d4`, branch `feat/shibuya-combat-mvp`.
현재 체크포인트 상태: **USER VERIFIED — 사용자 직접 플레이 승인 범위 한정**.
2026-10-19 사용자는 W 어퍼컷 → Space 추격, 추격 후/일반 점프의 공통 공중 체술, 공격 중 완전 체공과 정상 착지, LMB 3회의 2타 → 2타 → 1타 연계 동작을 승인했다. 극단적인 충돌·입력·무적·저프레임 경계 조건은 사용자 검증으로 확대하지 않으며 자동 검증 범위는 CODEX VALIDATED로 구분한다.

승인 범위와 현재 데이터 구조의 정확한 설명은 [PASS 3C 체크포인트](SHIBUYA_PASS3C_REVIEW.md#사용자-승인-체크포인트--2026-10-19)를 따른다. 아래는 체공 개선 당시의 검증 이력으로, 승인 전·commit/push 미실행 문구 및 당시 Git 개수는 역사적 기록이다. 최초 실패를 포함한 QA 근거는 그대로 유지한다.

## 확인한 원인과 변경

이전 공중 공격에는 매 프레임 기존 중력이 적용됐다. 추격 도착 때 대상의 수직 속도를 이어받은 후 자연 낙하했으며, 일반 점프에서 LMB를 누르면 공중 전용 데이터 대신 지상 공격 분기로 들어갔다.

일반 점프와 W 어퍼컷 → Space 추격 후의 LMB를 모두 `TryAirAttack` → `BeginCommonAirStep` → 기존 `ShibuyaMovesetStep` 판정으로 연결했다. 둘 다 같은 `ShibuyaAerialCombatDefinition.aerialAttack`을 사용한다. W/S는 공중에서 새 마무리 종류를 만들지 않는다. 기존 1입력 12+13의 두 타격 이벤트·이벤트별 중복 피해 방지·실제 높이/사거리/장애물 검사를 유지한다. 도착만으로 공격하지 않는다.

`ShibuyaAerialHoverRuntime`은 같은 `ShibuyaFighter`의 partial이며 높이 유지 소유권·수명만 처리한다. 별도 이동/피해 엔진은 없다. 준비·활성·회복 중 중력 속도를 0으로 유지하고, 기존 단일 CharacterController.Move로 이동한다. transform 위치 대입이나 전역 Time.timeScale 변경은 없다. 최종 displacement 전체를 강제로 0으로 만들지 않아 명시적 이동 클립과 충돌 처리는 유지한다. 이번에 수직 이동 클립 종류 자체를 새로 만들지는 않았다.

## 연계와 해제 정책

- 공중 무브셋은 공통 한 종류이며 한 입력 안의 2타를 유지한다. 짧은 유효 창에 새 클릭을 입력하면 같은 단계를 다시 실행할 수 있다. 버튼 유지로 자동 반복하지 않는다.
- 공격 말미의 예약은 실제 이전 적중·대상 거리/높이/무적/장애물 조건이 유효할 때만 받는다. 기존 확정 연계 상한을 늘리지 않고, 이미 정상 경직 중인 대상에만 기존 ReserveLink를 적용한다.
- 공격 후 짧은 입력 대기 체공도 실제 적중한 경우에만 허용한다. 공격자가 대기한다고 경직이 끝난 상대까지 멈추지 않는다. 뒤늦게 입력하면 확정 연계로 상대를 다시 묶지 않는다.
- 총 체공 상한은 첫 공중 공격 접수부터 **unscaled 실시간**으로 한 번만 시작한다. 후속 클릭·이벤트·히트스톱으로 갱신하지 않는다. 만료 시 공격/예약/체공을 취소하고 자연 낙하한다.
- 빗나간 공격은 공격자만 해당 단계 동안 체공하며 후속 대기 체공은 없다. 피격자 높이 유지는 실제 유효 적중과 현재의 합법적인 피격 경직이 있을 때만 적용한다. 경직 종료·무적·이탈·장애물·공격자 중단·비활성·사망·전투 종료이면 해제한다.
- 피격자에게 기존 발사 상태를 새로 만들거나, 지상 대상을 공중으로 강제 이동시키지 않는다. 공격자·피격자의 낙하 속도를 체공 중 누적하지 않고 0에서 기존 중력으로 복귀한다.
- Space 추격 이동 자체의 3D 경로·도착 조건·한 번 사용 정책은 그대로다. 추격 중에는 이 체공 제어를 적용하지 않는다.

## 한국어 Inspector

기존 `Assets/Shibuya/Data/Shibuya Aerial Combat.asset`에 네 항목만 추가했다. 원래 사용자 수치는 보존한다.

| 항목 | 기본값 |
|---|---|
| 공중 공격 중 완전 체공 | 켜짐 |
| 공중 연계 총 체공 상한 (실시간) | 1.2초 |
| 공중 후속 클릭 허용 시간 (0이면 1회만) | 0.12초 |
| 유효 공중 피격 경직 중 높이 유지 | 켜짐 |

SerializedProperty/CustomEditor와 Undo를 유지한다. 기존 `OneLaunchCannotRepeatChaseOrAirAttack` 검사는 후속 입력 창을 0으로 설정해 종전의 1회 정책을 그대로 검증한다. 새 기본값의 유한한 후속 연계는 신규 검사에서 따로 검증하며 기존 assertion을 삭제하거나 SKIP하지 않았다.

## 실제 Play Mode 측정

신규 13개 targeted 검사가 첫 실행에서 모두 통과했다. 실제 Unity Test Framework EnterPlayMode/ExitPlayMode를 사용했으며 새로운 발견·실행·XML 결과는 이전 QA와 분리한 `unity/Logs/ShibuyaPass3CHoverQA/`에 저장한다.

| 측정 | 결과 |
|---|---|
| 일반 점프 LMB, 준비~활성~회복 중 공격자 Y | 0.710873m → 0.710873m, 변화 0m |
| 추격 후 LMB, 준비~활성~회복 중 공격자 Y | 4.552643m → 4.552643m, 변화 0m |
| 합법적인 체공 피격 구간의 연속 프레임 Y 변화 | 최대 0m |
| 0.2초로 설정한 총 체공 상한 | 실제 약 0.201236초에 해제 |
| 상한 해제 첫 낙하 속도 | 약 -0.0513m/s, 누적 낙하 속도 튐 없음 |

마지막 공격 종료 뒤 낙하를 재개한 프레임은 공격 중 Y 유지 측정에서 제외한다. 피격자의 경직이 끝나면 공격자의 회복 모션이 아직 남아 있어도 피격자는 낙하하도록 별도 검사했다. 한 입력 2타, 유한한 재연계, 무적·피격·사망·비활성·Miss·체공 OFF·한글 Inspector/Undo도 검사했다. 원시 측정과 이름을 정리한 결과는 `focused-initial-13/metrics.jsonl`, `measurements.json`이다.

## 회귀·최종 상태

| 실행 | 결과 |
|---|---|
| `focused-initial-13` | 신규 체공 13 PASS / 0 FAIL |
| `regression-112` | 기존 112개 단일 실행: 111 PASS / 1 FAIL |
| `wall-diagnostic-1` | 기존 실패 1건: 1 PASS / 0 FAIL |
| `wall-path-proof-1` | 추가 실제 경로 차단 검사: 1 PASS / 0 FAIL |

**최신 고유 결과: 기존 112 + 신규 14 = 126 PASS / 0 FAIL / 0 SKIP.** 네 실행을 합친 최신 케이스별 결과이며 단일 126개 실행이 아니다. `latest-per-case-126.json`이 각 케이스의 실제 실행/XML 경로를 연결한다. discovery·실행 로그·XML의 이름/개수/결과를 대조했으며 이전 112개 고유 집합과 정확히 일치한다. 최초 실패 원본은 보존했다.

기존 `WallBeforeSpaceRejectsPathWithoutTeleport`가 최초 실행에서 추격 버퍼 예상과 달리 실패했다. 기존 조건·assertion을 바꾸지 않고 위치 기록만 추가한 재검증에서는 통과했다. 기록의 source Z=0.9553, 목적지 Z=1.0027로 경로가 거의 수직이었고, 벽 중심 Z=1.4053/캡슐 반경=0.350이었다. 작은 측면 접촉 여유에 민감했을 가능성은 있지만 최초 실패 프레임의 정확한 원인은 **미확정**이다. 체공 코드의 회귀라고 단정하거나 런타임 경로 정책을 바꾸지 않았다.

이 검사만 반복해 성공으로 덮지 않도록 별도 검사를 추가했다. 실제 띄워진 대상 위치를 테스트에서 전방으로 옮겨 추격 경로가 벽을 확실하게 가로지르게 하고, 장애물 추가 전 경로 열림 → 추가 후 실제 캡슐 경로 차단 → Space 버퍼 없음 → 추격 횟수 0 → 관통 없음까지 모두 검사했다. 원래 검사는 유지하며 기준을 낮추거나 SKIP하지 않았다. 기존 벽 검사의 초기 불안정 이력은 남는 한계다.

최종 Unity 조회:
- C# compilationFailed=false / compiling=false, 현재 Console 오류 0 / 경고 0. 세션 누적 Editor 경고는 138건이며 `Deleting invalid font reference` 이력을 포함한다. 누적 기록은 지우지 않았다.
- Play Mode 종료, scene dirty=false, 미저장 persistent Data Asset 0, missing script 0, Time.timeScale=1.
- 기존 피해 20/20/20/25, 넉백 6/4/4/11, 4타 히트스톱 0.065초, 어퍼컷 높이 5.5m/체공 0.12초, Player HP 100/Dummy HP 1000 유지.
- 기존 바닥 균열 소유 mesh/material 잔존 0/0. 새 체공 partial은 mesh/material을 생성하지 않는다.
- QA의 임시 auto-tick 0ms를 원래 enabled=true / 16ms / 별도 SessionState 선택 없음으로 복구했다. 임시 QA 출력 SessionState도 지웠다.
- 시작 시 1,059개 파일 SHA-256과 Shibuya 소스 사본을 보존했다. 이번 추가 패스에서 기존 미커밋 파일 10개만 변경됐고, 모든 기존 `.meta`와 이전 공중 자산 값이 보존됐다. 새 공중 자산 설정은 네 줄 추가뿐이다. Scene/Prefab/카메라/블록 모션/Gojo/VFX/Packages/ProjectSettings/잠금 파일은 그대로다.
- HEAD는 기준 SHA 그대로. 전체 working tree는 이전 PASS 3C 작업을 포함해 tracked modified 11 / untracked 18, staged 0. commit/push 미실행.

근거: `final-audit.json`, `protected-final-audit.json`, `session-restored.json`, `final-console-status.json`, 각 실행의 `results.xml`.

## 이번 추가 패스 파일

기존 미커밋 파일 중 수정:
- `Scripts/ShibuyaAerialCombatDefinition.cs`, `Scripts/ShibuyaAerialCombatRuntime.cs`
- `Scripts/ShibuyaFighter.cs`, `Scripts/ShibuyaHitStateRuntime.cs`, `Scripts/ShibuyaMovesetRuntime.cs`
- `Editor/ShibuyaInspectors.cs`, `Editor/ShibuyaMovesetInspector.cs`
- `Editor/ShibuyaPass3CTestRunner.cs` — 별도 QA 출력 경로 지원
- `Editor/ShibuyaPass3CTests.cs` — 종전 1회 공중 공격 정책의 명시적 설정, 벽 검사 위치 진단 기록 (기존 assertion 유지)
- `Data/Shibuya Aerial Combat.asset` — 위 네 설정 추가

위 경로의 공통 루트는 `unity/Assets/Shibuya/`다. 신규 파일은 `Scripts/ShibuyaAerialHoverRuntime.cs`와 `Editor/ShibuyaPass3CHoverTests.cs`, 각각의 `.meta`, 이 보고서다. 기존 Scene·Prefab·카메라·블록 시각 모션·Gojo/VFX·Packages·ProjectSettings와 기존 GUID는 수정하지 않는다.

## 사용자 Play Mode 검수

1. `ShibuyaCombatMVP`에서 Space로 일반 점프하고 새 LMB를 누른다. 지상 1타 대신 같은 공중 2타가 나오는지, 준비부터 회복까지 높이가 유지되는지 본다.
2. W+좌클릭 4타 어퍼컷 → 새 Space 추격 → 도착 뒤 새 LMB로 같은 공중 2타를 확인한다.
3. 공격 말미 또는 직후 짧은 창에 다시 클릭하면 같은 공중 평타로 연결되는지 확인한다. 계속 클릭해도 첫 공격부터 정해진 총 1.2초를 넘겨 무한 체공하지 않아야 한다.
4. 허공 공격, 늦은 입력, 피격 시에는 정상 낙하해야 한다. 추격 후 LMB를 누르지 않을 때는 종전처럼 낙하해야 한다.
5. 일반 4타/W 업/S 다운, 어퍼컷 높이·착지·균열도 확인한다. 새 공중 손맛은 아직 사용자 승인 전이다.

staging / commit / push / PASS 3D 구현은 하지 않는다.
