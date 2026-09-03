# JJK VFX External Implementation Research

작성 기준: 2026-09-02

목적:
현재 Gate 5B Gojo VFX를 단순 prototype particle 수준에서 production-facing VFX로 올리기 위해,
공개적으로 확인 가능한 Unity/VRChat/Unreal 기반 Jujutsu Kaisen / Gojo 구현 사례를 비교하고
우리 프로젝트에 가져올 수 있는 "원리"만 정리한다.

중요:
- 타인의 유료 asset / shader / texture / animation을 복사하거나 추출하지 않는다.
- 공개 설명, 공개 영상/이미지, 기술 스택에서 구현 원리만 참고한다.
- 엔진/도구가 공개 자료에서 확인되지 않는 경우 추정하지 않는다.

---

## 1. starkim1999 — Unity / VRChat

대표 공개 자료:
- Unlimited Hollow Purple - Imagined Animation
  https://starkim1999.gumroad.com/l/UnlimtedHollowPurple
  https://jinxxy.com/starkim1999/Xm3Mz
- Domain Expansion: Infinite Void - Imagined
  https://jinxxy.com/starkim1999/VyeRi

확인된 기술:
- Unity 2022.3.6f1
- Poiyomi 8.1+
- Leviathan ScreenSpace Ubershader
- Distortion Shader
- custom/third-party Particle Shader
- World Constraint / VRChat animation system
- particle + animation + screen shader를 조합

Blue:
- 구체 자체를 lore/anime/manga에 가깝게 보이게 만드는 것을 우선
- Purple merge 직전에 주변 particle가 Blue orb 안으로 들어가게 함
- "흡인"을 단순 색이 아니라 움직임으로 설명

Red:
- Blue의 반대
- merge 전 particle를 밖으로 방출해 척력 표현

Purple:
- Blue/Red fusion을 전체 animation sequence로 구성
- distortion / particle / screen shader를 동시에 사용

Unlimited Void:
- 애니 + Cursed Clash를 함께 reference
- domain 진입 tunnel을 별도 sequence로 구성
- tunnel 후 독립 domain environment로 전환
- domain 종료 fadeout도 별도 animation으로 설계

### 우리에게 가장 유용한 점

"원작 시각 문법 + screen distortion + 단계별 timing"의 균형이 좋다.

우리 프로젝트의 production 원칙과 가장 잘 맞는 외부 reference 중 하나:
- Blue/Red motion identity
- Distortion을 core feature로 취급
- Domain을 particle effect가 아니라 environment transition으로 취급
- 시작/유지/종료를 별도 presentation phase로 설계

---

## 2. Zepwlert — The Strongest Sorcerer — Unity / VRChat

공개 자료:
https://zepwlert.gumroad.com/l/sorcerer
https://jinxxy.com/Zepwlert/TheStrongest

확인된 기술/규모:
- Unity 2022
- from-scratch VFX
- Blue / Red / Purple / Infinite Void 포함
- ScreenSpace Ubershader
- DoppelGanger distortion wave/shader
- impact frame / screenshake 등 screen effects
- sound effects
- 총 236 custom ParticleSystem
- screen effects toggle 제공

공개 페이지는 다음 visual을 별도 showcase:
- Purple
- Red Down-Slam
- Red Barrage
- Blue Orb
- Blue Rock Telekinesis
- Infinite Void

### 장점

공개적으로 확인 가능한 사례 중 "레이어 양과 화면 임팩트"가 매우 큰 편이다.
단순 orb 하나가 아니라 screen effects / distortion / particle / sound까지 한 패키지 안에서 겹친다.

### 단점 / 우리에게 그대로 적용하면 안 되는 점

VRChat avatar 기준으로도 제작자가 "Very Poor" performance rank가 된다고 직접 경고한다.
236 ParticleSystem을 그대로 흉내 내는 것은 실제 arena combat game 구조에는 부적절하다.

### 우리에게 가져올 원리

