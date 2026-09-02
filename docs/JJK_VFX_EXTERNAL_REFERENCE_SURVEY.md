# JJK VFX External Reference Survey

작성 기준일: 2026-09-02
목적: Unity/Unreal 기반 고죠/JJK 실시간 VFX 구현 사례를 조사하고, JJK_game의 production VFX 파이프라인에 적용 가능한 원칙을 추출한다.

중요:
- 외부 유료/비공개 asset, shader, texture, project file을 복사/리핑하지 않는다.
- 공개된 설명, 영상, 스크린샷에서 visual principle / timing / layering / readability만 참고한다.
- fan-made 자료는 원작 정확성의 기준이 아니라 실시간 구현 방법과 게임 가독성 참고용이다.
- 원작/공식 게임 reference 우선순위는 docs/JJK_VFX_REFERENCE_GOJO.md를 따른다.

## 1. Starkim1999 — Unity / VRChat

대표 자료:
- Unlimited Hollow Purple: https://jinxxy.com/starkim1999/Xm3Mz
- Infinite Void: https://beta.jinxxy.com/starkim1999/VyeRi
- Infinite Void 영상: https://www.youtube.com/watch?v=q9EVYa__HBc

확인된 특징:
- Unity 2022.3.6f1, Poiyomi 8.1+, VRLabs World Constraint, Leviathan's Uberscreenshader.
- Distortion Shader + Particle Shader + particle/animation/screen layer 조합.
- Blue는 gravitational orb이며 particle가 orb 안으로 들어간다.
- Red는 Blue 반대로 particle가 바깥 방향으로 움직인다.
- Infinite Void는 anime + Cursed Clash reference, tunnel entry → explorable domain → break/fade exit의 3단 staging.

우리 적용:
- 현재 Blue R-2A의 core shader + inward flow 방향은 맞다.
- 다음 핵심은 실제 screen-space/local distortion.
- Domain은 Active 공간만이 아니라 Enter → Active → Break transition으로 설계한다.

## 2. FloppiiiVR — Unity / VRChat

대표 자료:
- Jujutsu Kaisen Satomi Gojo VRChat Avatar: https://booth.pm/ko/items/4932277

특징:
- Red / Blue / Black Flash / Hollow Purple / Unlimited Void 전체 set.
- Blue를 black-hole-like attractive force로 재해석해 주변 흡수와 scale을 강하게 강조.
- Unlimited Void에서 주변 world visual을 거의 사라지게 하고 별도 infinity world를 지배적으로 표시.
- Hollow Purple impact도 거대한 expanding purple force로 재해석.

우리 적용:
- 원작 정확성은 starkim/공식 reference보다 낮은 우선순위.
- 하지만 signature 기술이 전장을 지배해야 한다는 staging 교훈은 유용.
- 실제 gameplay object/collider를 숨기지 않고 presentation layer에서만 arena suppression.

## 3. CooGee — Unity / VRChat Hollow Purple

대표 자료:
- Hollow Purple: https://booth.pm/ja/items/6006054

확인된 특징:
- Unity 2022.3.6f1 / 2022.3.22f1.
- Simple / Origin 두 연출, SE 포함.
- 별도 ScreenJackEffect layer를 보유하며 flash/shake가 시야를 덮을 수 있는 presentation 구조.
- 업데이트에서 impact-frame screen effect를 수정하고 흑백 반전 대체 효과를 추가.

우리 적용:
- Purple 품질은 projectile body만으로 결정되지 않는다.
- merge/launch/impact 순간에 짧고 통제된 screen-space accent가 필요.
- Pass 7 camera/flash ownership과 충돌하지 않도록 screen effect는 별도 presentation layer로 설계.

## 4. YSA / Youssef Af — Unity Red Hollow Skill VFX

대표 자료:
- https://assetsale.herokuapp.com/en/contents/90490

확인된 구조:
- full 3D VFX.
- 3 textures (512px).
- 7 custom Shader Graph shaders.
- animation timing / lifetime / color를 하나의 script에서 제어.

우리 적용:
- polished Red/Purple을 Sphere + ParticleSystem 하나로 끝내는 것은 품질 ceiling이 낮다.
- core / shell / shockwave / streak / impact / screen accent를 서로 다른 shader/material layer로 관리.
- 작은 custom texture 여러 개 + lightweight material layer도 고려.
- 전체 stage timing은 presentation controller가 소유.

## 5. Deadlyxrebel — RealTimeVFX Hollow Purple WIP

대표 자료:
- https://realtimevfx.com/t/hollow-purple-wip1/27632

주의:
- 접근 가능한 자료에서는 engine을 확정할 수 없으므로 Unity/Unreal 특정 사례로 분류하지 않는다.

제작자가 직접 개선점으로 언급:
- stages 사이 transition.
- lightning impact decal.
- material transition.
- rock cracking / debris가 빨려 들어가는 반응.
- aura variations.

우리 적용:
- Purple 최종 polish는 particle 수보다 stage transition과 environment response가 중요.
- core VFX 승인 뒤 presentation-only debris/dust/crack response를 별도 pass로 검토.

## 6. Swammy — Unreal Engine 5 JJK Fan Game

