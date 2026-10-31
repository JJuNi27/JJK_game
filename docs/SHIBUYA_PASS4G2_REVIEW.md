# PASS 4G.2 — 패링 후 캐릭터별 반격 MVP

> 2026-10-31 체크포인트 보충: 사용자는 패링 이후 반격과 흑섬 연계를 MVP 수준 USER VERIFIED로 승인했다. 모든 취소/벽/사망/특수 방어 경계와 캐릭터별 모션·최종 연출의 승인은 아니다. `docs/SHIBUYA_PASS4G_CHECKPOINT.md` 참고. 아래 LOCAL UNCOMMITTED/검수 대기는 구현 당시 기록이다.

2026-10-31. LOCAL UNCOMMITTED / CODEX VALIDATED(targeted) / USER VERIFIED 대기. PASS4G.0→4G.1 게이트 통과 후 구현했다.

## 발동·입력·실행

- 실제 `TryParry` 성공에서만 상대 Fighter와 입력 기회0.5초를 기록한다. 패링만으로 공격하지 않는다. 성공자의0.1초 후속 자세와 별도 타이머다.
- `ShibuyaPlayerInput`의 fresh LMB 경계에서 실제 guard state의 잡기 우선권을 유지한 뒤 반격 기회를 소비한다. 기회가 없으면 기존 W/S 분기를 포함한 일반 TryAttack으로 돌아간다. Hold 자동 실행 없음. AI는 기존 TryAttack을 사용하므로 자동 반격이 추가되지 않는다.
- 패링 성공 뒤0.1초 중 들어온 새 LMB도 예약하고 자세 복귀 후 시작한다. 입력 기회가 있는 것만으로 이동/대시를 막지 않는다. 이미 대시/충전/공격 중이라면 반격 입력은 충돌 사유로 소비·거부된다.
- 임시 공통 반격은 기존 MovesetStep/hits/movements/StrikeDefinition을 그대로 실행한다. 별도 지상·공중 타격 엔진이나 별도 CharacterController.Move를 만들지 않았다. 미래 캐릭터는 반격 프로필의 이름·피해·모션·접근/이동·타격 배열·피격 결과·타격음/강도·선택적 시네마틱을 교체한다. 캐릭터 전용 고급 애니메이션/VFX/사운드를 제작하지 않았다.

## 대상 예약과 확정 적중

시작 시 거리4m/각도120도·생존·씬·특수 무적·다른 소유권·지형 시야를 검증한다. 유효하면 패링당한 **한 대상만** 최대3초 예약한다. 준비 동작이 길더라도 기존0.5초 경직과 독립적으로 유지한다. 예약 대상은 일반 이동·공격·가드·대시·패링을 새로 실행할 수 없다. 예약 완료까지 일반 사거리/기존 경직 종료 때문에 빗나가지 않도록, 해당 반격 이벤트만 reserved target에 기존 Health 트랜잭션을1회 적용한다. 전역 판정 범위·일반 추적·무브셋 예산은 변경하지 않는다.

실제 접근은 작성된 기존 ApproachLastHit 클립과 Fighter의 충돌 이동을 쓴다. 벽 너머 순간이동, 강제 좌표 스냅, 상대 전역 무적 해제는 하지 않는다. 벽이 새로 생기거나 특수 무적·별도 IDamageGuard·사망·전투 종료·비활성·시네마틱 충돌이 생기면 취소/거부하고 예약을 해제한다. 반격 확정 적중의 **명시적 예외**다. 특정 고죠 무하한 자산을 바꾸거나 실제 고죠 장면을 재실행하지 않았다.

입력 기회는 죽음·피격·다운·시네마틱·입력 캡처 상실·비활성·새 패링 소유권·만료에 취소된다. 이후 예약 상태도 같은 중단 경로에서 정리한다. 예약 상한이 작성된 전체 시퀀스와 남은 자세 시간을 수용하지 못하면 시작 자체를 거부한다. 긴 hit-stop/긴 작성 시퀀스는 저자가 예약 상한을 함께 조절해야 한다.

## 실제 데이터

- 기존 Parry asset: 공격자 경직 **0.35→0.5초**만 의미상 변경. 밀려남1.2m, 성공자 자세0.1초, window0.12/retry0.5/시각값 유지. Unity 재직렬화에서 필드 순서 변화는 있으나 다른 값은 동일하다.
- `Shibuya Counter.asset`: 입력 창0.5, 시작 거리4, 각도120, 예약 상한3, 반격 조건부 흑섬50%. 기본 피해20/준비0.18/활성0.04/회복0.25, HeavyCross, 경직0.3/넉백6/hit-stop0.06. 접근0.2초·최대1.2m·속도8·도착1.3m. 공통 MVP 초깃값이고 캐릭터 공통 고정 규칙이 아니다.
- 반격 치명타100%. 캐릭터 능력+타격 자격이 모두 허용하면 조건부 흑섬50%로 일반15/Zone20% 상한의 예외다. 치명1.5와 흑섬2.5는 대체 관계. 흑섬 보상은 같은 Applied-only ticket 경로를 사용한다.
- 한국어 Inspector는 SerializedProperty/기존 drawer를 재사용하며 Undo·배열·참조 편집을 보존한다.

