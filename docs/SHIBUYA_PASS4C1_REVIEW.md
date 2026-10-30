# PASS 4C.1 — 지상 대시 거리·쿨타임 튜닝

후속 PASS4C.2에서 거리·쿨타임은 유지하고 락온 대시 이동을 카메라 기준으로 통일했다. 최신 방향 정책은 `docs/SHIBUYA_PASS4C2_REVIEW.md`를 참조한다. 아래는 당시4C.1 검증 기록이다.

2026-10-30 KST · `feat/shibuya-combat-mvp` · HEAD `1202e940176daec9fbb0b8027702f600b2f677d4`.
**LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기**.

## 변경한 실제 Data Asset

`unity/Assets/Shibuya/Data/Shibuya Ground Dash.asset`의 네 scalar만 변경했다.

| 설정 | PASS 4C | PASS 4C.1 | 유지한 지속시간 |
|---|---:|---:|---:|
| 전방 거리 | 3.50m | 5.25m | 0.22s |
| 좌우 공유 거리 | 2.40m | 3.60m | 0.18s |
| 후방 거리 | 2.20m | 3.30m | 0.20s |
| 종료/중단 후 공통 쿨타임 | 0.65s | 0.45s | 해당 없음 |

기존 speedCurve 전체 직렬화, 무적 지연0.04s/지속0.10s, 스텝 기울기18도, GUID/참조는 byte 비교로 유지했다. Profile 클래스의 생성 초기값은 바꾸지 않았다. 실제 게임은 연결된 Data Asset을 읽는다. 한국어 Inspector의 기존 거리·시간·쿨타임 필드를 그대로 사용한다.

ShibuyaFighter/PlayerInput/GroundDashRuntime/LockOn/OrbitCamera/시네마틱/주력/HUD 소스는 이번 PASS 시작 시 SHA-256과 동일하다. 이전 PASS4C 미커밋 수정도 보존했다. 방향 스냅샷·대각선 정규화·단일 이동 소유권·충돌·입력 우선순위 변경 없음.

## 실제 Unity targeted 검증

QA 루트: `unity/Logs/ShibuyaPass4C1QA/20261030-tuning/`.

- `targeted/results.xml`: **단일 실행 16 PASS / 0 FAIL / 0 SKIP**. 데이터/한국어 Inspector/Undo 1, 동작 검사14, 다섯 상황 촬영1.
- `far-framing/results.xml`: **별도 실행 1 PASS / 0 FAIL / 0 SKIP**. 원거리 촬영 구도 보완 및 두 캐릭터 viewport 검사.
- 두 실행의 고유17케이스이며 단일17개 전체 실행이 아니다. 과거 PASS4C 20케이스를 더하지 않는다. 과거 PASS4A 회귀18건의 해소를 선언하지 않는다.
- 기존 검사 수치 기대값을 요청한 정책으로 갱신했다. 거리 허용오차0.04m 및 기존 시간/충돌/피해 기대값 유지. 종료 직후 쿨타임은 새0.45s 기준 관측 허용0.06s로 검사한다. 종료·F 중단의 만료 전 재시작 거부/만료 후 허용을 추가 확인했다.

장애물이 없는 동일 위치·카메라 기준 실제 수평 이동:

| 입력 | 실측 거리 | 입력 요청부터 종료 관측까지 |
|---|---:|---:|
| W+C | 5.250000m | 0.249788s |
| A+C | 3.600000m | 0.200095s |
| D+C | 3.600000m | 0.199661s |
| S+C | 3.300000m | 0.216631s |

관측 시간에는 입력 소비·검사 프레임 경계가 포함된다. Data Asset 지속시간0.22/0.18/0.20s를 늘린 것이 아니다. 원본: `targeted/legacy/ShibuyaPass4B3QA/4C-cardinal.json`.

