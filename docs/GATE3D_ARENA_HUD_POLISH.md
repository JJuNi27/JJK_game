# Gate 3D — Representative Arena + HUD Polish

작성 기준일: 2026-08-22

## 상태

```text
Gate 3A Combat Feel: USER VERIFIED
Gate 3B Prototype Character Presentation: USER VERIFIED
Gate 3C Signature Technique Presentation: USER VERIFIED
Gate 3D Representative Arena + HUD Polish: STARTED
Gate 3D Pass 1 · Arena Mood Scaffold: REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## Gate 3D 목적

현재 CombatMVP는 전투 규칙과 대표 기술 검증에는 충분하지만 화면은 여전히 회색 Prototype Arena에 가깝다.

Gate 3D에서는 전투 규칙을 건드리지 않고:

```text
1. 대표 전장 분위기
2. 캐릭터/술식이 읽히는 배경 명암
3. HUD와 월드 연출의 시각적 통합
4. Beauty Corner 최종 회귀 확인
```

을 진행한다.

실제 최종 맵 자산은 아직 없으므로 지금은 Shibuya/도심 야간 전투를 연상시키는 `GAME_ORIGINAL mood scaffold`만 사용한다.

---

# Pass 1 — Arena Mood Scaffold

새 파일:

```text
unity/Assets/Scripts/Core/PrototypeBeautyArenaPresentation.cs
```

`CombatMVP` Scene에서만 활성화되는 Runtime Presentation이다.

## 추가되는 표현

```text
- 푸른/남색 계열 야간 Ambient
- 먼 거리 Linear Fog
- 플레이 영역 밖 비충돌 도심 실루엣
- 청색 / 적색 / 주황 Neon accent
- 바닥의 얇은 이중 Arena ring
- 교차 Street lane 형태의 낮은 바닥 accent
- 차가운 조명 1개 + 따뜻한 조명 1개
```

도심 실루엣과 바닥 accent의 Collider는 비활성화/제거한다.

따라서 장식물이:

```text
Player movement
Enemy movement
Target Lock
Projectile
Domain
Spawn
Arena collision
```

을 막지 않도록 한다.

## 바꾸지 않은 것

```text
Damage
HP / CE
Cooldown
Skill rules
Domain rules
Player / Enemy spawn
Combat arena collider
Camera offset
Team Match
Target Lock
```

## 현재 성격

이 Pass는 최종 Shibuya 맵 제작이 아니다.

목적은 실제 배경 자산이 들어오기 전에:

```text
어두운 도심 배경
+ 밝은 Signature VFX
+ 캐릭터 실루엣
```

조합이 Beauty Corner에서 읽히는지 검증하는 것이다.

실제 Environment Model / Texture / Decal / Post Process 자산이 들어오면 이 Runtime scaffold는 제거 대상이다.

## 빠른 사용자 테스트

```text
1. git pull origin master
2. Unity Console 빨간 오류 확인
3. Play를 완전히 다시 시작

확인 포인트
- 기존 회색 테스트장보다 야간 도심 분위기가 확실히 생겼는지
- 플레이 영역 밖에 건물 실루엣/네온이 보이는지
- 바닥에 얇은 원형 accent가 보이는지
- 캐릭터와 허식 자 / 푸가 / 영역 VFX가 배경에 묻히지 않는지
- 걷기/회피/전투 중 보이지 않는 벽이나 장식 충돌이 생기지 않는지
```

정상 확인 후 다음 묶음은 `Gate 3D Pass 2 · HUD visual integration`이다.
