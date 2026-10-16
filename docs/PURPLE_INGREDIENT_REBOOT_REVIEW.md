# Purple Formation — Direction Reboot / Material-Language Rework

2026-09-19 · **CODEX VALIDATED / PENDING USER VISUAL REVIEW**

기존 baseline / Ingredient Polish / Macro를 보존하고 별도 Direction Reboot 후보를 추가했다. 이번 사용자의 지시에 따라 이전 Red의 bright-core 유지 방침을 대체하여, **Red도 어두운 압력 질량**으로 바꿨다. 완성된 Purple이나 독립 기술 Blue/Red에 적용하는 변경은 아니다. 새 후보 저장 기본값은 OFF이며 commit/push/merge하지 않았다.

## 비교 결과물

[전체 검토 페이지](../unity/Logs/PurpleIngredientRebootQA/Review/20260919_114110_831/index.html). **왼쪽 Current Macro / 오른쪽 Direction Reboot**다.

- [Side_AB_Full.mp4](../unity/Logs/PurpleIngredientRebootQA/Review/20260919_114110_831/Side_AB_Full.mp4)
- [Caster_AB_Full.mp4](../unity/Logs/PurpleIngredientRebootQA/Review/20260919_114110_831/Caster_AB_Full.mp4)
- [Side_AB_Grayscale_HalfSpeed.mp4](../unity/Logs/PurpleIngredientRebootQA/Review/20260919_114110_831/Side_AB_Grayscale_HalfSpeed.mp4)
- [Caster_AB_Grayscale_HalfSpeed.mp4](../unity/Logs/PurpleIngredientRebootQA/Review/20260919_114110_831/Caster_AB_Grayscale_HalfSpeed.mp4)
- [Formation focus 보조 영상](../unity/Logs/PurpleIngredientRebootQA/Review/20260919_114110_831/Side_AB_Formation_Focus.mp4)
- [Side 단계 still](../unity/Logs/PurpleIngredientRebootQA/Review/20260919_114110_831/Side_Phase_Comparison.jpg), [Caster 단계 still](../unity/Logs/PurpleIngredientRebootQA/Review/20260919_114110_831/Caster_Phase_Comparison.jpg)
- [Side 흑백 연속 프레임](../unity/Logs/PurpleIngredientRebootQA/Review/20260919_114110_831/Side_Motion_Grayscale_Samples.jpg), [Caster 흑백 연속 프레임](../unity/Logs/PurpleIngredientRebootQA/Review/20260919_114110_831/Caster_Motion_Grayscale_Samples.jpg)

Unity URP 실제 렌더, 시점별 원본 960×540 / 30fps, 좌우 비교 1920×540. Full은 0~5.833초로 formation/hold/release/초기 travel을 포함한다. 흑백은 동일 영상의 반속 변환이며 11.633초다. Focus는 Side의 formation 2.2초를 잘라 확대한 보조 자료다. 별도 새로운 시점 캡처나 runtime camera 변경이 아니다.

양쪽은 동일 stage/background/clock/camera/post-process다. 기존 Ingredient Polish와 Macro를 켠 상태에서 Reboot만 OFF/ON하여 비교했다. 유지하기로 한 Outer/Birth도 양쪽에서 동일하게 ON으로 렌더한 뒤 원래 값을 복구했다. 저장 profile/Scene에는 반영하지 않았다.

## 재질과 동작의 차이

| 대상 | Current Macro | Direction Reboot |
|---|---|---|
| Blue 중심 | 어두운 sink와 밝은 시안 계열 surface/긴 streak | Deep Navy의 불투명 질량, 어두운 중심, 제한적인 blue 균열/rim |
| Red 중심 | 작은 white pressure light와 붉은 shoulder | **White-sun core 없음**, 적흑색 질량과 붉은 균열/rim |
| 큰 공간 구조 | 길고 굵은 선두/꼬리 중심 | Blue는 회전하며 수축하는 band, Red는 바깥으로 밀려나는 partial pressure band |
| 보조 흐름 | 외부 궤적이 화면의 주된 인상 | 나머지 curve와 mote를 보조 흐름으로 유지; count 증가는 없음 |
| 합성 | 기존 additive filament | 주요 band/mote는 alpha blend로 어두운 두께와 발광층을 함께 표현 |

