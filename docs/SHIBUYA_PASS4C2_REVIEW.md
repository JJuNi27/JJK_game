# PASS 4C.2 — 락온 여부와 무관한 카메라 기준 지상 대시

후속 사용자 승인: PASS4D.0 요청에서 카메라 기준 대시 방향·속도의 직접 확인/승인을 명시했다. 자동 경계 검사를 모두 수동 승인으로 확대하지 않는다. 아래는 구현 당시 보고이며, 새 가드 작업은 `docs/SHIBUYA_PASS4D_REVIEW.md`를 참조한다.

2026-10-30 KST · `feat/shibuya-combat-mvp` · HEAD `1202e940176daec9fbb0b8027702f600b2f677d4`.
**LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기**.
사용자는 PASS4C.1 거리·속도·쿨타임과 비락온 방향에 만족한다고 피드백했다. 이번 새 락온 대시 방향의 직접 승인은 아직 받지 않았다.

## 1. 원인과 최소 수정

`ShibuyaGroundDashRuntime.TryGroundDash`는 기존에 유효한 `ExplicitLockTarget`이 있으면 `cameraForward` 대신 대상-공격자 위치 차이를 forward로 사용하고 right도 그 방향의 접선으로 계산했다. 그 결과 같은 카메라/WASD에서도 락온 상태만 이동 기준이 바뀌었다. 이 forward를 `dashFacing`에도 공유했다.

이제 이동은 항상 XZ에 투영·정규화한 cameraForward/cameraRight로 계산하고, 두 입력 성분을 합한 뒤 정규화한다. 타깃 위치는 이동 벡터에 사용하지 않는다. 정확히 수직인 카메라에서 forward가 퇴화하면 수평 cameraRight의 외적으로 복원한다. 두 카메라 축이 모두 퇴화한 예외만 수평 몸 방향/월드 전방으로 fallback한다. 일반 비락온 계산을 유지하며 유효한 카메라 방향에 새 타깃 정책을 섞지 않았다.

`GroundDashDirection`은 시작 순간 한 번 저장하며 기존 AdvanceGroundDash는 저장된 변위만 사용한다. 카메라/상대가 움직여도 재조준하지 않는다.
바라보기는 이동과 분리했다. 비락온에서는 시작 몸 방향 유지, 락온에서는 시작 타깃까지의 수평 방향을 별도 `lockFacing`으로 저장한다. 기존 시작 방향 유지 정책이며 실시간 자동 회전/새 락온 정책을 도입하지 않았다. 같은 위치의 타깃처럼 시선 벡터가 퇴화하면 기존 몸 방향을 유지한다. 대시 후 기존 락온 근접 공격 보정은 그대로 사용한다.

실제 이동은 계속 ShibuyaFighter의 기존 CharacterController motor 한 곳이 소유한다. LockOn/LockOnCameraRuntime/OrbitCamera/PlayerInput/Fighter 본체 코드는 이번 PASS에서 수정하지 않았다. 대시 종류·무적·회복/중단·충돌 구조도 변경하지 않았다.

## 2. 유지한 Data Asset

| 방향 | 거리 | 지속시간 | 일반 체술 무적 |
|---|---:|---:|---|
| W 및 W 성분 대각선 | 5.25m | 0.22s | 없음 |
| A/D | 3.60m | 0.18s | 0.04~0.14s |
| S 및 후방 대각선 | 3.30m | 0.20s | 0.04~0.14s |

종료/중단 후 쿨타임0.45s、speedCurve、스텝 기울기18도와 기존 한글 Inspector/프로필/메타를 byte 동일하게 보존했다. 주력 소모·공중 대시·공격 캔슬·VFX·신체 강화 추가 없음.

## 3. 실제 Unity targeted 결과

QA: `unity/Logs/ShibuyaPass4C2QA/20261030-camera-basis/targeted/`.
**단일 실행 20 PASS / 0 FAIL / 0 SKIP** (`results.xml`, `execution.jsonl`, `discovery.json`). 이전4C/4C.1 결과를 합산한 숫자가 아니다. 전체 전투 회귀 실행도 아니다.

