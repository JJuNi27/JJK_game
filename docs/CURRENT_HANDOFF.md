# Current Project Handoff

## Canonical checkpoint — 2026-09-18

- Branch: `feat/gojo-blue-screen-distortion`
- Remote code checkpoint: `d2ee74e8c20da795c500971c393ac097b22a2e5a`
- Commit: `feat: checkpoint production hollow purple presentation`
- 상태: **CODEX VALIDATED / USER VISUAL APPROVAL PENDING**

프로젝트는 주술회전 팬텀 퍼레이드의 기술 연출과 애니/만화 원작의 술식·영역·패시브 규칙을 Unity 6 URP 3D에서 최대한 고증하고, Strikeborn급 이상의 타격감·presentation density·environment reaction·aftermath를 지향하는 넓은 도시형 1:1 캐릭터 액션 격투게임이다. Gojo는 첫 production-quality 캐릭터일 뿐 고정 주인공이 아니다.

### 현재 Production Purple

- Body: **D2-R3** — chunky plasma mass, fused white-out core, dynamic arc.
- Outer: **OuterR2** — irregular corona/aura, outward lightning, debris/hero fragments, 국소 distortion.
- Charge/travel에 D2-R3 + OuterR2를 연결했다. Terminal explosion은 기존 Production 표현을 유지한다.
- 새 발사 레퍼런스를 바탕으로 travel에 짧은 world-space residue, 굵고 짧은 magenta wake, release peak, rear-biased velocity response를 추가했다. Body의 구형 정체성은 유지한다.
- gameplay semantics는 유지한다: 약 2초 소환·융합, 약 3초 완성 charge, 기존 release/impact, 기존 damage/radius/range, corrected release origin, 첫 적중 종료, 뒤 target 피해 없음, 미적중 max-range explosion, cancel/camera restore, 기존 terminal behavior.

Production Integration은 `ProductionPurpleIntegrationTests` **3 PASS / 0 FAIL**, C#·shader error 0이다. 첫 적중, 뒤 target 피해 차단, no-hit max range terminal, repeated lifecycle cleanup, camera restore, caster/side readability를 확인했다. 고정 observer 영상은 장면 가림으로 시각 합격 근거에서 제외했다.

Travel refinement는 targeted **2 PASS / 0 FAIL**, C#·shader error 0, Charge pixel difference 0이다. first-hit/behind-target/max-range/repeated cleanup 정책도 유지됐다. 자동 검증은 사용자 Play Mode 시각 승인이 아니며 현재 Purple은 **USER VERIFIED가 아니다**.

### Iteration lineage

- A/B/C: 서로 다른 Body 방향 exploration.
- Hybrid D: dark compressed mass. 이후 self-destruct Purple 목표와 맞지 않아 주 방향에서 제외.
- D2: 밝고 폭주하는 Violent Luminous Mass로 전환.
- R1: surface/internal energy, hotspot, hero lightning 분리. broad white blowout은 감소.
- R2: 3D depth 개선. hairball/yarn surface 문제가 발생.
- R3: 고주파 hairball 제거, chunky plasma mass, fused white-out core, dynamic arc. 현재 Body 후보.
- OuterR1: smooth shield/bubble 인상으로 최종 방향에서 제외.
- OuterR2: irregular corona, outward lightning, debris/hero fragment, distortion. 현재 Outer 후보.
- Travel refinement: 애니 발사 레퍼런스 기반 short world-space residue, broad short wake, velocity-driven outer trailing.

### Local-only references

아래 파일은 Git에 올리지 않는다.

- `_local_refs/videos/gojo_mod_reference.mp4` — Blue/Red/Purple formation 및 sequence.
- `_local_refs/videos/strikeborn_reference.mp4` — presentation density와 impact philosophy.
- `_local_refs/videos/purple_selfdestruct_reference.mp4` — 밝고 폭주하는 self-destruct Purple Body.
- `_local_refs/videos/고죠 무라사키 날리는 장면.mp4` — Release→Travel residue, broad short wake, velocity trailing.

### Design review workflow

큰 VFX/art 판단에서 여러 방향이 타당하거나 reference 해석·camera·timing·gameplay semantics 선택이 필요하면 2~4개 후보를 제시하고 사용자가 Art/VFX Director로 선택한다. AI는 production translation·implementation·validation을 담당한다. Compile bug, 명백한 artifact, cleanup leak, 명백한 regression은 최소 수정할 수 있다.

### Domain future queue

- 검은 barrier sphere 크기를 Inspector/Data에서 자유롭게 조절 가능하게 한다.
- barrier destruction은 현재 비용이 커서 보류한다.
- 포획 시 Gojo hand sign/blindfold lowering, opponent reaction, 애니/Naruto Storm/Sparking Zero 계열 오의 camera를 후보로 검토한다.
- forced camera가 gameplay/camera safety를 깨지 않게 한다.
- 이 queue는 현재 Purple checkpoint와 섞어 구현하지 않는다.

### 다음 즉시 작업

