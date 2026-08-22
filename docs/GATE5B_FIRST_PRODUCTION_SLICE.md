# Gate 5B — First Production-Quality Vertical Slice

작성 기준일: 2026-08-23

## 상태

```text
Gate 5A Third Character Stress Test: USER VERIFIED / CLOSED
Gate 5B First Production-Quality Vertical Slice: STARTED

Pass 1 · Character / Team Select Data Boundary:
REMOTE IMPLEMENTED / RUNTIME WIRING PENDING
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

현재 `F3`, `T`, Solo용 `Alpha1/2/3`은 prototype/developer harness이며 최종 사용자 캐릭터 선택/교대 UX로 사용하지 않는다.

세부 UX 계획:

```text
docs/CHARACTER_TEAM_SELECT_PLAN.md
```

---

# Pass 1 — Character / Team Select Data Boundary

새 파일:

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
- 현재 prototype 기본값은 GOJO + SUKUNA
```

이 데이터는 캐릭터 선택/팀 슬롯만 표현한다.

의도적으로 하지 않는 것:

```text
Battle GameObject 직접 생성
HP / CE 보관
교대 실행
KO 규칙
HUD 직접 제어
Character Select Canvas 생성
```

즉 UI가 Gameplay object를 직접 켜고 끄지 않고 다음 경계를 따르기 위한 첫 단계다.

```text
Character Select UI
→ MatchTeamSelection
→ Battle bootstrap / Team Controller
→ Fighter runtime
```

---

# 다음 작업

다음 세션의 첫 구현 묶음:

```text
1. PrototypePlayerTeamController가 MatchTeamSelection을 읽도록 확장
2. 1명 / 2명 / 3명 runtime roster 표현
3. Reserve Slot 1 / 2 교대 command 분리
4. 기존 T/F3는 developer-only harness로 격리
5. 그 위에 Character Select Canvas/TMP front-end 연결
```

단, 기존 Gate 2/5A 전투 회귀를 막기 위해 runtime 3인 확장은 별도 regression 단위로 검증한다.

---

# 오늘 마감점

2026-08-23 세션은 Gate 5A closure와 Gate 5B Match Selection 데이터 경계 시작까지 진행하고 종료한다.

다음 세션 시작점:

```text
Gate 5B Pass 2 · Battle Runtime consumes MatchTeamSelection
```
