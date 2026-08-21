# Gate 2B — KO Entry / Target Lock Handoff

작성 기준일: 2026-08-21

## 상태

```text
Gate 2B Opponent Team Core Runtime: USER VERIFIED
Enemy Team HUD · Team Battle View: USER VERIFIED
Automatic Target Lock Handoff: USER VERIFIED
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 결정

사용자 결정:

```text
상대 Active KO
→ Reserve 자동 입장
→ 플레이어 Target Lock도 새 Active로 자동 승계
```

이 정책은 현재 Gate 2B Team Battle 프로토타입의 `GAME_ORIGINAL` 규칙이다.

현재는 새 상대가 등장했는데 사용자가 TAB을 다시 눌러야 하는 흐름보다, 1:1 교대전의 전투 흐름을 끊지 않는 자동 승계를 우선한다.

## 구현

### TargetLockController

새 공개 API:

```text
TryLockTarget(Health target)
```

검사:

```text
- 플레이어가 살아 있음
- TargetLockController 활성
- 대상이 존재
- 대상이 살아 있음
- 대상 GameObject가 activeInHierarchy
- 최대 락온 거리 안
```

기존 수동 TAB 전환/해제 로직은 유지한다.

또한 비활성 Reserve가 실수로 Target 후보가 되지 않도록 `IsValidTarget`에서 `activeInHierarchy`를 확인한다.

### PrototypeOpponentTeamController

첫 Active KO 후 Reserve를 활성화한 직후:

```text
Reserve GameObject 활성
→ CurseBot 활성 확인
→ 플레이어 TargetLockController 탐색
→ TryLockTarget(new Active)
```

을 수행한다.

마지막 상대 KO에는 새 Reserve가 없으므로 자동 승계가 발생하지 않는다.

## 사용자 검증

2026-08-21 사용자 실제 Unity 확인:

```text
[USER VERIFIED]
- Team Battle에서 첫 상대를 KO하면 Reserve가 새 Active로 등장함
- TAB을 다시 누르지 않아도 Target Lock이 새 Active로 자동 승계됨
- 마지막 상대 KO 시 정상적으로 VICTORY 발생
- 최종 KO 뒤 존재하지 않는 대상에 Target Lock이 남지 않음
```

사용자 확인 메시지: `ㅇㅋ 정상확인이야`

## 완료 판정

```text
Automatic Target Lock Handoff: USER VERIFIED
```

이 항목으로 Gate 2B의 KO Entry / Target 정책 검증은 완료했다.

Gate 2 전체 종료 판정은 Gate 2A의 기존 사용자 검증, Gate 2B Core Runtime, Enemy Team HUD, 자동 Target Lock 검증을 합쳐 `docs/HANDOFF.md`와 `docs/DEVELOPMENT_ROADMAP.md`에 기록한다.
