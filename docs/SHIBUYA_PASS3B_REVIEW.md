# Shibuya PASS 3B — 지상 마무리 분기

기준: `7e0e4146de9ee1720f435bc84c2af7751e01876b`, `feat/shibuya-combat-mvp`.
상태: **USER VERIFIED — 사용자가 직접 승인한 핵심 조작·전투 흐름 한정 / 경계 조건은 CODEX VALIDATED**.

## 사용자 승인 체크포인트 — 2026-10-19

사용자가 PASS 3B와 3B.1을 Unity Play Mode에서 직접 플레이하고 최종 승인했다. 승인 범위는 기존 일반 4타, W+4타 어퍼컷, S+4타 다운, 개선된 어퍼컷 높이·착지, 대상 추적·유도, 일시적 바닥 균열, 전체 조작감과 전투 흐름이다.

직접 검수하지 않은 극단적 충돌·경계·무적·저프레임 등의 세부 조건은 USER VERIFIED로 확대하지 않는다. 자동 검증으로 확인한 범위는 CODEX VALIDATED로 구분한다. 최종 구현·설정·83개 고유 케이스의 복수 실행 근거는 [PASS 3B.1 보고서](SHIBUYA_PASS3B1_REVIEW.md)를 따른다. 아래 초기 데이터·측정·62개 단일 실행 기록은 최초 PASS 3B 당시의 이력이며 현재 튜닝값과 혼동하지 않는다.

## 구조와 데이터 소유권

- 1~3타와 방향 없는 기본 4타는 기존 Combat Profile의 타격 데이터를 그대로 실행한다. 기본 피해 20/20/20/25, 기존 타이밍/전진/추적/정렬/넉백/피격/히트스톱은 보존한다.
- `ShibuyaFinisherSet`은 기존 `ShibuyaMovesetStep`을 재사용해 어퍼컷/다운의 대체 마지막 단계만 가진다. 별도 공격 엔진이나 상태별 CharacterController를 만들지 않는다.
- 기본 Combat Profile의 새 `finishers` 참조는 `Assets/Shibuya/Data/Shibuya Ground Finishers.asset`을 가리킨다. 캐릭터별 `ShibuyaMovesetDefinition`도 자체 마무리 자산과 `finisherStepIndex`를 선택할 수 있다. 기존 PASS3A 예제에는 분기가 연결되지 않는다.
- `ShibuyaPlayerInput`은 실제 W/S 키를 이동 축과 별도로 읽는다. 네 번째 클릭 접수 시 선택을 저장하며, 버퍼 소비 때 다시 키나 카메라 방향을 읽지 않는다. W+S 또는 둘 다 없으면 기본 마무리다. 클릭 유지로 자동 연계하지 않는다.
- 일반적인 점프 중 새 지상 분기를 선택하지 않는다. 기존 기본 공격 경로는 유지한다.
- 확정 연계는 저장된 마무리의 실제 준비 시간을 사용하며 기존 0.75초 연결 상한과 3초 시퀀스 상한을 변경하지 않았다.

## 실제 피격 상태

- 공통 타격 이벤트에 `Normal / Launch / Knockdown` 결과와 공중/다운 설정을 추가했다. 정상 피해 판정이 적용된 경우에만 상태를 적용한다.
- 어퍼컷은 실제 상승 속도·중력·정점 체공으로 움직인다. CC 천장 충돌은 상승을 중단하고, 실제 바닥 접촉은 공중 상태를 해제한다. 떠 있는 동안 기본 이동/점프/공격은 제한된다. 공격자만 시각적으로 팔을 올리는 효과가 아니다.
- 다운은 실제 착지 후 최소 다운 시간, 자연 기상 시작, 기상 동작을 구분한다. 공중 대상은 낙하 속도로 내려오며 지면으로 순간이동하지 않는다. 추가 다운 적중은 이미 진행 중인 다운 타이머를 재시작하지 않는다.
- `CanRequestDownRecovery`는 향후 최소 경직 이후 탈출의 상태 조회 지점이다. 이번에는 WASD+Space 탈출 입력/무적/탈출 이동을 구현하지 않았다.
- 사망/전투 종료/비활성화는 공중·다운·기상 상태를 정리한다. 종료된 전투의 배우는 기존 종료 정책대로 멈추며, 새 전투는 Scene 재시작으로 초기화한다.
- 모든 이동은 기존 Fighter의 단일 CC 경로를 사용한다. 국소 히트스톱은 로컬 전투 시계·상태·중력 진행을 함께 멈추며 전역 Time.timeScale을 변경하지 않는다.

