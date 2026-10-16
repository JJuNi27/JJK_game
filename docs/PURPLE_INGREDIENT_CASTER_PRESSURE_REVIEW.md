# PurpleIngredientCasterPressure — 시전자 중심 차징 풍압

2026-10-06 KST. **CODEX VALIDATED (아래 기술 검증 범위) / PENDING USER VISUAL REVIEW / NOT USER VERIFIED**.

기존 PairPressure의 구체별 대형 풍압은 사용하지 않는다. PairPolish ingredient + 새 caster pressure 조합을 별도 opt-in/default OFF로 보존했다. Production 승격, commit/push/merge, 캡처 이후 visual polish는 하지 않았다.

## Change Contract

- Target gap: 큰 풍압의 중심을 각 ingredient가 아닌 Hollow Purple 시전자 공간에 둔다.
- Allowed scope: 독립 caster pressure runtime/shader/profile/한글 Inspector, canonical sequence의 opt-in lifecycle hook, 해당 QA.
- Must preserve: PairPolish의 Blue radius 감소·가속·중심 도착·흡수 flare, Red tension→burst→breakup, 다른 cadence와 상대 위치 bending. 모든 기존 후보·구체·크기·경로·접촉/융합 시각·Purple Body/Birth/completed hold/travel·gameplay·damage·camera ownership·USER VERIFIED·unrelated 로컬 변경.
- Expected visible change: Gojo 하단·좌우·앞뒤의 넓은 회백색 흐름이 차징 중 거칠어지고, 발사 때 앞으로 쓸린 뒤 시전자 자리에서 사라진다.
- Failure condition: 구체별 colored cloud, 흰 ribbon/sheet/fog, 완전한 cyclone/disc, projectile을 따라가는 charge wind, 원거리 존재감 부재.
- Required evidence: 밝은 중성 원속도 Side/Caster/Observer A/B, ingredient/release 인접 프레임, anchor·pose/timing·cleanup·shader/compile 검증.

## Reference와 기존 상태

PRIMARY: `_local_refs/videos/고죠 무라사키 날리는 장면.mp4`, 1198×666, 30fps. 원본 약 0.8–4.8s 준비 구간의 소수 프레임을 직접 확인했다. 회백색 흐름이 캐릭터 하단·옆 공간을 차지하고 Blue/Red/Purple 발광과 겹친다. 이것을 각 ingredient에서 발생하는 큰 구름으로 해석했던 이전 접근은 이번 기준에서 제외한다.

프레임상 caster 주변 공간 점유의 confidence는 high. 개별 공기 경로·정확한 lifetime·원속도 무게감은 중첩/포화 때문에 확정하지 않았다. “공간 전체의 압력이 시전자에 모인다”는 힘의 해석은 사용자 방향과 맞춘 inference다. 캐릭터 전방의 강한 Purple wrapper도 겹치므로 모든 바람의 원점을 Gojo라고 영상만으로 단정하지 않는다.

## 구현과 역할 분리

- `PurpleIngredientCasterPressure`는 Blue/Red/Purple body의 자식이 아니라 canonical sequence가 소유한다. `BindCaster`에서 opt-in으로 생성한다.
- Anchor는 charge 동안 caster 위치를 사용한다. 발사 시 고정하며 travel projectile의 위치·속도는 받지 않는다.
- 10개의 부분적인 넓은 공기 흐름, 총 2,590 vertices/mesh 1개/material 1개. 하단 흐름과 상체·측면 흐름이 다른 속도·seed·출현 envelope로 교체된다. 완전한 ring은 생성하지 않는다.
- White/light gray/cool gray의 broad density와 국소 sharp crest를 분리했다. Shader에서 span/width/coverage가 이동하고 공간별로 끊긴다. global Bloom/노출, 기존 sphere emission/shader는 변경하지 않았다.
- 초기 profile: radius 5.2m, height 3.4m, flow width 1.8m, opacity 0.82, crest emission 2.7, storm speed 1.55. 명목 범위가 그대로 화면상의 풍압 가독성을 보장하지는 않는다.
- Blue/Red 가까운 흐름에만 작은 방향 굴절을 준다. 전체 anchor는 옮기지 않고 ingredient path를 수정하지 않는다. Fusion 전후에는 흐름 폭이 일부 증가한다.
- Release 직전 0.18s 동안 약한 정렬, release 후 0.32s 동안 forward sweep과 fade를 사용한다. beam/shockwave 또는 새로운 travel wind는 만들지 않았다.
- 기존 TempestFinalCleanup travel/charge wrapper와 ConvergenceDiagnostic은 A/B 양쪽의 공통 조건이다. 기존 wrapper 자체는 수정하지 않았다.

