# Gate 5B — First Production-Quality Vertical Slice

작성 기준일: 2026-08-24

## 상태

```text
Gate 5A Third Character Stress Test: USER VERIFIED / CLOSED
Gate 5B First Production-Quality Vertical Slice: STARTED

Pass 1 · Character / Team Select Data Boundary: USER VERIFIED
Pass 2 · Battle Runtime consumes MatchTeamSelection:
USER VERIFIED

Pass 3A · Functional Character / Team Select front-end:
REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

---

# Gate 5B 목표

Gate 3에서 자산 부재로 미뤘던 production-quality 부분을 대표 한 판에 실제 연결한다.

```text
고죠 vs 스쿠나 대표 매치
실제 rigged model
Animator + 실제 animation set
production-grade technique VFX
Canvas/TMP final-style HUD
production SFX / Voice / Music
대표 맵 art / lighting / material / post process
정식 Character / Team Select front-end
```

목표 체감:

```text
"이제 실제 주술회전 게임 같다"
```

---

# 최종 Character / Team Select 방향

```text
Team Size: 1~3명

1명:
MAIN

2명:
MAIN + RESERVE 1

3명:
MAIN + RESERVE 1 + RESERVE 2
```

전투 시작 시 MAIN이 Active Fighter다.

최종 PC 교대 기본안:

```text
1 → Active ↔ Reserve 1
2 → Active ↔ Reserve 2
```

현재 `F3`, `F4`, `T`는 prototype/developer harness이며 최종 사용자 캐릭터 선택/교대 UX로 사용하지 않는다.

세부 UX 계획:

```text
docs/CHARACTER_TEAM_SELECT_PLAN.md
```

---

# Pass 1 — Character / Team Select Data Boundary · USER VERIFIED

파일:

```text
unity/Assets/Scripts/Player/MatchTeamSelection.cs
```

추가한 production-candidate 데이터:

```text
MatchTeamSlot
- Main
- Reserve1
- Reserve2

MatchTeamSelection
- Solo(main)
- Duo(main, reserve1)
- Trio(main, reserve1, reserve2)
- TeamSize
- slot 조회

MatchTeamSelectionStore
- Character Select와 Battle Scene 사이의 임시 cross-scene selection holder
- prototype default GOJO + SUKUNA
```

이 데이터는 캐릭터 선택/팀 슬롯만 표현하며 Battle GameObject, HP/CE, KO, HUD를 직접 소유하지 않는다.

2026-08-23 사용자 확인:

```text
추가 후 Unity compile / 기존 CombatMVP 시작 정상
```

따라서 Pass 1 데이터 경계는 USER VERIFIED 처리한다.

---

# Pass 2 — Battle Runtime consumes MatchTeamSelection

## 구현 상태

```text
USER VERIFIED
```

2026-08-27 사용자 회귀:
- Duo / Trio / Solo 전환 정상
- 1 / 2 Reserve slot 교대 정상
- 3인 KO 연쇄 / 마지막 KO defeat 정상
- F4 scene reload 후 전투 사운드 누락을 수정했고 재검증 정상

추가 combat timing cleanup:
- Hollow Purple lethal damage가 시전 즉시 승리를 띄우던 문제 수정
- 현재 prototype orb merge / travel timing에 맞춰 hit/damage/death/victory가 발생하도록 동기화
- 사용자 확인 정상

이번 묶음은 Pass 1의 선택 데이터를 실제 Player Team Runtime이 소비하게 연결한다.

변경 파일:

```text
unity/Assets/Scripts/Player/PrototypePlayerTeamController.cs
unity/Assets/Scripts/Core/PlayerCombatHudDataSource.cs
unity/Assets/Scripts/Core/CombatInputBindings.cs
```

## Runtime Team Model

기존 2인 고정 배열 / `activeIndex` 구조를 다음 slot 모델로 확장했다.

```text
members[0] = ACTIVE
members[1] = RESERVE 1
members[2] = RESERVE 2
```

`MatchTeamSelectionStore.PlayerTeam`을 읽어 실제 runtime roster를 1~3명으로 구성한다.

교대 시 캐릭터 ID를 새로 계산하는 대신 slot의 `TeamMemberState` 자체를 교환한다.

예:

```text
시작
A  GOJO
R1 SUKUNA
R2 MEGUMI

1 입력
A  SUKUNA
R1 GOJO
R2 MEGUMI

1 다시 입력
A  GOJO
R1 SUKUNA
R2 MEGUMI

2 입력
A  MEGUMI
R1 SUKUNA
R2 GOJO
```

각 TeamMemberState에 저장된 HP / CE / KO 상태도 slot swap과 함께 보존된다.

## 교대 입력

production-facing prototype command:

```text
1 → Reserve 1과 교대
2 → Reserve 2와 교대
```

기존 `T`는 현재 regression 편의를 위해 developer compatibility로 R1 교대에 남겨두었다.

교대 조건은 기존 규칙을 유지한다.

```text
CombatActionState.Normal
manual tag cooldown 완료
대상 Reserve 생존
```

## KO Auto Tag

Active가 KO되면:

```text
R1 생존 → R1 자동 교대
R1 KO + R2 생존 → R2 자동 교대
모든 Reserve KO → 교대 없음 → 기존 final defeat 경로
```

3인 확장에서 이 부분은 반드시 사용자 회귀로 확인한다.

## HUD Snapshot

기존 Gate 4B read-only 경계를 깨지 않고 확장했다.

추가:

```text
TeamSize
Reserve2Character
Reserve2Member
Reserve1TagState
Reserve2TagState
```

호환 유지:

```text
ReserveCharacter = R1
ReserveMember = R1
TagState = R1 state
```

Prototype Team HUD도 A / R1 / R2를 표시하도록 확장했다.

## Developer Harness

정식 Character Select UI가 아직 없으므로 regression용 임시 harness를 둔다.

```text
F3
GOJO + SUKUNA ↔ GOJO + MEGUMI

