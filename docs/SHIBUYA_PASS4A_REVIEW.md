# PASS 4A — 공통 타깃 락온 및 3D 조준 보조

작성: 2026-10-23 / `feat/shibuya-combat-mvp`
기준 HEAD: `42833ebfec9a176056ae983096c41c1ef4d8fb24`.

**LOCAL UNCOMMITTED / 전체 검증 보류(INCONCLUSIVE) / PENDING USER PLAY REVIEW.**
PASS 4A에 대한 사용자 직접 승인은 아직 없다. 기존 PASS 3D의 기능 승인과 구분한다.

## 시작 상태와 보존

- staged 0 / unstaged 25 / untracked 19. 기존 PASS 3D 체크포인트 제안 44개는 아직 미커밋이다.
- 수정 전 44개 파일을 새 QA 루트 `unity/Logs/ShibuyaPass4AQA/20261023/before/`에 복사하고 SHA-256을 기록했다.
- Pipeline 확인: ShibuyaCombatMVP, Play Mode 종료, 미저장 씬 없음, dirty Data Asset 없음. 기존 Console error 1 / warning 1.
- 기존 피해 20/20/20/25, 넉백 6/4/4/11, 강타 히트스톱 .065초, Player HP 100 / Dummy HP 1000 확인.
- 기존 Scene/Prefab/Data/Packages/ProjectSettings/locked 문서 48개와 이전 QA 파일 472개의 보존 해시를 기록했다.
- 기존 PASS 3D 문서, 실패 XML 및 검증 이력을 덮어쓰지 않는다. 새 테스트 출력은 모두 새 QA 루트로 분리한다.
- 기존 미커밋 파일 중 PASS 4A가 추가 수정한 파일은 AerialCombatRuntime, OrbitCamera, Pass3CTestRunner의 3개다. 나머지 41개는 시작 시점 해시와 동일하다. `pass4a-only.patch`에 이번 다섯 기존 코드 파일의 변경만 별도로 기록했다. 향후 PASS 3D만 커밋하려면 이전 44개 목록을 그대로 stage하지 말고 공유 파일의 추가 변경도 구분해야 한다.

## 구현과 책임

| 파일 | 역할 |
|---|---|
| `unity/Assets/Shibuya/Scripts/ShibuyaLockOnProfile.cs` | 공통 락온 튜닝 데이터 및 유효성 검사 |
| `unity/Assets/Shibuya/Scripts/ShibuyaLockOn.cs` | MMB 상태, 후보 선택, 유지/상실, 탐지 정책, 발사 시점 3D 조준 값 |
| `unity/Assets/Shibuya/Scripts/ShibuyaLockOnPresentation.cs` | 후보/확정/가림/상실 최소 표시와 선택적 디버그 선 |
| `unity/Assets/Shibuya/Scripts/ShibuyaLockOnCameraRuntime.cs` | 기존 OrbitCamera 보조와 Fighter의 명시적 근접 타깃 연결 |
| `unity/Assets/Shibuya/Scripts/ShibuyaPlayerInput.cs` | 원본 MMB 입력 전달과 Play Mode 시 공통 설정 연결 |
| `unity/Assets/Shibuya/Scripts/ShibuyaOrbitCamera.cs` | 수동 조작 유예, 기존 시네마틱 처리 다음의 보조 호출 |
| `unity/Assets/Shibuya/Scripts/ShibuyaTargetingRuntime.cs` | 기존 준비 구간 제한 안에서 명시적 타깃 우선 고려 |
| `unity/Assets/Shibuya/Scripts/ShibuyaAerialCombatRuntime.cs` | 공중 이벤트 준비 구간에 같은 명시적 타깃 고려 |
| `unity/Assets/Shibuya/Editor/ShibuyaLockOnInspector.cs` | SerializedProperty 기반 한글 Inspector, Undo 및 다중 편집 |
| `unity/Assets/Shibuya/Editor/ShibuyaPass4ATests.cs` | 신규 실제 Play Mode 기능/경계 검사 |
| `unity/Assets/Shibuya/Editor/ShibuyaPass3CTestRunner.cs` | 출력 루트 분리, 종료 중 데이터 참조 방어, 기본 프레임 계측. PASS 4A에는 타격 콜백 probe를 연결하지 않음 |
| `unity/Assets/Shibuya/Resources/Shibuya Lock On.asset` | 기존 Scene/Prefab 참조 변경 없이 로드할 공통 기본 설정 |

