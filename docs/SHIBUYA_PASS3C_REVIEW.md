# Shibuya PASS 3C — Space 공중 추격 + 공중 체술 MVP

기준: `3dff1714dd3ed590df3ae7fb199480043288e3d4`, `feat/shibuya-combat-mvp`.
현재 체크포인트 상태: **USER VERIFIED — 아래 명시된 사용자 직접 플레이 범위 한정**.
PASS 3B/3B.1의 기존 승인 범위와 설정은 보존한다.

## 사용자 승인 체크포인트 — 2026-10-19

사용자가 Unity Play Mode에서 다음 기능을 직접 확인하고 현재 동작을 승인했다.

- W+4타 어퍼컷 후 Space 공중 추격.
- 추격 이후 LMB 공중 체술.
- 일반 점프와 추격 후 동일한 공중 무브셋 사용.
- 공중 공격 중 중력에 의한 낙하 없음 및 공격 이후 정상 착지.
- LMB 3회로 2타 → 2타 → 1타, 총 5타가 가능한 현재 공중 연계 동작.

현재 데이터는 서로 다른 공중 단계 3개를 별도 작성한 배열이 아니라, 같은 2타 단계를 유효한 새 클릭으로 연계하고 총 체공 상한에서 마지막 단계가 종료되는 구조다. 사용자는 이 현재 동작을 승인했다. 적중 자체로 타격 횟수가 자동 증가하는 버그는 진단에서 재현되지 않았다.

자동 검증 범위는 **CODEX VALIDATED**로 구분한다. 기존 126개 고유 케이스의 최신 PASS는 네 실행의 케이스별 결과이며, 별도 진단은 단일 실행 9 PASS / 0 FAIL이다. 서로 다른 실행을 단일 135개 실행으로 표현하거나 중복 합산하지 않는다. 원본 XML과 최초 실패 이력은 로컬 QA 폴더에 보존하며 대용량 Logs는 커밋하지 않는다.

극단적인 충돌·입력·무적·저프레임 경계까지 사용자가 직접 검수했다고 확대하지 않는다. 기존 벽 검사 최초 실패의 정확한 원인 미확정, 수평 중심 공중 이동 작성 기능, Player 빌드·네트워크 미검증은 남는 한계다.

최종 체공 정책과 타격 계측은 [체공 개선 보고서](SHIBUYA_PASS3C_HOVER_REVIEW.md), [공중 횟수 진단 보고서](SHIBUYA_PASS3C_AIR_ATTACK_COUNT_REVIEW.md)를 참조한다. **아래 본문은 최초 구현 당시의 이력**이다. 초기의 1회 공중 공격 제한·공격 중 자연 낙하·승인 대기·commit/push 미실행 문구는 당시 기록이며, 현재 정책과 승인 상태는 위 체크포인트 및 후속 보고서가 우선한다.

## 상태·입력·대상 소유권

- `ShibuyaAerialCombatRuntime`은 기존 `ShibuyaFighter`의 partial이다. 새 이동 컴포넌트나 별도 공중 물리 엔진을 만들지 않았다.
- 공격자 `AirChasing / AirReady / AirFalling`과 공중 공격 모드를 피격자의 `Airborne`과 분리한다. 추격 중인 공격자를 공중 피격 상태로 만들어 행동 불능 처리하지 않는다.
- 실제 W 어퍼컷의 `DamageResolution.Applied`와 Launch 적용 후에만 추격 기회를 만든다. 일반 4타/S/빗나감/무적/명중 전 Space는 기회를 만들거나 선예약하지 않는다.
- 대상의 발사 버전과 피해 버전을 기록한다. 착지/사망/비활성화/무적/다른 공격자 개입이면 참조와 진행 중인 공중 공격을 해제한다. 자기 공중 공격의 실제 적중만 피해 버전을 갱신하며 다른 적으로 자동 재선정하지 않는다.
- Space를 실제로 누를 때 한 번 예약하고 국소 히트스톱 뒤 소비한다. 입력 허용·버퍼 시계는 Fighter의 로컬 전투 시계라 히트스톱 때문에 입력이 사라지지 않는다. 유효 추격이 없으면 기존 점프 요청 경로를 사용한다. 기존처럼 공격 중·피격 중 점프는 허용하지 않는다.
- 하나의 발사에서 추격 1회, 공중 공격 입력 1회만 허용한다. 공중 공격은 일반 피격 결과만 허용하고 새 추격 기회를 만들지 않는다. 무제한 후속 입력 버퍼나 추가 체공 구속을 만들지 않았다.