Surface는 별도 opaque shader이며 배경이 본체 안으로 비치지 않는다. 넓은 additive plasma나 정면 white core 대신 dark base, 불규칙한 rim, 조각난 좁은 emission 결을 사용한다. 순수 검은 공이 되지 않도록 낮은 표면 명암과 색이 남는다. 미세한 vertex 변형은 유지하지만 Purple R3의 chunky mass를 복제하지 않았다.

기존 10개 curve 중 6개를 세 방향의 paired partial band로 재사용한다. Blue의 band 반경은 local 1.8→0.82로 줄고, Red는 1.02→1.8로 커진다. Blue에는 수축 중 angular rotation, Red에는 짧은 압력 pulse가 있다. 남은 curve 4개와 mote 20개는 inward/outward 보조 흐름이다. Band 폭 0.26, Blue/Red cycle speed 1.1/1.4. 이는 외부 visual 경로이며 formation root의 크기·위치·경로·시간을 변경하지 않는다.

후기 fusion에서는 새 흐름을 blend out하고 기존 filament와 Birth compression으로 복귀한다. Tension bridge와 Birth compression 블록은 수정하지 않았다. Global Bloom/Volume, local distortion, gameplay force 시스템은 추가하지 않았다.

## 실제 화면 평가와 남은 한계

최종 평가: **어두운 주력 질량이라는 재질 방향은 Macro보다 요청에 가깝다. 사용자 비교 검토를 권장하며 Production 기본 적용 승인은 보류한다.**

Red의 변화가 가장 크다. 백색 중심점이 없어지고 적흑색 중심부가 넓게 보이며, 색을 빼도 Macro의 white-sun 인상과 명확히 다르다. Blue도 시안 장식선의 비중이 줄고 어두운 본체가 먼저 보인다. Rim/균열은 형체를 유지한다. 반면 Macro보다 화면 전체의 번쩍임과 긴 streak의 폭주감은 줄었다. 이는 질량 중심으로 바꾼 trade-off이며, 무조건 화려함이 향상됐다고 평가하지 않는다.

초기 preview에서 band가 규칙적인 점선·다중 선처럼 보여 단면과 끊김을 보정했다. 큰 고리가 본체를 압도하지 않도록 최대 반경과 원호 길이도 줄였다. 흑백에서 Blue band가 묻히는 문제는 **Blue 외부층의 명도만** 올려 보정했다. Red 중심 또는 전역 bloom을 키우지 않았다. 초기 결과도 QA 이력에 보존했다.

남은 한계:

1. 일부 시점에서 여전히 **고리로 둘러싸인 구체 / 공전 구조**로 읽힌다. 연속 프레임의 수축·팽창이 정지 화면보다 방향 구분에 유리하다. 색 없이 즉시 완전하게 구별된다고 주장하지 않는다.
2. 어두운 중심과 낮아진 보조 흐름 때문에 먼 Caster에서는 Macro보다 덜 폭발적으로 보인다. 명도 보정 후 Blue band가 흑백에서 나타나지만 Red보다 약하며, 두 동작의 즉각적인 구분은 아직 제한적이다. Blue의 dark mass/흐름 균형은 실제 전투 배경에서 사용자 확인이 필요하다.
3. 불규칙한 emission이 있어도 중심은 여전히 구체 표면으로 읽힌다. 완전한 체적 내부 흐름을 구현한 것은 아니며, 근접 band에는 LineRenderer의 ribbon 인상과 일부 각진 원호가 남는다.

## 검증 및 Purple 보존

최종 visual build의 targeted 검사: `purple-ingredient-reboot-final-04` **3 PASS / 0 FAIL**, C# error **0**, shader error **0**.

- `PurpleIngredientRebootTests.CanonicalPoseAndBodyLock`
- `PurpleIngredientRebootTests.FormationComparison`
- `PurpleIngredientRebootTests.IngredientDirectionsAndRepeatedCleanup`

앞선 안정화 build `purple-ingredient-reboot-final-02`에서는 위 3종과 아래 3종까지 **6 PASS / 0 FAIL**이었다. 이후 변경은 band 원호/반경/폭과 Blue 외부층 색에 국한되어 gameplay lifecycle을 반복하지 않았다. 고유 관련 테스트는 6종이다. 초기 preview 및 중간 visual 확인도 각각 3 PASS / 0 FAIL이며 최종 고유 테스트 수에 중복 합산하지 않는다. Full regression은 실행하지 않았다.

