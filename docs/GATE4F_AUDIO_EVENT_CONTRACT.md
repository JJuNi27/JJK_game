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
Gate 4F Pass 2 · Signature / Sukuna / Result Audio Migration: REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 목적

Prototype 단계에서는 Gameplay/Presentation 코드가 `PrototypeCombatAudio` 또는 `SukunaCombatAudio`를 직접 찾아 구체적인 재생 메서드를 호출했고, `PrototypeCombatAudio` 자체도 Health/visual 상태를 관찰해 소리를 결정했다.

Gate 4F의 목표는 다음 경계를 확보하는 것이다.

```text
Gameplay / Presentation event
→ CombatAudioEvent
→ Audio runtime adapter
→ 현재 Prototype audio runtime
```

Gameplay는 어떤 AudioClip, AudioSource, Resources path, Mixer, fallback synthesis가 사용되는지 모른다.

---

# Pass 1 — Semantic Audio Event Boundary · USER VERIFIED

핵심 파일:

```text
unity/Assets/Scripts/Core/CombatAudioEvent.cs
unity/Assets/Scripts/Player/PrototypeCombatAudioEventBridge.cs
```

Pass 1에서 먼저 옮긴 producer:

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

Event가 담는 정보:

```text
Owner
Semantic Event ID
Variant
Amplified 여부
```

다음 구현 세부사항은 event에 포함하지 않는다.

```text
AudioClip
Resources path
AudioSource
Volume
Mixer Group
Fallback waveform
Camera shake/flash 수치
```

2026-08-23 사용자 Unity 확인:

```text
정상
```

따라서:

```text
Gate 4F Pass 1: USER VERIFIED
```

---

# Pass 2 — Signature / Sukuna / Result Audio Migration

## CombatAudioEvent 확장

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

`TechniqueImpact`는 기존 prototype 코드에서 특정 캐릭터 이름을 몰라도 사용할 수 있는 범용 강한 기술 impact 호환 cue다.

---

# PrototypeCombatAudio 역할 축소

변경:

```text
unity/Assets/Scripts/Core/PrototypeCombatAudio.cs
```

기존에는 이 컴포넌트가:

```text
AudioSource/clip playback
+ HealthChanged 관찰
+ 전체 Health death 관찰
+ Hollow Purple visual activation 관찰
+ Unlimited Void visual activation 관찰
```

까지 동시에 담당했다.

Pass 2에서는 playback runtime 역할만 남겼다.

기존 `Play...` public 메서드는 회귀 안전을 위한 compatibility shim으로 유지하지만 더 이상 직접 clip을 재생하지 않는다.

예:

```text
PlayBasicSwing(3)
→ CombatAudioEvent.BasicSwing variant=3

PlayRedImpact()
→ CombatAudioEvent.TechniqueImpact

PlayVictory()
→ CombatAudioEvent.Victory
```

실제 clip 재생은 `Play...Runtime()` 메서드로 분리하고 `PrototypeCombatAudioEventBridge`만 호출한다.

새 production-facing 코드는 compatibility shim 대신 `CombatAudioEvents`를 직접 발행하는 것을 원칙으로 한다.

---

# PrototypeCombatAudioEventSource

새 파일:

```text
unity/Assets/Scripts/Player/PrototypeCombatAudioEventSource.cs
```

이 컴포넌트는 기존 prototype에서 아직 독립 semantic producer가 없던 상태를 관찰해 **event만 발행**한다.

```text
owner HP 감소
→ PlayerHit

owner 사망
→ Defeat

마지막 opponent 사망
→ Victory

HollowPurplePrototypeVisual activation
→ HollowPurple

UnlimitedVoidPrototypeVisual activation
→ UnlimitedVoid
```

중요:

```text
PrototypeCombatAudioEventSource는 AudioClip을 재생하지 않는다.
PrototypeCombatAudioEventBridge가 event를 받아 runtime playback을 수행한다.
```

따라서 prototype observation과 audio playback 책임을 분리했다.

---

# Sukuna Audio Migration

변경:

```text
unity/Assets/Scripts/Player/SukunaCombatAudio.cs
```

기존 호출:

```text
SukunaDomainController
→ SukunaCombatAudio.PlayDomain()

SukunaDomainFugaBlast
→ SukunaCombatAudio.PlayDomainFuga()
```

은 그대로 유지해 gameplay timing 회귀를 줄이되, 메서드 내부가 semantic event를 발행하도록 변경했다.

```text
PlayDomain()
→ MalevolentShrine

PlayDomainFuga()
→ Fuga amplified=true
```

`PrototypeCombatAudioEventBridge`가 이를 받아 현재 Sukuna runtime sound를 재생한다.

또 기존 `SukunaTechniqueController` 안의:

```text
PlayBasicSwing
PlayBasicHit
PlayRedImpact
```

호출도 `PrototypeCombatAudio` compatibility shim을 통과해 자동으로 semantic event 경계로 들어간다.

큰 Gameplay controller를 불필요하게 다시 작성하지 않고 기존 시점/조건을 보존하는 migration이다.

---

# 현재 Audio 흐름

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

향후 production audio adapter를 붙일 때 Gameplay producer를 AudioClip 구조에 맞춰 다시 작성하지 않는 것이 목표다.

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

기존 prototype audio tuning과 camera shake/flash 수치도 이번 Pass에서 의도적으로 유지한다.

---

# Pass 2 사용자 테스트

```text
1. cd D:\GitHub\JJK_game
2. git pull origin master
3. Unity compile 완료 대기
4. Console 빨간 오류 확인
5. CombatMVP Play
```

빠른 회귀:

```text
[ ] 고죠 기본공격 1/2/3 swing 정상
[ ] 명중 hit / 3타 finisher sound 정상
[ ] SPACE Dodge sound 정상
[ ] Q 창 cast + impact 정상
[ ] E 혁 cast + impact 정상
[ ] 허식 자 sound 정확히 1회, 이상한 중복 없음
[ ] 무량공처 sound 정확히 1회, 이상한 중복 없음
[ ] 플레이어 피격 sound 정상
[ ] T → 스쿠나 후 해/팔의 swing/hit sound 정상
[ ] 영역 밖 푸가 준비/impact sound 정상
[ ] 복마어주자 domain sound 정상
[ ] 영역 안 강화 푸가 impact + domain blast sound 정상, 이상한 이중/에코 없음
[ ] VICTORY / DEFEAT sound 정상
[ ] F2 scene reload 후 audio 정상
[ ] HP/CE/Damage/Tag/Camera/VFX 회귀 없음
[ ] Console 빨간 오류 없음
```

통과하면:

```text
Gate 4F Pass 2: USER VERIFIED
Gate 4F Audio Event Contract: USER VERIFIED
```

그 뒤 Gate 4 마지막 작업은:

```text
Prototype-only / Production-candidate 경계 최종 정리
→ Gate 4 전체 regression
→ Gate 4 CLOSE
```