최신 Travel refinement 영상에 대한 Gemini feedback과 사용자/ChatGPT 실제 영상 검수, Unity Play Mode 최종 시각 확인을 기다린다. 승인되면 Purple visual R&D를 종료한다. 승인 전 Blue/Red/Domain 새 visual track을 임의로 시작하지 않는다.

## 최신 작업 — Purple Release → Travel refinement / 사용자 시각 승인 대기

**2026-09-18:** 기존 Production 통합 위에서 Release 이후 외부 표현만 추가했다. 짧은 공간 잔류(최대 0.14초), 굵고 짧은 마젠타 wake, 속도에 따른 halo 후방 변형과 국소 굴절. D2-R3 Body·Charge OuterR2·gameplay·canonical sequence·camera·terminal 소스 보존.

- 최종 유효 targeted **2 PASS / 0 FAIL**, C#·shader 오류 0. lifecycle 3회, 첫 대상 HP45/뒤 대상 HP100, max-range·cancel·terminal legacy·material 증가 0 확인. 초기 render의 QA actor 위치 차이를 수정하고 렌더만 재검증. Full regression 없음.
- Charge On/Off RGB 차이 0. Travel 중앙 46프레임 435,390 pixel sample 차이 0. 측면 wake/후방 비대칭은 읽히며 일부 연무·원거리 residue 가독성·GPU 비용 미측정이 남는다.
- [최종 보고서](PURPLE_TRAVEL_REFINEMENT_REVIEW.md), [영상·비교](../unity/Logs/PurpleTravelRefinementQA/Final/comparison.html). Caster 4.03초, Side 이전/이후 2.03초. 격자 QA 무대이며 도시 시야 전체 재검증이 아님.
- 기존 파일은 `ProductionPurpleOuterRuntime.cs`와 상태 문서 2개만 task 시작 대비 변경. 보호 snapshot 나머지 754개 동일. **LOCAL / CODEX VALIDATED / PENDING USER VISUAL REVIEW**. Git 금지 작업 없음.

## 보존된 이전 작업 — Production Integration

## 최신 작업 — D2-R3 + OuterR2 Production Integration / 사용자 시각 승인 대기

**2026-09-18 결과 정리:** 중단 직전 최종 targeted 결과 **3 PASS / 0 FAIL / C#·shader 오류 0**을 확인했다. 새 조합은 기존 canonical sequence의 charge/travel에 명시적으로 적용하고 terminal은 기존 표현을 유지한다. 재개 후 소스 수정·Unity 재실행·새 Play Mode 캡처 없이 최종 RGB24의 인코딩과 보고만 완료했다.

- 5회 반복 lifecycle: material/mesh 증가 0, 관측 최대 particle 68, 최대 잔류 1.755m, 계측 travel 루프 managed allocation 0 bytes. 실제 무적중 max-range 종료 PASS. 기존 실제 first-target/뒤 target 피해 차단/camera restore 검사 1/1 PASS도 보존했다.
- 최종 고정 Observer 렌더는 장면 가림으로 판독 불가. Caster·측면 추적 영상은 제공하며 모든 관점 QA 완료로 주장하지 않는다. GPU frame-time과 실제 거리/배경 대비는 남은 리스크다.
- [최종 보고서](PURPLE_PRODUCTION_INTEGRATION_REVIEW.md), [최종 영상·비교](../unity/Logs/PurpleProductionIntegrationQA/Final/comparison.html).
- Production 파일 변경은 `PurpleEnergyBody.cs`, `PrototypeHollowPurplePresentationRuntime.cs` 두 개. 이전 Production/3차 baseline 49개 archive 보존, 기존 exploration/사용자 art/gameplay/Scene/Package/Settings 보존. 새 Profile의 통합 사용을 끄면 다음 시전부터 기존 표현 선택 가능.
- **LOCAL / CODEX VALIDATED / PENDING USER VISUAL REVIEW**. 로컬 Production 검수 후보이며 최종 출시 품질 승인 아님. 추가 polish와 Git 금지 작업 없이 사용자 확인 대기.

## 보존된 이전 작업 — OuterR2 exploration

## 최신 작업 — D2-R3-OuterR2 완료 / 사용자 시각 승인 대기

**2026-09-16:** 중단된 OuterR2를 현재 tree에서 이어서 마무리했다. R3 본체 LOCK, OuterR1·기존 결과 보존. 완료된 Halo는 재개 이후 수정하지 않았다. 실제 Hero 발생 구간을 확인한 뒤 크기·잔광만 최소 보정하고, 외향 번개 taper/flicker/분기 변화와 국소 왜곡을 순서대로 완료했다. **Production 미적용.**

