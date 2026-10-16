# Purple Ingredient — Pass 2 최종 파열·충돌 후보

2026-09-20. **CODEX VALIDATED / PENDING USER VISUAL REVIEW**.

**2026-09-21 재개 마무리:** 현재 diff와 최종 QA XML/JSON을 다시 읽고 필수 비교 영상 6개의 실제 존재·용량·MP4 메타데이터를 확인했다. 코드/셰이더/자산 수정, Unity 검증 재실행, 재렌더는 하지 않았다. [결과 파일 manifest](../unity/Logs/PurpleIngredientFinalQA/delivery_manifest_20260921.json)에 절대 경로·용량·SHA256·영상 길이를 기록했다. 아래 검증 수치는 9월 20일에 완료된 실행 결과다.

사용자 요청의 두 항목만 수정했다. 기존 Pass 2를 보존하고 `PurpleIngredientFinalProfile` 기본 OFF로 별도 선택할 수 있다. Commit/push/merge 및 production 기본 ON 변경은 없다.

## 결과 보기

[영상·스틸 모음](../unity/Logs/PurpleIngredientFinalQA/Review/20260920_110915_481/index.html). 왼쪽 Pass 2 / 오른쪽 최종 후보.

| 영상 | 내용 |
|---|---|
| [Side_AB_Full.mp4](../unity/Logs/PurpleIngredientFinalQA/Review/20260920_110915_481/Side_AB_Full.mp4) | Side 전체, 30fps |
| [Caster_AB_Full.mp4](../unity/Logs/PurpleIngredientFinalQA/Review/20260920_110915_481/Caster_AB_Full.mp4) | Caster 전체, 30fps |
| [Side_AB_Grayscale_HalfSpeed.mp4](../unity/Logs/PurpleIngredientFinalQA/Review/20260920_110915_481/Side_AB_Grayscale_HalfSpeed.mp4) | Side 흑백 반속 |
| [Caster_AB_Grayscale_HalfSpeed.mp4](../unity/Logs/PurpleIngredientFinalQA/Review/20260920_110915_481/Caster_AB_Grayscale_HalfSpeed.mp4) | Caster 흑백 반속 |
| [Ribbon_Close_Before_After.mp4](../unity/Logs/PurpleIngredientFinalQA/Review/20260920_110915_481/Ribbon_Close_Before_After.mp4) | 0.1–1.4초 외곽 파열 근접 비교 |
| [Fusion_ImpactFrame_HalfSpeed.mp4](../unity/Logs/PurpleIngredientFinalQA/Review/20260920_110915_481/Fusion_ImpactFrame_HalfSpeed.mp4) | 1.3–2.1초를 60Hz로 캡처한 반속 비교 |

[충돌 60Hz 연속 프레임](../unity/Logs/PurpleIngredientFinalQA/Review/20260920_110915_481/Collision_60Hz_Contact_Sheet.jpg), [Side 근접](../unity/Logs/PurpleIngredientFinalQA/Review/20260920_110915_481/Side_Core_Detail.jpg), [Caster 구간 비교](../unity/Logs/PurpleIngredientFinalQA/Review/20260920_110915_481/Caster_Phase_Comparison.jpg).

원본 RGB와 PNG: `unity/Logs/PurpleIngredientFinalQA/Final/20260920_110915_481/`. 전체 영상은 175프레임/5.833초, 충돌 원본은 49프레임/60Hz다. 전체 30fps 영상에는 2/60초 accent가 한 샘플만 잡힐 수 있으므로 두 프레임의 반전은 60Hz 상세 영상으로 확인한다. 이번 후보의 앞선 프리뷰 세 폴더도 남겨 두었다.

## 실제 원인과 변경

**리본:** Pass 2의 주 곡선은 약 72–109도 길이의 연결된 경로였고, 폭과 발광 가장자리가 연속적으로 유지됐다. Pressure2 shader의 tear에도 0.3의 불투명도 바닥값이 있어 중간이 완전히 끊어지지 않았다. 따라서 lifetime 하나의 문제가 아니었다.

새 `PurpleIngredientFinalFlow`는 기존 10개 curve를 재사용한다. 약 28–44도 길이, 꺾인 구간, 가는 끝, 서로 엇갈린 약 0.20–0.27초 이벤트로 바꿨다. 새 rupture shader는 완전히 투명해지는 절단부를 사용하고 긴 발광 테두리를 제거했다. Blue는 회전하며 안쪽으로, Red는 빠르게 바깥으로 이동한다. 짧은 곡선이 불투명 본체 뒤에만 머무르지 않도록 반경 진행 분포를 조정했다. 이것은 외곽 line의 움직임이며 Ingredient 두 본체의 fusion trajectory 수정이 아니다. Mote의 기존 위치·시간 로직은 유지하고 공유 pressure shader만 후보 셰이더를 사용한다. Curve 10 + mote 20 + bridge 1, 개수 증가는 없다.