## Inspector와 초기 데이터

한국어 SerializedProperty Inspector에서 다음을 조절한다.
- Combat Profile: **방향별 마무리 설정** 참조. 기존 기본 4타 설정은 그대로다.
- 마무리 자산: **W 어퍼컷 마무리 / S 다운 마무리**, 타격 이벤트/실제 이동 클립 배열.
- 각 이벤트: 피해, 준비/활성/회복, 판정, 경직, 넉백, 히트스톱, 모션, 타격 연출 강도, 타격음 높낮이, **유효 적중 후 상태**.
- **공중 상태 설정**: 상승 속도, 공중 중력, 정점 체공, 공중 피격 외형 기울기.
- **다운 상태 설정**: 낙하 속도, 최소 다운 유지, 자연 기상 시작, 기상 동작 시간, 외형 기울기.
- 커스텀 무브셋: 마무리 참조 및 방향 마무리 입력 단계(0부터).

초기 두 분기는 피해 25, 준비/활성/회복 0.18/0.10/0.32초, 국소 히트스톱 0.065초다. 상승 속도 11, 공중 중력 25, 정점 체공 0.06초다. 다운은 최소 0.45초, 자연 기상 0.85초, 기상 동작 0.22초다. 이 값들은 새 자산의 초기값이며 사용자가 Inspector에서 변경할 수 있다.

기존 Combat Profile을 저장하며 PASS3A에서 이미 사용하던 연결 설정 1/0.24/0.75/3도 명시적으로 직렬화됐다. 기존 설정값의 변경이 아니며, 기존 필드에는 삭제/대체가 없다. Dummy HP 1000, Player HP 100은 유지한다.

## 자동 검증

실제 Unity Test Framework discovery/execution/XML을 `unity/Logs/ShibuyaPass3BQA`에 기록했다. exit code만으로 PASS 처리하지 않았다.

- 사전 focused: 신규 18 PASS / 0 FAIL, 추가 데이터 교체·취소 2 PASS / 0 FAIL. 각각 `targeted-initial-18/`, `targeted-extra-2/`에 보존했다.
- **최종 단일 실행: 62 PASS / 0 FAIL / 0 SKIP**. 기존 PASS1~3A 42개와 PASS3B 20개를 한 번에 실행했다. discovery, execution JSONL, XML의 실제 고유 케이스가 모두 62개다. 사전 focused 20개를 더해 82개라고 세지 않는다. 실패한 테스트나 테스트 실패 후 재실행은 없었다.
- 최종 증거: `results.xml`, `discovery.json`, `execution.jsonl`, `metrics.txt`. 기존 QA가 덮어쓰는 작은 증거는 실행 전에 `pre-regression-evidence/`에 복사했다.
- 기본/어퍼컷/다운 모두 정상 4입력 적중에서 총 피해 85. W/S를 1타부터 유지하거나 4타만 입력해도 선택되며, 버퍼 예약 이후 키 변경은 선택을 바꾸지 않는다. W+S는 기본 마무리다.
- 장애물 없는 상승 최고 높이 약 **2.40m**, 천장 아래 약 **0.99m**, 벽 앞 도달 Z 약 **1.02m**. 상승 후 실제 CC 착지·행동 복귀를 확인했다.
- 다운은 로컬 피격 시간 기준 약 **0.85초**에 기상 시작, 약 **1.08초**에 복귀했다. 추가 다운 타격이 기상 시점을 재시작하지 않았다. 공중 다운은 낙하·바닥 충돌 후부터 최소 다운 시간을 적용했다.
- 별도 캐릭터 무브셋에서 마무리 피해 **31**, 준비 **0.16초**, 상승 속도 **7**을 데이터만 바꿔 실행했고, 원래 기본 자산은 유지됐다.
- 최종 `final-audit.json`: Play Mode=false, Scene dirty=false, dirty persistent asset 목록 비어 있음, 누락 스크립트 0, Time.timeScale=1. Dummy HP 1000/Player HP 100 및 기존 PASS3A 예제 유효성 확인.
- 최종 `final-console.json`: 컴파일 실패 false, 현재 Console error **0**, warning **1**. 기존 `Deleting invalid font reference.` 경고가 남아 있다. Pipeline 세션 이력의 오류 2건은 이번 작업 이전 PASS3A TestRunner를 Play Mode/미저장 상태에서 실행한 보호 예외이며, PASS3B 런타임 오류로 분류하지 않았다. 이력은 지우지 않았다.
- 마지막 읽기 전용 상태 조회 1회가 Pipeline의 5000ms 메인 스레드 제한으로 시간 초과했으며, 동일 조회 재시도는 성공했다. 이는 테스트 실패나 데이터 복구 작업이 아니다.

