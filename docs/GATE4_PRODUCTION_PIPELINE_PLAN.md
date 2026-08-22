# Gate 4 — Production Pipeline Extraction Plan

작성 기준일: 2026-08-23

## 상태

```text
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED
Gate 4 Production Pipeline Extraction: IN PROGRESS

4A Character Presentation Contract: USER VERIFIED
4B HUD Data Binding Extraction: USER VERIFIED
4C Technique Presentation Request: USER VERIFIED
4D VFX Lifecycle Contract · Pass 1: REMOTE IMPLEMENTED / USER TEST PENDING
4E Animation Contract: NOT STARTED
4F Audio Event Contract: NOT STARTED
```

## 목적

Gate 1~3의 빠른 prototype 구조를 세 번째 캐릭터부터 반복 가능한 production pipeline으로 정리한다.

```text
새 Fighter 추가
→ identity/profile 등록
→ Gameplay 고유 규칙 구현
→ HUD는 snapshot 계약 소비
→ technique event는 semantic presentation request 발행
→ Camera/VFX/Animation/Audio는 독립 consumer
```

핵심 원칙:

```text
1. 실제 반복된 것만 공통화
2. 캐릭터 고유 Gameplay Rule은 억지로 범용화하지 않음
3. Data / Gameplay / Presentation 책임 분리
4. Presentation은 Gameplay 결과를 바꾸지 않음
5. migration 뒤 Gojo/Sukuna 회귀 검증
6. 실제 asset이 없는 부분은 가짜 production asset contract를 만들지 않음
```

---

# 4A — Character Presentation Contract ✅

```text
CharacterPresentationProfile
→ 이름 / variant / HUD accent / CE accent / Q/E/R/V label+accent
```

현재 Gojo/Sukuna identity 표시의 중복 하드코딩을 줄였다.

Gameplay damage/CE/domain rule은 profile 소유가 아니다.

---

# 4B — HUD Data Binding Extraction ✅

```text
Gameplay Components
→ PlayerCombatHudDataSource / OpponentCombatHudDataSource
→ read-only Snapshot
→ temporary IMGUI HUD
```

이관 범위:

```text
Skill Deck
Player Active/Reserve/Tag HUD
Opponent Active/Reserve/Mode HUD
Match HUD의 HP/CE/Dodge/Combo/Telegraph/Help identity
```

F1 Help overlay의 화면 가림은 prototype layout debt로 남겨 최종 Canvas/TMP HUD에서 해결한다.

---

# 4C — Technique Presentation Request ✅

```text
Technique event detection
→ TechniquePresentationRequest
   ├→ PrototypeTechniquePresentationDirector
   │   → Flash / Shake / FOV / Focus / HitStop
   └→ PrototypeSignatureSpatialVfxController
       → Spatial presentation
```

현재 semantic technique IDs:

```text
HollowPurple
Fuga
UnlimitedVoid
MalevolentShrine
```

Producer는 사건의 의미/실제 origin/direction/amplified 여부만 발행하고 Camera/VFX 세부 튜닝을 모른다.

---

# 4D — VFX Lifecycle Contract ▶

Pass 1 구현:

```text
PresentationVfxSpawnRequest
PresentationVfxTimePolicy
PresentationVfxStopMode
IPresentationVfxInstance
PresentationVfxHandle
IPresentationVfxRuntime
PresentationVfxRuntime
PrototypePresentationVfxRuntime
```

현재 구조:

```text
TechniquePresentationRequest
→ Spatial VFX mapping
→ PresentationVfxSpawnRequest
→ PresentationVfxRuntime
→ Prototype procedural renderer
```

목표:

```text
현재 LineRenderer prototype
→ 향후 Particle prefab / VFX Graph / pooled runtime
```

교체 시 technique producer/consumer 수정 최소화.

현재 lifecycle contract:

```text
World / Follow anchor
Scaled / Unscaled timing
Natural lifetime
FadeOut stop
Immediate stop
Destroy
```

허식 자 canonical orb runtime override는 이번 pass에서 억지 통합하지 않는다.

---

# 4E — Animation Contract

실제 rigged model/Animator asset 연결을 위한 event contract 후보:

```text
Locomotion
Dodge
Basic Attack 1/2/3
Technique Anticipation
Technique Release
Technique Recovery
Domain Start / Active / End
Tag Out / In
Hit / KO
```

현재 procedural avatar pose를 production Animator 구조로 그대로 확장하지 않는다.

---

# 4F — Audio Event Contract

목표:

```text
Voice Event
Technique SFX Event
Hit SFX Event
Domain SFX Event
Match Result Event
```

AudioClip 교체가 Gameplay Controller 수정으로 이어지지 않게 한다.

---

# Gate 4 완료 조건

```text
[x] Fighter identity/HUD data 공통 profile 경계
[x] Player/Opponent HUD read-only data contract
[x] Technique semantic presentation request
[ ] VFX runtime/lifecycle 사용자 회귀 검증
[ ] Animation production contract
[ ] Audio production contract
[ ] Prototype-only / Production-candidate 경계 최종 정리
```

완료 후 Gate 5 Third Character Stress Test에서 세 번째 캐릭터를 추가해 이 구조가 실제로 복붙 없이 확장되는지 검증한다.