새 C# 및 Asset에 필요한 새 .meta와 Resources 폴더 .meta를 포함한다. 기존 GUID는 변경하지 않는다.

### 선택·유지·상실

- 누르는 동안 화면 중앙 후보 탐색, 놓을 때 확정. 락온 중 짧은 클릭은 해제, 길게 누르면 재선택한다.
- 화면 안, 화면 중앙 허용 반경 안, 실제 3D 월드 거리 50m 이내인 유효 캐릭터만 신규 후보가 된다. 화면 중심 오차가 같은 경우 InstanceID로 결정한다.
- 유지 거리는 60m다. 신규/유지 거리와 공격 사거리는 별개다.
- 환경 Collider만 시야를 차단한다. 다른 파이터의 일시적 가림은 무시한다. 가림 상태 1.5초 이내 복구 가능, 유예 만료/거리 초과/대상 사망·비활성/전투 종료 시 상실한다.
- 상실·수동 해제 뒤 자동으로 다른 상대를 선택하지 않는다.
- `IShibuyaTargetDetection`이 대상 유효성과 조준점/시야를 담당한다. 현재 전투에는 팀 판정 데이터가 없으므로 자신을 제외한 살아 있는 시부야 파이터를 상대 후보로 본다. 향후 팀·특수 탐지는 이 정책에서 확장하며 이번에는 구현하지 않는다.

### 카메라와 공격

- 카메라는 새 소유자가 생기지 않는다. 기존 OrbitCamera의 yaw/pitch만 제한 속도로 보조하고 감도·pitch clamp·환경 SphereCast는 유지한다.
- 탐색 중 보조 없음. 수동 마우스 조작 후 잠시 보조를 쉬므로 주변을 볼 수 있다.
- 시네마틱/복귀 블렌드가 우선이다. 해당 경로가 끝나기 전 보조는 실행되지 않는다. 소유 캐릭터가 바뀐 카메라에 이전 락온을 적용하지 않는다.
- `ComboTarget`과 명시적 락온은 별개다. 타격별 준비 시작 시 유효한 락온을 우선 고려하며, 기존 거리/각도/높이/무적/벽/실제 도달 가능성/방향 입력/회전 예산을 우회하지 않는다.
- authored 이동이 타깃을 소유한 이벤트는 기존 이동 정책을 유지한다. 락온을 위해 새로운 접근 이동이나 추격 예산을 추가하지 않는다.
- 실제 피해는 기존 Collider overlap/Health 경로로만 적용한다. 락온이 없거나 근접 보정에 부적합하면 기존 자동 보정 경로를 보존한다.
- `IShibuyaAimSource.GetAim(emissionOrigin)`은 호출 시점의 대상 충돌체 중심(+데이터 보정)으로 XYZ 방향을 계산한다. 미선택 시 카메라 전방을 반환한다. 루트 pitch, 실제 기술 발사, 투사체 유도·피해를 추가하지 않는다.

## Inspector

Project에서 `Assets/Shibuya/Resources/Shibuya Lock On.asset` 선택. 실제 설정 필드는 한국어이며 SerializedProperty를 통해 Undo/기본 편집 흐름을 유지한다.

| 설정 | 기본값 |
|---|---|
| 신규 / 유지 거리 | 50 / 60m |
| 중앙 후보 반경 | 뷰포트 0.22 |
| 재선택 누름 시간 | 0.25초 |
| 시야 상실 유예 | 1.5초 |
| 카메라 강도 / 수렴 / 최대 각속도 | 0.7 / 5 / 100°/s |
| 수동 카메라 조작 후 유예 | 0.35초 |
| 상실 표시 / 표시 크기 | 0.8초 / 30 |

