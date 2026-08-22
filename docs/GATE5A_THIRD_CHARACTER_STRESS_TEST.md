# Gate 5A — Third Character Stress Test

작성 기준일: 2026-08-23

## 상태

```text
Gate 4 Production Pipeline Extraction: USER VERIFIED / CLOSED
Gate 5A Third Character Stress Test: IN PROGRESS

Pass 1 · Third Character Identity / Roster / HUD Plumbing:
REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

---

# 선택 캐릭터

```text
후시구로 메구미
```

선택 이유:

```text
고죠/스쿠나 = 사용자 본체가 직접 기술을 발동하는 구조
메구미 = 식신이라는 별도 전투 주체를 소환/관리하는 구조
```

따라서 Gate 4의 공통 경계가 단순 세 번째 색놀이가 아니라 구조가 다른 Fighter에서도 버티는지 확인하기 좋다.

---

# Gate 5A 성공 기준

```text
1. 새 Character ID/Profile을 추가해 기존 HUD가 수정 없이 identity를 표시한다.
2. Team state/HP/CE 보존 구조가 세 번째 Character ID에서도 동작한다.
3. Animation State/Cue contract가 세 번째 Fighter를 받아들인다.
4. 이후 식신 술식이 Technique Presentation Request를 재사용한다.
5. 이후 식신 VFX가 VFX Lifecycle을 재사용한다.
6. 이후 메구미 SFX가 CombatAudioEvent를 재사용한다.
7. 고죠/스쿠나 Presentation/HUD 코드를 대량 복붙하지 않는다.
```

고유 식신 AI/소환 규칙 자체가 별도 Gameplay 코드인 것은 정상이다.

---

# Pass 1 — Identity / Roster / HUD Plumbing

이번 Pass는 술식 구현 전에 Gate 4의 가장 바깥쪽 contract부터 검증한다.

## 새 Character ID

```text
PrototypeCharacterId.MegumiStudent
```

## Presentation Profile

```text
FUSHIGURO MEGUMI · 도쿄고 학생
HUD: MEGUMI
Short: 메구미

Q 옥견
E 누에
R 만상
V 감합암예정
```

현재 skill label은 향후 Gameplay slot 계획을 위한 presentation metadata다.
Pass 1에서는 Q/E/R/V Gameplay는 아직 구현하지 않는다.

## Prototype Visual

```text
MegumiPrototypeAvatar
→ PrototypeMegumiAvatar
```

이것은 Gate 5A 구조 테스트용 procedural placeholder다.
실제 메구미 모델이 아니다.

`PrototypeFighterPresentationController`는 Gate 4E의 `FighterAnimationStateSnapshot.CharacterId`를 보고 새 Megumi visual root를 선택한다.

Pass 1에서는 별도 production animation을 만들지 않고 기존 non-Sukuna prototype pose fallback을 재사용한다.

## Stress Roster

기존 2인 팀 규칙 자체를 3인 팀으로 확장하지 않는다.
Gate 5A 목적은 `세 번째 Character` 검증이지 `3인 Team UI` 검증이 아니기 때문이다.

대신 동일한 2인 Active/Reserve 구조에서 roster만 교체한다.

```text
기본:
GOJO + SUKUNA

F3:
GOJO + MEGUMI

다시 F3:
GOJO + SUKUNA
```

F3를 누르면 현재 scene을 reload해 roster를 안전하게 다시 초기화한다.

Team HUD 상단:

```text
TEAM · G/S
TEAM · G/M
```

으로 현재 stress roster를 표시한다.

---

# 이번 Pass에서 의도적으로 하지 않은 것

```text
옥견 Gameplay
누에 Gameplay
만상 Gameplay
감합암예정 Gameplay
식신 AI
식신 HP/소환 지속시간
메구미 전용 camera/VFX/audio tuning
3인 팀 UI
production model / animation
```

먼저 `세 번째 Character ID가 Gate 4 경계를 통과하는가`를 확인한 뒤 식신 Gameplay로 들어간다.

---

# 사용자 테스트

```powershell
cd D:\GitHub\JJK_game
git pull origin master
```

Unity compile 완료 후 `CombatMVP` Play.

빠른 확인:

```text
[ ] 기본 시작은 기존 GOJO + SUKUNA이며 회귀 없음
[ ] F3 → scene reload
[ ] Team HUD가 TEAM · G/M으로 바뀜
[ ] Reserve row가 메구미로 표시됨
[ ] T Tag → 메구미 전환
[ ] 메구미의 검은 머리/남색 계열 prototype avatar가 표시됨
[ ] Active HUD 이름/색이 MEGUMI profile로 바뀜
[ ] Skill Deck이 Q 옥견 / E 누에 / R 만상 / V 감합암예정 표시
[ ] 메구미 상태에서 이동 / 기본 1-2-3 / Dodge 동작
[ ] 기본 attack/dodge prototype pose가 깨지지 않음
[ ] 메구미 HP/CE 저장 후 T → 고죠 → 다시 T → 메구미 시 값 보존
[ ] 다시 F3 → 기본 GOJO + SUKUNA roster 복귀
[ ] 고죠/스쿠나 기존 전투 회귀 없음
[ ] Console 빨간 오류 없음
```

주의:

```text
메구미 Q/E/R/V는 Pass 1에서는 아직 동작하지 않는 것이 정상이다.
```

통과하면:

```text
Gate 5A Pass 1: USER VERIFIED
```

다음:

```text
Pass 2 · First Summon Gameplay
→ 옥견을 첫 대표 식신으로 구현
→ 소환체 lifetime/targeting/gameplay ownership 검증
→ Technique Presentation Request / VFX Lifecycle / Audio Event 재사용
```
