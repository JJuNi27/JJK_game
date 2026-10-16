# Hollow Purple — Blue / Red Ingredient Polish

2026-09-19 · **CODEX VALIDATED / PENDING USER VISUAL REVIEW**

오직 `PurpleFusionIngredient`가 만드는 두 재료 구체를 수정하는 별도 후보다. 독립 기술 Blue/Red, Purple D2-R3 Body, Outer, Birth, Travel, gameplay, timing/path/scale/hold/release/camera/terminal은 보존했다. 후보 저장 기본값은 OFF다. commit/push/merge/staging 없음.

## 실제 구현 확인과 변경

기존에는 각각 10개 curve, 20개 mote, tension bridge 1개가 있었다. Blue는 안으로 모이고 Red는 바깥으로 퍼지는 방향이 이미 구현되어 있었으며, shader도 Blue dark core와 Red white core를 구분하고 있었다. 기존 렌더에서는 구형 표면의 넓은 밝은 반점이 흐름보다 먼저 보였고, Red core의 좁은 smoothstep 경계가 작은 흰 원처럼 읽혔다.

원본 `HollowPurpleIngredient.shader`는 그대로 두고 이를 바탕으로 `HollowPurpleIngredientPolish.shader`를 만들었다. 새 Profile이 켜진 경우에만 Ingredient가 후보 shader/flow를 선택한다.

| 대상 | 후보 변경 |
|---|---|
| Surface / silhouette | 2개 주파수의 천천히 움직이는 noise로 작은 radial vertex displacement. Profile amplitude 0.07은 최대 ±7% 반경 범위이며 실제 noise 값에 따라 더 작다. Root와 자식 sphere scale은 그대로다. |
| 표면 에너지 | 넓은 반점 기여를 줄이고 더 가는 밝은 에너지 결과 끊어진 rim을 강조. 거대한 chunky blob/털/꽃 형태는 추가하지 않음. |
| Blue core | 어두운 흡인 중심 유지. 부드러운 dark sink 전이로 주변 발광과 대비. White core로 바꾸지 않음. |
| Blue flow | 기존 곡선의 안쪽 선두를 밝히고 바깥 꼬리는 얇게 처리. 곡선 span을 줄이고 안쪽 이동을 강화. 기존 mote의 inward 방향 유지. |
| Red core | 좁은 hard replacement 대신 additive pressure light와 넓은 red shoulder. 중심의 밝기는 유지하면서 주변 에너지와 연결. |
| Red flow | 기존 curve가 짧은 outward burst로 이동하고 소멸. 제한된 angular span, 두꺼운 선두/얇은 꼬리, 불규칙한 밝기 변화. Mote의 outward 방향 유지. |

재료 구체 각각의 renderer 수는 그대로 **curve 10 + mote 20 + bridge 1 + surface 1**이다. 새 인력/척력 시스템, 새 particle system, 추가 light, global Bloom/Volume 변경 없음.

Birth compression에 진입하기 전 fusion 0.55~0.72 구간에서 flow 보강을 원래 값으로 부드럽게 돌린다. 기존 late-fusion compression block과 tension bridge는 수정하지 않았다. Ingredient shader는 재료 구체에만 적용되며 completed Purple에는 사용되지 않는다.

## 실제 화면 평가

**Red의 개선이 가장 명확하다.** 기존 흰 원의 잘린 경계가 부드러운 고압 광원으로 바뀌고, 바깥쪽 짧은 flare/mote가 더 쉽게 보인다. 단순히 전역 bloom을 올린 결과가 아니다. 작은 실제 Caster 화면에서는 여전히 밝은 중심점으로 압축되어 보일 수 있다.

**Blue는 어두운 중심과 가는 밝은 결의 대비가 좋아졌다.** 넓은 밝은 반점이 줄어 이전보다 얼음/행성 표면 인상이 약해졌고, 가까운 Side에서는 짧은 streak가 안으로 모이는 흐름을 확인할 수 있다. 여러 curve가 겹치는 순간에는 여전히 공전하는 선처럼 읽히는 부분이 있어 완벽한 흡인 표현이라고 주장하지 않는다.

그레이스케일 연속 샘플에서도 Blue의 길게 휘어 들어오는 흐름과 Red의 짧은 바깥 burst는 어느 정도 구분된다. 다만 실제 Caster의 작고 짧은 formation 구간에서는 근접/반속 영상보다 구별이 약하다. 색상 없이도 즉시 완전히 구별된다고 평가하지 않는다.

Silhouette는 구체 정체성을 유지하는 미세한 비대칭이다. 가까이 보면 윤곽 변화가 보이지만 Caster에서는 매우 미세하다. 형태를 강하게 깨기보다 energy pattern/core/flow를 함께 개선하는 쪽을 선택했다. 현재 후보는 baseline보다 검토할 가치가 있으며 최종 시각 승인은 사용자에게 남긴다.

## 비교 영상 / still

- [전체 비교 페이지](../unity/Logs/PurpleIngredientPolishQA/Review/20260918_154439_532/index.html)
- [Close Side — formation A/B](../unity/Logs/PurpleIngredientPolishQA/Review/20260918_154439_532/Side_AB_Formation.mp4)
- [실제 Caster — formation A/B](../unity/Logs/PurpleIngredientPolishQA/Review/20260918_154439_532/Caster_AB_Formation.mp4)
- [Close Side — 흑백 반속](../unity/Logs/PurpleIngredientPolishQA/Review/20260918_154439_532/Side_AB_Grayscale_HalfSpeed.mp4)
- [Caster — 흑백 반속](../unity/Logs/PurpleIngredientPolishQA/Review/20260918_154439_532/Caster_AB_Grayscale_HalfSpeed.mp4)
- [Close Side 단계 still](../unity/Logs/PurpleIngredientPolishQA/Review/20260918_154439_532/Side_Phase_Comparison.jpg), [Caster 단계 still](../unity/Logs/PurpleIngredientPolishQA/Review/20260918_154439_532/Caster_Phase_Comparison.jpg)
- 같은 폴더의 `Side_AB_Full.mp4`, `Caster_AB_Full.mp4`는 0~5.83초 전체 시퀀스로 Birth/hold/release/초기 travel의 연속성을 포함한다.

