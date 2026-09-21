# 고죠 VFX 아키텍처 / 현재 기준

## Purple Ingredient Final2 checkpoint — 2026-09-22

상태는 **CODEX VALIDATED / USER VISUAL REVIEW INCOMPLETE / NOT USER VERIFIED**다. 이번 저장 범위는 Final2 단독이 아니라 Polish → Macro → Reboot → Reboot2 → Final → Final2 누적 Ingredient chain이다. Final2는 technical checkpoint이며 최종 visual baseline이 아니다.

Blue/Red dark mass 방향, fusion trajectory ±0.08m, fusion/collision timing 및 production Purple regression protection을 보존한다. Known issue는 reference보다 약한 outer energy, 일부 화면의 Red glowing mesh/plastic shard 인상, reference의 violent/unstable cursed-energy motion에 못 미치는 주변 흐름이다. 향후 Skills/MCP/Reference Breakdown 도입 후 시각 개선을 재개한다.

작성 기준: **2026-09-18**

이 문서는 presentation과 gameplay를 분리하고, 현재 채택 후보와 보호 규칙을 기록한다. 자동 테스트 통과를 USER VERIFIED라고 표현하지 않는다.

## Purple Formation Ingredient 후보 (2026-09-19)

**2026-09-20 최종 파열·충돌 후보:** `PurpleIngredientFinalProfile` 기본 OFF. Pass 2가 활성화됐을 때만 기존 Ingredient의 pressure shader/10개 curve를 `HollowPurpleIngredientRupture` / `PurpleIngredientFinalFlow`로 선택한다. 기존 mote 경로는 유지한다. 접촉 accent는 새 `PurpleIngredientCollisionFinal` shader만 선택하며 timing/duration/camera 소유권은 그대로다. Dark surface와 canonical sequence source는 이번 패스에서 수정하지 않았다. [최종 비교·검증·한계](../PURPLE_INGREDIENT_FINAL_REVIEW.md). 기존 Pass 2 자산은 보존한다.

**2026-09-20 Pass 2:** `PurpleIngredientReboot2Profile` 기본 OFF. Reboot Body shader/profile을 그대로 사용하고 전용 pressure2 shader/flow로 외곽만 강화한다. 사용자 승인 범위 내에서 canonical merge의 additive Y arc만 Blue +0.45/Red -0.1575m에서 ±0.08m로 바꾼다. 나머지 position/time/scale/hold/production 분기는 보존한다. Root-owned `PurpleIngredientCollisionAccent`는 명목 sphere 첫 접촉(현재 약 1.444초)을 계산하여 2/60초 world-local 흑백 집중선을 표시하며 camera lease/Canvas를 생성하지 않는다. Birth 시작 1.945초 전에 종료한다. [Pass 2 구현/QA/한계](../PURPLE_INGREDIENT_REBOOT2_REVIEW.md).

완성 Purple의 **Outer Lightning-Storm Re-evaluation은 후속 메모만** 있으며 이번 구현에 포함하지 않는다.

최신 Direction Reboot는 사용자가 이전 bright-core 방향을 대체하도록 명시한 별도 후보다. `PurpleIngredientRebootProfile` 기본 OFF, 이전 후보보다 우선한다. 새 opaque surface에서 Blue/Red 모두 dark mass를 사용하고 Red white core를 제거한다. 기존 curve 6개는 입체적인 partial collapse/pressure band로, 나머지 curve/mote는 보조 흐름으로 사용한다. Alpha blend용 pressure material이 Ingredient당 1개 추가되고 OnDestroy에서 해제된다. 후기 fusion에서는 원래 filament/Birth compression으로 복귀한다. 기존 후보 자산/확정 Purple은 보존한다. [Reboot 실제 비교·검증·한계](../PURPLE_INGREDIENT_REBOOT_REVIEW.md).

2차 Macro 후보는 별도 `PurpleIngredientMacroProfile`(기본 OFF)과 `HollowPurpleIngredientMacro` shader를 사용한다. `PurpleFusionIngredient`에만 hook이 있고 독립 기술은 참조하지 않는다. 기존 31개 line의 경로/폭/발광을 바꾸며 Macro mote는 6점 trail이다. 후기 fusion에서는 기존 Birth compression으로 돌아간다. 비교 렌더는 기존 Ingredient 후보 ON을 공통으로 두고 Macro OFF/ON으로 비교했다. 기존 profile/shader를 덮어쓰지 않았다. [Macro 결과 및 남은 한계](../PURPLE_INGREDIENT_MACRO_REVIEW.md). 확정 Purple, timing/path/size, gameplay/camera/terminal은 보호한다.

`PurpleFusionIngredient`만 `PurpleIngredientPolishProfile`의 opt-in 후보를 읽는다. 기본 OFF. 원본 `HollowPurpleIngredient` shader는 보존하고 후보 전용 `HollowPurpleIngredientPolish`를 사용한다. 기존 curve 10/mote 20/bridge 1개를 재사용하며, Blue dark sink와 Red additive pressure core를 구분한다. Late-fusion compression 진입 전 flow 보강을 원래 값으로 blend back한다. Sphere transform/formation timing/path/Body/Birth/standalone Blue·Red에는 쓰기하지 않는다. [실제 비교 및 검증](../PURPLE_INGREDIENT_POLISH_REVIEW.md).

## 로컬 opt-in 후보 — Final Polish (2026-09-18)