- 화면 비교: 보호막 경계 감소, 큰 aura와 소수 Hero shard, 짧고 불규칙한 외부 방전, 밝은 격자에서의 국소 굴절. 남은 한계는 Halo 연무 인상, 가는 번개 끝의 aliasing, 파편·왜곡의 거리/배경 의존성이다.
- 최종 targeted **2/2 PASS / C#·shader error 0**. Full regression 없음. 중심부 Outer on/off RGB 차이 0. 보호 snapshot 문서 2개 외 **793개 동일**, R3 source/meta **16개 동일**, frozen baseline **49/49 동일**.
- [최종 보고서](PURPLE_D2_R3_OUTER_R2_REVIEW.md), [R3 / OuterR1 / OuterR2 비교](../unity/Logs/PurpleOuterR2QA/comparison.html). Front/Side/SlightLow still, Front/Orbit 3초, 실제 Hero 출현 영상 제공.
- 실행: VFXLab Play Mode → `Tools > JJK Game > VFXLab > Purple D2-R3-OuterR2 비교`.
- **LOCAL / CODEX PREVIEW / PENDING USER VISUAL REVIEW**. 반영 검토 가치는 있으나 사용자 확인 전 추가 polish·production 이식 없음. Git 금지 작업 없음.

## 보존된 이전 작업 — D2-R3-OuterR1

## 최신 작업 — D2-R3-OuterR1 완료 / 사용자 시각 승인 대기

**2026-09-16:** 사용자가 목표 방향에 도달했다고 판단한 R3 본체는 동결했다. 별도 OuterR1에서 **Halo → Particle Sparks → Particle Outward Lightning → Background Distortion** 순서로 추가·확인했다. Production 적용 없음.

- 실제 화면: 1.55배 구형 aura와 짧은 외향 방전으로 규모감 증가. 본체 중심부 on/off pixel 차이 0. Halo의 보호막 같은 매끈함, 아크의 정지 네온 인상, 작은 spark·distortion의 배경/거리 의존성은 남은 한계다.
- 최종 targeted **2/2 PASS / C#·shader error 0**. Full regression 없음. 보호 문서 2개 외 **707개 동일**, R3 소스/metadata **16개 동일**, 3차 baseline **49/49 동일**.
- [보고서](PURPLE_D2_R3_OUTER_R1_REVIEW.md), [Front/Orbit 3초 비교](../unity/Logs/PurpleOuterR1QA/comparison.html).
- 실행: VFXLab Play Mode → `Tools > JJK Game > VFXLab > Purple D2-R3-OuterR1 비교`.
- **LOCAL / CODEX PREVIEW / PENDING USER VISUAL REVIEW**. Production에 가져갈 가치가 있는 후보이나, 이식 전 Outer 전용 refinement 1회를 권한다. 사용자 확인 전 추가 작업 없음. 본체 재authoring 없음.

## 보존된 이전 작업 — D2-R3 본체

## 최신 작업 — D2-R3 최종 비교 완료 / 사용자 시각 승인 대기

**2026-09-16:** R2를 보존한 별도 R3. 가는 섬유 결을 큰 plasma mass로 변경하고 광원 사이 luminous connection, 약 0.08~0.15초의 dynamic arc event를 구현했다. 마지막 flow speed·국소 rupture 변화가 적용된 최종 결과를 확인했다. 재개 요청 이후 추가 시각 수정·Unity 재검증 없이 보고/비교 자료만 마무리했다.

- 실제 화면: hairball과 분리된 흰 구슬 인상 감소, 짧은 방전 출현·소멸 확인. 접힌 막 같은 면은 일부 남으며 광원 융합으로 개별 depth 구분은 약해진다. 레퍼런스의 거친 폭주감에는 아직 부족하다. **Production 적용 없음.**
- 최종 최소 targeted **2/2 PASS / C#·shader error 0**. Full regression 없음. 보호 snapshot 문서 2개 외 **634개 동일**, 3차 baseline **49/49 동일**. R2/R1/production·기존 결과 보존.
- [최종 보고서](PURPLE_D2_R3_REVIEW.md), [최종 비교 페이지](../unity/Logs/PurpleD2R3QA/Final/comparison.html). Front/Side/SlightLow 및 Front/Orbit 3초 비교 제공.
- 실행: VFXLab Play Mode → `Tools > JJK Game > VFXLab > Purple Hybrid D2-R3 비교`.
- **LOCAL / CODEX PREVIEW / PENDING USER VISUAL REVIEW**. 사용자 확인 전 추가 polish·production 이식 없음. Git 금지 작업 없음.

## 보존된 이전 작업 — D2-R2

## 최신 작업 — D2-R2 형태·질감 refinement / 사용자 시각 승인 대기

**2026-09-16:** R1을 동결하고 별도 R2를 추가했다. 평면 LineRenderer 대신 단면이 있는 3D arc mesh, 방향성 플라즈마 섬유, 부드러운 중앙 source cluster, 날카로운 검은 틈을 구현했다. D2/R1/production/3차 baseline은 보존했다.

- 실제 비교: 세포 무늬·작은 원형 core·평면 지그재그는 줄었고 찢어진 외곽·발광 덩어리·아크 앞뒤 가림은 개선됐다. 일부 결은 띠처럼 보이며 밝은 core 위의 아크 단면과 내향 흐름은 아직 약하다. 조건부 refinement 추천, production 적용 없음.
- 최종 최소 targeted **2/2 PASS / C#·shader error 0**. Full regression 없음. Front/Side/SlightLow 및 Front/Orbit 3초 비교 완료, 전체 PNG 19장. 보호 문서 2개 외 **574개 동일**, baseline **49/49 동일**.
- VFXLab Play Mode → `Tools > JJK Game > VFXLab > Purple Hybrid D2-R2 비교`. [보고서](PURPLE_D2_R2_REVIEW.md), [비교 페이지](../unity/Logs/PurpleD2R2QA/comparison.html).
- **LOCAL / CODEX PREVIEW / PENDING USER VISUAL REVIEW**. 기존 후보는 모두 보존했고 자동 production 이식은 하지 않는다. Git 금지 작업 없음.