## 검증 이력

`unity/Logs/ShibuyaPass4GQA/20261031-combat-grades/g2-validation/results.xml`: **단일 실행12 PASS/0 FAIL/0 SKIP**(데이터2+Play Mode10). 이후 추가 경계·기존 기능 최종 targeted 실행은 별도 `final-targeted/`에 보존한다. 단일 전체 실행으로 합산하지 않는다.

초기12개에서 확인: 유효 패링+새 클릭, 자동 실행 없음, 만료 후 평타/자유 대시, 잡기 우선권, Hold 중복 방지, 긴 준비0.8초/패링 경직 종료 뒤에도8m로 옮긴 예약 대상에1회 치명타30, 벽 전/중 차단, 특수 IDamageGuard 차단, capture/피해/disable/end 정리, 흑섬 보상1회, 다단2회 독립 흑섬.

최종 `final-targeted/results.xml`은 **단일27 PASS/0 FAIL/0 SKIP**다. G1 실제 경로7개, G2 추가 경계4개와 등급 재검증2개, 아우라 동일성1개, 기존 회귀13개다. 기본/업/다운·W 입력 버퍼·공중3입력5히트·카메라 시네마틱 복귀·8방향 대시·실제 패링 중단/밀려남·잡기·긴급 탈출·공격형 및 패링형 AI·사망/새 전투 재시작을 확인했다. 추가 경계는 입력 deadline 직전/동일 시각(동일 시각은 만료), 특수 무적/시네마틱/대상 사망, 제3자 전투 지속, QA 창 프로필/Stats 복원, 원속도 촬영이다.

**전체 최신 고유58 PASS/0 FAIL/0 SKIP는5회 실행 집계**다. 58개를 단일 실행한 결과가 아니다. 최초 G0 데이터 검사1 FAIL/수정과 G1 테스트 CS0535 컴파일 이력1건을 유지한다. `latest-unique-results.json`에 실행별 집계·최신 케이스 출처가 있다. 최종 보상 dedup 검사는 교차 시퀀스 뒤 늦게 들어오는 중복 ticket도 거부하며, 초과 피해55와 실제 HP 감소10을 구분했다.

기존 기본 피해를 검증하는 QA fixture(4B3/3B/3C1)만 일반 등급 RNG를 주입해 신규 치명타 확률과 독립적으로 검사했다. 실게임25%는 변경하지 않았다. 기존4D1 테스트의 경직 기대값은 명시 승인한0.5초로 변경했으며 오차 허용 폭은 이전과 같다. 다른 기본 피해·게임플레이 기대값을 느슨하게 하지 않았다. 과거 모든 세대의 전체 테스트를 실행한 것은 아니며,100 주력을 전제하던 과거 시점 테스트를 이번 전체 성공으로 포함하지 않는다.

최종 컴파일 정상, Editor Console error0/warning1(font). Pipeline 세션은 수정한 CS0535 error1을 보유하며 이를 오류0으로 숨기지 않는다. Play stopped/씬 clean/fps -1/Time.timeScale1/runInBackground false, AutoTick true16ms/PlayUnfocused 유지. 새 런타임 QA 프로필 사본·Shibuya/Aura 소유 Mesh/Material 잔류0/0/0. 관련10개 Data Asset에서 missing reference0. 증거는 `environment-final.json`, `references-and-focus-final.json`이다. GPU 프로파일링·장시간 soak·실제 네트워크는 수행하지 않았다.

원속도 영상: `final-targeted/legacy/ShibuyaPass4B3QA/4G2-parry-counter-original-speed-60fps.mp4`. 75개 실제 Play 캡처와 JSON 타임스탬프를 기준으로60fps에 재배치한 것이며 네이티브60fps 촬영/보간이 아니다. 첫25fps concat 출력도 삭제하지 않고 보존했지만 타임스탬프 양자화가 있어 최종 원속도 증거로 사용하지 않는다.

검증 JSON은 각 실행의 `legacy/ShibuyaPass4B3QA/`에 있다. 원본 실패/컴파일 이력을 보존한다. PASS4A 과거 미해결18건은 해결됐다고 기록하지 않는다.

## 사용자 검수 방법

1. `ShibuyaCombatMVP` Play. AI 기본OFF를 유지한다. `JJK Game/시부야 PASS4G/공격 등급·반격 검수`를 열고 현재 플레이어 연결.
2. 패링 연습은 기존 `JJK Game/시부야 PASS4D/가드 피격 검수 도구` → `패링 반복 연습 시작 (3초 예고)`. Game View에서 예고에 맞춰 새 RMB를 누른다.
3. 패링 성공 직후0.5초 안에 LMB를1회 누르면 일반 평타 대신 반격. 아무것도 누르지 않으면 자동 공격하지 않는다. 늦은 LMB는 일반 평타다. RMB가 물리적으로 계속 눌려도 실제 가드는 패링 성공 시 소비되므로 반격이 잡기로 바뀌지 않아야 한다.
4. 기본 캐릭터는 흑섬 OFF이므로 반격 피해30. QA 창에서 런타임 흑섬 허용 후 강제 흑섬 추첨을 선택하면 피해50, 주력+10·Zone·스택을 관측한다. 다음 Zone 흑섬은55. 일반 HUD에 등급/가드 체력을 추가하지 않았다.
5. 창을 닫으면 도구 소유 런타임 사본만 복원된다. 종료/새 Play에서 기본 확률·흑섬OFF 및 새 대전 스택0으로 돌아간다.

