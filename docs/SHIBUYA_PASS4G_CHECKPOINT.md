# PASS 4G — 사용자 승인 Git 체크포인트 (2026-10-31)

## 저장 범위와 상태

브랜치 `feat/shibuya-combat-mvp`, 기준 로컬/원격 HEAD `ce6626ba6148c34e9ea4b98024a47556f9645854`, origin `https://github.com/JJuNi27/JJK_game.git`. 저장 전 tracked dirty34 / untracked50 / staged0이며 원격 실조회 SHA와 추적 참조가 모두 일치했다. 이전47개 보고 이후 테스트/.meta/누락 수정 보고서3개가 추가된 정상 상태다.

이 문서가 포함되는 다음 체크포인트 커밋은 PASS4G.0~4G.3와 숫자 누락 수정의 의존성을 함께 저장한다. 정확한 커밋 SHA와 push 결과는 해당 Git 이력 및 완료 보고에서 확인한다. 사용자 승인에 따라 현재 브랜치에만 일반 commit/push하며 강제 push·merge·rebase·브랜치 전환은 하지 않는다. 새 게임플레이·VFX·Unity 테스트를 실행하지 않는다.

## USER VERIFIED — MVP 수준

사용자가 Unity Play Mode에서 직접 확인한 범위만 승인으로 기록했다.

- 치명타가 실제로 발동함.
- 흑섬이 실제로 발동함.
- Zone 진입 및 스택 동작.
- Zone 버프 적용.
- 패링 이후 반격과 흑섬 연계.
- 데미지 숫자 표시.
- 기본 콤보에서 타격마다 숫자가 정상 출력됨.
- 이전 숫자 누락 수정 이후의 기본 표시 동작.

모든 공격 분류 예외, 확률 통계 경계, 다수 대상·극단적 다단, 반격 취소/사망/벽, 특수 방어·무하한 전체 회귀, 장시간 자원·성능, 화면 밖 숫자, 다단 흑섬 문구 겹침은 사용자 승인으로 확대하지 않는다. 자동 검증 근거가 있는 부분만 CODEX VALIDATED로 유지한다. 흑섬 VFX·Zone 전용 연출·캐릭터별 반격 애니메이션·모든 캐릭터 최종 밸런스는 최종 승인하지 않았다.

## 기능·데이터와 의존성 검토

공격 분류, 시부야 주력200/아우라 기존 크기 유지, 치명25%·1.5배, 흑섬2.5배/자격/최종 상한, Applied-only 주력·스택·Zone 보상, 패링 후0.5초 입력·대상 예약 반격, 실제 HP 감소량 기반 숫자/옵션/슬롯, accepted action별 중복 키 수정과 필요한 테스트·한국어 Inspector를 함께 저장한다.

공용 Core 수정2개도 의존성에 포함한다. `Health.cs`는 알림 전 실제 HP 감소를 읽는 out 오버로드이고 기존 API·방어·알림 순서를 유지한다. `CursedEnergyController.cs`는 actor-local 선택적 비용 modifier를 읽으며 이 컴포넌트가 없는 기존 캐릭터는 원래 비용을 유지한다. 고죠 전용 코드·VFX 자산 변경과 혼동하지 않는다. 기존 Shibuya 아우라 Controller 수정은 주력200에 대응하는 캐릭터 기준 용량 정규화이며 공유 Shader/Geometry/Profile의 신규 시각 변경이 아니다.

기본 피해20/20/20/25, Dummy HP1000/Player HP100과 기존 대시·가드·공중·카메라 값은 그대로다. 이전 승인된 변경은 시부야 주력100→200과 패링 상대 경직0.35→0.5초이며 이번 저장에서 값을 추가 조정하지 않는다. PASS3A 예제 자산 등의 긴 diff는 공격 메타데이터 및 기존 기본값의 Unity 직렬화 보충이며 관련 보고서의 의미 비교 근거를 보존한다. 기존 .meta GUID 변경은 없다.

