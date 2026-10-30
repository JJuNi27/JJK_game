# PASS 4C — 지상 방향 대시·회피 MVP

후속: PASS4C.1에서 거리와 쿨타임만 조정했다. 최신 값·검증은 `docs/SHIBUYA_PASS4C1_REVIEW.md`를 참조한다. 아래는 변경 전 PASS4C의 원본 결과다.

2026-10-29 착수 / 2026-10-30 KST 마무리. **LOCAL UNCOMMITTED / CODEX VALIDATED(targeted 범위) / USER VERIFIED 대기**.
브랜치 `feat/shibuya-combat-mvp`, 보호 기준 HEAD `1202e940176daec9fbb0b8027702f600b2f677d4`.

## 1. 입력과 상태 소유권

`ShibuyaPlayerInput`의 C KeyDown 한 번을 기존 입력 프레임에 추가했다. 홀드·쿨타임 중 입력은 자동 반복/예약하지 않는다. 방향 없는 C는 동작하지 않는다. Escape/포커스 상실/비활성 시 C 래치를 정리한다.

- 비락온: 카메라의 수평 forward/right 기준 WASD 정규화 방향.
- 락온: 유효한 명시적 타깃까지의 수평 방향을 forward로 사용한다. W 접근, S 후퇴, A/D는 그 방향의 접선. 처음/첫 번째 적/이름으로 선택하지 않는다.
- 앞 성분이 있는 대각선은 Forward, 뒤 성분이 있으면 Backward, 순수 좌우는 Side. 대각선 이동 거리 증가 없음.
- 이동 경로와 시각적 몸 방향은 시작 시 저장한다. 락온에서는 시작 타깃 방향을 바라보고, 비락온에서는 기존 몸 방향을 유지한다. 상대/카메라가 움직여도 중간에 경로를 재조준하지 않는다. 대상이 없거나 해제·사망/비활성이면 카메라 기준으로 복귀한다.
- 같은 프레임의 기존 공격/Space 처리를 C보다 먼저 수행하여 공격/점프를 우선한다. 대시 중 공격과 Space 점프/추격은 시작하지 않으며 종료 후 새 입력을 정상 처리한다. 공격 캔슬/공중 대시를 만들지 않았다.
- F 홀드 또는 충전 중에는 C 시작 불가. 대시 중 새 F를 누르면 대시/회피를 종료하고 기존 F 충전을 우선한다. F를 취소해 대시하는 기능은 없다.

새 partial `ShibuyaGroundDashRuntime`은 변위만 계산한다. `CharacterController.Move`는 기존 `ShibuyaFighter.Update`의 경로를 그대로 사용한다. 중력·월드 충돌·캡슐 분리·넉백과 기존 시네마틱의 소유권을 유지한다. 이동 위치를 순간이동시키지 않는다.

## 2. Data Asset / 한국어 Inspector

`Assets/Shibuya/Data/Shibuya Ground Dash.asset`을 공통 `Shibuya Combat Profile.groundDash`에 연결했다. 기존 전투 Profile의 JSON 비교에서 의미상 변경은 이 참조 하나뿐이다.

| 유형 | 요청 거리 | 설정 지속시간 | 회피 무적 |
|---|---:|---:|---|
| 전방/전방 대각선 | 3.5m | 0.22s | 없음 |
| 순수 좌우 | 2.4m | 0.18s | 시작 후0.04~0.14s |
| 후방/후방 대각선 | 2.2m | 0.20s | 시작 후0.04~0.14s |

모든 방향은 **실제 종료/중단 시점부터 공통0.65s 쿨타임**. 신체 강화/주력 소모는 없다. 위 수치는 실험값이며 USER VERIFIED 밸런스로 잠그지 않았다.

한국어 SerializedProperty Inspector에서 사용 여부, 각 방향 거리/시간, 무적 시작 지연/길이, 종료 후 쿨타임, 속도 Curve, 스텝 기울기18°를 조절한다. Undo·복수 선택·Min/Range·Object reference 기능을 유지한다.

