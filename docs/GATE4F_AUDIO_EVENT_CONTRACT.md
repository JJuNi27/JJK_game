# Gate 4F — Audio Event Contract

작성 기준일: 2026-08-23

## 상태

```text
Gate 4A Character Presentation Contract: USER VERIFIED
Gate 4B HUD Data Binding Extraction: USER VERIFIED
Gate 4C Technique Presentation Request: USER VERIFIED
Gate 4D VFX Lifecycle Contract: USER VERIFIED
Gate 4E Animation Contract: USER VERIFIED
Gate 4F Pass 1 · Semantic Audio Event Boundary: REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 목적

Prototype 단계에서는 Gameplay 코드가 `PrototypeCombatAudio`를 직접 찾아 다음 메서드를 호출했다.

```text
PlayBasicSwing
PlayBasicHit
PlayDodge
PlayBlueCast
PlayBlueImpact
PlayRedCast
PlayRedImpact
```

이 방식에서는 나중에 AudioClip, AudioSource, Mixer, Wwise/FMOD 같은 audio runtime 구조를 바꾸면 Gameplay 코드까지 수정할 위험이 있다.

Gate 4F는 다음 경계로 이동한다.

```text
Gameplay event
→ CombatAudioEvent
→ Audio runtime adapter
→ 현재 PrototypeCombatAudio
```

Gameplay는 어떤 AudioClip이 재생되는지 모른다.

---

# Pass 1 — CombatAudioEvent

새 파일:

```text
unity/Assets/Scripts/Core/CombatAudioEvent.cs
```

핵심 타입:

```text
CombatAudioEventId
CombatAudioEvent
CombatAudioEvents
```

현재 semantic id:

```text
BasicSwing
BasicHit
Dodge
GojoBlueCast
GojoBlueImpact
GojoRedCast
GojoRedImpact
HollowPurple
UnlimitedVoid
MalevolentShrine
Fuga
PlayerHit
Victory
Defeat
```

Event가 담는 정보:

```text
Owner
Semantic Event ID
Variant (예: 기본공격 1/2/3)
Amplified 여부
```

다음 구현 세부사항은 포함하지 않는다.

```text
AudioClip
Resources path
AudioSource
Volume
Mixer Group
Fallback waveform
Camera shake/flash 수치
```

---

# Prototype Audio Adapter

새 파일:

```text
unity/Assets/Scripts/Player/PrototypeCombatAudioEventBridge.cs
```

경로:

```text
CombatAudioEvents
→ PrototypeCombatAudioEventBridge
→ PrototypeCombatAudio
```

Bridge는 player owner를 기준으로 자기 이벤트만 소비한다.

현재 PrototypeCombatAudio의 local override/fallback clip 체계는 그대로 유지한다.

즉 production audio runtime을 도입할 때 producer를 바꾸지 않고 adapter를 교체할 수 있는 첫 경계를 확보했다.

---

# Pass 1 Producer Migration

## BasicAttack

기존:

```text
BasicAttack
→ PrototypeCombatAudio.GetOrCreate
→ PlayBasicSwing / PlayBasicHit
```

현재:

```text
BasicAttack
→ CombatAudioEvent.BasicSwing / BasicHit
```

1/2/3타 정보는 `Variant`로 전달한다.

## ThirdPersonPlayerController

기존:

```text
Dodge 시작
→ PrototypeCombatAudio.PlayDodge
```

현재:

```text
Dodge 시작
→ CombatAudioEvent.Dodge
```

## GojoTechniqueController

기존:

```text
창 cast/impact
혁 cast/impact
→ PrototypeCombatAudio 직접 호출
```

현재:

```text
GojoBlueCast
GojoBlueImpact
GojoRedCast
GojoRedImpact
→ CombatAudioEvents
```

Gameplay의 damage, CE, cooldown, projectile/field timing은 변경하지 않았다.

---

# 의도적으로 Pass 2로 남긴 것

현재 일부 prototype audio 경로는 아직 직접 연결이다.

```text
SukunaTechniqueController의 참격/푸가 보조 사운드
SukunaDomainController의 복마어주자/영역 푸가 사운드
Hollow Purple / Unlimited Void의 기존 visual activation 기반 audio trigger
PlayerHit / Victory / Defeat의 PrototypeCombatAudio 내부 Health 관찰
```

Pass 1 검증 후 이 경로들을 semantic event로 옮기고 중복 trigger를 제거할지 판단한다.

특히 허식 자/무량공처는 기존 audio trigger를 유지했으므로 이번 Pass에서 소리가 두 번 나도록 새 event를 발행하지 않는다.

---

# 바꾸지 않은 Gameplay / Presentation

```text
Damage
HP / CE
Cooldown
Basic combo timing
Dodge movement / invulnerability
창/혁 판정
허식 자
푸가
Domain rules
Tag / KO / Victory rules
Camera / HitStop
VFX
실제 AudioClip / fallback sound tuning
```

---

# Pass 1 사용자 테스트

```text
1. cd D:\GitHub\JJK_game
2. git pull origin master
3. Unity compile 완료 대기
4. Console 빨간 오류 확인
5. CombatMVP Play
```

빠른 회귀:

```text
[ ] 고죠 기본공격 1/2/3 swing sound 정상
[ ] 적 명중 시 hit/finisher sound 정상
[ ] SPACE Dodge sound 정상
[ ] Q 창 cast + impact sound 정상
[ ] E 혁 cast + impact sound 정상
[ ] 허식 자 / 무량공처 기존 sound 정상이며 중복 재생 없음
[ ] T → 스쿠나 후 기존 참격/푸가/영역 sound 회귀 없음
[ ] HP/CE/Damage/Tag/Camera/VFX 회귀 없음
[ ] Console 빨간 오류 없음
```

통과하면:

```text
Gate 4F Pass 1: USER VERIFIED
```

다음 Pass에서 Sukuna/Signature/Result audio 경로까지 공통 event boundary로 합쳐 Gate 4F를 닫는다.
