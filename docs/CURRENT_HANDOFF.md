# Current Project Handoff

작성 기준: 2026-09-01

## 현재 단계

Gate 5B First Production-Quality Vertical Slice 진행 중.

현재 VFX 상태:

- Pass 8 JJK-Referenced Production Particle VFX Runtime:
  USER VISUAL REVIEW FAILED / REWORK REQUIRED
- Pass 8R-1 Gojo Signature VFX Reference-First Rework:
  USER VISUAL PARTIAL / DETAIL REWORK REQUIRED

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
