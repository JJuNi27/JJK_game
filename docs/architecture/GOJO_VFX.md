# 고죠 VFX 아키텍처 / 현재 기준

작성 기준: **2026-09-18**

이 문서는 presentation과 gameplay를 분리하고, 현재 채택 후보와 보호 규칙을 기록한다. 자동 테스트 통과를 USER VERIFIED라고 표현하지 않는다.

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
