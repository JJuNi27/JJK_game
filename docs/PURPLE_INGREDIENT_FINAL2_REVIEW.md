# Purple Ingredient Final 2 — 공간 반전 / 외곽 벌크업

2026-09-21 KST. **CODEX VALIDATED / USER VISUAL REVIEW INCOMPLETE / NOT USER VERIFIED**.

이 문서는 최종 visual baseline이 아니라 재현 가능한 technical checkpoint다. Blue/Red dark mass 방향, fusion trajectory ±0.08m, fusion/collision timing과 production Purple regression protection을 보존한다. Outer energy는 reference와 아직 충분히 일치하지 않으며, Red burst가 일부 화면에서 glowing mesh/plastic shard처럼 읽히고 surrounding energy가 reference의 violent/unstable cursed-energy motion에 미치지 않는 한계가 남아 있다. Skills/MCP/Reference Breakdown 도입 후 시각 재검토한다.

이전 Final을 보존하고 요청한 두 항목만 별도 후보로 교체했다. 새 `PurpleIngredientFinal2Profile` 기본 OFF. 새로운 본체 방향, gameplay 또는 camera 변경은 없다.

## 비교 결과

[영상·스틸 모음](../unity/Logs/PurpleIngredientFinal2QA/Review/20260920_175153_277/index.html). 왼쪽 **Previous Final**, 오른쪽 **New Final 2**.

| 실제 파일 | 길이 / 형식 |
|---|---|
| [Side_AB_Full.mp4](../unity/Logs/PurpleIngredientFinal2QA/Review/20260920_175153_277/Side_AB_Full.mp4) | 5.833초, 1920×540, 30fps |
| [Caster_AB_Full.mp4](../unity/Logs/PurpleIngredientFinal2QA/Review/20260920_175153_277/Caster_AB_Full.mp4) | 5.833초, 1920×540, 30fps |
| [Ribbon_Close_Before_After.mp4](../unity/Logs/PurpleIngredientFinal2QA/Review/20260920_175153_277/Ribbon_Close_Before_After.mp4) | 1.3초, 1400×370, 30fps; 원본 0.1–1.4초 |
| [Fusion_ImpactFrame_HalfSpeed.mp4](../unity/Logs/PurpleIngredientFinal2QA/Review/20260920_175153_277/Fusion_ImpactFrame_HalfSpeed.mp4) | 1.617초, 1400×370, 60fps 반속; 원본 1.3–2.1초 |

[근접 비교](../unity/Logs/PurpleIngredientFinal2QA/Review/20260920_175153_277/Side_Core_Detail.jpg), [Caster 비교](../unity/Logs/PurpleIngredientFinal2QA/Review/20260920_175153_277/Caster_Phase_Comparison.jpg), [충돌 60Hz 연속 프레임](../unity/Logs/PurpleIngredientFinal2QA/Review/20260920_175153_277/Collision_60Hz_Contact_Sheet.jpg), [흑백 동작 샘플](../unity/Logs/PurpleIngredientFinal2QA/Review/20260920_175153_277/Side_Motion_Grayscale_Samples.jpg).

실제 파일 존재와 ffprobe 메타데이터를 확인했다. 원본 RGB/PNG는 `unity/Logs/PurpleIngredientFinal2QA/Final/20260920_175153_277/`에 있다. 폴더 timestamp는 UTC다. 30fps 전체 영상은 2/60초 accent 중 한 프레임만 샘플할 수 있으므로 반전의 두 단계는 60Hz 상세 영상으로 확인한다.

## 변경 1 — 충돌 그래픽 교체

새 후보에서는 대칭 starburst shader를 사용하지 않는다. 기존 world-local collision volume에 `PurpleIngredientCollisionTear`를 선택한다. 이미 켜져 있는 URP Opaque Texture에서 실제 구체·배경을 읽고, 비대칭 행 단위 변위와 고대비 negative exposure를 적용한다. 그 위에 별이나 집중선 그림을 얹지 않는다. 첫 프레임과 다음 프레임의 명암이 반전되며, 영역 밖은 유지된다. 초기 중앙 장식선도 제거했다.

첫 접촉 **1.444183초**, 길이 **2/60초**, 종료 약 **1.4775초** 그대로. 기존 Birth accent는 1.945초에 시작하며 서로 겹치지 않는다. 별도 post-process stack/RendererFeature/Canvas/camera lease를 만들지 않았고 ProjectSettings/URP asset도 변경하지 않았다.

