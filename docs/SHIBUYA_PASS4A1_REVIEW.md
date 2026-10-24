# PASS 4A.1 — 락온 공격 방향 개선

> 2026-10-25 체크포인트 문서 갱신: 사용자는 이후 실제 플레이에서 공격 방향 전환의 속도·자연스러움에 만족했고 이 동작을 유지하도록 요청했다. 그 직접 확인 범위는 USER VERIFIED로 보존한다. 아래 PENDING/USER VERIFIED 아님 표시는 당시 자동 검증 보고이며, 전체 회귀18건이 해결됐다는 의미로 변경하지 않는다.

2026-10-23 / LOCAL UNCOMMITTED / `feat/shibuya-combat-mvp`
기준 HEAD: `42833ebfec9a176056ae983096c41c1ef4d8fb24`.

**PASS 4A.1 targeted 범위 CODEX VALIDATED / 사용자 Play Mode 검수 대기.**
PASS 4A 전체 검증 보류 상태와 과거 미해결 18건을 완료로 승격하지 않는다. USER VERIFIED 아님.

## 실제 원인

- 명시적 락온 자체는 저장돼 있었다. 공격이 카메라 Forward를 직접 다시 읽는 경로가 원인은 아니었다.
- 기존 `LockedMeleeTarget`이 일반 근접 자동 보정용 `ValidAim(..., acquiring:true)`를 호출했다. 현재 몸 방향 기준 acquisition angle, 제한된 회전/접근으로 예측한 도달 가능성이 명시적 락온까지 거부했다.
- 유효한 락온을 골라도 후속 준비 보정에 일반 retention angle, 최대 회전 각도, 반대 이동 입력에 의한 보정 중단이 다시 적용됐다. 따라서 카메라 기준 이동으로 몸이 상대 반대로 향한 뒤에는 공격이 그쪽을 유지했다. 공격 중 일반 이동이 몸을 매 프레임 카메라 방향으로 재회전시키는 코드는 없다.
- 공중 경로에도 같은 락온 선택 제한이 있었고, 기존 공중 접촉/추격 대상보다 새 명시적 락온을 우선하는 경로가 있었다.

## 최소 변경과 정책

1. 명시적 선택과 일반 soft acquisition을 분리했다. 선택된 락온은 몸/카메라 각도나 근접 사거리로 다시 선택 거부하지 않는다. 사거리 밖에서도 수평 방향은 잡을 수 있지만 추가 원거리 접근이나 피해를 허용하지 않는다.
2. 실제 몸의 yaw만 준비 구간에서 제한 속도로 회전한다. 회전 시간은 `min(기존 strike.Startup, 락온 준비 구간 상한)`, 누적 회전은 타격별 상한 이내다. 활성/회복 중 무한 회전하지 않는다. 장애물·무적·사망·거리 이탈은 보정을 중단한다.
3. 접근 이동은 기존 trackingEnabled, 거리/높이/도달 가능성 검사 및 접근 예산으로 제한한다. CharacterController 단일 이동 소유권과 기존 OverlapSphere/정면/벽/Health 판정은 변경하지 않았다. 준비 시간이 부족하면 뒤쪽 적에게 자동 적중하지 않는다.
4. 시작 시각이 0인 이벤트는 공격 수락 시, 지연 이벤트는 해당 준비 시작 시 대상을 저장한다. 도중 락온 변경/해제로 현재 이벤트를 다른 적에게 재조준하지 않는다. 저장 대상이 무효화되면 보정만 중단한다. 다음 이벤트/공격은 새 락온을 사용할 수 있다.
5. 공중에서는 유효한 기존 접촉 대상 → 유효한 추격 대상 → 명시적 락온 순서다. authored 이동이 대상 소유권을 가진 이벤트는 그 소유권을 유지한다. 일반 점프와 추격 후 공격 엔진/데이터는 분리하지 않았다.
6. 원본 W/S 입력과 버퍼, 지상/공중 공격 데이터, 카메라 구현은 유지했다. `ShibuyaLockOnCameraRuntime.cs`의 변경은 Fighter partial 부분만이며 카메라 partial은 동일하다.

## 변경 파일

기존 로컬 작업 위에 추가 수정한 7개:

- `unity/Assets/Shibuya/Scripts/ShibuyaTargetingRuntime.cs`: 이벤트별 목표 저장, 명시적 회전과 기존 접근 보정 분리.
- `unity/Assets/Shibuya/Scripts/ShibuyaAerialCombatRuntime.cs`: 공중 대상 우선순위와 같은 회전 정책 연결.
- `unity/Assets/Shibuya/Scripts/ShibuyaLockOnCameraRuntime.cs`: Fighter의 명시적 선택/검증/준비 회전. 카메라 부분 불변.
- `unity/Assets/Shibuya/Scripts/ShibuyaLockOn.cs`: 술식별 락온 조준 opt-out 오버로드.
- `unity/Assets/Shibuya/Scripts/ShibuyaLockOnProfile.cs`: 별도 회전 정책 데이터/검증.
- `unity/Assets/Shibuya/Editor/ShibuyaLockOnInspector.cs`: 새 필드 한국어 SerializedProperty 편집.
- `unity/Assets/Shibuya/Resources/Shibuya Lock On.asset`: 새 정책의 명시적 기본값만 추가. 기존 필드 불변.

신규: `unity/Assets/Shibuya/Editor/ShibuyaPass4A1Tests.cs`, 필요한 `.meta`, 이 보고서.
기존 테스트 기대값과 PASS 4A 보고서/QA 기록은 수정하지 않았다.

## 한국어 Inspector / 3D 조준

`Assets/Shibuya/Resources/Shibuya Lock On.asset`:

| 실제 표시 | 새 기본값 |
|---|---:|
| 락온 공격 준비 회전 사용 | ON |
| 락온 공격 초당 회전 각도 | 2400°/초 |
| 타격별 락온 최대 회전 각도 | 180° |
| 락온 회전 준비 구간 상한 (초) | 0.12 |

공격 준비 시간을 늘린 것이 아니다. 예를 들어 준비 0.10초인 타격은 최대 0.10초만 회전한다. 각도/속도를 줄여 회전이 부족해지면 실제 판정도 빗나갈 수 있다. SerializedProperty, Undo, 다중 편집 흐름 유지.

향후 술식은 발사 순간 `IShibuyaAimSource.GetAim(actualMuzzle.position)`로 `Origin/Point/Direction/Target` 값 사본을 받는다. 몸 yaw와 독립적으로 목표 Collider 중심+설정 offset까지 XYZ를 계산한다. `GetAim(origin, false)`는 해당 술식만 자유 카메라 조준을 사용한다. 반환 Direction을 한 번 저장해 발사에 사용하면 대상이 움직여도 그 값이 자동 갱신되지 않는다. 실제 투사체/유도/피해나 Gojo 기술 연결은 구현하지 않았다.

## 자동 검증

QA 루트: `unity/Logs/ShibuyaPass4A1QA/20261023-165916/`.

| 별도 실행 | 결과 |
|---|---:|
| `pilot/results.xml` — 몸/카메라 180도 반대 | 1 PASS |
| `targeted/results.xml` — 나머지 신규 방향/공중/조준/Inspector | 11 PASS |
| `finishers-pursuit/results.xml` — 락온 일반/W/S 4타, 실제 Space 추격 우선권 | 4 PASS |
| `core-regression/results.xml` — 기존 영향 검사 7개 + 보강한 벽 검사 1개 | 8 PASS |

**총 24회 실행, 최신 고유 23케이스 = 23 PASS / 0 FAIL / 0 SKIP. 단일 전체 실행 아님.**
신규 고유 16개 + 기존 고유 7개다. 벽 검사는 선택 우선권 assertion 보강 후 한 번 재실행했으므로 중복 집계하지 않았다. `discovery.json`, `execution.jsonl`, 원본 XML 및 `validation-summary.json` 보존.

확인한 실제 동작:

