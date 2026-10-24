# 시부야 공중 추격 접촉·피격 이동 갱신

2026-10-22 / `feat/shibuya-combat-mvp` / 시작 HEAD `42833ebfec9a176056ae983096c41c1ef4d8fb24`.

사용자가 승인한 새 공중 전투 규칙의 구현이다. 기존 PASS 3D 미커밋 구현과 실패 XML을 보존한다. **LOCAL USER VERIFIED — 기능 범위 한정 / CODEX VALIDATED / 체크포인트 커밋 전**.

## 사용자 기능 승인 — 2026-10-22

사용자는 ShibuyaCombatMVP를 직접 플레이하여 W 어퍼컷 → Space 추격 후 무피해 접촉·자연 낙하, 추격 후 공중 평타 3클릭 연계, PASS 3D 시네마틱 발동·카메라 복귀를 승인했다.

캐릭터별 세부 모션과 시네마틱 카메라 구도·연출 품질은 승인 대상이 아니며 후속 폴리싱을 보류한다. 충돌·무적·저프레임·제3자 간섭 등 자동 테스트로만 확인한 세부 경계 조건은 CODEX VALIDATED 범위로 유지한다. 원래 실패가 PASS 3D에서 새로 유발됐는지의 INCONCLUSIVE 판정은 기능 승인과 별개로 유지한다.

다음 개발 후보는 지상 방향 대시·회피다. 이번 문서 정리에서 구현하지 않는다. [체크포인트 포함·제외 제안](SHIBUYA_PASS3D_CHECKPOINT_PLAN.md)을 검토한 뒤 staging/commit/push를 별도 승인받는다. 아래의 사용자 확인 순서는 재현 방법이며, 승인된 세 기능을 다시 미승인 상태로 되돌리는 의미가 아니다.

## 수정 전 관찰과 인과관계

이전 대표 실패는 공중 입력 3/단계 3/이벤트 5에 실제 적중 4, 피해 50이었다. 별도 물리 진단에서 실제 OverlapSphere 대상 제외를 확인했다. 입력 중복과 체공 만료가 직접 원인은 아니었다.

이번 런타임 수정 전 `before-metrics`의 같은 입력 경로 대조는 일반 점프·추격 모두 5회 적중/피해 62로 통과했다. 따라서 과거 실패를 항상 재현되는 결함으로 바꾸어 설명하지 않는다. 별도 기존 실패 기록과 함께 사거리 경계 민감성을 보여준다.

| 수정 전 관찰 | 일반 점프 | 어퍼컷 추격 |
|---|---:|---:|
| 추격 도착 시 잔여 발사 수평 이동 | 해당 없음 | 0.5277m |
| 첫 공중 이벤트 직전 잔여 발사 이동 | 0m | 0.1838m |
| 첫 이벤트 Z 간격 | 1.4000m | 1.7214m |
| 마지막 이벤트 Z 간격 | 1.5366m | 2.0417m |

일반 점프도 타격마다 약 0.034m의 작은 넉백이 있다. 추격 경로에는 기존 발사의 잔여 이동이 더해졌다. 공격자 위치는 공중 공격 동안 고정되며 기존 공중 조준은 회전만 수행한다. 서로 다른 실행의 좌표를 합쳐 한 궤적으로 취급하지 않는다.

PASS 3C 승인본의 공중 이동·체공·피격·타깃팅 코드와 관련 설정은 이번 수정 전에도 같았다. 공통 Fighter에는 PASS 3D 연결 변경이 있었다. 승인본과 현재 코드를 같은 환경에서 실행한 비파괴 A/B는 하지 않았으므로 **PASS 3D가 새로 유발했는지는 INCONCLUSIVE**로 유지한다.

## 새 상태 전환

1. Space 입력은 기존 유효한 어퍼컷 토큰과 경로 검사를 거쳐 실제 이동한다.
2. 기존 AfterAerialMotor의 상승·지면 이탈·수평 근접·높이 조건과 지형 가림 검사까지 만족해야 접촉한다. 세션 참가 중에는 접촉하지 않는다.
3. 유효 도착에서 피격자의 기존 발사 이동만 종료한다. 발사 수평 잔량·정점 대기·상승 속도·내려갈 때의 기존 수평 드리프트를 재개하지 않는다. airborne/중력/착지 상태는 유지한다.
4. 무피해 접촉 경직과 높이 유지를 짧게 적용한다. HP, DamageResolved, 평타 적중 횟수, 공격 단계, 타격 이벤트를 생성하지 않는다. 접촉만으로 LMB가 실행되지 않는다.
5. 후속 LMB가 없으면 접촉 만료 후 현재 위치에서 자연 낙하한다. 공격자의 피격·종료·대상 무효화·무적·지형 가림에도 유지 조건이 해제된다.
6. 새 실제 타격이 Applied된 후 피격 반응은 이전 발사 이동을 종료하고 새 타격의 넉백을 적용한다. Normal은 airborne를 유지하고, Launch는 새 데이터의 발사 궤도를 시작하며, Knockdown은 기존 다운 상태로 전환한다. 거부된 피해에는 이 경로가 실행되지 않는다.

