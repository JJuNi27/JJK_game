# PurpleIngredientPairPressure — 대형 풍압 후보

상태: **CODEX VALIDATED / PENDING USER VISUAL REVIEW / NOT USER VERIFIED**

별도 opt-in 후보이며 저장 상태는 기본 OFF다. Production 또는 최종 visual baseline으로 승격하지 않았다. 캡처 이후 자동 polish, commit/push는 수행하지 않았다.

## Change Contract

- Target gap: PairPolish의 방향성 개선은 보존하면서, 작은 trail/mote보다 먼저 읽힐 수 있는 넓은 흡인/척력 공간을 회복한다.
- Allowed scope: Ingredient 전용 macro pressure runtime, shader, profile, 한글 Inspector, opt-in 연결 및 해당 QA.
- Must preserve: 기존 PairPolish/Final2/FlowFirst, Blue/Red body·크기, ±0.08m trajectory, formation/contact/fusion timing, Purple Body/Outer/Birth/Travel, gameplay, camera ownership, terminal, USER VERIFIED 값과 unrelated dirty 작업.
- Expected visible change: Blue는 넓은 공간에서 휘어 들어오며 좁아지고 가속한다. Red는 tension 이후 빠르게 넓어져 외곽에서 풀린다.
- Failure condition: sphere만 먼저 읽힘, clean ribbon/sheet/완전한 disc, 동일 모션의 recolor, particle spam, 기존 motion 변경.
- Required evidence: 밝은 중성 환경의 원속도 Side/Caster/Observer A/B, 인접 프레임, targeted assertions, resource cleanup, 보호 범위 비교.

## 구현

각 ingredient에 4개의 입체적인 압력 volume을 추가했다. 기존 PairPolish의 mote/flow 수와 경로는 바꾸지 않았다. 각 volume은 180 vertices이며 런타임이 소유한 mesh/material은 종료 시 해제한다.

- Blue: 기존 event clock/cadence에 맞추어 넓은 곡선 경로가 중심으로 수축·가속한다. 폭도 줄어들고 body 안에서 소멸한다.
- Red: 기존 tension과 cadence를 유지한다. 좁고 압축된 구간에서 시작해 열린 공간으로 확장하고 외곽에서 breakup된다. 전체 원형 disc를 만들지 않았다.
- Shader: volume 안의 3D animated density/erosion과 국소 crest를 사용한다. cyan/blue-gray와 crimson/red-gray의 diffuse body에 일부 밝은 crest를 겹친다. global Bloom/Volume은 수정하지 않았다.
- 초기 profile: reach 2.7, width 1.15, depth 0.72, density 0.72, diffuse emission 1.55, crest emission 4.2. 기존 body radius 약 1.025인 local 기준이며, 명목 reach와 실제 화면에서 읽히는 범위는 다르다.
- 융합 후반에는 pressure를 비활성화하여 completed Purple hold까지 따라가지 않는다.

## 실제 화면 평가

| 시점 | 관찰 근거 | 평가/한계 |
|---|---|---|
| Side | 0.35s 및 1.10–1.35s에서 Blue 주변의 넓은 회청색 영역이 기존 작은 선보다 분명하다. 일부 edge도 침범한다. | 공간 점유는 증가했다. 그러나 1.10s의 위쪽 volume은 둥근 반투명 구름/껍질처럼 분리되어 보인다. 압축된 공기나 허리케인으로 충분히 읽힌다고 판정하지 않는다. |
| Caster | 0.35s, 1.10s의 Blue pressure 존재감 증가. 1.35s에 body 앞쪽 중첩이 보인다. | Blue 개선은 읽히지만 Red의 넓은 pressure는 더 약하다. 두 sphere의 지배력이 여전히 강하다. |
| Observer | 0.35s, 1.10s에 Blue pressure patch가 보인다. | Red의 주변 공간 변화가 작고 두 sphere가 먼저 읽힌다. 요청한 원거리 흡인/척력의 즉시 가독성은 목표 미달이다. |

**Visual similarity: MIXED. Art direction: MIXED.** 풍압 면적 증가의 confidence는 높다. 화면에서의 force direction 및 웅장함을 충분히 달성했다는 confidence는 낮다. 반대 근거는 위의 rounded cloud 형태, 약한 Red, Observer의 sphere dominance다.

원속도 60fps 영상은 제공했다. AI 평가는 동일 시각의 프레임 및 인접 시퀀스에 근거하며, 사람의 원속도 체감 평가와 동등하다고 주장하지 않는다. 정지 프레임이나 runtime 방향 assertion만으로 시각 성공을 확정하지 않는다. 일부 Caster 장면의 캐릭터 주변 cyan HUD 표시도 새 pressure로 세지 않았다.

## Technical / Regression