## 실제 3D 이동과 충돌

상대 현재 월드 위치와 제한된 현재 속도 예측으로 3D 목적지를 계산한다. 목적지까지 남은 거리에 따라 감속하고, 시간·총 이동 거리·대상 거리·회전 한도를 적용한다. 이동 요청은 기존 Fighter의 **단일 `CharacterController.Move` 경로**에 합산한다. 런타임에서 `transform.position` 대입, 순간이동, 다른 mover, 전역 Time.timeScale 변경을 하지 않는다.

최소 도착 간격은 실제 캡슐 반경·skin보다 작아질 수 없다. 처음부터 높이가 비슷한 지상 순간을 도착으로 오인하지 않도록, 대상이 발사 위치보다 충분히 상승하고 공격자가 실제로 지면을 떠난 이후에만 도착을 인정한다. 실제 높이 차도 별도로 검사한다.

입력 전 캡슐 경로 검사와 이동 중 CC 충돌·정체 검사를 사용한다. 벽/천장/거리/시간/대상 이탈 시 현재 위치에서 자연 낙하한다. 자동 우회 경로 탐색이나 원위치 순간 복귀는 없다. 추격 후 낙하도 기존 착지 캡슐 분리 경로를 사용하고, 다른 캐릭터 머리를 월드 지면으로 취급하지 않는다.

피격 중단 시 추격 참조·공격을 없애되 공중에 있었다면 낙하 정리 상태는 실제 지면 접촉까지 유지한다. 착지만으로 남아 있는 피격 경직을 지우지 않는다. 사망/전투 종료/비활성화는 상태를 정리하며 기존 전투 종료 정책을 따른다.

## 공중 체술 데이터와 판정

- 새 `ShibuyaAerialCombatDefinition`이 추격과 공중 1단계만 소유한다. 기본 Combat Profile의 `aerialCombat`을 사용하며, 캐릭터 Moveset의 `aerialCombat`이 있으면 해당 데이터를 우선한다. 기존 지상 타격 데이터와 기본 마무리 자산을 복사·이관하지 않는다.
- 도착은 피해를 주지 않는다. 도착 뒤 **새 LMB 입력**이 있어야 실행한다. 버튼 유지로 반복하지 않으며, 추격 도중 LMB를 미리 누른 것은 예약하지 않는다.
- 기존 `ShibuyaMovesetStep`의 이벤트·이동 배열, 실제 활성 구간 판정, 이벤트별 Health 중복 방지, 준비/활성/회복, 이동 도착 조건을 재사용한다. 공중 전용 피해 엔진을 별도로 만들지 않았다.
- 실제 공격 구·상대 높이 차·장애물을 검사한다. 적이 멀어지면 빗나가며 적을 판정 안으로 이동시키지 않는다. 준비 중 제한된 방향 조준만 허용하고 기존 지상 추적 전진을 중복 적용하지 않는다.
- 기본은 한 입력 안의 **12 + 13 피해** 두 이벤트다. 시작 시점 0/0.16초, 각각 준비 0.08 / 활성 0.08 / 회복 0.14초, 경직 0.16초, 수평 넉백 속도 1.2, 국소 히트스톱 0.035초다. 현재 CharacterStatsProfile 공격·방어 배율은 이전처럼 피해 계산에 적용되지 않는다.
- 도착 때 상대의 수직 속도를 이어받고 기존 중력으로 낙하한다. 피격자의 기존 발사 높이·중력·정점 체공을 늘리거나 공중 타격으로 다시 띄우지 않는다. 공격하지 않아도, 공격이 끝나도 낙하·지상 제어로 돌아간다.