첫 렌더는 지나치게 미세한 조각처럼 보여 분절 길이·발광·알파를 보정했다. 마지막 보정은 외곽 파열의 가림 비중 감소였으며, 완성 Purple이나 본체 shader는 손대지 않았다.

**충돌:** 부드러운 원형 alpha 경계와 가는 집중선을, 불투명 흑백 방사형 파열·굵은 wedge·중심점으로 교체했다. 다음 nominal frame은 명암을 반전한다. 기존 world-local cube의 shader만 선택하며 Canvas, 카메라 lease, Volume 변경은 없다. 접촉 **1.444183초**, 길이 **2/60초** 그대로다. 약 1.4775초에 종료하고 기존 Birth accent 시작 1.945초와 겹치지 않는다.

## 화면 기준 평가

| 항목 | 관찰 |
|---|---|
| Ribbon/Cable | 본체를 크게 두르는 긴 연속선이 사라져 공전 고리·전선 인상이 크게 줄었다. 모든 납작한 조각의 인상까지 제거됐다고 보지는 않는다. |
| 파열 | Side 연속 프레임에서 짧은 각진 조각들이 다른 위치에 나타나고 사라진다. 개수를 늘린 난사는 아니다. Blue의 일부는 굽은 번개 조각으로, Red의 일부는 얇은 에너지 파편으로 읽힌다. |
| 방향성 | inward/outward의 반경 이동은 검사로 유지 확인. 근접 연속 화면에서 구분되지만, Caster 거리·흑백에서는 특히 Red의 척력이 명확히 읽히는지 사용자 확인이 필요하다. |
| 충돌 | 1.4500 / 1.4667초 두 프레임에서 굵은 흑백 파열이 즉시 구분된다. 반투명 원형 UI 인상은 줄었고 중심이 뚜렷하다. 의도적으로 그래픽적인 평면 악센트이며 물리적인 3D 폭발은 아니다. |
| 궤적 | 본체 transform 81개 샘플 차이 0. 기존 ±0.08m 대칭 접근 유지. |
| 확정 Purple | 2초 이후 두 시점 각각 115프레임에서 원본 RGB 변경 픽셀 0. Body/Outer/Birth/Travel의 비교 화면 동일. |

**Trade-off / 남은 한계:** 연속 발광 면적이 줄어 Pass 2보다 외곽 아우라의 지속적인 존재감은 약해졌다. 일부 짧은 조각은 번개보다 평면적인 shard로 읽힌다. 충돌의 강도는 개선됐지만 의도적으로 강한 흑백 starburst이므로 실제 플레이에서의 취향·가독성 판단은 사용자에게 남긴다. 따라서 기술 검증 완료 후 후보로 제공하며 시각 승인이나 production 기본 적용으로 간주하지 않는다.

## 검증

분리된 `unity/Logs/PolishQA/Project`에서 targeted만 실행했다. Full regression 없음.

| 실행 로그 (AstraCombatQA) | PASS / FAIL | 목적 |
|---|---:|---|
| `purple-ingredient-final-preview-01` | 5 / 0 | 첫 비교, contact/pose/lifecycle/방향 |
| `purple-ingredient-final-02` (2026-09-20 실행) | 4 / 0 | 파열 길이·명도 보정, 기존 피해/카메라 regression |
| `purple-ingredient-rupture-20260920-final03` | 2 / 0 | 발광·알파 변경 후 비교/cleanup |
| `purple-ingredient-rupture-20260920-final04` | **2 / 0** | 최종 반경 진행 보정 후 비교/방향/cleanup |

이번 작업의 **고유 7종 PASS**, 모든 실행 C# error **0**, shader error **0**. 마지막 수정은 Flow의 반경 진행 함수만이므로 contact/pose/gameplay 전체를 반복하지 않았다.

- `PurpleIngredientFinalTests.FormationComparison`
- `PurpleIngredientFinalTests.IngredientDirectionsAndRepeatedCleanup`
- `PurpleIngredientFinalTests.FusionTrajectoryAndBodyLock`
- `PurpleIngredientFinalTests.CollisionContactWindowAndCleanup`
- `PurpleIngredientFinalTests.TravelTargetsRangeAndRepeatedCleanup`
- `AstraCombatVfxTests.PurpleHoldReleaseIsContinuousAndDamageUsesSameCorridor`
- `AstraCombatVfxTests.PurpleCameraCancellationAndResourceCleanup`