모든 이동은 기존 단일 CharacterController.Move를 사용한다. 전역 Time.timeScale, 순간이동 정렬, 히트박스 확대, 추가 자동 추적은 없다.

## 데이터·보호값

신규 `chaseContactHoldTime = 0.18초`: 한국어 Inspector **추격 도착 무피해 접촉 경직·높이 유지 시간**. 기존 첫 타격 준비 0.08초와 도착 후 새 입력·한 프레임 처리 여유를 위한 짧은 초기 MVP 값이다. 기존 0.55초 공중 입력 창 전체를 확정 구속하는 시간이 아니다. 0이면 높이 유지/추가 경직 없이 기존 이동만 종료한다. SerializedProperty/Undo/숫자 유효성 검사를 유지한다.

기존 지상 피해 20/20/20/25, 공중 피해 12+13, 공중 체공 1.2초, 후속 입력 창 0.12초, 발사 높이·거리·시간·중력, 히트스톱, HP Dummy1000/Player100은 유지한다. 공중 movements 배열과 이동 보정 상한도 바꾸지 않았다. 이번 승인된 이동 종료 정책 이외의 보호된 동작은 수정 범위에 넣지 않는다.

## 파일 범위

게임플레이/데이터:
- `unity/Assets/Shibuya/Scripts/ShibuyaAerialCombatDefinition.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaAerialCombatRuntime.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaHitStateRuntime.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaFighter.cs` — 기존 PASS 3D 연결을 유지한 피격 이동 종료 추가.
- `unity/Assets/Shibuya/Data/Shibuya Aerial Combat.asset` — 새 접촉 시간 한 항목만 추가.
- `unity/Assets/Shibuya/Editor/ShibuyaInspectors.cs` — 한글 필드 추가.

검증:
- `Editor/ShibuyaPass3DAirContactTests.cs`와 필요한 `.meta` 신규.
- `Editor/ShibuyaPass3CTestRunner.cs`의 OutputPath는 별도 실행 폴더의 legacy 하위에 기존 테스트의 이미지·계측을 분리한다.
- 기존 PASS1/2/2.1/2.2/2.3/3A/3B/3B.1/3C/Hover/AirCount/3D 테스트는 출력 경로만 이 helper로 연결했다. 기존 assertion과 입력·타이밍은 유지했다. 예전 XML/로그/영상/스크린샷을 덮어쓰지 않기 위한 변경이다.
- 이 보고서. 기존 Scene/Prefab/Packages/ProjectSettings/Gojo·VFX/기존 .meta는 수정하지 않는다.

## 검증 이력과 증거

증거 루트: `unity/Logs/ShibuyaPass3DAirContactQA/20261022/`.
사전/최종 자산 감사 및 환경 복구: `unity/Logs/ShibuyaAirContactQA/20261022/`.

- `before`: 새 계측의 Vector3 JSON 순환 참조 저장 오류 2건. 테스트/QA 도구 오류로 보존했다.
- `before-metrics`: 런타임 수정 전 일반 점프/추격 2 PASS. 동일 계측의 상세 프레임을 보존한다.
- `targeted-initial`: 기존 실패 검사와 새 검사 17개 실행. 테스트용 제3자 파이터의 카메라 없는 입력 컴포넌트 때문에 전환 검사 4건이 실패했다. 테스트 구성만 수정했고 런타임 방어를 약화하지 않았다.
- `targeted-recheck`: 위 4건 PASS, 실제 약 30fps 접촉/낙하 검사 PASS. 착지 테스트는 어퍼컷의 기존 국소 히트스톱 종료를 기다리지 않은 준비 조건 때문에 실패했다.
- `land-recheck`: 실제 지면 접촉 뒤 Space를 시도하도록 준비 절차를 고쳐 PASS. 가짜 접촉 금지 assertion은 유지했다.
- 신규 검사 18개 최신 고유 PASS는 위 실행들의 케이스별 집계이며 단일 18 PASS 실행이 아니다.
- 초기 신규 테스트의 enum 명칭 오타 CS0117은 수정했다. 오류/실패 이력은 최종 현재 Console 상태와 구분한다.
- `final-regression-153`: 기존 135개 정확한 이름과 시네마틱 18개의 최종 **단일 실행 153 PASS / 0 FAIL / 0 SKIP**. XML leaf 이름 집합이 기존 135 + 시네마틱 18과 정확히 일치한다. 이전 여러 실행의 PASS를 이 단일 결과에 합산하지 않았다.

## 최종 측정·감사