- 실제 NUnit assertion harness 실행: **9 PASS / 0 FAIL**. 새 pressure 검사 3개 + 기존 PairPolish 검사 6개. Unity Test Runner discovery가 아니라 명시적인 assertion 호출이며 machine-readable [targeted.xml](../unity/Logs/PurpleIngredientPairPressureQA/targeted.xml)을 저장했다.
- C# compile 오류 0, candidate shader 오류 0, 최종 console error 0. Console에 기존 경고 3개(Input Manager 및 이전 capture의 unused variable 2개)가 기록되어 있다.
- PairPolish의 mote 좌표, 12개 flow의 각 20개 지점, ingredient body의 shader/scale/property 비교 동일. Blue inward/Red outward, 반복 cleanup 및 fusion 후 pressure 비활성화 검사 통과.
- Side/Caster/Observer 각각 385 sample의 위치·크기·활성 상태, contact, 기존 flash/hold/pulse 기록 동일. 첫 접촉 기록 약 1.44418s 유지.
- Play Mode QA 종료 후 ingredient Material/Mesh **0/0**, PairPolish/PairPressure root **0/0**. profile OFF 및 background 설정 복구. Editor는 VFXLab Edit Mode, dirty=False로 복귀했다. Scene 파일은 저장하지 않았다.
- 작업 시작 hash와 비교한 기존 파일 변경은 opt-in hook을 추가한 `PurpleFusionIngredient.cs` 하나뿐이다. 기존 body, profiles, PairPolish runtime, Scene/ProjectSettings 및 unrelated 작업의 내용은 보존했다.
- **전체 화면 Purple 구간 RGB 비교: INCONCLUSIVE.** 2.2s 이후 다른 프레임 수는 Side 189, Caster/Observer 각각 253이다. Live character/HUD 등의 비교 변수가 있어 모든 차이를 Purple에 귀속하거나 regression PASS로 승격하지 않는다.
- 7.0s terminal still: Side max RGB diff 0. Caster max diff 209/5,491 changed pixels, Observer max diff 126/233 changed pixels이므로 두 시점은 INCONCLUSIVE다.
- full regression은 실행하지 않았다.

## 결과물

공통 폴더: `D:\JJK_game\unity\Logs\PurpleIngredientPairPressureQA\AB\20261005_140832_989`

모든 Full 영상: LEFT **BASELINE = PairPolish**, RIGHT **CANDIDATE = PairPressure**, 밝은 중성 환경, 1920×540, 60fps, 385 frames, 6.416667s.

- [Side_AB_Full.mp4](../unity/Logs/PurpleIngredientPairPressureQA/AB/20261005_140832_989/Side_AB_Full.mp4)
- [Caster_AB_Full.mp4](../unity/Logs/PurpleIngredientPairPressureQA/AB/20261005_140832_989/Caster_AB_Full.mp4)
- [Observer_AB_Full.mp4](../unity/Logs/PurpleIngredientPairPressureQA/AB/20261005_140832_989/Observer_AB_Full.mp4)
- Side/Caster 각각 `*_AB_Grayscale_OriginalSpeed.mp4`도 제공한다.
- 각 시점의 `*_Ingredient_Review.jpg`, `*_Contact_Adjacent.jpg`, `*_Blue_Absorption_Adjacent.jpg`에 동일 timestamp/인접 프레임을 정리했다.
- [comparison_validation.json](../unity/Logs/PurpleIngredientPairPressureQA/AB/20261005_140832_989/comparison_validation.json), `ConsoleFinal.json`, `StateFinal.json`, `EndQa.json`에 검증 기록을 보존했다. QA 미디어/로그는 local-only다.

## 이번 변경 파일

기존 파일: `unity/Assets/Scripts/Player/PurpleFusionIngredient.cs` — 기존 경로를 보존한 opt-in macro pressure 연결.

신규 파일(각 Unity 파일에 신규 `.meta` 포함):

- `unity/Assets/Scripts/Player/PurpleIngredientPairPressureProfile.cs`
- `unity/Assets/Scripts/Player/PurpleIngredientPairPressureVolume.cs`
- `unity/Assets/Resources/VFX/PurpleIngredientPairPressure.shader`
- `unity/Assets/Resources/VFX/PurpleIngredientPairPressureProfile.asset`
- `unity/Assets/Editor/PurpleIngredientPairPressureProfileEditor.cs`
- `unity/Assets/Editor/PurpleIngredientPairPressureCapture.cs`
- `unity/Assets/Editor/PurpleIngredientPairPressureValidation.cs`
- `docs/PURPLE_INGREDIENT_PAIR_PRESSURE_REVIEW.md`

ASTRA ESCALATION: 없음. 보호 조건을 바꾸거나 공용 architecture를 재설계하지 않았다. 시각 목표는 완전히 달성하지 못했으며, A/B 제출 후 추가 자동 튜닝 없이 사용자 검수를 기다린다.
