# Gate 4B — HUD Data Binding Extraction

작성 기준일: 2026-08-23

## 상태

```text
Gate 4A Character Presentation Contract: USER VERIFIED
Gate 4B Pass 1 · Active Fighter / Skill Deck Binding: USER VERIFIED
Gate 4B Pass 2 · Player Team HUD Binding: USER VERIFIED
Gate 4B Pass 3 · Opponent Team HUD Binding: REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 목적

Gate 3 HUD는 동작 검증을 위해 각 UI 코드가 Gameplay Controller를 직접 읽는 방식으로 빠르게 만들어졌다.

```text
HUD → Health
HUD → CursedEnergyController
HUD → PrototypeCharacterController
HUD → CombatActionGate
HUD → Player/Opponent Team Controller
```

Gate 4B에서는 UI가 구체 Controller 대신 `읽기 전용 HUD 데이터 계약`을 소비하도록 단계적으로 바꾼다.

---

# Pass 1 — PlayerCombatHudDataSource · USER VERIFIED

파일:

```text
unity/Assets/Scripts/Core/PlayerCombatHudDataSource.cs
```

핵심 타입:

```text
PlayerCombatHudDataSource
PlayerCombatHudSnapshot
```

DataSource는 값을 소유하지 않고 기존 Gameplay Controller에서 읽어 Snapshot만 만든다.
Gameplay Command도 실행하지 않는다.

첫 Consumer는 `PrototypeSkillDeckHud`였다.

2026-08-23 사용자 실제 확인:

```text
정상임
```

따라서:

```text
Gate 4B Pass 1: USER VERIFIED
```

---

# Pass 2 — Player Team HUD Binding · USER VERIFIED

추가 읽기 전용 타입:

```text
PlayerTagHudState
PlayerTeamMemberHudSnapshot
```

`PlayerCombatHudSnapshot`이 Active/Reserve와 Tag 화면에 필요한 정보를 제공한다.

```text
Active / Reserve Character ID
CharacterPresentationProfile
Initialized
KO
HP
CE
Tag READY / COOLDOWN / ACTION LOCK / RESERVE KO
```

Gameplay 소유권은 그대로 `PrototypePlayerTeamController`에 있다.

```text
Tag 실행
HP / CE 저장 및 복원
KO Auto Tag
Invulnerability
Tag cooldown
```

HUD가 필요한 Reserve 저장 상태만 읽도록 `TryGetStoredMemberState(...)` read-only bridge를 사용한다.

2026-08-23 사용자 실제 확인:

```text
모두 정상!
```

따라서:

```text
Gate 4B Pass 2: USER VERIFIED
```

---

# Pass 3 — Opponent Team HUD Binding

새 파일:

```text
unity/Assets/Scripts/Core/OpponentCombatHudDataSource.cs
```

핵심 타입:

```text
OpponentCombatHudDataSource
OpponentCombatHudSnapshot
OpponentTeamMemberHudSnapshot
```

Snapshot이 제공하는 정보:

```text
Encounter Mode
TEAM BATTLE 여부
Living member count
Team size
Reserve Entry notice
Active member
Reserve member
- index / CURSE A,B identity
- active 여부
- HP current / max
- KO
```

`OpponentCombatHudDataSource`는 다음을 하지 않는다.

```text
F2 Mode 전환
Scene reload
KO 처리
Reserve 입장
Target Lock 승계
Victory 판정
```

이 Gameplay는 기존 `PrototypeOpponentTeamController` / `MatchController`가 계속 소유한다.

## PrototypeOpponentTeamController Migration

Controller에 HUD용 read-only access만 추가했다.

```text
IsInitialized
ActiveMemberIndex
TeamSize
EntryNoticeActive
GetMember(index)
```

기존 IMGUI 렌더링은 이제 직접 private `members[]`, `activeIndex`, `entryNoticeUntil`을 조합하지 않고:

```text
OpponentCombatHudDataSource
→ OpponentCombatHudSnapshot
→ Mode chip
→ Opponent Team panel
→ Active / Reserve rows
```

경로로 화면을 그린다.

즉 Controller 내부에 임시 IMGUI가 아직 같이 있지만 UI 데이터 경계는 분리했다.

---

# Pass 3 사용자 테스트

```text
1. cd D:\GitHub\JJK_game
2. git pull origin master
3. Unity compile 완료 대기
4. Console 빨간 오류 확인
5. CombatMVP Play
```

확인:

```text
[ ] 시작 TRAINING · MULTI CURSE 표시 정상
[ ] F2 → TEAM BATTLE 전환 정상
[ ] 우측 OPPONENT panel 정상
[ ] A CURSE A / R CURSE B HP 표시 정상
[ ] 첫 Active KO 뒤 Reserve 입장
[ ] RESERVE ENTRY 약 1.5초 표시
[ ] 새 Reserve가 Active row로 이동
[ ] Target Lock 자동 승계 정상
[ ] 첫 KO에는 VICTORY 없음
[ ] 마지막 상대 KO에만 VICTORY
[ ] 다시 F2 mode 전환 시 기존 동작 정상
```

통과하면:

```text
Gate 4B Pass 3: USER VERIFIED
```

그 뒤 `MatchController`에 남아 있는 legacy non-team player/enemy HUD 및 전투 상태 표시를 조사해 4B의 마지막 범위를 결정한다.

---

# 바꾸지 않은 Gameplay

```text
Input Binding
HP / CE 규칙
Damage
Cooldown 수치
Technique 조건
Domain Rule
Tag 실행 규칙
KO Auto Tag
Tag invulnerability
Target Lock
Opponent Reserve Entry
Victory / Defeat
```

Gate 4B는 HUD가 상태를 `어디서 읽는지`만 분리한다.
