# Gate 3C — Signature Technique Presentation

작성 기준일: 2026-08-22

## 상태

```text
Gate 3A Combat Feel: USER VERIFIED
Gate 3B Prototype Presentation: USER VERIFIED
Gate 3C Signature Technique Presentation: STARTED
Gate 3C Pass 1 · Signature Activation Feedback: USER VERIFIED
Gate 3C Pass 2 · Anticipation + Release/Impact Timing: REMOTE IMPLEMENTED / USER TEST PENDING
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

## 구현

파일:

```text
unity/Assets/Scripts/Player/PrototypeSignatureTechniqueFeedbackController.cs
```

`BasicAttack`이 있는 Fighter Shell에 런타임 자동 부착된다.

기존 술식 로직에는 직접 손대지 않았다.

감지:

```text
허식 자
→ HollowPurplePrototypeVisual 비활성 → 활성

푸가
→ 새 SukunaFugaProjectile 등장

무량공처
→ GojoDomainController.State == Active 진입

복마어주자
→ SukunaDomainController.IsActive 진입
```

피드백:

```text
허식 자
- 보라 Flash
- 강한 Release Shake
- 짧은 Hit Stop

푸가
- 주황/적색 Flash
- 중강도 Release Shake
- 짧은 Hit Stop

복마어주자 내부 푸가
- 일반 푸가보다 조금 강한 Release Feedback

무량공처
- 청색 계열 Flash
- 영역 개방 충격

복마어주자
- 적색 Flash
- 무량공처보다 거친 개방 충격
```

수치는 모두 `GAME_ORIGINAL` Beauty Corner placeholder다.

## 사용자 검증

2026-08-22 사용자 실제 Unity 확인:

```text
[USER VERIFIED]
- 허식 자 Activation Feedback 정상
- 무량공처 Activation Feedback 정상
- 푸가 Activation Feedback 정상
- 복마어주자 Activation Feedback 정상
- 기존 대표 술식 동작에 눈에 띄는 회귀 없음
```

사용자 판정: `정상 확인`.

따라서:

```text
Gate 3C Pass 1 · Signature Activation Feedback: USER VERIFIED
```

---

# Pass 2 — Anticipation + Release/Impact Timing

## 목적

Pass 1은 `발동 순간 한 번의 큰 피드백`이었다.

Pass 2에서는 대표 기술을 다음 리듬으로 읽히게 한다.

```text
준비 / 긴장
→ Release
→ Culmination / Impact
```

아직 최종 애니메이션 자산이 없으므로 카메라/Flash 타이밍으로 이 구조만 먼저 검증한다.

## 변경 파일

```text
unity/Assets/Scripts/Player/PrototypeSignatureTechniqueFeedbackController.cs
unity/Assets/Scripts/Player/SukunaFugaProjectile.cs
```

## 1. 무량공처 Anticipation

`GojoDomainController`가:

```text
Normal
→ DomainReady
```

로 들어가는 순간 약한 청색 Flash + 작은 Camera Shake를 준다.

실제 영역이 Active 되는 순간의 강한 피드백은 Pass 1 그대로 유지된다.

따라서:

```text
V로 영역 준비
→ 약한 긴장 Cue
→ 기존 장인 입력 성공
→ Active 순간 큰 영역 개방 Cue
```

가 된다.

## 2. 복마어주자 Anticipation

`SukunaDomainController.IsCasting`이 시작되는 순간 약한 암적색 Flash + 작은 Shake를 추가했다.

약 0.65초 Cast가 끝나 Active가 되면 Pass 1의 강한 적색 개방 피드백이 발생한다.

게임의 Cast Time 자체는 바꾸지 않았다.

## 3. 푸가 Anticipation → Release → Impact

현재 푸가는 실제로 약 0.52초 Cast Time이 있으므로 세 단계 분리가 가장 명확하다.

```text
R 입력이 유효할 가능성이 높은 순간
→ 약한 주황/적색 Anticipation