환경 레이어, 목표점 보정, UI 색/활성화, 디버그 선 활성화/길이도 조절 가능하다. 디버그 선은 저장 기본 OFF다. 거리·시간·보조 튜닝은 Data 소유, 작은 수학 epsilon/입력 상태 처리만 코드 소유다.

## 검증 기록

- 최초 targeted: `targeted-initial/results.xml` — 단일 실행 **15 PASS / 1 FAIL / 0 SKIP**.
- 실패한 `EnvironmentWallBlocksNewSelectionButFightersDoNot`: 벽 뒤 타깃을 가린 뒤에도 벽 앞의 다른 유효 파이터를 테스트에서 남겨뒀다. 그 파이터 선택은 올바른 동작이었다. 첫 단계에서 캐릭터 가림 무시를 확인한 다음 그 보조 파이터를 비활성화하고 환경 차단을 검증하도록 구성을 수정했다. 원본 실패를 보존한다.
- `targeted-recheck/results.xml`: 별도 6건 실행 **5 PASS / 1 FAIL / 0 SKIP**. 앞선 벽 테스트는 통과했고, 추가 벽 모서리 검사에서 이동 시각부터 기다린 것과 실제 가림 감지 시각이 어긋나 실패했다.
- `corner-timing-recheck/results.xml`: **2 PASS / 0 FAIL / 0 SKIP**. 가림 감지부터 계측한 실제 유예는 약 **1.502초**. 감지 프레임의 delta가 약 166ms였다. 유예 설정을 늘리지 않고 감지 시작을 명시적으로 관찰하도록 테스트를 수정했다. 재선택 후보 없음의 상실 피드백도 확인했다.
- 신규 최신 고유 19건은 위 여러 targeted 실행에 분산되어 있으며 단일 19 PASS 실행으로 표현하지 않는다.
- 중단 당시 실행 중이던 단일 190건은 정상 종료됐다. 아래에 원본과 재검증을 분리해 기록한다.

## 재개 검증 최종 기록 — 2026-10-23

**PASS 4A 전체 CODEX VALIDATED 완료를 선언하지 않는다.** 신규 락온의 최신 고유 19개는 PASS지만 기존 회귀 18 FAIL이 남았다. USER VERIFIED 승격은 하지 않는다.

아래 실행 경로의 공통 루트는 `unity/Logs/ShibuyaPass4AQA/20261023/`다.

| 실행 | PASS / FAIL / SKIP | 해석 |
|---|---|---|
| `final-regression/results.xml` | **171 / 19 / 0** | 최초 단일 190건. 미실행 0. 다시 실행하지 않음 |
| `failed-critical-60fps/results.xml` | **0 / 8 / 0** | 실제 미적중·공중 상태·시네마틱 실패 8개 우선 진단. 계측 간섭이 발견된 이력으로 보존 |
| `failed-buffered-60fps/results.xml` | **1 / 18 / 0** | 최초 실패 19개만 재검증. 카메라 유예 검사 1개 PASS |
| 최신 고유 케이스별 전체 190개 | **172 / 18 / 0** | 최초 통과 171개와 실패 19개 재검증 결과를 결합. **단일 전체 실행 결과가 아님** |
| 신규 락온 19개만의 최신 고유 결과 | **19 / 0 / 0** | 최초 18 PASS + 카메라 재검증 1 PASS. 단일 19 PASS 실행이 아님 |
| 기존 회귀 171개 최신 고유 결과 | **153 / 18 / 0** | 기존 135개 120/15, 공중 접촉 18개 16/2, 시네마틱 18개 17/1 |

정확한 전체 테스트 이름, 원본/재실행 메시지·stack 및 케이스별 근거 XML은 `resume/validation-summary.json`에 기록했다. XML 최상위 duration은 Domain Reload를 포함한 전체 벽시계 소요 시간으로 사용하지 않는다.

### 실행 환경과 계측 한계