## 보존된 이전 작업 — D2-R1

## 최신 작업 — D2-R1 refinement / 사용자 시각 승인 대기

**2026-09-15:** D2를 동결하고 별도 **D2-R1 — Internal Supernova + Violent Outer Energy**를 만들었다. 넓은 surface white를 내부 core + 서로 다른 깊이의 hotspot 3개로 분리하고, 제한된 Hero lightning·국소 외곽 파열·비동기 발광 변화를 추가했다. 밝은 Purple/Magenta와 구형 기반 유지. Production 적용 없음.

- 실제 비교: R1은 내부 광원·번개가 D2보다 잘 분리된다. D2보다 넓은 백색 폭주감은 약하다. 둥근 hotspot, 그래픽 지그재그처럼 보이는 번개, 약한 내향 가독성은 남은 한계다.
- 최소 targeted **2/2 PASS / C#·shader error 0**. 전체 regression 미실행. Unity 빌드 도구 오류 2회는 재시도 후 통과. 최종 3방향 still + Front/Orbit 3초 비교 완료; 전체 R1 PNG 14장.
- 기존 snapshot 문서 2개 외 **555개 동일**, 3차 baseline **49/49 동일**. D2/D/A/B/C, 사용자 prototype·Scene, production/gameplay/Package/Settings 보존.
- VFXLab Play Mode → `Tools > JJK Game > VFXLab > Purple Hybrid D2-R1 비교`. [보고서](PURPLE_D2_R1_REVIEW.md), [비교 페이지](../unity/Logs/PurpleD2R1QA/comparison.html).
- **LOCAL / CODEX PREVIEW / PENDING USER VISUAL REVIEW**. 다음 조정은 내부 spike 강화와 현재 깊이 분리의 균형을 사용자에게 선택받는다. 승인 없이 production 이식하지 않는다. Git 금지 작업 없음.

## 보존된 이전 작업 — D / D2

## 최신 작업 — Hybrid D / D2 비교 · 사용자 시각 선택 대기

**2026-09-15:** 별도 Hybrid D를 만든 뒤, 최신 첨부 지시에 따라 D를 동결하고 **Hybrid D2 — Violent Luminous Mass** exploration을 추가했다. Production·3차 baseline·A/B/C·D·사용자 prototype·Scene·gameplay 값·Package/ProjectSettings는 보존했다.

- D는 어두운 압축 질량, D2는 변형 plasma core·백색 hotspot·hot pink 파열·분기 번개가 주역이다. 실제 화면 차이는 명확하다. D2의 백색 합침, 내향 흐름 가독성, 밝은 배경 위 번개는 남은 한계다.
- D2 최종 최소 targeted **2/2 PASS**, C#·shader error **0**. 효율 지시에 따라 전체 regression을 실행하지 않았다. Front/Side/SlightLow still, Front/Orbit 0–3초 motion, 보존된 D와 비교 자료 완료.
- **LOCAL / CODEX PREVIEW / PENDING USER VISUAL REVIEW**. D2를 production에 임의 적용하지 않는다. 다음 수정 후보는 hotspot 분리 / 내향 통로 / 번개 가독성이다.
- Unity: VFXLab Play Mode → `Tools > JJK Game > VFXLab > Purple Hybrid D2 비교`. [보고서](PURPLE_HYBRID_D2_REVIEW.md), [비교 페이지](../unity/Logs/PurpleHybridD2QA/comparison.html).
- 3차 baseline 49/49 동일, D2 시작 snapshot 중 문서 2개 외 539개 동일. index 비어 있음. Git 금지 작업 없음.

## 보존된 이전 작업 — A/B/C 비교

## 최신 작업 — Purple Body A/B/C 비교 / 사용자 요소 선택 대기

**2026-09-15:** 중단된 Purple Body Prototype Exploration Pass를 기존 A/B/C에서 이어서 마무리했다. 아래 **3차 production baseline은 그대로 동결**되어 있다. 별도 exploration shader·profile·비교 창만 사용하며 production 적용은 없다.

