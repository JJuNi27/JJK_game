# Gate 4B — HUD Data Binding Extraction

작성 기준일: 2026-08-23

## 상태

```text
Gate 4A Character Presentation Contract: USER VERIFIED
Gate 4B Pass 1 · Active Fighter / Skill Deck Binding: USER VERIFIED
Gate 4B Pass 2 · Player Team HUD Binding: USER VERIFIED
Gate 4B Pass 3 · Opponent Team HUD Binding: USER VERIFIED
Gate 4B Pass 4 · Match HUD Snapshot Migration: REMOTE IMPLEMENTED / USER TEST PENDING
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
HUD → BasicAttack / Movement / CurseBot telegraph
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

# Pass 3 — Opponent Team HUD Binding · USER VERIFIED

파일:

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
Active / Reserve member
HP current / max
KO
```

`PrototypeOpponentTeamController`는 HUD용 read-only access만 공개하고 실제 Gameplay는 계속 직접 소유한다.

```text
F2 Mode 전환
Scene reload
KO 처리
Reserve 입장
Target Lock 승계
```

2026-08-23 사용자 실제 확인:

```text
정상!
```

따라서:

```text
Gate 4B Pass 3: USER VERIFIED
```

---

# Pass 4 — Match HUD Snapshot Migration

이번 마지막 migration에서는 `MatchController`의 표시 코드가 직접 Gameplay 컴포넌트를 읽는 부분을 추가로 줄였다.

## Player snapshot 확장

`PlayerCombatHudSnapshot`에 Match HUD용 read-only 표시 값을 추가했다.

```text
CE Profile Label
Dodge active / ready / cooldown
Basic attack chain step / label
Hit combo count / label
```

이 값들은 기존 `CursedEnergyController`, `ThirdPersonPlayerController`, `BasicAttack`에서 읽기만 한다.

Gameplay ownership은 바뀌지 않는다.

## Opponent snapshot 확장

`OpponentCombatHudSnapshot`에 다음 표시 값을 추가했다.

```text
Attack telegraph count
Maximum telegraph progress
```

현재 활성 상태인 CurseBot만 계산한다.

## MatchController Migration

기존 HUD 직접 참조 중 다음을 Snapshot 경로로 전환했다.

```text
Player identity / accent
Player HP / CE
Team mode 여부
Dodge chip
Basic chain / combo indicator
Opponent Training HP panels
Living Curse count
Enemy attack DODGE warning
F1 control help의 캐릭터/기술명
```

현재 경로:

```text
Gameplay Components
→ PlayerCombatHudDataSource / OpponentCombatHudDataSource
→ Snapshot
→ MatchController temporary IMGUI renderer
```

`MatchController`가 실제 전투 종료를 위해 사용하는 `BasicAttack`, `Movement`, `TargetLock`, enemy bot references는 Gameplay 책임이라 그대로 남겨 둔다.

고죠의 영역 입력 timing bar도 현재는 `GojoDomainController` 고유 UI라 직접 연결을 유지하고, 이후 Gate 4C Technique Presentation Contract에서 다룬다.

## 작은 HUD 회귀 수정

Team HUD의 Active fighter panel이 76px인데 기존 MatchController의 Dodge chip 기준 rect는 62px였다.

이번 migration에서 Team mode일 때 Dodge chip 기준 높이를 76px로 맞춰 Active HUD 하단과 겹칠 가능성을 제거했다.

---

# Pass 4 사용자 테스트

```text
1. cd D:\GitHub\JJK_game
2. git pull origin master
3. Unity compile 완료 대기
4. Console 빨간 오류 확인
5. CombatMVP Play
```

빠른 확인:

```text
[ ] 시작 화면 Player / Curse HUD 정상
[ ] Dodge Ready / cooldown / DODGING chip 정상
[ ] 기본 1/2/3 chain 및 combo 표시 정상
[ ] 주령 공격 예고 때 중앙 DODGE! 경고 + 진행 bar 정상
[ ] F1 도움말에서 고죠 기술명 정상
[ ] T Tag 후 스쿠나 Skill/Help identity 정상
[ ] Team mode에서 Dodge chip이 좌측 Active HUD와 겹치지 않음
[ ] F2 Training / Team Battle 전환 정상
[ ] Training에서 CURSE A/B HP panel 정상
[ ] Team Battle에서 기존 Opponent Team HUD 정상
[ ] 첫/마지막 KO Victory 규칙 정상
[ ] HP/CE/Tag/Target Lock/술식 Gameplay 회귀 없음
```

통과하면:

```text
Gate 4B HUD Data Binding Extraction: USER VERIFIED
```

로 닫고 다음은:

```text
Gate 4C Technique Presentation Request
```

로 넘어간다.

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
