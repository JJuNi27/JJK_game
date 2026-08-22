# Gate 4D — VFX Lifecycle Contract

작성 기준일: 2026-08-23

## 상태

```text
Gate 4A Character Presentation Contract: USER VERIFIED
Gate 4B HUD Data Binding Extraction: USER VERIFIED
Gate 4C Technique Presentation Request: USER VERIFIED
Gate 4D Pass 1 · Runtime/Handle/Lifecycle Boundary: USER VERIFIED

Gate 4D VFX Lifecycle Contract: USER VERIFIED
NEXT: Gate 4E Animation Contract
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 목적

Gate 4C에서 Spatial VFX는 `TechniquePresentationRequest`를 소비하게 되었지만 여전히 구체 구현인 `PrototypeSignatureSpatialVfx`를 직접 호출하고 있었다.

Gate 4D는 다음 교체가 Gameplay/Technique consumer 변경으로 이어지지 않게 VFX renderer를 lifecycle runtime 뒤로 숨긴다.

```text
procedural LineRenderer prototype
→ Particle prefab
→ VFX Graph
→ pooled production VFX
```

---

# Pass 1 — Presentation VFX Lifecycle Contract · USER VERIFIED

새 파일:

```text
unity/Assets/Scripts/Core/PresentationVfxLifecycle.cs
unity/Assets/Scripts/Player/PrototypePresentationVfxRuntime.cs
```

핵심 타입:

```text
PresentationVfxSpawnRequest
PresentationVfxTimePolicy
PresentationVfxStopMode
IPresentationVfxInstance
PresentationVfxHandle
IPresentationVfxRuntime
PresentationVfxRuntime
```

공통 spawn/lifecycle 정보:

```text
World anchor 또는 Follow Transform
Follow local offset
Primary / Secondary presentation color
Start / End radius
Duration
Spin speed
Scaled / Unscaled time policy
Natural lifetime
FadeOut stop
Immediate stop
```

caller는 더 이상 `GameObject`, `LineRenderer`, `Light`, `Destroy timing`을 알지 않는다.

현재 경로:

```text
TechniquePresentationRequest
→ PresentationVfxSpawnRequest
→ PresentationVfxRuntime
→ PrototypePresentationVfxRuntime
→ PrototypeSignatureSpatialVfx
```

`PrototypeSignatureSpatialVfx`는 `IPresentationVfxInstance`를 구현하며 기존 Gate 3/4C 시각값을 유지한다.

---

# 사용자 검증

2026-08-23 사용자가 다음 범위를 확인했다.

```text
허식 자 release burst + canonical purple orb
무량공처 blue follow aura / active burst
복마어주자 red follow aura / active burst
영역 밖 푸가 release / impact
영역 안 강화 푸가
VFX 자연 종료 / 잔류 없음
F2 scene reload 후 VFX
Camera / HitStop / Gameplay 회귀
```

사용자 결과:

```text
정상!!
```

따라서:

```text
Gate 4D Pass 1: USER VERIFIED
Gate 4D VFX Lifecycle Contract: USER VERIFIED
```

---

# Pass 2를 지금 만들지 않는 이유

현재 Beauty Corner의 signature spatial VFX는 짧은 burst/follow aura 중심이고, 명시적으로 장시간 소유하며 Stop/Fade 해야 하는 production effect가 아직 없다.

이미 Handle에 `FadeOut / Immediate` 경계를 확보했으므로 실제 persistent domain aura, buff, summon, beam, channel effect가 들어왔을 때 구체 요구를 보고 확장하는 편이 안전하다.

즉 필요하지 않은 추상화를 미리 늘리지 않고 Gate 4E로 이동한다.

---

# 바꾸지 않은 것

```text
TechniquePresentationRequest 의미/발행 시점
Camera / HitStop 연출
Spatial VFX 색상/크기/지속시간
Hollow Purple canonical orb runtime override
Damage
CE
Cooldown
Domain
Tag / KO / Target / Victory rules
```

다음:

```text
Gate 4E Animation Contract
```