- 실제 화면 차이: A는 작은 코어와 어두운 질량, B는 굵게 불규칙한 외곽, C는 내부층과 별도 바깥 파열. B의 빽빽한 가시를 줄이고 C의 층별 발광을 분리했다. C의 순수 내향 수렴 가독성은 부분 충족이며 회전·말려듦이 더 강하다.
- 최종 자료: Front / Side / Slight Low 정지 비교, 고정 Front 및 동일 Orbit 0–3초 motion, 후처리 OFF 포함 **558 PNG**. 이전 A/B/C와 QA 자료 보존, 새 테스트 실행은 고유 Runs 폴더에 저장.
- Prototype **3/3**, 기존 Purple·공유 영역 **36/36**, 관련 합계 **39/39 PASS**. 기존 production render-review는 이전 캡처를 보존하기 위해 재실행하지 않았다. C#·shader compile error 0.
- **LOCAL / CODEX PREVIEW / PENDING USER VISUAL REVIEW**. A의 암부·코어, B의 외곽, C의 내부 구조 중 가져갈 요소를 사용자가 선택하기 전에는 production을 수정하지 않는다.
- Unity: VFXLab Play Mode → `Tools > JJK Game > VFXLab > Purple 본체 A B C 비교`. 별도 render target 창이며 기존 카메라·사용자 Scene·`Purple_UserPrototype`을 변경하지 않는다.
- [비교 보고서](PURPLE_BODY_EXPLORATION_REVIEW.md), [정지·motion 비교 페이지](../unity/Logs/PurpleBodyExplorationQA/comparison.html). commit/push/merge/reset/clean/restore 없음, index 비어 있음.

## 이전 작업 — 3차 production baseline 보존 기록

작성 기준: **2026-09-15 Purple 3차 baseline 마무리**

## 최신 작업 — Hollow Purple 3차 baseline 고정

사용자의 마지막 요청은 **추가 시각 수정 없이 중단된 최종 regression / visual QA / 보고서만 마무리**하는 것이다. 현재 3차 구현을 baseline으로 보존했다. 다음 작업에서도 새 요청 없이 본체를 다시 authoring하지 않는다.

- 공통 시계: **2초 소환·융합 → 3초 완성 후 충전 → 8프레임/4컷 임팩트 → 5.1333초 발사**. 기본 travel 1.6초. 이전 0.32초 hold와 1.2067초 발사는 과거 값이다.
- 구형의 어두운 Purple, 소환용 Blue 흡입/Red 반발 흐름, caster camera와 observer 분리, 첫 적중/최대사거리 폭발을 구현했다. 기존 독립 Blue/Red·상흔 기반·사용자 Scene/모델/오디오 작업은 보존했다.
- **명시적으로 승인된 규칙 변경:** 첫 적중에서 본체 종료 및 뒤쪽 적 피해 중단. damage 55 / radius 3.2 / range 48 / push 34 / stun 1 / energy 45 및 gameplay Data Asset은 보존했다.
- 최종 관련 회귀 **37/37 PASS**, C#·shader compile error **0**. 실제 시전, 반복/lock/no-target, 첫 적중/최대사거리, 소환·충전 취소, 카메라 소유권/복귀, overlay/terminal/material 정리, 공유 Data/Inspector/beat clock 포함. 전체 프로젝트의 모든 테스트를 실행한 것은 아니다.
- 실제 Unity PNG **305장**, caster/observer/종점 프레임 검토 및 샘플 재생 자료 정리. **LOCAL / CODEX VALIDATED / PENDING USER VISUAL REVIEW**. 사용자 시각 승인은 아직 없다.
- LOCAL HEAD `a9c6ccf9c09b2effdd559e83e865296e45f600b2`, `feat/gojo-blue-screen-distortion`. index 비어 있음. reset/clean/restore/staging/commit/push/merge 없음.
- [3차 최종 보고서](PURPLE_THIRD_PRODUCTION_REVIEW.md), [최종 테스트](../unity/Logs/AstraCombatQA/purple-third-final.xml), [고정 baseline manifest](../unity/Logs/AstraCombatQA/purple-third-frozen-baseline.json).

## 이전 1·2차 작업 기록

**Purple 2차 피드백:** 사용자 요청에 따라 공통 pre-launch를 **1.2067초**로 확장했다. Blue 먼저 → Red 대치 → 융합 → 기존 hold 0.32초 → 흑백 4프레임 → 발사. 새 시간은 한글 GojoPolishSettings에서 조절하며 피해 발생도 이 시계에 동기화한다. 밝은 성운 인상을 줄이기 위해 외층 발광/haze를 줄이고 조밀한 어두운 질량과 작은 백색 코어로 변경했다. 1차의 빠른 발사 타이밍은 현재 기준이 아니다. 자세한 2차 결과는 아래 보고서 상단 참조.

2차 최종 `purple-second-final.xml`: **33/33 PASS / C#·shader compile error 0**. 단계/4프레임 명시 검사, 실제 전투·피해 경로, 카메라/취소/정리, scar, 공유 Data/Inspector 및 렌더 확인. **PENDING USER VISUAL REVIEW**. 아래 52/53 오디오 실패 기록은 1차 이력이며 수정하지 않았다.

사용자가 이전 VFX를 **4/10 / PROTOTYPE BASELINE**으로 평가해 Blue/Red 추가 작업을 중단하고 **Hollow Purple presentation 재authoring**을 요청했다. 이전 Blue/Red 변경은 보존되어 있으며 완료 승인 상태가 아니다.

