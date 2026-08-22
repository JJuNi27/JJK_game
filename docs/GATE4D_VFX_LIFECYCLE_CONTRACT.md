# Gate 4D — VFX Lifecycle Contract

작성 기준일: 2026-08-23

## 상태

```text
Gate 4A Character Presentation Contract: USER VERIFIED
Gate 4B HUD Data Binding Extraction: USER VERIFIED
Gate 4C Technique Presentation Request: USER VERIFIED
Gate 4D Pass 1 · Runtime/Handle/Lifecycle Boundary: REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 목적

Gate 4C에서 Spatial VFX가 `TechniquePresentationRequest`를 소비하도록 만들었지만, 소비자는 여전히 구체 구현인 `PrototypeSignatureSpatialVfx`를 직접 호출하고 있었다.

즉 다음 교체 시점에 결합이 남아 있었다.

```text
procedural LineRenderer prototype
→ Particle prefab
→ VFX Graph
→ pooled production VFX
```

Gate 4D는 이 구체 렌더링 구현을 lifecycle runtime 뒤로 숨긴다.

---

# Pass 1 — Presentation VFX Lifecycle Contract

새 파일:

```text
unity/Assets/Scripts/Core/PresentationVfxLifecycle.cs
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

## Spawn Request

현재 공통 spawn 정보:

```text
World anchor 또는 Follow Transform
Follow local offset
Primary / Secondary presentation color
Start / End radius
Duration
Spin speed
Scaled / Unscaled time policy
```

이 정보는 현재 prototype ring renderer가 필요한 최소 공통 수치다.

중요하게 caller는 더 이상:

```text
GameObject 생성
LineRenderer 생성
Light 생성
Destroy timing
```

을 알지 않는다.

## Handle / Lifecycle

`PresentationVfxHandle`은 생성된 VFX 인스턴스에 대해:

```text
IsValid
IsAlive
Stop(FadeOut)
Stop(Immediate)
```

경계를 제공한다.

현재 짧은 signature burst들은 자연 수명 종료를 사용하지만, 이후 long-lived aura/domain/persistent effect가 명시적으로 Stop/Fade를 요청할 수 있는 계약을 먼저 확보했다.

---

# Prototype Runtime Adapter

새 파일:

```text
unity/Assets/Scripts/Player/PrototypePresentationVfxRuntime.cs
```

경로:

```text
PresentationVfxRuntime
→ IPresentationVfxRuntime
→ PrototypePresentationVfxRuntime
→ PrototypeSignatureSpatialVfx
```

현재 procedural 구현은 adapter 뒤에 남아 있다.

나중에 실제 prefab/VFX Graph runtime을 등록하면 technique consumer를 수정하지 않고 교체할 수 있는 방향이다.

---

# PrototypeSignatureSpatialVfx Migration

`PrototypeSignatureSpatialVfx`는 이제:

```text
IPresentationVfxInstance
```

를 구현한다.

추가 lifecycle:

```text
자연 lifetime 종료
FadeOut stop
Immediate stop
Scaled / Unscaled update policy
Follow target anchor
World anchor
```

기존 Gate 3/4C의 ring/light 시각값은 유지한다.

Legacy `SpawnFollowAura` / `SpawnWorldBurst` wrapper는 당장 다른 prototype caller 회귀를 막기 위해 유지하지만 내부적으로 새 Spawn Request 경로를 사용한다.

---

# Spatial Consumer Migration

변경:

```text
unity/Assets/Scripts/Player/PrototypeSignatureSpatialVfxController.cs
```

기존:

```text
TechniquePresentationRequest
→ PrototypeSignatureSpatialVfx.Spawn...
```

현재:

```text
TechniquePresentationRequest
→ PresentationVfxSpawnRequest
→ PresentationVfxRuntime
→ registered runtime implementation
```

즉 Spatial consumer도 구체 LineRenderer prototype 타입을 알 필요가 없어졌다.

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
Tag/KO/Target/Victory rules
```

허식 자 canonical orb 자체는 별도 특수 prototype visual이므로 이번 Pass에서 lifecycle runtime으로 억지 통합하지 않았다.

---

# Pass 1 사용자 테스트

```text
1. cd D:\GitHub\JJK_game
2. git pull origin master
3. Unity compile 완료 대기
4. Console 빨간 오류 확인
5. CombatMVP Play
```

빠른 확인:

```text
[ ] 허식 자 release purple burst 정상
[ ] 허식 자 큰 보라 구체 자체도 기존 형태 유지
[ ] 무량공처 준비 blue follow aura 정상
[ ] 무량공처 개방 large blue burst 정상
[ ] T → 스쿠나 정상
[ ] 복마어주자 준비 red follow aura 정상
[ ] 복마어주자 개방 large red burst 정상
[ ] 영역 밖 푸가 release / impact burst 정상
[ ] 영역 안 강화 푸가 burst 강도 정상
[ ] VFX가 정상 수명 뒤 사라짐
[ ] 같은 VFX가 잔류하거나 무한 생성되지 않음
[ ] F2 scene reload 후 VFX 계속 정상
[ ] Camera/HitStop/Gameplay 회귀 없음
```

통과하면:

```text
Gate 4D Pass 1: USER VERIFIED
```

다음은 long-lived effect ownership/stop policy가 실제로 필요한 지점을 조사한 뒤, 필요하면 Pass 2로 확장하고 아니면 4D를 닫아 4E Animation Contract로 넘어간다.
