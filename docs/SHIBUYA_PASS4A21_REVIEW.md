# PASS 4A.2.1 — 화면 밖 락온 대상 복구

2026-10-23 / `feat/shibuya-combat-mvp` / HEAD `42833ebfec9a176056ae983096c41c1ef4d8fb24`.

**LOCAL UNCOMMITTED / targeted 범위 CODEX VALIDATED / PENDING USER PLAY REVIEW.**
기존 PASS 4A 전체 회귀 미해결 18건은 해결로 처리하지 않는다. USER VERIFIED 승격, 전체 190개 재실행, staging/commit/push 없음.

## 실제 재현과 수정

약 5.6m 뒤쪽 대상, pitch 상한 70°, 수동 조작 유예 종료 조건에서 수정 전 테스트가 실패했다. 6초 뒤 yaw 0°, pitch 70°, 최종 viewport `(0.500, -0.449, 4.914)`로 화면 밖에 남았다. 기존 계산은 pitch를 약 33.35° 더 올리려 했지만 clamp 후 변화가 0°였다. 환경 가림은 없었다. **카메라 앞(z>0)에 있는 대상도 orbit 중심 뒤쪽에서는 화면 밖 복구가 정체될 수 있다.**

변경 후 같은 테스트/기대값에서 pitch 70°를 유지하고 yaw 약 109.26°로 회전했다. 최종 viewport `(0.785, 0.688, 7.461)`로 복귀하고 이후 0.6초 동안 화면 안 유지 검사를 통과했다. 단순 yaw 변화가 아니라 실제 출력 Camera.WorldToViewportPoint로 판정했다.

- 기존 수동 조작 0.8초, MMB 탐색, 시네마틱/복귀 블렌드 및 복귀 후 0.5초 대기를 우선한다.
- 화면 안에서는 기존 Lazy Lock-on/Dead Zone/겹침 보정을 유지한다.
- 화면 밖에서 pitch가 막히거나 일반 보정이 진행하지 못하면 유한한 복구를 시작한다.
- 대상의 orbit 중심 방향을 기준으로 최대 10개 yaw/pitch 후보를 한 번 평가한다. 기존 focus·어깨·거리·충돌 arm을 반영하며, 후보 평가가 실제 arm 상태를 진행시키지 않도록 계산을 분리했다.
- 현재 pitch를 유지하는 후보와 대상 높이를 고려한 후보 중 가시성이 확보되는 작은 각도 변화 방향을 선택한다. 대칭 상황은 기존 어깨 방향으로 결정하고 시도 중 방향을 뒤집지 않는다.
- 기존 회전 속도/가속도/한 프레임 상한을 재사용한다. 복구 중 구도 오프셋을 정지하여 서로 싸우지 않게 한다.
- 실제 출력 대상이 화면 여백 안에서 최소 3프레임 및 0.12초 유지돼야 일반 보정으로 돌아간다.
- 가능한 후보가 없거나 4초 상한에 도달하면 무한 회전 대신 멈춘다. `ScreenRecoveryStatus`로 상태/실패 원인을 조회할 수 있다. 수동 조작, 재선택 또는 의미 있는 대상 이동 등으로 새 시도가 가능하다.
- UI는 환경 가림 / 화면 밖 / 화면 안을 구분한다. 화면 밖이라는 이유로 락온을 해제하지 않으며, 선택·거리·LOS 규칙은 수정하지 않았다.

## 변경 파일 — 이번 패스 기준

기존 로컬 파일 추가 수정 5개:

- `unity/Assets/Shibuya/Scripts/ShibuyaLockOnFramingRuntime.cs`: 경계 복구 호출 및 상태 정리, 충돌 pose 평가 분리.
- `unity/Assets/Shibuya/Scripts/ShibuyaLockOnProfile.cs`: 복구 설정 5개 및 유효성 검사.
- `unity/Assets/Shibuya/Editor/ShibuyaLockOnInspector.cs`: 한국어 SerializedProperty 표시.
- `unity/Assets/Shibuya/Resources/Shibuya Lock On.asset`: 신규 설정의 명시적 기본값.
- `unity/Assets/Shibuya/Scripts/ShibuyaLockOnPresentation.cs`: 환경 가림과 화면 가시성 UI 구분.