- 실제 LOCAL HEAD: `a9c6ccf9c09b2effdd559e83e865296e45f600b2`, branch `feat/gojo-blue-screen-distortion`. cached origin ref는 동일하다. 이번 세션에서 remote를 fetch하거나 commit/push하지 않았다.
- Purple 전용 volume/filament/impact/scar shader, depth ribbon/번개, 짧은 camera lease, 불규칙 상흔 구현. gameplay/Data·formation 구조·0.32초 hold·앞선 ReleaseOrigin 수정 보존.
- **CODEX VALIDATED**: Purple 전용 검사 통과, 공유 범위 회귀 52/53 PASS. 기존 Audio Profile `skill1.voice`가 HEAD부터 비어 있는 것과 사용자 로컬 테스트의 기대값 불일치 1건은 미변경. 최종 hitch/실제 시전/취소 추가 2/2 PASS.
- **PENDING USER VISUAL REVIEW**: 시각 도약, 반전 강도, depth/번개, camera, scar. 주관적 품질을 USER VERIFIED로 올리지 않는다.
- 관련 없는 사용자 Scene/ProjectSettings/셰이더·문서·테스트와 gameplay Data 28개 보호 파일의 시작 해시 일치. reset/clean/restore/checkout/add/commit/push/merge 없음.
- 자세한 구현·로컬 렌더·검증·남은 항목: [PURPLE_PRODUCTION_REAUTHOR_REVIEW.md](PURPLE_PRODUCTION_REAUTHOR_REVIEW.md).

아래 branch/checkpoint 설명은 2026-09-13 이전 기록이다. 현재 상태는 위 기록과 실제 working tree를 우선한다.

> 이 문서가 현재 상태 판단의 최우선 기준이다.
> 2026-09-06 시점의 상세 historical handoff는
> `docs/archive/CURRENT_HANDOFF_2026-09-06.md`에 보존한다.

## 0. 현재 Branch / Remote / Local 구분

현재 작업 branch:

`feat/gojo-blue-screen-distortion`

최신 remote production-code checkpoint:

`142606ea3fe7a7298bb2ea1c54bb80efcaf170c6`
`feat: finalize Gojo P0-P6 presentation checkpoint`

2026-09-13 docs handoff checkpoint 이후에도 **Combat Data-driven migration + Inspector 한글화 구현은 사용자 PC LOCAL working tree에 있음**.

따라서 새 채팅/에이전트는:
- GitHub remote에 Data Profile 구현이 이미 올라왔다고 가정하지 말 것
- 먼저 `git status -sb`, `git log -1 --oneline`, 실제 diff를 확인할 것
- REMOTE / LOCAL / USER VERIFIED / CODEX VALIDATED를 구분할 것

## 1. 게임 한 문장 방향

**팬텀 퍼레이드의 기술 연출과 주술회전 원작의 술식/영역/패시브 규칙을 Unity 6 URP 3D로 최대한 고증하고, Strikeborn급 이상의 타격감과 연출 밀도를 지향하는 넓은 도시형 1:1 캐릭터 액션 격투게임.**

중요:
- Gojo는 첫 production-quality 캐릭터일 뿐 고정 주인공이 아니다.
- VFXLab도 Gojo 전용 scene이 아니라 장기적으로 공용 Character Presentation Lab이다.
- 공통 시스템은 Character name hardcoding보다 Trait / Passive / Domain rule / Data Profile을 우선한다.

상세:
- `docs/design/GAME_VISION.md`
- `docs/design/VFX_DIRECTION.md`
- `docs/architecture/COMBAT_DATA_DRIVEN.md`

## 2. 2026-09-13 공식 USER VERIFIED 상태

### Gojo movement
- Walk Speed **3**
- Run Speed **14**
- Blend Tree: Idle 0 @1.00 / Walk 3 @1.15 / Run 14 @1.00
- 일반 이동 Walk / Shift Run / Space Dodge

### P0 Unlimited Void physical gameplay — CLOSED
Domain ACTIVE 내부:
- 이동 정상
- Basic Attack 1 / 2 / Finisher 정상
- 실제 hit / damage 정상
- Space Dodge 정상
- Domain은 ACTIVE 상태 유지
- Blue / Red / Purple / Domain technique는 ACTIVE 동안 lock 유지

자연 종료 후:
- Basic Attack 정상
- Space Dodge 정상
- Technique burnout policy 유지 가능

### Presentation baseline
- Gojo Blue 1차 baseline + 4-hit gameplay
- Unlimited Void blue-black nebula background
- Hollow Purple formation / fusion baseline
- `Gojo_Blender_MasterAvatar` 기반 Humanoid pipeline
- Idle / Walk / Run 사용자 검증 완료

## 3. LOCAL USER VERIFIED — Combat Data-driven migration

사용자가 Unity에서 직접 확인:
- Data Asset의 damage 값 변경이 실제 runtime damage에 반영됨
- 이동 정상
- Basic Attack 정상
- Blue / Red / Purple 정상
- Domain 정상

따라서 **Combat Data-driven migration은 LOCAL USER VERIFIED**.

현재 LOCAL 핵심 Profile / Definition:
- `CharacterStatsProfile`
- `CursedEnergyProfile`
- `BasicAttackProfile` + variable `BasicAttackStep[]`
- `GojoTechniqueGameplayProfile`
- `DomainGameplayProfile`
- `BurnoutPolicyProfile`
- `TargetingProfile`
- `TrainingBotProfile`
- `CharacterCombatDefinition`
- `CharacterCombatCatalog`
- Trait / Passive 확장 seam