- 재개 당시 컴파일은 완료 상태였고 기존 보완이 DLL에 반영돼 있었다. HEAD는 `42833ebfec9a176056ae983096c41c1ef4d8fb24`, staged 0, tracked modified 27, untracked 파일 35였다.
- Domain Reload가 약 35~60초 걸렸고 Pipeline 동적 평가·취소 요청이 timeout 또는 reload로 소실됐다. 프로세스를 강제 종료하지 않았다. 진단용 8개도 자연 종료했으며 취소로 누락된 검사는 없다.
- 60fps는 목표 제한이며 실제 안정된 60fps 확보를 뜻하지 않는다. 최초 실행의 보통 프레임은 약 2ms였지만 119~333ms 지연이 섞였다. 재검증 초반 중앙값은 약 16.7ms지만 큰 지연은 계속됐다.
- 추가한 타격 진단 콜백 자체에서 **611.9~633.6ms**를 측정했다. 버퍼 저장으로 바꿔도 비용이 남았으므로 디스크 쓰기만 원인이라고 확정하지 않는다. 이 구간의 실패를 게임 결함으로 확정하지 않았다.
- `failed-buffered-60fps`의 첫 두 검사에 해당 콜백 기록이 남는다. `LocalHitStopDefersRemainingEventAndPreservesBuffer` Play 진입 중 진단 해제·구독 분리가 적용됐다(`resume/probe-disabled.json`). 이 검사는 Pipeline 요청·reload 지연과 겹쳐 180초 timeout이었다. 이후에도 실패가 남았으므로 모든 실패를 추가 계측으로만 설명하지 않는다.
- 과거 승인 QA 소스에는 AutoTick 0ms + 60fps 조합이 있었다. 이번 재검증은 16ms에서 시작했다. 도중 0ms 전환 요청은 timeout됐고 정확한 적용 시점을 독립적으로 확인하지 못했다. 환경 안정화 성공이나 이 차이의 인과관계를 확정하지 않는다.
- 락온 비선택 상태의 기존 회귀와 선택 상태를 포함한 신규 검사를 구분한다. 동일 기존 케이스를 락온 ON/OFF로 완전히 짝지은 대조 검증은 미완료다.
- 테스트 기대값·timeout·피해·넉백·체공 상한을 완화하지 않았고 성공 결과만 모으는 반복은 하지 않았다.

### 남은 실패와 실제 관측

아래 테스트명 앞에는 `JJKGame.Shibuya.EditorTools.`가 붙는다. W/S 7개를 포함한 모든 개별 이름·실패 원문은 위 summary에 있다.

