# Gate 3 — Beauty Corner

작성 기준일: 2026-08-21

## 상태

```text
Gate 1 Core Combat Proof: USER VERIFIED
Gate 2 Match Architecture Proof: USER VERIFIED
Gate 3 Beauty Corner: STARTED
Gate 3A Combat Feel Pass 1: USER VERIFIED
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

## 다음 Pass

다음은:

```text
Gate 3A Combat Feel Pass 2
- 짧은 Hit Stop
- 3타 FINISH 중심으로 시작
- 영역/술식 시간 로직에 영향이 없는지 검증
```

Hit Stop은 Time Scale을 건드릴 가능성이 있어 Shake보다 리스크가 높다.

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

Gate 4에서는 Beauty Corner에서 실제 반복된 계약을 추출하므로 Codex 사용 비중이 더 커질 예정이다.