## 실제 화면 평가

| 시점/beat | 관찰 | 한계 및 confidence |
|---|---|---|
| Side 0.95–1.35s | 캐릭터 하단·좌우·구체 앞뒤를 가로지르는 회백색 pressure가 생겼다. 구체별 둥근 blue/red cloud는 없다. | 중심/역할 수정은 high. 일부 조각은 아직 매끈한 흰 swish/ribbon처럼 보이며 완성된 hurricane이라고 판정하지 않는다. |
| Caster 0.35–1.35s | 기존 작은 ingredient detail 위에 중성 공기 흐름이 겹친다. | 밝은 바닥에서는 broad gray body가 약하고 sharp white 조각이 먼저 읽힌다. “엄청난 압력”보다는 몇 개의 바람 조각으로 읽힐 반대 근거가 남는다. medium. |
| Fusion 직전 1.35s/진행 1.58s | pressure의 일부가 body edge/근처를 가리며 ingredient의 색은 유지된다. | 매 순간 증가하는 압박감은 뚜렷하지 않다. 일부 구간의 공간 coverage가 비어 보인다. medium/low. |
| Observer 0.35–1.35s | caster 근처에 white pressure 변화가 추가된다. | 먼 거리에서는 여전히 sphere가 먼저 읽히며 거대한 차징 폭풍의 웅장함은 부족하다. high confidence의 한계. |
| Release 약 5.13–5.47s | 시전자 위치에서 앞으로 쓸리고 빠르게 소멸한다. 기록상 projectile와 분리되어 있다. | 기존 Purple wrapper가 크고 밝아 새 전환 일부를 가린다. 동작 분리는 high, 체감 선명도는 medium/low. |

Visual similarity: **MIXED**. Art direction: **MIXED**. 역할/anchor 정정에는 가치가 있으나, ribbon 인상과 원거리 pressure density 문제를 충분히 해결했다고 주장하지 않는다. Candidate 쪽 일부 cyan 원형 표시가 캐릭터 주변에 보이지만 기존 live HUD/camera 표시이며 새 중성 풍압으로 세지 않았다.

원속도 60fps 비교 영상은 제공했다. AI review는 timed/adjacent frame evidence에 근거하며 사람의 실시간 체감과 동등하다고 주장하지 않는다. 이 평가는 사용자 원속도 검수를 대체하지 않는다.

## 검증

- 최종 유효 targeted 결과: **10개 고유 검사 PASS / 0 FAIL** — 기존 PairPolish 6개, 새 OFF/shader/Inspector·anchor/release·cleanup 3개, 실제 Play Mode canonical pose 검사 1개.
- NUnit assertion을 명시적으로 호출한 기존 harness 방식이다. Unity Test Runner discovery를 주장하지 않는다. [targeted-final.xml](../unity/Logs/PurpleIngredientCasterPressureQA/targeted-final.xml)은 9개 유효 Edit Mode 결과와 수정된 [play-canonical.xml](../unity/Logs/PurpleIngredientCasterPressureQA/play-canonical.xml)을 연결한다. 기존 통과 9개를 이유 없이 반복하지 않았다.
- 최초 canonical 검사를 Edit Mode에서 잘못 실행한 이력이 있다. Assertion은 통과했지만 기존 `ProductionPurpleBodyRuntime.Configure:27` 및 `ProductionPurpleOuterRuntime.Configure:79`의 Collider `Destroy` 호출이 console error 8건을 만들었다. 새 QA는 이 검사를 Play Mode로 분리하고 해당 1개만 재검증했다. Production 코드는 변경하지 않았다.
- 초기 QA 클래스 import 전 호출 실패, Play 진입 직후 domain reload 중 Scene 준비의 일시적 network 오류, Pipeline 5초 timeout도 로그에 보존했다. Timeout 때 실제 검사는 XML을 끝까지 생성했다. process exit 0만으로 PASS를 판정하지 않았다.
- C# compile error 0, 최종 candidate shader supported/error 0. **Console 전체 이력: error 9건, warning 10건.** Error는 위 Edit Mode 오용 8건 + Pipeline timeout 1건이다. 올바른 Play Mode 재검증 이후 추가 error 0; 최종 Editor console ground-truth error 0. 따라서 이 작업 전체를 “console error 0”이라고 표현하지 않는다. 경고에는 새 `renderer` field의 상속 member hiding 경고와 기존 capture/Input Manager 경고가 포함된다.
- 실제 A/B 3시점 각각 385 sample의 body/ingredient 위치·크기·활성 상태, contact, 공통 flash/hold/pulse 기록 동일. 접촉 시각 약 1.44418s 유지.
- 모든 시점의 caster pressure anchor는 `(512,1,512)` 하나였다. 약 5.13333s에 Release, 5.46667s에 Ended. 5.45s부터 renderer 비활성. Projectile을 따라가지 않는다.
- Play Mode capture 종료와 canonical 재검증 후 모두 ingredient Material/Mesh **0/0**, PairPolish/CasterPressure root **0/0**, candidate OFF, background 원상 복구. 기존 VFXLab Edit Mode로 복귀하며 Scene 파일을 저장하지 않았다.
- 작업 시작 hash 대비 기존 파일 변경은 `PrototypeHollowPurplePresentationRuntime.cs`의 opt-in hook 10줄뿐이다. PairPolish/PairPressure/Final2/기존 Purple runtime·shader·profile·Scene/ProjectSettings/unrelated 변경은 보존했다.
- 5.6s 이후 49 frames RGB: Side 전 프레임 diff 0. Caster/Observer는 전 프레임 차이가 있어 **INCONCLUSIVE**다. 7.0s terminal still은 Side diff 0; Caster max diff 161/7,856 changed pixels, Observer max diff 197/361 changed pixels. 두 시점의 terminal을 PASS로 승격하지 않는다. Live actor/HUD가 비교 변수이며 모든 차이의 원인을 확정하지 않는다.
- Charge/hold 중 RGB 차이는 새 caster pressure가 의도적으로 존재하므로 기존 Purple 변경으로 곧바로 해석하지 않는다. Full regression은 실행하지 않았다.

