# Current Project Handoff

작성 기준: 2026-09-04

## 현재 단계

Gate 5B First Production-Quality Vertical Slice 진행 중.

현재 VFX 상태:

- Pass 8 JJK-Referenced Production Particle VFX Runtime:
  USER VISUAL REVIEW FAILED / REWORK REQUIRED
- Pass 8R-1 Gojo Signature VFX Reference-First Rework:
  USER VISUAL PARTIAL / DETAIL REWORK REQUIRED
- Pass 8R-2A Gojo Blue Production VFX Benchmark:
  REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
- Gojo Blue Localized Screen Distortion:
  REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
- Gojo Blue Visual-Only Environment Suction:
  REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
- Gojo Blue Final Timing / Readability Tuning:
  REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
- Gojo Blue Stylized Suction Wind Layer:
  REMOTE IMPLEMENTED / USER VISUAL TEST PENDING

Pass 8R-1은 Pass 8 대비 방향성은 개선됐지만 final-quality VFX로 승인되지 않았다.

## 사용자 시각 피드백 핵심

- Blue/Red/Purple/Unlimited Void 모두 Pass 8 대비 개선됐지만 디테일이 부족함
- primitive sphere / flat material / simple particle / line 느낌이 아직 강함
- 저퀄리티 “뿅뿅 발사” 인상을 벗어나려면 shader / distortion / layered timing이 필요
- Sukuna slash 계열은 상대적으로 개선되었으므로 현재 rework 대상에서 제외
- localized distortion은 오류 없이 동작하지만 field 강도가 흐림처럼 보일 정도로 약했음
- `FastClockwiseInwardStreaks`의 긴 stretched billboard가 laser bar처럼 보여 우선 rework 대상이 됨
- distortion `0.19`와 procedural particle mask는 개선됐지만 Blue 주변 공간이 비어
  “파란 구체 + arc”처럼 보이는 문제가 다음 우선순위가 됨

## 기준 문서

- docs/JJK_VFX_REFERENCE_GOJO.md
- docs/JJK_VFX_EXTERNAL_IMPLEMENTATION_RESEARCH.md
- docs/NEW_CHAT_BOOTSTRAP.md
- docs/GATE5B_FIRST_PRODUCTION_SLICE.md
- docs/DEVELOPMENT_ROADMAP.md
- 사용자 조사 요약: Unity 고죠 VFX 구현 조사 요약
- docs/JJK_VFX_EXTERNAL_REFERENCE_SURVEY.md

## 외부 조사에서 확인된 다음 방향

좋은 Gojo VFX는 단일 ParticleSystem이 아니라 다음을 중첩하는 방식이 적합:

- Mesh / Sphere or custom mesh
- Shader / Shader Graph
- Particle System / VFX Graph
- Screen Distortion
- Post Processing
- Camera / feedback
- Timing

기술별 핵심:

Blue:
- Core shader
- inward + spiral particle
- distortion

Red:
- core
- outward particle
- layered shockwave
- distortion

Hollow Purple:
- Blue/Red fusion
- flash
- purple energy body
- short trail
- distortion

Unlimited Void:
- dedicated environment
- galaxy/star/void layers
- screen-space transition
- lighting

## 현재 렌더링/패키지 제약

- Unity 6 / URP 17.3
- VFX Graph 패키지는 현재 `Packages/manifest.json`에 설치되어 있지 않음
- PC Renderer에는 현재 Screen Space Ambient Occlusion만 Renderer Feature로 등록되어 있음
- Full Screen Pass / distortion Renderer Feature는 아직 구성되어 있지 않음
- 따라서 다음 pass에서 VFX Graph나 Full Screen Pass를 전제로 코드를 쓰지 말고,
  기존 ParticleSystem + custom URP shader를 먼저 production benchmark로 만든 뒤
  distortion pipeline은 별도 검증 가능한 단계로 추가할 것

## 다음 작업