## 한국어 Inspector 초기값

새 자산: `Assets/Shibuya/Data/Shibuya Aerial Combat.asset`.
SerializedProperty/CustomEditor를 사용하며 Undo·배열·오브젝트 참조 편집을 유지한다.

| 설정 | 초기값 |
|---|---|
| 공중 추격 사용 | 켜짐 (실제 적중+Space 필요) |
| Space 허용 시간 / 입력 버퍼 | 0.70 / 0.20초 |
| 대상 최대 거리 / 누적 이동 상한 / 추격 시간 | 9m / 9m / 0.80초 |
| 최고 속도 / 접근 감속도 | 24m/s / 65m/s² |
| 도착 간격 / 추가 허용 오차 / 높이 오차 | 1.25m / 0.12m / 0.45m |
| 공중 도착에 필요한 실제 상승 높이 | 2.5m (원래 발사 높이는 변경 안 함) |
| 예측 시간 / 예측 이동 상한 | 0.05초 / 0.60m |
| 회전 속도 / 누적 회전 상한 | 720도/초 / 100도 |
| 충돌 정체 종료 시간 | 0.08초 |
| 추격 후 착지 캡슐 분리 속도 | 8m/s |
| 실패 직후 낙하 속도 상한 / 낙하 이동 입력 배율 | 6m/s / 0.25 |
| 도착 후 공중 체술 입력 시간 / 타격 높이 차 | 0.55초 / 1.1m |
| 임시 추격 자세 기울기 | 18도 |

타격별 피해·판정·준비/활성/회복·경직·넉백·히트스톱·모션과 이동 클립도 기존 단계 Inspector에서 수정한다. 추격 실패 정책은 MVP에서 현재 위치 자연 낙하로 고정하며 속도·이동 입력을 위 데이터로 조절한다. 낮은 발사 높이의 새 캐릭터를 작성할 때는 도착 최소 상승 높이도 함께 조절해야 한다. 배열 구조 편집은 Play Mode 밖에서 한다.

## 검증 기록

증거 루트: `unity/Logs/ShibuyaPass3CQA/`. 실제 Unity Test Framework EditMode 테스트 안에서 EnterPlayMode/ExitPlayMode로 실제 런타임을 실행했다. process exit 0이나 내부 상태 flag만으로 PASS 처리하지 않는다.

| 실행 | 결과 | 의미 |
|---|---|---|
| `focused-initial-6` | 3 PASS / 3 FAIL | 첫 구현의 너무 이른 도착 판정 발견 |
| `focused-fixed-6` | 6 PASS / 0 FAIL | 실제 상승 높이 조건으로 도착 결함 수정 |
| `new-initial-29` | 27 PASS / 2 FAIL | 피격 경직과 두 발사 사이의 재현 조건 확인 |
| `recheck-fixed-6` | 6 PASS / 0 FAIL | 실패 2건 및 관련 입력/적중/데이터 재검증 |
| `regression-initial-83` | 74 PASS / 9 FAIL | 기존 체크포인트의 정확한 83개 이름을 사용한 단일 실행 |
| `diagnostic-throttled-9` | 0 PASS / 9 FAIL | 기존 검사 그대로 프레임·상태 기록 추가, 긴 Editor 프레임 재현 |
| `control-default16-2` | 2 PASS / 0 FAIL | 재시작된 Editor의 기본 auto-tick 16ms에서 정렬/일반 콤보 대조 검사 |
| `recheck-autotick0-9` | 9 PASS / 0 FAIL | QA 동안만 auto-tick 0ms, 실패했던 기존 검사 원본 전체 통과 |

**최신 고유 케이스 집계: 기존 83 + 신규 29 = 112 PASS / 0 FAIL / 0 SKIP.** 단일 112개 실행 결과가 아니다. 각 실행의 discovery·execution.jsonl·XML 이름/개수/결과를 대조했으며, 원래 83개 체크포인트와 현재 83개 집합이 정확히 일치한다. 근거: `latest-per-case-112.json`의 케이스별 실행 경로와 원본 XML. 최초 실패 결과도 모두 별도 폴더에 보존했다.