## 검증 결과 — 기존 원본 재사용

| 범위 | 최신 고유 결과 | 실행 관계 |
|---|---|---|
| PASS4G.0~4G.2 |58 PASS / 0 FAIL / 0 SKIP|5회 실행 이력, 마지막 targeted 단일27 PASS|
| PASS4G.3 |33 PASS / 0 FAIL / 0 SKIP|6회 실행 이력, 선택 회귀 단일14 PASS|
| 숫자 누락 수정 |13 PASS / 0 FAIL / 0 SKIP|최신 PASS 출처6개 실행, 초기 FAIL 실행은 별도 보존|

각 최신 케이스를 원본 XML fullname/result와 대조해 불일치0이다. G3 전체 이력6개 중 현재 최신 PASS의 출처는5개이며, 최초 실패 계측 실행도 이력에 남는다. 위 범위는 중복되고 단일104개 전체 실행이 아니다. 이번 저장에서는 기존 전체 회귀를 다시 실행하지 않았다.

숫자 누락은 콤보 token을 개별 거래 중복 키로 쓴 문제다. 실제 기본4타 HP4/결과1/생성1에서 수정 후4/4/4, 공중 부분5/5/5로 확인했다. 기존 피해·확률·Zone 값을 재튜닝하지 않았다. 정상 후속 흑섬의 보상 commit도 같은 식별자 결함에서 복구된다.

최초 실패 XML/trace/영상, 폰트 family 렌더 결함 수정 이력, Domain Reload 폰트 경고, Pipeline timeout5건을 보존한다. 마지막 누락 수정 시점의 컴파일 정상/Console error0·warning5(기존4F 테스트3+폰트2)는 과거 검증 기록이다. 이번 Git 저장에서 현재 Unity Console 상태를 새로 검사한 결과로 표현하지 않는다. PASS4A 과거 미해결18건은 여전히 별도 기록이다.

근거:
- `unity/Logs/ShibuyaPass4GQA/20261031-combat-grades/latest-unique-results.json`
- `unity/Logs/ShibuyaPass4G3QA/20261031-damage-display/latest-unique-results.json`
- `unity/Logs/ShibuyaPass4G3DeliveryQA/20261031-missing-numbers/latest-unique-results.json`

## 저장 파일 — 명시적 경로85개

기존 수정34개 + 신규51개다. 문서11 / 런타임23 / Editor18 / Data Asset11 / .meta22. 인수인계·잠금 문서와 기존 PASS 보고서에는 최신 사용자 승인 절만 추가하고 과거 미커밋/HEAD/FAIL 당시 기록을 남긴다.