속도 Curve는 시작0→빠른 가속(0.12 지점1.5)→감속(0.65 지점1)→종료0 형태다. 양수 구간의 누적분을 시작 시64구간으로 샘플링/정규화하여 최종 거리와 속도가 서로 모순되지 않게 했다. 음수/비유한 샘플은0, 전체 적분이0이면 일정 속도로 대체한다. 거리·시간·무적 수치는 시작 시 스냅샷한다. 충돌로 막혀도 해당 프레임의 요청 거리는 소비되어 이후 누적 분출되지 않는다.

## 3. 실제 피해·피격 판정

기존 `Health.ReceiveDamage → IDamageGuard → Applied 확인 → 경직/넉백/HitConfirmed/히트스톱/시네마틱` 경로를 재사용한다.

참가자 시네마틱의 기존 guard를 먼저 처리하고, 그 외에는 활성 Side/Backward 회피 구간에서 **PhysicalStrike + Unblockable 아님**인 일반 공격만 차단한다. 현재 공통 guard의 Block 결과이므로 DamageResolution은 `Guarded`다. 전역 Health 무적 타이머를 부여하지 않았으며 `IsGroundDashEvading`으로 이 회피 구간을 조회할 수 있다.

술식·환경 피해·Unblockable 공격은 새 회피 면역을 받지 않는다. 잡기 예외/특수 기술 회피는 후속 설계다. 유효 Applied 피해, 사망·전투 종료·비활성·시네마틱 진입·지면 이탈·F 집중·설정 해제는 대시/회피를 정리한다.

일반 공격 파이프라인 대조에서:
- 유효 회피: 피해0, HitConfirmed0, 경직/넉백0, 히트스톱0, 시네마틱 없음.
- 동일한 테스트용 타격 이벤트를 회피 없이 적중: 피해20, 히트스톱1회, 참가자 시네마틱 활성.
- 이 검사는 충돌 범위 안에 남겨 guard 경로를 실제 호출하려고 **런타임 복제 Data**의 측면 거리만0.01m로 설정했다. 저장 기본2.4m는 수정하지 않았다. 테스트용 한 단계 무브셋·시네마틱도 저장하지 않았다.

## 4. targeted QA와 측정

루트: `unity/Logs/ShibuyaPass4CQA/20261029-ground-dash/`.

| 실행 | PASS | FAIL | SKIP |
|---|---:|---:|---:|
| `targeted/results.xml` | 10 | 1 | 0 |
| `focused-regression-media/results.xml` | 9 | 0 | 0 |
| `resources-resume/results.xml` | 1 | 0 | 0 |
| `air-and-framed-media/results.xml` | 2 | 0 | 0 |

**최신 고유20케이스 PASS / 0 FAIL / 0 SKIP(미디어 검사 포함)**. 복수 실행 집계이며 단일22개/20개 전체 성공 실행으로 표현하지 않는다. 같은 락온/시네마틱/미디어 재실행은 중복 합산하지 않는다.

최초 실패는 `CinematicCancelsDashAndKeepsCameraOwnership`에서 `TryStartAfterApplied`가 반환한 세션 객체를 NUnit `Is.True`로 검사한 테스트 작성 오류였다. 객체 존재 검사로 수정 후 대시 취소·참가자 위치/카메라 소유권·회복 검사를 통과했다. 기대 동작/게임 수치를 낮추거나 런타임 문제를 숨기지 않았고 원본 XML을 보존했다.

신규 검사는 방향4개·대각선4개·중립 C, 홀드/쿨타임, 타깃/카메라 기준과 시작 방향 고정·대상 사망 fallback, 실제 피해/피격/히트스톱/시네마틱 차단, 벽/타 캐릭터, 공격3구간·경직·다운/기상·점프·히트스톱·사망, F/HUD, 실제 어퍼컷→Space 추격/공중 체술 중 C 금지, 반복 리소스/조작 복귀를 확인했다. 다운/기상·국소 히트스톱의 개별 금지 조건은 QA에서 상태를 직접 구성했고, 공중 추격/공중 체술은 실제 입력·전투 경로로 재현했다.