신규 29개 케이스의 최신 결과는 모두 PASS다. 최초 29개 단일 실행을 29 PASS라고 표현하지 않는다. 피격 중단 검사는 착지해도 남은 경직 동안 공격 거절을 확인한 뒤 경직 종료 후 재개를 검증한다. 시간/거리 상한의 두 번째 발사는 첫 대상이 실제 착지·회복한 뒤 시작하도록 분리했다. 추격 제한이나 충돌 assertion을 약화·삭제하거나 SKIP 처리하지 않았다.

관측:
- 실제 도착에서 플레이어 Y≈4.6m, 대상 Y≈4.8m, 수평 간격 약 1.38m, 이동 요청 약 4.84m. 도착 시 기존 카메라에서 대상 viewport도 확인했다.
- LMB 한 번에 12+13 피해, 다중 Collider 중복 피해 없음. 도착만으로 추가 피해 없음.
- 공중 히트스톱 때 공격자/피격자 위치·로컬 시계는 멈추고 제3자는 이동하며 Time.timeScale=1이다.
- 0.35초 이벤트 시계를 건너뛴 실제 UpdateAttack 호출에서도 0→1 순서로 각각 1회 적중했다. 이는 지속적인 렌더 저FPS 성능 벤치마크는 아니다.
- 다른 캡슐 장애물 사례의 최소 중심선 간격 약 0.836m였고, 무한 추격이나 추가 피해 없이 지면으로 복귀했다.

### 기존 9건 실패와 Editor 실행 환경

실패 분류는 정렬 1건, 히트스톱 1건, 동일한 `CommandScenario`의 W/S·버퍼 검사 7건이다. 기존 테스트 코드와 assertion은 수정하지 않았다. 신규 runner의 읽기 전용 프레임 기록으로 확인했다.

- 정렬 검사에서는 약 199ms 프레임이 기존 100ms 보정 시간을 지나가며 정렬이 종료됐다. 진단 실행의 관측 후 잔여 보정량은 0이었다.
- 히트스톱 검사는 70ms 정지 중 25ms를 기다린 뒤 시계가 같다고 검사하지만, 약 200ms 프레임에서는 검사 재개 전에 실제 정지가 끝나 시계가 진행했다.
- W/S 7건의 실패 위치는 모두 `ShibuyaPass3BTests.cs:85`였다. 기록에서 1단계가 약 182ms 프레임 뒤 2단계로 이미 진행했고, 예약 버퍼가 정상 소비된 뒤 `HasBufferedAttack`을 검사했다. 입력 유실과 같은 상황으로 해석하지 않았다.

중단 전 `set_autotick` 적용 요청은 timeout됐고 상태 백업 파일도 생성되지 않았다. 재개 시 이전 Editor는 종료되어 있었고 새 Editor가 열려 있었다. 새 세션에서 실제 값은 enabled=true / interval=16ms / 저장된 SessionState 선택 없음이었다. 미저장 씬과 실행 중 테스트는 없었다.

중요한 반증: **재시작된 Editor에서는 기본 16ms에서도 대표 두 검사가 통과**했다. 따라서 16ms 설정 자체나 auto-tick 비활성화를 실패의 유일한 원인이라고 단정하지 않는다. 확인된 직접 요인은 긴 Editor 프레임으로 짧은 상태·관측 창을 건너뛴 것이며, 그 지연을 만든 OS/Editor 내부 원인까지 확정하지 않았다.

기존 Pipeline 패키지의 auto-tick을 QA 동안만 0ms로 사용해 같은 9개 검사를 재실행했다. 공격 구간 최대 프레임은 약 13–30ms였고 9개 모두 PASS였다. 게임 런타임·Data Asset·기존 테스트 31개 파일의 SHA-256이 대조 실행 전후 동일하다. 이를 통과시키기 위한 런타임 변경, 시간/허용치 완화, assertion 삭제, SKIP은 하지 않았다. 이 결과를 실제 게임의 200ms 지연에서도 모든 지상 보정이 동일함을 증명한 것으로 확대하지 않는다.