F4
DUO GOJO/SUKUNA
→ TRIO GOJO/SUKUNA/MEGUMI
→ SOLO GOJO
→ DUO GOJO/SUKUNA
```

`F3/F4/T`는 production UX가 아니다.

---

# Pass 2 사용자 회귀

```powershell
cd D:\GitHub\JJK_game
git pull origin master
```

Unity compile 완료 후 `CombatMVP` Play.

## 1. 기본 DUO

```text
[ ] 시작 GOJO + SUKUNA 정상
[ ] 1 → SUKUNA Active / GOJO R1
[ ] 1 다시 → GOJO Active
[ ] 2 → 아무 변화 없음
[ ] HP / CE가 왕복 교대 후 보존
[ ] T도 developer compatibility로 R1 교대 가능
```

## 2. TRIO

기본 DUO에서 `F4`.

```text
[ ] reload 후 TEAM · 3
[ ] A GOJO / R1 SUKUNA / R2 MEGUMI 표시
[ ] 1 → SUKUNA Active / GOJO R1
[ ] 1 다시 → GOJO Active
[ ] 2 → MEGUMI Active / 이전 Active가 R2
[ ] 여러 번 1/2 교대해도 각 캐릭터 HP / CE 보존
[ ] 메구미 Active에서 Q 옥견 정상
[ ] 술식/회피 등 Action 중 1/2 교대 차단
```

KO 회귀:

```text
[ ] 첫 Active KO → 살아있는 R1 자동 교대, DEFEAT 아님
[ ] 다음 Active KO → 남은 R2 자동 교대, DEFEAT 아님
[ ] 마지막 Fighter KO에만 DEFEAT
```

## 3. SOLO

TRIO에서 `F4`.

```text
[ ] reload 후 GOJO Solo
[ ] R1/R2 Team HUD 없음
[ ] 1 / 2 입력으로 교대되지 않음
[ ] 이동 / 기본공격 / Q/E/R/V 정상
```

다시 `F4` → 기본 DUO 복귀.

## 4. 기존 회귀

```text
[ ] F3 G/S ↔ G/M developer stress roster 정상
[ ] F2 Opponent Team 첫 KO reserve entry / target handoff / final victory 정상
[ ] VFX / Camera / Audio 중복 이상 없음
[ ] Console 빨간 오류 없음
```

---

# Pass 2 통과 후

```text
Gate 5B Pass 2: USER VERIFIED
```

그 다음 구현은 정식 `Character / Team Select` Canvas/TMP front-end가 `MatchTeamSelectionStore`를 작성하고 Battle Scene으로 넘기는 흐름이다.

3인 Runtime이 먼저 검증된 뒤 UI를 붙여 UI 문제와 Team Gameplay 문제를 섞지 않는다.


---

# Pass 3A — Functional Character / Team Select Front-End

상태:

```text
REMOTE IMPLEMENTED / USER TEST PENDING
```

현재 production asset과 dedicated select scene이 아직 없으므로 `SampleScene`을 임시 host로 사용한다.
실제 Battle GameObject를 직접 조작하지 않고 다음 경계를 검증한다.

```text
Character Select UI
→ MatchTeamSelection
→ MatchTeamSelectionStore
→ CombatMVP
→ PrototypePlayerTeamController
```

현재 구현:

```text
Roster: GOJO / SUKUNA / MEGUMI
1~3명 선택
선택 순서 = MAIN / R1 / R2
중복 선택 차단
slot 클릭 제거
UNDO / CLEAR
BATTLE 확정
키보드 1/2/3, Backspace, C, Enter
캐릭터 variant + Q/E/R/V preview
Canvas 기반 1920x1080 scale UI
```

주의:
- 현재 typography는 Unity UI Text + runtime system font fallback이다.
- Gate 5B final-style 단계에서 dedicated font asset과 TMP binding으로 교체한다.
- 실제 portrait / 3D preview 자산 hook도 production asset 투입 시 연결한다.

사용자 테스트:

```text
1. SampleScene Play
2. GOJO만 선택 → BATTLE → CombatMVP Solo
3. SampleScene 재시작 → GOJO + SUKUNA → Duo
4. SampleScene 재시작 → GOJO + SUKUNA + MEGUMI → Trio
5. 순서를 바꿔 MAIN이 선택 순서 첫 캐릭터인지 확인
6. slot 제거 / Undo / Clear 확인
7. Battle 진입 후 1/2 교대와 HP/CE 보존 확인
8. Console red error 없음
```
