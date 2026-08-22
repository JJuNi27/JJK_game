# Gate 3 — Beauty Corner

작성 기준일: 2026-08-23

## 최종 상태

```text
Gate 1 Core Combat Proof: USER VERIFIED
Gate 2 Match Architecture Proof: USER VERIFIED
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED

Gate 3A Combat Feel: USER VERIFIED
Gate 3B Prototype Character Presentation: USER VERIFIED
Gate 3C Signature Technique Presentation: USER VERIFIED
Gate 3D Arena + HUD Polish: USER VERIFIED
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

최종 회귀 기록:

```text
docs/GATE3_FINAL_REGRESSION.md
```

---

# Gate 3 목적

Gate 3은 게임 전체를 최종 아트 품질로 완성하는 단계가 아니라, 대표 전투 범위에서 다음이 실제 플레이 중 함께 성립하는지 증명하는 단계다.

```text
Combat Feel
Character Presentation
Signature Technique Presentation
Arena Mood
HUD Information Hierarchy
```

2026-08-23 최종 한 세션 회귀에서 사용자가 `다 정상!`으로 확인하여 Gate 3을 닫는다.

---

# 3A — Combat Feel · USER VERIFIED

```text
기본 3타 적중 Camera Shake
짧은 Hit Stop
Hit Flash
Impact 위치 Procedural Burst
1타 < 2타 < 3타 FINISH 강도
허공/방어 시 잘못된 적중 피드백 방지
```

핵심 파일:

```text
unity/Assets/Scripts/Player/BasicAttack.cs
unity/Assets/Scripts/Core/PrototypeHitStopController.cs
unity/Assets/Scripts/Core/PrototypeHitImpactVfx.cs
unity/Assets/Scripts/Camera/SimpleCameraFollow.cs
```

---

# 3B — Prototype Character Presentation · USER VERIFIED

현재 저장소에는 실제 rigged model/Animator 자산이 없다.

Prototype에서 검증:

```text
고죠 / 스쿠나 Fighter별 기본 자세 차이
locomotion lean
Dodge presentation
Tag entry presentation
Fighter Shell 유지 상태에서 Active Fighter 표현 교체
```

현재 procedural pose는 최종 애니메이션이 아니며 실제 모델이 들어오면 Animator 기반 전용 모션으로 교체한다.

---

# 3C — Signature Technique Presentation · USER VERIFIED

```text
Signature Flash / Shake / Hit Stop
FOV Kick
Camera World Focus
Spatial Ring / Burst VFX
푸가 Release / Explosion emphasis
무량공처 / 복마어주자 anticipation + activation
```

허식 「자」:

```text
기존 긴 Purple Beam 제거
청색 + 적색 구체 수렴
→ 큰 보라 구체 생성
→ 전방 이동
```

현재 Prototype 제한:

```text
Visual = 이동하는 구체
Damage = 기존 즉시 Capsule 판정
```

Visual/Damage timing 일치는 Production 구조에서 다룬다.

---

# 3D — Arena + HUD Polish · USER VERIFIED

Arena:

```text
야간 도심 Mood
Fog
비충돌 건물 실루엣
Neon accent
Arena ring
Mood light
```

HUD:

```text
Player Active / Reserve
Opponent Active / Reserve
HP / CE
Tag / KO / Reserve Entry
F2 Mode strip
Contextual Skill Deck
```

Skill Deck:

```text
고죠: Q 창 / E 혁 / R 허식 자 / V 무량공처
스쿠나: Q 해 / E 팔 / R 푸가 / V 복마어주자
```

상태:

```text
READY
DODGE
CASTING
DOMAIN INPUT
DOMAIN ACTIVE
TECHNIQUE BURNOUT
DISABLED
```

---

# 최종 회귀 · USER VERIFIED

한 세션에서 확인:

```text
Console 핵심 오류 없음
Player Tag / HP / CE / KO Auto Tag
Opponent Team / Reserve Entry / Victory
Target Lock handoff
기본 Hit Feedback
고죠 대표 술식 + 무량공처
스쿠나 대표 술식 + 복마어주자 + 푸가
허식 자 구체형
Arena presentation
HUD / Skill Deck 상태 전환
VICTORY / DEFEAT 조건
```

사용자 최종 판정:

```text
다 정상!
```

따라서:

```text
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED
```

---

# Gate 3 완료가 의미하지 않는 것

아직 Production 자산 의존:

```text
실제 rigged Gojo/Sukuna model
Animator animation set
final environment art
Canvas/TMP final HUD
production Voice/SFX/Music
```

Gate 3에서 증명한 것은 이 자산을 연결할 전투 감각, 표현 타이밍, 카메라/VFX 규칙, HUD 정보 구조다.

---

# 다음 — Gate 4 Production Pipeline Extraction

Beauty Corner에서 실제 반복된 구조만 공통화한다.

```text
4A Character Presentation Contract
4B HUD Data Binding
4C Technique Presentation Request
4D VFX Lifecycle
4E Animation Contract
4F Audio Event Contract
```

거대한 범용 Ability System은 만들지 않는다.

계획:

```text
docs/GATE4_PRODUCTION_PIPELINE_PLAN.md
```