검사 후 **켜짐 / 16ms / 별도 저장 선택 없음**으로 복구하고 원래 세션 값과 일치함을 검증했다. `session-original.json`, `autotick-before.json`, `session-restored.json`이 근거다. Packages/ProjectSettings/EditorPrefs를 변경하지 않았다.

### 최종 컴파일·Console·정리 상태

- 마지막 Unity audit: `compiling=false`, `compilationFailed=false`, 현재 Console **오류 0 / 경고 1**. 경고는 `Deleting invalid font reference`이며 새 세션 Pipeline 누적 기록에는 경고 11건이 남아 있다. Console을 수동으로 지우지 않았다.
- 이전 Editor 세션에는 Pipeline의 `Main thread operation timed out after 5000ms` 오류가 실제 발생했다. 이전 `ShibuyaPass3B1-Editor.log`와 실패 QA 결과를 보존하며, 전체 작업 기간에 오류가 전혀 없었다고 표현하지 않는다. 패키지 재설치나 MCP 재설정은 하지 않았다.
- Play Mode 종료, 씬 dirty=false, 미저장 persistent ScriptableObject 0, 열린 씬 missing script 0, Time.timeScale=1.
- 기존 바닥 균열 소유 mesh/material 잔존 **0/0**. 공중 추격은 별도 mesh/material을 생성하지 않는다. 이전 Editor 종료 로그의 전역 `JobTempAlloc` 경고는 이 소유 리소스 검사와 별개이며, 3C 원인이나 전역 native leak 없음으로 단정하지 않는다.
- 피해 20/20/20/25, 준비 0.10/0.09/0.10/0.18초, 활성 0.08/0.07/0.07/0.10초, 회복 0.22/0.18/0.18/0.32초, 넉백 6/4/4/11, 기존 4타 히트스톱 0.065초 유지.
- Player HP 100 / Dummy HP 1000, 어퍼컷 목표 높이 5.5m / 정점 체공 0.12초 유지. 새 공중 데이터 유효, 피해 12+13, 실제 참조 GUID 정상.

세부 결과: `final-audit.json`, `final-console-errors.json`, `final-console-warnings.json`, `protected-final-audit.json`.

## 보호 범위

시작 시 tracked 1,046개 SHA-256을 `before.json`에 기록했다. 최종 검사에서 예상한 Shibuya 파일 11개만 변경됐으며 기존 Profile YAML은 공중 설정 참조 한 줄 이외에 동일하다. 원래 Finishers/Stats/Scene/Prefab/카메라/Gojo/VFX/Packages/ProjectSettings/잠금 문서/기존 `.meta`는 보존했다. 기존 작은 QA 기록도 `pre-regression-evidence/`에 복사했다. HEAD는 기준 `3dff1714dd3ed590df3ae7fb199480043288e3d4` 그대로이고 staged 0이다.

## 직접 Play Mode 확인

1. 기존 `Assets/Shibuya/Scenes/ShibuyaCombatMVP.unity`를 Play한다. 다른 씬이나 예제 메뉴는 필요 없다.
2. W를 누르고 좌클릭 4회로 어퍼컷을 적중시킨다. 적이 떠오른 것을 확인하고 0.7초 안에 Space를 새로 누른다. 명중 전 Space는 추격으로 저장되지 않는다.
3. 공격자가 공중의 적 옆으로 접근한 뒤 LMB를 **다시 클릭**한다. 한 번의 클릭에 두 번 타격하며 누른 채로 반복하지 않는다. 도착 후 입력 시간은 기본 0.55초다.
4. 추격만 하고 LMB를 누르지 않아도 자연스럽게 떨어지는지, 공격 뒤 다시 지상 이동·달리기·점프·4타가 되는지 확인한다.
5. 일반 4타/S 다운에서는 Space 추격이 생기지 않는지 확인한다. S의 원래 바닥 균열도 그대로다.
6. `Shibuya Aerial Combat.asset`에서 공중 데이터만 조절한다. 기본 피해 20/20/20/25, 원래 어퍼컷 높이/착지, HP 자산은 초기화하지 않는다.