검증 근거: [summary](../unity/Logs/PurpleIngredientFinalQA/validation_summary.json), [pixel diff](../unity/Logs/PurpleIngredientFinalQA/Review/20260920_110915_481/comparison_metrics.json), [pose lock](../unity/Logs/PurpleIngredientFinalQA/locks.json), [contact](../unity/Logs/PurpleIngredientFinalQA/collision.json), [lifecycle](../unity/Logs/PurpleIngredientFinalQA/lifecycle.json), [ingredient cleanup](../unity/Logs/PurpleIngredientFinalQA/ingredient_lifecycle.json), [video metadata](../unity/Logs/PurpleIngredientFinalQA/Review/20260920_110915_481/video_validation.json).

81 pose 샘플 차이 0, 3회 접촉 중 취소 material 증가 0, 6회 Ingredient spawn/dispose material 증가 0, 반복 cast 4회 material/production mesh 증가 0. First target HP45, behind HP100, max range/charge·travel 취소/legacy terminal 정상. 카메라 restore 회귀 검사 통과. 새 런타임 Mesh 생성 코드는 없다. GPU 성능 벤치마크를 새로 했다는 의미는 아니다.

## 보호 및 변경 파일

시작 시 745개 tracked/untracked 파일 SHA256을 기록했다. 기존 파일의 허용 변경은 아래 runtime 2개와 handoff 문서 3개뿐이다. 나머지 740개 동일, 삭제 0, 예상 밖 변경 0. [보존 검사](../unity/Logs/PurpleIngredientFinalQA/final_preservation.json), [git status -sb](../unity/Logs/PurpleIngredientFinalQA/git-status-final.txt).

기존 수정:

- `unity/Assets/Scripts/Player/PurpleFusionIngredient.cs`: Pass 2일 때만 최종 flow/pressure 선택.
- `unity/Assets/Scripts/Player/PurpleIngredientCollisionAccent.cs`: 최종 collision shader 선택만 추가. 시점·길이·소유권 그대로.
- `docs/CURRENT_HANDOFF.md`, `docs/active/CURRENT_TASK.md`, `docs/architecture/GOJO_VFX.md`.

신규 (각 Unity 파일의 `.meta` 포함):

- `unity/Assets/Scripts/Player/PurpleIngredientFinalProfile.cs`
- `unity/Assets/Scripts/Player/PurpleIngredientFinalFlow.cs`
- `unity/Assets/Resources/VFX/PurpleIngredientFinalProfile.asset`
- `unity/Assets/Resources/VFX/HollowPurpleIngredientRupture.shader`
- `unity/Assets/Resources/VFX/PurpleIngredientCollisionFinal.shader`
- `unity/Assets/Editor/PurpleIngredientFinalProfileEditor.cs` — 한국어 toggle/설명, SerializedProperty/Undo 유지.
- `unity/Assets/Editor/PurpleIngredientFinalTests.cs`
- 이 보고서 및 ignored `unity/Logs/PurpleIngredientFinalQA` 결과/패키징 스크립트.

`PrototypeHollowPurplePresentationRuntime.cs`, 기존 Reboot/Pass 2 shader·flow·profile, 모든 Purple Body/Outer/Birth/Travel·gameplay·camera·terminal 파일, standalone Blue/Red, 사용자 Scene/ProjectSettings/기존 로컬 art는 시작 snapshot과 동일하다. 기존 dirty 변경은 그대로 남겼다.

**QA 로그 보존 예외:** 실행 이름 `purple-ingredient-final-02`가 9월 18일 Ingredient Polish 실행과 중복된 것을 뒤늦게 확인했다. 해당 과거 `.log/.xml` 한 쌍은 이번 9월 20일 결과로 덮어써졌다. 원본 로그를 복구했다고 주장하지 않는다. 이전 [검증 요약](../unity/Logs/PurpleIngredientPolishQA/validation_summary.json)과 영상·이미지·보고서는 보존됐지만, 이전 보고서의 해당 로그 링크는 이제 이전 실행의 증거로 사용할 수 없다. 이후 실행은 날짜와 고유 suffix를 사용했다. 위 745개 보존 검사는 ignored QA 로그까지 포함하는 검사가 아니다.

## 후보 사용 / 다음 단계

Pass 2와 새 `PurpleIngredientFinalProfile`의 후보 toggle을 함께 켜면 새 시전부터 적용된다. 새 toggle OFF는 기존 Pass 2다. QA에서는 양쪽 모두 Polish/Macro/Reboot/Pass 2 및 Outer/Birth를 같은 값으로 ON하고 Final만 전환했다. 실제 저장된 기존 toggle 값과 새 OFF 기본값은 보존했다.

Branch `feat/gojo-blue-screen-distortion`, HEAD `7f5c6979e893f3a79a291934403f7cc28cc232af` 유지, staged 없음. Commit/push/merge/reset/clean/restore 실행 없음. 다음은 필수 6개 영상에 대한 **사용자 visual review**다.