가져올 것:
- particle texture / material variation
- impact-frame / screen-space layer라는 시각 계층
- 한 기술 안에서 여러 크기/속도/불투명도 layer를 쌓는 방식
- Blue 주변 visual telekinesis/debris 같은 "환경 반응처럼 보이는" 보조 layer

가져오지 않을 것:
- 200개가 넘는 ParticleSystem 구조
- VRChat Very Poor 수준의 effect count
- avatar 내부에 모든 VFX를 거대하게 탑재하는 구조

우리 목표:
"Zepwlert 수준의 layer richness를 훨씬 적은 renderer / ParticleSystem으로 재현"

---

## 3. Floppiii — Satomi Gojo / Purple Obliteration — Unity / VRChat

공개 자료:
https://floppiiivr.gumroad.com/l/jujutsukaisen
https://booth.pm/ko/items/4932277
https://payhip.com/b/uM28s

확인된 요소:
- Reversal Red
- Lapse Blue
- Hollow Purple
- Domain Expansion
- Black Flash
- Particle Materials
- Particle Textures
- Additive Intensify Shader / Anime Particle Shader
- ScreenSpace Ubershader
- sound + animator/keyframe sequence

Blue:
- sky에 Blue를 유지할 수 있고
- 작은 black hole을 생성해 주변을 빨아들이는 reimagined animation을 사용

Unlimited Void:
- 주변 world를 사실상 보이지 않게 만드는 대규모 scene takeover
- "world of infinity"로 전환
- black hole climax

### 장점

"기술 하나를 화면 전체 set-piece로 만든다"는 점이 강하다.
particle texture와 anime particle shader를 적극 사용해서
단순 primitive sphere + line 느낌을 피한다.

### 우리에게 가져올 원리

- particle texture가 실제 detail ceiling을 크게 올림
- animation / keyframe으로 stage를 명확히 분리
- Domain은 background takeover를 강하게 사용
- Blue에 black-hole-like inward visual identity를 부여

### 그대로 적용하면 안 되는 점

Floppiii의 Domain은 world를 99.5% invisible하게 만드는 VRChat showcase 방식이다.
우리 arena game에서 gameplay readability를 파괴할 수 있으므로 그대로 따라하지 않는다.

우리 버전:
- collider / gameplay world는 유지
- renderer suppression / overlay / void enclosure만 presentation에서 관리
- fighter / Target Lock / HUD는 항상 읽히게 유지

---

## 4. Swammy — Jujutsu Kaisen Fan Game — Unreal Engine 5

공개 자료:
- YouTube: I made a FREE Jujutsu Kaisen Fan Game!
  https://www.youtube.com/watch?v=KfhswSu_gGI
- Patreon project files:
  https://www.patreon.com/SwammyXO/posts/jujutsu-kaisen-4-110182594
- Gojo Hollow Purple update:
  https://www.patreon.com/posts/gojo-hollow-jjk-112324635

확인된 엔진:
- UE5.4+ project files가 공개/배포된 기록이 있음
- 실제 플레이 가능한 JJK fan game
- Gojo special attacks가 gameplay에 들어가 있음

### 장점

VRChat showcase와 달리 실제 3D action game 안에서:
- camera distance
- enemy readability
- move timing
- projectile scale
- combat flow

를 어떻게 맞추는지 보기 좋다.

### 우리에게 가져올 원리

Swammy는 "최고의 shader breakdown" reference라기보다
"실제 게임 안에서 signature VFX가 어느 크기와 시간으로 보여야 하는가" reference로 사용한다.

즉:
- starkim/Zepwlert/Floppiii → visual layer reference
- Swammy → gameplay integration / readability reference

---

## 5. Deadlyxrebel — Hollow Purple real-time VFX portfolio

공개 자료:
https://realtimevfx.com/t/hollow-purple-wip1/27632

공개 게시물에서 확인된 개선 항목:
- stage transitions
- lightning impact decal
- rock debris material transition / cracking
- debris가 빨려 들어가는 과정
- aura alternatives
- Blue/Red가 하나로 merge되는 sequence

