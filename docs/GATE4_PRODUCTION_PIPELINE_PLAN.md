# Gate 4 — Production Pipeline Extraction Plan

작성 기준일: 2026-08-23

## 최종 상태

```text
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED
Gate 4 Production Pipeline Extraction: USER VERIFIED / CLOSED

4A Character Presentation Contract: USER VERIFIED
4B HUD Data Binding Extraction: USER VERIFIED
4C Technique Presentation Request: USER VERIFIED
4D VFX Lifecycle Contract: USER VERIFIED
4E Animation Contract: USER VERIFIED
4F Audio Event Contract: USER VERIFIED
Prototype / Production Boundary Review: COMPLETE
Gate 4 Final Regression: USER VERIFIED
```

관련 문서:

```text
docs/ROADMAP_SCOPE_REFINEMENT.md
docs/GATE4_PRODUCTION_BOUNDARIES.md
docs/GATE4_FINAL_REGRESSION.md
```

---

# 목적

Gate 1~3의 빠른 prototype 구조에서 실제 반복된 부분만 추출해, 세 번째 Fighter부터 복붙 없이 재사용할 수 있는 제작 경계를 만든다.

```text
새 Fighter
→ Character identity/profile
→ 고유 Gameplay 구현
→ HUD Snapshot
→ Technique Presentation Request
→ Camera / VFX / Animation / Audio consumer
```

핵심 원칙:

```text
1. 실제 반복된 것만 공통화
2. 캐릭터 고유 Gameplay Rule은 범용 Ability System으로 억지 통합하지 않음
3. Data / Gameplay / Presentation 책임 분리
4. Presentation은 Gameplay 결과를 바꾸지 않음
5. Migration 뒤 Gojo/Sukuna 회귀 검증
6. 실제 asset이 없으면 가짜 production asset 규격을 고정하지 않음
```

---

# 완료된 경계

## 4A Character Presentation

```text
CharacterPresentationProfile
CharacterPresentationProfiles
CharacterSkillPresentation
```

이름 / variant / HUD accent / CE accent / Q/E/R/V presentation metadata를 소유한다.
Damage / CE 수치 / cooldown / domain rule은 소유하지 않는다.

## 4B HUD Data Binding

```text
Gameplay
→ PlayerCombatHudDataSource / OpponentCombatHudDataSource
→ read-only Snapshot
→ 현재 IMGUI HUD
```

최종 UI는 Gate 5B에서 Canvas/TMP로 교체 가능하다.

## 4C Technique Presentation Request

```text
Technique event
→ TechniquePresentationRequest
→ Camera/HitStop consumer
→ Spatial VFX consumer
```

Producer는 technique identity / phase / origin / direction / amplified 의미만 전달한다.

## 4D VFX Lifecycle

```text
PresentationVfxSpawnRequest
PresentationVfxHandle
IPresentationVfxRuntime
PresentationVfxRuntime
```

현재 procedural renderer 대신 향후 Particle / VFX Graph / Prefab / Pool runtime을 연결할 수 있다.

## 4E Animation Contract

```text
Gameplay
→ FighterAnimationStateSource
→ FighterAnimationStateSnapshot / FighterAnimationCue
→ prototype pose 또는 미래 Animator adapter
```

실제 rigged asset이 없으므로 Animator parameter/layer 구조는 아직 고정하지 않는다.

## 4F Audio Event

```text
Gameplay / Presentation
→ CombatAudioEvent
→ Audio adapter
→ 현재 prototype audio runtime
```

Gameplay는 AudioClip / AudioSource / Resources path / Mixer 구조를 모른다.

---

# Gate 4 완료 조건

```text
[x] Fighter identity/profile 경계
[x] Player/Opponent HUD read-only contract
[x] Technique semantic presentation request
[x] VFX lifecycle contract
[x] Animation-facing state/cue contract
[x] Audio event contract
[x] Prototype-only / Production-candidate 경계 정리
[x] Gate 4 전체 regression
```

2026-08-23 사용자 최종 판정:

```text
정상
```

따라서 Gate 4를 닫는다.

---

# NEXT — Gate 5A Third Character Stress Test

1차 스트레스 캐릭터:

```text
후시구로 메구미
```

선택 이유:

```text
식신/소환체라는 별도 전투 주체가 필요해
단순 투사체 캐릭터보다 Gate 4 contract를 더 강하게 검증할 수 있음
```

Gate 5A에서 공통 Presentation/HUD를 복사해야 한다면 Gate 4 경계를 보강한다.
고유 식신 Gameplay 자체가 별도 코드인 것은 정상이다.

Gate 5A 통과 뒤 Gate 5B First Production-Quality Vertical Slice에서 실제 모델/Animator/VFX/Canvas-TMP/Voice/SFX/맵 아트를 대표 한 판에 연결한다.