Gojo Data Asset 위치:
`unity/Assets/Characters/Gojo/Data/`

주요 asset:
- `Gojo Character Stats.asset`
- `Gojo Cursed Energy Profile.asset`
- `Gojo Basic Attack Profile.asset`
- `Gojo Technique Gameplay Profile.asset`
- `Gojo Unlimited Void Gameplay.asset`
- `Gojo Burnout Policy.asset`
- `Gojo Targeting Profile.asset`
- `Gojo Combat Definition.asset`

공용 LOCAL asset:
- `Assets/Characters/Common/Data/Training Bot Normal Profile.asset`
- `Assets/Characters/Common/Data/Training Bot Domain Amplification Profile.asset`
- `Assets/Resources/CombatData/Character Combat Catalog.asset`

Runtime 연결:
- `Health` → Character Stats
- `BasicAttack` → BasicAttackProfile / AttackStep[]
- `CursedEnergyController` → CursedEnergyProfile
- `GojoTechniqueController` → Blue / Red gameplay data
- `GojoTechniqueChainController` → Purple / Blue→Red synergy
- `GojoDomainController` → Domain gameplay data
- `TechniqueBurnoutController` → Burnout policy
- `TargetLockController` → Targeting profile
- `CurseBotController` → TrainingBotProfile
- `PrototypeCharacterController` → Catalog → CharacterCombatDefinition → individual profiles

검증:
- Unity compile error **0**
- data smoke **3/3**
- full Unity EditMode regression **50/50**
- `git diff --check` 이상 없음
- 사용자 실제 gameplay 검증 완료

## 4. LOCAL USER VERIFIED — Inspector 완전 한글화

사용자가 실제 Unity Inspector에서 한글화가 정상 적용된 것을 확인함.
따라서 이전의 `CODEX VALIDATED / USER VISUAL CHECK PENDING` 상태는 종료하고
**LOCAL USER VERIFIED**로 승격한다.

구현/검증 내용:
- 10개 신규 Data Profile용 scoped `CustomEditor`
- `BasicAttackStep`, Blue / Red / Purple / Blue→Red synergy `PropertyDrawer`
- 평타 `cooldown`은 문맥상 `입력 간격`으로 표시
- Pull/Push는 실제 velocity 의미에 맞춰 `끌어당김 속도` / `밀어내기 속도`
- C# identifier / serialization key / Asset filename은 영어 유지
- localization focused tests **24/24**
- 실제 13개 asset `Editor.CreateEditor` + `SerializedObject` 검증
- 대상 13개 `.asset` SHA-256 전후 불일치 **0**
- runtime / Scene / VFX / Audio 변경 없음
- compile error **0**
- 사용자 Inspector 시각 확인 정상

## 5. 이번 구조화로 제거한 주요 Hardcoding

LOCAL migration에서 제거/일반화:
- `chainIndex >= 2` 기반 basic attack 3타 고정 전제
- `ATTACK CHAIN n / 3` 고정 표시
- Gojo controller들의 반복적인 Six Eyes CE profile 강제 적용
- `CurseBotController`의 `name.Contains("_B")` gameplay ability 결정
- HUD `BONUS +12` 중복 숫자
- Red spawn gameplay 값의 production constant 직접 의존

핵심 원칙:
**코드는 HOW / Data Asset은 WHAT.**

밸런스나 캐릭터별 설정을 바꾸기 위해 C#을 열어야 한다면 먼저 Data ownership을 의심한다.

## 6. 영구 Inspector / Data 작성 규칙

이후 새 Profile / ScriptableObject / 사용자 설정 UI를 만들 때 처음부터 적용:
- C# class / field / enum identifier: **영어 유지**
- Unity Inspector / Data Asset / 사용자가 직접 만지는 설정 UI: **한국어 표시가 기본**
- Header만 한국어이고 실제 field label이 영어라면 **미완성**
- 새 Data Profile 생성 시 한글 Inspector를 동시에 구현
- 일반 serialized field label을 `InspectorNameAttribute`만으로 해결하지 않음
- 기존 `SerializedProperty` + scoped `CustomEditor` / `PropertyDrawer` + Korean `GUIContent` 흐름 우선
- Undo / prefab override / object picker / enum / array / foldout / Range / Min 보존
- 사용자가 자주 조절할 값은 Data/Inspector로, 알고리즘 내부 epsilon/수학 상수는 코드에 둘 수 있음

## 7. Hardcoding 분류 규칙

A. `MIGRATE NOW`
- 캐릭터/기술/봇/밸런스 등 사용자 조절 gameplay data

B. `ASTRA / PRESENTATION 이후 MIGRATE`
- camera shake / FOV / hit-stop / flash / technique presentation color
- Infinity ripple presentation
- Target Lock indicator presentation
- VFX presentation tuning

C. `KEEP IN CODE`
- epsilon
- clamp
- state transition mechanics
- collection/event plumbing
- 실제 알고리즘 내부 상수