| 항목 | 결과 |
|---|---|
| 기존 135개 회귀 | 최종 단일 실행 안에서 135 PASS |
| 기존 시네마틱 18개 | 같은 최종 실행 안에서 18 PASS |
| 새 접촉/이동 검사 18개 | targeted-initial/recheck/land-recheck의 최신 고유 18 PASS (복수 실행) |
| 수정 후 일반 점프 3입력 | 단계 3 / 이벤트 5 / 적용 5 / 피해 62 |
| 수정 후 추격 3입력 | 단계 3 / 이벤트 5 / 적용 5 / 피해 62 |
| 수정 후 추격 첫/마지막 이벤트 Z 간격 | 1.3631m / 1.5021m |
| 마지막 이벤트 직전 타격 구 중심~피격 Collider ClosestPoint 거리 | 0.2721m (공격 반경 0.9m) |
| 접촉 직후 원래 발사 수평 잔량 / 수직 속도 | 0m / 0m/s |
| 접촉만으로 추가 HP 피해 / 공격 생성 | 0 / 0 |
| 접촉 후 무공격 낙하까지 Z 변위 | 0m, 원래 수평 발사 이동 재개 없음 |
| 저프레임 검사 실제 중앙 프레임 간격 | 약 33.255ms (약 30fps) |
| Play Mode 포커스 샘플 | 142개 timing 파일, 117,509 프레임 중 Editor focused 0 |

위 수정 전/후 수치는 각 실행의 프레임 계측이다. frozen PASS 3C 실행과의 A/B가 아니므로 원래 실패의 PASS 3D 유발 여부는 미확정으로 남긴다. 현재의 공중 기본 데이터는 여전히 Normal 피격 이벤트만 허용한다. 이번 Launch/Knockdown 갱신 검사는 공통 타격 엔진의 실제 지상 공격으로 이미 공중에 있는 대상을 때려 검증했으며, 새 공중 전용 마무리 기술은 만들지 않았다.

최종 컴파일 실패 없음. Play Mode 종료, active scene `ShibuyaCombatMVP`, sceneDirty=false, persistent dirty asset 0, missing script 0, Time.timeScale=1. 소유 Ground Crack mesh/material 0/0.

**현재 Console은 error 1 / warning 1이다.** error는 최종 AutoTick 복구 요청이 Pipeline의 5초 main-thread 응답 제한을 초과하며 남긴 도구 로그이다. `environment-restored.json`에서 실제 복구는 완료됐음을 확인했다. warning은 기존 `Deleting invalid font reference.`이다. Console을 지우지 않았으며 오류 0이라고 보고하지 않는다. 최종 153개 실행에는 게임 런타임 예외/테스트 실패가 없었다. 초기 테스트용 제3자 입력 구성 예외와 enum/계측 오류의 이력도 보존했다. `final-console-errors.json`의 Pipeline 누적 이력 수치와 현재 Unity Console 카운트를 구분한다.

임시 AutoTick 0ms를 시작 상태 true/16ms로 복구했고 저장된 interval 선택은 -1(미설정), targetFrameRate=-1, vSync=0, 테스트 active=false다. QA Root/Diagnostic 임시 키를 정리했다. Play Unfocused를 바꾸거나 실제 Windows 창을 직접 조작하지 않았다. Play Mode 샘플은 모든 로딩 구간의 Windows 포커스 상태를 증명하지 않는다.

기존 파일 1,082개 SHA-256 감사에서 이번 작업의 기존 파일 변경은 위 코드·데이터·Inspector·테스트 출력 경로 19개뿐이다. 기존 Scene/Prefab/Packages/ProjectSettings/Gojo/VFX/locked 문서/기존 .meta 및 로컬 PASS 3D 보고서가 동일하다. Aerial Combat.asset은 새 접촉 시간 한 줄을 제외하면 HEAD와 동일하다. 기존 PASS 3D QA 214개 파일은 해시 전부 동일, 삭제 0.

최종 Git: `feat/shibuya-combat-mvp`, HEAD `42833ebfec9a176056ae983096c41c1ef4d8fb24`, staged 0 / unstaged 23 / untracked 18. 앞선 PASS 3D 작업을 포함한 수치다. staging/commit/push/merge 없음.

증거: `validation-summary.json`, `final-regression-153/results.xml`, 각 targeted 실행의 XML/프레임 trace, `scope-audit.json`, `before-audit.json`, `final-audit.json`, `environment-restored.json`, `focus-audit.json`, `final-console-errors.json`.

## 사용자가 직접 확인할 순서

1. ShibuyaCombatMVP Play Mode에서 Space 없이 W+LMB 4타: 기존 어퍼컷 궤도 확인.
2. W+4타 적중 뒤 새 Space: 실제 도착 때만 잠깐 멈추고 HP가 더 줄지 않는지 확인. LMB 없이 기다리면 원래 포물선을 재개하지 않고 낙하해야 한다.
3. 도착 뒤 새 LMB 3회: 2+2+1 적중, 작은 타격별 밀림과 공격 중 체공 확인. 계속 누르고 있기만 해서는 자동 연계되지 않아야 한다.
4. 일반 점프 후 같은 공중 LMB와 비교하고 종료·피격·착지 시 정상 복귀를 확인.
5. `JJK Game → 시부야 PASS3D → 시네마틱 예제 ON (Play Mode 전용)` 후 W/S 없는 일반 4타: 기존 약 0.5초/3샷과 복귀 확인. 같은 메뉴 OFF 또는 Play Mode 종료로 해제한다.

사용자 직접 플레이 승인과 자동 검증을 구분한다. staging/commit/push/merge 및 PASS4 구현은 하지 않는다.