| 검사 | 근거와 남은 한계 |
|---|---|
| `ShibuyaPass22Tests.FrontAndObliqueStandingCombos` | 최초 118.8ms 프레임에 예약이 소비되고 실제 2타까지 실행돼 중간 buffer 관측을 놓쳤음. 재검증은 1타 buffer window timeout, 611.9ms 계측 간섭 포함. 전체 4타 성공을 증명한 것은 아님 |
| `ShibuyaPass22Tests.VictimAlignmentIsBoundedAndLeavesRealActionGaps` | 최초 130.3ms가 0.1초 정렬 구간을 넘었음. 기대 측면 보정 >0.08m, 실제 0m. 재검증도 0m이며 633.6ms 계측 간섭 포함. 승인 정렬 값 유지 |
| `ShibuyaPass3ATests.LocalHitStopDefersRemainingEventAndPreservesBuffer` | 최초 70ms 정지 뒤 157.3ms 프레임으로 clock 진행. 재검증은 180초 timeout. 정지 미적용으로 확정하지도 PASS로 바꾸지도 않음 |
| `ShibuyaPass3B1Tests.FocusSurvivesCameraTurnAndNearbyActor` | 최초 243.5ms 뒤 실제 2타 미적중. 이동된 대상에 잔여 넉백이 적용돼 사거리 이탈. 단순 buffer 관측 실패가 아님. 재검증도 timeout. PASS 4A가 새로 유발했는지는 미확정 |
| `ShibuyaPass3BTests` W/S·기본 마무리 입력 7개 | `FourthSThenWRetainsBufferedDown`, `FourthWThenReleaseRetainsBufferedUppercut`, `NeutralFourClicksKeepVerifiedFinisher`, `RawWCommandIsIndependentOfWorldMoveAndLook`, `SHeldFromFirstClickSelectsDown`, `SimultaneousWSFallsBackToNormal`, `WHeldFromFirstClickSelectsUppercut`. 최초 buffer 관측 실패, 재검증 조건 timeout. 분기 로직 회귀 여부 미확정 |
| `ShibuyaPass3CAirCountTests.ThreeClicksOnHitRequireThreeAcceptedSteps` | 최초와 재검증 timeout. 자동 타격 중복이라고 단정하지 않음. 최초 전체 실행의 별도 `JumpThreeClicks`와 `PursuitThreeClicks`는 통과한 반대 근거 |
| `ShibuyaPass3CHoverTests.FreshBufferedClickReusesSameStepWithoutRenewingHoverBudget` | 기대 <1.04초, 최초 1.11565초, 재검증 **1.98870초**. 관측 지연과 런타임 상한 처리의 기여 미분리 |
| `ShibuyaPass3CHoverTests.HoverToggleOffKeepsSharedAirDataAndAppliesGravity` | 기대 높이 변화 >0.05m, 실제 0m로 재현. 최초 종료 중 QA `ProbeAirHit` NRE 2건은 별도 기록기 결함이며 중력 실패 원인으로 바로 연결하지 않음 |
| `ShibuyaPass3CTests.HitStopBuffersNativeSpaceAndChaseWinsOverJump` | 재검증 True 기대에 False. 현재 환경에서 짧은 정지·Space 예약 구간 확인 실패 |
| `ShibuyaPass3DAirContactTests.ContactHasNoDamageOrAttackAndFallsWithoutOldLaunch` | 재검증 기대 Y 4.36118m, 실제 **1.14311m**. 실제 하강이며 단순 flag 실패가 아님. 요청 대기 0.06초·실제 경과·접촉 유지 0.18초의 비교 필요 |
| `ShibuyaPass3DAirContactTests.LowFrameContactAndCleanup` | 기대 Y 4.29387m, 실제 **1.25093m**. 테스트의 의도된 30fps 설정 유지. 원인 미확정 |
| `ShibuyaPass3DTests.FinisherDamageShotsAndCameraRestore` | 최초 샷 0·2, 재검증 샷 0만 관측해 0·1·2 검사 실패. 관측 누락과 실제 렌더 샷 건너뜀 미분리 |

신규 `ShibuyaPass4ATests.CameraAssistsYawPitchSmoothlyAndRespectsManualSearchAndClamp`는 재검증 PASS다. 최초 333ms 프레임과 유예 만료 가능성은 근거지만, 통과만으로 최초 원인을 확정하지 않는다.

### 정리·복구 및 보존