## 출력

폴더: `D:\JJK_game\unity\Logs\PurpleIngredientCasterPressureQA\AB\20261005_160603_258` — 폴더명의 UTC 식별자를 KST 작업 날짜와 혼동하지 않는다.

LEFT **BASELINE = PairPolish**, RIGHT **CANDIDATE = CasterPressure**. PairPressure는 양쪽 OFF. 밝은 중성 환경, 1920×540, 60fps, 원속도 385 frames.

- [Side_AB_Full.mp4](../unity/Logs/PurpleIngredientCasterPressureQA/AB/20261005_160603_258/Side_AB_Full.mp4)
- [Caster_AB_Full.mp4](../unity/Logs/PurpleIngredientCasterPressureQA/AB/20261005_160603_258/Caster_AB_Full.mp4)
- [Observer_AB_Full.mp4](../unity/Logs/PurpleIngredientCasterPressureQA/AB/20261005_160603_258/Observer_AB_Full.mp4)
- 각 시점의 `*_Ingredient_Review.jpg`, `*_Ingredient_Adjacent.jpg`, `*_Release_Adjacent.jpg`에 준비/융합 직전/발사 전후 프레임을 정리했다. 미디어/QA 로그는 local-only다.
- `comparison_validation.json`, `validation-summary.json`, `ConsoleAfterPlayCanonical.json`, `ResourcesAfterPlayCanonical.json`에 수치와 이력을 보존했다.

## 변경 파일

기존: `unity/Assets/Scripts/Player/PrototypeHollowPurplePresentationRuntime.cs` — 후보 생성/sample/dispose만 연결.

신규(각 Unity 파일의 `.meta` 포함):

- `unity/Assets/Scripts/Player/PurpleIngredientCasterPressure.cs`
- `unity/Assets/Scripts/Player/PurpleIngredientCasterPressureProfile.cs`
- `unity/Assets/Resources/VFX/PurpleIngredientCasterPressure.shader`
- `unity/Assets/Resources/VFX/PurpleIngredientCasterPressureProfile.asset`
- `unity/Assets/Editor/PurpleIngredientCasterPressureProfileEditor.cs`
- `unity/Assets/Editor/PurpleIngredientCasterPressureCapture.cs`
- `unity/Assets/Editor/PurpleIngredientCasterPressureValidation.cs`
- `docs/PURPLE_INGREDIENT_CASTER_PRESSURE_REVIEW.md`

ASTRA ESCALATION 없음. LOCK이나 공용 rendering architecture를 변경하지 않았다. 캡처 이후 변경은 QA의 실행 phase 정정뿐이며 VFX 값/영상은 추가 튜닝·재렌더하지 않았다. 사용자 검수를 기다린다.
