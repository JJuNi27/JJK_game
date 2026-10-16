# Hollow Purple Ingredient — Direction Reboot Pass 2

2026-09-20 · **CODEX VALIDATED / PENDING USER VISUAL REVIEW**

중단된 working tree에서 이어서 완료했다. 궤적 correction, 첫 접촉 accent, 외곽 에너지 강화는 중단 전에 이미 구현되어 있었다. 재개 후에는 diff와 실패 로그를 확인하고 **QA의 inactive-pose 기대값 및 60Hz 상세 렌더의 actor 초기화만 수정**했다. Runtime 시각 구현을 새로 만들거나 추가 polish하지 않았다.

기존 baseline / Ingredient Polish / Macro / Direction Reboot 후보는 보존한다. 새 `PurpleIngredientReboot2Profile.asset` 기본값은 OFF다. Commit / push / merge / staging 없음.

## 결과 영상

[전체 비교 페이지](../unity/Logs/PurpleIngredientReboot2QA/Review/20260920_100823_323/index.html). **왼쪽 Direction Reboot / 오른쪽 Reboot Pass 2**.

- [Side_AB_Full.mp4](../unity/Logs/PurpleIngredientReboot2QA/Review/20260920_100823_323/Side_AB_Full.mp4)
- [Caster_AB_Full.mp4](../unity/Logs/PurpleIngredientReboot2QA/Review/20260920_100823_323/Caster_AB_Full.mp4)
- [Side_AB_Grayscale_HalfSpeed.mp4](../unity/Logs/PurpleIngredientReboot2QA/Review/20260920_100823_323/Side_AB_Grayscale_HalfSpeed.mp4)
- [Caster_AB_Grayscale_HalfSpeed.mp4](../unity/Logs/PurpleIngredientReboot2QA/Review/20260920_100823_323/Caster_AB_Grayscale_HalfSpeed.mp4)
- [Fusion_Trajectory_Close.mp4](../unity/Logs/PurpleIngredientReboot2QA/Review/20260920_100823_323/Fusion_Trajectory_Close.mp4)
- [Fusion_ImpactFrame_HalfSpeed.mp4](../unity/Logs/PurpleIngredientReboot2QA/Review/20260920_100823_323/Fusion_ImpactFrame_HalfSpeed.mp4)

보조 자료: [Side 단계 비교](../unity/Logs/PurpleIngredientReboot2QA/Review/20260920_100823_323/Side_Phase_Comparison.jpg), [Caster 단계 비교](../unity/Logs/PurpleIngredientReboot2QA/Review/20260920_100823_323/Caster_Phase_Comparison.jpg), [60Hz 접촉 프레임 시트](../unity/Logs/PurpleIngredientReboot2QA/Review/20260920_100823_323/Collision_60Hz_Contact_Sheet.jpg), [Side 흑백 motion 표본](../unity/Logs/PurpleIngredientReboot2QA/Review/20260920_100823_323/Side_Motion_Grayscale_Samples.jpg).

Full은 0~5.833초 / 30fps의 실제 Unity URP 렌더다. 시점별 960×540 원본을 좌우 1920×540으로 배치했다. 흑백 반속은 11.633초다. Trajectory Close는 1.1~2.1초의 Side를 crop한 영상이다. Impact detail은 **1.3~2.1초를 별도로 60Hz sampling한 49프레임**을 반속으로 재생한다. 30fps 영상을 단순 복제하여 60Hz 검증이라고 표시하지 않았다.

양쪽은 동일 stage / camera / clock / post-process이며 기존 Outer/Birth를 동일하게 ON으로 렌더하고 원래 설정을 복구했다. 기존 Polish/Macro/Reboot도 양쪽 ON이고 Pass 2만 OFF/ON이다. Source profile 저장값과 Scene은 변경하지 않았다. 초기 `20260919_125146_568` 결과도 보존하지만, 그 실행의 `Fusion_*` 상세 렌더는 actor 위치 초기화 누락으로 비어 있어 검수 근거에서 제외한다.

