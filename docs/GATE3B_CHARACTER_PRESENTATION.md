# Gate 3B — Representative Character Presentation

작성 기준일: 2026-08-21

## 상태

```text
Gate 3A Combat Feel: USER VERIFIED
Gate 3B Character Presentation Pass 1: USER VERIFIED
Gate 3B Character Presentation Pass 2 Bundle: USER VERIFIED
Gate 3B Prototype Presentation: USER VERIFIED

NEXT: Gate 3C Signature Technique Presentation
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## Gate 3A 종료 근거

2026-08-21 사용자 실제 Unity 확인으로 다음을 통과했다.

```text
기본 3타 Camera Shake
Hit Stop
Hit Flash
1타 < 2타 < 3타 FINISH 무게 차이
실제 피해 위치 Impact VFX
허공 공격에는 적중 피드백 없음
```

Pass 3 첫 구현에서 `DamageContext.ImpactPoint`를 잘못 참조해 CS1061 컴파일 오류가 발생했다.

실제 DamageContext 프로퍼티는 `HitPoint`이므로 `context.HitPoint`로 수정했고 이후 사용자 확인을 통과했다.

따라서 Gate 3A Combat Feel은 USER VERIFIED로 종료한다.

---

## Gate 3B 현재 자산 상황

현재 저장소에는 실제 고죠/스쿠나 3D 모델/리깅 애니메이션 자산이 아직 없다.

현재 화면 캐릭터는:

```text
GojoPrototypeAvatar
SukunaPrototypeAvatar
```

가 Primitive 기반으로 런타임 외형을 만든다.

따라서 실제 모델을 기다리며 멈추지 않고, 나중에 Animator/모델로 교체할 수 있는 표현 규칙을 먼저 검증했다.

현재 동작들은 최종 품질이 아니라 상태/타이밍 검증용 placeholder다.

최종 목표는 실제 리깅 모델 + Animator + 캐릭터별 전용 애니메이션이며, 현재 Primitive 팔/몸 회전 방식은 실제 제작 단계에서 교체한다.

최종 Character Asset Contract 추출은 Gate 4에서 한다. 지금 거대한 범용 애니메이션 시스템은 만들지 않는다.

---

# Pass 1 — Basic Attack Pose + Tag Entry Readability

## 구현

파일:

```text
unity/Assets/Scripts/Player/PrototypeFighterPresentationController.cs
```

씬 세팅 없이 `RuntimeInitializeOnLoadMethod(AfterSceneLoad)`로 BasicAttack이 있는 Fighter Shell에 자동 부착한다.

`DefaultExecutionOrder(1500)`으로 기존 Prototype Avatar의 이동 흔들림 계산 뒤에 공격 포즈를 덧씌운다.

기본 공격:

```text
1타 → 한쪽 팔 전진
2타 → 반대쪽 팔 전진
3타 FINISH → 양팔 + 상체 전진 강조
```

고죠와 스쿠나는 같은 기본 3타 시스템을 사용하지만 포즈 수치를 다르게 했다.

```text
GOJO
- 비교적 정돈된 회전
- FINISH 상체 전진 절제

