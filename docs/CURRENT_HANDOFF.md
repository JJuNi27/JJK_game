# Current Project Handoff

작성 기준: 2026-09-02

## 현재 단계

Gate 5B First Production-Quality Vertical Slice 진행 중.

현재 VFX 상태:

- Pass 8 JJK-Referenced Production Particle VFX Runtime:
  USER VISUAL REVIEW FAILED / REWORK REQUIRED
- Pass 8R-1 Gojo Signature VFX Reference-First Rework:
  USER VISUAL PARTIAL / DETAIL REWORK REQUIRED
- Pass 8R-2A Gojo Blue Production VFX Benchmark:
  REMOTE IMPLEMENTED / USER VISUAL TEST PENDING

Pass 8R-1은 Pass 8 대비 방향성은 개선됐지만 final-quality VFX로 승인되지 않았다.

## 사용자 시각 피드백 핵심

- Blue/Red/Purple/Unlimited Void 모두 Pass 8 대비 개선됐지만 디테일이 부족함
- primitive sphere / flat material / simple particle / line 느낌이 아직 강함
- 저퀄리티 “뿅뿅 발사” 인상을 벗어나려면 shader / distortion / layered timing이 필요
- Sukuna slash 계열은 상대적으로 개선되었으므로 현재 rework 대상에서 제외

## 기준 문서

- docs/JJK_VFX_REFERENCE_GOJO.md
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
- renderer feature와 분리된 future `GojoBlueDistortionSource` hook

VFX Graph, Full Screen Pass, PC Renderer YAML, external texture는 추가하지 않았다.
Red/Purple/Unlimited Void/Sukuna/Megumi와 gameplay 값도 변경하지 않았다.

현재 다음 단계는 Unity에서 shader compile error가 없는지 확인하고 Blue wide/close/impact 장면을
시각 검수하는 것이다. Blue가 사용자 합격하기 전에는 Red production pass로 넘어가지 않는다.


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

Blue R-2A가 사용자 시각 합격을 받는 경우:

Pass 8R-2B · Reusable Gojo Distortion Pipeline + Blue Final Polish

목표:
- 현재 GojoBlueDistortionSource hook을 실제 reusable screen/local distortion pipeline에 연결
- Blue는 inward pinch/lens distortion
- 향후 Red는 outward bulge, Purple은 stronger localized warp로 재사용
- raw Renderer YAML 직접 수정 금지
- Unity 6 / URP 17.3 공식 API/Context7 확인 후 Editor-safe Renderer Feature 또는 Full Screen Pass 방식 검토
- HUD는 distortion 영향 밖
- Pass 7 camera/flash/hit-stop ownership 유지

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