## 구현 내용

### 외곽 에너지

Reboot의 dark-mass surface shader, silhouette 설정, Blue dark centre와 Red의 white-core 없는 적흑색 질량을 그대로 사용한다. 새로운 표현은 Ingredient 외곽 flow와 전용 pressure material에만 들어간다.

- 기존 curve 10 / mote 20 / bridge 1을 재사용한다. Count 증가는 없다.
- Curve 6개는 폭이 있는 불규칙한 broken band다. 도달 반경 1.8→2.35 local units, 기준 폭 0.26→0.42로 확대했다. 원형을 완전히 잇지 않고, 구간별 반경/방향/폭 변화와 검게 갈라진 단면을 사용한다.
- Blue는 바깥에서 안으로 반경이 줄면서 휘어 들어가고, Red는 표면에서 바깥으로 팽창한다. 동일 반경의 공전 경로가 아니다.
- 나머지 curve 4개는 발생마다 방향이 달라지는 약 0.12초 방전이다. Mote는 짧은 보조 inward/outward 궤적이다. Particle 난사나 additive cloud는 추가하지 않았다.
- 명도는 외부 에너지에 집중한다. Blue는 흑백에서도 불규칙한 수축 band가 보이고, Red는 white core를 다시 넣지 않고 바깥 압력층을 강화한다.

### Fusion trajectory

기존 `UpdateMerge`는 같은 중앙 이동 경로 위에 Blue `sin(tπ) × +0.45m`, Red `sin(tπ) × -0.1575m`를 별도로 더했다. 이 비대칭이 Blue만 위로 들리는 원인이었다.

Pass 2에서는 **Blue +0.08m / Red -0.08m**의 대칭적인 작은 opposing-force arc를 사용한다. 수평 separation과 smoothstep 진행, 시작 위치, 공통 completed offset, 구체 크기, summon/fusion duration, 최종 중앙점은 그대로다. 공통 경로가 최종 hold 위치로 올라가는 이동은 유지하며, Blue만 추가로 크게 뜨던 부분을 줄였다. 후보 OFF이면 기존 식을 그대로 사용한다.

### Ingredient collision accent

현재 formation scale/separation과 새 수직 arc에서 두 **명목 sphere 표면의 첫 접촉**을 계산한다. 현재 설정의 접촉은 약 **1.444183초**, duration은 **2/60초 = 0.03333초**다. 셰이더의 미세한 vertex displacement까지 물리 충돌 계산에 사용하는 것은 아니다.

첫 프레임은 충돌 주변의 백색 고대비와 검은 집중선, 두 번째는 흑색 반전과 밝은 집중선이다. 이후 즉시 비활성화한다. 60Hz 최종 렌더의 1.4500 / 1.4667초에 보이며 1.4833초에는 사라진다. 기존 Birth flash의 활성 시작 1.945초보다 충분히 앞서 끝난다.

World-local volume과 URP opaque scene color를 사용하는 별도 shader다. 화면 전체 white-out이 아니며, 투명 외곽 에너지 일부는 원래 색이 남는다. Canvas, camera lease, camera pose/FOV, global Volume/renderer settings를 변경하지 않는다. 기존 Birth flash/shell 및 release impact frame은 그대로다.

## 실제 화면 평가

**이전 Reboot보다 사용자 비교 검토 가치가 높다.** Dark mass를 유지한 채 외곽의 존재감이 명확히 커졌고, Side/Caster 모두 ‘조용한 어두운 공’ 인상이 줄었다. 흑백에서도 Blue의 밝은 broken band와 Red의 외향 pressure 층이 이전보다 잘 보인다. 완전한 매끈한 원형 고리보다 불규칙한 전류/균열 인상이 강하다.

