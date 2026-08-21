# Gate 3C — Signature Technique Presentation

작성 기준일: 2026-08-22

## 상태

```text
Gate 3A Combat Feel: USER VERIFIED
Gate 3B Prototype Presentation: USER VERIFIED
Gate 3C Signature Technique Presentation: STARTED
Gate 3C Pass 1 · Signature Activation Feedback: REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 목적

대표 술식 자체의 규칙은 이미 Gate 1에서 USER VERIFIED다.

Gate 3C에서는 규칙을 다시 만들지 않고, 사용 순간의 시각/카메라/타격감이 `일반 기술과 확실히 다르게 읽히는가`를 검증한다.

대표 범위:

```text
GOJO
- 허식 자
- 무량공처

SUKUNA
- 푸가
- 복마어주자
```

현재 실제 캐릭터 애니메이션/최종 VFX 자산이 없으므로 Pass 1은 기존 Prototype 연출 위에 Activation Feedback만 얹는다.

---

# Pass 1 — Signature Activation Feedback

## 새 파일

```text
unity/Assets/Scripts/Player/PrototypeSignatureTechniqueFeedbackController.cs
```

`BasicAttack`이 있는 Fighter Shell에 런타임 자동 부착된다.

기존 술식 로직에는 직접 손대지 않는다.

## 감지 방식

### 허식 자

기존 `GojoTechniqueChainController`가 생성하는:

```text
HollowPurplePrototypeVisual
```

의 비활성 → 활성 전환을 감지한다.

### 푸가

새 `SukunaFugaProjectile` 인스턴스가 씬에 등장하는 순간을 감지한다.

```text
일반 푸가
SukunaFugaProjectile

복마어주자 내부 증폭 푸가
SukunaDomainFugaProjectile
```

둘은 같은 Projectile Component를 사용하지만 GameObject 이름으로 증폭 버전을 구분해 피드백 강도를 조금 다르게 한다.

### 무량공처

`GojoDomainController.State == Active`로 처음 전환되는 프레임을 감지한다.

### 복마어주자

`SukunaDomainController.IsActive`가 처음 true가 되는 프레임을 감지한다.

## 피드백

모두 기존:

```text
SimpleCameraFollow.Flash
SimpleCameraFollow.AddShake
PrototypeHitStopController.Request
```

를 재사용한다.

새 거대 Presentation 시스템은 만들지 않는다.

현재 방향:

```text
허식 자
- 보라 계열 Flash
- 가장 강한 축의 Release Shake
- 약 0.065초 Hit Stop

푸가
- 주황/적색 Flash
- 중강도 Release Shake
- 약 0.035초 Hit Stop

복마어주자 내부 푸가
- 일반 푸가보다 조금 강한 적색 Feedback

무량공처
- 청색/보라 계열 Flash
- 영역 개방 순간 짧은 Shake
- 약 0.045초 Hit Stop

복마어주자
- 적색 Flash
- 무량공처보다 거친 Shake
- 약 0.055초 Hit Stop
```

수치는 모두 `GAME_ORIGINAL` Beauty Corner placeholder이며 최종값이 아니다.

## 바꾸지 않은 것

```text
술식 Damage
CE Cost
Cooldown
Range
Cast Time
영역 Duration
영역 Sure Hit
푸가 준비 조건
허식 자 준비 조건
Target Lock
Tag
```

## 사용자 빠른 테스트

자투리 시간용으로 이번 테스트는 짧게 한다.

```text
1. git pull origin master
2. Console 빨간 오류 없는지 확인

3. 고죠 허식 자 발동
→ 발사 순간 보라 Flash + 큰 Shake + 순간적인 무게감

4. 고죠 무량공처 성공
→ 영역이 Active 되는 순간 청색 계열 Flash + 짧은 충격

5. T → 스쿠나
6. 해 + 팔 사용 후 푸가
→ 실제 투사체가 나가는 순간 주황/적색 Flash + Shake

7. 복마어주자
→ Active 되는 순간 적색 Flash + 더 거친 충격
```

기존 술식 자체가 정상 작동하면서 네 가지 Signature가 일반 공격보다 확실히 `큰 기술을 썼다`고 읽히면 Pass 1 성공이다.

## 다음

Pass 1이 USER VERIFIED 되면 다음 묶음에서:

```text
- Signature Cast Anticipation
- Release/Impact를 분리한 Camera Timing
- 기술별 전용 VFX/SFX hook 위치 정리
```

를 다룬다.

실제 모델/Animator 자산이 준비되면 placeholder 자세 대신 전용 시전 애니메이션과 Animation Event를 연결한다.