정상 종료와 F 중단 모두 종료 관측 시 잔여 쿨타임0.45s. 만료 후 첫 관측은 각각0.450416s/0.463006s. 만료 전 시작 거부, 만료 후 새 요청 허용. C 홀드 자동 반복 없음.
측면 일반 체술은16.60ms에 Applied,83.29ms에 Guarded,166.73ms에 Applied. 전방은83.65ms에도 Applied. 무적 설정0.04~0.14s 유지.
벽에 막혔을 때 z=-7.480, 소모된 예정 이동3.240664m, 벽 제거 후1.518571m로 막힌 이동을 누적하지 않았다. 다른 캡슐과 간격0.73m, 관통 없음.
락온 시작 방향 고정·해제 후 카메라 기준 복귀, 공격/점프/피격/다운/공중/추격/히트스톱/사망 차단, 시네마틱 소유권, 실제 회피 피해·히트스톱·시네마틱 미발동, F 충전, 반복8회 리소스 바인딩·이동/재공격 복귀 검사를 통과했다.

## 원속도 자료

`unity/Logs/ShibuyaPass4C1QA/20261030-tuning/media/`:

- `free-forward.mp4`
- `lock-near-side.mp4` (대상 시작 거리5m)
- `lock-far-side-unobstructed.mp4` (대상 시작 거리25m)
- `free-back.mp4`
- `cooldown-repeat.mp4` (별도 C 입력3회)
- 같은 이름의 `-sequence.jpg` 인접 프레임 자료.

960×720 H.264. 실제 게임 시간의 캡처 간격을 반영한 VFR, 가속/슬로모션/프레임 보간 없음. 30fps 캡처 목표이나 실제 표본은 약23~26fps이며 timestamps를 보존한다. 검증용 구도이며 실제 자유/락온 OrbitCamera 품질의 사용자 승인을 대체하지 않는다. 최초 `lock-far-side.mp4`는 건물 가림으로 촬영 품질이 부족했다. 삭제하지 않고 별도 `far-framing` 실행으로 촬영만 보완했다. 기존 PASS4C 영상도 보존했다.

## 환경·보호·파일

C# 컴파일 성공. 최종 Editor Console 현재 error0/warning0, compilationFailed=false. Pipeline 보존 버퍼에는 이전 `/api/exec` 5초 timeout 오류1건과 누적 `Deleting invalid font reference.` 경고42건이 남아 있다. 전체 세션 무오류라고 주장하지 않으며 Console을 지우지 않았다.
종료 시 Play=false, 씬 dirty=false, QA=false, targetFrameRate=-1, Time.timeScale=1. 테스트에서만60fps를 사용하고 복구했다. AutoTick/Play Unfocused 설정과 포커스를 변경하지 않았다.
작업 시작1204개 추적/비추적 파일 SHA-256 비교에서 문서 갱신 전 변경은 dash asset과4C 테스트 파일 둘뿐이었다. Scene/Prefab/Packages/ProjectSettings/기존meta/Gojo/VFX/승인 아우라/HP1000·100/피해20·20·20·25 불변. 기존 XML/실패 기록/미커밋 파일 보존. `git diff --check` 통과. 실제 GPU/GC 성능 및 극단적 저프레임 전투 전체 회귀는 수행하지 않았다.

이번 변경: dash asset, `Editor/ShibuyaPass4CTests.cs`, 이 보고서, `docs/CURRENT_HANDOFF.md`, `docs/active/CURRENT_TASK.md`, 기존4C 보고서의 후속 링크. 런타임/Inspector 코드 변경 없음. staging/commit/push/reset/clean/merge/rebase/worktree 없음.

직접 검수: `ShibuyaCombatMVP` Play → 비락온 WASD+C와 대각선 → MMB 락온 후 근거리/원거리 A/D/S+C → 마우스 자유 회전 → C 홀드와 종료 후 짧은 재입력 → 벽/더미 앞 대시 → F 충전 및 Space 추격 확인. 거리·속도 체감과 락온 측면의 자유 카메라 조합은 사용자 확인 대기다. 추가 방향 정책/카메라 수정/후속 기능은 구현하지 않았다.