Projectile 생성
→ 기존 Release Feedback

Projectile Explode
→ 더 강한 Impact Flash + Shake + 짧은 Hit Stop
```

복마어주자 내부 증폭 푸가는 Release와 Impact를 일반 푸가보다 강하게 유지한다.

### Fuga Explosion Hook

`SukunaFugaProjectile`에 Presentation 전용 정적 이벤트를 추가했다.

```text
Exploded(Health owner, Vector3 worldPosition, bool domainAmplified)
```

중요:

```text
기존 onExploded callback
→ 기존 Damage / Domain Fuga Blast
→ 기존 Explosion Ring 생성
→ Presentation Exploded event
```

순서라서 Presentation subscriber에 문제가 생겨도 핵심 Damage 처리보다 뒤에서 호출되도록 했다.

또한 `owner`를 함께 넘겨 향후 적 스쿠나/다른 Fighter의 푸가가 생겨도 플레이어 카메라 피드백이 잘못 반응하지 않도록 현재 Fighter Shell의 Health와 일치할 때만 처리한다.

## 4. 허식 자 Release → Culmination

현재 허식 자 Prototype은 실제 이동형 Projectile이 아니라 Capsule 범위에 즉시 Damage를 적용한다.

따라서 이번 단계에서 가짜 Cast Delay를 넣거나 Damage 시점을 늦추지 않았다.

대신 기존 Release 피드백 약 0.09초 뒤에 작은 보라색 `Culmination Pulse`를 한 번 더 준다.

```text
Release
→ 큰 보라 피드백
→ 약 0.09초
→ 작은 Culmination Pulse
```

이는 실제 Impact 판정을 새로 만드는 것이 아니라, 현재 즉시 판정 Prototype에서 `발사 → 에너지 마무리` 리듬을 시험하는 Presentation placeholder다.

실제 모델/애니메이션/VFX가 들어가면 Animation Event 또는 실제 충돌 이벤트로 교체한다.

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

## VFX / SFX Hook 위치 정리

Gate 4에서 공통 계약으로 추출하기 전 현재 반복되는 지점은 다음과 같다.

```text
Signature Anticipation
- DomainReady / IsCasting / valid Ultimate input

Signature Release
- HollowPurplePrototypeVisual 활성
- Fuga Projectile 생성
- Domain Active 진입

Signature Impact / Culmination
- FugaProjectile.Exploded
- Purple Culmination placeholder timer
```

최종 자산 단계에서는 이 지점에:

```text
Animation Event
전용 VFX prefab
전용 Voice/SFX event
Camera profile
```

을 연결한다.

---

## 사용자 빠른 테스트

```text
1. git pull origin master
2. Unity Console 빨간 오류 확인

[고죠]
3. V로 무량공처 준비
→ 준비 시작 때 아주 약한 청색 Cue
→ 실제 영역 Active 때 기존보다 큰 개방 Cue

4. 허식 자
→ 발사 순간 큰 보라 Cue
→ 아주 잠깐 뒤 작은 두 번째 보라 Pulse

[스쿠나]
5. T → 스쿠나
6. 해 + 팔 후 푸가
→ R 입력 때 약한 긴장 Cue
→ 약 0.52초 후 Projectile 발사 때 Release Cue
→ 적/최대거리에서 폭발할 때 가장 강한 Impact Cue

7. 복마어주자
→ Casting 시작 때 약한 적색 Cue
→ Active 순간 강한 적색 개방 Cue
```

성공 기준:

```text
기술이 한 번 번쩍이는 것이 아니라
준비 → 발사/개방 → 마무리/충돌 리듬으로 읽힘

기존 Damage / CE / Cooldown / Domain / Tag 흐름 회귀 없음
Console 빨간 오류 없음
```

정상 확인되면 Gate 3C Pass 2를 USER VERIFIED로 닫고, 다음 묶음에서 대표 술식별 화면 공간 VFX/카메라 방향성을 더 강화한다.
