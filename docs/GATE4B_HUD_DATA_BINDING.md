# Gate 4B — HUD Data Binding Extraction

작성 기준일: 2026-08-23

## 상태

```text
Gate 4A Character Presentation Contract: USER VERIFIED
Gate 4B Pass 1 · Active Fighter / Skill Deck Binding: REMOTE IMPLEMENTED / USER TEST PENDING
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

# Pass 1 — PlayerCombatHudDataSource

새 파일:

```text
unity/Assets/Scripts/Core/PlayerCombatHudDataSource.cs
```

핵심 타입:

```text
PlayerCombatHudDataSource
PlayerCombatHudSnapshot
```

Snapshot은 현재 다음을 제공한다.

```text
Active Character ID
CharacterPresentationProfile
HP current / max
CE current / max
CE 존재 여부
CombatActionState
Technique Burnout
Technique 사용 가능
Ultimate 사용 가능
Domain 사용 가능
Team Mode
Reserve Character ID
Living Reserve 여부
Tag Cooldown remaining
```

중요:

```text
DataSource는 값을 소유하지 않는다.
Gameplay Command를 실행하지 않는다.
기존 Gameplay Controller에서 읽어서 Snapshot만 만든다.
```

따라서 Presentation Layer가 Gameplay Rule의 새로운 소유자가 되지 않는다.

---

# 첫 Consumer Migration — PrototypeSkillDeckHud

기존 직접 참조:

```text
PrototypeCharacterController
Health
CombatActionGate
```

을 제거하고:

```text
PlayerCombatHudDataSource
→ PlayerCombatHudSnapshot
```

만 읽도록 변경했다.

Skill label/accent는 Snapshot에 포함된 `CharacterPresentationProfile`에서 읽는다.

상태:

```text
READY
DODGE
CASTING
DOMAIN INPUT
DOMAIN ACTIVE
TECHNIQUE BURNOUT
DISABLED
```

도 Snapshot의 ActionState/Burnout에서 결정한다.

Q/E/R/V 입력 pulse는 기존 UI-local presentation이므로 그대로 유지한다.

---

# 바꾸지 않은 것

```text
Input Binding
HP / CE 규칙
Damage
Cooldown
Technique 조건
Domain Rule
Tag
KO Auto Tag
Target Lock
Opponent Team
Victory / Defeat
```

즉 이번 migration은 HUD가 상태를 `어디서 읽는지`만 바꾼다.

---

# 사용자 테스트

```text
1. cd D:\GitHub\JJK_game
2. git pull origin master
3. Unity compile 완료 대기
4. Console 빨간 오류 확인
5. CombatMVP Play
```

확인:

```text
[ ] 고죠 Skill Deck 정상 표시
[ ] Q 창 / E 혁 / R 허식 자 / V 무량공처
[ ] Q/E/R/V pulse 정상
[ ] READY / DODGE / CASTING 상태 정상
[ ] T Tag 후 스쿠나 Skill Deck 즉시 변경
[ ] Q 해 / E 팔 / R 푸가 / V 복마어주자
[ ] HP / CE / Tag 기존 동작 정상
[ ] 대표 술식 입력 정상
```

통과하면:

```text
Gate 4B Pass 1: USER VERIFIED
```

로 닫고 Pass 2에서 Player Team HUD의 Active/Reserve 정보 공급을 같은 HUD 계약 쪽으로 옮긴다.
