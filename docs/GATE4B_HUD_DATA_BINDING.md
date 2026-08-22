# Gate 4B — HUD Data Binding Extraction

작성 기준일: 2026-08-23

## 상태

```text
Gate 4A Character Presentation Contract: USER VERIFIED
Gate 4B Pass 1 · Active Fighter / Skill Deck Binding: USER VERIFIED
Gate 4B Pass 2 · Player Team HUD Binding: REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 목적

Gate 3 HUD는 동작 검증을 위해 각 UI 코드가 Gameplay Controller를 직접 읽는 방식으로 빠르게 만들어졌다.

예:

```text
HUD → Health
HUD → CursedEnergyController
HUD → PrototypeCharacterController
HUD → CombatActionGate
HUD → PrototypePlayerTeamController
```

이 구조는 최종 Canvas/TMP HUD로 교체하거나 세 번째 Fighter를 추가할 때 결합도가 높다.

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

첫 Consumer는 `PrototypeSkillDeckHud`였고 다음 직접 참조를 제거했다.

```text
PrototypeCharacterController
Health
CombatActionGate
```

대신:

```text
PlayerCombatHudDataSource
→ PlayerCombatHudSnapshot
```

만 읽는다.

2026-08-23 사용자 실제 확인:

```text
정상임
```

따라서:

```text
Gate 4B Pass 1: USER VERIFIED
```

---

# Pass 2 — Player Team HUD Binding

이번 Pass에서는 같은 Snapshot 계약을 Player Team HUD까지 확장했다.

추가 읽기 전용 타입:

```text
PlayerTagHudState
PlayerTeamMemberHudSnapshot
```

`PlayerCombatHudSnapshot`이 이제 추가로 제공한다.

```text
Active Member snapshot
Reserve Member snapshot
- Character ID
- CharacterPresentationProfile
- Active 여부
- Initialized 여부
- KO 여부
- HP
- CE

Tag HUD state
- READY
- COOLDOWN
- ACTION LOCK
- RESERVE KO
```

## PrototypePlayerTeamController의 역할 분리

Gameplay 소유권은 그대로 유지한다.

```text
Tag 실행
HP / CE 저장 및 복원
KO Auto Tag
Invulnerability
Tag cooldown
```

은 여전히 `PrototypePlayerTeamController`가 담당한다.

HUD가 필요한 reserve 저장 상태만 읽을 수 있도록 다음 read-only bridge를 추가했다.

```text
TryGetStoredMemberState(...)
```

이 메서드는 상태를 변경하지 않는다.

## Player Team HUD Migration

기존 `OnGUI()`는 직접:

```text
Health
CursedEnergyController
private TeamMemberState
CombatActionGate
```

를 읽어 Active/Reserve/Tag 화면을 구성했다.

현재 HUD 렌더링 경로는:

```text
PlayerCombatHudDataSource
→ PlayerCombatHudSnapshot
→ Active Fighter panel
→ Active / Reserve rows
→ Tag state
```

으로 변경했다.

즉 같은 `PrototypePlayerTeamController` 안에 Gameplay와 임시 IMGUI 렌더링이 함께 남아 있기는 하지만, 렌더링 부분은 더 이상 Gameplay 필드를 직접 조합하지 않는다.

이 단계는 이후 Canvas/TMP HUD 컴포넌트를 별도 객체로 옮길 때 필요한 중간 migration이다.

---

# 바꾸지 않은 것

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
Opponent Team
Victory / Defeat
```

이번 migration은 HUD가 상태를 `어디서 읽는지`만 바꾼다.

---

# Pass 2 사용자 테스트

```text
1. cd D:\GitHub\JJK_game
2. git pull origin master
3. Unity compile 완료 대기
4. Console 빨간 오류 확인
5. CombatMVP Play
```

빠른 확인:

```text
[ ] 시작 고죠 Active HUD HP/CE 정상
[ ] Team panel A 고죠 / R 스쿠나 정상
[ ] T TAG READY 표시 정상
[ ] T Tag 후 Active/Reserve가 서로 교체
[ ] Tag 직후 cooldown 초 표시 정상
[ ] 술식/회피 중 ACTION LOCK 표시 정상
[ ] 고죠/스쿠나 HP/CE 독립 보존 정상
[ ] Reserve가 KO면 RESERVE KO 표시 및 수동 Tag 차단
[ ] 첫 Active KO → 살아있는 Reserve Auto Tag
[ ] 마지막 Player KO에만 DEFEAT
[ ] Skill Deck도 Active Fighter와 함께 정상 변경
```

통과하면:

```text
Gate 4B Pass 2: USER VERIFIED
```

다음은 Opponent/Match HUD까지 같은 방식으로 묶을지 조사한 뒤 4B를 닫고 Gate 4C Technique Presentation Request로 넘어간다.