LOCAL audit:
`docs/COMBAT_HARDCODING_AUDIT.md`
이 파일은 아직 remote에 있다고 가정하지 않는다.

남은 대표 debt:
- `worldDeathY` → 향후 `ArenaRules`
- Presentation feedback → Astra polish 후 `TechniquePresentationProfile` / `CombatCameraProfile`
- 비이관 Sukuna/Megumi CE legacy fallback
- prototype 캐릭터별 component enable/disable switch
- gameplay가 아닌 roster/team sorting `_B` convention

## 8. Domain 아키텍처 — 절대 합치지 않음

독립 유지:
1. **Gameplay Capture Radius** — 누가 포획되는가
2. **Visual Barrier Radius / Diameter** — 원래 세계의 검은 구형 결계 크기
3. **Domain Interior Space** — 별도 pocket-dimension 내부 공간 크기

Barrier 크기를 바꿔도 capture 판정이나 interior 크기가 자동으로 바뀌면 안 된다.

의도 흐름:
`Original World → cast/capture → black barrier → transition/cinematic → purple tunnel → isolated Unlimited Void interior → gameplay → safe return`

현재 barrier HP / destruction / outside rescue / simultaneous inside-outside combat은 범위 밖.

상세:
- `docs/architecture/DOMAIN_SYSTEM.md`
- `docs/architecture/CAMERA_PRESENTATION.md`

## 9. 다음 Astra visual track

품질 기준:
**원작 정체성 + Strikeborn급 이상의 impact density / environment reaction / aftermath.**

### Blue
1차 baseline 보호 + 2차 enhancement:
- environment inward reaction
- 4-hit별 더 읽히는 beat
- final collapse peak
- restrained distortion/camera
- residual spatial shimmer / dust pull

### Red
Repulsion identity 유지:
- compressed anticipation
- violent release
- pressure-reactive travel
- impact flash
- irregular repulsive shockwave
- outward debris/dust
- gray-white pressure vapor
- aftermath

불꽃 기술처럼 만들지 않는다.

### Hollow Purple
보호:
- formation / fusion
- current hold baseline

남은 실제 문제:
- 발사 시 약간 아래 방향으로 나가는 launch-axis 문제
- 실제 anchor/release vector 원인을 수정
- 눈속임 lateral offset으로 덮지 않음

2차 enhancement:
- release compression / flash
- travel distortion
- branching violet lightning
- environment reaction
- scar / residual electricity / spatial shimmer

### Unlimited Void
보호:
- blue-black nebula background
- capture/barrier/interior/restoration architecture

남은 visual:
- Cosmic Eye 전체 scale/intensity heartbeat/breathing pulse 제거
- cloud/rim/tail subtle flow 유지
- release 중 ceiling/dome/shell artifact 실제 원인 제거
- caster-centered world-space release 유지
- White Blood `퉁 → pause → 투둥 → pause → 퉁` 3-group beat
- White Blood timing은 Data/Inspector 조절 가능 유지

상세:
`docs/design/VFX_DIRECTION.md`

## 10. Gojo 모델 / Animation 지속 규칙

현재 기준 pipeline:
- actual model: `Gojo_Blender_Master`
- Avatar: `Gojo_Blender_MasterAvatar`
- Animator: `Gojo_Animator`
- Apply Root Motion: OFF
- 동일 Blender Armature에서 export한 animation FBX는
  `Humanoid → Copy From Other Avatar → Gojo_Blender_MasterAvatar`
- 기존 `Gojo_Blindfold_RiggedAvatar`와 Blender-export animation FBX를 섞지 않는다.

Local/copyright-sensitive asset:
- `unity/Assets/LocalModels/`
- `unity/Assets/Resources/LocalAudio/`
- root `사운드 모음/`
- `.blend`, local FBX / texture / audio

명시적 승인 없이 공개 Git에 올리거나 덮어쓰지 않는다.

## 11. 다음 안전한 작업 순서

현재 LOCAL Data migration + Inspector localization은 모두 사용자 검증 완료.

권장 순서:
1. `git status -sb`로 LOCAL dirty tree 재확인
2. Data migration + localization 관련 파일만 **selective staging**
3. `git diff --cached --name-status` 검토
4. 구조 checkpoint commit / push
5. 그 뒤 Astra visual track 시작

절대 `git add .`로 LOCAL Scene/VFX/Audio/Model/unrelated 작업을 섞지 않는다.
reset / clean / checkout으로 사용자 로컬 작업을 제거하지 않는다.

## 12. 문서 우선순위

새 채팅은 다음 순서로 읽는다.

1. `AGENTS.md`
2. `docs/design/GAME_VISION.md`
3. `docs/CURRENT_HANDOFF.md`
4. `docs/active/CURRENT_TASK.md`
5. `docs/locked/USER_VERIFIED_SETTINGS.md`
6. 작업별 architecture 문서

`docs/DEVELOPMENT_ROADMAP.md`와 오래된 Gate/Pass 문서는 historical context로 참고할 수 있으나,
현재 상태가 충돌하면 위 최신 문서가 우선한다.
