# Gate 2B — Opponent Team Architecture

작성 기준일: 2026-08-21

이 문서는 Gate 2B의 최소 상대 팀 구조와 사용자 테스트 기준을 기록한다.

## 상태

```text
Gate 1 Core Combat: USER VERIFIED
Gate 2A Player Active / Reserve: USER VERIFIED
Team-aware HUD Cleanup: USER VERIFIED
Gate 2B Opponent Team Runtime: REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## Gate 2B 목적

Gate 2A에서 플레이어 쪽의 Active / Reserve 구조는 실제 Unity에서 검증됐다.

Gate 2B에서는 상대편도 최소한 다음 구조를 가질 수 있는지 확인한다.

```text
PLAYER TEAM
Active + Reserve

VS

OPPONENT TEAM
Active + Reserve
```

다만 기존 Curse A + Curse B 동시 전투 환경은 스쿠나의 다중 해, 복마어주자, 영역 안 광역 푸가 등을 회귀 테스트하는 데 필요하다.

따라서 기존 훈련 환경을 없애지 않고 두 모드를 분리한다.

```text
TRAINING · MULTI CURSE
- Curse A + Curse B 동시 활성
- 기존 Gate 1 / Gate 2A 회귀 테스트 환경 보존

TEAM BATTLE
- Curse A Active
- Curse B Reserve
- Active KO 시 Reserve 자동 입장
- 마지막 상대 팀원 KO 시에만 VICTORY
```

## 이번 구현 범위

새 파일:

```text
unity/Assets/Scripts/Enemy/PrototypeOpponentTeamController.cs
```

이번 단계에서는 MatchController와 CurseBotController를 대규모로 뜯어고치지 않는다.

`PrototypeOpponentTeamController`가 런타임에 현재 두 CurseBot을 찾아 Gate 2B 최소 구조만 얹는다.

이 선택은 현재 개발 방법인 `Risk-Driven Vertical Slice Production`에 따른 것이다.

먼저 상대 팀 구조가 성립하는지를 검증한 뒤, 실제 캐릭터 모델/애니메이션/정식 Match Flow가 들어가는 시점에 더 큰 Entity 구조를 결정한다.

## 모드 전환

현재 임시 입력:

```text
F2 · MODE
```

첫 Play 시작 기본값:

```text
TRAINING · MULTI CURSE
```

F2를 누르면:

```text
현재 모드 반전
→ 같은 Scene 재시작
→ 선택한 모드로 다시 구성
```

예:

```text
TRAINING
→ F2
→ Scene reload
→ TEAM BATTLE

TEAM BATTLE
→ F2
→ Scene reload
→ TRAINING
```

F2는 최종 게임 조작이 아닌 `GAME_ORIGINAL` 개발용 임시 입력이다.

## Team Battle 상대 구조

현재 임시 상대 팀:

```text
CURSE A · Active
CURSE B · Reserve
```

두 적은 별도 GameObject이므로 각자 Health 상태를 독립적으로 가진다.

Team Battle 시작 시:

```text
Curse A 활성
Curse B GameObject 비활성
```

따라서 Reserve는:

```text
- 전장에 보이지 않음
- Target Lock 후보가 아님
- FindObjectsByType 기반 현재 전장 범위 공격 대상에서도 제외됨
- 자기 HP가 변하지 않음
```

첫 Active KO:

```text
Curse A HP 0
→ Curse A KO
→ 아직 Curse B가 살아 있으므로 기존 MatchController VICTORY 조건 미충족
→ 죽은 Active를 전장에서 비활성
→ Curse B를 저장된 시작 위치에서 활성
→ Curse B가 새 Active
```

마지막 상대 KO:

```text
Curse B HP 0
→ 두 상대 Health 모두 KO
→ 기존 MatchController LivingEnemyCount = 0
→ VICTORY
```

## Target Lock 주의

현재 최소 프로토타입에서는 첫 Active가 KO되면 기존 Target Lock은 죽은 타깃을 자동 해제한다.

새 Reserve가 입장한 뒤 TAB으로 다시 락온할 수 있어야 한다.

자동 타깃 승계는 이번 Gate 2B 최소 범위에 넣지 않았다.

이유:

```text
카메라 / Target / KO Entry의 최종 정책은
실제 Team Battle의 입장 연출과 캐릭터 Entity 구조를 더 확인한 뒤 결정
```

필요하면 Gate 2B 후속 단계에서 자동 승계를 추가한다.

## HUD

모든 모드에서 화면 상단 중앙에 임시 모드 칩을 표시한다.

```text
F2 · MODE · TRAINING · MULTI CURSE
또는
F2 · MODE · TEAM BATTLE
```

Team Battle에서는 우측에 추가 프로토타입 패널을 표시한다.

```text
OPPONENT TEAM
ACTIVE · CURSE A · HP ...
RESERVE · CURSE B · HP ...
```

첫 KO 뒤에는 Active / Reserve 역할이 바뀌고 잠시 `RESERVE ENTRY` 안내가 표시된다.

현재 기존 MatchController의 CURSE A / B HP 패널과 CURSES 2/2 표시는 그대로 남는다.

이는 Gate 2B 구조 검증을 우선하기 위한 임시 상태이며, 상대 Team Runtime 검증 뒤 Enemy HUD를 team-aware로 정리할지 판단한다.

## 사용자 테스트

```text
1. git pull origin master
2. Unity Console 빨간 오류 없는지 확인
3. Play 시작