Production baseline 위에 `PurpleFinalPolishProfile`의 독립 `outerPolishEnabled` / `fusionBirthEnabled` hook을 추가했다. **두 값 모두 기본 OFF**, 사용자 시각 검토 전이다. 기존 Production/Travel profile은 바뀌지 않는다.

Outer 후보는 `ProductionPurpleOuterRuntime.Configure`에서 별도 outer settings와 `PurplePolishHalo`만 선택한다. Body 설정과 residual architecture는 그대로다. Fusion 후보는 canonical sequence가 기존 ingredient mote의 압축 강조를 요청하고, root 소유 `PurpleFusionBirthAccent`를 hold 시작 전후에만 sample한다. 기존 Blue/Red transform, duration, hold, release, camera, terminal 및 damage code를 수정하지 않는다. Ingredient 비활성화 이후의 outward accent 때문에 새 layer가 필요하며 Dispose 시 모든 자원이 root와 함께 제거된다.

이 후보를 기존 R3/OuterR2 baseline 승인으로 혼동하지 않는다. 실제 비교, 성능 범위, shader 보정 이력 및 남은 보호막 인상은 [최종 Polish 보고서](../PURPLE_FINAL_POLISH_REVIEW.md)를 참조한다.

## 전체 방향

원작 정체성, 강한 가독성, 아우라, 타격감, 깊이와 움직임을 우선한다. 기술은 생성→이동→폭발만으로 끝내지 않고 environment reaction, aftermath, camera/screen feedback까지 평가한다. Gojo는 첫 production-quality 캐릭터이며 프로젝트의 고정 주인공은 아니다.

## Blue / 아오

현재 1차 baseline은 **USER VERIFIED**다. 4-hit gameplay, attraction/singularity, core, debris와 기본 silhouette를 보호한다. 2차 polish는 별도 승인된 track에서 진행한다.

## Red / 아카

Repulsion identity를 유지한다. 향후 enemy/character, destructible world, hard world solid, trigger/ignore, max range를 구분하는 collision response가 적합하다. 잔류는 불꽃 연기보다 밀려나는 회백색 pressure vapor와 debris를 지향한다. 현재 Purple 승인 과정과 섞어 수정하지 않는다.

## Hollow Purple

상태: **CODEX VALIDATED / USER VISUAL APPROVAL PENDING**.

### Body

Production charge/travel Body는 **D2-R3**다. Chunky plasma mass, fused white-out core, 비동기 internal flow와 짧은 dynamic arc가 핵심이다. R2의 hairball/yarn 인상을 줄인 최종 Body 후보다.

### Outer

Production charge/travel Outer는 **OuterR2**다. 불균일한 corona/aura, outward lightning, 소수 hero fragment와 작은 plasma shard, 국소 background distortion을 사용한다. OuterR1의 smooth shield/bubble 인상은 최종 방향에서 제외했다.

### Charge

D2-R3 + OuterR2의 정적 visual과 기존 canonical buildup을 유지한다. Blue 선행→Red 등장/대치→fusion→완성 Purple hold→impact frames→release 순서다. Travel 전용 shader와 wake는 Release 이전에 활성화하지 않는다.

### Travel

`_local_refs/videos/고죠 무라사키 날리는 장면.mp4`의 발사 구간을 바탕으로 presentation만 확장했다.

- 일부 outward lightning/spark/fragment는 직선·등속 production 경로에서 발생 위치에 최대 0.14초 잔류한다.
- 굵고 짧은 magenta plasma wake를 사용하며 긴 comet/fireball/smoke/laser trail은 피한다.
- Release 순간이 sustained travel보다 강하다.
- Outer energy는 앞쪽이 압축되고 뒤쪽이 끌리며, Body Transform 자체는 늘리지 않는다.
- Distortion은 뒤쪽의 국소 보조 효과다.

### Terminal

First-hit 또는 max-range terminal explosion은 **기존 Production 표현**을 유지한다. D2-R3 + OuterR2 및 travel 전용 wake/stretch/residue를 terminal core에 적용하지 않는다.

### Gameplay semantics

Presentation 변경과 분리해 다음을 보호한다.

- 기존 damage, gameplay radius, range, push/stun, energy cost.
- corrected central release origin과 수평 launch axis.
- 첫 living target 접촉 시 본체 종료 및 terminal 생성.
- 뒤쪽 target damage 없음.
- 미적중 시 max-range terminal.
- cancel, cleanup, camera restore와 repeated-cast cleanup.

## 구현 기술

현재 프로젝트에는 VFX Graph package가 없다. Purple은 **Unity 6 URP + custom Shader + 기본 Particle System/mesh** 구조로 구현했다. VFX Graph 설치를 다음 작업의 전제로 두지 않는다.

사용자가 조절하는 presentation 값은 Profile/Data Asset과 한글 Inspector를 제공한다. 코드는 생성·sampling·lifecycle을 담당한다.

## 검증 기록

- Production integration targeted: **3 PASS / 0 FAIL**, C#·shader errors 0.
- Travel refinement targeted: **2 PASS / 0 FAIL**, C#·shader errors 0, Charge pixel difference 0.
- Caster/Side readability 확인. 고정 observer 결과는 장면 가림 때문에 합격 근거에서 제외.
- GPU frame-time, 복잡한 실제 도시 배경의 원거리 residue/distortion, wake의 일부 연무 인상은 남은 리스크다.

상세 근거는 `docs/PURPLE_PRODUCTION_INTEGRATION_REVIEW.md`와 `docs/PURPLE_TRAVEL_REFINEMENT_REVIEW.md`를 우선한다.