주의:
공개 검색 결과만으로 이 작업의 엔진을 확정하지 않는다.

### 우리에게 중요한 점

우리 Purple이 아직 "orb + trail" 수준에 머물면 부족하다.

특히 가져올 원리:
- 주변 debris가 기술에 반응하는 visual-only layer
- merge 전에 Blue/Red 주변 환경이 변하는 느낌
- lightning/impact decal
- material cracking / fragment transition
- stage transition 자체를 polish 대상으로 다룸

---

# 6. 현재 우리 구현과 비교

현재 Blue benchmark:

- custom URP procedural energy shader
- deep body / Fresnel shell / outer shell
- inward particle
- clockwise / counter spiral
- broken convergence arcs
- impact collapse
- GojoBlueDistortionSource-driven localized distortion shell
- URP `_CameraOpaqueTexture` actual screen sampling (user visual test pending)
- external texture 없이 procedural soft mote / tapered streak / broken wisp particle mask 구현
- presentation-only ground dust / sparse dark fragment inward suction 구현

현재 가장 큰 차이:

## A. Actual screen distortion proof 구현, 사용자 시각 검수 대기

외부 고퀄 사례의 공통점:
- ScreenSpace Ubershader
- Distortion shader
- distortion wave

우리:
- distortion-like energy shell과 별도의 transparent localized sampling shell
- world radius/strength/impact metadata를 실제 shader property에 연결
- field subtle inward / impact stronger inward warp
- 기존 PC opaque texture를 사용해 Renderer Feature/YAML 변경 없음

결론:
구현 상태는 `REMOTE IMPLEMENTED / USER VISUAL TEST PENDING`이며 Unity에서 강도와 shader compile을
확인한 뒤 승인 여부를 결정한다.

## B. Procedural particle mask 1차 구현, 사용자 시각 검수 대기

외부 사례:
- particle textures
- anime particle shader
- additive intensify
- custom particle material

우리:
- 하나의 Blue 전용 procedural particle shader에서 3개 mask mode 제공
- shared template + renderer별 MaterialPropertyBlock으로 mode/fade/emission/breakup 구동
- fast streak stretch/lifetime 축소, slow mote와 corona silhouette 분리

결론:
구현 상태는 `REMOTE IMPLEMENTED / USER VISUAL TEST PENDING`이다. Unity에서 긴 laser bar가 실제로
줄었는지, soft edge와 inward readability가 유지되는지 확인한 뒤 추가 texture 필요성을 결정한다.

## C. Visual-only environment suction 1차 구현, 사용자 시각 검수 대기

고퀄 reference:
- debris suction
- rock cracking
- Blue rock telekinesis
- impact decal

우리:
- Blue-owned ground dust와 dark debris ParticleSystem 2개만 추가
- 실제 environment object/physics 없이 낮은 spawn illusion 사용
- negative radial speed > weak orbit > restrained noise 순서로 중심 수렴
- impact는 짧은 lifetime과 normalized compression 기반 simulation 가속 사용

결론:
구현 상태는 `REMOTE IMPLEMENTED / USER VISUAL TEST PENDING`이다. Unity에서 바닥 착시, sparse density,
fighter/enemy readability와 inward identity가 유지되는지 확인한다.

## D. Stage timing layer 구현, 사용자 시각 검수 대기

고퀄 reference는:
charge → establish core → environment reaction → compression → release/impact

처럼 작은 단계가 분리됨.

우리:
- field normalized time으로 dense core를 먼저 점화
- Fresnel/outer shell, distortion, corona를 뒤이어 확립
- slow spiral과 ground dust/dark debris 환경 반응을 중간에 도입
- fast spiral은 후반 convergence에서 최종 강도에 도달
- impact는 짧은 hold 뒤 collapse를 가속하고 기존 lifecycle fade로 종료