- `PurpleIngredientRebootTests.TravelTargetsRangeAndRepeatedCleanup`
- `AstraCombatVfxTests.PurpleHoldReleaseIsContinuousAndDamageUsesSameCorridor`
- `AstraCombatVfxTests.PurpleCameraCancellationAndResourceCleanup`

검사 근거:

- Canonical position/scale/forward **81개 값 차이 0**. Production/timing profile 동일.
- **2초 이후 Side/Caster 각각 115프레임 RGB 변경 픽셀 합계 0.** 압축 영상이 아니라 동일 실행의 RGB24 원본 비교다.
- Ingredient 6회 spawn/cleanup, curve/mote 방향 12회 검사, 개당 LineRenderer 31, material 증가 0.
- 실제 cast 4회: 첫 target HP45, 뒤 target HP100, max-range terminal, travel/birth cancel, legacy terminal 보존. 종료 후 material/mesh 증가 0.
- 비교 렌더의 dispose/cleanup와 camera position/FOV 복귀 통과.
- 작업 전 기존 tracked/untracked **713개 SHA256 snapshot** 대비 코드 변경은 `PurpleFusionIngredient.cs` 하나다. 기존 Ingredient/Macro 자산과 확정 Purple·standalone Blue/Red·Scene·ProjectSettings는 동일했다. 문서 변경은 별도 기록한다.

[최종 XML](../unity/Logs/AstraCombatQA/purple-ingredient-reboot-final-04.xml), [최종 log](../unity/Logs/AstraCombatQA/purple-ingredient-reboot-final-04.log), [기존 Purple 포함 XML](../unity/Logs/AstraCombatQA/purple-ingredient-reboot-final-02.xml), [검증 요약](../unity/Logs/PurpleIngredientRebootQA/validation_summary.json), [lifecycle](../unity/Logs/PurpleIngredientRebootQA/lifecycle.json), [pose locks](../unity/Logs/PurpleIngredientRebootQA/locks.json), [픽셀 차이](../unity/Logs/PurpleIngredientRebootQA/Review/20260919_114110_831/comparison_metrics.json), [최종 파일 보존](../unity/Logs/PurpleIngredientRebootQA/final_preservation.json).

## 변경 파일과 사용 방법

수정한 기존 runtime 파일:

- `unity/Assets/Scripts/Player/PurpleFusionIngredient.cs` — opt-in 선택, band material 연결·해제, 기존 flow 후 후보 sampling hook.

신규 파일(각 `.meta` 포함):

- `unity/Assets/Scripts/Player/PurpleIngredientRebootProfile.cs`
- `unity/Assets/Scripts/Player/PurpleIngredientRebootFlow.cs`
- `unity/Assets/Resources/VFX/PurpleIngredientRebootProfile.asset`
- `unity/Assets/Resources/VFX/HollowPurpleIngredientReboot.shader`
- `unity/Assets/Resources/VFX/HollowPurpleIngredientPressure.shader`
- `unity/Assets/Editor/PurpleIngredientRebootProfileEditor.cs`
- `unity/Assets/Editor/PurpleIngredientRebootTests.cs`

새 profile의 한국어 Inspector에서 `질량 방향 후보 사용`을 켜고 새 시전을 시작하면 적용된다. 기존 후보보다 우선하며 기본값은 OFF다. 영상과 정확히 같은 A/B는 기존 Polish/Macro ON을 공통으로 두고 Reboot만 전환한다. 후보 선택이 이전 shader/profile 파일을 덮어쓰지 않는다.

상태 문서: 이 보고서, `CURRENT_HANDOFF.md`, `active/CURRENT_TASK.md`, `architecture/GOJO_VFX.md`. QA 자료는 `unity/Logs/PurpleIngredientRebootQA/`의 local-only 결과다. 기존 exploration/user art/reference/QA 삭제·덮어쓰기 없음.

비용: renderer 수는 동일하다. Reboot 전용 pressure material이 Ingredient당 1개 추가되어 총 3개이며 종료 시 해제한다. 별도 Mesh 객체는 생성하지 않고 기존 sphere/LineRenderer를 사용한다. Mote 6점 때문에 최대 line 제어점은 Macro와 같은 472개다. CPU sampling/투명 band의 overdraw 비용은 존재하며 GPU ms/목표 기기 성능은 측정하지 않았다.

Branch `feat/gojo-blue-screen-distortion`, HEAD `7f5c6979e893f3a79a291934403f7cc28cc232af` 유지. Staging/commit/push/merge 없음. **사용자 visual review를 기다린다.**