신규 focused 실행은 `ShibuyaPass3BTestRunner.Run()` 또는 **JJK Game → 시부야 PASS3B → PASS3B 신규 자동 검증**이다. 기존 PASS1~3A까지 포함하려면 다음 C#을 Pipeline `eval_file`로 실행한다.

```csharp
JJKGame.Shibuya.EditorTools.ShibuyaPass3BTestRunner.RunFiltered("ShibuyaPass");
return true;
```

Scene을 저장하고 Play Mode를 종료한 상태에서 실행한다. 기존 QA 기록을 보존한 후 재실행한다. QA Logs·이미지·캐시·임시 snippet은 Git 대상이 아니다.

## 사용자 직접 검수 방법

1. 기존 `Assets/Shibuya/Scenes/ShibuyaCombatMVP.unity`를 열어 Play한다. 별도 Scene 선택이나 예제 메뉴가 필요 없다.
2. W/S 없이 좌클릭 4회: 기존 수평 강타. 정상 사거리에서 W 없이 자동 전진하며 연결되는지 확인한다.
3. 처음부터 W를 유지하고 4회 클릭, 또는 네 번째 클릭에만 W: 어퍼컷과 실제 상승·낙하·착지.
4. S를 유지하거나 네 번째 클릭에만 S: 내려치는 공격과 다운·기상.
5. 세 번째 공격 중 다음 클릭을 예약한 직후 W/S를 놓거나 반대로 바꾼다. 이미 선택한 마무리가 유지돼야 한다.
6. W와 S를 동시에 누르면 기본 수평 마무리여야 한다. 타깃이 없으면 혼자 공격할 뿐 대상의 공중/다운 상태가 생기지 않는다.
7. `Shibuya Ground Finishers.asset`에서 새 두 분기만 튜닝한다. 기존 4타/HP 자산을 임의로 초기화하지 않는다. 배열 구조 편집은 Play Mode 밖에서 한다.

## 다음 단계와 한계

- 핵심 조작·전투 흐름은 위 사용자 승인 범위에서 USER VERIFIED다. 사용자 검수 밖의 모든 모션·경계 상황까지 승인됐다고 해석하지 않는다.
- 공중 추격/공중 체술/다운 탈출/회피/방어/술식/순간이동/시네마틱/멀티플레이는 구현하지 않았다.
- PASS3C는 공중 상태·로컬 시간·마무리 결과를 재사용할 수 있다. Space 추격 입력, 추격 도착 판정, 공중 공격 정책과 카메라는 별도 설계/승인한다.
- 기존 CharacterStatsProfile 공격/방어 배율은 여전히 피해 계산에 적용하지 않는다.

## 변경 파일

기존 9개 수정:
- `unity/Assets/Shibuya/Scripts/ShibuyaFighter.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaPlayerInput.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaMovesetDefinition.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaMovesetRuntime.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaCombatProfile.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaBlockVisual.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaInspectors.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaMovesetInspector.cs`
- `unity/Assets/Shibuya/Data/Shibuya Combat Profile.asset`

신규 (Unity 파일 6개와 각각의 `.meta`, 보고서 1개):
- `unity/Assets/Shibuya/Scripts/ShibuyaFinisherSet.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaHitStateRuntime.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaFinisherDataBuilder.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass3BTests.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass3BTestRunner.cs`
- `unity/Assets/Shibuya/Data/Shibuya Ground Finishers.asset`
- `docs/SHIBUYA_PASS3B_REVIEW.md`

최초 PASS 3B 시작 시점의 기존 파일 1,024개를 SHA-256으로 대조한 결과, 당시 위 9개 이외의 기존 파일 변경·누락은 없었다. 당시 기본 프로필은 새 필드 5개를 제외한 원래 모든 행이 일치하며 기존 수치는 변경하지 않았다. 기존 Scene/Prefab/Gojo/VFX/잠금 문서/Packages/ProjectSettings와 기존 `.meta` GUID를 보존했다. 당시 구현 단계에서는 staging/commit/push를 하지 않았으며, 이번에는 사용자의 별도 승인으로 PASS 3B/3B.1만 현재 브랜치에 체크포인트로 저장한다.
