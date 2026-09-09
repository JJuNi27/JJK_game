# JJK_game — VFX 방향 / Strikeborn Impact Standard

상태: **장기 시각 품질 기준**

## 핵심 목표

기존 목표:
**팬텀 퍼레이드 기술을 3D로 원작 고증 높게 재현**

확장된 목표:
**팬텀 퍼레이드 + 애니/만화 원작 고증을 기반으로 하되, 실제 플레이 순간의 연출 밀도·타격감·환경 반응·잔류감은 Strikeborn급 이상을 목표로 한다.**

Strikeborn이나 Roblox asset을 복사하는 것이 아니다.
가져올 것은:
- timing
- presentation density
- impact readability
- environment reaction
- aftermath
- camera/screen feedback
- 강한 silhouette

표현은 Unity URP / Shader / Particle System / VFX Graph / Full Screen FX / Camera feedback 등 현재 프로젝트 기술에 맞게 구현한다.

## 금지되는 낮은 완성 기준

다음으로 끝내지 않는다:

`기술 생성 → 이동 → 펑 → 즉시 소멸`

기술 하나가 강하게 기억되려면 가능한 범위에서 다음 presentation beat를 고려한다.

## 공통 Presentation Recipe

1. **Anticipation / 예고**
   - 자세
   - 에너지 모임
   - 주변 먼지/빛/공간의 사전 반응

2. **Charge / 형성**
   - core growth
   - orbit / attraction / compression
   - voice / audio hook

3. **Release / 발사**
   - flash
   - ring
   - camera impulse
   - FOV kick
   - hit-stop/impact-frame가 필요한 기술은 짧고 의도적으로

4. **Travel / 이동**
   - trail
   - distortion
   - lightning
   - pressure / suction
   - 주변 환경 반응

5. **Impact / 충돌**
   - 명확한 peak frame
   - irregular shockwave
   - debris
   - dust
   - hit flash
   - 대상 knockback / destruction과 presentation 동기화

6. **Environment Reaction**
   - ground crack / scar
   - debris
   - dust
   - 건물/월드 reaction hook
   - destructible system과 향후 연결 가능

7. **Aftermath / 잔류**
   - smoke / vapor
   - pressure cloud
   - residual electricity
   - spatial shimmer
   - scar / dust
   - 기술이 끝난 뒤에도 현장이 바뀌었다는 느낌

8. **Camera / Screen FX**
   - camera impulse
   - FOV
   - restrained distortion
   - impact frame / negative frame는 기술 정체성에 맞을 때만

9. **Audio**
   - voice
   - cast
   - release
   - travel
   - hit
   - environment
   - aftermath
   각 beat에 독립 연결 가능

## 중요 원칙

### 1. 원작 정체성이 Strikeborn보다 우선
Strikeborn 연출을 그대로 붙이지 않는다.

예:
- Red는 불꽃 기술이 아니라 repulsion
- Blue는 attraction / singularity
- Purple은 공간을 지우고 찢는 듯한 융합 기술

각 기술의 물리/술식 정체성에 맞는 aftermath를 만든다.

### 2. 모든 기술을 똑같이 화려하게 만들지 않는다
밀도는 높이되 silhouette는 캐릭터/기술마다 달라야 한다.

### 3. Environment + Aftermath는 핵심 품질 요소
본체 VFX만 보고 완료 판정하지 않는다.

### 4. Hit feel은 여러 시스템의 합
좋은 타격감 =
VFX
+ animation timing
+ hit stop
+ camera
+ audio
+ target reaction
+ environment reaction

### 5. VFXLab은 공용 Character Presentation Lab
Gojo 전용이 아니다.
앞으로 선택한 캐릭터의 기술을 같은 품질 기준으로 검수한다.

---

## Gojo Blue — 2차 enhancement 방향

현재 baseline은 1차 완성으로 보호.

보존:
- 4-hit
- singularity
- attraction
- core
- debris 기본 언어

추가 후보:
- 주변 environment particle의 사전 inward reaction
- 4-hit별 더 읽히는 presentation beat
- 마지막 collapse의 강한 peak
- 짧고 restrained한 distortion/camera feedback
- collapse 후 residual spatial shimmer / dust pull
- 현재 깔끔한 silhouette를 가리지 않는 추가 레이어

갈아엎지 않는다.

---

## Gojo Red — 2차 enhancement 방향

원작 키워드:
**Repulsion / 압축된 척력이 한 번에 해방됨**

Presentation 예:
Anticipation
→ compressed Red
→ violent release
→ pressure-distorted travel
→ hit flash
→ irregular repulsive shock
→ outward debris/dust
→ gray-white vapor/air pressure
→ residual pressure decay

화염 폭발처럼 만들지 않는다.

---

## Hollow Purple — 2차 enhancement 방향

현재 formation/fusion은 핵심 보호 대상.

Presentation 예:
formation
→ fusion
→ hold
→ violent release
→ launch compression
→ travel distortion
→ branching violet lightning
→ nearby debris/space reaction
→ wide scar
→ residual violet electricity
→ spatial shimmer / aftermath

목표:
**"예쁜 보라색 구체"가 아니라, 지나간 공간 자체가 손상된 느낌.**

---

## 참고 설계 철학 — Modular Skill Presentation

장기적으로 skill presentation을 한 코드 덩어리보다 재사용 가능한 beat/node로 조립 가능하게 설계한다.

예:
`AbilityDefinition`
- Gameplay
  - Damage
  - Range
  - Projectile
  - Collision
- PresentationRecipe
  - Anticipation
  - Release
  - Travel
  - Impact
  - Environment
  - Aftermath
  - Camera
  - ScreenFX
  - Audio

Jujutsu Shenanigans Skill Builder 같은 모듈형 timeline/visual-node 철학은 참고할 수 있지만,
Roblox asset/API를 그대로 복사하지 않는다.

현재 프로젝트에서 실제 일반화는 단계적으로 진행한다.
