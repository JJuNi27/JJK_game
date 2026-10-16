# Purple Formation Ingredient — Macro-aggression 후보

2026-09-19 · **CODEX VALIDATED / PENDING USER VISUAL REVIEW**

기존 Ingredient 후보를 보존하고 별도 Macro 후보를 완성했다. 이번 재개 시점에는 Red 선두 보강과 최종 Unity 실행까지 완료되어 있었다. 해당 결과를 읽고 최종 영상 인코딩, 실제 렌더 비교, 보호 파일 확인, 보고서를 마무리했다. 재개 후 추가 visual 수정이나 Unity 재실행은 하지 않았다. Commit / push / merge 없음.

## 비교 자료

[영상 모음](../unity/Logs/PurpleIngredientMacroQA/Review/20260918_161849_981/index.html). **왼쪽은 기존 Ingredient Candidate, 오른쪽은 Macro Candidate**다. Production 원본과의 비교가 아니다.

- [Side 전체](../unity/Logs/PurpleIngredientMacroQA/Review/20260918_161849_981/Side_AB_Full.mp4)
- [Caster 전체](../unity/Logs/PurpleIngredientMacroQA/Review/20260918_161849_981/Caster_AB_Full.mp4)
- [Side 흑백 반속](../unity/Logs/PurpleIngredientMacroQA/Review/20260918_161849_981/Side_AB_Grayscale_HalfSpeed.mp4)
- [Caster 흑백 반속](../unity/Logs/PurpleIngredientMacroQA/Review/20260918_161849_981/Caster_AB_Grayscale_HalfSpeed.mp4)
- [Side 단계별 still](../unity/Logs/PurpleIngredientMacroQA/Review/20260918_161849_981/Side_Phase_Comparison.jpg), [Caster 단계별 still](../unity/Logs/PurpleIngredientMacroQA/Review/20260918_161849_981/Caster_Phase_Comparison.jpg)
- [Side 흑백 motion 연속 표본](../unity/Logs/PurpleIngredientMacroQA/Review/20260918_161849_981/Side_Motion_Grayscale_Samples.jpg), [Caster 흑백 motion 연속 표본](../unity/Logs/PurpleIngredientMacroQA/Review/20260918_161849_981/Caster_Motion_Grayscale_Samples.jpg)

실제 Unity URP 렌더다. 동일 조건의 각 시점 960×540 / 30fps 원본을 좌우 1920×540으로 조합했다. 전체 영상은 0~5.833초(formation, hold, release, travel 시작), 흑백 반속은 11.633초다. Side는 전체 외부 흐름을 담는 QA observer, Caster는 기존 production 시퀀스가 제어하는 카메라다. 밝은 회색 바닥과 어두운 상부 배경을 함께 사용했다. 모든 전투 배경에서의 검증을 의미하지 않는다.

## 변경 내용

- **Blue:** dark core 유지. 기존 curve/mote를 회전하면서 반경이 줄어드는 궤적으로 바꾸고, 선두를 굵게·꼬리를 가늘게 했다. 흐름 도달 반경 3.05 local units, 주 곡선 기준 폭 0.24, 속도 1.7 cycles/s. 긴 잔상이 흡인 방향을 보여준다. 구체 root의 크기나 경로는 바꾸지 않았다.
- **Red:** 짧은 외향 burst와 발생마다 달라지는 방향, 밝기 pulse를 사용한다. 첫 Macro preview보다 선두 폭을 0.25→0.30, 발광 계수를 2.2→2.5로 보강하고 선두에 밝은 핑크 성분을 추가했다. 코어 크기를 늘리는 수정은 하지 않았다. 곡선은 약 0.30초 동안 외향 이동하고 다음 발생 전 사라진다.
- **표면:** 별도 Macro shader에서 변형 강도 0.07→0.13, 더 빠른 surface flow, 제한적인 dark fissure를 사용한다. Blue의 어두운 흡인 중심과 Red의 밝은 압력 중심을 구분한다. Red의 넓은 백색 shoulder는 줄여 외부 분출과 표면 대비가 읽히도록 했다.
- **구조:** curve 10 / mote 20 / bridge 1을 그대로 재사용한다. Macro mote 궤적만 2→6개 점으로 확장한다. 후기 fusion에서는 흐름 보강을 blend out하고 기존 Birth compression 블록과 tension bridge를 그대로 사용한다. 별도 distortion 또는 global Bloom 변경은 없다.

