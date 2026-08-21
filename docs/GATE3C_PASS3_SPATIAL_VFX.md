# Gate 3C Pass 3 — Spatial Signature VFX

작성 기준일: 2026-08-22

## 상태

```text
Gate 3C Pass 1 · Signature Activation Feedback: USER VERIFIED
Gate 3C Pass 2A · Anticipation + Release/Impact Timing: USER FEEDBACK · TOO SUBTLE
Gate 3C Pass 2B · Readability Tuning + FOV Kick: USER VERIFIED
Gate 3C Pass 3 · Spatial Signature VFX: USER VERIFIED
Gate 3C Pass 3A · Hollow Purple Form Correction: USER VERIFIED
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## Pass 3 — Spatial Signature VFX

화면 전체 Flash/FOV만으로 끝내지 않고 술식이 실제 월드 공간을 점유하는 느낌을 추가했다.

주요 파일:

```text
unity/Assets/Scripts/Player/PrototypeSignatureSpatialVfx.cs
unity/Assets/Scripts/Player/PrototypeSignatureSpatialVfxController.cs
```

현재 공간 VFX:

```text
무량공처 준비
→ 고죠 주변 청색 이중 에너지 링 확장

무량공처 Active
→ 캐릭터 중심에서 큰 청색 개방 Ring

복마어주자 Casting
→ 스쿠나 주변 적색 이중 에너지 링 확장

복마어주자 Active
→ DomainCenter 중심에서 큰 적색 개방 Ring

푸가 Release
→ Projectile 생성 위치에서 주황/적색 Burst

푸가 Explosion
→ 실제 폭발 위치에서 더 큰 Ring Burst
→ 복마어주자 내부 증폭 푸가는 더 크게 표시
```

Ring과 Light는 `Time.unscaledTime` 기준으로 움직여 짧은 Hit Stop 중에도 연출이 읽힌다.

2026-08-22 사용자 실제 Unity 확인으로 위 공간 VFX 적용이 확인되어 Pass 3는 USER VERIFIED다.

---

# Pass 3A — Hollow Purple Form Correction

## 원작 형태 문제

기존 `GojoTechniqueChainController`의 Prototype Visual은:

```text
PurpleOuterBeam
PurpleCoreBeam
```

두 LineRenderer가 전방 전체 길이를 즉시 채워 허식 「자」가 레이저포처럼 보였다.

사용자 기준:

```text
아오(창) + 아카(혁)
→ 두 힘이 합쳐짐
→ 하나의 거대한 보라색 구체
→ 그 구체를 전방으로 발사
```

이 형태가 맞아야 한다.

## 첫 두 시도 실패

초기에는 별도 Visual Component를 Fighter Shell에 붙여 기존 Beam을 끄고 구체 Visual을 덮어쓰는 방식을 사용했다.

하지만 실제 Unity에서는 기존 Beam이 계속 살아 있었고, 한 차례 Bootstrap 타입 참조 컴파일 오류도 발생했다.

따라서 이 부착 기반 override 구조는 폐기했다.

삭제한 파일:

```text
PrototypeHollowPurpleOrbVisual.cs
PrototypeHollowPurpleOrbBootstrap.cs
각 .meta
```

## 최종 Prototype 해결

현재 파일:

```text
unity/Assets/Scripts/Player/PrototypeHollowPurplePresentationRuntime.cs
```

이 Runtime은 Play 시작 시 독립 runner로 생성된다.

약 0.08초 간격으로 `GojoTechniqueChainController`를 찾고, 각 컨트롤러의:

```text
HollowPurplePrototypeVisual
```

을 추적한다.

해당 legacy root 내부의 모든 `LineRenderer`와 `Light`는 지속적으로 비활성화하므로 기존 Purple Beam이 다시 켜져도 화면에는 나오지 않는다.

legacy root가 발동되는 순간 별도의 독립 Visual Sequence를 생성한다.

현재 흐름:

```text
청색 구체 + 적색 구체
→ 약 0.24초 동안 중앙으로 수렴
→ 큰 보라색 구체 하나 생성
→ 약 18m 전방 이동
→ 구체 주변 회전 Energy Ring + Point Light
```

레이저처럼 보일 가능성을 줄이기 위해 긴 Trail은 사용하지 않는다.

## 현재 Gameplay 판정

이번 Pass의 목적은 Presentation 형태 교정이다.

따라서 Damage / CE / Cooldown / 준비 조건은 변경하지 않았다.

현재 피해 판정은 여전히 기존 `GojoTechniqueChainController.ApplyPurpleDamage()`의 즉시 Capsule 판정이다.

```text
시각 표현
→ 이동하는 거대 보라 구체

현재 Prototype Damage timing
→ 즉시 Capsule 판정
```

최종 제작 단계에서는 실제 Projectile/Volume 이동과 피해 시점을 일치시키는 것이 목표다.

## 사용자 검증

2026-08-22 실제 Unity 테스트 과정:

```text
1차: 기존 레이저 그대로 → 실패
2차: Bootstrap compile error → 실패
3차: 여전히 Beam 형태 → 실패
최종 Runtime 방식 → 구체형으로 정상 표시
```

사용자 판정:

```text
그래 이제 좀 정상적이구나
```

따라서:

```text
Gate 3C Pass 3A · Hollow Purple Form Correction: USER VERIFIED
```

로 닫는다.
