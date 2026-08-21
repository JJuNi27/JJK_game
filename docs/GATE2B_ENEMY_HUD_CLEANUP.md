# Gate 2B — Enemy Team HUD Cleanup

작성 기준일: 2026-08-21

## 상태

```text
Gate 2B Opponent Team Core Runtime: USER VERIFIED
Enemy Team HUD Cleanup · Team Battle View: USER VERIFIED
Enemy Team HUD Cleanup · Training Regression: USER TEST PENDING
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

## 2026-08-21 사용자 캡처 확인

사용자가 Team Battle에서 두 상대를 모두 KO한 최종 VICTORY 화면을 공유했다.

캡처에서 다음을 직접 확인했다.

```text
[USER VERIFIED · Team Battle View]
- 기존 CURSE A / CURSE B 개별 HP 패널이 보이지 않음
- 기존 CURSES 2/2 카운트가 보이지 않음
- 우측 상단에 OPPONENT TEAM 패널 하나만 존재
- 최종 상태가 OPPONENT TEAM · 0/2로 표시됨
- Curse A / Curse B가 모두 HP 0 / KO로 표시됨
- 두 상대를 모두 KO한 뒤 VICTORY 정상 표시
```

따라서 Team Battle 화면의 중복 Enemy HUD 제거와 최종 팀 상태 표시는 사용자 검증 완료로 본다.

Training 모드에서 기존 `CURSE A / CURSE B / CURSES 2/2` HUD가 그대로 유지되는지는 Gate 2B 최종 회귀에서 한 번 더 확인한다.

## 사용자 확인

```text
1. git pull origin master
2. Unity Console 빨간 오류 확인

[남은 회귀]
3. F2로 Training 복귀
→ 기존 우측 CURSE A / CURSE B 패널과 CURSES 2/2가 유지되는지

4. F2로 Team Battle 재진입
→ OPPONENT TEAM 단일 HUD가 유지되는지
```

남은 Training 회귀까지 사용자가 실제 Unity에서 확인하면:

```text
Enemy Team HUD Cleanup: USER VERIFIED
```

로 최종 변경한다.

다음 Gate 2B 작업은 `KO Entry / Target Lock 자동 승계`다.