`PurpleIngredientMacroProfile.asset`은 **기본 OFF**다. 기존 `PurpleIngredientPolishProfile.asset`도 저장값 OFF로 보존했다. 이 보고서와 같은 비교를 Play Mode에서 재현하려면 기존 Ingredient 후보를 켠 상태에서 Macro 후보를 OFF/ON하여 새로 시전한다. Macro가 켜지면 전용 shader/flow가 우선한다. 비교 렌더에서는 유지하기로 한 Outer/Birth 후보를 양쪽 모두 일시적으로 켰고 종료 후 원래 값을 복구했다. 저장 asset을 ON으로 바꾸지 않았다.

## 실제 화면 평가

**기존 후보보다 강한 비교 후보로 추천한다. 최종 채택은 사용자 시각 검토 대기다.**

Blue는 Side와 Caster 모두 외부 에너지의 범위와 굵기가 크게 늘었다. 연속 프레임에서 긴 선두가 휘면서 중심으로 들어가며, 기존의 가늘고 정돈된 장식선보다 흡인 동작이 읽힌다. Dark core도 유지된다. 다만 정지 프레임만 보면 일부 곡선은 여전히 공전 고리처럼 보일 수 있다.

Red는 Caster에서 어둡던 첫 Macro preview보다 선두가 뚜렷해졌다. 흑백 비교에서도 외곽의 밝은 분출이 확인된다. 코어 전체를 백색 bloom 덩어리로 키우지 않으면서 존재감을 확보했다. Blue의 길게 감기는 궤적과 Red의 짧고 불규칙한 외향 이동은 이전보다 구별된다. 다만 압력의 강도는 Blue의 흡인만큼 일관되게 읽히지는 않는다.

표면 대비와 불안정성이 늘었고 구체 정체성은 남는다. 대규모 검은 얼룩, 화염 상승, 먼지 폭풍으로 바뀌지는 않았다. 단, 화면이 복잡해진 비용은 있다. 두 재료가 접근할 때 외부 선이 겹치고, 일부 Red flare는 리본·작은 갈고리처럼 보인다. Caster에서 표면 breakup 자체는 작게 읽혀 개선의 대부분이 외부 흐름에서 온다.

남은 한계 Top 3:

1. 단일 프레임의 Blue 곡선에는 공전 인상이 남는다. 동작 구분은 영상으로 판단해야 한다.
2. Red의 일부 flare는 압력 파열보다 휘어진 리본처럼 보이며, 분출 사이에는 존재감이 낮아진다.
3. Fusion 중 두 재료의 흐름이 겹쳐 시각 밀도가 높아진다. 더 복잡한 전투 배경과 사용자 Play Mode에서 최종 허용 강도를 확인해야 한다.

## 검증 결과

최종 `purple-ingredient-macro-final-02`: **6 PASS / 0 FAIL**, C# compile error **0**, shader error **0**. ShaderUtil의 관련 shader 지원/컴파일 메시지 검사도 통과했다. Full regression은 실행하지 않았다. 초기 preview는 별도 3 PASS / 0 FAIL이며 최종 결과와 중복 집계하지 않는다.

최종 실행한 테스트:

- `PurpleIngredientMacroTests.CanonicalPoseAndBodyLock`
- `PurpleIngredientMacroTests.FormationComparison`
- `PurpleIngredientMacroTests.IngredientDirectionsAndRepeatedCleanup`
- `PurpleIngredientMacroTests.TravelTargetsRangeAndRepeatedCleanup`
- `AstraCombatVfxTests.PurpleHoldReleaseIsContinuousAndDamageUsesSameCorridor`
- `AstraCombatVfxTests.PurpleCameraCancellationAndResourceCleanup`

검사 내용:

- Canonical position/scale/forward **81개 값 차이 0**. Production/timing profile 동일.
- **2.0초 이후 Side/Caster 각각 115프레임 RGB 변경 픽셀 합계 0.** 압축 영상이 아닌 동일 실행의 RGB24 원본끼리 비교했다.
- Ingredient **6회 spawn / 12회 inward·outward 방향 검사**, material 증가 0, 개당 LineRenderer 31 유지.
- 실제 cast **4회**: 첫 target HP 45 / 뒤 target HP 100, max-range terminal, travel cancel, birth cancel, legacy terminal 유지. 종료 후 material/mesh 증가 0.
- 비교 렌더 dispose/cleanup 및 Caster 카메라 위치/FOV 복귀 통과.
- 영상 ffprobe 확인: 전체 175프레임, 흑백 반속 349프레임, 30fps. 인코딩 오류 없음.

원본 [Unity XML](../unity/Logs/AstraCombatQA/purple-ingredient-macro-final-02.xml), [Unity log](../unity/Logs/AstraCombatQA/purple-ingredient-macro-final-02.log), [lifecycle](../unity/Logs/PurpleIngredientMacroQA/lifecycle.json), [Ingredient cleanup](../unity/Logs/PurpleIngredientMacroQA/ingredient_lifecycle.json), [pose locks](../unity/Logs/PurpleIngredientMacroQA/locks.json), [픽셀 비교](../unity/Logs/PurpleIngredientMacroQA/Review/20260918_161849_981/comparison_metrics.json), [보존·오류 검사](../unity/Logs/PurpleIngredientMacroQA/preservation_and_validation.json).

## 보호 범위와 변경 파일

작업 시작 시점의 기존 tracked/untracked **700개 파일 SHA256**과 비교했다. 문서 정리 전 실제 변경된 기존 파일은 **`PurpleFusionIngredient.cs` 1개뿐**이다. 기존 Ingredient profile/shader/tests, standalone Blue/Red, Production Purple/Outer/Birth/Travel, gameplay/timing/camera/terminal 및 기존 Scene/ProjectSettings 파일은 해당 snapshot과 동일했다. 기존 dirty 변경을 원복하지 않았다. 기존 local-only art/reference/QA 결과에 쓰거나 삭제하지 않았다.

이번 패스의 runtime 변경:

- `unity/Assets/Scripts/Player/PurpleFusionIngredient.cs` — Macro opt-in hook, 기존 line 재사용.
- 신규 `unity/Assets/Scripts/Player/PurpleIngredientMacroFlow.cs`
- 신규 `unity/Assets/Scripts/Player/PurpleIngredientMacroProfile.cs`
- 신규 `unity/Assets/Resources/VFX/HollowPurpleIngredientMacro.shader`
- 신규 `unity/Assets/Resources/VFX/PurpleIngredientMacroProfile.asset`
- 신규 `unity/Assets/Editor/PurpleIngredientMacroProfileEditor.cs` — 한국어 Inspector.
- 신규 `unity/Assets/Editor/PurpleIngredientMacroTests.cs`
- 위 신규 Unity 파일들의 `.meta`.

문서: 이 보고서와 `CURRENT_HANDOFF.md`, `active/CURRENT_TASK.md`, `architecture/GOJO_VFX.md`. 로컬 QA: `unity/Logs/PurpleIngredientMacroQA/` 아래 원본/영상/검사 결과. 첫 Macro preview `20260918_161626_930` 및 기존 Ingredient QA도 보존했다.

성능은 renderer/material 개수가 동일하지만 무료 변경은 아니다. Ingredient당 line 제어점이 최대 392→472개이며 surface noise와 흐름 계산, 넓어진 투명 영역의 비용이 늘어난다. 이번 검증은 누수와 lifecycle 검증이며 CPU/GPU ms 또는 목표 기기 성능 측정을 대신하지 않는다.

현재 branch는 `feat/gojo-blue-screen-distortion`, HEAD는 `7f5c6979e893f3a79a291934403f7cc28cc232af` 유지. 기존 로컬 작업과 새 후보가 uncommitted 상태로 남는다. 후보를 Production 기본값으로 적용하지 않았다.
