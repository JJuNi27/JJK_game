# Gate 4C — Technique Presentation Request

작성 기준일: 2026-08-23

## 상태

```text
Gate 4A Character Presentation Contract: USER VERIFIED
Gate 4B HUD Data Binding Extraction: USER VERIFIED
Gate 4C Pass 1 · Semantic Request + Camera/HitStop Director: USER VERIFIED
Gate 4C Pass 2 · Shared Origin/Direction + Spatial VFX Consumer: USER VERIFIED

Gate 4C Technique Presentation Request: USER VERIFIED
NEXT: Gate 4D VFX Lifecycle Contract
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 목적

Gate 3에서는 대표 술식의 연출을 빠르게 증명하기 위해 `PrototypeSignatureTechniqueFeedbackController`가 Camera/HitStop 구현을 직접 호출하고, `PrototypeSignatureSpatialVfxController`도 같은 술식 상태를 별도로 다시 감지했다.

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

# Pass 1 — Semantic Request + Camera/HitStop · USER VERIFIED

파일:

```text
unity/Assets/Scripts/Core/TechniquePresentationRequest.cs
unity/Assets/Scripts/Core/PrototypeTechniquePresentationDirector.cs
unity/Assets/Scripts/Player/PrototypeSignatureTechniqueFeedbackController.cs
```

핵심 타입:

```text
TechniquePresentationId
TechniquePresentationPhase
TechniquePresentationRequest
TechniquePresentationRequests
```

경로:

```text
술식 상태 탐지
→ TechniquePresentationRequest
→ PrototypeTechniquePresentationDirector
→ Flash / Shake / FOV / World Focus / Hit Stop
```

2026-08-23 사용자 실제 확인:

```text
정상이다
```

---

# Pass 2 — Shared Origin / Direction + Spatial VFX · USER VERIFIED

Request 위치를 특정 카메라 소비자의 완성 좌표가 아니라 `술식 사건의 semantic origin`으로 정리하고 방향을 추가했다.

현재 Request가 제공하는 정보:

```text
Owner
Technique ID
Phase
Semantic World Origin
Optional Direction
Amplified 여부
```

Producer는 사건의 의미/위치만 알리고, Camera Director가 focus offset을 계산한다.

Spatial VFX Controller도 더 이상 Gojo/Sukuna domain state, Purple visual, Fuga projectile를 별도로 다시 감지하지 않고 같은 semantic request를 소비한다.

현재 구조:

```text
Technique event detection
→ TechniquePresentationRequest
   ├→ Camera / HitStop Director
   └→ Spatial VFX Controller
```

Spatial mapping:

```text
UnlimitedVoid / Anticipation → blue follow aura
UnlimitedVoid / Active       → large blue world burst
MalevolentShrine / Anticipation → red follow aura
MalevolentShrine / Active       → large shrine-center red burst
HollowPurple / Release       → purple release burst
Fuga / Release               → projectile release burst
Fuga / Impact                → explosion burst
```

Gate 3에서 검증된 색상/반경/지속시간/회전값을 유지했다.

2026-08-23 사용자 실제 확인:

```text
전부 정상!
```

확인 범위에는 허식 자, 무량공처, 복마어주자, 영역 밖/안 푸가, 카메라 복귀, Spatial VFX 중복 여부가 포함됐다.

따라서:

```text
Gate 4C Pass 2: USER VERIFIED
Gate 4C Technique Presentation Request: USER VERIFIED
```

---

# 의도적으로 아직 별도 경계에 남긴 것

```text
Hollow Purple runtime orb override
Voice / SFX
Animation
```

허식 자 Prototype Visual/Damage timing 불일치도 Gate 4C에서 변경하지 않았다.

다음 Gate 4D에서는 Spatial VFX의 `구체 렌더러 구현` 자체를 lifecycle runtime 뒤로 숨긴다.

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
