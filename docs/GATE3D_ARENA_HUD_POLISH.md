# Gate 3D — Representative Arena + HUD Polish

작성 기준일: 2026-08-22

## 상태

```text
Gate 3A Combat Feel: USER VERIFIED
Gate 3B Prototype Character Presentation: USER VERIFIED
Gate 3C Signature Technique Presentation: USER VERIFIED
Gate 3D Representative Arena + HUD Polish: STARTED
Gate 3D Pass 1 · Arena Mood Scaffold: USER VERIFIED
Gate 3D Pass 2 · Team HUD Visual Integration: REMOTE IMPLEMENTED / USER TEST PENDING
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

현재 기능성 HUD의 정보 구조는 유지하면서 `디버그 박스` 느낌을 줄이고 야간 도심 Beauty Corner와 어울리는 전투 HUD 언어로 정리한다.

변경 파일:

```text
unity/Assets/Scripts/Player/PrototypePlayerTeamController.cs
unity/Assets/Scripts/Enemy/PrototypeOpponentTeamController.cs
```

## Player HUD

```text
- 상단 좌측 Active Fighter 패널을 76px 높이의 전투 카드로 재구성
- 캐릭터명 / 시대 라벨 / ACTIVE 상태 분리
- HP / CE bar를 어두운 HUD plate 위에 통합
- 고죠는 청색, 스쿠나는 적색 accent 유지
- 좌측 accent stripe와 얇은 highlight line 추가
```

Team panel:

```text
- 기존 92px 패널을 더 얇은 76px 구조로 압축
- TEAM / T TAG 상태를 헤더에 분리
- Active / Reserve는 A / R row로 간결화
- HP / CE 상태 정보는 유지
- KO / ACTION LOCK / Cooldown 정보 유지
```

## Opponent HUD

```text
- 우측 상단 Team panel을 Player HUD와 대칭되는 76px plate로 재구성
- OPPONENT / 생존 수 / RESERVE ENTRY 상태 분리
- Active / Reserve row를 간결화
- 우측 red/orange accent stripe 사용
```

Mode chip:

```text
기존 큰 F2 MODE 박스
→ 화면 상단 중앙의 얇은 21px 상태 strip
```

## 바꾸지 않은 것

```text
HP / CE 값
Tag 기능
KO Auto Tag
Opponent Reserve Entry
F2 Encounter Mode
Damage
Skill / Domain
Target Lock
Camera
Arena collision
```

즉 이번 Pass는 정보와 게임 규칙은 그대로 두고 Presentation만 정리한다.

## 빠른 사용자 테스트

```text
1. git pull origin master
2. Unity Console 빨간 오류 확인
3. Play 시작

확인 포인트
- 좌측 Active HUD가 이전보다 게임 HUD처럼 정리됐는지
- 하단 좌측 Team panel이 겹치지 않고 Active / Reserve 정보가 읽히는지
- F2 Team Battle에서 우측 Opponent panel이 대칭적으로 보이는지
- 상단 MODE strip이 중앙 공격 표시와 겹치지 않는지
- 고죠 → 스쿠나 Tag 시 accent / 이름 / HP / CE가 정상 전환되는지
- 상대 첫 KO → Reserve Entry → 최종 KO까지 HUD 정보가 정상 유지되는지
```

정상 확인 후 다음 묶음은 `Gate 3D Pass 3 · Match / Skill HUD hierarchy polish + Beauty Corner regression`이다.
