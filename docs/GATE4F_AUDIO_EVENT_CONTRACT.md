# Gate 4F — Audio Event Contract

작성 기준일: 2026-08-23

## 상태

```text
Gate 4A Character Presentation Contract: USER VERIFIED
Gate 4B HUD Data Binding Extraction: USER VERIFIED
Gate 4C Technique Presentation Request: USER VERIFIED
Gate 4D VFX Lifecycle Contract: USER VERIFIED
Gate 4E Animation Contract: USER VERIFIED
Gate 4F Pass 1 · Semantic Audio Event Boundary: USER VERIFIED
Gate 4F Pass 2 · Signature / Sukuna / Result Audio Migration: USER VERIFIED
Gate 4F Audio Event Contract: USER VERIFIED
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 목적

Prototype 단계에서는 Gameplay/Presentation 코드가 `PrototypeCombatAudio` 또는 `SukunaCombatAudio`를 직접 찾아 구체적인 재생 메서드를 호출했고, `PrototypeCombatAudio` 자체도 Health/visual 상태를 관찰해 소리를 결정했다.

Gate 4F는 다음 경계를 확보한다.

```text
Gameplay / Presentation event
→ CombatAudioEvent
→ Audio runtime adapter
→ 현재 Prototype audio runtime
```

Gameplay는 AudioClip, AudioSource, Resources path, Mixer, fallback synthesis를 모른다.

---

# Pass 1 — Semantic Audio Event Boundary · USER VERIFIED

핵심 파일:

```text
unity/Assets/Scripts/Core/CombatAudioEvent.cs
unity/Assets/Scripts/Player/PrototypeCombatAudioEventBridge.cs
```

먼저 옮긴 producer:

```text
BasicAttack
- BasicSwing
- BasicHit

ThirdPersonPlayerController
- Dodge

GojoTechniqueController
- GojoBlueCast
- GojoBlueImpact
- GojoRedCast
- GojoRedImpact
```

Event 정보:

```text
Owner
Semantic Event ID
Variant
Amplified 여부
```

Event에 넣지 않는 구현 세부사항:

```text
AudioClip
Resources path
AudioSource
Volume
Mixer Group
Fallback waveform
Camera shake/flash 수치
```

2026-08-23 사용자 Unity 결과:

```text
정상
```

---

# Pass 2 — Signature / Sukuna / Result Audio Migration · USER VERIFIED

현재 semantic id:

```text
BasicSwing
BasicHit
Dodge
GojoBlueCast
GojoBlueImpact
GojoRedCast
GojoRedImpact
TechniqueImpact
HollowPurple
UnlimitedVoid
MalevolentShrine
Fuga
PlayerHit
Victory
Defeat
```

`TechniqueImpact`는 기존 prototype 코드의 강한 기술 impact 호출을 특정 캐릭터 이름에 묶지 않고 event boundary로 넘기기 위한 호환 cue다.

## PrototypeCombatAudio 역할 축소

기존에는:

```text
AudioSource/clip playback
+ HealthChanged 관찰
+ 전체 Health death 관찰
+ Hollow Purple visual activation 관찰
+ Unlimited Void visual activation 관찰
```

까지 담당했다.

Pass 2 이후에는 playback runtime 역할을 중심으로 남겼다.

기존 `Play...` public 메서드는 회귀 안전용 compatibility shim으로 유지하지만 실제 clip playback 대신 semantic event를 발행한다.

```text
PlayBasicSwing(3)
→ BasicSwing variant=3

PlayRedImpact()
→ TechniqueImpact

PlayVictory()
→ Victory
```

실제 재생은 `Play...Runtime()` 계열이 담당하며 `PrototypeCombatAudioEventBridge`가 호출한다.

새 production-facing producer는 compatibility shim 대신 `CombatAudioEvents`를 직접 발행하는 것을 원칙으로 한다.

## PrototypeCombatAudioEventSource

```text
unity/Assets/Scripts/Player/PrototypeCombatAudioEventSource.cs
```

아직 독립 production producer가 없는 prototype 상태를 관찰해 event만 발행한다.

```text
owner HP 감소 → PlayerHit
owner 사망 → Defeat
마지막 opponent 사망 → Victory
HollowPurplePrototypeVisual activation → HollowPurple
UnlimitedVoidPrototypeVisual activation → UnlimitedVoid
```

이 Source는 AudioClip을 재생하지 않는다.

## Sukuna audio migration

기존 gameplay timing을 보존하기 위해 큰 Controller의 호출 시점은 유지하되, 내부는 semantic event로 변환한다.

```text
SukunaDomainController
→ SukunaCombatAudio.PlayDomain()
→ MalevolentShrine event

SukunaDomainFugaBlast
→ SukunaCombatAudio.PlayDomainFuga()
→ Fuga amplified event
```

SukunaTechniqueController의 기존 `PlayBasicSwing / PlayBasicHit / PlayRedImpact` 호출 역시 compatibility shim을 통해 event boundary로 진입한다.

현재 흐름:

```text
Basic / Dodge / Gojo producer
→ CombatAudioEvents 직접 발행

Prototype-only visual/result observer
→ PrototypeCombatAudioEventSource
→ CombatAudioEvents

기존 Sukuna prototype caller
→ compatibility shim
→ CombatAudioEvents

CombatAudioEvents
→ PrototypeCombatAudioEventBridge
   ├→ PrototypeCombatAudio runtime playback
   └→ SukunaCombatAudio runtime playback
```

---

# 사용자 검증

2026-08-23 사용자가 Pass 2 회귀를 확인했다.

```text
기본공격 / Dodge
창 / 혁
허식 자 / 무량공처
스쿠나 해 / 팔 / 푸가 / 복마어주자
Player Hit
Victory / Defeat
F2 scene reload
기존 HP / CE / Tag / Camera / VFX 회귀
```

사용자 결과:

```text
정상이다!!
```

따라서:

```text
Gate 4F Pass 2: USER VERIFIED
Gate 4F Audio Event Contract: USER VERIFIED
```

---

# 바꾸지 않은 Gameplay / Presentation

```text
Damage
HP / CE
Cooldown
Basic combo timing
Dodge movement / invulnerability
창/혁 판정
허식 자 판정 및 visual timing
푸가 조건/피해/폭발
Domain rules
Tag / KO / Target handoff
Victory / Defeat 조건
VFX lifecycle
Animation contract
```

기존 prototype audio tuning과 camera shake/flash 수치도 유지했다.

---

# 다음

```text
Prototype-only / Production-candidate 경계 최종 정리
→ Gate 4 전체 regression
→ Gate 4 CLOSE
```
