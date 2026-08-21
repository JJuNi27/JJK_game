# Gate 3C — Signature Technique Presentation

작성 기준일: 2026-08-22

## 상태

```text
Gate 3A Combat Feel: USER VERIFIED
Gate 3B Prototype Presentation: USER VERIFIED
Gate 3C Signature Technique Presentation: STARTED
Gate 3C Pass 1 · Signature Activation Feedback: USER VERIFIED
Gate 3C Pass 2A · Anticipation + Release/Impact Timing: USER FEEDBACK · TOO SUBTLE
Gate 3C Pass 2B · Readability Tuning + FOV Kick: REMOTE IMPLEMENTED / USER TEST PENDING
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

현재 실제 캐릭터 애니메이션/최종 VFX 자산이 없으므로 기존 Prototype 연출 위에 Presentation timing을 단계적으로 얹는다.

---

# Pass 1 — Signature Activation Feedback

파일:

```text
unity/Assets/Scripts/Player/PrototypeSignatureTechniqueFeedbackController.cs
```

감지:

```text
허식 자 → HollowPurplePrototypeVisual 활성
푸가 → SukunaFugaProjectile 생성
무량공처 → GojoDomainController.State == Active
복마어주자 → SukunaDomainController.IsActive
```

기존 `SimpleCameraFollow.Flash`, `AddShake`, `PrototypeHitStopController.Request`를 재사용했다.

2026-08-22 사용자 실제 Unity 확인:

```text
[USER VERIFIED]
- 허식 자 Activation Feedback 정상
- 무량공처 Activation Feedback 정상
- 푸가 Activation Feedback 정상
- 복마어주자 Activation Feedback 정상
- 기존 대표 술식 동작에 눈에 띄는 회귀 없음
```

따라서:

```text
Gate 3C Pass 1 · Signature Activation Feedback: USER VERIFIED
```

---

# Pass 2A — Anticipation + Release/Impact Timing

## 목적

대표 기술을:

```text
준비 / 긴장
→ Release
→ Culmination / Impact
```

리듬으로 읽히게 하는 첫 시도다.

구현:

```text
무량공처
- DomainReady 진입 때 약한 청색 Cue
- Active 때 개방 Cue

복마어주자
- Casting 시작 때 약한 적색 Cue
- Active 때 개방 Cue

푸가
- 유효 R 입력 때 Anticipation
- Projectile 생성 때 Release
- Projectile Explode 때 Impact

허식 자
- Release
- 약 0.09초 뒤 Culmination Pulse
```

`SukunaFugaProjectile`에는 Presentation 전용 정적 이벤트:

```text
Exploded(Health owner, Vector3 worldPosition, bool domainAmplified)
```

를 추가했다.

Damage / Domain Fuga Blast / Explosion Ring 처리 뒤에 Presentation 이벤트가 호출되므로 핵심 전투 판정보다 뒤에서 연출이 연결된다.

## 사용자 피드백

2026-08-22 사용자 실제 Unity 테스트:

```text
[USER FEEDBACK]
- 무량공처 V 입력 때 약한 청색 Cue를 알아보기 어려움
- 복마어주자 Casting의 약한 적색 Cue를 알아보기 어려움
- 전체적으로 Pass 2에서 무엇이 달라졌는지 체감하기 어려움
```

즉 기능 고장으로 판정하지는 않지만 `준비 → Release → Impact` 구분이 눈에 띄지 않아 Pass 2A는 완료 처리하지 않는다.

---

# Pass 2B — Readability Tuning + FOV Kick

## 목적

Pass 2A의 문제는 Feedback이 기존 VFX에 묻힐 정도로 약했다는 것이다.

단순히 Flash alpha만 무작정 키우지 않고, 카메라 렌즈 변화까지 추가해 각 단계의 실루엣을 명확하게 만든다.

## 변경 파일

```text
unity/Assets/Scripts/Camera/SimpleCameraFollow.cs
unity/Assets/Scripts/Player/PrototypeSignatureTechniqueFeedbackController.cs
```

## FOV Kick API

`SimpleCameraFollow`에:

```text
AddFovKick(delta, duration)
```

를 추가했다.

`Time.unscaledTime` 기준의 짧은 FOV pulse이며 종료 시 기존 FOV로 복구된다.

규칙:

```text
Anticipation
→ 음수 FOV delta
→ 화면이 순간적으로 좁아져 집중/긴장감

Release / Domain Active / Impact
→ 양수 FOV delta
→ 순간적으로 화면이 벌어지며 힘이 터지는 느낌
```

이 값은 GAME_ORIGINAL Beauty Corner placeholder다.

## 강화된 구분

```text
무량공처 V 준비
- 청색 Flash를 기존보다 눈에 띄게 강화
- 약 -4.5 FOV 집중 Cue

복마어주자 Casting 시작
- 적색 Flash 강화
- 약 -5.5 FOV 집중 Cue

허식 자 Release
- 기존 보라 Feedback 강화
- 약 +7 FOV Release Kick
- Culmination에도 작은 +3 FOV Pulse

푸가
- R Anticipation: -4 ~ -5 FOV
- Projectile Release: +5 ~ +6 FOV
- Explosion Impact: +9, Domain Amplified는 +11 FOV

무량공처 Active
- +6 FOV 개방 Kick

복마어주자 Active
- +8 FOV 개방 Kick
```

Flash/Shake 강도도 Pass 2A보다 한 단계 올렸다.

## 바꾸지 않은 것

```text
Damage
CE Cost
Cooldown
Range
Fuga Cast Time
Domain Cast Time
Domain Duration
Sure Hit
푸가 준비 조건
허식 자 준비 조건
Target Lock
Tag
```

---

## 다음 사용자 테스트

이번에는 모든 기술을 길게 볼 필요 없이 차이가 가장 쉽게 보이는 두 개부터 확인한다.

```text
1. git pull origin master
2. Console 빨간 오류 확인

3. 고죠에서 V 한 번
→ 영역 준비가 시작되는 순간 화면이 살짝 좁아지고 청색 Cue가 분명히 보여야 함

4. 스쿠나에서 V 한 번
→ Casting 시작 순간 화면이 더 강하게 좁아지고 적색 Cue가 보여야 함

5. 가능하면 푸가
→ R 준비: 화면 좁아짐
→ 발사: 화면 벌어짐
→ 폭발: 가장 큰 화면 벌어짐 + 충격
```

이제도 차이가 거의 안 보인다면 Flash/Shake 수치 문제가 아니라 현재 Prototype VFX/UI에 묻히는 Presentation 방식 자체의 문제로 보고, 다음 단계에서는 화면 전체 Tint가 아닌 공간 VFX/캐릭터 주변 Cast VFX로 전환한다.
