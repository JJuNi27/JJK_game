# Gate 3C Pass 3 — Spatial Signature VFX

작성 기준일: 2026-08-22

## 상태

```text
Gate 3C Pass 1 · Signature Activation Feedback: USER VERIFIED
Gate 3C Pass 2A · Anticipation + Release/Impact Timing: USER FEEDBACK · TOO SUBTLE
Gate 3C Pass 2B · Readability Tuning + FOV Kick: USER VERIFIED
Gate 3C Pass 3 · Spatial Signature VFX: USER VERIFIED
Gate 3C Pass 3A · Hollow Purple Form Correction: REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## Pass 2B 사용자 확인

2026-08-22 사용자 실제 Unity 테스트에서 FOV Kick과 강화된 화면 피드백에 대해:

```text
오 확실히 표가 난다 화면 이펙트
```

라고 확인했다.

따라서 Pass 2B는 USER VERIFIED로 기록한다.

## Pass 3 목적

화면 전체 Flash/FOV만으로 끝내지 않고 술식이 실제 월드 공간을 점유하는 느낌을 추가한다.

파일:

```text
unity/Assets/Scripts/Player/PrototypeSignatureSpatialVfx.cs
unity/Assets/Scripts/Player/PrototypeSignatureSpatialVfxController.cs
```

둘 다 최종 자산이 아닌 Beauty Corner placeholder다.

## 현재 공간 VFX

```text
무량공처 준비
→ 고죠 주변 청색 이중 에너지 링 확장

복마어주자 Casting
→ 스쿠나 주변 적색 이중 에너지 링 확장

푸가 Release
→ Projectile 생성 위치에서 주황/적색 Burst

푸가 Explosion
→ 실제 폭발 위치에서 더 큰 공간 Ring Burst
→ 복마어주자 내부 증폭 푸가는 더 크게 표시

무량공처 Active
→ 캐릭터 중심에서 큰 청색 개방 Ring

복마어주자 Active
→ DomainCenter 중심에서 큰 적색 개방 Ring
```

Ring과 Light는 `Time.unscaledTime` 기준으로 움직여 짧은 Hit Stop 중에도 연출이 읽히도록 했다.

## Pass 3 사용자 확인

2026-08-22 사용자 실제 Unity 확인:

```text
[USER VERIFIED]
- 무량공처 준비/개방 공간 VFX 적용 확인
- 복마어주자 Casting/개방 공간 VFX 적용 확인
- 푸가 발사/폭발 공간 VFX 적용 확인
- 앞서 안내한 Pass 3 변경사항이 실제 화면에 적용됨
```

단, 허식 「자」의 기존 자체 Visual이 레이저/빔처럼 보이는 원작 재현 문제를 사용자가 지적했다.

따라서 공간 VFX 시스템 자체는 USER VERIFIED로 닫되, 허식 「자」의 형태는 별도 교정한다.

---

# Pass 3A — Hollow Purple Form Correction

## 사용자 원작 기준 피드백

사용자 지적:

```text
허식 「자」는 레이저포가 아니다.
아오(창)와 아카(혁)가 합쳐져 하나의 거대한 보라색 구체가 되고,
그 구체를 전방으로 발사하는 형태여야 한다.
```

기존 `GojoTechniqueChainController`의 Prototype Visual은:

```text
PurpleOuterBeam
PurpleCoreBeam
```

두 LineRenderer가 전방 전체 길이를 즉시 그려 레이저처럼 보였다.

## 교정 구현

새 파일:

```text
unity/Assets/Scripts/Player/PrototypeHollowPurpleOrbVisual.cs
```

씬 세팅 없이 Fighter Shell에 자동 부착된다.

기존 `HollowPurplePrototypeVisual`의 Beam LineRenderer와 중간 고정 Light를 Presentation 단계에서 비활성화하고 새 Visual로 덮어쓴다.

새 흐름:

```text
아오 계열 청색 구체 + 아카 계열 적색 구체
→ 좌우에서 중앙으로 약 0.18초 수렴
→ 두 구체가 사라지며 거대한 보라색 구체 생성
→ 보라 구체가 전방으로 약 18m 이동
→ 짧은 보라 Trail + 구체 주변 회전 Energy Ring + Point Light
```

핵심은 전방 전체를 한 번에 채우는 Beam을 없애고 `이동하는 단일 거대 구체`의 실루엣으로 바꾼 것이다.

현재 구체 이동은 `Time.unscaledTime` 기반 Presentation이다.

## 아직 유지하는 Prototype 판정

이번 Pass는 형태 교정이 목적이므로 Gate 1에서 검증한 기존 전투 판정은 변경하지 않았다.

현재 Damage는 여전히 `GojoTechniqueChainController.ApplyPurpleDamage()`의 즉시 Capsule 판정을 사용한다.

즉:

```text
시각 표현
→ 아오 + 아카 합체 후 거대 보라 구체 이동

현재 Prototype Damage timing
→ 기존 즉시 Capsule 판정 유지
```

최종 제작 단계에서는 실제 Purple Projectile/Volume의 이동과 Damage timing을 일치시키는 것이 목표다.

## 바꾸지 않은 것

```text
Damage
CE Cost
Cooldown
Range
허식 자 준비 조건
Blue / Red 연계 규칙
Tag
Target Lock
```

## 빠른 사용자 테스트

```text
1. git pull origin master
2. Unity Console 빨간 오류 확인
3. 고죠로 창 + 혁 준비 후 R

확인 포인트
- 기존 긴 보라 레이저가 더 이상 보이지 않는지
- 청색/적색 작은 구체가 좌우에서 합쳐지는지
- 합쳐진 뒤 큰 보라색 구체 하나가 전방으로 날아가는지
- 보라 구체가 캐릭터보다 충분히 큰 기술로 읽히는지
```

정상 확인되면:

```text
Gate 3C Pass 3A · Hollow Purple Form Correction: USER VERIFIED
```

로 닫는다.