- 카메라/몸 정면, 90도, 180도에서 실제 LMB 입력 → 몸 회전 → 20 피해 1회.
- 느린 회전 설정에서 뒤쪽 적중 없음. 원래 Startup 불변.
- 먼 락온 대상은 방향만 맞추고 적중/추가 접근 없음. 벽/무적 피해 차단.
- 해제 후 soft melee 복귀. 현재 이벤트 목표는 재선택으로 바뀌지 않고 다음 공격에 적용.
- 일반 점프의 락온 공중 공격, 기존 접촉 대상 및 실제 어퍼컷→Space 추격 대상 우선권.
- 락온 상태에서 일반/W/S 4타 모두 총 피해 85, 원본 키 분기와 각 상태 유지.
- 카메라 반대 방향/공중 대상에 실제 발사 위치 기준 XYZ 조준, 값 사본 및 opt-out.
- 기존 jump/pursuit 3클릭 각 5타, W/S 버퍼 선택 보존, 기존 이벤트별 목표 유지, 기존 근접 보정, 시네마틱 우선/복귀 검사.

전체 190개는 재실행하지 않았다. 과거 최초 171 PASS / 19 FAIL 및 이후 최신 고유 172 PASS / 18 FAIL 기록은 그대로다. 이번에 재검사한 과거 W/S 일부가 통과했어도 과거 실패 원인이나 나머지 실패 해결을 의미하지 않는다. 그 18건은 PASS 4A 전체 검증 보류 기록으로 유지한다.

## 보호 / Editor / Console

- 기존 보호 파일 48개 해시 일치. 피해 20/20/20/25, Dummy HP 1000, Player HP 100, 기존 Scene/Prefab/Data/Packages/ProjectSettings/GUID 보존.
- 기존 QA 472개, 최초 전체 실행 221개, 진단 실행 14개, 재검증 9개 해시 일치. `preservation-audit.json`에 기록.
- 작업 시작 109개 스냅샷 중 위 승인된 7개만 변경. 기존 PASS 3D/4A 나머지 미커밋 파일, handoff/current task 및 기존 보고서 보존.
- C# 재컴파일 성공. 최종 현재 Console error 0 / warning 1 (`Deleting invalid font reference`, ScriptReloadProperties). 시작 전 누적 오류 1건은 07:23의 Play Mode 실행 전제 예외 기록이며 삭제하지 않았다. 작업 시작 cursor 이후 새 error 없음. 전체 이력을 error 0으로 주장하지 않는다.
- Play Mode 종료, 활성 QA 없음, 미저장 씬 없음, dirty Data Asset 없음.
- AutoTick ON/16ms, 저장 override 없음(-1), targetFrameRate -1, vSync 0, Time.timeScale 1 유지. 임시 설정 변경 없음. Play Unfocused 값 2 유지. 화면/마우스 직접 조작 및 포커스 강제 호출 없음.
- 브랜치/HEAD 불변. staged 0. 기존 dirty 작업과 신규 파일은 모두 미커밋. commit/push 없음.

## 사용자 Play Mode 확인

1. `Assets/Shibuya/Scenes/ShibuyaCombatMVP.unity`를 열고 Play. 평타 사거리로 더미에 접근한다.
2. 더미를 화면 중앙에 두고 MMB를 눌렀다 놓아 확정한다. 마우스로 90도/반대 방향을 보고 바로 LMB. 카메라의 기존 보조 동작과 별개로 몸이 상대를 향해 공격하는지 확인한다.
3. 일반 4클릭, 첫 타부터 W 유지+4클릭, S 유지+4클릭을 각각 확인한다. MMB 짧게 클릭해 해제한 자유 전투도 비교한다.
4. 일반 점프 LMB, W 어퍼컷→Space→LMB를 비교한다. 추격 중 다른 대상을 선택하더라도 진행 중인 유효 공중 대상이 유지되는지 확인한다.
5. 먼 상대나 벽 뒤 상대가 락온됐다고 근접 피해가 생기지 않는지 확인한다. 방향 보정은 사거리 증가가 아니다.
6. 시네마틱은 Play Mode에서 `JJK Game → 시부야 PASS3D → 시네마틱 예제 ON (Play Mode 전용)` 후 일반 4타 유효 적중. 종료 후 자유 카메라/조작 복귀 확인. OFF는 같은 메뉴의 `시네마틱 예제 OFF (Play Mode 전용)`.

회전 속도의 자연스러움과 실제 손맛은 사용자 검수 대상이다. 카메라 강제 보조의 부담/가림/구도 문제는 이번에 조정하지 않았으며 PASS 4A.2로 남긴다. 새 전체 저프레임 보증이나 과거 18건 해결을 선언하지 않는다.