실제 화면에서 두 구체의 표면과 윤곽이 찢어진 영역 안에서 반전되는 것을 확인했다. 이전의 독립된 별 스티커는 사라졌다. 첫 프레임은 국소적으로 매우 밝고, 다음 프레임은 검은 질량과 흰 표면 흔적으로 돌아온다. 긴 full-screen white-out은 아니다.

## 변경 2 — 외곽 에너지 벌크업

재료당 최대 3개, 서로 엇갈린 짧은 burst를 사용한다. 이전의 작은 curve/mote 조각들은 초기 구간에서 숨기고 후기 fusion에서 기존 표현으로 복귀한다. 기본적인 동시 이벤트 수는 2~3개이며, 본체 가림/소멸 단계에서는 화면에 보이는 수가 줄 수 있다.

`PurpleIngredientBurstVolume`의 실제 두께가 있는 작은 mesh로 짧은 꺾임·가는 끝·짧은 분기를 구성했다. 이벤트마다 방향/굵기/분기가 바뀌며 약 0.23–0.29초 내에서 진행된다. Blue는 회전 성분과 함께 중심으로 수축하고, Red는 시작부터 빠르게 바깥으로 이동한다. 긴 연결 ring/cable 또는 수많은 emitter를 추가하지 않았다. 좁은 밝은 하이라이트와 어두운 측면을 사용해 단색 판 같은 인상을 줄였다.

초기 프리뷰에서 발광을 높여도 평평하고 어두웠던 원인은 새 fade 코드의 `Mathf.SmoothStep` 인자 사용이었다. 셰이더의 edge-based `smoothstep`과 달리 Unity API는 시작값·끝값·보간율이므로, 올바른 `InverseLerp`로 수정했다. 중간 이벤트 평균 vertex alpha는 약 **0.946**으로 확인했고, 알파 >0.8 검사를 추가했다. 이전 Final 후보의 코드/재질은 수정하지 않았다.

화면상 작은 confetti의 수가 줄고, Caster에서도 굵은 선두와 덩어리가 더 잘 읽힌다. 연속 프레임에서 Blue의 수축과 Red의 외향 이동을 확인했다. 본체를 길게 감싸는 리본으로 돌아가지 않았다.

## 남은 한계

- 아주 가까이 정지하면 일부 아크의 각진 mesh/짧은 막대 인상이 남는다. 길게 고정되는 cable은 줄었지만 유기적인 번개와 완전히 같다고 보지는 않는다.
- 가림이나 이벤트 교체 순간에는 Blue가 특히 한 덩어리만 뚜렷하게 보일 수 있다. 색을 제거한 먼 시점에서의 힘 방향은 사용자 영상 검토가 필요하다.
- 충돌은 실제 장면을 이용하지만 여전히 국소 화면 굴절/반전이다. 첫 negative frame의 밝은 면적과 torn boundary가 실제 플레이에서 자연스러운지는 사용자 판단을 기다린다.

기술 검증과 비교용 후보 제공까지 완료했다. 주관적 시각 승인을 확정하지 않는다.

## 검증 결과

최종 실행 `ingredient-final2-20260921-final08`: **1 PASS / 0 FAIL**, `PurpleIngredientFinal2Tests.FormationComparison`. 마지막 수정은 아크 shader의 좁은 하이라이트 하나였으므로 전체 regression은 반복하지 않았다. 최종 실행 포함 모든 실행에서 **C# error 0 / shader error 0**.

이번 작업의 관련 고유 **8종 유효 PASS**:

- `FormationComparison`
- `FusionTrajectoryAndBodyLock`
- `CollisionContactWindowAndCleanup`
- `IngredientDirectionsAndRepeatedCleanup`
- `TravelTargetsRangeAndRepeatedCleanup`
- `BurstVisualPreview`
- 기존 `AstraCombatVfxTests.PurpleHoldReleaseIsContinuousAndDamageUsesSameCorridor`
- 기존 `AstraCombatVfxTests.PurpleCameraCancellationAndResourceCleanup`

단, 초기 `probe03`은 비활성 mesh의 빈 진단 배열을 읽는 QA 도구 오류로 **0 PASS / 1 FAIL**이었다. 빈 배열 처리를 고친 뒤 `probe04/05` 각 **2/0**, `probe07` **1/0**으로 재확인했다. 이 실패를 누락하거나 모든 과거 실행이 성공했다고 표현하지 않는다. `01`은 5/0, `02`는 4/0, `final06`은 2/0이었다. 모두 날짜와 고유 suffix로 실행하여 과거 QA 로그를 덮어쓰지 않았다.

