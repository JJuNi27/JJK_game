# Gate 4C — Technique Presentation Request

작성 기준일: 2026-08-23

## 상태

```text
Gate 4A Character Presentation Contract: USER VERIFIED
Gate 4B HUD Data Binding Extraction: USER VERIFIED
Gate 4C Pass 1 · Semantic Request + Camera/HitStop Director: USER VERIFIED
Gate 4C Pass 2 · Shared Origin/Direction + Spatial VFX Consumer: REMOTE IMPLEMENTED / USER TEST PENDING
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

Pass 1에서 기존 Camera/HitStop 직접 호출을 다음 경로로 이관했다.

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

따라서:

```text
Gate 4C Pass 1: USER VERIFIED
```

---

# Pass 2 — Semantic Origin / Direction Contract

Camera와 Spatial VFX가 같은 request를 소비하려면 request의 위치 정보가 특정 소비자의 완성 좌표여서는 안 된다.

Pass 1에서는 `WorldPoint`가 사실상 Camera Focus용 좌표에 가까웠다.
Pass 2에서는 이 값을 `술식 이벤트가 실제로 발생한 semantic origin`으로 정리하고 방향을 추가했다.

현재 Request가 제공하는 정보:

```text
Owner
Technique ID
Phase
Semantic World Origin
Optional Direction
Amplified 여부
```

새 factory:

```text
TechniquePresentationRequest.AtPose(...)
```

Producer는 이제 다음 정도만 알려준다.

```text
허식 자 Release
- origin: caster 앞 실제 release 지점
- direction: caster forward

푸가 Release
- origin: projectile 생성/발사 지점
- direction: projectile forward

푸가 Impact
- origin: 실제 폭발 지점

무량공처
- origin: caster 위치

복마어주자 Active
- origin: DomainCenter
```

Camera 전용 `+7.5m`, `+3.2m` 같은 focus offset은 Producer가 알지 않는다.

---

# Pass 2 — Camera Director refinement

`PrototypeTechniquePresentationDirector`가 semantic origin/direction을 받아 기술/phase별 Camera Focus 좌표를 스스로 계산하도록 변경했다.

즉:

```text
Request = 사건의 의미/위치
Director = 카메라 표현 규칙
```

경계를 더 명확하게 했다.

Gate 3에서 사용자 검증된 Flash/Shake/FOV/Focus/HitStop 강도와 지속시간은 유지한다.

---

# Pass 2 — Spatial VFX Consumer Migration

변경:

```text
unity/Assets/Scripts/Player/PrototypeSignatureSpatialVfxController.cs
```

기존에는 Spatial VFX Controller가 별도로 다시 감지했다.

```text
GojoDomainController state 감시
SukunaDomainController state 감시
HollowPurplePrototypeVisual active 감시
SukunaFugaProjectile 검색
SukunaFugaProjectile.Exploded 직접 구독
```

현재는 이 중복 탐지를 제거하고:

```text
TechniquePresentationRequests
→ PrototypeSignatureSpatialVfxController
→ PrototypeSignatureSpatialVfx
```

경로로 바꿨다.

현재 shared request consumer:

```text
Camera / HitStop Director
Spatial VFX Controller
```

두 소비자가 동일한 술식 사건을 받지만 서로의 구현을 알 필요가 없다.

## Spatial mapping

```text
UnlimitedVoid / Anticipation
→ blue follow aura

UnlimitedVoid / Active
→ large blue world burst

MalevolentShrine / Anticipation
→ red follow aura

MalevolentShrine / Active
→ large shrine-center red burst

HollowPurple / Release
→ purple release burst

Fuga / Release
→ projectile release burst

Fuga / Impact
→ explosion burst
```

기존 Gate 3 Spatial VFX의 색상/반경/지속시간/회전값을 그대로 이동했다.

---

# 의도적으로 아직 안 옮긴 것

```text
Hollow Purple runtime orb override
Voice / SFX
Animation
```

허식 자의 Prototype Visual/Damage timing 불일치도 이번 migration에서 변경하지 않는다.

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
[ ] 허식 자 Release: 기존 보라색 공간 burst가 caster 앞에 정상 발생
[ ] 허식 자 Camera Flash/Shake/FOV/Focus도 기존 체감 유지
[ ] 무량공처 준비: blue follow aura + camera pull/focus 정상
[ ] 무량공처 개방: 큰 blue burst + camera 연출 정상
[ ] T → 스쿠나 정상
[ ] 복마어주자 준비: red follow aura 정상
[ ] 복마어주자 개방: DomainCenter 부근 큰 red burst 정상
[ ] 영역 밖 푸가 발사: projectile 위치에서 orange/red release burst 정상
[ ] 푸가 폭발: 실제 impact 위치에서 큰 burst 정상
[ ] 영역 안 강화 푸가도 더 강한 spatial burst 정상
[ ] 기술 후 카메라 정상 복귀
[ ] 동일 술식에서 Spatial VFX가 중복으로 두 번 나오지 않음
[ ] HP/CE/Damage/Tag/Domain gameplay 회귀 없음
```

통과하면:

```text
Gate 4C Pass 2: USER VERIFIED
```

다음은 shared request와 현재 Hollow Purple runtime visual override의 경계를 정리하고 Gate 4D VFX Lifecycle Contract로 넘어갈지 판단한다.
