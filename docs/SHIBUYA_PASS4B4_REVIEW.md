# 시부야 PASS 4B.4 — 실제 주력 자원 / F 홀드 충전

> 2026-10-25 사용자 확인 반영: 실제 F 홀드 주력 충전이 동작함을 사용자가 확인하고 관련 체크포인트 저장을 승인했다(USER VERIFIED — 직접 플레이 범위). 아래는 구현 당시의 자동 검증/검수 안내 기록이다. HUD 세부 품질·모든 경계 조건을 사용자 승인으로 확대하지 않는다. 신체 강화 패시브는 보류이며 구현되지 않았다.

2026-10-25(KST). LOCAL / CODEX VALIDATED / 사용자 Play Mode 승인 대기.
브랜치 `feat/shibuya-combat-mvp`, 기준 HEAD `42833ebfec9a176056ae983096c41c1ef4d8fb24`.
기존 PASS 3D~4B.3c 미커밋 작업 위에 필요한 연결만 추가했다. commit/push/staging 없음.

## 1. 자원과 입력 구조

기존 공통 `CursedEnergyController`의 CurrentEnergy, 소비/회복 API, EnergyChanged 이벤트를 재사용했다. 별도 현재 주력 풀은 만들지 않았다.

- `CursedEnergyProfile.RecoveryMode`: 기존 기본값은 `Automatic`(0), 시부야 자원만 `HoldCharge`(1)로 opt-in.
- 홀드 모드의 최대치는 **매번 Profile.MaxEnergy**에서 읽는다. 아우라와 HUD, 소비/회복이 같은 최대치를 참조한다.
- 기존 `regenerationPerSecond`를 홀드 모드의 초당 충전량으로 재사용한다. 별도의 중복 충전량 필드는 없다.
- 홀드 모드 시작량은 `MaxEnergy * StartingRatio`. 기존 절대 시작량은 자동 회복 모드에서만 사용하며 Inspector도 모드에 맞는 값만 표시한다.
- 시부야 에셋은 최대100 / 시작 비율1 / 충전 가능 / 초당12. 기존 최대치·회복량·소모 배율은 바꾸지 않았다. 홀드 모드에서는 기존 자동 회복 지연을 사용하지 않는다.
- `TrySpend`, `CanSpend`, `Restore`, `SetCurrentEnergy`, `ResetEnergy` 및 `EnergyChanged` 사용 가능. 최대치 변경으로 현재치가 줄거나 최대치만 달라져도 이벤트를 제공한다.
- `ShibuyaEnergyCharge`가 F 의도와 회복을 연결한다. `ShibuyaPlayerInput`에서 기존 아우라와 같은 F 입력을 전달하며 이동/공격 입력은 바꾸지 않는다. 자원 컴포넌트는 로컬 플레이어에 런타임 연결하며, 공통 컴포넌트를 NPC에도 연결할 수 있다.
- 최대치에서도 `HoldDuration`은 계속 누적된다. 실제 F 해제는 `ChargeReleased(duration)`, 피격/포커스 상실/비활성 등은 `ChargeInterrupted(duration)`로 분리했다. `ChargeStarted`도 제공한다. 강화 조건·버프·대시는 구현하지 않았다.
- 경직/발사 피격/다운/시네마틱/사망/전투 종료 중 충전하지 않는다. 가능한 상태로 복귀한 뒤 F가 계속 유지되면 새 홀드를 시작한다. 이동·일반 점프·자기 공격 자체는 충전을 막지 않는다.
- 회복/홀드 시간은 Time.deltaTime 기준이다. 전역 Time.timeScale과 기존 참가자별 히트스톱은 변경하지 않았다.

## 2. 본인 HUD / 한국어 Inspector

기존 `ShibuyaMatch.OnGUI`의 본인 초록 HP 아래에 파란 현재/최대 주력바를 추가했다. `충전 중` / `최대 유지`를 표시한다. HUD 자원 원본은 Match.Player의 자원 하나이며 상대의 현재 주력 UI는 없다.

`Assets/Shibuya/Data/Shibuya Cursed Energy.asset`에서 다음 한국어 항목을 편집한다.

- 최대 주력량
- 주력 회복 방식
- 시작 주력 비율 (0~1)
- 홀드 충전 가능
- 초당 주력 충전량
- 기존 주력 소모 배율 / 최소 술식 소모량 / 알림 표시 시간

