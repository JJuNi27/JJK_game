# Gate 3D — Representative Arena + HUD Polish

작성 기준일: 2026-08-22

## 상태

```text
Gate 3A Combat Feel: USER VERIFIED
Gate 3B Prototype Character Presentation: USER VERIFIED
Gate 3C Signature Technique Presentation: USER VERIFIED
Gate 3D Representative Arena + HUD Polish: STARTED
Gate 3D Pass 1 · Arena Mood Scaffold: USER VERIFIED
Gate 3D Pass 2 · Team HUD Visual Integration: USER VERIFIED
Gate 3D Pass 3 · Contextual Skill Deck HUD: REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## Gate 3D 목적

현재 CombatMVP는 전투 규칙과 대표 기술 검증에는 충분하지만 화면은 여전히 Prototype Arena / Debug HUD 성격이 강하다.

Gate 3D에서는 전투 규칙을 건드리지 않고:

```text
1. 대표 전장 분위기
2. 캐릭터/술식이 읽히는 배경 명암
3. HUD와 월드 연출의 시각적 통합
4. Beauty Corner 최종 회귀 확인
```

을 진행한다.

실제 최종 맵 자산은 아직 없으므로 지금은 Shibuya/도심 야간 전투를 연상시키는 `GAME_ORIGINAL mood scaffold`를 사용한다.

---

# Pass 1 — Arena Mood Scaffold

파일:

```text
unity/Assets/Scripts/Core/PrototypeBeautyArenaPresentation.cs
```

`CombatMVP` Scene에서만 활성화되는 Runtime Presentation이다.

추가 표현:

```text
- 푸른/남색 계열 야간 Ambient
- 먼 거리 Linear Fog
- 플레이 영역 밖 비충돌 도심 실루엣
- 청색 / 적색 / 주황 Neon accent
- 바닥의 얇은 이중 Arena ring
- 교차 Street lane 형태의 낮은 바닥 accent
- 차가운 조명 1개 + 따뜻한 조명 1개
```

도심 실루엣과 바닥 accent의 Collider는 의도적으로 비활성화/제거한다.

따라서 현재 Prototype 건물은 `배경 실루엣`이며 실제 벽이나 플레이 가능한 건물 구조가 아니다.

## Pass 1 사용자 확인

2026-08-22 사용자 Unity 실제 확인:

```text
- 건물 실루엣 표시 정상
- 맵 링 표시 정상
- 건물은 통과 가능
```

건물 통과는 현재 설계상 정상이다. 장식 Collider를 의도적으로 제거했기 때문이다.

따라서:

```text
Gate 3D Pass 1 · Arena Mood Scaffold: USER VERIFIED
```

---

# Pass 2 — Team HUD Visual Integration

변경 파일:

```text
unity/Assets/Scripts/Player/PrototypePlayerTeamController.cs
unity/Assets/Scripts/Enemy/PrototypeOpponentTeamController.cs
```

Player / Opponent Team HUD의 기능성 정보는 유지하면서 디버그 박스 느낌을 줄이고 야간 Beauty Corner에 맞는 전투 HUD plate로 정리했다.

유지한 정보:

```text
- Active / Reserve
- HP / CE
- Tag Ready / Cooldown / Action Lock
- KO
- Opponent Reserve Entry
- F2 Encounter Mode
```

게임 규칙은 변경하지 않았다.

## Pass 2 사용자 확인

2026-08-22 사용자 Unity 실제 확인:

```text
HUD 변경 정상
Tag / HP / CE / Opponent Team 표시 정상
```

사용자 판정:

```text
ㅇㅇ 정상이다
```

따라서:

```text
Gate 3D Pass 2 · Team HUD Visual Integration: USER VERIFIED
```

---

# Pass 3 — Contextual Skill Deck HUD

새 파일:

```text
unity/Assets/Scripts/Core/PrototypeSkillDeckHud.cs
```

기존 Match HUD를 뜯어고치지 않고, Beauty Corner에서 항상 필요한 `현재 캐릭터의 4개 대표 술식`을 한눈에 읽게 만드는 별도 하단 Skill Deck를 추가한다.

## 표시 구조

고죠:

```text
Q 창
E 혁
R 허식 자
V 무량공처
```

스쿠나:

```text
Q 해
E 팔
R 푸가
V 복마어주자
```

Tag로 Active 캐릭터가 바뀌면 Skill Deck도 자동으로 교체된다.

## 상태 표현

`CombatActionGate`를 읽기 전용으로 사용한다.

```text
READY
DODGE
CASTING
DOMAIN INPUT
DOMAIN ACTIVE
TECHNIQUE BURNOUT
DISABLED
```

현재 시작할 수 없는 술식은 chip이 흐려진다.

Q / E / R / V 입력 순간 해당 chip이 약 0.18초 강조되어 입력과 연출의 연결이 눈에 보이도록 한다.

## 위치

기본은 화면 우하단이다.

작은 화면에서는 기존 하단 Domain HUD와 겹침을 줄이기 위해 조금 위로 자동 이동한다.

## 바꾸지 않은 것

```text
Input Binding
Damage
CE Cost
Cooldown
Cast Time
Domain Rule
Tag
Target Lock
Match Result
```

즉 전부 Presentation이다.

## 빠른 사용자 테스트

```text
1. git pull origin master
2. Unity Console 빨간 오류 확인
3. Play 시작

확인 포인트
- 우하단에 Q/E/R/V Skill Deck가 표시되는지
- 고죠에서 창 / 혁 / 허식 자 / 무량공처가 보이는지
- T Tag 후 스쿠나의 해 / 팔 / 푸가 / 복마어주자로 즉시 바뀌는지
- Q/E/R/V를 누를 때 해당 chip이 잠깐 강조되는지
- Dodge / Casting / Domain 상태에서 상단 상태 라벨과 chip 밝기가 자연스럽게 바뀌는지
- 기존 Team HUD / Domain HUD / 전투 입력이 깨지지 않는지
```

Pass 3 검증 후 Gate 3D는 `Beauty Corner regression`을 한 번 묶어서 확인하고 닫는 방향으로 진행한다.