별도 기존5개 회귀도 PASS: 독립4타 피해·버퍼·넉백, 유효 마무리 시네마틱/카메라 복귀, 반대 카메라의 락온 타격, 다른 락온이 기존 Space 추격/공중 타격 대상을 빼앗지 못함, 최대 주력100/150/200의 F 충전/자원 이벤트.

측정:
- W: **3.500m**, 입력 주입 전부터 종료 관측까지0.2494s.
- A/D: **2.400m / 2.400m**, 각각 약0.1999s.
- S: **2.1999998m**, 약0.2330s.
- 이 시간은 입력 주입/관측의 프레임 간격을 포함한다. 저장 동작시간0.22/0.18/0.20을 변경하지 않았다. 프레임 단위로 종료를 관측하므로 정확히 같은 벽시계 시간으로 보고하지 않는다.
- 측면 시작 후16.05ms는 Applied,83.38ms는 Guarded,167.29ms는 Applied. 후방83.55ms도 Guarded. 전방·Unblockable·술식·환경 대조는 Applied.
- 벽 앞 Z=-7.480m에서 막힌 동안 요청 거리2.3838m 소비. 벽 제거 후 이동은0.8183m였고 막힌 전체 거리를 몰아서 이동하지 않았다. 타 캐릭터 최종 캡슐 간격0.73m로 관통하지 않았다.
- 8회 대시 후 transform13개/mesh binding7개/material binding7개 및 참조 ID 동일. 주력100 유지. 종료 후0.25초 일반 이동1.2015m, 종료 직후 공격 허용.

과거 PASS4A 회귀18건을 이번 검사로 전부 해결했다고 선언하지 않는다. 일부 중복 케이스의 최신 targeted PASS와 전체18건 원인 규명은 다르다.

## 5. 원속도 영상

최종 영상은 `media-framed/`:

- `front-free-W.mp4`: 비락온 W+C / 정면 기술 촬영.
- `diagonal-free-A.mp4`: 비락온 A+C / 사선 촬영.
- `locked-D.mp4`: 락온 D+C / 두 캐릭터가 보이는 촬영.
- `runtime-camera-W.mp4`: 기존 실제 OrbitCamera를 켠 비락온 W+C.

각각960×720 H.264, 약2.15~2.18초,51~54개 캡처 프레임. 실제 게임 타임스탬프를 VFR로 인코딩했고 시간 가속/보간은 하지 않았다. 고정60fps 영상이 아니며 순간적인 동작은 캡처 표본 수에 한계가 있다. 첫3개는 QA 추종/구도 카메라이고 마지막은 실제 게임 카메라다. 각 `*-sequence.png`와 원본 타임스탬프 JSON도 함께 남긴다.

최초 영상은 락온 구도에서 플레이어가 가장자리로 빠져 확인이 어려웠다. 게임 동작은 바꾸지 않고 QA 카메라 구도와 화면 내 플레이어 검사를 보완해 재촬영했다. 이전 `media/`와 원본 프레임도 삭제하지 않았다.

## 6. 코드·설정 변경과 보호

기존8개 변경:
- `unity/Assets/Shibuya/Scripts/ShibuyaFighter.cs`: 대시 상태/기존 단일 motor 연결·입력/중단.
- `Scripts/ShibuyaPlayerInput.cs`: C KeyDown/래치/입력 우선순위.
- `Scripts/ShibuyaCinematicFighterRuntime.cs`: 기존 guard 뒤 회피 연결, 진입 시 대시 취소.
- `Scripts/ShibuyaBlockVisual.cs`: VisualRoot의 작은 방향별 스텝 기울기. Collider/루트 위치를 시각 모션으로 옮기지 않는다.
- `Scripts/ShibuyaCombatProfile.cs`, `Editor/ShibuyaInspectors.cs`: 공통 SO 참조/한국어 이름.
- `Editor/ShibuyaPass3CTestRunner.cs`: 새4C QA 출력 경로 허용만 추가.
- `Data/Shibuya Combat Profile.asset`: groundDash 참조만 추가.
앞 두 항목 이후의 상대 경로는 `unity/Assets/Shibuya/` 기준이다.