## 변경 파일 (총 24개)

기존 파일 수정 11개:
- `unity/Assets/Shibuya/Data/Shibuya Combat Profile.asset` — 새 공중 설정 참조 한 줄.
- `unity/Assets/Shibuya/Editor/ShibuyaInspectors.cs` — 새 설정 한국어 표시.
- `unity/Assets/Shibuya/Editor/ShibuyaMovesetInspector.cs` — 공중 설정 CustomEditor.
- `unity/Assets/Shibuya/Scripts/ShibuyaBlockVisual.cs` — 최소 추격 자세 및 낙하 상태 표시.
- `unity/Assets/Shibuya/Scripts/ShibuyaCombatProfile.cs` — 공용 공중 설정 참조.
- `unity/Assets/Shibuya/Scripts/ShibuyaFighter.cs` — 입력·로컬 시계·단일 이동·판정 통합.
- `unity/Assets/Shibuya/Scripts/ShibuyaHitStateRuntime.cs` — 실제 발사 버전, 추격 중 수직 이동 및 착지 분리.
- `unity/Assets/Shibuya/Scripts/ShibuyaMovesetDefinition.cs` — 캐릭터별 공중 설정 선택 참조.
- `unity/Assets/Shibuya/Scripts/ShibuyaMovesetRuntime.cs` — 기존 이벤트 초기화 재사용 및 유효 Launch 후 추격 기회.
- `unity/Assets/Shibuya/Scripts/ShibuyaPlayerInput.cs` — 새 Space 누름과 버튼 유지 구분.
- `unity/Assets/Shibuya/Scripts/ShibuyaTargetingRuntime.cs` — 공중 세션에 지상 접근 중복 적용 방지.

신규 파일 13개:
- `unity/Assets/Shibuya/Scripts/ShibuyaAerialCombatDefinition.cs` + `.meta`.
- `unity/Assets/Shibuya/Scripts/ShibuyaAerialCombatRuntime.cs` + `.meta`.
- `unity/Assets/Shibuya/Editor/ShibuyaAerialDataBuilder.cs` + `.meta`.
- `unity/Assets/Shibuya/Editor/ShibuyaPass3CTests.cs` + `.meta`.
- `unity/Assets/Shibuya/Editor/ShibuyaPass3CTestRunner.cs` + `.meta`.
- `unity/Assets/Shibuya/Data/Shibuya Aerial Combat.asset` + `.meta`.
- `docs/SHIBUYA_PASS3C_REVIEW.md`.

QA 로그·XML·측정 파일은 무시되는 `unity/Logs/ShibuyaPass3CQA/`에 저장한다. 기존 씬·프리팹이나 사용자 데이터 파일을 재생성하지 않았다.

## 한계와 후속 확장 지점

공중 1단계 MVP이며 도착 전 LMB 버퍼, 공중 4타, 전용 캐릭터 애니메이션, 자동 장애물 우회, 긴급 탈출, 시네마틱, 멀티플레이는 없다. 다른 공격자의 유효 피해 개입은 원래 추격 문맥을 취소하는 보수적인 규칙이며, 추후 다인전의 소유권 정책은 별도 설계한다.

카메라는 수정하지 않았다. 도착 시점 시야는 검사했지만 전체 원속도 조작감·임시 블록 모션·모든 카메라 각도의 체감은 사용자 검수 대상이다. Player 빌드·네트워크·극단적인 밀집 지형의 모든 경계를 검증했다고 주장하지 않는다. 다음 단계는 이 단일 공중 단계 데이터와 상태를 재사용할 수 있지만 PASS 3D나 새 기능은 별도 승인 전 구현하지 않는다.

staging / commit / push / 브랜치 전환 / worktree 생성은 하지 않는다.
