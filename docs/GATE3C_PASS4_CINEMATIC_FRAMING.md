# Gate 3C Pass 4 — Cinematic Framing

작성 기준일: 2026-08-22

## 상태

```text
Gate 3C Pass 3A · Hollow Purple Form Correction: USER VERIFIED
Gate 3C Pass 4 · Cinematic Framing: REMOTE IMPLEMENTED / USER TEST PENDING
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

## 새 Camera API

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

## 사용자 빠른 테스트

```text
1. git pull origin master
2. Unity Console 빨간 오류 확인

3. 고죠 허식 「자」
→ 발사 순간 카메라가 아주 짧게 전방 구체 방향을 같이 보는지
→ 이후 자연스럽게 플레이어 중심으로 돌아오는지

4. 스쿠나 푸가
→ 발사 때 전방으로 시선이 조금 이동
→ 폭발 순간 실제 폭발 위치를 짧게 강조

5. 무량공처 / 복마어주자
→ 영역 개방 때 카메라 시선이 약간 위/공간 VFX 쪽으로 들리는지

6. 일반 이동/기본공격
→ 카메라가 평소처럼 안정적인지
```

성공 기준:

```text
Signature가 터지는 위치가 더 잘 읽힘
카메라가 멋대로 휙 돌아가 불편하지 않음
종료 후 자동으로 기존 플레이어 중심 시점 복귀
기존 전투 흐름 회귀 없음
```