- 신규5개: 8방향×락온/비락온16회 비교, 후방/측면 카메라·고피치, 근거리/원거리 움직이는 상대+실제 OrbitCamera 조작 중 경로 고정, 대시 후 락온 공격, 원속도5상황 촬영.
- 기존15개: 데이터/한글 Inspector/Undo와 대시 관련 동작14개. 기존 타깃 기준을 기대하던 `LockedDirectionsSnapshotAndFreeFallback`만 새 카메라 기준 정책으로 기대 벡터를 변경했다. 각도 허용0.1도와 나머지 판정 기준은 유지했다.
- 동일 yaw37/pitch25에서 8방향 모두 락온·비락온 방향 오차0.1도 미만. 비락온 몸 yaw-60 유지, 락온 몸은 대상 방향 유지도 확인했다.
- yaw180/pitch20, yaw90/pitch85, yaw-90/pitch-75, yaw37/pitch90에서 W+D 방향이 기대값0.1도 이내이고 Y 성분은0이다. 실제 수평 이동은5.24934~5.250001m.
- 대상 시작 거리3m/25m, 대상 이동 및 실제 카메라 수동 회전 중 측면 변위 모두 X3.60m/Z0m. 카메라 최종 yaw300/240으로 변해도 시작 경로를 유지하고 락온을 유지했다.
- 대시 후 락온 1타 유효 적중, 더미 HP1000→980. 기존 공격 방향 시스템을 교체하지 않았다.
- 기존 거리 실측 W5.25/A3.60/D3.60/S3.30000019m. 정상 종료·F 중단 시 잔여 쿨타임 각각0.45s, 만료 전 재시작 거부/만료 후 허용. 첫 만료 관측은0.466880/0.459836s로 프레임 경계가 포함된다.
- 무적 구간/실제 피해 차단/전방 무적 없음, 대각선 분류, C 홀드, 상태별 시작 금지, 벽·캡슐 충돌, 시네마틱 소유권, F 충전, Space 추격/공중 평타 중 차단, 반복 리소스·이동/재공격 복귀를 확인했다.
- 과거 PASS4A 회귀18건을 해결로 소급 판정하지 않는다.

## 4. 원속도 비교 자료

`unity/Logs/ShibuyaPass4C2QA/20261030-camera-basis/media/`:

- `free-WD.mp4`, `locked-WD.mp4`: 동일 시작 위치·yaw0 카메라. 둘 다 시작 월드 방향 X/Z=(0.707106769,0.707106769).
- `locked-near-D.mp4` (시작 거리5m), `locked-far-D.mp4` (25m).
- `live-camera-WD.mp4`: 대시 도중 실제 OrbitCamera 수동 입력, 경로 고정.
- `free-vs-locked-WD.jpg`: 동일 카메라의 인접 시간 비교. 각 영상의 `-sequence.jpg`와 원본 timestamps도 보존했다.

960×720 H.264 VFR, 실제 게임 캡처 시각 간격 기준. 속도 변경·프레임 보간 없음. 약1.76~1.78초/42~44표본으로 짧은 대시를 보여준다. 처음4개는 방향 비교용 촬영 구도, 마지막은 실제 OrbitCamera를 사용했다. 기술 자료이며 최종 조작감·카메라 품질 승인을 대신하지 않는다.

## 5. 보호·환경·변경 파일

변경 전1205개 Git 추적/비추적 파일 SHA-256을 저장했다. 문서 정리 전에는 dash runtime과4C 테스트 파일만 변경됐다. 기존4C/4C.1 미커밋 작업, Gojo/VFX/승인 아우라、Scene/Prefab/Packages/ProjectSettings、Data Asset/메타、HP1000·100、피해20·20·20·25는 그대로다. 이전 QA 원본/실패 XML과 변경 전 소스를 보존했다.

C# 컴파일 성공. 종료 시 Play=false, 씬 dirty=false, QA=false, targetFrameRate=-1, Time.timeScale=1. 테스트에서만60fps를 사용하고 복구했다. AutoTick/Play Unfocused/포커스 설정 변경 없음. 최종 Console 현재 error0/warning1 (`Deleting invalid font reference.`). Pipeline 버퍼의 이전 timeout 오류1건·누적 font 경고65건은 보존했으며 전체 세션 무오류라고 표현하지 않는다.

이번 변경 파일6개: `Scripts/ShibuyaGroundDashRuntime.cs`, `Editor/ShibuyaPass4CTests.cs` (이상 `unity/Assets/Shibuya/` 기준), 이 보고서, `docs/CURRENT_HANDOFF.md`, `docs/active/CURRENT_TASK.md`, 기존4C.1 보고서 후속 링크. Inspector/에셋 신규 생성 없음. staging/commit/push/reset/clean/merge/rebase/worktree/Computer Use 없음.

## 6. 사용자 직접 검수

ShibuyaCombatMVP Play → 카메라를 같은 방향에 두고 비락온/락온 W+D+C 비교 → A/D/S 및 나머지 대각선 → 카메라를 뒤/옆/하이 앵글로 회전해 재시도 → 진행 중 마우스 회전 → 근거리/원거리 상대 → 대시 후 공격/F/Space 추격 확인. 대시는 시작 때의 카메라 기준을 유지하므로 진행 중 카메라를 돌린 뒤 새 화면의 오른쪽으로 계속 꺾이는 동작은 의도적으로 없다.

남은 한계: 짧은 자동 입력/카메라 표본이며 실제 손 조작의 체감은 사용자 검수 대기다. 큰 프레임 지연·동시 다인전·복잡한 지형의 전체 회귀/성능은 이번 targeted 결과로 보증하지 않는다. 추가 기능이나 카메라 재설계 없이 종료한다.
