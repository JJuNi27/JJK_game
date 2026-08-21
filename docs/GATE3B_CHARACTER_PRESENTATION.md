# Gate 3B — Representative Character Presentation

작성 기준일: 2026-08-21

## 상태

```text
Gate 3A Combat Feel Pass 1: USER VERIFIED
Gate 3A Combat Feel Pass 2 Bundle: USER VERIFIED
Gate 3A Combat Feel Pass 3 Impact VFX: USER VERIFIED
Gate 3A Combat Feel: USER VERIFIED

Gate 3B Character Presentation Pass 1: REMOTE IMPLEMENTED / USER TEST PENDING
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

실제 DamageContext 프로퍼티는:

```text
HitPoint
```

이므로 `context.HitPoint`로 수정했고, 사용자가 수정본을 다시 확인했다.

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

따라서 실제 모델을 기다리며 멈추지 않고, 나중에 Animator/모델로 교체할 수 있는 표현 규칙을 먼저 검증한다.

최종 Character Asset Contract 추출은 Gate 4에서 한다. 지금 거대한 범용 애니메이션 시스템은 만들지 않는다.

---

# Pass 1 — Basic Attack Pose + Tag Entry Readability

## 새 파일

```text
unity/Assets/Scripts/Player/PrototypeFighterPresentationController.cs
```

씬 세팅 없이 `RuntimeInitializeOnLoadMethod(AfterSceneLoad)`로 BasicAttack이 있는 Fighter Shell에 자동 부착한다.

`DefaultExecutionOrder(1500)`으로 기존 Prototype Avatar의 이동 흔들림 계산 뒤에 동작해 공격 포즈만 덧씌운다.

## 기본 공격 포즈

기존 `BasicAttack.DisplayChainStep`을 읽어 1/2/3타마다 짧은 포즈를 만든다.

```text
1타
→ 한쪽 팔 전진

2타
→ 반대쪽 팔 전진

3타 FINISH
→ 양팔 + 상체 전진 강조
```

Hit Stop 중 캐릭터 포즈도 같이 멈추는 편이 자연스러우므로 이 포즈 시간은 `Time.time` 기준이다.

## 캐릭터별 차이

고죠와 스쿠나가 같은 기본 3타 시스템을 사용해도 몸짓은 같지 않게 한다.

```text
GOJO
- 비교적 정돈되고 가벼운 회전
- FINISH 상체 전진은 절제

SUKUNA
- 더 큰 팔 각도
- 더 공격적인 상체 전진/회전
```

현재 수치는 모두 GAME_ORIGINAL Beauty Corner placeholder다.

## Tag Entry Readability

Active 캐릭터 Visual Root가 고죠 ↔ 스쿠나로 바뀌면 약 0.22초 동안 작은 Scale Pulse를 준다.

```text
Tag
→ 새 캐릭터 외형 활성
→ 약 1.075 scale에서 1.0으로 빠르게 복귀
```

최종 Tag Animation이 아니라 현재 Fighter Shell 방식에서 교대가 시각적으로 읽히는지만 검증하기 위한 placeholder다.

## 바꾸지 않은 것

```text
Damage
Hit Stop 수치
Hit VFX
공격 판정
콤보 시간
HP / CE
Tag 규칙
Target Lock
고죠/스쿠나 술식
영역
```

## 사용자 테스트

```text
1. git pull origin master
2. Unity Console 빨간 오류 없는지 확인
3. 고죠로 LMB 1→2→3
→ 팔/상체가 공격마다 실제로 움직이는지
→ 3타가 가장 큰 포즈인지

4. T → 스쿠나
→ 교대 직후 아주 짧은 크기 Pulse가 보이는지

5. 스쿠나 LMB 1→2→3
→ 고죠보다 조금 더 공격적인 동작으로 보이는지

6. 이동하면서 공격
→ 기존 걷기 팔/다리 흔들림이 완전히 깨지지 않는지

7. 허공 공격
→ Impact VFX/Hit Stop은 없어도 공격 포즈 자체는 나오는지
```

공격 포즈는 `공격 시도`의 표현이므로 허공에서도 나오는 것이 정상이다. 반대로 Hit Stop/Flash/Impact VFX는 실제 적중에서만 발생한다.

## 완료 기준

사용자가 실제 Unity에서 다음을 확인하면:

```text
Gate 3B Character Presentation Pass 1: USER VERIFIED
```

로 변경한다.

다음 Pass에서는 실제 모델 자산이 아직 없다면 회피/이동 stance readability를 묶어서 진행하고, 실제 모델이 준비되면 즉시 Animator/Model integration으로 전환한다.
