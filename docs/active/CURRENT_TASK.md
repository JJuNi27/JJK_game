# 현재 작업 — Purple Ingredient Final2 technical checkpoint / 시각 검토 미완료

## Checkpoint 상태

**CODEX VALIDATED / USER VISUAL REVIEW INCOMPLETE / NOT USER VERIFIED**.

Final2와 그 선행 Purple Ingredient exploration chain을 재현 가능한 technical checkpoint로 보존한다. Final2는 최종 visual baseline이 아니다. Blue/Red dark mass, fusion trajectory ±0.08m, fusion/collision timing 및 production Purple regression protection은 유지한다.

Known issues: outer energy가 reference와 충분히 일치하지 않음, Red burst가 일부 화면에서 glowing mesh/plastic shard처럼 읽힘, surrounding energy가 reference의 violent/unstable cursed-energy motion보다 약함. Skills/MCP/Reference Breakdown 도입 후 재검토한다. 사용자 시각 승인 전 기본 적용이나 Production 반영은 하지 않는다.

작성 기준: **2026-09-20**

Branch: `feat/gojo-blue-screen-distortion`

Remote code checkpoint: `d2ee74e8c20da795c500971c393ac097b22a2e5a`

상태: **CODEX VALIDATED / PENDING USER VISUAL REVIEW**

최신 작업은 **Pass 2 최종 파열·충돌 후보**다. `PurpleIngredientFinalProfile` 기본 OFF. 기존 dark mass, ±0.08m trajectory, contact 약 1.444초와 2/60초 길이를 보존하고, 외곽 긴 ribbon만 짧은 불규칙 파열로, collision graphic만 강한 흑백 집중선으로 교체했다. 확정 Purple/standalone Blue·Red는 수정하지 않았다.

고유 targeted 7종 통과, 최종 flow 보정 후 2 PASS / 0 FAIL, C#/shader 0. Pose 81개 동일, 2초 이후 각 시점 115프레임 RGB 차이 0. [최종 보고서](../PURPLE_INGREDIENT_FINAL_REVIEW.md), [필수 6개 비교 영상](../../unity/Logs/PurpleIngredientFinalQA/Review/20260920_110915_481/index.html). 외곽 지속 발광 면적 감소와 일부 flat shard, Caster의 Red 방향성은 사용자 검토 항목이다. 과거 QA 로그 이름 충돌 예외도 보고서에 기록했다. 아래 완료 결과는 보존 이력이다.

최신 완료 결과는 **Direction Reboot Pass 2**다. Dark mass를 보존하고 외곽 broken energy 강화, Blue/Red fusion vertical arc를 ±0.08m로 정리, 첫 접촉 약 1.444초에 2/60초 국소 흑백 accent를 추가했다. 기존 후보/확정 Purple은 보존했고 새 profile 기본 OFF다.

재개 후 QA 기대값·상세 캡처 초기화만 수정했다. 최종 실행 5 PASS / 0 FAIL, 고유 관련 7종 유효 PASS, C#/shader 오류 0. 2초 이후 두 시점 각각 115프레임 RGB 차이 0. [최신 보고서](../PURPLE_INGREDIENT_REBOOT2_REVIEW.md), [필수 6개 비교 영상](../../unity/Logs/PurpleIngredientReboot2QA/Review/20260920_100823_323/index.html). 아래 Reboot/Macro 설명은 이전 이력이다.

최신 결과는 **Direction Reboot / Material-Language Rework**다. 사용자가 Red bright core 해석을 폐기하도록 명시하여, Blue/Red 모두 dark mass + rim/cracked energy로 전환한 별도 후보를 만들었다. 기존 baseline/Polish/Macro는 보존했고 `PurpleIngredientRebootProfile.asset` 기본 OFF다. Blue 수축 band / Red 팽창 pressure band와 보조 흐름을 사용한다. 확정 Purple 및 독립 Blue/Red는 그대로다.

[최신 Reboot 보고서](../PURPLE_INGREDIENT_REBOOT_REVIEW.md), [Macro/Reboot Side·Caster·흑백 반속 비교](../../unity/Logs/PurpleIngredientRebootQA/Review/20260919_114110_831/index.html). 고유 관련 targeted 6종 PASS, 최종 visual 변경 뒤 3종 재검사 PASS, C#/shader 오류 0. 두 시점 모두 2초 이후 115프레임 RGB 차이 0. 남은 고리 인상과 원거리 흐름 가독성을 사용자에게 확인받는다. 아래 Macro/Polish 설명은 보존 이력이다.

최신 완료 작업은 **Macro-aggression 2차 후보**다. 기존 Ingredient 후보를 보존하고 별도 `PurpleIngredientMacroProfile.asset` 기본 OFF로 저장했다. Blue의 굵은 회전 흡인과 Red의 밝은 외향 pressure 선두를 강화했다. 재개 시 마지막 보강과 최종 검증 완료를 확인했고 추가 visual 수정 없이 영상/보고서를 마무리했다.

최종 targeted **6 PASS / 0 FAIL**, C#/shader 오류 0, pose 81개 차이 0, 2초 이후 두 시점 각각 115프레임 RGB 차이 0. 반복 cast 및 Ingredient cleanup 통과. [최신 Macro 보고서](../PURPLE_INGREDIENT_MACRO_REVIEW.md), [필수 비교 영상 4개](../../unity/Logs/PurpleIngredientMacroQA/Review/20260918_161849_981/index.html). 기존 후보보다 존재감은 커졌으나 일부 리본형 flare/공전 인상과 fusion 밀도는 사용자 확인이 필요하다. 아래 Ingredient 기록은 이전 패스 이력이다.