Pass 8R-2를 단순 detail tuning이 아니라:

Gojo VFX Production Pipeline Upgrade

로 진행.

한 번에 네 기술을 모두 크게 고치지 않는다.

순서:

1. Blue Production VFX
2. Red Production VFX
3. Hollow Purple Production VFX
4. Unlimited Void Production Environment / Transition

Blue 하나에서 먼저 production-quality 기준을 만든 뒤 다음 기술에 같은 파이프라인을 재사용한다.

## Pass 8R-2A 구현 handoff

Blue에만 다음 production benchmark를 원격 구현했다.

- Resources에 포함되는 custom URP procedural energy shader
- deep body / Fresnel shell / distortion-like outer shell 3-layer core
- scene-owned shared material template + renderer별 MaterialPropertyBlock
- fast clockwise inward streak + slow counter spiral mote
- 서로 다른 길이/폭/alpha/timing을 가진 outer/mid broken arc 8개
- outward burst가 아닌 impact collapse
- renderer feature와 분리된 `GojoBlueDistortionSource` 기반 localized screen distortion shell
- procedural mask 기반 Blue 전용 particle shader와 shared material template
- presentation-only ground dust와 sparse dark debris inward suction layer

VFX Graph, Full Screen Pass, PC Renderer YAML, external texture는 추가하지 않았다.
Red/Purple/Unlimited Void/Sukuna/Megumi와 gameplay 값도 변경하지 않았다.

Blue benchmark를 Unity에서 처음 실행하면서 실제 runtime/shader 문제 2종을 발견했고 master에서 수정했다.

- `6487acd`: Particle orbital X/Y/Z MinMaxCurve mode 불일치 + 생성 직후 playing 상태에서 duration 변경 오류 수정
- `8247db9`: HLSL/D3D11에서 예약어와 충돌한 shader 함수 인자 `point`를 `samplePos`로 변경

이후 사용자는 실제 Blue effect를 확인할 수 있는 상태까지 도달했다.
다만 Blue를 USER VERIFIED로 승인한 것은 아니다. 현재는 외부 고퀄 Unity/Unreal 구현과 비교하면서
detail ceiling과 다음 polish 방향을 정하는 단계다.

추가 외부 구현 조사 결과는:
`docs/JJK_VFX_EXTERNAL_IMPLEMENTATION_RESEARCH.md`

에 기록했다.

현재 reference 역할:
- starkim1999: lore/readability + inward/outward motion + distortion + Domain tunnel/environment
- Zepwlert "The Strongest Sorcerer": 236 ParticleSystem, screen-space/distortion/sound를 겹친 layer-rich benchmark
- Floppiii: particle texture / anime particle shader / screen-space cinematic set-piece benchmark
- Swammy JJK Fan Game (UE5.4+): 실제 action-game scale/readability/integration benchmark
- Deadlyxrebel Hollow Purple: debris suction / material cracking / impact decal / stage transition detail 아이디어

현재 다음 단계는 Blue를 바로 완료 처리하는 것이 아니라
`Blue Production VFX Benchmark 2차 polish`를 검토하는 것이다.

권장 우선순위:
1. visual-only dust/debris suction Unity 시각 검수
2. field distortion과 short tapered particle 유지 확인
3. dust/debris density와 readability 최종 튜닝
4. presentation layer onset stagger
5. 사용자 영상/스크린샷 기반 final art tuning

Blue가 사용자 합격하기 전에는 Red production pass로 넘어가지 않는다.

## Gojo Blue Localized Screen Distortion

상태:

```text
REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
```

- `GojoBlueDistortion.shader`가 URP `_CameraOpaqueTexture`를 실제 sample한다.
- Blue instance마다 `BlueScreenDistortionShell`을 만들고 world radius/strength/impact metadata를
  `MaterialPropertyBlock`으로 shader에 연결한다.
- field Blue base strength는 사용자 피드백에 따라 `0.15 → 0.19`로 조정했고,
  impact base strength `0.24`는 유지한다.