[Training 보존]
4. 첫 시작이 TRAINING · MULTI CURSE인지 확인
5. Curse A와 Curse B가 둘 다 동시에 전장에 보이는지
6. TAB으로 두 적을 각각 Target Lock 할 수 있는지

[Team Battle 진입]
7. F2
→ 같은 Scene이 재시작되어야 함
8. 상단 칩이 TEAM BATTLE로 바뀌는지
9. 전장에는 상대가 한 명만 보이는지
10. OPPONENT TEAM 패널에서
→ ACTIVE CURSE A
→ RESERVE CURSE B
인지
11. TAB으로 현재 Active만 잡히는지

[첫 상대 KO]
12. Active Curse를 HP 0까지 공격
→ VICTORY가 뜨지 않아야 함
→ 죽은 Curse가 사라져야 함
→ Reserve가 자동으로 전장에 들어와야 함
→ OPPONENT TEAM의 ACTIVE / RESERVE 역할이 바뀌어야 함
→ 전 캐릭터는 KO 표시되어야 함
13. TAB으로 새 Active를 락온할 수 있는지

[최종 상대 KO]
14. 새 Active도 HP 0
→ 그때 VICTORY

[Training 복귀]
15. 재시작 뒤 F2로 TRAINING 복귀
16. 다시 두 Curse가 동시에 존재하는지

[플레이어 회귀]
17. 고죠 ↔ 스쿠나 T Tag 정상
18. HP / CE 보존 정상
19. 고죠/스쿠나 기존 기술 정상
20. 두 Curse Training 환경에서 다중 기술 회귀 정상
```

## Acceptance Criteria

```text
1. Training과 Team Battle을 분리 가능
2. Training에서는 두 Curse 동시 전투 유지
3. Team Battle에서는 1 Active + 1 Reserve
4. Reserve는 전장/타깃/광역 대상에서 제외
5. 첫 상대 KO에 VICTORY 발생하지 않음
6. 첫 상대 KO 뒤 Reserve 자동 입장
7. KO된 전 상대는 다시 등장하지 않음
8. 마지막 상대 KO에만 VICTORY
9. 새 Active를 TAB으로 락온 가능
10. 기존 플레이어 팀/기술 회귀 없음
```

## 아직 하지 않는 것

```text
상대의 수동 Tag AI
지원 공격
3인 상대 팀
실제 상대 주술사 캐릭터
상대 캐릭터별 CE snapshot
교대 애니메이션
자동 Target Lock 승계
Character Cost
온라인
```

현재 Curse 상대는 Match Architecture 검증용 임시 데이터다.

Gate 2B 상대 Team Runtime을 사용자 검증한 뒤 다음을 결정한다.

```text
1. Enemy Team HUD 정리
2. KO Entry / Target 승계 정책
3. 실제 Team Battle MatchController 분리 수준
4. Gate 2 종료 여부
5. Gate 3 Beauty Corner 진입
```
