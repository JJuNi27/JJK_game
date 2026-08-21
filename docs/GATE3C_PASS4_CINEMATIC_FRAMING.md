# Gate 3C Pass 4 — Cinematic Framing

작성 기준일: 2026-08-22

## 상태

```text
Gate 3C Pass 3A · Hollow Purple Form Correction: USER VERIFIED
Gate 3C Pass 4 · Cinematic Framing: USER VERIFIED
Gate 3C Signature Technique Presentation: USER VERIFIED
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 목적

Gate 3C Pass 2~3에서 Flash / Shake / FOV / Spatial VFX를 넣었지만 카메라의 시선 중심은 계속 플레이어에게 고정되어 있었다.

이번 Pass에서는 대표 술식의 `힘이 터지는 위치`를 카메라가 아주 짧게 따라 보도록 한다.

완전한 컷신 카메라나 강제 카메라 전환은 만들지 않는다.

기존 플레이 감각을 유지하면서 짧은 Look Focus만 추가한다.

## 변경 파일

```text
unity/Assets/Scripts/Camera/SimpleCameraFollow.cs
unity/Assets/Scripts/Player/PrototypeSignatureTechniqueFeedbackController.cs
```

## Camera API

`SimpleCameraFollow`에:

```text
AddWorldFocus(Vector3 worldPoint, float strength, float duration)
```

를 추가했다.

작동 방식:

```text
평소
→ 기존 player look point

Signature Focus가 들어오면
→ 짧은 시간 동안 player look point와 world focus point 사이를 blend
→ sin envelope로 자연스럽게 들어왔다가 빠짐
→ 종료 후 자동으로 player 중심으로 복귀
```

타이밍은 `Time.unscaledTime`을 사용하므로 Hit Stop 중에도 framing cue가 읽힌다.

최대 focus strength를 제한해서 플레이어 카메라를 완전히 빼앗지 않는다.

## 적용

```text
무량공처 준비
→ 고죠 상체/위쪽으로 약한 focus

무량공처 개방
→ 위쪽 공간 VFX를 읽도록 조금 더 높은 focus

복마어주자 Casting
→ 스쿠나 위쪽으로 약한 focus

복마어주자 개방
→ DomainCenter 위 공간을 짧게 focus

허식 「자」 Release
→ 고죠 전방 약 7.5m 지점을 focus
→ 구체가 전방으로 나가는 방향을 카메라가 읽게 함

푸가 Anticipation
→ 전방 시전 방향으로 약한 focus

푸가 Projectile Release
→ 투사체 앞쪽으로 focus

푸가 Explosion
→ 실제 Exploded worldPosition으로 가장 강한 focus
→ 복마어주자 내부 증폭 푸가는 조금 더 강함
```

## 바꾸지 않은 것

```text
Damage
CE
Cooldown
Range
Cast Time
Domain Duration
Sure Hit
Target Lock
Tag
이동/회피
기존 카메라 위치 offset
```

즉 카메라 위치 자체를 순간이동시키는 것이 아니라 `보는 방향`만 짧게 보조한다.

## 사용자 검증

2026-08-22 사용자 실제 Unity 테스트에서 카메라 시선 보조를 확인한 뒤:

```text
잘 작동하는 듯?
```

이라고 판정했다.

직전 확인 과정에서 사용자는 이번 변경이 `카메라 위치 이동`이 아니라 `기술이 터지는 방향/지점을 짧게 보는 시선 변화`라는 점을 확인했고, 실제 동작도 정상으로 보인다고 응답했다.

따라서:

```text
Gate 3C Pass 4 · Cinematic Framing: USER VERIFIED
Gate 3C Signature Technique Presentation: USER VERIFIED
```

로 닫는다.

최종 상용 품질 VFX / 전용 애니메이션 / 실제 음성 자산 교체는 이후 Production 단계에서 계속 진행한다.