Scoped CustomEditor와 SerializedProperty로 Undo·복수 선택·기존 편집 기능을 유지한다. 컴포넌트의 한국어 Inspector는 현재치/최대치/충전 여부/홀드 시간을 읽기 전용으로 보여 주고 Data Asset을 안내한다. 기존 자동 회복용 캐시 값을 홀드 모드에서 중복 편집하게 하지 않는다.

## 3. 검증 결과와 원본

QA 루트: `unity/Logs/ShibuyaPass4B4QA/20261025-energy/`.

| 별도 실행 | PASS | FAIL | SKIP | 원본 |
|---|---:|---:|---:|---|
| 신규 자원·F·HUD·기본 조작 | 9 | 0 | 0 | `initial/results.xml` |
| 관련 기존 회귀 | 6 | 0 | 0 | `targeted-regression/results.xml` |
| 해제/중단 이벤트 구분 후 관련 재확인 | 2 | 0 | 0 | `release-cancel-events/results.xml` |

**최신 고유 케이스 15 PASS / 0 FAIL / 0 SKIP.** 마지막2건은 신규9건 중2건의 재실행이며 17개 고유 검사로 합산하지 않는다. 전체 전투 회귀 실행이 아니다. 실제 Unity EnterPlayMode/Update/ExitPlayMode를 사용했으며 사용자 직접 플레이 승인을 대신하지 않는다.

신규9개: `ProfileOwnershipKoreanInspectorAndUndo`, `CapacitiesStatesRecoveryAndResourceEvents`, `FullHoldReleaseAndRapidCycles`, `StartingRatioDisabledChargingAndLiveMaximum`, `AuraCurrentIndependenceAndOwnerOnlyHud`, `ExistingAutomaticRecoveryRemainsAutomatic`, `ChargeDoesNotReplaceMovementJumpOrAttack`, `HitDeathMatchEndAndRestartCancelCharge`, `HudVisualEvidence`.

관련 기존6개: 씬/Inspector 참조, 타격별 독립 수치·버퍼4타, 공중3클릭5타·높이 유지, 마무리 시네마틱 카메라 복귀, 반대 카메라 방향에서 락온 공격, 4B.3c의 2.7/3.0/3.5/4.0 미리보기·현재 주력 독립성.

측정 예:

- 최대100 / 시작0: 홀드0.3154492초 → 현재3.78539014 (초당12).
- 최대150 / 시작75: 홀드0.3167464초 → 현재78.80094.
- 최대200 / 시작100: 홀드0.3002955초 → 현재103.603554.
- 시작 비율0.25에서 최대100/150/200 → 25/37.5/50. 충전 비활성에서는 F를 유지해도 회복 없음.
- F 12회 반복 및 입력 컴포넌트 비활성: 정상 해제13회와 별도 중단1회. 최대 주력에서도 홀드 누적 확인.
- 현재0/100/200을 변경해도 최대200 아우라 배율2.0, 메시 폭1.76495385 유지. 같은 Visual을 반복 재사용하고 소유 메시3개 유지, 해제 후 렌더 비활성/파티클0. 중복 자원/충전 컴포넌트 없음.
- 런타임 상대에게 별도 자원17을 부여해도 본인 HUD 원본은 본인 자원으로 유지되고 상대 자원은 표시/변경하지 않음.
- 피격/전투 종료/사망 시 충전 중단, 새 전투에서 기본 프로필의 최대치로 시작.

상세 수치 JSON과 HUD 이미지는 `initial/legacy/ShibuyaPass4B3QA/`에 있으며 마지막 이벤트 수치는 `release-cancel-events/legacy/ShibuyaPass4B3QA/4B4-hold-cycles.json`에 보존했다.
HUD 이미지: `initial/legacy/ShibuyaPass4B3QA/4B4-player-hud-charging.png`.

## 4. 보호와 변경 파일

시작 시 1,182개 비무시 파일을 SHA-256으로 기록했다. 기존9개만 이번 범위에서 변경되고1,173개는 동일하다. 삭제0. 원본 Gojo VFX·Scene·Prefab·Packages·ProjectSettings·기존 GUID·아우라 Geometry/Visual/Profile/Shader·색상 프로필·미리보기 후보는 동일하다. 전투/카메라/락온/공중전 알고리즘을 수정하지 않았다. 기존 기본 피해20/20/20/25, Dummy HP1000, Player HP100 유지.

이번에 변경한 기존9개:

- `unity/Assets/Scripts/Core/CursedEnergyProfile.cs`
- `unity/Assets/Scripts/Core/CursedEnergyController.cs`
- `unity/Assets/Editor/KoreanCombatInspectorEditors.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaPlayerInput.cs`
- `unity/Assets/Shibuya/Scripts/ShibuyaMatch.cs`
- `unity/Assets/Shibuya/Data/Shibuya Cursed Energy.asset`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4B3Tests.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4B3aTests.cs`
- `unity/Assets/Shibuya/Editor/ShibuyaPass4B3cTests.cs`

마지막3개는 이제 입력 경로에서 자원 컴포넌트가 존재하므로 검사의 `AddComponent`만 `GetOrCreate`로 교체했다. 기대값/타이밍/판정 기준은 바꾸지 않았다. 공통 Core 확장은 기존 자동 회복을 기본값으로 보존하며 별도 Play Mode 검사도 통과했다.

신규7개: `Scripts/ShibuyaEnergyCharge.cs`, `Editor/ShibuyaEnergyChargeInspector.cs`, `Editor/ShibuyaPass4B4Tests.cs`와 각각의 `.meta` (앞 경로는 `unity/Assets/Shibuya/`), 이 보고서.

4B.3c 승인 외형을 위한 시각 변경은 없다. 4.0이나 다른 실험 배율을 영구 프로필로 승격하지 않았다. 잔여 주력이 줄어도 기존 최대치 기반 크기를 유지한다. 기존 dirty handoff/current task와 이전 QA를 덮어쓰지 않았다.

## 5. 컴파일·Console·환경

최종 C# compile 성공. 이번 작업에서 aura shader는 수정하지 않았고 렌더 캡처에 shader 오류가 발생하지 않았다.
테스트 작성 중 존재하지 않는 피격 API/enum 사용으로 컴파일 오류5개가 발생했으며 올바른 기존 API로 수정했다. 오류 이력을 삭제하지 않았다. 마지막 확인의 현재 Console error0 / warning1이며, 보존 버퍼에는 위 컴파일 오류5개와 `Deleting invalid font reference.` 등의 경고가 남아 있다. 오류가 전혀 없었던 세션으로 표현하지 않는다.

최종 Play stopped / 씬 clean / QA 비활성 / targetFrameRate -1 / vSync0 / Time.timeScale1. AutoTick true/16ms 확인, AutoTick/Play Unfocused는 변경하지 않았다. QA의 프레임률60은 teardown에서 -1로 복구했다. Play Mode 종료 후 활성 자원/충전 컴포넌트0 확인. Windows 직접 조작·포커스 요청 없음.

`before-sha256.json`, `after-audit.json`, `final-editor-console.json`에 보존/상태 근거를 남긴다. 이전 QA XML·실패 기록을 삭제하거나 새 결과로 덮지 않았다.

## 6. 사용자 확인 방법 / 남은 한계

1. 기존 `ShibuyaCombatMVP`에서 Play. 본인 초록 HP 아래 파란 주력100/100을 확인한다.
2. 런타임 플레이어의 `ShibuyaEnergyCharge` 컴포넌트 메뉴 → `검증/현재 주력 비우기 (Play Mode)` 또는 `절반`을 선택한다. 선택은 런타임 현재치만 바꾼다.
3. Game View로 돌아와 F 홀드. 기존 아우라가 켜지고 초당12 회복하는지, 해제하면 멈추는지 확인한다. 최대치에서도 `최대 유지`와 Inspector의 홀드 시간이 계속 증가해야 한다.
4. Data Asset의 최대100/150/200·시작 비율을 비교할 때는 변경값을 기록하고 사용자가 의도한 값만 저장한다. 시작 비율은 새 전투/ResetEnergy에서 적용된다. 현재 주력 증감으로 아우라 크기가 변하면 안 된다.
5. F를 반복하고 이동·점프·타격·락온·공중 추격·시네마틱을 직접 확인한다. 상대 현재 주력바는 없어야 한다. 전투 종료 뒤 새 전투의 시작량도 확인한다.

HUD 배치/폰트 가독성/충전 속도의 체감과 4B.3c 시각 최종값은 사용자 검수 대상이다. 넓은 맵/다인전/저사양 GPU/멀티플레이는 미검증이다. 강화 준비·임팩트 프레임·F 해제 버프·강화 중 아우라 지속·공격력/이속 강화·W+F 대시는 이번 구현에 없다. 후속 시스템은 제공된 자원/홀드/해제/중단 API에서 연결한다.