| 구분 | 경로 |
|---|---|
|수정|`docs/CURRENT_HANDOFF.md`|
|신규|`docs/SHIBUYA_PASS4G0_REVIEW.md`|
|신규|`docs/SHIBUYA_PASS4G1_REVIEW.md`|
|신규|`docs/SHIBUYA_PASS4G2_REVIEW.md`|
|신규|`docs/SHIBUYA_PASS4G3_DELIVERY_REVIEW.md`|
|신규|`docs/SHIBUYA_PASS4G3_REVIEW.md`|
|신규|`docs/SHIBUYA_PASS4G_CHECKPOINT.md`|
|수정|`docs/active/CURRENT_TASK.md`|
|신규|`docs/design/COMBAT_ACTION_VOCABULARY.md`|
|수정|`docs/design/SHIBUYA_COMBAT_REBOOT.md`|
|수정|`docs/locked/USER_VERIFIED_SETTINGS.md`|
|수정|`unity/Assets/Scripts/Core/CursedEnergyController.cs`|
|수정|`unity/Assets/Scripts/Core/Health.cs`|
|수정|`unity/Assets/Shibuya/Data/PASS3A Knockback Approach.asset`|
|수정|`unity/Assets/Shibuya/Data/PASS3A Multi Hit.asset`|
|수정|`unity/Assets/Shibuya/Data/Shibuya Aerial Combat.asset`|
|신규|`unity/Assets/Shibuya/Data/Shibuya Attack Grades.asset`|
|신규|`unity/Assets/Shibuya/Data/Shibuya Attack Grades.asset.meta`|
|수정|`unity/Assets/Shibuya/Data/Shibuya Combat Profile.asset`|
|신규|`unity/Assets/Shibuya/Data/Shibuya Counter.asset`|
|신규|`unity/Assets/Shibuya/Data/Shibuya Counter.asset.meta`|
|수정|`unity/Assets/Shibuya/Data/Shibuya Cursed Energy.asset`|
|수정|`unity/Assets/Shibuya/Data/Shibuya Grab.asset`|
|수정|`unity/Assets/Shibuya/Data/Shibuya Ground Finishers.asset`|
|수정|`unity/Assets/Shibuya/Data/Shibuya Parry.asset`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaAttackClassificationMigration.cs`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaAttackClassificationMigration.cs.meta`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaAttackGradeInspector.cs`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaAttackGradeInspector.cs.meta`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaCombatGradeReview.cs`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaCombatGradeReview.cs.meta`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaCounterInspector.cs`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaCounterInspector.cs.meta`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaDamageNumberInspector.cs`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaDamageNumberInspector.cs.meta`|
|수정|`unity/Assets/Shibuya/Editor/ShibuyaGrabInspector.cs`|
|수정|`unity/Assets/Shibuya/Editor/ShibuyaInspectors.cs`|
|수정|`unity/Assets/Shibuya/Editor/ShibuyaMovesetInspector.cs`|
|수정|`unity/Assets/Shibuya/Editor/ShibuyaPass3BTests.cs`|
|수정|`unity/Assets/Shibuya/Editor/ShibuyaPass3C1Tests.cs`|
|수정|`unity/Assets/Shibuya/Editor/ShibuyaPass3CTestRunner.cs`|
|수정|`unity/Assets/Shibuya/Editor/ShibuyaPass4B3Tests.cs`|
|수정|`unity/Assets/Shibuya/Editor/ShibuyaPass4D1Tests.cs`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaPass4G0Tests.cs`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaPass4G0Tests.cs.meta`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaPass4G1Tests.cs`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaPass4G1Tests.cs.meta`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaPass4G2Tests.cs`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaPass4G2Tests.cs.meta`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaPass4G3DeliveryTests.cs`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaPass4G3DeliveryTests.cs.meta`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaPass4G3Tests.cs`|
|신규|`unity/Assets/Shibuya/Editor/ShibuyaPass4G3Tests.cs.meta`|
|신규|`unity/Assets/Shibuya/Resources/Shibuya Damage Numbers.asset`|
|신규|`unity/Assets/Shibuya/Resources/Shibuya Damage Numbers.asset.meta`|
|수정|`unity/Assets/Shibuya/Scripts/CursedEnergyAuraController.cs`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaAttackGradeProfile.cs`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaAttackGradeProfile.cs.meta`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaAttackGradeRuntime.cs`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaAttackGradeRuntime.cs.meta`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaAttackGrades.cs`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaAttackGrades.cs.meta`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaAttackMetadata.cs`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaAttackMetadata.cs.meta`|
|수정|`unity/Assets/Shibuya/Scripts/ShibuyaCinematicFighterRuntime.cs`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaCombatDamageResult.cs`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaCombatDamageResult.cs.meta`|
|수정|`unity/Assets/Shibuya/Scripts/ShibuyaCombatProfile.cs`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaCounterProfile.cs`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaCounterProfile.cs.meta`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaCounterRuntime.cs`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaCounterRuntime.cs.meta`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaDamageNumberProfile.cs`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaDamageNumberProfile.cs.meta`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaDamageNumbers.cs`|
|신규|`unity/Assets/Shibuya/Scripts/ShibuyaDamageNumbers.cs.meta`|
|수정|`unity/Assets/Shibuya/Scripts/ShibuyaEnergyCharge.cs`|
|수정|`unity/Assets/Shibuya/Scripts/ShibuyaFighter.cs`|
|수정|`unity/Assets/Shibuya/Scripts/ShibuyaGrabProfile.cs`|
|수정|`unity/Assets/Shibuya/Scripts/ShibuyaGrabRuntime.cs`|
|수정|`unity/Assets/Shibuya/Scripts/ShibuyaGuardRuntime.cs`|
|수정|`unity/Assets/Shibuya/Scripts/ShibuyaHitStateRuntime.cs`|
|수정|`unity/Assets/Shibuya/Scripts/ShibuyaParryRuntime.cs`|
|수정|`unity/Assets/Shibuya/Scripts/ShibuyaPlayerInput.cs`|
|수정|`unity/Assets/Shibuya/Scripts/ShibuyaTargetingRuntime.cs`|