- distortion shell은 Blue energy/particle보다 먼저 그려지고 Screen Space Overlay HUD에는 영향을 주지 않는다.
- PC RP asset의 기존 opaque texture 설정을 사용하며 Renderer Feature/YAML은 변경하지 않았다.
- shell과 binding은 기존 `GojoBlueVfxInstance` lifetime/fade를 그대로 따른다.
- Unity shader compile과 실제 시각 결과는 아직 사용자 검수 전이다.

## Gojo Blue Procedural Particle Mask 1차

상태:

```text
REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
```

- 외부 texture 없이 `GojoBlueParticle.shader`의 procedural UV mask를 사용한다.
- 하나의 shared material template에서 soft mote / tapered streak / broken wisp를 renderer별
  `MaterialPropertyBlock` mode로 선택한다.
- Fast streak의 inward speed는 유지하면서 lifetime, velocity stretch, length scale을 줄였다.
- SlowCounterSpiralMotes는 soft mote, BlueCorona는 random-rotation broken wisp를 사용한다.
- generic particle factory의 기본 material/stretch 값과 다른 technique VFX는 변경하지 않았다.
- 실제 particle silhouette, warning, replay/cleanup 결과는 Unity 사용자 검수 전이다.

## Gojo Blue Visual-Only Environment Suction

상태:

```text
REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
```

- `GojoBlueVfxInstance`가 `GroundDustSuction`과 `DarkDebrisFragments` ParticleSystem을 직접 소유한다.
- dust는 Blue anchor 기준 낮은 얇은 영역에서 soft mote로 떠오르며 중심으로 수렴한다.
- debris는 sparse dark blue-gray fragment로 생성되어 약하게 orbit한 뒤 중심에서 축소·소멸한다.
- inward radial speed가 orbit보다 크고 noise는 가장 약하게 제한된다.
- impact cue는 짧은 lifetime/높은 inward speed와 normalized compression 기반 simulation 가속을 사용한다.
- `GojoBlueParticle.shader`에 procedural `DarkFragment` mode 하나만 추가했다.
- Rigidbody, collider, Physics query, scene object 이동, gameplay 값 변경은 없다.
- 두 system은 기존 Blue instance particle/material lifecycle과 cleanup을 그대로 따른다.
- 실제 밀도, 바닥 착시, fighter readability와 cleanup 결과는 Unity 사용자 검수 전이다.

## Gojo Blue Final Timing / Readability Tuning

상태:

```text
REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
```

- gameplay duration이나 spawn request를 바꾸지 않고 `GojoBlueVfxInstance` 내부 normalized time만 사용한다.
- field는 dense core 0~10% → Fresnel/distortion 8~22% → outer/corona 12~28% → dust 18~34%
  → debris 22~38% → slow spiral 24~42% → arc 30~50% → fast spiral 42~58% 순서다.
- field distortion base `0.19`와 impact base `0.24`는 유지하며 field distortion만 거의 0에서 시작한다.
- field convergence arc는 core/environment 뒤에 등장하며 alpha scale을 `0.80`으로 낮췄다.
- dust의 alpha/size/lift와 debris의 size/blue-gray 대비/lift를 높였고 particle 수와 inward 속도는
  크게 늘리지 않았다.
- impact는 0~20% hold, 20~75% collapse 가속, lifecycle의 기존 종료 fade로 이어진다.
- layer별 `MaterialPropertyBlock` 갱신과 기존 ParticleSystem module을 재사용하며 per-frame Play/Stop이나
  새 managed allocation은 추가하지 않았다.
- Blue 전체는 아직 USER VERIFIED가 아니며 실제 onset 순서, dust/debris 가독성, impact handoff와
  replay/cleanup은 Unity 사용자 검수 전이다.

## Gojo Blue Stylized Suction Wind Layer

상태:

```text
REMOTE IMPLEMENTED / USER VISUAL TEST PENDING
```

- Blue 전용 `AirflowSuctionWisps`와 `WindRibbonConvergence` ParticleSystem을 추가했다.
- 기존 shared `GojoBlueParticle` material에 procedural `AirflowWisp`/`WindRibbon` mask mode를 추가하고
  renderer별 `MaterialPropertyBlock`으로 선택한다.
- 두 layer는 local 3D height/radius variation, 약한 orbit/noise, 가속되는 inward radial curve를 사용한다.
- airflow는 field 18~34%, sparse ribbon은 28~52%에 등장해 core/shell보다 늦고 fast convergence보다
  먼저 공기 흡입을 보조한다.
- 외부 asset, VFX Graph, fluid/fog simulation, generic particle factory 변경은 없다.
- 기존 Blue instance lifetime과 cleanup을 따르며 실제 3D depth, arc 구분, core readability와 누적 여부는
  Unity 사용자 검수 전이다. Blue 전체 상태도 계속 USER VISUAL TEST PENDING이다.

## Gate 5B VFXLab developer tool

Gate 5B VFX production iteration을 위해 캐릭터 이동, prototype/future authored motion, 실제 production
Blue VFX, orbit camera, scene-owned lighting/Bloom, replay/loop/slow motion을 한 scene에서 검수하는
developer-only `Assets/Scenes/VFXLab.unity`를 추가했다. Combat gameplay 없이 동일
`GojoBlueVfxInstance`를 호출하며 상세 사용법은 `docs/VFX_LAB.md`를 따른다.

Blue 상태는 계속 `REMOTE IMPLEMENTED / USER VISUAL TEST PENDING`이다.


## Pass 8R-2A Unity test / hotfix 상태

Pass 8R-2A merge 이후 실제 Unity Editor에서 다음 오류가 발견되어 master에서 직접 수정했다.

1. Particle Orbital Velocity curves must all be in the same mode
- Blue fast/slow spiral의 orbital X/Y/Z MinMaxCurve mode를 모두 TwoConstants로 통일
- 새 ParticleSystem을 module 설정 전에 StopEmittingAndClear하여 duration 변경 오류도 같이 수정

2. GojoBlueEnergy.shader compile error: unexpected token 'point'
- HLSL/D3D11 parser에서 예약어 충돌을 피하도록 함수 인자 point를 samplePos로 변경

현재 master의 마지막 shader hotfix 기준 commit:
- 8247db95de7086b9a0ffde361fb7c9afce4df47c

중요:
- 위 오류 수정 후 Blue 효과 자체는 사용자가 확인했으나 최종 시각 합격/불합격 판정은 아직 기록되지 않았다.
- 따라서 Pass 8R-2A 상태는 REMOTE IMPLEMENTED / USER VISUAL TEST PENDING을 유지한다.
- 다음 채팅에서 먼저 사용자에게 최신 Blue의 시각 평을 확인하고, 합격 전에는 Red로 넘어가지 않는다.

## 2026-09-02 외부 Unity/Unreal JJK VFX 재조사

상세:
- docs/JJK_VFX_EXTERNAL_REFERENCE_SURVEY.md

핵심 reference:
- starkim1999 (Unity/VRChat): Blue/Red/Purple/Infinite Void. Distortion Shader + Particle Shader + Uberscreen shader. Blue inward / Red outward를 제작자가 직접 설명.
- FloppiiiVR (Unity/VRChat): Blue를 black-hole-like attraction으로 크게 staging, Unlimited Void에서 world visual을 거의 suppression하여 domain이 공간 전체를 지배.
- CooGee (Unity/VRChat): Hollow Purple에 별도 ScreenJackEffect, flash/shake/screen accent.
- YSA/Youssef Af (Unity): Red Hollow Skill에 3 textures + 7 custom Shader Graph shaders + script-controlled stage timing.
- Deadlyxrebel (RealtimeVFX, engine 미확정): Purple stage transition, lightning decals, material transition, debris/environment response를 polish 항목으로 제시.
- Swammy (UE5.4+): JJK fan game, placeholder Hollow Purple 사용자 피드백 후 rework 사례.
- Fortnite JJK (Unreal production): large-scale 3D gameplay에서 Purple silhouette/readability 참고.
- Raffi Ramadhan (UE5.3): JJK cursed-energy realtime aura.
- VinceVFX (Unity): JJK-style gameplay VFX에서 custom shading, mesh/projector shape, value/readability를 강조.