## 남은 품질·확장 제한

예약은 기능적인 적중 보장이다. 극단적으로 멀리 옮겨진 대상도 지형 시야가 유지되면 맞으므로, 모든 거리에서 주먹이 실제 표면에 닿아 보이는 최종 애니메이션 품질을 보장하지 않는다. 캐릭터별 접근/위치 연출은 후속 검수 대상이다. 네트워크 동기화·전용 시네마틱·기술 VFX는 미구현이다. 이번 결과는 자동 기술 검증이며 사용자 조작감 승인과 구분한다.

## 실제 변경 파일 범위

전체 정확한 파일 목록/초기 해시 비교는 QA 루트 `final-file-inventory-and-protection.json`이다. Git에는 tracked dirty31/untracked34/staged0이며, 이 중 `unity/unity.slnx`1개는 기존 사용자 변경을 그대로 보존한 것이다. 이번 작업이 수정·생성한 범위는 나머지64개다. Unity 신규 스크립트13개와 데이터2개는 각각 `.meta`를 함께 생성했고, 기존 `.meta` 변경0/신규 meta 누락0/기존1292개 추적 파일 중 예상 밖 변경0/삭제0을 확인했다.

Unity 경로는 `unity/Assets/Shibuya/` 기준:

- **G0:** `Scripts/ShibuyaAttackMetadata.cs`, `Scripts/ShibuyaCombatProfile.cs`, `Scripts/ShibuyaGrabProfile.cs`, `Scripts/CursedEnergyAuraController.cs`; `Data/Shibuya Combat Profile.asset`, `Shibuya Ground Finishers.asset`, `Shibuya Aerial Combat.asset`, `PASS3A Knockback Approach.asset`, `PASS3A Multi Hit.asset`, `Shibuya Grab.asset`, `Shibuya Cursed Energy.asset`; `Editor/ShibuyaAttackClassificationMigration.cs`, `ShibuyaGrabInspector.cs`, `ShibuyaInspectors.cs`, `ShibuyaMovesetInspector.cs`, `ShibuyaPass4G0Tests.cs`.
- **G1:** 신규 `Scripts/ShibuyaAttackGradeProfile.cs`, `ShibuyaAttackGrades.cs`, `ShibuyaAttackGradeRuntime.cs`, `Data/Shibuya Attack Grades.asset`, `Editor/ShibuyaAttackGradeInspector.cs`, `ShibuyaPass4G1Tests.cs`. 기존 `Scripts/ShibuyaFighter.cs`, `ShibuyaCombatProfile.cs`, `ShibuyaEnergyCharge.cs` 및 공통 `unity/Assets/Scripts/Core/CursedEnergyController.cs`에 실제 피해/소비/회복 연결.
- **G2:** 신규 `Scripts/ShibuyaCounterProfile.cs`, `ShibuyaCounterRuntime.cs`, `Data/Shibuya Counter.asset`, `Editor/ShibuyaCounterInspector.cs`, `ShibuyaCombatGradeReview.cs`, `ShibuyaPass4G2Tests.cs`; 기존 `Scripts/ShibuyaFighter.cs`, `ShibuyaCombatProfile.cs`, `ShibuyaCinematicFighterRuntime.cs`, `ShibuyaHitStateRuntime.cs`, `ShibuyaParryRuntime.cs`, `ShibuyaPlayerInput.cs`, `ShibuyaTargetingRuntime.cs`, `ShibuyaEnergyCharge.cs`, `Data/Shibuya Parry.asset`, 공통 한글 Inspector 연결.
- **QA 공통:** `Editor/ShibuyaPass3CTestRunner.cs`의 G 출력 경로 허용, `ShibuyaPass3BTests.cs`/`ShibuyaPass3C1Tests.cs`/`ShibuyaPass4B3Tests.cs`의 명시적 기본 피해 RNG, `ShibuyaPass4D1Tests.cs`의 승인된0.5초 경직 기대값.
- **문서:** 현재 handoff/task/design3개 갱신, G0/G1/G2 보고서3개와 표준 행동 명칭 문서1개 신규. 잠금 USER VERIFIED 문서와 AGENTS는 변경하지 않았다.

기존 Gojo/VFX·승인 아우라 Profile/Shader/Geometry·Scene/Prefab·Packages/ProjectSettings·기존 GUID는 해시 기준 불변이다. 원격/로컬 HEAD는 계속 `ce6626ba6148c34e9ea4b98024a47556f9645854`, 브랜치 변경·staging·commit·push 없음. QA 원본·영상·로컬 자산은 Git에 추가하지 않는다.