Fusion에서는 Blue의 큰 위쪽 arc가 줄어 두 질량의 높이가 더 균형 있게 유지된다. 중심 접근 후 짧은 흑백 accent가 독립된 접촉 beat로 읽히고, 이후 기존 Purple 형성/Birth/hold로 이어진다. 처음 접촉하는 시점과 완성된 Purple의 Birth peak를 같은 순간으로 재정의하지 않았다.

남은 한계:

1. 굵은 band의 일부 구간은 여전히 **납작한 ribbon / 톱니 모양 윤곽**으로 읽힌다. 완전한 체적 lightning 표현은 아니다.
2. 색 없이 inward/outward를 판단하려면 motion이 필요하다. 정지 프레임이나 먼 Caster에서는 두 방향의 즉각적인 구별이 제한적이다. Fusion 중 일부 외곽 flow가 서로 겹친다.
3. Collision은 의도적으로 매우 짧다. 30fps에서는 한 프레임으로 보이고 낮은 프레임율·hitch에서는 window를 건너뛸 수 있다. 지속 시간을 늘리거나 기존 Birth에 합치는 변경은 하지 않았다. 첫 백색 프레임은 근접 화면에서 강하므로 최종 강도는 사용자 확인 대상이다.

## 검증

최종 실행 `purple-ingredient-reboot2-final-02`: **5 PASS / 0 FAIL**, C# compile error **0**, shader error **0**.

- `PurpleIngredientReboot2Tests.FusionTrajectoryAndBodyLock`
- `PurpleIngredientReboot2Tests.FormationComparison`
- `PurpleIngredientReboot2Tests.TravelTargetsRangeAndRepeatedCleanup`
- `AstraCombatVfxTests.PurpleHoldReleaseIsContinuousAndDamageUsesSameCorridor`
- `AstraCombatVfxTests.PurpleCameraCancellationAndResourceCleanup`

중단 전에 통과한 아래 2종은 runtime 구현이 동일하여 반복하지 않았다.

- `PurpleIngredientReboot2Tests.CollisionContactWindowAndCleanup`
- `PurpleIngredientReboot2Tests.IngredientDirectionsAndRepeatedCleanup`

따라서 **관련 고유 7종 모두 최종 유효 PASS**다. 초기 실행은 3 PASS / 1 FAIL이었으며 이를 숨기거나 전체 무실패로 집계하지 않는다. 실패 원인은 inactive Ingredient transform이 마지막 merge 위치를 유지하는데 검사에서 그 의도된 수직 차이를 고려하지 않은 것이다. 마지막 merge sample 기준의 예상 위치로 검사를 수정했고 PASS를 확인했다. Full regression은 실행하지 않았다.

확인한 내용:

- 81개 position/scale/forward sample에서 **허용한 Ingredient Y correction 외 차이 0**. Purple pose, root scale, forward, 수평 경로, Production/timing profile 보존.
- **2초 이후 Side/Caster 각각 115프레임 RGB 변경 픽셀 합계 0.** 동일 실행의 압축 전 RGB24를 비교했다.
- Ingredient 6회 spawn/cleanup, inward/outward 방향 검사 12회, material 증가 0.
- Collision nominal 60Hz visible frame 2개, 접촉 시 centre distance와 radius sum 일치, active contact 중 cancel/dispose 3회 후 material 증가 0.
- 실제 cast 4회: 첫 target HP45 / 뒤 target HP100, max-range terminal, travel/birth cancel, legacy terminal. 종료 후 material/mesh 증가 0.
- Render cleanup와 카메라 위치/FOV 복귀 통과. 기존 camera ownership 코드는 변경하지 않았다.

