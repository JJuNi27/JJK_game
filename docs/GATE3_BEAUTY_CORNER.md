# Gate 3 — Beauty Corner

작성 기준일: 2026-08-21

## 상태

```text
Gate 1 Core Combat Proof: USER VERIFIED
Gate 2 Match Architecture Proof: USER VERIFIED
Gate 3 Beauty Corner: STARTED
Gate 3A Combat Feel Pass 1: USER VERIFIED
Gate 3A Combat Feel Pass 2 Bundle: USER VERIFIED
Gate 3A Combat Feel Pass 3 Impact VFX: REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## Gate 3 목적

이 단계는 게임 전체를 90% 완성하는 단계가 아니다.

작은 대표 범위를 거의 완성품 품질로 만들어 실제 제작 파이프라인과 표현 품질 기준을 찾는 단계다.

목표:

```text
대표 맵 한 구역
실제 고죠/스쿠나 모델
공통 이동/기본공격/회피 애니메이션
고죠: 허식 자 + 무량공처 대표 연출
스쿠나: 푸가 + 복마어주자 대표 연출
전용 보이스/SFX
카메라/Hit Stop/피격 피드백
HUD
```

## Gate 2 종료 근거

2026-08-21까지 사용자 실제 Unity 검증으로 다음이 통과했다.

```text
Player Active / Reserve
HP / CE 독립 보존
수동 Tag / KO Auto Tag
상대 Active / Reserve
Training / Team Battle 분리
첫 상대 KO 뒤 Reserve 자동 입장
마지막 상대 KO에만 VICTORY
Player / Enemy Team-aware HUD
상대 Reserve 등장 시 Target Lock 자동 승계
```

상세:

```text
docs/TEAM_MATCH_GATE2.md
docs/GATE2B_OPPONENT_TEAM.md
docs/GATE2B_ENEMY_HUD_CLEANUP.md
docs/GATE2B_TARGET_HANDOFF.md
```

## Gate 3 진행 순서

```text
3A. Combat Feel Pass
3B. Representative Character Presentation
3C. Signature Technique Presentation
3D. Representative Arena + HUD Polish
```

실제 모델/맵 자산이 아직 완성되지 않아도 반복 작업이 적은 `전투 감각`부터 먼저 진행한다.

거대한 범용 Presentation/Ability 시스템은 만들지 않는다.

Beauty Corner에서 실제 반복이 확인된 뒤 Gate 4에서 공통 계약으로 추출한다.

---

# Gate 3A — Combat Feel Pass 1

## 목표

기본 3타의 게임 규칙은 바꾸지 않고 `적중했다`는 감각만 강화한다.

현재 `SimpleCameraFollow`에는 이미 프로토타입용:

```text
AddShake(amplitude, duration)
Flash(...)
```

API가 존재한다.

이번 Pass에서는 새 거대 시스템을 만들지 않고 기존 `AddShake`만 실제 기본 공격 적중에 연결한다.

## 변경 파일

```text
unity/Assets/Scripts/Player/BasicAttack.cs
```

## 구현 규칙

공격이 실제 `DamageResolution.Applied`일 때만 카메라 피드백이 발생한다.

```text
헛공격
→ Shake 없음

무하한/방어 등에 막힌 공격
→ Shake 없음

실제 HP에 적용된 공격
→ Shake 발생
```

강도:

```text
1타: 약한 Shake
2타: 조금 강한 Shake
3타 FINISH: 가장 강한 Shake
```

현재 프로토타입 수치:

```text
1타 amplitude 0.075 / 0.07s
2타 amplitude 0.11  / 0.085s
3타 amplitude 0.22  / 0.13s
```

이 수치는 `GAME_ORIGINAL` Beauty Corner 튜닝 값이며 최종값이 아니다.

## 이번 Pass에서 바꾸지 않는 것

```text
피해량
공격 범위
공격 쿨타임
넉백
히트스턴
HP / CE
Tag
Target Lock
술식
영역
```

즉 전투 규칙 회귀 리스크를 최소화한다.

## 사용자 테스트

2026-08-21 사용자 실제 Unity 테스트:

```text
[USER VERIFIED]
- 기본 공격 적중 카메라 Shake 정상
- 1타 < 2타 < 3타 FINISH 순서의 강도 차이 정상
- 허공 공격에는 Shake 없음
- 기존 전투 흐름에 이상 없음
```

사용자 판정: `정상`.

따라서:

```text
Gate 3A Combat Feel Pass 1: USER VERIFIED
```

---

# Gate 3A — Combat Feel Pass 2 Bundle

## 속도 조정

Gate 1~2처럼 작은 변경마다 사용자 pull/test를 반복하지 않고, Gate 3부터는 관련된 국소 표현 작업을 2~4개씩 묶어 한 번에 검증한다.

이번 묶음:

```text
1. 짧은 Hit Stop
2. 미세한 Hit Flash
3. Hit Stop 중에도 Camera Shake/Flash가 정상 진행되도록 unscaled feedback timing
```

## 변경 파일

```text
unity/Assets/Scripts/Core/PrototypeHitStopController.cs
unity/Assets/Scripts/Camera/SimpleCameraFollow.cs
unity/Assets/Scripts/Player/BasicAttack.cs
```

## Hit Stop

`PrototypeHitStopController`는 첫 사용 시 런타임에 자동 생성되며 별도 Scene 세팅이 필요 없다.

```text
실제 적중
→ 짧게 Time.timeScale 감소
→ Time.unscaledTime 기준으로 종료
→ 원래 Time Scale 복원
```

현재 기본 3타 값:

```text
1타 0.022초
2타 0.030초
3타 FINISH 0.055초