왼쪽 Baseline / 오른쪽 Candidate. Formation 영상은 0~2.2초, 30fps. Still 시점은 Blue 단독 0.333초 / 두 구체 0.833초 / fusion 직전 1.067초 / fusion 중 1.433초다. 두 후보 모두 보존된 Outer Polish와 최종 Birth Accent를 QA에서 ON으로 고정했다. Source Profile의 저장 toggle은 변경하지 않았다.

Close Side는 비교 전용 카메라 FOV40이다. 실제 Caster presentation camera는 기존 코드/설정 그대로다. 같은 stage/background/post process/clock/카메라로 비교했다. 흑백 반속 영상은 렌더 후 검토용 변환이며 게임 shader/camera 변경이 아니다.

원본: `unity/Logs/PurpleIngredientPolishQA/Final/20260918_154439_532/`. 첫 후보 렌더 `20260918_154117_113`도 보존했다. 최종본은 위 링크 기준이다.

## 검증 / 보존

최종 build targeted **5 PASS / 0 FAIL**, C# error **0**, shader error **0**:

- `PurpleIngredientPolishTests.CanonicalPoseAndBodyLock`
- `PurpleIngredientPolishTests.FormationComparison`
- `PurpleIngredientPolishTests.IngredientDirectionsAndRepeatedCleanup`
- `AstraCombatVfxTests.PurpleHoldReleaseIsContinuousAndDamageUsesSameCorridor`
- `AstraCombatVfxTests.PurpleCameraCancellationAndResourceCleanup`

첫 후보에서 위 3개 Ingredient 검사와 `TravelTargetsRangeAndRepeatedCleanup`도 통과했다. 고유 테스트 6종, 전체 실행 기록 9 PASS / 0 FAIL이다. 이후 변경은 표면/flow 전이와 QA close framing이었으므로 4회 gameplay lifecycle 검사를 반복하지 않았다. Full regression은 실행하지 않았다.

- canonical position/scale/forward **81개 값 차이 0**, Production/timing Profile 값 변화 0.
- 같은 실행의 Baseline/Candidate **2초부터 115프레임 전체 RGB 차이 0**, Side/Caster 모두 확인. Birth/hold/release/travel 화면 보존 근거다.
- Ingredient 단독 생성/파괴 6회, Blue inward / Red outward의 curve/mote 방향 12회 확인, owned material 증가 0.
- 첫 후보 gameplay lifecycle 4회: first target HP45 / behind HP100, max-range terminal, travel cancel, birth-time cancel, legacy terminal, material/mesh 증가 0.
- 최종 full render 종료 시 owned material 0, camera position/FOV 복구.
- 작업 전 SHA-256 기준으로 기존 runtime 변경은 `PurpleFusionIngredient.cs` 하나다. 원본 Ingredient shader, standalone Blue/Red 및 확정된 Purple 자산은 동일하다. 관련 없는 dirty/local art/Scene/ProjectSettings 보존.

로그: `unity/Logs/AstraCombatQA/purple-ingredient-polish-01.*`, `purple-ingredient-final-02.*`.
증거: `unity/Logs/PurpleIngredientPolishQA/locks.json`, `ingredient_lifecycle.json`, `lifecycle.json`, `preservation_final.json`, 최종 Review의 `comparison_metrics.json`.

## 비용 / 설정 / 변경 파일

추가 runtime material/mesh/renderer 수는 0. 재료 구체별 surface/filament material 2개를 그대로 사용한다. 후보 vertex shader에 noise 2회, fragment noise는 기존 3회에서 5회로 증가한다. Flow 후보 구간은 기존 curve 계산 뒤 추가 위치 보정과 SetPositions를 수행하므로 CPU 계산량도 소폭 증가한다. 이는 formation 동안만 활성인 두 작은 구체의 비용이며, GPU ms/저사양 기기 성능을 측정한 결과는 아니다.

새 `PurpleIngredientPolishProfile.asset`은 기본 OFF이고 한국어 Inspector로 제공한다. 설정: silhouette 0.07, Blue width ×1.8 / emission ×1.6, Red width ×1.65 / emission ×1.5, mote length 0.34(기존 0.12). 새 시전에 적용된다. Fusion timing이나 Ingredient scale을 바꾸는 설정은 없다.

실제 runtime 수정:

- `unity/Assets/Scripts/Player/PurpleFusionIngredient.cs`

신규 Unity 파일 및 각각 `.meta`:

- `unity/Assets/Scripts/Player/PurpleIngredientPolishProfile.cs`
- `unity/Assets/Resources/VFX/PurpleIngredientPolishProfile.asset`
- `unity/Assets/Resources/VFX/HollowPurpleIngredientPolish.shader`
- `unity/Assets/Editor/PurpleIngredientPolishProfileEditor.cs`
- `unity/Assets/Editor/PurpleIngredientPolishTests.cs`

문서: 이 보고서, `CURRENT_HANDOFF.md`, `active/CURRENT_TASK.md`, `architecture/GOJO_VFX.md`. QA 자료는 `unity/Logs/` local-only.