외부 사례와 사용자 제공 조사 문서의 공통 결론:
- core silhouette
- multiple shader/material layers
- particle motion
- screen-space distortion
- stage timing
- environment response
- gameplay-camera readability
를 함께 써야 production-quality JJK VFX에 가까워진다.

## 다음 권장 작업

먼저 CombatMVP와 VFXLab에서 ground dust 위치, dark debris 밀도, inward/orbit 우선순위,
impact collapse, fighter readability, replay/cleanup을 사용자 시각 검수한다. 기존 distortion과
tapered particle 유지도 함께 확인한 뒤에만 Blue final polish와 Red/Purple 재사용 경계를 결정한다.

Blue가 아직 시각적으로 부족하다고 판정되면:
- R-2B로 넘어가기 전에 Blue shader/noise/core/spiral/arc art tuning을 먼저 수행한다.

이후 순서:
1. Blue final acceptance
2. reusable distortion
3. Red production VFX
4. Hollow Purple production VFX
5. Unlimited Void environment + Enter/Active/Break transition


## 변경 금지

- gameplay damage
- CE
- cooldown
- range
- hitbox
- pull/push
- hit timing
- Purple 0.24 / 0.78 / 18m timing
- Domain duration/radius/state/input
- camera gameplay
- HUD
- Target Lock
- Tag
- AI
- Sukuna/Megumi current gameplay

## 검증 정책

- Codex static build != Unity Play Mode
- REMOTE IMPLEMENTED와 USER VERIFIED 구분
- 사용자 시각 검수 전에는 VFX pass를 USER VERIFIED로 닫지 않음
- 큰 작업은 feature branch → commit/push → GitHub diff review → merge → Unity user test 순서


## 새 채팅 시작용 문구

새 채팅에서는 아래처럼 요청하면 된다.

"GitHub JJuNi27/JJK_game 프로젝트 작업을 이어서 할게.
먼저 master의 docs/CURRENT_HANDOFF.md, docs/JJK_VFX_REFERENCE_GOJO.md,
docs/JJK_VFX_EXTERNAL_REFERENCE_SURVEY.md, docs/DEVELOPMENT_ROADMAP.md,
docs/GATE5B_FIRST_PRODUCTION_SLICE.md를 읽고 현재 Gate/Pass 상태와 마지막 Blue VFX 테스트 상태를 요약해줘.
기존 USER VERIFIED 상태를 되돌리지 말고, REMOTE IMPLEMENTED와 USER VISUAL VERIFIED를 구분해.
지금은 Gate 5B Pass 8R 계열 고죠 production VFX를 reference-first 방식으로 재작업 중이고,
Blue가 사용자 시각 합격하기 전에는 Red로 넘어가면 안 돼.
내가 마지막으로 본 Blue의 시각 평가부터 물어보고 그 결과에 따라 다음 작업을 정해줘.
큰 작업은 Codex용 프롬프트를 만들어주고, feature branch → commit/push → GitHub diff review → merge → Unity user test 순서를 유지해줘." 


## 새 채팅 인수인계

새 채팅 시작용 복붙 지시문은 `docs/NEW_CHAT_BOOTSTRAP.md`에 저장되어 있다.
새 채팅은 해당 문서와 CURRENT_HANDOFF를 GitHub에서 직접 읽은 뒤 현재 master를 확인하고 이어간다.