원본 근거: [최종 XML](../unity/Logs/AstraCombatQA/purple-ingredient-reboot2-final-02.xml), [최종 log](../unity/Logs/AstraCombatQA/purple-ingredient-reboot2-final-02.log), [검증 이력](../unity/Logs/PurpleIngredientReboot2QA/validation_summary.json), [trajectory locks](../unity/Logs/PurpleIngredientReboot2QA/locks.json), [collision](../unity/Logs/PurpleIngredientReboot2QA/collision.json), [Ingredient cleanup](../unity/Logs/PurpleIngredientReboot2QA/ingredient_lifecycle.json), [gameplay lifecycle](../unity/Logs/PurpleIngredientReboot2QA/lifecycle.json), [픽셀 비교](../unity/Logs/PurpleIngredientReboot2QA/Review/20260920_100823_323/comparison_metrics.json).

## 보호 파일 / 변경 파일

작업 시작 전 **728개 기존 tracked/untracked 파일 SHA256**을 기준으로 비교했다. 실제 runtime 변경은 아래 2개뿐이며 나머지 보호 소스·자산은 동일하다. 기존 unrelated dirty 작업은 보존했다.

- `unity/Assets/Scripts/Player/PurpleFusionIngredient.cs` — Pass 2 flow/material 선택. 기존 Reboot Body 재사용.
- `unity/Assets/Scripts/Player/PrototypeHollowPurplePresentationRuntime.cs` — 후보 한정 vertical arc, owned contact accent 생성/sample. Purple Body/Outer/Birth/Travel/hold/release/terminal 분기는 보존.

신규 Unity 파일(각 `.meta` 포함):

- `PurpleIngredientReboot2Profile.cs`, `PurpleIngredientReboot2Flow.cs`, `PurpleIngredientCollisionAccent.cs` (`Assets/Scripts/Player/`)
- `PurpleIngredientReboot2Profile.asset`, `HollowPurpleIngredientPressure2.shader`, `PurpleIngredientCollision.shader` (`Assets/Resources/VFX/`)
- `PurpleIngredientReboot2ProfileEditor.cs`, `PurpleIngredientReboot2Tests.cs` (`Assets/Editor/`)

문서: 이 보고서, `CURRENT_HANDOFF.md`, `active/CURRENT_TASK.md`, `architecture/GOJO_VFX.md`. QA 도구와 결과는 `unity/Logs/PurpleIngredientReboot2QA/`의 local-only 자료다. [최종 보존 확인](../unity/Logs/PurpleIngredientReboot2QA/final_preservation.json), [git status -sb](../unity/Logs/PurpleIngredientReboot2QA/git-status-final.txt).

Standalone Blue/Red, D2-R3 Body, OuterR2, Birth shader/shell, Travel, camera/terminal 구현, Scene/Animator/FBX/LocalModels/LocalAudio/ProjectSettings를 수정하지 않았다. 기존 Ingredient/Reboot 프로필·셰이더도 덮어쓰지 않았다.

새 후보는 한국어 Inspector의 `리부트 2차 후보 사용`을 켜고 새 시전부터 확인할 수 있다. 기본 OFF다. 영상과 같은 A/B는 기존 Polish/Macro/Reboot ON을 공통으로 두고 Pass 2만 전환한다. 사용자 승인 전 기본 Production 적용은 하지 않는다.

비용: Ingredient renderer/material 수는 이전 Reboot와 같고, collision volume용 renderer/material이 cast당 1개 추가되어 2프레임만 활성화된다. 새 Mesh 객체를 직접 생성하지 않으며 기존 primitive/LineRenderer를 사용한다. GPU frame time은 측정하지 않았다.

## 후속 메모 — 이번에는 구현하지 않음

**Purple Outer Lightning-Storm Re-evaluation**: 현재 완성 Purple의 halo/aura가 레퍼런스보다 부드럽거나 규칙적으로 pulsate하는지 검토하고, 필요하면 broken lightning / irregular fragments / storm-like motion을 별도 후보로 비교한다. 이번 Pass 2에는 어떤 Outer 변경도 넣지 않았다.

Branch `feat/gojo-blue-screen-distortion`, HEAD `7f5c6979e893f3a79a291934403f7cc28cc232af` 유지. 사용자 visual approval을 기다린다.