SUKUNA
- 더 큰 팔 각도
- 더 공격적인 상체 전진/회전
```

Tag 시 새 Active Visual Root에 약 0.22초 Scale Pulse를 준다.

## 사용자 검증

2026-08-21 사용자 실제 Unity 확인:

```text
[USER VERIFIED]
- 고죠 기본 1/2/3타 포즈 동작 정상
- 스쿠나 기본 1/2/3타 포즈 동작 정상
- 3타 FINISH가 더 큰 포즈로 보임
- Tag Entry Pulse 동작 정상
- 기존 공격/적중 피드백 흐름 정상
```

사용자 체감상 Primitive 포즈가 매우 단순하고 코믹하게 보였지만 이는 현재 placeholder 품질 한계이며, 동작 규칙 검증 자체는 정상 통과했다.

따라서:

```text
Gate 3B Character Presentation Pass 1: USER VERIFIED
```

---

# Pass 2 Bundle — Locomotion Stance + Dodge Readability

## 목적

실제 Animator가 없는 상태에서도 `가만히 있음 / 이동 / 회피 / 공격`이 실루엣만으로 구분되도록 한다.

이번 작업도 최종 애니메이션이 아니라 Character Presentation 규칙 검증용 placeholder다.

## 변경 파일

```text
unity/Assets/Scripts/Player/ThirdPersonPlayerController.cs
unity/Assets/Scripts/Player/PrototypeFighterPresentationController.cs
```

## ThirdPersonPlayerController

Presentation이 회피 상태를 추측하지 않도록 기존 이동 시스템에서 읽기 전용 정보를 공개한다.

```text
DodgeProgress
DodgeDirection
```

회피 판정/속도/무적/쿨타임 수치는 변경하지 않았다.

## Locomotion Stance

CharacterController의 실제 평면 속도를 읽어 Visual Root에 아주 약한 전진 기울기/호흡 회전을 덧씌운다.

```text
GOJO
- 비교적 직립
- 이동 시 작은 전진 기울기

SUKUNA
- 기본적으로 조금 더 앞으로 숙임
- 이동 시 공격적인 전진 기울기 증가
```

기존 Prototype Avatar의 팔/다리 걷기 흔들림은 그대로 사용한다.

## Dodge Presentation

`ThirdPersonPlayerController.IsDodging` 동안:

```text
Visual Root 전방 기울기
팔을 뒤로 빼는 Dash 자세
아주 약한 세로 Stretch
```

를 적용한다.

스쿠나는 고죠보다 각도와 Stretch를 조금 더 크게 둔다.

회피 자체의 실제 이동/무적 프레임은 기존 코드 그대로다.

## 우선순위

같은 프레임에 여러 Presentation 상태가 겹칠 경우:

```text
Attack Pose
> Dodge Pose
> Locomotion Stance
```

순서로 표현한다.

Tag Entry Scale Pulse는 별도로 적용되며 이후 상태가 필요한 경우 Scale을 덮어쓸 수 있다. 이는 현재 Prototype 한정이며 실제 Animator 파이프라인에서 다시 설계한다.

## 바꾸지 않은 것

```text
이동 속도
회전 속도
회피 속도
회피 거리
회피 쿨타임
회피 무적
Damage
Hit Stop
Hit VFX
HP / CE
Tag 규칙
Target Lock
술식/영역
```

## 사용자 검증

2026-08-21 사용자 실제 Unity 확인:

```text
[USER VERIFIED]
- 고죠 이동 중 전진 기울기 정상
- 스쿠나 이동 중 더 공격적인 자세 정상
- 고죠 SPACE 회피 Dash Pose 정상
- 스쿠나 SPACE 회피의 더 과격한 자세 정상
- 정지 / 이동 / 회피 / 공격 상태 구분 정상
- 기존 전투 흐름에 눈에 띄는 이상 없음
```

사용자 판정: `이동 모션들은 정상`.

따라서:

```text
Gate 3B Character Presentation Pass 2 Bundle: USER VERIFIED
Gate 3B Prototype Presentation: USER VERIFIED
```

---

## 다음 시작점

다음 작업은:

```text
Gate 3C Signature Technique Presentation
```

대표 범위:

```text
GOJO
- 허식 자
- 무량공처

SUKUNA
- 푸가
- 복마어주자
```

현재 Prototype 동작 품질을 더 다듬는 데 시간을 쓰지 않고, 실제 모델/Animator 자산이 준비되면 공격/이동/회피/Tag placeholder를 고급 애니메이션으로 교체한다.

Gate 3C부터는 VFX/SFX/카메라/캐릭터 Presentation 연결처럼 여러 파일을 함께 다루는 작업이 늘어나므로 Codex 사용 비중을 높인다.