신규 5개:

- `unity/Assets/Shibuya/Scripts/ShibuyaLockOnRecoveryRuntime.cs` 및 `.meta`.
- `unity/Assets/Shibuya/Editor/ShibuyaPass4A21Tests.cs` 및 `.meta`.
- 이 보고서.

기존 OrbitCamera 본체, CinematicCameraRuntime, Fighter/Targeting/Aerial 코드 및 기존 테스트는 이번 패스에서 변경하지 않았다. 보호 스냅샷 175개 중 위 5개 외 170개는 SHA-256 동일, 누락 0개다. 기존 미커밋 PASS 3D/4A/4A.1/4A.2와 보고서, Scene/Prefab/기존 GUID/Packages/ProjectSettings/잠금값을 보존했다. 근거는 QA 루트의 `preservation-check.json`.

## Inspector

`Assets/Shibuya/Resources/Shibuya Lock On.asset`:

| 한국어 표시 | 기본값 |
|---|---:|
| 화면 밖 대상 복구 사용 | ON |
| 화면 복귀 성공 여백 (뷰포트 비율) | 0.06 |
| 일반 보정 정체 확인 시간 (초) | 0.3 |
| 화면 복귀 안정 확인 시간 (초) | 0.12 |
| 화면 복구 시도 시간 상한 (초) | 4 |

별도 복구 회전 속도는 만들지 않았다. 기존 100°/초·가속 180°/초²·보조 강도 0.7을 재사용한다. 기존 공격 회전 2400°/초·최대 180°·준비 상한 0.12초, 선택/유지 50/60m, 환경 가림 유예 1.5초 및 기존 카메라 값은 불변이다. 한글 필드 등록과 SerializedProperty/Undo 기존 검사를 통과했다.

## 자동 검증 및 원본 실패

QA 루트: `unity/Logs/ShibuyaPass4A21QA/20261023-184455/`.

| 별도 실행 | PASS / FAIL / SKIP |
|---|---:|
| `before-fix/results.xml` — 수정 전 원인 재현 | 0 / 1 / 0 |
| `after-fix-pilot/results.xml` — 동일 재현 검사 | 1 / 0 / 0 |
| `targeted/results.xml` — 나머지 신규 6개 + 기존 4개 | 9 / 1 / 0 |
| `cinematic-grace-recheck/results.xml` — 신규 유예 검사 시점 수정 후 | 1 / 0 / 0 |

**수정 후 복수 실행 12회, 최신 고유 11케이스 = 11 PASS / 0 FAIL / 0 SKIP.** 신규 7개 + 기존 4개. 단일 11개 전체 실행 또는 전체 회귀 통과로 표현하지 않는다. `validation-summary.json`, 원본 XML, execution 및 계측 JSON을 보존한다.

신규 시네마틱 검사 최초 실패는 테스트 시점 오류였다. 복귀 완료 후 추가 프레임 검사를 수행하고 다시 0.5초를 기다려, 이미 유예가 끝난 뒤에도 yaw 불변을 요구했다. 실제 차이는 약 0.00323°이고 위치/각도 복귀 점프는 0m/0°였다. 실패 stack은 테스트 156행이다. 런타임/허용 오차를 바꾸지 않고 실제 `lockResumeAt` 이전 모든 관측 프레임에서 yaw·pitch 불변을 검사하도록 수정했다. 재검증에서 유예 중 257프레임 불변, 이후 실제 화면 복귀를 통과했다. 실패 당시 테스트 소스도 `targeted/test-source-before-grace-clock-fix.txt`로 보존한다.

확인 범위:

- 최고 pitch 정면·뒤 대각선·정반대, 복구 방향 일관성과 한 프레임 회전 상한, 화면 복귀 후 유지.
- 수동 입력/0.8초 유예/MMB 탐색 양보.
- 기둥 근처 충돌 안전, 공중 높이 대상의 실제 출력 복귀. 공중 fixture는 y=7m에서 국소 정지해 카메라만 분리 검사했으며 실제 추격 전체 회귀가 아니다. viewport 약 `(0.736, 0.937, 8.709)`, pitch -25°로 화면 위쪽 가까이에 남는다.
- 보호된 pitch/focus 범위에서 불가능한 35m 직상방 fixture는 제한 상태에서 안정적으로 정지. 해제/대상 사망 시 상태 초기화.
- 복구 도중 시네마틱이 우선권을 갖고 종료 후 위치·각도 점프 0m/0°, 유예 후 화면 복귀.
- 기존 비중앙 Dead Zone 유지, 근거리 겹침 완화/크기 유지, 한국어 Inspector/Undo/공격 회전값, 몸·카메라 반대 방향에서 실제 락온 공격 적중.