결론:
gameplay timing을 바꾸지 않고 presentation 내부 normalized time으로 layer onset을 stagger했다.
구현 상태는 `REMOTE IMPLEMENTED / USER VISUAL TEST PENDING`이며 실제 단계 구분과 impact handoff는
Unity 사용자 검수 후 확정한다.

---

# 7. 다음 권장 작업 — Blue Production VFX Benchmark 2차 polish

Blue를 아직 USER VERIFIED로 닫지 않는다.

다음 Blue 작업 우선순위:

## 1순위 — Real Screen Distortion Proof 사용자 검수

목표:
- Blue screen position 중심으로 주변 background UV가 안쪽으로 휘어짐
- 반경 밖은 영향 없음
- strength는 GojoBlueDistortionSource에서 읽음
- impact 시 0.1~0.2초 더 강하게 compression
- HUD는 왜곡하지 않음

현재 구현:
- Blue instance-local transparent sphere가 `_CameraOpaqueTexture`를 sample
- 기존 VFX instance lifecycle/fade에 따라 생성/파기
- Screen Space Overlay HUD는 sampling 대상 밖
- renderer asset YAML 변경 없음

## 2순위 — Custom particle texture/mask 사용자 검수

현재 직접 구현:
- soft radial mote
- tapered streak
- broken energy wisp

목표:
기본 billboard / 선 모양을 줄임.

## 3순위 — Visual-only environment suction 사용자 검수

- 낮은 ground dust 위치와 동시 표시 밀도
- sparse dark fragment visibility
- inward > orbit > noise motion hierarchy
- impact collapse와 replay/cleanup

실제 Rigidbody / map gameplay는 건드리지 않음.

## 4순위 — Internal timing stagger 사용자 검수

현재 구현:
- 0~15%: dense core ignition
- 10~35%: Fresnel/outer field, corona, distortion establishment
- 20~55%: slow spiral과 dust/debris environment reaction
- 45~85%: fast spiral을 포함한 full convergence
- impact: 0~20% hold 뒤 20~75% collapse 가속

실제 Blue gameplay duration 변경 없음.

## 5순위 — final Blue art tuning

- core size
- shell alpha
- distortion radius
- streak count
- arc opacity
- Bloom level

사용자 스크린샷/영상 기반 반복 튜닝.

---

# 8. 기술별 reference 역할 정리

Blue:
- starkim1999: lore/readability + inward motion + distortion
- Zepwlert: layer richness / screen impact
- Floppiii: particle texture / black-hole-like attraction
- Swammy: actual-game scale/readability

Red:
- starkim1999: outward motion identity
- Zepwlert: impact richness
- Floppiii: charge → white-hot → launch animation idea
- Swammy: game timing/scale

Purple:
- starkim1999: fusion + distortion sequence
- Deadlyxrebel: debris/material/environment reaction
- Floppiii: layered particle texture / screen-space spectacle
- Swammy: playable combat integration

Unlimited Void:
- starkim1999: anime/game reference tunnel → domain → fadeout
- Floppiii: strong environment takeover / black-hole climax
- Zepwlert: full-screen effect richness
- Swammy: gameplay readability

---

# 9. 최종 판단

공개적으로 확인 가능한 자료 기준으로:

- "우리 구조에 가장 직접적으로 참고할 균형형": starkim1999
- "시각 레이어 밀도/화려함 benchmark": Zepwlert
- "cinematic set-piece / texture-driven benchmark": Floppiii
- "실제 action game integration benchmark": Swammy (UE5)
- "Purple 환경 반응/detail 아이디어": Deadlyxrebel

따라서 우리 목표는 특정 제작자 하나를 복제하는 것이 아니라:

starkim의 원작성/왜곡
+
Zepwlert의 layer richness
+
Floppiii의 texture/timing
+
Swammy의 gameplay readability
+
Deadlyxrebel의 environment interaction

을 우리 Unity 6 URP production pipeline에 맞게
더 적은 renderer/particle count로 재구성하는 것이다.