신규: `Scripts/ShibuyaGroundDashProfile.cs`, `Scripts/ShibuyaGroundDashRuntime.cs`, `Editor/ShibuyaGroundDashInspector.cs`, `Editor/ShibuyaPass4CTests.cs`, `Data/Shibuya Ground Dash.asset` 및 각각meta. 이 보고서와 현재 작업 문서도 갱신한다.

기존 Gojo/VFX·모델·애니메이션·아우라 프로필/Shader·크기 곡선/색·주력 충전/HUD 코드·카메라 코드·Scene/Prefab·Packages/ProjectSettings·기존meta를 변경하지 않았다. 기본 피해20/20/20/25, Dummy HP1000, Player HP100 유지. `USER_VERIFIED_SETTINGS.md`는 수정하지 않는다. 신체 강화/대시 공격/공중 대시/공격 캔슬/가드·패링·잡기는 미구현이다.

## 7. 컴파일·Console·환경·남은 검수

최종 C# compile 성공. 테스트 종료 직후 Console은 error0/warning1이었으나, 최종 복합 상태 조회1회가 Pipeline main-thread 5초 timeout으로 실패했다. 이후 작은 조회는 정상 복구됐고 최종 Console은 **error1/warning1**이다. 오류는 이 도구 조회 timeout, 경고는 기존 `Deleting invalid font reference.`이며 지우지 않았다. 테스트 중 게임 예외는 검출되지 않았지만 전체 세션을 error0으로 보고하지 않는다. 실패 테스트 XML과 Console 오류도 별개로 보고한다. shader를 수정하지 않았다. 반복 대시에 새 Renderer/Material/Mesh를 만들지 않으며 바인딩 동일성을 확인했다. 실제 GPU/GC/다인전 성능 프로파일링은 하지 않았다.

QA에서만 targetFrameRate60을 사용하고 원래 -1로 복구했다. 최종 작은 조회에서 Play stopped / 씬 clean / activeQA=false도 확인했다. AutoTick/Play Unfocused/Time.timeScale은 변경하지 않았다. Computer Use/Windows 직접 조작/포커스 요청 없음. staging/commit/push/reset/clean/merge/worktree 생성 없음.

직접 Play 확인:
1. `ShibuyaCombatMVP`에서 방향키+C. C만 누르거나 계속 유지하면 새 대시가 반복되지 않아야 한다.
2. W/좌우/S와 전후 대각선을 비교하고, 종료 직후 공격·이동을 확인한다. 같은 프레임 Space 또는 공격은 C보다 우선한다.
3. MMB로 락온한 뒤 W/S/A/D의 타깃 기준 방향과 경로 고정을 확인한다. 락온 해제 후 카메라 기준으로 돌아와야 한다.
4. 측면·후방은 시작 직후와 끝부분이 무적이 아니다. 기본 더미는 공격하지 않으므로 회피 타이밍의 자동 피해 대조는 QA에 있고 사용자 체감 평가는 후속 공격 AI/대전에서도 필요하다.
5. 벽·더미 앞에서 관통/이동량 누적이 없는지, F 홀드에서는 C가 차단되고 주력/HUD/승인 아우라가 정상인지 확인한다.
6. 거리·시간·무적·쿨타임·속도 Curve·스텝 기울기는 새 한국어 Data Asset Inspector에서 조절한다. 이번 값은 초기 실험값이다.

실제 손맛, 급격한 시점 변경/방향 반전, 비락온 시작 몸 방향 유지, 다양한 지형·동시 이동하는 캐릭터·지속 저프레임은 사용자 검수/후속 경계다. CODEX VALIDATED는 targeted 기술 범위이며 새 기능 USER VERIFIED 또는 전체 과거 회귀 해결을 뜻하지 않는다.
