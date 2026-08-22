# Gate 4C — Technique Presentation Request

작성 기준일: 2026-08-23

## 상태

```text
Gate 4A Character Presentation Contract: USER VERIFIED
Gate 4B HUD Data Binding Extraction: USER VERIFIED
Gate 4C Pass 1 · Semantic Request + Camera/HitStop Director: REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 목적

Gate 3에서는 대표 술식의 연출을 빠르게 증명하기 위해 `PrototypeSignatureTechniqueFeedbackController`가 다음 구현을 직접 호출했다.

```text
SimpleCameraFollow.Flash
SimpleCameraFollow.AddShake
SimpleCameraFollow.AddFovKick
SimpleCameraFollow.AddWorldFocus
PrototypeHitStopController.Request
```

이 방식은 고죠/스쿠나 두 명을 검증하기에는 빠르지만, 캐릭터와 술식이 늘어나면 탐지 코드가 카메라/히트스톱의 세부 튜닝까지 직접 알아야 한다.

Gate 4C의 목표는:

```text
"무슨 술식에서 어떤 단계가 발생했는가"
```

와

```text
"그 사건을 카메라/VFX/SFX로 어떻게 표현하는가"
```

를 분리하는 것이다.

거대한 범용 Ability System은 만들지 않는다.

---

# Pass 1 — Semantic Request Contract

새 파일:

```text
unity/Assets/Scripts/Core/TechniquePresentationRequest.cs
```

핵심 타입:

```text
TechniquePresentationId
- HollowPurple
- Fuga
- UnlimitedVoid
- MalevolentShrine

TechniquePresentationPhase
- Anticipation
- Release
- Culmination
- Impact
- Active
- End

TechniquePresentationRequest
TechniquePresentationRequests
```

Request가 담는 정보:

```text
Owner
Technique ID
Phase
Optional World Point
Amplified 여부
```

중요하게 Request에는 다음 세부 튜닝이 없다.

```text
Flash color/alpha
Shake amplitude/duration
FOV delta
Focus strength
Hit Stop duration/scale
```

즉 producer는 연출 구현 수치를 알 필요가 없다.

---

# Pass 1 — Presentation Director

새 파일:

```text
unity/Assets/Scripts/Core/PrototypeTechniquePresentationDirector.cs
```

이 Director가 semantic request를 구독하고 Gate 3에서 검증된 기존 presentation 값을 적용한다.

현재 소비 구현:

```text
Camera Flash
Camera Shake
FOV Kick
World Focus
Hit Stop
```

Gate 3에서 이미 사용자 검증된 수치를 그대로 옮겼으며 Gameplay 수치나 판정을 바꾸지 않는다.

---

# Producer Migration

변경:

```text
unity/Assets/Scripts/Player/PrototypeSignatureTechniqueFeedbackController.cs
```

기존:

```text
술식 상태 탐지
→ Camera/HitStop 구현 직접 호출
```

현재:

```text
술식 상태 탐지
→ TechniquePresentationRequest 발행
→ PrototypeTechniquePresentationDirector
→ Camera/HitStop 적용
```

현재 request mapping:

```text
무량공처 DomainReady → UnlimitedVoid / Anticipation
무량공처 Active      → UnlimitedVoid / Active

복마어주자 Casting   → MalevolentShrine / Anticipation
복마어주자 Active    → MalevolentShrine / Active

허식 자 시작         → HollowPurple / Release
허식 자 후속 강조    → HollowPurple / Culmination

푸가 입력 anticipation → Fuga / Anticipation
푸가 projectile 생성   → Fuga / Release
푸가 폭발               → Fuga / Impact
```

푸가는 기존과 동일하게 영역 강화 여부를 `Amplified`로 넘긴다.

---

# 의도적으로 아직 안 옮긴 것

```text
Spatial procedural VFX
Hollow Purple runtime orb override
Voice / SFX
Animation
```

이들은 같은 request를 소비하도록 다음 Pass/Gate 4D~4F에서 단계적으로 연결한다.

특히 허식 자 Visual/Damage timing 불일치는 이번 migration에서 건드리지 않는다.

---

# 바꾸지 않은 Gameplay

```text
Damage
CE cost / regen
Cooldown
Fuga 준비 조건
Domain input/cast/active rule
Infinity
Sure Hit
Tag
KO
Target Lock
Victory / Defeat
```

---

# 사용자 테스트

```text
1. cd D:\GitHub\JJK_game
2. git pull origin master
3. Unity compile 완료 대기
4. Console 빨간 오류 확인
5. CombatMVP Play
```

빠른 회귀:

```text
[ ] 허식 자: 기존처럼 release Flash/Shake/FOV/Focus/HitStop 체감
[ ] 허식 자: 청/적 합류 → 큰 보라 구체 형태 유지
[ ] 무량공처: 준비 순간 camera pull/focus 정상
[ ] 무량공처: 개방 순간 Flash/Shake/FOV/Focus 정상
[ ] T → 스쿠나 정상
[ ] 영역 밖 푸가: anticipation → projectile release → explosion 강조 정상
[ ] 복마어주자: casting anticipation / active 연출 정상
[ ] 영역 안 강화 푸가 연출 정상
[ ] 기술 후 카메라가 정상 시점으로 복귀
[ ] HP/CE/Damage/Tag/Domain gameplay 회귀 없음
```

통과하면:

```text
Gate 4C Pass 1: USER VERIFIED
```

다음 Pass에서는 Spatial VFX controller가 동일 semantic request를 소비하도록 경계를 확장할지 조사한다.