Hit Stop 중 상대 Time Scale = 기존의 8%
```

전체 전투 시간을 잠깐 같이 늦추므로 현재 `Time.time` 기반 공격 쿨타임/영역/술식 타이머도 같은 양만큼 같이 멈춘다. 즉 특정 시스템만 시간이 흘러가는 불일치를 피한다.

## Hit Flash

실제 HP 피해가 적용된 기본 공격에만 아주 약한 전체 화면 Flash를 추가한다.

```text
1타 0.025 alpha / 0.055초
2타 0.040 alpha / 0.070초
3타 0.075 alpha / 0.095초
```

1~2타는 흰색, 3타 FINISH는 약한 금빛 계열이다.

## Camera Feedback Timing

Hit Stop으로 `Time.time`이 느려져도 Shake/Flash가 멈춰서 길게 남지 않도록 `SimpleCameraFollow`의 해당 피드백 타이밍만 `Time.unscaledTime`으로 전환했다.

카메라 기본 Follow 자체는 기존 scaled `Time.deltaTime`을 유지한다.

## 발생 조건

Pass 1과 동일하게:

```text
헛공격
→ Hit Stop 없음 / Flash 없음 / Shake 없음

피해가 방어되어 DamageResolution.Applied가 아님
→ 피드백 없음

실제 HP 피해 적용
→ Shake + Hit Stop + Flash
```

여러 적이 한 번의 기본 공격 범위에 맞아도 해당 공격 1회당 피드백은 한 번만 발생한다.

## 사용자 검증

2026-08-21 사용자 실제 Unity 테스트:

```text
[USER VERIFIED]
- 1/2/3타 Hit Stop 정상
- Hit Flash 정상
- 기존 Shake 정상
- 3타 FINISH 강조 정상
- 허공 공격에는 피드백 없음
- 눈에 띄는 기존 전투 회귀 없음
```

사용자 판정: `다 정상`.

따라서:

```text
Gate 3A Combat Feel Pass 2 Bundle: USER VERIFIED
```

---

# Gate 3A — Combat Feel Pass 3 Impact VFX

## 목적

카메라 전체 피드백만으로는 `어디를 때렸는지`의 공간적 정보가 약하므로 실제 충돌 위치에 짧은 Hit Burst를 추가한다.

외부 Particle/VFX 자산이 아직 없으므로 지금은 최종 이펙트가 아니라 Beauty Corner용 procedural placeholder다.

## 변경 파일

```text
unity/Assets/Scripts/Core/PrototypeHitImpactVfx.cs
unity/Assets/Scripts/Player/BasicAttack.cs
```

## 규칙

`DamageResolution.Applied`가 발생한 적의 ImpactPoint에만 VFX를 생성한다.

```text
1타
→ 작고 짧은 흰색/청백색 Burst

2타
→ 조금 더 큰 청백색 Burst

3타 FINISH
→ 가장 큰 금빛 Burst
```

외형은 런타임 LineRenderer ray를 여러 방향으로 순간 확장하고 사라지게 하는 방식이다.

Hit Stop 중에도 자연스럽게 사라져야 하므로 VFX 수명/확장은 `Time.unscaledTime` 기준으로 동작한다.

여러 적이 동시에 실제 피해를 받으면 각 피해 위치에 각각 Burst가 생긴다.

## 이번에도 바꾸지 않는 것

```text
데미지
범위
쿨타임
넉백
히트스턴
Hit Stop 수치
Tag
Target Lock
술식/영역 규칙
```

## 사용자 테스트

```text
1. git pull origin master
2. Console 빨간 오류 확인
3. 주령에게 기본 3타 적중

확인:
- 맞은 위치에 짧은 Burst가 실제로 보이는지
- 1타 < 2타 < 3타 순으로 크기/강조가 커지는지
- 3타 금빛 FINISH Burst가 과하게 화면을 가리지 않는지
- Hit Stop 중 VFX가 멈춘 채 오래 남지 않는지

4. 허공 공격
→ Impact VFX 없음

5. 두 주령을 한 번에 맞히는 상황
→ 실제 피해를 받은 각 대상 위치에 VFX가 생기는지
```

정상 확인되면 Gate 3A Combat Feel을 여기서 닫고 Gate 3B로 넘어간다.

---

## Codex 사용 기준

속도 향상을 위해 이후 작업은 범위에 따라 직접 수정과 Codex를 병행한다.

```text
직접 수정
- 작은 수치/조건
- 1~3개 파일의 국소 변경
- 즉시 사용자 체감 테스트가 필요한 튜닝

Codex 우선
- 여러 파일을 동시에 읽고 수정해야 하는 Presentation 연결
- 반복되는 VFX/SFX/Animation 이벤트 연결
- 구조 리팩터링/마이그레이션
- 테스트 추가
```

Gate 3B부터 Codex 사용 비중을 높이고, Gate 4에서는 Beauty Corner에서 실제 반복된 계약 추출에 본격 사용한다.