보존된 Production baseline은 **D2-R3 Body + OuterR2 + Release→Travel refinement**이며 charge/travel에만 적용된다. Terminal explosion은 기존 Production visual을 유지한다. 최신 로컬 작업은 **Purple Formation Blue/Red Ingredient 후보**다. `PurpleIngredientPolishProfile.asset` 기본 OFF이며 새 시전에 적용된다. 기존 Outer/Birth는 `PurpleFinalPolishProfile.asset`에 그대로 보존했고 저장 toggle도 변경하지 않았다. Remote HEAD는 `7f5c6979e893f3a79a291934403f7cc28cc232af` 그대로이고 새 commit/push는 없다.

이전 사용자 결정: Outer Polish 및 Birth compression/flash/debris/electric accent 유지. Birth shell은 최대 0.11초의 broken partial shockwave로 수정되어 이번 Ingredient 작업에서 그대로 보존한다. 이전 shell [보고서](../PURPLE_BIRTH_SHELL_REFINEMENT_REVIEW.md)와 [영상](../../unity/Logs/PurpleBirthShellQA/Review/index.html)도 보존한다.

## 다음 순서

1. 최신 [최종 파열·충돌 보고서](../PURPLE_INGREDIENT_FINAL_REVIEW.md)와 [Pass 2 / 최종 비교 영상](../../unity/Logs/PurpleIngredientFinalQA/Review/20260920_110915_481/index.html)를 검수한다. 이전 후보/보고서도 보존한다.
2. Ribbon 감소, 짧은 파열의 밀도, 강화된 첫 접촉 graphic을 사용자에게 확인받는다. 확정된 본체·궤적·Body/Outer/Birth/Travel/gameplay/camera는 다시 작업하지 않는다.
3. Unity Play Mode에서 최종 visual을 확인한다. 사용자 승인 전 toggle 기본값을 ON으로 바꾸지 않는다.
4. 승인되면 Purple visual R&D를 종료한다.
5. 이후 사용자 선택에 따라 Blue distortion/particle/environment reaction, Red pressure/range/residual pressure, Domain visual/camera 중 다음 track으로 이동한다.

Purple 승인 전에는 새 visual track을 임의로 시작하지 않는다.

후속 메모: **Purple Outer Lightning-Storm Re-evaluation**은 별도 후보 작업이다. 현재 halo/aura의 부드러움·규칙적인 pulse를 레퍼런스와 비교하고 필요하면 broken lightning/irregular fragments/storm motion을 검토한다. 이번 Pass 2에서는 구현하지 않았으며 사용자 요청 전 착수하지 않는다.

## 검증 기준

- 최신 Ingredient 후보: 최종 targeted **5 PASS / 0 FAIL**, C#/shader 오류 0. Pose 81개 차이 0, 2초 이후 Side/Caster 115프레임 RGB 차이 0. `PurpleIngredientPolishProfile` 기본 OFF, 원본 Ingredient shader 보존. 아래는 이전 작업의 검증 이력이다.

- 9월 19일 shell-only: `PurpleFinalPolishTests.FullComparison` **1 PASS / 0 FAIL**, C#/shader 오류 0. Spawn/render/dispose/material cleanup/camera restore 확인. Gameplay와 timing source는 동일하므로 기존 regression 반복 없음.

- 최신 Polish 관련 targeted **6종 PASS / 0 FAIL**, C#/shader 오류 0. 초기 Outer preview 별도 1 PASS. Full regression 없음.
- 두 후보 ON lifecycle 4회, first/behind/max-range/birth cancel/travel cancel/legacy terminal, material/mesh 증가 0.
- canonical transform 81개 차이 0, Production/timing profile 변화 0. Outer hold 중심 11,000픽셀 차이 0. Birth OFF/ON은 2.3초 이후 전 프레임 차이 0.
- 세부 로그 및 비용/시각 한계는 최신 비교 보고서 참조. 아래는 보존된 baseline의 이전 검증 이력이다.

- Production integration: **3 PASS / 0 FAIL**, C#·shader error 0.
- Travel refinement: **2 PASS / 0 FAIL**, C#·shader error 0.
- Charge pixel difference 0.
- first target hit 종료, behind-target damage 없음, no-hit max-range terminal, cancel/cleanup/camera restore, repeated lifecycle cleanup 유지.
- Caster/Side readability 확인. 고정 observer는 장면 가림 때문에 시각 합격 근거에서 제외.
- 자동 테스트와 Codex 렌더 검토는 사용자 Play Mode 승인과 같지 않다.

## 보호 범위

- D2-R3 Body, OuterR2 Charge, gameplay 수치와 semantics, canonical timing, camera architecture, terminal explosion.
- `Purple_UserPrototype`, 사용자 Scene/Animator/FBX, LocalModels, LocalAudio, local reference, QA output.
- USER VERIFIED 설정은 `docs/locked/USER_VERIFIED_SETTINGS.md`를 따른다.

결과 문서:

- `docs/PURPLE_FINAL_POLISH_REVIEW.md`
- `docs/PURPLE_PRODUCTION_INTEGRATION_REVIEW.md`
- `docs/PURPLE_TRAVEL_REFINEMENT_REVIEW.md`
- `docs/PURPLE_D2_R3_REVIEW.md`
- `docs/PURPLE_D2_R3_OUTER_R2_REVIEW.md`

## Domain future queue

Barrier 크기의 Inspector/Data 조절, barrier destruction 보류, Gojo hand sign/blindfold lowering와 trapped opponent reaction cinematic camera 후보를 기록만 한다. Purple 승인 작업과 섞지 않는다.
