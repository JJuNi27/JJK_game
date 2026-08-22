# Gate 4A — Character Presentation Contract

작성 기준일: 2026-08-23

## 상태

```text
Gate 3 Beauty Corner Prototype Proof: USER VERIFIED
Gate 4 Production Pipeline Extraction: STARTED
Gate 4A Pass 1 · Character Presentation Profile: REMOTE IMPLEMENTED / USER TEST PENDING
```

Assistant는 Unity를 직접 실행/컴파일했다고 주장하지 않는다.

## 왜 먼저 이걸 하는가

Gate 3에서 고죠/스쿠나를 빠르게 검증하면서 다음 정보가 여러 파일에 반복됐다.

```text
캐릭터 이름
짧은 이름
시대/Variant 라벨
HUD 색상
CE 보조 색상
Q/E/R/V 기술명
기술별 표현 색상
```

세 번째 캐릭터를 추가할 때 이 문자열/색상을 HUD마다 다시 복사하면 캐릭터 추가 비용과 오류가 계속 늘어난다.

Gate 4A는 Gameplay를 범용화하는 작업이 아니라 `누가 전투 중인지 화면에 표현하는 데이터`만 한 곳으로 모은다.

---

# 새 Contract

파일:

```text
unity/Assets/Scripts/Player/CharacterPresentationProfile.cs
```

타입:

```text
CharacterPresentationProfile
CharacterSkillPresentation
CharacterPresentationSkillSlot
CharacterPresentationProfiles
```

현재 Profile:

```text
GojoModern
- GOJO SATORU · 현대 · 교사
- 고죠
- 현대 · 교사 / 현대
- blue HUD / CE accent
- Q 창
- E 혁
- R 허식 자
- V 무량공처

SukunaShibuyaYujiBody
- RYOMEN SUKUNA · 시부야 사변
- 스쿠나
- 시부야 사변 / 시부야
- red HUD / CE accent
- Q 해
- E 팔
- R 푸가
- V 복마어주자
```

## 의도적으로 넣지 않은 것

```text
HP
CE max/start/regen
Damage
Cooldown
Cast Time
Fuga condition
Infinity rule
Domain rule
Sure Hit
Tag rule
```

이런 값은 Presentation identity가 아니라 Gameplay Rule이므로 기존 시스템 소유권을 유지한다.

---

# Migration

## PrototypeCharacterController

기존:

```text
DisplayName ternary
character switch chip 색상/이름 하드코딩
Sukuna HUD/도움말 일부 기술명 하드코딩
```

현재:

```text
PresentationProfile
→ DisplayName
→ HUD Accent / Energy Accent
→ Q/E/R/V label
```

## PrototypePlayerTeamController

기존 로컬 helper:

```text
CharacterAccent()
CharacterName()
CharacterShortName()
CharacterEraLabel()
```

을 제거하고 `CharacterPresentationProfiles.Get(characterId)`에서 읽도록 이관했다.

Tag/HP/CE/KO 로직은 수정하지 않았다.

## PrototypeSkillDeckHud

기존:

```text
if Sukuna
→ 해 / 팔 / 푸가 / 복마어주자 + red/orange colors
else
→ 창 / 혁 / 허식 자 / 무량공처 + blue/red/purple colors
```

현재:

```text
Active CharacterPresentationProfile
→ Q/E/R/V label + accent
```

으로 바꿨다.

Skill availability는 여전히 `CombatActionGate`를 읽고, 입력은 여전히 `CombatInputBindings`를 사용한다.

즉 Profile은 표시 데이터만 제공한다.

---

# Asset Hook 정책

현재 실제 rigged Gojo/Sukuna model, portrait, Animator Controller asset이 저장소에 없다.

그래서 이번 Pass에서는 존재하지 않는 asset reference를 Profile에 가짜로 설계하지 않았다.

실제 asset이 들어오면 다음을 검토한다.

```text
ScriptableObject-backed CharacterPresentationProfile
Portrait/Icon reference
Presentation Prefab reference
RuntimeAnimatorController reference
Technique icon set
```

지금은 반복이 실제로 확인된 데이터만 추출한다.

---

# 사용자 테스트

```text
1. cd D:\GitHub\JJK_game
2. git pull origin master
3. Unity compile 완료 대기
4. Console 빨간 오류 확인
5. CombatMVP Play
```

빠른 확인:

```text
A. 시작 고죠
[ ] 좌측 Active HUD가 고죠/청색 계열 그대로
[ ] 우하단 GOJO · 현대
[ ] Q 창 / E 혁 / R 허식 자 / V 무량공처

B. T Tag → 스쿠나
[ ] 좌측 Active HUD가 스쿠나/적색 계열로 변경
[ ] Team A/R 이름 정상
[ ] 우하단 SUKUNA · 시부야
[ ] Q 해 / E 팔 / R 푸가 / V 복마어주자

C. 상태
[ ] Q/E/R/V pulse 정상
[ ] DODGE / CASTING / DOMAIN 상태 표시 정상
[ ] HP/CE Tag 보존 정상

D. Gameplay smoke
[ ] 창/혁/허식 자 정상
[ ] 해/팔/푸가 정상
[ ] 무량공처/복마어주자 정상
```

## 성공 기준

화면은 Gate 3 마지막 모습과 거의 같아야 한다.

이번 작업의 성공은 `새로운 것이 보이는 것`이 아니라:

```text
기존 표시/Gameplay는 그대로
하지만 코드에서는 Fighter identity가 한 Profile에서 공급됨
```

이다.

사용자 확인 후:

```text
Gate 4A Pass 1: USER VERIFIED
```

로 닫고 남은 identity hardcode를 조사해 4A를 마무리하거나 바로 4B HUD Data Binding으로 이어간다.