대표 자료:
- UE5.4+ project reference: https://www.patreon.com/SwammyXO/posts/jujutsu-kaisen-4-110182594
- Hollow Purple rework: https://www.patreon.com/SwammyXO/posts/gojo-hollow-jjk-112324635

확인:
- UE5.4+ fan game.
- placeholder Hollow Purple이 나쁘다는 피드백 뒤 rework했다고 제작자가 직접 언급.

우리 적용:
- isolated showcase가 아니라 실제 gameplay camera/distance에서 읽히는지 검증.
- REMOTE IMPLEMENTED와 USER VISUAL ACCEPTANCE를 계속 분리.

## 7. Fortnite Jujutsu Kaisen — Unreal production reference

대표 자료:
- Epic 공식 v25.30 JJK update: https://www.fortnite.com/news/fortnite-battle-royale-v25-30-update-break-the-curse-with-jujutsu-kaisen

확인:
- Hollow Purple이 실제 대규모 3D gameplay에서 사용되며 경로상의 건물을 파괴하고 적에게 피해를 주는 역할.

우리 적용:
- exact visual copy가 목적이 아니라 projectile silhouette, scale, travel readability의 production 기준으로 참고.
- final Purple은 화려해도 opponent / HUD / target lock을 가리지 않아야 함.

## 8. Raffi Ramadhan — Unreal Engine 5 JJK Cursed Energy

대표 자료:
- https://raffiramadhan.artstation.com/projects/OG2YKy

확인:
- Unreal Engine 5.3으로 JJK cursed-energy/aura realtime VFX 제작.

우리 적용:
- character cursed-energy aura family 참고 가능.
- 현재는 signature skill benchmark가 우선이므로 aura는 LATER.

## 9. VinceVFX — Unity JJK-style gameplay VFX

공개 자료에서 확인:
- JJK-style Fireball/AOE를 gameplay skill VFX로 제작.
- 제작자가 해당 프로젝트 engine은 Unity3D라고 직접 답변.
- color/brightness balance, step functions, skeletal mesh projectors, custom shading, staged mesh particles를 강조.

우리 적용:
- JJK 느낌은 particle count보다 graphic shape / value separation / material breakup이 중요.
- Blue/Red arc를 LineRenderer만으로 만들기보다 mesh/decal/projector 계열을 향후 검토.
- emission을 무작정 높이지 않고 색/밝기 계층 유지.

## 공통 결론

좋은 realtime 사례의 공통점:
1. 하나의 particle system으로 끝내지 않는다.
2. core silhouette가 먼저 읽힌다.
3. 기술의 물리 성질을 motion으로 표현한다.
4. custom shader/material layer를 역할별로 나눈다.
5. screen-space effect/distortion을 signature 순간에 사용한다.
6. charge/formation/release/travel/impact stage를 분리한다.
7. Domain은 particle effect가 아니라 environment + transition으로 다룬다.
8. stage transition과 environment response가 최종 품질을 크게 올린다.
9. 실제 gameplay camera에서 readability를 별도로 검증한다.

이는 사용자 제공 조사 문서의 'Mesh + Particle/VFX Graph + Shader + Screen Distortion + Post Processing + Camera + Timing' 결론과 일치한다.

## JJK_game 적용 우선순위

현재 repo 현실:
- Unity 6 / URP 17.3.
- VFX Graph 미설치.
- PC Renderer Feature는 SSAO 중심.
- Full Screen Pass/distortion pipeline 미구축.
- Pass 7이 camera shake / FOV / flash / hit-stop 소유.

추천 순서:

A. Blue R-2A Unity visual acceptance 마무리
- shader/runtime hotfix 반영.
- core/spiral/arc가 실제 Game View에서 합격하는지 확인.

B. Pass 8R-2B — Reusable Gojo Distortion Pipeline + Blue Final Polish
- generic distortion source contract.
- Blue: inward lens/pinch.
- Red: outward bulge/repulsion.
- Purple: stronger localized warp.
- renderer asset raw YAML 직접 수정 금지.
- Editor-safe Renderer Feature / Full Screen Pass 방식 검토.
- HUD는 distortion 영향 밖.

C. Pass 8R-2C — Red Production VFX
- Red core shader.
- 2~3 layered shock fronts.
- outward distortion.
- pressure build → release.

D. Pass 8R-2D — Hollow Purple Production VFX
- Blue/Red material reuse.
- dedicated merge transition.
- purple body shader.
- lightning/filament.
- short trail.
- localized distortion.
- impact screen accent.
- core 승인 후 presentation-only debris/environment response 검토.

E. Pass 8R-2E — Unlimited Void Production Environment
- arena visual suppression.
- dedicated void environment.
- tunnel/entry transition.
- active environment.
- break/fade exit.
- reversible presentation state.
- gameplay collider/rules untouched.

## 금지 / 주의
- 외부 유료 asset/project file/shader/texture 리핑·복사 금지.
- reference는 visual principle만 사용.
- VFX 때문에 gameplay timing을 바꾸지 않음.
- screen effect가 HUD readability를 깨지 않음.
- user visual acceptance 전 USER VERIFIED 금지.