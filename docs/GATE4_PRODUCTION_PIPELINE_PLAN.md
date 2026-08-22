# Gate 4 — Production Pipeline Extraction Plan

작성 기준일: 2026-08-23

## 상태

```text
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED
Gate 4 Production Pipeline Extraction: FINAL REGRESSION PENDING

4A Character Presentation Contract: USER VERIFIED
4B HUD Data Binding Extraction: USER VERIFIED
4C Technique Presentation Request: USER VERIFIED
4D VFX Lifecycle Contract: USER VERIFIED
4E Animation Contract: USER VERIFIED
4F Audio Event Contract: USER VERIFIED
Prototype / Production Boundary Review: COMPLETE
Gate 4 Final Regression: USER TEST PENDING
```

관련 문서:

```text
docs/ROADMAP_SCOPE_REFINEMENT.md
docs/GATE4_PRODUCTION_BOUNDARIES.md
docs/GATE4_FINAL_REGRESSION.md
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

# 4A — Character Presentation Contract ✅ USER VERIFIED

```text
CharacterPresentationProfile
→ 이름 / variant / HUD accent / CE accent / Q/E/R/V label+accent
```

Gameplay damage/CE/domain rule은 profile 소유가 아니다.

---

# 4B — HUD Data Binding Extraction ✅ USER VERIFIED

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

F1 Help overlay의 화면 가림은 prototype layout debt로 남겨 Gate 5B final-style Canvas/TMP HUD에서 해결한다.

---

# 4C — Technique Presentation Request ✅ USER VERIFIED

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

# 4D — VFX Lifecycle Contract ✅ USER VERIFIED

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

향후 Particle prefab / VFX Graph / pooled runtime으로 교체할 수 있는 lifecycle 경계를 확보했다.

---

# 4E — Animation Contract ✅ USER VERIFIED

```text
Gameplay Components
→ FighterAnimationStateSource
→ FighterAnimationStateSnapshot / FighterAnimationCue
→ 현재 procedural pose 또는 미래 Animator adapter
```

현재 실제 rigged model/Animator asset이 없으므로 Animator parameter/layer/Avatar Mask를 임의로 확정하지 않는다.

---

# 4F — Audio Event Contract ✅ USER VERIFIED

```text
Gameplay / Presentation event
→ CombatAudioEvent
→ PrototypeCombatAudioEventBridge
→ 현재 prototype audio runtime
```

Pass 1:

```text
BasicAttack / Dodge / Gojo 창·혁
→ semantic event migration
```

Pass 2:

```text
Sukuna / Signature / PlayerHit / Victory / Defeat
→ semantic event boundary migration
```

`PrototypeCombatAudio`는 playback runtime 중심으로 역할을 축소했고, prototype-only 상태 관찰은 `PrototypeCombatAudioEventSource`로 분리했다.

2026-08-23 사용자 결과:

```text
정상이다!!
```

따라서 Gate 4F는 USER VERIFIED.

---

# Prototype / Production Boundary Review ✅ COMPLETE

세부 문서:

```text
docs/GATE4_PRODUCTION_BOUNDARIES.md
```

Production-candidate contract:

```text
Character Profile
HUD Snapshot
Technique Presentation Request
VFX Lifecycle
Animation State/Cue
Audio Event
```

Prototype-only implementation:

```text
procedural character/avatar rendering
IMGUI HUD
prototype arena art
LineRenderer/procedural VFX renderer
prototype audio bridge/runtime/fallback
prototype state observation adapters
```

Gate 5B에서는 contract를 유지하면서 실제 asset 기반 consumer/runtime으로 교체할 수 있다.

---

# Gate 4 완료 조건

```text
[x] Fighter identity/HUD data 공통 profile 경계
[x] Player/Opponent HUD read-only data contract
[x] Technique semantic presentation request
[x] VFX runtime/lifecycle contract
[x] Animation-facing state/cue contract
[x] Audio event contract 사용자 회귀 검증
[x] Prototype-only / Production-candidate 경계 최종 정리
[ ] Gate 4 전체 regression
```

마지막 체크리스트:

```text
docs/GATE4_FINAL_REGRESSION.md
```

이 회귀가 통과하면:

```text
Gate 4 Production Pipeline Extraction: USER VERIFIED / CLOSED
Gate 5A Third Character Stress Test: START
```

Gate 5A에서 세 번째 캐릭터를 추가해 실제로 복붙 없이 확장되는지 검증한다.

Gate 5A 통과 뒤 Gate 5B First Production-Quality Vertical Slice에서 실제 모델/Animator/VFX/Canvas-TMP/Voice/SFX/맵 아트를 대표 한 판에 연결한다.