| 보호/자원 검사 | 결과 |
|---|---|
| Canonical position/scale/forward | 81개 샘플 차이 0 |
| Production / timing profile | 동일 |
| 첫 접촉/길이 | 1.444183초 / nominal 60Hz 2프레임 |
| 2초 이후 Purple-only RGB | Side 115프레임, Caster 115프레임 모두 변경 픽셀 0 |
| Ingredient 반복 spawn/dispose | 6회, material 증가 0, 새 burst mesh 증가 0 |
| 아크 방향/두께/알파 | Blue inward, Red outward, bounds 깊이 및 중간 알파 검사 PASS |
| Repeated cast | 4회, material/production·burst mesh 증가 0 |
| 피해/종료 | 첫 target HP45, 뒤 target HP100, max-range, 기존 terminal 유지 |
| Cancel / camera | Birth/Travel cancel 및 기존 camera restore regression PASS |
| Standalone Blue/Red | 해당 source/asset 해시 동일. 런타임 Ingredient 생성 호출은 Purple canonical sequence 두 곳뿐 |

[검증 전체 요약](../unity/Logs/PurpleIngredientFinal2QA/validation_summary.json), [pixel diff](../unity/Logs/PurpleIngredientFinal2QA/Review/20260920_175153_277/comparison_metrics.json), [locks](../unity/Logs/PurpleIngredientFinal2QA/locks.json), [collision](../unity/Logs/PurpleIngredientFinal2QA/collision.json), [lifecycle](../unity/Logs/PurpleIngredientFinal2QA/lifecycle.json), [ingredient cleanup](../unity/Logs/PurpleIngredientFinal2QA/ingredient_lifecycle.json), [재질 진단](../unity/Logs/PurpleIngredientFinal2QA/Final/20260920_175153_277/burst-materials.txt).

아크 geometry 비용은 재료당 mesh 3개, material 1개, 최대 336 vertices / 576 triangles다. 사용 중인 초기 구간에는 이전 30개 outer line을 비활성화한다. 새 mesh는 Dispose 시 해제된다. GPU 성능 benchmark를 수행했다는 의미는 아니다.

## 변경 파일 / 보존

기존 runtime 수정은 두 파일의 후보 분기뿐이다:

- `unity/Assets/Scripts/Player/PurpleFusionIngredient.cs`: Final2일 때 bulk component 생성·sample, 기존 외곽 line visibility 처리. 본체 surface/trajectory/Birth 로직 변경 없음.
- `unity/Assets/Scripts/Player/PurpleIngredientCollisionAccent.cs`: 새 collision shader 선택. timing/ownership 변경 없음.

신규 파일과 각각의 `.meta`:

- `Scripts/Player/PurpleIngredientFinal2Profile.cs`
- `Scripts/Player/PurpleIngredientBurstVolume.cs`
- `Resources/VFX/PurpleIngredientFinal2Profile.asset`
- `Resources/VFX/PurpleIngredientBurstVolume.shader`
- `Resources/VFX/PurpleIngredientCollisionTear.shader`
- `Editor/PurpleIngredientFinal2ProfileEditor.cs` — 한국어 toggle, SerializedProperty 유지.
- `Editor/PurpleIngredientFinal2Tests.cs`

위 경로는 `unity/Assets/` 기준. 이번 checkpoint에서는 이 보고서와 handoff 문서의 상태 블록을 갱신했다. 과거 기록은 삭제하거나 USER VERIFIED 상태로 승격하지 않았다.

시작 시 tracked/untracked **760개**를 해시 기록했다. **758개 동일**, 기존 runtime **2개 변경**, 삭제 **0개**였다. 기존 Final/Pass 2 자산, dark mass, canonical trajectory source, 확정 Purple 전체, standalone Blue/Red, 사용자 Scene/ProjectSettings/local art는 시작 시점 그대로다. [보존 검사](../unity/Logs/PurpleIngredientFinal2QA/final_preservation.json), [git status -sb](../unity/Logs/PurpleIngredientFinal2QA/git-status-final.txt). unity/Logs/ 산출물은 gitignored local evidence이며 repository checkpoint에는 포함하지 않는다.

## 사용 상태

`PurpleIngredientReboot2Profile` + `PurpleIngredientFinalProfile` + 새 `PurpleIngredientFinal2Profile`의 후보 toggle이 켜진 새 시전에 적용된다. Final2 OFF면 이전 Final이다. 저장된 toggle은 모두 원래 값이며 새 후보 기본 OFF. QA에서는 양쪽에 기존 profile chain 및 Outer/Birth를 동일하게 켜고 Final2만 전환했다.

Branch `feat/gojo-blue-screen-distortion`, HEAD `7f5c6979e893f3a79a291934403f7cc28cc232af` 유지. Commit/push/merge/reset/clean/restore 없음. 다음은 사용자 visual review다.