## 제외·보존과 정적 점검

`unity/unity.slnx`의 기존 Assembly-CSharp / Editor 프로젝트 순서 변경은 기능 의존성이 없어 staging하지 않는다. 원래 로컬 바이트를 보존하며 임의 restore하지 않는다. QA 이미지·영상·XML·trace, `unity/Logs`, Library/Temp/UserSettings, LocalModels/LocalAudio, `_local_refs`와 레퍼런스·기타 ignored 캐시는 Git에 추가하지 않는다.

저장 전 기존 tracked/untracked1342개와 관련 QA642개의 SHA-256을 별도 기록했다. 저장 이후 비교로 기존 소스/설정/미디어의 보존을 확인한다. 기존 문서9개와 이 새 문서를 정리했고 소스·Data Asset 값은 수정하지 않았다. staged 검사에서 발견한 신규 자산 .meta3개의 빈 userData/assetBundleName/assetBundleVariant 뒤 공백9곳만 제거했다. GUID·키·값은 동일하며 원본 문자열은 로컬 감사 기록에 남긴다. GitHub에 올리는 것은 위 명시적 경로뿐이다.

신규22개 .meta의 대상 쌍, 후보 GUID 중복, Data Asset의 script/참조 GUID 해석을 점검했다. 누락 쌍0/후보 GUID 중복0/해석 불가 참조0. 자격증명 패턴 검사 발견0. 기존84개 변경 중 `.slnx`를 제외한83개가 구현 범위이며 잠금 문서와 이 체크포인트 문서를 추가해85개다. staged 목록·diff·크기·참조를 확인하고 기존 사용자 변경 혼입이 없을 때만 저장한다.

정적 검사·최종 staging·보존 증거는 로컬 전용 `unity/Logs/ShibuyaPass4GCheckpoint/20261031-user-verified/`에 기록한다. 이 디렉터리 자체는 커밋하지 않는다.

## 다음 작업 — 이번에 시작하지 않음

다음 후보는 흑섬 VFX 및 Zone 시각 피드백이다. 기능적 발동 기반은 저장하지만 연출 품질은 아직 최종 승인하지 않았다. 별도 기획/구현 승인 후 진행한다.

향후 특수 방어·술식 중화 공통 시스템은 논의 초안이다. 무하한 상시 차단의 피해/피격/경직 부재, 영역전연의 자격 패시브·선택 태세·술식 제한/주력 소모, 간이영역의 필중 대응, 신카게류 발도의 조건부 반응, 천역모·흑승의 장비 기반 무효화/교란을 검토한다. 확정 적중과 특수 방어 관통은 별개다. `대상 추적 → 적중 판정 → 특수 방어 → 실제 피해·피격·보상` 경계를 논의하되 확정 명세나 구현 완료로 취급하지 않는다.