## Compile / Console / 환경

- 신규 테스트에서 private `ApplyLocalHitStop`을 직접 호출한 컴파일 오류 2개를 테스트용 reflection 호출로 수정했다. 게임플레이 API는 바꾸지 않았다.
- 마지막 compile: error 0 / warning 0. 최종 Pipeline 연결 정상, Editor Play Mode 종료, 실행 중 QA 없음, 미저장 씬/dirty Data Asset 없음.
- 최종 Editor Console ground truth: error 0 / warning 0. Pipeline 누적 버퍼에는 위 수정된 컴파일 오류 2개와 기존 `Deleting invalid font reference.` 경고 13개가 남아 있다. 로그를 지우지 않았다. `final-console.json`에 둘을 구분해 보존한다.
- AutoTick/프레임 설정을 이번에 변경하지 않았다. 최종 AutoTick true/16ms, SessionState 저장 간격 없음(-1), targetFrameRate -1, vSync 0, Time.timeScale 1. Play Unfocused(2) 유지. `final-editor.json` 참조.
- Computer Use, 포커스 강제 전환 옵션, 추가 전체 회귀 없음.
- 최종 Git: 기준 HEAD/브랜치 유지, staged 0 / tracked unstaged 27 / untracked 48. 이전 PASS의 미커밋 작업을 포함한 전체 수이며 이번 패스 파일 수와 다르다. `git diff --check` 오류 없음(기존 파일의 LF→CRLF 안내만 출력).

## 사용자 Play Mode 확인

1. 저장된 `Assets/Shibuya/Scenes/ShibuyaCombatMVP.unity`를 열고 Play. 기존 MMB 누름→놓기로 더미를 선택한다.
2. 약 5~6m에서 카메라를 최고 하이 앵글까지 올리고 상대가 뒤쪽/뒤 대각선으로 화면 밖에 오도록 돌린다. 마우스를 놓고 0.8초 뒤 좌우 orbit으로 상대가 돌아오는지 확인한다.
3. 돌아오는 도중 마우스를 움직이거나 MMB 탐색을 시작하여 즉시 수동 조작을 존중하는지 확인한다. 이미 잘 보이는 구도는 중앙으로 강제 정렬되지 않아야 한다.
4. 기둥/벽 근처, 어퍼컷 대상, 가까운 거리에서 불쾌한 회전·재이탈·급격한 충돌 복귀가 없는지 확인한다. 카메라를 반대로 돌려 LMB를 눌러도 승인된 공격 회전은 락온 대상을 향해야 한다.
5. 시네마틱은 기존 `JJK Game → 시부야 PASS3D → 시네마틱 예제 ON (Play Mode 전용)` 후 기본 4타 적중으로 확인한다. 같은 메뉴 OFF 또는 Play 종료로 해제한다. 시네마틱 중 복구 개입 없이 원래 구도로 부드럽게 돌아와야 한다.

## 남은 한계

이는 작은 정적 후보 집합을 쓰는 경계 복구이며 범용 카메라 경로 탐색기가 아니다. 복잡한 좁은 공간, 빠른 이동 대상 또는 보호된 pitch 범위를 크게 벗어난 높이에서 모든 장면의 화면 복귀를 보장하지 않는다. 실패 상태는 수동 입력/대상 변화로 새 시도가 가능하지만 모든 환경 변화의 자동 재탐색을 구현한 것은 아니다.

가시성 성공 기준은 대상 조준점이며 캐릭터 전체 bounds가 화면 안에 들어옴을 보증하지 않는다. 공중 검사에서도 위쪽 여유가 작았다. 저프레임·여러 해상도/화면비·빠른 연속 공중전의 사용감은 사용자 Play Mode 검수가 필요하다. 구도/멀미/자연스러움은 USER VERIFIED로 선언하지 않는다.