- 이 재개에서 게임플레이 runtime·Data Asset·기존 테스트 기대값은 변경하지 않았다. 최종 추가 변경은 이 문서와 QA 기록기뿐이다.
- 기록기는 사라질 수 있는 profile 대신 실행 중인 event snapshot을 범위 검사 후 읽는다. 추가한 무거운 계측·임시 메뉴·프레임 제어·락온 강제 선택 도구는 제거했다. PASS 4A에서는 타격 콜백 probe를 연결하지 않는다. 패치와 시도별 소스는 `resume/`에 보존했다.
- 복구: **AutoTick ON / 16ms**, 저장 override 없음, **targetFrameRate -1 / VSync 0**. 재개 당시 남은 임시 0ms로 복구한 것이 아니다. QA cap·seed·root·diagnostic 세션 키도 정리했다. 근거: `environment-restored.json`, `resume/frame-settings-restored.json`.
- `final-editor.json`: Play 종료, 씬 dirty false, dirty Data Asset 0, missing script 0, Time.timeScale 1, ground material/mesh 0/0.
- 피해 20/20/20/25, 넉백 6/4/4/11, 강타 정지 .065초, Player HP 100 / Dummy HP 1000, 어퍼컷 높이 5.5m / apex .12초, 체공 1.2초 / link .12초, 공중 피해 12/13 및 GUID 유지.
- 마지막 복구 audit의 현재 Console은 error 0 / warning 1이다. **전체 작업 중 error 0이라는 뜻은 아니다.** QA NRE 2건, Pipeline timeout과 모든 실패 이력을 삭제하지 않았다.
- 최종 컴파일 오류 0. 실제 DLL에서 임시 메뉴·확장 probe·프레임 제어 제거도 확인했다. 보호 48개, 이전 QA 472개, 최초 실행 증거 221개, 진단 증거 14개, 재검증 증거 9개 해시가 모두 동일하다. 재개 시작의 62개 작업 파일 중 변경은 QA 기록기와 이 문서뿐이다.
- `resume/final-preservation-audit.json`: HEAD/브랜치 유지, staged 0 / tracked modified 27 / untracked 파일 35. `resume/final-console.json`: 컴파일 실패 false, 현재 error 0 / warning 1, 누적 Pipeline history error 71. 누적 이력을 현재 Console 수치와 혼동하지 않는다. `resume/final-pipeline.json`: 기존 PID 17096, 127.0.0.1:7800 연결 가능, 패키지 0.7.0-exp.1 유지.
- staging/commit/push/reset/clean/worktree 및 Computer Use를 실행하지 않았다. 전체 완료·USER VERIFIED 승격·새 기능 구현 없이 종료한다.

현재 남은 과제는 Editor의 긴 reload·프레임 지연을 독립적으로 확인하고 실제 기능 결함과 분리하는 것이다. 기존 USER VERIFIED 기능 승인을 철회하거나 경계 조건까지 확대하지 않는다.

## 사용자 Play Mode 확인

1. 기존 `Assets/Shibuya/Scenes/ShibuyaCombatMVP.unity`를 연다. Play 후 화면 클릭으로 입력을 잡는다.
2. MMB를 누른 채 카메라를 움직여 더미를 화면 중앙에 둔다. 후보 표시 후 놓으면 락온이 확정된다.
3. 짧게 MMB 클릭하면 해제된다. 다시 선택하고 길게 누르는 동안 자유롭게 탐색할 수 있는지 확인한다.
4. 락온 상태에서 마우스로 주변을 둘러보고, 조작을 멈추면 카메라 보조가 부드럽게 복귀하는지 확인한다.
5. 멀리서 LMB를 눌러도 피해가 발생하지 않음을 확인한다. 근접 기본 4타/W 어퍼컷/S 다운, Space 추격과 공중 LMB도 확인한다.
6. `JJK Game → 시부야 PASS3D → 시네마틱 예제 ON (Play Mode 전용)` 후 기본 마무리 유효 적중: 시네마틱과 복귀 중 카메라 충돌이 없는지 확인한다. 같은 경로 OFF 또는 Play 종료로 해제한다.
7. 높이 차이 3D 조준은 Inspector의 디버그 선으로 확인할 수 있다. 이번에는 실제 술식이나 발사체를 연결하지 않았다.

## 제약

UI와 카메라 조작감은 기능 프로토타입이며 사용자 직접 검수 대기다. 다중 대상/50~60m/벽 모서리/사망 경계는 테스트용 객체로 검증하며 기존 1:1 씬에 영구 객체를 추가하지 않는다. 기존 Pitch 제한 밖의 극단적 고도차까지 카메라가 강제로 돌아가지는 않는다. 시야는 카메라에서 대상 조준점까지의 환경 Raycast로 판단하며 신체 전체의 부분 가림을 분석하는 탐지는 아니다. 네트워크·팀 게임·특수 탐지·실제 투사체·다른 PASS 기능은 구현하지 않는다.

PASS 3D는 여전히 별도 승인 체크포인트 준비 상태이며 이번에 커밋하지 않는다. staging/commit/push/merge는 실행하지 않는다.
