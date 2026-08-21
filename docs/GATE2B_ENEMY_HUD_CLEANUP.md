# Gate 2B — Enemy Team HUD Cleanup

작성 기준일: 2026-08-21

## 상태

```text
Gate 2B Opponent Team Core Runtime: USER VERIFIED
Enemy Team HUD Cleanup: REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 배경

Gate 2B 핵심 런타임은 사용자 실제 Unity 테스트에서 다음이 확인됐다.

```text
- Team Battle에서 상대가 한 번에 한 명씩 등장
- 첫 상대 KO 뒤 Reserve 자동 입장
- 첫 상대 KO만으로는 VICTORY가 발생하지 않음
- 마지막 상대까지 KO해야 VICTORY
```

핵심 구조가 확인됐으므로 다음으로 임시 중복 Enemy HUD를 정리한다.

기존 Team Battle 화면에는 다음 두 HUD가 동시에 존재할 수 있었다.

```text
MatchController
- CURSE A HP
- CURSE B HP
- CURSES 2/2

PrototypeOpponentTeamController
- OPPONENT TEAM
- ACTIVE / RESERVE
```

Team Battle에서는 Active / Reserve 정보가 더 중요하므로 중복 표시를 제거한다.

## 변경 내용

### MatchController

Team Battle이 요청된 상태에서는 기존 Enemy 개별 패널과 `CURSES x/2` 카운트를 그리지 않는다.

```text
TRAINING · MULTI CURSE
→ 기존 CURSE A / CURSE B / CURSES 2/2 HUD 유지

TEAM BATTLE
→ 기존 Enemy HUD 숨김
→ OPPONENT TEAM HUD가 상대 팀 정보를 단독 담당
```

Enemy 공격 경고는 Reserve GameObject가 비활성인 경우 계산에서 제외하도록 `activeInHierarchy`를 확인한다.

### PrototypeOpponentTeamController

Team Battle용 상대 HUD를 우측 상단으로 이동한다.

```text
OPPONENT TEAM · 2/2
ACTIVE · CURSE A · HP ...
RESERVE · CURSE B · HP ...
```

첫 Active KO 뒤에는:

```text
OPPONENT TEAM · 1/2 · RESERVE ENTRY
ACTIVE · CURSE B · HP ...
RESERVE · CURSE A · HP 0/... · KO
```

형태가 된다.

`TeamBattleModeRequested` 정적 상태를 노출해 MatchController가 Team Battle 중인지 HUD 수준에서 확인할 수 있도록 했다.

## 사용자 확인

```text
1. git pull origin master
2. Unity Console 빨간 오류 확인
3. Training 시작
→ 기존 우측 CURSE A / CURSE B 패널과 CURSES 2/2가 유지되는지

4. F2 → Team Battle
→ 기존 CURSE A / CURSE B 개별 패널이 사라지는지
→ 기존 CURSES 2/2 카운트가 사라지는지
→ 우측 상단 OPPONENT TEAM 패널 하나만 보이는지

5. 첫 상대 KO
→ OPPONENT TEAM이 2/2 → 1/2로 변하는지
→ Active / Reserve 역할이 정상 교체되는지
→ RESERVE ENTRY 안내가 잠시 보이는지

6. 마지막 상대 KO
→ VICTORY 정상
```

위 화면과 승패 흐름을 사용자가 실제 Unity에서 확인하면:

```text
Enemy Team HUD Cleanup: USER VERIFIED
```

로 변경한다.

그 다음 Gate 2B의 마지막 판단 항목은 `KO Entry / Target Lock 승계 정책`과 최종 회귀다.
